using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace EpicLoot_UnityLib
{
    public class AugmentUI : EnchantingTableUIPanelBase
    {
        public Text AvailableEffectsText;
        public Text AvailableEffectsHeader;
        public Scrollbar AvailableEffectsScrollbar;

        public RectTransform EnchantList;
        public GameObject EnchantmentListPrefab;

        [Header("Cost")]
        public Text CostLabel;
        public MultiSelectItemList CostList;

        public delegate List<InventoryItemListElement> GetAugmentableItemsDelegate();
        public delegate List<Tuple<string, bool>> GetAugmentableEffectsDelegate(ItemDrop.ItemData item, bool runemode);
        public delegate string GetAvailableEffectsDelegate(ItemDrop.ItemData item, int augmentIndex);
        public delegate List<InventoryItemListElement> GetAugmentCostDelegate(ItemDrop.ItemData item, int augmentIndex);
        // Returns the augment choice dialog
        public delegate GameObject AugmentItemDelegate(ItemDrop.ItemData item, int augmentIndex);

        public static GetAugmentableItemsDelegate GetAugmentableItems;
        public static GetAugmentableEffectsDelegate GetAugmentableEffects;
        public static GetAvailableEffectsDelegate GetAvailableEffects;
        public static GetAugmentCostDelegate GetAugmentCost;
        public static AugmentItemDelegate AugmentItem;

        private int _augmentIndex;
        private GameObject _choiceDialog;
        private List<Toggle> _AugmentSelectors = new List<Toggle>();

        public override void Awake()
        {
            base.Awake();
        }

        [UsedImplicitly]
        public void OnEnable()
        {

            if (EnchantList.childCount > 0)
            {
                foreach (Transform child in EnchantList)
                {
                    Destroy(child.gameObject);
                }
            }
            _AugmentSelectors.Clear();
            _augmentIndex = -1;

            if (AvailableEffectsHeader != null)
            {
                var augmentChoices = 2;
                var featureValues = EnchantingTableUI.instance.SourceTable.GetFeatureCurrentValue(EnchantingFeature.Augment);
                if (!float.IsNaN(featureValues.Item1))
                {
                    augmentChoices = (int)featureValues.Item1;
                }

                var colorPre = augmentChoices > 2 ? "<color=#EAA800>" : "";
                var colorPost = augmentChoices > 2 ? "</color>" : "";
                AvailableEffectsHeader.text = Localization.instance.Localize($"$mod_epicloot_augment_availableeffects {colorPre}($mod_epicloot_augment_choices){colorPost}", augmentChoices.ToString());
            }

            OnAugmentIndexChanged();

            var items = GetAugmentableItems();
            AvailableItems.SetItems(items.Cast<IListElement>().ToList());
            DeselectAll();
        }

        public override void Update()
        {
            base.Update();

            if (!_locked && ZInput.IsGamepadActive())
            {
                if (ZInput.GetButtonDown("JoyButtonY"))
                {
                    var activeAugmentCount = _AugmentSelectors.Count();
                    var nextAugmentIndex = (_augmentIndex + 1) % activeAugmentCount;
                    _AugmentSelectors[nextAugmentIndex].isOn = true;
                    ZInput.ResetButtonStatus("JoyButtonY");
                }

                if (AvailableEffectsScrollbar != null)
                {
                    var rightStickAxis = ZInput.GetJoyRightStickY();
                    if (Mathf.Abs(rightStickAxis) > 0.5f)
                    {
                        AvailableEffectsScrollbar.value = Mathf.Clamp01(AvailableEffectsScrollbar.value + rightStickAxis * -0.1f);
                    }
                }
            }

            if (_choiceDialog != null && !_choiceDialog.activeSelf)
            {
                Unlock();
                Destroy(_choiceDialog);
                _choiceDialog = null;
                Cancel();

                AvailableItems.ForeachElement((i, e) =>
                {
                    if (!e.IsSelected())
                    {
                        return;
                    }
                    e.SetItem(e.GetListElement());
                    e.Refresh();
                });
                RefreshAugmentSelectors();
                OnAugmentIndexChanged();
            }
        }

        public void SelectAugmentIndex(int index)
        {
            if (index != _augmentIndex) {
                Debug.Log($"Setting augment index {index}");
                _augmentIndex = index;
                OnAugmentIndexChanged();
            }
        }

        public void OnAugmentIndexChanged()
        {
            var selectedItem = AvailableItems.GetSingleSelectedItem<InventoryItemListElement>();
            if (selectedItem?.Item1.GetItem() == null)
            {
                MainButton.interactable = false;
                AvailableEffectsText.text = "";
                CostLabel.enabled = false;
                CostList.SetItems(new List<IListElement>());
                _augmentIndex = -1;
                return;
            }

            

            if (_augmentIndex < 0)
            {
                AvailableEffectsText.text = string.Empty;
                CostLabel.enabled = false;
                CostList.SetItems(new List<IListElement>());
                MainButton.interactable = false;
            }
            else
            {
                var item = selectedItem.Item1.GetItem();
                Debug.Log($"OnAugmentIndexChanged called with index {_augmentIndex} for item {item}");
                var info = GetAvailableEffects(item, _augmentIndex);
                
                AvailableEffectsText.text = info;
                ScrollEnchantInfoToTop();

                CostLabel.enabled = true;
                var cost = GetAugmentCost(item, _augmentIndex);
                CostList.SetItems(cost.Cast<IListElement>().ToList());

                var featureValues = EnchantingTableUI.instance.SourceTable.GetFeatureCurrentValue(EnchantingFeature.Augment);
                var reenchantCostReduction = float.IsNaN(featureValues.Item2) ? 0 : featureValues.Item2;
                if (reenchantCostReduction > 0)
                {
                    CostLabel.text = Localization.instance.Localize($"$mod_epicloot_augmentcost " +
                        $"<color=#EAA800>(-{reenchantCostReduction}% $item_coins!)</color>");
                }
                else
                {
                    CostLabel.text = Localization.instance.Localize("$mod_epicloot_augmentcost");
                }

                var canAfford = LocalPlayerCanAffordCost(cost);
                var featureUnlocked = EnchantingTableUI.instance.SourceTable.IsFeatureUnlocked(EnchantingFeature.Augment);
                MainButton.interactable = featureUnlocked && canAfford && _augmentIndex >= 0;
            }
        }

        private void ScrollEnchantInfoToTop()
        {
            AvailableEffectsScrollbar.value = 1;
        }

        protected override void DoMainAction()
        {
            var selectedItem = AvailableItems.GetSingleSelectedItem<InventoryItemListElement>();
            if (selectedItem?.Item1.GetItem() == null)
            {
                Cancel();
                return;
            }

            var item = selectedItem.Item1.GetItem();
            var cost = GetAugmentCost(item, _augmentIndex);

            var player = Player.m_localPlayer;
            if (!player.NoCostCheat())
            {
                if (!LocalPlayerCanAffordCost(cost))
                {
                    Debug.LogError("[Augment Item] ERROR: Tried to augment item but could not afford the cost. This should not happen!");
                    return;
                }

                foreach (var costElement in cost)
                {
                    InventoryManagement.Instance.RemoveItem(costElement.GetItem());
                }
            }

            if (_choiceDialog != null)
            {
                Destroy(_choiceDialog);
            }

            _choiceDialog = AugmentItem(item, _augmentIndex);

            Lock();
        }

        protected override void OnSelectedItemsChanged()
        {
            var entry = AvailableItems.GetSingleSelectedItem<InventoryItemListElement>();
            var item = entry?.Item1.GetItem();

            RefreshAugmentSelectors();

            if (item == null)
            {
                AvailableEffectsText.text = string.Empty;
            }

            _augmentIndex = 0;
            OnAugmentIndexChanged();
        }

        private void RefreshAugmentSelectors()
        {
            var entry = AvailableItems.GetSingleSelectedItem<InventoryItemListElement>();
            Debug.Log($"Refreshing augment selectors for {entry} selectors");
            // Clear the enchantment list
            if (EnchantList.childCount > 0) {
                foreach (Transform child in EnchantList) {
                    Destroy(child.gameObject);
                }
            }
            _AugmentSelectors.Clear();
            Debug.Log($"Cleared lists");
            if (entry == null || entry.Item1 == null) {
                return;
            }
            var item = entry?.Item1.GetItem();
            Debug.Log($"Getting augment effects for {item}");
            var augmentableEffects = GetAugmentableEffects(item, false);

            Debug.Log($"Got augmentable effects {augmentableEffects.Count}");

            int enchantIndex = 0;
            foreach (var effect in augmentableEffects)
            {
                Debug.Log($"Adding enchantment {effect.Item1} at index {enchantIndex}");
                var enchantmentListElement = Instantiate(EnchantmentListPrefab, EnchantList);
                var enchantmentElement = enchantmentListElement.GetComponentInChildren<Text>();
                var enchantmentbutton = enchantmentListElement.GetComponent<Toggle>();
                foreach (var audio_source in enchantmentListElement.GetComponentsInChildren<AudioSource>()) {
                    audio_source.volume = AudioVolumeLevel();
                }
                _AugmentSelectors.Add(enchantmentbutton);
                enchantmentbutton.onValueChanged.AddListener((isOn) => {
                    if (isOn) {
                        SelectAugmentIndex(_AugmentSelectors.IndexOf(enchantmentbutton));
                    }
                });
                //if (enchantmentbutton != null) { enchantmentbutton.interactable = true; }
                if (enchantmentElement != null) {
                    enchantmentElement.text = effect.Item1;
                }
                if (effect.Item2 == false) {
                    enchantmentbutton.interactable = false;
                }
                enchantmentListElement.SetActive(true);
                
                enchantIndex++;
            }
        }

        public override bool CanCancel()
        {
            return base.CanCancel() || (_choiceDialog != null && _choiceDialog.activeSelf);
        }

        public override void Cancel()
        {
            base.Cancel();
            OnAugmentIndexChanged();
        }

        public override void Lock()
        {
            base.Lock();
            foreach (var selector in _AugmentSelectors)
            {
                selector.interactable = false;
            }
        }

        public override void Unlock()
        {
            base.Unlock();
            foreach (var selector in _AugmentSelectors)
            {
                selector.interactable = true;
            }
        }

        public override void DeselectAll()
        {
            AvailableItems.DeselectAll();
        }
    }
}
