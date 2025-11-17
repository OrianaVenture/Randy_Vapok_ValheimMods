using System;
using EpicLoot.MagicItemEffects;
using HarmonyLib;
using Jotunn.Managers;
using UnityEngine;

namespace EpicLoot.Magic.MagicItemEffects
{
    public class InstantMeads
    {
        private static bool addingModifiedStatusEffect = false;
        
        [HarmonyPatch(typeof(SEMan))]
        public static class InstantMeads_SEMan_AddStatusEffect_Patch
        {
            [HarmonyPatch(nameof(SEMan.AddStatusEffect), new Type[] {
                typeof(StatusEffect), typeof(bool), typeof(int), typeof(float)
            })]
            [HarmonyPrefix]
            public static bool Prefix(SEMan __instance, ref StatusEffect statusEffect, bool resetTime, int itemLevel, float skillLevel)
            {
                if (addingModifiedStatusEffect) return true;
                var player = __instance.m_character as Player;
                if (player == null || player != Player.m_localPlayer) return true;
                if (!player.HasActiveMagicEffect(MagicEffectType.InstantMead)) return true;
                if (!ModifyWithLowHealth.PlayerHasLowHealth(player)) return true;
                if (statusEffect == null) return true;

                
                StatusEffect existing = ObjectDB.instance.GetStatusEffect(statusEffect.NameHash());
                
                if (existing is not SE_Stats seStats) return true;
                if (!seStats.CanAdd(player)) return true;
                if (!CheckStatusEffectFields(seStats)) return true;

                SE_Stats newStatusEffect = (SE_Stats)seStats.Clone();
                
                float healthOT = newStatusEffect.m_healthOverTime;
                float stamOT   = newStatusEffect.m_staminaOverTime;
                float eitrOT   = newStatusEffect.m_eitrOverTime;
                
                newStatusEffect.m_healthOverTime = 0f;
                newStatusEffect.m_staminaOverTime = 0f;
                newStatusEffect.m_eitrOverTime = 0f;

                addingModifiedStatusEffect = true;
                player.GetSEMan().AddStatusEffect(newStatusEffect, false);
                addingModifiedStatusEffect = false;
                
                if (healthOT > 0f) player.Heal(healthOT);
                if (stamOT   > 0f) player.AddStamina(stamOT);
                if (eitrOT   > 0f) player.AddEitr(eitrOT);
                
                return false;
            }
        }

        public static bool CheckStatusEffectFields(SE_Stats se)
        {
            bool hasHealth = se.m_healthOverTime > 0f;
            bool hasStamina = se.m_staminaOverTime > 0f;
            bool hasEitr = se.m_eitrOverTime > 0f;
            return hasHealth || hasStamina || hasEitr;
        }
    }
}
