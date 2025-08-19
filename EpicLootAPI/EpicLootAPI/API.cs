using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;
using Object = UnityEngine.Object;

namespace EpicLootAPI;
public static class EpicLoot
{
    private static readonly List<MagicItemEffectDefinition> MagicItemEffectDefinitions = new();
    private static readonly List<AbilityDefinition> Abilities = new();
    private static readonly List<MaterialConversion> MaterialConversions = new();
    private static readonly LegendaryItemConfig LegendaryConfig = new();
    private static readonly RecipesConfig Recipes = new();
    private static readonly List<Sacrifice> Sacrifices = new List<Sacrifice>();
    
    /// <summary>
    /// Simple version of EpicLoot Data classes to JSON serialize
    /// </summary>
    
    #region Magic Item
    [Serializable][PublicAPI]
    public class MagicItemEffect
    {
        public const float DefaultValue = 1;

        public int Version = 1;
        public string EffectType = "";
        public float EffectValue;

        public MagicItemEffect()
        {
        }

        public MagicItemEffect(string type, float value = DefaultValue)
        {
            EffectType = type;
            EffectValue = value;
        }
    }

    [Serializable][PublicAPI]
    public class MagicItem
    {
        public int Version = 2;
        public ItemRarity Rarity;
        public List<MagicItemEffect> Effects = new List<MagicItemEffect>();
        public string TypeNameOverride = "";
        public int AugmentedEffectIndex = -1;
        public List<int> AugmentedEffectIndices = new List<int>();
        public string DisplayName = "";
        public string LegendaryID = "";
        public string SetID = "";
    }
    
    [PublicAPI]
    public static void Set(this MagicItemEffectDefinition.ValueDef? def, float min, float max, float increment)
    {
        def ??= new MagicItemEffectDefinition.ValueDef();
        def.MinValue = min;
        def.MaxValue = max;
        def.Increment = increment;
    }

    [Serializable][PublicAPI]
    public class MagicItemEffectDefinition
    {
        [Serializable]
        public class ValueDef
        {
            public float MinValue;
            public float MaxValue;
            public float Increment;
            
            public ValueDef(){}

            public ValueDef(float min, float max, float increment)
            {
                MinValue = min;
                MaxValue = max;
                Increment = increment;
            }
        }

        [Serializable][PublicAPI]
        public class ValuesPerRarityDef
        {
            public ValueDef? Magic;
            public ValueDef? Rare;
            public ValueDef? Epic;
            public ValueDef? Legendary;
            public ValueDef? Mythic;
        }

        public string Type;
        public string DisplayText;
        public string Description;
        public MagicItemEffectRequirements Requirements = new MagicItemEffectRequirements();
        public ValuesPerRarityDef ValuesPerRarity = new ValuesPerRarityDef();
        public float SelectionWeight = 1;
        public bool CanBeAugmented = true;
        public bool CanBeDisenchanted = true;
        public string Comment = "";
        public List<string> Prefixes = new List<string>();
        public List<string> Suffixes = new List<string>();
        public string EquipFx = "";
        public FxAttachMode EquipFxMode = FxAttachMode.Player;
        public string Ability = "";
        
        [Description("Adds your new magic item definition to a list, use RegisterMagicItems() to send to epic loot")]
        public MagicItemEffectDefinition(string effectType, string displayText = "", string description = "")
        {
            Type = effectType;
            DisplayText = displayText;
            Description = description;

            MagicItemEffectDefinitions.Add(this);
        }
    }

    [Serializable][PublicAPI]
    public class MagicItemEffectRequirements
    {
        private static List<string> _flags = new List<string>();
        public bool NoRoll;
        public bool ExclusiveSelf = true;
        public List<string> ExclusiveEffectTypes = new List<string>();
        public List<string> MustHaveEffectTypes = new List<string>();
        public List<string> AllowedItemTypes = new List<string>();
        public List<string> ExcludedItemTypes = new List<string>();
        public List<ItemRarity> AllowedRarities = new List<ItemRarity>();
        public List<ItemRarity> ExcludedRarities = new List<ItemRarity>();
        public List<Skills.SkillType> AllowedSkillTypes = new List<Skills.SkillType>();
        public List<Skills.SkillType> ExcludedSkillTypes = new List<Skills.SkillType>();
        public List<string> AllowedItemNames = new List<string>();
        public List<string> ExcludedItemNames = new List<string>();
        public bool? ItemHasPhysicalDamage;
        public bool? ItemHasElementalDamage;
        public bool? ItemHasChopDamage;
        public bool? ItemUsesDurability;
        public bool? ItemHasNegativeMovementSpeedModifier;
        public bool? ItemHasBlockPower;
        public bool? ItemHasParryPower;
        public bool? ItemHasNoParryPower;
        public bool? ItemHasArmor;
        public bool? ItemHasBackstabBonus;
        public bool? ItemUsesStaminaOnAttack;
        public bool? ItemUsesEitrOnAttack;
        public bool? ItemUsesHealthOnAttack;
        public bool? ItemUsesDrawStaminaOnAttack;

        public List<string> CustomFlags = new();
    }
    [PublicAPI]
    public enum FxAttachMode
    {
        None,
        Player,
        ItemRoot,
        EquipRoot
    }
    [PublicAPI]
    public enum ItemRarity
    {
        Magic,
        Rare,
        Epic,
        Legendary,
        Mythic
    }
    
    #endregion
    
    #region Legendary
    [Serializable][PublicAPI]
    public class GuaranteedMagicEffect
    {
        public string Type;
        public MagicItemEffectDefinition.ValueDef? Values;

        public GuaranteedMagicEffect(string type, MagicItemEffectDefinition.ValueDef values)
        {
            Type = type;
            Values = values;
        }
        
        public GuaranteedMagicEffect(string type, float min, float max, float increment) : this(type, new MagicItemEffectDefinition.ValueDef(min, max, increment)){}
    }
    
    [PublicAPI]
    public static void Add(this List<GuaranteedMagicEffect> list, string type, float min = 1, float max = 1,
        float increment = 1) => list.Add(new GuaranteedMagicEffect(type, min, max, increment));

    [Serializable][PublicAPI]
    public class TextureReplacement
    {
        public string ItemID;
        public string MainTexture;
        public string ChestTex;
        public string LegsTex;

        public TextureReplacement(string itemID, string mainTex = "", string chestTex = "", string legsTex = "")
        {
            ItemID = itemID;
            MainTexture = mainTex;
            ChestTex = chestTex;
            LegsTex = legsTex;
        }
    }

    [Serializable][PublicAPI]
    public class LegendaryInfo
    {
        public string ID;
        public string Name;
        public string Description;
        public MagicItemEffectRequirements Requirements = new ();
        public List<GuaranteedMagicEffect> GuaranteedMagicEffects = new List<GuaranteedMagicEffect>();
        public int GuaranteedEffectCount = -1;
        public float SelectionWeight = 1;
        public string EquipFx = "";
        public FxAttachMode EquipFxMode = FxAttachMode.Player;
        public List<TextureReplacement> TextureReplacements = new List<TextureReplacement>();
        public bool IsSetItem;
        public bool Enchantable;
        public List<RecipeRequirement> EnchantCost = new List<RecipeRequirement>();

        public LegendaryInfo(LegendaryType type, string ID, string name, string description)
        {
            this.ID = ID;
            Name = name;
            Description = description;

            switch (type)
            {
                case LegendaryType.Legend:
                    LegendaryConfig.LegendaryItems.Add(this);
                    break;
                case LegendaryType.Mythic:
                    LegendaryConfig.MythicItems.Add(this);
                    break;
            }
        }
    }
    [PublicAPI]
    public enum LegendaryType { Legend, Mythic }

    [Serializable][PublicAPI]
    public class SetBonusInfo
    {
        public int Count;
        public GuaranteedMagicEffect Effect;

        public SetBonusInfo(int count, string type, MagicItemEffectDefinition.ValueDef values)
        {
            Count = count;
            Effect = new GuaranteedMagicEffect(type, values);
        }
        
        public SetBonusInfo(int count, string type, float min, float max, float increment) : this (count, type, new MagicItemEffectDefinition.ValueDef(min, max, increment)){}
    }
    
    [PublicAPI]
    public static void Add(this List<SetBonusInfo> list, int count, string type, float min, float max, float increment) =>
        list.Add(new SetBonusInfo(count, type, min, max, increment));

    [Serializable][PublicAPI]
    public class LegendarySetInfo
    {
        public string ID;
        public string Name;
        public List<string> LegendaryIDs = new List<string>();
        public List<SetBonusInfo> SetBonuses = new List<SetBonusInfo>();

        public LegendarySetInfo(LegendaryType type, string ID, string name)
        {
            this.ID = ID;
            Name = name;

            switch (type)
            {
                case LegendaryType.Legend:
                    LegendaryConfig.LegendarySets.Add(this);
                    break;
                case LegendaryType.Mythic:
                    LegendaryConfig.MythicSets.Add(this);
                    break;
            }
        }
    }

    [Serializable]
    private class LegendaryItemConfig
    {
        public List<LegendaryInfo> LegendaryItems = new List<LegendaryInfo>();
        public List<LegendarySetInfo> LegendarySets = new List<LegendarySetInfo>();
        public List<LegendaryInfo> MythicItems = new List<LegendaryInfo>();
        public List<LegendarySetInfo> MythicSets = new List<LegendarySetInfo>();

        public bool HasValues()
        {
            if (LegendaryItems.Count > 0) return true;
            if (LegendarySets.Count > 0) return true;
            if (MythicItems.Count > 0) return true;
            if (MythicSets.Count > 0) return true;

            return false;
        }
    }
    #endregion
    
    #region Recipe
    [Serializable][PublicAPI]
    public class RecipeRequirement
    {
        public string item;
        public int amount;

        public RecipeRequirement(string item, int amount = 1)
        {
            this.item = item;
            this.amount = amount;
        }
    }
    
    [PublicAPI]
    public static void Add(this List<RecipeRequirement> list, string item, int amount = 1) =>
        list.Add(new(item, amount));

    [Serializable][PublicAPI]
    public class Recipe
    {
        public string name;
        public string item;
        public int amount;
        public string craftingStation;
        public int minStationLevel = 1;
        public bool enabled = true;
        public string repairStation = "";
        public List<RecipeRequirement> resources = new List<RecipeRequirement>();

        public Recipe(string name, string item, CraftingTable craftingTable, int amount = 1)
        {
            this.name = name;
            this.item = item;
            this.amount = amount;
            craftingStation = craftingTable.GetInternalName();
            Recipes.recipes.Add(this);
        }
    }
    
    [PublicAPI]
    public enum CraftingTable
    {
        [InternalName("piece_workbench")] Workbench,
        [InternalName("piece_cauldron")] Cauldron,
        [InternalName("forge")] Forge,
        [InternalName("piece_artisanstation")] ArtisanTable,
        [InternalName("piece_stonecutter")] StoneCutter,
        [InternalName("piece_magetable")] MageTable,
        [InternalName("blackforge")] BlackForge,
        [InternalName("piece_preptable")] FoodPreparationTable,
        [InternalName("piece_MeadCauldron")] MeadKetill,
    }
    
    private class InternalName : Attribute
    {
        public readonly string internalName;
        public InternalName(string internalName) => this.internalName = internalName;
    }
    
    private static string GetInternalName(this CraftingTable table)
    {
        Type type = typeof(CraftingTable);
        MemberInfo[] memInfo = type.GetMember(table.ToString());
        if (memInfo.Length <= 0) return table.ToString();
        var attr = (InternalName)Attribute.GetCustomAttribute(memInfo[0], typeof(InternalName));
        return attr != null ? attr.internalName : table.ToString();
    }
    
    [Serializable]
    private class RecipesConfig
    {
        public List<Recipe> recipes = new List<Recipe>();
        public bool HasValues() => recipes.Count > 0;
    }
    
    #endregion
    
    #region Ability
    [Serializable][PublicAPI]
    public enum AbilityActivationMode
    {
        Passive,
        Triggerable,
        Activated
    }

    [Serializable][PublicAPI]
    public enum AbilityAction
    {
        Custom,
        StatusEffect
    }

    [Serializable][PublicAPI]
    public class AbilityDefinition
    {
        public string ID;
        public string IconAsset; // Will need to tweak EpicLoot class to allow for custom icons to be passed
        public AbilityActivationMode ActivationMode; // Only Activate works, since Triggerable is unique per Ability
        public float Cooldown;
        public AbilityAction Action; // Always Status Effect since custom is too complex behavior to pass through
        public List<string> ActionParams = new List<string>();

        [Description("Register a status effect ability which activates on player input")]
        public AbilityDefinition(string ID, string iconAsset, float cooldown, string statusEffectName)
        {
            this.ID = ID;
            IconAsset = iconAsset;
            ActivationMode = AbilityActivationMode.Activated;
            Cooldown = cooldown;
            Action = AbilityAction.StatusEffect;
            ActionParams.Add(statusEffectName);
            
            Abilities.Add(this);
        }
    }
    
    #endregion
    
    #region Material Conversion
    [Serializable][PublicAPI]
    public enum MaterialConversionType
    {
        Upgrade,
        Convert,
        Junk
    }

    [Serializable][PublicAPI]
    public class MaterialConversionRequirement
    {
        public string Item;
        public int Amount;

        public MaterialConversionRequirement(string item, int amount = 1)
        {
            Item = item;
            Amount = amount;
        }
    }
    
    [PublicAPI]
    public static void Add(this List<MaterialConversionRequirement> list, string item, int amount = 1) =>
        list.Add(new MaterialConversionRequirement(item, amount));

    [Serializable][PublicAPI]
    public class MaterialConversion
    {
        public string Name;
        public string Product;
        public int Amount;
        public MaterialConversionType Type;
        public List<MaterialConversionRequirement> Resources = new();

        [Description("Creates a new material conversion definition.")]
        public MaterialConversion(MaterialConversionType type, string name, string product, int amount = 1)
        {
            Name = name;
            Product = product;
            Amount = amount;
            Type = type;

            MaterialConversions.Add(this);
        }
    }
    #endregion
    
    #region Sacrifice
    [Serializable][PublicAPI]
    public class ItemAmountConfig
    {
        public string Item;
        public int Amount;

        public ItemAmountConfig(string item, int amount = 1)
        {
            Item = item;
            Amount = amount;
        }
    }

    [PublicAPI]
    public static void Add(this List<ItemAmountConfig> list, string item, int amount = 1) =>
        list.Add(new(item, amount));
    

    [Serializable][PublicAPI]
    public class Sacrifice
    {
        [Description("Conditional, checks item needs to be magic")]
        public bool IsMagic;
        [Description("Can be null")]
        public ItemRarity Rarity;
        [Description("Conditional, if empty, does not check if item is of correct type")]
        public List<string> ItemTypes = new List<string>();
        [Description("Conditional, if empty, does not check if item shared name is in list")]
        public List<string> ItemNames = new List<string>();
        public List<ItemAmountConfig> Products = new List<ItemAmountConfig>();

        [Description("Create a new intance of a disenchant product entry")]
        public Sacrifice()
        {
            Sacrifices.Add(this);
        }

        public void AddRequiredItemType(params ItemDrop.ItemData.ItemType[] types)
        {
            foreach (ItemDrop.ItemData.ItemType type in types) ItemTypes.Add(type.ToString());
        }
    }
    #endregion

    #region Helpers
    [PublicAPI]
    public static void Add<T>(this List<T> list, params T[] items) => list.AddRange(items);
    
    #endregion
    
    #region Reflection Methods

    private static readonly Method API_AddMagicEffect = new("EpicLoot.API, EpicLoot", "AddMagicEffect");
    private static readonly Method API_GetTotalActiveMagicEffectValue = new ("EpicLoot.API, EpicLoot", "GetTotalActiveMagicEffectValue");
    private static readonly Method API_GetTotalActiveMagicEffectValueForWeapon = new("EpicLoot.API, EpicLoot", "GetTotalActiveMagicEffectValueForWeapon");
    private static readonly Method API_HasActiveMagicEffect = new ("EpicLoot.API, EpicLoot", "HasActiveMagicEffect");
    private static readonly Method API_HasActiveMagicEffectOnWeapon = new("EpicLoot.API, EpicLoot", "HasActiveMagicEffectOnWeapon");
    private static readonly Method API_GetTotalActiveSetEffectValue = new("EpicLoot.API, EpicLoot", "GetTotalActiveSetEffectValue");
    private static readonly Method API_GetMagicEffectDefinitionCopy = new("EpicLoot.API, EpicLoot", "GetMagicItemEffectDefinition");
    private static readonly Method API_GetAllActiveMagicEffects = new("EpicLoot.API, EpicLoot", "GetAllActiveMagicEffects");
    private static readonly Method API_GetAllSetMagicEffects = new("EpicLoot.API, EpicLoot", "GetAllActiveSetMagicEffects");
    private static readonly Method API_GetPlayerTotalActiveMagicEffectValue = new("EpicLoot.API, EpicLoot", "GetTotalPlayerActiveMagicEffectValue");
    private static readonly Method API_PlayerHasActiveMagicEffect = new("EpicLoot.API, EpicLoot", "PlayerHasActiveMagicEffect");
    private static readonly Method API_AddLegendaryItemConfig = new("EpicLoot.API, EpicLoot", "AddLegendaryItemConfig");
    private static readonly Method API_AddAbility = new("EpicLoot.API, EpicLoot", "AddAbility");
    private static readonly Method API_HasLegendaryItem = new("EpicLoot.API, EpicLoot", "HasLegendaryItem");
    private static readonly Method API_HasLegendarySet = new("EpicLoot.API, EpicLoot", "HasLegendarySet");
    private static readonly Method API_RegisterAsset = new("EpicLoot.API, EpicLoot", "RegisterAsset");
    private static readonly Method API_AddMaterialConversion = new("EpicLoot.API, EpicLoot", "AddMaterialConversion");
    private static readonly Method API_AddRecipes = new("EpicLoot.API, EpicLoot", "AddRecipes");
    private static readonly Method API_AddSacrifices = new("EpicLoot.API, EpicLoot", "AddSacrifices");
    #endregion

    /// <summary>
    /// Helper function to register everything
    /// </summary>
    [PublicAPI][Description("Send all your custom conversions, effects, item definitions, etc... to Epic Loot")]
    public static void RegisterAll()
    {
        RegisterMaterialConversions();
        RegisterMagicItems();
        RegisterLegendaryItems();
        RegisterAbilities();
        RegisterRecipes();
        RegisterSacrifices();
    }

    /// <summary>
    /// Register enchanting costs
    /// </summary>
    /// <returns></returns>
    [PublicAPI]
    public static bool RegisterSacrifices()
    {
        if (Sacrifices.Count <= 0) return false;
        string json = JsonConvert.SerializeObject(Sacrifices);
        object? result = API_AddSacrifices.Invoke(json);
        return (bool)(result ?? false);
    }
    
    /// <summary>
    /// Register all recipes
    /// </summary>
    /// <returns></returns>
    [PublicAPI]
    public static bool RegisterRecipes()
    {
        if (!Recipes.HasValues()) return false;
        string json = JsonConvert.SerializeObject(Recipes.recipes);
        object? result = API_AddRecipes.Invoke(json);
        return (bool)(result ?? false);
    }

    /// <summary>
    /// Helper function to register all defined material conversions
    /// </summary>
    [PublicAPI]
    public static void RegisterMaterialConversions()
    {
        foreach (var conversion in new List<MaterialConversion>(MaterialConversions)) AddMaterialConversion(conversion);
    }
    
    /// <summary>
    /// Register material conversion to EpicLoot MaterialConversions.Conversions
    /// </summary>
    /// <param name="materialConversion"></param>
    /// <returns>True if added to MaterialConversions.Conversions</returns>
    [Description("serializes to json and sends to EpicLoot")][PublicAPI]
    public static bool AddMaterialConversion(MaterialConversion materialConversion)
    {
        MaterialConversions.Remove(materialConversion);
        string data = JsonConvert.SerializeObject(materialConversion);
        object? result = API_AddMaterialConversion.Invoke(data);
        return (bool)(result ?? false);
    }
    
    /// <summary>
    /// Register asset into EpicLoot in order to target them in your definitions
    /// </summary>
    /// <param name="name"></param>
    /// <param name="asset"></param>
    /// <returns></returns>
    [PublicAPI]
    public static bool RegisterAsset(string name, Object asset)
    {
        object? result = API_RegisterAsset.Invoke(name, asset);
        return (bool)(result ?? false);
    }

    /// <summary>
    /// Get a simple copy of existing magic effect
    /// </summary>
    /// <param name="effectType"></param>
    /// <returns></returns>
    [PublicAPI]
    public static MagicItemEffectDefinition? GetMagicEffectDefinitionCopy(string effectType)
    {
        string result = (string)(API_GetMagicEffectDefinitionCopy.Invoke(effectType) ?? "");
        if (string.IsNullOrEmpty(result)) return null;
        try
        {
            MagicItemEffectDefinition? copy = JsonConvert.DeserializeObject<MagicItemEffectDefinition>(result);
            return copy;
        }
        catch
        {
            Debug.LogWarning("[EpicLoot API] Failed to parse magic item effect definition json");
            return null;
        }
    }
    
    /// <summary>
    /// Helper function, register all magic item that have been defined
    /// </summary>
    [PublicAPI]
    public static void RegisterMagicItems()
    {
        foreach (var item in new List<MagicItemEffectDefinition>(MagicItemEffectDefinitions)) AddMagicEffect(item);
    }

    /// <summary>
    /// Register magic effect to Epic Loot
    /// </summary>
    /// <param name="definition"></param>
    /// <returns></returns>
    [PublicAPI]
    public static bool AddMagicEffect(MagicItemEffectDefinition definition)
    {
        MagicItemEffectDefinitions.Remove(definition);
        string data = JsonConvert.SerializeObject(definition);
        object? result = API_AddMagicEffect.Invoke(data);

        return (bool)(result ?? false);
    }

    /// <summary>
    /// ⚠️ Conditional behavior: Returns different results based on player parameter,
    /// Player null → Checks if item has effect,
    /// Player provided → Checks if player has effect 
    /// </summary>
    /// <param name="player"></param>
    /// <param name="item"></param>
    /// <param name="effectType"></param>
    /// <param name="effectValue"></param>
    /// <returns></returns>
    [PublicAPI]
    public static bool HasActiveMagicEffectOnWeapon(Player player, ItemDrop.ItemData item, string effectType, out float effectValue)
    {
        effectValue = 0f;
        object? output = API_HasActiveMagicEffectOnWeapon.Invoke(player, item, effectType, effectValue);
        
        return (bool)(output ?? false);
    }

    /// <summary>
    /// ⚠️ Conditional behavior: Returns different results based on player parameter,
    /// Player null → returns item effect values,
    /// Player provided → returns player effect values
    /// </summary>
    /// <param name="player"></param>
    /// <param name="item"></param>
    /// <param name="effectType"></param>
    /// <param name="scale"></param>
    /// <returns></returns>
    [PublicAPI]
    public static float GetTotalActiveMagicEffectValue(Player player, ItemDrop.ItemData item, string effectType,
        float scale = 1.0f)
    {
        object? result = API_GetTotalActiveMagicEffectValue.Invoke(player, item, effectType);
        return (float)(result ?? 0f);
    }

    /// <summary>
    /// ⚠️ Conditional behavior: Returns different results based on player parameter,
    /// Player null → returns item effect values,
    /// Player provided → returns effect value without item
    /// </summary>
    /// <param name="player"></param>
    /// <param name="item"></param>
    /// <param name="effectType"></param>
    /// <param name="scale"></param>
    /// <returns></returns>
    [PublicAPI]
    public static float GetTotalActiveMagicEffectValueForWeapon(Player player, ItemDrop.ItemData item,
        string effectType, float scale = 1.0f)
    {
        object? result = API_GetTotalActiveMagicEffectValueForWeapon.Invoke(player, item, effectType, scale);
        return (float)(result ?? 0f);
    }

    /// <summary>
    /// Returns true if player has effect
    /// </summary>
    /// <param name="player"></param>
    /// <param name="effectType"></param>
    /// <param name="ignoreThisItem"></param>
    /// <param name="scale"></param>
    /// <returns></returns>
    [PublicAPI]
    public static bool HasActiveMagicEffect(Player player, string effectType, ItemDrop.ItemData? ignoreThisItem = null, float scale = 1.0f)
    {
        object? result = API_HasActiveMagicEffect.Invoke(player, ignoreThisItem, effectType, scale);
        return (bool)(result ?? false);
    }

    /// <summary>
    /// ⚠️ Currently hard-coded to ModifyArmor, 
    /// returns total effect on set
    /// </summary>
    /// <param name="player"></param>
    /// <param name="effectType"></param>
    /// <param name="scale"></param>
    /// <returns></returns>
    [PublicAPI]
    public static float GetTotalActiveSetEffectValue(Player player, string effectType, float scale = 1.0f)
    {
        object? result = API_GetTotalActiveSetEffectValue.Invoke(player, effectType, scale);
        return (float)(result ?? 0f);
    }

    /// <summary>
    /// Returns a list of effects on player
    /// </summary>
    /// <param name="player"></param>
    /// <param name="effectType"></param>
    /// <returns></returns>
    [PublicAPI]
    public static List<MagicItemEffect> GetAllActiveMagicEffects(this Player player, string? effectType = null)
    {
        object? result = API_GetAllActiveMagicEffects.Invoke(player, effectType);
        List<string> list = (List<string>)(result ?? new List<string>());
        if (list.Count <= 0) return new List<MagicItemEffect>();
        List<MagicItemEffect> output = new List<MagicItemEffect>();
        foreach (var item in list)
        {
            try
            {
                MagicItemEffect? magicItemEffect = JsonConvert.DeserializeObject<MagicItemEffect>(item);
                if (magicItemEffect == null) continue;
                output.Add(magicItemEffect);
            }
            catch
            {
                Debug.LogWarning("[EpicLoot API] Failed to parse magic item effect");
            }
        }

        return output;
    }

    /// <summary>
    /// Returns list of effects on active set
    /// </summary>
    /// <param name="player"></param>
    /// <param name="effectType"></param>
    /// <returns></returns>
    [PublicAPI]
    public static List<MagicItemEffect> GetAllActiveSetMagicEffects(this Player player, string? effectType = null)
    {
        object? result = API_GetAllSetMagicEffects.Invoke(player, effectType);
        List<string> list = (List<string>)(result ?? new List<string>());
        if (list.Count <= 0) return new List<MagicItemEffect>();
        List<MagicItemEffect> output = new List<MagicItemEffect>();
        foreach (var item in list)
        {
            try
            {
                MagicItemEffect? magicItemEffect = JsonConvert.DeserializeObject<MagicItemEffect>(item);
                if (magicItemEffect == null) continue;
                output.Add(magicItemEffect);
            }
            catch
            {
                Debug.LogWarning("[EpicLoot API] Failed to parse magic item effect");
            }
        }

        return output;
    }

    /// <summary>
    /// Returns total effect value on player
    /// </summary>
    /// <param name="player"></param>
    /// <param name="effectType"></param>
    /// <param name="scale"></param>
    /// <param name="ignoreThisItem"></param>
    /// <returns></returns>
    [PublicAPI]
    public static float GetTotalActiveMagicEffectValue(this Player player, string effectType, float scale = 1.0f,
        ItemDrop.ItemData? ignoreThisItem = null)
    {
        object? result = API_GetPlayerTotalActiveMagicEffectValue.Invoke(player, effectType, scale, ignoreThisItem);
        return (float)(result ?? 0f);
    }

    /// <summary>
    /// Returns true if player has effect
    /// </summary>
    /// <param name="player"></param>
    /// <param name="effectType"></param>
    /// <param name="effectValue"></param>
    /// <param name="scale"></param>
    /// <param name="ignoreThisItem"></param>
    /// <returns></returns>
    [PublicAPI]
    public static bool HasActiveMagicEffect(this Player player, string effectType, out float effectValue,
        float scale = 1.0f, ItemDrop.ItemData? ignoreThisItem = null)
    {
        effectValue = 0f;
        object? result = API_PlayerHasActiveMagicEffect.Invoke(player, effectType, effectValue, scale, ignoreThisItem);
        return (bool)(result ?? false);
    }

    /// <summary>
    /// Registers all legendary items and set definitions
    /// </summary>
    /// <returns></returns>
    [PublicAPI]
    public static bool RegisterLegendaryItems()
    {
        if (!LegendaryConfig.HasValues()) return false;
        string data = JsonConvert.SerializeObject(LegendaryConfig);
        object? result = API_AddLegendaryItemConfig.Invoke(data);
        return (bool)(result ?? false);
    }

    /// <summary>
    /// Helper function, register all defined abilities
    /// </summary>
    [PublicAPI]
    public static void RegisterAbilities()
    {
        foreach (var ability in new List<AbilityDefinition>(Abilities)) AddAbility(ability);
    }

    /// <summary>
    /// Register ability definition
    /// </summary>
    /// <param name="ability"></param>
    /// <returns></returns>
    [PublicAPI]
    public static bool AddAbility(AbilityDefinition ability)
    {
        Abilities.Remove(ability);
        string data = JsonConvert.SerializeObject(ability);
        object? result = API_AddAbility.Invoke(data);
        return (bool)(result ?? false);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="player"></param>
    /// <param name="legendaryItemID"></param>
    /// <returns>true if player has legendary item</returns>
    [PublicAPI]
    public static bool HasLegendaryItem(this Player player, string legendaryItemID)
    {
        object? result = API_HasLegendaryItem.Invoke(player, legendaryItemID);
        return (bool)(result ?? false);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="player"></param>
    /// <param name="legendarySetID"></param>
    /// <param name="count"></param>
    /// <returns>true if player has full legendary set</returns>
    [PublicAPI]
    public static bool HasLegendarySet(this Player player, string legendarySetID, out int count)
    {
        count = 0;
        var result = API_HasLegendarySet.Invoke(player, legendarySetID, count);
        return (bool)(result ?? false);
    }

    /// <summary>
    /// Helper class for dynamically invoking static methods from external assemblies using reflection.
    /// Provides caching of Type instances for improved performance and simplified method invocation.
    /// Useful for calling methods in plugins or external libraries without direct references.
    /// </summary>
    private class Method
    {
        /// <summary>
        /// Cache of previously resolved Type instances to avoid repeated Type.GetType() calls.
        /// Key: Full type name with assembly (e.g., "MyNamespace.MyClass, MyAssembly")
        /// Value: Resolved Type instance
        /// </summary>
        private static readonly Dictionary<string, Type> CachedTypes = new();
        private readonly MethodInfo? info;

        /// <summary>
        /// Invokes the cached static method with the provided arguments.
        /// This method performs no parameter validation - ensure arguments match the target method signature.
        /// </summary>
        /// <param name="args">
        /// Arguments to pass to the target method. The array length, types, and order must exactly match 
        /// the target method's parameter signature. Passing incorrect arguments will result in runtime exceptions.
        /// </param>
        /// <returns>
        /// The return value from the invoked method, or null if:
        /// - The method returns void
        /// - The method could not be resolved during construction
        /// - The method invocation fails
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when argument types don't match the method signature</exception>
        /// <exception cref="TargetParameterCountException">Thrown when argument count doesn't match the method signature</exception>
        /// <exception cref="TargetInvocationException">Thrown when the invoked method throws an exception</exception>
        public object? Invoke(params object?[] args) => info?.Invoke(null, args);
        
        /// <summary>
        /// Constructs a Method helper that resolves and caches a static method for later invocation.
        /// Uses reflection to locate the specified method in the target type and assembly.
        /// </summary>
        /// <param name="typeNameWithAssembly">
        /// The fully qualified type name including assembly information.
        /// 
        /// Format: "Namespace.ClassName, AssemblyName"
        /// 
        /// Examples:
        /// - "EpicLoot.API, EpicLoot" - API class in EpicLoot namespace from EpicLoot assembly
        /// - "MyMod.Utilities.Helper, MyMod" - Helper class in MyMod.Utilities namespace from MyMod assembly
        /// - "System.IO.File, mscorlib" - File class from System.IO namespace in mscorlib assembly
        /// 
        /// Note: The assembly name should match the .dll filename without extension.
        /// For assemblies in the GAC, use the full strong name if needed.
        /// </param>
        /// <param name="methodName">
        /// The exact name of the static method to locate within the specified type.
        /// Method names are case-sensitive and must match exactly.
        /// 
        /// Examples:
        /// - "AddMagicEffect"
        /// - "GetPlayerData"
        /// - "ProcessItems"
        /// 
        /// Note: This implementation only searches for public static methods. Private, instance, 
        /// or non-static methods will not be found.
        /// </param>
        /// <remarks>
        /// Construction Process:
        /// 1. Checks the type cache for previously resolved types
        /// 2. If not cached, uses Type.GetType() to resolve the type from the assembly
        /// 3. Caches the resolved type for future use
        /// 4. Uses reflection to find the specified static method
        /// 5. Logs warnings if type or method resolution fails
        /// 
        /// Performance Notes:
        /// - Type resolution is expensive, but results are cached
        /// - Method resolution is performed once during construction
        /// - Subsequent Invoke() calls have minimal reflection overhead
        /// 
        /// Common Failure Scenarios:
        /// - Assembly not loaded or accessible
        /// - Type name typos or incorrect namespace
        /// - Method name typos or case mismatch
        /// - Method is private, instance, or non-static
        /// - Method is overloaded (this class finds the first matching name)
        /// </remarks>
        public Method(string typeNameWithAssembly, string methodName)
        {
            // Try to get cached type first for performance
            if (!CachedTypes.TryGetValue(typeNameWithAssembly, out Type? type))
            {
                // Attempt to resolve the type from the assembly
                if (Type.GetType(typeNameWithAssembly) is not { } resolvedType)
                {
                    Debug.LogWarning($"[EpicLoot API] Failed to resolve type: '{typeNameWithAssembly}'. " +
                                   "Verify the namespace, class name, and assembly name are correct. " +
                                   "Ensure the assembly is loaded and accessible.");
                    return;
                }

                type = resolvedType;
                CachedTypes[typeNameWithAssembly] = resolvedType;
            }

            // Additional null check (defensive programming, should not happen if TryGetValue succeeded)
            if (type == null)
            {
                Debug.LogWarning($"[EpicLoot API] Type resolution returned null for: '{typeNameWithAssembly}'");
                return;
            }

            // Locate the static method by name
            info = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);
            if (info == null)
            {
                Debug.LogWarning(
                    $"[EpicLoot API] Failed to find public static method '{methodName}' in type '{type.FullName}'. " +
                    "Verify the method name is correct, the method exists, and it is marked as public static. ");
            }
        }
        /// <summary>
        /// Gets the parameter information for the resolved method.
        /// Useful for validating arguments before calling Invoke().
        /// </summary>
        /// <returns>Array of ParameterInfo objects describing the method parameters, or empty array if method not resolved.</returns>
        public ParameterInfo[] GetParameters() => info?.GetParameters() ?? Array.Empty<ParameterInfo>();
        
        public static void ClearCache() => CachedTypes.Clear();
    }
}