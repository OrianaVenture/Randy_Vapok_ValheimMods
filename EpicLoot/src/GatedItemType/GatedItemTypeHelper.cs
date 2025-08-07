using System.Collections.Generic;
using System.Linq;
using BepInEx;
using EpicLoot.Adventure;
using EpicLoot.Adventure.Feature;
using EpicLoot.General;
using Jotunn.Managers;

namespace EpicLoot.GatedItemType
{
    public enum GatedPieceTypeMode
    {
        Unlimited,
        BossKillUnlocksCurrentBiomePieces,
        BossKillUnlocksNextBiomePieces
    }

    public enum GatedItemTypeMode
    {
        Unlimited,
        BossKillUnlocksCurrentBiomeItems,
        BossKillUnlocksNextBiomeItems,
        PlayerMustKnowRecipe,
        PlayerMustHaveCraftedItem
    }

    public class GatedItemDetails
    {
        public List<string> RequiredBosses { get; set; }
        public string Type { get; set; }
    }

    public class Fallback
    {
        public string Type { get; set; }
        public string Item { get; set; }
    }

    public static class GatedItemTypeHelper
    {
        public static ItemInfoConfig GatedConfig;

        public static Dictionary<string, Dictionary<string, List<string>>> ItemsByTypeAndBoss =
            new Dictionary<string, Dictionary<string, List<string>>>();
        public static Dictionary<string, GatedItemDetails> AllItemsWithDetails =
            new Dictionary<string, GatedItemDetails>();
        public static Dictionary<string, Fallback> FallbackByType = new Dictionary<string, Fallback>();

        public static List<string> BossKeysInOrder = new List<string>();

        private const string NO_BOSS = "none";

        public static void Initialize(ItemInfoConfig config)
        {
            GatedConfig = config;
            ItemsByTypeAndBoss.Clear();
            AllItemsWithDetails.Clear();
            FallbackByType.Clear();
            BossKeysInOrder.Clear();

            // Add to required lists
            foreach (ItemTypeInfo info in config.ItemInfo)
            {
                if (!FallbackByType.ContainsKey(info.Type))
                {
                    FallbackByType.Add(info.Type, new Fallback
                    {
                        Type = info.Fallback,
                        Item = info.ItemFallback
                    });
                }

                Dictionary<string, List<string>> itemsByBoss = [];
                foreach (var itemByBoss in info.ItemsByBoss)
                {
                    if (itemsByBoss.ContainsKey(itemByBoss.Key))
                    {
                        EpicLoot.Log($"Merging [{itemByBoss.Key}] entries, duplicates will be removed.");
                        itemsByBoss[itemByBoss.Key].Union(itemByBoss.Value).ToList();
                    }
                    else
                    {
                        itemsByBoss.Add(itemByBoss.Key, itemByBoss.Value);
                    }

                    foreach (var item in itemByBoss.Value)
                    {
                        if (AllItemsWithDetails.ContainsKey(item))
                        {
                            List<string> reqBosses = AllItemsWithDetails[item].RequiredBosses;
                            if (!reqBosses.Contains(itemByBoss.Key)) {
                                EpicLoot.Log($"{item} already registered, merging boss keys.");
                                reqBosses.Add(itemByBoss.Key);
                            }

                            AllItemsWithDetails[item].RequiredBosses = reqBosses;
                            continue;
                        }

                        AllItemsWithDetails.Add(item, new GatedItemDetails()
                        {
                            Type = info.Type,
                            RequiredBosses = new List<string>() { itemByBoss.Key }
                        });
                    }
                }

                if (!ItemsByTypeAndBoss.ContainsKey(info.Type))
                {
                    ItemsByTypeAndBoss.Add(info.Type, itemsByBoss);
                }
            }

            // Items can be ungated, add a dummy entry to account for this
            BossKeysInOrder.Add(NO_BOSS);

            foreach (var boss in AdventureDataManager.Config.Bounties.Bosses) {
                BossKeysInOrder.Add(boss.BossDefeatedKey);
            }
            EpicLoot.Log($"Updated Itemconfig total item entries: {AllItemsWithDetails.Keys.Count}");
        }

        /// <summary>
        /// Attempts to get a valid item of the specified type.
        /// </summary>
        public static string GetGatedItemFromType(string itemType, GatedItemTypeMode mode,
            HashSet<string> currentSelected, List<string> validBosses, bool allowDuplicate = false,
            bool allowTypeFallback = false, bool allowItemFallback = false)
        {
            if (validBosses.Count == 0)
            {
                // TODO: this should never trigger
                return FallbackByType[itemType].Item;
            }

            if (!ItemsByTypeAndBoss.ContainsKey(itemType))
            {
                return null;
            }

            string item = null;
            foreach (var boss in validBosses)
            {
                item = GetGatedItemFromBossTier(itemType, boss, currentSelected,
                    mode, new HashSet<string>(), allowTypeFallback, allowDuplicate);

                if (item != null)
                {
                    return item;
                }
            }

            if (allowItemFallback)
            {
                return FallbackByType[itemType].Item;
            }

            return null;
        }

        /// <summary>
        /// Attempts to select a item of the specified type and boss level.
        /// If an items fails to be selected can search the fallback type at the same boss tier.
        /// </summary>
        private static string GetGatedItemFromBossTier(string itemType, string boss,
            HashSet<string> currentSelected,
            GatedItemTypeMode mode,
            HashSet<string> typesSearched,
            bool allowFallback = true,
            bool allowDuplicate = false)
        {
            if (ItemsByTypeAndBoss[itemType].ContainsKey(boss)) {
                List<string> items = ItemsByTypeAndBoss[itemType][boss];
                bool gated = true;

                foreach (string item in items.shuffleList())
                {
                    gated = CheckIfItemNeedsGate(mode, item);
                    if (gated)
                    {
                        continue;
                    }

                    if (!allowDuplicate && currentSelected.Contains(item))
                    {
                        continue;
                    }

                    return item;
                }
            }

            if (allowFallback && FallbackByType.ContainsKey(itemType))
            {
                typesSearched.Add(itemType);
                Fallback fallback = FallbackByType[itemType];

                if (!fallback.Type.IsNullOrWhiteSpace() &&
                    !typesSearched.Contains(fallback.Type))
                {
                    return GetGatedItemFromBossTier(fallback.Type,
                        boss, currentSelected, mode, typesSearched, false, true);
                }
            }

            return null;
        }

        public static string GetGatedItemNameFromItemOrType(string itemOrType, GatedItemTypeMode gatedMode)
        {
            if (string.IsNullOrEmpty(itemOrType))
            {
                return null;
            }

            string type = itemOrType;
            List<string> validBosses;

            List<string> bossList = null;

            if (!ItemsByTypeAndBoss.ContainsKey(itemOrType))
            {
                // Passed string is an item
                if (gatedMode == GatedItemTypeMode.Unlimited)
                {
                    return itemOrType;
                }

                if (!CheckIfItemNeedsGate(gatedMode, itemOrType, out GatedItemDetails itemDetails))
                {
                    return itemOrType;
                }

                if (itemDetails == null)
                {
                    return null;
                }

                type = itemDetails.Type;
                bossList = itemDetails.RequiredBosses;
            }

            validBosses = DetermineValidBosses(gatedMode, false, bossList);

            return GetGatedItemFromType(type, gatedMode, new HashSet<string> { }, validBosses, true, true, true);
        }

        /// <summary>
        /// Returns a list of defeated bosses in the same order as defined in the configurations.
        /// If gating mode unlocks next biome it will also include the next tier of bosses.
        /// </summary>
        public static List<string> DetermineValidBosses(GatedItemTypeMode mode, bool lowestFirst = true, List<string> requiredBosses = null)
        {
            var validBosses = new List<string>();

            if (BossKeysInOrder == null || BossKeysInOrder.Count == 0)
            {
                return validBosses;
            }

            // Find index of highest boss allowed
            int highestIndex = 0;

            if (requiredBosses != null && requiredBosses.Count > 0)
            {
                foreach (var boss in requiredBosses)
                {
                    if (!boss.IsNullOrWhiteSpace())
                    {
                        int index = BossKeysInOrder.IndexOf(boss);
                        if (index > highestIndex)
                        {
                            highestIndex = index;
                        }
                    }
                }
            }
            else
            {
                highestIndex = BossKeysInOrder.Count - 1;
            }

            if (mode == GatedItemTypeMode.Unlimited ||
                mode == GatedItemTypeMode.PlayerMustKnowRecipe ||
                mode == GatedItemTypeMode.PlayerMustHaveCraftedItem)
            {
                validBosses.AddRange(BossKeysInOrder.GetRange(0, highestIndex + 1));
            }
            else
            {
                bool previousAdded = (mode == GatedItemTypeMode.BossKillUnlocksNextBiomeItems);
                bool add = false;
                // NO_BOSS is the first entry, add and skip in loop
                validBosses.Add(NO_BOSS);

                for (int i = 1; i <= highestIndex; i++)
                {
                    var boss = BossKeysInOrder[i];
                    add = false;

                    if (previousAdded && mode == GatedItemTypeMode.BossKillUnlocksNextBiomeItems)
                    {
                        add = true;
                    }

                    if (ZoneSystem.instance.GetGlobalKey(boss))
                    {
                        previousAdded = true;
                        add = true;
                    }
                    else
                    {
                        previousAdded = false;
                    }

                    if (add)
                    {
                        validBosses.Add(boss);
                    }
                }
            }

            if (validBosses.Count > 0)
            {
                validBosses = validBosses.Distinct().ToList();

                if (!lowestFirst)
                {
                    validBosses.Reverse();
                }
            }

            return validBosses;
        }

        private static bool CheckIfItemNeedsGate(GatedItemTypeMode mode, string itemName,
            out GatedItemDetails itemGatingDetails)
        {
            AllItemsWithDetails.TryGetValue(itemName, out itemGatingDetails);
            if (itemGatingDetails == null)
            {
                EpicLoot.Log($"Item {itemName} was not found in the iteminfo configuration, gating not evaluated. " +
                    $"Item will be allowed to drop.");
                return false;
            }

            return CheckIfItemNeedsGate(mode, itemName);
        }

        /// <summary>
        /// Returns true if item is gated, false if the item is not gated.
        /// </summary>
        private static bool CheckIfItemNeedsGate(GatedItemTypeMode mode, string itemName)
        {
            if (Player.m_localPlayer == null)
            {
                return true;
            }

            switch (mode)
            {
                case GatedItemTypeMode.Unlimited:
                    return false;
                case GatedItemTypeMode.PlayerMustKnowRecipe:
                    string name = GetItemName(itemName);
                    return !Player.m_localPlayer.IsRecipeKnown(name);
                case GatedItemTypeMode.PlayerMustHaveCraftedItem:
                    name = GetItemName(itemName);
                    return !Player.m_localPlayer.m_knownMaterial.Contains(name);
                case GatedItemTypeMode.BossKillUnlocksCurrentBiomeItems:
                case GatedItemTypeMode.BossKillUnlocksNextBiomeItems:
                    return CheckGateBossKill(itemName, mode);
                default:
                    return true; // Fallback, item will be gated- we could not gate it properly
            }
        }

        /// <summary>
        /// Returns true if the item is gated, false if allowed.
        /// </summary>
        private static bool CheckGateBossKill(string itemName, GatedItemTypeMode mode)
        {
            GatedItemDetails details;
            AllItemsWithDetails.TryGetValue(itemName, out details);

            if (details == null || details.RequiredBosses == null)
            {
                return false;
            }

            foreach (var boss in details.RequiredBosses)
            {
                var key = GetBossKeyForMode(boss, mode);

                if (!string.IsNullOrEmpty(key) && !ZoneSystem.instance.GetGlobalKey(key))
                {
                    return true;
                }
            }

            return false;
        }

        private static string GetBossKeyForMode(string bossKey, GatedItemTypeMode mode)
        {
            if (bossKey != null && mode == GatedItemTypeMode.BossKillUnlocksNextBiomeItems)
            {
                var key = Bosses.GetPrevBossKey(bossKey);
                if (key != null)
                {
                    return key;
                }
            }

            return bossKey;
        }

        private static string GetItemName(string item)
        {
            var itemPrefab = PrefabManager.Instance.GetPrefab(item);
            if (itemPrefab == null)
            {
                EpicLoot.LogError($"Tried to get gated itemID ({item}) but there is no prefab with that ID!");
                return null;
            }

            var itemDrop = itemPrefab.GetComponent<ItemDrop>();
            if (itemDrop == null)
            {
                EpicLoot.LogError($"Tried to get gated itemID ({item}) but its prefab has no ItemDrop component!");
                return null;
            }

            return itemDrop.m_itemData.m_shared.m_name;
        }
    }
}
