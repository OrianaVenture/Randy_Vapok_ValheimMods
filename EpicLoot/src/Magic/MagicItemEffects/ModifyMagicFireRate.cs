using HarmonyLib;

namespace EpicLoot.MagicItemEffects;

public class ModifyMagicFireRate
{
    [HarmonyPatch(typeof(Attack), nameof(Attack.UpdateProjectile))]
    public static class ModifyFireRate_Attack_UpdateProjectile_Patch
    {
        private static float originalBurstValue;
        
        public static void Prefix(Attack __instance)
        {
            if (__instance.m_character is not Player) return;
            Player player = __instance.m_character as Player;
            if (player.HasActiveMagicEffect(MagicEffectType.ModifyMagicFireRate, out float effect, 0.01f)) {
                originalBurstValue = __instance.m_burstInterval;
                __instance.m_burstInterval *= 1 - effect;
            }
        }
        
        public static void Postfix(Attack __instance)
        {
            if (__instance.m_character is not Player) return;
            Player player = __instance.m_character as Player;
            if (player.HasActiveMagicEffect(MagicEffectType.ModifyMagicFireRate, out float _)) {
                __instance.m_burstInterval = originalBurstValue;
            }
        }
    }
}