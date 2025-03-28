using HarmonyLib;
using System;
using static Player;

namespace EpicLoot.MagicItemEffects
{
    [HarmonyPatch]
    public static class Bulkup_Effect
    {
        static float health_bonus_from_effect = 0f;

        [HarmonyPatch(typeof(SEMan), nameof(SEMan.ModifyHealthRegen))]
        public static class ModifyHealthRegen
        {
            public static void Postfix(SEMan __instance, ref float regenMultiplier)
            {
                if (__instance.m_character.IsPlayer() && Player.m_localPlayer != null)
                {
                    float bulkupValue = Player.m_localPlayer.GetTotalActiveMagicEffectValue(MagicEffectType.BulkUp);
                    if (bulkupValue > 0)
                    {
                        // clamp down to 1 for the health regen value, can't remove more than 100% of the regen
                        float regen_penalty = bulkupValue * 0.1f;
                        if (regen_penalty > 1) { regen_penalty = 1; }
                        regenMultiplier = regenMultiplier * (1 - regen_penalty);
                        DetermineBulkhealthValue(false);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Player), nameof(Player.EatFood))]
        public static class Player_Eat_Food
        {
            public static void Postfix(Player __instance)
            {
                DetermineBulkhealthValue(false);
            }
        }

        [HarmonyPatch(typeof(Player), nameof(Player.GetTotalFoodValue))]
        public static class Player_GetTotalFoodValue
        {
            public static void Postfix(Player __instance, ref float hp)
            {
                DetermineBulkhealthValue(false);
                hp += health_bonus_from_effect;
            }
        }

        [HarmonyPatch(typeof(Player), nameof(Player.GetBaseFoodHP))]
        public static class Player_GetBaseFoodHP
        {
            public static void Postfix(Player __instance, ref float __result)
            {
                DetermineBulkhealthValue(true);
                __result += health_bonus_from_effect;
            }
        }

        private static void DetermineBulkhealthValue(bool skip_if_set = false)
        {
            // EpicLoot.Log($"Checking Bulk Health Values, current bonus {health_bonus_from_effect}");
            // Skip the details if the value is already set and we arn't updating it.
            if (skip_if_set && health_bonus_from_effect > 0) { return; }

            float bulkupValue = Player.m_localPlayer.GetTotalActiveMagicEffectValue(MagicEffectType.BulkUp);
            if (bulkupValue > 0)
            {
                float health_regen_from_food = 0f;
                foreach (Food food2 in Player.m_localPlayer.m_foods)
                {
                    health_regen_from_food += food2.m_item.m_shared.m_foodRegen;
                }
                // The bulkup bonus to health
                // float bulk_health_bonus = (bulkupValue / 25) + 1;
                float bulk_health_bonus = (float)Math.Sqrt(bulkupValue / 60f) + 1.2f;
                if (bulk_health_bonus > 3f) {
                    bulk_health_bonus = 3f;
                }
                health_bonus_from_effect = bulk_health_bonus * health_regen_from_food;
                // EpicLoot.Log($"Bulkup setting bonus health {health_bonus_from_effect}");
            } else {
                health_bonus_from_effect = 0;
            }
        }
    }
}
