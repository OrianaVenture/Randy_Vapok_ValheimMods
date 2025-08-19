using System;
using EpicLoot.MagicItemEffects;
using Newtonsoft.Json;
using System.Collections.Generic;
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
    
    public static readonly Dictionary<string, MagicItemEffectDefinition> ExternalMagicItemEffectDefinitions = new();
    public static readonly Dictionary<string, AbilityDefinition> ExternalAbilities = new();
    public static readonly List<LegendaryItemConfig> ExternalLegendaryConfig = new();
    public static readonly Dictionary<string, Object> ExternalAssets = new();
    public static readonly List<MaterialConversion> ExternalMaterialConversions = new();
    public static readonly List<RecipeConfig> ExternalRecipes = new();
    public static readonly List<DisenchantProductsConfig> ExternalSacrifices = new();
    public static readonly List<BountyTargetConfig> ExternalBountyTargets = new();

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
    }

    /// <summary>
    /// Reloads cached external adventure data into AdventureDataManager.Config
    /// </summary>
    public static void ReloadExternalAdventureData()
    {
        ReloadExternalBounties();
    }

    /// <summary>
    /// Reloads cached external bounties into AdventureDataManager.Config.Targets
    /// </summary>
    public static void ReloadExternalBounties()
    {
        AdventureDataManager.Config.Bounties.Targets.AddRange(ExternalBountyTargets);
        OnReload?.Invoke("Reloaded external bounties");
    }

    /// <summary>
    /// Reloads cached external enchanting costs into EnchantCostHelper.Config
    /// </summary>
    public static void ReloadExternalSacrifices()
    {
        EnchantCostsHelper.Config.DisenchantProducts.AddRange(ExternalSacrifices);
        OnReload?.Invoke("Reloaded external sacrifices");
    }

    /// <summary>
    /// Reloads cached external recipes into RecipesHelper.Config.recipes
    /// </summary>
    public static void ReloadExternalRecipes()
    {
        RecipesHelper.Config.recipes.AddRange(ExternalRecipes);
        OnReload?.Invoke("Reloaded external recipes");
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
        OnReload?.Invoke("Reloaded external material conversions");
    }
    
    /// <summary>
    /// Reloads cached external magic effects into AllDefinitions via Add().
    /// </summary>
    public static void ReloadExternalMagicEffects()
    {
        foreach (MagicItemEffectDefinition effect in ExternalMagicItemEffectDefinitions.Values) MagicItemEffectDefinitions.Add(effect);
        OnReload?.Invoke("Reloaded external magic effects");
    }

    /// <summary>
    /// Reloads cached external abilities into AbilityDefinitions.
    /// </summary>
    public static void ReloadExternalAbilities()
    {
        foreach (KeyValuePair<string, AbilityDefinition> kvp in ExternalAbilities)
        {
            AbilityDefinitions.Config.Abilities.Add(kvp.Value);
            AbilityDefinitions.Abilities[kvp.Key] = kvp.Value;
        }
        OnReload?.Invoke("Reloaded external abilities");
    }

    /// <summary>
    /// Reloads cached legendary items and sets into UniqueLegendaryHelper.
    /// </summary>
    public static void ReloadExternalLegendary()
    {
        foreach (LegendaryItemConfig config in ExternalLegendaryConfig)
        {
            UniqueLegendaryHelper.LegendaryInfo.AddInfo(config.LegendaryItems);
            UniqueLegendaryHelper.MythicInfo.AddInfo(config.MythicItems);
            AddSet(UniqueLegendaryHelper.LegendarySets, UniqueLegendaryHelper._legendaryItemsToSetMap, config.LegendarySets);
            AddSet(UniqueLegendaryHelper.MythicSets, UniqueLegendaryHelper._mythicItemsToSetMap, config.MythicSets);
        }
        OnReload?.Invoke("Reloaded external legendary abilities");
    }

    /// <summary>
    /// Reloads cached assets into EpicLoot._assetCache.
    /// </summary>
    public static void ReloadExternalAssets()
    {
        foreach (KeyValuePair<string, Object> kvp in ExternalAssets)
        {
            EpicLoot._assetCache[kvp.Key] = kvp.Value;
        }
        OnReload?.Invoke("Reloaded external assets");
    }

    /// <summary>
    /// Called via reflection with a serialized MagicItemEffectDefinition, 
    /// which is deserialized and added to EpicLoot.
    /// </summary>
    /// <param name="json">Serialized magic effect definition</param>
    /// <returns>True if added successfully, otherwise false</returns>
    [PublicAPI]
    public static string AddMagicEffect(string json)
    {
        try
        {
            MagicItemEffectDefinition def = JsonConvert.DeserializeObject<MagicItemEffectDefinition>(json);
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
    /// <summary>
    /// Called via reflection with a unique key and a serialized MagicItemEffectDefinition
    /// </summary>
    /// <param name="key"></param>
    /// <param name="json"></param>
    /// <returns>True if updated</returns>

    [PublicAPI]
    public static bool UpdateMagicEffect(string key, string json)
    {
        if (!RuntimeRegistry.TryGetValue(key, out MagicItemEffectDefinition original)) return false;
        MagicItemEffectDefinition def = JsonConvert.DeserializeObject<MagicItemEffectDefinition>(json);
        original.CopyFieldsFrom(def);
        return true;
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
    public static string AddLegendaryItemConfig(string json)
    {
        try
        {
            LegendaryItemConfig config = JsonConvert.DeserializeObject<LegendaryItemConfig>(json);
            
            ExternalLegendaryConfig.Add(config);
            
            UniqueLegendaryHelper.Config.LegendaryItems.AddRange(config.LegendaryItems);
            UniqueLegendaryHelper.Config.LegendarySets.AddRange(config.LegendarySets);
            UniqueLegendaryHelper.Config.MythicItems.AddRange(config.MythicItems);
            UniqueLegendaryHelper.Config.MythicSets.AddRange(config.MythicSets);
            
            UniqueLegendaryHelper.LegendaryInfo.AddInfo(config.LegendaryItems);
            UniqueLegendaryHelper.MythicInfo.AddInfo(config.MythicItems);
            AddSet(UniqueLegendaryHelper.LegendarySets, UniqueLegendaryHelper._legendaryItemsToSetMap, config.LegendarySets);
            AddSet(UniqueLegendaryHelper.MythicSets, UniqueLegendaryHelper._mythicItemsToSetMap, config.MythicSets);
            return RuntimeRegistry.Register(config);
        }
        catch
        {
            OnError?.Invoke("Failed to parse legendary item config from external plugin");
            return null;
        }
    }

    /// <summary>
    /// Called via reflection with a unique key and serialized LegendaryItemConfig
    /// </summary>
    /// <param name="key"></param>
    /// <param name="json"></param>
    /// <returns>True if updated</returns>
    [PublicAPI]
    public static bool UpdateLegendaryItemConfig(string key, string json)
    {
        if (!RuntimeRegistry.TryGetValue(key, out LegendaryItemConfig original)) return false;
        LegendaryItemConfig config = JsonConvert.DeserializeObject<LegendaryItemConfig>(json);
        original.CopyFieldsFrom(config);
        return true;
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
            else OnError?.Invoke($"Duplicate entry found for Legendary Info: {info.ID} when adding from external plugin");
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
                OnError?.Invoke($"Duplicate entry found for Legendary Set: {info.ID} when adding from external plugin");
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
                    OnError?.Invoke($"Duplicate entry found for Legendary Set: {info.ID}: {legendaryID} when adding from external plugin");
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
    public static string AddAbility(string json)
    {
        try
        {
            AbilityDefinition def = JsonConvert.DeserializeObject<AbilityDefinition>(json);
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

    /// <summary>
    /// Called via reflection with a unique key and a serialized AbilityDefinition
    /// </summary>
    /// <param name="key"></param>
    /// <param name="json"></param>
    /// <returns>True if updated</returns>
    [PublicAPI]
    public static bool UpdateAbility(string key, string json)
    {
        if (!RuntimeRegistry.TryGetValue(key, out AbilityDefinition original)) return false;
        AbilityDefinition def = JsonConvert.DeserializeObject<AbilityDefinition>(json);
        original.CopyFieldsFrom(def);
        return true;
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
        foreach (ItemDrop.ItemData item in player.GetEquipment())
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
        foreach (ItemDrop.ItemData item in player.GetEquipment())
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
            OnError?.Invoke("Duplicate asset: " + name);
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
    /// <returns>unique key if added to MaterialConversions.Conversions</returns>
    [PublicAPI]
    public static string AddMaterialConversion(string json)
    {
        try
        {
            MaterialConversion conversion = JsonConvert.DeserializeObject<MaterialConversion>(json);
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

    /// <summary>
    /// Called via reflection with a unique key and a serialized MaterialConversion
    /// </summary>
    /// <param name="key"></param>
    /// <param name="json"></param>
    /// <returns>True if updated</returns>
    [PublicAPI]
    public static bool UpdateMaterialConversion(string key, string json)
    {
        if (!RuntimeRegistry.TryGetValue(key, out MaterialConversion original)) return false;
        MaterialConversion conversion = JsonConvert.DeserializeObject<MaterialConversion>(json);
        original.CopyFieldsFrom(conversion);
        return true;
    }

    /// <summary>
    /// Called via reflection with serialized List of RecipeConfig
    /// </summary>
    /// <param name="json"></param>
    /// <returns>unique key if successfully added</returns>
    [PublicAPI]
    public static string AddRecipes(string json)
    {
        // TODO: Figure out why it looks like recipes are added twice
        try
        {
            List<RecipeConfig> recipes = JsonConvert.DeserializeObject<List<RecipeConfig>>(json);
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

    /// <summary>
    /// Called via reflection with a unique key and a serialized List RecipeConfig
    /// </summary>
    /// <param name="key"></param>
    /// <param name="json"></param>
    /// <returns>True if updated</returns>
    [PublicAPI]
    public static bool UpdateRecipes(string key, string json)
    {
        if (!RuntimeRegistry.TryGetValue(key, out List<RecipeConfig> list)) return false;
        List<RecipeConfig> recipes = JsonConvert.DeserializeObject<List<RecipeConfig>>(json);
        ExternalRecipes.ReplaceThenAdd(list, recipes);
        RecipesHelper.Config.recipes.ReplaceThenAdd(list, recipes);
        return true;
    }
    
    /// <summary>
    /// Called via reflection serialized List DisenchantProductsConfig
    /// </summary>
    /// <param name="json"></param>
    /// <returns>True if added to EnchantCostsHelper.Config.DisenchantProducts</returns>
    [PublicAPI]
    public static string AddSacrifices(string json)
    {
        try
        {
            List<DisenchantProductsConfig> sacrifices = JsonConvert.DeserializeObject<List<DisenchantProductsConfig>>(json);
            ExternalSacrifices.AddRange(sacrifices);
            EnchantCostsHelper.Config.DisenchantProducts.AddRange(sacrifices);
            return RuntimeRegistry.Register(sacrifices);
        }
        catch
        {
            OnError?.Invoke("Failed to parse sacrifices from external plugin");
            return null;
        }
    }

    /// <summary>
    /// Searches the runtime registry for matching key, to grab instanced objects,
    /// Removes them from EnchantCostsHelper.Config.DisenchantProducts, 
    /// Adds updated objects back into EnchantCostsHelper.Config.DisenchantProducts
    /// </summary>
    /// <param name="key"></param>
    /// <param name="json"></param>
    /// <returns>True if items are removed and re-added</returns>
    [PublicAPI]
    public static bool UpdateSacrifices(string key, string json)
    {
        try
        {
            if (!RuntimeRegistry.TryGetValue<List<DisenchantProductsConfig>>(key, out var list)) return false;
            List<DisenchantProductsConfig> sacrifices = JsonConvert.DeserializeObject<List<DisenchantProductsConfig>>(json);
            EnchantCostsHelper.Config.DisenchantProducts.ReplaceThenAdd(list, sacrifices);
            ExternalSacrifices.ReplaceThenAdd(list, sacrifices);
            return true;
        }
        catch
        {
            OnError?.Invoke("Failed to parse sacrifices from external plugin");
            return false;
        }
    }

    /// <summary>
    /// Called via reflection serialized List BountyTargetConfig
    /// </summary>
    /// <param name="json"></param>
    /// <returns></returns>
    [PublicAPI]
    public static string AddBountyTargets(string json)
    {
        try
        {
            List<BountyTargetConfig> bounties =  JsonConvert.DeserializeObject<List<BountyTargetConfig>>(json);
            ExternalBountyTargets.AddRange(bounties);
            AdventureDataManager.Config.Bounties.Targets.AddRange(bounties);
            return RuntimeRegistry.Register(bounties);
        }
        catch
        {
            OnError?.Invoke("Failed to parse list of target bounties from external plugin");
            return null;
        }
    }

    [PublicAPI]
    public static bool UpdateBountyTargets(string key, string json)
    {
        if (!RuntimeRegistry.TryGetValue<List<BountyTargetConfig>>(key, out var list)) return false;
        List<BountyTargetConfig> bounties =  JsonConvert.DeserializeObject<List<BountyTargetConfig>>(json);
        ExternalBountyTargets.ReplaceThenAdd(list, bounties);
        AdventureDataManager.Config.Bounties.Targets.ReplaceThenAdd(list, bounties);
        return true;
    }

    /// <summary>
    /// Simple registry for external assets, useful to keep track of objects,
    /// Keys are generated and returned to external API,
    /// to store and use to update specific objects
    /// <remarks>
    /// 1. External plugin invokes to 'add', on success, returns unique key
    /// 2. Key stored in registry with associated object
    /// 3. External plugin invokes to 'update' using unique key to target object
    /// </remarks>
    /// </summary>
    private static class RuntimeRegistry
    {
        private static readonly Dictionary<string, object> registry = new();
        private static int counter = 0;

        /// <summary>
        /// Register an object into string, object dictionary
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string Register(object obj)
        {
            string key = $"obj_{++counter}";
            registry[key] = obj;
            return key;
        }
        
        /// <summary>
        /// Tries to get object using unique key
        /// </summary>
        /// <param name="key">unique key</param>
        /// <param name="value">object as class type</param>
        /// <typeparam name="T">class type</typeparam>
        /// <returns>True if object found matching key</returns>
        public static bool TryGetValue<T>(string key, out T value) where T : class
        {
            if (registry.TryGetValue(key, out var obj) && obj is T result)
            {
                value = result;
                return true;
            }

            value = null;
            return false;
        }
    }

    /// <summary>
    /// Helper function to copy all fields from one instance to the other
    /// </summary>
    /// <param name="target"></param>
    /// <param name="source"></param>
    /// <typeparam name="T"></typeparam>
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
}
