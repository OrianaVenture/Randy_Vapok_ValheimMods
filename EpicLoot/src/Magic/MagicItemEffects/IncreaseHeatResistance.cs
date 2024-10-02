using HarmonyLib;

namespace EpicLoot.MagicItemEffects;

public class IncreaseHeatResistance
{
    [HarmonyPatch(typeof(Player), nameof(Player.GetEquipmentHeatResistanceModifier))]
    public static class IncreaseHeatResistance_Player_GetEquipmentHeatResistanceModifier_Patch
    {
        public static void Postfix(Player __instance, ref float __result)
        {
            if (__instance == null)
            {
                return;
            }
            __result += __instance.GetTotalActiveMagicEffectValue(MagicEffectType.IncreaseHeatResistance, 0.01f);
        }
    }
    
}