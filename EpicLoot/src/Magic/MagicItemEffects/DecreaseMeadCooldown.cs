using System;
using EpicLoot.MagicItemEffects;
using HarmonyLib;
using Jotunn.Managers;
using UnityEngine;

namespace EpicLoot.Magic.MagicItemEffects
{
    public class DecreaseMeadCooldown
    {
        private static bool addingModifiedStatusEffect = false;
        
        [HarmonyPatch(typeof(SEMan))]
        public static class DecreasedMeadCooldown_SEMan_AddStatusEffect_Patch
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
                
                float effectValue = player.GetTotalActiveMagicEffectValue(MagicEffectType.DecreaseMeadCooldown, 0.01f);
                if (effectValue == 0) return true;
                if (statusEffect == null) return true;
                
                StatusEffect existing = ObjectDB.instance.GetStatusEffect(statusEffect.NameHash());
                if (existing is not SE_Stats seStats) return true;
                if (!seStats.CanAdd(player)) return true;
                SE_Stats newStatusEffect = (SE_Stats)seStats.Clone();
                newStatusEffect.m_ttl *= Mathf.Clamp01(1f - effectValue);
                    
                addingModifiedStatusEffect = true;
                player.GetSEMan().AddStatusEffect(newStatusEffect, false);
                addingModifiedStatusEffect = false;
                
                return false;
            }
        }
    }
}
