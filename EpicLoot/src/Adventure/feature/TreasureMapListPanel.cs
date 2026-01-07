using EpicLoot_UnityLib;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace EpicLoot.Adventure.Feature
{
    class TreasureMapListPanel : MerchantListPanel<TreasureMapListElement>
    {
        private readonly MerchantPanel _merchantPanel;
        private IEnumerator SpawnTreasureChestCoroutine;

        public TreasureMapListPanel(MerchantPanel merchantPanel, TreasureMapListElement elementPrefab) 
            : base(
                merchantPanel.transform.Find("TreasureMap/Panel/ItemList") as RectTransform,
                elementPrefab,
                merchantPanel.transform.Find("TreasureMap/TreasureMapBuyButton").GetComponent<Button>(),
                merchantPanel.transform.Find("TreasureMap/TimeLeft").GetComponent<Text>())
        {
            _merchantPanel = merchantPanel;
        }

        public override bool NeedsRefresh(bool currenciesChanged)
        {
            return currenciesChanged || _currentInterval != AdventureDataManager.TreasureMaps.GetCurrentInterval();
        }

        public override void RefreshButton(Currencies playerCurrencies)
        {
            var selectedItem = GetSelectedItem();
            MainButton.interactable = selectedItem != null && selectedItem.CanAfford && !selectedItem.AlreadyPurchased;

            var tooltip = MainButton.GetComponent<UITooltip>();
            if (tooltip != null)
            {
                tooltip.m_text = "";
                if (selectedItem != null && !selectedItem.CanAfford)
                {
                    tooltip.m_text = "$mod_epicloot_merchant_cannotafford";
                }
                else if (selectedItem != null && selectedItem.AlreadyPurchased)
                {
                    tooltip.m_text = "$mod_epicloot_merchant_purchasedtooltip";
                }
            }
        }

        protected override void OnMainButtonClicked()
        {
            if (SpawnTreasureChestCoroutine != null)
            {
                return;
            }

            var player = Player.m_localPlayer;
            if (player == null)
            {
                return;
            }

            var treasureMap = GetSelectedItem();
            if (treasureMap != null)
            {
                SpawnTreasureChestCoroutine = AdventureDataManager.TreasureMaps
                    .SpawnTreasureChest(treasureMap.Biome, player, treasureMap.Price, OnSpawnTreasureChest);
                player.StartCoroutine(SpawnTreasureChestCoroutine);
            }
        }

        private void OnSpawnTreasureChest(int price, bool success, Vector3 position)
        {
            if (success)
            {
                InventoryManagement.Instance.RemoveItem(MerchantPanel.GetCoinsName(), price);
                
                if (StoreGui.instance.m_trader != null)
                {
                    StoreGui.instance.m_trader.OnBought(null);
                }

                StoreGui.instance.m_buyEffects?.Create(Player.m_localPlayer.transform.position, Quaternion.identity);
            }

            SpawnTreasureChestCoroutine = null;
        }

        public override void RefreshItems(Currencies currencies)
        {
            _currentInterval = AdventureDataManager.TreasureMaps.GetCurrentInterval();
            EpicLoot.Log($"[TreasureMap] RefreshItems: Starting panel refresh, interval={_currentInterval}");

            DestroyAllListElementsInList();
            var allItems = AdventureDataManager.TreasureMaps.GetTreasureMaps();
            EpicLoot.Log($"[TreasureMap] RefreshItems: Got {allItems.Count} treasure maps to display");
            for (var index = 0; index < allItems.Count; index++)
            {
                var itemInfo = allItems[index];
                EpicLoot.Log($"[TreasureMap] RefreshItems: Creating UI element for biome={itemInfo.Biome} (value={(int)itemInfo.Biome}), cost={itemInfo.Cost}");
                var itemElement = Object.Instantiate(ElementPrefab, List);
                itemElement.gameObject.SetActive(true);
                itemElement.SetItem(itemInfo, currencies.Coins);
                var i = index;
                itemElement.OnSelected += (x) => OnItemSelected(i);
                itemElement.SetSelected(i == _selectedItemIndex);
            }
            EpicLoot.Log($"[TreasureMap] RefreshItems: Panel refresh complete");
        }

        public override void UpdateRefreshTime()
        {
            UpdateRefreshTime(AdventureDataManager.TreasureMaps.GetSecondsUntilRefresh());
        }
    }
}
