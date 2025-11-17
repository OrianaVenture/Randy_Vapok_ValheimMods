using EpicLoot.Config;
using HarmonyLib;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace EpicLoot
{
    public class WelcomeMessage : MonoBehaviour
    {
        public static void SetPlayerHasSeenMessage() {
            ELConfig.AlwaysShowWelcomeMessage.Value = false;
        }

        public void Awake()
        {
            var titleText = transform.Find("Title")?.GetComponent<Text>();
            titleText.text = Localization.instance.Localize(titleText.text) + $"{EpicLoot.Version}";

            var contentText = transform.Find("Content")?.GetComponent<Text>();
            contentText.text = Localization.instance.Localize(contentText.text);

            var discordButton = transform.Find("DiscordButton")?.GetComponent<Button>();
            var patchNotesButton = transform.Find("PatchNotesButton")?.GetComponent<Button>();
            var closeButton = transform.Find("CloseButton")?.GetComponent<Button>();

            var overhaulMinimalButton = transform.Find("overhaul_minimal")?.GetComponent<Button>();
            overhaulMinimalButton?.onClick.AddListener(SetOverhaulMinimalAndClick);
            var overhaulBalancedButton = transform.Find("overhaul_balanced")?.GetComponent<Button>();
            overhaulBalancedButton?.onClick.AddListener(SetOverhaulBalancedAndClick);
            var overhaulLegendaryButton = transform.Find("overhaul_legendary")?.GetComponent<Button>();
            overhaulLegendaryButton?.onClick.AddListener(SetOverhaulLegendaryAndClick);

            if (EpicLoot.HasAuga)
            {
                EpicLootAuga.ReplaceBackground(gameObject, true);
                EpicLootAuga.FixFonts(gameObject);
                if (discordButton != null)
                    discordButton = EpicLootAuga.ReplaceButton(discordButton);
                if (patchNotesButton != null)
                    patchNotesButton = EpicLootAuga.ReplaceButton(patchNotesButton);
                if (closeButton != null)
                    closeButton = EpicLootAuga.ReplaceButton(closeButton);
            }

            if (discordButton != null)
                discordButton.onClick.AddListener(OnJoinDiscordClick);
            if (patchNotesButton != null)
                patchNotesButton.onClick.AddListener(OnPatchNotesClick);
            if (closeButton != null)
                closeButton.onClick.AddListener(Close);
        }

        public void OnJoinDiscordClick()
        {
            Application.OpenURL("https://discord.gg/ZNhYeavv3C");
            Close();
        }

        public void OnPatchNotesClick()
        {
            Application.OpenURL("https://thunderstore.io/c/valheim/p/RandyKnapp/EpicLoot/changelog/");
            Close();
        }

        public void Close()
        {
            SetPlayerHasSeenMessage();
            Destroy(gameObject);
        }

        public void SetOverhaulBalancedAndClick()
        {
            ELConfig.BalanceConfigurationType.Value = "balanced";
            ELConfig.ItemsUnidentifiedDropRatio.Value = 0.8f;
            ELConfig.ItemsToMaterialsDropRatio.Value = 0.95f;
            OnOverhaulButtomClick();
            Close();
        }

        public void SetOverhaulMinimalAndClick()
        {
            ELConfig.BalanceConfigurationType.Value = "minimal";
            ELConfig.ItemsToMaterialsDropRatio.Value = 1.0f;
            OnOverhaulButtomClick();
            Close();
        }

        public void SetOverhaulLegendaryAndClick()
        {
            ELConfig.BalanceConfigurationType.Value = "legendary";
            ELConfig.ItemsUnidentifiedDropRatio.Value = 0.2f;
            ELConfig.ItemsToMaterialsDropRatio.Value = 0.1f;
            OnOverhaulButtomClick();
            Close();
        }

        public void OnOverhaulButtomClick() {
            string basecfglocation = Path.Combine(ELConfig.GetOverhaulDirectoryPath(), "magiceffects.json");
            var overhaulfiledata = EpicLoot.ReadEmbeddedResourceFile(ELConfig.GetDefaultEmbeddedFileLocation("magiceffects.json"));
            File.WriteAllText(basecfglocation, overhaulfiledata);
        }

    }

    [HarmonyPatch(typeof(FejdStartup), nameof(FejdStartup.Start))]
    public class WelcomeMessage_FejdStartup_Start_Patch
    {
        public static void Postfix(FejdStartup __instance)
        {
            if (ELConfig.AlwaysShowWelcomeMessage.Value)
            {
                var welcomeMessage = Object.Instantiate(EpicLoot.Assets.WelcomMessagePrefab, __instance.transform, false);
                welcomeMessage.name = "WelcomeMessage";
                welcomeMessage.AddComponent<WelcomeMessage>();
                welcomeMessage.GetComponent<WelcomeMessage>();
                ELConfig.AlwaysShowWelcomeMessage.Value = false;
            }
        }
    }
}
