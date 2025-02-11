using HarmonyLib;
using TMPro;
using UnityEngine;

namespace EpicLoot.src.GamePatches
{
    [HarmonyPatch]
    public static class PatchOnHoverFix
    {

        [HarmonyPatch(typeof(InventoryGrid), nameof(InventoryGrid.CreateItemTooltip))]
        [HarmonyPrefix]
        public static void Postfix(UITooltip __instance, UITooltip tooltip)
        {
            RectTransform tooltip_box = (RectTransform)UITooltip.m_tooltip.transform.GetChild(0).transform;
            Vector3[] array = new Vector3[4];
            tooltip_box.GetWorldCorners(array);

            // If the tooltip is larger than the screen is the only time we actually care about resizing it.
            // Next stage would be optimizing space used in the tooltip itself and/or increasing the size further here
            // OR splitting out the text value at a relative newline and building a new column for it
            // another useful potential would be moving the tooltip comparision from below to side-by-side
            if (((array[0].y * -1f) + array[1].y) > (float)Screen.height)
            {
                RectTransform bkground_transform = UITooltip.m_tooltip.transform.Find("Bkg").GetComponent<RectTransform>();
                bkground_transform.sizeDelta = new Vector2(x: 510, y: bkground_transform.sizeDelta.y);
                RectTransform topic_transform = UITooltip.m_tooltip.transform.Find("Bkg/Topic").GetComponent<RectTransform>();
                topic_transform.sizeDelta = new Vector2(x: 490, y: topic_transform.sizeDelta.y);
                GameObject text_go = UITooltip.m_tooltip.transform.Find("Bkg/Text").gameObject;
                RectTransform text_transform = text_go.GetComponent<RectTransform>();
                text_transform.sizeDelta = new Vector2(x: 490, y: text_transform.sizeDelta.y);
                TextMeshProUGUI text_g = text_go.GetComponent<TextMeshProUGUI>();
                text_g.fontSize = 14;
                text_g.fontSizeMax = 15;
                text_g.fontSizeMin = 12;
            }
        }
    }
}
