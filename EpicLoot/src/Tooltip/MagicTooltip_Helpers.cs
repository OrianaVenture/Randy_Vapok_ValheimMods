using System;
using UnityEngine;

namespace EpicLoot;

public partial class MagicTooltip
{
    public static string GetDamageTooltipString(MagicItem item, HitData.DamageTypes instance,
            Skills.SkillType skillType, string magicColor)
    {
        if (Player.m_localPlayer == null)
        {
            return "";
        }

        bool allMagic = item.HasEffect(MagicEffectType.ModifyDamage);
        bool physMagic = item.HasEffect(MagicEffectType.ModifyPhysicalDamage);
        bool elemMagic = item.HasEffect(MagicEffectType.ModifyElementalDamage);
        bool bluntMagic = item.HasEffect(MagicEffectType.AddBluntDamage);
        bool slashMagic = item.HasEffect(MagicEffectType.AddSlashingDamage);
        bool pierceMagic = item.HasEffect(MagicEffectType.AddPiercingDamage);
        bool fireMagic = item.HasEffect(MagicEffectType.AddFireDamage);
        bool frostMagic = item.HasEffect(MagicEffectType.AddFrostDamage);
        bool lightningMagic = item.HasEffect(MagicEffectType.AddLightningDamage);
        bool poisonMagic = item.HasEffect(MagicEffectType.AddPoisonDamage);
        bool spiritMagic = item.HasEffect(MagicEffectType.AddSpiritDamage);
        bool coinHoarderMagic = Player.m_localPlayer.HasActiveMagicEffect(MagicEffectType.CoinHoarder, out float _cv);
        bool spellswordMagic = item.HasEffect(MagicEffectType.SpellSword);
        Player.m_localPlayer.GetSkills().GetRandomSkillRange(out float min, out float max, skillType);
        string str = String.Empty;
        if (instance.m_damage != 0.0)
        {
            bool magic = allMagic || spellswordMagic;
            str = str + "\n$inventory_damage: " + DamageRange(instance.m_damage, min, max, magic, magicColor);
        }
        if (instance.m_blunt != 0.0)
        {
            bool magic = allMagic || physMagic || bluntMagic || coinHoarderMagic || spellswordMagic;
            str = str + "\n$inventory_blunt: " + DamageRange(instance.m_blunt, min, max, magic, magicColor);
        }
        if (instance.m_slash != 0.0)
        {
            bool magic = allMagic || physMagic || slashMagic || coinHoarderMagic || spellswordMagic;
            str = str + "\n$inventory_slash: " + DamageRange(instance.m_slash, min, max, magic, magicColor);
        }
        if (instance.m_pierce != 0.0)
        {
            bool magic = allMagic || physMagic || pierceMagic || coinHoarderMagic || spellswordMagic;
            str = str + "\n$inventory_pierce: " + DamageRange(instance.m_pierce, min, max, magic, magicColor);
        }
        if (instance.m_fire != 0.0)
        {
            bool magic = allMagic || elemMagic || fireMagic || coinHoarderMagic || spellswordMagic;
            str = str + "\n$inventory_fire: " + DamageRange(instance.m_fire, min, max, magic, magicColor);
        }
        if (instance.m_frost != 0.0)
        {
            bool magic = allMagic || elemMagic || frostMagic || coinHoarderMagic || spellswordMagic;
            str = str + "\n$inventory_frost: " + DamageRange(instance.m_frost, min, max, magic, magicColor);
        }
        if (instance.m_lightning != 0.0)
        {
            bool magic = allMagic || elemMagic || lightningMagic || coinHoarderMagic || spellswordMagic;
            str = str + "\n$inventory_lightning: " + DamageRange(instance.m_lightning, min, max, magic, magicColor);
        }
        if (instance.m_poison != 0.0)
        {
            bool magic = allMagic || elemMagic || poisonMagic || coinHoarderMagic || spellswordMagic;
            str = str + "\n$inventory_poison: " + DamageRange(instance.m_poison, min, max, magic, magicColor);
        }
        if (instance.m_spirit != 0.0)
        {
            bool magic = allMagic || elemMagic || spiritMagic || coinHoarderMagic || spellswordMagic;
            str = str + "\n$inventory_spirit: " + DamageRange(instance.m_spirit, min, max, magic, magicColor);
        }
        return str;
    }
    
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
            itemEitrRegenModifier += magicItem.GetTotalEffectValue(MagicEffectType.ModifyEitrRegen);

        return (itemEitrRegenModifier == 0) ? "0%" : $"{itemEitrRegenModifier:+0;-0}%";
    }
    
    public static string GetMovementModifier(ItemDrop.ItemData item, MagicItem magicItem,
        out bool magicMovement, out bool removePenalty)
    {
        magicMovement = magicItem.HasEffect(MagicEffectType.ModifyMovementSpeed);
        removePenalty = magicItem.HasEffect(MagicEffectType.RemoveSpeedPenalty);

        float itemMovementModifier = removePenalty ? 0 : item.m_shared.m_movementModifier * 100f;
        if (magicMovement)
        {
            itemMovementModifier += magicItem.GetTotalEffectValue(MagicEffectType.ModifyMovementSpeed);
        }

        return (itemMovementModifier == 0) ? "0%" : $"{itemMovementModifier:+0;-0}%";
    }
}