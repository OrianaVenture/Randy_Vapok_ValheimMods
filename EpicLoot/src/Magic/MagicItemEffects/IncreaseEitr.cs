using HarmonyLib;

namespace EpicLoot.MagicItemEffects
{
    [HarmonyPatch(typeof(Player), nameof(Player.GetTotalFoodValue))]
    public static class IncreaseEitr_Player_GetTotalFoodValue_Patch
    {
        public static void Postfix(Player __instance, ref float eitr)
        {
            eitr += __instance.GetTotalActiveMagicEffectValue(MagicEffectType.IncreaseEitr);
        }
    }
}