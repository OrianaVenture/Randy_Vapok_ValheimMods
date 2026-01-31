using System.Collections.Generic;
using System.Linq;
using EpicLoot.LegendarySystem;
using UnityEngine;

namespace EpicLoot;

public static partial class MagicCommands
{
    private static void SpawnMagicItemSet(Terminal.ConsoleEventArgs args)
    {
        if (Player.m_localPlayer == null)
        {
            args.Context.AddString("> Local player is null");
            return;
        }
        
        string setID = args.GetString(2);
        if (string.IsNullOrEmpty(setID))
        {
            args.Context.AddString("> Specify Set ID");
            return;
        }
        
        args.Context.AddString($"magicitemset - setID:{setID}");
        
        if (!UniqueLegendaryHelper.TryGetLegendarySetInfo(setID,
                out LegendarySetInfo setInfo, out ItemRarity rarity))
        {
            args.Context.AddString($"> Could not find set info for setID: ({setID})");
            return;
        }

        string itemPrefabName = args.GetString(3);
        
        foreach (string legendaryID in setInfo.LegendaryIDs)
        {
            if (!UniqueLegendaryHelper.TryGetLegendaryInfo(legendaryID, out LegendaryInfo itemInfo))
            {
                args.Context.AddString($"> Could not find legendary/mythic info for legendaryID: ({legendaryID})");

                return;
            }

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
                    new LootDrop()
                    {
                        Item = itemPrefabName,
                        Rarity = GetRarityTable(rarity.ToString()),
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

            Vector3 dropPoint = GetItemSpawnPosition(Player.m_localPlayer);
            LootRoller.CheatRollingItem = true;
            LootRoller.RollLootTableAndSpawnObjects(loot, 1, loot.Object, dropPoint);

            LootRoller.CheatRollingItem = false;
            LootRoller.CheatForceLegendary = null;
            LootRoller.CheatForceMythic = null;
            LootRoller.CheatDisableGating = previousDisableGatingState;
        }
    }

    private static List<string> GetMagicItemSetOptions(int i) => i switch
    {
        2 => GetLegendaryMythicSetIDs(),
        3 => GetItemOptions(),
        _ => []
    };

    private static List<string> GetLegendaryMythicSetIDs() => 
        UniqueLegendaryHelper.LegendarySets.Keys
        .Union(UniqueLegendaryHelper.MythicSets.Keys)
        .ToList();
}