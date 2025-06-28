using EpicLoot;
using HarmonyLib;
using Jotunn.Entities;
using Jotunn.Managers;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace EpicLoot.MagicItemEffects
{
    [HarmonyPatch(typeof(Player), nameof(Player.UpdateDodge))]
    [HarmonyPatch(new Type[] { typeof(float) })]

    public static class DodgeBuff
    {
        private static void Postfix(Player __instance)
        {
            int rushHash = "Adrenaline_Rush".GetStableHashCode(); 

            if (__instance == Player.m_localPlayer &&
                __instance.m_dodgeInvincible &&
                __instance.m_seman.GetStatusEffect(rushHash) == null && // Checks if that status effect is on __instance to prevent to many checks. Check back here to make refreshing the buff possible.
                __instance.GetTotalActiveMagicEffectValue(MagicEffectType.DodgeBuff, 1f) > 0f)

            {
                __instance.m_seman.AddStatusEffect(rushHash);
                Jotunn.Logger.LogInfo("Dodged detected. Buff Applied");
            }
        }
    } //If character holds item with dodgebuff add status effect on dodge

    public class StatusEffects_Utils_DodgeBuff // Need to make this the status effect that shows up and apply the damage to this buff
    {
        public void CreateMyStatusEffect()
        {
            SE_Stats myStatusEffect = ScriptableObject.CreateInstance<SE_Stats>(); // create new instance of se_stats

            Sprite iconSprite = ObjectDB.instance.GetStatusEffect("Rested".GetStableHashCode()).m_icon;

            //fill out fields in se_stats to make the status effect I want
            myStatusEffect.name = "Adrenaline_Rush";
            myStatusEffect.m_name = "Adrenaline Rush";
            myStatusEffect.m_tooltip = "Increased damage for the duration of the effect.";
            myStatusEffect.m_icon = iconSprite;
            myStatusEffect.m_ttl = 20f;

            ObjectDB.instance.m_StatusEffects.Add(myStatusEffect);

            //Instantiate the effect in code
            CustomStatusEffect Adrenaline_Rush = new CustomStatusEffect(myStatusEffect, fixReference: false);
            //CustomStatusEffect MyTestBuff = new CustomStatusEffect(testEffect, fixReference: false);
            //Register the status effect into the game
            ItemManager.Instance.AddStatusEffect(Adrenaline_Rush);
            //ItemManager.Instance.AddStatusEffect(MyTestBuff);
        }

        // Called in epic loot status effect thingy to add to list of status effects in game.

        // use jottun custom status effect to create a jottun status effect 

        // use jottun add custom status effect to add to game

        // if stuck look at monster modifiers 

        // build add to game  and use dev command to add status effect to player


    }
}

