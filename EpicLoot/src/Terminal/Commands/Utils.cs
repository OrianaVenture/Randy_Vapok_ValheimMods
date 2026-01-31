using System.Collections.Generic;
using System.Linq;
using EpicLoot.GatedItemType;
using EpicLoot.LegendarySystem;
using JetBrains.Annotations;
using UnityEngine;

namespace EpicLoot;

[UsedImplicitly]
public static partial class MagicCommands
{
    public const string HEX_Gray = "#B2BEB5";
    public const string HEX_LightRed = "#ff8080ff";
    private static List<string> GetCreatureNames(int i) => 
        ZNetScene.instance ? ZNetScene.instance.m_prefabs
            .Where(p => p.GetComponent<Character>())
            .Select(c => c.name)
            .ToList()
        : [];

    private static List<string> GetPrintOptions(int i) =>
    [
        "loottable", "abilities", "adventuredata", "enchantcosts",
        "enchantingupgrades", "iteminfo", "itemnames", "legendaries",
        "magiceffects", "materialconversion", "recipes"
    ];
    
    private static List<string> GetEffectOptions() => MagicItemEffectDefinitions.AllDefinitions.Keys.ToList();

    private static List<string> GetRarityOptions() => ["random", "magic", "rare", "epic", "legendary", "mythic"];

    private static List<string> GetItemOptions() => 
        ObjectDB.instance ? ObjectDB.instance.m_items
        .Where(x => EpicLoot.CanBeMagicItem(x.GetComponent<ItemDrop>().m_itemData))
        .Where(x => x.name != "HelmetDverger" && x.name != "BeltStrength" && x.name != "Wishbone")
        .Select(x => x.name)
        .ToList() : 
        [];
    
    private static float[] GetRarityTable(string rarityName)
    {
        float[] rarityTable = [1, 1, 1, 1, 1];
        switch (rarityName.ToLowerInvariant())
        {
            case "magic":
                rarityTable = [1, 0, 0, 0, 0];
                break;
            case "rare":
                rarityTable = [0, 1, 0, 0, 0];
                break;
            case "epic":
                rarityTable = [0, 0, 1, 0, 0];
                break;
            case "legendary":
                rarityTable = [0, 0, 0, 1, 0];
                break;
            case "mythic":
                rarityTable = [0, 0, 0, 0, 1];
                break;
        }

        return rarityTable;
    }

    private static string GetRandomItemName(ItemRarity rarity, LegendaryInfo itemInfo)
    {
        MagicItem dummyMagicItem = new MagicItem { Rarity = rarity };
        List<ItemDrop> allowedItems = new List<ItemDrop>();
        foreach (string itemName in GatedItemTypeHelper.AllItemsWithDetails.Keys)
        {
            GameObject itemPrefab = ObjectDB.instance.GetItemPrefab(itemName);
            if (itemPrefab == null)
            {
                continue;
            }

            ItemDrop itemDrop = itemPrefab.GetComponent<ItemDrop>();
            if (itemDrop == null)
            {
                continue;
            }

            ItemDrop.ItemData itemData = itemDrop.m_itemData;
            itemData.m_dropPrefab = itemPrefab;
            bool checkRequirements = itemInfo.Requirements.CheckRequirements(itemData, dummyMagicItem);

            if (checkRequirements)
            {
                allowedItems.Add(itemDrop);
            }
        }

        if (allowedItems.Count == 0)
        {
            return string.Empty;
        }

        int selected = UnityEngine.Random.Range(0, allowedItems.Count);
        return allowedItems.ElementAt(selected).name;
    }
}