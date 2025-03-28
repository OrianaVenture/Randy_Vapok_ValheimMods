using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace EpicLoot.MagicItemEffects
{
    [HarmonyPatch]
    public static class MultiShot
    {
        private static HitData.DamageTypes modifyDamage = new HitData.DamageTypes
        {
            m_damage = 0.3f,
            m_blunt = 0.3f,
            m_slash = 0.3f,
            m_pierce = 0.3f,
            m_chop = 0.3f,
            m_pickaxe = 0.3f,
            m_fire = 0.3f,
            m_frost = 0.3f,
            m_lightning = 0.3f,
            m_poison = 0.3f,
            m_spirit = 0.3f
        };

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
            var weaponDamage = __instance.GetWeapon().m_shared.m_damages;
            weaponDamage.Modify(modifyDamage);
            __instance.GetWeapon().m_shared.m_damages = weaponDamage;

            var player = (Player)__instance.m_character;

            if (player.HasActiveMagicEffect(MagicEffectType.TripleBowShot, out float tripleBowEffectValue))
            {
                isTripleShotActive = true;

                if (__instance.m_projectileAccuracy < 3)
                {
                    __instance.m_projectileAccuracy = 3;
                }
                else
                {
                    __instance.m_projectileAccuracy = __instance.m_weapon.m_shared.m_attack.m_projectileAccuracy * 1.25f;
                }

                __instance.m_projectiles = 3;
            }
            else
            {
                isTripleShotActive = false;
            }

            if (player.HasActiveMagicEffect(MagicEffectType.DoubleMagicShot, out float doubleMagicEffectValue))
            {
                if (__instance.m_projectileAccuracy < 5)
                {
                    __instance.m_projectileAccuracy = 5;
                    __instance.m_projectileAccuracyMin = 3;
                }
                else
                {
                    __instance.m_projectileAccuracy = __instance.m_weapon.m_shared.m_attack.m_projectileAccuracy * 1.25f;
                }

                __instance.m_projectiles = __instance.m_weapon.m_shared.m_attack.m_projectiles * 2;
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
    [HarmonyPatch(typeof(Attack), nameof(Attack.UseAmmo))]
    public static class UseAmmoTranspilerPatch
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var code = new List<CodeInstruction>(instructions);

            var removeItemMethod = AccessTools.Method(typeof(Inventory), nameof(Inventory.RemoveItem),
                new Type[] { typeof(ItemDrop.ItemData), typeof(int) });

            for (int i = 0; i < code.Count; i++)
            {
                if (code[i].Calls(removeItemMethod))
                {
                    code[i] = new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(UseAmmoTranspilerPatch), nameof(CustomRemoveItem)));
                }
            }

            return code.AsEnumerable();
        }

        public static bool CustomRemoveItem(Inventory inventory, ItemDrop.ItemData item, int amount)
        {
            if (MultiShot.isTripleShotActive)
            {
                MultiShot.isTripleShotActive = false;
                return inventory.RemoveItem(item, amount * 3); // TODO fix?
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
                    __result *= 2;
                }
            }
        }
    }
}