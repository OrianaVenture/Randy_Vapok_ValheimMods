using HarmonyLib;

namespace EpicLoot.MagicItemEffects
{
    [HarmonyPatch(typeof(Attack), nameof(Attack.GetAttackHealth))]
    public class ModifyAttackHealth_Attack_GetAttackHealth_Patch
    {
        public static bool Prefix(Attack __instance, ref float __result)
        {
            if (__instance.m_character is Player player)
            {
                float modifier = MagicEffectsHelper.GetTotalActiveMagicEffectValueForWeapon(player, __instance.m_weapon, MagicEffectType.ModifyAttackHealthUse, 0.01f);
                float newAttackHealthPercentage = __instance.m_attackHealthPercentage * (1 - modifier);
        
                float newAttackHealth = __instance.m_attackHealth + __instance.m_character.GetHealth() * newAttackHealthPercentage / 100f;
                float skillFactor = __instance.m_character.GetSkillFactor(__instance.m_weapon.m_shared.m_skillType);
                __result = newAttackHealth - newAttackHealth * 0.33f * skillFactor;
                
                return true;
            }
            
            return false;
        }
    }
}
