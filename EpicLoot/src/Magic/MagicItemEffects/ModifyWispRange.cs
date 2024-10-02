using HarmonyLib;
using UnityEngine;

namespace EpicLoot.MagicItemEffects
{
    [HarmonyPatch(typeof(SE_Demister), nameof(SE_Demister.UpdateStatusEffect))]
    public static class ModifyWispRange_SE_Demister_UpdateStatusEffect_Patch
    {
        private static float _originalEndRange = -1f;
        
        public static void Prefix(SE_Demister __instance)
        {
            if (!__instance.m_character.IsPlayer() || __instance.m_ballPrefab == null)
            {
                return;
            }
            
            var forceField = __instance.m_ballPrefab.GetComponentInChildren<ParticleSystemForceField>();
            if (forceField == null)
            {
                return;
            }
            
            if (_originalEndRange < 0)
            {
                _originalEndRange = forceField.endRange;
            }
            
            Player player = (Player)__instance.m_character;
            if (player.HasActiveMagicEffect(MagicEffectType.ModifyWispRange))
            {
                var multiplier = player.GetTotalActiveMagicEffectValue(MagicEffectType.ModifyWispRange, 0.01f);
                forceField.endRange = _originalEndRange * (1 + multiplier);
            }
            else
            {
                forceField.endRange = _originalEndRange;
            }
        }
    }
}