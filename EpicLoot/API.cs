using EpicLoot.MagicItemEffects;
using Newtonsoft.Json;
using System.Collections.Generic;
using Common;
using EpicLoot.Abilities;
using EpicLoot.Crafting;
using EpicLoot.CraftingV2;
using EpicLoot.LegendarySystem;
using JetBrains.Annotations;
using Object = UnityEngine.Object;

namespace EpicLoot;

public static class API
{
    private static bool ShowLogs = false;
    public static readonly Dictionary<string, MagicItemEffectDefinition> ExternalMagicItemEffectDefinitions = new();
    public static readonly Dictionary<string, AbilityDefinition> ExternalAbilities = new();
    public static readonly List<LegendaryItemConfig> ExternalLegendaryConfig = new();
    public static readonly Dictionary<string, Object> ExternalAssets = new();
    public static readonly List<MaterialConversion> ExternalMaterialConversions = new();
    public static readonly List<RecipeConfig> ExternalRecipes = new();
    public static readonly List<DisenchantProductsConfig> ExternalSacrifices = new();

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
    }

    /// <summary>
    /// Reloads cached external enchanting costs into EnchantCostHelper.Config
    /// </summary>
    public static void ReloadExternalSacrifices()
    {
        EnchantCostsHelper.Config.DisenchantProducts.AddRange(ExternalSacrifices);
        if (ShowLogs) EpicLoot.Log("Reloaded external sacrifices");
    }

    /// <summary>
    /// Reloads cached external recipes into RecipesHelper.Config.recipes
    /// </summary>
    public static void ReloadExternalRecipes()
    {
        RecipesHelper.Config.recipes.AddRange(ExternalRecipes);
        if (ShowLogs) EpicLoot.Log("Reloaded external recipes");
    }

    /// <summary>
    /// Reloads cached external material conversions into MaterialConversions.Conversions
    /// </summary>
    public static void ReloadExternalMaterialConversions()
    {
        foreach (MaterialConversion entry in ExternalMaterialConversions)
        {
            MaterialConversions.Config.MaterialConversions.Add(entry);
            MaterialConversions.Conversions.Add(entry.Type, entry);
        }
        if (ShowLogs) EpicLoot.Log("Reloaded external material conversions");
    }
    
    /// <summary>
    /// Reloads cached external magic effects into AllDefinitions via Add().
    /// </summary>
    public static void ReloadExternalMagicEffects()
    {
        foreach (var effect in ExternalMagicItemEffectDefinitions.Values) MagicItemEffectDefinitions.Add(effect);
        if (ShowLogs) EpicLoot.Log("Reloaded external magic effects");
    }

    /// <summary>
    /// Reloads cached external abilities into AbilityDefinitions.
    /// </summary>
    public static void ReloadExternalAbilities()
    {
        foreach (var kvp in ExternalAbilities)
        {
            AbilityDefinitions.Config.Abilities.Add(kvp.Value);
            AbilityDefinitions.Abilities[kvp.Key] = kvp.Value;
        }
        if (ShowLogs) EpicLoot.Log("Reloaded external abilities");
    }

    /// <summary>
    /// Reloads cached legendary items and sets into UniqueLegendaryHelper.
    /// </summary>
    public static void ReloadExternalLegendary()
    {
        foreach (var config in ExternalLegendaryConfig)
        {
            UniqueLegendaryHelper.LegendaryInfo.AddInfo(config.LegendaryItems);
            UniqueLegendaryHelper.MythicInfo.AddInfo(config.MythicItems);
            AddSet(UniqueLegendaryHelper.LegendarySets, UniqueLegendaryHelper._legendaryItemsToSetMap, config.LegendarySets);
            AddSet(UniqueLegendaryHelper.MythicSets, UniqueLegendaryHelper._mythicItemsToSetMap, config.MythicSets);
        }
        if (ShowLogs) EpicLoot.Log("Reloaded external legendary");
    }

    /// <summary>
    /// Reloads cached assets into EpicLoot._assetCache.
    /// </summary>
    public static void ReloadExternalAssets()
    {
        foreach (var kvp in ExternalAssets)
        {
            EpicLoot._assetCache[kvp.Key] = kvp.Value;
        }
        if (ShowLogs) EpicLoot.Log("Reloaded external assets");
    }

    /// <summary>
    /// Called via reflection with a serialized MagicItemEffectDefinition, 
    /// which is deserialized and added to EpicLoot.
    /// </summary>
    /// <param name="json">Serialized magic effect definition</param>
    /// <returns>True if added successfully, otherwise false</returns>
    [PublicAPI]
    public static bool AddMagicEffect(string json)
    {
        try
        {
            var def = JsonConvert.DeserializeObject<MagicItemEffectDefinition>(json);
            MagicItemEffectDefinitions.Add(def);
            ExternalMagicItemEffectDefinitions[def.Type] = def;
            
            return true;
        }
        catch
        {
            EpicLoot.LogWarning("Failed to parse magic effect from external plugin");
            return false;
        }
    }

    /// <summary>
    /// Called via reflection
    /// </summary>
    /// <param name="type">Effect type</param>
    /// <returns>serialized object of magic effect definition if found</returns>
    [PublicAPI]
    public static string GetMagicItemEffectDefinition(string type)
    {
        if (!MagicItemEffectDefinitions.AllDefinitions.TryGetValue(type, out MagicItemEffectDefinition definition)) return "";
        return JsonConvert.SerializeObject(definition);
    }

    /// <summary>
    /// Called via reflection
    /// </summary>
    /// <param name="player">can be null</param>
    /// <param name="item">can be null</param>
    /// <param name="effectType"></param>
    /// <param name="scale"></param>
    /// <returns></returns>
    [PublicAPI]
    public static float GetTotalActiveMagicEffectValue([CanBeNull] Player player, [CanBeNull] ItemDrop.ItemData item, string effectType, float scale) => 
        MagicEffectsHelper.GetTotalActiveMagicEffectValue(player, item, effectType, scale);

    /// <summary>
    /// Called via reflection
    /// </summary>
    /// <param name="player">can be null</param>
    /// <param name="item">can be null</param>
    /// <param name="effectType"></param>
    /// <param name="scale"></param>
    /// <returns></returns>
    [PublicAPI]
    public static float GetTotalActiveMagicEffectValueForWeapon([CanBeNull] Player player, [CanBeNull] ItemDrop.ItemData item, string effectType, float scale) =>
        MagicEffectsHelper.GetTotalActiveMagicEffectValueForWeapon(player, item, effectType, scale);

    /// <summary>
    /// Called via reflection
    /// </summary>
    /// <param name="player">can be null</param>
    /// <param name="effectType"></param>
    /// <param name="item">can be null</param>
    /// <param name="effectValue"></param>
    /// <returns>True if player or item has magic effect</returns>
    [PublicAPI]
    public static bool HasActiveMagicEffect([CanBeNull] Player player, string effectType, [CanBeNull] ItemDrop.ItemData item, ref float effectValue) =>
        MagicEffectsHelper.HasActiveMagicEffect(player, item, effectType, out effectValue);
    
    /// <summary>
    /// Called via reflection
    /// </summary>
    /// <param name="player">can be null</param>
    /// <param name="item"></param>
    /// <param name="effectType"></param>
    /// <param name="effectValue"></param>
    /// <returns>True if magic effect is on item</returns>
    [PublicAPI]
    public static bool HasActiveMagicEffectOnWeapon(Player player, ItemDrop.ItemData item, string effectType, ref float effectValue) => 
        MagicEffectsHelper.HasActiveMagicEffectOnWeapon(player, item, effectType, out effectValue);

    /// <summary>
    /// Called via reflection.
    /// Currently hard coded to Modify Armor effect type ???
    /// </summary>
    /// <param name="player"></param>
    /// <param name="effectType"></param>
    /// <param name="scale"></param>
    /// <returns></returns>
    [PublicAPI]
    public static float GetTotalActiveSetEffectValue(Player player, string effectType, float scale) =>
        MagicEffectsHelper.GetTotalActiveSetEffectValue(player, effectType, scale);
    
    /// <summary>
    /// Called via reflection.
    /// </summary>
    /// <param name="player"></param>
    /// <param name="effectType">filter</param>
    /// <returns>list of active magic effects on player</returns>
    [PublicAPI]
    public static List<string> GetAllActiveMagicEffects(Player player, string effectType = null)
    {
        List<MagicItemEffect> list = player.GetAllActiveMagicEffects(effectType);
        List<string> output = new List<string>();
        foreach (MagicItemEffect item in list)
        {
            output.Add(JsonConvert.SerializeObject(item));
        }

        return output;
    }

    /// <summary>
    /// Called via reflection.
    /// </summary>
    /// <param name="player"></param>
    /// <param name="effectType">filter</param>
    /// <returns>list of active magic effects on set</returns>
    [PublicAPI]
    public static List<string> GetAllActiveSetMagicEffects(Player player, string effectType = null)
    {
        List<MagicItemEffect> list = player.GetAllActiveSetMagicEffects(effectType);
        List<string> output = new List<string>();
        foreach (MagicItemEffect item in list)
        {
            output.Add(JsonConvert.SerializeObject(item));
        }

        return output;
    }

    /// <summary>
    /// Called via reflection.
    /// </summary>
    /// <param name="player"></param>
    /// <param name="effectType"></param>
    /// <param name="scale"></param>
    /// <param name="ignoreThisItem"></param>
    /// <returns>total effect value found on player</returns>
    [PublicAPI]
    public static float GetTotalPlayerActiveMagicEffectValue(Player player, string effectType, float scale,
        ItemDrop.ItemData ignoreThisItem = null) =>
        player.GetTotalActiveMagicEffectValue(effectType, scale, ignoreThisItem);

    /// <summary>
    /// Called via reflection.
    /// </summary>
    /// <param name="player"></param>
    /// <param name="effectType"></param>
    /// <param name="effectValue"></param>
    /// <param name="scale"></param>
    /// <param name="ignoreThisItem"></param>
    /// <returns>True if player has magic effect</returns>
    [PublicAPI]
    public static bool PlayerHasActiveMagicEffect(Player player, string effectType, ref float effectValue,
        float scale = 1.0f, ItemDrop.ItemData ignoreThisItem = null) =>
        player.HasActiveMagicEffect(effectType, out effectValue, scale, ignoreThisItem);

    /// <summary>
    /// Called via reflection with a serialized LegendaryItemConfig
    /// </summary>
    /// <param name="json"></param>
    /// <returns>True if added to EpicLoot UniqueLegendaryHelper</returns>
    [PublicAPI]
    public static bool AddLegendaryItemConfig(string json)
    {
        try
        {
            var config = JsonConvert.DeserializeObject<LegendaryItemConfig>(json);
            
            ExternalLegendaryConfig.Add(config);
            
            UniqueLegendaryHelper.Config.LegendaryItems.AddRange(config.LegendaryItems);
            UniqueLegendaryHelper.Config.LegendarySets.AddRange(config.LegendarySets);
            UniqueLegendaryHelper.Config.MythicItems.AddRange(config.MythicItems);
            UniqueLegendaryHelper.Config.MythicSets.AddRange(config.MythicSets);
            
            UniqueLegendaryHelper.LegendaryInfo.AddInfo(config.LegendaryItems);
            UniqueLegendaryHelper.MythicInfo.AddInfo(config.MythicItems);
            AddSet(UniqueLegendaryHelper.LegendarySets, UniqueLegendaryHelper._legendaryItemsToSetMap, config.LegendarySets);
            AddSet(UniqueLegendaryHelper.MythicSets, UniqueLegendaryHelper._mythicItemsToSetMap, config.MythicSets);
            return true;
        }
        catch
        {
            EpicLoot.LogWarning("Failed to parse legendary item config from external plugin");
            return false;
        }
    }

    /// <summary>
    /// Helper function to add legendary item to relevant dictionary
    /// </summary>
    /// <param name="target"></param>
    /// <param name="legendaryItems"></param>
    private static void AddInfo(this Dictionary<string, LegendaryInfo> target, List<LegendaryInfo> legendaryItems)
    {
        foreach (LegendaryInfo info in legendaryItems)
        {
            if (!target.ContainsKey(info.ID)) target[info.ID] = info;
            else EpicLoot.LogWarning($"Duplicate entry found for Legendary Info: {info.ID} when adding from external plugin");
        }
    }

    /// <summary>
    /// Helper function to add legendary sets to relevant dictionaries
    /// </summary>
    /// <param name="target"></param>
    /// <param name="itemToSet"></param>
    /// <param name="legendarySet"></param>
    private static void AddSet(Dictionary<string, LegendarySetInfo> target, Dictionary<string, LegendarySetInfo> itemToSet, List<LegendarySetInfo> legendarySet)
    {
        foreach (LegendarySetInfo info in legendarySet)
        {
            if (!target.ContainsKey(info.ID))
            {
                target[info.ID] = info;
            }
            else
            {
                EpicLoot.LogWarning($"Duplicate entry found for Legendary Set: {info.ID} when adding from external plugin");
                continue;
            }

            foreach (var legendaryID in info.LegendaryIDs)
            {
                if (!itemToSet.ContainsKey(legendaryID))
                {
                    itemToSet[legendaryID] = info;
                }
                else
                {
                    EpicLoot.LogWarning($"Duplicate entry found for Legendary Set: {info.ID}: {legendaryID} when adding from external plugin");
                }
            }
        }
    }
    
    /// <summary>
    /// Called via reflection with a serialized AbilityDefinition
    /// </summary>
    /// <param name="json"></param>
    /// <returns>True if added to AbilityDefinitions</returns>
    [PublicAPI]
    public static bool AddAbility(string json)
    {
        try
        {
            var def = JsonConvert.DeserializeObject<AbilityDefinition>(json);
            if (AbilityDefinitions.Abilities.ContainsKey(def.ID))
            {
                EpicLoot.LogWarning($"Duplicate entry found for Abilities: {def.ID} when adding from external plugin.");
                return false;
            }
            ExternalAbilities[def.ID] = def;
            AbilityDefinitions.Config.Abilities.Add(def);
            AbilityDefinitions.Abilities[def.ID] = def;
            return true;
        }
        catch
        {
            EpicLoot.LogWarning("Failed to parse ability definition passed in through external plugin.");
            return false;
        }
    }

    /// <summary>
    /// Called via reflection. Can be useful for external plugins to know, so they can design features around it.
    /// </summary>
    /// <param name="player"></param>
    /// <param name="legendaryItemID"></param>
    /// <returns>True if player has item</returns>
    [PublicAPI]
    public static bool HasLegendaryItem(Player player, string legendaryItemID)
    {
        foreach (var item in player.GetEquipment())
        {
            if (item.IsMagic(out var magicItem) && magicItem.LegendaryID == legendaryItemID) return true;
        }

        return false;
    }

    /// <summary>
    /// Called via reflection. Can be useful for external plugins to know, so they can design features around it.
    /// </summary>
    /// <param name="player"></param>
    /// <param name="legendarySetID"></param>
    /// <param name="count"></param>
    /// <returns>True if player has full set</returns>
    [PublicAPI]
    public static bool HasLegendarySet(Player player, string legendarySetID, ref int count)
    {
        if (!UniqueLegendaryHelper.LegendarySets.TryGetValue(legendarySetID, out LegendarySetInfo info) || !UniqueLegendaryHelper.MythicSets.TryGetValue(legendarySetID, out info))
        {
            return false;
        }
        foreach (var item in player.GetEquipment())
        {
            if (item.IsMagic(out var magicItem) && magicItem.SetID == legendarySetID)
            {
                ++count;
            }
        }

        return count >= info.LegendaryIDs.Count;
    }

    /// <summary>
    /// Called via reflection
    /// </summary>
    /// <param name="name"></param>
    /// <param name="asset"></param>
    /// <returns>True if added to EpicLoot._assetCache</returns>
    [PublicAPI]
    public static bool RegisterAsset(string name, Object asset)
    {
        if (EpicLoot._assetCache.ContainsKey(name)) // made _assetCache public
        {
            EpicLoot.LogWarning("Duplicate asset: " + name);
            return false;
        }
        EpicLoot._assetCache[name] = asset;
        ExternalAssets[name] = asset;
        return true;
    }

    /// <summary>
    /// Called via reflection with serialized MaterialConversion
    /// </summary>
    /// <param name="json"></param>
    /// <returns>True if added to MaterialConversions.Conversions</returns>
    [PublicAPI]
    public static bool AddMaterialConversion(string json)
    {
        try
        {
            MaterialConversion conversion = JsonConvert.DeserializeObject<MaterialConversion>(json);
            ExternalMaterialConversions.Add(conversion);
            MaterialConversions.Config.MaterialConversions.Add(conversion);
            MaterialConversions.Conversions.Add(conversion.Type, conversion);
            return true;
        }
        catch
        {
            EpicLoot.LogWarning("Failed to parse material conversion passed in through external plugin.");
            return false;
        }
    }

    /// <summary>
    /// Called via reflection with serialized List of RecipeConfig
    /// </summary>
    /// <param name="json"></param>
    /// <returns></returns>
    [PublicAPI]
    public static bool AddRecipes(string json)
    {
        try
        {
            List<RecipeConfig> recipes = JsonConvert.DeserializeObject<List<RecipeConfig>>(json);
            ExternalRecipes.AddRange(recipes);
            RecipesHelper.Config.recipes.AddRange(recipes);
            return true;
        }
        catch
        {
            EpicLoot.LogWarning("Failed to parse recipe from external plugin");
            return false;
        }
    }

    /// <summary>
    /// Called via reflection serialized List DisenchantProductsConfig
    /// </summary>
    /// <param name="json"></param>
    /// <returns>True if added to EnchantCostsHelper.Config.DisenchantProducts</returns>
    [PublicAPI]
    public static bool AddSacrifices(string json)
    {
        try
        {
            List<DisenchantProductsConfig> sacrifices = JsonConvert.DeserializeObject<List<DisenchantProductsConfig>>(json);
            ExternalSacrifices.AddRange(sacrifices);
            EnchantCostsHelper.Config.DisenchantProducts.AddRange(sacrifices);
            return true;
        }
        catch
        {
            EpicLoot.LogWarning("Failed to parse enchanting costs from external plugin");
            return false;
        }
    }
}
