using HarmonyLib;

namespace EpicLoot.MagicItemEffects;

public class ModifyFireRate
{
    [HarmonyPatch(typeof(Attack), nameof(Attack.UpdateProjectile))]
    public static class ModifyFireRate_Attack_UpdateProjectile_Patch
    {
        private static float originalBurstValue;
        
        public static void Prefix(Attack __instance)
        {
            Player player = __instance.m_character as Player;
            if (player.HasActiveMagicEffect(MagicEffectType.ModifyFireRate, out float effect, 0.01f)) {
                originalBurstValue = __instance.m_burstInterval;
                __instance.m_burstInterval *= 1 - effect;
            }
        }
        
        public static void Postfix(Attack __instance)
        {
            Player player = __instance.m_character as Player;
            if (player.HasActiveMagicEffect(MagicEffectType.ModifyFireRate, out float _)) {
                __instance.m_burstInterval = originalBurstValue;
            }
        }
    }
}