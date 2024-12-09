using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace EpicLoot.MagicItemEffects;

public class ModifySummonHealth
{
    /*private static readonly Dictionary<Humanoid, float> originalHealth = new Dictionary<Humanoid, float>();

    [HarmonyPatch(typeof(Attack), nameof(Attack.FireProjectileBurst))]
    public class ModifySummonHealth_Attack_FireProjectileBurst_Patch
    {
        public static void Prefix(Attack __instance)
        {
            if (!(__instance.m_character is Player player) ||
                !MagicEffectsHelper.HasActiveMagicEffect(player, __instance.m_weapon, MagicEffectType.ModifySummonHealth, out float effectValue) ||
                __instance.m_attackProjectile == null)
            {
                return;
            }

            if (!__instance.m_attackProjectile.TryGetComponent<SpawnAbility>(out var spawnAbility))
            {
                return;
            }

            var spawnPrefab = spawnAbility.m_spawnPrefab[0];
            if (spawnPrefab == null)
            {
                return;
            }

            if (!spawnPrefab.TryGetComponent<Humanoid>(out var humanoid))
            {
                return;
            }

            if (!originalHealth.ContainsKey(humanoid))
            {
                originalHealth[humanoid] = humanoid.m_health;
            }

            humanoid.m_health *= 1 + effectValue;
        }

        public static void Postfix(Attack __instance)
        {
            if (__instance.m_attackProjectile == null)
            {
                return;
            }

            var spawnProjectile = __instance.m_attackProjectile;
            if (!spawnProjectile.TryGetComponent<SpawnAbility>(out var spawnAbility))
            {
                return;
            }

            var spawnPrefab = spawnAbility.m_spawnPrefab[0];
            
            if (spawnPrefab == null)
            {
                return;
            }

            if (!spawnPrefab.TryGetComponent<Humanoid>(out var humanoid))
            {
                return;
            }

            if (originalHealth.TryGetValue(humanoid, out var origHealth))
            {
                humanoid.m_health = origHealth;
                originalHealth.Remove(humanoid);
            }
        }
    }*/
}