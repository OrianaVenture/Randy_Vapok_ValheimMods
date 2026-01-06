using System;
using System.Collections.Generic;

namespace EpicLoot.Biome
{
    [Serializable]
    public class BiomeTreasureMapConfig
    {
        public int Cost;
        public int ForestTokens;
        public int IronTokens;
        public int GoldTokens;
        public int Coins;
        public float MinRadius;
        public float MaxRadius;
    }

    [Serializable]
    public class BiomeIdentifyLootListsConfig
    {
        public List<string> Random = new List<string>();
        public List<string> Weapon = new List<string>();
        public List<string> Armor = new List<string>();
        public List<string> Utility = new List<string>();
    }

    [Serializable]
    public class BiomeIdentifyCostConfig
    {
        public string Item;
        /// <summary>
        /// Cost amounts indexed by rarity: [Magic, Rare, Epic, Legendary, Mythic]
        /// </summary>
        public List<int> AmountPerRarity = new List<int>();
    }

    [Serializable]
    public class BiomeBountyTargetAddConfig
    {
        public string ID;
        public int Count;
    }

    [Serializable]
    public class BiomeBountyTargetConfig
    {
        public string TargetID;
        public int RewardGold;
        public int RewardIron;
        public int RewardCoins;
        public List<BiomeBountyTargetAddConfig> Adds = new List<BiomeBountyTargetAddConfig>();
    }

    [Serializable]
    public class BiomeDefinitionConfig
    {
        /// <summary>
        /// Biome identifier/friendly name (e.g., "Meadows", "BlackForest").
        /// This is the primary key used for lookups.
        /// </summary>
        public string Biome;

        /// <summary>
        /// The Heightmap.Biome enum value as an integer.
        /// Vanilla biomes use their standard values (e.g., Meadows=1, BlackForest=8).
        /// Modded biomes extend the enum with their own values (e.g., 8192).
        /// </summary>
        public int BiomeID;

        /// <summary>
        /// Sort order for biome progression. Lower values appear first.
        /// </summary>
        public int Order;

        /// <summary>
        /// Hex color code for UI display (e.g., "#75d966" for Meadows green).
        /// </summary>
        public string DisplayColor;

        /// <summary>
        /// The prefab name of the boss for this biome (e.g., "Eikthyr").
        /// </summary>
        public string BossPrefab;

        /// <summary>
        /// The global key set when the boss is defeated (e.g., "defeated_eikthyr").
        /// </summary>
        public string BossDefeatedKey;

        public BiomeTreasureMapConfig TreasureMap = new BiomeTreasureMapConfig();
        public BiomeIdentifyLootListsConfig IdentifyLootLists = new BiomeIdentifyLootListsConfig();
        public BiomeIdentifyCostConfig IdentifyCost = new BiomeIdentifyCostConfig();
        public List<BiomeBountyTargetConfig> BountyTargets = new List<BiomeBountyTargetConfig>();

        /// <summary>
        /// Gets the Heightmap.Biome enum value from the BiomeID.
        /// </summary>
        public Heightmap.Biome GetBiomeEnum() => (Heightmap.Biome)BiomeID;
    }

    [Serializable]
    public class BiomeDataConfig
    {
        public List<BiomeDefinitionConfig> BiomeDefinitions = new List<BiomeDefinitionConfig>();
    }
}
