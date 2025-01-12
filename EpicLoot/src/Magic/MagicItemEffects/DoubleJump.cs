using HarmonyLib;
using UnityEngine;

namespace EpicLoot.MagicItemEffects
{
    [HarmonyPatch]
    public static class DoubleJump
    {
        // This can use the magic effect system to track more charges in the future
        public static int multi_jump_combo = 0;

        [HarmonyPatch(typeof(Character), nameof(Character.Jump))]
        public static class Character_Jump_Patch
        {
            public static bool Prefix(Character __instance)
            {
                if (__instance.IsPlayer())
                {
                    if (Player.m_localPlayer == null)
                    {
                        // skip to normal jumping
                        return true;
                    }
                    if (__instance.IsOnGround()) {
                        multi_jump_combo = 0;
                    } else {
                        multi_jump_combo++;
                    }
                    // You can't keep jumping
                    if (multi_jump_combo >= Player.m_localPlayer.GetTotalActiveMagicEffectValue(MagicEffectType.DoubleJump))
                    {
                        return false;
                    } else {
                        MultiJump(__instance);
                        // skip the original jump, since we already multi-jumped
                        return false;
                    }

                }
                return true;
            }
        }

        public static void MultiJump(Character player)
        {
            if (player.IsEncumbered() || player.InDodge() || player.IsKnockedBack() || player.IsStaggering())
            {
                return;
            }

            if (!player.HaveStamina(player.m_jumpStaminaUsage))
            {
                Hud.instance.StaminaBarEmptyFlash();
                return;
            }

            float speed = player.m_speed;
            player.m_seman.ApplyStatusEffectSpeedMods(ref speed, player.m_currentVel);
            float player_jum_skill_factor = 0f;
            Skills skills = player.GetSkills();
            if (skills != null)
            {
                player_jum_skill_factor = skills.GetSkillFactor(Skills.SkillType.Jump);
                player.RaiseSkill(Skills.SkillType.Jump);
            }

            Vector3 jump = player.m_body.velocity;
            Vector3 player_vector_up = (player.gameObject.transform.position + Vector3.up).normalized;
            float player_skill_jump_with_offset = 1f + player_jum_skill_factor * 0.4f;
            float full_player_jumpforce = player.m_jumpForce * player_skill_jump_with_offset;
            float player_normal_direction_velocity_travel = Vector3.Dot(player_vector_up, jump);
            if (player_normal_direction_velocity_travel < full_player_jumpforce)
            {
                jump += player_vector_up * (full_player_jumpforce - player_normal_direction_velocity_travel);
            }

            jump += player.m_moveDir * player.m_jumpForceForward * player_skill_jump_with_offset;

            player.m_seman.ApplyStatusEffectJumpMods(ref jump);
            if (!(jump.x <= 0f) || !(jump.y <= 0f) || !(jump.z <= 0f))
            {
                player.ForceJump(jump);
            }
        }
    }
}
