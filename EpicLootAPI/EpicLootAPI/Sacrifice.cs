using System;
using System.Collections.Generic;
using System.ComponentModel;
using JetBrains.Annotations;

namespace EpicLootAPI;

[Serializable][PublicAPI]
public class ItemAmount
{
    public string Item;
    public int Amount;

    public ItemAmount(string item, int amount = 1)
    {
        Item = item;
        Amount = amount;
    }
}

[Serializable][PublicAPI]
public class Sacrifice
{
    [Description("Conditional, checks item needs to be magic")]
    public bool IsMagic;
    [Description("Can be null")]
    public ItemRarity Rarity;
    [Description("Conditional, if empty, does not check if item is of correct type")]
    public List<string> ItemTypes = new List<string>();
    [Description("Conditional, if empty, does not check if item shared name is in list")]
    public List<string> ItemNames = new List<string>();
    public List<ItemAmount> Products = new List<ItemAmount>();
    [Description("Create a new intance of a disenchant product entry")]
    public Sacrifice()
    {
        EpicLoot.Sacrifices.Add(this);
    }
    public void AddRequiredItemType(params ItemDrop.ItemData.ItemType[] types)
    {
        foreach (ItemDrop.ItemData.ItemType type in types) ItemTypes.Add(type.ToString());
    }
}