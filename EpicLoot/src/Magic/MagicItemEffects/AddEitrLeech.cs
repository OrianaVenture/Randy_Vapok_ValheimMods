using System;
using EpicLoot.Healing;
using HarmonyLib;
using UnityEngine;

namespace EpicLoot.MagicItemEffects
{
    public static class AddEitrLeech
    {
        [HarmonyPatch(typeof(Character), nameof(Character.Damage))]
        public static class AddEitrLeechAddLifeSteal_Character_Damage_Patch
        {
            public static void Postfix(HitData hit)
            {
                CheckAndDoEitrLeech(hit);
            }
        }
        
        public static void CheckAndDoEitrLeech(HitData hit)
        {
            try
            {
                if (!hit.HaveAttacker())
                {
                    return;
                }

                var attacker = hit.GetAttacker() as Humanoid;
                if (attacker == null)
                {
                    return;
                }

                var weapon = attacker.GetCurrentWeapon();
                if (Attack_Patch.ActiveAttack != null)
                    weapon = Attack_Patch.ActiveAttack.m_weapon;

                // in case weapon's durability is destroyed after hit?
                // OR in case damage is delayed and player hides weapon
                if (weapon == null || !weapon.IsMagic() || !(attacker is Player player))
                {
                    return; 
                }
                    

                var eitrleechMultiplier = 0f;
                eitrleechMultiplier += MagicEffectsHelper.GetTotalActiveMagicEffectValueForWeapon(
                    player, weapon, MagicEffectType.EitrLeech, 0.01f);


                if (eitrleechMultiplier == 0)
                {
                    return; 
                }
                
                float eitrLeechAmount = hit.m_damage.GetTotalDamage() * eitrleechMultiplier;
                
                attacker.AddEitr(eitrLeechAmount);
            }
            catch (Exception e)
            {
                EpicLoot.LogError(e.Message);
            }
        }
    }
}