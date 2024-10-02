using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace EpicLoot.MagicItemEffects
{
    [HarmonyPatch]
    public static class MultiShot
    {
        
        public static bool isTripleShotActive = false;
        
        [HarmonyPatch(typeof(Attack), nameof(Attack.FireProjectileBurst))]
        [HarmonyPrefix]
        public static void Attack_FireProjectileBurst_Prefix(Attack __instance)
        {
            if (__instance?.GetWeapon() == null || __instance.m_character == null || !__instance.m_character.IsPlayer())
            {
                return;
            }
               

            var weaponDamage = __instance.GetWeapon()?.GetDamage();
            if (!weaponDamage.HasValue)
            {
                return;
            }
                

            var player = (Player)__instance.m_character;
            var doubleShot = player.HasActiveMagicEffect(MagicEffectType.DoubleMagicShot);
            var tripleShot = player.HasActiveMagicEffect(MagicEffectType.TripleBowShot);

            if (tripleShot)
            {
                isTripleShotActive = true;
                
                if (__instance.m_projectileAccuracy < 2)
                {
                    __instance.m_projectileAccuracy = 2;
                }

                __instance.m_projectiles = 3;
            }
            else
            {
                isTripleShotActive = false;
            }
            
            if (doubleShot)
            {
                if (__instance.m_projectileAccuracy < 2)
                {
                    __instance.m_projectileAccuracy = 2;
                }

                if (__instance.m_projectileBursts != 1)
                {
                    __instance.m_projectiles = 2;
                }
                else
                {
                    __instance.m_projectiles *= 2;
                }
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

            
            var removeItemMethod = AccessTools.Method(typeof(Inventory), nameof(Inventory.RemoveItem), new Type[] { typeof(ItemDrop.ItemData), typeof(int) });
            
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
                return inventory.RemoveItem(item, amount * 3);
            }
            
            return inventory.RemoveItem(item, amount);
        }
    }
}