using HarmonyLib;

namespace EpicLoot.src.Magic.MagicItemEffects
{
    [HarmonyPatch]
    internal static class DartingThoughts
    {
        public static class ModifyEitrRegen
        {
            [HarmonyPatch(typeof(SEMan), nameof(SEMan.ModifyEitrRegen))]
            public static void Postfix(SEMan __instance, ref float regenMultiplier)
            {
                if (__instance.m_character.IsPlayer() && Player.m_localPlayer != null)
                {
                    if (Player.m_localPlayer.HasActiveMagicEffect(MagicEffectType.DartingThoughts, out float dartThoughtsValue)) {
                        regenMultiplier *= (1 + dartThoughtsValue);
                    }
                }
            }
        }

        public static class ModifyMaxEitr
        {
            [HarmonyPatch(typeof(Player), nameof(Player.GetMaxEitr))]
            public static void Postfix(Player __instance, ref float __result)
            {
                if (__instance.HasActiveMagicEffect(MagicEffectType.DartingThoughts, out float dartThoughtsValue)) {
                    __result *= (1 - (dartThoughtsValue/3));
                }
            }
        }
    }
}
