using EpicLoot.Abilities;
using HarmonyLib;
using Jotunn.Entities;
using Jotunn.Managers;
using System;
using System.Collections.Generic;
using System.Security.Policy;
using UnityEngine;

namespace EpicLoot.MagicItemEffects

{

    public class WorldUtils
    {
        public static List<Character> GetAllCharacter(Vector3 position, float range)
        {
            Collider[] hits = Physics.OverlapBox(position, Vector3.one * range, Quaternion.identity);
            List<Character> characters = new List<Character>();

            foreach (var hit in hits)
            {
                var npc = hit.transform.root.gameObject.GetComponentInChildren<Character>();
                if (npc != null)
                {
                    characters.Add(npc);
                }
            }

            return characters;
        }
    }

    public class StatusUtils
    {
        public void CreateMyStatusEffect()
        {
            SE_Stats myStatusEffect = ScriptableObject.CreateInstance<SE_Stats>();

            Sprite iconSprite = ObjectDB.instance.GetStatusEffect("Rested".GetStableHashCode()).m_icon;

            myStatusEffect.name = "Fortify";
            myStatusEffect.m_name = "Fortifying";
            myStatusEffect.m_tooltip = "Reduced damage taken";
            myStatusEffect.m_icon = iconSprite;
            myStatusEffect.m_ttl = 10f;
            myStatusEffect.m_staminaRegenMultiplier = 20f;
            myStatusEffect.m_speedModifier = 3f;

            ObjectDB.instance.m_StatusEffects.Add(myStatusEffect);

            CustomStatusEffect Fortify = new CustomStatusEffect(myStatusEffect, fixReference: false);

            ItemManager.Instance.AddStatusEffect(Fortify);
        }
    }

    public class AuraBobAndRotate : MonoBehaviour
    {
        public float bobAmplitude = 0.1f;      // Height of bobbing
        public float bobFrequency = 2f;        // Speed of bobbing
        public float rotationSpeed = 45f;      // Degrees per second

        private Vector3 initialPosition;

        void Start()
        {
            initialPosition = transform.localPosition;
        }

        void Update()
        {
            float newY = initialPosition.y + Mathf.Sin(Time.time * bobFrequency) * bobAmplitude;
            transform.localPosition = new Vector3(initialPosition.x, newY, initialPosition.z);

            //rotate on ankles/feet
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
        }

    }

    [HarmonyPatch(typeof(Player), nameof(Player.Update))]
    public class FortifyAura
    {
        private static float auraTimer = 0f;
        private const float AuraIntereval = .2f;
        private const float AuraRange = 10f;

        static void Postfix(Player __instance)
        {
            if (ZNetScene.instance != null)
            {
                auraTimer += Time.deltaTime;
                if (auraTimer > AuraIntereval)
                {
                    auraTimer = 0f; //Reset timer so interval can check again
                    Vector3 playerPostion = __instance.transform.position;
                    List<Character> characters = WorldUtils.GetAllCharacter(playerPostion, AuraRange);
                    List<Character> allies = WorldUtils.GetAllCharacter(playerPostion, AuraRange);

                    allies.Clear();
                    for (int i = 0; i < characters.Count; i++)
                    {
                        var character = characters[i];
                        if (character.IsPlayer())
                        {
                            allies.Add(character);
                        }
                    }

                    List<Character> enemies = WorldUtils.GetAllCharacter(playerPostion, AuraRange);

                    for (int i = 0; i < characters.Count; i++)
                    {
                        var character = characters[i];
                        if (!character.IsPlayer())
                        {
                            enemies.Add(character);
                        }
                    }

                    int rushHash = "Fortify".GetStableHashCode();

                    bool anyHasFortifyAura = false;
                    for (int i = 0; i < allies.Count; i++)
                    {
                        var allyPlayer = allies[i] as Player;
                        if (allyPlayer != null && allyPlayer.GetTotalActiveMagicEffectValue(MagicEffectType.FortifyAura, 1f) > 0f)
                        {
                            anyHasFortifyAura = true;
                            break;
                        }
                    }

                    if (anyHasFortifyAura)
                    {
                        for (int i = 0; i < allies.Count; i++)
                        {
                            var ally = allies[i];
                            var seMan = ally.GetSEMan();
                            if (seMan == null) continue;

                            var effect = seMan.GetStatusEffect(rushHash);
                            if (effect != null)
                            {
                                float remaining = effect.m_ttl - effect.m_time;
                                if (remaining < 8f)
                                {
                                    seMan.RemoveStatusEffect(rushHash, true);
                                    seMan.AddStatusEffect(rushHash);
                                }
                            }
                            else
                            {
                                seMan.AddStatusEffect(rushHash);
                                //var auraPos = Player.m_localPlayer.transform.position;
                                //GameObject starInstance = UnityEngine.Object.Instantiate(EpicLoot.Aurafive, auraPos, Quaternion.Euler(90, 0, 0));
                                //starInstance.transform.SetParent(Player.m_localPlayer.transform);
                                //starInstance.transform.localPosition = new Vector3(0f, 0.2f, 0f); // effect above ankles to not clip into ground
                                //starInstance.AddComponent<AuraBobAndRotate>();
                            }


                        }
                    }

                }
            }
        }
    }
}


