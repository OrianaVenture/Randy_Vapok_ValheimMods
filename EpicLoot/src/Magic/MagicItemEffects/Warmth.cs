using HarmonyLib;

namespace EpicLoot.MagicItemEffects
{
    public static class Warmth
    {
        public static int AddingStatusFromEnv;

        //public void UpdateEnvStatusEffects(float dt)
        [HarmonyPatch(typeof(Player), nameof(Player.UpdateEnvStatusEffects))]
        public static class Warmth_Player_UpdateEnvStatusEffects_Patch
        {
            public static bool Prefix()
            {
                AddingStatusFromEnv++;
                return true;
            }

            public static void Postfix(Player __instance)
            {
                AddingStatusFromEnv--;
            }
        }

        [HarmonyPatch(typeof(SEMan), nameof(SEMan.AddStatusEffect), typeof(int), typeof(bool), typeof(int), typeof(float))]
        public static class Warmth_SEMan_AddStatusEffect_Patch
        {
            public static bool Prefix(SEMan __instance, int nameHash)
            {
                if (AddingStatusFromEnv > 0 && __instance.m_character.IsPlayer() && nameHash == "Freezing".GetHashCode())
                {
                    var player = (Player) __instance.m_character;
                    var hasWarmthEquipment = player.HasActiveMagicEffect(MagicEffectType.Warmth);
                    if (hasWarmthEquipment)
                    {
                        return false;
                    }
                }

                return true;
            }
        }
    }
    
}
