using HarmonyLib;

namespace EpicLoot.MagicItemEffects;

public class IncreaseTreeDrop : IncreaseDrop
{

    [HarmonyPatch(typeof(TreeLog), nameof(TreeLog.Destroy))]
    public static class IncreaseTreeDrop_TreeLog_Destroy_Patch
    {
        private static void Prefix(TreeLog __instance, HitData hitData)
        {
            if (hitData != null)
            {
                TryDropExtraItems(hitData.GetAttacker(), MagicEffectType.IncreaseTreeDrop, __instance.m_dropWhenDestroyed, __instance.transform.position);
            }
        }
    }

    [HarmonyPatch(typeof(TreeBase), nameof(TreeBase.RPC_Damage))]
    public static class IncreaseTreeDrop_TreeBase_RPC_Damage_Patch
    {
        private static void Postfix(TreeBase __instance, HitData hit)
        {
            if (hit != null && __instance.m_nview == null && !__instance.gameObject.activeSelf)
            {
                TryDropExtraItems(hit.GetAttacker(), MagicEffectType.IncreaseTreeDrop, __instance.m_dropWhenDestroyed, __instance.transform.position);
            }
        }
    }

    [HarmonyPatch(typeof(Destructible), nameof(Destructible.Destroy))]
    public static class IncreaseTreeDrop_Destructible_Destroy_Patch
    {
        private static void Prefix(Destructible __instance, HitData hit)
        {
            if (hit != null && __instance.GetDestructibleType() == DestructibleType.Tree)
            {
                var dropList = __instance.gameObject.GetComponent<DropOnDestroyed>();
                if (dropList == null)
                {
                    return;
                }

                TryDropExtraItems(hit.GetAttacker(), MagicEffectType.IncreaseTreeDrop, dropList.m_dropWhenDestroyed, __instance.transform.position);
            }
        }
    }
}
