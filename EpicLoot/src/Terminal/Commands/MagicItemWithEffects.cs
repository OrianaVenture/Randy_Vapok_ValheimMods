using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EpicLoot;

public static partial class MagicCommands
{
    private static void SpawnMagicItemWithEffects(Terminal.ConsoleEventArgs args)
    {
        if (Player.m_localPlayer == null)
        {
            args.Context.AddString("> Local Player is null");
            return;
        }
        string effectArg = args.GetString(2);
        string itemPrefabNameArg = args.GetString(3);
        
        if (string.IsNullOrEmpty(effectArg) ||
            string.IsNullOrEmpty(itemPrefabNameArg))
        {
            args.Context.AddString("> Specify effectType, itemID");
            return;
        }
        
        args.Context.AddString($"magicitem - {itemPrefabNameArg} with effect: {effectArg}");
        
        MagicItemEffectDefinition magicItemEffectDef = MagicItemEffectDefinitions.Get(effectArg);
        if (magicItemEffectDef == null)
        {
            args.Context.AddString($"> Could not find effect: {effectArg}");
            return;
        }
        
        GameObject itemPrefab = ObjectDB.instance.GetItemPrefab(itemPrefabNameArg);
        if (itemPrefab == null)
        {
            args.Context.AddString($"> Could not find item: {itemPrefabNameArg}");
            return;
        }
        ItemDrop.ItemData fromItemData = itemPrefab.GetComponent<ItemDrop>().m_itemData;
        if (!EpicLoot.CanBeMagicItem(fromItemData))
        {
            args.Context.AddString($"> Can't be magic item: {itemPrefabNameArg}");
            return;
        }
        MagicItemEffectRequirements effectRequirements = magicItemEffectDef.Requirements;
        ItemRarity itemRarity = effectRequirements.AllowedRarities.Count == 0 ? ItemRarity.Magic :
            effectRequirements.AllowedRarities.First();
        float[] rarityTable = GetRarityTable(itemRarity.ToString());
        LootTable loot = new LootTable
        {
            Object = "Console",
            Drops = [[1, 1]],
            Loot =
            [
                new LootDrop()
                {
                    Item = itemPrefab.name,
                    Rarity = rarityTable
                }
            ]
        };
        
        Vector3 dropPoint = GetItemSpawnPosition(Player.m_localPlayer);
        LootRoller.CheatRollingItem = true;
        LootRoller.CheatForceMagicEffect = true;
        LootRoller.ForcedMagicEffect = effectArg;
        LootRoller.RollLootTableAndSpawnObjects(loot, 1, loot.Object, dropPoint);
        LootRoller.CheatForceMagicEffect = false;
        LootRoller.ForcedMagicEffect = string.Empty;
        LootRoller.CheatRollingItem = false;
    }

    private static List<string> GetSpawnMagicItemWithEffectsOptions(int i) => i switch
    {
        2 => GetEffectOptions(),
        3 => GetItemOptions(),
        _ => [],
    };
}