using HarmonyLib;

namespace EpicLoot.MagicItemEffects;

[HarmonyPatch(typeof(Character), nameof(Character.SetMaxHealth))]
public class ModifySummonHealth
{
    public static void Prefix(Character __instance, ref float health)
    {
        if (!__instance.IsTamed()) { return; }
        //EpicLoot.Log($"Checking for Summon Health Increase {__instance.name} {health}");

        Tameable isTamable = __instance.GetComponent<Tameable>();
        if (isTamable == null) { return; }
        if (isTamable.m_levelUpOwnerSkill == Skills.SkillType.BloodMagic || isTamable.m_levelUpOwnerSkill == Skills.SkillType.ElementalMagic)
        {
            if (Player.m_localPlayer != null && Player.m_localPlayer.HasActiveMagicEffect(MagicEffectType.ModifySummonHealth, out float effectValue, 0.01f))
            {
                //EpicLoot.Log($"Increasing summon health {effectValue}%");
                health *= (1 + effectValue);
            }
        }

    }
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