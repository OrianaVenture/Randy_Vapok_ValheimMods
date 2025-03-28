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
            if (player != null)
            {
                var saveData = player.GetAdventureSaveData();
                foreach (var biome in player.m_knownBiome)
                {
                    var lootTableName = $"TreasureMapChest_{biome}";
                    var lootTableExists = LootRoller.GetLootTable(lootTableName).Count > 0;
                    if (lootTableExists)
                    {
                        var purchased = saveData.HasPurchasedTreasureMap(currentInterval, biome);
                        var cost = AdventureDataManager.Config.TreasureMap.BiomeInfo.Find(x => x.Biome == biome);
                        if (cost.Cost > 0)
                        {
                            results.Add(new TreasureMapItemInfo() {
                                Biome = biome,
                                Interval = currentInterval,
                                Cost = cost.Cost,
                                AlreadyPurchased = purchased
                            });
                        }
                    }
                }
            }

            return results.OrderBy(x => x.Cost).ToList();
        }

        public IEnumerator SpawnTreasureChest(Heightmap.Biome biome, Player player, int price, Action<int, bool, Vector3> callback)
        {
            player.Message(MessageHud.MessageType.Center, "$mod_epicloot_treasuremap_locatingmsg");
            var saveData = player.GetAdventureSaveData();
            yield return BountyLocationEarlyCache.LazyCacheGetBiomePoint(biome, saveData, (success, spawnPoint, normal) =>
            {
                if (success)
                {
                    CreateTreasureSpawner(biome, spawnPoint, saveData);
                    callback?.Invoke(price, true, spawnPoint);
                }
                else
                {
                    callback?.Invoke(0, false, Vector3.zero);
                }
            });
        }

        private void CreateTreasureSpawner(Heightmap.Biome biome,  Vector3 spawnPoint, AdventureSaveData saveData)
        {
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
            asc.SetTreasure(treasure_details);

            var offset2 = UnityEngine.Random.insideUnitCircle * (AdventureDataManager.Config.TreasureMap.MinimapAreaRadius * 0.8f);
            var offset = new Vector3(offset2.x, 0, offset2.y);
            saveData.PurchasedTreasureMap(treasure_details);

            Minimap.instance.ShowPointOnMap(spawnPoint + offset);
        }
    }
}
