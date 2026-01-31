namespace EpicLoot;

using HarmonyLib;
using UnityEngine;

[HarmonyPatch(typeof(Humanoid))]
static class Humanoid_Patch
{
    // Handle ItemDrop.ItemData that have null m_dropPrefab values to prevent NRE in method.
    // TODO: Validate if this is needed, or can be fixed in a better way.
    [HarmonyPatch(nameof(Humanoid.SetupVisEquipment))]
    [HarmonyPrefix]
    static void SetupVisEquipment_Prefix(Humanoid __instance, VisEquipment visEq, bool isRagdoll)
    {
        GameObject dummyPrefab = EpicAssets.DummyPrefab();

        if (!dummyPrefab)
        {
            EpicLoot.LogWarning(
                "Unable to find empty object, may cause unexpected errors for Humanoid.SetupVisEquipment method.");
            return;
        }

        AssignEmptyToNull(ref __instance.m_leftItem, dummyPrefab);
        AssignEmptyToNull(ref __instance.m_rightItem, dummyPrefab);
        AssignEmptyToNull(ref __instance.m_hiddenLeftItem, dummyPrefab);
        AssignEmptyToNull(ref __instance.m_hiddenRightItem, dummyPrefab);
        AssignEmptyToNull(ref __instance.m_chestItem, dummyPrefab);
        AssignEmptyToNull(ref __instance.m_legItem, dummyPrefab);
        AssignEmptyToNull(ref __instance.m_helmetItem, dummyPrefab);
        AssignEmptyToNull(ref __instance.m_shoulderItem, dummyPrefab);
        AssignEmptyToNull(ref __instance.m_utilityItem, dummyPrefab);
        AssignEmptyToNull(ref __instance.m_trinketItem, dummyPrefab);
    }

    private static void AssignEmptyToNull(ref ItemDrop.ItemData data, GameObject dummyPrefab)
    {
        if (data != null && !data.m_dropPrefab)
        {
            data.m_dropPrefab = dummyPrefab;
        }
    }
}
