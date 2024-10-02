using System.Collections.Generic;
using HarmonyLib;

namespace EpicLoot.MagicItemEffects
{
    public class Bloodlust
    {
        public class BloodlustItemData
        {
            public string ItemName { get; set; }
            public float StaminaUsage { get; set; }
        }
        
        public static Dictionary<string, BloodlustItemData> itemDataDictionary = new Dictionary<string, BloodlustItemData>();
        
        /// <summary>
        ///  Method to retrieve and store the item's attackStamina value.
        /// </summary>
        /// <param name="weapon"></param>
        public static void SetBloodlustStamina(ItemDrop.ItemData weapon)
        {
            if (!itemDataDictionary.ContainsKey(weapon.GetMagicItem().DisplayName))
            {
                itemDataDictionary[weapon.GetMagicItem().DisplayName] = new BloodlustItemData
                {
                    ItemName = weapon.GetMagicItem().DisplayName,
                    StaminaUsage = weapon.m_shared.m_attack.m_attackStamina,
                }; 
            }
        }
    }
    
    [HarmonyPatch(typeof(Attack), "GetAttackStamina")]
    public static class Attack_GetAttackStamina_Prefix_Patch
    {
        public static void Prefix(Attack __instance, ref float __result)
        {
            if (__instance.m_character is Player player &&
                MagicEffectsHelper.HasActiveMagicEffectOnWeapon(player, __instance.m_weapon, MagicEffectType.Bloodlust))
            {
                Bloodlust.SetBloodlustStamina(__instance.m_weapon);
                
                __instance.m_attackStamina = 0f;
                __instance.m_weapon.m_shared.m_attack.m_attackStamina = 0f;
                __result = 0f;
            }
        }
    }
    
    [HarmonyPatch(typeof(Attack), nameof(Attack.GetAttackHealth))]
    public class Bloodlust_Attack_GetAttackHealth_Patch
    {
        public static void Postfix(Attack __instance, ref float __result)
        {
            if (__instance.m_character is Player player && MagicEffectsHelper.HasActiveMagicEffectOnWeapon(player, __instance.m_weapon, MagicEffectType.Bloodlust))
            {
                if (Bloodlust.itemDataDictionary.TryGetValue(__instance.m_weapon.GetMagicItem().DisplayName, out Bloodlust.BloodlustItemData bloodlustItemData))
                {
                    float newAttackHealth = bloodlustItemData.StaminaUsage;
                    __instance.m_weapon.m_shared.m_attack.m_attackHealth = bloodlustItemData.StaminaUsage;
                    float skillFactor = __instance.m_character.GetSkillFactor(__instance.m_weapon.m_shared.m_skillType);
                    __result = newAttackHealth - newAttackHealth * 0.33f * skillFactor;
                }
            }
        }
    }
}

