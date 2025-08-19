using EpicLoot.Abilities;
using EpicLoot.LegendarySystem;
using EpicLoot.MagicItemEffects;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace EpicLoot
{
    public static class API
    {
        /// <summary>
        /// This section is inside EpicLoot
        /// API uses reflection to find the functions below and call them
        /// </summary>
        /// TODO: make docs for all API calls and examples
        public static bool AddMagicEffect(string json) {
            var def = JsonConvert.DeserializeObject<MagicItemEffectDefinition>(json);
            return AddMagicEffect(def);
        }

        public static bool AddMagicEffect(MagicItemEffectDefinition magicEffect) {
            try{
                MagicItemEffectDefinitions.Add(magicEffect);
                return true;
            } catch {
                EpicLoot.LogWarning("Failed to parse magic effect from external plugin");
                return false;
            }
        }

        public static string GetMagicItemEffectDefinition(string type, ref string result)
        {
            if (!MagicItemEffectDefinitions.AllDefinitions.TryGetValue(type, out MagicItemEffectDefinition definition)) return "";
            return JsonConvert.SerializeObject(definition);
        }

        public static float GetTotalActiveMagicEffectValue(Player player, ItemDrop.ItemData item, string effectType, float scale) =>
            MagicEffectsHelper.GetTotalActiveMagicEffectValue(player, item, effectType, scale);

        public static float GetTotalActiveMagicEffectValueForWeapon(Player player, ItemDrop.ItemData item, string effectType, float scale) =>
            MagicEffectsHelper.GetTotalActiveMagicEffectValueForWeapon(player, item, effectType, scale);

        public static bool HasActiveMagicEffect(Player player, ItemDrop.ItemData item, string effectType, ref float effectValue) =>
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
                UniqueLegendaryHelper.Config.LegendaryItems.AddRange(config.LegendaryItems);
                UniqueLegendaryHelper.Config.LegendarySets.AddRange(config.LegendarySets);
                UniqueLegendaryHelper.Config.MythicItems.AddRange(config.MythicItems);
                UniqueLegendaryHelper.Config.MythicSets.AddRange(config.MythicSets);

                AddInfo(UniqueLegendaryHelper.LegendaryInfo, config.LegendaryItems);
                AddInfo(UniqueLegendaryHelper.MythicInfo, config.MythicItems);
                AddSet(UniqueLegendaryHelper.LegendarySets, config.LegendarySets);
                AddSet(UniqueLegendaryHelper.LegendarySets, config.LegendarySets);

                return true;
            }
            catch
            {
                EpicLoot.LogWarning("Failed to parse legendary item config from external plugin");
                return false;
            }

            void AddInfo(Dictionary<string, LegendaryInfo> target, List<LegendaryInfo> legendaryItems)
            {
                foreach (var info in legendaryItems)
                {
                    if (!target.ContainsKey(info.ID)) target[info.ID] = info;
                    else EpicLoot.LogWarning($"Duplicate entry found for Legendary Info: {info.ID} when adding from external plugin");
                }
            }

            void AddSet(Dictionary<string, LegendarySetInfo> target, List<LegendarySetInfo> legendarySet)
            {
                foreach (var info in legendarySet)
                {
                    if (!target.ContainsKey(info.ID)) target[info.ID] = info;
                    else EpicLoot.LogWarning($"Duplicate entry found for Legendary Set: {info.ID} when adding from external plugin");
                }
            }
        }

        public static bool AddAbility(string json)
        {
            try
            {
                var def = JsonConvert.DeserializeObject<AbilityDefinition>(json);
                AbilityDefinitions.Config.Abilities.Add(def);
                if (AbilityDefinitions.Abilities.ContainsKey(def.ID))
                {
                    EpicLoot.LogWarning($"Duplicate entry found for Abilities: {def.ID} when adding from external plugin.");
                    return false;
                }
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
    }
}
