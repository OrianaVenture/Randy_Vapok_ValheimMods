using HarmonyLib;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;
using static Attack;
using Object = UnityEngine.Object;

namespace EpicLoot.MagicItemEffects
{
    [HarmonyPatch(typeof(Attack))]
    public static class ExplodingArrow_Patch
    {
        //[HarmonyEmitIL("./dumps")]
        //[HarmonyDebug]
        [HarmonyTranspiler]
        [HarmonyPatch(nameof(Attack.FireProjectileBurst))]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions /*, ILGenerator generator*/)
        {
            var codeMatcher = new CodeMatcher(instructions);
            codeMatcher.MatchStartForward(
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(Attack), nameof(Attack.m_weapon))),
                new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Stfld, AccessTools.Field(typeof(ItemDrop.ItemData), nameof(ItemDrop.ItemData.m_lastProjectile)))
                ).Advance(3).InsertAndAdvance(
                new CodeInstruction(OpCodes.Ldloc_S, (byte)20),
                new CodeInstruction(OpCodes.Ldarg_0),
                Transpilers.EmitDelegate(UpdateProjectileHit)
                ).ThrowIfNotMatch("Unable to patch Exploding Arrows AOE.");
            return codeMatcher.Instructions();
        }

        private static void UpdateProjectileHit(GameObject shot, Attack instance)
        {
            //EpicLoot.Log($"Checking to set exploding arrow");
            if (Player.m_localPlayer != null && instance.m_character == Player.m_localPlayer && Player.m_localPlayer.HasActiveMagicEffect(MagicEffectType.ExplosiveArrows, out float effectValue, 0.01f))
            {
                //EpicLoot.Log($"Exploding Arrow set for projectile: {effectValue} on {shot.gameObject.name}");
                shot.GetComponent<Projectile>()?.m_nview.GetZDO().Set("el-aw", effectValue);
            }
        }
    }


    [HarmonyPatch(typeof(Projectile), nameof(Projectile.Awake))]
    public class RPC_ExplodingArrow_Projectile_Awake_Patch
    {
        [UsedImplicitly]
        private static void Postfix(Projectile __instance)
        {
            __instance.m_nview.Register<Vector3, float>("el-aw", RPC_ExplodingArrow);
        }

        private static void RPC_ExplodingArrow(long sender, Vector3 position, float explodingArrowStrength)
        {
            var poisonPrefab = ZNetScene.instance.GetPrefab("vfx_blob_attack");
            var poisonCloud = Object.Instantiate(poisonPrefab, position, Quaternion.identity);
            var particles = poisonCloud.transform.Find("particles");
            var cloudParticles = particles.Find("ooz (1)").GetComponent<ParticleSystem>();
            var main = cloudParticles.main;
            main.startColor = new Color(0.9f, 0.3f, 0, 0.5f);
            main.simulationSpeed = 7;
            var splashParticles = particles.Find("wetsplsh").GetComponent<ParticleSystem>();
            main = splashParticles.main;
            main.startColor = new Color(1, 0.14f, 0.1f, 1);
            main.simulationSpeed = 3;

            var characters = new List<Character>();
            Character.GetCharactersInRange(poisonCloud.transform.localPosition, 4f, characters);
            foreach (var c in characters)
            {
                if (!c.IsOwner() || (c.IsPlayer() && !c.IsPVPEnabled()))
                {
                    continue;
                }

                var fireHit = new HitData { m_damage = { m_fire = explodingArrowStrength } };
                c.Damage(fireHit);
            }
        }
    }

    [HarmonyPatch(typeof(Projectile), nameof(Projectile.OnHit))]
    public class ExplodingArrowHit_Projectile_OnHit_Patch
    {
        [UsedImplicitly]
        private static void Prefix(out Tuple<bool, bool> __state, Projectile __instance)
        {
            __state = new Tuple<bool, bool>(__instance.m_stayAfterHitStatic, __instance.m_stayAfterHitDynamic);
            __instance.m_stayAfterHitStatic = true;
            __instance.m_stayAfterHitDynamic = true;
            //EpicLoot.Log($"Exploding Arrow onhit with static hit state: {__state.Item1}  dynamic hit state: {__state.Item2}");
        }

        [UsedImplicitly]
        private static void Postfix(Tuple<bool, bool> __state, Vector3 hitPoint, Projectile __instance)
        {
            //EpicLoot.Log($"Exploding Arrow hit at {hitPoint} with state {__state}");
            if (__instance == null || __instance.m_nview == null || __instance.m_nview.GetZDO() == null)
                return;

            if (__instance.m_didHit)
            {
                var explodingArrow = __instance.m_nview.GetZDO().GetFloat("el-aw", float.NaN);
                //EpicLoot.Log($"Exploding Arrow hit with strength {explodingArrow}");
                if (!float.IsNaN(explodingArrow))
                {
                    var explodingArrowStrength = explodingArrow * __instance.m_damage.GetTotalDamage();
                    __instance.m_nview.InvokeRPC(ZRoutedRpc.Everybody, "el-aw", hitPoint, explodingArrowStrength);
                }

                if (__state.Item1 || __state.Item2)
                {
                    ZNetScene.instance.Destroy(__instance.gameObject);
                }
            }

            __instance.m_stayAfterHitStatic = __state.Item1;
            __instance.m_stayAfterHitDynamic = __state.Item2;
        }
    }
}