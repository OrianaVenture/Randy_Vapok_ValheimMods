using System;
using System.Text;

namespace EpicLoot;

public static class AugaTooltipPreprocessor
{
    public static Tuple<string, string> PreprocessTooltipStat(ItemDrop.ItemData item, string label, string value)
    {
        Player localPlayer = Player.m_localPlayer;

        if (item.IsMagic(out MagicItem magicItem))
        {
            string magicColor = magicItem.GetColorString();

            bool allMagic = magicItem.HasEffect(MagicEffectType.ModifyDamage);
            bool physMagic = magicItem.HasEffect(MagicEffectType.ModifyPhysicalDamage);
            bool elemMagic = magicItem.HasEffect(MagicEffectType.ModifyElementalDamage);
            bool bluntMagic = magicItem.HasEffect(MagicEffectType.AddBluntDamage);
            bool slashMagic = magicItem.HasEffect(MagicEffectType.AddSlashingDamage);
            bool pierceMagic = magicItem.HasEffect(MagicEffectType.AddPiercingDamage);
            bool fireMagic = magicItem.HasEffect(MagicEffectType.AddFireDamage);
            bool frostMagic = magicItem.HasEffect(MagicEffectType.AddFrostDamage);
            bool lightningMagic = magicItem.HasEffect(MagicEffectType.AddLightningDamage);
            bool poisonMagic = magicItem.HasEffect(MagicEffectType.AddPoisonDamage);
            bool spiritMagic = magicItem.HasEffect(MagicEffectType.AddSpiritDamage);
            switch (label)
            {
                case "$item_durability":
                    if (magicItem.HasEffect(MagicEffectType.Indestructible))
                    {
                        value = $"<color={magicColor}>Indestructible</color>";
                    }
                    else if (magicItem.HasEffect(MagicEffectType.ModifyDurability))
                    {
                        value = $"<color={magicColor}>{value}</color>";
                    }
                    break;

                case "$item_weight":
                    if (magicItem.HasEffect(MagicEffectType.ReduceWeight) ||
                        magicItem.HasEffect(MagicEffectType.Weightless))
                    {
                        value = $"<color={magicColor}>{value}</color>";
                    }
                    break;

                case "$inventory_damage":
                    if (allMagic)
                    {
                        value = $"<color={magicColor}>{value}</color>";
                    }
                    break;

                case "$inventory_blunt":
                    if (allMagic || physMagic || bluntMagic)
                    {
                        value = $"<color={magicColor}>{value}</color>";
                    }
                    break;

                case "$inventory_slash":
                    if (allMagic || physMagic || slashMagic)
                    {
                        value = $"<color={magicColor}>{value}</color>";
                    }
                    break;

                case "$inventory_pierce":
                    if (allMagic || physMagic || pierceMagic)
                    {
                        value = $"<color={magicColor}>{value}</color>";
                    }
                    break;

                case "$inventory_fire":
                    if (allMagic || elemMagic || fireMagic)
                    {
                        value = $"<color={magicColor}>{value}</color>";
                    }
                    break;

                case "$inventory_frost":
                    if (allMagic || elemMagic || frostMagic)
                    {
                        value = $"<color={magicColor}>{value}</color>";
                    }
                    break;

                case "$inventory_lightning":
                    if (allMagic || elemMagic || lightningMagic)
                    {
                        value = $"<color={magicColor}>{value}</color>";
                    }
                    break;

                case "$inventory_poison":
                    if (allMagic || elemMagic || poisonMagic)
                    {
                        value = $"<color={magicColor}>{value}</color>";
                    }
                    break;

                case "$inventory_spirit":
                    if (allMagic || elemMagic || spiritMagic)
                    {
                        value = $"<color={magicColor}>{value}</color>";
                    }
                    break;

                case "$item_backstab":
                    if (magicItem.HasEffect(MagicEffectType.ModifyBackstab))
                    {
                        float totalBackstabBonusMod = magicItem.GetTotalEffectValue(MagicEffectType.ModifyBackstab, 0.01f);
                        float backstabValue = item.m_shared.m_backstabBonus * (1.0f + totalBackstabBonusMod);
                        value = $"<color={magicColor}>{backstabValue:0.#}x</color>";
                    }
                    break;

                case "$item_blockarmor":
                    if (magicItem.HasEffect(MagicEffectType.ModifyBlockPower))
                    {
                        float baseBlockPower = item.GetBaseBlockPower(item.m_quality);
                        string blockPowerPercentageString = item.GetBlockPowerTooltip(item.m_quality).ToString("0");
                        value = $"<color={magicColor}>{baseBlockPower}</color> " +
                            $"<color={magicColor}>({blockPowerPercentageString})</color>";
                    }
                    break;

                case "$item_deflection":
                    if (magicItem.HasEffect(MagicEffectType.ModifyParry))
                    {
                        value = $"<color={magicColor}>{item.GetDeflectionForce(item.m_quality)}</color>";
                    }
                    break;

                case "$item_parrybonus":
                    if (magicItem.HasEffect(MagicEffectType.ModifyParry))
                    {
                        float totalParryBonusMod = magicItem.GetTotalEffectValue(MagicEffectType.ModifyParry, 0.01f);
                        float timedBlockBonus = item.m_shared.m_timedBlockBonus * (1.0f + totalParryBonusMod);
                        value = $"<color={magicColor}>{timedBlockBonus:0.#}x</color>";
                    }
                    break;

                case "$item_armor":
                    if (magicItem.HasEffect(MagicEffectType.ModifyArmor))
                    {
                        value = $"<color={magicColor}>{value}</color>";
                    }
                    break;

                case "$item_staminause":
                    if (magicItem.HasEffect(MagicEffectType.ModifyAttackStaminaUse) ||
                        magicItem.HasEffect(MagicEffectType.ModifyBlockStaminaUse))
                    {
                        value = $"<color={magicColor}>{value}</color>";
                    }
                    break;
            }

            if (label.StartsWith("$item_movement_modifier") &&
                (magicItem.HasEffect(MagicEffectType.RemoveSpeedPenalty) ||
                magicItem.HasEffect(MagicEffectType.ModifyMovementSpeed)))
            {
                int colorIndex = label.IndexOf("<color", StringComparison.Ordinal);
                if (colorIndex >= 0)
                {
                    StringBuilder sb = new StringBuilder(label);
                    sb.Remove(colorIndex, "<color=#XXXXXX>".Length);
                    sb.Insert(colorIndex, $"<color={magicColor}>");

                    string itemMovementModDisplay = MagicTooltip.GetMovementModifier(
                        item, magicItem, out _, out _);
                    int valueIndex = colorIndex + "<color=#XXXXXX>".Length;
                    int percentIndex = label.IndexOf("%", valueIndex, StringComparison.Ordinal);
                    sb.Remove(valueIndex, percentIndex - valueIndex + 1);
                    sb.Insert(valueIndex, itemMovementModDisplay);

                    label = sb.ToString();
                }
            }
        }

        bool magicEitrRegen = magicItem?.HasEffect(MagicEffectType.ModifyEitrRegen) ?? false;
        if (label.StartsWith("$item_eitrregen_modifier") && (magicEitrRegen ||
            item.m_shared.m_eitrRegenModifier != 0) && localPlayer != null)
        {
            string itemEitrRegenModDisplay = MagicTooltip.GetEitrRegenModifier(item, magicItem, out _);

            float equipEitrRegenModifier = localPlayer.GetEquipmentEitrRegenModifier() * 100.0f;
            float equipMagicEitrRegenModifier = localPlayer.GetTotalActiveMagicEffectValue(MagicEffectType.ModifyEitrRegen);
            float totalEitrRegenModifier = equipEitrRegenModifier + equipMagicEitrRegenModifier;
            if (magicEitrRegen && magicItem != null)
                itemEitrRegenModDisplay = $"<color={magicItem.GetColorString()}>{itemEitrRegenModDisplay}</color>";
            label = $"$item_eitrregen_modifier: {itemEitrRegenModDisplay} " +
                $"($item_total: <color={magicItem.GetColorString()}>{totalEitrRegenModifier:+0;-0}%</color>)";
        }

        return new Tuple<string, string>(label, value);
    }
}