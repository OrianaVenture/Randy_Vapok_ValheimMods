﻿using HarmonyLib;

namespace AdvancedPortals
{
    [HarmonyPatch]
    public static class Teleport_Patch
    {
        public static AdvancedPortal CurrentAdvancedPortal;

        public static void Generic_Prefix(TeleportWorld __instance)
        {
            CurrentAdvancedPortal = __instance.GetComponent<AdvancedPortal>();
        }

        public static void Generic_Postfix()
        {
            CurrentAdvancedPortal = null;
        }

        [HarmonyPatch(typeof(TeleportWorld), nameof(TeleportWorld.UpdatePortal))]
        [HarmonyPrefix]
        public static void TeleportWorld_UpdatePortal_Prefix(TeleportWorld __instance)
        {
            CurrentAdvancedPortal = __instance.GetComponent<AdvancedPortal>();
        }

        [HarmonyPatch(typeof(TeleportWorld), nameof(TeleportWorld.SetText))]
        [HarmonyPostfix]
        public static void TeleportWorld_SetText_Postfix()
        {
            Game.instance.ConnectPortals();
        }

        
        [HarmonyPatch(typeof(TeleportWorld), nameof(TeleportWorld.UpdatePortal))]
        [HarmonyPostfix]
        public static void TeleportWorld_UpdatePortal_Postfix()
        {
            CurrentAdvancedPortal = null;
        }

        [HarmonyPatch(typeof(TeleportWorld), nameof(TeleportWorld.Teleport))]
        [HarmonyPrefix]
        public static void TeleportWorld_Teleport_Prefix(TeleportWorld __instance)
        {
            CurrentAdvancedPortal = __instance.GetComponent<AdvancedPortal>();
        }

        [HarmonyPatch(typeof(TeleportWorld), nameof(TeleportWorld.Teleport))]
        [HarmonyPostfix]
        public static void TeleportWorld_Teleport_Postfix()
        {
            CurrentAdvancedPortal = null;
        }

        [HarmonyPatch(typeof(Inventory), nameof(Inventory.IsTeleportable))]
        [HarmonyPrefix]
        public static bool Inventory_IsTeleportable_Pretfix(Inventory __instance, ref bool __result)
        {
            if (CurrentAdvancedPortal == null)
                return true;

            if (CurrentAdvancedPortal.AllowEverything)
            {
                __result = true;
                return false;
            }

            foreach (var itemData in __instance.GetAllItems())
            {
                if (!itemData.m_shared.m_teleportable && itemData.m_dropPrefab != null && !CurrentAdvancedPortal.AllowedItems.Contains(itemData.m_dropPrefab.name))
                {
                    __result = false;
                    return false;
                }
            }

            __result = true;
            return false;
        }
    }
}
