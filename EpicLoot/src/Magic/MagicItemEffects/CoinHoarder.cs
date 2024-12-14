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
    public static float GetCoinHoarderValue(Player player, float effectValue)
    {
        if (player == null)
        {
            return 0f;
        }

        ItemDrop.ItemData[] mcoins = player.m_inventory.GetAllItems()
                .Where(val => val.m_dropPrefab.name == "Coins").ToArray();

        if (mcoins.Length == 0)
        {
            return 0f;
        }

        float totalCoins = mcoins.Sum(coin => coin.m_stack);
        float coinHoarderBonus = Mathf.Log10(effectValue * totalCoins) * 8.7f;
        return coinHoarderBonus / 100f;
    }

    public static bool HasCoinHoarder(out float coinHoarderDamageMultiplier)
    {
        coinHoarderDamageMultiplier = 0f;
        if (Player.m_localPlayer.HasActiveMagicEffect(MagicEffectType.CoinHoarder, out float coinHoarderEffectValue))
        {
            coinHoarderDamageMultiplier = GetCoinHoarderValue(Player.m_localPlayer, coinHoarderEffectValue);
            return true;
        }

        return false;
    }
    
}