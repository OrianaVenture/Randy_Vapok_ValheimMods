#nullable enable
using System;
using EpicLoot.MagicItemEffects;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Common;
using EpicLoot.Abilities;
using EpicLoot.Adventure;
using EpicLoot.Crafting;
using EpicLoot.CraftingV2;
using EpicLoot.LegendarySystem;
using JetBrains.Annotations;
using Object = UnityEngine.Object;

namespace EpicLoot;

public static class API
{
    private static bool ShowLogs = false;
    private static event Action<string>? OnReload;
    private static event Action<string>? OnError;
    private static event Action<string>? OnDebug;
    
    private static readonly Dictionary<string, MagicItemEffectDefinition> ExternalMagicItemEffectDefinitions = new();
    private static readonly Dictionary<string, AbilityDefinition> ExternalAbilities = new();
    private static readonly Dictionary<ItemRarity, List<LegendaryInfo>> ExternalLegendaryItems = new();
    private static readonly Dictionary<ItemRarity, List<LegendarySetInfo>> ExternalLegendarySets = new();
    private static readonly Dictionary<string, Object> ExternalAssets = new();
    private static readonly List<MaterialConversion> ExternalMaterialConversions = new();
    private static readonly List<RecipeConfig> ExternalRecipes = new();
    private static readonly List<DisenchantProductsConfig> ExternalSacrifices = new();
    private static readonly List<BountyTargetConfig> ExternalBountyTargets = new();
    private static readonly Dictionary<SecretStashType, List<SecretStashItemConfig>> ExternalSecretStashItems = new();
    private static readonly List<TreasureMapBiomeInfoConfig> ExternalTreasureMaps = new();
    private static readonly Dictionary<string, Dictionary<string, Delegate>> AbilityProxies = new();

    /// <summary>
    /// Static constructor, runs automatically once before the API class is first used.
    /// </summary>
    static API()
    {
        MagicItemEffectDefinitions.OnSetupMagicItemEffectDefinitions += ReloadExternalMagicEffects;
        UniqueLegendaryHelper.OnSetupLegendaryItemConfig += ReloadExternalLegendary;
        AbilityDefinitions.OnSetupAbilityDefinitions += ReloadExternalAbilities;
        MaterialConversions.OnSetupMaterialConversions += ReloadExternalMaterialConversions;
        RecipesHelper.OnSetupRecipeConfig += ReloadExternalRecipes;
        EnchantCostsHelper.OnSetupEnchantingCosts += ReloadExternalSacrifices;
        AdventureDataManager.OnSetupAdventureData += ReloadExternalAdventureData;

        OnReload += message =>
        {
            if (!ShowLogs) return;
            EpicLoot.Log(message);
        };
        OnError += message =>
        {
            if (!ShowLogs) return;
            EpicLoot.LogWarning(message);
        };
        OnDebug += message =>
        {
            if (!ShowLogs) return;
            EpicLoot.LogWarningForce(message);
        };
    }

    #region Reload
    /// <summary>
    /// Reloads cached external adventure data into <see cref="AdventureDataManager.Config"/>
    /// </summary>
    private static void ReloadExternalAdventureData()
    {
        ReloadExternalBounties();
        ReloadExternalSecretStashItems();
        ReloadExternalTreasures();
    }

    /// <summary>
    /// Reloads cached secret stash items into <see cref="AdventureDataManager.Config"/>
    /// </summary>
    private static void ReloadExternalSecretStashItems()
    {
        foreach (KeyValuePair<SecretStashType, List<SecretStashItemConfig>> kvp in ExternalSecretStashItems)
        {
            switch (kvp.Key)
            {
                case SecretStashType.Materials:
                    AdventureDataManager.Config.SecretStash.Materials.AddRange(kvp.Value);
                    break;
                case SecretStashType.OtherItems:
                    AdventureDataManager.Config.SecretStash.OtherItems.AddRange(kvp.Value);
                    break;
                case SecretStashType.RandomItems:
                    AdventureDataManager.Config.SecretStash.RandomItems.AddRange(kvp.Value);
                    break;
                case SecretStashType.Gamble:
                    AdventureDataManager.Config.Gamble.GambleCosts.AddRange(kvp.Value);
                    break;
                case SecretStashType.Sale:
                    AdventureDataManager.Config.TreasureMap.SaleItems.AddRange(kvp.Value);
                    break;
            }
        }
    }
    /// <summary>
    /// Reloads cached external treasure maps into <see cref="AdventureDataManager.Config"/>
    /// </summary>
    private static void ReloadExternalTreasures()
    {
        foreach (TreasureMapBiomeInfoConfig? treasure in ExternalTreasureMaps)
        {
            AdventureDataManager.Config.TreasureMap.BiomeInfo.Add(treasure);
        }
        OnReload?.Invoke("Reloaded external treasures");
    }

    /// <summary>
    /// Reloads cached external bounties into <see cref="AdventureDataManager.Config"/>
    /// </summary>
    private static void ReloadExternalBounties()
    {
        AdventureDataManager.Config.Bounties.Targets.AddRange(ExternalBountyTargets);
        OnReload?.Invoke("Reloaded external bounties");
    }

    /// <summary>
    /// Reloads cached external enchanting costs into <see cref="EnchantCostsHelper.Config"/>
    /// </summary>
    private static void ReloadExternalSacrifices()
    {
        EnchantCostsHelper.Config.DisenchantProducts.AddRange(ExternalSacrifices);
        OnReload?.Invoke("Reloaded external sacrifices");
    }

    /// <summary>
    /// Reloads cached external recipes into <see cref="RecipesHelper.Config"/>
    /// </summary>
    private static void ReloadExternalRecipes()
    {
        RecipesHelper.Config.recipes.RemoveAll(ExternalRecipes);
        RecipesHelper.Config.recipes.AddRange(ExternalRecipes);
        OnReload?.Invoke("Reloaded external recipes");
    }

    /// <summary>
    /// Reloads cached external material conversions into <see cref="MaterialConversions.Conversions"/>
    /// </summary>
    private static void ReloadExternalMaterialConversions()
    {
        foreach (MaterialConversion entry in ExternalMaterialConversions)
        {
            MaterialConversions.Config.MaterialConversions.Add(entry);
        }
        OnReload?.Invoke("Reloaded external material conversions");
    }
    
    /// <summary>
    /// Reloads cached external magic effects into <see cref="MagicItemEffectDefinitions.AllDefinitions"/>
    /// </summary>
    private static void ReloadExternalMagicEffects()
    {
        foreach (MagicItemEffectDefinition effect in ExternalMagicItemEffectDefinitions.Values)
        {
            MagicItemEffectDefinitions.Add(effect);
        }
        OnReload?.Invoke("Reloaded external magic effects");
    }

    /// <summary>
    /// Reloads cached external abilities into <see cref="AbilityDefinitions.Config"/> and <see cref="AbilityDefinitions.Abilities"/>
    /// </summary>
    private static void ReloadExternalAbilities()
    {
        foreach (KeyValuePair<string, AbilityDefinition> kvp in ExternalAbilities)
        {
            AbilityDefinitions.Config.Abilities.Add(kvp.Value);
        }
        OnReload?.Invoke("Reloaded external abilities");
    }

    /// <summary>
    /// Reloads cached legendary items and sets into <see cref="UniqueLegendaryHelper"/>
    /// </summary>
    private static void ReloadExternalLegendary()
    {
        foreach (KeyValuePair<ItemRarity, List<LegendaryInfo>> kvp in ExternalLegendaryItems)
        {
            switch (kvp.Key)
            {
                case ItemRarity.Legendary:
                    UniqueLegendaryHelper.Config.LegendaryItems.AddRange(kvp.Value);
                    break;
                case ItemRarity.Mythic:
                    UniqueLegendaryHelper.Config.MythicItems.AddRange(kvp.Value);
                    break;
            }
        }

        foreach (KeyValuePair<ItemRarity, List<LegendarySetInfo>> kvp in ExternalLegendarySets)
        {
            switch (kvp.Key)
            {
                case ItemRarity.Legendary:
                    UniqueLegendaryHelper.Config.LegendarySets.AddRange(kvp.Value);
                    break;
                case ItemRarity.Mythic:
                    UniqueLegendaryHelper.Config.MythicSets.AddRange(kvp.Value);
                    break;
            }
        }
        OnReload?.Invoke("Reloaded external legendary abilities");
    }

    /// <summary>
    /// Reloads cached assets into <see cref="EpicLoot._assetCache"/>
    /// </summary>
    public static void ReloadExternalAssets()
    {
        foreach (KeyValuePair<string, Object> kvp in ExternalAssets)
        {
            EpicLoot._assetCache[kvp.Key] = kvp.Value;
        }
        OnReload?.Invoke("Reloaded external assets");
    }
    #endregion
    #region Treasure
    /// <param name="json">JSON serialized <see cref="TreasureMapBiomeInfoConfig"/></param>
    /// <returns>unique identifier</returns>
    [PublicAPI]
    public static string? AddTreasureMap(string json)
    {
        try
        {
            var map = JsonConvert.DeserializeObject<TreasureMapBiomeInfoConfig>(json);
            if (map == null) return null;
            ExternalTreasureMaps.Add(map);
            AdventureDataManager.Config.TreasureMap.BiomeInfo.Add(map);
            return RuntimeRegistry.Register(map);
        }
        catch
        {
            OnError?.Invoke("Failed to parse treasure map from external plugin");
            return null;
        }
    }

    /// <param name="key">unique identifier</param>
    /// <param name="json">JSON serialized <see cref="TreasureMapBiomeInfoConfig"/></param>
    /// <returns></returns>
    [PublicAPI]
    public static bool UpdateTreasureMap(string key, string json)
    {
        if (!RuntimeRegistry.TryGetValue(key, out TreasureMapBiomeInfoConfig original)) return false;
        try
        {
            var map = JsonConvert.DeserializeObject<TreasureMapBiomeInfoConfig>(json);
            original.CopyFieldsFrom(map);
            return true;
        }
        catch
        {
            OnError?.Invoke("Failed to parse treasure map from external plugin");
            return false;
        }
    }
    #endregion
    #region Adventure
    private enum SecretStashType
    {
        Materials, 
        RandomItems, 
        OtherItems,
        Gamble,
        Sale
    }
    
    /// <param name="type"><see cref="SecretStashType"/></param>
    /// <param name="json">JSON serialized <see cref="SecretStashItemConfig"/></param>
    /// <returns>unique identifier if added</returns>
    [PublicAPI]
    public static string? AddSecretStashItem(string type, string json)
    {
        try
        {
            if (!Enum.TryParse(type, true, out SecretStashType stashType)) return null;
            SecretStashItemConfig? secretStash = JsonConvert.DeserializeObject<SecretStashItemConfig>(json);
            if (secretStash == null) return null;
            ExternalSecretStashItems.AddOrSet(stashType, secretStash);
            switch (stashType)
            {
                case SecretStashType.Materials:
                    AdventureDataManager.Config.SecretStash.Materials.Add(secretStash);
                    break;
                case SecretStashType.OtherItems:
                    AdventureDataManager.Config.SecretStash.OtherItems.Add(secretStash);
                    break;
                case SecretStashType.RandomItems:
                    AdventureDataManager.Config.SecretStash.RandomItems.Add(secretStash);
                    break;
                case SecretStashType.Gamble:
                    AdventureDataManager.Config.Gamble.GambleCosts.Add(secretStash);
                    break;
                case SecretStashType.Sale:
                    AdventureDataManager.Config.TreasureMap.SaleItems.Add(secretStash);
                    break;
            }
            return RuntimeRegistry.Register(secretStash);
        }
        catch
        {
            OnError?.Invoke("Failed to parse secret stash from external plugin");
            return null;
        }
    }
    
    /// <param name="key">unique identifier</param>
    /// <param name="json">JSON serialized <see cref="SecretStashItemConfig"/></param>
    /// <returns>True if fields copied</returns>
    
    [PublicAPI]
    public static bool UpdateSecretStashItem(string key, string json)
    {
        if (!RuntimeRegistry.TryGetValue(key, out SecretStashItemConfig original)) return false;
        SecretStashItemConfig? secretStash = JsonConvert.DeserializeObject<SecretStashItemConfig>(json);
        if (secretStash == null) return false;
        original.CopyFieldsFrom(secretStash);
        return true;
    }
    #endregion
    #region Magic Effect
    /// <param name="json">JSON serialized <see cref="MagicItemEffectDefinition"/></param>
    /// <returns>unique identifier if registered</returns>
    [PublicAPI]
    public static string? AddMagicEffect(string json)
    {
        try
        {
            MagicItemEffectDefinition? def = JsonConvert.DeserializeObject<MagicItemEffectDefinition>(json);
            if (def == null) return null;
            MagicItemEffectDefinitions.Add(def);
            ExternalMagicItemEffectDefinitions[def.Type] = def;
            return RuntimeRegistry.Register(def);
        }
        catch
        {
            OnError?.Invoke("Failed to parse magic effect from external plugin");
            return null;
        }
    }

    /// <param name="key">unique identifier</param>
    /// <param name="json">JSON serialized <see cref="MagicItemEffectDefinition"/></param>
    /// <returns>true if updated</returns>

    [PublicAPI]
    public static bool UpdateMagicEffect(string key, string json)
    {
        if (!RuntimeRegistry.TryGetValue(key, out MagicItemEffectDefinition original)) return false;
        MagicItemEffectDefinition? def = JsonConvert.DeserializeObject<MagicItemEffectDefinition>(json);
        if (def == null) return false;
        original.CopyFieldsFrom(def);
        return true;
    }
    #endregion
    #region States
    /// <remarks>
    /// Can be useful for external plugins to know, so they can design features around it.
    /// </remarks>
    /// <param name="player"></param>
    /// <param name="legendaryItemID"></param>
    /// <returns>true if player has item</returns>
    [PublicAPI]
    public static bool HasLegendaryItem(Player player, string legendaryItemID)
    {
        foreach (ItemDrop.ItemData item in player.GetEquipment())
        {
            if (item.IsMagic(out var magicItem) && magicItem.LegendaryID == legendaryItemID) return true;
        }
        return false;
    }

    /// <remarks>
    /// Can be useful for external plugins to know, so they can design features around it.
    /// </remarks>
    /// <param name="player"></param>
    /// <param name="legendarySetID"></param>
    /// <param name="count"></param>
    /// <returns>true if player has full set</returns>
    [PublicAPI]
    public static bool HasLegendarySet(Player player, string legendarySetID, ref int count)
    {
        if (!UniqueLegendaryHelper.TryGetLegendarySetInfo(legendarySetID, out LegendarySetInfo legendarySetInfo, out ItemRarity _)) return false;
        count = player.GetEquippedSetPieces(legendarySetID).Count;
        return count >= legendarySetInfo.LegendaryIDs.Count;
    }
    /// <param name="type"><see cref="MagicEffectType"/></param>
    /// <returns>serialized object of magic effect definition if found</returns>
    [PublicAPI]
    public static string GetMagicItemEffectDefinition(string type)
    {
        if (!MagicItemEffectDefinitions.AllDefinitions.TryGetValue(type, out MagicItemEffectDefinition definition)) return "";
        return JsonConvert.SerializeObject(definition);
    }
    
    /// <param name="player">can be null</param>
    /// <param name="item">can be null</param>
    /// <param name="effectType"><see cref="MagicEffectType"/></param>
    /// <param name="scale"></param>
    /// <returns></returns>
    [PublicAPI]
    public static float GetTotalActiveMagicEffectValue(Player? player,ItemDrop.ItemData? item, string effectType, float scale) => 
        MagicEffectsHelper.GetTotalActiveMagicEffectValue(player, item, effectType, scale);
    
    /// <param name="player">can be null</param>
    /// <param name="item">can be null</param>
    /// <param name="effectType"><see cref="MagicEffectType"/></param>
    /// <param name="scale"></param>
    /// <returns></returns>
    [PublicAPI]
    public static float GetTotalActiveMagicEffectValueForWeapon(Player? player, ItemDrop.ItemData? item, string effectType, float scale) =>
        MagicEffectsHelper.GetTotalActiveMagicEffectValueForWeapon(player, item, effectType, scale);

    /// <param name="player">can be null</param>
    /// <param name="effectType"></param>
    /// <param name="item">can be null</param>
    /// <param name="effectValue"><see cref="MagicEffectType"/></param>
    /// <returns>True if player or item has magic effect</returns>
    [PublicAPI]
    public static bool HasActiveMagicEffect(Player? player, string effectType, ItemDrop.ItemData? item,
        ref float effectValue) => MagicEffectsHelper.HasActiveMagicEffect(player, item, effectType, out effectValue);
    
    /// <param name="player">can be null</param>
    /// <param name="item"></param>
    /// <param name="effectType"></param>
    /// <param name="effectValue"><see cref="MagicEffectType"/></param>
    /// <returns>True if magic effect is on item</returns>
    [PublicAPI]
    public static bool HasActiveMagicEffectOnWeapon(Player player, ItemDrop.ItemData item, string effectType, ref float effectValue) => 
        MagicEffectsHelper.HasActiveMagicEffectOnWeapon(player, item, effectType, out effectValue);

    /// <remarks>
    /// Currently hard coded to Modify Armor effect type ???
    /// </remarks>
    /// <param name="player"></param>
    /// <param name="effectType"><see cref="MagicEffectType"/></param>
    /// <param name="scale"></param>
    /// <returns></returns>
    [PublicAPI]
    public static float GetTotalActiveSetEffectValue(Player player, string effectType, float scale) =>
        MagicEffectsHelper.GetTotalActiveSetEffectValue(player, effectType, scale);
    
    /// <param name="player"></param>
    /// <param name="effectType">filter by <see cref="MagicEffectType"/></param>
    /// <returns>list of active magic effects on player</returns>
    [PublicAPI]
    public static List<string> GetAllActiveMagicEffects(Player player, string? effectType = null)
    {
        List<MagicItemEffect> list = player.GetAllActiveMagicEffects(effectType);
        List<string> output = new List<string>();
        foreach (MagicItemEffect item in list)
        {
            output.Add(JsonConvert.SerializeObject(item));
        }

        return output;
    }
    
    /// <param name="player"></param>
    /// <param name="effectType">filter by <see cref="MagicEffectType"/></param>
    /// <returns>list of active magic effects on set</returns>
    [PublicAPI]
    public static List<string> GetAllActiveSetMagicEffects(Player player, string? effectType = null)
    {
        List<MagicItemEffect> list = player.GetAllActiveSetMagicEffects(effectType);
        List<string> output = new List<string>();
        foreach (MagicItemEffect item in list)
        {
            output.Add(JsonConvert.SerializeObject(item));
        }

        return output;
    }
    
    /// <param name="player"></param>
    /// <param name="effectType"><see cref="MagicEffectType"/></param>
    /// <param name="scale"></param>
    /// <param name="ignoreThisItem"></param>
    /// <returns>total effect value found on player</returns>
    [PublicAPI]
    public static float GetTotalPlayerActiveMagicEffectValue(Player? player, string effectType, float scale,
        ItemDrop.ItemData? ignoreThisItem = null) =>
        player.GetTotalActiveMagicEffectValue(effectType, scale, ignoreThisItem);

    /// <param name="player"></param>
    /// <param name="effectType"><see cref="MagicEffectType"/></param>
    /// <param name="effectValue"></param>
    /// <param name="scale"></param>
    /// <param name="ignoreThisItem"></param>
    /// <returns>True if player has magic effect</returns>
    [PublicAPI]
    public static bool PlayerHasActiveMagicEffect(Player? player, string effectType, ref float effectValue,
        float scale = 1.0f, ItemDrop.ItemData? ignoreThisItem = null) => player.HasActiveMagicEffect(effectType, out effectValue, scale, ignoreThisItem);
    
    #endregion
    #region Legendary
    [PublicAPI]
    public static string? AddLegendaryItem(string type, string json)
    {
        try
        {
            if (!Enum.TryParse(type, true, out ItemRarity rarity)) return null;
            LegendaryInfo? config = JsonConvert.DeserializeObject<LegendaryInfo>(json);
            if (config == null) return null;
            switch (rarity)
            {
                case ItemRarity.Legendary:
                    UniqueLegendaryHelper.Config.LegendaryItems.Add(config);
                    break;
                case ItemRarity.Mythic:
                    UniqueLegendaryHelper.Config.MythicItems.Add(config);
                    break;
            }
            ExternalLegendaryItems.AddOrSet(rarity, config);
            return RuntimeRegistry.Register(config);
        }
        catch
        {
            OnError?.Invoke("Failed to parse legendary item from external plugin.");
            return null;
        }
    }

    [PublicAPI]
    public static bool UpdateLegendaryItem(string key, string json)
    {
        if (!RuntimeRegistry.TryGetValue(key, out LegendaryInfo legendaryInfo)) return false;
        LegendaryInfo? config = JsonConvert.DeserializeObject<LegendaryInfo>(json);
        if (config == null) return false;
        legendaryInfo.CopyFieldsFrom(config);
        return true;
    }

    [PublicAPI]
    public static string? AddLegendarySet(string type, string json)
    {
        try
        {
            if (!Enum.TryParse(type, true, out ItemRarity rarity)) return null;
            LegendarySetInfo? config = JsonConvert.DeserializeObject<LegendarySetInfo>(json);
            if (config == null) return null;
            switch (rarity)
            {
                case ItemRarity.Legendary:
                    UniqueLegendaryHelper.LegendarySets[config.ID] = config;
                    UniqueLegendaryHelper.Config.LegendarySets.Add(config);
                    foreach (var name in config.LegendaryIDs)
                    {
                        UniqueLegendaryHelper._legendaryItemsToSetMap[name] = config;
                    }
                    break;
                case ItemRarity.Mythic:
                    UniqueLegendaryHelper.MythicSets[config.ID] = config;
                    UniqueLegendaryHelper.Config.MythicSets.Add(config);
                    foreach (var name in config.LegendaryIDs)
                    {
                        UniqueLegendaryHelper._mythicItemsToSetMap[name] = config;
                    }
                    break;
            }
            ExternalLegendarySets.AddOrSet(rarity, config);
            return RuntimeRegistry.Register(config);
        }
        catch
        {
            OnError?.Invoke("Failed to parse legendary set from external plugin.");
            return null;
        }
    }

    [PublicAPI]
    public static bool UpdateLegendarySet(string key, string json)
    {
        if (!RuntimeRegistry.TryGetValue(key, out LegendarySetInfo legendarySetInfo)) return false;
        LegendarySetInfo? config = JsonConvert.DeserializeObject<LegendarySetInfo>(json);
        if (config == null) return false;
        legendarySetInfo.CopyFieldsFrom(config);
        return true;
    }
    #endregion
    #region Ability
    /// <param name="json">JSON serialized <see cref="AbilityDefinition"/></param>
    /// <returns>unique identifier if registered</returns>
    [PublicAPI]
    public static string? AddAbility(string json)
    {
        try
        {
            AbilityDefinition? def = JsonConvert.DeserializeObject<AbilityDefinition>(json);
            if (def == null) return null;
            if (AbilityDefinitions.Abilities.ContainsKey(def.ID))
            {
                OnError?.Invoke($"Duplicate entry found for Abilities: {def.ID} when adding from external plugin.");
                return null;
            }
            ExternalAbilities[def.ID] = def;
            AbilityDefinitions.Config.Abilities.Add(def);
            AbilityDefinitions.Abilities[def.ID] = def;
            return RuntimeRegistry.Register(def);
        }
        catch
        {
            OnError?.Invoke("Failed to parse ability definition passed in through external plugin.");
            return null;
        }
    }
    
    /// <param name="key">unique identifier</param>
    /// <param name="json">JSON serialized <see cref="AbilityDefinition"/></param>
    /// <returns>true if updated</returns>
    [PublicAPI]
    public static bool UpdateAbility(string key, string json)
    {
        if (!RuntimeRegistry.TryGetValue(key, out AbilityDefinition original)) return false;
        AbilityDefinition? def = JsonConvert.DeserializeObject<AbilityDefinition>(json);
        if (def == null) return false;
        original.CopyFieldsFrom(def);
        return true;
    }
    #endregion
    #region Asset
    /// <param name="name"><see cref="string"/></param>
    /// <param name="asset"><see cref="Object"/></param>
    /// <returns>True if added to <see cref="EpicLoot._assetCache"/></returns>
    [PublicAPI]
    public static bool RegisterAsset(string name, Object asset)
    {
        if (EpicLoot._assetCache.ContainsKey(name)) // made _assetCache public
        {
            OnError?.Invoke("Duplicate asset: " + name);
            return false;
        }
        EpicLoot._assetCache[name] = asset;
        ExternalAssets[name] = asset;
        return true;
    }
    #endregion
    #region Material Conversion
    /// <param name="json">JSON serialized <see cref="MaterialConversion"/></param>
    /// <returns>unique key if added to <see cref="MaterialConversions.Conversions"/></returns>
    [PublicAPI]
    public static string? AddMaterialConversion(string json)
    {
        try
        {
            MaterialConversion? conversion = JsonConvert.DeserializeObject<MaterialConversion>(json);
            if (conversion == null) return null;
            ExternalMaterialConversions.Add(conversion);
            MaterialConversions.Config.MaterialConversions.Add(conversion);
            MaterialConversions.Conversions.Add(conversion.Type, conversion);
            return RuntimeRegistry.Register(conversion);
        }
        catch
        {
            OnError?.Invoke("Failed to parse material conversion passed in through external plugin.");
            return null;
        }
    }
    
    /// <param name="key">unique identifier</param>
    /// <param name="json">JSON serialized <see cref="MaterialConversion"/></param>
    /// <returns>True if updated</returns>
    [PublicAPI]
    public static bool UpdateMaterialConversion(string key, string json)
    {
        if (!RuntimeRegistry.TryGetValue(key, out MaterialConversion original)) return false;
        MaterialConversion? conversion = JsonConvert.DeserializeObject<MaterialConversion>(json);
        if (conversion == null) return false;
        original.CopyFieldsFrom(conversion);
        return true;
    }
    #endregion
    #region Recipe
    [PublicAPI]
    public static string? AddRecipe(string json)
    {
        try
        {
            var recipe = JsonConvert.DeserializeObject<RecipeConfig>(json);
            if (recipe == null) return null;
            ExternalRecipes.Add(recipe);
            RecipesHelper.Config.recipes.Add(recipe);
            return RuntimeRegistry.Register(recipe);
        }
        catch
        {
            OnError?.Invoke("Failed to parse recipe passed in through external plugin.");
            return null;
        }
    }
    
    /// <param name="json">JSON serialized List of <see cref="RecipeConfig"/></param>
    /// <returns>unique key if successfully added</returns>
    [PublicAPI]
    public static string? AddRecipes(string json)
    {
        // TODO: Figure out why it looks like recipes are added twice
        // PRIORITY: Low
        // Some interesting logic about re-initializing recipes after item manager on items registered ??
        // Current fix, remove external recipes, then add again on reload
        try
        {
            List<RecipeConfig>? recipes = JsonConvert.DeserializeObject<List<RecipeConfig>>(json);
            if (recipes == null) return null;
            ExternalRecipes.AddRange(recipes);
            RecipesHelper.Config.recipes.AddRange(recipes);
            return RuntimeRegistry.Register(recipes);
        }
        catch
        {
            OnError?.Invoke("Failed to parse recipe from external plugin");
            return null;
        }
    }
    
    /// <param name="key">unique identifier <see cref="string"/></param>
    /// <param name="json">JSON serialized List of <see cref="MaterialConversion"/></param>
    /// <returns>True if updated</returns>
    [PublicAPI]
    public static bool UpdateRecipes(string key, string json)
    {
        if (!RuntimeRegistry.TryGetValue(key, out List<RecipeConfig> list)) return false;
        List<RecipeConfig>? recipes = JsonConvert.DeserializeObject<List<RecipeConfig>>(json);
        if (recipes == null ) return false;
        ExternalRecipes.ReplaceThenAdd(list, recipes);
        RecipesHelper.Config.recipes.ReplaceThenAdd(list, recipes);
        return true;
    }
    #endregion
    #region Sacrifice
    /// <param name="json">JSON serialized <see cref="DisenchantProductsConfig"/></param>
    /// <returns>True if added to <see cref="EnchantCostsHelper.Config"/></returns>
    [PublicAPI]
    public static string? AddSacrifice(string json)
    {
        try
        {
            DisenchantProductsConfig? sacrifice = JsonConvert.DeserializeObject<DisenchantProductsConfig>(json);
            if (sacrifice == null) return null;
            ExternalSacrifices.Add(sacrifice);
            EnchantCostsHelper.Config.DisenchantProducts.Add(sacrifice);
            return RuntimeRegistry.Register(sacrifice);
        }
        catch
        {
            OnError?.Invoke("Failed to parse sacrifice from external plugin");
            return null;
        }
    }
    
    /// <param name="key">unique identifier</param>
    /// <param name="json">JSON serialized <see cref="DisenchantProductsConfig"/></param>
    /// <returns>True if updated</returns>
    [PublicAPI]
    public static bool UpdateSacrifice(string key, string json)
    {
        if (!RuntimeRegistry.TryGetValue(key, out DisenchantProductsConfig disenchantProduct)) return false;
        DisenchantProductsConfig? sacrifice = JsonConvert.DeserializeObject<DisenchantProductsConfig>(json);
        disenchantProduct.CopyFieldsFrom(sacrifice);
        return true;
    }
    #endregion
    #region Bounty
    [PublicAPI]
    public static string? AddBountyTarget(string json)
    {
        try
        {
            var bounty = JsonConvert.DeserializeObject<BountyTargetConfig>(json);
            if (bounty == null) return null;
            ExternalBountyTargets.Add(bounty);
            AdventureDataManager.Config.Bounties.Targets.Add(bounty);
            return RuntimeRegistry.Register(bounty);
        }
        catch
        {
            OnError?.Invoke("Failed to parse bounty target passed in through external plugin.");
            return null;
        }
    }

    [PublicAPI]
    public static bool UpdateBountyTarget(string key, string json)
    {
        if (!RuntimeRegistry.TryGetValue(key, out BountyTargetConfig bountyTarget)) return false;
        BountyTargetConfig? config = JsonConvert.DeserializeObject<BountyTargetConfig>(json);
        bountyTarget.CopyFieldsFrom(config);
        return true;
    }
    #endregion
    #region Helpers
    /// <summary>
    /// Helper function to add into dictionary of lists
    /// </summary>
    /// <param name="dict">Dictionary T key, List V values</param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="V"></typeparam>
    private static void AddOrSet<T, V>(this Dictionary<T, List<V>> dict, T key, V value)
    {
        if (!dict.ContainsKey(key)) dict[key] = new List<V>();
        dict[key].Add(value);
    }
    /// <summary>
    /// Helper function to copy all fields from one instance to the other
    /// </summary>
    /// <param name="target"><see cref="T"/></param>
    /// <param name="source"><see cref="T"/></param>
    /// <typeparam name="T"><see cref="T"/></typeparam>
    private static void CopyFieldsFrom<T>(this T target, T source)
    {
        foreach (FieldInfo field in typeof(T).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            var value = field.GetValue(source);
            field.SetValue(target, value);
        }
    }

    /// <summary>
    /// Helper function, removes all from list, then adds new items into list
    /// </summary>
    /// <param name="list"></param>
    /// <param name="original"></param>
    /// <param name="replacements"></param>
    /// <typeparam name="T"></typeparam>
    private static void ReplaceThenAdd<T>(this List<T> list, List<T> original, List<T> replacements)
    {
        list.RemoveAll(original);
        list.AddRange(replacements);
    }

    /// <summary>
    /// Helper function, removes all instances from one list in the other list
    /// </summary>
    /// <param name="list"></param>
    /// <param name="itemsToRemove"></param>
    /// <typeparam name="T"></typeparam>
    private static void RemoveAll<T>(this List<T> list, List<T> itemsToRemove)
    {
        foreach (var item in itemsToRemove) list.Remove(item);
    }
    #endregion
    #region Registry
    /// <summary>
    /// Simple registry for external assets, useful to keep track of objects,
    /// Keys are generated and returned to external API,
    /// to store and use to update specific objects
    /// </summary>
    /// <remarks>
    /// 1. External plugin invokes to 'add', on success, returns unique key
    /// 2. Key stored in registry with associated object
    /// 3. External plugin invokes to 'update' using unique key to target object
    /// </remarks>
    private static class RuntimeRegistry
    {
        private static readonly Dictionary<string, object> registry = new();
        private static int counter;
        
        /// <param name="obj"></param>
        /// <returns>unique identifier</returns>
        public static string Register(object obj)
        {
            string typeName = obj.GetType().Name;
            string key = $"{typeName}_obj_{++counter}";
            registry[key] = obj;
            return key;
        }
        
        /// <param name="key">unique key <see cref="string"/></param>
        /// <param name="value">object as class type <see cref="T"/></param>
        /// <typeparam name="T">class type <see cref="T"/></typeparam>
        /// <returns>True if object found matching key</returns>
        public static bool TryGetValue<T>(string key, out T value) where T : class
        {
            if (registry.TryGetValue(key, out object obj) && obj is T result)
            {
                value = result;
                return true;
            }

            value = null!;
            return false;
        }

        [PublicAPI]
        public static int GetCount() => counter;
        
        [PublicAPI]
        public static List<string> GetRegisteredKeys() => registry.Keys.ToList();
    }
    #endregion
    #region Ability Proxy
    /// <param name="abilityID"></param>
    /// <param name="proxy"></param>
    /// <returns>true, if callback functions found using unique ability ID</returns>
    private static bool TryGetProxyAbility(string abilityID, out Dictionary<string, Delegate> proxy) => AbilityProxies.TryGetValue(abilityID, out proxy);
    
    /// <param name="json">JSON serialized <see cref="AbilityDefinition"/></param>
    /// <param name="delegates">callback functions</param>
    /// <returns>unique identifier if registered</returns>
    [PublicAPI]
    public static string? RegisterProxyAbility(string json, Dictionary<string, Delegate> delegates)
    {
        try
        {
            AbilityDefinition? ability = JsonConvert.DeserializeObject<AbilityDefinition>(json);
            if (ability == null) return null;
            AbilityFactory.Register(ability.ID, typeof(AbilityProxy));
            AbilityProxies[ability.ID] = delegates;
            AbilityProxyDefinition def = new AbilityProxyDefinition(ability, delegates);
            AbilityDefinitions.Config.Abilities.Add(ability);
            AbilityDefinitions.Abilities[ability.ID] = ability;
            return RuntimeRegistry.Register(def);
        }
        catch
        {
            OnError?.Invoke("Failed to parse ability definition from external plugin");
            return null;
        }
    }
    
    /// <param name="key">unique identifier</param>
    /// <param name="json">JSON serialized <see cref="AbilityDefinition"/></param>
    /// <param name="proxy">callback functions</param>
    /// <returns></returns>
    [PublicAPI]
    public static bool UpdateProxyAbility(string key, string json, Dictionary<string, Delegate> proxy)
    {
        if (!RuntimeRegistry.TryGetValue(key, out AbilityProxyDefinition kvp)) return false;
        AbilityDefinition? def = JsonConvert.DeserializeObject<AbilityDefinition>(json);
        kvp.Ability.CopyFieldsFrom(def);
        kvp.Delegates.CopyFieldsFrom(proxy);
        return true;
    }

    /// <summary>
    /// Custom class to contain proxy definition, to register as a unit
    /// </summary>
    private class AbilityProxyDefinition
    {
        public readonly AbilityDefinition Ability;
        public readonly Dictionary<string, Delegate> Delegates;
        public AbilityProxyDefinition(AbilityDefinition ability, Dictionary<string, Delegate> delegates)
        {
            Ability = ability;
            Delegates = delegates;
        }
    }
    
    /// <param name="proxy"><see cref="AbilityProxy"/></param>
    /// <param name="abilityID">unique ability ID</param>
    /// <returns>true if dictionary of callbacks injected into new instance of proxy</returns>
    public static bool InjectCallbacks(this AbilityProxy proxy, string abilityID)
    {
        if (!TryGetProxyAbility(abilityID, out Dictionary<string, Delegate> delegates)) return false;
        proxy._callbacks = delegates;
        return true;
    }
    
    /// <summary>
    /// Ability wrapper to handle callbacks
    /// </summary>
    public class AbilityProxy : Ability
    {
        public Dictionary<string, Delegate> _callbacks = new Dictionary<string, Delegate>();
        private T? GetCallback<T>(string name) where T : Delegate
        {
            if (_callbacks.TryGetValue(name, out Delegate? del) && del is T typed) return typed;
            return null;
        }

        public override void Initialize(AbilityDefinition abilityDef, Player player)
        {
            base.Initialize(abilityDef, player);
            if (GetCallback<Action<Player, string, float>>(nameof(Initialize)) is not { } callback) return;
            callback(player, abilityDef.ID, abilityDef.Cooldown);
        }

        public override void OnUpdate()
        {
            if (GetCallback<Action>(nameof(OnUpdate)) is not { } callback) base.OnUpdate();
            else callback();
        }

        protected override bool ShouldTrigger()
        {
            return GetCallback<Func<bool>>(nameof(ShouldTrigger)) is not { } callback
                ? base.ShouldTrigger()
                : callback();
        }

        public override bool IsOnCooldown() => GetCallback<Func<bool>>(nameof(IsOnCooldown)) is not { } callback
            ? base.IsOnCooldown()
            : callback();

        public override float TimeUntilCooldownEnds() =>
            GetCallback<Func<float>>(nameof(TimeUntilCooldownEnds)) is not { } callback
                ? base.TimeUntilCooldownEnds()
                : callback();

        public override float PercentCooldownComplete() =>
            GetCallback<Func<float>>(nameof(PercentCooldownComplete)) is not { } callback
                ? base.PercentCooldownComplete()
                : callback();

        public override bool CanActivate() => GetCallback<Func<bool>>(nameof(CanActivate)) is not { } callback
            ? base.CanActivate()
            : callback();

        public override void TryActivate()
        {
            if (GetCallback<Action>(nameof(TryActivate)) is not { } callback) base.TryActivate();
            else callback();
        }

        protected override void Activate()
        {
            if (GetCallback<Action>(nameof(Activate)) is not { } callback) base.Activate();
            else callback();
        }

        protected override void ActivateCustomAction()
        {
            if (GetCallback<Action>(nameof(ActivateCustomAction)) is not { } callback) base.ActivateCustomAction();
            else callback();
        }

        protected override void ActivateStatusEffectAction()
        {
            if (GetCallback<Action>(nameof(ActivateStatusEffectAction)) is not { } callback)
                base.ActivateStatusEffectAction();
            else callback();
        }

        protected override bool HasCooldown() => GetCallback<Func<bool>>(nameof(HasCooldown)) is not { } callback
            ? base.HasCooldown()
            : callback();

        protected override void SetCooldownEndTime(float cooldownEndTime)
        {
            if (GetCallback<Action<float>>(nameof(SetCooldownEndTime)) is not { } callback)
                base.SetCooldownEndTime(cooldownEndTime);
            else callback(cooldownEndTime);
        }
        public override float GetCooldownEndTime() => GetCallback<Func<float>>(nameof(GetCooldownEndTime)) is not { } callback 
            ? base.GetCooldownEndTime() : callback();
    }
    #endregion
}
