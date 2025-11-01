using HarmonyLib;

namespace EpicLoot.MagicItemEffects
{
    [HarmonyPatch(typeof(Attack), nameof(Attack.GetAttackHealth))]
    public class ModifyAttackHealth_Attack_GetAttackHealth_Patch
    {
        public static void Prefix(Attack __instance, ref float __state)
        {
            if (__instance.m_character is Player player)
            {
                __state = __instance.m_attackHealthPercentage;
                float modifier = MagicEffectsHelper.GetTotalActiveMagicEffectValueForWeapon(
                    player, __instance.m_weapon, MagicEffectType.ModifyAttackHealthUse, 0.01f);
                __instance.m_attackHealthPercentage *= 1.0f - modifier;
            }
        }

        public static void Postfix(Attack __instance, ref float __state)
        {
            __instance.m_attackHealthPercentage = __state;
        }
    }
}
