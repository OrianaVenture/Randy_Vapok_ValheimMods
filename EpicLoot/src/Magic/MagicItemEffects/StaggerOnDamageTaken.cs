using HarmonyLib;
using JetBrains.Annotations;
using Random = UnityEngine.Random;

namespace EpicLoot.MagicItemEffects
{
    [HarmonyPatch(typeof(Character), nameof(Character.RPC_Damage))]
    public class RPC_StaggerOnDamageTaken_Character_RPC_Damage_Patch
    {
        [UsedImplicitly]
        private static void Postfix(Character __instance, HitData hit)
        {
            if (hit == null || __instance == null)
            {
                return;
            }

            var attacker = hit.GetAttacker();

            if (__instance is Player player &&
                    attacker != null && attacker != __instance && !attacker.IsStaggering() &&
                    player.HasActiveMagicEffect(MagicEffectType.StaggerOnDamageTaken, out float effectValue, 0.01f))
            {
                // Don't stagger friendly players, only PvP enabled ones
                if (attacker.IsPVPEnabled() == false)
                {
                    return;
                }
                if (Random.Range(0f, 1f) < effectValue)
                {
                    attacker.Stagger(-attacker.transform.forward);
                }
            }
        }
    }
}