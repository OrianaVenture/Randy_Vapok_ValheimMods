using HarmonyLib;

namespace EpicLoot.MagicItemEffects
{
    [HarmonyPatch(typeof(Attack), nameof(Attack.GetAttackEitr))]
    public class ModifyAttackEitr_Attack_GetAttackEitr_Patch
    {
        public static void Postfix(Attack __instance, ref float __result)
        {
            if (__instance.m_character is Player player)
            {
                __result *= 1 - MagicEffectsHelper.GetTotalActiveMagicEffectValueForWeapon(
                    player, __instance.m_weapon, MagicEffectType.ModifyAttackEitrUse, 0.01f);
            }
        }
    }
}
