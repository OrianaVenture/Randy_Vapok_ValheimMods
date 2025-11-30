namespace EpicLoot;

public partial class MagicTooltip
{
    private void Damage()
    {
        text.Append(GetDamageTooltipString(magicItem, item.GetDamage(qualityLevel, Game.m_worldLevel),
            item.m_shared.m_skillType, magicColor));
    }

    private void AttackStaminaUse()
    {
        bool hasAttackStaminaModifiers = magicItem.HasEffect(MagicEffectType.ModifyAttackStaminaUse) ||
                                  magicItem.HasEffect(MagicEffectType.ModifyBlockStaminaUse);
        string magicAttackStaminaColor = hasAttackStaminaModifiers ? magicColor : "orange";
        float staminaUsePercentage = 1 - magicItem.GetTotalEffectValue(MagicEffectType.ModifyAttackStaminaUse, 0.01f);
        float totalStaminaUse = staminaUsePercentage * item.m_shared.m_attack.m_attackStamina;
        if (item.m_shared.m_attack.m_attackStamina > 0.0 && !magicItem.HasEffect(MagicEffectType.Bloodlust))
        {
            text.Append($"\n$item_staminause: <color={magicAttackStaminaColor}>{totalStaminaUse:#.#}</color>");
        }
    }

    private void Dodge()
    {
        bool DodgeBuff = magicItem.HasEffect(MagicEffectType.DodgeBuff);
        string DodgeBuffColor = DodgeBuff ? magicColor : "orange";
        if (DodgeBuff)
        {
            float dodgeBuffValue = magicItem.GetTotalEffectValue(MagicEffectType.DodgeBuff, 1f);
        }
    }

    private void Offset()
    {
        bool OffSetAttack = magicItem.HasEffect(MagicEffectType.OffSetAttack);
        string OffSetAttackColor = OffSetAttack ? magicColor : "orange";
        if (OffSetAttack)
        {
            float offSetAttackValue = magicItem.GetTotalEffectValue(MagicEffectType.OffSetAttack, 1f);
        }
    }

    private void ChainLightning()
    {
        bool ChainLightning = magicItem.HasEffect(MagicEffectType.ChainLightning);
        string ChainLightningColor = ChainLightning ? magicColor : "orange";
        if (ChainLightning)
        {
            float ChainLightningValue = magicItem.GetTotalEffectValue(MagicEffectType.ChainLightning, 1f);
        }
    }

    private void Apportation()
    {
        bool Apportation = magicItem.HasEffect(MagicEffectType.Apportation);
        string ApportationColor = Apportation ? magicColor : "orange";
        if (Apportation)
        {
            float ApportationValue = magicItem.GetTotalEffectValue(MagicEffectType.Apportation, 1f);
        }
    }

    private void EitrUse()
    {
        bool hasSpellSword = magicItem.HasEffect(MagicEffectType.SpellSword);
        bool hasDoubleMagicShot = magicItem.HasEffect(MagicEffectType.DoubleMagicShot);
        bool hasEitrUseModifier = magicItem.HasEffect(MagicEffectType.ModifyAttackEitrUse);
        
        bool hasAttackEitrModifier = hasEitrUseModifier || hasDoubleMagicShot || hasSpellSword;
        
        string magicAttackEitrColor = hasAttackEitrModifier ? magicColor : "orange";
        
        float eitrUsePercentage = 1 - magicItem.GetTotalEffectValue(MagicEffectType.ModifyAttackEitrUse, 0.01f);
        float totalEitrUse = hasDoubleMagicShot
            ? eitrUsePercentage * (item.m_shared.m_attack.m_attackEitr * 2)
            : eitrUsePercentage * item.m_shared.m_attack.m_attackEitr;

        if (item.m_shared.m_attack.m_attackEitr > 0.0 || hasSpellSword)
        {
            float base_cost = item.m_shared.m_attack.m_attackStamina;
            if (base_cost == 0f) { base_cost = 4; }
            totalEitrUse = totalEitrUse + (base_cost / 2);
            
            text.Append($"\n$item_eitruse: <color={magicAttackEitrColor}>{totalEitrUse:#.#}</color>");
        }
    }

    private void HealthUse()
    {
        bool hasBloodlust = magicItem.HasEffect(MagicEffectType.Bloodlust);
        string bloodlustColor = hasBloodlust ? magicColor : "orange";
        float bloodlustStaminaUse = item.m_shared.m_attack.m_attackStamina;
        float healthUsageReduction = 1 - magicItem.GetTotalEffectValue(MagicEffectType.ModifyAttackHealthUse, 0.01f);
        
        if (hasBloodlust) 
        {
            float skillmodCost = bloodlustStaminaUse - bloodlustStaminaUse * 0.33f * Player.m_localPlayer.GetSkillFactor(item.m_shared.m_skillType);
            text.Append($"\n$item_healthuse: <color={bloodlustColor}>{(bloodlustStaminaUse * healthUsageReduction):#.#} ({skillmodCost})</color>");
        }
        else
        {
            if (item.m_shared.m_attack.m_attackHealth > 0.0) {
                float skillmodCost = item.m_shared.m_attack.m_attackHealth - item.m_shared.m_attack.m_attackHealth * 0.33f * Player.m_localPlayer.GetSkillFactor(item.m_shared.m_skillType);
                text.Append($"\n$item_healthuse: <color=orange>{item.m_shared.m_attack.m_attackHealth * healthUsageReduction} ({skillmodCost})</color>");
            }
        }
        
        if (item.m_shared.m_attack.m_attackHealthPercentage > 0.0) 
        {
            bool magicAttackHealth = magicItem.HasEffect(MagicEffectType.ModifyAttackHealthUse);
            string magicAttackHealthColor = magicAttackHealth ? magicColor : "orange";
            float totalHealthPercentageUse = healthUsageReduction * item.m_shared.m_attack.m_attackHealthPercentage;
            
            float healthCost = totalHealthPercentageUse / 100;
            float skillmodCost = healthCost - healthCost * 0.33f * Player.m_localPlayer.GetSkillFactor(item.m_shared.m_skillType);
            text.Append($"\n$item_healthuse: <color={magicAttackHealthColor}>{healthCost:##.#%} ({skillmodCost})</color>");
        }
    }

    private void DrawStaminaUse()
    {
        if (item.m_shared.m_attack.m_drawStaminaDrain > 0.0)
        {
            bool hasDrawStaminaUseModifier = magicItem.HasEffect(MagicEffectType.ModifyDrawStaminaUse);
            string attackDrawStaminaColor = hasDrawStaminaUseModifier ? magicColor : "orange";
            
            float attackDrawStaminaPercentage = 1 - magicItem.GetTotalEffectValue(MagicEffectType.ModifyDrawStaminaUse, 0.01f);
            float totalAttackDrawStamina = attackDrawStaminaPercentage * item.m_shared.m_attack.m_drawStaminaDrain;
            
            text.Append($"\n$item_staminahold: " +
                     $"<color={attackDrawStaminaColor}>{totalAttackDrawStamina:#.#}/s</color>");
            
        }
    }
    
    private void Parry()
    {
        if (item.m_shared.m_timedBlockBonus > 1.0)
        {
            bool hasParryModifier = magicItem.HasEffect(MagicEffectType.ModifyParry);
            float totalParryBonusMod = magicItem.GetTotalEffectValue(MagicEffectType.ModifyParry, 0.01f);
            string magicParryColor = hasParryModifier ? magicColor : "orange";
            
            text.Append($"\n$item_deflection: " +
                        $"<color={magicParryColor}>{item.GetDeflectionForce(qualityLevel)}</color>");

            float timedBlockBonus = item.m_shared.m_timedBlockBonus;
            if (hasParryModifier)
            {
                timedBlockBonus *= 1.0f + totalParryBonusMod;
            }

            text.Append($"\n$item_parrybonus: <color={magicParryColor}>{timedBlockBonus:0.#}x</color>");
        }
    }

    private void Backstab()
    {
        bool hasBackstabModifier = magicItem.HasEffect(MagicEffectType.ModifyBackstab);
        float totalBackstabBonusMod = magicItem.GetTotalEffectValue(MagicEffectType.ModifyBackstab, 0.01f);
        string magicBackstabColor = hasBackstabModifier ? magicColor : "orange";
        float backstabValue = item.m_shared.m_backstabBonus * (1.0f + totalBackstabBonusMod);
        text.Append($"\n$item_backstab: <color={magicBackstabColor}>{backstabValue:0.#}x</color>");
    }

    private void Projectile()
    {
        string projectileTooltip = item.GetProjectileTooltip(qualityLevel);
        if (projectileTooltip.Length > 0)
        {
            text.Append("\n\n");
            text.Append(projectileTooltip);
        }
    }

    private void Chain()
    {
        string chainTooltip2 = item.GetChainTooltip(qualityLevel, skillLevel);
        if (chainTooltip2.Length > 0)
        {
            text.Append("\n\n");
            text.Append(chainTooltip2);
        }
    }
    
    private void Knockback()
    {
        text.AppendFormat("\n$item_knockback: <color=orange>{0}</color>", item.m_shared.m_attackForce);
    }

    private void MaxAdrenaline()
    {
        text.AppendFormat("\n$item_maxadrenaline: <color=orange>{0}</color>", item.m_shared.m_maxAdrenaline);
    }

    private void EitrRegen()
    {
        bool hasEitrRegenModifier = magicItem.HasEffect(MagicEffectType.ModifyEitrRegen);
        if ((hasEitrRegenModifier || item.m_shared.m_eitrRegenModifier != 0) && localPlayer != null)
        {
            string itemEitrRegenModDisplay = GetEitrRegenModifier(item, magicItem, out _);

            float equipEitrRegenModifier = localPlayer.GetEquipmentEitrRegenModifier() * 100.0f;
            float equipMagicEitrRegenModifier = localPlayer.GetTotalActiveMagicEffectValue(MagicEffectType.ModifyEitrRegen);
            float totalEitrRegenModifier = equipEitrRegenModifier + equipMagicEitrRegenModifier;
            string color = (hasEitrRegenModifier) ? magicColor : "orange";
            string totalColor = equipMagicEitrRegenModifier > 0 ? magicColor : "yellow";
            text.Append($"\n$item_eitrregen_modifier: <color={color}>{itemEitrRegenModDisplay}</color> " +
                        $"($item_total: <color={totalColor}>{totalEitrRegenModifier:+0;-0}%</color>)");
        }
    }

    private void Movement()
    {
        bool hasMovementModifier = magicItem.HasEffect(MagicEffectType.ModifyMovementSpeed);
        if ((hasMovementModifier || item.m_shared.m_movementModifier != 0) && localPlayer != null)
        {
            string itemMovementModDisplay = GetMovementModifier(item, magicItem, out _, out bool removePenalty);

            float movementModifier = localPlayer.GetEquipmentMovementModifier();
            float totalMovementModifier = movementModifier * 100f;
            string color = (removePenalty || hasMovementModifier) ? magicColor : "orange";
            text.Append($"\n$item_movement_modifier: <color={color}>{itemMovementModDisplay}</color> " +
                        $"($item_total:<color=yellow>{totalMovementModifier:+0;-0}%</color>)");
        }
    }
}