using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace EpicLoot.MagicItemEffects;

public class ModifySummonDamage
{
    /*private static readonly Dictionary<Humanoid, Dictionary<ItemDrop, HitData.DamageTypes>> originalDamages = new Dictionary<Humanoid, Dictionary<ItemDrop, HitData.DamageTypes>>();
    
    [HarmonyPatch(typeof(Attack), nameof(Attack.FireProjectileBurst))]
    public class ModifySummonDamage_Attack_FireProjectileBurst_Patch
    {
        public static void Prefix(Attack __instance)
        {
            if (!(__instance.m_character is Player player) || 
                !MagicEffectsHelper.HasActiveMagicEffectOnWeapon(player, __instance.m_weapon, MagicEffectType.ModifySummonDamage, out float effectValue, 0.01f) ||
                __instance.m_attackProjectile == null)
            {
                return;
            }

            float modifier = 1 + effectValue;
            var spawnProjectile = __instance.m_attackProjectile;

            if (!spawnProjectile.TryGetComponent<SpawnAbility>(out var spawnAbility))
            {
                return;
            }

            var spawnPrefab = spawnAbility.m_spawnPrefab[0];

            if (spawnPrefab == null || !spawnPrefab.TryGetComponent<Humanoid>(out var humanoid))
            {
                return;
            }

            GameObject[] randomWeapons = humanoid.m_randomWeapon;
            if (randomWeapons != null)
            {
                if (!originalDamages.ContainsKey(humanoid))
                {
                    originalDamages[humanoid] = new Dictionary<ItemDrop, HitData.DamageTypes>();
                }

                foreach (var weapon in randomWeapons)
                {
                    if (weapon.TryGetComponent(out ItemDrop itemDrop))
                    {
                        var itemDropDamages = itemDrop.m_itemData.m_shared.m_damages;

                        if (!originalDamages[humanoid].ContainsKey(itemDrop))
                        {
                            originalDamages[humanoid][itemDrop] = itemDropDamages;
                        }

                        itemDropDamages.Modify(modifier);
                    }
                }
            }

            GameObject[] defaultItems = humanoid.m_defaultItems;
            if (defaultItems != null)
            {
                if (!originalDamages.ContainsKey(humanoid))
                {
                    originalDamages[humanoid] = new Dictionary<ItemDrop, HitData.DamageTypes>();
                }

                foreach (var weapon in defaultItems)
                {
                    if (weapon.TryGetComponent(out ItemDrop itemDrop))
                    {
                        var itemDropDamages = itemDrop.m_itemData.m_shared.m_damages;

                        if (!originalDamages[humanoid].ContainsKey(itemDrop))
                        {
                            originalDamages[humanoid][itemDrop] = itemDropDamages;
                        }

                        itemDropDamages.Modify(modifier);
                    }
                }
            }
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

            if (spawnPrefab == null || !spawnPrefab.TryGetComponent<Humanoid>(out var humanoid))
            {
                return;
            }

            if (!originalDamages.TryGetValue(humanoid, out var itemDropDamages))
            {
                return;
            }

            foreach (var kvp in itemDropDamages)
            {
                if (kvp.Key.TryGetComponent(out ItemDrop itemDrop))
                {
                    var originalDamage = kvp.Value;
                    itemDrop.m_itemData.m_shared.m_damages = originalDamage;
                }
            }

            originalDamages.Remove(humanoid);
        }
    }*/
}