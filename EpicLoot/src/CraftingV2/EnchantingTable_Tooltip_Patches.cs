using EpicLoot_UnityLib;
using HarmonyLib;

namespace EpicLoot.CraftingV2;

public static class EnchantingTable_Tooltip_Patches
{
    [HarmonyPatch(typeof(MultiSelectItemListElement), nameof(MultiSelectItemListElement.Awake))]
    private static class MultiSelectItemListElement_Awake
    {
        private static void Postfix(MultiSelectItemListElement __instance)
        {
            if (__instance.Tooltip == null)
            {
                // only add to elements that do not already have tooltips handled by randy's code
                // which are the tabs, item resources and adventure panel items
                UITooltip tooltip = __instance.gameObject.AddComponent<UITooltip>();
                tooltip.m_tooltipPrefab = StoreGui.instance.m_listElement.GetComponent<UITooltip>().m_tooltipPrefab;
            }
        }
    }
    
    [HarmonyPatch(typeof(MultiSelectItemListElement), nameof(MultiSelectItemListElement.SetItem))]
    private static class MultiSelectItemListElement_SetItem
    {
        private static void Postfix(MultiSelectItemListElement __instance, IListElement item)
        {
            UITooltip uiTooltip = __instance.gameObject.GetComponent<UITooltip>();
            if (uiTooltip == null)
            {
                return;
            }

            if (item != null && item.GetItem() is {} itemData && EpicLoot.IsAllowedMagicItemType(itemData.m_shared.m_itemType))
            {
                // only add tooltip to items that are enchantable
                // this prevents materials in Convert Materials tab to have tooltip displayed
                uiTooltip.Set(itemData.GetDisplayName(), itemData.GetTooltip());
            }
            else
            {
                uiTooltip.Set("", "");

            }
        }
    }
    
}