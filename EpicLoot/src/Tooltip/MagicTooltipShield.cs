namespace EpicLoot;

public partial class MagicTooltip
{
    private void AddBlockAndParry()
    {
        if (item.m_shared.m_timedBlockBonus > 1.0)
        {
            bool hasParryModifier = magicItem.HasEffect(MagicEffectType.ModifyParry);
            string magicParryColor = hasParryModifier ? magicColor : "orange";

            float totalParryBonusMod = magicItem.GetTotalEffectValue(MagicEffectType.ModifyParry, 0.01f);
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
    
    
    private void AddBlockArmor()
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

    private void AddParryAdrenaline()
    {
        if (item.m_shared.m_perfectBlockAdrenaline > 0.0)
        {
            text.Append($"\n$item_parryadrenaline: <color=orange>{item.m_shared.m_perfectBlockAdrenaline}</color>");
        }
    }
}