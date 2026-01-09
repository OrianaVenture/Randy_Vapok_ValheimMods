using EpicLoot.Biome;
using Jotunn.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace EpicLoot.Adventure.Feature
{
    public class TreasureMapItemInfo
    {
        public Heightmap.Biome Biome;
        public int Interval;
        public int Cost;
        public bool AlreadyPurchased;
    }

    public class TreasureMapsAdventureFeature : AdventureFeature
    {
        public override AdventureFeatureType Type => AdventureFeatureType.TreasureMaps;
        public override int RefreshInterval => AdventureDataManager.Config.TreasureMap.RefreshInterval;

        public List<TreasureMapItemInfo> GetTreasureMaps()
        {
            var results = new List<TreasureMapItemInfo>();

            var player = Player.m_localPlayer;
            var currentInterval = GetCurrentInterval();
            EpicLoot.Log($"[TreasureMap] GetTreasureMaps: Starting, player={player?.GetPlayerName() ?? "null"}, interval={currentInterval}");
            if (player != null)
            {
                EpicLoot.Log($"[TreasureMap] GetTreasureMaps: Player known biomes count={player.m_knownBiome.Count}");
                foreach (var knownBiome in player.m_knownBiome)
                {
                    EpicLoot.Log($"[TreasureMap] GetTreasureMaps: Player knows biome={knownBiome} (value={(int)knownBiome})");
                }

                var saveData = player.GetAdventureSaveData();
                foreach (var biome in player.m_knownBiome)
                {
                    // Use friendly biome name if available (for modded biomes), otherwise use enum name
                    var biomeName = BiomeDataManager.GetBiomeName(biome);
                    var lootTableName = $"TreasureMapChest_{biomeName}";
                    var lootTableExists = LootRoller.GetLootTable(lootTableName).Count > 0;
                    EpicLoot.Log($"[TreasureMap] GetTreasureMaps: Checking biome={biome}, biomeName={biomeName}, lootTable={lootTableName}, exists={lootTableExists}");
                    if (lootTableExists)
                    {
                        var purchased = saveData.HasPurchasedTreasureMap(currentInterval, biome);
                        var treasureMapConfig = BiomeDataManager.GetTreasureMapConfig(biome);
                        EpicLoot.Log($"[TreasureMap] GetTreasureMaps: biome={biome}, cost config found={treasureMapConfig != null}, cost={(treasureMapConfig?.Cost ?? -1)}, purchased={purchased}");
                        if (treasureMapConfig != null && treasureMapConfig.Cost > 0)
                        {
                            results.Add(new TreasureMapItemInfo() {
                                Biome = biome,
                                Interval = currentInterval,
                                Cost = treasureMapConfig.Cost,
                                AlreadyPurchased = purchased
                            });
                            EpicLoot.Log($"[TreasureMap] GetTreasureMaps: Added biome={biome} to results");
                        }
                        else
                        {
                            EpicLoot.Log($"[TreasureMap] GetTreasureMaps: Skipped biome={biome} - no cost config or cost <= 0");
                        }
                    }
                }
            }

            EpicLoot.Log($"[TreasureMap] GetTreasureMaps: Returning {results.Count} treasure maps");
            return results.OrderBy(x => x.Cost).ToList();
        }

        public IEnumerator SpawnTreasureChest(Heightmap.Biome biome, Player player, int price, Action<int, bool, Vector3> callback)
        {
            EpicLoot.Log($"[TreasureMap] SpawnTreasureChest: Starting for biome={biome} (value={(int)biome}), price={price}");
            player.Message(MessageHud.MessageType.Center, "$mod_epicloot_treasuremap_locatingmsg");
            var saveData = player.GetAdventureSaveData();
            yield return BountyLocationEarlyCache.TryGetBiomePoint(biome, saveData, (success, spawnPoint) =>
            {
                EpicLoot.Log($"[TreasureMap] SpawnTreasureChest: TryGetBiomePoint returned success={success}, spawnPoint={spawnPoint}");
                if (success)
                {
                    CreateTreasureSpawner(biome, spawnPoint, saveData);
                    callback?.Invoke(price, true, spawnPoint);
                }
                else
                {
                    EpicLoot.Log($"[TreasureMap] SpawnTreasureChest: Failed to find spawn point for biome={biome}");
                    callback?.Invoke(0, false, Vector3.zero);
                }
            });
        }

        private void CreateTreasureSpawner(Heightmap.Biome biome,  Vector3 spawnPoint, AdventureSaveData saveData)
        {
            EpicLoot.Log($"[TreasureMap] CreateTreasureSpawner: Creating spawner for biome={biome} (value={(int)biome}) at {spawnPoint}");
            Quaternion rotation = Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f);
            GameObject gameObject = PrefabManager.Instance.GetPrefab("EL_SpawnController");
            GameObject created_go = Object.Instantiate(gameObject, spawnPoint, rotation);
            AdventureSpawnController asc = created_go.GetComponent<AdventureSpawnController>();
            TreasureMapChestInfo treasure_details = new TreasureMapChestInfo()
            {
                Biome = biome,
                Interval = GetCurrentInterval(),
                Position = spawnPoint,
                PlayerID = Player.m_localPlayer.GetPlayerID()
            };
            EpicLoot.Log($"[TreasureMap] CreateTreasureSpawner: TreasureMapChestInfo created with biome={treasure_details.Biome} (value={(int)treasure_details.Biome})");
            asc.SetTreasure(treasure_details);

            var offset2 = UnityEngine.Random.insideUnitCircle *
                (AdventureDataManager.Config.TreasureMap.MinimapAreaRadius * 0.8f);
            var offset = new Vector3(offset2.x, 0, offset2.y);
            saveData.PurchasedTreasureMap(treasure_details);

            Minimap.instance.ShowPointOnMap(spawnPoint + offset);
            EpicLoot.Log($"[TreasureMap] CreateTreasureSpawner: Complete, map shown on minimap");
        }
    }
}
