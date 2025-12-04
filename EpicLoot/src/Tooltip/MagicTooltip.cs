using System.Text;

namespace EpicLoot;

public partial class MagicTooltip(ItemDrop.ItemData item, MagicItem magicItem, int qualityLevel)
{
    private static Player localPlayer => Player.m_localPlayer;
    private static readonly StringBuilder text = new StringBuilder(256);

    private readonly string magicColor = magicItem.GetColorString();
    private readonly string itemTypeName = magicItem.GetItemTypeName(item.Extended());
    private readonly float skillLevel = localPlayer.GetSkillLevel(item.m_shared.m_skillType);
    
    public string GetTooltip()
    {
        text.Clear();

        MagicDisplayName();
        MagicSetLabel();
        Description();
        text.Append("\n");
        
        DLC();
        ItemDrop.ItemData.AddHandedTip(item, text);
        CrafterID();
        Teleportable();
        Valuable();
        Weight();
        Quality();
        Durability();
        
        switch (item.m_shared.m_itemType)
        {
            case ItemDrop.ItemData.ItemType.Consumable:
                if (item.m_shared.m_food > 0.0)
                {
                    FoodHealth();
                    FoodStamina();
                    FoodBurn();
                    FoodRegen();
                }
                break;
            case ItemDrop.ItemData.ItemType.OneHandedWeapon:
            case ItemDrop.ItemData.ItemType.Bow:
            case ItemDrop.ItemData.ItemType.TwoHandedWeapon:
            case ItemDrop.ItemData.ItemType.TwoHandedWeaponLeft:
            case ItemDrop.ItemData.ItemType.Torch:
                Damage();
                AttackStaminaUse();
        
                //TODO: explain why these are here, else, remove
                Dodge();
                Offset();
                ChainLightning();
                Apportation();
                //
        
                EitrUse();
                HealthUse();
                DrawStaminaUse();
                BlockArmor();
                Parry();
                Knockback();
                Backstab();
                Projectile();
                break;
            case ItemDrop.ItemData.ItemType.Shield:
                BlockArmor();
                ShieldBlockAndParry();
                DamageModifiers();
                break;
            case ItemDrop.ItemData.ItemType.Helmet:
            case ItemDrop.ItemData.ItemType.Chest:
            case ItemDrop.ItemData.ItemType.Legs:
            case ItemDrop.ItemData.ItemType.Shoulder:
                Armor();
                DamageModifiers();
                break;
            case ItemDrop.ItemData.ItemType.Ammo:
                AmmoDamage();
                Knockback();
                break;
            case ItemDrop.ItemData.ItemType.Trinket:
                MaxAdrenaline();
                break;
        }

        StatusEffect();
        ChainTooltip();
        EitrRegen();
        Movement();
        
        MagicEffects();
        
        Set();
        FullAdrenaline();
        
        return text.ToString();
    }

    private void MagicDisplayName()
    {
        // removed duplicate display name if it does not have unique name
        if (magicItem.IsUniqueLegendary())
        {
            text.Append($"<color={magicColor}>{magicItem.GetRarityDisplay()} {itemTypeName}</color>\n");
        }
    }

    private void MagicSetLabel()
    {
        if (item.IsMagicSetItem())
        {
            switch (item.GetRarity())
            {
                case ItemRarity.Legendary:
                    text.Append($"<color={EpicLoot.GetSetItemColor()}>$mod_epicloot_legendarysetlabel</color>\n");
                    break;
                case ItemRarity.Mythic:
                    text.Append($"<color={EpicLoot.GetSetItemColor()}>$mod_epicloot_mythicsetlabel</color>\n");
                    break;
            }
        }
    }
    
    private void Description()
    {
        text.Append(item.GetDescription());
    }

    private void DLC()
    {
        if (item.m_shared.m_dlc.Length > 0)
        {
            text.Append("\n<color=#00ffffff>$item_dlc</color>");
        }
    }

    private void CrafterID()
    {
        if (item.m_crafterID != 0L)
        {
            text.AppendFormat("\n$item_crafter: <color=orange>{0}</color>", item.m_crafterName);
        }
    }

    private void Teleportable()
    {
        if (!item.m_shared.m_teleportable)
        {
            text.Append("\n<color=orange>$item_noteleport</color>");
        }
    }

    private void Valuable()
    {
        if (item.m_shared.m_value > 0)
        {
            text.AppendFormat("\n$item_value: <color=orange>{0} ({1})</color>", item.GetValue(), item.m_shared.m_value);
        }
    }

    private void Weight()
    {
        bool hasWeightModifier = magicItem.HasEffect(MagicEffectType.ReduceWeight) ||
                                 magicItem.HasEffect(MagicEffectType.Weightless);
        string weightColor = hasWeightModifier ? magicColor : "orange";
        text.Append($"\n$item_weight: <color={weightColor}>{item.GetWeight():0.0}</color>");
    }

    private void Quality()
    {
        if (item.m_shared.m_maxQuality > 1)
        {
            text.AppendFormat("\n$item_quality: <color=orange>{0}</color>", qualityLevel);
        }
    }

    private void Durability()
    {
        bool isIndestructible = magicItem.HasEffect(MagicEffectType.Indestructible);
        if (!isIndestructible && item.m_shared.m_useDurability)
        {
            bool hasDurabilityModifier = magicItem.HasEffect(MagicEffectType.ModifyDurability);
            
            string maxDurabilityColor1 = hasDurabilityModifier ? magicColor : "orange";
            string maxDurabilityColor2 = hasDurabilityModifier ? magicColor : "yellow";

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
        else if (isIndestructible)
        {
            text.Append($"\n$item_durability: <color={magicColor}>$mod_epicloot_me_indestructible_display</color>");
        }
    }
    
    private void StatusEffect()
    {
        string statusEffectTooltip2 = item.GetStatusEffectTooltip(qualityLevel, skillLevel);
        if (statusEffectTooltip2.Length > 0)
        {
            text.Append("\n\n");
            text.Append(statusEffectTooltip2);
        }
    }

    private void ChainTooltip()
    {
        string chainTooltip = item.GetChainTooltip(qualityLevel, skillLevel);
        if (chainTooltip.Length > 0)
        {
            text.Append("\n\n");
            text.Append(chainTooltip);
        }
    }
    
    private void MagicEffects()
    {
        text.AppendLine(magicItem.GetTooltip());
    }

    private void Set()
    {
        // Either EpicLoot set or base-game set
        if (item.IsSetItem())
        {
            text.Append(item.GetSetTooltip());
        }
    }

    private void FullAdrenaline()
    {
        if (item.m_shared.m_fullAdrenalineSE != null)
        {
            text.Append($"\n$item_fulladrenaline: <color=orange>{item.m_shared.m_fullAdrenalineSE.GetTooltipString()}</color>");
        }
    }
}