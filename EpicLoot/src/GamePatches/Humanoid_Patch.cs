using HarmonyLib;

namespace EpicLoot
{
    [HarmonyPatch(typeof(Humanoid))]
    public static class Humanoid_Patch
    {
        // Handle ItemDrop.ItemData that have null m_dropPrefab values to prevent NRE in method.
        // TODO: Validate if this is needed, or can be fixed in a better way.
        [HarmonyPatch(nameof(Humanoid.SetupVisEquipment))]
        [HarmonyPrefix]
        public static void SetupVisEquipment_Prefix(Humanoid __instance, VisEquipment visEq, bool isRagdoll)
        {
            if (EpicAssets.DummyPrefab() == null)
            {
<<<<<<< HEAD
                visEq.SetLeftItem((__instance.m_leftItem != null) ? __instance.m_leftItem?.m_dropPrefab?.name : "", __instance.m_leftItem?.m_variant ?? 0);
                visEq.SetRightItem((__instance.m_rightItem != null) ? __instance.m_rightItem?.m_dropPrefab?.name : "");
                
                if (__instance.IsPlayer())
                {
                    visEq.SetLeftBackItem((__instance.m_hiddenLeftItem != null) ? __instance.m_hiddenLeftItem.m_dropPrefab?.name : "", __instance.m_hiddenLeftItem?.m_variant ?? 0);
                    visEq.SetRightBackItem((__instance.m_hiddenRightItem != null) ? __instance.m_hiddenRightItem.m_dropPrefab?.name : "");
                }
            }

            visEq.SetChestItem((__instance.m_chestItem != null) ? __instance.m_chestItem?.m_dropPrefab?.name : "");
            visEq.SetLegItem((__instance.m_legItem != null) ? __instance.m_legItem?.m_dropPrefab?.name : "");
            visEq.SetHelmetItem((__instance.m_helmetItem != null) ? __instance.m_helmetItem?.m_dropPrefab?.name : "");
            visEq.SetShoulderItem((__instance.m_shoulderItem != null) ? __instance.m_shoulderItem?.m_dropPrefab?.name : "", __instance.m_shoulderItem?.m_variant ?? 0);
            visEq.SetUtilityItem((__instance.m_utilityItem != null) ? __instance.m_utilityItem?.m_dropPrefab?.name : "");
            visEq.SetTrinketItem((__instance.m_trinketItem != null) ? __instance.m_trinketItem?.m_dropPrefab?.name : "");
            
            if (__instance.IsPlayer())
            {
                visEq.SetBeardItem(__instance.m_beardItem);
                visEq.SetHairItem(__instance.m_hairItem);
            }
=======
                EpicLoot.LogWarning("Unable to find empty object, may cause unexpected errors for Humanoid.SetupVisEquipment method.");
                return;
            }

            AssignEmptyToNull(ref __instance.m_leftItem);
            AssignEmptyToNull(ref __instance.m_rightItem);
            AssignEmptyToNull(ref __instance.m_hiddenLeftItem);
            AssignEmptyToNull(ref __instance.m_hiddenRightItem);
            AssignEmptyToNull(ref __instance.m_chestItem);
            AssignEmptyToNull(ref __instance.m_legItem);
            AssignEmptyToNull(ref __instance.m_helmetItem);
            AssignEmptyToNull(ref __instance.m_shoulderItem);
            AssignEmptyToNull(ref __instance.m_utilityItem);
            AssignEmptyToNull(ref __instance.m_trinketItem);
        }
>>>>>>> main

        private static void AssignEmptyToNull(ref ItemDrop.ItemData data)
        {
            if (data != null && data.m_dropPrefab == null)
            {
                data.m_dropPrefab = EpicAssets.DummyPrefab();
            }
        }
    }
}
