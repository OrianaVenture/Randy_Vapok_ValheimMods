using System;
using HarmonyLib;

namespace EpicLoot.MagicItemEffects;

public class ModifyBlockForce
{
    [HarmonyPatch(typeof(ItemDrop.ItemData), nameof(ItemDrop.ItemData.GetBaseBlockPower), typeof(int))]
    public static class ModifyBlockForce_ItemData_GetDeflectionForce_Patch
    {
        public static void Postfix(ItemDrop.ItemData __instance, ref float __result)
        {
            var player = PlayerExtensions.GetPlayerWithEquippedItem(__instance);
            var totalParryMod = 0f;
            ModifyWithLowHealth.Apply(player, MagicEffectType.ModifyBlockForce,
                effect =>
                {
                    totalParryMod +=
                        MagicEffectsHelper.GetTotalActiveMagicEffectValueForWeapon(player, __instance, effect, 0.01f);
                });

            __result *= 1.0f + totalParryMod;
        }
    }
}