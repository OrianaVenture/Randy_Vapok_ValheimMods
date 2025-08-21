using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace EpicLootAPI;

[Serializable][PublicAPI]
public class GuaranteedMagicEffect
{
    public string Type;
    public ValueDef? Values;
    public GuaranteedMagicEffect(string type, ValueDef values)
    {
        Type = type;
        Values = values;
    }
    public GuaranteedMagicEffect(string type, float min = 1, float max = 1, float increment = 1) : this(type, new ValueDef(min, max, increment)){}
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
        this.type = type;
        LegendaryItems.Add(this);
    }

    private LegendaryType type;

    internal static readonly List<LegendaryInfo> LegendaryItems = new();
    internal static readonly Method API_AddLegendaryItem = new ("AddLegendaryItem");
    internal static readonly Method API_UpdateLegendaryItem = new ("UpdateLegendaryItem");

    public static void RegisterAll()
    {
        foreach(var item in new List<LegendaryInfo>(LegendaryItems)) item.Register();
    }
    public bool Register()
    {
        string data = JsonConvert.SerializeObject(this);
        object? result = API_AddLegendaryItem.Invoke(type.ToString(), data);
        if (result is not string key) return false;
        RunTimeRegistry.Register(this, key);
        LegendaryItems.Remove(this);
        return true;
    }

    public bool Update()
    {
        if (!RunTimeRegistry.TryGetValue(this, out string key)) return false;
        string data = JsonConvert.SerializeObject(this);
        object? result = API_UpdateLegendaryItem.Invoke(key, data);
        return (bool)(result ?? false);
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
    public SetBonusInfo(int count, string type, ValueDef values)
    {
        Count = count;
        Effect = new GuaranteedMagicEffect(type, values);
    }
    public SetBonusInfo(int count, string type, float min, float max, float increment) : this (count, type, new ValueDef(min, max, increment)){}
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
        this.type = type;
        LegendarySets.Add(this);
    }
    
    private LegendaryType type;
    internal static readonly List<LegendarySetInfo> LegendarySets = new();
    internal static readonly Method API_AddLegendarySet = new ("AddLegendarySet");
    internal static readonly Method API_UpdateLegendarySet = new ("UpdateLegendarySet");
    
    public static void RegisterAll()
    {
        foreach(var set in new List<LegendarySetInfo>(LegendarySets)) set.Register();
    }
    public bool Register()
    {
        string data = JsonConvert.SerializeObject(this);
        object? result = API_AddLegendarySet.Invoke(type.ToString(), data);
        if (result is not string key) return false;
        RunTimeRegistry.Register(this, key);
        return true;
    }

    public bool Update()
    {
        if (!RunTimeRegistry.TryGetValue(this, out string key)) return false;
        string data = JsonConvert.SerializeObject(this);   
        object? result = API_UpdateLegendarySet.Invoke(key, data);
        return (bool)(result ?? false);
    }
}