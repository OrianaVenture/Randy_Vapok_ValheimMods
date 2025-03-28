using HarmonyLib;

namespace EpicLoot.MagicItemEffects
{
    [HarmonyPatch]
    public static class EitrWeaving
    {
        [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.BlockAttack))]
        public static class PatchParry_EitrWeave
        {
            public static void Postfix(Humanoid __instance, Character attacker)
            {
                // EpicLoot.Log($"Checking for eitr weave block");
                if (__instance is Player player && player == Player.m_localPlayer) {
                    var eitrWeaveValue = player.GetTotalActiveMagicEffectValue(MagicEffectType.EitrWeave);
                    if (eitrWeaveValue > 0)
                    {
                        ItemDrop.ItemData currentBlocker = __instance.GetCurrentBlocker();
                        bool parried_attack = currentBlocker.m_shared.m_timedBlockBonus > 1f && __instance.m_blockTimer != -1f && __instance.m_blockTimer < 0.25f;
                        // EpicLoot.Log($"Checking if block was a parry {parried_attack} {currentBlocker.m_shared.m_timedBlockBonus > 1f} {__instance.m_blockTimer != -1f && __instance.m_blockTimer < 0.25f}");
                        if (parried_attack == false)
                        {
                            return;
                        }
                        player.AddEitr(eitrWeaveValue);
                    }
                }
            }
        }
    }
}
