using HarmonyLib;
using Jotunn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EpicLoot.MagicItemEffects
{

    [HarmonyPatch]

    [HarmonyPatch(typeof(Attack), nameof(Attack.DoMeleeAttack))]

    public class OffSetAttack
    {
        private static readonly Dictionary<Player, float> ActiveOffSetTimers = new();
        private static readonly HashSet<Player> ActiveOffSetPlayers = new();
        private const float StatggerImmunityandDamageReductionDuration = .5f; // Seconds of stagger immunity and damage reduction
        private const float DamageReductionMultiplier = 0.2f; // Place holder scaled on buff effect below

        static void Postfix(Attack __instance)
        {

            if (Player.m_localPlayer.GetTotalActiveMagicEffectValue(MagicEffectType.OffSetAttack, 1f) == 0) return;

            // vanilla Valheim Hit detection
            Transform transform;
            Vector3 direction;
            __instance.GetMeleeAttackDir(out transform, out direction);

            Vector3 localDirection = __instance.m_character.transform.InverseTransformDirection(direction);
            float halfAngle = __instance.m_attackAngle / 2f;
            float stepSize = 4f;
            float attackRange = __instance.m_attackRange;

            Vector3 attackOrigin = transform.position + Vector3.up * __instance.m_attackHeight
                + __instance.m_character.transform.right * __instance.m_attackOffset;

            int layerMask = __instance.m_hitTerrain ? Attack.m_attackMaskTerrain : Attack.m_attackMask;

            for (float angle = -halfAngle; angle <= halfAngle; angle += stepSize)
            {
                Quaternion swingRotation = Quaternion.identity;
                if (__instance.m_attackType == Attack.AttackType.Horizontal)
                    swingRotation = Quaternion.Euler(0f, -angle, 0f);
                else if (__instance.m_attackType == Attack.AttackType.Vertical)
                    swingRotation = Quaternion.Euler(angle, 0f, 0f);

                Vector3 finalDir = __instance.m_character.transform.TransformDirection(swingRotation * localDirection);

                RaycastHit[] hits = (__instance.m_attackRayWidth > 0f)
                    ? Physics.SphereCastAll(attackOrigin, __instance.m_attackRayWidth, finalDir, Mathf.Max(0f, attackRange - __instance.m_attackRayWidth), layerMask, QueryTriggerInteraction.Ignore)
                    : Physics.RaycastAll(attackOrigin, finalDir, attackRange, layerMask, QueryTriggerInteraction.Ignore);

                foreach (RaycastHit hit in hits)
                {
                    GameObject targetGO = Projectile.FindHitObject(hit.collider);
                    Character hitCharacter = targetGO.GetComponent<Character>();

                    bool MeleeHit = hitCharacter != null &&
                                  hitCharacter == Player.m_localPlayer;

                    Player player = hitCharacter as Player;
                    if (player == null) return;
                    if (player != MeleeHit) return;
                    ActiveOffSetPlayers.Add(player);
                }
            }
        }
        [HarmonyPatch(typeof(Attack), nameof(Attack.OnAttackTrigger))]
        [HarmonyPrefix]
        public static bool Prefix(Attack __instance)
        {
            var attacker = __instance.m_character;

            if (attacker is Player player)
            {
                if (__instance.m_currentAttackCainLevel == 2)
                {
                    ActiveOffSetTimers[player] = Time.time + StatggerImmunityandDamageReductionDuration;
                }
                return true;
            }
            return true;
        } // Chain attack level detection for OffSetAttack

        [HarmonyPatch(typeof(Character), nameof(Character.Damage))]
        private static void Prefix(Character __instance, HitData hit)
        {
            if (Player.m_localPlayer == null) return;

            float offSetValue = Player.m_localPlayer.GetTotalActiveMagicEffectValue(MagicEffectType.OffSetAttack, .01f);

            if (offSetValue == 0f) return;

            float DamageReductionMultiplier = 1 - offSetValue;

            var attacker = hit.GetAttacker();
            float now = Time.time;

            // Reduce damage to player during OffSetAttack window
            if (__instance is Player player && (ActiveOffSetPlayers.Contains(player)) && ActiveOffSetTimers.TryGetValue(player, out float endTime) && now <= endTime)
            {
                hit.m_damage.Modify(DamageReductionMultiplier); // DR
                hit.m_pushForce = 0f; // knock back immunity
                hit.m_staggerMultiplier = 0f;
                AudioSource.PlayClipAtPoint(EpicLoot.Assets.OffSetSFX, player.transform.position);
            }

            foreach (var kvp in new List<KeyValuePair<Player, float>>(ActiveOffSetTimers))
            {
                if (now > kvp.Value)
                    ActiveOffSetTimers.Remove(kvp.Key);
            }

        }

    }
}