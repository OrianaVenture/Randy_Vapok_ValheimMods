using EpicLoot.General;
using HarmonyLib;
using System;

namespace EpicLoot.MagicItemEffects
{
    [HarmonyPatch(typeof(ItemDrop.ItemData), nameof(ItemDrop.ItemData.GetBaseBlockPower), typeof(int))]
    public static class ModifyBlockPower_ItemData_GetBaseBlockPower_Patch
    {
        public static void Postfix(ItemDrop.ItemData __instance, ref float __result)
        {
            var player = PlayerExtensions.GetPlayerWithEquippedItem(__instance);
            var totalBlockPowerMod = 0f;
            ModifyWithLowHealth.Apply(player, MagicEffectType.ModifyBlockPower, effect =>
            {
                totalBlockPowerMod += MagicEffectsHelper.GetTotalActiveMagicEffectValue(player, __instance, effect, 0.01f);
            });

            __result *= 1.0f + totalBlockPowerMod;

            if (player != null && player.m_leftItem == null &&
                player.HasActiveMagicEffect(MagicEffectType.Duelist, out float effectValue, 0.01f))
            {
                __result += __instance.GetDamage().EpicLootGetTotalDamage() * effectValue;
            }

            __result = (float)Math.Round(__result, 1);
        }
    }
}