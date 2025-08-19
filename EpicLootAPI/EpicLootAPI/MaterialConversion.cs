using System;
using System.Collections.Generic;
using System.ComponentModel;
using JetBrains.Annotations;

namespace EpicLootAPI;

[Serializable][PublicAPI]
public enum MaterialConversionType
{
    Upgrade,
    Convert,
    Junk
}

[Serializable][PublicAPI]
public class MaterialConversionRequirement
{
    public string Item;
    public int Amount;

    public MaterialConversionRequirement(string item, int amount = 1)
    {
        Item = item;
        Amount = amount;
    }
}

[Serializable][PublicAPI]
public class MaterialConversion
{
    public string Name;
    public string Product;
    public int Amount;
    public MaterialConversionType Type;
    public List<MaterialConversionRequirement> Resources = new();
    [Description("Creates a new material conversion definition.")]
    public MaterialConversion(MaterialConversionType type, string name, string product, int amount = 1)
    {
        Name = name;
        Product = product;
        Amount = amount;
        Type = type;

        EpicLoot.MaterialConversions.Add(this);
    }
}