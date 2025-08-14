using EpicLoot.MagicItemEffects;
using HarmonyLib;

namespace EpicLoot.src.Magic.MagicItemEffects
{
    [HarmonyPatch]
    internal static class DartingThoughts
    {
        [HarmonyPatch(typeof(SEMan), nameof(SEMan.ModifyEitrRegen))]
        public static class ModifyEitrRegen_SEMan_DartingThoughts_Patch
        {
            public static void Postfix(SEMan __instance, ref float eitrMultiplier)
            {
                if (__instance.m_character.IsPlayer() && Player.m_localPlayer != null)
                {
                    if (Player.m_localPlayer.HasActiveMagicEffect(MagicEffectType.DartingThoughts, out float dartThoughtsValue, 0.01f)) {
                        eitrMultiplier += (1 + dartThoughtsValue);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Player), nameof(Player.GetTotalFoodValue))]
        public static class ModifyMaxEitr
        {
            public static void Postfix(Player __instance, ref float eitr)
            {
                if (__instance.HasActiveMagicEffect(MagicEffectType.DartingThoughts, out float dartThoughtsValue)) {
                    eitr *= (1 - (dartThoughtsValue / 3));
                }
            }
            
        }
    }
}
