using AnyRPG;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

namespace AnyRPG {
    public class MainSettingsMenuController : WindowContentController {

        // toggle buttons
        public HighlightButton soundButton;
        public HighlightButton controlsButton;
        public HighlightButton videoButton;
        public HighlightButton keyBindingsButton;
        public HighlightButton userInterfaceButton;

        [Header("GAME SETTINGS")]
        public GameObject showhudtext;
        public GameObject tooltipstext;
        public GameObject difficultynormaltext;
        public GameObject difficultynormaltextLINE;
        public GameObject difficultyhardcoretext;
        public GameObject difficultyhardcoretextLINE;

        [Header("CONTROLS SETTINGS")]

        // sliders
        public Slider masterSlider;
        public Slider musicSlider;
        public Slider effectsSlider;
        public Slider ambientSlider;
        public Slider uiSlider;
        public Slider voiceSlider;
        public Slider mouseLookSpeedSlider;
        public Slider mouseTurnSpeedSlider;
        public Slider keyboardTurnSpeedSlider;
        public OnOffTextButton invertMouseButton;

        [Header("UI SETTINGS")]
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

        [Header("Player Interface SETTINGS")]
        public OnOffTextButton showPlayerNameButton;
        public OnOffTextButton showPlayerFactionButton;
        public OnOffTextButton hideFullHealthBarButton;


        // allow dragging of all UI elements
        public OnOffTextButton lockUIButton;

        // sliders
        public Slider inventoryOpacitySlider;
        public Slider actionBarOpacitySlider;
        public Slider questTrackerOpacitySlider;
        public Slider combatLogOpacitySlider;
        public Slider systemMenuOpacitySlider;
        public Slider popupWindowOpacitySlider;
        public Slider pagedButtonsOpacitySlider;
        public Slider inventorySlotOpacitySlider;

        private float defaultInventoryOpacity = 0.5f;
        private float defaultActionBarOpacity = 0.5f;
        private float defaultQuestTrackerOpacity = 0.3f;
        private float defaultPopupWindowOpacity = 0.8f;
        private float defaultPagedButtonsOpacity = 0.8f;
        private float defaultInventorySlotOpacity = 0.5f;
        private float defaultSystemMenuOpacity = 0.8f;
        private float defaultCombatLogOpacity = 0.8f;
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


        private float defaultMouseLookSpeed = 0.5f;
        private float defaultMouseTurnSpeed = 0.5f;
        private float defaultKeyboardTurnSpeed = 0.5f;
        private int defaultInvertMouse = 0;

        [Header("Panels")]
        [Tooltip("The UI Panel that holds the GAME window tab")]
        public GameObject PanelSound;
        [Tooltip("The UI Panel that holds the CONTROLS window tab")]
        public GameObject PanelControls;
        [Tooltip("The UI Panel that holds the VIDEO window tab")]
        public GameObject PanelVideo;
        [Tooltip("The UI Panel that holds the KEY BINDINGS window tab")]
        public GameObject PanelKeyBindings;
        [Tooltip("The UI Panel that holds the USER INTERFACE window tab")]
        public GameObject PanelUserInterface;

        public void Start() {

            //LoadDifficultySettings();
            LoadVolumeSliderValues();
            LoadUISettings();
            LoadControlsSettings();

        }

        private void LoadUISettings() {
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


            inventoryOpacitySlider.value = PlayerPrefs.GetFloat("InventoryOpacity");
            inventorySlotOpacitySlider.value = PlayerPrefs.GetFloat("InventorySlotOpacity");
            actionBarOpacitySlider.value = PlayerPrefs.GetFloat("ActionBarOpacity");
            questTrackerOpacitySlider.value = PlayerPrefs.GetFloat("QuestTrackerOpacity");
            combatLogOpacitySlider.value = PlayerPrefs.GetFloat("CombatLogOpacity");
            popupWindowOpacitySlider.value = PlayerPrefs.GetFloat("PopupWindowOpacity");
            pagedButtonsOpacitySlider.value = PlayerPrefs.GetFloat("PagedButtonsOpacity");
            systemMenuOpacitySlider.value = PlayerPrefs.GetFloat("SystemMenuOpacity");

            if (PlayerPrefs.GetInt("UseQuestTracker") == 0) {
                useQuestTrackerButton.DeSelect();
            } else if (PlayerPrefs.GetInt("UseQuestTracker") == 1) {
                useQuestTrackerButton.Select();
            }
            if (PlayerPrefs.GetInt("UseActionBar2") == 0) {
                useActionBar2Button.DeSelect();
            } else if (PlayerPrefs.GetInt("UseActionBar2") == 1) {
                useActionBar2Button.Select();
            }
            if (PlayerPrefs.GetInt("UseActionBar3") == 0) {
                useActionBar3Button.DeSelect();
            } else if (PlayerPrefs.GetInt("UseActionBar3") == 1) {
                useActionBar3Button.Select();
            }
            if (PlayerPrefs.GetInt("UseActionBar4") == 0) {
                useActionBar4Button.DeSelect();
            } else if (PlayerPrefs.GetInt("UseActionBar4") == 1) {
                useActionBar4Button.Select();
            }
            if (PlayerPrefs.GetInt("UseActionBar5") == 0) {
                useActionBar5Button.DeSelect();
            } else if (PlayerPrefs.GetInt("UseActionBar5") == 1) {
                useActionBar5Button.Select();
            }
            if (PlayerPrefs.GetInt("UseActionBar6") == 0) {
                useActionBar6Button.DeSelect();
            } else if (PlayerPrefs.GetInt("UseActionBar6") == 1) {
                useActionBar6Button.Select();
            }
            if (PlayerPrefs.GetInt("UseActionBar7") == 0) {
                useActionBar7Button.DeSelect();
            } else if (PlayerPrefs.GetInt("UseActionBar7") == 1) {
                useActionBar7Button.Select();
            }

            if (PlayerPrefs.GetInt("UseMessageFeed") == 0) {
                useMessageFeedButton.DeSelect();
            } else if (PlayerPrefs.GetInt("UseMessageFeed") == 1) {
                useMessageFeedButton.Select();
            }

            if (PlayerPrefs.GetInt("UseFloatingCombatText") == 0) {
                useFloatingCombatTextButton.DeSelect();
            } else if (PlayerPrefs.GetInt("UseFloatingCombatText") == 1) {
                useFloatingCombatTextButton.Select();
            }

            if (PlayerPrefs.GetInt("UseExperienceBar") == 0) {
                useExperienceBarButton.DeSelect();
            } else if (PlayerPrefs.GetInt("UseExperienceBar") == 1) {
                useExperienceBarButton.Select();
            }

            if (PlayerPrefs.GetInt("UseMiniMap") == 0) {
                useMiniMapButton.DeSelect();
            } else if (PlayerPrefs.GetInt("UseMiniMap") == 1) {
                useMiniMapButton.Select();
            }

            if (PlayerPrefs.GetInt("UseFloatingCastBar") == 0) {
                useFloatingCastBarButton.DeSelect();
            } else if (PlayerPrefs.GetInt("UseFloatingCastBar") == 1) {
                useFloatingCastBarButton.Select();
            }

            if (PlayerPrefs.GetInt("UsePlayerUnitFrame") == 0) {
                usePlayerUnitFrameButton.DeSelect();
            } else if (PlayerPrefs.GetInt("UsePlayerUnitFrame") == 1) {
                usePlayerUnitFrameButton.Select();
            }

            if (PlayerPrefs.GetInt("UseFocusUnitFrame") == 0) {
                useFocusUnitFrameButton.DeSelect();
            } else if (PlayerPrefs.GetInt("UseFocusUnitFrame") == 1) {
                useFocusUnitFrameButton.Select();
            }

            if (PlayerPrefs.GetInt("UseStatusEffectBar") == 0) {
                useStatusEffectBarButton.DeSelect();
            } else if (PlayerPrefs.GetInt("UseStatusEffectBar") == 1) {
                useStatusEffectBarButton.Select();
            }

            if (PlayerPrefs.GetInt("UseCombatLog") == 0) {
                useCombatLogButton.DeSelect();
            } else if (PlayerPrefs.GetInt("UseCombatLog") == 1) {
                useCombatLogButton.Select();
            }

            if (PlayerPrefs.GetInt("LockUI") == 0) {
                lockUIButton.DeSelect();
            } else if (PlayerPrefs.GetInt("LockUI") == 1) {
                lockUIButton.Select();
            }

            if (PlayerPrefs.GetInt("ShowPlayerName") == 0) {
                showPlayerNameButton.DeSelect();
            } else if (PlayerPrefs.GetInt("ShowPlayerName") == 1) {
                showPlayerNameButton.Select();
            }

            if (PlayerPrefs.GetInt("ShowPlayerFaction") == 0) {
                showPlayerFactionButton.DeSelect();
            } else if (PlayerPrefs.GetInt("ShowPlayerFaction") == 1) {
                showPlayerFactionButton.Select();
            }

            if (PlayerPrefs.GetInt("HideFullHealthBar") == 0) {
                hideFullHealthBarButton.DeSelect();
            } else if (PlayerPrefs.GetInt("HideFullHealthBar") == 1) {
                hideFullHealthBarButton.Select();
            }

            UIManager.MyInstance.CheckUISettings(true);

        }

        private void LoadControlsSettings() {
            if (!PlayerPrefs.HasKey("MouseLookSpeed")) {
                PlayerPrefs.SetFloat("MouseLookSpeed", defaultMouseLookSpeed);
            }
            if (!PlayerPrefs.HasKey("MouseTurnSpeed")) {
                PlayerPrefs.SetFloat("MouseTurnSpeed", defaultMouseTurnSpeed);
            }
            if (!PlayerPrefs.HasKey("KeyboardTurnSpeed")) {
                PlayerPrefs.SetFloat("KeyboardTurnSpeed", defaultKeyboardTurnSpeed);
            }
            if (!PlayerPrefs.HasKey("MouseInvert")) {
                PlayerPrefs.SetInt("MouseInvert", defaultInvertMouse);
            }
            mouseLookSpeedSlider.value = PlayerPrefs.GetFloat("MouseLookSpeed");
            mouseTurnSpeedSlider.value = PlayerPrefs.GetFloat("MouseTurnSpeed");
            keyboardTurnSpeedSlider.value = PlayerPrefs.GetFloat("KeyboardTurnSpeed");

            // check mouse inverse
            if (PlayerPrefs.GetInt("MouseInvert") == 0) {
                invertMouseButton.DeSelect();
            } else if (PlayerPrefs.GetInt("MouseInvert") == 1) {
                invertMouseButton.Select();
            }
        }

        public void ToggleinvertMouse() {
            if (PlayerPrefs.GetInt("MouseInvert") == 0) {
                PlayerPrefs.SetInt("MouseInvert", 1);
                MessageFeedManager.MyInstance.WriteMessage("Invert Mouse: on");
                invertMouseButton.Select();
            } else {
                PlayerPrefs.SetInt("MouseInvert", 0);
                MessageFeedManager.MyInstance.WriteMessage("Invert Mouse: off");
                invertMouseButton.DeSelect();
            }
        }

        public void ToggleUseQuestTracker() {
            if (PlayerPrefs.GetInt("UseQuestTracker") == 0) {
                PlayerPrefs.SetInt("UseQuestTracker", 1);
                MessageFeedManager.MyInstance.WriteMessage("Quest Tracker: on");
                useQuestTrackerButton.Select();
            } else {
                PlayerPrefs.SetInt("UseQuestTracker", 0);
                MessageFeedManager.MyInstance.WriteMessage("Quest Tracker: off");
                useQuestTrackerButton.DeSelect();
            }
            UIManager.MyInstance.CheckQuestTrackerSettings();
        }

        public void ToggleUseCombatLog() {
            if (PlayerPrefs.GetInt("UseCombatLog") == 0) {
                PlayerPrefs.SetInt("UseCombatLog", 1);
                MessageFeedManager.MyInstance.WriteMessage("Combat Log: on");
                useCombatLogButton.Select();
            } else {
                PlayerPrefs.SetInt("UseCombatLog", 0);
                MessageFeedManager.MyInstance.WriteMessage("Combat Log: off");
                useCombatLogButton.DeSelect();
            }
            UIManager.MyInstance.CheckCombatLogSettings();
        }

        public void ToggleUseActionBar2() {
            if (PlayerPrefs.GetInt("UseActionBar2") == 0) {
                PlayerPrefs.SetInt("UseActionBar2", 1);
                MessageFeedManager.MyInstance.WriteMessage("Action Bar 2: on");
                useActionBar2Button.Select();
            } else {
                PlayerPrefs.SetInt("UseActionBar2", 0);
                MessageFeedManager.MyInstance.WriteMessage("Action Bar 2: off");
                useActionBar2Button.DeSelect();
            }
            UIManager.MyInstance.UpdateActionBars();
        }

        public void ToggleUseActionBar3() {
            if (PlayerPrefs.GetInt("UseActionBar3") == 0) {
                PlayerPrefs.SetInt("UseActionBar3", 1);
                MessageFeedManager.MyInstance.WriteMessage("Action Bar 3: on");
                useActionBar3Button.Select();
            } else {
                PlayerPrefs.SetInt("UseActionBar3", 0);
                MessageFeedManager.MyInstance.WriteMessage("Action Bar 3: on");
                useActionBar3Button.DeSelect();
            }
            UIManager.MyInstance.UpdateActionBars();
        }

        public void ToggleUseActionBar4() {
            if (PlayerPrefs.GetInt("UseActionBar4") == 0) {
                PlayerPrefs.SetInt("UseActionBar4", 1);
                MessageFeedManager.MyInstance.WriteMessage("Action Bar 4: on");
                useActionBar4Button.Select();
            } else {
                PlayerPrefs.SetInt("UseActionBar4", 0);
                MessageFeedManager.MyInstance.WriteMessage("Action Bar 4: off");
                useActionBar4Button.DeSelect();
            }
            UIManager.MyInstance.UpdateActionBars();
        }

        public void ToggleUseActionBar5() {
            if (PlayerPrefs.GetInt("UseActionBar5") == 0) {
                PlayerPrefs.SetInt("UseActionBar5", 1);
                MessageFeedManager.MyInstance.WriteMessage("Action Bar 5: on");
                useActionBar5Button.Select();
            } else {
                PlayerPrefs.SetInt("UseActionBar5", 0);
                MessageFeedManager.MyInstance.WriteMessage("Action Bar 5: on");
                useActionBar5Button.DeSelect();
            }
            UIManager.MyInstance.UpdateActionBars();
        }

        public void ToggleUseActionBar6() {
            if (PlayerPrefs.GetInt("UseActionBar6") == 0) {
                PlayerPrefs.SetInt("UseActionBar6", 1);
                MessageFeedManager.MyInstance.WriteMessage("Action Bar 6: on");
                useActionBar6Button.Select();
            } else {
                PlayerPrefs.SetInt("UseActionBar6", 0);
                MessageFeedManager.MyInstance.WriteMessage("Action Bar 6: off");
                useActionBar6Button.DeSelect();
            }
            UIManager.MyInstance.UpdateActionBars();
        }

        public void ToggleUseActionBar7() {
            if (PlayerPrefs.GetInt("UseActionBar7") == 0) {
                PlayerPrefs.SetInt("UseActionBar7", 1);
                MessageFeedManager.MyInstance.WriteMessage("Action Bar 7: on");
                useActionBar7Button.Select();
            } else {
                PlayerPrefs.SetInt("UseActionBar7", 0);
                MessageFeedManager.MyInstance.WriteMessage("Action Bar 7: off");
                useActionBar7Button.DeSelect();
            }
            UIManager.MyInstance.UpdateActionBars();
        }

        public void ToggleUseStatusEffectBar() {
            if (PlayerPrefs.GetInt("UseStatusEffectBar") == 0) {
                PlayerPrefs.SetInt("UseStatusEffectBar", 1);
                MessageFeedManager.MyInstance.WriteMessage("Status Effect Bar: on");
                useStatusEffectBarButton.Select();
            } else {
                PlayerPrefs.SetInt("UseStatusEffectBar", 0);
                MessageFeedManager.MyInstance.WriteMessage("Status Effect Bar: off");
                useStatusEffectBarButton.DeSelect();
            }
            UIManager.MyInstance.UpdateStatusEffectBar();
        }

        public void ToggleUseFocusUnitFrameButton() {
            if (PlayerPrefs.GetInt("UseFocusUnitFrame") == 0) {
                PlayerPrefs.SetInt("UseFocusUnitFrame", 1);
                MessageFeedManager.MyInstance.WriteMessage("Focus Unit Frame: on");
                useFocusUnitFrameButton.Select();
            } else {
                PlayerPrefs.SetInt("UseFocusUnitFrame", 0);
                MessageFeedManager.MyInstance.WriteMessage("Focus Unit Frame: off");
                useFocusUnitFrameButton.DeSelect();
            }
            UIManager.MyInstance.UpdateFocusUnitFrame();
        }

        public void ToggleUsePlayerUnitFrameButton() {
            if (PlayerPrefs.GetInt("UsePlayerUnitFrame") == 0) {
                PlayerPrefs.SetInt("UsePlayerUnitFrame", 1);
                MessageFeedManager.MyInstance.WriteMessage("Player Unit Frame: on");
                usePlayerUnitFrameButton.Select();
            } else {
                PlayerPrefs.SetInt("UsePlayerUnitFrame", 0);
                MessageFeedManager.MyInstance.WriteMessage("Player Unit Frame: off");
                usePlayerUnitFrameButton.DeSelect();
            }
            UIManager.MyInstance.UpdatePlayerUnitFrame();
        }

        public void ToggleUseFloatingCastBarButton() {
            if (PlayerPrefs.GetInt("UseFloatingCastBar") == 0) {
                PlayerPrefs.SetInt("UseFloatingCastBar", 1);
                MessageFeedManager.MyInstance.WriteMessage("Floating Cast Bar: on");
                useFloatingCastBarButton.Select();
            } else {
                PlayerPrefs.SetInt("UseFloatingCastBar", 0);
                MessageFeedManager.MyInstance.WriteMessage("Floating Cast Bar: off");
                useFloatingCastBarButton.DeSelect();
            }
            UIManager.MyInstance.UpdateFloatingCastBar();
        }

        public void ToggleUseMiniMapButton() {
            if (PlayerPrefs.GetInt("UseMiniMap") == 0) {
                PlayerPrefs.SetInt("UseMiniMap", 1);
                useMiniMapButton.Select();
                MessageFeedManager.MyInstance.WriteMessage("Minimap: on");
            } else {
                PlayerPrefs.SetInt("UseMiniMap", 0);
                useMiniMapButton.DeSelect();
                MessageFeedManager.MyInstance.WriteMessage("Minimap: off");
            }
            UIManager.MyInstance.UpdateMiniMap();
        }

        public void ToggleUseExperienceBarButton() {
            if (PlayerPrefs.GetInt("UseExperienceBar") == 0) {
                PlayerPrefs.SetInt("UseExperienceBar", 1);
                MessageFeedManager.MyInstance.WriteMessage("Experience Bar: on");
                useExperienceBarButton.Select();
            } else {
                PlayerPrefs.SetInt("UseExperienceBar", 0);
                MessageFeedManager.MyInstance.WriteMessage("Experience Bar: off");
                useExperienceBarButton.DeSelect();
            }
            UIManager.MyInstance.UpdateExperienceBar();
        }

        public void ToggleUseFloatingCombatTextButton() {
            if (PlayerPrefs.GetInt("UseFloatingCombatText") == 0) {
                PlayerPrefs.SetInt("UseFloatingCombatText", 1);
                MessageFeedManager.MyInstance.WriteMessage("Floating Combat Text: on");
                useFloatingCombatTextButton.Select();
            } else {
                PlayerPrefs.SetInt("UseFloatingCombatText", 0);
                MessageFeedManager.MyInstance.WriteMessage("Floating Combat Text: off");
                useFloatingCombatTextButton.DeSelect();
            }
            UIManager.MyInstance.UpdateFloatingCombatText();
        }

        public void ToggleUseMessageFeedButton() {
            if (PlayerPrefs.GetInt("UseMessageFeed") == 0) {
                PlayerPrefs.SetInt("UseMessageFeed", 1);
                MessageFeedManager.MyInstance.WriteMessage("Use Message Feed: on");
                useMessageFeedButton.Select();
            } else {
                MessageFeedManager.MyInstance.WriteMessage("Use Message Feed: off");
                PlayerPrefs.SetInt("UseMessageFeed", 0);
                useMessageFeedButton.DeSelect();
            }
            UIManager.MyInstance.UpdateMessageFeed();
        }

        public void ToggleShowPlayerNameButton() {
            Debug.Log("MainSettingsMenuController.ToggleShowPlayerNameButton()");
            if (PlayerPrefs.GetInt("ShowPlayerName") == 0) {
                PlayerPrefs.SetInt("ShowPlayerName", 1);
                //Debug.Log("MainSettingsMenuController.ToggleShowPlayerNameButton(): showplayername now set to 1");
                MessageFeedManager.MyInstance.WriteMessage("Show Player Name: on");
                showPlayerNameButton.Select();
            } else {
                MessageFeedManager.MyInstance.WriteMessage("Show Player Name: off");
                PlayerPrefs.SetInt("ShowPlayerName", 0);
                //Debug.Log("MainSettingsMenuController.ToggleShowPlayerNameButton(): showplayername now set to 0");
                showPlayerNameButton.DeSelect();
            }
            // not really reputation, but it will trigger the right check
            SystemEventManager.TriggerEvent("OnReputationChange", new EventParamProperties());
        }

        public void ToggleShowPlayerFactionButton() {
            //Debug.Log("MainSettingsMenuController.ToggleShowPlayerFactionButton()");
            if (PlayerPrefs.GetInt("ShowPlayerFaction") == 0) {
                PlayerPrefs.SetInt("ShowPlayerFaction", 1);
                //Debug.Log("MainSettingsMenuController.ToggleShowPlayerNameButton(): showplayername now set to 1");
                MessageFeedManager.MyInstance.WriteMessage("Show Player Faction: on");
                showPlayerFactionButton.Select();
            } else {
                MessageFeedManager.MyInstance.WriteMessage("Show Player Faction: off");
                PlayerPrefs.SetInt("ShowPlayerFaction", 0);
                //Debug.Log("MainSettingsMenuController.ToggleShowPlayerNameButton(): showplayername now set to 0");
                showPlayerFactionButton.DeSelect();
            }
            // not really reputation, but it will trigger the right check
            SystemEventManager.TriggerEvent("OnReputationChange", new EventParamProperties());
        }

        public void ToggleHideFullHealthBarButton() {
            //Debug.Log("MainSettingsMenuController.ToggleHideFullHealthBarButton()");
            if (PlayerPrefs.GetInt("HideFullHealthBar") == 0) {
                PlayerPrefs.SetInt("HideFullHealthBar", 1);
                MessageFeedManager.MyInstance.WriteMessage("Hide Full Healthbar: on");
                hideFullHealthBarButton.Select();
            } else {
                MessageFeedManager.MyInstance.WriteMessage("Hide full healthbar: off");
                PlayerPrefs.SetInt("HideFullHealthBar", 0);
                hideFullHealthBarButton.DeSelect();
            }
            // not really reputation, but it will trigger the right check
            SystemEventManager.TriggerEvent("OnReputationChange", new EventParamProperties());
        }

        public void ToggleLockUIButton() {
            if (PlayerPrefs.GetInt("LockUI") == 0) {
                PlayerPrefs.SetInt("LockUI", 1);
                MessageFeedManager.MyInstance.WriteMessage("Lock UI: on");
                lockUIButton.Select();
            } else {
                MessageFeedManager.MyInstance.WriteMessage("Lock UI: off");
                PlayerPrefs.SetInt("LockUI", 0);
                lockUIButton.DeSelect();
            }
            UIManager.MyInstance.UpdateLockUI();
        }

        public void InventoryOpacitySliderUpdate() {
            PlayerPrefs.SetFloat("InventoryOpacity", inventoryOpacitySlider.value);
            UIManager.MyInstance.UpdateInventoryOpacity();
            SystemEventManager.MyInstance.NotifyOnInventoryTransparencyUpdate();
        }

        public void InventorySlotOpacitySliderUpdate() {
            PlayerPrefs.SetFloat("InventorySlotOpacity", inventorySlotOpacitySlider.value);
            UIManager.MyInstance.UpdateInventoryOpacity();
            SystemEventManager.MyInstance.NotifyOnInventoryTransparencyUpdate();
        }

        public void ActionBarOpacitySliderUpdate() {
            PlayerPrefs.SetFloat("ActionBarOpacity", actionBarOpacitySlider.value);
            UIManager.MyInstance.UpdateActionBarOpacity();
        }

        public void QuestTrackerOpacitySliderUpdate() {
            PlayerPrefs.SetFloat("QuestTrackerOpacity", questTrackerOpacitySlider.value);
            UIManager.MyInstance.UpdateQuestTrackerOpacity();
        }

        public void CombatLogOpacitySliderUpdate() {
            PlayerPrefs.SetFloat("CombatLogOpacity", combatLogOpacitySlider.value);
            UIManager.MyInstance.UpdateCombatLogOpacity();
        }

        public void PopupWindowOpacitySliderUpdate() {
            PlayerPrefs.SetFloat("PopupWindowOpacity", popupWindowOpacitySlider.value);
            UIManager.MyInstance.UpdatePopupWindowOpacity();
        }

        public void PagedButtonsOpacitySliderUpdate() {
            //Debug.Log("MainSettingsMenuController.PagedButtonsOpacitySliderUpdate()");
            PlayerPrefs.SetFloat("PagedButtonsOpacity", pagedButtonsOpacitySlider.value);
            SystemEventManager.MyInstance.NotifyOnPagedButtonsTransparencyUpdate();
        }

        public void SystemMenuOpacitySliderUpdate() {
            PlayerPrefs.SetFloat("SystemMenuOpacity", systemMenuOpacitySlider.value);
            UIManager.MyInstance.UpdateSystemMenuOpacity();
        }

        private void LoadVolumeSliderValues() {
            masterSlider.value = PlayerPrefs.GetFloat(AudioManager.MyInstance.MasterVolume);
            musicSlider.value = PlayerPrefs.GetFloat(AudioManager.MyInstance.MusicVolume);
            ambientSlider.value = PlayerPrefs.GetFloat(AudioManager.MyInstance.AmbientVolume);
            effectsSlider.value = PlayerPrefs.GetFloat(AudioManager.MyInstance.EffectsVolume);
            uiSlider.value = PlayerPrefs.GetFloat(AudioManager.MyInstance.UiVolume);
            voiceSlider.value = PlayerPrefs.GetFloat(AudioManager.MyInstance.VoiceVolume);
        }

        private void LoadDifficultySettings() {
            if (PlayerPrefs.GetInt("NormalDifficulty") == 1) {
                difficultynormaltextLINE.gameObject.SetActive(true);
                difficultyhardcoretextLINE.gameObject.SetActive(false);
            } else {
                difficultyhardcoretextLINE.gameObject.SetActive(true);
                difficultynormaltextLINE.gameObject.SetActive(false);
            }
        }

        public void MasterSlider() {
            AudioManager.MyInstance.SetMasterVolume(masterSlider.value);
        }

        public void MusicSlider() {
            AudioManager.MyInstance.SetMusicVolume(musicSlider.value);
        }

        public void AmbientSlider() {
            AudioManager.MyInstance.SetAmbientVolume(ambientSlider.value);
        }

        public void EffectsSlider() {
            AudioManager.MyInstance.SetEffectsVolume(effectsSlider.value);
        }

        public void UISlider() {
            AudioManager.MyInstance.SetUIVolume(uiSlider.value);
        }

        public void VoiceSlider() {
            AudioManager.MyInstance.SetVoiceVolume(voiceSlider.value);
        }

        public void MouseLookSpeedSlider() {
            PlayerPrefs.SetFloat("MouseLookSpeed", mouseLookSpeedSlider.value);
        }

        public void MouseTurnSpeedSlider() {
            PlayerPrefs.SetFloat("MouseTurnSpeed", mouseTurnSpeedSlider.value);
        }

        public void KeyboardTurnSpeedSlider() {
            PlayerPrefs.SetFloat("KeyboardTurnSpeed", keyboardTurnSpeedSlider.value);
        }

        public void ResetNavigationHighlights() {
            // turn off all navigation button highlights
            soundButton.DeSelect();
            controlsButton.DeSelect();
            videoButton.DeSelect();
            keyBindingsButton.DeSelect();
            userInterfaceButton.DeSelect();
        }

        public void ResetSettingsPanels() {
            // disable all settings panels
            PanelSound.gameObject.SetActive(false);
            PanelControls.gameObject.SetActive(false);
            PanelVideo.gameObject.SetActive(false);
            PanelKeyBindings.gameObject.SetActive(false);
            PanelUserInterface.gameObject.SetActive(false);
        }

        public void SoundPanel() {
            ResetSettingsPanels();
            PanelSound.gameObject.SetActive(true);

            ResetNavigationHighlights();
            soundButton.Select();
        }

        public void VideoPanel() {
            ResetSettingsPanels();
            PanelVideo.gameObject.SetActive(true);

            ResetNavigationHighlights();
            videoButton.Select();
        }

        public void ControlsPanel() {
            ResetSettingsPanels();
            PanelControls.gameObject.SetActive(true);

            ResetNavigationHighlights();
            controlsButton.Select();
        }

        public void KeyBindingsPanel() {
            ResetSettingsPanels();
            PanelKeyBindings.gameObject.SetActive(true);

            ResetNavigationHighlights();
            keyBindingsButton.Select();
        }
        public void UserInterfacePanel() {
            ResetSettingsPanels();
            PanelUserInterface.gameObject.SetActive(true);

            ResetNavigationHighlights();
            userInterfaceButton.Select();
        }

        public void CloseMenu() {
            //SystemWindowManager.MyInstance.mainMenuWindow.OpenWindow();
            SystemWindowManager.MyInstance.settingsMenuWindow.CloseWindow();
        }

        public override void ReceiveOpenWindowNotification() {
            base.ReceiveOpenWindowNotification();
            UserInterfacePanel();
        }
    }
}