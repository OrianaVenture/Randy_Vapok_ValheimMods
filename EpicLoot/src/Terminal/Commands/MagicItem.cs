using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EpicLoot;

public static partial class MagicCommands
{
    private static void SpawnMagicItem(Terminal.ConsoleEventArgs args)
    {
        if (Player.m_localPlayer == null)
        {
            args.Context.AddString("> Local player is null");
            return;
        }
            
        string rarity = args.GetString(2, "random");
        string itemPrefabName = args.GetString(3, "random");
        int count = args.GetInt(4, 1);
        int effectCount = args.GetInt(5, -1);
        
        args.Context.AddString($"> magicitem: rarity: {rarity}, item: {itemPrefabName}, count: {count}, effects: {effectCount}");

        List<string> allItemNames = null;
        
        LootRoller.CheatEffectCount = effectCount;

        for (int i = 0; i < count; ++i)
        {
            float[] rarityTable = GetRarityTable(rarity);

            if (itemPrefabName == "random")
            {
                allItemNames ??= GetEnchantableItemNames();
                
                WeightedRandomCollection<string> weightedRandomTable =
                    new WeightedRandomCollection<string>(allItemNames, _ => 1);
                itemPrefabName = weightedRandomTable.Roll();
            }

            if (ObjectDB.instance.GetItemPrefab(itemPrefabName) == null)
            {
                args.Context.AddString($"> Could not find item: {itemPrefabName}");
                break;
            }

            args.Context.AddString($">  {i + 1} - rarity: [{string.Join(", ", rarityTable)}], item: {itemPrefabName}");

            LootTable loot = new LootTable()
            {
                Object = "Console",
                Drops = [[1, 1]],
                Loot =
                [
                    new LootDrop()
                    {
                        Item = itemPrefabName,
                        Rarity = rarityTable
                    }
                ]
            };

            Vector3 dropPoint = GetItemSpawnPosition(Player.m_localPlayer);
            LootRoller.CheatRollingItem = true;
            LootRoller.RollLootTableAndSpawnObjects(loot, 1, loot.Object, dropPoint);
            LootRoller.CheatRollingItem = false;
        }
        LootRoller.CheatEffectCount = -1;
    }

    private static List<string> GetSpawnMagicItemOptions(int i) =>
        i switch
        {
            2 => GetRarityOptions(),
            3 => GetItemOptions(),
            _ => [],
        };

    private static List<string> GetEnchantableItemNames() => ObjectDB.instance ? 
        ObjectDB.instance.m_items
        .Where(x => EpicLoot.CanBeMagicItem(x.GetComponent<ItemDrop>().m_itemData))
        .Where(x => x.name != "HelmetDverger" && x.name != "BeltStrength" && x.name != "Wishbone")
        .Select(x => x.name)
        .ToList() : 
        [];

    private static Vector3 GetItemSpawnPosition(Player player)
    {
        Vector3 randomOffset = UnityEngine.Random.insideUnitSphere;
        Vector3 dropPoint = player.transform.position +
                            player.transform.forward * 3 + Vector3.up * 1.5f + randomOffset;
        return dropPoint;
    }
}