using HarmonyLib;
using System;
using System.Collections.Generic;

namespace EpicLoot.MagicItemEffects
{
    [HarmonyPatch(typeof(Player), nameof(Player.ApplyArmorDamageMods))]
    public static class ModifyResistance_Player_ApplyArmorDamageMods_Patch
    {
        public static void Postfix(Player __instance, ref HitData.DamageModifiers mods)
        {
            var damageMods = new List<HitData.DamageModPair>();

            if (__instance.HasActiveMagicEffect(MagicEffectType.AddFireResistance, out float fireResistanceEffectValue))
            {
                damageMods.Add(new HitData.DamageModPair() { m_type = HitData.DamageType.Fire, m_modifier = HitData.DamageModifier.Resistant});
            }
            if (__instance.HasActiveMagicEffect(MagicEffectType.AddFrostResistance, out float frostResistanceEffectValue))
            {
                damageMods.Add(new HitData.DamageModPair() { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Resistant });
            }
            if (__instance.HasActiveMagicEffect(MagicEffectType.AddLightningResistance, out float lightningResistanceEffectValue))
            {
                damageMods.Add(new HitData.DamageModPair() { m_type = HitData.DamageType.Lightning, m_modifier = HitData.DamageModifier.Resistant });
            }
            if (__instance.HasActiveMagicEffect(MagicEffectType.AddPoisonResistance, out float poisonResistanceEffectValue))
            {
                damageMods.Add(new HitData.DamageModPair() { m_type = HitData.DamageType.Poison, m_modifier = HitData.DamageModifier.Resistant });
            }
            if (__instance.HasActiveMagicEffect(MagicEffectType.AddSpiritResistance, out float spiritResistanceEffectValue))
            {
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
            if (__instance is not Player player)
            {
                return;
            }

            float sum(string effect, float additional)
            {
                float value = 1;
                value -= player.GetTotalActiveMagicEffectValue(effect, 0.01f) + additional;

                return Math.Max(value, 0);
            }
            
            float elementalResistance = player.GetTotalActiveMagicEffectValue(MagicEffectType.AddElementalResistancePercentage, 0.01f);
            float physicalResistance = player.GetTotalActiveMagicEffectValue(MagicEffectType.AddPhysicalResistancePercentage, 0.01f);

            // elemental resistances
            hit.m_damage.m_fire *= sum(MagicEffectType.AddFireResistancePercentage, elementalResistance);
            hit.m_damage.m_frost *= sum(MagicEffectType.AddFrostResistancePercentage, elementalResistance);
            hit.m_damage.m_lightning *= sum(MagicEffectType.AddLightningResistancePercentage, elementalResistance);
            hit.m_damage.m_poison *= sum(MagicEffectType.AddPoisonResistancePercentage, elementalResistance);
            hit.m_damage.m_spirit *= sum(MagicEffectType.AddSpiritResistancePercentage, elementalResistance);
            
            // physical resistances
            hit.m_damage.m_blunt *= sum(MagicEffectType.AddBluntResistancePercentage, physicalResistance);
            hit.m_damage.m_slash *= sum(MagicEffectType.AddSlashingResistancePercentage, physicalResistance);
            hit.m_damage.m_pierce *= sum(MagicEffectType.AddPiercingResistancePercentage, physicalResistance);
            hit.m_damage.m_chop *= sum(MagicEffectType.AddChoppingResistancePercentage, physicalResistance);
        }
    }
}
