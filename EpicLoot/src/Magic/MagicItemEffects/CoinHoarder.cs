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
        if (player == null) {
            return 1f;
        }

        ItemDrop.ItemData[] mcoins = player.m_inventory.GetAllItems()
                .Where(val => val.m_dropPrefab.name == "Coins").ToArray();

        if (mcoins.Length == 0) {
            return 1f;
        }

        float totalCoins = mcoins.Sum(coin => coin.m_stack);
        float coinHoarderBonus = (Mathf.Log10(effectValue * totalCoins) * 5.5f / 100f) + 1f;
        return coinHoarderBonus;
    }

    public static bool HasCoinHoarder()
    {
        if (Player.m_localPlayer.HasActiveMagicEffect(MagicEffectType.CoinHoarder, out float _cv)) {
            return true;
        }
        return false;
    }
    
}