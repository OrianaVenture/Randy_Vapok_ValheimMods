using System;
using JetBrains.Annotations;
using System.Collections.Generic;

namespace EpicLootAPI;

[Serializable][PublicAPI]
public class RecipeRequirement
{
    public string item;
    public int amount;
    public RecipeRequirement(string item, int amount = 1)
    {
        this.item = item;
        this.amount = amount;
    }
}

[Serializable][PublicAPI]
public class CustomRecipe
{
    public string name;
    public string item;
    public int amount;
    public string craftingStation;
    public int minStationLevel = 1;
    public bool enabled = true;
    public string repairStation = "";
    public List<RecipeRequirement> resources = new List<RecipeRequirement>();
    
    public CustomRecipe(string name, string item, CraftingTable craftingTable, int amount = 1)
    {
        this.name = name;
        this.item = item;
        this.amount = amount;
        craftingStation = craftingTable.GetInternalName();
        EpicLoot.Recipes.recipes.Add(this);
    }
}

[PublicAPI]
public enum CraftingTable
{
    [InternalName("piece_workbench")] Workbench,
    [InternalName("piece_cauldron")] Cauldron,
    [InternalName("forge")] Forge,
    [InternalName("piece_artisanstation")] ArtisanTable,
    [InternalName("piece_stonecutter")] StoneCutter,
    [InternalName("piece_magetable")] MageTable,
    [InternalName("blackforge")] BlackForge,
    [InternalName("piece_preptable")] FoodPreparationTable,
    [InternalName("piece_MeadCauldron")] MeadKetill,
}

internal class InternalName : Attribute
{
    public readonly string internalName;
    public InternalName(string internalName) => this.internalName = internalName;
}

[Serializable]
internal class RecipesConfig
{
    public List<CustomRecipe> recipes = new List<CustomRecipe>();
    public bool HasValues() => recipes.Count > 0;
}