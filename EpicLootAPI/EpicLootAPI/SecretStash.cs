using System;
using JetBrains.Annotations;

namespace EpicLootAPI;

[PublicAPI]
public enum SecretStashType
{
    Materials, 
    RandomItems, 
    OtherItems,
    Gamble,
    Sale
}

[Serializable][PublicAPI]
public class SecretStashItem
{
    public string Item;
    public int CoinsCost;
    public int ForestTokenCost;
    public int IronBountyTokenCost;
    public int GoldBountyTokenCost;
    public SecretStashItem(SecretStashType type, string item)
    {
        Item = item;
        EpicLoot.SecretStashes.AddOrSet(type, this);
    }
}