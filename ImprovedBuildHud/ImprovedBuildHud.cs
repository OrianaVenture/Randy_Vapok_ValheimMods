using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;

namespace ImprovedBuildHud;

public static class ImprovedBuildHudConfig
{
    public static ConfigEntry<string> InventoryAmountFormat;
    public static ConfigEntry<string> InventoryAmountColor;
    public static ConfigEntry<string> CanBuildAmountFormat;
    public static ConfigEntry<string> CanBuildAmountColor;
}

[BepInPlugin(PluginId, "Improved Build HUD", "1.0.9")]
public class ImprovedBuildHud : BaseUnityPlugin
{
    public const string PluginId = "randyknapp.mods.improvedbuildhud";

    private Harmony _harmony;

    public void Awake()
    {
        ImprovedBuildHudConfig.InventoryAmountFormat = Config.Bind("General", "Inventory Amount Format", "({0})",
            "Format for the amount of items in the player inventory to show after the required amount. " +
            "Uses standard C# format rules. Leave empty to hide altogether.");
        ImprovedBuildHudConfig.InventoryAmountColor = Config.Bind("General", "Inventory Amount Color", "#add8e6ff",
            "Color to set the inventory amount after the requirement amount. " +
            "Leave empty to set no color. You can use the #XXXXXX hex color format.");
        ImprovedBuildHudConfig.CanBuildAmountFormat = Config.Bind("General", "Build Amount Format", "({0})",
            "Format for the amount of times you can build the currently selected item with your current inventory. " +
            "Uses standard C# format rules. Leave empty to hide altogether.");
        ImprovedBuildHudConfig.CanBuildAmountColor = Config.Bind("General", "Build Amount Color", "white",
            "Color to set the can-build amount. Leave empty to set no color. You can use the #XXXXXX hex color format.");

        _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), PluginId);
    }
}
