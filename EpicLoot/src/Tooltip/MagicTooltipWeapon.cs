using UnityEngine;

namespace EpicLoot;

public partial class MagicTooltip
{
    private void AddDamages()
    {
        HitData.DamageTypes damages = item.GetDamage(qualityLevel, Game.m_worldLevel);
        localPlayer.GetSkills().GetRandomSkillRange(out float min, out float max, item.m_shared.m_skillType);
        
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
        bool coinHoarderMagic = localPlayer.HasActiveMagicEffect(MagicEffectType.CoinHoarder, out float _cv);
        bool spellswordMagic = magicItem.HasEffect(MagicEffectType.SpellSword);
        
        if (damages.m_damage != 0.0)
        {
            bool isMagic = allMagic || spellswordMagic;

            text.AppendFormat("\n{0}: {1}", 
                "$inventory_damage",
                DamageRange(damages.m_damage, min, max, isMagic, magicColor));
        }

        if (damages.m_blunt != 0.0)
        {
            bool isMagic = allMagic || physMagic || bluntMagic || coinHoarderMagic || spellswordMagic;
            text.AppendFormat("\n{0}: {1}", 
                "$inventory_blunt",
                DamageRange(damages.m_blunt, min, max, isMagic, magicColor));
        }

        if (damages.m_slash != 0.0)
        {
            bool isMagic = allMagic || physMagic || slashMagic || coinHoarderMagic || spellswordMagic;
            text.AppendFormat("\n{0}: {1}", 
                "$inventory_slash",
                DamageRange(damages.m_slash, min, max, isMagic, magicColor));
        }

        if (damages.m_pierce != 0.0)
        {
            bool isMagic = allMagic || physMagic || pierceMagic || coinHoarderMagic || spellswordMagic;
            text.AppendFormat("\n{0}: {1}", 
                "$inventory_pierce",
                DamageRange(damages.m_pierce, min, max, isMagic, magicColor));
        }

        if (damages.m_fire != 0.0)
        {
            bool isMagic = allMagic || elemMagic || fireMagic || coinHoarderMagic || spellswordMagic;
            text.AppendFormat("\n{0}: {1}", 
                "$inventory_fire",
                DamageRange(damages.m_fire, min, max, isMagic, magicColor));
        }
        if (damages.m_frost != 0.0)
        {
            bool isMagic = allMagic || elemMagic || frostMagic || coinHoarderMagic || spellswordMagic;
            text.AppendFormat("\n{0}: {1}", 
                "$inventory_frost",
                DamageRange(damages.m_frost, min, max, isMagic, magicColor));
        }
        if (damages.m_lightning != 0.0)
        {
            bool isMagic = allMagic || elemMagic || lightningMagic || coinHoarderMagic || spellswordMagic;
            text.AppendFormat("\n{0}: {1}", 
                "$inventory_lightning",
                DamageRange(damages.m_lightning, min, max, isMagic, magicColor));
        }
        if (damages.m_poison != 0.0)
        {
            bool isMagic = allMagic || elemMagic || poisonMagic || coinHoarderMagic || spellswordMagic;
            text.AppendFormat("\n{0}: {1}", 
                "$inventory_poison",
                DamageRange(damages.m_poison, min, max, isMagic, magicColor));
        }
        
        if (damages.m_spirit != 0.0)
        {
            bool isMagic = allMagic || elemMagic || spiritMagic || coinHoarderMagic || spellswordMagic;
            text.AppendFormat("\n{0}: {1}", 
                "$inventory_spirit",
                DamageRange(damages.m_spirit, min, max, isMagic, magicColor));
        }
    }

    private void AddDamageMultiplierByTotalHealthMissing()
    {
        if (item.m_shared.m_attack.m_damageMultiplierByTotalHealthMissing > 0.0)
        {
            text.Append(
                $"\n$item_damagemultipliertotal: <color=orange>{item.m_shared.m_attack.m_damageMultiplierByTotalHealthMissing * 100}%</color>");
        }
    }

    private void AddDamageMultiplierPerMissingHP()
    {
        if (item.m_shared.m_attack.m_damageMultiplierPerMissingHP > 0.0)
        {
            text.Append(
                $"\n$item_damagemultplierhp: <color=orange>{item.m_shared.m_attack.m_damageMultiplierPerMissingHP * 100}%</color>");
        }
    }

    private void AddAttackStaminaUse()
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

    private void AddDodge()
    {
        bool hasDodgeBuff = magicItem.HasEffect(MagicEffectType.DodgeBuff);
        if (hasDodgeBuff)
        {
            float dodgeBuffValue = magicItem.GetTotalEffectValue(MagicEffectType.DodgeBuff, 1f);
            // TODO: if using this tooltip, localize this
            text.Append($"\n$mod_epicloot_dodge: <color={magicColor}>{dodgeBuffValue:#.#}</color>");
        }
    }

    private void AddOffset()
    {
        bool hasOffSetAttack = magicItem.HasEffect(MagicEffectType.OffSetAttack);
        if (hasOffSetAttack)
        {
            float offSetAttackValue = magicItem.GetTotalEffectValue(MagicEffectType.OffSetAttack, 1f);
            // TODO: if using this tooltip, localize this
            text.Append($"\n$mod_epicloot_offset: <color={magicColor}>{offSetAttackValue:#.#}</color>");
        }
    }

    private void AddChainLightning()
    {
        bool hasChainLightning = magicItem.HasEffect(MagicEffectType.ChainLightning);
        if (hasChainLightning)
        {
            float ChainLightningValue = magicItem.GetTotalEffectValue(MagicEffectType.ChainLightning, 1f);
            // TODO: if using this tooltip, localize this
            text.Append(
                $"\n$mod_epicloot_chainlightning: <color={magicColor}>{ChainLightningValue:#.#}</color>");
        }
    }

    private void AddApportation()
    {
        bool hasApportation = magicItem.HasEffect(MagicEffectType.Apportation);
        if (hasApportation)
        {
            float ApportationValue = magicItem.GetTotalEffectValue(MagicEffectType.Apportation, 1f);
            // TODO: if adding this tooltip, localize this
            text.Append($"\n$mod_epicloot_apportation: <color={magicColor}>{ApportationValue:#.#}</color>");
        }
    }

    private void AddEitrUse()
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
            if (base_cost == 0f)
            {
                base_cost = 4;
            }
            totalEitrUse = totalEitrUse + (base_cost / 2);
            
            text.Append($"\n$item_eitruse: <color={magicAttackEitrColor}>{totalEitrUse:#.#}</color>");
        }
    }

    private void AddHealthUse()
    {
        bool hasBloodlust = magicItem.HasEffect(MagicEffectType.Bloodlust);
        float healthUsageReduction = 1 - magicItem.GetTotalEffectValue(MagicEffectType.ModifyAttackHealthUse, 0.01f);
        
        if (hasBloodlust) 
        {
            float bloodlustStaminaUse = item.m_shared.m_attack.m_attackStamina;

            float skillmodCost = bloodlustStaminaUse - bloodlustStaminaUse * 0.33f * Player.m_localPlayer.GetSkillFactor(item.m_shared.m_skillType);
            text.Append($"\n$item_healthuse: <color={magicColor}>{(bloodlustStaminaUse * healthUsageReduction):#.#} ({skillmodCost})</color>");
        }
        else
        {
            if (item.m_shared.m_attack.m_attackHealth > 0.0) 
            {
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

    private void AddHealthHitReturn()
    {
        if (item.m_shared.m_attack.m_attackHealthReturnHit > 0.0)
        {
            text.Append(
                $"\n$item_healthhitreturn: <color=orange>{item.m_shared.m_attack.m_attackHealthReturnHit}</color>");
        }
    }

    private void AddHealthUsePercentage()
    {
        if (item.m_shared.m_attack.m_attackHealthPercentage > 0.0)
        {
            text.Append(
                $"\n$item_healthuse: <color=orange>{item.m_shared.m_attack.m_attackHealthPercentage:0.0}</color>");
        }
    }

    private void AddDrawStaminaUse()
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
    
    private void AddParry()
    {
        // this is the same as add block and parry, except label is $item_deflection vs $item_blockforce
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

    private void AddBackstab()
    {
        bool hasBackstabModifier = magicItem.HasEffect(MagicEffectType.ModifyBackstab);
        float totalBackstabBonusMod = magicItem.GetTotalEffectValue(MagicEffectType.ModifyBackstab, 0.01f);
        string magicBackstabColor = hasBackstabModifier ? magicColor : "orange";
        float backstabValue = item.m_shared.m_backstabBonus * (1.0f + totalBackstabBonusMod);
        text.Append($"\n$item_backstab: <color={magicBackstabColor}>{backstabValue:0.#}x</color>");
    }

    private void AddProjectileTooltip()
    {
        string projectileTooltip = item.GetProjectileTooltip(qualityLevel);
        if (projectileTooltip.Length > 0)
        {
            text.Append("\n\n");
            text.Append(projectileTooltip);
        }
    }

    private void AddTameOnly()
    {
        if (item.m_shared.m_tamedOnly)
        {
            text.Append($"\n<color=orange>$item_tameonly</color>");
        }
    }
    
    private void AddKnockback()
    {
        text.AppendFormat("\n$item_knockback: <color=orange>{0}</color>", item.m_shared.m_attackForce);
    }

    private void AddMaxAdrenaline()
    {
        text.AppendFormat("\n$item_maxadrenaline: <color=orange>{0}</color>", item.m_shared.m_maxAdrenaline);
    }

    private void AddEitrRegen()
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

    private void AddMovement()
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