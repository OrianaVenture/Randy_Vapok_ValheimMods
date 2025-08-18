using EpicLoot.MagicItemEffects;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using EpicLoot.Abilities;
using EpicLoot.LegendarySystem;
using JetBrains.Annotations;
using UnityEngine;

namespace EpicLoot;

public static class API
{
    public static readonly Dictionary<string, MagicItemEffectDefinition> ExternalMagicItemEffectDefinitions = new();
    public static readonly Dictionary<string, AbilityDefinition> ExternalAbilities = new();
    public static readonly List<LegendaryItemConfig> ExternalLegendaryConfig = new();
    public static readonly Dictionary<string, Object> ExternalAssets = new();

    static API()
    {
        MagicItemEffectDefinitions.OnSetupMagicItemEffectDefinitions += ReloadExternalMagicEffects;
        UniqueLegendaryHelper.OnSetupLegendaryItemConfig += ReloadExternalLegendary;
        AbilityDefinitions.OnSetupAbilityDefinitions += ReloadExternalAbilities;
    }
    
    /// <summary>
    /// Reloads cached external magic effects into AllDefinitions via Add().
    /// </summary>
    public static void ReloadExternalMagicEffects()
    {
        foreach (var effect in ExternalMagicItemEffectDefinitions.Values) MagicItemEffectDefinitions.Add(effect);
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
    }

    /// <summary>
    /// Called via reflection with a serialized MagicItemEffectDefinition, 
    /// which is deserialized and added to EpicLoot.
    /// </summary>
    /// <param name="json">Serialized magic effect definition</param>
    /// <returns>True if added successfully, otherwise false</returns>
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
    public static bool HasActiveMagicEffectOnWeapon(Player player, ItemDrop.ItemData item, string effectType, ref float effectValue) => 
        MagicEffectsHelper.HasActiveMagicEffectOnWeapon(player, item, effectType, out effectValue);

    /// <summary>
    /// Called via reflection.
    /// Currenctly hard coded to Modify Armor effect type ???
    /// </summary>
    /// <param name="player"></param>
    /// <param name="effectType"></param>
    /// <param name="scale"></param>
    /// <returns></returns>
    public static float GetTotalActiveSetEffectValue(Player player, string effectType, float scale) =>
        MagicEffectsHelper.GetTotalActiveSetEffectValue(player, effectType, scale);
    
    public static List<string> GetAllActiveMagicEffects(Player player, string effectType = null)
    {
        var list = player.GetAllActiveSetMagicEffects(effectType);
        var output = new List<string>();
        foreach (var item in list)
        {
            output.Add(JsonConvert.SerializeObject(item));
        }

        return output;
    }

    public static List<string> GetAllActiveSetMagicEffects(Player player, string effectType = null)
    {
        var list = player.GetAllActiveSetMagicEffects(effectType);
        var output = new List<string>();
        foreach (var item in list)
        {
            output.Add(JsonConvert.SerializeObject(item));
        }

        return output;
    }

    public static float GetTotalPlayerActiveMagicEffectValue(Player player, string effectType, float scale,
        ItemDrop.ItemData ignoreThisItem = null) =>
        player.GetTotalActiveMagicEffectValue(effectType, scale, ignoreThisItem);

    public static bool PlayerHasActiveMagicEffect(Player player, string effectType, ref float effectValue,
        float scale = 1.0f, ItemDrop.ItemData ignoreThisItem = null) =>
        player.HasActiveMagicEffect(effectType, out effectValue, scale, ignoreThisItem);

    /// <summary>
    /// Called via reflection with a serialized LegendaryItemConfig
    /// </summary>
    /// <param name="json"></param>
    /// <returns>True if added to EpicLoot UniqueLegendaryHelper</returns>
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
            
            // needed to change dictionaries to public inside UniqueLegendaryHelper

            return true;
        }
        catch
        {
            EpicLoot.LogWarning("Failed to parse legendary item config from external plugin");
            return false;
        }
    }

    private static void AddInfo(this Dictionary<string, LegendaryInfo> target, List<LegendaryInfo> legendaryItems)
    {
        foreach (var info in legendaryItems)
        {
            if (!target.ContainsKey(info.ID)) target[info.ID] = info;
            else EpicLoot.LogWarning($"Duplicate entry found for Legendary Info: {info.ID} when adding from external plugin");
        }
    }

    private static void AddSet(Dictionary<string, LegendarySetInfo> target, Dictionary<string, LegendarySetInfo> itemToSet, List<LegendarySetInfo> legendarySet)
    {
        foreach (var info in legendarySet)
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

    public static bool HasLegendaryItem(Player player, string legendaryItemID)
    {
        foreach (var item in player.GetEquipment())
        {
            if (item.IsMagic(out var magicItem) && magicItem.LegendaryID == legendaryItemID) return true;
        }

        return false;
    }

    public static bool HasLegendarySet(Player player, string legendarySetID)
    {
        if (!UniqueLegendaryHelper.LegendarySets.TryGetValue(legendarySetID, out LegendarySetInfo info) || !UniqueLegendaryHelper.MythicSets.TryGetValue(legendarySetID, out info))
        {
            return false;
        }
        int count = 0;
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
}
