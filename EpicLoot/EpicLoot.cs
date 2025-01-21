using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using AdventureBackpacks.API;
using BepInEx;
using Common;
using EpicLoot.Adventure;
using EpicLoot.Config;
using EpicLoot.Crafting;
using EpicLoot.CraftingV2;
using EpicLoot.Data;
using EpicLoot.GatedItemType;
using EpicLoot.MagicItemEffects;
using EpicLoot.Patching;
using EpicLoot.src.Adventure.bounties;
using HarmonyLib;
using JetBrains.Annotations;
using Jotunn;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace EpicLoot
{
    public enum LogLevel
    {
        Info,
        Warning,
        Error
    }

    public enum BossDropMode
    {
        Default,
        OnePerPlayerOnServer,
        OnePerPlayerNearBoss
    }

    public enum GatedBountyMode
    {
        Unlimited,
        BossKillUnlocksCurrentBiomeBounties,
        BossKillUnlocksNextBiomeBounties
    }

    public class Assets
    {
        public AssetBundle AssetBundle;
        public Sprite EquippedSprite;
        public Sprite AugaEquippedSprite;
        public Sprite GenericSetItemSprite;
        public Sprite AugaSetItemSprite;
        public Sprite GenericItemBgSprite;
        public Sprite AugaItemBgSprite;
        public GameObject[] MagicItemLootBeamPrefabs = new GameObject[5];
        public readonly Dictionary<string, GameObject[]> CraftingMaterialPrefabs = new Dictionary<string, GameObject[]>();
        public Sprite SmallButtonEnchantOverlay;
        public AudioClip[] MagicItemDropSFX = new AudioClip[5];
        public AudioClip ItemLoopSFX;
        public AudioClip AugmentItemSFX;
        public GameObject MerchantPanel;
        public Sprite MapIconTreasureMap;
        public Sprite MapIconBounty;
        public AudioClip AbandonBountySFX;
        public AudioClip DoubleJumpSFX;
        public GameObject DebugTextPrefab;
        public GameObject AbilityBar;
        public GameObject WelcomMessagePrefab;
    }

    public class PieceDef
    {
        public string Table;
        public string CraftingStation;
        public string ExtendStation;
        public List<RecipeRequirementConfig> Resources = new List<RecipeRequirementConfig>();
    }

    [BepInPlugin(PluginId, DisplayName, Version)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    [BepInDependency("randyknapp.mods.auga", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("vapok.mods.adventurebackpacks", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("kg.ValheimEnchantmentSystem", BepInDependency.DependencyFlags.SoftDependency)]
    public class EpicLoot : BaseUnityPlugin
    {
        public const string PluginId = "randyknapp.mods.epicloot";
        public const string DisplayName = "Epic Loot";
        public const string Version = "0.10.7";

        public static readonly List<ItemDrop.ItemData.ItemType> AllowedMagicItemTypes = new List<ItemDrop.ItemData.ItemType>
        {
            ItemDrop.ItemData.ItemType.Helmet,
            ItemDrop.ItemData.ItemType.Chest,
            ItemDrop.ItemData.ItemType.Legs,
            ItemDrop.ItemData.ItemType.Shoulder,
            ItemDrop.ItemData.ItemType.Utility,
            ItemDrop.ItemData.ItemType.Bow,
            ItemDrop.ItemData.ItemType.OneHandedWeapon,
            ItemDrop.ItemData.ItemType.TwoHandedWeapon,
            ItemDrop.ItemData.ItemType.TwoHandedWeaponLeft,
            ItemDrop.ItemData.ItemType.Shield,
            ItemDrop.ItemData.ItemType.Tool,
            ItemDrop.ItemData.ItemType.Torch,
        };

        public static readonly Dictionary<string, string> MagicItemColors = new Dictionary<string, string>()
        {
            { "Red",    "#ff4545" },
            { "Orange", "#ffac59" },
            { "Yellow", "#ffff75" },
            { "Green",  "#80fa70" },
            { "Teal",   "#18e7a9" },
            { "Blue",   "#00abff" },
            { "Indigo", "#709bba" },
            { "Purple", "#d078ff" },
            { "Pink",   "#ff63d6" },
            { "Gray",   "#dbcadb" },
        };

        public static readonly Assets Assets = new Assets();
        public static readonly List<GameObject> RegisteredPrefabs = new List<GameObject>();
        public static readonly List<GameObject> RegisteredItemPrefabs = new List<GameObject>();
        public static readonly Dictionary<GameObject, PieceDef> RegisteredPieces = new Dictionary<GameObject, PieceDef>();
        private static readonly Dictionary<string, Action<ItemDrop>> _customItemSetupActions = new Dictionary<string, Action<ItemDrop>>();
        private static readonly Dictionary<string, Object> _assetCache = new Dictionary<string, Object>();
        public static bool AlwaysDropCheat = false;
        public const Minimap.PinType BountyPinType = (Minimap.PinType) 800;
        public const Minimap.PinType TreasureMapPinType = (Minimap.PinType) 801;
        public static bool HasAuga;
        public static bool HasAdventureBackpacks;
        public static bool AugaTooltipNoTextBoxes;
        

        public static event Action AbilitiesInitialized;
        public static event Action LootTableLoaded;

        private static EpicLoot _instance;
        private Harmony _harmony;
        private float _worldLuckFactor;
        internal ELConfig cfg;

        [UsedImplicitly]
        private void Awake()
        {
            _instance = this;

            var assembly = Assembly.GetExecutingAssembly();
            
            EIDFLegacy.CheckForExtendedItemFrameworkLoaded(_instance);

            LoadEmbeddedAssembly(assembly, "EpicLoot-UnityLib.dll");
            cfg = new ELConfig(Config);

            // Set the referenced common logger to the EL specific reference so that common things get logged
            PrefabCreator.Logger = Logger;

            HasAdventureBackpacks = ABAPI.IsLoaded();

            LoadPatches();
            InitializeAbilities();
            PrintInfo();
            AddLocalizations();

            LoadAssets();

            EnchantingUIController.Initialize();

            _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), PluginId);

            LootTableLoaded?.Invoke();
        }

        private static void LoadEmbeddedAssembly(Assembly assembly, string assemblyName)
        {
            var stream = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.{assemblyName}");
            if (stream == null)
            {
                LogErrorForce($"Could not load embedded assembly ({assemblyName})!");
                return;
            }

            using (stream)
            {
                var data = new byte[stream.Length];
                stream.Read(data, 0, data.Length);
                Assembly.Load(data);
            }
        }

        public void Start()
        {
            HasAuga = Auga.API.IsLoaded();

            if (HasAuga)
            {
                Auga.API.ComplexTooltip_AddItemTooltipCreatedListener(ExtendAugaTooltipForMagicItem);
                Auga.API.ComplexTooltip_AddItemStatPreprocessor(AugaTooltipPreprocessor.PreprocessTooltipStat);
            }
        }

        public static void ExtendAugaTooltipForMagicItem(GameObject complexTooltip, ItemDrop.ItemData item)
        {
            Auga.API.ComplexTooltip_SetTopic(complexTooltip, Localization.instance.Localize(item.GetDecoratedName()));

            var isMagic = item.IsMagic(out var magicItem);

            var inFront = true;
            var itemBG = complexTooltip.transform.Find("Tooltip/IconHeader/IconBkg/Item");
            if (itemBG == null)
            {
                itemBG = complexTooltip.transform.Find("InventoryElement/icon");
                inFront = false;
            }

            RectTransform magicBG = null;
            if (itemBG != null)
            {
                var itemBGImage = itemBG.GetComponent<Image>();
                magicBG = (RectTransform)itemBG.transform.Find("magicItem");
                if (magicBG == null)
                {
                    var magicItemObject = Instantiate(itemBGImage, inFront ?
                        itemBG.transform : itemBG.transform.parent).gameObject;
                    magicItemObject.name = "magicItem";
                    magicItemObject.SetActive(true);
                    magicBG = (RectTransform)magicItemObject.transform;
                    magicBG.anchorMin = Vector2.zero;
                    magicBG.anchorMax = new Vector2(1, 1);
                    magicBG.sizeDelta = Vector2.zero;
                    magicBG.pivot = new Vector2(0.5f, 0.5f);
                    magicBG.anchoredPosition = Vector2.zero;
                    var magicItemInit = magicBG.GetComponent<Image>();
                    magicItemInit.color = Color.white;
                    magicItemInit.raycastTarget = false;
                    magicItemInit.sprite = GetMagicItemBgSprite();

                    if (!inFront)
                    {
                        magicBG.SetSiblingIndex(0);
                    }
                }
            }

            if (magicBG != null)
            {
                magicBG.gameObject.SetActive(isMagic);
            }

            if (item.IsMagicCraftingMaterial())
            {
                var rarity = item.GetCraftingMaterialRarity();
                Auga.API.ComplexTooltip_SetIcon(complexTooltip, item.m_shared.m_icons[GetRarityIconIndex(rarity)]);
            }

            if (isMagic)
            {
                var magicColor = magicItem.GetColorString();
                var itemTypeName = magicItem.GetItemTypeName(item.Extended());

                if (magicBG != null)
                {
                    magicBG.GetComponent<Image>().color = item.GetRarityColor();
                }

                Auga.API.ComplexTooltip_SetIcon(complexTooltip, item.GetIcon());

                string localizedSubtitle;
                if (item.IsLegendarySetItem())
                {
                    localizedSubtitle = $"<color={GetSetItemColor()}>" +
                        $"$mod_epicloot_legendarysetlabel</color>, {itemTypeName}\n";
                }
                else
                {
                    localizedSubtitle = $"<color={magicColor}>{magicItem.GetRarityDisplay()} {itemTypeName}</color>";
                }

                try
                {
                    Auga.API.ComplexTooltip_SetSubtitle(complexTooltip, Localization.instance.Localize(localizedSubtitle));
                }
                catch (Exception)
                {
                    Auga.API.ComplexTooltip_SetSubtitle(complexTooltip, localizedSubtitle);
                }
                
                if (AugaTooltipNoTextBoxes)
                    return;
                
                //Don't need to process the InventoryTooltip Information.
                if (complexTooltip.name.Contains("InventoryTooltip"))
                    return;

                //The following is used only for Crafting Result Panel.
                Auga.API.ComplexTooltip_AddDivider(complexTooltip);

                var magicItemText = magicItem.GetTooltip();
                var textBox = Auga.API.ComplexTooltip_AddTwoColumnTextBox(complexTooltip);
                magicItemText = magicItemText.Replace("\n\n", "");
                Auga.API.TooltipTextBox_AddLine(textBox, magicItemText);
                
                if (magicItem.IsLegendarySetItem())
                {
                    var textBox2 = Auga.API.ComplexTooltip_AddTwoColumnTextBox(complexTooltip);
                    Auga.API.TooltipTextBox_AddLine(textBox2, item.GetSetTooltip());
                }
                
                try
                {
                    Auga.API.ComplexTooltip_SetDescription(complexTooltip,
                        Localization.instance.Localize(item.GetDescription()));
                }
                catch (Exception)
                {
                    Auga.API.ComplexTooltip_SetDescription(complexTooltip, item.GetDescription());
                }
            }
        }

        public static void LoadPatches()
        {
            FilePatching.LoadAllPatches();
        }

        private void AddLocalizations()
        {
            CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();
            // load all localization files within the localizations directory
            Log("Loading Localizations.");
            foreach (string embeddedResouce in typeof(EpicLoot).Assembly.GetManifestResourceNames())
            {
                if (!embeddedResouce.Contains("localizations")) { continue; }
                string localization = ReadEmbeddedResourceFile(embeddedResouce);
                // This will clean comments out of the localization files
                string cleaned_localization = Regex.Replace(localization, @"\/\/.*\n", "");
                // Log($"Cleaned Localization: {cleaned_localization}");
                var localization_name = embeddedResouce.Split('.');
                Log($"Adding localization: {localization_name[2]}");
                Localization.AddJsonFile(localization_name[2], cleaned_localization);
            }
        }

        private static void InitializeAbilities()
        {
            MagicEffectType.Initialize();
            AbilitiesInitialized?.Invoke();
        }

        public static void Log(string message)
        {
            if (ELConfig._loggingEnabled.Value && ELConfig._logLevel.Value <= LogLevel.Info)
            {
                _instance.Logger.LogInfo(message);
            }
        }

        public static void LogWarning(string message)
        {
            if (ELConfig._loggingEnabled.Value && ELConfig._logLevel.Value <= LogLevel.Warning)
            {
                _instance.Logger.LogWarning(message);
            }
        }

        public static void LogError(string message)
        {
            if (ELConfig._loggingEnabled.Value && ELConfig._logLevel.Value <= LogLevel.Error)
            {
                _instance.Logger.LogError(message);
            }
        }

        public static void LogWarningForce(string message)
        {
            _instance.Logger.LogWarning(message);
        }

        public static void LogErrorForce(string message)
        {
            _instance.Logger.LogError(message);
        }

        private void LoadAssets()
        {
            var assetBundle = LoadAssetBundle("epicloot");

            Assets.AssetBundle = assetBundle;
            Assets.EquippedSprite = assetBundle.LoadAsset<Sprite>("Equipped");
            Assets.AugaEquippedSprite = assetBundle.LoadAsset<Sprite>("AugaEquipped");
            Assets.GenericSetItemSprite = assetBundle.LoadAsset<Sprite>("GenericSetItemMarker");
            Assets.AugaSetItemSprite = assetBundle.LoadAsset<Sprite>("AugaSetItem");
            Assets.GenericItemBgSprite = assetBundle.LoadAsset<Sprite>("GenericItemBg");
            Assets.AugaItemBgSprite = assetBundle.LoadAsset<Sprite>("AugaItemBG");
            Assets.SmallButtonEnchantOverlay = assetBundle.LoadAsset<Sprite>("SmallButtonEnchantOverlay");
            Assets.MagicItemLootBeamPrefabs[(int)ItemRarity.Magic] = assetBundle.LoadAsset<GameObject>("MagicLootBeam");
            Assets.MagicItemLootBeamPrefabs[(int)ItemRarity.Rare] = assetBundle.LoadAsset<GameObject>("RareLootBeam");
            Assets.MagicItemLootBeamPrefabs[(int)ItemRarity.Epic] = assetBundle.LoadAsset<GameObject>("EpicLootBeam");
            Assets.MagicItemLootBeamPrefabs[(int)ItemRarity.Legendary] = assetBundle.LoadAsset<GameObject>("LegendaryLootBeam");
            Assets.MagicItemLootBeamPrefabs[(int)ItemRarity.Mythic] = assetBundle.LoadAsset<GameObject>("MythicLootBeam");

            Assets.MagicItemDropSFX[(int)ItemRarity.Magic] = assetBundle.LoadAsset<AudioClip>("MagicItemDrop");
            Assets.MagicItemDropSFX[(int)ItemRarity.Rare] = assetBundle.LoadAsset<AudioClip>("RareItemDrop");
            Assets.MagicItemDropSFX[(int)ItemRarity.Epic] = assetBundle.LoadAsset<AudioClip>("EpicItemDrop");
            Assets.MagicItemDropSFX[(int)ItemRarity.Legendary] = assetBundle.LoadAsset<AudioClip>("LegendaryItemDrop");
            Assets.MagicItemDropSFX[(int)ItemRarity.Mythic] = assetBundle.LoadAsset<AudioClip>("MythicItemDrop");
            Assets.ItemLoopSFX = assetBundle.LoadAsset<AudioClip>("ItemLoop");
            Assets.AugmentItemSFX = assetBundle.LoadAsset<AudioClip>("AugmentItem");

            Assets.MerchantPanel = assetBundle.LoadAsset<GameObject>("MerchantPanel");
            Assets.MapIconTreasureMap = assetBundle.LoadAsset<Sprite>("TreasureMapIcon");
            Assets.MapIconBounty = assetBundle.LoadAsset<Sprite>("MapIconBounty");
            Assets.AbandonBountySFX = assetBundle.LoadAsset<AudioClip>("AbandonBounty");
            Assets.DoubleJumpSFX = assetBundle.LoadAsset<AudioClip>("DoubleJump");
            Assets.DebugTextPrefab = assetBundle.LoadAsset<GameObject>("DebugText");
            Assets.AbilityBar = assetBundle.LoadAsset<GameObject>("AbilityBar");
            Assets.WelcomMessagePrefab = assetBundle.LoadAsset<GameObject>("WelcomeMessage");

            LoadCraftingMaterialAssets(assetBundle, "Runestone");

            LoadCraftingMaterialAssets(assetBundle, "Shard");
            LoadCraftingMaterialAssets(assetBundle, "Dust");
            LoadCraftingMaterialAssets(assetBundle, "Reagent");
            LoadCraftingMaterialAssets(assetBundle, "Essence");

            LoadBuildPiece(assetBundle, "piece_enchanter", new PieceDef()
            {
                Table = "_HammerPieceTable",
                CraftingStation = "piece_workbench",
                Resources = new List<RecipeRequirementConfig>
                {
                    new RecipeRequirementConfig { item = "Stone", amount = 10 },
                    new RecipeRequirementConfig { item = "SurtlingCore", amount = 3 },
                    new RecipeRequirementConfig { item = "Copper", amount = 3 },
                }
            });
            LoadBuildPiece(assetBundle, "piece_augmenter", new PieceDef()
            {
                Table = "_HammerPieceTable",
                CraftingStation = "piece_workbench",
                Resources = new List<RecipeRequirementConfig>
                {
                    new RecipeRequirementConfig { item = "Obsidian", amount = 10 },
                    new RecipeRequirementConfig { item = "Crystal", amount = 3 },
                    new RecipeRequirementConfig { item = "Bronze", amount = 3 },
                }
            });
            LoadBuildPiece(assetBundle, "piece_enchantingtable", new PieceDef() {
                Table = "_HammerPieceTable",
                CraftingStation = "piece_workbench",
                Resources = new List<RecipeRequirementConfig>
                {
                    new RecipeRequirementConfig { item = "FineWood", amount = 10 },
                    new RecipeRequirementConfig { item = "SurtlingCore", amount = 1 }
                }
            });

            LoadItem(assetBundle, "LeatherBelt");
            LoadItem(assetBundle, "SilverRing");
            LoadItem(assetBundle, "GoldRubyRing");
            LoadItem(assetBundle, "Andvaranaut", SetupAndvaranaut);

            LoadItem(assetBundle, "ForestToken");
            LoadItem(assetBundle, "IronBountyToken");
            LoadItem(assetBundle, "GoldBountyToken");

            LoadAllZNetAssets(assetBundle);


            GameObject bounty_spawner = assetBundle.LoadAsset<GameObject>("Assets/EpicLoot/Prefabs/Adventure/EL_SpawnController.prefab");
            bounty_spawner.AddComponent<AdventureSpawnController>();
            CustomPrefab prefab_obj = new CustomPrefab(bounty_spawner, false);
            PrefabManager.Instance.AddPrefab(prefab_obj);
        }

        public static T LoadAsset<T>(string assetName) where T : Object
        {
            try
            {
                if (_assetCache.ContainsKey(assetName))
                {
                    return (T)_assetCache[assetName];
                }

                var asset = Assets.AssetBundle.LoadAsset<T>(assetName);
                _assetCache.Add(assetName, asset);
                return asset;
            }
            catch (Exception e)
            {
                LogErrorForce($"Error loading asset ({assetName}): {e.Message}");
                return null;
            }
        }

        private static void LoadItem(AssetBundle assetBundle, string assetName, Action<ItemDrop> customSetupAction = null)
        {
            var prevForceDisable = ZNetView.m_forceDisableInit;
            ZNetView.m_forceDisableInit = true;
            var prefab = assetBundle.LoadAsset<GameObject>(assetName);
            ZNetView.m_forceDisableInit = prevForceDisable;
            RegisteredItemPrefabs.Add(prefab);
            RegisteredPrefabs.Add(prefab);
            if (customSetupAction != null)
            {
                _customItemSetupActions.Add(prefab.name, customSetupAction);
            }
        }

        private static void LoadBuildPiece(AssetBundle assetBundle, string assetName, PieceDef pieceDef)
        {
            var prefab = assetBundle.LoadAsset<GameObject>(assetName);
            RegisteredPieces.Add(prefab, pieceDef);
            RegisteredPrefabs.Add(prefab);
        }

        private static void LoadCraftingMaterialAssets(AssetBundle assetBundle, string type)
        {
            var prefabs = new GameObject[5];
            foreach (ItemRarity rarity in Enum.GetValues(typeof(ItemRarity)))
            {
                var assetName = $"{type}{rarity}";
                var prefab = assetBundle.LoadAsset<GameObject>(assetName);
                if (prefab == null)
                {
                    LogErrorForce($"Tried to load asset {assetName} but it does not exist in the asset bundle!");
                    continue;
                }
                prefabs[(int) rarity] = prefab;
                RegisteredPrefabs.Add(prefab);
                RegisteredItemPrefabs.Add(prefab);
            }
            Assets.CraftingMaterialPrefabs.Add(type, prefabs);
        }

        private void LoadAllZNetAssets(AssetBundle assetBundle)
        {
            var znetAssets = assetBundle.LoadAllAssets();
            foreach (var asset in znetAssets)
            {
                if (asset is GameObject assetGo && assetGo.GetComponent<ZNetView>() != null)
                {
                    if (!_assetCache.ContainsKey(asset.name))
                        _assetCache.Add(asset.name, assetGo);
                    
                    if (!RegisteredPrefabs.Contains(assetGo))
                        RegisteredPrefabs.Add(assetGo);
                }
            }
        }

        [UsedImplicitly]
        private void OnDestroy()
        {
            _instance = null;
        }

        public static void TryRegisterPrefabs(ZNetScene zNetScene)
        {
            if (zNetScene == null || zNetScene.m_prefabs == null || zNetScene.m_prefabs.Count <= 0)
            {
                return;
            }

            foreach (var prefab in RegisteredPrefabs)
            {
                if (!zNetScene.m_prefabs.Contains(prefab))
                {
                    zNetScene.m_prefabs.Add(prefab);
                }
            }
        }

        public static void TryRegisterPieces(List<PieceTable> pieceTables, List<CraftingStation> craftingStations)
        {
            foreach (var entry in RegisteredPieces)
            {
                var prefab = entry.Key;
                if (prefab == null)
                {
                    LogError($"Tried to register piece but prefab was null!");
                    continue;
                }

                var pieceDef = entry.Value;
                if (pieceDef == null)
                {
                    LogError($"Tried to register piece ({prefab}) but pieceDef was null!");
                    continue;
                }

                var piece = prefab.GetComponent<Piece>();
                if (piece == null)
                {
                    LogError($"Tried to register piece ({prefab}) but Piece component was missing!");
                    continue;
                }

                var pieceTable = pieceTables.Find(x => x.name == pieceDef.Table);
                if (pieceTable == null)
                {
                    LogError($"Tried to register piece ({prefab}) but could not find piece table " +
                        $"({pieceDef.Table}) (pieceTables({pieceTables.Count})= " +
                        $"{string.Join(", ", pieceTables.Select(x =>x.name))})!");
                    continue;
                }

                if (pieceTable.m_pieces.Contains(prefab))
                {
                    continue;
                }

                pieceTable.m_pieces.Add(prefab);

                var pieceStation = craftingStations.Find(x => x.name == pieceDef.CraftingStation);
                piece.m_craftingStation = pieceStation;

                var resources = new List<Piece.Requirement>();
                foreach (var resource in pieceDef.Resources)
                {
                    var resourcePrefab = ObjectDB.instance.GetItemPrefab(resource.item);
                    resources.Add(new Piece.Requirement()
                    {
                        m_resItem = resourcePrefab.GetComponent<ItemDrop>(),
                        m_amount = resource.amount
                    });
                }
                piece.m_resources = resources.ToArray();

                var stationExt = prefab.GetComponent<StationExtension>();
                if (stationExt != null && !string.IsNullOrEmpty(pieceDef.ExtendStation))
                {
                    var stationPrefab = pieceTable.m_pieces.Find(x => x.name == pieceDef.ExtendStation);
                    if (stationPrefab != null)
                    {
                        var station = stationPrefab.GetComponent<CraftingStation>();
                        stationExt.m_craftingStation = station;
                    }

                    var otherExt = pieceTable.m_pieces.Find(x => x.GetComponent<StationExtension>() != null);
                    if (otherExt != null)
                    {
                        var otherStationExt = otherExt.GetComponent<StationExtension>();
                        var otherPiece = otherExt.GetComponent<Piece>();

                        stationExt.m_connectionPrefab = otherStationExt.m_connectionPrefab;
                        piece.m_placeEffect.m_effectPrefabs = otherPiece.m_placeEffect.m_effectPrefabs.ToArray();
                    }
                }
                else
                {
                    var workshopPrefab = pieceTable.m_pieces.FirstOrDefault(x => x.name == "piece_workshop");
                    if (workshopPrefab != null && workshopPrefab.GetComponent<Piece>() is Piece otherPiece)
                        piece.m_placeEffect.m_effectPrefabs = otherPiece.m_placeEffect.m_effectPrefabs.ToArray();
                }
            }
        }

        public static bool IsObjectDBReady()
        {
            // Hack, just making sure the built-in items and prefabs have loaded
            return ObjectDB.instance != null && ObjectDB.instance.m_items.Count != 0 &&
                ObjectDB.instance.GetItemPrefab("Amber") != null;
        }

        public static void TryRegisterItems()
        {
            if (!IsObjectDBReady())
            {
                return;
            }

            
            foreach (var prefab in RegisteredItemPrefabs)
            {
                var itemDrop = prefab.GetComponent<ItemDrop>();
                if (itemDrop != null)
                {
                    //Set icons for crafting materials

                    if (itemDrop.m_itemData.IsMagicCraftingMaterial() || itemDrop.m_itemData.IsRunestone())
                    {
                        var rarity = itemDrop.m_itemData.GetRarity();
                        
                        if (itemDrop.m_itemData.IsMagicCraftingMaterial())
                        {
                            itemDrop.m_itemData.m_variant = GetRarityIconIndex(rarity);
                        }
                    }
                }
            }

            foreach (var prefab in RegisteredItemPrefabs)
            {
                var itemDrop = prefab.GetComponent<ItemDrop>();
                if (itemDrop != null)
                {
                    if (ObjectDB.instance.GetItemPrefab(prefab.name.GetStableHashCode()) == null)
                    {
                        ObjectDB.instance.m_items.Add(prefab);
                    }
                }
            }

            foreach (var prefab in RegisteredItemPrefabs)
            {
                var itemDrop = prefab.GetComponent<ItemDrop>();
                if (itemDrop != null)
                {
                    if (_customItemSetupActions.TryGetValue(prefab.name, out var action))
                    {
                        action?.Invoke(itemDrop);
                    }
                }
            }

            ObjectDB.instance.UpdateRegisters();

            var pieceTables = new List<PieceTable>();
            foreach (var itemPrefab in ObjectDB.instance.m_items)
            {
                var itemDrop = itemPrefab.GetComponent<ItemDrop>();
                if (itemDrop == null)
                {
                    LogError($"An item without an ItemDrop ({itemPrefab}) exists in ObjectDB.instance.m_items! " +
                        $"Don't do this!");
                    continue;
                }
                var item = itemDrop.m_itemData;
                if (item != null && item.m_shared.m_buildPieces != null && 
                    !pieceTables.Contains(item.m_shared.m_buildPieces))
                {
                    pieceTables.Add(item.m_shared.m_buildPieces);
                }
            }

            var craftingStations = new List<CraftingStation>();
            foreach (var pieceTable in pieceTables)
            {
                craftingStations.AddRange(pieceTable.m_pieces
                    .Where(x => x.GetComponent<CraftingStation>() != null)
                    .Select(x => x.GetComponent<CraftingStation>()));
            }

            TryRegisterPieces(pieceTables, craftingStations);
            SetupStatusEffects();
        }

        public static void TryRegisterRecipes()
        {
            if (!IsObjectDBReady())
            {
                return;
            }

            RecipesHelper.SetupRecipes();
        }

        private static void SetupAndvaranaut(ItemDrop prefab)
        {
            var andvaranaut = prefab.m_itemData;
            var wishbone = ObjectDB.instance.GetItemPrefab("Wishbone").GetComponent<ItemDrop>().m_itemData;

            // first, create custom status effect
            var originalFinder = wishbone.m_shared.m_equipStatusEffect;
            var wishboneFinder = ScriptableObject.CreateInstance<SE_CustomFinder>();

            // Copy all values from finder
            Common.Utils.CopyFields(originalFinder, wishboneFinder, typeof(SE_Finder));
            wishboneFinder.name = "CustomWishboneFinder";

            var andvaranautFinder = ScriptableObject.CreateInstance<SE_CustomFinder>();
            Common.Utils.CopyFields(wishboneFinder, andvaranautFinder, typeof(SE_CustomFinder));
            andvaranautFinder.name = "Andvaranaut";
            andvaranautFinder.m_name = "$mod_epicloot_item_andvaranaut";
            andvaranautFinder.m_icon = andvaranaut.GetIcon();
            andvaranautFinder.m_tooltip = "$mod_epicloot_item_andvaranaut_tooltip";
            andvaranautFinder.m_startMessage = "$mod_epicloot_item_andvaranaut_startmsg";

            // Setup restrictions
            andvaranautFinder.RequiredComponentTypes = new List<Type> { typeof(TreasureMapChest) };
            wishboneFinder.DisallowedComponentTypes = new List<Type> { typeof(TreasureMapChest) };

            // Add to list
            ObjectDB.instance.m_StatusEffects.Remove(originalFinder);
            ObjectDB.instance.m_StatusEffects.Add(andvaranautFinder);
            ObjectDB.instance.m_StatusEffects.Add(wishboneFinder);

            // Set new status effect
            andvaranaut.m_shared.m_equipStatusEffect = andvaranautFinder;
            wishbone.m_shared.m_equipStatusEffect = wishboneFinder;

            // Setup magic item
            var magicItem = new MagicItem
            {
                Rarity = ItemRarity.Epic,
                TypeNameOverride = "$mod_epicloot_item_andvaranaut_type"
            };
            magicItem.Effects.Add(new MagicItemEffect(MagicEffectType.Andvaranaut));

            prefab.m_itemData.SaveMagicItem(magicItem);
        }

        private static void SetupStatusEffects()
        {
            var lightning = ObjectDB.instance.GetStatusEffect("Lightning".GetHashCode());
            var paralyzed = ScriptableObject.CreateInstance<SE_Paralyzed>();
            Common.Utils.CopyFields(lightning, paralyzed, typeof(StatusEffect));
            paralyzed.name = "Paralyze";
            paralyzed.m_name = "mod_epicloot_se_paralyze";

            ObjectDB.instance.m_StatusEffects.Add(paralyzed);
        }

        

        public static AssetBundle LoadAssetBundle(string filename)
        {
            var assembly = Assembly.GetCallingAssembly();
            var assetBundle = AssetBundle.LoadFromStream(assembly.GetManifestResourceStream(
                $"{assembly.GetName().Name}.{filename}"));

            return assetBundle;
        }

        /// <summary>
        /// This reads an embedded file resouce name, these are all resouces packed into the DLL
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        internal static string ReadEmbeddedResourceFile(string filename)
        {
            //EpicLoot.Log($"Attempting to load resource path: {filename}");
            //foreach (string embeddedResouce in typeof(EpicLoot).Assembly.GetManifestResourceNames())
            //{
            //    EpicLoot.Log($"resource: {embeddedResouce}");
            //}
            using (var stream = typeof(EpicLoot).Assembly.GetManifestResourceStream(filename))
            {
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        internal static List<string> GetEmbeddedResourceNamesFromDirectory(string embedded_location = "EpicLoot.config.")
        {
            List<string> resourcenames = new List<string>();
            foreach (string embeddedResouce in typeof(EpicLoot).Assembly.GetManifestResourceNames())
            {
                if (embeddedResouce.Contains(embedded_location))
                {
                    // Got to strip the starting 'EpicLoot.config.' off, so we just take the ending substring
                    resourcenames.Add(embeddedResouce.Substring(16));
                }
            }
            return resourcenames;
        }

        public static bool CanBeMagicItem(ItemDrop.ItemData item)
        {
            return item != null && IsPlayerItem(item) && Nonstackable(item) && 
                IsNotRestrictedItem(item) && AllowedMagicItemTypes.Contains(item.m_shared.m_itemType);
        }

        public static Sprite GetMagicItemBgSprite()
        {
            return HasAuga ? Assets.AugaItemBgSprite : Assets.GenericItemBgSprite;
        }

        public static Sprite GetEquippedSprite()
        {
            return HasAuga ? Assets.AugaEquippedSprite : Assets.EquippedSprite;
        }

        public static Sprite GetSetItemSprite()
        {
            return HasAuga ? Assets.AugaSetItemSprite : Assets.GenericSetItemSprite;
        }

        public static string GetMagicEffectPip(bool hasBeenAugmented)
        {
            return HasAuga ? (hasBeenAugmented ? "♢" : "♦") : (hasBeenAugmented ? "◇" : "◆");
        }

        private static bool IsNotRestrictedItem(ItemDrop.ItemData item)
        {
            if (item.m_dropPrefab != null && LootRoller.Config.RestrictedItems.Contains(item.m_dropPrefab.name))
                return false;
            return !LootRoller.Config.RestrictedItems.Contains(item.m_shared.m_name);
        }

        private static bool Nonstackable(ItemDrop.ItemData item)
        {
            return item.m_shared.m_maxStackSize == 1;
        }

        private static bool IsPlayerItem(ItemDrop.ItemData item)
        {
            // WTF, this is the only thing I found different between player usable items and items that are only for enemies
            return item.m_shared.m_icons.Length > 0;
        }

        public static string GetCharacterCleanName(Character character)
        {
            return character.name.Replace("(Clone)", "").Trim();
        }

        public static void OnCharacterDeath(CharacterDrop characterDrop)
        {
            if (!CanCharacterDropLoot(characterDrop.m_character))
            {
                return;
            }

            var characterName = GetCharacterCleanName(characterDrop.m_character);
            var level = characterDrop.m_character.GetLevel();
            var dropPoint = characterDrop.m_character.GetCenterPoint() +
                characterDrop.transform.TransformVector(characterDrop.m_spawnOffset);

            OnCharacterDeath(characterName, level, dropPoint);
        }

        public static bool CanCharacterDropLoot(Character character)
        {
            return character != null && !character.IsTamed();
        }

        public static void OnCharacterDeath(string characterName, int level, Vector3 dropPoint)
        {
            var lootTables = LootRoller.GetLootTable(characterName);
            if (lootTables != null && lootTables.Count > 0)
            {
                var loot = LootRoller.RollLootTableAndSpawnObjects(lootTables, level, characterName, dropPoint);
                Log($"Rolling on loot table: {characterName} (lvl {level}), " +
                    $"spawned {loot.Count} items at drop point({dropPoint}).");
                DropItems(loot, dropPoint);
                foreach (var l in loot)
                {
                    var itemData = l.GetComponent<ItemDrop>().m_itemData;
                    var magicItem = itemData.GetMagicItem();
                    if (magicItem != null)
                    {
                        Log($"  - {itemData.m_shared.m_name} <{l.transform.position}>: " +
                            $"{string.Join(", ", magicItem.Effects.Select(x => x.EffectType.ToString()))}");
                    }
                }
            }
            else
            {
                Log($"Could not find loot table for: {characterName} (lvl {level})");
            }
        }

        public static void DropItems(List<GameObject> loot, Vector3 centerPos, float dropHemisphereRadius = 0.5f)
        {
            foreach (var item in loot)
            {
                var vector3 = Random.insideUnitSphere * dropHemisphereRadius;
                vector3.y = Mathf.Abs(vector3.y);
                item.transform.position = centerPos + vector3;
                item.transform.rotation = Quaternion.Euler(0.0f, Random.Range(0, 360), 0.0f);

                var rigidbody = item.GetComponent<Rigidbody>();
                if (rigidbody != null)
                {
                    var insideUnitSphere = Random.insideUnitSphere;
                    if (insideUnitSphere.y < 0.0)
                    {
                        insideUnitSphere.y = -insideUnitSphere.y;
                    }
                    rigidbody.AddForce(insideUnitSphere * 5f, ForceMode.VelocityChange);
                }
            }
        }

        private void PrintInfo()
        {
            const string devOutputPath = @"C:\Users\rknapp\Documents\GitHub\ValheimMods\EpicLoot";
            if (!Directory.Exists(devOutputPath))
            {
                return;
            }

            var t = new StringBuilder();
            t.AppendLine($"# EpicLoot Data v{Version}");
            t.AppendLine();
            t.AppendLine("*Author: RandyKnapp*");
            t.AppendLine("*Source: [Github](https://github.com/RandyKnapp/ValheimMods/tree/main/EpicLoot)*");
            t.AppendLine();

            // Magic item effects per rarity
            t.AppendLine("# Magic Effect Count Weights Per Rarity");
            t.AppendLine();
            t.AppendLine("Each time a **MagicItem** is rolled a number of **MagicItemEffects** are added based on its **Rarity**. The percent chance to roll each number of effects is found on the following table. These values are hardcoded.");
            t.AppendLine();
            t.AppendLine("The raw weight value is shown first, followed by the calculated percentage chance in parentheses.");
            t.AppendLine();
            t.AppendLine("|Rarity|1|2|3|4|5|6|");
            t.AppendLine("|--|--|--|--|--|--|--|");
            t.AppendLine(GetMagicEffectCountTableLine(ItemRarity.Magic));
            t.AppendLine(GetMagicEffectCountTableLine(ItemRarity.Rare));
            t.AppendLine(GetMagicEffectCountTableLine(ItemRarity.Epic));
            t.AppendLine(GetMagicEffectCountTableLine(ItemRarity.Legendary));
            t.AppendLine(GetMagicEffectCountTableLine(ItemRarity.Mythic));
            t.AppendLine();

            var rarities = new List<ItemRarity>();
            foreach (ItemRarity value in Enum.GetValues(typeof(ItemRarity)))
            {
                rarities.Add(value);
            }

            var skillTypes = new List<Skills.SkillType>();
            foreach (Skills.SkillType value in Enum.GetValues(typeof(Skills.SkillType)))
            {
                if (value == Skills.SkillType.None
                    || value == Skills.SkillType.WoodCutting
                    || value == Skills.SkillType.Jump
                    || value == Skills.SkillType.Sneak
                    || value == Skills.SkillType.Run
                    || value == Skills.SkillType.Swim
                    || value == Skills.SkillType.All)
                {
                    continue;
                }
                skillTypes.Add(value);
            }

            // Magic item effects
            t.AppendLine("# MagicItemEffect List");
            t.AppendLine();
            t.AppendLine("The following lists all the built-in **MagicItemEffects**. MagicItemEffects are defined in `magiceffects.json` and are parsed and added " +
                         "to `MagicItemEffectDefinitions` on Awake. EpicLoot uses an string for the types of magic effects. You can add your own new types using your own identifiers.");
            t.AppendLine();
            t.AppendLine("Listen to the event `MagicItemEffectDefinitions.OnSetupMagicItemEffectDefinitions` (which gets called in `EpicLoot.Awake`) to add your own using instances of MagicItemEffectDefinition.");
            t.AppendLine();
            t.AppendLine("  * **Display Text:** This text appears in the tooltip for the magic item, with {0:?} replaced with the rolled value for the effect, formatted using the shown C# string format.");
            t.AppendLine("  * **Requirements:** A set of requirements.");
            t.AppendLine("    * **Flags:** A set of predefined flags to check certain weapon properties. The list of flags is: `NoRoll, ExclusiveSelf, ItemHasPhysicalDamage, ItemHasElementalDamage, ItemUsesDurability, ItemHasNegativeMovementSpeedModifier, ItemHasBlockPower, ItemHasNoParryPower, ItemHasParryPower, ItemHasArmor, ItemHasBackstabBonus, ItemUsesStaminaOnAttack, ItemUsesEitrOnAttack, ItemUsesHealthOnAttack`");
            t.AppendLine("    * **ExclusiveEffectTypes:** This effect may not be rolled on an item that has already rolled on of these effects");
            t.AppendLine($"    * **AllowedItemTypes:** This effect may only be rolled on items of a the types in this list. When this list is empty, this is usually done because this is a special effect type added programmatically  or currently not allowed to roll. Options are: `{string.Join(", ", AllowedMagicItemTypes)}`");
            t.AppendLine($"    * **ExcludedItemTypes:** This effect may only be rolled on items that are not one of the types on this list.");
            t.AppendLine($"    * **AllowedRarities:** This effect may only be rolled on an item of one of these rarities. Options are: `{string.Join(", ", rarities)}`");
            t.AppendLine($"    * **ExcludedRarities:** This effect may only be rolled on an item that is not of one of these rarities.");
            t.AppendLine($"    * **AllowedSkillTypes:** This effect may only be rolled on an item that uses one of these skill types. Options are: `{string.Join(", ", skillTypes)}`");
            t.AppendLine($"    * **ExcludedSkillTypes:** This effect may only be rolled on an item that does not use one of these skill types.");
            t.AppendLine("    * **AllowedItemNames:** This effect may only be rolled on an item with one of these names. Use the unlocalized shared name, i.e.: `$item_sword_iron`");
            t.AppendLine("    * **ExcludedItemNames:** This effect may only be rolled on an item that does not have one of these names.");
            t.AppendLine("    * **CustomFlags:** A set of any arbitrary strings for future use");
            t.AppendLine("  * **Value Per Rarity:** This effect may only be rolled on items of a rarity included in this table. The value is rolled using a linear distribution between Min and Max and divisible by the Increment.");
            t.AppendLine();

            foreach (var definitionEntry in MagicItemEffectDefinitions.AllDefinitions)
            {
                var def = definitionEntry.Value;
                t.AppendLine($"## {def.Type}");
                t.AppendLine();
                t.AppendLine($"> **Display Text:** {Localization.instance.Localize(def.DisplayText)}");
                t.AppendLine("> ");

                if (def.Prefixes.Count > 0)
                {
                    t.AppendLine($"> **Prefixes:** {Localization.instance.Localize(string.Join(", ", def.Prefixes))}");
                }

                if (def.Suffixes.Count > 0)
                {
                    t.AppendLine($"> **Suffixes:** {Localization.instance.Localize(string.Join(", ", def.Suffixes))}");
                }

                if (def.Prefixes.Count > 0 || def.Suffixes.Count > 0)
                {
                    t.AppendLine("> ");
                }

                var allowedItemTypes = def.GetAllowedItemTypes();
                t.AppendLine("> **Allowed Item Types:** " + (allowedItemTypes.Count == 0 ? "*None*" : 
                    string.Join(", ", allowedItemTypes)));
                t.AppendLine("> ");
                t.AppendLine("> **Requirements:**");
                t.Append(def.Requirements);

                if (def.HasRarityValues())
                {
                    t.AppendLine("> ");
                    t.AppendLine("> **Value Per Rarity:**");
                    t.AppendLine("> ");
                    t.AppendLine("> |Rarity|Min|Max|Increment|");
                    t.AppendLine("> |--|--|--|--|");

                    if (def.ValuesPerRarity.Magic != null)
                    {
                        var v = def.ValuesPerRarity.Magic;
                        t.AppendLine($"> |Magic|{v.MinValue}|{v.MaxValue}|{v.Increment}|");
                    }
                    if (def.ValuesPerRarity.Rare != null)
                    {
                        var v = def.ValuesPerRarity.Rare;
                        t.AppendLine($"> |Rare|{v.MinValue}|{v.MaxValue}|{v.Increment}|");
                    }
                    if (def.ValuesPerRarity.Epic != null)
                    {
                        var v = def.ValuesPerRarity.Epic;
                        t.AppendLine($"> |Epic|{v.MinValue}|{v.MaxValue}|{v.Increment}|");
                    }
                    if (def.ValuesPerRarity.Legendary != null)
                    {
                        var v = def.ValuesPerRarity.Legendary;
                        t.AppendLine($"> |Legendary|{v.MinValue}|{v.MaxValue}|{v.Increment}|");
                    }

                    if (def.ValuesPerRarity.Mythic != null)
                    {
                        var v = def.ValuesPerRarity.Legendary;
                        t.AppendLine($"> |Mythic|{v.MinValue}|{v.MaxValue}|{v.Increment}|");
                    }
                }
                if (!string.IsNullOrEmpty(def.Comment))
                {
                    t.AppendLine("> ");
                    t.AppendLine($"> ***Notes:*** *{def.Comment}*");
                }

                t.AppendLine();
            }

            // Item Sets
            t.AppendLine("# Item Sets");
            t.AppendLine();
            t.AppendLine("Sets of loot drop data that can be referenced in the loot tables");

            foreach (var lootTableEntry in LootRoller.ItemSets)
            {
                var itemSet = lootTableEntry.Value;

                t.AppendLine($"## {lootTableEntry.Key}");
                t.AppendLine();
                WriteLootList(t, 0, itemSet.Loot);
                t.AppendLine();
            }

            // Loot tables
            t.AppendLine("# Loot Tables");
            t.AppendLine();
            t.AppendLine("A list of every built-in loot table from the mod. " +
                "The name of the loot table is the object name followed by a number signifying the level of the object.");

            foreach (var lootTableEntry in LootRoller.LootTables)
            {
                var list = lootTableEntry.Value;

                foreach (var lootTable in list)
                {
                    t.AppendLine($"## {lootTableEntry.Key}");
                    t.AppendLine();
                    WriteLootTableDrops(t, lootTable);
                    WriteLootTableItems(t, lootTable);
                    t.AppendLine();
                }
            }

            File.WriteAllText(Path.Combine(devOutputPath, "info.md"), t.ToString());
        }

        private static void WriteLootTableDrops(StringBuilder t, LootTable lootTable)
        {
            var highestLevel = lootTable.LeveledLoot != null && lootTable.LeveledLoot.Count > 0 ?
                lootTable.LeveledLoot.Max(x => x.Level) : 0;
            var limit = Mathf.Max(3, highestLevel);
            for (var i = 0; i < limit; i++)
            {
                var level = i + 1;
                var dropTable = LootRoller.GetDropsForLevel(lootTable, level, false);
                if (dropTable == null || dropTable.Count == 0)
                {
                    continue;
                }

                float total = dropTable.Sum(x => x.Value);
                if (total > 0)
                {
                    t.AppendLine($"> | Drops (lvl {level}) | Weight (Chance) |");
                    t.AppendLine($"> | -- | -- |");
                    foreach (var drop in dropTable)
                    {
                        var count = drop.Key;
                        var value = drop.Value;
                        var percent = (value / total) * 100;
                        t.AppendLine($"> | {count} | {value} ({percent:0.#}%) |");
                    }
                }
                t.AppendLine();
            }
        }

        private static void WriteLootTableItems(StringBuilder t, LootTable lootTable)
        {
            var highestLevel = lootTable.LeveledLoot != null && lootTable.LeveledLoot.Count > 0 ?
                lootTable.LeveledLoot.Max(x => x.Level) : 0;
            var limit = Mathf.Max(3, highestLevel);
            for (var i = 0; i < limit; i++)
            {
                var level = i + 1;
                var lootList = LootRoller.GetLootForLevel(lootTable, level, false);
                if (ArrayUtils.IsNullOrEmpty(lootList))
                {
                    continue;
                }

                WriteLootList(t, level, lootList);
            }
        }

        private static void WriteLootList(StringBuilder t, int level, LootDrop[] lootList)
        {
            var levelDisplay = level > 0 ? $" (lvl {level})" : "";
            t.AppendLine($"> | Items{levelDisplay} | Weight (Chance) | Magic | Rare | Epic | Legendary | Mythic |");
            t.AppendLine("> | -- | -- | -- | -- | -- | -- | -- |");

            float totalLootWeight = lootList.Sum(x => x.Weight);
            foreach (var lootDrop in lootList)
            {
                var percentChance = lootDrop.Weight / totalLootWeight * 100;
                if (lootDrop.Rarity == null || lootDrop.Rarity.Length == 0)
                {
                    t.AppendLine($"> | {lootDrop.Item} | {lootDrop.Weight} ({percentChance:0.#}%) | " +
                        $"1 (100%) | 0 (0%) | 0 (0%) | 0 (0%) | 0 (0%) |");
                    continue;
                }

                float rarityTotal = lootDrop.Rarity.Sum();
                float[] rarityPercent =
                {
                    lootDrop.Rarity[0] / rarityTotal * 100,
                    lootDrop.Rarity[1] / rarityTotal * 100,
                    lootDrop.Rarity[2] / rarityTotal * 100,
                    lootDrop.Rarity[3] / rarityTotal * 100,
                    lootDrop.Rarity[4] / rarityTotal * 100
                };
                t.AppendLine($"> | {lootDrop.Item} | {lootDrop.Weight} ({percentChance:0.#}%) " +
                             $"| {lootDrop.Rarity[0]} ({rarityPercent[0]:0.#}%) " +
                             $"| {lootDrop.Rarity[1]} ({rarityPercent[1]:0.#}%) " +
                             $"| {lootDrop.Rarity[2]:0.#} ({rarityPercent[2]:0.#}%) " +
                             $"| {lootDrop.Rarity[3]} ({rarityPercent[3]:0.#}%) |" +
                             $"| {lootDrop.Rarity[4]} ({rarityPercent[4]:0.#}%) |");
            }

            t.AppendLine();
        }

        private static string GetMagicEffectCountTableLine(ItemRarity rarity)
        {
            var effectCounts = LootRoller.GetEffectCountsPerRarity(rarity, false);
            float total = effectCounts.Sum(x => x.Value);
            var result = $"|{rarity}|";
            for (var i = 1; i <= 7; ++i)
            {
                var valueString = " ";
                var i1 = i;
                if (effectCounts.TryFind(x => x.Key == i1, out var found))
                {
                    var value = found.Value;
                    var percent = value / total * 100;
                    valueString = $"{value} ({percent:0.#}%)";
                }
                result += $"{valueString}|";
            }
            return result;
        }

        public static string GetSetItemColor()
        {
            return ELConfig._setItemColor.Value;
        }

        public static string GetRarityDisplayName(ItemRarity rarity)
        {
            switch (rarity)
            {
                case ItemRarity.Magic:
                    return "$mod_epicloot_magic";
                case ItemRarity.Rare:
                    return "$mod_epicloot_rare";
                case ItemRarity.Epic:
                    return "$mod_epicloot_epic";
                case ItemRarity.Legendary:
                    return "$mod_epicloot_legendary";
                case ItemRarity.Mythic:
                    return "$mod_epicloot_mythic";
                default:
                    return "<non magic>";
            }
        }

        public static string GetRarityColor(ItemRarity rarity)
        {
            switch (rarity)
            {
                case ItemRarity.Magic:
                    return GetColor(ELConfig._magicRarityColor.Value);
                case ItemRarity.Rare:
                    return GetColor(ELConfig._rareRarityColor.Value);
                case ItemRarity.Epic:
                    return GetColor(ELConfig._epicRarityColor.Value);
                case ItemRarity.Legendary:
                    return GetColor(ELConfig._legendaryRarityColor.Value);
                case ItemRarity.Mythic:
                    return GetColor(ELConfig._mythicRarityColor.Value);
                default:
                    return "#FFFFFF";
            }
        }

        public static Color GetRarityColorARGB(ItemRarity rarity)
        {
            return ColorUtility.TryParseHtmlString(GetRarityColor(rarity), out var color) ? color : Color.white;
        }

        private static string GetColor(string configValue)
        {
            if (configValue.StartsWith("#"))
            {
                return configValue;
            }
            else
            {
                if (MagicItemColors.TryGetValue(configValue, out var color))
                {
                    return color;
                }
            }

            return "#000000";
        }

        public static int GetRarityIconIndex(ItemRarity rarity)
        {
            switch (rarity)
            {
                case ItemRarity.Magic:
                    return Mathf.Clamp(ELConfig._magicMaterialIconColor.Value, 0, 9);
                case ItemRarity.Rare:
                    return Mathf.Clamp(ELConfig._rareMaterialIconColor.Value, 0, 9);
                case ItemRarity.Epic:
                    return Mathf.Clamp(ELConfig._epicMaterialIconColor.Value, 0, 9);
                case ItemRarity.Legendary:
                    return Mathf.Clamp(ELConfig._legendaryMaterialIconColor.Value, 0, 9);
                case ItemRarity.Mythic:
                    return Mathf.Clamp(ELConfig._mythicMaterialIconColor.Value, 0, 9);
                default:
                    throw new ArgumentOutOfRangeException(nameof(rarity), rarity, null);
            }
        }

        public static AudioClip GetMagicItemDropSFX(ItemRarity rarity)
        {
            return Assets.MagicItemDropSFX[(int) rarity];
        }

        public static GatedItemTypeMode GetGatedItemTypeMode()
        {
            return ELConfig._gatedItemTypeModeConfig.Value;
        }

        public static BossDropMode GetBossTrophyDropMode()
        {
            return ELConfig._bossTrophyDropMode.Value;
        }

        public static float GetBossTrophyDropPlayerRange()
        {
            return ELConfig._bossTrophyDropPlayerRange.Value;
        }
        public static float GetBossCryptKeyPlayerRange()
        {
            return ELConfig._bossCryptKeyDropPlayerRange.Value;
        }

        public static BossDropMode GetBossCryptKeyDropMode()
        {
            return ELConfig._bossCryptKeyDropMode.Value;
        }

        public static BossDropMode GetBossWishboneDropMode()
        {
            return ELConfig._bossWishboneDropMode.Value;
        }

        public static float GetBossWishboneDropPlayerRange()
        {
            return ELConfig._bossWishboneDropPlayerRange.Value;
        }

        public static int GetAndvaranautRange()
        {
          return ELConfig._andvaranautRange.Value;
        }

        public static bool IsAdventureModeEnabled()
        {
            return ELConfig._adventureModeEnabled.Value;
        }

        private static string Clean(string name)
        {
            return name.Replace("'", "").Replace(",", "").Trim().Replace(" ", "_").ToLowerInvariant();
        }

        private static void ReplaceValueList(List<string> values, string field, string label, 
            MagicItemEffectDefinition effectDef, Dictionary<string, string> translations, ref string magicEffectsJson)
        {
            var newValues = new List<string>();
            for (var index = 0; index < values.Count; index++)
            {
                var value = values[index];
                string key;
                if (value.StartsWith("$"))
                {
                    key = value;
                }
                else
                {
                    key = GetId(effectDef, $"{field}{index + 1}");
                    AddTranslation(translations, key, value);
                }
                newValues.Add(key);
            }

            if (newValues.Count > 0)
            {
                var old = $"\"{label}\": [ {string.Join(", ", values.Select(x => $"\"{x}\""))} ]";
                var toReplace = $"\"{label}\": [\n        {string.Join(",\n        ",
                    newValues.Select(x => (x.StartsWith("$") ? $"\"{x}\"" : $"\"${x}\"")))}\n      ]";
                magicEffectsJson = ReplaceTranslation(magicEffectsJson, old, toReplace);
            }
        }

        private static string GetId(MagicItemEffectDefinition effectDef, string field)
        {
            return $"mod_epicloot_me_{effectDef.Type.ToLowerInvariant()}_{field.ToLowerInvariant()}";
        }

        private static void AddTranslation(Dictionary<string, string> translations, string key, string value)
        {
            translations.Add(key, value);
        }

        private static string ReplaceTranslation(string jsonFile, string original, string locId)
        {
            return jsonFile.Replace(original, locId);
        }

        private static string SetupTranslation(MagicItemEffectDefinition effectDef, string value, string field,
            string replaceFormat, Dictionary<string, string> translations, string jsonFile)
        {
            if (string.IsNullOrEmpty(value) || value.StartsWith("$"))
            {
                return jsonFile;
            }

            var key = GetId(effectDef, field);
            AddTranslation(translations, key, value);
            return ReplaceTranslation(jsonFile, string.Format(replaceFormat, value),
                string.Format(replaceFormat, $"${key}"));
        }

        public static float GetWorldLuckFactor()
        {
            return _instance._worldLuckFactor;
        }

        public static void SetWorldLuckFactor(float luckFactor)
        {
            _instance._worldLuckFactor = luckFactor;
        }
    }
}
