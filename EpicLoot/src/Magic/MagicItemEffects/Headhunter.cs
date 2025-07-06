using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace EpicLoot.src.Magic.MagicItemEffects
{
    public static class Headhunter
    {
        [HarmonyPatch(typeof(CharacterDrop), nameof(CharacterDrop.GenerateDropList))]
        public static class IncreaseTrophyDropChance
        {
            private static void Postfix(CharacterDrop __instance, List<KeyValuePair<GameObject, int>> __result)
            {
                if (Player.m_localPlayer != null && Player.m_localPlayer.HasActiveMagicEffect(MagicEffectType.HeadHunter, out float effectValue, 0.01f)) {
                    // check if we are already dropping a trophy
                    bool addTrophy = false;
                    GameObject trophyGo = null;
                    foreach (var drop in __result) {
                        if (drop.Key != null && drop.Key.name.EndsWith("Trophy")) {
                            // If we are already dropping a trophy, roll a chance to increase the drop amount
                            if (Random.Range(0f, 1f) < effectValue) {
                                // Increase the drop amount by 1
                                trophyGo = drop.Key;
                                addTrophy = true;
                            }
                            break;
                        }
                    }

                    // Roll to add a trophy if one isn't already dropping
                    if (addTrophy == false) {
                        foreach (var drop in __instance.m_drops) {
                            if (drop.m_prefab != null && drop.m_prefab.name.EndsWith("Trophy")) {
                                trophyGo = drop.m_prefab;
                                // Roll a chance to add this to the drop list
                                if (Random.Range(0f, 1f) < effectValue) {
                                    // Increase the drop amount by 1
                                    addTrophy = true;
                                }

                                break;
                            }
                        }
                    }
                    
                    if (addTrophy) {
                        __result.Add(new KeyValuePair<GameObject, int>(trophyGo, 1));
                    }
                    
                }
            }
        }
    }
}
