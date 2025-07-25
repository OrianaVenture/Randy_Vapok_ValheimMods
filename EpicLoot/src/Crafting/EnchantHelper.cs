using EpicLoot.CraftingV2;
using System;
using System.Collections.Generic;
using System.IO.Ports;

namespace EpicLoot.Crafting
{
    public class EnchantHelper
    {
        public static List<KeyValuePair<ItemDrop, int>> GetEnchantCosts(ItemDrop.ItemData item, ItemRarity rarity)
        {
            var costList = new List<KeyValuePair<ItemDrop, int>>();

            var enchantCostDef = EnchantCostsHelper.GetEnchantCost(item, rarity);
            if (enchantCostDef == null)
            {
                return costList;
            }

            foreach (var itemAmountConfig in enchantCostDef)
            {
                var prefab = ObjectDB.instance.GetItemPrefab(itemAmountConfig.Item).GetComponent<ItemDrop>();
                if (prefab == null)
                {
                    EpicLoot.LogWarning($"Tried to add unknown item ({itemAmountConfig.Item}) to enchant cost for item ({item.m_shared.m_name})");
                    continue;
                }
                costList.Add(new KeyValuePair<ItemDrop, int>(prefab, itemAmountConfig.Amount));
            }

            return costList;
        }

        public static List<KeyValuePair<ItemDrop, int>> GetIdentifyCost(List<Tuple<ItemDrop.ItemData, int>> items, LootRoller.LootRollCategories category) {
            var costList = new Dictionary<ItemDrop, int>();
            foreach (var item in items) {
                Enum.TryParse<Heightmap.Biome>(item.Item1.m_dropPrefab.name.Split('_')[0], out Heightmap.Biome biome);
                EpicLoot.Log($"Looking up identify cost for {item.Item1.m_shared.m_name} with rarity {item.Item1.GetRarity()} in category {category} and biome {biome}");
                var identifyCosts = EnchantCostsHelper.GetIdentifyCosts(category, item.Item1.GetRarity(), biome);
                if (identifyCosts.Count == 0) { continue; }
                foreach (var costEntry in identifyCosts) {
                    EpicLoot.Log($"Adding identify cost for {item.Item1.m_shared.m_name} ({item.Item1.m_shared.m_name}) - {costEntry.Item} x{costEntry.Amount}");
                    var prefab = ObjectDB.instance.GetItemPrefab(costEntry.Item).GetComponent<ItemDrop>();
                    if (prefab == null) {
                        EpicLoot.LogWarning($"Tried to add unknown item ({costEntry.Item}) to identify cost for item ({item.Item1.m_shared.m_name})");
                        continue;
                    }
                    var cost_for_stack = costEntry.Amount * item.Item2;
                    if (costList.ContainsKey(prefab)) {
                        costList[prefab] += cost_for_stack;
                    } else {
                        costList.Add(prefab, cost_for_stack);
                    }
                }
            }

            List <KeyValuePair<ItemDrop, int>> results = new List<KeyValuePair<ItemDrop, int>>();
            foreach (var kvp in costList) {
                results.Add(new KeyValuePair<ItemDrop, int>(kvp.Key, kvp.Value));
            }
            return results;
        }

        public static List<KeyValuePair<ItemDrop, int>> GetRuneCost(ItemDrop.ItemData item, ItemRarity rarity, RuneActions operation)
        {
            //EpicLoot.Log($"Looking up cost for {item} with rarity {rarity} using operation {operation}");
            var costList = new List<KeyValuePair<ItemDrop, int>>();

            var enchantCostDef = EnchantCostsHelper.GetRuneCost(item, rarity, operation);
            if (enchantCostDef == null)
            {
                return costList;
            }

            foreach (var itemAmountConfig in enchantCostDef)
            {
                var prefab = ObjectDB.instance.GetItemPrefab(itemAmountConfig.Item).GetComponent<ItemDrop>();
                if (prefab == null)
                {
                    EpicLoot.LogWarning($"Tried to add unknown item ({itemAmountConfig.Item}) to rune cost for item ({item.m_shared.m_name})");
                    continue;
                }
                costList.Add(new KeyValuePair<ItemDrop, int>(prefab, itemAmountConfig.Amount));
            }

            return costList;
        }
    }
}
