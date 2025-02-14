using EpicLoot.src.General;
using Microsoft.SqlServer.Server;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = System.Random;

namespace EpicLoot.Adventure.Feature
{
    public enum AdventureFeatureType
    {
        None,
        SecretStash,
        Gamble,
        TreasureMaps,
        Bounties
    }

    public abstract class AdventureFeature
    {
        public abstract AdventureFeatureType Type { get; }
        public abstract int RefreshInterval { get; }

        public int GetSecondsUntilRefresh()
        {
            return GetSecondsUntilIntervalRefresh(RefreshInterval);
        }

        public int GetCurrentInterval()
        {
            return GetCurrentInterval(RefreshInterval);
        }

        public Random GetRandom()
        {
            return GetRandomForInterval(GetCurrentInterval(), RefreshInterval);
        }

        public virtual void OnZNetStart()
        {
        }

        public virtual void OnZNetDestroyed()
        {
        }

        public virtual void OnWorldSave()
        {
        }

        protected static int GetSecondsUntilIntervalRefresh(int intervalDays)
        {
            if (ZNet.m_world == null || EnvMan.instance == null)
            {
                return -1;
            }

            var currentDay = EnvMan.instance.GetCurrentDay();
            var startOfNextInterval = GetNextMultiple(currentDay, intervalDays);
            var daysRemaining = (startOfNextInterval - currentDay) - EnvMan.instance.m_smoothDayFraction;
            return (int)(daysRemaining * EnvMan.instance.m_dayLengthSec);
        }

        protected static int GetNextMultiple(int n, int multiple)
        {
            return ((n / multiple) + 1) * multiple;
        }

        protected static int GetCurrentInterval(int intervalDays)
        {
            var currentDay = EnvMan.instance.GetCurrentDay();
            return currentDay / intervalDays;
        }

        private static int GetSeedForInterval(int currentInterval, int intervalDays)
        {
            var worldSeed = ZNet.m_world?.m_seed ?? 0;
            var playerId = (int)(Player.m_localPlayer?.GetPlayerID() ?? 0);
            return unchecked(worldSeed + playerId + currentInterval * 1000 + intervalDays * 100);
        }

        protected static Random GetRandomForInterval(int currentInterval, int intervalDays)
        {
            return new Random(GetSeedForInterval(currentInterval, intervalDays));
        }

        public static ItemDrop CreateItemDrop(string prefabName)
        {
            var itemPrefab = ObjectDB.instance.GetItemPrefab(prefabName);
            if (itemPrefab == null)
            {
                return null;
            }

            var itemDropPrefab = itemPrefab.GetComponent<ItemDrop>();
            if (itemDropPrefab == null)
            {
                return null;
            }

            ZNetView.m_forceDisableInit = true;
            var item = Object.Instantiate(itemDropPrefab);
            ZNetView.m_forceDisableInit = false;

            return item;
        }

        public static List<SecretStashItemInfo> CollectItems(List<SecretStashItemConfig> itemList)
        {
            return CollectItems(itemList, (x) => x.Item, (x) => true);
        }

        protected static List<SecretStashItemInfo> CollectItems(
            List<SecretStashItemConfig> itemList,
            Func<SecretStashItemConfig, string> itemIdPredicate,
            Func<ItemDrop.ItemData, bool> itemOkayToAddPredicate)
        {
            var results = new List<SecretStashItemInfo>();
            foreach (var itemConfig in itemList)
            {
                var itemId = itemIdPredicate(itemConfig);
                var itemDrop = CreateItemDrop(itemId);
                if (itemDrop == null)
                {
                    EpicLoot.LogWarning($"[AdventureData] Could not find item type (gated={itemId} orig={itemConfig}) in ObjectDB!");
                    continue;
                }

                var itemData = itemDrop.m_itemData;
                if (itemOkayToAddPredicate(itemData))
                {
                    results.Add(new SecretStashItemInfo(itemId, itemData, itemConfig.GetCost()));
                }
                ZNetScene.instance.Destroy(itemDrop.gameObject);
            }

            return results;
        }

        /// <summary>
        /// Randomly select N items from the list without duplicates.
        /// </summary>
        protected static void RollOnListNTimes<T>(List<T> list, int n, List<T> results)
        {
            // Randomize a list, and take a number of entries from it
            results = (List<T>)list.shuffleList().Take(n);
        }

        protected static T RollOnList<T>(List<T> list)
        {
            // Randomize a list and take one entry
            return  (T)list.shuffleList().Take(1);
        }
    }
}
