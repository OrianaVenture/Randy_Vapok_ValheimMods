using EpicLoot.Crafting;
using EpicLoot.Data;
using EpicLoot.LootBeams;
using HarmonyLib;
using UnityEngine;

namespace EpicLoot
{
    [HarmonyPatch(typeof(ItemDrop), nameof(ItemDrop.Load))]
    public static class ItemDrop_Load_Patch
    {
        // This patch is critical to load the custom data which powers EL magic items
        // Previously this was accomplished by ensuring that any ways an item could be created would try loading the data
        // This changes that logic by running a check if the item is magic when it is loaded, it also avoids excessive ZDO saves when no changes are made
        public static void Postfix(ItemDrop __instance)
        {
            if (__instance.m_itemData == null)
            {
                return;
            }

            MagicItemComponent magicItem = __instance.m_itemData.Data().Get<MagicItemComponent>();
            if (magicItem != null)
            {
                __instance.m_itemData = magicItem.Item;
                magicItem.Deserialize();
                __instance.m_itemData.SaveMagicItem(magicItem.MagicItem);
                __instance.Save();
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
                // This is to ensure that magicmaterials that were created without their itemdata has matching custom data, this creates an empty saved magicitem componet
                // But only if it does not already have the requesite entry
                if (itemData.IsMagicCraftingMaterial() && itemData.m_customData.Count == 0) {
                    itemData.m_customData.Add("randyknapp.mods.epicloot#EpicLoot.MagicItemComponent", "");
                }
                if (prefabData != null)
                {
                    itemData.m_dropPrefab = prefabData;
                }
            }
        }
    }
}