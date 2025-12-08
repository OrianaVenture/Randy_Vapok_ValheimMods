using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace EpicLoot.Magic.MagicItemEffects
{
    public static class Headhunter
    {
        [HarmonyPatch(typeof(CharacterDrop), nameof(CharacterDrop.GenerateDropList))]
        public static class IncreaseTrophyDropChance
        {
            private static void Postfix(CharacterDrop __instance)
            {
                if (Player.m_localPlayer != null &&
                Player.m_localPlayer.HasActiveMagicEffect(MagicEffectType.HeadHunter, out float effectValue, 0.01f))
                {
                    bool addTrophy = false;
                    GameObject trophyGo = null;
                    EpicLoot.Log("Player Headhunter checking for creature trophy drops");

                    foreach (var drop in __instance.m_drops)
                    {
                        if (drop.m_prefab != null && drop.m_prefab.name.Contains("Trophy"))
                        {
                            trophyGo = drop.m_prefab;
                            // Roll a chance to add this to the drop list
                            float randomv = Random.Range(0f, 1f);
                            EpicLoot.Log($"Checking chance to drop additional trophy {randomv} < {effectValue} {randomv < effectValue}");

                            if (randomv < effectValue)
                            {
                                // Increase the drop amount by 1
                                EpicLoot.Log("Adding trophy drop");
                                addTrophy = true;
                            }

                            break;
                        }
                    }
                    
                    if (addTrophy)
                    {
                        Vector3 iUS = UnityEngine.Random.insideUnitSphere;
                        if (iUS.y < 0f)
                        {
                            iUS.y = 0f - iUS.y;
                        }

                        // Drop a trophy, this happens outside of the drop system because otherwise things like DropThat will filter it out or prevent it.
                        GameObject go = GameObject.Instantiate(trophyGo,
                            (__instance.transform.position + Vector3.up * 0.5f),
                            Quaternion.Euler(0f, UnityEngine.Random.Range(0, 360), 0f));
                        Rigidbody rb = go.GetComponent<Rigidbody>();
                        rb.AddForce(iUS * 5f, ForceMode.VelocityChange);
                        //__result.Add(new KeyValuePair<GameObject, int>(trophyGo, 1));
                    }
                }
            }
        }
    }
}
