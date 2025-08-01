using HarmonyLib;
using UnityEngine;

namespace EpicLoot.MagicItemEffects;

public class IncreaseMiningDrop : IncreaseDrop
{

    [HarmonyPatch(typeof(MineRock), nameof(MineRock.RPC_Hit))]
    public static class IncreaseMiningDrop_MineRock_RPC_Hit_Patch
    {
        private static void Postfix(MineRock __instance, HitData hit)
        {
            if (hit != null && __instance.m_nview == null)
            {
                TryDropExtraItems(hit.GetAttacker(), MagicEffectType.IncreaseMiningDrop, __instance.m_dropItems, __instance.transform.position);
            }
        }
    }

    [HarmonyPatch(typeof(MineRock5), nameof(MineRock5.DamageArea))]
    public static class IncreaseMiningDrop_MineRock5_RPC_Hit_Patch
    {
        private static void Postfix(MineRock5 __instance, HitData hit, int hitAreaIndex, ref bool __result)
        {
            if (hit != null && __result)
            {
                var hitArea = __instance.GetHitArea(hitAreaIndex);
                Vector3 position = (__instance.m_hitEffectAreaCenter && hitArea.m_collider != null) ?
                    hitArea.m_collider.bounds.center : hit.m_point;
                TryDropExtraItems(hit.GetAttacker(), MagicEffectType.IncreaseMiningDrop, __instance.m_dropItems, position);
            }
        }
    }

    [HarmonyPatch(typeof(Destructible), nameof(Destructible.Destroy))]
    public static class IncreaseMiningDrop_Destructible_Destroy_Patch {
        private static void Prefix(Destructible __instance, HitData hit) {
            if (hit != null && __instance.GetDestructibleType() == DestructibleType.Default &&
                __instance.m_damages.m_chop == HitData.DamageModifier.Immune &&
                __instance.m_damages.m_pickaxe != HitData.DamageModifier.Immune)
            {
                var dropList = __instance.gameObject.GetComponent<DropOnDestroyed>();
                if (dropList == null)
                {
                    return;
                }

                TryDropExtraItems(hit.GetAttacker(), MagicEffectType.IncreaseMiningDrop, dropList.m_dropWhenDestroyed, __instance.transform.position);
            }
        }
    }
}