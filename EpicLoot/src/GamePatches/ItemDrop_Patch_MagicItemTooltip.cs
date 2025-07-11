using System;
using System.Text;
using EpicLoot.Crafting;
using EpicLoot.Data;
using EpicLoot.MagicItemEffects;
using EpicLoot.src.GamePatches;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;

namespace EpicLoot
{
    // Set the topic of the tooltip with the decorated name
    [HarmonyPatch(typeof(InventoryGrid), nameof(InventoryGrid.CreateItemTooltip),
        typeof(ItemDrop.ItemData), typeof(UITooltip))]
    public static class InventoryGrid_CreateItemTooltip_MagicItemComponent_Patch
    {
        [HarmonyAfter(new []{"kg.ValheimEnchantmentSystem"})]
        public static bool Prefix(ItemDrop.ItemData item, UITooltip tooltip, out string __state)
        {
            __state = null;
            string tooltipText;
            if (item.IsEquipable() && !item.m_equipped && Player.m_localPlayer != null &&
                Player.m_localPlayer.HasEquipmentOfType(item.m_shared.m_itemType) && ZInput.GetKey(KeyCode.LeftControl))
            {
                ItemDrop.ItemData otherItem = Player.m_localPlayer.GetEquipmentOfType(item.m_shared.m_itemType);
                tooltipText = item.GetTooltip();
                // Set the comparision tooltip to be shown side-by-side with our original tooltip
                PatchOnHoverFix.comparision_title = $"<color=#AAA><i>$mod_epicloot_currentlyequipped:" +
                    $"</i></color>" + otherItem.GetDecoratedName();
                PatchOnHoverFix.comparision_tooltip = otherItem.GetTooltip();
            } else {
                PatchOnHoverFix.comparision_tooltip = "";
                PatchOnHoverFix.comparision_added = false;
                tooltipText = item.GetTooltip();
            }
            tooltip.Set(item.GetDecoratedName(), tooltipText);
            return false;
        }
    }

    // Set the content of the tooltip
    [HarmonyPatch(typeof(ItemDrop.ItemData), nameof(ItemDrop.ItemData.GetTooltip),
        typeof(ItemDrop.ItemData), typeof(int), typeof(bool), typeof(float), typeof(int))]
    public static class MagicItemTooltip_ItemDrop_Patch
    {
        [UsedImplicitly]
        private static bool Prefix(ref string __result, ItemDrop.ItemData item, int qualityLevel)
        {
            if (item == null)
                return true;

            var localPlayer = Player.m_localPlayer;
            var text = new StringBuilder(256);

            MagicItem magicItem = item.GetMagicItem();

            if (magicItem == null)
                return true;

            string magicColor = magicItem.GetColorString();
            string itemTypeName = magicItem.GetItemTypeName(item.Extended());

            float skillLevel = localPlayer.GetSkillLevel(item.m_shared.m_skillType);

            text.Append($"<color={magicColor}>{magicItem.GetRarityDisplay()} {itemTypeName}</color>\n");
            if (item.IsLegendarySetItem())
            {
                text.Append($"<color={EpicLoot.GetSetItemColor()}>$mod_epicloot_legendarysetlabel</color>\n");
            }
            text.Append(item.GetDescription());
            
            text.Append("\n");
            if (item.m_shared.m_dlc.Length > 0)
            {
                text.Append("\n<color=#00ffffff>$item_dlc</color>");
            }

            ItemDrop.ItemData.AddHandedTip(item, text);
            if (item.m_crafterID != 0L)
            {
                text.AppendFormat("\n$item_crafter: <color=orange>{0}</color>", item.GetCrafterName());
            }

            if (!item.m_shared.m_teleportable)
            {
                text.Append("\n<color=orange>$item_noteleport</color>");
            }

            if (item.m_shared.m_value > 0)
            {
                text.AppendFormat("\n$item_value: <color=orange>{0} ({1})</color>", item.GetValue(), item.m_shared.m_value);
            }

            string weightColor = magicItem.HasEffect(MagicEffectType.ReduceWeight) ||
                magicItem.HasEffect(MagicEffectType.Weightless) ? magicColor : "orange";
            text.Append($"\n$item_weight: <color={weightColor}>{item.GetWeight():0.0}</color>");

            if (item.m_shared.m_maxQuality > 1)
            {
                text.AppendFormat("\n$item_quality: <color=orange>{0}</color>", qualityLevel);
            }

            bool indestructible = magicItem.HasEffect(MagicEffectType.Indestructible);
            if (!indestructible && item.m_shared.m_useDurability)
            {
                string maxDurabilityColor1 = magicItem.HasEffect(MagicEffectType.ModifyDurability) ? magicColor : "orange";
                string maxDurabilityColor2 = magicItem.HasEffect(MagicEffectType.ModifyDurability) ? magicColor : "yellow";

                float maxDurability = item.GetMaxDurability(qualityLevel);
                float durability = item.m_durability;
                float currentDurabilityPercentage = item.GetDurabilityPercentage() * 100f;
                string durabilityPercentageString = currentDurabilityPercentage.ToString("0");
                string durabilityValueString = durability.ToString("0");
                string durabilityMaxString = maxDurability.ToString("0");
                text.Append($"\n$item_durability: <color={maxDurabilityColor1}>{durabilityPercentageString}%</color> " +
                    $"<color={maxDurabilityColor2}>({durabilityValueString}/{durabilityMaxString})</color>");

                if (item.m_shared.m_canBeReparied)
                {
                    Recipe recipe = ObjectDB.instance.GetRecipe(item);
                    if (recipe != null)
                    {
                        int minStationLevel = recipe.m_minStationLevel;
                        text.AppendFormat("\n$item_repairlevel: <color=orange>{0}</color>", minStationLevel.ToString());
                    }
                }
            }
            else if (indestructible)
            {
                text.Append($"\n$item_durability: <color={magicColor}>$mod_epicloot_me_indestructible_display</color>");
            }

            bool magicBlockPower = magicItem.HasEffect(MagicEffectType.ModifyBlockPower);
            string magicBlockColor1 = magicBlockPower ? magicColor : "orange";
            string magicBlockColor2 = magicBlockPower ? magicColor : "yellow";
            bool magicParry = magicItem.HasEffect(MagicEffectType.ModifyParry);
            float totalParryBonusMod = magicItem.GetTotalEffectValue(MagicEffectType.ModifyParry, 0.01f);
            string magicParryColor = magicParry ? magicColor : "orange";
            switch (item.m_shared.m_itemType)
            {
                case ItemDrop.ItemData.ItemType.Consumable:
                    if (item.m_shared.m_food > 0.0)
                    {
                        text.AppendFormat("\n$item_food_health: <color=orange>{0}</color>", item.m_shared.m_food);
                        text.AppendFormat("\n$item_food_stamina: <color=orange>{0}</color>", item.m_shared.m_foodStamina);
                        text.AppendFormat("\n$item_food_duration: <color=orange>{0}s</color>", item.m_shared.m_foodBurnTime);
                        text.AppendFormat("\n$item_food_regen: <color=orange>{0} hp/tick</color>", item.m_shared.m_foodRegen);
                    }

                    string consumeStatusEffectTooltip = item.GetStatusEffectTooltip(qualityLevel, skillLevel);
                    if (consumeStatusEffectTooltip.Length > 0)
                    {
                        text.Append("\n\n");
                        text.Append(consumeStatusEffectTooltip);
                    }

                    break;

                case ItemDrop.ItemData.ItemType.OneHandedWeapon:
                case ItemDrop.ItemData.ItemType.Bow:
                case ItemDrop.ItemData.ItemType.TwoHandedWeapon:
                case ItemDrop.ItemData.ItemType.TwoHandedWeaponLeft:
                case ItemDrop.ItemData.ItemType.Torch:
                    text.Append(GetDamageTooltipString(magicItem, item.GetDamage(qualityLevel,Game.m_worldLevel),
                        item.m_shared.m_skillType, magicColor));

                    bool magicAttackStamina = magicItem.HasEffect(MagicEffectType.ModifyAttackStaminaUse) ||
                        magicItem.HasEffect(MagicEffectType.ModifyBlockStaminaUse);
                    string magicAttackStaminaColor = magicAttackStamina ? magicColor : "orange";
                    float staminaUsePercentage = 1 - magicItem.GetTotalEffectValue(MagicEffectType.ModifyAttackStaminaUse, 0.01f);
                    float totalStaminaUse = staminaUsePercentage * item.m_shared.m_attack.m_attackStamina;
                    if (item.m_shared.m_attack.m_attackStamina > 0.0 && !magicItem.HasEffect(MagicEffectType.Bloodlust))
                        text.Append($"\n$item_staminause: <color={magicAttackStaminaColor}>{totalStaminaUse:#.#}</color>");

                    bool DodgeBuff = magicItem.HasEffect(MagicEffectType.DodgeBuff);
                    string DodgeBuffColor = DodgeBuff ? magicColor : "orange";
                    if (DodgeBuff)
                    {
                        float dodgeBuffValue = magicItem.GetTotalEffectValue(MagicEffectType.DodgeBuff, 1f);
                    }

                    bool OffSetAttack = magicItem.HasEffect(MagicEffectType.OffSetAttack);
                    string OffSetAttackColor = OffSetAttack ? magicColor : "orange";
                    if (OffSetAttack)
                    {
                        float offSetAttackValue = magicItem.GetTotalEffectValue(MagicEffectType.OffSetAttack, 1f);
                    }

                    bool ChainLightning = magicItem.HasEffect(MagicEffectType.ChainLightning);
                    string ChainLightningColor = OffSetAttack ? magicColor : "orange";
                    if (ChainLightning)
                    {
                        float ChainLightningValue = magicItem.GetTotalEffectValue(MagicEffectType.ChainLightning, 1f);
                    }

                    bool ReducedReloadSpeed = magicItem.HasEffect(MagicEffectType.ReducedReloadSpeed);
                    string ReducedReloadSpeedColor = ReducedReloadSpeed ? magicColor : "orange";
                    if (ReducedReloadSpeed)
                    {
                        float ReducedReloadSpeedValue = magicItem.GetTotalEffectValue(MagicEffectType.ReducedReloadSpeed, 1f);
                    }

                    bool Apportation = magicItem.HasEffect(MagicEffectType.Apportation);
                    string ApportationColor = Apportation ? magicColor : "orange";
                    if (Apportation)
                    {
                        float ApportationValue = magicItem.GetTotalEffectValue(MagicEffectType.Apportation, 1f);
                    }

                    bool magicAttackEitr = magicItem.HasEffect(MagicEffectType.ModifyAttackEitrUse) || magicItem.HasEffect(MagicEffectType.DoubleMagicShot);
                    bool doubleMagicShot = magicItem.HasEffect(MagicEffectType.DoubleMagicShot);
                    string magicAttackEitrColor = magicAttackEitr ? magicColor : "orange";
                    float eitrUsePercentage = 1 - magicItem.GetTotalEffectValue(MagicEffectType.ModifyAttackEitrUse, 0.01f);
                    float totalEitrUse = doubleMagicShot
                        ? eitrUsePercentage * (item.m_shared.m_attack.m_attackEitr * 2)
                        : eitrUsePercentage * item.m_shared.m_attack.m_attackEitr;

                    bool hasSpellSword = magicItem.HasEffect(MagicEffectType.SpellSword);
                    if (item.m_shared.m_attack.m_attackEitr > 0.0 || hasSpellSword)
                    {
                        float base_cost = item.m_shared.m_attack.m_attackStamina;
                        if (base_cost == 0f) { base_cost = 4; }
                        totalEitrUse = totalEitrUse + (base_cost / 2);
                        string spellswordColor = hasSpellSword ? magicColor : "orange";
                        text.Append($"\n$item_eitruse: <color={spellswordColor}>{totalEitrUse:#.#}</color>");
                    }

                    bool hasBloodlust = magicItem.HasEffect(MagicEffectType.Bloodlust);
                    string bloodlustColor = hasBloodlust ? magicColor : "orange";
                    float bloodlustStaminaUse = item.m_shared.m_attack.m_attackStamina;
                    if (hasBloodlust)
                    {
                        text.Append($"\n$item_healthuse: <color={bloodlustColor}>{bloodlustStaminaUse:#.#}</color>");
                    }
                    else
                    {
                        if (item.m_shared.m_attack.m_attackHealth > 0.0)
                            text.Append($"\n$item_healthuse: " +
                                $"<color=orange>{item.m_shared.m_attack.m_attackHealth}</color>");
                    }
                    
                    bool magicAttackHealth = magicItem.HasEffect(MagicEffectType.ModifyAttackHealthUse);
                    string magicAttackHealthColor = magicAttackHealth ? magicColor : "orange";
                    float healthUsePercentage = 1 - magicItem.GetTotalEffectValue(MagicEffectType.ModifyAttackHealthUse, 0.01f);
                    float totalHealthPercentageUse = healthUsePercentage * item.m_shared.m_attack.m_attackHealthPercentage;
                    if (item.m_shared.m_attack.m_attackHealthPercentage > 0.0)
                        text.Append($"\n$item_healthuse: <color={magicAttackHealthColor}>{(totalHealthPercentageUse / 100):##.#%}</color>");
                    
                    bool attackDrawStamina = magicItem.HasEffect(MagicEffectType.ModifyDrawStaminaUse);
                    string attackDrawStaminaColor = attackDrawStamina ? magicColor : "orange";
                    float attackDrawStaminaPercentage = 1 - magicItem.GetTotalEffectValue(MagicEffectType.ModifyDrawStaminaUse, 0.01f);
                    float totalattackDrawStamina = attackDrawStaminaPercentage * item.m_shared.m_attack.m_drawStaminaDrain;
                    if (item.m_shared.m_attack.m_drawStaminaDrain > 0.0)
                        text.Append($"\n$item_staminahold: " +
                            $"<color={attackDrawStaminaColor}>{totalattackDrawStamina:#.#}/s</color>");

                    float baseBlockPower1 = item.GetBaseBlockPower(qualityLevel);
                    float blockPowerTooltipValue = item.GetBlockPowerTooltip(qualityLevel);
                    string blockPowerPercentageString = blockPowerTooltipValue.ToString("0");
                    text.Append($"\n$item_blockarmor: <color={magicBlockColor1}>{baseBlockPower1}</color> " +
                        $"<color={magicBlockColor2}>({blockPowerPercentageString})</color>");
                    if (item.m_shared.m_timedBlockBonus > 1.0)
                    {
                        text.Append($"\n$item_deflection: " +
                            $"<color={magicParryColor}>{item.GetDeflectionForce(qualityLevel)}</color>");

                        float timedBlockBonus = item.m_shared.m_timedBlockBonus;
                        if (magicParry)
                        {
                            timedBlockBonus *= 1.0f + totalParryBonusMod;
                        }

                        text.Append($"\n$item_parrybonus: <color={magicParryColor}>{timedBlockBonus:0.#}x</color>");
                    }

                    text.AppendFormat("\n$item_knockback: <color=orange>{0}</color>", item.m_shared.m_attackForce);

                    bool magicBackstab = magicItem.HasEffect(MagicEffectType.ModifyBackstab);
                    float totalBackstabBonusMod = magicItem.GetTotalEffectValue(MagicEffectType.ModifyBackstab, 0.01f);
                    string magicBackstabColor = magicBackstab ? magicColor : "orange";
                    float backstabValue = item.m_shared.m_backstabBonus * (1.0f + totalBackstabBonusMod);
                    text.Append($"\n$item_backstab: <color={magicBackstabColor}>{backstabValue:0.#}x</color>");

                    string projectileTooltip = item.GetProjectileTooltip(qualityLevel);
                    if (projectileTooltip.Length > 0)
                    {
                        text.Append("\n\n");
                        text.Append(projectileTooltip);
                    }

                    string statusEffectTooltip2 = item.GetStatusEffectTooltip(qualityLevel, skillLevel);
                    if (statusEffectTooltip2.Length > 0)
                    {
                        text.Append("\n\n");
                        text.Append(statusEffectTooltip2);
                    }

                    break;

                case ItemDrop.ItemData.ItemType.Shield:
                    float baseBlockPower2 = item.GetBaseBlockPower(qualityLevel);
                    blockPowerTooltipValue = item.GetBlockPowerTooltip(qualityLevel);
                    string str5 = blockPowerTooltipValue.ToString("0");
                    text.Append($"\n$item_blockarmor: <color={magicBlockColor1}>{baseBlockPower2}</color> " +
                        $"<color={magicBlockColor2}>({str5})</color>");
                    if (item.m_shared.m_timedBlockBonus > 1.0)
                    {
                        text.Append($"\n$item_blockforce: " +
                            $"<color={magicParryColor}>{item.GetDeflectionForce(qualityLevel)}</color>");

                        float timedBlockBonus = item.m_shared.m_timedBlockBonus;
                        if (magicParry)
                        {
                            timedBlockBonus *= 1.0f + totalParryBonusMod;
                        }

                        text.Append($"\n$item_parrybonus: <color={magicParryColor}>{timedBlockBonus:0.#}x</color>");
                    }
                    string damageModifiersTooltipString = SE_Stats.GetDamageModifiersTooltipString(item.m_shared.m_damageModifiers);
                    if (damageModifiersTooltipString.Length > 0)
                    {
                        text.Append(damageModifiersTooltipString);
                    }

                    break;

                case ItemDrop.ItemData.ItemType.Helmet:
                case ItemDrop.ItemData.ItemType.Chest:
                case ItemDrop.ItemData.ItemType.Legs:
                case ItemDrop.ItemData.ItemType.Shoulder:
                    string magicArmorColor = magicItem.HasEffect(MagicEffectType.ModifyArmor) ? magicColor : "orange";
                    text.Append($"\n$item_armor: " +
                        $"<color={magicArmorColor}>{item.GetArmor(qualityLevel,Game.m_worldLevel):0.#}</color>");
                    string modifiersTooltipString = SE_Stats.GetDamageModifiersTooltipString(item.m_shared.m_damageModifiers);
                    if (modifiersTooltipString.Length > 0)
                    {
                        text.Append(modifiersTooltipString);
                    }

                    string statusEffectTooltip3 = item.GetStatusEffectTooltip(qualityLevel, skillLevel);
                    if (statusEffectTooltip3.Length > 0)
                    {
                        text.Append("\n");
                        text.Append(statusEffectTooltip3);
                    }

                    break;

                case ItemDrop.ItemData.ItemType.Ammo:
                    text.Append(item.GetDamage(qualityLevel,Game.m_worldLevel).GetTooltipString(item.m_shared.m_skillType));
                    text.AppendFormat("\n$item_knockback: <color=orange>{0}</color>", item.m_shared.m_attackForce);
                    break;
            }

            bool magicEitrRegen = magicItem.HasEffect(MagicEffectType.ModifyEitrRegen);
            if ((magicEitrRegen || item.m_shared.m_eitrRegenModifier != 0) && localPlayer != null)
            {
                string itemEitrRegenModDisplay = GetEitrRegenModifier(item, magicItem, out _);

                float equipEitrRegenModifier = localPlayer.GetEquipmentEitrRegenModifier() * 100.0f;
                float equipMagicEitrRegenModifier = localPlayer.GetTotalActiveMagicEffectValue(MagicEffectType.ModifyEitrRegen);
                float totalEitrRegenModifier = equipEitrRegenModifier + equipMagicEitrRegenModifier;
                string color = (magicEitrRegen) ? magicColor : "orange";
                string totalColor = equipMagicEitrRegenModifier > 0 ? magicColor : "yellow";
                text.Append($"\n$item_eitrregen_modifier: <color={color}>{itemEitrRegenModDisplay}</color> " +
                    $"($item_total: <color={totalColor}>{totalEitrRegenModifier:+0;-0}%</color>)");
            }

            bool magicMovement = magicItem.HasEffect(MagicEffectType.ModifyMovementSpeed);
            if ((magicMovement || item.m_shared.m_movementModifier != 0) && localPlayer != null)
            {
                string itemMovementModDisplay = GetMovementModifier(item, magicItem, out _, out bool removePenalty);

                float movementModifier = localPlayer.GetEquipmentMovementModifier();
                float totalMovementModifier = movementModifier * 100f;
                string color = (removePenalty || magicMovement) ? magicColor : "orange";
                text.Append($"\n$item_movement_modifier: <color={color}>{itemMovementModDisplay}</color> " +
                    $"($item_total:<color=yellow>{totalMovementModifier:+0;-0}%</color>)");
            }

            // Add magic item effects here
            text.AppendLine(magicItem.GetTooltip());

            // Set stuff
            if (item.IsSetItem())
            {
                text.Append(item.GetSetTooltip());
            }

            __result = text.ToString();

            return false;
        }

        [UsedImplicitly]
        [HarmonyPriority(Priority.Last)]
        public static void Postfix(ref string __result, ItemDrop.ItemData item)
        {
            if (item == null)
                return;

            __result = EIDFLegacy.FormatCrafterName(__result);

            if (item.IsMagicCraftingMaterial() || item.IsRunestone())
            {
                string rarityDisplay = EpicLoot.GetRarityDisplayName(item.GetCraftingMaterialRarity());
                __result = $"<color={item.GetCraftingMaterialRarityColor()}>{rarityDisplay} " +
                    $"$mod_epicloot_craftingmaterial\n</color>" + __result;
            }

            if (!item.IsMagic())
            {
                var text = new StringBuilder();

                // Set stuff
                if (item.IsSetItem())
                {
                    // Remove old set stuff
                    int index = __result.IndexOf("\n\n$item_seteffect", StringComparison.InvariantCulture);
                    if (index >= 0)
                    {
                         __result = __result.Remove(index);
                    }

                    // Create new
                    text.Append(item.GetSetTooltip());
                }

                __result += text.ToString();
            }
            
            __result = __result.Replace("<color=orange>", "<color=#add8e6ff>");
            __result = __result.Replace("<color=yellow>", "<color=#add8e6ff>");
            __result = __result.Replace("\n\n\n", "\n\n");
        }

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
            bool coinHoarderMagic = CoinHoarder.HasCoinHoarder(out float coinHoarderEffectValue);
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

    public static class AugaTooltipPreprocessor
    {
        public static Tuple<string, string> PreprocessTooltipStat(ItemDrop.ItemData item, string label, string value)
        {
            var localPlayer = Player.m_localPlayer;

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

                    case "$item_crafter":
                        value = EIDFLegacy.GetCrafterName(value);
                        break;
                }
                
                if (label.StartsWith("$item_movement_modifier") &&
                    (magicItem.HasEffect(MagicEffectType.RemoveSpeedPenalty) ||
                    magicItem.HasEffect(MagicEffectType.ModifyMovementSpeed)))
                {
                    int colorIndex = label.IndexOf("<color", StringComparison.Ordinal);
                    if (colorIndex >= 0)
                    {
                        var sb = new StringBuilder(label);
                        sb.Remove(colorIndex, "<color=#XXXXXX>".Length);
                        sb.Insert(colorIndex, $"<color={magicColor}>");

                        string itemMovementModDisplay = MagicItemTooltip_ItemDrop_Patch.GetMovementModifier(
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
                string itemEitrRegenModDisplay = MagicItemTooltip_ItemDrop_Patch.GetEitrRegenModifier(item, magicItem, out _);

                float equipEitrRegenModifier = localPlayer.GetEquipmentEitrRegenModifier() * 100.0f;
                float equipMagicEitrRegenModifier = localPlayer.GetTotalActiveMagicEffectValue(MagicEffectType.ModifyEitrRegen);
                float totalEitrRegenModifier = equipEitrRegenModifier + equipMagicEitrRegenModifier;
                if (magicEitrRegen && magicItem != null)
                    itemEitrRegenModDisplay = $"<color={magicItem.GetColorString()}>{itemEitrRegenModDisplay}</color>";
                label = $"$item_eitrregen_modifier: {itemEitrRegenModDisplay} " +
                    $"($item_total: <color={Auga.API.Brown3}>{totalEitrRegenModifier:+0;-0}%</color>)";
            }

            switch (label)
            {
                case "$item_crafter":
                    value = EIDFLegacy.GetCrafterName(value);
                    break;
            }

            return new Tuple<string, string>(label, value);
        }
    }
}
