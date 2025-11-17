using HarmonyLib;
using System.Collections.Generic;

namespace EpicLoot.MagicItemEffects
{
    [HarmonyPatch(typeof(Character), nameof(Character.RPC_Damage))]
    public static class ModifyResistance_Character_RPC_Damage_Patch
    {
        public static void Prefix(Character __instance, HitData hit)
        {
            if (__instance is not Player player)
            {
                return;
            }

            float elementalResistance = GetCappedSharedResistance(player, MagicEffectType.AddElementalResistancePercentage);
            float physicalResistance = GetCappedSharedResistance(player, MagicEffectType.AddPhysicalResistancePercentage);

            //EpicLoot.Log($"Applying resistances for {player.GetPlayerName()} - Elemental: {elementalResistance}, Physical: {physicalResistance}");

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

            EpicLoot.Log($"Final damage after resistances for {player.GetPlayerName()}: {hit.m_damage}");
        }

        private static float GetCappedSharedResistance(Player player, string effect)
        {
            if (player.HasActiveMagicEffect(effect, out float value, 0.01f))
            {
                EpicLoot.Log($"{effect} active with value {value}");
                return value;
            }
            return 0f;
        }

        private static float GetCappedResistanceValue(Player player, string effect, float additional_resistance = 0f)
        {
            Dictionary<string, float> cfg = MagicItemEffectDefinitions.GetEffectConfig(effect);
            // No config for this type, default it (uncapped)
            if (cfg == null || !cfg.ContainsKey("MaxResistance")) { return (1f - player.GetTotalActiveMagicEffectValue(effect, 0.01f)); }
            // Config present, with a cap value
            float resistance = player.GetTotalActiveMagicEffectValue(effect, 0.01f) + additional_resistance;
            if (resistance == 0f) { return 1f; } // No resistance, return 100% damage

            EpicLoot.Log($"{effect} total resistance {resistance} including bonus {additional_resistance}");
            float max_resistance = (cfg["MaxResistance"] / 100f);
            if (resistance > max_resistance)
            {
                EpicLoot.Log($"Capped resistance for {effect} is {max_resistance}");
                resistance = max_resistance;
            }
            if (resistance >= 1) { EpicLoot.LogWarning($"Resistance calculated to 100%, player immune. Reduce max resistance below 100 in your configuration."); }
            float reduction = (1f - resistance);
            if (reduction < 0) { reduction = 1f; }
            // EpicLoot.Log($"Resistance for {effect}: {reduction * 100}%");
            return reduction;
        }
    }
}
