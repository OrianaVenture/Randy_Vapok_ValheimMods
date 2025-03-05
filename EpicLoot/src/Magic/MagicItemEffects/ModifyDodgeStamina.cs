using HarmonyLib;

namespace EpicLoot.MagicItemEffects
{
    [HarmonyPatch(typeof(Player), nameof(Player.UpdateDodge))]
    public static class ModifyDodgeStamina_Player_UpdateDodge_Patch
    {
        public static void Prefix(Player __instance, ref float __state)
        {
            // Store the original dodge stamina usage
            __state = __instance.m_dodgeStaminaUsage;
            
            if (__instance.IsPlayer())
            {
                float modifier = __instance.GetTotalActiveMagicEffectValue(MagicEffectType.ModifyDodgeStaminaUse, 0.01f);
                
                __instance.m_dodgeStaminaUsage *= 1.0f - modifier;
            }
        }
        
        public static void Postfix(Player __instance, float __state)
        {
            // Restore the original dodge stamina usage after the method is executed
            __instance.m_dodgeStaminaUsage = __state;
        }
    }
}