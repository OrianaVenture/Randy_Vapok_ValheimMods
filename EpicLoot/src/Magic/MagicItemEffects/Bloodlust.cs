using HarmonyLib;

namespace EpicLoot.MagicItemEffects
{
    [HarmonyPatch(typeof(Attack), nameof(Attack.GetAttackStamina))]
    public static class Attack_GetAttackStamina_Prefix_Patch_Bloodlust
    {
        public static bool Prefix(Attack __instance, ref float __result)
        {
            if (__instance.m_character is Player player &&
                MagicEffectsHelper.HasActiveMagicEffectOnWeapon(
                    player, __instance.m_weapon, MagicEffectType.Bloodlust, out float effectValue))
            {
                __result = 0f;
                return false;
            }

            return true;
        }
    }

    [HarmonyPriority(Priority.HigherThanNormal)]
    [HarmonyPatch(typeof(Attack), nameof(Attack.GetAttackHealth))]
    public class Bloodlust_Attack_GetAttackHealth_Patch
    {
        public static void Prefix(Attack __instance, ref float __state)
        {
            __state = __instance.m_attackHealth;

            if (__instance.m_character is Player player &&
                MagicEffectsHelper.HasActiveMagicEffectOnWeapon(
                    player, __instance.m_weapon, MagicEffectType.Bloodlust, out float effectValue))
            {
                __instance.m_attackHealth = __instance.m_attackStamina;
            }
        }
    }
}
