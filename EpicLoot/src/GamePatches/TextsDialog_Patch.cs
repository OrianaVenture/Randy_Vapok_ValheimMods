using EpicLoot.Adventure;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace EpicLoot
{
    [HarmonyPatch(typeof(TextsDialog), nameof(TextsDialog.UpdateTextsList))]
    public static class TextsDialog_UpdateTextsList_Patch
    {
        public static void Postfix(TextsDialog __instance)
        {
            Player player = Player.m_localPlayer;
            if (player == null)
            {
                return;
            }

            if (EpicLoot.HasAuga && __instance.transform.parent.name == "Lore")
            {
                return;
            }

            AddMagicEffectsPage(__instance, player);
            AddMagicEffectsExplainPage(__instance);
            AddTreasureAndBountiesPage(__instance, player);
        }

        public static void AddMagicEffectsPage(TextsDialog textsDialog, Player player)
        {
            var magicEffects = new Dictionary<string, List<KeyValuePair<MagicItemEffect, ItemDrop.ItemData>>>();

            List<ItemDrop.ItemData> allEquipment = player.GetEquipment();
            foreach (ItemDrop.ItemData item in allEquipment)
            {
                if (item.IsMagic())
                {
                    foreach (MagicItemEffect effect in item.GetMagicItem().Effects)
                    {
                        if (!magicEffects.TryGetValue(effect.EffectType,
                            out List<KeyValuePair<MagicItemEffect, ItemDrop.ItemData>> effectList))
                        {
                            effectList = new List<KeyValuePair<MagicItemEffect, ItemDrop.ItemData>>();
                            magicEffects.Add(effect.EffectType, effectList);
                        }

                        effectList.Add(new KeyValuePair<MagicItemEffect, ItemDrop.ItemData>(effect, item));
                    }
                }
            }

            StringBuilder t = new StringBuilder();

            foreach (KeyValuePair<string, List<KeyValuePair<MagicItemEffect, ItemDrop.ItemData>>> entry in magicEffects)
            {
                string effectType = entry.Key;
                MagicItemEffectDefinition effectDef = MagicItemEffectDefinitions.Get(effectType);
                float sum = entry.Value.Sum(x => x.Key.EffectValue);
                string totalEffectText = MagicItem.GetEffectText(effectDef, sum);
                ItemRarity highestRarity = (ItemRarity) entry.Value.Max(x => (int) x.Value.GetRarity());

                t.AppendLine($"<size=20><color={EpicLoot.GetRarityColor(highestRarity)}>{totalEffectText}</color></size>");
                foreach (KeyValuePair<MagicItemEffect, ItemDrop.ItemData> entry2 in entry.Value)
                {
                    MagicItemEffect effect = entry2.Key;
                    ItemDrop.ItemData item = entry2.Value;
                    t.AppendLine($" <color=#c0c0c0ff>- {MagicItem.GetEffectText(effect, item.GetRarity(), false)} ({item.GetDecoratedName()})</color>");
                }

                t.AppendLine();
            }

            textsDialog.m_texts.Insert(EpicLoot.HasAuga ? 0 : 2, 
                new TextsDialog.TextInfo(
                    Localization.instance.Localize($"{EpicLoot.GetMagicEffectPip(false)} $mod_epicloot_active_magic_effects"), 
                    Localization.instance.Localize(t.ToString())));
        }
        
        public static void AddTreasureAndBountiesPage(TextsDialog textsDialog, Player player)
        {
            StringBuilder t = new StringBuilder();

            AdventureSaveData saveData = player.GetAdventureSaveData();

            t.AppendLine("<color=orange><size=30>$mod_epicloot_merchant_treasuremaps</size></color>");
            t.AppendLine();

            IOrderedEnumerable<TreasureMapChestInfo> sortedTreasureMaps = saveData.TreasureMaps
                .Where(x => x.State == TreasureMapState.Purchased)
                .OrderBy(x => GetBiomeOrder(x.Biome));
            foreach (TreasureMapChestInfo treasureMap in sortedTreasureMaps)
            {
                t.AppendLine(Localization.instance.Localize($"$mod_epicloot_merchant_treasuremaps: " +
                    $"<color={GetBiomeColor(treasureMap.Biome)}>$biome_{treasureMap.Biome.ToString().ToLower()} " +
                    $"#{treasureMap.Interval + 1}</color>"));
            }

            t.AppendLine();
            t.AppendLine();
            t.AppendLine("<color=orange><size=30>$mod_epicloot_activebounties</size></color>");
            t.AppendLine();

            IOrderedEnumerable<BountyInfo> sortedBounties = saveData.Bounties.OrderBy(x => x.State);
            foreach (BountyInfo bounty in sortedBounties)
            {
                if (bounty.State != BountyState.InProgress && bounty.State != BountyState.Complete)
                {
                    continue;
                }

                string targetName = AdventureDataManager.GetBountyName(bounty);
                t.AppendLine($"<size=24>{targetName}</size>");
                t.Append($"  <color=#c0c0c0ff>$mod_epicloot_activebounties_classification: <color=#d66660>{AdventureDataManager.GetMonsterName(bounty.Target.MonsterID)}</color>, ");
                t.AppendLine($" $mod_epicloot_activebounties_biome: <color={GetBiomeColor(bounty.Biome)}>$biome_{bounty.Biome.ToString().ToLower()}</color></color>");

                string status = "";
                switch (bounty.State)
                {
                    case BountyState.InProgress:
                        status = ("<color=#00f0ff>$mod_epicloot_bounties_tooltip_inprogress</color>");
                        break;
                    case BountyState.Complete:
                        status = ("<color=#70f56c>$mod_epicloot_bounties_tooltip_vanquished</color>");
                        break;
                }

                t.Append($"  <color=#c0c0c0ff>$mod_epicloot_bounties_tooltip_status {status}");

                int iron = bounty.RewardIron;
                int gold = bounty.RewardGold;
                t.AppendLine($", $mod_epicloot_bounties_tooltip_rewards {(iron > 0 ? $"<color=white>{MerchantPanel.GetIronBountyTokenName()} x{iron}</color>" : "")}{(iron > 0 && gold > 0 ? ", " : "")}{(gold > 0 ? $"<color=#f5da53>{MerchantPanel.GetGoldBountyTokenName()} x{gold}</color>" : "")}</color>");
                t.AppendLine();
            }

            textsDialog.m_texts.Insert(EpicLoot.HasAuga ? 2 : 4, 
                new TextsDialog.TextInfo(
                    Localization.instance.Localize($"{EpicLoot.GetMagicEffectPip(false)} $mod_epicloot_adventure_title"), 
                    Localization.instance.Localize(t.ToString())));
        }
        
        public static string GetBiomeColor(Heightmap.Biome biome)
        {
            string biomeColor = "white";
            switch (biome)
            {
                case Heightmap.Biome.Meadows: biomeColor = "#75d966"; break;
                case Heightmap.Biome.BlackForest: biomeColor = "#72a178"; break;
                case Heightmap.Biome.Swamp: biomeColor = "#a88a6f"; break;
                case Heightmap.Biome.Mountain: biomeColor = "#a3bcd6"; break;
                case Heightmap.Biome.Plains: biomeColor = "#d6cea3"; break;
            }

            return biomeColor;
        }
        
        public static float GetBiomeOrder(Heightmap.Biome biome)
        {
            if (biome == Heightmap.Biome.BlackForest)
            {
                return 1.5f;
            }

            return (float) biome;
        }

        public static void AddMagicEffectsExplainPage(TextsDialog textsDialog)
        {
            IOrderedEnumerable<KeyValuePair<string, string>> sortedMagicEffects = MagicItemEffectDefinitions.AllDefinitions
                .Where(x => !x.Value.Requirements.NoRoll && x.Value.CanBeAugmented)
                .Select(x => new KeyValuePair<string, string>(string.Format(Localization.instance.Localize(x.Value.DisplayText), "<b><color=yellow>X</color></b>"), Localization.instance.Localize(x.Value.GetDescriptionTextWithConfig())))
                .OrderBy(x => x.Key);

            StringBuilder t = new StringBuilder();
            foreach (KeyValuePair<string, string> effectEntry in sortedMagicEffects)
            {
                t.AppendLine($"<size=24>{effectEntry.Key}</size>");
                t.AppendLine($"<color=#c0c0c0ff>{effectEntry.Value}</color>");
                t.AppendLine();
            }

            textsDialog.m_texts.Insert(EpicLoot.HasAuga ? 1 : 3,
                new TextsDialog.TextInfo(
                    Localization.instance.Localize($"{EpicLoot.GetMagicEffectPip(false)} $mod_epicloot_me_explaintitle"),
                    Localization.instance.Localize(t.ToString())));
        }
    }

    [HarmonyPatch(typeof(TextsDialog), nameof(TextsDialog.ShowText), typeof(TextsDialog.TextInfo))]
    public static class TextsDialog_ShowText_Patch
    {
        public static Transform TextContainer;
        public static TMP_Text TitleTextPrefab;
        public static TMP_Text DescriptionTextPrefab;

        public static bool Prefix(TextsDialog __instance, TextsDialog.TextInfo text)
        {
            if (EpicLoot.HasAuga)
            {
                return true;
            }

            if (TitleTextPrefab == null)
            {
                TextContainer = __instance.m_textAreaTopic.transform.parent;
                Image textContainerBackground = TextContainer.gameObject.AddComponent<Image>();
                textContainerBackground.color = new Color();
                textContainerBackground.raycastTarget = true;

                VerticalLayoutGroup verticalLayoutGroup = TextContainer.GetComponent<VerticalLayoutGroup>();
                verticalLayoutGroup.spacing = 0;

                TitleTextPrefab = Object.Instantiate(__instance.m_textAreaTopic, __instance.transform);
                TitleTextPrefab.gameObject.SetActive(false);
            }

            if (DescriptionTextPrefab == null)
            {
                DescriptionTextPrefab = Object.Instantiate(__instance.m_textArea, __instance.transform);
                DescriptionTextPrefab.gameObject.SetActive(false);
            }

            for (int i = 0; i < TextContainer.childCount; i++)
            {
                Object.Destroy(TextContainer.GetChild(i).gameObject);
            }

            TMP_Text description = Object.Instantiate(TitleTextPrefab, TextContainer);
            description.gameObject.SetActive(true);
            description.text = text.m_topic;

            string[] parts = text.m_text.Split('\n');
            foreach (string part in parts)
            {
                TMP_Text paragraphText = Object.Instantiate(DescriptionTextPrefab, TextContainer);
                paragraphText.gameObject.SetActive(true);
                paragraphText.text = part;
            }

            return false;
        }
    }
}
