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
    /// <summary>
    /// This section is inside EpicLoot
    /// API uses reflection to find the functions below and call them
    /// Add to main EpicLoot folder, next to EpicLoot.cs
    /// </summary>
    public static readonly Dictionary<string, MagicItemEffectDefinition> ExternalMagicItemEffectDefinitions = new();
    public static readonly Dictionary<string, AbilityDefinition> ExternalAbilities = new();
    public static readonly List<LegendaryItemConfig> ExternalLegendaryConfig = new();
    public static readonly Dictionary<string, Object> ExternalAssets = new();

    /// <summary>
    /// Since when configs reload, it clears dictionaries / lists
    /// We need to reload external assets
    /// </summary>
    [Description("Call whenever configs change in order to reload external assets")]
    public static void OnConfigChange()
    {
        // Reload external magic effects
        foreach (var effect in ExternalMagicItemEffectDefinitions.Values)
        {
            MagicItemEffectDefinitions.Add(effect);
        }
        
        // Reload external abilities
        foreach (var kvp in ExternalAbilities)
        {
            AbilityDefinitions.Config.Abilities.Add(kvp.Value);
            AbilityDefinitions.Abilities[kvp.Key] = kvp.Value;
        }
        
        // Reload external legendary items and sets
        foreach (var config in ExternalLegendaryConfig)
        {
            UniqueLegendaryHelper.LegendaryInfo.AddInfo(config.LegendaryItems);
            UniqueLegendaryHelper.MythicInfo.AddInfo(config.MythicItems);
            AddSet(UniqueLegendaryHelper.LegendarySets, UniqueLegendaryHelper._legendaryItemsToSetMap, config.LegendarySets);
            AddSet(UniqueLegendaryHelper.MythicSets, UniqueLegendaryHelper._mythicItemsToSetMap, config.MythicSets);
        }

        // Reload external assets // Not sure if this needed, doubt configs would change stored asset bundle assets
        
        // foreach (var kvp in ExternalAssets)
        // {
        //     EpicLoot._assetCache[kvp.Key] = kvp.Value;
        // }
    }
    
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

    public static string GetMagicItemEffectDefinition(string type, ref string result)
    {
        if (!MagicItemEffectDefinitions.AllDefinitions.TryGetValue(type, out MagicItemEffectDefinition definition)) return "";
        return JsonConvert.SerializeObject(definition);
    }

    public static float GetTotalActiveMagicEffectValue([CanBeNull] Player player, [CanBeNull] ItemDrop.ItemData item, string effectType, float scale) => 
        MagicEffectsHelper.GetTotalActiveMagicEffectValue(player, item, effectType, scale);

    public static float GetTotalActiveMagicEffectValueForWeapon([CanBeNull] Player player, [CanBeNull] ItemDrop.ItemData item, string effectType, float scale) =>
        MagicEffectsHelper.GetTotalActiveMagicEffectValueForWeapon(player, item, effectType, scale);

    public static bool HasActiveMagicEffect([CanBeNull] Player player, string effectType, [CanBeNull] ItemDrop.ItemData item, ref float effectValue) =>
        MagicEffectsHelper.HasActiveMagicEffect(player, item, effectType, out effectValue);
    
    public static bool HasActiveMagicEffectOnWeapon(Player player, ItemDrop.ItemData item, string effectType, ref float effectValue) => 
        MagicEffectsHelper.HasActiveMagicEffectOnWeapon(player, item, effectType, out effectValue);

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
