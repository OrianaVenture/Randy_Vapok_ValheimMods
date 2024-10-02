using System;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace EpicLoot.MagicItemEffects;

public class CoinHoarder
{
        // Method used to evaluate coins in players inventory. 
        // Used in ModifyDamage class to evluate damage modifier
        // Used in ItemDrop_Patch_MagicItemToolTip class to evaluate magic color of item damage numbers
        public static float GetCoinHoarderValue(Player player)
        {
            if (player == null)
            {
                return 0;
            }
            
            if (player.HasActiveMagicEffect(MagicEffectType.CoinHoarder))
            {
                ItemDrop.ItemData[] mcoins = player.m_inventory.GetAllItems()
                    .Where(val => val.m_dropPrefab.name == "Coins").ToArray();
                
                if (mcoins.Length == 0)
                {
                    return 0;
                }

                float totalCoins = mcoins.Sum(coin => coin.m_stack);
                float coin_hoarder_effect_value =
                    Player.m_localPlayer.GetTotalActiveMagicEffectValue(MagicEffectType.CoinHoarder, 0.01f);
                float coinHoarderBonus = Mathf.Log10(coin_hoarder_effect_value * totalCoins) * 8.7f;
                float coinHoarderDamageMultiplier = 1 + (coinHoarderBonus / 100f);
                // Debug.Log(
                //    $"Coinhorder bonus multipler {coinHoarderDamageMultiplier} coinhorder bonus: {coinHoarderBonus} inv coins: {totalCoins} coinhorder power: {coin_hoarder_effect_value}");

                return coinHoarderDamageMultiplier;
            }

            return 0;
        }
    
    
}