using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace EpicLoot_UnityLib
{
    public class RuneUI : EnchantingTableUIPanelBase
    {
        public Toggle RuneExtractButton;
        public Toggle RuneEtchButton;
        //public Toggle RuneImbueButton;

        [Header("Cost")]
        public Text CostLabel;
        public MultiSelectItemList CostList;

        [Header("Rune Selector")]
        public RectTransform EnchantList;
        public GameObject EnchantmentListPrefab;
        public MultiSelectItemList AvailableRunes;

        public AudioClip RunicActionCompleted;


        // These use delegates which are connected at runtime from the non-unity side of EL
        public delegate List<InventoryItemListElement> GetRuneModifyableItemsDelegate();
        public delegate List<InventoryItemListElement> GetApplyableRunesDelegate(ItemDrop.ItemData item, string selected_enchantment);
        public delegate List<Tuple<string, bool>> GetItemEnchantsDelegate(ItemDrop.ItemData item, bool runecheck);
        public delegate List<InventoryItemListElement> GetRuneExtractCostDelegate(ItemDrop.ItemData item, MagicRarityUnity rarity, float cost_modifier);
        public delegate List<InventoryItemListElement> GetRuneEtchCostDelegate(ItemDrop.ItemData item, MagicRarityUnity rarity, float cost_modifier);
        public delegate bool RuneItemDestructionEnabledDelegate();
        public delegate MagicRarityUnity GetItemRarityDelegate(ItemDrop.ItemData item);
        public delegate ItemDrop.ItemData GetItemEnchantedByRuneDelegate(ItemDrop.ItemData item,  int enchantment, float power_mod);
        public delegate string GetSelectedEnchantmentByIndexDelegate(ItemDrop.ItemData item, int enchantment);
        // returns the modified item
        public delegate GameObject ApplyRuneToItemAndReturnSuccess(ItemDrop.ItemData item, ItemDrop.ItemData rune, int enchantment);

        public static GetApplyableRunesDelegate GetApplyableRunes;
        public static GetRuneModifyableItemsDelegate GetRuneModifyableItems;
        public static GetItemEnchantsDelegate GetItemEnchants;
        public static GetRuneExtractCostDelegate GetRuneExtractCost;
        public static GetRuneEtchCostDelegate GetRuneEtchCost;
        public static GetItemEnchantedByRuneDelegate ItemToBeRuned;
        public static RuneItemDestructionEnabledDelegate ExtractItemsDestroyed;
        public static GetItemRarityDelegate GetItemRarity;
        public static ApplyRuneToItemAndReturnSuccess RuneEnchancedItem;
        public static GetSelectedEnchantmentByIndexDelegate GetSelectedEnchantmentByIndex;

        private RuneAction _runeAction;
        private GameObject _successDialog;
        private ItemDrop.ItemData _selectedItem;
        private ItemDrop.ItemData _selectedOverrideRune;
        private MagicRarityUnity _selectedRarity = MagicRarityUnity.Magic; // Default to Magic rarity
        private int _selectedEnchantmentIndex = -1;

        private enum RuneAction
        {
            Extract,
            Etch,
            //Imbue
        }

        public override void Awake()
        {
            base.Awake();

            RuneExtractButton.onValueChanged.AddListener((isOn) => {
                ExtractModeSelected(isOn);
            });
            RuneEtchButton.onValueChanged.AddListener((isOn) => {
                EtchModeSelected(isOn);
            });
            AvailableRunes.OnSelectedItemsChanged += OnSelectedOverrideRuneChanged;
        }

        [UsedImplicitly]
        public void OnEnable()
        {
            _runeAction = RuneAction.Extract;
            RuneExtractButton.isOn = true;
            var items = GetRuneModifyableItems();
            AvailableItems.SetItems(items.Cast<IListElement>().ToList());
            AvailableItems.DeselectAll();
            ClearEnchantmentList();
            AvailableRunes.SetItems(new List<IListElement>());
            _selectedEnchantmentIndex = 0;
        }

        public override void Update()
        {
            base.Update();

            bool featureUnlocked = EnchantingTableUI.instance.SourceTable.IsFeatureUnlocked(EnchantingFeature.Rune);
            if (!featureUnlocked && !Player.m_localPlayer.NoCostCheat()) {
                return;
            }



            // TODO add controller support
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

        public void UpdateDisplaySelectedItemEnchantments(ItemDrop.ItemData selected_item) {
            if (selected_item == null) {
                return;
            }

            // Set the enchantments to be selected based on the enchantments on this item
            var info = GetItemEnchants(_selectedItem, true);
            RefreshSelectableEnchantments();
            UpdateDisplayAvailableOverwriteEnchantments();
            // Set enchantment list to the enchantments of the selected item

            var featureValues = EnchantingTableUI.instance.SourceTable.GetFeatureCurrentValue(EnchantingFeature.Rune);
            float cost_reduction = featureValues.Item1 == 0f || featureValues.Item1 == float.NaN ? 1.0f : 1f - (featureValues.Item1 / 100f);
            float power_modifier = featureValues.Item2 == float.NaN ? 1.0f : (featureValues.Item2 / 100f);

            // Cost for Extracting the specific enchantment
            if (_runeAction == RuneAction.Extract) {
                CostLabel.enabled = true;
                var cost = GetRuneExtractCost(_selectedItem, _selectedRarity, cost_reduction);
                CostList.SetItems(cost.Cast<IListElement>().ToList());
                CheckIfActionDoable();
                MainButton.GetComponentInChildren<Text>().text = Localization.instance.Localize("$mod_epicloot_rune_extract");
            }
            
            // Cost for Etching the specific enchantment 
            if (_runeAction == RuneAction.Etch) {
                CostLabel.enabled = true;
                var cost = GetRuneEtchCost(_selectedItem, _selectedRarity, cost_reduction);
                CostList.SetItems(cost.Cast<IListElement>().ToList());
                CheckIfActionDoable();
                MainButton.GetComponentInChildren<Text>().text = Localization.instance.Localize("$mod_epicloot_rune_etch");
            }
        }

        public void UpdateDisplayAvailableOverwriteEnchantments()
        {
            if (_selectedItem == null || _runeAction == RuneAction.Extract || _selectedEnchantmentIndex <= -1) {
                AvailableRunes.SetItems(new List<IListElement>());
                return;
            }

            var availableEnchantRunes = GetApplyableRunes(_selectedItem, GetSelectedEnchantmentByIndex(_selectedItem, _selectedEnchantmentIndex));
            AvailableRunes.SetItems(availableEnchantRunes.Cast<IListElement>().ToList());
        }

        private void ClearEnchantmentList() {
            // Clear the enchantment list
            if (EnchantList.childCount > 0) {
                foreach (Transform child in EnchantList) {
                    Destroy(child.gameObject);
                }
            }
            _selectedEnchantmentIndex = -1;
        }

        private void RefreshSelectableEnchantments()
        {
            var entry = AvailableItems.GetSingleSelectedItem<InventoryItemListElement>();
            var item = entry?.Item1.GetItem();
            var augmentableEffects = GetItemEnchants(item, true);

            ClearEnchantmentList();

            int enchantIndex = 0;
            foreach(var effect in augmentableEffects) {
                // Debug.Log($"Adding enchantment {effect.Item1} at index {enchantIndex}");
                var enchantmentListElement = Instantiate(EnchantmentListPrefab, EnchantList);
                var enchantmentElement = enchantmentListElement.GetComponentInChildren<Text>();
                var enchantmentbutton = enchantmentListElement.GetComponent<Toggle>();
                enchantmentbutton.onValueChanged.AddListener((isOn) => {
                    if (isOn) {
                        SetSelectedEnchantIndex();
                        UpdateDisplayAvailableOverwriteEnchantments();
                    }
                });
                //if (enchantmentbutton != null) { enchantmentbutton.interactable = true; }
                if (enchantmentElement != null) {
                    enchantmentElement.text = effect.Item1;
                }
                enchantmentListElement.SetActive(true);
                enchantIndex++;
            }
        }

        private void SetSelectedEnchantIndex() {
            if (EnchantList.childCount > 0) {
                int index = 0;
                foreach (Transform child in EnchantList)
                {
                    if (child.GetComponent<Toggle>().isOn == true) {
                        _selectedEnchantmentIndex = index;
                        Debug.Log($"Setting selected enchantment index to {index}");
                        CheckIfActionDoable();
                        return;
                    }
                    index++;
                }
            }
            _selectedEnchantmentIndex = -1;
        }

        public bool LocalPlayerCanAffordRuneCost(List<InventoryItemListElement> cost)
        {
            if (cost == null || cost.Count == 0)
                return true;
            var player = Player.m_localPlayer;
            if (player == null)
                return false;

            if (Player.m_localPlayer.NoCostCheat()) {
                Debug.Log($"Rune nocost mode success.");
                return true;
            }

            foreach (var element in cost) {
                var item = element.GetItem();
                if (!InventoryManagement.Instance.HasItem(item)) {
                    Debug.Log($"Rune Cost failed, user does not have item {item.m_shared.m_name}.");
                    return false;
                }
            }

            // Etch costs - this only costs the source rune
            if (_runeAction == RuneAction.Etch) {
                
                // TODO: show cost is the one rune
                CostLabel.enabled = false;
                Debug.Log($"Rune etch cost check not implemented.");
                return true;
            }

            // Extracting cost requirements
            if (_runeAction == RuneAction.Extract) {
                // If we got herer we have all the stuff
                return true;
            }

            // Imbue costs
            //if (_runeAction == RuneAction.Imbue) {
            //    CostLabel.enabled = false;
            //    return true;
            //}

            // Fallthough, we should never hit this
            Debug.Log("Rune Cost fallthrough false");
            return false;
        }

        public void ExtractModeSelected(bool enabled)
        {
            if (!enabled) { return; }
            _runeAction = RuneAction.Extract;
            var selectedItem = AvailableItems.GetSingleSelectedItem<InventoryItemListElement>();
            // Deselect runes and clear them
            if (AvailableRunes.GetItemCount() > 0) {
                //AvailableRunes.DeselectAll();
                AvailableRunes.SetItems(new List<IListElement>());
            }
            // Clears the list of enchantments if no item is selected
            if (selectedItem?.Item1.GetItem() == null) {
                CostLabel.enabled = false;
                CostList.SetItems(new List<IListElement>());
            } else {
                // Check the currently selected item
                if (selectedItem?.Item1.GetItem() != _selectedItem) {
                    _selectedItem = selectedItem.Item1.GetItem();
                    _selectedRarity = GetItemRarity(_selectedItem);
                }
                UpdateDisplaySelectedItemEnchantments(_selectedItem);
            }
        }

        public void EtchModeSelected(bool enabled)
        {
            if (!enabled) { return; }
            _runeAction = RuneAction.Etch;
            var selectedItem = AvailableItems.GetSingleSelectedItem<InventoryItemListElement>();

            if (selectedItem?.Item1.GetItem() == null) {
                CostLabel.enabled = false;
                CostList.SetItems(new List<IListElement>());
            } else {
                // Check the currently selected item
                if (selectedItem?.Item1.GetItem() != _selectedItem) {
                    _selectedItem = selectedItem.Item1.GetItem();
                    _selectedRarity = GetItemRarity(_selectedItem);
                }
                UpdateDisplaySelectedItemEnchantments(_selectedItem);
            }

        }

        protected override void DoMainAction()
        {
            var selectedItem = AvailableItems.GetSelectedItems<InventoryItemListElement>().FirstOrDefault();

            // Clear any currently existing success dialog
            Cancel();

            if (selectedItem?.Item1.GetItem() == null) {
                return;
            }

            var featureValues = EnchantingTableUI.instance.SourceTable.GetFeatureCurrentValue(EnchantingFeature.Rune);
            float cost_reduction = featureValues.Item1 == 0f || featureValues.Item1 == float.NaN ? 1.0f : 1f - (featureValues.Item1 / 100f);
            float power_modifier = featureValues.Item2 == float.NaN ? 1.0f : (featureValues.Item2 / 100f);
            var item = selectedItem.Item1.GetItem();
            Debug.Log($"Selected item: {item.m_shared.m_name}, rarity: {_selectedRarity}, enchantment index: {_selectedEnchantmentIndex}, power modifier: {power_modifier}");

            if (_runeAction == RuneAction.Extract)
            {
                var cost = GetRuneExtractCost(item, _selectedRarity, cost_reduction);
                var player = Player.m_localPlayer;
                if (!player.NoCostCheat())
                {
                    if (!LocalPlayerCanAffordCost(cost)) {
                        return;
                    }

                    foreach (var costElement in cost)
                    {
                        InventoryManagement.Instance.RemoveItem(costElement.GetItem());
                    }
                }
                bool destroyExtractedItem = ExtractItemsDestroyed();
                Debug.Log($"Rune Extraction requires destruction? {destroyExtractedItem}.");
                if (destroyExtractedItem) {
                    // Destroy the item
                    InventoryManagement.Instance.RemoveItem(item);
                }
                ItemDrop.ItemData RuneWithEnchant = ItemToBeRuned(item, _selectedEnchantmentIndex, power_modifier);
                InventoryManagement.Instance.GiveItem(RuneWithEnchant);
                CostList.SetItems(new List<IListElement>());
            }

            // Modify an existing item and destroy the selected Rune
            if (_runeAction == RuneAction.Etch)
            {
                var rune = AvailableRunes.GetSingleSelectedItem<InventoryItemListElement>().Item1.GetItem();
                var itemtoEtch = selectedItem?.Item1.GetItem();

                if (_successDialog != null) { Destroy(_successDialog); }
                _successDialog = RuneEnchancedItem(itemtoEtch, rune, _selectedEnchantmentIndex);
                _successDialog.SetActive(true);
                // Remove the rune from the inventory
                InventoryManagement.Instance.RemoveItem(rune);
                CostList.SetItems(new List<IListElement>());
            }

            DeselectAll();
            
            //Lock();
            RefreshAvailableItems();
            _selectedEnchantmentIndex = -1;
            CostList.SetItems(new List<IListElement>());
        }

        protected override AudioClip GetCompleteAudioClip()
        {
            return RunicActionCompleted;
        }

        public void RefreshAvailableItems()
        {
            var items = GetRuneModifyableItems();
            RefreshSelectableEnchantments();
            //AvailableItems.SetItems(items.Cast<IListElement>().ToList());
            AvailableItems.DeselectAll();
            OnSelectedItemsChanged();
            //AvailableRunes.DeselectAll();
            //AvailableItems.SetItems(new List<IListElement>());
        }

        protected override void OnSelectedItemsChanged()
        {
            var selectedItem = AvailableItems.GetSingleSelectedItem<InventoryItemListElement>();
            if (selectedItem?.Item1.GetItem() != null)
            {
                _selectedItem = selectedItem.Item1.GetItem();
                _selectedRarity = GetItemRarity(_selectedItem);
                UpdateDisplaySelectedItemEnchantments(_selectedItem);
                _selectedEnchantmentIndex = -1;
            } else {
                ClearEnchantmentList();
            }
        }

        protected void OnSelectedOverrideRuneChanged()
        {
            var rune = AvailableRunes.GetSingleSelectedItem<InventoryItemListElement>();
            if (rune?.Item1.GetItem() != null) {
                _selectedOverrideRune = rune.Item1.GetItem();
                CheckIfActionDoable();
            } else {
                _selectedOverrideRune = null;
            }
        }

        private void CheckIfActionDoable()
        {
            bool state = true;

            // No selected item
            if (_selectedItem == null) {
                Debug.Log("No item selected.");
                state = false;
            }

            // Check for a selected Enchantment, to extract or to etch
            if (_selectedEnchantmentIndex == -1) {
                Debug.Log("No Enchantment selected.");
                state = false;
            }


            // Check costs, ignored if nocost mode
            var featureValues = EnchantingTableUI.instance.SourceTable.GetFeatureCurrentValue(EnchantingFeature.Rune);
            float cost_reduction = featureValues.Item1 == 0f || featureValues.Item1 == float.NaN ? 1.0f : 1f - (featureValues.Item1 / 100f);
            if (_runeAction == RuneAction.Etch && !Player.m_localPlayer.NoCostCheat()) {
                var cost = GetRuneEtchCost(_selectedItem, _selectedRarity, cost_reduction);
                CostList.SetItems(cost.Cast<IListElement>().ToList());
                state = LocalPlayerCanAffordRuneCost(cost);
                Debug.Log($"Afford cost? {state}");
            }
            if (_runeAction == RuneAction.Extract && !Player.m_localPlayer.NoCostCheat()) {
                var cost = GetRuneExtractCost(_selectedItem, _selectedRarity, cost_reduction);
                CostList.SetItems(cost.Cast<IListElement>().ToList());
                state = LocalPlayerCanAffordRuneCost(cost);
                Debug.Log($"Afford cost? {state}");
            }

            // Etching but does not have an override rune selected
            if (_runeAction == RuneAction.Etch && _selectedOverrideRune == null) {
                Debug.Log("No override enchantment selected.");
                state = false;
            }


            MainButton.interactable = state;
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
        }

        public override void Lock()
        {
            base.Lock();

            RuneExtractButton.interactable = false;
            RuneEtchButton.interactable = false;
        }

        public override void Unlock()
        {
            base.Unlock();

            RuneExtractButton.interactable = true;
            RuneEtchButton.interactable = true;
        }

        public override void DeselectAll()
        {
            AvailableItems?.DeselectAll();
            //AvailableRunes?.DeselectAll();
        }
    }
}
