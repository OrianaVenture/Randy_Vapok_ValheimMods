using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;

namespace EpicLoot.MagicItemEffects;

public class DecreaseForsakenCooldown
{
    [HarmonyPatch(typeof(Player), nameof(Player.ActivateGuardianPower))]
    private class DecreaseForsakenCooldown_Player_ActivateGuardianPower_Patch
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructionsEnumerable)
        {
            var instructions = instructionsEnumerable.ToList();
            int targetIndex = instructions.Count - 2;

            if (targetIndex >= 0 && instructions[targetIndex].opcode == OpCodes.Ldc_I4_0)
            {
                instructions[targetIndex].opcode = OpCodes.Ldc_I4_1;
            }

            return instructions;
        }

        private static void Postfix(Player __instance, ref bool __result)
        {
            if (__result)
            {
                float magicEffectValue =
                    __instance.GetTotalActiveMagicEffectValue(MagicEffectType.DecreaseForsakenCooldown) / 100f;
                
                __instance.m_guardianPowerCooldown *= 1 - magicEffectValue;
            }
        }
    }
}