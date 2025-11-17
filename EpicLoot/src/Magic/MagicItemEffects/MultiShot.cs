using HarmonyLib;
using System.Collections.Generic;
using System.Numerics;
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
                    weaponDamage.Modify(0.75f);
                }
                __instance.GetWeapon().m_shared.m_damages = weaponDamage;

                ModifyAttackCost(player, tripleshotcfg, __instance.GetAttackStamina(), __instance.GetAttackEitr(), __instance.GetAttackHealth());

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
                Dictionary<string, float> magicshotcfg = MagicItemEffectDefinitions.AllDefinitions[MagicEffectType.DoubleMagicShot].Config;

                // If chance is enabled, roll to see if the effect will run
                if (magicshotcfg != null && magicshotcfg["Chance"] < 1f) {
                    if (UnityEngine.Random.value > magicshotcfg["Chance"]) { return; }
                }

                // Modify double shot damage by the config or the fallback 0.5f;
                var weaponDamage = __instance.GetWeapon().m_shared.m_damages;
                if (magicshotcfg != null && magicshotcfg.ContainsKey("Damage")) {
                    weaponDamage.Modify(magicshotcfg["Damage"]);
                } else {
                    weaponDamage.Modify(0.9f);
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

                ModifyAttackCost(player, magicshotcfg, __instance.GetAttackStamina(), __instance.GetAttackEitr(), __instance.GetAttackHealth());

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

        public static void ModifyAttackCost(Player player, Dictionary<string, float> config, float stamcost, float eitrcost, float healthcost) {
            if (config != null && config.ContainsKey("CostScale")) {
                if (stamcost > 0) { player.UseStamina(healthcost * config["CostScale"]); }
                if (eitrcost > 0) { player.UseEitr(healthcost * config["CostScale"]); }
                if (healthcost > 0) { player.UseHealth(healthcost * config["CostScale"]); }
            } else {
                if (stamcost > 0) { player.UseStamina(healthcost * 2); }
                if (eitrcost > 0) { player.UseEitr(healthcost * 2); }
                if (healthcost > 0) { player.UseHealth(healthcost * 2); }
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
                if (tripleshotcfg != null && tripleshotcfg["Projectiles"] < 1f) {
                    amount *= Mathf.RoundToInt(tripleshotcfg["Projectiles"]);
                } else {
                    amount *= 3;
                }
            }
            return inventory.RemoveItem(item, amount);
        }

    }

}