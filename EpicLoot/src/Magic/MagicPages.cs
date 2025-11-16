using System;
using System.Collections.Generic;
using System.Linq;
using EpicLoot.Adventure;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace EpicLoot;

public class MagicPages : MonoBehaviour
{
    [HarmonyPatch(typeof(TextsDialog), nameof(TextsDialog.Awake))]
    private static class TextsDialog_Awake_Patch
    {
        private static void Postfix(TextsDialog __instance) => __instance.gameObject.AddComponent<MagicPages>();
    }

    [HarmonyPatch(typeof(TextsDialog), nameof(TextsDialog.UpdateTextsList))]
    private static class TextsDialog_UpdateTextsList_Patch
    {
        private static void Postfix(TextsDialog __instance)
        {
            if (!Player.m_localPlayer) return;
            
            __instance.m_texts.Insert(EpicLoot.HasAuga ? 0 : 2, MagicEffectsPage);
            __instance.m_texts.Insert(EpicLoot.HasAuga ? 1 : 3, ExplainPage);
            __instance.m_texts.Insert(EpicLoot.HasAuga ? 2 : 4, TreasureBountyPage);
        }
    }

    [HarmonyPatch(typeof(TextsDialog), nameof(TextsDialog.OnSelectText))]
    private static class TextsDialog_OnSelectText_Patch
    {
        private static bool Prefix(TextsDialog __instance, TextsDialog.TextInfo text)
        {
            if (!__instance.TryGetComponent(out MagicPages component)) return true;
            component.Reset();
            if (text is not MagicInfo magicInfo) return true;
            component.OnSelectText(magicInfo);
            return false;
        }
    }

    [HarmonyPatch(typeof(TextsDialog), nameof(TextsDialog.Setup))]
    private static class TextsDialog_Setup_Patch
    {
        private static void Postfix(TextsDialog __instance) => __instance.GetComponent<MagicPages>()?.Reset();
    }
    
    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.Hide))]
    public static class InventoryGui_Hide_Prefix
    {
        [UsedImplicitly]
        private static bool Prefix() => !InSearchField();
    }

    [HarmonyPatch(typeof(PlayerController), nameof(PlayerController.TakeInput))]
    public static class PlayerController_TakeInput_Patch
    {
        [UsedImplicitly]
        private static void Postfix(ref bool __result)
        {
            __result &= !InSearchField();
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.TakeInput))]
    public static class PlayerTakeInput_Patch
    {
        [UsedImplicitly]
        private static void Postfix(ref bool __result)
        {
            __result &= !InSearchField();
        } 
    }

    [HarmonyPatch(typeof(Chat), nameof(Chat.HasFocus))]
    public static class Chat_HasFocus_Patch
    {
        [UsedImplicitly]
        private static void Postfix(ref bool __result)
        {
            __result &= !InSearchField();
        } 
    }

    private static ExplainInfo ExplainPage;
    private static TreasureBountyInfo TreasureBountyPage;
    private static MagicEffectInfo MagicEffectsPage;

    public const int headerFontSize = 30;
    public const int largeFontSize = 24;
    public const int mediumFontSize = 18;
    public const int fontSize = 16;

    private static float minWidth;
    private static float minHeight;
    
    private SearchField Search;
    public TextList MagicPagesTextArea;
    public GameObject compendiumTextArea;
    [CanBeNull] private TextElement TitleElement;
    private bool wasGlowing;

    private static MagicPages instance;
    
    public void Awake()
    {
        ExplainPage = new ExplainInfo(Localization.instance.Localize($"{EpicLoot.GetMagicEffectPip(false)} $mod_epicloot_me_explaintitle"));
        TreasureBountyPage = new TreasureBountyInfo(Localization.instance.Localize($"{EpicLoot.GetMagicEffectPip(false)} $mod_epicloot_adventure_title"));
        MagicEffectsPage = new MagicEffectInfo(Localization.instance.Localize($"{EpicLoot.GetMagicEffectPip(false)} $mod_epicloot_active_magic_effects"));
        
        instance = this;
        
        Transform frame = transform.Find("Texts_frame");
        Image closeButtonImg = frame.Find("Closebutton").GetComponent<Image>();
        Button closeButton = closeButtonImg.GetComponent<Button>();
        RectTransform closeButtonRect = closeButtonImg.GetComponent<RectTransform>();
        compendiumTextArea = frame.Find("TextArea").gameObject;
        RectTransform textAreaRect = compendiumTextArea.GetComponent<RectTransform>();
        minWidth = textAreaRect.rect.width;
        minHeight = textAreaRect.rect.height;
        Search = new SearchField(frame);
        Search.SetPosition(closeButtonRect.position + new Vector3(closeButtonRect.sizeDelta.x * 1.5f - 10f, 0f));
        Search.SetSize(350f, 46f);
        Search.SetBackground(closeButtonImg);
        Search.background.sprite = closeButton.spriteState.disabledSprite;
        Search.SetFont(FontManager.GetFont(FontManager.FontOptions.AveriaSerifLibre));
        Search.field.onValueChanged.AddListener(OnSearch);
        
        MagicPagesTextArea = new TextList(compendiumTextArea, frame);
        Reset();
    }


    public void Update()
    {
        //  makes search field glow when focused
        if (wasGlowing && !InSearchField())
        {
            Search.EnableGlow(false);
            wasGlowing = false;
        }
        else if (!wasGlowing && InSearchField())
        {
            Search.EnableGlow(true);
            wasGlowing = true;
        }
    }

    public static bool InSearchField() => instance?.Search.field.isFocused ?? false;

    public void OnSearch(string query)
    {
        foreach (TextGroup element in MagicPagesTextArea.elements)
        {
            element.Enable(element.IsMatch(query.Trim()));
        }
    }

    public void Reset()
    {
        compendiumTextArea.SetActive(true);
        Search.Enable(false);
        MagicPagesTextArea.Enable(false);
        MagicPagesTextArea.Clear();
        TitleElement?.Destroy();
    }

    public void OnSelectText(MagicInfo text)
    {
        compendiumTextArea.SetActive(false);
        MagicPagesTextArea.Enable(true);
        Search.Enable(text.showSearchBar);
        Search.field.SetTextWithoutNotify(string.Empty);
        
        TitleElement = MagicPagesTextArea.Create(text.m_topic);
        TitleElement!.SetSize(minWidth, 100f);
        TitleElement.SetFontSize(40);
        TitleElement.SetColor(new Color(1f, 0.5f, 0f));
        TitleElement.SetAlignment(TextAnchor.MiddleCenter);
        
        text.Build(this);
        MagicPagesTextArea.ResizeOverlay();
    }
    
    public class TextList
    {
        private readonly GameObject obj;
        private readonly Image background;
        private readonly ScrollRect scrollRect;
        private readonly VerticalLayoutGroup layout;
        private readonly TextElement _template;
        public readonly List<TextGroup> elements = new();
        private readonly RectTransform overlayRect;

        public TextList(GameObject source, Transform parent)
        {
            obj = Instantiate(source, parent);
            obj.name = "EpicLootMagicTextArea";
            background = obj.GetComponent<Image>();
            scrollRect = obj.transform.Find("ScrollArea").GetComponent<ScrollRect>();
            layout = obj.transform.Find("ScrollArea/Content").GetComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.padding.left = 5;
            layout.spacing = 0;
            for (int i = 0; i < layout.transform.childCount; ++i)
            {
                var child = layout.transform.GetChild(i).gameObject;
                Destroy(child);
            }

            // add overlay to have something to raycast to scroll smoothly between gaps
            GameObject overlay = new GameObject("overlay");
            overlay.AddComponent<LayoutElement>().ignoreLayout = true;
            overlayRect = overlay.GetComponent<RectTransform>();
            overlayRect.SetParent(layout.transform);
            overlay.AddComponent<Image>().color = Color.clear;
            overlayRect.sizeDelta = new Vector2(minWidth, minHeight);
            overlayRect.anchoredPosition = Vector2.zero;

            _template = new TextElement(layout.transform);
        }

        public void ResizeOverlay()
        {
            float total = instance.TitleElement?.GetHeight() ?? 0f;
            total += elements.Sum(x => x.title.GetHeight() + x.content.Sum(y => y.GetHeight()));
            SetOverlayHeight(total);
        }

        private void SetOverlayHeight(float height) => overlayRect.sizeDelta = new Vector2(overlayRect.sizeDelta.x, Mathf.Max(height, minHeight));
        
        public void Clear()
        {
            foreach (TextGroup group in elements)
            {
                group.title.Destroy();
                foreach(TextElement element in group.content) element.Destroy(); 
            }
            elements.Clear();
        }

        public void Add(string title, params string[] content)
        {
            TextElement titleElement = _template.Create(title, layout.transform);
            titleElement.EnableOutline(true);
            List<TextElement> contentElements = new();
            foreach (string text in content)
            {
                TextElement contentElement = _template.Create(text, layout.transform);
                contentElements.Add(contentElement);
            }
            TextGroup group = new TextGroup(titleElement, contentElements.ToArray());
            elements.Add(group);
        }

        public void Enable(bool enable) => obj.SetActive(enable);
        
        public TextElement Create(string line) => _template.Create(line, layout.transform);
    }

    public class TextGroup(TextElement title, params TextElement[] content)
    {
        public readonly TextElement title = title;
        public readonly TextElement[] content = content;
        public bool IsMatch(string query) => title.IsMatch(query) || content.Any(x => x.IsMatch(query));
        public void Enable(bool enable)
        {
            title.Enable(enable);
            foreach (TextElement element in content) element.Enable(enable);
        }
    }

    public class TextElement
    {
        private readonly GameObject obj;
        private readonly RectTransform rect;
        private readonly Text text;
        private readonly Outline outline;

        public TextElement(Transform parent)
        {
            obj = new GameObject("text");
            obj.SetActive(false);
            rect = obj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(minWidth - 10f, 35f);
            rect.SetParent(parent);
            
            text = obj.AddComponent<Text>();
            outline = obj.AddComponent<Outline>();
            outline.enabled = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.font = FontManager.GetFont(FontManager.FontOptions.AveriaSerifLibre);
            text.fontSize = fontSize;
            text.supportRichText = true;
            
            obj.AddComponent<ContentSizeFitter>();
        }

        private TextElement(GameObject source)
        {
            obj = source;
            rect = obj.GetComponent<RectTransform>();
            text = obj.GetComponent<Text>();
            outline = obj.GetComponent<Outline>();
            Enable(true);
        }
        
        public float GetHeight() => rect.sizeDelta.y;

        public bool IsMatch(string query)
        {
            return text.text.ToLower().Contains(query.ToLower());
        }
        
        public void SetFont(Font font) => text.font = font;
        public void SetFontSize(int size) => text.fontSize = size;

        private void Set(string line)
        {
            text.text = Localization.instance.Localize(line);
        }
        public void SetParent(Transform parent) => rect.SetParent(parent);
        public void Destroy() => UnityEngine.Object.Destroy(obj);
        public void Enable(bool enable) => obj.SetActive(enable);
        public void EnableOutline(bool enable) => outline.enabled = enable;
        public void SetSize(float width, float height) => rect.sizeDelta = new Vector2(width, height);
        public void SetColor(Color color) => text.color = color;
        public void SetAlignment(TextAnchor alignment) => text.alignment = alignment;
        
        public TextElement Create(string line, Transform parent)
        {
            GameObject go = Instantiate(obj, parent);
            TextElement element = new TextElement(go);
            element.Set(line);
            return element;
        }
    }

    private class SearchField
    {
        private readonly GameObject obj;
        private readonly RectTransform rect;
        public readonly InputField field;
        private readonly Text placeholder;
        public readonly Image background;
        private readonly Image glow;
        private readonly RectTransform textRect;
        private readonly RectTransform placeholderRect;
        private const float widthPadding = 20f;

        public SearchField(Transform parent)
        {
            obj = new GameObject("searchField");
            rect = obj.AddComponent<RectTransform>();
            obj.transform.SetParent(parent);
            background = obj.AddComponent<Image>();
            field = obj.AddComponent<InputField>();
            field.targetGraphic = background;

            glow = new GameObject("glow").AddComponent<Image>();
            var craftGlow = InventoryGui.instance.m_crafting.Find("RepairButton/Glow").GetComponent<Image>();
            glow.sprite = craftGlow.sprite;
            glow.type = craftGlow.type;
            glow.color = craftGlow.color;
            glow.material = craftGlow.material;
            glow.rectTransform.SetParent(rect);
            glow.enabled = false;

            GameObject text = new GameObject("Text");
            textRect = text.AddComponent<RectTransform>();
            text.transform.SetParent(obj.transform);
            field.textComponent = text.AddComponent<Text>();;
            field.textComponent.alignment = TextAnchor.MiddleLeft;
            
            GameObject placeholderObj = new GameObject("Placeholder");
            placeholderRect = placeholderObj.AddComponent<RectTransform>();
            placeholderObj.transform.SetParent(obj.transform);
            placeholder = placeholderObj.AddComponent<Text>();
            //TODO: localize search text
            placeholder.text = "Search...";
            placeholder.color = Color.gray;
            field.placeholder = placeholder;
            placeholder.alignment = TextAnchor.MiddleLeft;
        }
        
        public void EnableGlow(bool enable) => glow.enabled = enable;

        public void SetPosition(Vector2 pos) => rect.position = pos;
        
        public void SetSize(float x, float y)
        {
            rect.sizeDelta = new Vector2(x, y);
            glow.rectTransform.sizeDelta = new Vector2(x, y);
            textRect.sizeDelta = new Vector2(x - widthPadding, y);
            placeholderRect.sizeDelta = new Vector2(x - widthPadding, y);
        }

        public void SetBackground(Image source)
        {
            background.sprite = source.sprite;
            background.material = source.material;
            background.color = source.color;
            background.type = source.type;
        }

        public void SetFont(Font font)
        {
            field.textComponent.font = font;
            placeholder.font = font;
        }

        public void Enable(bool enable) => obj.SetActive(enable);
    }
}

public class MagicEffectInfo(string topic) : MagicInfo(topic)
{
    public override void Build(MagicPages instance)
    {
        if (!Player.m_localPlayer) return;
        
        Dictionary<string, List<KeyValuePair<MagicItemEffect, ItemDrop.ItemData>>> magicEffects = new Dictionary<string, List<KeyValuePair<MagicItemEffect, ItemDrop.ItemData>>>();

        List<ItemDrop.ItemData> allEquipment = Player.m_localPlayer.GetEquipment();
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

        foreach (KeyValuePair<string, List<KeyValuePair<MagicItemEffect, ItemDrop.ItemData>>> entry in magicEffects)
        {
            string effectType = entry.Key;
            MagicItemEffectDefinition effectDef = MagicItemEffectDefinitions.Get(effectType);
            float sum = entry.Value.Sum(x => x.Key.EffectValue);
            string totalEffectText = MagicItem.GetEffectText(effectDef, sum);
            ItemRarity highestRarity = (ItemRarity) entry.Value.Max(x => (int) x.Value.GetRarity());

            List<string> content = new();
            foreach (KeyValuePair<MagicItemEffect, ItemDrop.ItemData> entry2 in entry.Value)
            {
                MagicItemEffect effect = entry2.Key;
                ItemDrop.ItemData item = entry2.Value;
                content.Add($" <color=#c0c0c0ff>- {MagicItem.GetEffectText(effect, item.GetRarity(), false)} ({item.GetDecoratedName()})</color>");
            }
            instance.MagicPagesTextArea.Add($"<size={MagicPages.mediumFontSize}><color={EpicLoot.GetRarityColor(highestRarity)}>{totalEffectText}</color></size>", content.ToArray());
        }
    }
}

public class ExplainInfo(string topic) : MagicInfo(topic)
{
    public override void Build(MagicPages instance)
    {
        IOrderedEnumerable<KeyValuePair<string, string>> sortedMagicEffects = MagicItemEffectDefinitions.AllDefinitions
            .Where(x => !x.Value.Requirements.NoRoll && x.Value.CanBeAugmented)
            .Select(x => new KeyValuePair<string, string>(string.Format(Localization.instance.Localize(x.Value.DisplayText),
                    "<b><color=yellow>X</color></b>"),
                Localization.instance.Localize(x.Value.Description)))
            .OrderBy(x => x.Key);
            
        foreach (KeyValuePair<string, string> kvp in sortedMagicEffects)
        {
            instance.MagicPagesTextArea.Add($"<size={MagicPages.largeFontSize}>{kvp.Key}</size>", $"<color=#c0c0c0ff>{kvp.Value}</color>", "");
        }
    }
}

public class TreasureBountyInfo(string topic) : MagicInfo(topic)
{
    public override void Build(MagicPages instance)
    {
        if (!Player.m_localPlayer) return;
        
        List<string> content = new();
        AdventureSaveData saveData = Player.m_localPlayer.GetAdventureSaveData();

        bool hasValues = false;
        
        if (saveData.TreasureMaps.Count > 0)
        {
            hasValues = true;
            IOrderedEnumerable<TreasureMapChestInfo> sortedTreasureMaps = saveData.TreasureMaps
                .Where(x => x.State == TreasureMapState.Purchased)
                .OrderBy(x => GetBiomeOrder(x.Biome));
            foreach (TreasureMapChestInfo treasureMap in sortedTreasureMaps)
            {
                content.Add(Localization.instance.Localize($"$mod_epicloot_merchant_treasuremaps: " +
                                                            $"<color={GetBiomeColor(treasureMap.Biome)}>$biome_{treasureMap.Biome.ToString().ToLower()} " +
                                                            $"#{treasureMap.Interval + 1}</color>"));
            }
            
            instance.MagicPagesTextArea.Add($"<color=orange><size={MagicPages.headerFontSize}>$mod_epicloot_merchant_treasuremaps</size></color>", content.ToArray());
            content.Clear();
        }

        if (saveData.Bounties.Count > 0)
        {
            hasValues = true;
            IOrderedEnumerable<BountyInfo> sortedBounties = saveData.Bounties.OrderBy(x => x.State);

            foreach (BountyInfo bounty in sortedBounties)
            {
                if (bounty.State != BountyState.InProgress && bounty.State != BountyState.Complete)
                {
                    continue;
                }

                string targetName = AdventureDataManager.GetBountyName(bounty);
                content.Add($"<size={MagicPages.largeFontSize}>{targetName}</size>  <color=#c0c0c0ff>$mod_epicloot_activebounties_classification: <color=#d66660>{AdventureDataManager.GetMonsterName(bounty.Target.MonsterID)}</color>, ");
                var info =
                    $" $mod_epicloot_activebounties_biome: <color={GetBiomeColor(bounty.Biome)}>$biome_{bounty.Biome.ToString().ToLower()}</color></color>";

                string status = "";
                switch (bounty.State)
                {
                    case BountyState.InProgress:
                        status = "<color=#00f0ff>$mod_epicloot_bounties_tooltip_inprogress</color>";
                        break;
                    case BountyState.Complete:
                        status = "<color=#70f56c>$mod_epicloot_bounties_tooltip_vanquished</color>";
                        break;
                }

                info += $"  <color=#c0c0c0ff>$mod_epicloot_bounties_tooltip_status {status}";

                content.Add(info);

                int iron = bounty.RewardIron;
                int gold = bounty.RewardGold;
                content.Add($", $mod_epicloot_bounties_tooltip_rewards {(iron > 0 ? $"<color=white>{MerchantPanel.GetIronBountyTokenName()} x{iron}</color>" : "")}{(iron > 0 && gold > 0 ? ", " : "")}{(gold > 0 ? $"<color=#f5da53>{MerchantPanel.GetGoldBountyTokenName()} x{gold}</color>" : "")}</color>"); 
            }
            instance.MagicPagesTextArea.Add($"<color=orange><size={MagicPages.headerFontSize}>$mod_epicloot_activebounties</size></color>", content.ToArray());
        }

        if (!hasValues)
        {
            //TODO: localize no active adventures text
            instance.MagicPagesTextArea.Add("No active adventures");
        }
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
}

public class MagicInfo(string topic, bool showSearchBar = true) : TextsDialog.TextInfo(topic, "")
{
    public readonly bool showSearchBar = showSearchBar;
    public virtual void Build(MagicPages instance){}
}

public static class FontManager
{
    public enum FontOptions
    {
        [InternalName("Norse")] Norse, 
        [InternalName("Norsebold") ]NorseBold, 
        [InternalName("AveriaSerifLibre-Regular")] AveriaSerifLibre,
        [InternalName("AveriaSerifLibre-Bold")] AveriaSerifLibreBold,
        [InternalName("AveriaSerifLibre-Light")] AveriaSerifLibreLight,
        [InternalName("LegacyRuntime")] LegacyRuntime
    }

    private class InternalName(string internalName) : Attribute
    {
        public readonly string internalName = internalName;
    }
    
    private static readonly Dictionary<FontOptions, Font> m_fonts = new();

    public static Font? GetFont(FontOptions option)
    {
        if (m_fonts.TryGetValue(option, out Font? font)) return font;
        Font[]? fonts = Resources.FindObjectsOfTypeAll<Font>();
        Font? match = fonts.FirstOrDefault(x => x.name == option.GetAttributeOfType<InternalName>().internalName);
        m_fonts[option] = match;
        return match;
    }
}