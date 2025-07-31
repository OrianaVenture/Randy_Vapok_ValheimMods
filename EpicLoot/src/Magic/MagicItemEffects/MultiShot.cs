using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;

namespace EpicLoot.MagicItemEffects
{
    [HarmonyPatch]
    public static class MultiShot
    {

        public static bool isTripleShotActive = false;

        [HarmonyPatch(typeof(Attack), nameof(Attack.FireProjectileBurst))]
        [HarmonyPrefix]
        public static void Attack_FireProjectileBurst_Prefix(Attack __instance, ref HitData.DamageTypes? __state)
        {
            if (__instance?.GetWeapon() == null || __instance.m_character == null || !__instance.m_character.IsPlayer())
            {
                return;
            }

            __state = __instance.GetWeapon().m_shared.m_damages;


            var player = (Player)__instance.m_character;

            if (player.HasActiveMagicEffect(MagicEffectType.TripleBowShot, out float tripleBowEffectValue)) {
                var tripleshotcfg = MagicItemEffectDefinitions.AllDefinitions[MagicEffectType.TripleBowShot].Config;

                // If chance is enabled, roll to see if the effect will run
                if (tripleshotcfg != null && tripleshotcfg["Chance"] < 1f) {
                    if (UnityEngine.Random.value > tripleshotcfg["Chance"]) { return; }
                }

                isTripleShotActive = true;

                // Modify triple shot damage by the config or the fallback 0.35f;
                var weaponDamage = __instance.GetWeapon().m_shared.m_damages;
                if (tripleshotcfg != null && tripleshotcfg.ContainsKey("Damage")) {
                    weaponDamage.Modify(tripleshotcfg["Damage"]);
                } else {
                    weaponDamage.Modify(0.35f);
                }
                __instance.GetWeapon().m_shared.m_damages = weaponDamage;

                if (__instance.GetWeapon().m_shared.m_attack.m_attackHealth > 0) {
                    if (tripleshotcfg != null && tripleshotcfg.ContainsKey("Projectiles")) {
                        int projectiles = __instance.m_weapon.m_shared.m_attack.m_projectiles * Mathf.RoundToInt(tripleshotcfg["Projectiles"]);
                        // If more than 1 projectile we remove 1, as the cost for the initial item is already paid
                        if (projectiles > 1) { projectiles -= 1; }
                        player.UseHealth(__instance.GetWeapon().m_shared.m_attack.m_attackHealth *= projectiles);
                    }
                    else {
                        player.UseHealth(__instance.GetWeapon().m_shared.m_attack.m_attackHealth *= 2);
                    }
                     
                }

                // Modify the shots accuracy, if its not defined, no accuracy change
                if (tripleshotcfg != null && tripleshotcfg.ContainsKey("Accuracy"))
                {
                    __instance.m_projectileAccuracy = __instance.m_weapon.m_shared.m_attack.m_projectileAccuracy * tripleshotcfg["Accuracy"];
                }

                // Set projectiles, if config is not defined its x3
                if (tripleshotcfg != null && tripleshotcfg.ContainsKey("Projectiles")) {
                    __instance.m_projectiles = __instance.m_weapon.m_shared.m_attack.m_projectiles * Mathf.RoundToInt(tripleshotcfg["Projectiles"]);
                } else {
                    __instance.m_projectiles = __instance.m_weapon.m_shared.m_attack.m_projectiles * 3;
                }
            } else {
                isTripleShotActive = false;
            }

            if (player.HasActiveMagicEffect(MagicEffectType.DoubleMagicShot, out float doubleMagicEffectValue)) {
                var magicshotcfg = MagicItemEffectDefinitions.AllDefinitions[MagicEffectType.DoubleMagicShot].Config;

                // If chance is enabled, roll to see if the effect will run
                if (magicshotcfg != null && magicshotcfg["Chance"] < 1f) {
                    if (UnityEngine.Random.value > magicshotcfg["Chance"]) { return; }
                }

                // Modify double shot damage by the config or the fallback 0.5f;
                var weaponDamage = __instance.GetWeapon().m_shared.m_damages;
                if (magicshotcfg != null && magicshotcfg.ContainsKey("Damage")) {
                    weaponDamage.Modify(magicshotcfg["Damage"]);
                } else {
                    weaponDamage.Modify(0.6f);
                }
                __instance.GetWeapon().m_shared.m_damages = weaponDamage;


                // Modify the shots accuracy, if its not defined, no accuracy change
                if (magicshotcfg != null && magicshotcfg.ContainsKey("Accuracy"))
                {
                    __instance.m_projectileAccuracy = __instance.m_weapon.m_shared.m_attack.m_projectileAccuracy * magicshotcfg["Accuracy"];
                }
                //// Cap the accuracy
                //if (__instance.m_projectileAccuracy < 5) {
                //    __instance.m_projectileAccuracy = 5;
                //    __instance.m_projectileAccuracyMin = 3;
                //}

                // Set projectiles, if config is not defined its x3
                if (magicshotcfg != null && magicshotcfg.ContainsKey("Projectiles")) {
                    __instance.m_projectiles = __instance.m_weapon.m_shared.m_attack.m_projectiles * Mathf.RoundToInt(magicshotcfg["Projectiles"]);
                } else {
                    __instance.m_projectiles = __instance.m_weapon.m_shared.m_attack.m_projectiles * 2;
                }
            }
        }

        [HarmonyPatch(typeof(Attack), nameof(Attack.FireProjectileBurst))]
        public static void Postfix(Attack __instance, ref HitData.DamageTypes? __state)
        {
            if (__state != null)
            {
                __instance.GetWeapon().m_shared.m_damages = __state.Value;
            }
        }
    }

    /// <summary>
    /// Patch to remove thrice ammo when using TripleShot
    /// </summary>
    [HarmonyPatch(typeof(Attack))]
    public static class UseAmmoTranspilerPatch
    {
        //[HarmonyDebug]
        [HarmonyTranspiler]
        [HarmonyPatch(nameof(Attack.UseAmmo))]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions /*, ILGenerator generator*/)
        {
            var codeMatcher = new CodeMatcher(instructions);
            codeMatcher.MatchStartForward(
                new CodeMatch(OpCodes.Callvirt),
                new CodeMatch(OpCodes.Ldarg_1),
                new CodeMatch(OpCodes.Ldind_Ref),
                new CodeMatch(OpCodes.Ldc_I4_1),
                new CodeMatch(OpCodes.Callvirt)
                ).Advance(4).RemoveInstructions(1).InsertAndAdvance(
                Transpilers.EmitDelegate(CustomRemoveItem)
                ).ThrowIfNotMatch("Unable to ammo removal for tripleshot.");
            return codeMatcher.Instructions();
        }

        public static bool CustomRemoveItem(Inventory inventory, ItemDrop.ItemData item, int amount)
        {
            if (MultiShot.isTripleShotActive) {
                MultiShot.isTripleShotActive = false;
                var tripleshotcfg = MagicItemEffectDefinitions.AllDefinitions[MagicEffectType.TripleBowShot].Config;

                // If chance is enabled, roll to see if the effect will run
                if (tripleshotcfg != null && tripleshotcfg["Projectiles"] < 1f) {
                    amount *= Mathf.RoundToInt(tripleshotcfg["Projectiles"]);
                } else {
                    amount *= 3;
                }
            }

            return inventory.RemoveItem(item, amount);
        }
    }

    [HarmonyPriority(Priority.HigherThanNormal)]
    [HarmonyPatch(typeof(Attack), nameof(Attack.GetAttackEitr))]
    public class DoubleMagicShot_Attack_GetAttackEitr_Patch
    {
        public static void Postfix(Attack __instance, ref float __result)
        {
            if (__instance.m_character is Player player)
            {
                if (MagicEffectsHelper.HasActiveMagicEffectOnWeapon(
                    player, __instance.m_weapon, MagicEffectType.DoubleMagicShot, out float effectValue))
                {
                    var tripleshotcfg = MagicItemEffectDefinitions.AllDefinitions[MagicEffectType.DoubleMagicShot].Config;
                    if (tripleshotcfg != null && tripleshotcfg.ContainsKey("EitrCostScale")) {
                        __result *= tripleshotcfg["EitrCostScale"];
                    } else {
                        __result *= 2;
                    }
                }
            }
        }
    }
}