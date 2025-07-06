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
        private const float HyperArmorDuration = .5f; // Seconds of hyper armor window
        private const float DamageReductionMultiplier = 0.2f; // Place holder scaled on buff effect below
        private const float StaggerBonusMultiplier = 1f; // Basline overridden by magic effect

        static void Postfix(Attack __instance)
        {

            if (__instance.m_character == Player.m_localPlayer)
                return;


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

                    bool wasMeleeHit = hitCharacter != null &&
                                  hitCharacter == Player.m_localPlayer;

                    Player player = hitCharacter as Player;
                    if (player != null &&
                        wasMeleeHit)
                    {
                        ActiveOffSetPlayers.Add(player);
                        Debug.Log("Local Player wasMeleeHit");
                    }
                }
            }
        } // Hit Detection
        [HarmonyPatch(typeof(Attack), nameof(Attack.OnAttackTrigger))]
        [HarmonyPrefix]
        public static bool Prefix(Attack __instance)
        {
            var attacker = __instance.m_character;

            Debug.Log($"[OnAttackTrigger] Attacker: {attacker?.GetType().Name}, Name: {attacker?.name}, CainLevel: {__instance.m_currentAttackCainLevel}");

            if (attacker is Player player)
            {
                if (__instance.m_currentAttackCainLevel == 2)
                {
                    ActiveOffSetTimers[player] = Time.time + HyperArmorDuration;
                    Debug.Log($"OffSetAttack registered for {player.GetPlayerName()}");
                }
                return true;
            }
            return true;
        } // Chain attack level detection for OffSetAttack

        [HarmonyPatch(typeof(Character), nameof(Character.Damage))]
        private static void Prefix(Character __instance, HitData hit) 
        {
            if (Player.m_localPlayer != null)
            {
                float offsetValue = Player.m_localPlayer.GetTotalActiveMagicEffectValue(MagicEffectType.OffSetAttack, 1f);

                if (offsetValue > 0f)
                {
                    float StaggerBonusMultiplier = offsetValue * 0.01f;
                    float t = Mathf.Clamp01(Mathf.InverseLerp(150f, 300f, offsetValue));
                    float DamageReductionMultiplier = Mathf.Lerp(0.5f, 0.2f, t);

                    var attacker = hit.GetAttacker();
                    float now = Time.time;

                    // Reduce damage to player during OffSetAttack window
                    if (__instance is Player player && (ActiveOffSetPlayers.Contains(player)) && ActiveOffSetTimers.TryGetValue(player, out float endTime) && now <= endTime)
                    {
                        hit.m_damage.Modify(DamageReductionMultiplier); // DR
                        hit.m_pushForce = 0f; // knock back immunity
                        hit.m_staggerMultiplier = 0f; // stagger immunity
                        Debug.Log("OffSet DR active — damage reduced by 80%.");
                        Debug.Log($"[DR Check] __instance: {__instance.GetType().Name}, attacker: {hit.GetAttacker()?.GetType().Name}");
                    }

                    // Apply stagger bonus to enemies hit by a player in OffSet window
                    if (attacker is Player p && (ActiveOffSetPlayers.Contains(p)) && ActiveOffSetTimers.TryGetValue(p, out float pEndTime) && now <= pEndTime && __instance != p)
                    {
                        hit.m_staggerMultiplier *= StaggerBonusMultiplier;
                        //AudioSource.PlayClipAtPoint(EpicLoot.Assets.OffSetSFX, p.transform.position); I CANT GET THE EPICLOOT ASSETBUNDLE TO BUILD PROPERLY
                        Debug.Log("OffSet stagger bonus applied.");
                    }

                    foreach (var kvp in new List<KeyValuePair<Player, float>>(ActiveOffSetTimers))
                    {
                        if (now > kvp.Value)
                            ActiveOffSetTimers.Remove(kvp.Key);
                    }
                }
            }
        } // Damage calc
    }
}
