using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;

namespace AdvancedPortals;

[HarmonyPatch]
public static class Teleport_Patch
{
    [HarmonyPatch(typeof(TeleportWorld), nameof(TeleportWorld.Teleport))]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> TeleportWorld_Teleport_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
            .MatchForward(
                useEnd: false,
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(TeleportWorld), nameof(TeleportWorld.m_allowAllItems))))
            .Advance(offset: 1)
            .SetInstructionAndAdvance(Transpilers.EmitDelegate<Func<TeleportWorld, bool>>(CanTeleport))
            .InstructionEnumeration();
    }

    [HarmonyPatch(typeof(TeleportWorld), nameof(TeleportWorld.UpdatePortal))]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> TeleportWorld_UpdatePortal_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
            .MatchForward(
                useEnd: false,
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(TeleportWorld), nameof(TeleportWorld.m_allowAllItems))))
            .Advance(offset: 1)
            .SetInstructionAndAdvance(Transpilers.EmitDelegate<Func<TeleportWorld, bool>>(CanTeleport))
            .InstructionEnumeration();
    }

    // Fixup for Target Portal
    [HarmonyPrefix]
    [HarmonyPatch(typeof(TeleportWorldTrigger), nameof(TeleportWorldTrigger.OnTriggerEnter))]
    public static void TeleportWorldTrigger_OnTriggerEnter_Prefix(TeleportWorldTrigger __instance, out bool __state)
    {
        __state = __instance.m_teleportWorld.m_allowAllItems;

        if (!AdvancedPortals.TargetPortalInstalled)
        {
            return;
        }

        if (__instance.m_teleportWorld is AdvancedPortal)
        {
            __instance.m_teleportWorld.m_allowAllItems = CanTeleport(__instance.m_teleportWorld);
        }
    }

    // Fixup for Target Portal
    [HarmonyPostfix]
    [HarmonyPatch(typeof(TeleportWorldTrigger), nameof(TeleportWorldTrigger.OnTriggerEnter))]
    public static void TeleportWorldTrigger_OnTriggerEnter_Postfix(TeleportWorldTrigger __instance, bool __state)
    {
        if (!AdvancedPortals.TargetPortalInstalled)
        {
            return;
        }

        __instance.m_teleportWorld.m_allowAllItems = __state;
    }

    static bool CanTeleport(TeleportWorld portal)
    {
        if (portal.m_allowAllItems)
        {
            return true;
        }

        if (portal is not AdvancedPortal)
        {
            return false;
        }

        var inventory = Player.m_localPlayer.m_inventory;

        string portalName = Utils.GetPrefabName(portal.gameObject.name);
        foreach (var itemData in inventory.GetAllItems())
        {
            if (!itemData.m_shared.m_teleportable &&
                (itemData.m_dropPrefab != null &&
                !(AdvancedPortal.AllowedItem(portalName, itemData.m_dropPrefab.name))))
            {
                Debug.Log($"{itemData.m_dropPrefab.name} Not allowed!");
                return false;
            }
        }

        return true;
    }
}
