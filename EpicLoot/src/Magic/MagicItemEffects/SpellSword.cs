using EpicLoot.MagicItemEffects;
using HarmonyLib;

namespace EpicLoot.src.Magic.MagicItemEffects
{

    [HarmonyPatch(typeof(Attack), nameof(Attack.GetAttackStamina))]
    public static class Attack_GetAttackStamina_Prefix_Patch
    {
        public static bool Prefix(Attack __instance, ref float __result)
        {
            if (__instance.m_character is Player player &&
                MagicEffectsHelper.HasActiveMagicEffectOnWeapon(
                    player, __instance.m_weapon, MagicEffectType.SpellSword, out float effectValue))
            {
                if (__result > 2) {
                    __result = __result / 2;
                    return false;
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Attack), nameof(Attack.GetAttackEitr))]
    public class Spellsword_Attack_GetAttackEitr_Patch
    {
        public static void Prefix(Attack __instance, ref float __state)
        {
            __state = __instance.m_attackEitr;
            if (__instance.m_character is Player player &&
                MagicEffectsHelper.HasActiveMagicEffectOnWeapon(
                    player, __instance.m_weapon, MagicEffectType.SpellSword, out float effectValue))
            {
                float base_cost = __instance.m_attackStamina;
                if (base_cost == 0f) { base_cost = 4; }
                __instance.m_attackEitr = __instance.m_attackEitr + (__instance.m_attackStamina / 2);
            }
        }

        public static void Postfix(Attack __instance, ref float __state)
        {
            __instance.m_attackEitr = __state;
        }
    }
}
