using HarmonyLib;

namespace EpicLoot.MagicItemEffects
{
    // public float GetMaxDurability(int quality) =>
    //   this.m_shared.m_maxDurability + (float) Mathf.Max(0, quality - 1) * this.m_shared.m_durabilityPerLevel;
    [HarmonyPatch(typeof(ItemDrop.ItemData), nameof(ItemDrop.ItemData.GetDrawStaminaDrain))]
    public static class ModifyDrawStamina_ItemData_GetDrawStaminaDrain_Patch
    {
        public static void Postfix(ItemDrop.ItemData __instance, ref float __result)
        {
            if (__instance.IsMagic(out var magicItem) && magicItem.HasEffect(MagicEffectType.ModifyDrawStaminaUse))
            {
                float modifier = magicItem.GetTotalEffectValue(MagicEffectType.ModifyDrawStaminaUse, 0.01f);
                float skillFactor = Player.m_localPlayer.GetSkillFactor(__instance.m_shared.m_skillType);

                float newDrawStaminaDrain = __instance.m_shared.m_attack.m_drawStaminaDrain * (1 - modifier);
                __result = newDrawStaminaDrain - newDrawStaminaDrain * 0.33f * skillFactor;
            }
        }
    }
}