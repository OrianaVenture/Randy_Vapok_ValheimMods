using JetBrains.Annotations;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UnityEngine;

namespace EpicLoot_UnityLib.src
{
    internal class RuneUI : EnchantingTableUIPanelBase
    {
        public Text EnchantInfo;
        public Scrollbar EnchantInfoScrollbar;
        public List<Toggle> RuneActionButtons;

        [Header("Cost")]
        public Text CostLabel;
        public MultiSelectItemList CostList;

        public AudioClip[] EnchantCompleteSFX;

        public delegate List<InventoryItemListElement> GetEnchantableItemsDelegate();
        public delegate string GetEnchantInfoDelegate(ItemDrop.ItemData item, MagicRarityUnity rarity);
        public delegate List<InventoryItemListElement> GetEnchantCostDelegate(ItemDrop.ItemData item, MagicRarityUnity rarity);
        // Returns the success dialog
        public delegate GameObject EnchantItemDelegate(ItemDrop.ItemData item, MagicRarityUnity rarity);

        public static GetEnchantableItemsDelegate GetEnchantableItems;
        public static GetEnchantInfoDelegate GetEnchantInfo;
        public static GetEnchantCostDelegate GetEnchantCost;
        public static EnchantItemDelegate EnchantItem;

        private ToggleGroup _toggleGroup;
        private MagicRarityUnity _runeAction;
        private GameObject _successDialog;

        private enum RuneAction
        {
            Etch,
            Extract,
            Imbue
        }

        public override void Awake()
        {
            base.Awake();

            if (RuneActionButtons.Count > 0)
            {
                _toggleGroup = RuneActionButtons[0].group;
                _toggleGroup.EnsureValidState();
            }

            //for (var index = 0; index < RuneActionButtons.Count; index++)
            //{
            //    var rarityButton = RuneActionButtons[index];
            //    rarityButton.onValueChanged.AddListener((isOn) => {
            //        if (isOn)
            //            RefreshRuneAction();
            //    });
            //}
        }

        [UsedImplicitly]
        public void OnEnable()
        {
            _runeAction = MagicRarityUnity.Magic;
            OnRarityChanged();
            RuneActionButtons[0].isOn = true;
            var items = GetEnchantableItems();
            AvailableItems.SetItems(items.Cast<IListElement>().ToList());
        }

        public override void Update()
        {
            base.Update();

            //if (!_locked && ZInput.IsGamepadActive())
            //{
            //    if (ZInput.GetButtonDown("JoyButtonY"))
            //    {
            //        var nextModeIndex = ((int)_runeAction + 1) % RuneActionButtons.Count;
            //        RuneActionButtons[nextModeIndex].isOn = true;
            //        ZInput.ResetButtonStatus("JoyButtonY");
            //    }

            //    if (EnchantInfoScrollbar != null)
            //    {
            //        var rightStickAxis = ZInput.GetJoyRightStickY();
            //        if (Mathf.Abs(rightStickAxis) > 0.5f)
            //            EnchantInfoScrollbar.value = Mathf.Clamp01(EnchantInfoScrollbar.value + rightStickAxis * -0.1f);
            //    }
            //}

            // Check if the action is completed, and unlock the UI
            if (_successDialog != null && !_successDialog.activeSelf)
            {
                Unlock();
                Destroy(_successDialog);
                _successDialog = null;
            }
        }

        public void RefreshRuneAction()
        {
            var prevRarity = _runeAction;
            for (var index = 0; index < RuneActionButtons.Count; index++)
            {
                var button = RuneActionButtons[index];
                if (button.isOn)
                {
                    _runeAction = (MagicRarityUnity)index;
                }
            }

            if (prevRarity != _runeAction)
                OnRarityChanged();
        }

        public void OnRarityChanged()
        {
            var selectedItem = AvailableItems.GetSingleSelectedItem<InventoryItemListElement>();
            if (selectedItem?.Item1.GetItem() == null)
            {
                MainButton.interactable = false;
                EnchantInfo.text = "";
                CostLabel.enabled = false;
                CostList.SetItems(new List<IListElement>());
                return;
            }

            var item = selectedItem.Item1.GetItem();
            var info = GetEnchantInfo(item, _runeAction);

            EnchantInfo.text = info;

            CostLabel.enabled = true;
            var cost = GetEnchantCost(item, _runeAction);
            CostList.SetItems(cost.Cast<IListElement>().ToList());

            var canAfford = LocalPlayerCanAffordCost(cost);
            var featureUnlocked = EnchantingTableUI.instance.SourceTable.IsFeatureUnlocked(EnchantingFeature.Enchant);
            MainButton.interactable = featureUnlocked && canAfford;
        }

        protected override void DoMainAction()
        {
            var selectedItem = AvailableItems.GetSelectedItems<InventoryItemListElement>().FirstOrDefault();

            Cancel();

            if (selectedItem?.Item1.GetItem() == null)
            {
                return;
            }

            var item = selectedItem.Item1.GetItem();
            var cost = GetEnchantCost(item, _runeAction);

            var player = Player.m_localPlayer;
            if (!player.NoCostCheat())
            {
                if (!LocalPlayerCanAffordCost(cost))
                {
                    Debug.LogError("[Enchant Item] ERROR: Tried to enchant item but could not afford the cost. This should not happen!");
                    return;
                }

                foreach (var costElement in cost)
                {
                    InventoryManagement.Instance.RemoveItem(costElement.GetItem());
                }
            }

            if (_successDialog != null)
            {
                Destroy(_successDialog);
            }

            DeselectAll();
            Lock();

            _successDialog = EnchantItem(item, _runeAction);

            RefreshAvailableItems();
        }

        protected override AudioClip GetCompleteAudioClip()
        {
            return EnchantCompleteSFX[(int)_runeAction];
        }

        public void RefreshAvailableItems()
        {
            var items = GetEnchantableItems();
            AvailableItems.SetItems(items.Cast<IListElement>().ToList());
            AvailableItems.DeselectAll();
            OnSelectedItemsChanged();
        }

        protected override void OnSelectedItemsChanged()
        {
            OnRarityChanged();
        }

        public override bool CanCancel()
        {
            return base.CanCancel() || (_successDialog != null && _successDialog.activeSelf);
        }

        public override void Cancel()
        {
            base.Cancel();

            if (_successDialog != null && _successDialog.activeSelf)
            {
                Destroy(_successDialog);
                _successDialog = null;
            }

            OnRarityChanged();
        }

        public override void Lock()
        {
            base.Lock();

            foreach (var modeButton in RuneActionButtons)
            {
                modeButton.interactable = false;
            }
        }

        public override void Unlock()
        {
            base.Unlock();

            foreach (var modeButton in RuneActionButtons)
            {
                modeButton.interactable = true;
            }
        }

        public override void DeselectAll()
        {
            AvailableItems.DeselectAll();
        }
    }
}
