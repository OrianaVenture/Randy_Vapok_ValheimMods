using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace EpicLoot.Magic.MagicItemEffects
{
    public static class DecreaseMeadCooldown
    {
        private static readonly List<int> meads = new();
        
        [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))]
        public static class DecreaseMeadCooldown_RegisterMeads
        {
            private static void Postfix(ZNetScene __instance)
            {
                Fermenter fermenter = __instance.GetPrefab("fermenter").GetComponent<Fermenter>();
                foreach (Fermenter.ItemConversion conversion in fermenter.m_conversion)
                {
                    ItemDrop to = conversion.m_to;
                    StatusEffect status = to.m_itemData.m_shared.m_consumeStatusEffect;
                    if (status == null) continue;
                    meads.Add(status.NameHash());
                }
            }
        }
        
        [HarmonyPatch(typeof(SEMan), nameof(SEMan.AddStatusEffect), typeof(StatusEffect), typeof(bool), typeof(int), typeof(float))]
        public static class DecreaseMeadCooldown_AddStatusEffect_Patch
        {
            private static void Postfix(SEMan __instance, ref StatusEffect __result)
            {
                if (__result == null) return;
                if (!IsMead(__result)) return;
                Player player = __instance.m_character as Player;
                if (player == null || player != Player.m_localPlayer) return;
                float effectValue = player.GetTotalActiveMagicEffectValue(MagicEffectType.DecreaseMeadCooldown, 0.01f);
                if (effectValue == 0) return;
                __result.m_ttl *= Mathf.Clamp01(1f - effectValue);
            }
        }

        public static bool IsMead(StatusEffect statusEffect) => IsMead(statusEffect.NameHash());
        public static bool IsMead(int nameHash) => meads.Contains(nameHash);
    }
}
