using System;
using System.Collections.Generic;
using System.Linq;

namespace EpicLoot.Biome
{
    public static class BiomeDataManager
    {
        public static BiomeDataConfig Config;

        // Primary lookup by biome string (e.g., "Meadows", "BlackForest", "8192")
        private static Dictionary<string, BiomeDefinitionConfig> _biomeDefinitionLookup;

        // Secondary lookup by Heightmap.Biome enum value for backward compatibility
        private static Dictionary<Heightmap.Biome, BiomeDefinitionConfig> _biomeEnumLookup;

        private static List<string> _biomesInOrder;

        #nullable enable
        public static event Action? OnBiomeDataInitialized;
        #nullable disable

        public static void Initialize(BiomeDataConfig config)
        {
            Config = config;
            BuildLookups();
            OnBiomeDataInitialized?.Invoke();
            EpicLoot.Log($"[BiomeDataManager] Initialized with {Config.BiomeDefinitions.Count} biome definitions");
        }

        public static BiomeDataConfig GetCFG()
        {
            return Config;
        }

        private static void BuildLookups()
        {
            _biomeDefinitionLookup = new Dictionary<string, BiomeDefinitionConfig>(StringComparer.OrdinalIgnoreCase);
            _biomeEnumLookup = new Dictionary<Heightmap.Biome, BiomeDefinitionConfig>();
            _biomesInOrder = new List<string>();

            if (Config?.BiomeDefinitions == null)
            {
                EpicLoot.LogWarning("[BiomeDataManager] No biome definitions found in config");
                return;
            }

            // Sort by Order and build lookups
            var sortedBiomes = Config.BiomeDefinitions.OrderBy(b => b.Order).ToList();

            foreach (var biomeConfig in sortedBiomes)
            {
                // Primary lookup by string
                if (!_biomeDefinitionLookup.ContainsKey(biomeConfig.Biome))
                {
                    _biomeDefinitionLookup[biomeConfig.Biome] = biomeConfig;
                    _biomesInOrder.Add(biomeConfig.Biome);
                    EpicLoot.Log($"[BiomeDataManager] Registered biome: {biomeConfig.Biome} (Order={biomeConfig.Order})");

                    // Secondary lookup by enum (if parseable)
                    if (biomeConfig.TryGetBiomeEnum(out var biomeEnum))
                    {
                        if (!_biomeEnumLookup.ContainsKey(biomeEnum))
                        {
                            _biomeEnumLookup[biomeEnum] = biomeConfig;
                        }
                    }
                }
                else
                {
                    EpicLoot.LogWarning($"[BiomeDataManager] Duplicate biome definition for {biomeConfig.Biome}, skipping");
                }
            }
        }

        /// <summary>
        /// Converts a Heightmap.Biome enum to its string representation for lookup.
        /// </summary>
        public static string BiomeToString(Heightmap.Biome biome)
        {
            return biome.ToString();
        }

        /// <summary>
        /// Gets the biome definition config by biome string.
        /// </summary>
        public static BiomeDefinitionConfig GetBiomeDefinition(string biome)
        {
            if (_biomeDefinitionLookup == null || string.IsNullOrEmpty(biome))
            {
                return null;
            }

            return _biomeDefinitionLookup.TryGetValue(biome, out var config) ? config : null;
        }

        /// <summary>
        /// Gets the biome definition config by Heightmap.Biome enum.
        /// </summary>
        public static BiomeDefinitionConfig GetBiomeDefinition(Heightmap.Biome biome)
        {
            if (_biomeEnumLookup == null)
            {
                return null;
            }

            return _biomeEnumLookup.TryGetValue(biome, out var config) ? config : null;
        }

        /// <summary>
        /// Gets all biome strings in progression order.
        /// </summary>
        public static List<string> GetBiomesInOrder()
        {
            return _biomesInOrder ?? new List<string>();
        }

        /// <summary>
        /// Gets the display color for a biome.
        /// </summary>
        public static string GetBiomeColor(string biome)
        {
            var config = GetBiomeDefinition(biome);
            return config?.DisplayColor ?? "#ffffff";
        }

        /// <summary>
        /// Gets the display color for a biome by enum.
        /// </summary>
        public static string GetBiomeColor(Heightmap.Biome biome)
        {
            var config = GetBiomeDefinition(biome);
            return config?.DisplayColor ?? "#ffffff";
        }

        /// <summary>
        /// Gets the biome order/index for sorting purposes.
        /// </summary>
        public static int GetBiomeOrder(string biome)
        {
            var config = GetBiomeDefinition(biome);
            return config?.Order ?? 999;
        }

        /// <summary>
        /// Gets the biome order/index for sorting purposes by enum.
        /// </summary>
        public static int GetBiomeOrder(Heightmap.Biome biome)
        {
            var config = GetBiomeDefinition(biome);
            return config?.Order ?? 999;
        }

        /// <summary>
        /// Gets the boss prefab name for a biome.
        /// </summary>
        public static string GetBossPrefab(string biome)
        {
            var config = GetBiomeDefinition(biome);
            return config?.BossPrefab;
        }

        /// <summary>
        /// Gets the boss prefab name for a biome by enum.
        /// </summary>
        public static string GetBossPrefab(Heightmap.Biome biome)
        {
            var config = GetBiomeDefinition(biome);
            return config?.BossPrefab;
        }

        /// <summary>
        /// Gets the boss defeated key for a biome.
        /// </summary>
        public static string GetBossDefeatedKey(string biome)
        {
            var config = GetBiomeDefinition(biome);
            return config?.BossDefeatedKey;
        }

        /// <summary>
        /// Gets the boss defeated key for a biome by enum.
        /// </summary>
        public static string GetBossDefeatedKey(Heightmap.Biome biome)
        {
            var config = GetBiomeDefinition(biome);
            return config?.BossDefeatedKey;
        }

        /// <summary>
        /// Gets the treasure map config for a biome.
        /// </summary>
        public static BiomeTreasureMapConfig GetTreasureMapConfig(string biome)
        {
            var config = GetBiomeDefinition(biome);
            return config?.TreasureMap;
        }

        /// <summary>
        /// Gets the treasure map config for a biome by enum.
        /// </summary>
        public static BiomeTreasureMapConfig GetTreasureMapConfig(Heightmap.Biome biome)
        {
            var config = GetBiomeDefinition(biome);
            return config?.TreasureMap;
        }

        /// <summary>
        /// Gets the identify loot lists for a biome.
        /// </summary>
        public static BiomeIdentifyLootListsConfig GetIdentifyLootLists(string biome)
        {
            var config = GetBiomeDefinition(biome);
            return config?.IdentifyLootLists;
        }

        /// <summary>
        /// Gets the identify loot lists for a biome by enum.
        /// </summary>
        public static BiomeIdentifyLootListsConfig GetIdentifyLootLists(Heightmap.Biome biome)
        {
            var config = GetBiomeDefinition(biome);
            return config?.IdentifyLootLists;
        }

        /// <summary>
        /// Gets the identify loot list for a specific biome and identify type.
        /// </summary>
        /// <param name="biome">The biome string (e.g., "Meadows", "BlackForest")</param>
        /// <param name="identifyType">The identify type (e.g., "Random", "Weapon", "Armor", "Utility")</param>
        /// <returns>List of loot table names for the specified biome and type</returns>
        public static List<string> GetIdentifyLootListByType(string biome, string identifyType)
        {
            var lootLists = GetIdentifyLootLists(biome);
            if (lootLists == null)
            {
                return new List<string>();
            }

            return identifyType?.ToLower() switch
            {
                "random" => lootLists.Random ?? new List<string>(),
                "weapon" => lootLists.Weapon ?? new List<string>(),
                "armor" => lootLists.Armor ?? new List<string>(),
                "utility" => lootLists.Utility ?? new List<string>(),
                _ => lootLists.Random ?? new List<string>()
            };
        }

        /// <summary>
        /// Gets the identify cost config for a biome.
        /// </summary>
        public static BiomeIdentifyCostConfig GetIdentifyCost(string biome)
        {
            var config = GetBiomeDefinition(biome);
            return config?.IdentifyCost;
        }

        /// <summary>
        /// Gets the identify cost config for a biome by enum.
        /// </summary>
        public static BiomeIdentifyCostConfig GetIdentifyCost(Heightmap.Biome biome)
        {
            var config = GetBiomeDefinition(biome);
            return config?.IdentifyCost;
        }

        /// <summary>
        /// Gets the bounty targets for a biome.
        /// </summary>
        public static List<BiomeBountyTargetConfig> GetBountyTargets(string biome)
        {
            var config = GetBiomeDefinition(biome);
            return config?.BountyTargets ?? new List<BiomeBountyTargetConfig>();
        }

        /// <summary>
        /// Gets the bounty targets for a biome by enum.
        /// </summary>
        public static List<BiomeBountyTargetConfig> GetBountyTargets(Heightmap.Biome biome)
        {
            var config = GetBiomeDefinition(biome);
            return config?.BountyTargets ?? new List<BiomeBountyTargetConfig>();
        }

        /// <summary>
        /// Checks if a biome has treasure maps enabled (Cost > 0).
        /// </summary>
        public static bool IsTreasureMapEnabled(string biome)
        {
            var treasureMap = GetTreasureMapConfig(biome);
            return treasureMap != null && treasureMap.Cost > 0;
        }

        /// <summary>
        /// Checks if a biome has treasure maps enabled by enum.
        /// </summary>
        public static bool IsTreasureMapEnabled(Heightmap.Biome biome)
        {
            var treasureMap = GetTreasureMapConfig(biome);
            return treasureMap != null && treasureMap.Cost > 0;
        }

        /// <summary>
        /// Gets all biome strings that have treasure maps enabled.
        /// </summary>
        public static List<string> GetTreasureMapBiomes()
        {
            return _biomesInOrder?.Where(IsTreasureMapEnabled).ToList() ?? new List<string>();
        }

        /// <summary>
        /// Gets all biomes that have a boss defined.
        /// </summary>
        public static List<string> GetBiomesWithBosses()
        {
            if (_biomeDefinitionLookup == null)
            {
                return new List<string>();
            }

            return _biomesInOrder?
                .Where(b => !string.IsNullOrEmpty(GetBossPrefab(b)))
                .ToList() ?? new List<string>();
        }

        /// <summary>
        /// Gets all biomes that have bounty targets defined.
        /// </summary>
        public static List<string> GetBiomesWithBounties()
        {
            if (_biomeDefinitionLookup == null)
            {
                return new List<string>();
            }

            return _biomesInOrder?
                .Where(b => GetBountyTargets(b).Count > 0)
                .ToList() ?? new List<string>();
        }

        /// <summary>
        /// Checks if a biome definition exists.
        /// </summary>
        public static bool HasBiome(string biome)
        {
            return _biomeDefinitionLookup?.ContainsKey(biome) ?? false;
        }

        /// <summary>
        /// Checks if a biome definition exists by enum.
        /// </summary>
        public static bool HasBiome(Heightmap.Biome biome)
        {
            return _biomeEnumLookup?.ContainsKey(biome) ?? false;
        }
    }
}
