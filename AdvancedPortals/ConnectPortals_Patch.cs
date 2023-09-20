using System.Collections.Generic;
using HarmonyLib;

namespace AdvancedPortals
{
    [HarmonyPatch]
    public static class ConnectPortals_Patch
    {
        public static readonly string[] _portalPrefabs = { "portal_ancient", "portal_obsidian", "portal_blackmarble" };
        
        /// <summary>
        /// Add custom prefabs to the portals list to be recognized throughout the game.
        /// </summary>
        [HarmonyPatch(typeof(Game), nameof(Game.Awake))]
        public static class Game_Awake_Patch
        {
            public static void Prefix(Game __instance)
            {
                if (__instance.PortalPrefabHash == null)
                {
                    __instance.PortalPrefabHash = new List<int>();
                }

                foreach (var portal in _portalPrefabs)
                {
                    __instance.PortalPrefabHash.Add(portal.GetStableHashCode());
                }
            }
        }
    }
}
