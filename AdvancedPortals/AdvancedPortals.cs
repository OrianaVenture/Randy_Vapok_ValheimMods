using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Common;
using HarmonyLib;
using JetBrains.Annotations;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using Newtonsoft.Json;
using UnityEngine;

namespace AdvancedPortals;

[BepInPlugin(PluginId, DisplayName, Version)]
[BepInIncompatibility("com.github.xafflict.UnrestrictedPortals")]
[BepInDependency(TargetPortalName, BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency(Jotunn.Main.ModGuid)]
[NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
public class AdvancedPortals : BaseUnityPlugin
{
    public const string PluginId = "randyknapp.mods.advancedportals";
    public const string DisplayName = "Advanced Portals";
    public const string Version = "1.1.0";

    public static string AncientPortalName = "portal_ancient";
    public static string ObsidianPortalName = "portal_obsidian";
    public static string BlackmarblePortalName = "portal_blackmarble";
    public static readonly string[] PortalPrefabs = { AncientPortalName, ObsidianPortalName, BlackmarblePortalName };

    private static ConfigEntry<bool> _ancientPortalEnabled;
    private static ConfigEntry<string> _ancientPortalRecipe;
    private static ConfigEntry<string> _ancientPortalAllowedItems;
    private static ConfigEntry<bool> _ancientPortalAllowEverything;
    private static ConfigEntry<bool> _obsidianPortalEnabled;
    private static ConfigEntry<string> _obsidianPortalRecipe;
    private static ConfigEntry<string> _obsidianPortalAllowedItems;
    private static ConfigEntry<bool> _obsidianPortalAllowEverything;
    private static ConfigEntry<bool> _obsidianPortalAllowPreviousPortalItems;
    private static ConfigEntry<bool> _blackMarblePortalEnabled;
    private static ConfigEntry<string> _blackMarblePortalRecipe;
    private static ConfigEntry<string> _blackMarblePortalAllowedItems;
    private static ConfigEntry<bool> _blackMarblePortalAllowEverything;
    private static ConfigEntry<bool> _blackMarblePortalAllowPreviousPortalItems;

    private static AdvancedPortals _instance;
    private Harmony _harmony = new(PluginId);
    private static string ConfigFileName = PluginId + ".cfg";
    private static string ConfigFileFullPath = BepInEx.Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;

    internal const string TargetPortalName = "org.bepinex.plugins.targetportal";
    public static bool TargetPortalInstalled = false;

    [UsedImplicitly]
    public void Awake()
    {
        _instance = this;

        TargetPortalInstalled = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(TargetPortalName);

        bool save = Config.SaveOnConfigSet;
        Config.SaveOnConfigSet = false;

        string ancientPortal = "Portal 1 - Ancient";
        string obsidianPortal = "Portal 2 - Obsidian";
        string marblePortal = "Portal 3 - Black Marble";

        /*AddConfig(ancientPortal, "Ancient Portal Enabled",
            "Enable the Ancient Portal",
            true, true, ref _ancientPortalEnabled);*/
        AddConfig("Ancient Portal Recipe", ancientPortal,
            "The items needed to build the Ancient Portal. A comma separated list of ITEM:QUANTITY pairs separated by a colon.",
            true, "ElderBark:20,Iron:5,SurtlingCore:2", ref _ancientPortalRecipe);
        AddConfig("Ancient Portal Allowed Items", ancientPortal,
            "A comma separated list of the item types allowed through the Ancient Portal",
            true, "Copper, CopperOre, CopperScrap, Tin, TinOre, Bronze, BronzeScrap", ref _ancientPortalAllowedItems);
        AddConfig("Ancient Portal Allow Everything", ancientPortal,
            "Allow all items through the Ancient Portal (overrides Allowed Items)",
            true, false, ref _ancientPortalAllowEverything);

        /*AddConfig(obsidianPortal, "Obsidian Portal Enabled",
            "Enable the Obsidian Portal",
            true, true, ref _obsidianPortalEnabled);*/
        AddConfig("Obsidian Portal Recipe", obsidianPortal,
            "The items needed to build the Obsidian Portal. A comma separated list of ITEM:QUANTITY pairs separated by a colon.",
            true, "Obsidian:20,Silver:5,SurtlingCore:2", ref _obsidianPortalRecipe);
        AddConfig("Obsidian Portal Allowed Items", obsidianPortal,
            "A comma separated list of the item types allowed through the Obsidian Portal",
            true, "Iron, IronScrap", ref _obsidianPortalAllowedItems);
        AddConfig("Obsidian Portal Allow Everything", obsidianPortal,
            "Allow all items through the Obsidian Portal (overrides Allowed Items)",
            true, false, ref _obsidianPortalAllowEverything);
        AddConfig("Obsidian Portal Use All Previous", obsidianPortal,
            "Additionally allow all items from the Ancient Portal",
            true, true, ref _obsidianPortalAllowPreviousPortalItems);

        /*AddConfig(marblePortal, "Black Marble Portal Enabled",
            "Enable the Black Marble Portal",
            true, true, ref _blackMarblePortalEnabled);*/
        AddConfig("Black Marble Portal Recipe", marblePortal,
            "The items needed to build the Black Marble Portal. A comma separated list of ITEM:QUANTITY pairs separated by a colon.",
            true, "BlackMarble:20,BlackMetal:5,Eitr:2", ref _blackMarblePortalRecipe);
        AddConfig("Black Marble Portal Allowed Items", marblePortal,
            "A comma separated list of the item types allowed through the Black Marble Portal",
            true, "Silver, SilverOre", ref _blackMarblePortalAllowedItems);
        AddConfig("Black Marble Portal Allow Everything", marblePortal,
            "Allow all items through the Black Marble Portal (overrides Allowed Items)",
            true, true, ref _blackMarblePortalAllowEverything);
        AddConfig("Black Marble Portal Use All Previous", marblePortal,
            "Additionally allow all items from the Obsidian and Ancient Portal",
            true, true, ref _blackMarblePortalAllowPreviousPortalItems);

        Config.SaveOnConfigSet = save;
        Config.Save();

        _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), PluginId);

        AddPortals();
        SetupWatcher();

        PrefabManager.OnVanillaPrefabsAvailable += () => UpdateConfigurations();
    }

    private static void UpdateConfigurations()
    {
        var ancientPortal = PrefabManager.Instance.GetPrefab(AncientPortalName);
        var obsidianPortal = PrefabManager.Instance.GetPrefab(ObsidianPortalName);
        var marblePortal = PrefabManager.Instance.GetPrefab(BlackmarblePortalName);

        var teleportAncient = ancientPortal.GetComponent<AdvancedPortal>();
        var teleportObsidian = obsidianPortal.GetComponent<AdvancedPortal>();
        var teleportMarble = marblePortal.GetComponent<AdvancedPortal>();

        teleportAncient.m_allowAllItems = _ancientPortalAllowEverything.Value;
        var ancientAllowed = StringToSet(_ancientPortalAllowedItems.Value);
        AdvancedPortal.SetAllowedItems(AncientPortalName, ancientAllowed);

        teleportObsidian.m_allowAllItems = _obsidianPortalAllowEverything.Value;
        var obsidianAllowed = StringToSet(_obsidianPortalAllowedItems.Value);
        if (_obsidianPortalAllowPreviousPortalItems.Value)
        {
            obsidianAllowed = obsidianAllowed.Union(ancientAllowed).ToHashSet();
        }
        AdvancedPortal.SetAllowedItems(ObsidianPortalName, obsidianAllowed);

        teleportMarble.m_allowAllItems = _blackMarblePortalAllowEverything.Value;
        var marbleAllowed = StringToSet(_blackMarblePortalAllowedItems.Value);
        if (_blackMarblePortalAllowPreviousPortalItems.Value)
        {
            marbleAllowed = marbleAllowed.Union(obsidianAllowed).ToHashSet();
        }
        AdvancedPortal.SetAllowedItems(BlackmarblePortalName, marbleAllowed);

        var pieceAncient = ancientPortal.GetComponent<Piece>();
        var pieceObsidian = obsidianPortal.GetComponent<Piece>();
        var pieceMarble = marblePortal.GetComponent<Piece>();

        var ancientConfig = MakeRecipeFromConfig("Ancient Portal", _ancientPortalRecipe.Value);
        var obsidianConfig = MakeRecipeFromConfig("Obsidian Portal", _obsidianPortalRecipe.Value);
        var marbleConfig = MakeRecipeFromConfig("Black Marble Portal", _blackMarblePortalRecipe.Value);

        pieceObsidian.m_resources = obsidianConfig;
        pieceAncient.m_resources = ancientConfig;
        pieceMarble.m_resources = marbleConfig;

        PrefabManager.OnVanillaPrefabsAvailable -= () => UpdateConfigurations();
        
    }

    public static HashSet<string> StringToSet(string str)
    {
        var set = new HashSet<string>();

        if (!str.IsNullOrWhiteSpace())
        {
            List<string> keys = str.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            for (var lcv = 0; lcv < keys.Count; lcv++)
            {
                set.Add(keys[lcv].Trim());
            }
        }

        return set;
    }

    private static Piece.Requirement[] MakeRecipeFromConfig(string portalName, string configString)
    {
        var recipe = new List<RecipeRequirementConfig>();

        var entries = configString.Replace(" ", "").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var entry in entries)
        {
            var parts = entry.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
            {
                _instance.Logger.LogError($"Incorrectly formatted recipe for {portalName}! " +
                    $"Should be 'ITEM:QUANITY,ITEM2:QUANTITY' etc.");
                return new Piece.Requirement[] { };
            }

            var item = parts[0];
            var amountString = parts[1];
            if (!int.TryParse(amountString, out var amount))
            {
                _instance.Logger.LogError($"Incorrectly formatted recipe for {portalName}! " +
                    $"Should be 'ITEM:QUANITY,ITEM2:QUANTITY' etc.");
                return new Piece.Requirement[] { };
            }
            recipe.Add(new RecipeRequirementConfig() {item = item, amount = amount});
        }

        if (recipe.Count == 0)
        {
            _instance.Logger.LogError($"Incorrectly formatted recipe for {portalName}! " +
                $"Must have at least one entry, should be 'ITEM:QUANITY,ITEM2:QUANTITY' etc.");
        }

        var resources = new List<Piece.Requirement>();
        foreach (var resource in recipe)
        {
            var resourcePrefab = PrefabManager.Instance.GetPrefab(resource.item);

            resources.Add(new Piece.Requirement()
            {
                m_resItem = resourcePrefab.GetComponent<ItemDrop>(),
                m_amount = resource.amount
            });
        }
        return resources.ToArray();
    }

    private readonly ConfigurationManagerAttributes AdminConfig = new ConfigurationManagerAttributes { IsAdminOnly = true };
    private readonly ConfigurationManagerAttributes ClientConfig = new ConfigurationManagerAttributes { IsAdminOnly = false };

    private void AddConfig<T>(string key, string section, string description, bool synced, T value, ref ConfigEntry<T> configEntry)
    {
        string extendedDescription = GetExtendedDescription(description, synced);
        configEntry = Config.Bind(section, key, value,
            new ConfigDescription(extendedDescription, null, synced ? AdminConfig : ClientConfig));
    }

    public string GetExtendedDescription(string description, bool synchronizedSetting)
    {
        return description + (synchronizedSetting ? " [Synced with Server]" : " [Not Synced with Server]");
    }

    [UsedImplicitly]
    public void OnDestroy()
    {
        _instance = null;
        Config.Save();
    }

    private void SetupWatcher()
    {
        FileSystemWatcher watcher = new(BepInEx.Paths.ConfigPath, ConfigFileName);
        watcher.Changed += ReadConfigValues;
        watcher.Created += ReadConfigValues;
        watcher.Renamed += ReadConfigValues;
        watcher.IncludeSubdirectories = true;
        watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
        watcher.EnableRaisingEvents = true;
    }

    private DateTime _lastReloadTime;
    private const long RELOAD_DELAY = 10000000; // One second

    private void ReadConfigValues(object sender, FileSystemEventArgs e)
    {
        var now = DateTime.Now;
        var time = now.Ticks - _lastReloadTime.Ticks;
        if (!File.Exists(ConfigFileFullPath) || time < RELOAD_DELAY) return;

        try
        {
            _instance.Logger.LogInfo("Attempting to reload configuration...");
            Config.Reload();
        }
        catch
        {
            _instance.Logger.LogError($"There was an issue loading {ConfigFileName}");
        }

        _lastReloadTime = now;

        if (ZNet.instance != null && !ZNet.instance.IsDedicated())
        {
            UpdateConfigurations();
        }
    }

    [Serializable]
    public struct PieceConfiguration
    {
        public string Prefab;
        public PieceConfig Config;
    }

    private List<PieceConfiguration> LoadJsons()
    {
        var json = AssetUtils.LoadTextFromResources("Configurations.json", Assembly.GetExecutingAssembly());
        try
        {
            var config = JsonConvert.DeserializeObject<List<PieceConfiguration>>(json);
            return config;
        }
        catch
        {
            _instance.Logger.LogError("Issue loading pieces! Contact the mod author.");
        }

        return new List<PieceConfiguration>();
    }

    private void AddPortals()
    {
        var assembly = Assembly.GetCallingAssembly();
        AssetBundle bundle = AssetUtils.LoadAssetBundleFromResources($"{assembly.GetName().Name}.advancedportals", Assembly.GetExecutingAssembly());

        List<PieceConfiguration> pieces = LoadJsons();

        foreach (PieceConfiguration piece in pieces)
        {
            GameObject go = bundle.LoadAsset<GameObject>(piece.Prefab);
            PieceManager.Instance.AddPiece(new CustomPiece(go, true, piece.Config));

            AddPortal.Hashes.Add(go.name.GetStableHashCode());
        }
    }
}
