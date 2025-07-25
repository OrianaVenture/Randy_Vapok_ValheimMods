using BepInEx;
using BepInEx.Configuration;
using Common;
using EpicLoot.Abilities;
using EpicLoot.Adventure;
using EpicLoot.Crafting;
using EpicLoot.CraftingV2;
using EpicLoot.GatedItemType;
using EpicLoot.LegendarySystem;
using EpicLoot.Patching;
using EpicLoot_UnityLib;
using Jotunn.Entities;
using Jotunn.Managers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using YamlDotNet.Serialization;

namespace EpicLoot.Config
{
    internal class ELConfig
    {
        public static ConfigFile cfg;

        public static ConfigEntry<string> _setItemColor;
        public static ConfigEntry<string> _magicRarityColor;
        public static ConfigEntry<string> _rareRarityColor;
        public static ConfigEntry<string> _epicRarityColor;
        public static ConfigEntry<string> _legendaryRarityColor;
        public static ConfigEntry<string> _mythicRarityColor;
        public static ConfigEntry<int> _magicMaterialIconColor;
        public static ConfigEntry<int> _rareMaterialIconColor;
        public static ConfigEntry<int> _epicMaterialIconColor;
        public static ConfigEntry<int> _legendaryMaterialIconColor;
        public static ConfigEntry<int> _mythicMaterialIconColor;
        public static ConfigEntry<bool> UseScrollingCraftDescription;
        public static ConfigEntry<bool> TransferMagicItemToCrafts;
        public static ConfigEntry<CraftingTabStyle> CraftingTabStyle;
        public static ConfigEntry<bool> _loggingEnabled;
        public static ConfigEntry<LogLevel> _logLevel;
        public static ConfigEntry<bool> UseGeneratedMagicItemNames;
        public static ConfigEntry<GatedItemTypeMode> _gatedItemTypeModeConfig;
        public static ConfigEntry<GatedBountyMode> BossBountyMode;
        public static ConfigEntry<GatedPieceTypeMode> GatedFreebuildMode;
        public static ConfigEntry<BossDropMode> _bossTrophyDropMode;
        public static ConfigEntry<float> _bossTrophyDropPlayerRange;
        public static ConfigEntry<int> _andvaranautRange;
        public static ConfigEntry<bool> ShowEquippedAndHotbarItemsInSacrificeTab;
        public static ConfigEntry<bool> _adventureModeEnabled;
        public static readonly ConfigEntry<string>[] AbilityKeyCodes = new ConfigEntry<string>[AbilityController.AbilitySlotCount];
        public static ConfigEntry<TextAnchor> AbilityBarAnchor;
        public static ConfigEntry<Vector2> AbilityBarPosition;
        public static ConfigEntry<TextAnchor> AbilityBarLayoutAlignment;
        public static ConfigEntry<float> AbilityBarIconSpacing;
        public static ConfigEntry<float> SetItemDropChance;
        public static ConfigEntry<float> GlobalDropRateModifier;
        public static ConfigEntry<float> ItemsToMaterialsDropRatio;
        public static ConfigEntry<bool> AlwaysShowWelcomeMessage;
        public static ConfigEntry<bool> OutputPatchedConfigFiles;
        public static ConfigEntry<bool> EnchantingTableUpgradesActive;
        public static ConfigEntry<bool> EnableLimitedBountiesInProgress;
        public static ConfigEntry<int> MaxInProgressBounties;
        public static ConfigEntry<EnchantingTabs> EnchantingTableActivatedTabs;
        public static ConfigEntry<BossDropMode> _bossCryptKeyDropMode;
        public static ConfigEntry<float> _bossCryptKeyDropPlayerRange;
        public static ConfigEntry<BossDropMode> _bossWishboneDropMode;
        public static ConfigEntry<float> _bossWishboneDropPlayerRange;
        public static ConfigEntry<string> BalanceConfigurationType;
        public static ConfigEntry<bool> AutoAddEquipment;
        public static ConfigEntry<bool> AutoRemoveEquipmentNotFound;
        public static ConfigEntry<bool> OnlyAddEquipmentWithRecipes;
        public static ConfigEntry<float> ItemsUnidentifiedDropRatio;
        public static ConfigEntry<float> UIAudioVolumeAdjustment;
        public static ConfigEntry<float> IdentificationBonusPower;

        public static ConfigEntry<bool> RuneExtractDestroysItem;

        private static CustomRPC LootTablesRPC;
        private static CustomRPC MagicEffectsRPC;
        private static CustomRPC ItemConfigRPC;
        private static CustomRPC RecipesRPC;
        private static CustomRPC EnchantingCostsRPC;
        private static CustomRPC ItemNamesRPC;
        private static CustomRPC AdventureDataRPC;
        private static CustomRPC LegendariesRPC;
        private static CustomRPC AbilitiesRPC;
        private static CustomRPC MaterialConversionRPC;
        private static CustomRPC EnchantingUpgradesRPC;

        private static string LocalizationDir = GetLocalizationDirectoryPath();
        private static readonly List<string> LocalizationLanguages = new List<string>() {
            "English",
            "Swedish",
            "French",
            "Italian",
            "German",
            "Spanish",
            "Russian",
            "Romanian",
            "Bulgarian",
            "Macedonian",
            "Finnish",
            "Danish",
            "Norwegian",
            "Icelandic",
            "Turkish",
            "Lithuanian",
            "Czech",
            "Hungarian",
            "Slovak",
            "Polish",
            "Dutch",
            "Portuguese_European",
            "Portuguese_Brazilian",
            "Chinese",
            "Chinese_Trad",
            "Japanese",
            "Korean",
            "Hindi",
            "Thai",
            "Abenaki",
            "Croatian",
            "Georgian",
            "Greek",
            "Serbian",
            "Ukrainian",
            "Latvian"
        };

        public static IDeserializer yamldeserializer = new DeserializerBuilder().Build();
        public static ISerializer yamlserializer = new SerializerBuilder().DisableAliases().ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitDefaults).Build();
        public static ISerializer yamlserializerforRPC = new SerializerBuilder().JsonCompatible().ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitDefaults).Build();

        public ELConfig(ConfigFile Config)
        {
            // ensure all the config values are created
            cfg = Config;
            cfg.SaveOnConfigSet = true;
            CreateConfigValues(Config);
            SetupConfigRPCs();
            InitializeConfig();
        }

        public void SetupConfigRPCs()
        {
            LootTablesRPC = NetworkManager.Instance.AddRPC("epicloot_loottables_RPC", OnServerRecieveConfigs, OnClientRecieveLootConfigs);
            MagicEffectsRPC = NetworkManager.Instance.AddRPC("epicloot_magiceffect_RPC", OnServerRecieveConfigs, OnClientRecieveMagicConfigs);
            ItemConfigRPC = NetworkManager.Instance.AddRPC("epicloot_itemconfig_RPC", OnServerRecieveConfigs, OnClientRecieveItemInfoConfigs);
            RecipesRPC = NetworkManager.Instance.AddRPC("epicloot_recipes_RPC", OnServerRecieveConfigs, OnClientRecieveRecipesConfigs);
            EnchantingCostsRPC = NetworkManager.Instance.AddRPC("epicloot_enchantingcosts_RPC", OnServerRecieveConfigs, OnClientRecieveEnchantingCostsConfigs);
            ItemNamesRPC = NetworkManager.Instance.AddRPC("ItemNamesRPC", OnServerRecieveConfigs, OnClientRecieveItemNameConfigs);
            AdventureDataRPC = NetworkManager.Instance.AddRPC("AdventureDataRPC", OnServerRecieveConfigs, OnClientRecieveAdventureDataConfigs);
            LegendariesRPC = NetworkManager.Instance.AddRPC("LegendariesRPC", OnServerRecieveConfigs, OnClientRecieveLegendaryItemConfigs);
            AbilitiesRPC = NetworkManager.Instance.AddRPC("AbilitiesRPC", OnServerRecieveConfigs, OnClientRecieveAbilityConfigs);
            MaterialConversionRPC = NetworkManager.Instance.AddRPC("MaterialConversionRPC", OnServerRecieveConfigs, OnClientRecieveMaterialConversionConfigs);
            EnchantingUpgradesRPC = NetworkManager.Instance.AddRPC("EnchantingUpgradesRPC", OnServerRecieveConfigs, OnClientRecieveEnchantingUpgradesConfigs);
        }

        private void CreateConfigValues(ConfigFile Config)
        {
            // Item Colors
            _magicRarityColor = Config.Bind("Item Colors", "Magic Rarity Color", "Blue",
                "The color of Magic rarity items, the lowest magic item tier. " +
                "(Optional, use an HTML hex color starting with # to have a custom color.) " +
                "Available options: Red, Orange, Yellow, Green, Teal, Blue, Indigo, Purple, Pink, Gray");
            _magicMaterialIconColor = Config.Bind("Item Colors", "Magic Crafting Material Icon Index", 5,
                "Indicates the color of the icon used for magic crafting materials. A number between 0 and 9. " +
                "Available options: 0=Red, 1=Orange, 2=Yellow, 3=Green, 4=Teal, 5=Blue, 6=Indigo, 7=Purple, 8=Pink, 9=Gray");
            _rareRarityColor = Config.Bind("Item Colors", "Rare Rarity Color", "Yellow",
                "The color of Rare rarity items, the second magic item tier. " +
                "(Optional, use an HTML hex color starting with # to have a custom color.) " +
                "Available options: Red, Orange, Yellow, Green, Teal, Blue, Indigo, Purple, Pink, Gray");
            _rareMaterialIconColor = Config.Bind("Item Colors", "Rare Crafting Material Icon Index", 2,
                "Indicates the color of the icon used for rare crafting materials. A number between 0 and 9. " +
                "Available options: 0=Red, 1=Orange, 2=Yellow, 3=Green, 4=Teal, 5=Blue, 6=Indigo, 7=Purple, 8=Pink, 9=Gray");
            _epicRarityColor = Config.Bind("Item Colors", "Epic Rarity Color", "Purple",
                "The color of Epic rarity items, the third magic item tier. " +
                "(Optional, use an HTML hex color starting with # to have a custom color.) " +
                "Available options: Red, Orange, Yellow, Green, Teal, Blue, Indigo, Purple, Pink, Gray");
            _epicMaterialIconColor = Config.Bind("Item Colors", "Epic Crafting Material Icon Index", 7,
                "Indicates the color of the icon used for epic crafting materials. A number between 0 and 9. " +
                "Available options: 0=Red, 1=Orange, 2=Yellow, 3=Green, 4=Teal, 5=Blue, 6=Indigo, 7=Purple, 8=Pink, 9=Gray");
            _legendaryRarityColor = Config.Bind("Item Colors", "Legendary Rarity Color", "Teal",
                "The color of Legendary rarity items, the fourth magic item tier. " +
                "(Optional, use an HTML hex color starting with # to have a custom color.) " +
                "Available options: Red, Orange, Yellow, Green, Teal, Blue, Indigo, Purple, Pink, Gray");
            _legendaryMaterialIconColor = Config.Bind("Item Colors", "Legendary Crafting Material Icon Index", 4,
                "Indicates the color of the icon used for legendary crafting materials. A number between 0 and 9. " +
                "Available options: 0=Red, 1=Orange, 2=Yellow, 3=Green, 4=Teal, 5=Blue, 6=Indigo, 7=Purple, 8=Pink, 9=Gray");
            _mythicRarityColor = Config.Bind("Item Colors", "Mythic Rarity Color", "Orange",
                "The color of Mythic rarity items, the highest magic item tier. " +
                "(Optional, use an HTML hex color starting with # to have a custom color.) " +
                "Available options: Red, Orange, Yellow, Green, Teal, Blue, Indigo, Purple, Pink, Gray");
            _mythicMaterialIconColor = Config.Bind("Item Colors", "Mythic Crafting Material Icon Index", 1,
                "Indicates the color of the icon used for legendary crafting materials. A number between 0 and 9. " +
                "Available options: 0=Red, 1=Orange, 2=Yellow, 3=Green, 4=Teal, 5=Blue, 6=Indigo, 7=Purple, 8=Pink, 9=Gray");
            _setItemColor = Config.Bind("Item Colors", "Set Item Color", "#26ffff",
                "The color of set item text and the set item icon. Use a hex color, default is cyan");

            // Crafting UI
            UseScrollingCraftDescription = Config.Bind("Crafting UI", "Use Scrolling Craft Description", true,
                "Changes the item description in the crafting panel to scroll instead of scale when it gets too " +
                "long for the space.");
            CraftingTabStyle = Config.Bind("Crafting UI", "Crafting Tab Style", Crafting.CraftingTabStyle.HorizontalSquish,
                "Sets the layout style for crafting tabs, if you've got too many. " +
                "Horizontal is the vanilla method, but might overlap other mods or run off the screen. " +
                "HorizontalSquish makes the buttons narrower, works okay with 6 or 7 buttons. " +
                "Vertical puts the tabs in a column to the left the crafting window. " +
                "Angled tries to make more room at the top of the crafting panel by angling the tabs, " +
                "works okay with 6 or 7 tabs.");
            ShowEquippedAndHotbarItemsInSacrificeTab = Config.Bind("Crafting UI",
                "ShowEquippedAndHotbarItemsInSacrificeTab", false,
                "If set to false, hides the items that are equipped or on your hotbar in the Sacrifice items list.");
            UIAudioVolumeAdjustment = Config.Bind("Crafting UI", "AudioVolumeAdjustment", 1.0f, 
                new ConfigDescription("Multiplies the crafting UI sound volume by this percentage [0.0-2.0], 2 = doublely loud UI sounds, 0 = no UI sounds.",
                new AcceptableValueRange<float>(0, 2)));

            // Logging
            _loggingEnabled = Config.Bind("Logging", "Logging Enabled", false, "Enable logging");
            _logLevel = Config.Bind("Logging", "Log Level", LogLevel.Info,
                "Only log messages of the selected level or higher");

            // General
            UseGeneratedMagicItemNames = Config.Bind("General", "Use Generated Magic Item Names", true,
                "If true, magic items uses special, randomly generated names based on their rarity, type, and magic effects.");
            AutoAddEquipment = BindServerConfig("General", "Auto Add Equipment", true, "Automatically adds equipment types that can be enchanted to possible drops and gates them" +
                "behind their respective bosses. Disabling this also disables automatic removal of items not found.");
            AutoRemoveEquipmentNotFound = BindServerConfig("General", "Auto Remove Equipment Not Found", true,
                "Automatically removes equipment types that is not found when loading the game.");
            OnlyAddEquipmentWithRecipes = BindServerConfig("General", "Only Add Equipment With Recipes", true,
                "Equipment must be able to be created by a recipe in order to automatically get selected. If this is disabled enemy weapons can be added to drops, they are not always valid.");

            // Balance
            BalanceConfigurationType = BindServerConfig("Balance", "Balance Template", "Default",
                "Sets the type of balance configuration to use. " +
                "balanced: the recommended balancing, you'll be powerful but stronger enemies will still be a huge threat. " +
                "vanilla: uses heavy downtuning to not make you overpowered without any mods to increase difficulty." +
                "legacy: this was the only balance option prior to 0.12, used by itself this will make you godlike.", new AcceptableValueList<string>("balanced", "legacy", "vanilla"));
            _gatedItemTypeModeConfig = BindServerConfig("Balance", "Item Drop Limits",
                GatedItemTypeMode.BossKillUnlocksCurrentBiomeItems,
                "Sets how the drop system limits what item types can drop. " +
                "Unlimited: no limits, exactly what's in the loot table will drop. " +
                "BossKillUnlocksCurrentBiomeItems: items will drop for the current biome if the that biome's boss has been killed " +
                "(Leather gear will drop once Eikthyr is killed). " +
                "BossKillUnlocksNextBiomeItems: items will only drop for the current biome if the previous biome's boss is killed " +
                "(Bronze gear will drop once Eikthyr is killed). " +
                "PlayerMustKnowRecipe: (local world only) the item can drop if the player can craft it. " +
                "PlayerMustHaveCraftedItem: (local world only) the item can drop if the player has already crafted it " +
                "or otherwise picked it up. If an item type cannot drop, it will downgrade to an item of the same type and " +
                "skill that the player has unlocked (i.e. swords will stay swords) according to iteminfo.json.");
            BossBountyMode = BindServerConfig("Balance", "Gated Bounty Mode", GatedBountyMode.Unlimited,
                "Sets whether available bounties are ungated or gated by boss kills.");
            GatedFreebuildMode = Config.Bind("Balance", "Gated Freebuild Mode", GatedPieceTypeMode.BossKillUnlocksCurrentBiomePieces,
                "Sets whether available pieces for the Freebuild effect are ungated or gated by boss kills.");
            _bossTrophyDropMode = BindServerConfig("Balance", "Boss Trophy Drop Mode", BossDropMode.OnePerPlayerNearBoss,
                "Sets bosses to drop a number of trophies equal to the number of players. " +
                "Optionally set it to only include players within a certain distance, " +
                "use 'Boss Trophy Drop Player Range' to set the range.");
            _bossTrophyDropPlayerRange = BindServerConfig("Balance", "Boss Trophy Drop Player Range", 100.0f,
                "Sets the range that bosses check when dropping multiple trophies using the OnePerPlayerNearBoss drop mode.");
            _bossCryptKeyDropMode = BindServerConfig("Balance", "Crypt Key Drop Mode", BossDropMode.OnePerPlayerNearBoss,
                "Sets bosses to drop a number of crypt keys equal to the number of players. " +
                "Optionally set it to only include players within a certain distance, " +
                "use 'Crypt Key Drop Player Range' to set the range.");
            _bossCryptKeyDropPlayerRange = BindServerConfig("Balance", "Crypt Key Drop Player Range", 100.0f,
                "Sets the range that bosses check when dropping multiple crypt keys using the OnePerPlayerNearBoss drop mode.");
            _bossWishboneDropMode = BindServerConfig("Balance", "Wishbone Drop Mode", BossDropMode.OnePerPlayerNearBoss,
                "Sets bosses to drop a number of wishbones equal to the number of players. " +
                "Optionally set it to only include players within a certain distance, " +
                "use 'Crypt Key Drop Player Range' to set the range.");
            _bossWishboneDropPlayerRange = BindServerConfig("Balance", "Wishbone Drop Player Range", 100.0f,
                "Sets the range that bosses check when dropping multiple wishbones using the OnePerPlayerNearBoss drop mode.");
            _adventureModeEnabled = BindServerConfig("Balance", "Adventure Mode Enabled", true,
                "Set to true to enable all the adventure mode features: secret stash, gambling, treasure maps, and bounties. " +
                "Set to false to disable. This will not actually remove active treasure maps or bounties from your save.");
            _andvaranautRange = BindServerConfig("Balance", "Andvaranaut Range", 20,
                "Sets the range that Andvaranaut will locate a treasure chest.");
            SetItemDropChance = BindServerConfig("Balance", "Set Item Drop Chance", 0.15f,
                "The percent chance that a legendary item will be a set item. Min = 0, Max = 1");
            GlobalDropRateModifier = BindServerConfig("Balance", "Global Drop Rate Modifier", 1.0f,
                "A global percentage that modifies how likely items are to drop. " +
                "1 = Exactly what is in the loot tables will drop. " +
                "0 = Nothing will drop. " +
                "2 = The number of items in the drop table are twice as likely to drop " +
                "(note, this doesn't double the number of items dropped, just doubles the relative chance for them to drop). " +
                "Min = 0, Max = 4");
            ItemsUnidentifiedDropRatio = BindServerConfig("Balance", "Items Unidentified Drop Ratio", 0.0f, "" +
                "Sets the chance that items are dropped as unidentified. This takes precedent over 'Items To Materials Drop Ratio' which means" +
                "out of 100 items dropped, unidenified items are first calculated, and then the remainder is used to calculate material vs item drop.");
            ItemsToMaterialsDropRatio = BindServerConfig("Balance", "Items To Materials Drop Ratio", 0.0f,
                "Sets the chance that item drops are instead dropped as magic crafting materials. " +
                "0 = all items, no materials. " +
                "1 = all materials, no items. Values between 0 and 1 change the ratio of items to materials that drop. " +
                "At 0.5, half of everything that drops would be items and the other half would be materials. " +
                "Min = 0, Max = 1");
            TransferMagicItemToCrafts = BindServerConfig("Balance", "Transfer Enchants to Crafted Items", false,
                "When enchanted items are used as ingredients in recipes, transfer the highest enchant to the " +
                "newly crafted item. Default: False.");
            RuneExtractDestroysItem = BindServerConfig("Balance", "Rune Extract Destroys Item", true,
                "When extracting a rune from an item, the item will be destroyed. If false, the item will be returned intact. " +
                "Default: True.");
            IdentificationBonusPower = BindServerConfig("Balance", "Identification Bonus Power", 1.1f, "Power modification applied to effects that are identified.");


            // Debug
            AlwaysShowWelcomeMessage = Config.Bind("Debug", "AlwaysShowWelcomeMessage", false,
                "Just a debug flag for testing the welcome message, do not use.");
            OutputPatchedConfigFiles = Config.Bind("Debug", "OutputPatchedConfigFiles", false,
                "Just a debug flag for testing the patching system, do not use.");

            // Abilities
            AbilityKeyCodes[0] = Config.Bind("Abilities", "Ability Hotkey 1", "g", "Hotkey for Ability Slot 1.");
            AbilityKeyCodes[1] = Config.Bind("Abilities", "Ability Hotkey 2", "h", "Hotkey for Ability Slot 2.");
            AbilityKeyCodes[2] = Config.Bind("Abilities", "Ability Hotkey 3", "j", "Hotkey for Ability Slot 3.");
            AbilityBarAnchor = Config.Bind("Abilities", "Ability Bar Anchor", TextAnchor.LowerLeft,
                "The point on the HUD to anchor the ability bar. Changing this also changes the pivot of the ability bar to that corner. " +
                "For reference: the ability bar size is 208 by 64.");
            AbilityBarPosition = Config.Bind("Abilities", "Ability Bar Position", new Vector2(150, 170),
                "The position offset from the Ability Bar Anchor at which to place the ability bar.");
            AbilityBarLayoutAlignment = Config.Bind("Abilities", "Ability Bar Layout Alignment", TextAnchor.LowerLeft,
                "The Ability Bar is a Horizontal Layout Group. This value indicates how the elements inside are aligned. " +
                "Choices with 'Center' in them will keep the items centered on the bar, even if there are fewer than the maximum allowed. " +
                "'Left' will be left aligned, and similar for 'Right'.");
            AbilityBarIconSpacing = Config.Bind("Abilities", "Ability Bar Icon Spacing", 8.0f, "The number of units between the icons on the ability bar.");

            // Enchanting Table
            EnchantingTableUpgradesActive = BindServerConfig("Enchanting Table", "Upgrades Active", true,
                "Toggles Enchanting Table Upgrade Capabilities. If false, enchanting table features will be unlocked set to Level 1");
            EnchantingTableActivatedTabs = BindServerConfig("Enchanting Table", $"Table Features Active",
                EnchantingTabs.Sacrifice | EnchantingTabs.Augment | EnchantingTabs.Enchant | EnchantingTabs.Disenchant |
                EnchantingTabs.Upgrade | EnchantingTabs.ConvertMaterials, $"Toggles Enchanting Table Feature on and off completely.");
            EnchantingTableUpgradesActive.SettingChanged += (_, _) => EnchantingTableUI.UpdateUpgradeActivation();
            EnchantingTableActivatedTabs.SettingChanged += (_, _) => EnchantingTableUI.UpdateTabActivation();

            // Bounty Management
            EnableLimitedBountiesInProgress = BindServerConfig("Bounty Management", "Enable Bounty Limit", false,
                "Toggles limiting bounties. Players unable to purchase if enabled and maximum bounty in-progress count is met");
            MaxInProgressBounties = BindServerConfig("Bounty Management", "Max Bounties Per Player", 5,
                "Max amount of in-progress bounties allowed per player.");
        }

        public static void InitializeConfig()
        {
            SychronizeConfig<LootConfig>("loottables.yaml", LootRoller.Initialize, LootTablesRPC, LootRoller.Config);
            SychronizeConfig<MagicItemEffectsList>("magiceffects.yaml", MagicItemEffectDefinitions.Initialize, MagicEffectsRPC, MagicItemEffectDefinitions.GetMagicItemEffectDefinitions());
            // Adventure data has to be loaded before iteminfo, as iteminfo uses the adventure data to determine what items can drop
            SychronizeConfig<AdventureDataConfig>("adventuredata.yaml", AdventureDataManager.Initialize, AdventureDataRPC, AdventureDataManager.Config);
            SychronizeConfig<ItemInfoConfig>("iteminfo.yaml", GatedItemTypeHelper.Initialize, ItemConfigRPC, GatedItemTypeHelper.GatedConfig);
            SychronizeConfig<RecipesConfig>("recipes.yaml", RecipesHelper.Initialize, RecipesRPC, RecipesHelper.Config);
            SychronizeConfig<EnchantingCostsConfig>("enchantcosts.yaml", EnchantCostsHelper.Initialize, EnchantingCostsRPC, EnchantCostsHelper.Config);
            SychronizeConfig<ItemNameConfig>("itemnames.yaml", MagicItemNames.Initialize, ItemNamesRPC, MagicItemNames.Config);
            SychronizeConfig<LegendaryItemConfig>("legendaries.yaml", UniqueLegendaryHelper.Initialize, LegendariesRPC, UniqueLegendaryHelper.Config);
            SychronizeConfig<AbilityConfig>("abilities.yaml", AbilityDefinitions.Initialize, AbilitiesRPC, AbilityDefinitions.Config);
            SychronizeConfig<MaterialConversionsConfig>("materialconversions.yaml", MaterialConversions.Initialize, MaterialConversionRPC, MaterialConversions.Config);
            SychronizeConfig<EnchantingUpgradesConfig>("enchantingupgrades.yaml", EnchantingTableUpgrades.InitializeConfig, EnchantingUpgradesRPC, EnchantingTableUpgrades.Config);
            //SetupPatchConfigFileWatch(FilePatching.PatchesDirPath);
            // SetupConfigFileWatcher(ELConfig.GetOverhaulDirectoryPath());

            ItemManager.OnItemsRegistered += InitializeRecipeOnReady;
        }

        /// <summary>
        /// Recipes cannot be created until the game is launched.
        /// Watch for issues, this can potentially trigger after client config synchronization and break.
        /// </summary>
        private static void InitializeRecipeOnReady()
        {
            var jsonFile = EpicLoot.ReadEmbeddedResourceFile("EpicLoot.config.recipes.json");
            var result = JsonConvert.DeserializeObject<RecipesConfig>(jsonFile);
            if (RecipesHelper.Config == null) {
                RecipesHelper.Initialize(result);
            } else {
                RecipesHelper.Initialize(RecipesHelper.Config);
            }
            ItemManager.OnItemsRegistered -= InitializeRecipeOnReady;
        }

        public static string GetLocalizationDirectoryPath()
        {
            var localizationFolder = Path.Combine(Paths.ConfigPath, "EpicLoot", "localizations");
            var dirInfo = Directory.CreateDirectory(localizationFolder);
            return dirInfo.FullName;
        }

        public static string GetOverhaulDirectoryPath()
        {
            var overhaulfolder = Path.Combine(Paths.ConfigPath, "EpicLoot", "baseconfig");
            var dirInfo = Directory.CreateDirectory(overhaulfolder);
            return dirInfo.FullName;
        }

        public static void SychronizeConfig<T>(string filename, Action<T> setupMethod, CustomRPC targetRPC, T config) where T : class
        {
            string overhaul_file_location = ELConfig.GetOverhaulDirectoryPath() + '\\' + filename;

            // Ensure that the core config file exists
            if (File.Exists(overhaul_file_location) == false) {
                EpicLoot.Log($"Base config file {overhaul_file_location} does not exist, creating it from embedded default config.");
                var overhaulfiledata = EpicLoot.ReadEmbeddedResourceFile("EpicLoot.config.overhauls." + BalanceConfigurationType.Value + "." + filename);
                File.WriteAllText(overhaul_file_location, overhaulfiledata);
            }

            // Attempt to parse the core config, if its not valid use the embedded default config
            try {
                var contents = yamldeserializer.Deserialize<T>(File.ReadAllText(overhaul_file_location));
                setupMethod(contents);
            } catch (Exception e) {
                EpicLoot.LogWarningForce($"Core Config file {overhaul_file_location} is invalid and internal defaults will be used instead." + e);
                var defaultConfig = EpicLoot.ReadEmbeddedResourceFile("EpicLoot.config.overhauls." + BalanceConfigurationType.Value + "." + filename);
                setupMethod(yamldeserializer.Deserialize<T>(defaultConfig));
            }

            EpicLoot.Log($"Core Config file {overhaul_file_location} Loaded and set.");
            // At this point we have a valid config, either from external file or from embedded defaults.

            // Setup the initial synchronization for network connection
            SynchronizationManager.Instance.AddInitialSynchronization(targetRPC, _ => SendConfig(yamlserializerforRPC.Serialize(config)));

            // Encapsulated file watcher modification method for the config file
            void FileModified(object sender, FileSystemEventArgs e) {
                if (e.FullPath != overhaul_file_location) { return; }
                EpicLoot.Log($"Config file {overhaul_file_location} {e.FullPath} has been modified, attempting to update config.");
                if (!File.Exists(overhaul_file_location)) { return; }
                bool valid_update = false;
                try {
                    var contents = yamldeserializer.Deserialize<T>(File.ReadAllText(overhaul_file_location));
                    EpicLoot.Log($"Config file {overhaul_file_location} has been modified, updating config.");
                    setupMethod(contents);
                    valid_update = true;
                } catch (Exception ex) {
                    EpicLoot.LogWarningForce($"Config file {overhaul_file_location} is invalid and config will not be updated." + ex);
                }
                if (valid_update == false) { return; }
                if (GUIManager.IsHeadless()) {
                    try {
                        targetRPC.SendPackage(ZNet.instance.m_peers, SendConfig(yamlserializerforRPC.Serialize(config)));
                        Jotunn.Logger.LogInfo("Sent levels configs to clients.");
                    }
                    catch {
                        Jotunn.Logger.LogError($"Error while server syncing {overhaul_file_location} configs");
                    }
                }
            }

            // Setup the file watcher for the config file
            var fsw = new FileSystemWatcher(ELConfig.GetOverhaulDirectoryPath());
            fsw.Created += new FileSystemEventHandler(FileModified);
            fsw.Changed += new FileSystemEventHandler(FileModified);
            fsw.Renamed += new RenamedEventHandler(FileModified);
            fsw.Deleted += new FileSystemEventHandler(FileModified);
            fsw.NotifyFilter = NotifyFilters.LastWrite;
            fsw.SynchronizingObject = ThreadingHelper.SynchronizingObject;
            fsw.EnableRaisingEvents = true;
            fsw.Filter = filename;
        }

        public static void StartupProcessModifiedLocalizations()
        {
            string[] files = Directory.GetFiles(LocalizationDir, "*", SearchOption.AllDirectories);
            EpicLoot.Log($"Processing localization startup file patches: {string.Join(",", files)}");
            foreach (var file in files)
            {
                if (!file.Contains(".json")) {
                    EpicLoot.Log($"File: {file} is not a supported format, ignoring.");
                    continue;
                }
                var fileInfo = new FileInfo(file);
                string language = file.Trim().Split(Path.DirectorySeparatorChar).Last().Split('.').First().Trim();
                if (!LocalizationLanguages.Contains(language)) {
                    EpicLoot.LogWarning($"{language} is not a supported language [{string.Join(", ", LocalizationLanguages.ToArray())}]");
                    continue;
                }
                Dictionary<string, string> localization_updates = new Dictionary<string, string>();
                string contents = File.ReadAllText(file);
                string cleaned_localization = Regex.Replace(contents, @"\/\/.*\n", "");
                localization_updates = JsonConvert.DeserializeObject<Dictionary<string, string>>(cleaned_localization);

                CheckAndUpdateLocalization(localization_updates, language);
            }
        }

        internal static void CheckAndUpdateLocalization(Dictionary<string, string> localization_updates, string language)
        {
            foreach (var localization in localization_updates)
            {
                EpicLoot.Log($"Updating localization: {localization.Key} - {localization.Value}");
                LocalizationManager.Instance.GetLocalization().ClearToken(language, localization.Key);
                LocalizationManager.Instance.GetLocalization().AddTranslation(language, localization.Key, localization.Value);
            }
        }

        public static void SetupConfigFileWatcher(string path, Action fileModificationMethod)
        {

        }

        private static IEnumerator OnClientRecieveLootConfigs(long sender, ZPackage package)
        {
            LootRoller.Initialize(ClientRecieveParseJsonConfig<LootConfig>(package.ReadString()));
            yield return null;
        }

        private static IEnumerator OnClientRecieveMagicConfigs(long sender, ZPackage package)
        {
            MagicItemEffectDefinitions.Initialize(ClientRecieveParseJsonConfig<MagicItemEffectsList>(package.ReadString()));
            yield return null;
        }

        private static IEnumerator OnClientRecieveItemInfoConfigs(long sender, ZPackage package)
        {
            GatedItemTypeHelper.Initialize(ClientRecieveParseJsonConfig<ItemInfoConfig>(package.ReadString()));
            yield return null;
        }

        private static IEnumerator OnClientRecieveRecipesConfigs(long sender, ZPackage package)
        {
            RecipesHelper.Initialize(ClientRecieveParseJsonConfig<RecipesConfig>(package.ReadString()));
            yield return null;
        }

        private static IEnumerator OnClientRecieveEnchantingCostsConfigs(long sender, ZPackage package)
        {
            EnchantCostsHelper.Initialize(ClientRecieveParseJsonConfig<EnchantingCostsConfig>(package.ReadString()));
            yield return null;
        }

        private static IEnumerator OnClientRecieveItemNameConfigs(long sender, ZPackage package)
        {
            MagicItemNames.Initialize(ClientRecieveParseJsonConfig<ItemNameConfig>(package.ReadString()));
            yield return null;
        }

        private static IEnumerator OnClientRecieveAdventureDataConfigs(long sender, ZPackage package)
        {
            AdventureDataManager.UpdateAventureData(ClientRecieveParseJsonConfig<AdventureDataConfig>(package.ReadString()));
            yield return null;
        }

        private static IEnumerator OnClientRecieveLegendaryItemConfigs(long sender, ZPackage package)
        {
            UniqueLegendaryHelper.Initialize(ClientRecieveParseJsonConfig<LegendaryItemConfig>(package.ReadString()));
            yield return null;
        }

        private static IEnumerator OnClientRecieveAbilityConfigs(long sender, ZPackage package)
        {
            AbilityDefinitions.Initialize(ClientRecieveParseJsonConfig<AbilityConfig>(package.ReadString()));
            yield return null;
        }

        private static IEnumerator OnClientRecieveMaterialConversionConfigs(long sender, ZPackage package)
        {
            MaterialConversions.Initialize(ClientRecieveParseJsonConfig<MaterialConversionsConfig>(package.ReadString()));
            yield return null;
        }

        private static IEnumerator OnClientRecieveEnchantingUpgradesConfigs(long sender, ZPackage package)
        {
            EnchantingTableUpgrades.InitializeConfig(ClientRecieveParseJsonConfig<EnchantingUpgradesConfig>(package.ReadString()));
            yield return null;
        }

        private static T ClientRecieveParseJsonConfig<T>(string json)
        {
            try {
                return JsonConvert.DeserializeObject<T>(json);
            } catch (Exception e) {
                EpicLoot.LogError($"There was an error syncing client configs: {e}");
            }
            return default;
        }

        public static ZPackage SendConfig(string zpackage_content) {
            ZPackage package = new ZPackage();
            package.Write(zpackage_content);
            return package;
        }

        public static void SendLootConfigs() {
            LootTablesRPC.SendPackage(ZNet.instance.m_peers, SendConfig(JsonConvert.SerializeObject(LootRoller.Config)));
        }

        public static void SendMagicEffectConfigs() {
            MagicEffectsRPC.SendPackage(ZNet.instance.m_peers, SendConfig(JsonConvert.SerializeObject(MagicItemEffectDefinitions.GetMagicItemEffectDefinitions())));
        }

        public static void SendItemInfoConfigs() {
            ItemConfigRPC.SendPackage(ZNet.instance.m_peers, SendConfig(JsonConvert.SerializeObject(GatedItemTypeHelper.GatedConfig)));
        }

        public static void SendRecipesConfigs()
        {
            RecipesRPC.SendPackage(ZNet.instance.m_peers, SendConfig(JsonConvert.SerializeObject(RecipesHelper.Config)));
        }

        public static void SendEnchantCostConfigs()
        {
            EnchantingCostsRPC.SendPackage(ZNet.instance.m_peers, SendConfig(JsonConvert.SerializeObject(EnchantCostsHelper.Config)));
        }

        public static void SendMagicItemNamesConfigs()
        {
            ItemNamesRPC.SendPackage(ZNet.instance.m_peers, SendConfig(JsonConvert.SerializeObject(MagicItemNames.Config)));
        }

        public static void SendAdventureDataConfigs()
        {
            AdventureDataRPC.SendPackage(ZNet.instance.m_peers, SendConfig(JsonConvert.SerializeObject(AdventureDataManager.Config)));
        }

        public static void SendLegendaryConfigs()
        {
            LegendariesRPC.SendPackage(ZNet.instance.m_peers, SendConfig(JsonConvert.SerializeObject(UniqueLegendaryHelper.Config)));
        }

        public static void SendAbilitiesConfigs()
        {
            AbilitiesRPC.SendPackage(ZNet.instance.m_peers, SendConfig(JsonConvert.SerializeObject(AbilityDefinitions.Config)));
        }

        public static void SendMaterialConversionConfigs()
        {
            MaterialConversionRPC.SendPackage(ZNet.instance.m_peers, SendConfig(JsonConvert.SerializeObject(MaterialConversions.Config)));
        }

        public static void SendEnchantingTableUpgradeConfigs()
        {
            EnchantingUpgradesRPC.SendPackage(ZNet.instance.m_peers, SendConfig(JsonConvert.SerializeObject(EnchantingTableUpgrades.Config)));
        }

        public static IEnumerator OnServerRecieveConfigs(long sender, ZPackage package)
        {
            EpicLoot.Log("Server received config from client, rejecting due to being the server.");
            yield return null;
        }

        /// <summary>
        /// Helper to bind configs for <TYPE>
        /// </summary>
        /// IsAdminOnly ensures this is a server authoratative value
        /// <returns></returns>
        public static ConfigEntry<T> BindServerConfig<T>(string category, string key, T value, string description, AcceptableValueList<string> acceptableValues = null, bool advanced = false)
        {
            return cfg.Bind(category, key, value,
                new ConfigDescription(
                    description,
                    acceptableValues,
                new ConfigurationManagerAttributes { IsAdminOnly = true, IsAdvanced = advanced })
            );
        }
    }
}
