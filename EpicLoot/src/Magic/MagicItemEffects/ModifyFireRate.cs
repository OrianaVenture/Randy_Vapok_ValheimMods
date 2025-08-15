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
            
            if (player == Player.m_localPlayer)
            {
                originalBurstValue = __instance.m_burstInterval;

                float effectValue =
                    player.GetTotalActiveMagicEffectValue(MagicEffectType.ModifyFireRate, 0.01f);
                if (effectValue > 0)
                {
                    __instance.m_burstInterval *= 1 - effectValue;
                }
            }
        }
        
        public static void Postfix(Attack __instance)
        {
            Player player = __instance.m_character as Player;
            
            if (player == Player.m_localPlayer)
            {
                __instance.m_burstInterval = originalBurstValue;
            }
        }
    }
}