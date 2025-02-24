using System.Collections.Generic;
using System.Linq;
using EpicLoot.Adventure;
using EpicLoot.Adventure.Feature;
using EpicLoot.src.General;
using HarmonyLib;
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
        public string reqBoss { get; set; }

        public List<string> reqBosses { get; set; }
        public string category { get; set; }
        public string fallback_category { get; set; }

        public string fallback_item { get; set; }
    }

    public static class GatedItemTypeHelper
    {
        public static ItemInfoConfig gatedConfig;
        // public static readonly List<ItemTypeInfo> ItemInfos = new List<ItemTypeInfo>();

        // List of all items of that type, not sorted
        public static Dictionary<string, List<string>> ItemsByType = new Dictionary<string, List<string>>();
        // List of all items by their boss tier, this is effectively the boss-gated list, by item type.
        public static Dictionary<string, Dictionary<string, List<string>>> ItemsByTypeAndBoss = new Dictionary<string, Dictionary<string, List<string>>>();
        // List of all items with their respective bossgate, this is a shortcircuit list for the commonly used lootroll
        public static Dictionary<string, GatedItemDetails> AllItemsWithDetails = new Dictionary<string, GatedItemDetails>();
        // List of fallbacks linked by category
        public static Dictionary<string, string> FallsbackCategoryByCategory = new Dictionary<string, string>();
        public static Dictionary<string, string> FallbackItemsByCategory = new Dictionary<string, string>();

        public static List<string> BossOrder = new List<string>();
        public static List<string> ReverseBossOrder = new List<string>();

        // List of the categories used
        public static List<string> ItemCategories = new List<string>();
        public static void Initialize(ItemInfoConfig config)
        {
            gatedConfig = config;
            // ItemInfos.Clear();
            ItemsByType.Clear();
            ItemsByTypeAndBoss.Clear();
            AllItemsWithDetails.Clear();
            FallsbackCategoryByCategory.Clear();
            FallbackItemsByCategory.Clear();
            BossOrder.Clear();
            ReverseBossOrder.Clear();
            ItemCategories.Clear();

            // Building these item lists requires potentially merging a number of patches which will have duplicate data structures in
            // The resulting dictionaries and shortened lists are useful and important for short logical evaluations of whether or not an item is gated

            // Add to required lists
            foreach (var info in config.ItemInfo) {
                // ItemInfos.Add(info);
                if (ItemsByType.ContainsKey(info.Type)) {
                    EpicLoot.Log($"Merging [{info.Type}] entries, duplicates will be removed.");
                    ItemsByType[info.Type].Union(info.Items).ToList();
                } else { ItemsByType.Add(info.Type, info.Items); }
                if (!(FallsbackCategoryByCategory.ContainsKey(info.Type))) {
                    FallsbackCategoryByCategory.Add(info.Type, info.Fallback);
                }
                if (!(FallbackItemsByCategory.ContainsKey(info.Type))) {
                    FallbackItemsByCategory.Add(info.Type, info.UngatedFallback);
                }
                Dictionary<string, List<string>> itemsByBoss = [];
                foreach (var bossItem in info.ItemsByBoss) {
                    if (itemsByBoss.ContainsKey(bossItem.Key)) {
                        EpicLoot.Log($"Merging [{bossItem.Key}] entries, duplicates will be removed.");
                        itemsByBoss[bossItem.Key].Union(bossItem.Value).ToList();
                    } else { itemsByBoss.Add(bossItem.Key, bossItem.Value); }
                    foreach (var item in bossItem.Value) {
                        if (AllItemsWithDetails.ContainsKey(item)) {
                            EpicLoot.Log($"{item} already registered, merging boss keys.");
                            List<string> reqBosses = AllItemsWithDetails[item].reqBosses;
                            if (!reqBosses.Contains(bossItem.Key)) { reqBosses.Add(bossItem.Key); }
                            AllItemsWithDetails[item].reqBosses = reqBosses;
                            continue;
                        }
                        AllItemsWithDetails.Add(item, new GatedItemDetails() { reqBoss = bossItem.Key, category = info.Type, fallback_category = info.Fallback, fallback_item = info.UngatedFallback, reqBosses = new List<string>() { bossItem.Key } });
                    }
                }
                if (ItemsByTypeAndBoss.ContainsKey(info.Type)) {
                    EpicLoot.Log("Duplicate entry in ItemByTypeAndBoss.");
                } else { ItemsByTypeAndBoss.Add(info.Type, itemsByBoss); }

            }
            // Boss order needs to be from the end of the game to current. This allows us to walk down the tiers from the end easily and select the current progression specific tier
            // then degrade tiers if we are unable to select the current tier item and its fallback DEPTH number of times
            ItemCategories = [.. GatedItemTypeHelper.ItemsByType.Keys];

            foreach (var boss in AdventureDataManager.Config.Bounties.Bosses) {
                BossOrder.Add(boss.BossDefeatedKey);
            }
            ReverseBossOrder = BossOrder.ToList();
            ReverseBossOrder.Reverse();
        }

        // This is used for gambling, and will return a random item from the category
        public static string GetItemFromCategory(string itemCategory, GatedItemTypeMode mode, List<string> already_selected, int depth = 4)
        {
            EpicLoot.Log($"Getting {itemCategory} with gating style {mode}");
            switch (mode) {
                case GatedItemTypeMode.BossKillUnlocksCurrentBiomeItems:
                    List<string> player_defeated_bosses = DeterminePlayerDefeatedBiomes(true);
                    if (player_defeated_bosses.Count == 0) {
                        return FallbackItemsByCategory[itemCategory];
                    }
                    EpicLoot.Log($"Player has defeated the following bosses: {string.Join(",", player_defeated_bosses)}");
                    if (ItemsByTypeAndBoss.ContainsKey(itemCategory))
                    {
                        string item = null;
                        foreach (var defeated_boss in player_defeated_bosses)
                        {
                            EpicLoot.Log($"Checking {itemCategory} for boss {defeated_boss}");
                            item = getGatedWeaponFromList(itemCategory, defeated_boss, already_selected, mode, false, true);
                            if (item != null) { return item; }
                        }
                        // we couldn't find anything within the primary category on the target item types, check the fallback category
                        if (item == null) {
                            return getGatedWeaponFromList(FallsbackCategoryByCategory[itemCategory], player_defeated_bosses.First(), already_selected, mode, true, true);
                        }
                    } else {
                        EpicLoot.LogWarning($"Item Category [{itemCategory}] not found in ItemInfo.");
                        return null;
                    }
                    break;
                
                // Because knowing the recipe and having crafted the item all occur on the next biome style gating we can just use this for all of those cases
                case GatedItemTypeMode.PlayerMustKnowRecipe:
                case GatedItemTypeMode.PlayerMustHaveCraftedItem:
                case GatedItemTypeMode.BossKillUnlocksNextBiomeItems:
                    // Get the defeated bosses in non-reversed order, so eik will be the first boss
                    List<string> defeated_bosses = DeterminePlayerDefeatedBiomes();
                    int highest_boss_index = BossOrder.Count;
                    string highest_boss = "defeated_eikthyr"; // Default to the first boss
                    if (defeated_bosses.Count > 0)
                    {
                        foreach (var defeated_boss in defeated_bosses)
                        {
                            if (BossOrder.IndexOf(defeated_boss) < highest_boss_index)
                            {
                                EpicLoot.Log($"Setting defeated boss {defeated_boss} index: {BossOrder.IndexOf(defeated_boss)}");
                                highest_boss_index = BossOrder.IndexOf(defeated_boss);
                            }
                        }
                        if (mode == GatedItemTypeMode.BossKillUnlocksNextBiomeItems) {
                            if (highest_boss_index - 1 >= 0) {
                                EpicLoot.Log($"current highest boss {BossOrder[highest_boss_index]} being set to next boss: {BossOrder[highest_boss_index - 1]}");
                                highest_boss = BossOrder[highest_boss_index - 1];
                            } else {
                                highest_boss = BossOrder.Last();
                            }
                        } else {
                            highest_boss = BossOrder[highest_boss_index];
                        }
                    }
                    EpicLoot.Log($"Checking {itemCategory} for boss {highest_boss}");
                    return getGatedWeaponFromList(itemCategory, highest_boss, already_selected, mode, true, true);
                case GatedItemTypeMode.Unlimited:
                    // Go through each biome level and look for recipes the player knows, those are valid drops for unlimited mode
                    List<string> unlimited_mode_bosses = BossOrder;
                    unlimited_mode_bosses.Reverse();
                    foreach (var boss in unlimited_mode_bosses) {
                        string item = getGatedWeaponFromList(itemCategory, boss, already_selected, GatedItemTypeMode.PlayerMustKnowRecipe, false, true);
                        if (item != null) {
                            return item;
                        }
                    }
                    break;
            }
            // No valid item from this category found
            EpicLoot.LogWarning($"No valid item found in [{itemCategory}], using fallback.");
            return FallbackItemsByCategory[itemCategory];
        }

        private static string getGatedWeaponFromList(string itemCategory, string boss, List<string> already_selected, GatedItemTypeMode mode, bool should_fallback = false, bool duplicate_instead_of_fallthrough = false, bool gaurentee_item = false)
        {
            EpicLoot.Log($"Checking {itemCategory} for tier with {boss} - {ItemsByTypeAndBoss[itemCategory].ContainsKey(boss)}");
            string valid_fallback_items = null;
            if (ItemsByTypeAndBoss[itemCategory].ContainsKey(boss))
            {
                List<string> category_weapons = ItemsByTypeAndBoss[itemCategory][boss];
                category_weapons.shuffleList();
                bool needs_gate = true;
                foreach (var weapon in category_weapons)
                {
                    // Don't select the same thing twice
                    needs_gate = CheckIfItemNeedsGate(mode, weapon);
                    if (already_selected.Contains(weapon)) {
                        valid_fallback_items = weapon;
                        continue;
                    }
                    EpicLoot.Log($"Selected {weapon}");
                    return weapon;
                }
            }
            // We fall back to the ungated item
            if (valid_fallback_items != null && duplicate_instead_of_fallthrough) {
                EpicLoot.Log($"Selected Duplicate fallback {valid_fallback_items}");
                return valid_fallback_items;
            }
            if (should_fallback) {
                // Try one more time with the fallback for this category instead, this will not trigger an infinite loop since it will not fallback itself
                return getGatedWeaponFromList(FallsbackCategoryByCategory[itemCategory], boss, already_selected, mode, false, true);
            }
            if (gaurentee_item) {
                EpicLoot.Log($"Selecting hardfallback {FallbackItemsByCategory[itemCategory]}");
                return FallbackItemsByCategory[itemCategory];
            } else {
                return null;
            } 
        }

        public static string GetGatedItemID(string itemName, int depth = 2)
        {
            return GetGatedItemID(itemName, EpicLoot.GetGatedItemTypeMode(), depth);
        }

        // Always returns the highest tier item in a category, with fallback, and randomization
        private static string GetGatedItemID(string itemID, GatedItemTypeMode mode, int depth = 2)
        {
            EpicLoot.Log($"Checking GatedItemID for {itemID}");
            if (string.IsNullOrEmpty(itemID)) {
                EpicLoot.LogError($"Tried to get gated itemID with null or empty itemID!");
                return null;
            }
            // We are in ungated mode
            if (mode == GatedItemTypeMode.Unlimited) {
                EpicLoot.Log($"Unlimited gating mode {itemID}");
                return itemID;
            }
            // Passed item is not gated, return it immediately
            if (!CheckIfItemIDNeedsGate(mode, itemID, out GatedItemDetails itemDetails))
            {
                EpicLoot.Log($"Item is not gated {itemID}");
                return itemID;
            }
            List<string> defeated_bosses = DeterminePlayerDefeatedBiomes();
            // The fact that we got to this point means the selected item is gated, and we are trying to select a fallback
            bool at_tier = false;
            foreach(string boss in ReverseBossOrder) {
                EpicLoot.Log($"checking tier: ({boss}) - {at_tier == true} || {itemDetails.reqBosses.Contains(boss) == true}");
                // Is item current tier?
                if (at_tier == true || itemDetails.reqBosses.Contains(boss) == true) {
                    at_tier = true;
                } else {
                    continue;
                }
                if (defeated_bosses.Contains(boss) != true) {
                    EpicLoot.Log($"Player has not defeated {boss}, reducing tier.");
                    continue;
                }
                // One of the tiers below, but same item type
                if (ItemsByTypeAndBoss[AllItemsWithDetails[itemID].category].ContainsKey(boss)) {
                    string potentialItem = GatedItemFromListWithCritiera(mode, AllItemsWithDetails[itemID].category, boss, depth);
                    if (potentialItem != null) {
                        // Selected item is valid and not gated
                        EpicLoot.Log($"Gate selected alternative: {potentialItem}");
                        return potentialItem;
                    } else {
                        // Selected item is gated or not valid, we try the fallback x times
                        string current_category = AllItemsWithDetails[itemID].category;
                        for (int i = 0; i < depth; i++) {
                            potentialItem = GatedItemFromCriteriaIsValid(mode, current_category, boss);
                            if (potentialItem != null) {
                                EpicLoot.Log($"Gate selected fallback alternative: {potentialItem}");
                                return potentialItem;
                            }
                            // select the fallback for our current category
                            current_category = FallsbackCategoryByCategory[current_category];
                        }
                    }
                }
            }
            // We were not able to select an item
            EpicLoot.Log($"Unable to determine gating for {itemID}, returning a fallback.");
            return FallbackItemsByCategory.First().Value;
        }

        // Check the list of items for the current tier of the selected type, up to a maximum number to test
        // Randomly ordered.
        private static string GatedItemFromCriteriaIsValid(GatedItemTypeMode mode, string category, string boss)
        {
            // Check if this category has entries
            if (!(ItemsByTypeAndBoss[category][boss].Count > 0)) { return null; }
            int selected = UnityEngine.Random.Range(0, ItemsByTypeAndBoss[category][boss].Count);
            string tempItemId = ItemsByTypeAndBoss[category][boss][selected];
            // If the item does not need to be gated we return this item, having found a valid item for this
            if (!CheckIfItemIDNeedsGate(mode, tempItemId))
            {
                return tempItemId;
            }
            return null;
        }

        private static string GatedItemFromListWithCritiera(GatedItemTypeMode mode, string category, string boss, int max_tries = 4)
        {
            // Check if this category has entries
            if (!(ItemsByTypeAndBoss[category][boss].Count > 0)) { return null; }
            List<string> allItemTypeFromTier = ItemsByTypeAndBoss[category][boss];
            allItemTypeFromTier.shuffleList();
            int tries = 0;
            foreach(string item in allItemTypeFromTier) {
                if (tries > max_tries) { return null; }
                if (!CheckIfItemIDNeedsGate(mode, item)) {
                    return item;
                }
                tries += 1;
            }
            return null;
        }

        private static string GetItemName(string itemID)
        {
            var itemPrefab = PrefabManager.Instance.GetPrefab(itemID);
            if (itemPrefab == null) {
                EpicLoot.LogError($"Tried to get gated itemID ({itemID}) but there is no prefab with that ID!");
                return null;
            }
            var itemDrop = itemPrefab.GetComponent<ItemDrop>();
            if (itemDrop == null) {
                EpicLoot.LogError($"Tried to get gated itemID ({itemID}) but its prefab has no ItemDrop component!");
                return null;
            }
            return itemDrop.m_itemData.m_shared.m_name;
        }

        // Returns a list of defeated bosses, in order from the end of the game
        // First entry will be the highest tier boss defeated
        // Bosses that have not been defeated will be skipped, which can result in gaps in the normal progression list
        private static List<string> DeterminePlayerDefeatedBiomes(bool reverse = true)
        {
            var defeated_bosses = new List<string>();
            List<BountyBossConfig> PlayerBossProgression = AdventureDataManager.Config.Bounties.Bosses;
            if (PlayerBossProgression == null || PlayerBossProgression.Count == 0) {
                EpicLoot.Log("Player has not defeated any bosses.");
                return defeated_bosses; 
            }
            foreach ( var boss in PlayerBossProgression ) {
               if (ZoneSystem.instance.GetGlobalKey(boss.BossDefeatedKey)) {
                    defeated_bosses.Add(boss.BossDefeatedKey);
                }
            }
            // No need to reorder or filter the list if its empty
            if (defeated_bosses.Count == 0) { return defeated_bosses; }
            defeated_bosses = defeated_bosses.Distinct().ToList();
            // The list should start from the end of the game, since the boss list returned is not ordered
            // we may need to reverse it if the first entry is eikthyr
            // alternatively we could use a strong sorting system- but that would mean bosses would need to be defined in a specific order
            // and any mods that add bosses would need their boss keys added
            if ( reverse) {
                defeated_bosses.Reverse();
            }
            EpicLoot.Log($"Defeated bosses {string.Join(",", defeated_bosses)}");
            return defeated_bosses;
        }

        // Just checks if this item is gated or not
        private static bool CheckIfItemIDNeedsGate(GatedItemTypeMode mode, string itemID)
        {
            string itemName = GetItemName(itemID);
            return GateEvaluation(mode, itemName);
        }

        // returns details about the gated item when checked
        private static bool CheckIfItemIDNeedsGate(GatedItemTypeMode mode, string itemName, out GatedItemDetails itemGatingDetails)
        {
            AllItemsWithDetails.TryGetValue(itemName, out itemGatingDetails);
            return GateEvaluation(mode, itemName);
        }

        private static bool CheckIfItemNeedsGate(GatedItemTypeMode mode, string itemName)
        {
            return GateEvaluation(mode, itemName);
        }

        /// <summary>
        /// Returns true if item is gated, false if the item is not gated.
        /// </summary>
        private static bool GateEvaluation(GatedItemTypeMode mode, string itemName)
        {
            if (Player.m_localPlayer == null) {
                EpicLoot.Log($"Local player unset, item is gated.");
                return true;
            }
            switch (mode)
            {
                case GatedItemTypeMode.Unlimited:
                    return false;
                case GatedItemTypeMode.PlayerMustKnowRecipe:
                    return !Player.m_localPlayer.IsRecipeKnown(itemName);
                case GatedItemTypeMode.PlayerMustHaveCraftedItem:
                    return !Player.m_localPlayer.m_knownMaterial.Contains(itemName);
                case GatedItemTypeMode.BossKillUnlocksCurrentBiomeItems:
                case GatedItemTypeMode.BossKillUnlocksNextBiomeItems:
                    List<string> reqbosses = null;
                    string reqboss = null;
                    GatedItemDetails details;
                    AllItemsWithDetails.TryGetValue(itemName, out details);
                    if (details != null) {
                        reqbosses = details.reqBosses;
                        reqboss = details.reqBoss;
                    }
                    if (reqbosses == null) { return false; }
                    if (mode == GatedItemTypeMode.BossKillUnlocksCurrentBiomeItems) {
                        foreach(var boss in reqbosses) {
                            if (ZoneSystem.instance.GetGlobalKey(boss)) {
                                return false;
                            }
                        }
                        return false;
                    }
                    var prevBossKey = Bosses.GetPrevBossKey(reqboss);
                    // No previous boss || the player has the previous boss key?
                    if (string.IsNullOrEmpty(prevBossKey) || ZoneSystem.instance.GetGlobalKey(prevBossKey)) {
                        return false;
                    }
                    return true;
            }
            // Fallback, item will be gated- we could not gate it properly
            return true; 
        }
       
    }
}
