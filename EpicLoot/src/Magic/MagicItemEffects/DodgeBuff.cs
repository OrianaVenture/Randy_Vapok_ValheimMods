using HarmonyLib;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace EpicLoot.MagicItemEffects;

public static class DodgeBuff
{
    [HarmonyPatch(typeof(Attack), nameof(Attack.DoMeleeAttack))]
    private static class DodgeBuff_Attack_DoMeleeAttack_Patch
    {
        public static bool DodgeWasDetected = false;
        static int rushHash = "Adrenaline_Rush".GetStableHashCode();

        private static void Postfix(Attack __instance)
        {
            if (Player.m_localPlayer.GetTotalActiveMagicEffectValue(MagicEffectType.DodgeBuff, 1f) == 0f) return;

            //copied hit detection from Valheim code
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
                    ? Physics.SphereCastAll(attackOrigin, __instance.m_attackRayWidth, finalDir,
                    Mathf.Max(0f, attackRange - __instance.m_attackRayWidth), layerMask, QueryTriggerInteraction.Ignore)
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
                        if (player.GetSEMan().GetStatusEffect(rushHash) == null &&
                            player.GetTotalActiveMagicEffectValue(MagicEffectType.DodgeBuff, 1f) > 0f &&
                            dodgedHit)
                        {
                            player.GetSEMan().AddStatusEffect(rushHash);
                            AudioSource.PlayClipAtPoint(EpicLoot.Assets.DodgeBuffSFX, player.transform.position);
                            return;
                        }
                    }
                }
            }
        }
    }

    public static void CreateMyStatusEffect()
    {
        SE_Stats myStatusEffect = ScriptableObject.CreateInstance<SE_Stats>(); // create new instance of se_stats

        Sprite iconSprite = EpicLoot.Assets.DodgeBuffSprite;

        //fill out fields in se_stats to make the status effect I want
        myStatusEffect.name = "Adrenaline_Rush";
        myStatusEffect.m_name = Localization.instance.Localize("$mod_epicloot_me_adrenaline_rush");
        myStatusEffect.m_tooltip = Localization.instance.Localize("$mod_epicloot_me_adrenaline_rush_desc");
        myStatusEffect.m_icon = iconSprite;
        myStatusEffect.m_ttl = 10f;

        //Instantiate the effect in code
        CustomStatusEffect Adrenaline_Rush = new CustomStatusEffect(myStatusEffect, fixReference: false);
        //Register the status effect into the game
        ItemManager.Instance.AddStatusEffect(Adrenaline_Rush);
    }
}