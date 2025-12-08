using UnityEngine;

namespace EpicLoot;

public partial class MagicTooltip
{
    public static string DamageRange(float damage, float minFactor, float maxFactor,
        bool magic = false, string magicColor = "")
    {
        int num1 = Mathf.RoundToInt(damage * minFactor);
        int num2 = Mathf.RoundToInt(damage * maxFactor);
        string color1 = magic ? magicColor : "orange";
        string color2 = magic ? magicColor : "yellow";
        return $"<color={color1}>{Mathf.RoundToInt(damage)}</color> " +
               $"<color={color2}>({num1}-{num2}) </color>";
    }
    
    public static string GetEitrRegenModifier(ItemDrop.ItemData item, MagicItem magicItem, out bool magicEitrRegen)
    {
        magicEitrRegen = magicItem?.HasEffect(MagicEffectType.ModifyEitrRegen) ?? false;
        float itemEitrRegenModifier = item.m_shared.m_eitrRegenModifier * 100f;
        if (magicEitrRegen && magicItem != null)
        {
            itemEitrRegenModifier += magicItem.GetTotalEffectValue(MagicEffectType.ModifyEitrRegen);
        }

        return (itemEitrRegenModifier == 0) ? "0%" : $"{itemEitrRegenModifier:+0;-0}%";
    }
    
    public static string GetMovementModifier(ItemDrop.ItemData item, MagicItem magicItem,
        out bool hasMovementModifiers, out bool hasRemoveSpeedPenaltyModifier)
    {
        hasMovementModifiers = magicItem.HasEffect(MagicEffectType.ModifyMovementSpeed);
        hasRemoveSpeedPenaltyModifier = magicItem.HasEffect(MagicEffectType.RemoveSpeedPenalty);

        float baseMovementModifierPercentage = hasRemoveSpeedPenaltyModifier ? 0 : item.m_shared.m_movementModifier * 100f;
        if (hasMovementModifiers)
        {
            baseMovementModifierPercentage += magicItem.GetTotalEffectValue(MagicEffectType.ModifyMovementSpeed);
        }

        return (baseMovementModifierPercentage == 0) ? "0%" : $"{baseMovementModifierPercentage:+0;-0}%";
    }
}