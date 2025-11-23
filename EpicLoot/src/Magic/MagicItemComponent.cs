using Common;
using EpicLoot.Crafting;
using EpicLoot.Data;
using EpicLoot.LegendarySystem;
using EpicLoot.MagicItemEffects;
using HarmonyLib;
using JetBrains.Annotations;
using Jotunn.Managers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using CodeInstruction = HarmonyLib.CodeInstruction;
using Object = UnityEngine.Object;

namespace EpicLoot
{
    public class MagicItemComponent : CustomItemData
    {
        public const string TypeID = "rkel";

        public MagicItem MagicItem;

        protected override bool AllowStackingIdenticalValues { get; set; } = true;
        public void SetMagicItem(MagicItem magicItem)
        {
            if (magicItem == null)
                return;

            MagicItem = magicItem;
            Value = Serialize();
            Save();

            if (Player.m_localPlayer == null)
                return;

            if (Item.m_equipped && Player.m_localPlayer.IsItemEquiped(Item))
                Multiplayer_Player_Patch.UpdatePlayerZDOForEquipment(Player.m_localPlayer, Item, MagicItem != null);
        }

        public string Serialize()
        {
            return JsonConvert.SerializeObject(MagicItem, Formatting.None);
        }

        public void Deserialize()
        {
            try
            {
                if (string.IsNullOrEmpty(Value))
                    return;

                MagicItem = JsonConvert.DeserializeObject<MagicItem>(Value);
            }
            catch (Exception)
            {
                EpicLoot.LogError($"[{nameof(MagicItemComponent)}] Could not deserialize MagicItem json data! ({Item?.m_shared?.m_name})"); 
                throw;
            }
        }

        public CustomItemData Clone()
        {
            return MemberwiseClone() as CustomItemData;
        }

        public override void FirstLoad()
        {
            if (Item.m_shared.m_name == "$item_helmet_dverger")
            {
                MagicItem magicItem = new MagicItem();
                magicItem.Rarity = ItemRarity.Rare;
                magicItem.Effects.Add(new MagicItemEffect(MagicEffectType.DvergerCirclet));
                magicItem.TypeNameOverride = "$mod_epicloot_circlet";

                MagicItem = magicItem;
            }
            else if (Item.m_shared.m_name == "$item_beltstrength")
            {
                MagicItem magicItem = new MagicItem();
                magicItem.Rarity = ItemRarity.Rare;
                magicItem.Effects.Add(new MagicItemEffect(MagicEffectType.Megingjord));
                magicItem.TypeNameOverride = "$mod_epicloot_belt";

                MagicItem = magicItem;
            }
            else if (Item.m_shared.m_name == "$item_wishbone")
            {
                MagicItem magicItem = new MagicItem();
                magicItem.Rarity = ItemRarity.Epic;
                magicItem.Effects.Add(new MagicItemEffect(MagicEffectType.Wishbone));
                magicItem.TypeNameOverride = "$mod_epicloot_remains";

                MagicItem = magicItem;
            }
            
            FixupValuelessEffects();
            SetMagicItem(MagicItem);
        }

        public override void Load()
        {
            if (!string.IsNullOrEmpty(Value))
            {
                Deserialize();
            }

            FixupValuelessEffects();

            //Check Indestructible on Item
            Indestructible.MakeItemIndestructible(Item);

            SetMagicItem(MagicItem);
        }

        private void FixupValuelessEffects()
        {
            if (MagicItem == null)
                return;

            foreach (MagicItemEffect effect in MagicItem.Effects)
            {
                if (MagicItemEffectDefinitions.IsValuelessEffect(effect.EffectType, MagicItem.Rarity) &&
                    !Mathf.Approximately(effect.EffectValue, 1))
                {
                    EpicLoot.Log($"Fixing up effect on {MagicItem.DisplayName}: effect={effect.EffectType}");
                    effect.EffectValue = 1;
                }
            }
        }
    }

    public static class ItemDataExtensions
    {
        public static bool IsMagic(this ItemDrop.ItemData itemData)
        {
            MagicItemComponent magicData = itemData.Data().Get<MagicItemComponent>();
            return magicData != null && magicData.MagicItem != null;
        }

        public static bool IsUnidentified(this ItemDrop.ItemData itemData)
        {
            MagicItemComponent mic = itemData.Data().Get<MagicItemComponent>();
            if (mic == null) { return false; }
            return mic.MagicItem.IsUnidentified;
        }

        public static bool IsMagic(this ItemDrop.ItemData itemData, out MagicItem magicItem)
        {
            magicItem = itemData.GetMagicItem();
            return magicItem != null;
        }

        public static bool UseMagicBackground(this ItemDrop.ItemData itemData)
        {
            return itemData.IsMagic() || itemData.IsRunestone();
        }

        public static bool HasRarity(this ItemDrop.ItemData itemData)
        {
            return itemData.IsMagic() || itemData.IsMagicCraftingMaterial() || itemData.IsRunestone();
        }

        public static ItemRarity GetRarity(this ItemDrop.ItemData itemData)
        {
            if (itemData.IsMagic())
            {
                return itemData.GetMagicItem().Rarity;
            }
            else if (itemData.IsMagicCraftingMaterial())
            {
                return itemData.GetCraftingMaterialRarity();
            }
            else if (itemData.IsRunestone())
            {
                return itemData.GetRunestoneRarity();
            }

            throw new ArgumentException("itemData is not magic item, magic crafting material, or runestone");
        }

        public static Color GetRarityColor(this ItemDrop.ItemData itemData)
        {
            string colorString = "white";
            if (itemData.IsMagic())
            {
                colorString = itemData.GetMagicItem().GetColorString();
            }
            else if (itemData.IsMagicCraftingMaterial())
            {
                colorString = itemData.GetCraftingMaterialRarityColor();
            }
            else if (itemData.IsRunestone())
            {
                colorString = itemData.GetRunestoneRarityColor();
            }

            return ColorUtility.TryParseHtmlString(colorString, out Color color) ? color : Color.white;
        }

        public static bool HasMagicEffect(this ItemDrop.ItemData itemData, string effectType)
        {
            return itemData.GetMagicItem()?.HasEffect(effectType) ?? false;
        }

        public static void SaveMagicItem(this ItemDrop.ItemData itemData, MagicItem magicItem)
        {
            itemData.Data().GetOrCreate<MagicItemComponent>().SetMagicItem(magicItem);
        }

        public static bool IsExtended(this ItemDrop.ItemData itemData)
        {
            return itemData.Data().Get<MagicItemComponent>() != null;
        }

        public static ItemDrop.ItemData Extended(this ItemDrop.ItemData itemData)
        {
            MagicItemComponent value = itemData.Data().GetOrCreate<MagicItemComponent>();
            return value.Item;
        }

        public static MagicItem GetMagicItem(this ItemDrop.ItemData itemData)
        {
            return itemData.Data().Get<MagicItemComponent>()?.MagicItem;
        }

        public static string GetDisplayName(this ItemDrop.ItemData itemData)
        {
            // TODO: investigate
            string name = itemData.m_shared.m_name;

            if (itemData.IsMagic(out MagicItem magicItem) && !string.IsNullOrEmpty(magicItem.DisplayName))
            {
                const string pattern = @"\(.+?[+\-]\d+.+?\)";
                Match match = Regex.Match(itemData.m_shared.m_name, pattern);
                string appendedText = string.Empty;

                if (match.Success)
                {
                    string matchedValue = match.Value;
                    appendedText = $" {matchedValue}";
                }

                name = magicItem.DisplayName + appendedText;
            }

            return name;
        }

        public static string GetDecoratedName(this ItemDrop.ItemData itemData, string colorOverride = null)
        {
            string color = "white";
            string name = GetDisplayName(itemData);

            if (!string.IsNullOrEmpty(colorOverride))
            {
                color = colorOverride;
            }
            else if (itemData.IsMagic(out MagicItem magicItem))
            {
                color = magicItem.GetColorString();
            }
            else if (itemData.IsMagicCraftingMaterial() || itemData.IsRunestone())
            {
                color = itemData.GetCraftingMaterialRarityColor();
            }

            return $"<color={color}>{name}</color>";
        }

        public static string GetDescription(this ItemDrop.ItemData itemData)
        {
            if (itemData.IsMagic())
            {
                MagicItem magicItem = itemData.GetMagicItem();
                if (magicItem.IsUniqueLegendary() &&
                    UniqueLegendaryHelper.TryGetLegendaryInfo(magicItem.LegendaryID, out LegendaryInfo itemInfo))
                {
                    return itemInfo.Description;
                }
            }
            return itemData.m_shared.m_description;
        }

        public static bool IsPartOfSet(this ItemDrop.ItemData itemData, string setName)
        {
            return itemData.GetSetID() == setName;
        }

        public static bool CanBeAugmented(this ItemDrop.ItemData itemData)
        {
            if (!itemData.IsMagic())
            {
                return false;
            }

            return itemData.GetMagicItem().Effects.Select(effect => MagicItemEffectDefinitions.Get(effect.EffectType))
                .Any(effectDef => effectDef.CanBeAugmented);
        }

        public static bool CanBeRunified(this ItemDrop.ItemData itemData)
        {
            if (!itemData.IsMagic())
            {
                return false;
            }

            return itemData.GetMagicItem().Effects.Select(effect => MagicItemEffectDefinitions.Get(effect.EffectType))
                .Any(effectDef => effectDef.CanBeRunified);
        }

        public static string GetSetID(this ItemDrop.ItemData itemData, out bool isMundane)
        {
            isMundane = true;
            if (itemData.IsMagic(out MagicItem magicItem) && !string.IsNullOrEmpty(magicItem.SetID))
            {
                isMundane = false;
                return magicItem.SetID;
            }

            if (!string.IsNullOrEmpty(itemData.m_shared.m_setName))
            {
                return itemData.m_shared.m_setName;
            }

            return null;
        }

        public static string GetSetID(this ItemDrop.ItemData itemData)
        {
            return GetSetID(itemData, out _);
        }

        public static LegendarySetInfo GetLegendarySetInfo(this ItemDrop.ItemData itemData)
        {
            UniqueLegendaryHelper.TryGetLegendarySetInfo(itemData.GetSetID(), out LegendarySetInfo setInfo, out ItemRarity rarity);
            return setInfo;
        }

        public static bool IsSetItem(this ItemDrop.ItemData itemData)
        {
            return !string.IsNullOrEmpty(itemData.GetSetID());
        }

        public static bool IsMagicSetItem(this ItemDrop.ItemData itemData)
        {
            return itemData.IsMagic(out MagicItem magicItem) && !string.IsNullOrEmpty(magicItem.SetID);
        }

        public static bool IsMundaneSetItem(this ItemDrop.ItemData itemData)
        {
            return !string.IsNullOrEmpty(itemData.m_shared.m_setName);
        }

        public static int GetSetSize(this ItemDrop.ItemData itemData)
        {
            string setID = itemData.GetSetID(out bool isMundane);
            if (!string.IsNullOrEmpty(setID))
            {
                if (isMundane)
                {
                    return itemData.m_shared.m_setSize;
                }
                else if (UniqueLegendaryHelper.TryGetLegendarySetInfo(setID, out LegendarySetInfo setInfo, out ItemRarity rarity))
                {
                    return setInfo.LegendaryIDs.Count;
                }
            }

            return 0;
        }

        public static List<string> GetSetPieces(string setName)
        {
            if (UniqueLegendaryHelper.TryGetLegendarySetInfo(setName, out LegendarySetInfo setInfo, out ItemRarity rarity))
            {
                return setInfo.LegendaryIDs;
            }

            return GetMundaneSetPieces(ObjectDB.instance, setName);
        }

        public static List<string> GetMundaneSetPieces(ObjectDB objectDB, string setName)
        {
            List<string> results = new List<string>();
            foreach (GameObject itemPrefab in objectDB.m_items)
            {
                if (itemPrefab == null)
                {
                    EpicLoot.LogError("Null Item left in ObjectDB! (This means that a prefab was deleted and not an instance)");
                    continue;
                }

                ItemDrop itemDrop = itemPrefab.GetComponent<ItemDrop>();
                if (itemDrop == null)
                {
                    EpicLoot.LogError($"Item in ObjectDB missing ItemDrop: ({itemPrefab.name})");
                    continue;
                }

                if (itemDrop.m_itemData.m_shared.m_setName == setName)
                {
                    results.Add(itemPrefab.GetComponent<ItemDrop>().m_itemData.m_shared.m_name);
                }
            }

            return results;
        }

        public static GameObject InitializeCustomData(this ItemDrop.ItemData itemData)
        {
            GameObject prefab = itemData.m_dropPrefab;
            if (prefab != null)
            {
                ItemDrop itemDropPrefab = prefab.GetComponent<ItemDrop>();
                if (EpicLoot.CanBeMagicItem(itemDropPrefab.m_itemData) && !itemData.IsExtended())
                {
                    MagicItemComponent instanceData = itemData.Data().Add<MagicItemComponent>();
                    MagicItemComponent prefabData = itemDropPrefab.m_itemData.Data().Get<MagicItemComponent>();

                    if (instanceData != null && prefabData != null)
                    {
                        instanceData.SetMagicItem(prefabData.MagicItem);
                    }

                    return itemDropPrefab.gameObject;
                }
            }

            return null;
        }

        public static string GetSetTooltip(this ItemDrop.ItemData item)
        {
            if (item == null || Player.m_localPlayer == null)
            {
                return String.Empty;
            }

            StringBuilder text = new StringBuilder();

            try
            {
                // TODO: Clean up code associated with set data
                text.Append(GetMundaneSetTooltip(item));
                text.Append(GetMagicSetTooltip(item));
            }
            catch (Exception e)
            {
                EpicLoot.LogWarning($"[GetSetTooltip] Error on item {item.m_shared?.m_name} - {e.Message}");
            }

            return text.ToString();
        }

        public static string GetMundaneSetTooltip(ItemDrop.ItemData item)
        {
            if (string.IsNullOrEmpty(item.m_shared.m_setName))
            {
                return String.Empty;
            }

            string setID = item.m_shared.m_setName;
            int setSize = item.m_shared.m_setSize;

            return GetSetTooltip(item, setID, setSize, true);
        }

        public static string GetMagicSetTooltip(ItemDrop.ItemData item)
        {
            string setID = item.GetSetID(out bool isMundane);

            if (isMundane)
            {
                return String.Empty;
            }

            int setSize = item.GetSetSize();

            return GetSetTooltip(item, setID, setSize, false);
        }

        private static string GetSetTooltip(ItemDrop.ItemData item, string setID, int setSize, bool isMundane)
        {
            StringBuilder text = new StringBuilder();
            List<string> setPieces = GetSetPieces(setID);
            List<ItemDrop.ItemData> currentSetEquipped = Player.m_localPlayer.GetEquippedSetPieces(setID);

            string setDisplayName = GetSetDisplayName(item, isMundane);
            text.Append($"\n\n<color={EpicLoot.GetSetItemColor()}> $mod_epicloot_set: " +
                $"{setDisplayName} ({currentSetEquipped.Count}/{setSize}):</color>");

            foreach (string setItemName in setPieces)
            {
                bool isEquipped = IsSetItemEquipped(currentSetEquipped, setItemName, isMundane);
                string color = isEquipped ? "white" : "#808080ff";
                string displayName = GetSetItemDisplayName(setItemName, isMundane);
                text.Append($"\n  <color={color}>{displayName}</color>");
            }

            if (isMundane)
            {
                string setEffectColor = currentSetEquipped.Count == setSize ? EpicLoot.GetSetItemColor() : "#808080ff";
                float skillLevel = Player.m_localPlayer.GetSkillLevel(item.m_shared.m_skillType);
                text.Append($"\n<color={setEffectColor}>({setSize}) ‣ " +
                    $"{item.GetSetStatusEffectTooltip(item.m_quality, skillLevel).Replace("\n", " ")}</color>");
            }
            else
            {
                LegendarySetInfo setInfo = item.GetLegendarySetInfo();

                if (setInfo != null)
                {
                    foreach (SetBonusInfo setBonusInfo in setInfo.SetBonuses.OrderBy(x => x.Count))
                    {
                        bool hasEquipped = currentSetEquipped.Count >= setBonusInfo.Count;
                        MagicItemEffectDefinition effectDef = MagicItemEffectDefinitions.Get(setBonusInfo.Effect.Type);

                        if (effectDef == null)
                        {
                            EpicLoot.LogError($"Set Tooltip: Could not find effect ({setBonusInfo.Effect.Type}) " +
                                $"for set ({setInfo.ID}) bonus ({setBonusInfo.Count})!");
                            continue;
                        }

                        string display = MagicItem.GetEffectText(effectDef, setBonusInfo.Effect.Values?.MinValue ?? 0);
                        text.Append($"\n<color={(hasEquipped ? EpicLoot.GetSetItemColor() : "#808080ff")}>" +
                            $"({setBonusInfo.Count}) ‣ {display}</color>");
                    }
                }
            }

            return text.ToString();
        }

        private static string GetSetItemDisplayName(string setItemName, bool isMundane)
        {
            if (isMundane)
            {
                return setItemName;
            }
            else if (UniqueLegendaryHelper.TryGetLegendaryInfo(setItemName, out LegendaryInfo itemInfo))
            {
                return itemInfo.Name;
            }

            return setItemName;
        }

        public static string GetSetDisplayName(ItemDrop.ItemData item, bool isMundane)
        {
            if (!isMundane)
            {
                LegendarySetInfo setInfo = item.GetLegendarySetInfo();
                if (setInfo != null)
                {
                    return Localization.instance.Localize(setInfo.Name);
                }
                else
                {
                    return $"<unknown set: {item.GetSetID()}>";
                }
            }

            if (item.m_shared.m_setStatusEffect?.m_name != null)
            {
                return LocalizationManager.Instance.TryTranslate(item.m_shared.m_setStatusEffect.m_name);
            }

            return "<unknown set>";
        }

        public static bool IsSetItemEquipped(List<ItemDrop.ItemData> currentSetEquipped, string setItemName, bool isMundane)
        {
            if (isMundane)
            {
                return currentSetEquipped.Find(x => x.m_shared.m_name == setItemName) != null;
            }
            else
            {
                return currentSetEquipped.Find(x => x.IsMagic(out MagicItem magicItem) && magicItem.LegendaryID == setItemName) != null;
            }
        }
    }

    public static class EquipmentEffectCache
    {
        public static ConditionalWeakTable<Player, Dictionary<string, float?>> EquippedValues = new ConditionalWeakTable<Player, Dictionary<string, float?>>();

        [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.UnequipItem))]
        public static class EquipmentEffectCache_Humanoid_UnequipItem_Patch
        {
            [UsedImplicitly]
            public static void Prefix(Humanoid __instance)
            {
                if (__instance is Player player)
                {
                    Reset(player);
                }
            }
        }

        [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.EquipItem))]
        public static class EquipmentEffectCache_Humanoid_EquipItem_Patch
        {
            [UsedImplicitly]
            public static void Prefix(Humanoid __instance)
            {
                if (__instance is Player player)
                {
                    Reset(player);
                }
            }
        }

        public static void Reset(Player player)
        {
            EquippedValues.Remove(player);
        }

        public static float? Get(Player player, string effect, Func<float?> calculate)
        {
            if (effect == null || player == null) { return 0f; } // default fail out if the requested key is null
            Dictionary<string, float?> values = EquippedValues.GetOrCreateValue(player);
            if (values.TryGetValue(effect, out float? value))
            {
                return value;
            }

            return values[effect] = calculate();
        }
    }

    public static class PlayerExtensions
    {
        public static List<ItemDrop.ItemData> GetEquipment(this Player player)
        {
            List<ItemDrop.ItemData> results = new List<ItemDrop.ItemData>();
            if (player.m_rightItem != null)
                results.Add(player.m_rightItem);
            if (player.m_leftItem != null)
                results.Add(player.m_leftItem);
            if (player.m_chestItem != null)
                results.Add(player.m_chestItem);
            if (player.m_legItem != null)
                results.Add(player.m_legItem);
            if (player.m_helmetItem != null)
                results.Add(player.m_helmetItem);
            if (player.m_shoulderItem != null)
                results.Add(player.m_shoulderItem);
            if (player.m_utilityItem != null)
                results.Add(player.m_utilityItem);
            if (player.m_trinketItem != null)
                results.Add(player.m_trinketItem);
            return results;
        }

        public static List<MagicItemEffect> GetAllActiveMagicEffects(this Player player, string effectType = null)
        {
            IEnumerable<MagicItemEffect> equipEffects = player.GetEquipment()
                .Where(x => x.IsMagic())
                .SelectMany(x => x.GetMagicItem().GetEffects(effectType));
            List<MagicItemEffect> setEffects = player.GetAllActiveSetMagicEffects(effectType);
            return equipEffects.Concat(setEffects).ToList();
        }

        public static List<MagicItemEffect> GetAllActiveSetMagicEffects(this Player player, string effectType = null)
        {
            List<MagicItemEffect> activeSetEffects = new List<MagicItemEffect>();
            HashSet<LegendarySetInfo> equippedSets = player.GetEquippedSets();
            foreach (LegendarySetInfo setInfo in equippedSets)
            {
                int count = player.GetEquippedSetPieces(setInfo.ID).Count;
                foreach (SetBonusInfo setBonusInfo in setInfo.SetBonuses)
                {
                    if (count >= setBonusInfo.Count && (effectType == null || setBonusInfo.Effect.Type == effectType))
                    {
                        MagicItemEffect effect = new MagicItemEffect(setBonusInfo.Effect.Type, setBonusInfo.Effect.Values?.MinValue ?? MagicItemEffect.DefaultValue);
                        activeSetEffects.Add(effect);
                    }
                }
            }

            return activeSetEffects;
        }

        public static HashSet<LegendarySetInfo> GetEquippedSets(this Player player)
        {
            HashSet<LegendarySetInfo> sets = new HashSet<LegendarySetInfo>();
            foreach (ItemDrop.ItemData itemData in player.GetEquipment())
            {
                if (itemData.IsMagic(out MagicItem magicItem) && magicItem.IsLegendarySetItem())
                {
                    if (UniqueLegendaryHelper.TryGetLegendarySetInfo(magicItem.SetID, out LegendarySetInfo setInfo, out ItemRarity rarity))
                    {
                        sets.Add(setInfo);
                    }
                }
            }

            return sets;
        }

        public static float GetTotalActiveMagicEffectValue(this Player player, string effectType,
            float scale = 1.0f, ItemDrop.ItemData ignoreThisItem = null)
        {
            float totalValue = scale * (EquipmentEffectCache.Get(player, effectType, () =>
            {
                List<MagicItemEffect> allEffects = player.GetAllActiveMagicEffects(effectType);
                return allEffects.Count > 0 ? allEffects.Select(x => x.EffectValue).Sum() : null;
            }) ?? 0);

            if (ignoreThisItem != null && player.IsItemEquiped(ignoreThisItem) && ignoreThisItem.IsMagic(out MagicItem magicItem))
            {
                totalValue -= magicItem.GetTotalEffectValue(effectType, scale);
            }

            return totalValue;
        }

        public static bool HasActiveMagicEffect(this Player player, string effectType, out float effectValue,
            float scale = 1.0f, ItemDrop.ItemData ignoreThisItem = null)
        {
            effectValue = GetTotalActiveMagicEffectValue(player, effectType, scale, ignoreThisItem);
            return effectValue > 0;
        }
        
        public static bool HasActiveMagicEffect(this Player player, string effectType)
        {
            if (player == null) return false;
            List<MagicItemEffect> effects = player.GetAllActiveMagicEffects(effectType.ToString());

            return effects.Count > 0;
        }

        public static List<ItemDrop.ItemData> GetEquippedSetPieces(this Player player, string setName)
        {
            return player.GetEquipment().Where(x => x.IsPartOfSet(setName)).ToList();
        }

        public static bool HasEquipmentOfType(this Player player, ItemDrop.ItemData.ItemType type)
        {
            return player.GetEquipment().Exists(x => x != null && x.m_shared.m_itemType == type);
        }

        public static ItemDrop.ItemData GetEquipmentOfType(this Player player, ItemDrop.ItemData.ItemType type)
        {
            return player.GetEquipment().FirstOrDefault(x => x != null && x.m_shared.m_itemType == type);
        }

        public static Player GetPlayerWithEquippedItem(ItemDrop.ItemData itemData)
        {
            return Player.s_players.FirstOrDefault(player => player.IsItemEquiped(itemData));
        }
    }

    public static class ItemBackgroundHelper
    {
        public static Image CreateAndGetMagicItemBackgroundImage(GameObject elementGo, GameObject equipped, bool isInventoryGrid)
        {
            RectTransform magicItemTransform = (RectTransform)elementGo.transform.Find("magicItem");
            if (magicItemTransform == null)
            {
                GameObject magicItemObject = Object.Instantiate(equipped, equipped.transform.parent);
                magicItemObject.transform.SetSiblingIndex(EpicLoot.HasAuga ? equipped.transform.GetSiblingIndex() : equipped.transform.GetSiblingIndex() + 1);
                magicItemObject.name = "magicItem";
                magicItemObject.SetActive(true);
                magicItemTransform = (RectTransform)magicItemObject.transform;
                magicItemTransform.anchorMin = magicItemTransform.anchorMax = new Vector2(0.5f, 0.5f);
                magicItemTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 64);
                magicItemTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 64);
                magicItemTransform.pivot = new Vector2(0.5f, 0.5f);
                magicItemTransform.anchoredPosition = Vector2.zero;
                Image magicItemInit = magicItemTransform.GetComponent<Image>();
                magicItemInit.color = Color.white;
                magicItemInit.raycastTarget = false;
            }

            // Also add set item marker
            if (isInventoryGrid)
            {
                RectTransform setItemTransform = (RectTransform)elementGo.transform.Find("setItem");
                if (setItemTransform == null)
                {
                    GameObject setItemObject = Object.Instantiate(equipped, equipped.transform.parent);
                    setItemObject.transform.SetAsLastSibling();
                    setItemObject.name = "setItem";
                    setItemObject.SetActive(true);
                    setItemTransform = (RectTransform)setItemObject.transform;
                    setItemTransform.anchorMin = setItemTransform.anchorMax = new Vector2(0.5f, 0.5f);
                    setItemTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 64);
                    setItemTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 64);
                    setItemTransform.pivot = new Vector2(0.5f, 0.5f);
                    setItemTransform.anchoredPosition = Vector2.zero;
                    Image setItemInit = setItemTransform.GetComponent<Image>();
                    setItemInit.raycastTarget = false;
                    setItemInit.sprite = EpicLoot.GetSetItemSprite();
                    setItemInit.color = ColorUtility.TryParseHtmlString(EpicLoot.GetSetItemColor(), out Color color) ? color : Color.white;
                }
            }

            // Also change equipped image
            Image equippedImage = equipped.GetComponent<Image>();
            if (equippedImage != null && (!isInventoryGrid || !EpicLoot.HasAuga))
            {
                equippedImage.sprite = EpicLoot.GetEquippedSprite();
                equippedImage.color = Color.white;
                equippedImage.raycastTarget = false;
                RectTransform rectTransform = equipped.RectTransform();
                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                rectTransform.pivot = new Vector2(0.5f, 0.5f);
                rectTransform.anchoredPosition = Vector2.zero;
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, equippedImage.sprite.texture.width);
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, equippedImage.sprite.texture.height);
            }

            return magicItemTransform.GetComponent<Image>();
        }
    }

    [HarmonyPatch(typeof(InventoryGrid), nameof(InventoryGrid.UpdateGui), typeof(Player), typeof(ItemDrop.ItemData))]
    public static class InventoryGrid_UpdateGui_MagicItemComponent_Patch
    {
        public static void UpdateGuiElements(InventoryGrid.Element element, bool used)
        {
            element.m_used = used;
            Transform magicItemTransform = element.m_go.transform.Find("magicItem");
            if (magicItemTransform != null)
            {
                Image magicItem = magicItemTransform.GetComponent<Image>();
                if (magicItem != null)
                {
                    magicItem.enabled = false;
                }
            }

            Transform setItemTransform = element.m_go.transform.Find("setItem");
            if (setItemTransform != null)
            {
                Image setItem = setItemTransform.GetComponent<Image>();
                if (setItem != null)
                {
                    setItem.enabled = false;
                }
            }
        }
   
        public static void UpdateGuiItems(ItemDrop.ItemData itemData, InventoryGrid.Element element)
        {
            Image magicItem = ItemBackgroundHelper.CreateAndGetMagicItemBackgroundImage(element.m_go, element.m_equiped.gameObject, true);
            if (itemData.UseMagicBackground())
            {
                magicItem.enabled = true;
                magicItem.sprite = EpicLoot.GetMagicItemBgSprite();
                magicItem.color = itemData.GetRarityColor();
            }
            else
            {
                magicItem.enabled = false;
            }

            Transform setItemTransform = element.m_go.transform.Find("setItem");
            if (setItemTransform != null)
            {
                Image setItem = setItemTransform.GetComponent<Image>();
                if (setItem != null)
                {
                    setItem.enabled = itemData.IsSetItem();
                }
            }
        }
   
        [UsedImplicitly]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            bool successPatch1 = false;
            bool successPatch2 = false;
            List<CodeInstruction> instrs = instructions.ToList();

            int counter = 0;

            CodeInstruction LogMessage(CodeInstruction instruction)
            {
                //EpicLoot.Log($"IL_{counter}: Opcode: {instruction.opcode} Operand: {instruction.operand}");
                return instruction;
            }

            System.Reflection.FieldInfo elementUsedField = AccessTools.DeclaredField(typeof(InventoryGrid.Element), nameof(InventoryGrid.Element.m_used));
            System.Reflection.FieldInfo elementQueuedField = AccessTools.DeclaredField(typeof(InventoryGrid.Element), nameof(InventoryGrid.Element.m_queued));

            for (int i = 0; i < instrs.Count; ++i)
            {
                if (i > 6 && instrs[i].opcode == OpCodes.Stfld && instrs[i].operand.Equals(elementUsedField) && instrs[i-1].opcode == OpCodes.Ldc_I4_0
                    && instrs[i-2].opcode == OpCodes.Call && instrs[i-3].opcode == OpCodes.Ldloca_S)
                {
                    //Element Spot
                    CodeInstruction callInstruction = new CodeInstruction(OpCodes.Call, AccessTools.DeclaredMethod(typeof(InventoryGrid_UpdateGui_MagicItemComponent_Patch), nameof(UpdateGuiElements)));
                    //Move Any Labels from the instruction position being patched to new instruction.
                    if (instrs[i].labels.Count > 0)
                    {
                        instrs[i].MoveLabelsTo(callInstruction);
                    }

                    //Get Element variable
                    yield return LogMessage(callInstruction);
                    counter++;
                    
                    //Skip Stfld Instruction
                    i++;

                    successPatch1 = true;

                } else if (i > 6 && instrs[i].opcode == OpCodes.Ldloc_S && instrs[i+1].opcode == OpCodes.Ldfld && instrs[i+1].operand.Equals(elementQueuedField)
                           && instrs[i+2].opcode == OpCodes.Ldarg_1 && instrs[i+3].opcode == OpCodes.Call)
                {
                    //Item Spot
                    object elementOperand = instrs[i].operand;
                    object itemDataOperand = instrs[i - 5].operand;

                    CodeInstruction ldLocsInstruction = new CodeInstruction(OpCodes.Ldloc_S, itemDataOperand);
                    //Move Any Labels from the instruction position being patched to new instruction.
                    if (instrs[i].labels.Count > 0)
                    {
                        instrs[i].MoveLabelsTo(ldLocsInstruction);
                    }

                    //Get Item variable
                    yield return LogMessage(ldLocsInstruction);
                    counter++;
        
                    //Get Element variable
                    yield return LogMessage(new CodeInstruction(OpCodes.Ldloc_S,elementOperand));
                    counter++;
        
                    //Patch Calling Method
                    yield return LogMessage(new CodeInstruction(OpCodes.Call, AccessTools.DeclaredMethod(typeof(InventoryGrid_UpdateGui_MagicItemComponent_Patch), nameof(UpdateGuiItems))));
                    counter++;
                    successPatch2 = true;
                }
                
                yield return LogMessage(instrs[i]);
                counter++;
            }
            
            if (!successPatch2 || !successPatch1)
            {
                EpicLoot.LogError($"InventoryGrid.UpdateGui Transpiler Failed To Patch");
                EpicLoot.LogError($"!successPatch1: {!successPatch1}");
                EpicLoot.LogError($"!successPatch2: {!successPatch2}");
                Thread.Sleep(5000);
            }
        }
    }

    [HarmonyPatch(typeof(HotkeyBar), nameof(HotkeyBar.UpdateIcons), typeof(Player))]
    public static class HotkeyBar_UpdateIcons_Patch
    {
        public static void UpdateElements(HotkeyBar.ElementData element, bool used)
        {
            element.m_used = used;
            Transform magicItemTransform = element.m_go.transform.Find("magicItem");
            if (magicItemTransform != null)
            {
                Image magicItem = magicItemTransform.GetComponent<Image>();
                if (magicItem != null)
                {
                    magicItem.enabled = false;
                }
            }
        }

        public static void UpdateIcons(HotkeyBar.ElementData element, ItemDrop.ItemData itemData)
        {
            Image magicItem = ItemBackgroundHelper.CreateAndGetMagicItemBackgroundImage(element.m_go, element.m_equiped, false);
            if (itemData != null && itemData.UseMagicBackground())
            {
                magicItem.enabled = true;
                magicItem.sprite = EpicLoot.GetMagicItemBgSprite();
                magicItem.color = itemData.GetRarityColor();
            }
            else
            {
                magicItem.enabled = false;
            }
        }
        
        [UsedImplicitly]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instrs = instructions.ToList();
            bool successPatch1 = false;
            bool successPatch2 = false;

            int counter = 0;

            CodeInstruction LogMessage(CodeInstruction instruction)
            {
                //EpicLoot.Log($"IL_{counter}: Opcode: {instruction.opcode} Operand: {instruction.operand}");
                return instruction;
            }

            System.Reflection.FieldInfo elementDataEquipedField = AccessTools.DeclaredField(typeof(HotkeyBar.ElementData), nameof(HotkeyBar.ElementData.m_equiped));
            System.Reflection.FieldInfo itemDataEquipedField = AccessTools.DeclaredField(typeof(ItemDrop.ItemData), nameof(ItemDrop.ItemData.m_equipped));
            System.Reflection.MethodInfo setActiveMethod = AccessTools.DeclaredMethod(typeof(GameObject), nameof(GameObject.SetActive));
            System.Reflection.FieldInfo elementUsedField = AccessTools.DeclaredField(typeof(HotkeyBar.ElementData), nameof(HotkeyBar.ElementData.m_used)); 

            for (int i = 0; i < instrs.Count; ++i)
            {

                if (i > 6 && instrs[i].opcode == OpCodes.Stfld && instrs[i].operand.Equals(elementUsedField) && instrs[i-1].opcode == OpCodes.Ldc_I4_0
                    && instrs[i-2].opcode == OpCodes.Call && instrs[i-3].opcode == OpCodes.Ldloca_S)
                {
                    //Element Spot
                    CodeInstruction callInstruction = new CodeInstruction(OpCodes.Call, AccessTools.DeclaredMethod(typeof(HotkeyBar_UpdateIcons_Patch), nameof(UpdateElements)));
                    //Move Any Labels from the instruction position being patched to new instruction.
                    if (instrs[i].labels.Count > 0)
                    {
                        instrs[i].MoveLabelsTo(callInstruction);
                    }

                    //Get Element variable
                    yield return LogMessage(callInstruction);
                    counter++;
                    
                    //Skip Stfld Instruction
                    i++;
                    successPatch1 = true;
                }
                
                yield return LogMessage(instrs[i]);
                counter++;

                if (i > 6 && instrs[i].opcode == OpCodes.Callvirt && instrs[i].operand.Equals(setActiveMethod) && instrs[i-1].opcode == OpCodes.Ldfld
                    && instrs[i-1].operand.Equals(itemDataEquipedField) && instrs[i-2].opcode == OpCodes.Ldloc_S && instrs[i-3].opcode == OpCodes.Ldfld
                    && instrs[i-3].operand.Equals(elementDataEquipedField) && instrs[i-4].opcode == OpCodes.Ldloc_S)
                {
                    object elementOperand = instrs[i - 4].operand;
                    object itemDataOperand = instrs[i - 2].operand;

                    CodeInstruction ldLocInstruction = new CodeInstruction(OpCodes.Ldloc_S,elementOperand);
                    //Move Any Labels from the instruction position being patched to new instruction.
                    if (instrs[i].labels.Count > 0)
                        instrs[i].MoveLabelsTo(ldLocInstruction);

                    //Get Element
                    yield return LogMessage(ldLocInstruction);
                    counter++;
                    
                    //Get Item Data
                    yield return LogMessage(new CodeInstruction(OpCodes.Ldloc_S,itemDataOperand));
                    counter++;
          
                    //Patch Calling Method
                    yield return LogMessage(new CodeInstruction(OpCodes.Call, AccessTools.DeclaredMethod(typeof(HotkeyBar_UpdateIcons_Patch), nameof(UpdateIcons))));
                    counter++;
                    successPatch2 = true;
                }
            }
            
            if (!successPatch2 || !successPatch1)
            {
                EpicLoot.LogError($"HotkeyBar.UpdateIcons Transpiler Failed To Patch");
                EpicLoot.LogError($"!successPatch1: {!successPatch1}");
                EpicLoot.LogError($"!successPatch2: {!successPatch2}");
                Thread.Sleep(5000);
            }

        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.GetActionProgress),
        new Type[] { typeof(string), typeof(float) }, new ArgumentType[] { ArgumentType.Out, ArgumentType.Out })]
    public static class Player_GetActionProgress_Patch
    {
        public static void Postfix(Player __instance, ref string name)
        {
            if (__instance.m_actionQueue.Count > 0)
            {
                Player.MinorActionData equip = __instance.m_actionQueue[0];
                if (equip.m_type != Player.MinorActionData.ActionType.Reload)
                {
                    if (equip.m_duration > 0.5f)
                    {
                        name = equip.m_type == Player.MinorActionData.ActionType.Unequip ? "$hud_unequipping " + equip.m_item.GetDecoratedName() : "$hud_equipping " + equip.m_item.GetDecoratedName();
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(ItemDrop))]
    public static class ItemDrop_Patches
    {
        [HarmonyPatch(typeof(ItemDrop), nameof(ItemDrop.GetHoverText))]
        [HarmonyPrefix]
        public static bool GetHoverText_Prefix(ItemDrop __instance, ref string __result)
        {
            string str = __instance.m_itemData.GetDecoratedName();
            if (__instance.m_itemData.m_quality > 1)
            {
                str = $"{str}[{__instance.m_itemData.m_quality}] ";
            }
            else if (__instance.m_itemData.m_stack > 1)
            {
                str = $"{str} x{__instance.m_itemData.m_stack}";
            }
            __result = Localization.instance.Localize($"{str}\n[<color=yellow><b>$KEY_Use</b></color>] $inventory_pickup");
            return false;
        }

        [HarmonyPatch(typeof(ItemDrop), nameof(ItemDrop.GetHoverName))]
        [HarmonyPrefix]
        public static bool GetHoverName_Prefix(ItemDrop __instance, ref string __result)
        {
            __result = __instance.m_itemData.GetDecoratedName();
            return false;
        }
    }
}
