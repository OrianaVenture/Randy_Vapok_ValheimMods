using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using JetBrains.Annotations;

namespace EpicLoot.MagicItemEffects
{
    [HarmonyPatch]
    public static class FreeBuildGuiDisplay_Recipe_GetRequiredStation_Patch
    {
        [UsedImplicitly]
        private static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.DeclaredMethod(typeof(Player), nameof(Player.HaveRequirements), new[] {typeof(Piece), typeof(Player.RequirementMode)});
            yield return AccessTools.DeclaredMethod(typeof(Player), nameof(Player.CheckCanRemovePiece));
            yield return AccessTools.DeclaredMethod(typeof(Hud), nameof(Hud.SetupPieceInfo));
        }

        private static Dictionary<string, Boolean> FreeBuildablePieces = new Dictionary<string, bool> { };
        private static int CurrentGlobalKeyset = 0;
        private static bool CurrentlyBossGated = true;

        [UsedImplicitly]
        private static void Prefix(ref CraftingStation __state, Piece piece)
        {
            if (piece == null || Player.m_localPlayer == null || ZoneSystem.instance == null)
            {
                return;
            }

            // We always check to see if the config for boss gating has changed or if the bosses killed have changed
            // This ensures after killing a boss or the config being updated that we rebuild the dictionary of what is freebuildable
            if (CurrentGlobalKeyset != ZoneSystem.instance.m_globalKeysEnums.Count || CurrentlyBossGated != EpicLoot.EnableFreebuildBossGating.Value)
            {
                FreeBuildablePieces.Clear();
                CurrentGlobalKeyset = ZoneSystem.instance.m_globalKeysEnums.Count;
                CurrentlyBossGated = EpicLoot.EnableFreebuildBossGating.Value;
            }

            __state = piece.m_craftingStation;

            if (piece.m_craftingStation != null && piece.m_craftingStation.name != null && Player.m_localPlayer.HasActiveMagicEffect(MagicEffectType.FreeBuild))
            {
                //EpicLoot.Log($"Piece does require a crafting station, and player has freebuild.");
                if (FreeBuildablePieces.ContainsKey(piece.m_name)) 
                {
                    // EpicLoot.Log($"Checking diectionary for freebuild status");
                    if (FreeBuildablePieces[piece.m_name] == true) { piece.m_craftingStation = null;  }
                } else
                {
                    bool pieceFreebuild = CanBeFreeBuilt(piece.m_craftingStation.name);
                    FreeBuildablePieces.Add(piece.m_name, pieceFreebuild);
                    // EpicLoot.Log($"Dictionary entry not present, checking freebuild and adding entry {piece.m_name}-{pieceFreebuild}");
                    if (pieceFreebuild) { piece.m_craftingStation = null; }
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

        private static bool CanBeFreeBuilt(string piece_name)
        {
            if (EpicLoot.EnableFreebuildBossGating.Value)
            {
                switch (piece_name)
                {
                    // Building with iron or stone requires having defeated the elder
                    case "piece_stonecutter":
                    case "forge":
                        if (ZoneSystem.instance.GetGlobalKey("defeated_gdking"))
                        {
                            EpicLoot.Log($"Stonecutter & forge check for gdking {ZoneSystem.instance.GetGlobalKey("defeated_gdking")}");
                            return true; 
                        }
                        break;
                    // building with the artisan table requires defeating moder
                    case "piece_artisanstation":
                        if (ZoneSystem.instance.GetGlobalKey("defeated_dragon") == true)
                        {
                            EpicLoot.Log($"artisan station check for dragon {ZoneSystem.instance.GetGlobalKey("defeated_dragon")}");
                            return true;
                        }
                        break;
                    // building with the blackforge or galdur table requires defeating yag
                    case "blackforge":
                    case "piece_magetable":
                        if (ZoneSystem.instance.GetGlobalKey("defeated_goblinking"))
                        {
                            EpicLoot.Log($"blackforge & magetable check for goblinking {ZoneSystem.instance.GetGlobalKey("defeated_goblinking")}");
                            return true;
                        }
                        break;
                    // if we hit default, we've checked everything we know from vanilla, this will ensure the no-cost is applied to mod piece table building requirements
                    // we could instead start with a check for the workbench and only support vanilla crafting structures- but what fun is that?
                    default:
                        return true;
                }
                return false;
            }
            // fall through for not enabling global key gate checking of freebuild
            return true;
        }
    }
}