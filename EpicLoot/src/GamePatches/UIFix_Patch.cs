using HarmonyLib;
using System;
using UnityEngine;

namespace EpicLoot.src.GamePatches
{
    [HarmonyPatch]
    public static class PatchOnHoverFix
    {
        [HarmonyPatch(typeof(Utils), nameof(Utils.ClampUIToScreen))]
        [HarmonyPrefix]
        public static void Postfix(RectTransform transform)
        {
            Vector3[] array = new Vector3[4];
            transform.GetWorldCorners(array);
            float ypos = 0;
            //    // Four corners returns 'vectors' (x,y) in this order
            //    //fourCornersArray[0] = new Vector3(x, y, 0f); // bottom left
            //    //fourCornersArray[1] = new Vector3(x, yMax, 0f); // top left
            //    //fourCornersArray[2] = new Vector3(xMax, yMax, 0f); // top right
            //    //fourCornersArray[3] = new Vector3(xMax, y, 0f); // bottom right
            // If this is larger than the screen we need to set its top position to the top of the screen
            if (((array[0].y * -1f) + array[1].y) > (float)Screen.height)
            {
                // the Y tooltip height is past the max, so we set it to the top of the screen
                ypos = (float)Screen.height;

                // Update the y position of the tooltip
                Vector3 position = transform.position;
                position.x = transform.position.x;
                position.y += ypos;
                transform.position = position;
            }
        }
    }
}
