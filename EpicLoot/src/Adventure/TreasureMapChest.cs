using EpicLoot.Biome;
using HarmonyLib;
using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace EpicLoot.Adventure
{
    public class TreasureMapChest : MonoBehaviour
    {
        public Heightmap.Biome Biome;
        public int Interval;

        // Use friendly biome name if available (for modded biomes), otherwise use enum name
        public string LootTableName => $"TreasureMapChest_{BiomeDataManager.GetBiomeName(Biome)}";

        public void Setup(long playerID, Heightmap.Biome biome, int treasureMapInterval)
        {
            EpicLoot.Log($"[TreasureMap] TreasureMapChest.Setup: Starting for biome={biome} (value={(int)biome}), interval={treasureMapInterval}, playerID={playerID}");
            Reinitialize(biome, treasureMapInterval, false, playerID);

            var container = GetComponent<Container>();
            if (container == null || container.m_nview == null)
            {
                EpicLoot.LogError($"Trying to set up TreasureMapChest ({biome} {treasureMapInterval}) but there was no Container component!");
                return;
            }

            var zdo = container.m_nview.GetZDO();
            if (zdo != null && zdo.IsValid())
            {
                container.GetInventory().RemoveAll();

                zdo.Set("TreasureMapChest.Interval", Interval);
                zdo.Set("TreasureMapChest.Biome", Biome.ToString());
                zdo.Set("creator", playerID);
                EpicLoot.Log($"[TreasureMap] TreasureMapChest.Setup: Stored biome as string='{Biome.ToString()}' in ZDO");

                var items = LootRoller.RollLootTable(LootTableName, 1, LootTableName, transform.position);
                EpicLoot.Log($"[TreasureMap] TreasureMapChest.Setup: LootTable={LootTableName}, rolled {items.Count} items");
                items.ForEach(item => container.m_inventory.AddItem(item));

                var treasureMapConfig = BiomeDataManager.GetTreasureMapConfig(biome);
                EpicLoot.Log($"[TreasureMap] TreasureMapChest.Setup: TreasureMapConfig found={treasureMapConfig != null} for biome={biome}");
                if (treasureMapConfig?.ForestTokens > 0)
                    container.m_inventory.AddItem("ForestToken", treasureMapConfig.ForestTokens, 1, 0, 0, string.Empty);

                if (treasureMapConfig?.IronTokens > 0)
                    container.m_inventory.AddItem("IronBountyToken", treasureMapConfig.IronTokens, 1, 0, 0, string.Empty);

                if (treasureMapConfig?.GoldTokens > 0)
                    container.m_inventory.AddItem("GoldBountyToken", treasureMapConfig.GoldTokens, 1, 0, 0, string.Empty);

                if (treasureMapConfig?.Coins > 0)
                    container.m_inventory.AddItem("Coins", treasureMapConfig.Coins, 1, 0, 0, string.Empty);

                container.Save();
                EpicLoot.Log($"[TreasureMap] TreasureMapChest.Setup: Complete for biome={biome}");
            }
            else
            {
                EpicLoot.LogError($"Trying to set up TreasureMapChest ({biome} {treasureMapInterval}) but ZDO was not valid!");
            }
        }

        public void Reinitialize(Heightmap.Biome biome, int treasureMapInterval, bool hasBeenFound, long ownerPlayerId)
        {
            Biome = biome;
            Interval = treasureMapInterval;
            gameObject.layer = 12;

            var container = GetComponent<Container>();
            if (container != null)
            {
                var label = Localization.instance.Localize("$mod_epicloot_treasurechest_name",
                    $"$biome_{Biome.ToString().ToLower()}", (treasureMapInterval + 1).ToString());
                container.m_name = Localization.instance.Localize(label);
                container.m_privacy = hasBeenFound ? Container.PrivacySetting.Public : Container.PrivacySetting.Private;
                container.m_autoDestroyEmpty = true;
            }

            var piece = GetComponent<Piece>();
            if (piece != null)
            {
                piece.m_creator = ownerPlayerId;
            }

            if (!hasBeenFound)
            {
                var beacon = gameObject.AddComponent<Beacon>();
                beacon.m_range = EpicLoot.GetAndvaranautRange();
            }

            // TODO Figure out why this damn thing won't float
            // Idea: It's probably because of the snap to ground chests have? Or gravity physics?
            /*var rigidbody = gameObject.AddComponent<Rigidbody>();
            rigidbody.constraints =
                RigidbodyConstraints.FreezePositionX
                | RigidbodyConstraints.FreezePositionZ
                | RigidbodyConstraints.FreezeRotation;
            rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;

            var floating = gameObject.AddComponent<Floating>();
            floating.m_waterLevelOffset = 0.3f;
            floating.TerrainCheck();*/

            Destroy(gameObject.GetComponent<WearNTear>());
        }
    }

    [HarmonyPatch(typeof(Container), nameof(Container.Awake))]
    public static class Container_Awake_Patch
    {
        public static void Postfix(Container __instance)
        {
            var zdo = __instance.m_nview.GetZDO();
            if (zdo != null)
            {
                var biomeString = zdo.GetString($"{nameof(TreasureMapChest)}.{nameof(TreasureMapChest.Biome)}");
                if (!string.IsNullOrEmpty(biomeString))
                {
                    EpicLoot.Log($"[TreasureMap] Container_Awake_Patch: Found TreasureMapChest ZDO with biomeString='{biomeString}'");
                    if (Enum.TryParse(biomeString, out Heightmap.Biome biome))
                    {
                        EpicLoot.Log($"[TreasureMap] Container_Awake_Patch: Parsed biome={biome} (value={(int)biome})");
                        var interval = zdo.GetInt("TreasureMapChest.Interval");
                        var hasBeenFound = zdo.GetBool("TreasureMapChest.HasBeenFound");
                        var owner = zdo.GetLong("creator");
                        var treasureMapChest = __instance.gameObject.AddComponent<TreasureMapChest>();
                        treasureMapChest.Reinitialize(biome, interval, hasBeenFound, owner);
                    }
                    else
                    {
                        EpicLoot.LogError($"[EpicLoot.Adventure.Container_Awake] Unknown biome: {biomeString} - Enum.TryParse failed");
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(Container), nameof(Container.RPC_OpenRespons))]
    public static class Container_RPC_OpenRespons_Patch
    {
        public static void Postfix(Container __instance, long uid, bool granted)
        {
            var zdo = __instance.m_nview.GetZDO();
            if (zdo == null || !zdo.IsValid() || zdo.GetBool("TreasureMapChest.HasBeenFound"))
            {
                return;
            }

            var player = Player.m_localPlayer;
            if (granted && player != null)
            {
                var treasureMapChest = __instance.GetComponent<TreasureMapChest>();
                if (treasureMapChest != null)
                {
                    EpicLoot.Log($"Player is opening treasure map chest ({treasureMapChest.Biome}, {treasureMapChest.Interval})!");
                    var saveData = player.GetAdventureSaveData();
                    saveData.FoundTreasureChest(treasureMapChest.Interval, treasureMapChest.Biome);

                    zdo.Set("TreasureMapChest.HasBeenFound", true);

                    __instance.m_privacy = Container.PrivacySetting.Public;

                    MessageHud.instance.ShowBiomeFoundMsg("Treasure Found!", true);

                    Object.Destroy(treasureMapChest.GetComponent<Beacon>());
                    Object.Destroy(treasureMapChest.GetComponent<Rigidbody>());
                }
            }
        }
    }
}
