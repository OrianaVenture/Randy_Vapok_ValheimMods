using HarmonyLib;
using TMPro;

namespace ImprovedBuildHud;

[HarmonyPatch(typeof(Hud), nameof(Hud.SetupPieceInfo), typeof(Piece))]
public static class Hud_Patch
{
    private static void Postfix(Piece piece, TMP_Text ___m_buildSelection)
    {
        if (piece != null && !string.IsNullOrEmpty(ImprovedBuildHudConfig.CanBuildAmountFormat.Value))
        {
            if (piece.m_resources.Length == 0)
            {
                return;
            }

            var player = Player.m_localPlayer;

            var fewestPossible = int.MaxValue;
            foreach (var requirement in piece.m_resources)
            {
                var currentAmount = player.GetInventory().CountItems(requirement.m_resItem.m_itemData.m_shared.m_name);
                var canMake = currentAmount / requirement.m_amount;
                if (canMake < fewestPossible)
                {
                    fewestPossible = canMake;
                }
            }

            var canBuildDisplay = string.Format(ImprovedBuildHudConfig.CanBuildAmountFormat.Value, fewestPossible);
            if (!string.IsNullOrEmpty(ImprovedBuildHudConfig.CanBuildAmountColor.Value))
            {
                canBuildDisplay = $"<color={ImprovedBuildHudConfig.CanBuildAmountColor.Value}>{canBuildDisplay}</color>";
            }

            ___m_buildSelection.text = $"{___m_buildSelection.text} {canBuildDisplay}";
        }
    }
}
