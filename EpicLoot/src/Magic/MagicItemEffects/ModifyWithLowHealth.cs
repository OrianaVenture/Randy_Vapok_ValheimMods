﻿using System;
using System.Linq.Expressions;
using UnityEngine;

namespace EpicLoot.MagicItemEffects
{
    public static class ModifyWithLowHealth
    {
        public static float defaultValue = 0.3f;

        public static void Apply(Player player, string name, Action<string> action)
        {
            action(name);
            if (PlayerHasLowHealth(player))
            {
                action(name + "LowHealth");
            }
        }

        public static void ApplyOnlyForLowHealth(Player player, string name, Action<string> action)
        {
            if (PlayerHasLowHealth(player))
            {
                action(name + "LowHealth");
            }
        }

        public static bool PlayerHasLowHealth(Player player)
        {
            return player != null && player.GetHealth() / player.GetMaxHealth() < Mathf.Min(GetLowHealthPercentage(player),1.0f);
        }

        public static float GetLowHealthPercentage(Player player)
        {
            float lowHealthThreshold = 0.3f; 

            if (player == null)
            {
                return lowHealthThreshold; 
            }
            
            if (player.HasActiveMagicEffect(MagicEffectType.ModifyLowHealth))
            {
               
                float magicEffectValue = player.GetTotalActiveMagicEffectValue(MagicEffectType.ModifyLowHealth, 0.01f);
                lowHealthThreshold += magicEffectValue;
            }
            
            return lowHealthThreshold;
        }
    }
}
