using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace EpicLoot.MagicItemEffects
{
    [HarmonyPatch(typeof(Player), nameof(Player.ApplyArmorDamageMods))]
    public static class ModifyResistance_Player_ApplyArmorDamageMods_Patch
    {
        public static void Postfix(Player __instance, ref HitData.DamageModifiers mods)
        {
            var damageMods = new List<HitData.DamageModPair>();

            if (__instance.HasActiveMagicEffect(MagicEffectType.AddFireResistance, out float _)) {
                damageMods.Add(new HitData.DamageModPair() { m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Resistant});
            }
            if (__instance.HasActiveMagicEffect(MagicEffectType.AddFrostResistance, out float _)) {
                damageMods.Add(new HitData.DamageModPair() { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Resistant });
            }
            if (__instance.HasActiveMagicEffect(MagicEffectType.AddLightningResistance, out float _)) {
                damageMods.Add(new HitData.DamageModPair() { m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Resistant });
            }
            if (__instance.HasActiveMagicEffect(MagicEffectType.AddPoisonResistance, out float _)) {
                damageMods.Add(new HitData.DamageModPair() { m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.Resistant });
            }
            if (__instance.HasActiveMagicEffect(MagicEffectType.AddSpiritResistance, out float _)) {
                damageMods.Add(new HitData.DamageModPair() { m_type = HitData.DamageType.Spirit, m_modifier = HitData.DamageModifier.Resistant });
            }

            mods.Apply(damageMods);
        }
    }

    [HarmonyPatch(typeof(Character), nameof(Character.RPC_Damage))]
    public static class ModifyResistance_Character_RPC_Damage_Patch
    {
        public static void Prefix(Character __instance, HitData hit)
        {
            if (__instance is not Player player) {
                return;
            }
            
            float elementalResistance = player.GetTotalActiveMagicEffectValue(MagicEffectType.AddElementalResistancePercentage, 0.01f);
            
            float physicalResistance = player.GetTotalActiveMagicEffectValue(MagicEffectType.AddPhysicalResistancePercentage, 0.01f);

            // elemental resistances
            hit.m_damage.m_fire *= GetCappedResistanceValue(player, MagicEffectType.AddFireResistancePercentage, elementalResistance);
            hit.m_damage.m_frost *= GetCappedResistanceValue(player, MagicEffectType.AddFrostResistancePercentage, elementalResistance);
            hit.m_damage.m_lightning *= GetCappedResistanceValue(player, MagicEffectType.AddLightningResistancePercentage, elementalResistance);
            hit.m_damage.m_poison *= GetCappedResistanceValue(player, MagicEffectType.AddPoisonResistancePercentage, elementalResistance);
            hit.m_damage.m_spirit *= GetCappedResistanceValue(player, MagicEffectType.AddSpiritResistancePercentage, elementalResistance);
            
            // physical resistances
            hit.m_damage.m_blunt *= GetCappedResistanceValue(player, MagicEffectType.AddBluntResistancePercentage, physicalResistance);
            hit.m_damage.m_slash *= GetCappedResistanceValue(player, MagicEffectType.AddSlashingResistancePercentage, physicalResistance);
            hit.m_damage.m_pierce *= GetCappedResistanceValue(player, MagicEffectType.AddPiercingResistancePercentage, physicalResistance);
            hit.m_damage.m_chop *= GetCappedResistanceValue(player, MagicEffectType.AddChoppingResistancePercentage, physicalResistance);
        }

        private static float GetCappedResistanceValue(Player player, string effect, float additional_resistance = 0f) {
            Dictionary<string, float> cfg = MagicItemEffectDefinitions.GetEffectConfig(effect);
            // No config for this type, default it (uncapped)
            if (cfg == null || !cfg.ContainsKey("MaxResistance")) {  return player.GetTotalActiveMagicEffectValue(effect, 0.01f); }

            return Mathf.Min(player.GetTotalActiveMagicEffectValue(effect, 0.01f) + additional_resistance, cfg["MaxResistance"]);
        }
    }
}
