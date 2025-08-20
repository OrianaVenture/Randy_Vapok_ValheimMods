using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace EpicLoot.MagicItemEffects
{
    [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.EquipItem))]
    public class Reduced_Reload_Speed
    {
        public static void Postfix(Humanoid __instance, ItemDrop.ItemData item, bool triggerEquipEffects = true)
        {
            Player player = __instance as Player;
            if (player == null) return;

            if (item == null) return;

            if (!item.m_shared.m_attack.m_requiresReload) return;

            var buffEffect = player.GetTotalActiveMagicEffectValue(MagicEffectType.ReducedReloadSpeed, .01f);
            
            if (buffEffect <= 0) return;

            item.m_shared.m_attack.m_reloadTime = 2.5f - buffEffect;
        }
    }
}