using HarmonyLib;

namespace EpicLoot.MagicItemEffects
{
    [HarmonyPatch(typeof(Attack), nameof(Attack.GetAttackHealth))]
    [HarmonyPriority(Priority.High)]
    public class ModifyAttackHealth_Attack_GetAttackHealth_Patch
    {
        public static void Postfix(Attack __instance, float __result)
        {
            if (__instance.m_character is Player player) {
                float modifier = MagicEffectsHelper.GetTotalActiveMagicEffectValueForWeapon(
                    player, __instance.m_weapon, MagicEffectType.ModifyAttackHealthUse, 0.01f);
                float reduction = 1.0f - modifier;
                //EpicLoot.Log($"Modifying health cost HP {__result} * {reduction} = {__result * reduction}");
                __result *= reduction;
            }
        }
    }
}
