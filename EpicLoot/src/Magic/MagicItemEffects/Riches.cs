using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;

namespace EpicLoot.MagicItemEffects
{
    [HarmonyPatch(typeof(CharacterDrop), nameof(CharacterDrop.GenerateDropList))]
    public static class Riches_CharacterDrop_GenerateDropList_Patch
    {
        private static float richesValue = 0f;
        private static float lastUpdateCheck = 0;
        public static readonly Dictionary<GameObject, int> DefaultRichesTable = new Dictionary<GameObject, int> {
            { ObjectDB.instance.GetItemPrefab("SilverNecklace"), 30 },
            { ObjectDB.instance.GetItemPrefab("Ruby"), 20 },
            { ObjectDB.instance.GetItemPrefab("AmberPearl"), 10 },
            { ObjectDB.instance.GetItemPrefab("Amber"), 5 },
            { ObjectDB.instance.GetItemPrefab("Coins"), 1 },
        };

        public static Dictionary<GameObject, int> RichesTable = DefaultRichesTable;

        public static void UpdateRichesOnEffectSetup() {
            EpicLoot.Log("Updating riches table.");
            if (MagicItemEffectDefinitions.AllDefinitions.Count > 0 && MagicItemEffectDefinitions.AllDefinitions.ContainsKey(MagicEffectType.Riches)) {
                var richesConfig = MagicItemEffectDefinitions.AllDefinitions[MagicEffectType.Riches].Config;
                if (richesConfig != null && richesConfig.Count > 0) {
                    UpdateRichesTable(richesConfig);
                }
            }
        }

        public static void UpdateRichesTable(Dictionary<string, float> config) {
            Dictionary<GameObject, int> newRichesTable = new Dictionary<GameObject, int>();
            foreach (KeyValuePair<string, float> kv in config) {
                EpicLoot.Log($"Riches checking config item: {kv.Key} with value {kv.Value}");
                if (ObjectDB.instance.TryGetItemPrefab(kv.Key, out GameObject itemPrefab)) {
                    newRichesTable.Add(itemPrefab, Mathf.RoundToInt(kv.Value));
                }
            }
            if (newRichesTable.Count == 0) {
                EpicLoot.LogWarning($"Riches table is empty after update, using default riches table.");
                return;
            }

            RichesTable.Clear();
            RichesTable = newRichesTable;

            if (RichesTable.Count == 0) {
                RichesTable = DefaultRichesTable;
            }
        }

        [UsedImplicitly]
        private static void Postfix(CharacterDrop __instance, ref List<KeyValuePair<GameObject, int>> __result)
        {
            // Only do network updates for riches every minute
            if (lastUpdateCheck < Time.time) {
                EpicLoot.Log($"{lastUpdateCheck} < {Time.time}");
                lastUpdateCheck = Time.time + 60f;

                var playerList = new List<Player>();
                Player.GetPlayersInRange(__instance.m_character.transform.position, 100f, playerList);

                richesValue = playerList.Sum(player => player.m_nview.GetZDO().GetInt("el-rch")) * 0.01f;
            }
            // No riches present in the area, so nothing to do.
            if (richesValue <= 0) {
                return;
            }
            var richesRandomRoll = Random.Range(0f, 1f);

            if (richesValue > 1) {
                richesRandomRoll = Mathf.RoundToInt(richesRandomRoll * richesValue);
            }

            float richesActivateRoll = Random.Range(0f, 1f);
            EpicLoot.Log($"Riches roll amount: {richesActivateRoll} < {richesRandomRoll}");
            if (richesActivateRoll < richesRandomRoll) {

                // Randomly select _one_ loot item from the list, scale it based on the riches value, and add it to the drop list
                int selected = Random.Range(0, RichesTable.Count()-1);
                int amount = Mathf.RoundToInt(richesRandomRoll * 100 / RichesTable[RichesTable.Keys.ElementAt(selected)]);

                if (amount >= 1) {
                    __result.Add(new KeyValuePair<GameObject, int>(RichesTable.Keys.ElementAt(selected), amount));
                }
            }
        }
    }
}