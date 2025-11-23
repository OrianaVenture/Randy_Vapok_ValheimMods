using EpicLoot.LootBeams;
using HarmonyLib;
using UnityEngine;

namespace EpicLoot
{
    [HarmonyPatch(typeof(ItemDrop), nameof(ItemDrop.Awake))]
    public static class ItemDrop_Awake_Patch
    {
        public static void Postfix(ItemDrop __instance)
        {
            if (__instance.m_itemData == null)
            {
                return;
            }

            GameObject prefabData = __instance.m_itemData.InitializeCustomData();

            if (prefabData != null)
            {
                __instance.m_itemData.m_dropPrefab = prefabData;
            }

            if (__instance.gameObject.GetComponent<LootBeam>() == null)
            {
                __instance.gameObject.AddComponent<LootBeam>();
            }
        }
    }

    [HarmonyPatch(typeof(Inventory), nameof(Inventory.Load))]
    public static class Inventory_Load_Patch
    {
        public static void Postfix(Inventory __instance)
        {
            foreach (ItemDrop.ItemData itemData in __instance.m_inventory)
            {
                GameObject prefabData = itemData.InitializeCustomData();

                if (prefabData != null)
                {
                    itemData.m_dropPrefab = prefabData;
                }
            }
        }
    }
}