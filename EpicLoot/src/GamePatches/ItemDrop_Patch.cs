namespace EpicLoot;

using global::EpicLoot.Crafting;
using global::EpicLoot.LootBeams;
using HarmonyLib;

[HarmonyPatch(typeof(ItemDrop), nameof(ItemDrop.Awake))]
static class ItemDrop_Awake_Patch
{
    static void Postfix(ItemDrop __instance)
    {
        if (__instance.m_itemData == null)
        {
            return;
        }

        __instance.m_itemData.InitializeCustomData();

        if (!__instance.TryGetComponent(out LootBeam _))
        {
            __instance.gameObject.AddComponent<LootBeam>();
        }
    }
}

[HarmonyPatch(typeof(Inventory), nameof(Inventory.Load))]
static class Inventory_Load_Patch
{
    static void Postfix(Inventory __instance)
    {
        foreach (ItemDrop.ItemData itemData in __instance.m_inventory)
        {
            if (itemData.IsMagicCraftingMaterial())
            {
                itemData.CreateMagicItem();
            }

            itemData.InitializeCustomData();
        }
    }
}

[HarmonyPatch(typeof(Container), nameof(Container.Load))]
static class Container_Load_Patch
{
    static void Postfix(Container __instance)
    {
        foreach (ItemDrop.ItemData itemData in __instance.m_inventory.m_inventory)
        {
            if (itemData.IsMagicCraftingMaterial())
            {
                itemData.CreateMagicItem();
            }

            itemData.InitializeCustomData();
        }
    }
}
