using EpicLoot.Config;
using EpicLoot.GatedItemType;
using Jotunn.Managers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace EpicLoot.src.Magic
{
    static class AutoAddEnchantableItems
    {
        private static readonly List<string> IgnoredItems = LootRoller.Config.RestrictedItems.ToList();
        private static readonly List<string> mistlandCraftStations = new List<string> { "piece_magetable", "blackforge" };

        private static readonly List<string> AshLandsResources = new List<string> { "FlametalNew", "Blackwood", "CharredBone", "MoltenCore", "GemstoneBlue", "GemstoneGreen", "GemstoneRed", "CelestialFeather" };
        private static readonly List<string> MistlandsResources = new List<string> { "YggdrasilWood", "BlackMarble", "Eitr", "BlackCore", "Mandible", "Carapace", "ScaleHide", "YagluthDrop" };
        private static readonly List<string> PlainsResources = new List<string> { "Needle", "BlackMetal" };
        private static readonly List<string> MountainResources = new List<string> { "Silver", "Obsidian", "WolfHairBundle", "WolfClaw", "WolfFang" };
        private static readonly List<string> SwampResources = new List<string> { "Iron", "Chain", "ElderBark", "Guck" };
        private static readonly List<string> BlackForestResources = new List<string> { "Copper", "Tin", "Bronze", "RoundLog", "FineWood", "TrollHide" };
        private static readonly List<string> MeadowsResources = new List<string> { "Wood", "Stone", "Flint", "LeatherScraps", "DeerHide" };

        public static void CheckAndAddAllEnchantableItems() {
            ItemManager.OnItemsRegistered -= AutoAddEnchantableItems.CheckAndAddAllEnchantableItems;
            // Disable this
            if (ELConfig.AutoAddEquipment.Value == false && ELConfig.AutoRemoveEquipmentNotFound.Value == false) { return; }

            List<ItemTypeInfo> currentConfigs = GatedItemTypeHelper.GatedConfig.ItemInfo;

            Dictionary<string, ItemTypeInfo> itemsByCategory = new Dictionary<string, ItemTypeInfo>();
            Dictionary<string, ItemTypeInfo> foundbyCategory = new Dictionary<string, ItemTypeInfo>();

            foreach (ItemTypeInfo currentConfig in currentConfigs) {
                itemsByCategory.Add(currentConfig.Type, currentConfig);
                foundbyCategory.Add(currentConfig.Type, new ItemTypeInfo() { 
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

            List<ItemDrop> allEquipment = Resources.FindObjectsOfTypeAll<ItemDrop>().Where(i => i.m_itemData != null && 
                i.m_itemData.m_shared != null &&
                i.m_autoPickup == true &&
                string.IsNullOrEmpty(i.m_itemData.m_shared.m_dlc) &&
                !string.IsNullOrEmpty(i.m_itemData.m_shared.m_description) &&
                EpicLoot.AllowedMagicItemTypes.Contains(i.m_itemData.m_shared.m_itemType)).ToList();

            foreach (ItemDrop item in allEquipment)
            {
                string itemType = DetermineItemType(item.m_itemData);
                string itemName = item.name;

                // Check if the item is already in the config
                // If it does, add it to the foundBy
                bool itemfound = false;
                if (itemsByCategory.ContainsKey(itemType)) {
                    if (itemsByCategory[itemType].IgnoredItems.Contains(itemName)) {
                        foundbyCategory[itemType].IgnoredItems.Add(itemName);
                        itemfound = true;
                        continue;
                    } else {
                        foreach (var entry in itemsByCategory[itemType].ItemsByBoss) {
                            if (entry.Value.Contains(itemName)) {
                                foundbyCategory[itemType].ItemsByBoss[entry.Key].Add(itemName);
                                itemfound = true;
                                break;
                            }
                        }
                    }
                }
                if (itemfound) { continue; }

                // Item already exists in the config | Or we are not auto-adding items
                //if (itemfound || ELConfig.AutoAddEquipment.Value == false) { continue; }

                string key = DetermineBossLevelForItem(item.m_itemData);
                if ((ELConfig.OnlyAddEquipmentWithRecipes.Value == true && key == "none") || (key == "none" && itemType == "none") || itemType == "Unkown" || IgnoredItems.Contains(itemName)) {
                    EpicLoot.Log($"skipping name:{itemName} type:{itemType} techlevel:{key}");
                    continue;
                }

                EpicLoot.Log($"{itemType} {key} add {itemName}");

                foundbyCategory[itemType].ItemsByBoss[key].Add(itemName);
            }

            // Compare the found items with the current config, if enabled add items, if enabled remove missing items
            if (ELConfig.AutoRemoveEquipmentNotFound.Value)
            {
                foreach (var fbc in foundbyCategory)
                {
                    if (ELConfig.AutoAddEquipment.Value)
                    {
                        // Replace entries with only the found values, removes non-found items and adds new ones
                        itemsByCategory[fbc.Key].IgnoredItems = foundbyCategory[fbc.Key].IgnoredItems;
                        itemsByCategory[fbc.Key].ItemsByBoss = foundbyCategory[fbc.Key].ItemsByBoss;
                    }
                    else
                    {
                        // Just remove items that are not found in the config
                        itemsByCategory[fbc.Key].IgnoredItems = foundbyCategory[fbc.Key].IgnoredItems.Where(e => itemsByCategory[fbc.Key].IgnoredItems.Contains(e)).ToList();
                        foreach (var entry in foundbyCategory[fbc.Key].ItemsByBoss)
                        {
                            itemsByCategory[fbc.Key].ItemsByBoss[entry.Key] = itemsByCategory[fbc.Key].ItemsByBoss[entry.Key].Where(e => entry.Value.Contains(e)).ToList();
                        }
                    }
                }
            }
            else
            {
                // Just add found items, dont remove missing items
                foreach (var fbc in foundbyCategory)
                {
                    if (ELConfig.AutoAddEquipment.Value)
                    {
                        // Replace entries with only the found values, removes non-found items and adds new ones
                        itemsByCategory[fbc.Key].IgnoredItems = itemsByCategory[fbc.Key].IgnoredItems.Union(itemsByCategory[fbc.Key].IgnoredItems).ToList();
                        foreach (var entry in fbc.Value.ItemsByBoss)
                        {
                            itemsByCategory[fbc.Key].ItemsByBoss[entry.Key] = itemsByCategory[fbc.Key].ItemsByBoss[entry.Key].Union(entry.Value).ToList();
                        }
                    }
                }
            }

            // merge dataset and ensure unique values
            List<ItemTypeInfo> newConfig = new List<ItemTypeInfo>();
            foreach (var item in itemsByCategory) {
                if (item.Value.ItemsByBoss.Count > 0 || item.Value.IgnoredItems.Count > 0) {
                    Dictionary<string, List<string>> itemsByBossUniques = new();
                    foreach(var entry in item.Value.ItemsByBoss) {
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



            // Write out the new config, which will trigger a reload of the config
            try {
                string contents = ELConfig.yamlserializer.Serialize(new ItemInfoConfig() { ItemInfo = newConfig });
                string overhaul_file_location = ELConfig.GetOverhaulDirectoryPath() + '\\' + "iteminfo.yaml";
                File.WriteAllText(overhaul_file_location, contents);
            } catch (Exception e) {
                EpicLoot.LogError($"Failed to auto-add items to iteminfo.yaml: {e.Message}");
                return;
            }
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
                    return "Utility";
                default:
                    return itemType.ToString().ToLower();
                }


            EpicLoot.LogWarning($"Unknown item type for item {item.m_shared.m_name}: {itemType}");
            return "Unkown";
        }

        public static string DetermineBossLevelForItem(ItemDrop.ItemData item)
        {
            Recipe itemRecipe = ObjectDB.instance.GetRecipe(item);
            if (itemRecipe == null || itemRecipe.m_enabled == false) { return "none"; }

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
