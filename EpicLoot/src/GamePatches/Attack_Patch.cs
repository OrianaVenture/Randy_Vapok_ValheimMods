namespace EpicLoot;

using HarmonyLib;

[HarmonyPatch]
static class Attack_Patch
{
    // TODO: Move this to a manager/controller/utility as this is referenced in multiple patches and mod logic.
    public static Attack ActiveAttack = null;

    [HarmonyPatch(typeof(Attack), nameof(Attack.DoMeleeAttack))]
    [HarmonyPrefix]
    [HarmonyPriority(Priority.Last)]
    static void Attack_DoMeleeAttack_Prefix(Attack __instance)
    {
        ActiveAttack = __instance;
    }

    [HarmonyPatch(typeof(Attack), nameof(Attack.DoMeleeAttack))]
    [HarmonyPostfix]
    static void Attack_DoMeleeAttack_Postfix()
    {
        ActiveAttack = null;
    }
}
