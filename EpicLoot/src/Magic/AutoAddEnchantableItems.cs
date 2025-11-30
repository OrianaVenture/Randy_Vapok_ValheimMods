using BepInEx;
using EpicLoot.Adventure;
using EpicLoot.Config;
using EpicLoot.Crafting;
using EpicLoot.GatedItemType;
using Jotunn.Managers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

namespace EpicLoot.Magic
{
    static class AutoAddEnchantableItems
    {
        private static readonly List<string> IgnoredItems = LootRoller.Config.RestrictedItems.ToList();
        private static readonly List<string> mistlandCraftStations = new List<string> { "piece_magetable", "blackforge" };

        private static readonly List<string> AshLandsResources = new List<string> { "FlametalNew", "Blackwood", "CharredBone", "MoltenCore", "GemstoneBlue", "GemstoneGreen", "GemstoneRed", "CelestialFeather" };
        private static readonly List<string> MistlandsResources = new List<string> { "YggdrasilWood", "BlackMarble", "Eitr", "BlackCore", "Mandible", "Carapace", "ScaleHide", "YagluthDrop" };
        private static readonly List<string> PlainsResources = new List<string> { "Needle", "BlackMetal", "LinenThread", "UndeadBjornRibcage", "TrophyBjornUndead" };
        private static readonly List<string> MountainResources = new List<string> { "Silver", "Obsidian", "WolfHairBundle", "WolfClaw", "WolfFang" };
        private static readonly List<string> SwampResources = new List<string> { "Iron", "Chain", "ElderBark", "Guck", "Chitin", "SerpentScale" };
        private static readonly List<string> BlackForestResources = new List<string> { "Copper", "Tin", "Bronze", "RoundLog", "FineWood", "TrollHide", "BjornHide", "BjornPaw" };
        private static readonly List<string> MeadowsResources = new List<string> { "Wood", "Stone", "Flint", "LeatherScraps", "DeerHide" };

        private static readonly List<string> UncraftableItemsAllowed = new List<string> { "THSwordWood", "SpearWood", "BattleaxeWood", "KnifeWood", "MaceWood", "AtgeirWood", "AxeWood", "SledgeWood" };

        private static readonly Dictionary<string, string> tierToBossKey = new Dictionary<string, string>() {
            { "Tier0", "none" },
            { "Tier1", "defeated_eikthyr" },
            { "Tier2", "defeated_gdking" },
            { "Tier3", "defeated_bonemass" },
            { "Tier4", "defeated_dragon" },
            { "Tier5", "defeated_goblinking" },
            { "Tier6", "defeated_queen" },
            { "Tier7", "defeated_fader" },
        };
        private static readonly Dictionary<string, List<string>> setsToCategories = new Dictionary<string, List<string>>(){
            { "Weapons", new List<string>() { "Swords", "Axes", "TwoHandAxes", "Knives", "Fists", "Staffs", "Clubs", "Sledges", "Polearms", "Spears", "Bows" } },
            { "Tools", new List<string>() { "Pickaxes", "Torches", "Tools" } },
            { "Armor", new List<string>() { "ChestArmor", "LegsArmor", "HeadArmor", "ShouldersArmor" } },
            { "Shields", new List<string>() { "TowerShields", "RoundShields", "Bucklers" } },
        };

        public static void CheckAndAddAllEnchantableItems(bool deregister = true)
        {
            if (deregister)
            {
                MinimapManager.OnVanillaMapDataLoaded -= () => AutoAddEnchantableItems.CheckAndAddAllEnchantableItems();
            }

            if (ELConfig.AutoAddEquipment.Value == false && ELConfig.AutoRemoveEquipmentNotFound.Value == false)
            {
                return;
            }

            List<ItemTypeInfo> currentConfigs = GatedItemTypeHelper.GatedConfig.ItemInfo;

            Dictionary<string, ItemTypeInfo> itemsByCategory = new Dictionary<string, ItemTypeInfo>();
            Dictionary<string, ItemTypeInfo> foundByCategory = new Dictionary<string, ItemTypeInfo>();

            foreach (ItemTypeInfo currentConfig in currentConfigs)
            {
                itemsByCategory.Add(currentConfig.Type, currentConfig);
                foundByCategory.Add(currentConfig.Type, new ItemTypeInfo()
                {
                    ItemsByBoss = new Dictionary<string, List<string>>() {
                        { "none", new List<string>() },
                        { "defeated_eikthyr", new List<string>() },
                        { "defeated_gdking", new List<string>() },
                        { "defeated_bonemass", new List<string>() },
                        { "defeated_dragon", new List<string>() },
                        { "defeated_goblinking", new List<string>() },
                        { "defeated_queen", new List<string>() },
                        { "defeated_fader", new List<string>() }
                    },
                });
            }

            List<ItemDrop> allItems = Resources.FindObjectsOfTypeAll<ItemDrop>().ToList();
            List <ItemDrop> allEquipment = allItems.Where(i => i.m_itemData != null &&
                i.m_itemData.m_shared != null &&
                i.m_autoPickup == true &&
                string.IsNullOrEmpty(i.m_itemData.m_shared.m_dlc) &&
                !string.IsNullOrEmpty(i.m_itemData.m_shared.m_description) &&
                i.m_itemData.IsEquipable()).ToList();

            EpicLoot.Log($"Checking all equipment in game.");

            foreach (ItemDrop item in allEquipment)
            {
                string itemType = DetermineItemType(item.m_itemData);
                string itemName = item.name;
                bool rune = item.m_itemData.IsRunestone();
                // Check if the item is already in the config
                // If it is, add it to the foundBy
                bool itemfound = false;
                if (itemsByCategory.ContainsKey(itemType) && foundByCategory.ContainsKey(itemType))
                {
                    if (itemsByCategory[itemType].IgnoredItems.Contains(itemName))
                    {
                        foundByCategory[itemType].IgnoredItems.Add(itemName);
                        itemfound = true;
                        continue;
                    }
                    else
                    {
                        foreach (var entry in itemsByCategory[itemType].ItemsByBoss)
                        {
                            var catEntry = foundByCategory[itemType].ItemsByBoss;
                            if (entry.Value.Contains(itemName))
                            {
                                if (!catEntry.ContainsKey(entry.Key))
                                {
                                    catEntry.Add(entry.Key, new List<string>());
                                }

                                catEntry[entry.Key].Add(itemName);
                                itemfound = true;
                                break;
                            }
                        }
                    }
                }

                if (itemfound)
                {
                    continue;
                }

                string key = DetermineBossLevelForItem(item.m_itemData);

                if (UncraftableItemsAllowed.Contains(itemName))
                {
                    foundByCategory[itemType].ItemsByBoss[key].Add(itemName);
                    continue;
                }

                // Item already exists in the config | Or we are not auto-adding items
                //if (itemfound || ELConfig.AutoAddEquipment.Value == false) { continue; }
                if ((ELConfig.OnlyAddEquipmentWithRecipes.Value == true && key == "none") ||
                    (key == "none" && itemType == "none") ||
                    itemType == "Unkown" || 
                    IgnoredItems.Contains(itemName))
                {
                    EpicLoot.Log($"skipping name:{itemName} type:{itemType} techlevel:{key}");
                    continue;
                }

                EpicLoot.Log($"{itemType} {key} add {itemName}");
                foundByCategory[itemType].ItemsByBoss[key].Add(itemName);
            }

            // Compare the found items with the current config, if enabled add items, if enabled remove missing items
            if (ELConfig.AutoRemoveEquipmentNotFound.Value)
            {
                EpicLoot.Log($"Remove not-found equipment processing.");
                foreach (var fbc in foundByCategory)
                {
                    if (!itemsByCategory.ContainsKey(fbc.Key) || !foundByCategory.ContainsKey(fbc.Key))
                    {
                        continue;
                    }

                    if (ELConfig.AutoAddEquipment.Value)
                    {
                        // Replace entries with only the found values, removes non-found items and adds new ones
                        itemsByCategory[fbc.Key].IgnoredItems = foundByCategory[fbc.Key].IgnoredItems;
                        foreach (var key in itemsByCategory[fbc.Key].ItemsByBoss.Keys)
                        {
                            if (itemsByCategory[fbc.Key].ItemsByBoss.ContainsKey(key) &&
                                foundByCategory[fbc.Key].ItemsByBoss.ContainsKey(key) &&
                                itemsByCategory[fbc.Key].ItemsByBoss[key].Count != foundByCategory[fbc.Key].ItemsByBoss[key].Count)
                            {
                                var toaddlist = foundByCategory[fbc.Key].ItemsByBoss[key]
                                    .Except(itemsByCategory[fbc.Key].ItemsByBoss[key]).ToList();
                                var toremovelist = itemsByCategory[fbc.Key].ItemsByBoss[key]
                                    .Except(foundByCategory[fbc.Key].ItemsByBoss[key]).ToList();
                                if (toaddlist.Count > 0)
                                {
                                    EpicLoot.Log($"Adding entries in {key} that are not found in the config: {string.Join(", ", toaddlist)}");
                                }
                                if (toremovelist.Count > 0)
                                {
                                    EpicLoot.Log($"Removing entries in {key} that are not found in the config: {string.Join(", ", toremovelist)}");
                                }
                            }
                        }

                        itemsByCategory[fbc.Key].ItemsByBoss = foundByCategory[fbc.Key].ItemsByBoss;
                    }
                    else
                    {
                        // Just remove items that are not found in the config
                        itemsByCategory[fbc.Key].IgnoredItems = foundByCategory[fbc.Key].IgnoredItems
                            .Where(e => itemsByCategory[fbc.Key].IgnoredItems.Contains(e)).ToList();

                        foreach (var entry in foundByCategory[fbc.Key].ItemsByBoss)
                        {
                            if (!itemsByCategory[fbc.Key].ItemsByBoss.ContainsKey(entry.Key))
                            {
                                continue;
                            }

                            var reducedItems = itemsByCategory[fbc.Key].ItemsByBoss[entry.Key]
                                .Where(e => entry.Value.Contains(e)).ToList();
                            if (reducedItems.Count != itemsByCategory[fbc.Key].ItemsByBoss[entry.Key].Count)
                            {
                                EpicLoot.Log($"Removing items from {fbc.Key} {entry.Key} that are not found in the config: " +
                                    $"{string.Join(", ", itemsByCategory[fbc.Key].ItemsByBoss[entry.Key].Except(reducedItems))}");
                            }

                            itemsByCategory[fbc.Key].ItemsByBoss[entry.Key] = reducedItems;
                        }
                    }
                }
            }
            else
            {
                EpicLoot.Log("Adding found equipment that was not listed.");
                // Just add found items, dont remove missing items
                foreach (var fbc in foundByCategory)
                {
                    if (ELConfig.AutoAddEquipment.Value)
                    {
                        if (!itemsByCategory.ContainsKey(fbc.Key))
                        {
                            continue;
                        }

                        // Replace entries with only the found values, removes non-found items and adds new ones
                        itemsByCategory[fbc.Key].IgnoredItems = itemsByCategory[fbc.Key].IgnoredItems
                            .Union(itemsByCategory[fbc.Key].IgnoredItems).ToList();

                        foreach (var entry in fbc.Value.ItemsByBoss)
                        {
                            if (!itemsByCategory[fbc.Key].ItemsByBoss.ContainsKey(entry.Key))
                            {
                                continue;
                            }

                            itemsByCategory[fbc.Key].ItemsByBoss[entry.Key] =
                                itemsByCategory[fbc.Key].ItemsByBoss[entry.Key].Union(entry.Value).ToList();
                        }
                    }
                }
            }

            EpicLoot.Log("Merging datasets and ensuring no duplicate entries.");
            // merge dataset and ensure unique values
            List<ItemTypeInfo> newConfig = new List<ItemTypeInfo>();
            foreach (var item in itemsByCategory)
            {
                if (item.Value.ItemsByBoss.Count > 0 || item.Value.IgnoredItems.Count > 0)
                {
                    Dictionary<string, List<string>> itemsByBossUniques = new();
                    foreach(var entry in item.Value.ItemsByBoss)
                    {
                        itemsByBossUniques.Add(entry.Key, entry.Value.Distinct().ToList());
                    }

                    ItemTypeInfo uniqueItems = new ItemTypeInfo() {
                        IgnoredItems = item.Value.IgnoredItems.Distinct().ToList(),
                        ItemFallback = item.Value.ItemFallback,
                        Type = item.Value.Type,
                        ItemsByBoss = itemsByBossUniques
                    };

                    newConfig.Add(uniqueItems);
                }
            }

            if (ELConfig.AutoAddRemoveEquipmentFromVendor.Value)
            {
                EpicLoot.Log("Adding/Removing entries for the vendor from detected equipment.");
                Dictionary<string, SecretStashItemConfig> existingVendorItems = new Dictionary<string, SecretStashItemConfig>();
                List<string> foundItemEntry = new List<string>();

                // Add all of the items currently in the vendor items list
                EpicLoot.Log("Adding Entries to the vendor list.");
                foreach (SecretStashItemConfig gamble in AdventureDataManager.Config.Gamble.GambleCosts)
                {
                    if (existingVendorItems.ContainsKey(gamble.Item))
                    {
                        EpicLoot.LogWarning($"Item {gamble.Item} is already in the vendor items list, skipping duplicate.");
                        continue;
                    }

                    existingVendorItems.Add(gamble.Item, gamble);
                }

                // Check the iteminfo configs for existing and new items
                EpicLoot.Log("Checking info on existing entries.");
                foreach (ItemTypeInfo itemType in newConfig)
                {
                    foreach (KeyValuePair<string, List<string>> bossEntry in itemType.ItemsByBoss)
                    {
                        foreach (string itemName in bossEntry.Value)
                        {
                            if (existingVendorItems.ContainsKey(itemName))
                            {
                                // Found this entry
                                foundItemEntry.Add(itemName);
                            }
                            else
                            {
                                existingVendorItems.Add(itemName, new SecretStashItemConfig()
                                { Item = itemName, CoinsCost = DetermineCoinsCostForItem(bossEntry.Key) });
                            }
                        }
                    }
                }

                // Remove Items which are not found
                EpicLoot.Log("Removing invalid entries.");
                List<SecretStashItemConfig> newGambleItems = existingVendorItems
                    .Where(x => foundItemEntry.Contains(x.Key)).Select(x => x.Value).ToList();
                EpicLoot.Log("Building config.");
                AdventureDataConfig AdventureDataConfigReplacement = AdventureDataManager.Config;
                AdventureDataConfigReplacement.Gamble.GambleCosts = newGambleItems;

                // Write out the new config, which will trigger a reload of the config
                EpicLoot.Log("Writing config.");
                try
                {
                    string contents = JsonConvert.SerializeObject(AdventureDataConfigReplacement, Formatting.Indented);
                    string overhaulFileLocation = Path.Combine(ELConfig.GetOverhaulDirectoryPath(), "adventuredata.json");
                    File.WriteAllText(overhaulFileLocation, contents);
                }
                catch (Exception e)
                {
                    EpicLoot.LogError($"Failed to auto-add vendor items to adventuredata.json: {e.Message}");
                }

            }

            if (ELConfig.AutoAddRemoveEquipmentFromLootLists.Value)
            {
                EpicLoot.Log("Adding/Removing entries in the loot drop configuration.");
                LootConfig defaultcfg = LootRoller.Config;
                List<LootTable> updatedLootTables = [];
                List<LootItemSet> updatedItemSets = [];

                // entry of all of the currently defined meta sets as they are valid targets also
                List<string> metaItemSetNames = LootRoller.Config.ItemSets.Select(x => x.Name).ToList();
                // List of all of the currently valid items so we can always determine if its at least valid
                List<string> validItems = [];
                foreach (var entry in foundByCategory.Values)
                {
                    foreach (var iteme in entry.ItemsByBoss)
                    {
                        validItems.AddRange(iteme.Value);
                    }
                }
                List<string> magicMats = allItems.Where(i => i.m_itemData != null && (i.m_itemData.IsMagicCraftingMaterial() || i.m_itemData.IsRunestone()))
                    .Select(x => x.m_itemData.m_dropPrefab.name).ToList();
                //EpicLoot.Log($"Starting loottable Validation. Valid items to use {validItems.Count} from {allItems.Count}");
                //EpicLoot.Log($"Found Item Names: {string.Join(",", validItems)}");
                //EpicLoot.Log($"Found Item sets: {string.Join(",", metaItemSetNames)}");
                //EpicLoot.Log($"Found Magic mats: {string.Join(",", magicMats)}");

                foreach (LootItemSet lis in LootRoller.Config.ItemSets)
                {
                    List<LootDrop> entries = new List<LootDrop>();
                    List<string> addedItems = new List<string>();
                    // Validate existing entries in the lootset
                    EpicLoot.Log($"Checking LootSet entry: {lis.Name}");
                    foreach (var loot in lis.Loot)
                    {
                        if (validItems.Contains(loot.Item) || metaItemSetNames.Contains(loot.Item) || magicMats.Contains(loot.Item))
                        {
                            entries.Add(loot);
                            addedItems.Add(loot.Item);
                            continue;
                        }

                        EpicLoot.Log($"{loot.Item} is not a found item and will be removed from the loot tables.");
                    }

                    //EpicLoot.Log($"Checking Item Tier and Loottype.");
                    if (DetermineTierAndType(lis.Name, out string tier, out string loottype))
                    {
                        if (!tierToBossKey.ContainsKey(tier))
                        {
                            EpicLoot.Log($"tierToBoss does not contain {tier}");
                            continue;
                        }

                        string bosskey = tierToBossKey[tier];
                        foreach (ItemTypeInfo itemType in newConfig)
                        {
                            if (!setsToCategories.ContainsKey(loottype) ||
                                !setsToCategories[loottype].Contains(itemType.Type) ||
                                !itemType.ItemsByBoss.ContainsKey(bosskey))
                            {
                                continue;
                            }

                            //EpicLoot.Log($"Checking for ItemType entry: {itemType.Type}");
                            foreach (var gateditem in itemType.ItemsByBoss[bosskey])
                            {
                                // EpicLoot.Log($"Checking if the item was already added: {gateditem}");
                                if (addedItems.Contains(gateditem))
                                {
                                    continue;
                                }

                                entries.Add(new LootDrop() { Item = gateditem, Rarity = DetermineRarityForLoot(tier) });
                            }
                        }
                    }

                    if (entries.Count > 0) { updatedItemSets.Add(new LootItemSet { Name = lis.Name, Loot = entries.ToArray() }); }
                }

                EpicLoot.Log($"Checking loot tables for invalid entries.");
                List<string> metaLootTables = new List<string>();
                //LootRoller.Config.LootTables
                foreach (LootTable lt in LootRoller.Config.LootTables)
                {
                    List<LootDrop> updatedLootDrop = new List<LootDrop>();
                    List<LeveledLootDef> levelListDef = new List<LeveledLootDef>();

                    EpicLoot.Log($"Checking loot in {lt.Object}.");

                    // Valid existing entries
                    if (lt.Loot != null)
                    {
                        updatedLootDrop.AddRange(ValidateLootList(lt, metaLootTables, metaItemSetNames, validItems));
                    }

                    // Validate existing entries in the leveled loot drops
                    if (lt.LeveledLoot != null)
                    {
                        foreach (var lloot in lt.LeveledLoot)
                        {
                            List<LootDrop> updatedLootTableLL = new List<LootDrop>();
                            foreach(var ld in lloot.Loot)
                            {
                                if (validItems.Contains(ld.Item) || metaItemSetNames.Contains(ld.Item))
                                {
                                    updatedLootTableLL.Add(ld);
                                }
                            }
                            LeveledLootDef lld = new LeveledLootDef();
                            lld.Loot = updatedLootTableLL.ToArray();
                            levelListDef.Add(lld);
                        }
                    }

                    LootTable ltc = lt;
                    ltc.Loot = updatedLootDrop.ToArray();
                    updatedLootTables.Add(ltc);
                    metaLootTables.Add(lt.Object);
                }
                EpicLoot.Log($"Finished Validating loottable.");
                // Write out the new config, which will trigger a reload of the config
                try
                {
                    LootConfig newLootConfig = new LootConfig()
                    {
                        ItemSets = updatedItemSets.ToArray(),
                        LootTables = updatedLootTables.ToArray(),
                        MagicEffectsCount = LootRoller.Config.MagicEffectsCount,
                        RestrictedItems = LootRoller.Config.RestrictedItems
                    };
                    string contents = JsonConvert.SerializeObject(newLootConfig, Formatting.Indented);
                    string overhaulFileLocation = Path.Combine(ELConfig.GetOverhaulDirectoryPath(), "loottables.json");
                    File.WriteAllText(overhaulFileLocation, contents);
                }
                catch (Exception e)
                {
                    EpicLoot.LogError($"Failed to auto-update loottables.json: {e.Message}");
                }
            }
            EpicLoot.Log($"Writing iteminfo.");
            // Write out the new config, which will trigger a reload of the config
            try
            {
                string contents = JsonConvert.SerializeObject(new ItemInfoConfig() { ItemInfo = newConfig }, Formatting.Indented);
                string overhaulFileLocation = Path.Combine(ELConfig.GetOverhaulDirectoryPath(), "iteminfo.json");
                File.WriteAllText(overhaulFileLocation, contents);
            }
            catch (Exception e)
            {
                EpicLoot.LogError($"Failed to auto-add items to iteminfo.json: {e.Message}");
                return;
            }
            EpicLoot.Log($"All equipment, vendor, droptables and iteminfo configs validated.");
        }

        private static List<LootDrop> ValidateLootList(LootTable lt, List<string> metaLootTables, List<string> metaItemSetNames, List<string> validItems)
        {
            List<LootDrop> updatedLootDrop = new List<LootDrop>();
            foreach (var loot in lt.Loot)
            {
                if (loot.Item.Contains("."))
                {
                    var referenceAndIndex = loot.Item.Split('.');
                    EpicLoot.Log($"Validating meta reference {loot.Item} {referenceAndIndex[0]}");
                    if (metaItemSetNames.Contains(referenceAndIndex[0]) || metaLootTables.Contains(referenceAndIndex[0]))
                    {
                        updatedLootDrop.Add(loot);
                        continue;
                    }
                }

                if (!validItems.Contains(loot.Item) && !metaItemSetNames.Contains(loot.Item))
                {
                    EpicLoot.Log($"REMOVING: Loot table Item {loot.Item} not found.");
                    continue;
                }
                updatedLootDrop.Add(loot);
            }
            return updatedLootDrop;
        }

        private static int DetermineCoinsCostForItem(string bosskey) {
            return bosskey switch
            {
                "none" => 50,
                "defeated_eikthyr" => 100,
                "defeated_gdking" => 400,
                "defeated_bonemass" => 600,
                "defeated_dragon" => 900,
                "defeated_goblinking" => 1100,
                "defeated_queen" => 1300,
                "defeated_fader" => 1600,
                _ => 999,
            };
        }

        private static bool DetermineTierAndType(string name, out string tier, out string type)
        {
            tier = null;
            type = null;
            if (!name.Contains("Tier"))
            {
                EpicLoot.Log("Non Tiered entry");
                return false;
            }

            tier = name.Substring(0, 5);
            type = name.Substring(5);
            //EpicLoot.Log($"{name} = Tier={tier} Type={type}");
            // Maybe we want to ensure the everything groups are properly setup? How much loot table validation should we do?
            if (type == "Tier" || type == "Everything") { return false; }
            return true;
        }

        private static float[] DetermineRarityForLoot(string tier) {
            return tier switch {
                "Tier0" => [97, 2, 1, 0, 0],
                "Tier1" => [94, 3, 2, 1, 0],
                "Tier2" => [80, 14, 4, 2, 0],
                "Tier3" => [38, 50, 8, 3, 1],
                "Tier4" => [5, 35, 50, 17, 3],
                "Tier5" => [0, 15, 60, 20, 5],
                "Tier6" => [0, 10, 50, 30, 10],
                "Tier7" => [0, 5, 35, 45, 15],
                _ => [97, 2, 1, 0, 0],
            };
        }

        private static string DetermineItemType(ItemDrop.ItemData item) {
            var itemType = item.m_shared.m_itemType;
            switch(itemType)
            {
                case ItemDrop.ItemData.ItemType.TwoHandedWeapon:
                case ItemDrop.ItemData.ItemType.OneHandedWeapon:
                case ItemDrop.ItemData.ItemType.TwoHandedWeaponLeft:
                case ItemDrop.ItemData.ItemType.Attach_Atgeir:
                    switch (item.m_shared.m_skillType) {
                        case Skills.SkillType.Spears:
                            return "Spears";
                        case Skills.SkillType.Swords:
                            return "Swords";
                        case Skills.SkillType.Clubs:
                            if (itemType == ItemDrop.ItemData.ItemType.OneHandedWeapon) {
                                return "Clubs";
                            } else {
                                return "Sledges";
                            }
                        case Skills.SkillType.Axes:
                            if (itemType == ItemDrop.ItemData.ItemType.OneHandedWeapon) {
                                return "Axes";
                            } else {
                                return "TwoHandAxes";
                            }
                        case Skills.SkillType.Knives:
                            return "Knives";
                        case Skills.SkillType.Unarmed:
                            return "Fists";
                        case Skills.SkillType.ElementalMagic:
                        case Skills.SkillType.BloodMagic:
                            return "Staffs";
                        case Skills.SkillType.Polearms:
                            return "Polearms";
                        case Skills.SkillType.Pickaxes:
                            return "Pickaxes";
                    }
                    break;
                case ItemDrop.ItemData.ItemType.Shield:
                    if (item.m_shared.m_timedBlockBonus > 0) {
                        if (item.m_shared.m_timedBlockBonus >= 2.5f) {
                            return "Bucklers";
                        } else {
                            return "RoundShields";
                        }
                    } else {
                        return "TowerShields";
                    }
                case ItemDrop.ItemData.ItemType.Bow:
                    return "Bows";
                case ItemDrop.ItemData.ItemType.Helmet:
                    return "HeadArmor";
                case ItemDrop.ItemData.ItemType.Chest:
                    return "ChestArmor";
                case ItemDrop.ItemData.ItemType.Legs:
                    return "LegsArmor";
                case ItemDrop.ItemData.ItemType.Shoulder:
                    return "ShouldersArmor";
                case ItemDrop.ItemData.ItemType.Torch:
                    return "Torches";
                case ItemDrop.ItemData.ItemType.Tool:
                    return "Tools";
                case ItemDrop.ItemData.ItemType.Utility:
                case ItemDrop.ItemData.ItemType.Trinket:
                    return "Utility";
                default:
                    return itemType.ToString().ToLower();
                }


            EpicLoot.Log($"Unknown item type for item {item.m_shared.m_name}: {itemType}");
            return "Unkown";
        }

        public static string DetermineBossLevelForItem(ItemDrop.ItemData item)
        {
            Recipe itemRecipe = ObjectDB.instance.GetRecipe(item);
            if (itemRecipe == null || itemRecipe.m_enabled == false || itemRecipe.m_resources == null) { return "none"; }

            // We need to completely evaluate each tier until we find a match, so that we only match the highest tier for the selected item.

            // Ashlands check
            foreach (Piece.Requirement req in itemRecipe.m_resources) {
                if (AshLandsResources.Contains(req.m_resItem.name)) { return "defeated_fader"; }
            }

            // Mistlands check
            foreach (Piece.Requirement req in itemRecipe.m_resources) {
                if (MistlandsResources.Contains(req.m_resItem.name)) { return "defeated_queen"; }
            }

            // After we have checked resources for the Ashlands and Mistlands, we check if the item is crafted from a table which is only available mistlands or later.
            if (itemRecipe.m_craftingStation != null && mistlandCraftStations.Contains(itemRecipe.m_craftingStation.name)) {
                return "defeated_queen";
            }

            // Plains check
            foreach (Piece.Requirement req in itemRecipe.m_resources) {
                if (PlainsResources.Contains(req.m_resItem.name)) { return "defeated_goblinking"; }
            }

            // Mountain check
            foreach (Piece.Requirement req in itemRecipe.m_resources) {
                if (MountainResources.Contains(req.m_resItem.name)) { return "defeated_dragon"; }
            }

            // Swamp check
            foreach (Piece.Requirement req in itemRecipe.m_resources) {
                if (SwampResources.Contains(req.m_resItem.name)) { return "defeated_bonemass"; }
            }

            // Blackforest check
            foreach (Piece.Requirement req in itemRecipe.m_resources) {
                if (BlackForestResources.Contains(req.m_resItem.name)) { return "defeated_gdking"; }
            }

            // Meadows check
            foreach (Piece.Requirement req in itemRecipe.m_resources) {
                if (MeadowsResources.Contains(req.m_resItem.name)) { return "defeated_eikthyr"; }
            }

            return "none";
        }
    }
}
