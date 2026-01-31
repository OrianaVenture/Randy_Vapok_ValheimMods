using System;
using System.Collections.Generic;
using System.Linq;
using EpicLoot.GatedItemType;
using EpicLoot.LegendarySystem;
using UnityEngine;

namespace EpicLoot;

public static partial class MagicCommands
{
    private static void SpawnLegendaryMagicItem(Terminal.ConsoleEventArgs args)
    {
        string legendaryID = args.GetString(2);
        if (string.IsNullOrEmpty(legendaryID))
        {
            args.Context.AddString("> Specify legendaryID, itemID (optional)");
            return;
        }
        if (!UniqueLegendaryHelper.TryGetLegendaryInfo(legendaryID, out LegendaryInfo itemInfo))
        {
            args.Context.AddString($"> Could not find legendary/mythic info for legendaryID: ({legendaryID})");
            return;
        }

        string itemRarity = args.GetString(3, "Legendary");

        if (!Enum.TryParse(itemRarity, true, out ItemRarity rarity))
        {
            args.Context.AddString($"> Invalid rarity: {rarity}");
            return;
        }

        if (rarity is not (ItemRarity.Legendary or ItemRarity.Mythic))
        {
            args.Context.AddString($"> Invalid rarity: {rarity}");
            return;
        }
        
        string itemPrefabName = args.GetString(4, null);

        if (string.IsNullOrEmpty(itemPrefabName))
        {
            itemPrefabName = GetRandomItemName(rarity, itemInfo);
        }

        if (string.IsNullOrEmpty(itemPrefabName))
        {
            args.Context.AddString($"> Could not find suitable item for legendaryID: ({legendaryID})");
            return;
        }
        
        LootTable loot = new LootTable
        {
            Object = "Console",
            Drops = [[1, 1]],
            Loot =
            [
                new LootDrop
                {
                    Item = itemPrefabName,
                    Rarity = GetRarityTable(rarity.ToString())
                }
            ]
        };

        if (rarity == ItemRarity.Legendary)
        {
            LootRoller.CheatForceLegendary = legendaryID;
        }
        else
        {
            LootRoller.CheatForceMythic = legendaryID;
        }

        bool previousDisableGatingState = LootRoller.CheatDisableGating;
        LootRoller.CheatDisableGating = true;

        Vector3 randomOffset = UnityEngine.Random.insideUnitSphere;
        Vector3 dropPoint = Player.m_localPlayer.transform.position +
                            Player.m_localPlayer.transform.forward * 3 + Vector3.up * 1.5f + randomOffset;
        LootRoller.CheatRollingItem = true;
        LootRoller.RollLootTableAndSpawnObjects(loot, 1, loot.Object, dropPoint);

        LootRoller.CheatRollingItem = false;
        LootRoller.CheatForceLegendary = null;
        LootRoller.CheatForceMythic = null;
        LootRoller.CheatDisableGating = previousDisableGatingState;
    }

    private static List<string> GetMagicItemLegendaryOptions(int i) => i switch
    {
        2 => UniqueLegendaryHelper.LegendaryInfo.Keys
            .Union(UniqueLegendaryHelper.MythicInfo.Keys)
            .ToList(),
        3 => ["Legendary", "Mythic"],
        4 => GetItemOptions(),
        _ => []
    };
}