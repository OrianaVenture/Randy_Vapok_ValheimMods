using EpicLoot.Adventure.Feature;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EpicLoot.Adventure
{
    public static class AdventureDataManager
    {
        public static AdventureDataConfig Config;
        private static readonly Dictionary<string, Sprite> _cachedTrophySprites = new Dictionary<string, Sprite>();
        private static Dictionary<string, string> _biomeNameLookup;

        public static SecretStashAdventureFeature SecretStash;
        public static GambleAdventureFeature Gamble;
        public static TreasureMapsAdventureFeature TreasureMaps;
        public static BountiesAdventureFeature Bounties;
        public static int CheatNumberOfBounties = -1;
        #nullable enable
        public static event Action? OnSetupAdventureData;
        #nullable disable
        public static void Initialize(AdventureDataConfig config)
        {
            Config = config;

            // Build biome name lookup from Bosses config
            BuildBiomeNameLookup();
            
            OnSetupAdventureData?.Invoke();

            SecretStash = new SecretStashAdventureFeature();
            Gamble = new GambleAdventureFeature();
            TreasureMaps = new TreasureMapsAdventureFeature();
            Bounties = new BountiesAdventureFeature();

            Config.TreasureMap.UpdateBiomeList();
            EpicLoot.Log($"Updated/setup Adventure Data");
        }

        /// <summary>
        /// Builds a lookup dictionary from biome numbers to friendly names using the Bosses config.
        /// </summary>
        private static void BuildBiomeNameLookup()
        {
            _biomeNameLookup = new Dictionary<string, string>();

            if (Config?.Bounties?.Bosses == null)
            {
                EpicLoot.Log($"[AdventureDataManager] BuildBiomeNameLookup: No Bosses config found");
                return;
            }

            EpicLoot.Log($"[AdventureDataManager] BuildBiomeNameLookup: Processing {Config.Bounties.Bosses.Count} boss entries");

            foreach (var boss in Config.Bounties.Bosses)
            {
                string biomeValue = ((int)boss.Biome).ToString();
                EpicLoot.Log($"[AdventureDataManager] Boss entry: Biome={boss.Biome} ({biomeValue}), BiomeName='{boss.BiomeName ?? "null"}', BossPrefab={boss.BossPrefab}");

                if (!string.IsNullOrEmpty(boss.BiomeName))
                {
                    if (!_biomeNameLookup.ContainsKey(biomeValue))
                    {
                        _biomeNameLookup[biomeValue] = boss.BiomeName;
                        EpicLoot.Log($"[AdventureDataManager] Registered biome alias: {biomeValue} -> {boss.BiomeName}");
                    }
                }
            }

            EpicLoot.Log($"[AdventureDataManager] Built biome name lookup with {_biomeNameLookup.Count} entries");
        }

        /// <summary>
        /// Gets the friendly name for a biome, if one was defined in the Bosses config.
        /// Returns null if no name mapping exists.
        /// </summary>
        public static string GetBiomeName(Heightmap.Biome biome)
        {
            if (_biomeNameLookup == null)
            {
                return null;
            }

            string biomeKey = ((int)biome).ToString();
            return _biomeNameLookup.TryGetValue(biomeKey, out string name) ? name : null;
        }

        /// <summary>
        /// Gets the friendly name for a biome by its numeric string key.
        /// Returns null if no name mapping exists.
        /// </summary>
        public static string GetBiomeName(string biomeKey)
        {
            if (_biomeNameLookup == null)
            {
                return null;
            }

            return _biomeNameLookup.TryGetValue(biomeKey, out string name) ? name : null;
        }

        /// <summary>
        /// Gets all custom biome names defined in the Bosses config.
        /// Used to create unidentified item prefabs for user-defined biomes.
        /// </summary>
        public static IEnumerable<string> GetCustomBiomeNames()
        {
            if (_biomeNameLookup == null)
            {
                return Enumerable.Empty<string>();
            }

            return _biomeNameLookup.Values.Distinct();
        }

        public static AdventureDataConfig GetCFG()
        {
            return Config;
        }

        public static void UpdateAventureData(AdventureDataConfig config)
        {
            Config = config;

            Config.TreasureMap.UpdateBiomeList();
            EpicLoot.Log($"Updated Adventure Data");
        }

        public static Sprite GetTrophyIconForMonster(string monsterID, bool isGold)
        {
            if (_cachedTrophySprites.TryGetValue(monsterID, out var sprite))
            {
                return sprite;
            }

            if (ZNetScene.instance != null)
            {
                var prefab = ZNetScene.instance.GetPrefab(monsterID);
                if (prefab != null)
                {
                    var characterDrop = prefab.GetComponent<CharacterDrop>();
                    if (characterDrop != null)
                    {
                        var drops = characterDrop.m_drops.Select(x => x.m_prefab.GetComponent<ItemDrop>());
                        var trophyPrefab = drops.FirstOrDefault(x => x.m_itemData.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Trophy);
                        if (trophyPrefab != null)
                        {
                            sprite = trophyPrefab.m_itemData.GetIcon();
                            if (sprite != null)
                            {
                                _cachedTrophySprites.Add(monsterID, sprite);
                            }
                            return sprite;
                        }
                    }
                }
            }

            var noTrophySpriteName = $"NoTrophy{(isGold ? "Gold" : "Iron")}Sprite";
            if (_cachedTrophySprites.TryGetValue(noTrophySpriteName, out sprite))
            {
                return sprite;
            }

            if (ObjectDB.instance != null)
            {
                var tokenItem = ObjectDB.instance.GetItemPrefab(isGold ? "GoldBountyToken" : "IronBountyToken");
                if (tokenItem != null)
                {
                    sprite = tokenItem.GetComponent<ItemDrop>().m_itemData.GetIcon();
                    if (sprite != null)
                    {
                        _cachedTrophySprites.Add(noTrophySpriteName, sprite);
                    }
                    return sprite;
                }
            }

            return null;
        }

        public static string GetBountyName(BountyInfo bountyInfo)
        {
            return Localization.instance.Localize(string.IsNullOrEmpty(bountyInfo.TargetName) ?
                GetMonsterName(bountyInfo.Target.MonsterID) :
                bountyInfo.TargetName);
        }

        public static string GetMonsterName(string monsterID)
        {
            var monsterPrefab = ZNetScene.instance.GetPrefab(monsterID);
            return monsterPrefab?.GetComponent<Character>()?.m_name ?? monsterID;
        }

        public static void OnZNetStart()
        {
            SecretStash.OnZNetStart();
            Gamble.OnZNetStart();
            TreasureMaps.OnZNetStart();
            Bounties.OnZNetStart();
        }

        public static void OnZNetDestroyed()
        {
            SecretStash.OnZNetDestroyed();
            Gamble.OnZNetDestroyed();
            TreasureMaps.OnZNetDestroyed();
            Bounties.OnZNetDestroyed();
        }

        public static void OnWorldSave()
        {
            SecretStash.OnWorldSave();
            Gamble.OnWorldSave();
            TreasureMaps.OnWorldSave();
            Bounties.OnWorldSave();
        }
    }
}
