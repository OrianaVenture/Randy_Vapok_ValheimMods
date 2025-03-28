using HarmonyLib;

namespace EpicLoot.MagicItemEffects
{
    [HarmonyPatch(typeof(Player), nameof(Player.GetTotalFoodValue))]
    public static class IncreaseStamina_Player_GetTotalFoodValue_Patch
    {
        public static void Postfix(Player __instance, ref float stamina)
        {
            stamina += __instance.GetTotalActiveMagicEffectValue(MagicEffectType.IncreaseStamina);
        }
    }
}