using System;
using System.Collections.Generic;
using System.ComponentModel;
using JetBrains.Annotations;

namespace EpicLootAPI;

[PublicAPI]
public enum FxAttachMode
{
    None,
    Player,
    ItemRoot,
    EquipRoot
}
[PublicAPI]
public enum ItemRarity
{
    Magic,
    Rare,
    Epic,
    Legendary,
    Mythic
}

[Serializable][PublicAPI]
public class MagicItemEffect
{
    public int Version = 1;
    public string EffectType;
    public float EffectValue;
    public MagicItemEffect(string type, float value = 1)
    {
        EffectType = type;
        EffectValue = value;
    }
}

[Serializable][PublicAPI]
public class ValueDef
{
    public float MinValue;
    public float MaxValue;
    public float Increment;
            
    public ValueDef(){}

    public ValueDef(float min, float max, float increment)
    {
        MinValue = min;
        MaxValue = max;
        Increment = increment;
    }
}

[Serializable][PublicAPI]
public class ValuesPerRarityDef
{
    public ValueDef? Magic;
    public ValueDef? Rare;
    public ValueDef? Epic;
    public ValueDef? Legendary;
    public ValueDef? Mythic;
}

[Serializable][PublicAPI]
public class MagicItemEffectDefinition
{
    public string Type;
    public string DisplayText;
    public string Description;
    public MagicItemEffectRequirements Requirements = new MagicItemEffectRequirements();
    public ValuesPerRarityDef ValuesPerRarity = new ValuesPerRarityDef();
    public float SelectionWeight = 1;
    public bool CanBeAugmented = true;
    public bool CanBeDisenchanted = true;
    public string Comment = "";
    public List<string> Prefixes = new List<string>();
    public List<string> Suffixes = new List<string>();
    public string EquipFx = "";
    public FxAttachMode EquipFxMode = FxAttachMode.Player;
    public string Ability = "";
        
    [Description("Adds your new magic item definition to a list, use RegisterMagicItems() to send to epic loot")]
    public MagicItemEffectDefinition(string effectType, string displayText = "", string description = "")
    {
        Type = effectType;
        DisplayText = displayText;
        Description = description;

        EpicLoot.MagicEffects.Add(this);
    }
}

[Serializable][PublicAPI]
public class MagicItemEffectRequirements
{
    private static List<string> _flags = new List<string>();
    public bool NoRoll;
    public bool ExclusiveSelf = true;
    public List<string> ExclusiveEffectTypes = new List<string>();
    public List<string> MustHaveEffectTypes = new List<string>();
    public List<string> AllowedItemTypes = new List<string>();
    public List<string> ExcludedItemTypes = new List<string>();
    public List<ItemRarity> AllowedRarities = new List<ItemRarity>();
    public List<ItemRarity> ExcludedRarities = new List<ItemRarity>();
    public List<Skills.SkillType> AllowedSkillTypes = new List<Skills.SkillType>();
    public List<Skills.SkillType> ExcludedSkillTypes = new List<Skills.SkillType>();
    public List<string> AllowedItemNames = new List<string>();
    public List<string> ExcludedItemNames = new List<string>();
    public bool ItemHasPhysicalDamage;
    public bool ItemHasElementalDamage;
    public bool ItemHasChopDamage;
    public bool ItemUsesDurability;
    public bool ItemHasNegativeMovementSpeedModifier;
    public bool ItemHasBlockPower;
    public bool ItemHasParryPower;
    public bool ItemHasNoParryPower;
    public bool ItemHasArmor;
    public bool ItemHasBackstabBonus;
    public bool ItemUsesStaminaOnAttack;
    public bool ItemUsesEitrOnAttack;
    public bool ItemUsesHealthOnAttack;
    public bool ItemUsesDrawStaminaOnAttack;

    public List<string> CustomFlags = new();

    public void AddAllowedItemTypes(params ItemDrop.ItemData.ItemType[] types)
    {
        foreach(var type in types) AllowedItemTypes.Add(type.ToString());
    }

    public void AddExcludedItemTypes(params ItemDrop.ItemData.ItemType[] types)
    {
        foreach(var type in types) ExcludedItemTypes.Add(type.ToString());
    }
}