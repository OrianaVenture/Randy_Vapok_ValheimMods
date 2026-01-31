namespace EpicLoot.Crafting;

using global::EpicLoot.Data;
using global::EpicLoot.LootBeams;
using HarmonyLib;
using UnityEngine;

[HarmonyPatch(typeof(ItemDrop), nameof(ItemDrop.Awake))]
static class ItemDrop_Awake_Patch
{
    static void Postfix(ItemDrop __instance)
    {
        bool isMagic = __instance.m_itemData.IsMagicCraftingMaterial();
        bool isRunestone = __instance.m_itemData.IsRunestone();
        bool isUnidentified = __instance.m_itemData.IsUnidentified();

        if (isMagic || isRunestone || isUnidentified)
        {
            Transform particleContainer = __instance.transform.Find("Particles");

            if (particleContainer)
            {
                particleContainer.gameObject.AddComponent<AlwaysPointUp>();
            }

            ItemRarity rarity = isRunestone
                ? __instance.m_itemData.GetRunestoneRarity()
                : __instance.m_itemData.GetCraftingMaterialRarity();

            string magicColor = EpicLoot.GetRarityColor(rarity);
            int variant = isRunestone ? 0 : EpicLoot.GetRarityIconIndex(rarity);

            // Ensure unidenfitied items are loaded back up if they somehow become non-magical
            MagicItemComponent magicItem = __instance.m_itemData.Data().GetOrCreate<MagicItemComponent>();

            if (isUnidentified && magicItem.MagicItem == null)
            {
                magicItem.SetMagicItem(
                    new MagicItem
                    {
                        Rarity = rarity,
                        IsUnidentified = true,
                    });

                magicItem.Save();
            }

            if (ColorUtility.TryParseHtmlString(magicColor, out Color rgbaColor))
            {
                __instance.gameObject.AddComponent<BeamColorSetter>().SetColor(rgbaColor);
            }

            if (isUnidentified)
            {
                variant = 0;
            }

            __instance.m_itemData.m_variant = variant;
        }
    }
}

[HarmonyPatch(typeof(Inventory), nameof(Inventory.Load))]
static class Inventory_Load_Patch
{
    static void Postfix(Inventory __instance)
    {
        foreach (ItemDrop.ItemData item in __instance.m_inventory)
        {
            if (item.IsMagicCraftingMaterial())
            {
                ItemRarity rarity = item.GetCraftingMaterialRarity();
                int variant = EpicLoot.GetRarityIconIndex(rarity);
                item.m_variant = variant;
            }
        }
    }
}
