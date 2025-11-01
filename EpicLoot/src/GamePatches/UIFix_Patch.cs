using HarmonyLib;
using Jotunn.Managers;
<<<<<<< HEAD
using System;
=======
>>>>>>> main
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

namespace EpicLoot
{
    [HarmonyPatch]
    public static class PatchOnHoverFix
    {
<<<<<<< HEAD
        public static string comparision_title = "";
        public static string comparision_tooltip = "";
        public static bool comparision_added = false;
        public static GameObject comparisionTT = null;

        [HarmonyPatch(typeof(UITooltip), nameof(UITooltip.OnPointerExit))]
        public static class PointerExitDestroyComparision
=======
        public static string ComparisonTitleString = "";
        public static string ComparisonTooltipString = "";
        public static bool ComparisonAdded = false;
        public static GameObject ComparisonTT = null;

        [HarmonyPatch(typeof(UITooltip), nameof(UITooltip.OnPointerExit))]
        public static class PointerExitDestroyComparison
>>>>>>> main
        {
            static bool Prefix()
            {
                return false;
            }
        }

        [HarmonyPatch(typeof(InventoryGrid), nameof(InventoryGrid.CreateItemTooltip))]
        [HarmonyPostfix]
<<<<<<< HEAD
        public static void AddComparisionTooltip()
        {
            if (UITooltip.m_tooltip == null) {
                return;
            }
            // Reset comparision tooltip if it was previously added
            if (comparisionTT != null && comparision_tooltip == "")
            {
                GameObject.Destroy(comparisionTT);
                comparisionTT = null;
                comparision_added = false;
            }

            // Build a comparision tooltip if we are requested to show one
            if (comparision_tooltip != "" && comparision_added != true) {
                // Ensure the old tooltip is removed
                if (comparisionTT != null)
                {
                    GameObject.Destroy(comparisionTT);
                }
                comparisionTT = GameObject.Instantiate(UITooltip.m_tooltip, UITooltip.m_tooltip.transform);
                Transform scrollT = Utils.FindChild(comparisionTT.transform, "Canvas");
                RectTransform scrollRT = scrollT.GetComponent<RectTransform>();
                Transform header = Utils.FindChild(comparisionTT.transform, "Topic");
                header.GetComponent<TextMeshProUGUI>().text = Localization.instance.Localize(comparision_title);
                Transform contentt = Utils.FindChild(comparisionTT.transform, "Text");
                contentt.GetComponent<TextMeshProUGUI>().text = Localization.instance.Localize(comparision_tooltip);
                // Offset the comparision tooltip to the right of the original tooltip
                RectTransform tooltipTfm = (RectTransform)UITooltip.m_tooltip.transform;
                
                Vector3[] compareCorners = new Vector3[4];
                Vector3[] tooltipCorners = new Vector3[4];
                RectTransform TooltipTform = Utils.FindChild(UITooltip.m_tooltip.transform, "Canvas").GetComponent<RectTransform>();
                TooltipTform.GetWorldCorners(tooltipCorners);
                scrollRT.GetWorldCorners(compareCorners);
                scrollRT.anchoredPosition = new Vector2(tooltipTfm.anchoredPosition.x + 350f, tooltipTfm.anchoredPosition.y);
                // Offset calculation is needed to adjust the two canvases since they will have different heights
                float xoffset = Mathf.Abs(tooltipCorners[0].y - tooltipCorners[1].y);
                scrollRT.position = new Vector3(TooltipTform.position.x + (xoffset /2) + 5f, TooltipTform.position.y, 0);
                comparision_added = true;
=======
        public static void AddComparisonTooltip()
        {
            if (UITooltip.m_tooltip == null)
            {
                return;
            }

            // Reset comparison tooltip if it was previously added
            if (ComparisonTT != null && ComparisonTooltipString == "")
            {
                GameObject.Destroy(ComparisonTT);
                ComparisonTT = null;
                ComparisonAdded = false;
>>>>>>> main
            }

            // Build a comparison tooltip if we are requested to show one
            if (ComparisonTooltipString != "" && ComparisonAdded != true)
            {
                // Ensure the old tooltip is removed
                if (ComparisonTT != null)
                {
                    GameObject.Destroy(ComparisonTT);
                }

                ComparisonTT = GameObject.Instantiate(UITooltip.m_tooltip, UITooltip.m_tooltip.transform);

                Transform scrollT = Utils.FindChild(ComparisonTT.transform, "Canvas");
                RectTransform scrollRT = scrollT.GetComponent<RectTransform>();
                Transform header = Utils.FindChild(ComparisonTT.transform, "Topic");
                header.GetComponent<TextMeshProUGUI>().text = Localization.instance.Localize(ComparisonTitleString);
                Transform contentt = Utils.FindChild(ComparisonTT.transform, "Text");
                contentt.GetComponent<TextMeshProUGUI>().text = Localization.instance.Localize(ComparisonTooltipString);

                // Offset the comparision tooltip to the right of the original tooltip
                RectTransform tooltipTfm = (RectTransform)UITooltip.m_tooltip.transform;
                Vector3[] compareCorners = new Vector3[4];
                Vector3[] tooltipCorners = new Vector3[4];
                RectTransform tooltipTransform = Utils.FindChild(UITooltip.m_tooltip.transform, "Canvas").GetComponent<RectTransform>();
                tooltipTransform.GetWorldCorners(tooltipCorners);
                scrollRT.GetWorldCorners(compareCorners);
                scrollRT.anchoredPosition = new Vector2(tooltipTfm.anchoredPosition.x + 350f, tooltipTfm.anchoredPosition.y);

                // Offset calculation is needed to adjust the two canvases since they will have different heights
                float xoffset = Mathf.Abs(tooltipCorners[0].y - tooltipCorners[1].y);
                scrollRT.position = new Vector3(tooltipTransform.position.x + (xoffset /2) + 5f, tooltipTransform.position.y, 0);
                ComparisonAdded = true;
            }
        }

        [HarmonyPatch(typeof(UITooltip), nameof(UITooltip.OnHoverStart))]
        [HarmonyPostfix]
        public static void Postfix()
        {
            if (UITooltip.m_tooltip != null)
            {
                AddScrollbar(UITooltip.m_tooltip, 700f, 350f, 185f, -370f);
            }
        }

        public static void AddScrollbar(GameObject tooltipObject, float height, float width, float posx, float posy)
        {
            if (tooltipObject == null)
            {
                return;
            }

            Transform header = Utils.FindChild(tooltipObject.transform, "Topic");
            // No scrollbar for this thing
            if (header == null)
            {
                return;
            }

            GameObject scrollArea = GUIManager.Instance.CreateScrollView(
                tooltipObject.transform, false, true, 10f, 10f,
                GUIManager.Instance.ValheimScrollbarHandleColorBlock, Color.grey, width, height);

            // Hide the scrollbar by default, it will show when needed
            scrollArea.GetComponentInChildren<Scrollbar>().gameObject.SetActive(false);

            Transform contentt = Utils.FindChild(tooltipObject.transform, "Content");
            Transform tooltipTextTransform = Utils.FindChild(tooltipObject.transform, "Text");
            header.SetParent(contentt, false);
            tooltipTextTransform.SetParent(contentt, false);

            Transform scrolltform = Utils.FindChild(tooltipObject.transform, "Scroll View");
            // Scroll sensitivity fix for combat update
            ScrollRect scrollRect = scrolltform.GetComponent<ScrollRect>();
            scrollRect.scrollSensitivity = 800;

            // Copy the existing background from the header tooltip section to the content of the scrollview
            // Set the header section to match the width of the scroll area
            Transform bkgtform = tooltipObject.transform.Find("Bkg");
            if (bkgtform != null)
            {
                Image backgroundImage = bkgtform.GetComponent<Image>();
                Image contentbkgImage = contentt.gameObject.AddComponent<Image>();
                contentbkgImage.color = backgroundImage.color;
                contentbkgImage.sprite = backgroundImage.sprite;
                contentbkgImage.type = backgroundImage.type;
                // Remove the header background as it is no longer needed
                GameObject.Destroy(bkgtform.gameObject);
            }

            // Add a little padding to the viewport so text isn't jammed against the edge
            VerticalLayoutGroup vlg = contentt.GetComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(10, 0, 0, 0);

            // Add the scrollbar handler to allow mouse wheel scrolling while not hovering over the scrollbar
            scrolltform.gameObject.AddComponent<ScrollWheelHandler>();

            // Adjust the location of the tooltip otherwise it will be on the cursor
            RectTransform scrollRT = scrollArea.GetComponent<RectTransform>();
            scrollRT.anchorMin = new Vector2(0, 1);
            scrollRT.anchorMax = new Vector2(0, 1);
            scrollRT.anchoredPosition = new Vector2(posx, posy);
            RectTransform contentRT = contentt.GetComponent<RectTransform>();
            contentRT.offsetMax = new Vector2(360, 0); // Expand area so scrollbar isn't floating
            RectTransform textRT = tooltipTextTransform.GetComponent<RectTransform>();
            textRT.offsetMax = new Vector2(325, -22); // Expand text to get more of the full area
            header.GetComponent<RectTransform>().offsetMax = new Vector2(210, 0); // Expand title to get more of the full area
        }
    }

    /// <summary>
    /// The following class is largely taken from Azumatt's Tooltip Expansion mod:
    /// https://github.com/AzumattDev/TooltipExpansion/blob/main/CodeNShit/Monos/TooltipSizeAdjuster.cs
    /// </summary>
    public class ScrollWheelHandler : MonoBehaviour
    {
        private ScrollRect _scrollRect = null;

        public void Awake()
        {
            _scrollRect = GetComponent<ScrollRect>();
            if (_scrollRect == null)
            {
                EpicLoot.LogWarning("ScrollWheelHandler: No ScrollRect found on " + gameObject.name);
            }
            else
            {
                _scrollRect.verticalNormalizedPosition = 1f;
            }
        }

        public void Update()
        {
            // Only process if this object is active.
            if (!gameObject.activeInHierarchy || _scrollRect == null)
            {
                return;
            }

            // Get scroll wheel input regardless of pointer location.
            float scrollDelta = Input.GetAxis("Mouse ScrollWheel");
            if (!(Mathf.Abs(scrollDelta) > float.Epsilon))
            {
                return;
            }

            // Adjust the vertical scroll position.
            float newScrollPosition = _scrollRect.verticalNormalizedPosition + scrollDelta * 0.7f;
            _scrollRect.verticalNormalizedPosition = Mathf.Clamp01(newScrollPosition);
        }

        [HarmonyPatch(typeof(UITooltip), nameof(UITooltip.OnHoverStart))]
        [HarmonyPostfix]
        public static void Postfix() {
            if (UITooltip.m_tooltip != null) {
                AddScrollbar(UITooltip.m_tooltip, 700f, 350f, 185f, -370f);
            }
        }

        public static void AddScrollbar(GameObject tooltipObject, float height, float width, float posx, float posy) {
            if (tooltipObject == null) { return; }
            Transform header = Utils.FindChild(tooltipObject.transform, "Topic");
            // No scrollbar for this thing
            if (header == null) { return; }
            //EpicLoot.Log($"Setting scroll size: x:{width} y:{height}");
            GameObject scrollArea = GUIManager.Instance.CreateScrollView(tooltipObject.transform, false, true, 10f, 10f, GUIManager.Instance.ValheimScrollbarHandleColorBlock, Color.grey, width, height);

            // Hide the scrollbar by default, it will show when needed
            scrollArea.GetComponentInChildren<Scrollbar>().gameObject.SetActive(false);

            Transform contentt = Utils.FindChild(tooltipObject.transform, "Content");
            Transform tooltipTextTransform = Utils.FindChild(tooltipObject.transform, "Text");
            //EpicLoot.Log($"Found header: {header}, content: {contentt}, text: {tooltipTextTransform}");
            header.SetParent(contentt, false);
            tooltipTextTransform.SetParent(contentt, false);

            Transform scrolltform = Utils.FindChild(tooltipObject.transform, "Scroll View");
            // Scroll sensitivity fix for combat update
            ScrollRect scrollRect = scrolltform.GetComponent<ScrollRect>();
            scrollRect.scrollSensitivity = 800;

            // Copy the existing background from the header tooltip section to the content of the scrollview
            // Set the header section to match the width of the scroll area
            Transform bkgtform = tooltipObject.transform.Find("Bkg");
            if (bkgtform != null) {
                Image backgroundImage = bkgtform.GetComponent<Image>();
                Image contentbkgImage = contentt.gameObject.AddComponent<Image>();
                contentbkgImage.color = backgroundImage.color;
                contentbkgImage.sprite = backgroundImage.sprite;
                contentbkgImage.type = backgroundImage.type;
                // Remove the header background as it is no longer needed
                GameObject.Destroy(bkgtform.gameObject);
            }


            // Add a little padding to the viewport so text isn't jammed against the edge
            VerticalLayoutGroup vlg = contentt.GetComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(10, 0, 0, 0);

            // Add the scrollbar handler to allow mouse wheel scrolling while not hovering over the scrollbar
            scrolltform.gameObject.AddComponent<ScrollWheelHandler>();

            // Adjust the location of the tooltip otherwise it will be on the cursor
            RectTransform scrollRT = scrollArea.GetComponent<RectTransform>();
            scrollRT.anchorMin = new Vector2(0, 1);
            scrollRT.anchorMax = new Vector2(0, 1);
            scrollRT.anchoredPosition = new Vector2(posx, posy);
            RectTransform contentRT = contentt.GetComponent<RectTransform>();
            contentRT.offsetMax = new Vector2(360, 0); // Expand area so scrollbar isn't floating
            RectTransform textRT = tooltipTextTransform.GetComponent<RectTransform>();
            textRT.offsetMax = new Vector2(325, -22); // Expand text to get more of the full area
            header.GetComponent<RectTransform>().offsetMax = new Vector2(210, 0); // Expand title to get more of the full area
        }
    }

    /// <summary>
    /// The following class is largely taken from Azumatt's Tooltip Expansion mod:
    /// https://github.com/AzumattDev/TooltipExpansion/blob/main/CodeNShit/Monos/TooltipSizeAdjuster.cs
    /// </summary>
    public class ScrollWheelHandler : MonoBehaviour {
        private ScrollRect _scrollRect = null;

        public void Awake() {
            _scrollRect = GetComponent<ScrollRect>();
            if (_scrollRect == null) {
                EpicLoot.LogWarning("ScrollWheelHandler: No ScrollRect found on " + gameObject.name);
            } else {
                _scrollRect.verticalNormalizedPosition = 1f;
            }
        }

        public void Update()
        {
            // Only process if this object is active.
            if (!gameObject.activeInHierarchy || _scrollRect == null)
                return;

            // Get scroll wheel input regardless of pointer location.
            float scrollDelta = Input.GetAxis("Mouse ScrollWheel");
            if (!(Mathf.Abs(scrollDelta) > float.Epsilon)) return;
            // Adjust the vertical scroll position.
            float newScrollPosition = _scrollRect.verticalNormalizedPosition + scrollDelta * 0.7f;
            _scrollRect.verticalNormalizedPosition = Mathf.Clamp01(newScrollPosition);
        }
    }
}
