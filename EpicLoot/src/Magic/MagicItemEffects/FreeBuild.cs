using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using JetBrains.Annotations;

namespace EpicLoot.MagicItemEffects
{
    [HarmonyPatch]
    public class FreeBuildGuiDisplay_Recipe_GetRequiredStation_Patch
    {
        [UsedImplicitly]
        private static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.DeclaredMethod(typeof(Player), nameof(Player.HaveRequirements), new[] {typeof(Piece), typeof(Player.RequirementMode)});
            yield return AccessTools.DeclaredMethod(typeof(Player), nameof(Player.CheckCanRemovePiece));
            yield return AccessTools.DeclaredMethod(typeof(Hud), nameof(Hud.SetupPieceInfo));
        }

        [UsedImplicitly]
        private static void Prefix(ref CraftingStation __state, Piece piece)
        {
            if (piece == null || Player.m_localPlayer == null)
            {
                return;
            }

            __state = piece.m_craftingStation;
            if (Player.m_localPlayer.HasActiveMagicEffect(MagicEffectType.FreeBuild))
            {
                switch (piece.m_craftingStation.m_name)
                {
                    // Building with iron or stone requires having defeated the elder
                    case "piece_stonecutter":
                    case "forge":
                        if (ZoneSystem.instance.GetGlobalKey("defeated_gdking"))
                        { piece.m_craftingStation = null; }
                        break;
                    // building with the artisan table requires defeating moder
                    case "piece_artisanstation":
                        if (ZoneSystem.instance.GetGlobalKey("defeated_dragon"))
                        { piece.m_craftingStation = null; }
                        break;
                    // building with the blackforge or galdur table requires defeating yag
                    case "blackforge":
                    case "piece_magetable":
                        if (ZoneSystem.instance.GetGlobalKey("defeated_goblinking"))
                        { piece.m_craftingStation = null; }
                        break;
                    // if we hit default, we've checked everything we know from vanilla, this will ensure the no-cost is applied to mod piece table building requirements
                    // we could instead start with a check for the workbench and only support vanilla crafting structures- but what fun is that?
                    default:
                        piece.m_craftingStation = null;
                        break;
                }
            }
        }

        [UsedImplicitly]
        private static void Postfix(CraftingStation __state, Piece piece)
        {
            if (piece != null && Player.m_localPlayer != null)
            {
                piece.m_craftingStation = __state;
            }
        }
    }
}