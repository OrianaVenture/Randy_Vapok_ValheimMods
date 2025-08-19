using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace EpicLootAPI;

[Serializable][PublicAPI]
public class BountyMinion
{
    public string ID;
    public int Count;

    public BountyMinion(string ID, int count)
    {
        this.ID = ID;
        Count = count;
    }
}

[Serializable][PublicAPI]
public class BountyTarget
{
    public Heightmap.Biome Biome;
    public string TargetID;
    public int RewardGold;
    public int RewardIron;
    public int RewardCoins;
    public List<BountyMinion> Adds = new List<BountyMinion>();

    public BountyTarget(Heightmap.Biome biome, string targetID)
    {
        Biome = biome;
        TargetID = targetID;
            
        EpicLoot.BountyTargets.Add(this);
    }
}