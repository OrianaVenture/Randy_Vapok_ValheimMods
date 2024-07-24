using System.Collections.Generic;
using System.Linq;
using BepInEx;
using Common;
using EpicLoot.Adventure.Feature;

namespace EpicLoot.GatedItemType
{
    public enum GatedItemTypeMode
    {
        Unlimited,
        BossKillUnlocksCurrentBiomeItems,
        BossKillUnlocksNextBiomeItems,
        PlayerMustKnowRecipe,
        PlayerMustHaveCraftedItem
    }

    public static class GatedItemTypeHelper
    {
        public static readonly List<ItemTypeInfo> ItemInfos = new List<ItemTypeInfo>();
        public static readonly Dictionary<string, ItemTypeInfo> ItemInfoByID = new Dictionary<string, ItemTypeInfo>();
        public static readonly Dictionary<string, List<string>> ItemsPerBoss = new Dictionary<string, List<string>>();
        public static readonly Dictionary<string, string> BossPerItem = new Dictionary<string, string>();

        public static void Initialize(ItemInfoConfig config)
        {
            ItemInfos.Clear();
            ItemInfoByID.Clear();
            ItemsPerBoss.Clear();
            BossPerItem.Clear();

            foreach (var info in config.ItemInfo)
            {
                ItemInfos.Add(info);
               
                foreach (var itemID in info.Items)
                {
                    if (!ItemInfoByID.ContainsKey(itemID))
                    {
                        ItemInfoByID.Add(itemID, info);
                    }
                    else
                    {
                        EpicLoot.LogWarning($"Duplicate entry found for ItemInfo: {itemID}. " +
                            $"Please fix your configuration.");
                    }
                }

                foreach (var entry in info.ItemsByBoss)
                {
                    if (ItemsPerBoss.ContainsKey(entry.Key))
                    {
                        ItemsPerBoss[entry.Key].AddRange(entry.Value);
                    }
                    else
                    {
                        ItemsPerBoss.Add(entry.Key, entry.Value.ToList());
                    }

                    foreach (var itemID in entry.Value)
                    {
                        if (!BossPerItem.ContainsKey(itemID))
                        {
                            BossPerItem.Add(itemID, entry.Key);
                        }
                        else
                        {
                            EpicLoot.LogWarning($"Duplicate entry found for ItemInfo, " +
                                $"BossPerItem: {itemID} with boss {entry.Key}. " +
                                $"Please fix your configuration.");
                        }
                    }
                }
            }
        }

        public static string GetGatedItemID(string itemID, int depth)
        {
            return GetGatedItemID(itemID, EpicLoot.GetGatedItemTypeMode(), depth);
        }

        public static string GetGatedFallbackItem(string infoType, GatedItemTypeMode mode,
            string originalItemID, int depth)
        {
            if (depth > 10)
            {
                // Failed to find item, escape
                return null;
            }

            depth++;

            if (ItemInfos.TryFind(x => x.Type.Equals(infoType), out var info))
            {
                // infoType is a category
                string fallbackItem = GetItemFromCategory(info.Type, mode, depth);
                if (!fallbackItem.IsNullOrWhiteSpace())
                {
                    return fallbackItem;
                }
                else
                {
                    GetGatedFallbackItem(info.Fallback, mode, originalItemID, depth);
                }
            }

            if (GetItemName(infoType).IsNullOrWhiteSpace())
            {
                return null;
            }

            // This fallback is an item, not a category
            return infoType;
        }

        public static string GetGatedItemID(string itemID, GatedItemTypeMode mode, int depth)
        {
            if (string.IsNullOrEmpty(itemID))
            {
                EpicLoot.LogError($"Tried to get gated itemID with null or empty itemID!");
                return null;
            }

            if (mode == GatedItemTypeMode.Unlimited || !ItemInfoByID.TryGetValue(itemID, out var info))
            {
                return itemID;
            }

            // Why is this here?
            /*var itemName = GetItemName(itemID);
            if (itemName == null)
            {
                return null;
            }*/

            //var index = info.Items.IndexOf(itemID);

            if (info.Items.Count < 0)
            {
                // Items list is empty, no need to gate any items from of this type
                return itemID;
            }

            if (!CheckIfItemNeedsGate(mode, itemID))
            {
                // Passed item is not gated
                return itemID;
            }

            // Search all items in category
            int index = info.Items.Count - 1;
            var id = info.Items[index];

            while (CheckIfItemNeedsGate(mode, id))
            {
                if (index < 0)
                {
                    if (string.IsNullOrEmpty(info.Fallback))
                    {
                        return null;
                    }

                    return GetGatedFallbackItem(info.Fallback, mode, itemID, depth);
                }

                index--;
                itemID = info.Items[index];
            }

            return itemID;
        }

        private static string GetItemName(string itemID)
        {
            if (!EpicLoot.IsObjectDBReady())
            {
                EpicLoot.LogError($"Tried to get gated itemID ({itemID}) but ObjectDB is not initialized!");
                return null;
            }

            var itemPrefab = ObjectDB.instance.GetItemPrefab(itemID);
            if (itemPrefab == null)
            {
                EpicLoot.LogError($"Tried to get gated itemID ({itemID}) but there is no prefab with that ID!");
                return null;
            }

            var itemDrop = itemPrefab.GetComponent<ItemDrop>();
            if (itemDrop == null)
            {
                EpicLoot.LogError($"Tried to get gated itemID ({itemID}) but its prefab has no ItemDrop component!");
                return null;
            }

            var item = itemDrop.m_itemData;
            return item.m_shared.m_name;
        }

        /// <summary>
        /// Returns true if item is gated.
        /// </summary>
        private static bool CheckIfItemNeedsGate(GatedItemTypeMode mode, string itemID)
        {
            // Already checked
            /*if (mode == GatedItemTypeMode.Unlimited)
            {
                return false;
            }*/

            if (mode == GatedItemTypeMode.PlayerMustKnowRecipe)
            {
                string itemName = GetItemName(itemID);
                if (Player.m_localPlayer != null)
                {
                    return !Player.m_localPlayer.IsRecipeKnown(itemName);
                }

                return true; // Could not check
            }

            if (mode == GatedItemTypeMode.PlayerMustHaveCraftedItem)
            {
                string itemName = GetItemName(itemID);
                if (Player.m_localPlayer != null)
                {
                    return !Player.m_localPlayer.m_knownMaterial.Contains(itemName);
                }

                return true; // Could not check
            }

            if (!BossPerItem.ContainsKey(itemID))
            {
                EpicLoot.LogWarning($"Item ({itemID}) was not registered in iteminfo.json with any particular boss");
                return false;
            }

            var bossKeyForItem = BossPerItem[itemID];

            if (mode == GatedItemTypeMode.BossKillUnlocksCurrentBiomeItems)
            {
                if (string.IsNullOrEmpty(bossKeyForItem) || ZoneSystem.instance.GetGlobalKey(bossKeyForItem))
                {
                    return false;
                }

                return true; // Does not have key
            }
            
            if (mode == GatedItemTypeMode.BossKillUnlocksNextBiomeItems)
            {
                var prevBossKey = Bosses.GetPrevBossKey(bossKeyForItem);
                if (string.IsNullOrEmpty(prevBossKey) || ZoneSystem.instance.GetGlobalKey(prevBossKey))
                {
                    return false;
                }

                return true; // Does not have key
            }

            // TODO: add additional gating option for returning all past unlocked items too?

            return true; // Could not check
        }

        public static string GetItemFromCategory(string itemCategory, GatedItemTypeMode mode, int depth)
        {
            var itemInfo = ItemInfos.FirstOrDefault(x => x.Type == itemCategory);

            if (itemInfo == null)
            {
                EpicLoot.LogWarning($"Item Info for Category [{itemCategory}] not found in ItemInfo.json");
                return null;
            }
            
            return GetGatedItemID(itemInfo.Items[itemInfo.Items.Count - 1], mode, depth);
        }
    }
}
