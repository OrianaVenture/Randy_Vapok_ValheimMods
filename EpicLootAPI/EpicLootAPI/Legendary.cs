using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace EpicLootAPI;

[Serializable][PublicAPI]
public class GuaranteedMagicEffect
{
    public string Type;
    public MagicItemEffectDefinition.ValueDef? Values;
    public GuaranteedMagicEffect(string type, MagicItemEffectDefinition.ValueDef values)
    {
        Type = type;
        Values = values;
    }
    public GuaranteedMagicEffect(string type, float min = 1, float max = 1, float increment = 1) : this(type, new MagicItemEffectDefinition.ValueDef(min, max, increment)){}
}

[Serializable][PublicAPI]
public class TextureReplacement
{
    public string ItemID;
    public string MainTexture;
    public string ChestTex;
    public string LegsTex;

    public TextureReplacement(string itemID, string mainTex = "", string chestTex = "", string legsTex = "")
    {
        ItemID = itemID;
        MainTexture = mainTex;
        ChestTex = chestTex;
        LegsTex = legsTex;
    }
}

[Serializable][PublicAPI]
public class LegendaryInfo
{
    public string ID;
    public string Name;
    public string Description;
    public MagicItemEffectRequirements Requirements = new ();
    public List<GuaranteedMagicEffect> GuaranteedMagicEffects = new List<GuaranteedMagicEffect>();
    public int GuaranteedEffectCount = -1;
    public float SelectionWeight = 1;
    public string EquipFx = "";
    public FxAttachMode EquipFxMode = FxAttachMode.Player;
    public List<TextureReplacement> TextureReplacements = new List<TextureReplacement>();
    public bool IsSetItem;
    public bool Enchantable;
    public List<RecipeRequirement> EnchantCost = new List<RecipeRequirement>();
    public LegendaryInfo(LegendaryType type, string ID, string name, string description)
    {
        this.ID = ID;
        Name = name;
        Description = description;

        switch (type)
        {
            case LegendaryType.Legendary:
                EpicLoot.LegendaryConfig.LegendaryItems.Add(this);
                break;
            case LegendaryType.Mythic:
                EpicLoot.LegendaryConfig.MythicItems.Add(this);
                break;
        }
    }
}

[PublicAPI]
public enum LegendaryType
{
    Legendary, 
    Mythic
}

[Serializable][PublicAPI]
public class SetBonusInfo
{
    public int Count;
    public GuaranteedMagicEffect Effect;
    public SetBonusInfo(int count, string type, MagicItemEffectDefinition.ValueDef values)
    {
        Count = count;
        Effect = new GuaranteedMagicEffect(type, values);
    }
    public SetBonusInfo(int count, string type, float min, float max, float increment) : this (count, type, new MagicItemEffectDefinition.ValueDef(min, max, increment)){}
}

[Serializable][PublicAPI]
public class LegendarySetInfo
{
    public string ID;
    public string Name;
    public List<string> LegendaryIDs = new List<string>();
    public List<SetBonusInfo> SetBonuses = new List<SetBonusInfo>();
    public LegendarySetInfo(LegendaryType type, string ID, string name)
    {
        this.ID = ID;
        Name = name;

        switch (type)
        {
            case LegendaryType.Legendary:
                EpicLoot.LegendaryConfig.LegendarySets.Add(this);
                break;
            case LegendaryType.Mythic:
                EpicLoot.LegendaryConfig.MythicSets.Add(this);
                break;
        }
    }
}

[Serializable]
internal class LegendaryItemConfig
{
    public List<LegendaryInfo> LegendaryItems = new List<LegendaryInfo>();
    public List<LegendarySetInfo> LegendarySets = new List<LegendarySetInfo>();
    public List<LegendaryInfo> MythicItems = new List<LegendaryInfo>();
    public List<LegendarySetInfo> MythicSets = new List<LegendarySetInfo>();
    public bool HasValues()
    {
        if (LegendaryItems.Count > 0) return true;
        if (LegendarySets.Count > 0) return true;
        if (MythicItems.Count > 0) return true;
        if (MythicSets.Count > 0) return true;

        return false;
    }
}