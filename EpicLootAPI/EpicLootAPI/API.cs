using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;
using Object = UnityEngine.Object;

namespace EpicLootAPI;
public static class EpicLoot
{
    private const string Namespace = "EpicLoot";
    private const string ClassName = "API";
    private const string Assembly = "EpicLoot";
    private const string API_LOCATION = Namespace + "." + ClassName + ", " + Assembly;
    private static event Action<string>? OnError;
    static EpicLoot()
    {
        OnError += message => Debug.LogWarning("[EpicLoot API] " + message);
    }

    internal static readonly List<MagicItemEffectDefinition> MagicEffects = new();
    internal static readonly List<AbilityDefinition> Abilities = new();
    internal static readonly List<MaterialConversion> MaterialConversions = new();
    internal static readonly LegendaryItemConfig LegendaryConfig = new();
    internal static readonly RecipesConfig Recipes = new();
    internal static readonly List<Sacrifice> Sacrifices = new();
    internal static readonly List<BountyTarget> BountyTargets = new();
    internal static readonly Dictionary<SecretStashType, List<SecretStashItem>> SecretStashes = new();
    internal static readonly List<TreasureMapBiomeInfoConfig> Treasures = new();
    
    #region Reflection Methods
    private static readonly Method API_AddMagicEffect = new("AddMagicEffect");
    private static readonly Method API_GetTotalActiveMagicEffectValue = new ("GetTotalActiveMagicEffectValue");
    private static readonly Method API_GetTotalActiveMagicEffectValueForWeapon = new("GetTotalActiveMagicEffectValueForWeapon");
    private static readonly Method API_HasActiveMagicEffect = new("HasActiveMagicEffect");
    private static readonly Method API_HasActiveMagicEffectOnWeapon = new("HasActiveMagicEffectOnWeapon");
    private static readonly Method API_GetTotalActiveSetEffectValue = new("GetTotalActiveSetEffectValue");
    private static readonly Method API_GetMagicEffectDefinitionCopy = new("GetMagicItemEffectDefinition");
    private static readonly Method API_GetAllActiveMagicEffects = new("GetAllActiveMagicEffects");
    private static readonly Method API_GetAllSetMagicEffects = new("GetAllActiveSetMagicEffects");
    private static readonly Method API_GetPlayerTotalActiveMagicEffectValue = new("GetTotalPlayerActiveMagicEffectValue");
    private static readonly Method API_PlayerHasActiveMagicEffect = new("PlayerHasActiveMagicEffect");
    private static readonly Method API_AddLegendaryItemConfig = new("AddLegendaryItemConfig");
    private static readonly Method API_AddAbility = new("AddAbility");
    private static readonly Method API_RegisterProxyAbility = new("RegisterProxyAbility");
    private static readonly Method API_UpdateProxyAbility = new("UpdateProxyAbility");
    private static readonly Method API_HasLegendaryItem = new("HasLegendaryItem");
    private static readonly Method API_HasLegendarySet = new("HasLegendarySet");
    private static readonly Method API_RegisterAsset = new("RegisterAsset");
    private static readonly Method API_AddMaterialConversion = new("AddMaterialConversion");
    private static readonly Method API_AddRecipes = new("AddRecipes");
    private static readonly Method API_AddSacrifices = new("AddSacrifices");
    private static readonly Method API_AddBountyTargets = new("AddBountyTargets");
    private static readonly Method API_UpdateBountyTargets = new("UpdateBountyTargets");
    private static readonly Method API_UpdateSacrifices = new ("UpdateSacrifices");
    private static readonly Method API_UpdateMagicEffect = new ("UpdateMagicEffect");
    private static readonly Method API_UpdateLegendaryItem = new ("UpdateLegendaryItemConfig");
    private static readonly Method API_UpdateAbility = new ("UpdateAbility");
    private static readonly Method API_UpdateMaterialConversion = new ("UpdateMaterialConversion");
    private static readonly Method API_UpdateRecipes = new ("UpdateRecipes");
    private static readonly Method API_AddSecretStashItem = new("AddSecretStashItem");
    private static readonly Method API_UpdateSecretStashItem = new("UpdateSecretStashItem");
    private static readonly Method API_AddTreasureMap = new("AddTreasureMap");
    private static readonly Method API_UpdateTreasureMap = new("UpdateTreasureMap");
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
        RegisterBountyTargets();
        RegisterAllTreasureMaps();
    }

    #region Bounty
    [PublicAPI]
    public static void RegisterBountyTargets()
    {
        if (BountyTargets.Count <= 0) return;
        string json = JsonConvert.SerializeObject(BountyTargets);
        object? result = API_AddBountyTargets.Invoke(json);
        if (result is not string key) return;
        RunTimeRegistry.Register(BountyTargets, key);
        BountyTargets.Clear();
    }

    [PublicAPI]
    public static bool UpdateBountyTargets()
    {
        if (!RunTimeRegistry.TryGetValue(BountyTargets, out var key)) return false;
        string json = JsonConvert.SerializeObject(BountyTargets);
        object? result =  API_UpdateBountyTargets.Invoke(key, json);
        return (bool)(result ?? false);
    }
    #endregion

    #region Sacrifices
    /// <returns>unique key if added</returns>
    [PublicAPI]
    public static void RegisterSacrifices()
    {
        if (Sacrifices.Count <= 0) return;
        string json = JsonConvert.SerializeObject(Sacrifices);
        object? result = API_AddSacrifices.Invoke(json);
        if (result is not string key) return;
        RunTimeRegistry.Register(Sacrifices, key);
    }
    
    /// <returns>true if updated</returns>
    [PublicAPI]
    public static bool UpdateSacrifices()
    {
        if (!RunTimeRegistry.TryGetValue(Sacrifices, out string key)) return false;
        string json = JsonConvert.SerializeObject(Sacrifices);
        object? result = API_UpdateSacrifices.Invoke(key, json);
        return (bool)(result ?? false);
    }
    #endregion
    
    #region Recipes
    /// <summary>
    /// Invokes <see cref="API_AddRecipes"/> with serialized List <see cref="CustomRecipe"/>
    /// </summary>
    /// <returns>Unique key if added</returns>
    [PublicAPI]
    public static void RegisterRecipes()
    {
        if (!Recipes.HasValues()) return;
        string json = JsonConvert.SerializeObject(Recipes.recipes);
        object? result = API_AddRecipes.Invoke(json);
        if (result is not string key) return;
        RunTimeRegistry.Register(Recipes.recipes, key);
        Recipes.recipes.Clear();
    }

    /// <summary>
    /// Invokes <see cref="API_UpdateRecipes"/> with unique key and serialized List <see cref="CustomRecipe"/>
    /// </summary>
    /// <returns>True if updated</returns>
    [PublicAPI]
    public static bool UpdateRecipes()
    {
        if (!RunTimeRegistry.TryGetValue(Recipes.recipes, out var key)) return false;
        string json = JsonConvert.SerializeObject(Recipes.recipes);
        object? result = API_UpdateRecipes.Invoke(key, json);
        return (bool)(result ?? false);
    }
    #endregion
    
    #region Material Conversions
    /// <summary>
    /// Helper function to register all defined material conversions
    /// </summary>
    [PublicAPI]
    public static void RegisterMaterialConversions()
    {
        foreach (MaterialConversion conversion in new List<MaterialConversion>(MaterialConversions)) 
            RegisterConversion(conversion);
    }
    
    /// <summary>
    /// Register material conversion to EpicLoot MaterialConversions.Conversions
    /// </summary>
    /// <param name="materialConversion"></param>
    /// <returns>True if added to MaterialConversions.Conversions</returns>
    [Description("serializes to json and sends to EpicLoot")][PublicAPI]
    public static void RegisterConversion(MaterialConversion materialConversion)
    {
        MaterialConversions.Remove(materialConversion);
        string data = JsonConvert.SerializeObject(materialConversion);
        object? result = API_AddMaterialConversion.Invoke(data);
        if (result is not string key) return;
        RunTimeRegistry.Register(materialConversion, key);
    }

    /// <summary>
    /// Invokes UpdateMaterialConversions with unique key and serialized MaterialConversion
    /// </summary>
    /// <param name="materialConversion"></param>
    /// <returns></returns>
    [PublicAPI]
    public static bool UpdateMaterialConversions(MaterialConversion materialConversion)
    {
        if (!RunTimeRegistry.TryGetValue(materialConversion, out var key)) return false;
        string json = JsonConvert.SerializeObject(materialConversion);
        object? result =  API_UpdateMaterialConversion.Invoke(key, json);
        return (bool)(result ?? false);
    }
    #endregion
    
    #region Assets
    /// <summary>
    /// Register asset into EpicLoot in order to target them in your definitions
    /// </summary>
    /// <param name="name"><see cref="string"/></param>
    /// <param name="asset"><see cref="Object"/></param>
    /// <returns></returns>
    [PublicAPI]
    public static bool RegisterAsset(string name, Object asset)
    {
        object? result = API_RegisterAsset.Invoke(name, asset);
        return (bool)(result ?? false);
    }
    #endregion
    
    #region Magic Effect
    /// <param name="effectType"><see cref="EffectType"/></param>
    /// <returns>simple copy of existing magic effect definition</returns>
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
            OnError?.Invoke("Failed to parse magic item effect definition json");
            return null;
        }
    }
    
    [PublicAPI]
    public static void RegisterMagicItems()
    {
        foreach (MagicItemEffectDefinition item in new List<MagicItemEffectDefinition>(MagicEffects)) 
            RegisterMagicEffect(item);
    }

    /// <summary>
    /// Serialized to JSON and invokes <see cref="API_AddMagicEffect"/>
    /// </summary>
    /// <param name="definition"><see cref="MagicItemEffectDefinition"/></param>
    /// <returns></returns>
    [PublicAPI]
    public static bool RegisterMagicEffect(MagicItemEffectDefinition definition)
    {
        MagicEffects.Remove(definition);
        string data = JsonConvert.SerializeObject(definition);
        object? result = API_AddMagicEffect.Invoke(data);
        if (result is not string key) return false;
        RunTimeRegistry.Register(definition, key);
        return true;
    }

    /// <summary>
    /// Serialized to JSON and invokes <see cref="API_UpdateMagicEffect"/>
    /// </summary>
    /// <param name="definition"><see cref="MagicItemEffectDefinition"/></param>
    /// <returns>true if updated</returns>
    [PublicAPI]
    public static bool UpdateMagicEffect(MagicItemEffectDefinition definition)
    {
        if (!RunTimeRegistry.TryGetValue(definition, out string key)) return false;
        string json = JsonConvert.SerializeObject(definition);
        var result = API_UpdateMagicEffect.Invoke(key, json);
        return (bool)(result ?? false);
    }
    
    #endregion
    
    #region States
    /// <param name="player"></param>
    /// <param name="legendaryItemID"></param>
    /// <returns>true if player has legendary item</returns>
    [PublicAPI]
    public static bool HasLegendaryItem(this Player player, string legendaryItemID)
    {
        object? result = API_HasLegendaryItem.Invoke(player, legendaryItemID);
        return (bool)(result ?? false);
    }
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
    /// ⚠️ Conditional behavior: Returns different results based on player parameter,
    /// </summary>
    /// <param name="player"></param>
    /// <param name="item"></param>
    /// <param name="effectType"><see cref="EffectType"/></param>
    /// <param name="effectValue"></param>
    /// <returns>
    /// Player null → Checks if item has effect,
    /// Player provided → Checks if player has effect 
    /// </returns>
    [PublicAPI]
    public static bool HasActiveMagicEffectOnWeapon(Player player, ItemDrop.ItemData item, string effectType, out float effectValue)
    {
        effectValue = 0f;
        object? output = API_HasActiveMagicEffectOnWeapon.Invoke(player, item, effectType, effectValue);
        
        return (bool)(output ?? false);
    }

    /// <summary>
    /// ⚠️ Conditional behavior: Returns different results based on player parameter,
    /// </summary>
    /// <param name="player"></param>
    /// <param name="item"></param>
    /// <param name="effectType"></param>
    /// <param name="scale"></param>
    /// <returns>
    /// Player null → returns item effect values,
    /// Player provided → returns player effect values
    /// </returns>
    [PublicAPI]
    public static float GetTotalActiveMagicEffectValue(Player player, ItemDrop.ItemData item, string effectType,
        float scale = 1.0f)
    {
        object? result = API_GetTotalActiveMagicEffectValue.Invoke(player, item, effectType);
        return (float)(result ?? 0f);
    }

    /// <summary>
    /// ⚠️ Conditional behavior: Returns different results based on player parameter
    /// </summary>
    /// <param name="player"></param>
    /// <param name="item"></param>
    /// <param name="effectType"></param>
    /// <param name="scale"></param>
    /// <returns>
    /// Player null → returns item effect values,
    /// Player provided → returns effect value without item
    /// </returns>
    [PublicAPI]
    public static float GetTotalActiveMagicEffectValueForWeapon(Player player, ItemDrop.ItemData item,
        string effectType, float scale = 1.0f)
    {
        object? result = API_GetTotalActiveMagicEffectValueForWeapon.Invoke(player, item, effectType, scale);
        return (float)(result ?? 0f);
    }
    
    /// <param name="player"></param>
    /// <param name="effectType"><see cref="EffectType"/></param>
    /// <param name="ignoreThisItem"></param>
    /// <param name="scale"></param>
    /// <returns>true if player has effect</returns>
    [PublicAPI]
    public static bool HasActiveMagicEffect(Player player, string effectType, ItemDrop.ItemData? ignoreThisItem = null, float scale = 1.0f)
    {
        object? result = API_HasActiveMagicEffect.Invoke(player, ignoreThisItem, effectType, scale);
        return (bool)(result ?? false);
    }

    /// <summary>
    /// ⚠️ Currently hard-coded to ModifyArmor, 
    /// </summary>
    /// <param name="player"></param>
    /// <param name="effectType"><see cref="EffectType"/></param>
    /// <param name="scale"></param>
    /// <returns>total effect on set</returns>
    [PublicAPI]
    public static float GetTotalActiveSetEffectValue(Player player, string effectType, float scale = 1.0f)
    {
        object? result = API_GetTotalActiveSetEffectValue.Invoke(player, effectType, scale);
        return (float)(result ?? 0f);
    }
    
    /// <param name="player"></param>
    /// <param name="effectType"></param>
    /// <returns>list of effects on player</returns>
    [PublicAPI]
    public static List<MagicItemEffect> GetAllActiveMagicEffects(this Player player, string? effectType = null)
    {
        object? result = API_GetAllActiveMagicEffects.Invoke(player, effectType);
        List<string> list = (List<string>)(result ?? new List<string>());
        if (list.Count <= 0) return new List<MagicItemEffect>();
        List<MagicItemEffect> output = new List<MagicItemEffect>();
        output.DeserializeList(list);
        return output;
    }
    
    /// <param name="player"></param>
    /// <param name="effectType"><see cref="EffectType"/></param>
    /// <returns>list of effects on active set</returns>
    [PublicAPI]
    public static List<MagicItemEffect> GetAllActiveSetMagicEffects(this Player player, string? effectType = null)
    {
        object? result = API_GetAllSetMagicEffects.Invoke(player, effectType);
        List<string> list = (List<string>)(result ?? new List<string>());
        if (list.Count <= 0) return new List<MagicItemEffect>();
        List<MagicItemEffect> output = new List<MagicItemEffect>();
        output.DeserializeList(list);
        return output;
    }
    /// <summary>
    /// Helper function to JSON deserialize entire list
    /// </summary>
    /// <param name="output"></param>
    /// <param name="input"></param>
    /// <typeparam name="T"></typeparam>
    private static void DeserializeList<T>(this List<T> output, List<string> input)
    {
        foreach (string? item in input)
        {
            try
            {
                T? result = JsonConvert.DeserializeObject<T>(item);
                if (result == null) continue;
                output.Add(result);
            }
            catch
            {
                OnError?.Invoke($"Failed to parse {typeof(T).Name}");
            }
        }
    }
    
    /// <param name="player"></param>
    /// <param name="effectType"><see cref="EffectType"/></param>
    /// <param name="scale"></param>
    /// <param name="ignoreThisItem"></param>
    /// <returns>total effect value on player</returns>
    [PublicAPI]
    public static float GetTotalActiveMagicEffectValue(this Player player, string effectType, float scale = 1.0f,
        ItemDrop.ItemData? ignoreThisItem = null)
    {
        object? result = API_GetPlayerTotalActiveMagicEffectValue.Invoke(player, effectType, scale, ignoreThisItem);
        return (float)(result ?? 0f);
    }
    
    /// <param name="player"></param>
    /// <param name="effectType"><see cref="EffectType"/></param>
    /// <param name="effectValue"></param>
    /// <param name="scale"></param>
    /// <param name="ignoreThisItem"></param>
    /// <returns>true if player has effect</returns>
    [PublicAPI]
    public static bool HasActiveMagicEffect(this Player player, string effectType, out float effectValue,
        float scale = 1.0f, ItemDrop.ItemData? ignoreThisItem = null)
    {
        effectValue = 0f;
        object? result = API_PlayerHasActiveMagicEffect.Invoke(player, effectType, effectValue, scale, ignoreThisItem);
        return (bool)(result ?? false);
    }
    #endregion

    #region Legendary
    /// <summary>
    /// Serializes to JSON and invokes <see cref="API_AddLegendaryItemConfig"/>
    /// </summary>
    /// <returns>true if registered to runtime registry</returns>
    [PublicAPI]
    public static bool RegisterLegendaryItems()
    {
        if (!LegendaryConfig.HasValues()) return false;
        string data = JsonConvert.SerializeObject(LegendaryConfig);
        object? result = API_AddLegendaryItemConfig.Invoke(data);
        if (result is not string key) return false;
        RunTimeRegistry.Register(LegendaryConfig, key);
        LegendaryConfig.Clear(); // clean up cache once registered, so if called multiple times, not to register same definitions
        return true;
    }

    private static void Clear(this LegendaryItemConfig config)
    {
        config.LegendaryItems.Clear();
        config.LegendarySets.Clear();
        config.MythicItems.Clear();
        config.MythicSets.Clear();
    }

    /// <summary>
    /// Serialized to JSON and invokes <see cref="API_UpdateLegendaryItem"/> with unique identifier
    /// </summary>
    /// <returns>True if fields copied</returns>
    [PublicAPI]
    public static bool UpdateLegendaryItems()
    {
        if (!RunTimeRegistry.TryGetValue(LegendaryConfig, out string key)) return false;
        string data = JsonConvert.SerializeObject(LegendaryConfig);
        object? result = API_UpdateLegendaryItem.Invoke(key, data);
        return (bool)(result ?? false);
    }
    #endregion
    
    #region Abilities
    /// <summary>
    /// Helper function, register all defined abilities
    /// </summary>
    [PublicAPI]
    public static void RegisterAbilities()
    {
        foreach (AbilityDefinition ability in new List<AbilityDefinition>(Abilities)) 
            RegisterAbility(ability);
    }

    /// <summary>
    /// Serialized to JSON and invokes <see cref="API_AddAbility"/>
    /// </summary>
    /// <param name="ability"></param>
    /// <returns>true if registered to runtime registry</returns>
    [PublicAPI]
    public static bool RegisterAbility(AbilityDefinition ability)
    {
        Abilities.Remove(ability);
        string data = JsonConvert.SerializeObject(ability);
        object? result = API_AddAbility.Invoke(data);
        if (result is not string key) return false;
        RunTimeRegistry.Register(ability, key);
        return true;
    }

    /// <summary>
    /// Serialized to JSON and invokes <see cref="API_UpdateAbility"/> with unique identifier
    /// </summary>
    /// <param name="ability"></param>
    /// <returns>True if fields copied</returns>
    [PublicAPI]
    public static bool UpdateAbility(AbilityDefinition ability)
    {
        if (!RunTimeRegistry.TryGetValue(ability, out string key)) return false;
        string data = JsonConvert.SerializeObject(ability);
        var result = API_UpdateAbility.Invoke(key, data);
        return (bool)(result ?? false);
    }

    #endregion
    
    #region Proxy Abilities

    [PublicAPI]
    public static bool RegisterProxyAbility(AbilityProxyDefinition proxyAbility)
    {
        var json = JsonConvert.SerializeObject(proxyAbility.Ability);
        var result = API_RegisterProxyAbility.Invoke(json, proxyAbility.Callbacks);
        if (result is not string key) return false;
        RunTimeRegistry.Register(proxyAbility, key);
        return true;
    }

    [PublicAPI]
    public static bool UpdateProxyAbility(AbilityProxyDefinition proxyAbility)
    {
        if (!RunTimeRegistry.TryGetValue(proxyAbility, out string key)) return false;
        string json = JsonConvert.SerializeObject(proxyAbility.Ability);
        object? result = API_UpdateProxyAbility.Invoke(key, json,  proxyAbility.Callbacks);
        return (bool)(result ?? false);
    }
    
    #endregion
    
    #region Secret Stash & Gamble
    /// <summary>
    /// Serializes to JSON and invokes <see cref="API_AddSecretStashItem"/>, if successful, registered to runtime registry
    /// </summary>
    [PublicAPI]
    public static void RegisterSecretStashes()
    {
        foreach (KeyValuePair<SecretStashType, List<SecretStashItem>> kvp in SecretStashes)
        {
            foreach (SecretStashItem stash in kvp.Value)
            {
                string json = JsonConvert.SerializeObject(stash);
                object? result = API_AddSecretStashItem.Invoke(kvp.Key.ToString(), json);
                if (result is not string key) continue;
                RunTimeRegistry.Register(stash, key);
            }
        }
        SecretStashes.Clear();
    }

    /// <summary>
    /// Serializes to JSON and invokes <see cref="API_UpdateSecretStashItem"/> with unique identifier
    /// </summary>
    /// <param name="secretStash"></param>
    /// <returns>true if updated</returns>
    [PublicAPI]
    public static bool UpdateSecretStash(SecretStashItem secretStash)
    {
        if (!RunTimeRegistry.TryGetValue(secretStash, out string key)) return false;
        string json = JsonConvert.SerializeObject(secretStash);
        object? result = API_UpdateSecretStashItem.Invoke(key, json);
        return (bool)(result ?? false);
    }
    
    #endregion
    
    #region Treasure Map

    /// <summary>
    /// Helper function, register all treasure maps
    /// </summary>
    [PublicAPI]
    public static void RegisterAllTreasureMaps()
    {
        foreach (TreasureMapBiomeInfoConfig treasure in new List<TreasureMapBiomeInfoConfig>(Treasures)) 
            RegisterTreasureMap(treasure);
    }
    
    /// <summary>
    /// Serializes treasure data into JSON and invokes <see cref="API_AddTreasureMap"/>
    /// </summary>
    /// <param name="treasure"></param>
    /// <returns>true if registered to RunTimeRegistry with a unique identifier</returns>
    [PublicAPI]
    public static bool RegisterTreasureMap(TreasureMapBiomeInfoConfig treasure)
    {
        Treasures.Remove(treasure);
        string json = JsonConvert.SerializeObject(treasure);
        var result = API_AddTreasureMap.Invoke(json);
        if (result is not string key) return false;
        RunTimeRegistry.Register(treasure, key);
        return true;
    }

    /// <summary>
    /// Serializes treasure data in JSON and invokes <see cref="API_UpdateTreasureMap"/> with its unique identifier
    /// </summary>
    /// <param name="treasure"></param>
    /// <returns>true if updated</returns>
    [PublicAPI]
    public static bool UpdateTreasureMap(TreasureMapBiomeInfoConfig treasure)
    {
        if (!RunTimeRegistry.TryGetValue(treasure, out string key)) return false;
        string json = JsonConvert.SerializeObject(treasure);
        object? result = API_UpdateTreasureMap.Invoke(key, json);
        return (bool)(result ?? false);
    }
    
    #endregion
    
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
        /// Format: "Namespace.ClassName, AssemblyName"
        /// </param>
        /// <param name="methodName">
        /// The exact name of the static method to locate within the specified type.
        /// Method names are case-sensitive and must match exactly.
        /// </param>
        /// <param name="bindingFlags"></param>
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
        /// - Method is overloaded (this class finds the first matching name)
        /// </remarks>
        public Method(string typeNameWithAssembly, string methodName, BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Static)
        {
            if (!TryGetType(typeNameWithAssembly, out Type? type)) return;
            if (type == null)
            {
                OnError?.Invoke($"Type resolution returned null for: '{typeNameWithAssembly}'");
                return;
            }
            info = type.GetMethod(methodName, bindingFlags);
            if (info == null)
            {
                OnError?.Invoke(
                    $"Failed to find public static method '{methodName}' in type '{type.FullName}'. " +
                    "Verify the method name is correct, the method exists, and it is marked as public static. ");
            }
        }

        /// <summary>
        /// Helper constructor that automatically adds type name with assembly, defined at the top of the file.
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="bindingFlags"></param>
        public Method(string methodName, BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Static) : this(
            API_LOCATION, methodName, bindingFlags)
        {
        }
        
        /// <param name="typeNameWithAssembly"><see cref="string"/></param>
        /// <param name="type"><see cref="Type"/></param>
        /// <returns></returns>
        private static bool TryGetType(string typeNameWithAssembly, out Type? type)
        {
            // Try to get cached type first for performance
            if (CachedTypes.TryGetValue(typeNameWithAssembly, out type)) return true;
            // Attempt to resolve the type from the assembly
            if (Type.GetType(typeNameWithAssembly) is not { } resolvedType)
            {
                OnError?.Invoke($"Failed to resolve type: '{typeNameWithAssembly}'. " +
                                 "Verify the namespace, class name, and assembly name are correct. " +
                                 "Ensure the assembly is loaded and accessible.");
                return false;
            }

            type = resolvedType;
            CachedTypes[typeNameWithAssembly] = resolvedType;
            return true;
        }
        
        /// <summary>
        /// Searches for the specified public method whose parameters match the types
        /// </summary>
        /// <param name="typeNameWithAssembly"><see cref="string"/></param>
        /// <param name="methodName"><see cref="string"/></param>
        /// <param name="types">params array of <see cref="Type"/></param>
        public Method(string typeNameWithAssembly, string methodName, params Type[] types)
        {
            if (!TryGetType(typeNameWithAssembly, out Type? type)) return;

            // Additional null check (defensive programming, should not happen if TryGetValue succeeded)
            if (type == null)
            {
                OnError?.Invoke($"Type resolution returned null for: '{typeNameWithAssembly}'");
                return;
            }

            // Locate the static method by name
            info = type.GetMethod(methodName, types);
            if (info == null)
            {
                OnError?.Invoke(
                    $"Failed to find public static method '{methodName}' in type '{type.FullName}'. " +
                    "Verify the method name is correct, the method exists, and it is marked as public static. ");
            }
        }

        public Method(string methodName, params Type[] types) : this(API_LOCATION, methodName, types)
        {
        }

        /// <summary>
        /// Gets the parameter information for the resolved method.
        /// Useful for validating arguments before calling Invoke().
        /// </summary>
        /// <returns>Array of ParameterInfo objects describing the method parameters, or empty array if method not resolved.</returns>
        [PublicAPI]
        public ParameterInfo[] GetParameters() => info?.GetParameters() ?? Array.Empty<ParameterInfo>();
        
        [PublicAPI]
        public static void ClearCache() => CachedTypes.Clear();
    }
}