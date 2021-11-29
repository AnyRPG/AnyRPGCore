using AnyRPG;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

namespace AnyRPG {
    public class SystemUIPanelController : WindowContentController {

        [Header("UI Settings")]
        public OnOffTextButton useQuestTrackerButton;
        public OnOffTextButton useActionBar2Button;
        public OnOffTextButton useActionBar3Button;
        public OnOffTextButton useActionBar4Button;
        public OnOffTextButton useActionBar5Button;
        public OnOffTextButton useActionBar6Button;
        public OnOffTextButton useActionBar7Button;
        public OnOffTextButton useFocusUnitFrameButton;
        public OnOffTextButton usePlayerUnitFrameButton;
        public OnOffTextButton useFloatingCastBarButton;
        public OnOffTextButton useMiniMapButton;
        public OnOffTextButton useExperienceBarButton;
        public OnOffTextButton useFloatingCombatTextButton;
        public OnOffTextButton useMessageFeedButton;
        public OnOffTextButton useStatusEffectBarButton;
        public OnOffTextButton useCombatLogButton;

        [Header("Player Interface Settings")]
        public OnOffTextButton showPlayerNameButton;
        public OnOffTextButton showPlayerFactionButton;
        public OnOffTextButton hideFullHealthBarButton;


        [Header("Lock / Unlock UI")]
        public OnOffTextButton lockUIButton;

        [Header("Opacity")]

        public Slider inventoryOpacitySlider;
        public Slider actionBarOpacitySlider;
        public Slider questTrackerOpacitySlider;
        public Slider combatLogOpacitySlider;
        public Slider systemMenuOpacitySlider;
        public Slider popupWindowOpacitySlider;
        public Slider pagedButtonsOpacitySlider;
        public Slider inventorySlotOpacitySlider;

        // ui opacity defaults
        private float defaultInventoryOpacity = 0.5f;
        private float defaultActionBarOpacity = 0.5f;
        private float defaultQuestTrackerOpacity = 0.3f;
        private float defaultPopupWindowOpacity = 0.8f;
        private float defaultPagedButtonsOpacity = 0.8f;
        private float defaultInventorySlotOpacity = 0.5f;
        private float defaultSystemMenuOpacity = 0.8f;
        private float defaultCombatLogOpacity = 0.8f;

        // ui element visibility defaults
        private int defaultUseQuestTracker = 1;
        private int defaultUseActionBar2 = 1;
        private int defaultUseActionBar3 = 1;
        private int defaultUseActionBar4 = 1;
        private int defaultUseActionBar5 = 1;
        private int defaultUseActionBar6 = 1;
        private int defaultUseActionBar7 = 1;
        private int defaultUseFocusUnitFrameButton = 1;
        private int defaultUsePlayerUnitFrameButton = 1;
        private int defaultUseFloatingCastBarButton = 1;
        private int defaultUseMiniMapButton = 1;
        private int defaultUseExperienceBarButton = 1;
        private int defaultUseFloatingCombatTextButton = 1;
        private int defaultUseMessageFeedButton = 1;
        private int defaultUseStatusEffectBarButton = 1;
        private int defaultLockUIButton = 1;
        private int defaultUseCombatLogButton = 1;
        private int defaultShowPlayerNameButton = 1;
        private int defaultShowPlayerFactionButton = 1;
        private int defaultHideFullHealthBarButton = 1;

        // game manager references
        private UIManager uIManager = null;
        private SaveManager saveManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            SetUIDefaults();
            //LoadUISettings();
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            uIManager = systemGameManager.UIManager;
            saveManager = systemGameManager.SaveManager;
        }

        public void ResetWindowPositions() {
            uIManager.LoadDefaultWindowPositions();
            saveManager.SaveWindowPositions();
        }

        public void SetUIDefaults() {
            if (!PlayerPrefs.HasKey("InventoryOpacity")) {
                PlayerPrefs.SetFloat("InventoryOpacity", defaultInventoryOpacity);
            }
            if (!PlayerPrefs.HasKey("InventorySlotOpacity")) {
                PlayerPrefs.SetFloat("InventorySlotOpacity", defaultInventorySlotOpacity);
            }
            if (!PlayerPrefs.HasKey("ActionBarOpacity")) {
                PlayerPrefs.SetFloat("ActionBarOpacity", defaultActionBarOpacity);
            }
            if (!PlayerPrefs.HasKey("QuestTrackerOpacity")) {
                PlayerPrefs.SetFloat("QuestTrackerOpacity", defaultQuestTrackerOpacity);
            }
            if (!PlayerPrefs.HasKey("CombatLogOpacity")) {
                PlayerPrefs.SetFloat("CombatLogOpacity", defaultCombatLogOpacity);
            }
            if (!PlayerPrefs.HasKey("PopupWindowOpacity")) {
                PlayerPrefs.SetFloat("PopupWindowOpacity", defaultPopupWindowOpacity);
            }
            if (!PlayerPrefs.HasKey("PagedButtonsOpacity")) {
                PlayerPrefs.SetFloat("PagedButtonsOpacity", defaultPagedButtonsOpacity);
            }
            if (!PlayerPrefs.HasKey("SystemMenuOpacity")) {
                PlayerPrefs.SetFloat("SystemMenuOpacity", defaultSystemMenuOpacity);
            }
            if (!PlayerPrefs.HasKey("UseQuestTracker")) {
                PlayerPrefs.SetInt("UseQuestTracker", defaultUseQuestTracker);
            }
            if (!PlayerPrefs.HasKey("UseActionBar2")) {
                PlayerPrefs.SetInt("UseActionBar2", defaultUseActionBar2);
            }
            if (!PlayerPrefs.HasKey("UseActionBar3")) {
                PlayerPrefs.SetInt("UseActionBar3", defaultUseActionBar3);
            }
            if (!PlayerPrefs.HasKey("UseActionBar4")) {
                PlayerPrefs.SetInt("UseActionBar4", defaultUseActionBar4);
            }
            if (!PlayerPrefs.HasKey("UseActionBar5")) {
                PlayerPrefs.SetInt("UseActionBar5", defaultUseActionBar5);
            }
            if (!PlayerPrefs.HasKey("UseActionBar6")) {
                PlayerPrefs.SetInt("UseActionBar6", defaultUseActionBar6);
            }
            if (!PlayerPrefs.HasKey("UseActionBar7")) {
                PlayerPrefs.SetInt("UseActionBar7", defaultUseActionBar7);
            }
            if (!PlayerPrefs.HasKey("UseFocusUnitFrame")) {
                PlayerPrefs.SetInt("UseFocusUnitFrame", defaultUseFocusUnitFrameButton);
            }
            if (!PlayerPrefs.HasKey("UsePlayerUnitFrame")) {
                PlayerPrefs.SetInt("UsePlayerUnitFrame", defaultUsePlayerUnitFrameButton);
            }
            if (!PlayerPrefs.HasKey("UseFloatingCastBar")) {
                PlayerPrefs.SetInt("UseFloatingCastBar", defaultUseFloatingCastBarButton);
            }
            if (!PlayerPrefs.HasKey("UseMiniMap")) {
                PlayerPrefs.SetInt("UseMiniMap", defaultUseMiniMapButton);
            }
            if (!PlayerPrefs.HasKey("UseExperienceBar")) {
                PlayerPrefs.SetInt("UseExperienceBar", defaultUseExperienceBarButton);
            }
            if (!PlayerPrefs.HasKey("UseFloatingCombatText")) {
                PlayerPrefs.SetInt("UseFloatingCombatText", defaultUseFloatingCombatTextButton);
            }
            if (!PlayerPrefs.HasKey("UseMessageFeed")) {
                PlayerPrefs.SetInt("UseMessageFeed", defaultUseMessageFeedButton);
            }
            if (!PlayerPrefs.HasKey("UseStatusEffectBar")) {
                PlayerPrefs.SetInt("UseStatusEffectBar", defaultUseStatusEffectBarButton);
            }
            if (!PlayerPrefs.HasKey("UseCombatLog")) {
                PlayerPrefs.SetInt("UseCombatLog", defaultUseCombatLogButton);
            }
            if (!PlayerPrefs.HasKey("LockUI")) {
                PlayerPrefs.SetInt("LockUI", defaultLockUIButton);
            }
            if (!PlayerPrefs.HasKey("ShowPlayerName")) {
                PlayerPrefs.SetInt("ShowPlayerName", defaultShowPlayerNameButton);
            }
            if (!PlayerPrefs.HasKey("ShowPlayerFaction")) {
                PlayerPrefs.SetInt("ShowPlayerFaction", defaultShowPlayerFactionButton);
            }
            if (!PlayerPrefs.HasKey("HideFullHealthBar")) {
                PlayerPrefs.SetInt("HideFullHealthBar", defaultHideFullHealthBarButton);
            }
        }

        public void ResetToDefaults() {

            PlayerPrefs.SetFloat("InventoryOpacity", defaultInventoryOpacity);
            PlayerPrefs.SetFloat("InventorySlotOpacity", defaultInventorySlotOpacity);
            PlayerPrefs.SetFloat("ActionBarOpacity", defaultActionBarOpacity);
            PlayerPrefs.SetFloat("QuestTrackerOpacity", defaultQuestTrackerOpacity);
            PlayerPrefs.SetFloat("CombatLogOpacity", defaultCombatLogOpacity);
            PlayerPrefs.SetFloat("PopupWindowOpacity", defaultPopupWindowOpacity);
            PlayerPrefs.SetFloat("PagedButtonsOpacity", defaultPagedButtonsOpacity);
            PlayerPrefs.SetFloat("SystemMenuOpacity", defaultSystemMenuOpacity);
            PlayerPrefs.SetInt("UseQuestTracker", defaultUseQuestTracker);
            PlayerPrefs.SetInt("UseActionBar2", defaultUseActionBar2);
            PlayerPrefs.SetInt("UseActionBar3", defaultUseActionBar3);
            PlayerPrefs.SetInt("UseActionBar4", defaultUseActionBar4);
            PlayerPrefs.SetInt("UseActionBar5", defaultUseActionBar5);
            PlayerPrefs.SetInt("UseActionBar6", defaultUseActionBar6);
            PlayerPrefs.SetInt("UseActionBar7", defaultUseActionBar7);
            PlayerPrefs.SetInt("UseFocusUnitFrame", defaultUseFocusUnitFrameButton);
            PlayerPrefs.SetInt("UsePlayerUnitFrame", defaultUsePlayerUnitFrameButton);
            PlayerPrefs.SetInt("UseFloatingCastBar", defaultUseFloatingCastBarButton);
            PlayerPrefs.SetInt("UseMiniMap", defaultUseMiniMapButton);
            PlayerPrefs.SetInt("UseExperienceBar", defaultUseExperienceBarButton);
            PlayerPrefs.SetInt("UseFloatingCombatText", defaultUseFloatingCombatTextButton);
            PlayerPrefs.SetInt("UseMessageFeed", defaultUseMessageFeedButton);
            PlayerPrefs.SetInt("UseStatusEffectBar", defaultUseStatusEffectBarButton);
            PlayerPrefs.SetInt("UseCombatLog", defaultUseCombatLogButton);
            PlayerPrefs.SetInt("LockUI", defaultLockUIButton);
            PlayerPrefs.SetInt("ShowPlayerName", defaultShowPlayerNameButton);
            PlayerPrefs.SetInt("ShowPlayerFaction", defaultShowPlayerFactionButton);
            PlayerPrefs.SetInt("HideFullHealthBar", defaultHideFullHealthBarButton);

            LoadUISettings();
        }


        public override void Init() {
            base.Init();
            // this must be done in start because the defaults are set in the UI manager after all the Configure() calls are made
            LoadUISettings();
        }
        

        private void LoadUISettings() {
            //Debug.Log("MainSettingsMenuController.LoadUISettings()");
            
            inventoryOpacitySlider.value = PlayerPrefs.GetFloat("InventoryOpacity");
            inventorySlotOpacitySlider.value = PlayerPrefs.GetFloat("InventorySlotOpacity");
            actionBarOpacitySlider.value = PlayerPrefs.GetFloat("ActionBarOpacity");
            questTrackerOpacitySlider.value = PlayerPrefs.GetFloat("QuestTrackerOpacity");
            combatLogOpacitySlider.value = PlayerPrefs.GetFloat("CombatLogOpacity");
            popupWindowOpacitySlider.value = PlayerPrefs.GetFloat("PopupWindowOpacity");
            pagedButtonsOpacitySlider.value = PlayerPrefs.GetFloat("PagedButtonsOpacity");
            systemMenuOpacitySlider.value = PlayerPrefs.GetFloat("SystemMenuOpacity");

            if (PlayerPrefs.GetInt("UseQuestTracker") == 0) {
                useQuestTrackerButton.SetOff();
            } else if (PlayerPrefs.GetInt("UseQuestTracker") == 1) {
                useQuestTrackerButton.SetOn();
            }
            if (PlayerPrefs.GetInt("UseActionBar2") == 0) {
                useActionBar2Button.SetOff();
            } else if (PlayerPrefs.GetInt("UseActionBar2") == 1) {
                useActionBar2Button.SetOn();
            }
            if (PlayerPrefs.GetInt("UseActionBar3") == 0) {
                useActionBar3Button.SetOff();
            } else if (PlayerPrefs.GetInt("UseActionBar3") == 1) {
                useActionBar3Button.SetOn();
            }
            if (PlayerPrefs.GetInt("UseActionBar4") == 0) {
                useActionBar4Button.SetOff();
            } else if (PlayerPrefs.GetInt("UseActionBar4") == 1) {
                useActionBar4Button.SetOn();
            }
            if (PlayerPrefs.GetInt("UseActionBar5") == 0) {
                useActionBar5Button.SetOff();
            } else if (PlayerPrefs.GetInt("UseActionBar5") == 1) {
                useActionBar5Button.SetOn();
            }
            if (PlayerPrefs.GetInt("UseActionBar6") == 0) {
                useActionBar6Button.SetOff();
            } else if (PlayerPrefs.GetInt("UseActionBar6") == 1) {
                useActionBar6Button.SetOn();
            }
            if (PlayerPrefs.GetInt("UseActionBar7") == 0) {
                useActionBar7Button.SetOff();
            } else if (PlayerPrefs.GetInt("UseActionBar7") == 1) {
                useActionBar7Button.SetOn();
            }

            if (PlayerPrefs.GetInt("UseMessageFeed") == 0) {
                useMessageFeedButton.SetOff();
            } else if (PlayerPrefs.GetInt("UseMessageFeed") == 1) {
                useMessageFeedButton.SetOn();
            }

            if (PlayerPrefs.GetInt("UseFloatingCombatText") == 0) {
                useFloatingCombatTextButton.SetOff();
            } else if (PlayerPrefs.GetInt("UseFloatingCombatText") == 1) {
                useFloatingCombatTextButton.SetOn();
            }

            if (PlayerPrefs.GetInt("UseExperienceBar") == 0) {
                useExperienceBarButton.SetOff();
            } else if (PlayerPrefs.GetInt("UseExperienceBar") == 1) {
                useExperienceBarButton.SetOn();
            }

            if (PlayerPrefs.GetInt("UseMiniMap") == 0) {
                useMiniMapButton.SetOff();
            } else if (PlayerPrefs.GetInt("UseMiniMap") == 1) {
                useMiniMapButton.SetOn();
            }

            if (PlayerPrefs.GetInt("UseFloatingCastBar") == 0) {
                useFloatingCastBarButton.SetOff();
            } else if (PlayerPrefs.GetInt("UseFloatingCastBar") == 1) {
                useFloatingCastBarButton.SetOn();
            }

            if (PlayerPrefs.GetInt("UsePlayerUnitFrame") == 0) {
                usePlayerUnitFrameButton.SetOff();
            } else if (PlayerPrefs.GetInt("UsePlayerUnitFrame") == 1) {
                usePlayerUnitFrameButton.SetOn();
            }

            if (PlayerPrefs.GetInt("UseFocusUnitFrame") == 0) {
                useFocusUnitFrameButton.SetOff();
            } else if (PlayerPrefs.GetInt("UseFocusUnitFrame") == 1) {
                useFocusUnitFrameButton.SetOn();
            }

            if (PlayerPrefs.GetInt("UseStatusEffectBar") == 0) {
                useStatusEffectBarButton.SetOff();
            } else if (PlayerPrefs.GetInt("UseStatusEffectBar") == 1) {
                useStatusEffectBarButton.SetOn();
            }

            if (PlayerPrefs.GetInt("UseCombatLog") == 0) {
                useCombatLogButton.SetOff();
            } else if (PlayerPrefs.GetInt("UseCombatLog") == 1) {
                useCombatLogButton.SetOn();
            }

            if (PlayerPrefs.GetInt("LockUI") == 0) {
                lockUIButton.SetOff();
            } else if (PlayerPrefs.GetInt("LockUI") == 1) {
                lockUIButton.SetOn();
            }

            if (PlayerPrefs.GetInt("ShowPlayerName") == 0) {
                showPlayerNameButton.SetOff();
            } else if (PlayerPrefs.GetInt("ShowPlayerName") == 1) {
                showPlayerNameButton.SetOn();
            }

            if (PlayerPrefs.GetInt("ShowPlayerFaction") == 0) {
                showPlayerFactionButton.SetOff();
            } else if (PlayerPrefs.GetInt("ShowPlayerFaction") == 1) {
                showPlayerFactionButton.SetOn();
            }

            if (PlayerPrefs.GetInt("HideFullHealthBar") == 0) {
                hideFullHealthBarButton.SetOff();
            } else if (PlayerPrefs.GetInt("HideFullHealthBar") == 1) {
                hideFullHealthBarButton.SetOn();
            }

            uIManager.CheckUISettings(!uIManager.PlayerUI.activeSelf);

        }

        public void ToggleUseQuestTracker() {
            if (PlayerPrefs.GetInt("UseQuestTracker") == 0) {
                PlayerPrefs.SetInt("UseQuestTracker", 1);
                uIManager.MessageFeedManager.WriteMessage("Quest Tracker: on");
                useQuestTrackerButton.SetOn();
            } else {
                PlayerPrefs.SetInt("UseQuestTracker", 0);
                uIManager.MessageFeedManager.WriteMessage("Quest Tracker: off");
                useQuestTrackerButton.SetOff();
            }
            uIManager.CheckQuestTrackerSettings();
        }

        public void ToggleUseCombatLog() {
            if (PlayerPrefs.GetInt("UseCombatLog") == 0) {
                PlayerPrefs.SetInt("UseCombatLog", 1);
                uIManager.MessageFeedManager.WriteMessage("Combat Log: on");
                useCombatLogButton.SetOn();
            } else {
                PlayerPrefs.SetInt("UseCombatLog", 0);
                uIManager.MessageFeedManager.WriteMessage("Combat Log: off");
                useCombatLogButton.SetOff();
            }
            uIManager.CheckCombatLogSettings();
        }

        public void ToggleUseActionBar2() {
            if (PlayerPrefs.GetInt("UseActionBar2") == 0) {
                PlayerPrefs.SetInt("UseActionBar2", 1);
                uIManager.MessageFeedManager.WriteMessage("Action Bar 2: on");
                useActionBar2Button.SetOn();
            } else {
                PlayerPrefs.SetInt("UseActionBar2", 0);
                uIManager.MessageFeedManager.WriteMessage("Action Bar 2: off");
                useActionBar2Button.SetOff();
            }
            uIManager.UpdateActionBars();
        }

        public void ToggleUseActionBar3() {
            if (PlayerPrefs.GetInt("UseActionBar3") == 0) {
                PlayerPrefs.SetInt("UseActionBar3", 1);
                uIManager.MessageFeedManager.WriteMessage("Action Bar 3: on");
                useActionBar3Button.SetOn();
            } else {
                PlayerPrefs.SetInt("UseActionBar3", 0);
                uIManager.MessageFeedManager.WriteMessage("Action Bar 3: on");
                useActionBar3Button.SetOff();
            }
            uIManager.UpdateActionBars();
        }

        public void ToggleUseActionBar4() {
            if (PlayerPrefs.GetInt("UseActionBar4") == 0) {
                PlayerPrefs.SetInt("UseActionBar4", 1);
                uIManager.MessageFeedManager.WriteMessage("Action Bar 4: on");
                useActionBar4Button.SetOn();
            } else {
                PlayerPrefs.SetInt("UseActionBar4", 0);
                uIManager.MessageFeedManager.WriteMessage("Action Bar 4: off");
                useActionBar4Button.SetOff();
            }
            uIManager.UpdateActionBars();
        }

        public void ToggleUseActionBar5() {
            if (PlayerPrefs.GetInt("UseActionBar5") == 0) {
                PlayerPrefs.SetInt("UseActionBar5", 1);
                uIManager.MessageFeedManager.WriteMessage("Action Bar 5: on");
                useActionBar5Button.SetOn();
            } else {
                PlayerPrefs.SetInt("UseActionBar5", 0);
                uIManager.MessageFeedManager.WriteMessage("Action Bar 5: on");
                useActionBar5Button.SetOff();
            }
            uIManager.UpdateActionBars();
        }

        public void ToggleUseActionBar6() {
            if (PlayerPrefs.GetInt("UseActionBar6") == 0) {
                PlayerPrefs.SetInt("UseActionBar6", 1);
                uIManager.MessageFeedManager.WriteMessage("Action Bar 6: on");
                useActionBar6Button.SetOn();
            } else {
                PlayerPrefs.SetInt("UseActionBar6", 0);
                uIManager.MessageFeedManager.WriteMessage("Action Bar 6: off");
                useActionBar6Button.SetOff();
            }
            uIManager.UpdateActionBars();
        }

        public void ToggleUseActionBar7() {
            if (PlayerPrefs.GetInt("UseActionBar7") == 0) {
                PlayerPrefs.SetInt("UseActionBar7", 1);
                uIManager.MessageFeedManager.WriteMessage("Action Bar 7: on");
                useActionBar7Button.SetOn();
            } else {
                PlayerPrefs.SetInt("UseActionBar7", 0);
                uIManager.MessageFeedManager.WriteMessage("Action Bar 7: off");
                useActionBar7Button.SetOff();
            }
            uIManager.UpdateActionBars();
        }

        public void ToggleUseStatusEffectBar() {
            if (PlayerPrefs.GetInt("UseStatusEffectBar") == 0) {
                PlayerPrefs.SetInt("UseStatusEffectBar", 1);
                uIManager.MessageFeedManager.WriteMessage("Status Effect Bar: on");
                useStatusEffectBarButton.SetOn();
            } else {
                PlayerPrefs.SetInt("UseStatusEffectBar", 0);
                uIManager.MessageFeedManager.WriteMessage("Status Effect Bar: off");
                useStatusEffectBarButton.SetOff();
            }
            uIManager.UpdateStatusEffectBar();
        }

        public void ToggleUseFocusUnitFrameButton() {
            if (PlayerPrefs.GetInt("UseFocusUnitFrame") == 0) {
                PlayerPrefs.SetInt("UseFocusUnitFrame", 1);
                uIManager.MessageFeedManager.WriteMessage("Focus Unit Frame: on");
                useFocusUnitFrameButton.SetOn();
            } else {
                PlayerPrefs.SetInt("UseFocusUnitFrame", 0);
                uIManager.MessageFeedManager.WriteMessage("Focus Unit Frame: off");
                useFocusUnitFrameButton.SetOff();
            }
            uIManager.UpdateFocusUnitFrame();
        }

        public void ToggleUsePlayerUnitFrameButton() {
            if (PlayerPrefs.GetInt("UsePlayerUnitFrame") == 0) {
                PlayerPrefs.SetInt("UsePlayerUnitFrame", 1);
                uIManager.MessageFeedManager.WriteMessage("Player Unit Frame: on");
                usePlayerUnitFrameButton.SetOn();
            } else {
                PlayerPrefs.SetInt("UsePlayerUnitFrame", 0);
                uIManager.MessageFeedManager.WriteMessage("Player Unit Frame: off");
                usePlayerUnitFrameButton.SetOff();
            }
            uIManager.UpdatePlayerUnitFrame();
        }

        public void ToggleUseFloatingCastBarButton() {
            if (PlayerPrefs.GetInt("UseFloatingCastBar") == 0) {
                PlayerPrefs.SetInt("UseFloatingCastBar", 1);
                uIManager.MessageFeedManager.WriteMessage("Floating Cast Bar: on");
                useFloatingCastBarButton.SetOn();
            } else {
                PlayerPrefs.SetInt("UseFloatingCastBar", 0);
                uIManager.MessageFeedManager.WriteMessage("Floating Cast Bar: off");
                useFloatingCastBarButton.SetOff();
            }
            uIManager.UpdateFloatingCastBar();
        }

        public void ToggleUseMiniMapButton() {
            if (PlayerPrefs.GetInt("UseMiniMap") == 0) {
                PlayerPrefs.SetInt("UseMiniMap", 1);
                useMiniMapButton.SetOn();
                uIManager.MessageFeedManager.WriteMessage("Minimap: on");
            } else {
                PlayerPrefs.SetInt("UseMiniMap", 0);
                useMiniMapButton.SetOff();
                uIManager.MessageFeedManager.WriteMessage("Minimap: off");
            }
            uIManager.UpdateMiniMap();
        }

        public void ToggleUseExperienceBarButton() {
            if (PlayerPrefs.GetInt("UseExperienceBar") == 0) {
                PlayerPrefs.SetInt("UseExperienceBar", 1);
                uIManager.MessageFeedManager.WriteMessage("Experience Bar: on");
                useExperienceBarButton.SetOn();
            } else {
                PlayerPrefs.SetInt("UseExperienceBar", 0);
                uIManager.MessageFeedManager.WriteMessage("Experience Bar: off");
                useExperienceBarButton.SetOff();
            }
            uIManager.UpdateExperienceBar();
        }

        public void ToggleUseFloatingCombatTextButton() {
            if (PlayerPrefs.GetInt("UseFloatingCombatText") == 0) {
                PlayerPrefs.SetInt("UseFloatingCombatText", 1);
                uIManager.MessageFeedManager.WriteMessage("Floating Combat Text: on");
                useFloatingCombatTextButton.SetOn();
            } else {
                PlayerPrefs.SetInt("UseFloatingCombatText", 0);
                uIManager.MessageFeedManager.WriteMessage("Floating Combat Text: off");
                useFloatingCombatTextButton.SetOff();
            }
            uIManager.UpdateFloatingCombatText();
        }

        public void ToggleUseMessageFeedButton() {
            if (PlayerPrefs.GetInt("UseMessageFeed") == 0) {
                PlayerPrefs.SetInt("UseMessageFeed", 1);
                uIManager.MessageFeedManager.WriteMessage("Use Message Feed: on");
                useMessageFeedButton.SetOn();
            } else {
                uIManager.MessageFeedManager.WriteMessage("Use Message Feed: off");
                PlayerPrefs.SetInt("UseMessageFeed", 0);
                useMessageFeedButton.SetOff();
            }
            uIManager.UpdateMessageFeed();
        }

        public void ToggleShowPlayerNameButton() {
            Debug.Log("MainSettingsMenuController.ToggleShowPlayerNameButton()");
            if (PlayerPrefs.GetInt("ShowPlayerName") == 0) {
                PlayerPrefs.SetInt("ShowPlayerName", 1);
                //Debug.Log("MainSettingsMenuController.ToggleShowPlayerNameButton(): showplayername now set to 1");
                uIManager.MessageFeedManager.WriteMessage("Show Player Name: on");
                showPlayerNameButton.SetOn();
            } else {
                uIManager.MessageFeedManager.WriteMessage("Show Player Name: off");
                PlayerPrefs.SetInt("ShowPlayerName", 0);
                //Debug.Log("MainSettingsMenuController.ToggleShowPlayerNameButton(): showplayername now set to 0");
                showPlayerNameButton.SetOff();
            }
            // not really reputation, but it will trigger the right check
            SystemEventManager.TriggerEvent("OnReputationChange", new EventParamProperties());
        }

        public void ToggleShowPlayerFactionButton() {
            //Debug.Log("MainSettingsMenuController.ToggleShowPlayerFactionButton()");
            if (PlayerPrefs.GetInt("ShowPlayerFaction") == 0) {
                PlayerPrefs.SetInt("ShowPlayerFaction", 1);
                //Debug.Log("MainSettingsMenuController.ToggleShowPlayerNameButton(): showplayername now set to 1");
                uIManager.MessageFeedManager.WriteMessage("Show Player Faction: on");
                showPlayerFactionButton.SetOn();
            } else {
                uIManager.MessageFeedManager.WriteMessage("Show Player Faction: off");
                PlayerPrefs.SetInt("ShowPlayerFaction", 0);
                //Debug.Log("MainSettingsMenuController.ToggleShowPlayerNameButton(): showplayername now set to 0");
                showPlayerFactionButton.SetOff();
            }
            // not really reputation, but it will trigger the right check
            SystemEventManager.TriggerEvent("OnReputationChange", new EventParamProperties());
        }

        public void ToggleHideFullHealthBarButton() {
            //Debug.Log("MainSettingsMenuController.ToggleHideFullHealthBarButton()");
            if (PlayerPrefs.GetInt("HideFullHealthBar") == 0) {
                PlayerPrefs.SetInt("HideFullHealthBar", 1);
                uIManager.MessageFeedManager.WriteMessage("Hide Full Healthbar: on");
                hideFullHealthBarButton.SetOn();
            } else {
                uIManager.MessageFeedManager.WriteMessage("Hide full healthbar: off");
                PlayerPrefs.SetInt("HideFullHealthBar", 0);
                hideFullHealthBarButton.SetOff();
            }
            // not really reputation, but it will trigger the right check
            SystemEventManager.TriggerEvent("OnReputationChange", new EventParamProperties());
        }

        public void ToggleLockUIButton() {
            if (PlayerPrefs.GetInt("LockUI") == 0) {
                PlayerPrefs.SetInt("LockUI", 1);
                uIManager.MessageFeedManager.WriteMessage("Lock UI: on");
                lockUIButton.SetOn();
            } else {
                uIManager.MessageFeedManager.WriteMessage("Lock UI: off");
                PlayerPrefs.SetInt("LockUI", 0);
                lockUIButton.SetOff();
            }
            uIManager.UpdateLockUI();
        }

        public void InventoryOpacitySliderUpdate() {
            if (configureCount == 0) {
                return;
            }
            PlayerPrefs.SetFloat("InventoryOpacity", inventoryOpacitySlider.value);
            uIManager.UpdateInventoryOpacity();
            SystemEventManager.TriggerEvent("OnInventoryTransparencyUpdate", new EventParamProperties());
        }

        public void InventorySlotOpacitySliderUpdate() {
            if (configureCount == 0) {
                return;
            }
            PlayerPrefs.SetFloat("InventorySlotOpacity", inventorySlotOpacitySlider.value);
            uIManager.UpdateInventoryOpacity();
            SystemEventManager.TriggerEvent("OnInventoryTransparencyUpdate", new EventParamProperties());
        }

        public void ActionBarOpacitySliderUpdate() {
            if (configureCount == 0) {
                return;
            }
            PlayerPrefs.SetFloat("ActionBarOpacity", actionBarOpacitySlider.value);
            uIManager.UpdateActionBarOpacity();
        }

        public void QuestTrackerOpacitySliderUpdate() {
            if (configureCount == 0) {
                return;
            }
            PlayerPrefs.SetFloat("QuestTrackerOpacity", questTrackerOpacitySlider.value);
            uIManager.UpdateQuestTrackerOpacity();
        }

        public void CombatLogOpacitySliderUpdate() {
            if (configureCount == 0) {
                return;
            }
            PlayerPrefs.SetFloat("CombatLogOpacity", combatLogOpacitySlider.value);
            uIManager.UpdateCombatLogOpacity();
        }

        public void PopupWindowOpacitySliderUpdate() {
            if (configureCount == 0) {
                return;
            }
            PlayerPrefs.SetFloat("PopupWindowOpacity", popupWindowOpacitySlider.value);
            uIManager.UpdatePopupWindowOpacity();
        }

        public void PagedButtonsOpacitySliderUpdate() {
            //Debug.Log("MainSettingsMenuController.PagedButtonsOpacitySliderUpdate()");
            if (configureCount == 0) {
                return;
            }
            PlayerPrefs.SetFloat("PagedButtonsOpacity", pagedButtonsOpacitySlider.value);
            SystemEventManager.TriggerEvent("OnPagedButtonsTransparencyUpdate", new EventParamProperties());
        }

        public void SystemMenuOpacitySliderUpdate() {
            if (configureCount == 0) {
                return;
            }
            PlayerPrefs.SetFloat("SystemMenuOpacity", systemMenuOpacitySlider.value);
            uIManager.UpdateSystemMenuOpacity();
        }


    }
}