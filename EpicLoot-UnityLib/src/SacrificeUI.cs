using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace EpicLoot_UnityLib
{
    public class SacrificeUI : EnchantingTableUIPanelBase
    {
        enum SacrificeMode {
            Sacrifice,
            Identify
        }

        public Toggle SacrificeToggle;
        public Toggle IdentifyToggle;
        public GameObject IdentifyStylePanel;
        public MultiSelectItemList CostList;

        public Dropdown IdentifyStyle;
        // 0 - random
        // 1 - weapon
        // 2 - armor
        // 3 - ranged
        // 4 - melee
        // 5 - magic
        // 6 - accessory
        //

        public MultiSelectItemList SacrificeProducts;
        public EnchantBonus BonusPanel;
        public Text Warning;
        public Text Explainer;

        public delegate List<InventoryItemListElement> GetSacrificeItemsDelegate();
        public delegate List<InventoryItemListElement> GetSacrificeProductsDelegate(List<Tuple<ItemDrop.ItemData, int>> items);
        public delegate List<InventoryItemListElement> GetIdentifyCostDelegate(int filterType, List<Tuple<ItemDrop.ItemData, int>> unidentifiedItems);
        public delegate List<InventoryItemListElement> GetIdentifyItemsDelegate();
        public delegate List<InventoryItemListElement> GetRandomFilteredLootRollDelegate(int filterType, List<Tuple<ItemDrop.ItemData, int>> unidentifiedItems);
        public delegate List<InventoryItemListElement> GetPotentialIdentificationsDelegate(int filterType, List<ItemDrop.ItemData> items_selected);

        public static GetSacrificeItemsDelegate GetSacrificeItems;
        public static GetSacrificeProductsDelegate GetSacrificeProducts;
        public static GetIdentifyItemsDelegate GetIdentifyItems;
        public static GetIdentifyCostDelegate GetIdentifyCost;
        public static GetRandomFilteredLootRollDelegate GetRandomFilteredLoot;
        public static GetPotentialIdentificationsDelegate GetPotentialIdentifications;

        SacrificeMode _sacrificeMode = SacrificeMode.Sacrifice;

        public override void Awake()
        {
            base.Awake();

            SacrificeToggle.onValueChanged.AddListener((isOn) => {
                SacrificeModeSelected(isOn);
            });
            IdentifyToggle.onValueChanged.AddListener((isOn) => {
                IdentifyModeSelected(isOn);
            });

            // For some reason localization does not automatically apply to the dropdown options
            foreach (var entry in IdentifyStyle.options) {
                //if (!entry.text.Contains("$")) { continue; }
                entry.text = Localization.instance.Localize(entry.text);
            }

            // Trigger cost update when the identify style changes
            IdentifyStyle.onValueChanged.AddListener((value) => {
                OnSelectedItemsChanged();
            });
        }

        [UsedImplicitly]
        public void OnEnable()
        {
            var items = GetSacrificeItems();
            _sacrificeMode = SacrificeMode.Sacrifice;
            IdentifyStylePanel.SetActive(false);
            IdentifyToggle.isOn = false;
            SacrificeToggle.isOn = true;
            //SacrificeModeSelected(true);
            AvailableItems.SetItems(items.Cast<IListElement>().ToList());
            AvailableItems.DeselectAll();
        }

        protected override void DoMainAction()
        {
            if (_sacrificeMode == SacrificeMode.Identify)
            {
                IdentifyItems();
            }
            else if (_sacrificeMode == SacrificeMode.Sacrifice)
            {
                SacrificeItems();
            }
            Unlock();
        }

        private void IdentifyItems()
        {
            var selectedItems = AvailableItems.GetSelectedItems<IListElement>();
            var unidentifiedItems = selectedItems.Select(x => new Tuple<ItemDrop.ItemData, int>(x.Item1.GetItem(), x.Item2)).ToList();
            var filterType = IdentifyStyle.value;
            var player = Player.m_localPlayer;
            var cost = GetIdentifyCost(filterType, unidentifiedItems);

            if (!LocalPlayerCanAffordCost(cost)) {
                return;
            }

            if (!player.NoCostCheat()) {
                foreach (var costElement in cost)
                {
                    InventoryManagement.Instance.RemoveItem(costElement.GetItem());
                }
            }

            var identifiedItems = GetRandomFilteredLoot(filterType, unidentifiedItems);

            Cancel();
            RefreshAvailableItems();
            AvailableItems.GiveFocus(true, 0);
        }

        private void SacrificeItems()
        {
            var selectedItems = AvailableItems.GetSelectedItems<IListElement>();
            var sacrificeProducts = GetSacrificeProducts(selectedItems.Select(x => new Tuple<ItemDrop.ItemData, int>(x.Item1.GetItem(), x.Item2)).ToList());

            Cancel();

            var chanceToDoubleEntry = EnchantingTableUI.instance.SourceTable.GetFeatureCurrentValue(EnchantingFeature.Sacrifice);
            var chanceToDouble = float.IsNaN(chanceToDoubleEntry.Item1) ? 0.0f : chanceToDoubleEntry.Item1 / 100.0f;

            if (Random.Range(0.0f, 1.0f) < chanceToDouble)
            {
                EnchantingTableUI.instance.PlayEnchantBonusSFX();
                BonusPanel.Show();

                foreach (var sacrificeProduct in sacrificeProducts)
                {
                    sacrificeProduct.Item.m_stack *= 2;
                }
            }

            foreach (var selectedItem in selectedItems)
            {
                InventoryManagement.Instance.RemoveExactItem(selectedItem.Item1.GetItem(), selectedItem.Item2);
            }

            GiveItemsToPlayer(sacrificeProducts);

            RefreshAvailableItems();
            AvailableItems.GiveFocus(true, 0);
        }

        private void SacrificeModeSelected(bool isOn) {
            if (!isOn) { return; }
            _sacrificeMode = SacrificeMode.Sacrifice;
            var items = GetSacrificeItems();
            AvailableItems.SetItems(items.Cast<IListElement>().ToList());
            AvailableItems.DeselectAll();
            Warning.text = Localization.instance.Localize("$mod_epicloot_sacrifice_warning");
            Warning.color = Color.red;
            Explainer.text = Localization.instance.Localize("$mod_epicloot_sacrifice_productsexplainer");
            MainButton.GetComponentInChildren<Text>().text = Localization.instance.Localize("$mod_epicloot_sacrifice");
            OnSelectedItemsChanged();
            IdentifyStylePanel.SetActive(false);
            CostList.gameObject.SetActive(false);
        }

        private void IdentifyModeSelected(bool isOn) {
            if (!isOn) { return; }
            _sacrificeMode = SacrificeMode.Identify;
            var items = GetIdentifyItems();
            AvailableItems.SetItems(items.Cast<IListElement>().ToList());
            AvailableItems.DeselectAll();
            OnSelectedItemsChanged();
            Warning.text = Localization.instance.Localize("$mod_epicloot_identify_explain");
            Warning.color = new Color(1f, 0.631f, 0.235f);
            Explainer.text = Localization.instance.Localize("$mod_epicloot_identify_productsexplainer");
            MainButton.GetComponentInChildren<Text>().text = Localization.instance.Localize("$mod_epicloot_identify");
            //IdentifyStyle.gameObject.SetActive(true);
            IdentifyStylePanel.SetActive(true);
            CostList.gameObject.SetActive(true);
        }

        private void RefreshAvailableItems() {
            if (_sacrificeMode == SacrificeMode.Identify) {
                var items = GetIdentifyItems();
                AvailableItems.SetItems(items.Cast<IListElement>().ToList());
            } else if (_sacrificeMode == SacrificeMode.Sacrifice) {
                var items = GetSacrificeItems();
                AvailableItems.SetItems(items.Cast<IListElement>().ToList());
            }
            AvailableItems.DeselectAll();
            OnSelectedItemsChanged();
        }

        protected override void OnSelectedItemsChanged()
        {
            var selectedItems = AvailableItems.GetSelectedItems<IListElement>();
            if (_sacrificeMode == SacrificeMode.Sacrifice) {
                var sacrificeProducts = GetSacrificeProducts(selectedItems.Select(x => new Tuple<ItemDrop.ItemData, int>(x.Item1.GetItem(), x.Item2)).ToList());
                SacrificeProducts.SetItems(sacrificeProducts.Cast<IListElement>().ToList());
            }
            bool canAfford = true;
            if (_sacrificeMode == SacrificeMode.Identify) {
                var potentialIdentifyItems = GetPotentialIdentifications(IdentifyStyle.value, selectedItems.Select(x => x.Item1.GetItem()).ToList());
                SacrificeProducts.SetItems(potentialIdentifyItems.Cast<IListElement>().ToList());
                var unidentifiedItems = selectedItems.Select(x => new Tuple<ItemDrop.ItemData, int>(x.Item1.GetItem(), x.Item2)).ToList();
                var cost = GetIdentifyCost(IdentifyStyle.value, unidentifiedItems);
                CostList.SetItems(cost.Cast<IListElement>().ToList());
                canAfford = LocalPlayerCanAffordIdentifyCost(cost);
                if (potentialIdentifyItems.Count() == 0) { canAfford = false; }
            }

            var featureUnlocked = EnchantingTableUI.instance != null && EnchantingTableUI.instance.SourceTable != null && EnchantingTableUI.instance.SourceTable.IsFeatureUnlocked(EnchantingFeature.Sacrifice);
            MainButton.interactable = featureUnlocked && selectedItems.Count > 0 && canAfford;
        }

        public bool LocalPlayerCanAffordIdentifyCost(List<InventoryItemListElement> cost)
        {
            if (cost == null || cost.Count == 0)
                return true;
            var player = Player.m_localPlayer;
            if (player == null)
                return false;

            if (Player.m_localPlayer.NoCostCheat()){
                Debug.Log($"Rune nocost mode success.");
                return true;
            }

            foreach (var element in cost)
            {
                var item = element.GetItem();
                if (!InventoryManagement.Instance.HasItem(item))
                {
                    Debug.Log($"Identify Cost failed, user does not have item {item.m_shared.m_name}.");
                    return false;
                }
            }

            return true;
        }

        public override void Cancel() {
            if (_sacrificeMode == SacrificeMode.Sacrifice) {
                if (_useTMP)
                    _tmpButtonLabel.text = Localization.instance.Localize("$mod_epicloot_sacrifice");
                else
                    _buttonLabel.text = Localization.instance.Localize("$mod_epicloot_sacrifice");
            }
            if (SacrificeMode.Identify == _sacrificeMode) {
                if (_buttonLabel != null) {
                    _buttonLabel.text = Localization.instance.Localize("$mod_epicloot_identify");
                }
            }

            Unlock();
            // base.Cancel();
        }
        
        public override void DeselectAll()
        {
            AvailableItems.DeselectAll();
        }

        public override void Lock()
        {
            base.Lock();

            SacrificeToggle.interactable = false;
            IdentifyToggle.interactable = false;
        }

        public override void Unlock()
        {
            base.Unlock();

            SacrificeToggle.interactable = true;
            IdentifyToggle.interactable = true;
        }
    }
}
