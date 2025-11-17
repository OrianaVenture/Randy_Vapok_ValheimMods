using HarmonyLib;
using static Player;

namespace EpicLoot.MagicItemEffects
{
    [HarmonyPatch]
    public static class BulkupEffect
    {
        static float HealthBonus= 0f;

        [HarmonyPatch(typeof(SEMan), nameof(SEMan.ModifyHealthRegen))]
        public static class ModifyHealthRegen_Patch
        {
            public static void Postfix(SEMan __instance, ref float regenMultiplier)
            {
                if (__instance.m_character.IsPlayer() && Player.m_localPlayer != null)
                {
                    float bulkupValue = Player.m_localPlayer.GetTotalActiveMagicEffectValue(MagicEffectType.BulkUp);
                    if (bulkupValue > 0)
                    {
                        // clamp down to 1 for the health regen value, can't remove more than 100% of the regen
                        float regenPenalty = bulkupValue * 0.1f;
                        if (regenPenalty > 1) { regenPenalty = 1; }
                        regenMultiplier = regenMultiplier * (1 - regenPenalty);
                        DetermineBulkhealthValue(false);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Player), nameof(Player.EatFood))]
        public static class Player_EatFood_Patch
        {
            public static void Postfix(Player __instance)
            {
                DetermineBulkhealthValue(false);
            }
        }

        [HarmonyPatch(typeof(Player), nameof(Player.GetTotalFoodValue))]
        public static class Player_GetTotalFoodValue_Patch
        {
            public static void Postfix(Player __instance, ref float hp)
            {
                DetermineBulkhealthValue(false);
                hp += HealthBonus;
            }
        }

        [HarmonyPatch(typeof(Player), nameof(Player.GetBaseFoodHP))]
        public static class Player_GetBaseFoodHP_Patch
        {
            public static void Postfix(Player __instance, ref float __result)
            {
                DetermineBulkhealthValue(true);
                __result += HealthBonus;
            }
        }

        private static void DetermineBulkhealthValue(bool skip = false)
        {
            // Skip the details if the value is already set and we arn't updating it.
            if (skip && HealthBonus > 0)
            {
                return;
            }

            float bulkupValue = Player.m_localPlayer.GetTotalActiveMagicEffectValue(MagicEffectType.BulkUp);
            if (bulkupValue > 0)
            {
                float foodHealthRegen = 0f;
                foreach (Food food2 in Player.m_localPlayer.m_foods)
                {
                    foodHealthRegen += food2.m_item.m_shared.m_foodRegen;
                }

                // Include the bonus from the AddHealthRegen effect
                float regenAmount = Player.m_localPlayer.GetTotalActiveMagicEffectValue(MagicEffectType.AddHealthRegen);
                if (regenAmount > 0)
                {
                    foodHealthRegen += regenAmount;
                }

                // The bulkup bonus to health
                float bulkHealthBonus = ((bulkupValue / 30f) * 0.8f) + 1.5f;
                HealthBonus = bulkHealthBonus * foodHealthRegen;
            }
            else
            {
                HealthBonus = 0;
            }
        }
    }
}