using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;

namespace EpicLootAPI;

public static class Helpers
{
    [PublicAPI]
    public static void Add<T>(this List<T> list, params T[] items) => list.AddRange(items);
    
    /// <summary>
    /// Helper function to add into dictionary of lists
    /// </summary>
    /// <param name="dict">Dictionary T key, List V values</param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="V"></typeparam>
    internal static void AddOrSet<T, V>(this Dictionary<T, List<V>> dict, T key, V value)
    {
        if (!dict.ContainsKey(key)) dict[key] = new List<V>();
        dict[key].Add(value);
    }
    
    [PublicAPI]
    public static void AddMinion(this List<BountyMinion> list, string ID, int count) => list.Add(new BountyMinion(ID, count));
    
    [PublicAPI]
    public static void Add(this List<ItemAmount> list, string item, int amount = 1) => list.Add(new ItemAmount(item, amount));
    
    [PublicAPI]
    public static void Add(this List<MaterialConversionRequirement> list, string item, int amount = 1) =>
        list.Add(new MaterialConversionRequirement(item, amount));
    
    internal static string GetInternalName(this CraftingTable table)
    {
        Type type = typeof(CraftingTable);
        MemberInfo[] memInfo = type.GetMember(table.ToString());
        if (memInfo.Length <= 0) return table.ToString();
        var attr = (InternalName)Attribute.GetCustomAttribute(memInfo[0], typeof(InternalName));
        return attr != null ? attr.internalName : table.ToString();
    }
    
    [PublicAPI]
    public static void Add(this List<RecipeRequirement> list, string item, int amount = 1) =>
        list.Add(new RecipeRequirement(item, amount));
    
    [PublicAPI]
    public static void Add(this List<SetBonusInfo> list, int count, string type, float min = 1, float max = 1, float increment = 1) =>
        list.Add(new SetBonusInfo(count, type, min, max, increment));
    
    [PublicAPI]
    public static void Add(this List<GuaranteedMagicEffect> list, string type, float min = 1, float max = 1,
        float increment = 1) => list.Add(new GuaranteedMagicEffect(type, min, max, increment));
    
    [PublicAPI]
    public static void Set(this ValueDef? def, float min, float max, float increment)
    {
        def ??= new ValueDef();
        def.MinValue = min;
        def.MaxValue = max;
        def.Increment = increment;
    }
}