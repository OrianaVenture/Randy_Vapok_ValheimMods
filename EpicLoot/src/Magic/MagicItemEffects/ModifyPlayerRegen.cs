using HarmonyLib;

namespace EpicLoot.MagicItemEffects;

public static class ModifyPlayerRegen
{
    [HarmonyPatch]
    private static class SEMan_ModifyRegen_Patches
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(SEMan), nameof(SEMan.ModifyHealthRegen))]
        private static void ModifyHealthRegen_Postfix(SEMan __instance, ref float __result)
        {
            DoPostfix(__instance, MagicEffectType.ModifyHealthRegen, ref __result);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SEMan), nameof(SEMan.ModifyStaminaRegen))]
        private static void ModifyStaminaRegen_Postfix(SEMan __instance, ref float __result)
        {
            DoPostfix(__instance, MagicEffectType.ModifyStaminaRegen, ref __result);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SEMan), nameof(SEMan.ModifyEitrRegen))]
        private static void ModifyEitrRegen_Postfix(SEMan __instance, ref float __result)
        {
            DoPostfix(__instance, MagicEffectType.ModifyEitrRegen, ref __result);
        }
    }

    private static void DoPostfix(SEMan __instance, string magicEffect, ref float __result)
    {
        if (__instance.m_character != Player.m_localPlayer)
        {
            return;
        }

        __result += GetModifyRegenValue(Player.m_localPlayer, magicEffect);
    }

    public static float GetModifyRegenValue(Player player, string magicEffect)
    {
        var regenValue = 0f;
        ModifyWithLowHealth.Apply(player, magicEffect, effect =>
        {
            regenValue = player.GetTotalActiveMagicEffectValue(effect, 0.01f);
        });

        return regenValue;
    }

    /// <summary>
    /// Helper function primarily for the tooltip.
    /// </summary>
    public static float GetModifiedRegenValue(ItemDrop.ItemData item, string magicEffect, float originalValue)
    {
        if (item.HasMagicEffect(magicEffect))
        {
            return originalValue + GetModifyRegenValue(Player.m_localPlayer, magicEffect);
        }

        return originalValue;
    }
}