namespace EpicLoot;

public partial class MagicTooltip
{
    private void ShieldBlockAndParry()
    {
        bool hasParryModifier = magicItem.HasEffect(MagicEffectType.ModifyParry);
        float totalParryBonusMod = magicItem.GetTotalEffectValue(MagicEffectType.ModifyParry, 0.01f);
        string magicParryColor = hasParryModifier ? magicColor : "orange";
        
        if (item.m_shared.m_timedBlockBonus > 1.0)
        {
            text.Append($"\n$item_blockforce: " +
                        $"<color={magicParryColor}>{item.GetDeflectionForce(qualityLevel)}</color>");

            float timedBlockBonus = item.m_shared.m_timedBlockBonus;
            if (hasParryModifier)
            {
                timedBlockBonus *= 1.0f + totalParryBonusMod;
            }

            text.Append($"\n$item_parrybonus: <color={magicParryColor}>{timedBlockBonus:0.#}x</color>");
        }
    }
    
    
    private void BlockArmor()
    {
        bool hasMagicBlockPower = magicItem.HasEffect(MagicEffectType.ModifyBlockPower);
        string magicBlockColor1 = hasMagicBlockPower ? magicColor : "orange";
        string magicBlockColor2 = hasMagicBlockPower ? magicColor : "yellow";
        
        float baseBlockPower = item.GetBaseBlockPower(qualityLevel);
        float blockPowerTooltipValue = item.GetBlockPowerTooltip(qualityLevel);
        string blockPowerPercentageString = blockPowerTooltipValue.ToString("0");
        
        text.Append($"\n$item_blockarmor: <color={magicBlockColor1}>{baseBlockPower}</color> " +
                    $"<color={magicBlockColor2}>({blockPowerPercentageString})</color>");
    }
}