using EpicLoot.MagicItemEffects;
using HarmonyLib;

namespace EpicLoot.Magic.MagicItemEffects
{
    public static class InstantMeads
    {
        [HarmonyPatch(typeof(SEMan), nameof(SEMan.AddStatusEffect), 
            typeof(StatusEffect), typeof(bool), typeof(int), typeof(float))]
        public static class InstantMeads_SEMan_AddStatusEffect
        {
            private static bool Prefix(SEMan __instance, StatusEffect statusEffect, int itemLevel, float skillLevel, ref StatusEffect __result)
            {
                if (statusEffect == null) return true;
                if (!DecreaseMeadCooldown.IsMead(statusEffect)) return true;
                
                if (__instance.m_character is not Player player) return true;
                if (player != Player.m_localPlayer) return true;
                
                if (!statusEffect.CanAdd(__instance.m_character)) return true;
                
                if (!player.HasActiveMagicEffect(MagicEffectType.InstantMead)) return true;
                if (!ModifyWithLowHealth.PlayerHasLowHealth(player)) return true;
                
                if (statusEffect is not SE_Stats seStats) return true;
                if (!CheckStatusEffectFields(seStats)) return true;
                
                float healthOT = seStats.m_healthOverTime;
                float stamOT   = seStats.m_staminaOverTime;
                float eitrOT   = seStats.m_eitrOverTime;
                
                if (healthOT > 0f) player.Heal(healthOT);
                if (stamOT   > 0f) player.AddStamina(stamOT);
                if (eitrOT   > 0f) player.AddEitr(eitrOT);

                SE_Stats newStatus = (SE_Stats)seStats.Clone();
                newStatus.m_healthOverTime = 0f;
                newStatus.m_staminaOverTime = 0f;
                newStatus.m_eitrOverTime = 0f;

                __instance.m_statusEffects.Add(newStatus);
                __instance.m_statusEffectsHashSet.Add(statusEffect.NameHash());
                newStatus.Setup(__instance.m_character);
                newStatus.SetLevel(itemLevel, skillLevel);
                Gogan.LogEvent("Game", "StatusEffect", statusEffect.name, 0L);
                __result = newStatus;
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
