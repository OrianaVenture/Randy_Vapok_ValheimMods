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

            //Object.Instantiate(UITooltip.m_tooltip, tooltip.transform.GetComponentInParent<Canvas>().transform);

            // InventoryGrid.CreateItemTooltip
            //UITooltip.m_tooltip.transform.GetChild(0).gameObject.transform
            //Vector3[] array = new Vector3[4];
            //transform.GetWorldCorners(array);
            //float ypos = 0;
            ////    // Four corners returns 'vectors' (x,y) in this order
            ////    //fourCornersArray[0] = new Vector3(x, y, 0f); // bottom left
            ////    //fourCornersArray[1] = new Vector3(x, yMax, 0f); // top left
            ////    //fourCornersArray[2] = new Vector3(xMax, yMax, 0f); // top right
            ////    //fourCornersArray[3] = new Vector3(xMax, y, 0f); // bottom right
            //// If this is larger than the screen we need to set its top position to the top of the screen
            //if (((array[0].y * -1f) + array[1].y) > (float)Screen.height)
            //{
            //    // the Y tooltip height is past the max, so we set it to the top of the screen
            //    ypos = (float)Screen.height;

            //    // Update the y position of the tooltip
            //    Vector3 position = transform.position;
            //    position.x = transform.position.x;
            //    position.y += ypos;
            //    transform.position = position;
            //}
        }
    }
}
