using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EpicLoot;

public static partial class MagicCommands
{
    private static void SpawnMagicItem(Terminal.ConsoleEventArgs args)
    {
        if (Player.m_localPlayer == null) return ;
            
        string rarity = args.GetString(2, "random");
        string item = args.GetString(3, "random");
        int count = args.GetInt(4, 1);
        int effectCount = args.GetInt(5, -1);
        
        args.Context.AddString($"{TerminalManager.START_COMMAND} magicitem - rarity: {rarity}, item: {item}, count: {count}, count: {effectCount}");

        List<string> allItemNames = null;
        
        LootRoller.CheatEffectCount = effectCount;

        for (int i = 0; i < count; ++i)
        {
            float[] rarityTable = GetRarityTable(rarity);

            if (item == "random")
            {
                allItemNames ??= ObjectDB.instance.m_items
                    .Where(x => EpicLoot.CanBeMagicItem(x.GetComponent<ItemDrop>().m_itemData))
                    .Where(x => x.name != "HelmetDverger" && x.name != "BeltStrength" && x.name != "Wishbone")
                    .Select(x => x.name)
                    .ToList();
                
                WeightedRandomCollection<string> weightedRandomTable =
                    new WeightedRandomCollection<string>(allItemNames, _ => 1);
                item = weightedRandomTable.Roll();
            }

            if (ObjectDB.instance.GetItemPrefab(item) == null)
            {
                args.Context.AddString($"> Could not find item: {item}");
                break;
            }

            args.Context.AddString($">  {i + 1} - rarity: [{string.Join(", ", rarityTable)}], item: {item}");

            LootTable loot = new LootTable()
            {
                Object = "Console",
                Drops = [[1, 1]],
                Loot =
                [
                    new LootDrop()
                    {
                        Item = item,
                        Rarity = rarityTable
                    }
                ]
            };

            Vector3 randomOffset = UnityEngine.Random.insideUnitSphere;
            Vector3 dropPoint = Player.m_localPlayer.transform.position +
                                Player.m_localPlayer.transform.forward * 3 + Vector3.up * 1.5f + randomOffset;
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
}