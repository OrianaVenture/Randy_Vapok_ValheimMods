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
            return 1f;
        }

        ItemDrop.ItemData[] mcoins = player.m_inventory.GetAllItems()
                .Where(val => val.m_dropPrefab != null && val.m_dropPrefab.name == "Coins").ToArray();

        if (mcoins.Length == 0)
        {
            return 1f;
        }

        float totalCoins = mcoins.Sum(coin => coin.m_stack);
        if (totalCoins <= 1000)
        {
            // Linear fraction increase up till 1000 coins, then logarithmic decay increase (1.145x at 1000)
            return (1f + totalCoins * 0.000145f);
        }
        // Slope intercept at effectValue 3 * 1000 coins = 0.145065498747
        // This will result in a bump at higher effects and higher coin coints when going just over 1000 coins
        // But the logarithmic curve quickly diminishes these returns, 20,000 coins and 10 coinhoarder results in 0.22115
        float coinHoarderBonus = (Mathf.Log10(effectValue * totalCoins) * 6.258f / 150f) + 1f;
        return coinHoarderBonus;
    }

    public static bool HasCoinHoarder()
    {
        if (Player.m_localPlayer.HasActiveMagicEffect(MagicEffectType.CoinHoarder, out float _cv))
        {
            return true;
        }
        return false;
    }

}