using System.Collections.Generic;
using EpicLoot.MagicItemEffects;
using HarmonyLib;
using UnityEngine;

namespace EpicLoot.Magic.MagicItemEffects;

public class AutoMeads
{
    [HarmonyPatch(typeof(Character), nameof(Character.Damage))]
    public static class Character_Damage_Prefix_Patch
    {
        public static void Prefix(Character __instance, HitData hit)
        {
            Player player = __instance as Player;
            if (player == null || player != Player.m_localPlayer)
            {
                return;
            }

            if (!player.HasActiveMagicEffect(MagicEffectType.AutoMead))
            {
                return;
            }

            if (ModifyWithLowHealth.PlayerHasLowHealth(player) == false || PlayerWillBecomeHealthCritical(player, hit) == false)
            {
                return;
            }

            Inventory inventory = player.m_inventory;
            if (inventory == null)
            {
                return;
            }

            List<ItemDrop.ItemData> items = inventory.GetAllItemsOfType(ItemDrop.ItemData.ItemType.Consumable);
            foreach (ItemDrop.ItemData item in items)
            {
                if (HasHealthRegen(item))
                {
                    player.ConsumeItem(inventory, item);
                }
            }
        }
    }

    public static bool HasHealthRegen(ItemDrop.ItemData itemData)
    {
        StatusEffect statusEffect = itemData.m_shared.m_consumeStatusEffect;
        if (statusEffect == null)
        {
            return false;
        }

        if (statusEffect is SE_Stats seStats)
        {
            return (seStats.m_healthOverTime > 0 || seStats.m_healthUpFront > 0);
        }

        return false;
    }

    public static bool PlayerWillBecomeHealthCritical(Player player, HitData hit)
    {
        float lowHealthPercentage = Mathf.Min(ModifyWithLowHealth.GetLowHealthPercentage(player), 1.0f) * player.GetMaxHealth();
        float currentHealth = player.GetHealth();
        float hitTotalDamage = hit.GetTotalDamage();
        
        hitTotalDamage -= hit.m_damage.m_chop;
        hitTotalDamage -= hit.m_damage.m_pickaxe;
        hitTotalDamage -= hit.m_damage.m_spirit;

        float armorValue = player.GetBodyArmor();
        hitTotalDamage = ApplyArmor(hitTotalDamage, armorValue);

        if ((currentHealth - hitTotalDamage) < lowHealthPercentage)
        {
            return true;
        }

        return false;
    }

    public static float ApplyArmor(float dmg, float ac)
    {
        float num = Mathf.Clamp01(dmg / (ac * 4f)) * dmg;
        if ((double) ac < (double) dmg / 2.0)
        {
            num = dmg - ac;
        }

        return num;
    }
}
