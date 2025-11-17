using System;
using HarmonyLib;

namespace EpicLoot.MagicItemEffects;

public class ModifyBlockForce
{
    [HarmonyPatch(typeof(ItemDrop.ItemData), nameof(ItemDrop.ItemData.GetDeflectionForce), typeof(int))]
    public static class ModifyBlockForce_ItemData_GetDeflectionForce_Patch
    {
        public static void Postfix(ItemDrop.ItemData __instance, ref float __result)
        {
            if (Player.m_localPlayer.HasActiveMagicEffect(MagicEffectType.ModifyBlockForce, out float effect, 0.01f)) {
                __result *= 1.0f + effect;
                EpicLoot.Log($"Increased deflection force 1+{effect} = {__result}");
            }
        }
    }
}