using System.Collections.Generic;
using System.Linq.Expressions;
using HarmonyLib;
using UnityEngine;

namespace EpicLoot.MagicItemEffects;

public class IncreaseMiningDrop
{
    [HarmonyPatch(typeof(DropTable), nameof(DropTable.GetDropList), typeof(int))]
    public static class IncreaseMiningDrop_DropTable_GetDropList_Patch
    {
        private static void Prefix(ref int amount)
        {
            if (!IncreaseMiningDrop_Attack_OnAttackTrigger_Patch.playerHasIncreaseMiningDropEffect)
            {
                if (!IncreaseMiningDrop_MineRock5_RPC_Damage_Patch.IsMining)
                {
                    return; 
                }
            }
            
            int magicEffectValue = Mathf.FloorToInt(IncreaseMiningDrop_Attack_OnAttackTrigger_Patch.increaseMiningDropEffectValue);
            amount += magicEffectValue;
        }
    }

    [HarmonyPatch(typeof(Attack), nameof(Attack.GetAttackStamina))]
    public static class IncreaseMiningDrop_Attack_OnAttackTrigger_Patch
    {
        public static bool playerHasIncreaseMiningDropEffect = false;
        public static float increaseMiningDropEffectValue = 0;

        private static void Prefix(Attack __instance)
        {
            if (__instance.m_character is Player player)
            {
                if (player.HasActiveMagicEffect(MagicEffectType.IncreaseMiningDrop))
                {
                    playerHasIncreaseMiningDropEffect = true;
                    increaseMiningDropEffectValue = MagicEffectsHelper.GetTotalActiveMagicEffectValueForWeapon(player,
                        __instance.m_weapon, MagicEffectType.IncreaseMiningDrop, 1f);
                }
                
                playerHasIncreaseMiningDropEffect = false;
            }
        }
    }

    [HarmonyPatch(typeof(MineRock5), nameof(MineRock5.RPC_Damage))]
    public static class IncreaseMiningDrop_MineRock5_RPC_Damage_Patch
    {
        public static bool IsMining = false;

        private static bool Prefix(HitData hit)
        {
            return HandleMining(hit);
        }

        public static bool HandleMining(HitData hit)
        {
            if (hit.GetAttacker() is not Player player)
            {
                return true;
            }

            IsMining = true;
            return true;
        }

        private static void Finalizer()
        {
            IsMining = false;
        }
    }
    
    [HarmonyPatch(typeof(Destructible), nameof(Destructible.RPC_Damage))]
    public static class IncreaseMiningDrop_Destructible_RPC_Damage_Patch
    {
        private static bool Prefix(Destructible __instance, HitData hit)
        {
            if (!IncreaseMiningDrop_MineRock5_RPC_Damage_Patch.IsMining && __instance.m_damages.m_pickaxe != HitData.DamageModifier.Immune && __instance.m_damages.m_chop == HitData.DamageModifier.Immune && __instance.m_destructibleType != DestructibleType.Tree)
            {            
                return IncreaseMiningDrop_MineRock5_RPC_Damage_Patch.HandleMining(hit);
            }
            return true;
        }

        private static void Finalizer()
        {
            IncreaseMiningDrop_MineRock5_RPC_Damage_Patch.IsMining = false;
        }
    }
}