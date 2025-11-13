using HarmonyLib;
using UnityEngine;

namespace AdvancedPortals
{
    [HarmonyPatch]
    public static class Teleport_Patch
    {
        public static AdvancedPortal CurrentAdvancedPortal;

        public static void TargetPortal_HandlePortalClick_Prefix()
        {
            Vector3 playerPos = Player.m_localPlayer.transform.position;
            const float searchRadius = 2.0f;
            Collider[] colliders = Physics.OverlapSphere(playerPos, searchRadius);
            TeleportWorld closestTeleport = null;
            float minDistSquared = searchRadius * searchRadius + 1;
            foreach (Collider collider in colliders)
            {
                TeleportWorldTrigger twt = collider.gameObject.GetComponent<TeleportWorldTrigger>();
                if (twt == null)
                {
                    continue;
                }

                TeleportWorld tw = twt.GetComponentInParent<TeleportWorld>();
                if (tw == null)
                {
                    continue;
                }

                Vector3 d = collider.transform.position - playerPos;
                float distSquared = d.x * d.x + d.y * d.y + d.z * d.z;
                if (distSquared < minDistSquared)
                {
                    closestTeleport = tw;
                    minDistSquared = distSquared;
                }
            }

            if (closestTeleport != null)
            {
                Generic_Prefix(closestTeleport);
            }
        }

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

        // High priority to run after other mods
        [HarmonyPatch(typeof(Inventory), nameof(Inventory.IsTeleportable))]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.High)]
        public static void Inventory_IsTeleportable_Pretfix(Inventory __instance, ref bool __result, ref bool __runOriginal)
        {
            if (CurrentAdvancedPortal == null)
            {
                // Do not change run original rule
                return;
            }

            if (ZoneSystem.instance.GetGlobalKey(GlobalKeys.TeleportAll) || CurrentAdvancedPortal.AllowEverything)
            {
                __result = true;
                __runOriginal = false;
                return;
            }

            foreach (ItemDrop.ItemData itemData in __instance.GetAllItems())
            {
                if (!itemData.m_shared.m_teleportable && itemData.m_dropPrefab != null &&
                    !CurrentAdvancedPortal.AllowedItems.Contains(itemData.m_dropPrefab.name))
                {
                    __result = false;
                    __runOriginal = false;
                    return;
                }
            }

            __result = true;
            __runOriginal = false;
        }
    }
}
