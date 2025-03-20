using HarmonyLib;
using TMPro;
using UnityEngine;

namespace EpicLoot.src.GamePatches
{
    [HarmonyPatch]
    public static class PatchOnHoverFix
    {
        public static string comparision_title = "";
        public static string comparision_tooltip = "";
        public static bool comparision_added = false;

        [HarmonyPatch(typeof(InventoryGrid), nameof(InventoryGrid.CreateItemTooltip))]
        [HarmonyPostfix]
        public static void Postfix(UITooltip __instance, UITooltip tooltip)
        {
            if (UITooltip.m_tooltip == null) { return; }
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

            // Render the comparision tooltip next to our primary tooltip, regardless of size of the original tooltip
            if (comparision_tooltip != "" && comparision_added != true) {
                GameObject org_tt = UITooltip.m_tooltip.transform.Find("Bkg").gameObject;
                RectTransform org_bktransform = UITooltip.m_tooltip.transform.Find("Bkg").GetComponent<RectTransform>();
                GameObject new_tt = UnityEngine.Object.Instantiate(org_tt, UITooltip.m_tooltip.transform);
                // Test and determine why the topic is not being set properly
                // GameObject new_tt = UnityEngine.Object.Instantiate(org_tt);
                new_tt.transform.position = new Vector3(org_tt.transform.position.x + 5 + (org_bktransform.sizeDelta.x * 1.35f), org_tt.transform.position.y, org_tt.transform.position.z);
                
                GameObject text_go = new_tt.transform.Find("Text").gameObject;
                TextMeshProUGUI text_g = text_go.GetComponent<TextMeshProUGUI>();
                text_g.text = Localization.instance.Localize(comparision_tooltip);

                GameObject title_go = new_tt.transform.Find("Topic").gameObject;
                TextMeshProUGUI title_g = title_go.GetComponent<TextMeshProUGUI>();
                title_g.text = Localization.instance.Localize(comparision_title);
                comparision_added = true;
            }
        }
    }
}
