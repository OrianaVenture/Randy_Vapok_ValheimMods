using System.Collections.Generic;
using System.Linq.Expressions;
using HarmonyLib;
using UnityEngine;

namespace EpicLoot.MagicItemEffects;

public class IncreaseTreeDrop
{
    [HarmonyPatch(typeof(DropTable), nameof(DropTable.GetDropList), typeof(int))]
    public static class IncreaseTreeDrop_DropTable_GetDropList_Patch
    {
        private static void Prefix(ref int amount)
        {
            if (!IncreaseTreeDrop_Attack_OnAttackTrigger_Patch.playerHasIncreaseTreeDropEffect)
            {
                if (!IncreaseTreeDrop_TreeLog_RPC_Damage_Patch.IsCuttingTree)
                {
                    return; 
                }
            }
            
            int magicEffectValue = Mathf.FloorToInt(IncreaseTreeDrop_Attack_OnAttackTrigger_Patch.increaseTreeDropEffectValue);
            amount += magicEffectValue;
        }
    }

    [HarmonyPatch(typeof(Attack), nameof(Attack.GetAttackStamina))]
    public static class IncreaseTreeDrop_Attack_OnAttackTrigger_Patch
    {
        public static bool playerHasIncreaseTreeDropEffect = false;
        public static float increaseTreeDropEffectValue = 0;

        private static void Prefix(Attack __instance)
        {
            if (__instance.m_character is Player player)
            {
                if (player.HasActiveMagicEffect(MagicEffectType.IncreaseTreeDrop))
                {
                    playerHasIncreaseTreeDropEffect = true;
                    increaseTreeDropEffectValue = MagicEffectsHelper.GetTotalActiveMagicEffectValueForWeapon(player,
                        __instance.m_weapon, MagicEffectType.IncreaseTreeDrop, 1f);
                }
                
                playerHasIncreaseTreeDropEffect = false;
            }
        }
    }

    [HarmonyPatch(typeof(TreeLog), nameof(TreeLog.RPC_Damage))]
    public static class IncreaseTreeDrop_TreeLog_RPC_Damage_Patch
    {
        public static bool IsCuttingTree = false;

        private static bool Prefix(HitData hit)
        {
            return HandleCutting(hit);
        }

        public static bool HandleCutting(HitData hit)
        {
            if (hit.GetAttacker() is not Player player)
            {
                return true;
            }

            IsCuttingTree = true;
            return true;
        }

        private static void Finalizer()
        {
            IsCuttingTree = false;
        }
    }
    
    [HarmonyPatch(typeof(TreeBase), nameof(TreeBase.RPC_Damage))]
    public static class IncreaseTreeDrop_TreeBase_RPC_Damage_Patch
    {
        private static bool Prefix(TreeBase __instance, HitData hit)
        {
            if (!IncreaseTreeDrop_TreeLog_RPC_Damage_Patch.IsCuttingTree && __instance.m_damageModifiers.m_chop == HitData.DamageModifier.Normal && __instance.m_damageModifiers.m_chop != HitData.DamageModifier.Immune)
            {            
                return IncreaseTreeDrop_TreeLog_RPC_Damage_Patch.HandleCutting(hit);
            }
            return true;
        }

        private static void Finalizer()
        {
            IncreaseTreeDrop_TreeLog_RPC_Damage_Patch.IsCuttingTree = false;
        }
    }
    
    [HarmonyPatch(typeof(Destructible), nameof(Destructible.RPC_Damage))]
    public static class IncreaseTreeDrop_Destructible_RPC_Damage_Patch
    {
        private static bool Prefix(Destructible __instance, HitData hit)
        {
            if (!IncreaseTreeDrop_TreeLog_RPC_Damage_Patch.IsCuttingTree && __instance.m_destructibleType == DestructibleType.Tree && __instance.m_damages.m_chop == HitData.DamageModifier.Normal)
            {            
                return IncreaseTreeDrop_TreeLog_RPC_Damage_Patch.HandleCutting(hit);
            }
            return true;
        }

        private static void Finalizer()
        {
            IncreaseTreeDrop_TreeLog_RPC_Damage_Patch.IsCuttingTree = false;
        }
    }
}