using System;
using HarmonyLib;
using TMPro;
using UnityEngine;

namespace ImprovedBuildHud;

[HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.SetupRequirement),
    new Type[] { typeof(Transform), typeof(Piece.Requirement), typeof(Player), typeof(bool), typeof(int), typeof(int) })]
public static class InventoryGui_SetupRequirement_Patch
{
    static void Postfix(bool __result, Transform elementRoot, Piece.Requirement req, Player player,
        bool craft, int quality, int craftMultiplier)
    {
        if (!__result)
        {
            return;
        }

        var amountText = elementRoot.transform.Find("res_amount").GetComponent<TMP_Text>();

        if (!amountText.gameObject.activeSelf)
        {
            return;
        }

        amountText.richText = true;
        amountText.overflowMode = TextOverflowModes.Overflow;

        int currentAmount = player.GetInventory().CountItems(req.m_resItem.m_itemData.m_shared.m_name);
        string inventoryAmount = string.Format(ImprovedBuildHudConfig.InventoryAmountFormat.Value, currentAmount);

        if (!string.IsNullOrEmpty(ImprovedBuildHudConfig.InventoryAmountColor.Value))
        {
            inventoryAmount = $"<color={ImprovedBuildHudConfig.InventoryAmountColor.Value}>{inventoryAmount}</color>";
        }

        amountText.text = $"{amountText.text} {inventoryAmount}";
    }
}
