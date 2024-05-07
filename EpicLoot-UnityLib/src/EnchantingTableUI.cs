﻿using UnityEngine;
using UnityEngine.UI;

namespace EpicLoot_UnityLib
{
    public class EnchantingTableUI : MonoBehaviour
    {
        public GameObject Root;
        public GameObject Scrim;
        public TabHandler TabHandler;
        public GameObject TabScrim;

        [Header("Content")]
        public EnchantingTableUIPanelBase[] Panels;

        [Header("Audio")]
        public AudioSource Audio;
        public AudioClip TabClickSFX;
        public AudioClip EnchantBonusSFX;

        public EnchantingTable SourceTable { get; private set; }

        public static EnchantingTableUI instance { get; set; }

        public delegate void AugaFixupDelegate(EnchantingTableUI ui);
        public static AugaFixupDelegate AugaFixup;
        public delegate void TabActivationDelegate(EnchantingTableUI ui);
        public static TabActivationDelegate TabActivation;

        private int _hiddenFrames;

        public void Awake()
        {
            instance = this;
            Localization.instance.Localize(transform);

            var uiSFX = GameObject.Find("sfx_gui_button");
            if (uiSFX)
            {
                Audio.outputAudioMixerGroup = 
                    uiSFX.GetComponent<AudioSource>().outputAudioMixerGroup;
            }

            AugaFixup(this);
        }

        private void SetupTabs()
        {
            for (var index = 0; index < TabHandler.m_tabs.Count; index++)
            {
                var tabData = TabHandler.m_tabs[index];
                tabData.m_onClick.AddListener(PlayTabSelectSFX);
                var featureStatus = tabData.m_button.gameObject.GetComponent<FeatureStatus>();
                if (featureStatus != null)
                {
                    featureStatus.Refresh();
                }
            }
            TabActivation(this);
        }

        public static void Show(GameObject enchantingUiPrefab, EnchantingTable source)
        {
            bool firstSetup = false;
            if (instance == null && StoreGui.instance != null)
            {
                var inGameGui = StoreGui.instance.transform.parent;
                var siblingIndex = StoreGui.instance.transform.GetSiblingIndex() + 1;
                var enchantingUI = Instantiate(enchantingUiPrefab, inGameGui);
                enchantingUI.transform.SetSiblingIndex(siblingIndex);
                firstSetup = true;
            }

            if (instance == null)
            {
                return;
            }

            instance.SourceTable = source;
            instance.Root.SetActive(true);
            instance.Scrim.SetActive(true);
            instance.SourceTable.Refresh();

            foreach (var panel in instance.Panels)
            {
                panel.DeselectAll();
            }

            if (firstSetup)
            {
                instance.SetupTabs();
            }
        }

        public static void Hide()
        {
            if (instance == null)
            {
                return;
            }

            instance.Root.SetActive(false);
            instance.Scrim.SetActive(false);
            instance.SourceTable = null;
        }

        public static bool IsVisible()
        {
            return instance != null && ((instance._hiddenFrames <= 2) ||
                (instance.Root != null && instance.Root.activeSelf));
        }

        public static bool IsInTextInput()
        {
            if (!IsVisible())
            {
                return false;
            }

            var textFields = instance.Root.GetComponentsInChildren<InputField>(false);
            foreach (var inputField in textFields)
            {
                if (inputField.isFocused)
                {
                    return true;
                }
            }

            return false;
        }

        public void Update()
        {
            if (Root == null)
            {
                return;
            }

            if (!Root.activeSelf)
            {
                _hiddenFrames++;
                return;
            }

            _hiddenFrames = 0;

            var disallowClose = (Chat.instance != null && Chat.instance.HasFocus()) || 
                Console.IsVisible() || Menu.IsVisible() || (TextViewer.instance != null && 
                TextViewer.instance.IsVisible()) || Player.m_localPlayer.InCutscene();

            if (disallowClose)
            {
                return;
            }

            var gotCloseInput = ZInput.GetButtonDown("JoyButtonB") ||
                ZInput.GetKeyDown(KeyCode.Escape) || ZInput.GetKeyDown(KeyCode.Tab);

            if (gotCloseInput)
            {
                ZInput.ResetButtonStatus("JoyButtonB");
                ZInput.ResetButtonStatus("JoyJump");

                var panelCapturedInput = false;
                foreach (var panel in Panels)
                {
                    if (panel.isActiveAndEnabled && panel.CanCancel())
                    {
                        panel.Cancel();
                        panelCapturedInput = true;
                        break;
                    }
                }

                if (!panelCapturedInput)
                {
                    Hide();
                }
            }
        }

        public static void UpdateTabActivation()
        {
            TabActivation(instance);
        }

        public static void UpdateUpgradeActivation()
        {
            TabActivation(instance);
        }

        public void LockTabs()
        {
            TabScrim.SetActive(true);
        }

        public void UnlockTabs()
        {
            TabScrim.SetActive(false);
        }

        public void PlayTabSelectSFX()
        {
            Audio.PlayOneShot(TabClickSFX);
        }

        public void PlayEnchantBonusSFX()
        {
            Audio.PlayOneShot(EnchantBonusSFX);
        }
    }
}
