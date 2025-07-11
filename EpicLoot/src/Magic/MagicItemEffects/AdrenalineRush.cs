using HarmonyLib;
using Jotunn.Entities;
using Jotunn.Managers;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace EpicLoot.MagicItemEffects
{
    [HarmonyPatch(typeof(Attack), nameof(Attack.DoMeleeAttack))]
    public class DodgeDetectionandBuffApplicationPatch
    {
        public static bool DodgeWasDetected = false;

        static void Postfix(Attack __instance)
        {
            DodgeWasDetected = false;

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

                    bool dodgedHit = hitCharacter != null &&
                                     hitCharacter == Player.m_localPlayer &&
                                     hitCharacter.IsDodgeInvincible();

                    Player player = hitCharacter as Player;
                    if (player != null)
                    {
                        int rushHash = "Adrenaline_Rush".GetStableHashCode();

                        if (player.GetSEMan().GetStatusEffect(rushHash) == null &&
                            player.GetTotalActiveMagicEffectValue(MagicEffectType.DodgeBuff, 1f) > 0f &&
                            dodgedHit)
                        {
                            player.GetSEMan().AddStatusEffect(rushHash);
                            AudioSource.PlayClipAtPoint(EpicLoot.Assets.DoubleJumpSFX, player.transform.position);
                            Jotunn.Logger.LogInfo("Perfect Dodge detected. Adrenaline Rush applied.");
                            return;
                        }
                    }

                }
            }
        }
    }

    public class StatusEffects_Utils_DodgeBuff 
    {
        public void CreateMyStatusEffect()
        {
            SE_Stats myStatusEffect = ScriptableObject.CreateInstance<SE_Stats>(); // create new instance of se_stats

            Sprite iconSprite = ObjectDB.instance.GetStatusEffect("Rested".GetStableHashCode()).m_icon;

            //fill out fields in se_stats to make the status effect I want
            myStatusEffect.name = "Adrenaline_Rush";
            myStatusEffect.m_name = "Adrenaline Rush";
            myStatusEffect.m_tooltip = "Increased damage for the duration of the effect.";
            myStatusEffect.m_icon = iconSprite;
            myStatusEffect.m_ttl = 20f;

            ObjectDB.instance.m_StatusEffects.Add(myStatusEffect);

            //Instantiate the effect in code
            CustomStatusEffect Adrenaline_Rush = new CustomStatusEffect(myStatusEffect, fixReference: false);
            //CustomStatusEffect MyTestBuff = new CustomStatusEffect(testEffect, fixReference: false);
            //Register the status effect into the game
            ItemManager.Instance.AddStatusEffect(Adrenaline_Rush);
            //ItemManager.Instance.AddStatusEffect(MyTestBuff);
        }

        // Called in epic loot status effect thingy to add to list of status effects in game.

        // use jottun custom status effect to create a jottun status effect 

        // use jottun add custom status effect to add to game

        // if stuck look at monster modifiers 

        // build add to game  and use dev command to add status effect to player


    }
}

// ITS FUCKING WORKS!