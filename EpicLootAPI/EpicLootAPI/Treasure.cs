using System;
using JetBrains.Annotations;

namespace EpicLootAPI;

[Serializable][PublicAPI]
public class TreasureMapBiomeInfoConfig
{
    public Heightmap.Biome Biome;
    public int Cost;
    public int ForestTokens;
    public int GoldTokens;
    public int IronTokens;
    public int Coins;
    public float MinRadius;
    public float MaxRadius;
    public TreasureMapBiomeInfoConfig(Heightmap.Biome biome, int cost, float minRadius, float maxRadius)
    {
        Biome = biome;
        Cost = cost;
        MinRadius = minRadius;
        MaxRadius = maxRadius;

        EpicLoot.Treasures.Add(this);
    }
}