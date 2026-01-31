namespace EpicLoot.Crafting;

using HarmonyLib;

[HarmonyPatch(typeof(Player), nameof(Player.AddKnownItem))]
static class Player_AddKnownItem_Patch
{
    static bool Prefix(ItemDrop.ItemData item)
    {
        if (item.IsMagicCraftingMaterial())
        {
            int variant = EpicLoot.GetRarityIconIndex(item.GetCraftingMaterialRarity());
            item.m_variant = variant;
        }

        return true;
    }
}
