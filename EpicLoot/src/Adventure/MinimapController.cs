using HarmonyLib;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace EpicLoot.Adventure
{
    public enum MinimapPinQueueTask
    {
        AddTreasurePin,
        AddBountyPin,
        RemoveTreasurePin,
        RemoveBountyPin,
        RefreshAll
    }
    public class PinJob
    {
        public MinimapPinQueueTask Task { get; set; }
        public KeyValuePair<Tuple<int, Heightmap.Biome>, AreaPinInfo> TreasurePin { get; set; }
        public KeyValuePair<string, AreaPinInfo> BountyPin { get; set; }
        public bool DebugMode { get; set; }
    }
    public class AreaPinInfo
    {
        public Minimap.PinData Pin { get; set; }
        public Minimap.PinData Area { get; set; }
        public Minimap.PinData DebugPin { get; set; }
        
        //Pin Data
        public Vector3 Position { get; set; }
        public Minimap.PinType Type { get; set; }
        public string Name { get; set; }
        public bool Save { get; set; }
        public bool Checked { get; set; }
        public long OwnerId { get; set; }

        public AreaPinInfo()
        {
            Name = string.Empty;
            Save = false;
            Checked = false;
            OwnerId = 0L;
        }
    }
    public class AdventureToggle
    {
        public readonly GameObject instance;
        public readonly Toggle toggle;
        public readonly TextMeshProUGUI label;
        public readonly RectTransform rect;
        public readonly Image checkbox;
        public readonly Image checkmark;
        public readonly Image darken;
        public readonly UIGamePad gamepad;
        public readonly TextMeshProUGUI inputKey;

        public AdventureToggle(GameObject source, Transform parent, string name, UnityAction<bool> onToggle)
        {
            instance = UnityEngine.Object.Instantiate(source, parent);
            instance.name = name;
            rect = instance.GetComponent<RectTransform>();
            toggle = instance.GetComponentInChildren<Toggle>();
            toggle.onValueChanged.RemoveAllListeners();
            toggle.onValueChanged.AddListener(onToggle);
            label = Utils.FindChild(instance.transform, "Label").GetComponent<TextMeshProUGUI>();
            label.text = name;
            checkbox = Utils.FindChild(instance.transform, "Background").GetComponent<Image>();
            checkmark = Utils.FindChild(checkbox.transform, "Checkmark").GetComponent<Image>();
            darken = instance.GetComponent<Image>();
            gamepad = instance.GetComponentInChildren<UIGamePad>();
            inputKey = Utils.FindChild(instance.transform, "Key").GetComponent<TextMeshProUGUI>();
            ButtonSfx sfx = instance.GetComponentInChildren<ButtonSfx>();
            sfx.Start();
        }

        public void SetGamepadKey(string key)
        {
            gamepad.m_zinputKey = key;
            inputKey.text = Localization.instance.Localize(ZInput.instance.GetBoundKeyString(key, true));
        }

        public void SetLabel(string text) => label.text = Localization.instance.Localize(text);
        
        public void SetIcon(Sprite icon) => checkmark.sprite = icon;
        
        public void SetBackground(float transparency) => darken.color = new Color(darken.color.r, darken.color.g, darken.color.b, transparency);
    }

    [RequireComponent(typeof(Minimap))]
    public class MinimapController : MonoBehaviour
    {
        private static readonly Queue<PinJob> MinimapPinQueue = new();
        
        private Minimap _minimap;
        private static Player _player;
        
        public const float AreaScale = 2.1f;

        public static readonly Dictionary<Tuple<int, Heightmap.Biome>, AreaPinInfo> TreasureMapPins = new();
        public static readonly Dictionary<string, AreaPinInfo> BountyPins = new();
        public static bool DebugMode;
        private static bool _enabled;
        
        public virtual void Awake()
        {
            _minimap = GetComponent<Minimap>();
            
            if (!_minimap.m_icons.Exists(x => x.m_name == EpicLoot.TreasureMapPinType))
            {
                _minimap.m_icons.Add(new Minimap.SpriteData { m_name = EpicLoot.TreasureMapPinType, m_icon = EpicAssets.MapIconTreasureMap });
            }
            if (!_minimap.m_icons.Exists(x => x.m_name == EpicLoot.BountyPinType))
            {
                _minimap.m_icons.Add(new Minimap.SpriteData { m_name = EpicLoot.BountyPinType, m_icon = EpicAssets.MapIconBounty });
            }
            
            SetupToggles();
        }

        private void Start()
        {
            TreasureMapPins.Clear();
            BountyPins.Clear();
            
            if (_minimap.m_visibleIconTypes.Length < (int)EpicLoot.TreasureMapPinType + 1)
            {
                _minimap.m_visibleIconTypes = new bool[(int)EpicLoot.TreasureMapPinType + 1];
                for (var index = 0; index < _minimap.m_visibleIconTypes.Length; ++index)
                {
                    _minimap.m_visibleIconTypes[index] = true;
                }
            }
            
            var pinJob = new PinJob
            {
                Task = MinimapPinQueueTask.RefreshAll
            };
            AddPinJobToQueue(pinJob);
        }

        public virtual void Update()
        {
            if (!_enabled)
                return;

            while (MinimapPinQueue.Any())
            {
                ProcessMinimapPinTask(MinimapPinQueue.Dequeue());
            }
        }

        private void OnDestroy()
        {
            TreasureMapPins.Clear();
            BountyPins.Clear();

            _enabled = false;
        }

        private void SetupToggles()
        {
            GameObject original = Utils.FindChild(_minimap.transform, "SharedPanel").gameObject;
            
            GameObject container = new GameObject("EpicLoot Toggle Container");
            RectTransform rect = container.AddComponent<RectTransform>();
            rect.SetParent(original.transform.parent);

            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(0f, 0f);
            rect.pivot = new Vector2(0f, 1f);
            rect.sizeDelta = new Vector2(250f, 42f);
            rect.anchoredPosition = new Vector2(20f, 60f); //TODO: figure out how to programmatically set position to avoid screen size difference moving container, if it is a problem
            
            HorizontalLayoutGroup layout = container.AddComponent<HorizontalLayoutGroup>();
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.spacing = 5f;
            
            AdventureToggle bountyToggle = new AdventureToggle(original, rect, "Bounty", ToggleBounties);
            bountyToggle.SetIcon(EpicAssets.MapIconBounty);
            bountyToggle.SetGamepadKey("JoyLTrigger");
            bountyToggle.SetLabel("$mod_epicloot_merchant_bounties");

            AdventureToggle treasureToggle = new AdventureToggle(original, rect, "Treasure", ToggleTreasureMaps);
            treasureToggle.SetIcon(EpicAssets.MapIconTreasureMap);
            treasureToggle.SetGamepadKey("JoyRTrigger");
            treasureToggle.SetLabel("$mod_epicloot_merchant_treasuremaps");
        }

        private static void ToggleBounties(bool show)
        {
            if (_player == null) return;
                
                if (show)
                {
                    AdventureSaveData adventureSaveData = _player.GetAdventureSaveData();
                    if (adventureSaveData == null) return;
                    List<BountyInfo> currentBounties = adventureSaveData.GetInProgressBounties();
                    foreach (BountyInfo bounty in currentBounties)
                    {
                        string key = bounty.ID;
                        if (!BountyPins.ContainsKey(key))
                        {
                            AreaPinInfo pinInfo = new AreaPinInfo
                            {
                                Position = bounty.Position + bounty.MinimapCircleOffset,
                                Type = EpicLoot.BountyPinType,
                                Name = Localization.instance.Localize("$mod_epicloot_bounties_minimappin", AdventureDataManager.GetBountyName(bounty))
                            };

                            PinJob pinJob = new PinJob
                            {
                                Task = MinimapPinQueueTask.AddBountyPin,
                                DebugMode = DebugMode,
                                BountyPin = new KeyValuePair<string, AreaPinInfo>(key, pinInfo)
                            };

                            AddPinJobToQueue(pinJob);
                        }
                    }
                }
                else
                {
                    foreach (KeyValuePair<string, AreaPinInfo> pinEntry in BountyPins)
                    {
                        PinJob pinJob = new PinJob()
                        {
                            Task = MinimapPinQueueTask.RemoveBountyPin,
                            DebugMode = DebugMode,
                            BountyPin = new KeyValuePair<string, AreaPinInfo>(pinEntry.Key, pinEntry.Value)
                        };
                        AddPinJobToQueue(pinJob);
                    }
                }
        }

        private static void ToggleTreasureMaps(bool show)
        {
            if (_player == null) return;

                if (show)
                {
                    AdventureSaveData adventureSaveData = _player.GetAdventureSaveData();
                    if (adventureSaveData == null) return;
                    List<TreasureMapChestInfo> unfoundTreasureChests = adventureSaveData.GetUnfoundTreasureChests();

                    foreach (TreasureMapChestInfo chestInfo in unfoundTreasureChests)
                    {
                        Tuple<int, Heightmap.Biome> key = new Tuple<int, Heightmap.Biome>(chestInfo.Interval, chestInfo.Biome);
                        if (!TreasureMapPins.ContainsKey(key))
                        {
                            AreaPinInfo pinInfo = new AreaPinInfo
                            {
                                Position = chestInfo.Position + chestInfo.MinimapCircleOffset,
                                Type = EpicLoot.TreasureMapPinType,
                                Name = Localization.instance.Localize("$mod_epicloot_treasurechest_minimappin",
                                    Localization.instance.Localize($"$biome_{chestInfo.Biome.ToString().ToLowerInvariant()}"),
                                    (chestInfo.Interval + 1).ToString())
                            };

                            PinJob pinJob = new PinJob
                            {
                                Task = MinimapPinQueueTask.AddTreasurePin,
                                DebugMode = DebugMode,
                                TreasurePin = new KeyValuePair<Tuple<int, Heightmap.Biome>, AreaPinInfo>(key, pinInfo)
                            };

                            AddPinJobToQueue(pinJob);
                        }
                    }
                }
                else
                {
                    foreach (KeyValuePair<Tuple<int, Heightmap.Biome>, AreaPinInfo> pinEntry in TreasureMapPins)
                    {
                        PinJob pinJob = new PinJob()
                        {
                            Task = MinimapPinQueueTask.RemoveTreasurePin,
                            DebugMode = DebugMode,
                            TreasurePin = new KeyValuePair<Tuple<int, Heightmap.Biome>, AreaPinInfo>(pinEntry.Key, pinEntry.Value)
                        };
                        AddPinJobToQueue(pinJob);
                    }
                }
        }
        
        
        //Static Methods
        public static void AddPinJobToQueue(PinJob pinJob)
        {
            if (pinJob != null)
            {
                MinimapPinQueue.Enqueue(pinJob);
            }
        }

        public static void Enable(Player player)
        {
            _player = player;
            _enabled = true;
        }
        
        private void ProcessMinimapPinTask(PinJob pinJob)
        {
            switch (pinJob.Task)
            {
                case MinimapPinQueueTask.AddBountyPin:
                case MinimapPinQueueTask.AddTreasurePin:
                    AddPin(pinJob);
                    break;
                case MinimapPinQueueTask.RemoveTreasurePin:
                    RemovePin(pinJob.TreasurePin.Value);
                    TreasureMapPins.Remove(pinJob.TreasurePin.Key);
                    break;
                case MinimapPinQueueTask.RemoveBountyPin:
                    RemovePin(pinJob.BountyPin.Value);
                    BountyPins.Remove(pinJob.BountyPin.Key);
                    break;
                case MinimapPinQueueTask.RefreshAll:
                    RefreshPins();
                    break;
            }
        }

        private void AddPin(PinJob pinJob)
        {
            AreaPinInfo newPin = null;
            switch (pinJob.Task)
            {
                case MinimapPinQueueTask.AddBountyPin:
                    newPin = pinJob.BountyPin.Value;
                    break;
                case MinimapPinQueueTask.AddTreasurePin:
                    newPin = pinJob.TreasurePin.Value;
                    break;
            }

            if (newPin == null) return;
            
            //Add Area Pin
            newPin.Area = _minimap.AddPin(newPin.Position, Minimap.PinType.EventArea, string.Empty, false, false);
            newPin.Area.m_worldSize = AdventureDataManager.Config.TreasureMap.MinimapAreaRadius * AreaScale;
                    
            //Add Pin
            newPin.Pin = _minimap.AddPin(newPin.Position, newPin.Type, newPin.Name, false, false);
                    
            //Add Debug Pin
            if (pinJob.DebugMode)
            {
                newPin.DebugPin = _minimap.AddPin(newPin.Position, Minimap.PinType.Icon3,
                    $"{newPin.Position.x:0.0}, {newPin.Position.z:0.0}", false, false);
            }
            
            switch (pinJob.Task)
            {
                    
                case MinimapPinQueueTask.AddBountyPin:
                    BountyPins[pinJob.BountyPin.Key] = pinJob.BountyPin.Value;
                    break;
                case MinimapPinQueueTask.AddTreasurePin:
                    TreasureMapPins[pinJob.TreasurePin.Key] = pinJob.TreasurePin.Value;
                    break;
            }
        }

        private void RemovePin(AreaPinInfo pinEntry)
        {
            _minimap.RemovePin(pinEntry.Pin);
            _minimap.RemovePin(pinEntry.Area);
            if (pinEntry.DebugPin != null)
            {
                _minimap.RemovePin(pinEntry.DebugPin);
            }
        }

        private void RefreshPins()
        {
            if (_player == null)
                return;

            var adventureSaveData = _player.GetAdventureSaveData();
            if (adventureSaveData == null)
                return;

            var unfoundTreasureChests = adventureSaveData.GetUnfoundTreasureChests();
            var oldPins = TreasureMapPins.Where(pinEntry => !unfoundTreasureChests
                .Exists(x => x.Interval == pinEntry.Key.Item1 && x.Biome == pinEntry.Key.Item2)).ToList();
            foreach (var pinEntry in oldPins)
            {
                var pinJob = new PinJob
                {
                    Task = MinimapPinQueueTask.RemoveTreasurePin,
                    DebugMode = DebugMode,
                    TreasurePin = new KeyValuePair<Tuple<int, Heightmap.Biome>, AreaPinInfo>(pinEntry.Key, pinEntry.Value)
                };

                AddPinJobToQueue(pinJob);
            }

            foreach (var chestInfo in unfoundTreasureChests)
            {
                var key = new Tuple<int, Heightmap.Biome>(chestInfo.Interval, chestInfo.Biome);
                if (!TreasureMapPins.ContainsKey(key))
                {
                    var pinInfo = new AreaPinInfo
                    {
                        Position = chestInfo.Position + chestInfo.MinimapCircleOffset,
                        Type = EpicLoot.TreasureMapPinType,
                        Name = Localization.instance.Localize("$mod_epicloot_treasurechest_minimappin",
                            Localization.instance.Localize($"$biome_{chestInfo.Biome.ToString().ToLowerInvariant()}"),
                            (chestInfo.Interval + 1).ToString())
                    };

                    var pinJob = new PinJob
                    {
                        Task = MinimapPinQueueTask.AddTreasurePin,
                        DebugMode = DebugMode,
                        TreasurePin = new KeyValuePair<Tuple<int, Heightmap.Biome>, AreaPinInfo>(key, pinInfo)
                    };

                    AddPinJobToQueue(pinJob);
                }
            }

            var currentBounties = adventureSaveData.GetInProgressBounties();
            var oldBountyPins = BountyPins.Where(pinEntry => !currentBounties.Exists(x => x.ID == pinEntry.Key)).ToList();
            foreach (var pinEntry in oldBountyPins)
            {
                var pinJob = new PinJob
                {
                    Task = MinimapPinQueueTask.RemoveBountyPin,
                    DebugMode = DebugMode,
                    BountyPin = new KeyValuePair<string, AreaPinInfo>(pinEntry.Key, pinEntry.Value)
                };

                AddPinJobToQueue(pinJob);
            }

            foreach (var bounty in currentBounties)
            {
                var key = bounty.ID;
                if (!BountyPins.ContainsKey(key))
                {
                    var pinInfo = new AreaPinInfo
                    {
                        Position = bounty.Position + bounty.MinimapCircleOffset,
                        Type = EpicLoot.BountyPinType,
                        Name = Localization.instance.Localize("$mod_epicloot_bounties_minimappin", AdventureDataManager.GetBountyName(bounty))
                    };

                    var pinJob = new PinJob
                    {
                        Task = MinimapPinQueueTask.AddBountyPin,
                        DebugMode = DebugMode,
                        BountyPin = new KeyValuePair<string, AreaPinInfo>(key, pinInfo)
                    };

                    AddPinJobToQueue(pinJob);
                }
            }
        }
        
    }

    [HarmonyPatch(typeof(Minimap))]
    public static class MinimapPatch
    {
        [HarmonyPatch(nameof(Minimap.Awake))]
        [UsedImplicitly]
        public static void Postfix(Minimap __instance)
        {
            __instance.gameObject.AddComponent<MinimapController>();
        }
    }
    
    [HarmonyPatch(typeof(Player), nameof(Player.SetLocalPlayer))]
    public static class Player_SetLocalPlayer_Patch
    {
        [UsedImplicitly]
        public static void Postfix(Player __instance)
        {
            MinimapController.Enable(__instance);
        }
    }
}