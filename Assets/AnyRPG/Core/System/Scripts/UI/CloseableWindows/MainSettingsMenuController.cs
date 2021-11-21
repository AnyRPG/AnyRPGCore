using AnyRPG;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

namespace AnyRPG {
    public class MainSettingsMenuController : WindowContentController {

        [Header("MAIN BUTTONS")]
        public HighlightButton soundButton;
        public HighlightButton controlsButton;
        public HighlightButton videoButton;
        public HighlightButton keyBindingsButton;
        public HighlightButton userInterfaceButton;
        public HighlightButton returnButton;

        [Header("CONTROLS SETTINGS")]

        public Slider mouseLookSpeedSlider;
        public Slider mouseTurnSpeedSlider;
        public Slider keyboardTurnSpeedSlider;
        public OnOffTextButton invertMouseButton;

        private float defaultMouseLookSpeed = 0.5f;
        private float defaultMouseTurnSpeed = 0.5f;
        private float defaultKeyboardTurnSpeed = 0.5f;
        private int defaultInvertMouse = 0;

        [Header("Panels")]
        [Tooltip("The UI Panel that holds the GAME window tab")]
        public SystemSoundPanelController PanelSound;
        [Tooltip("The UI Panel that holds the CONTROLS window tab")]
        public GameObject PanelControls;
        [Tooltip("The UI Panel that holds the VIDEO window tab")]
        public SystemVideoPanelController PanelVideo;
        [Tooltip("The UI Panel that holds the KEY BINDINGS window tab")]
        public SystemKeyBindPanelController PanelKeyBindings;
        [Tooltip("The UI Panel that holds the USER INTERFACE window tab")]
        public SystemUIPanelController PanelUserInterface;

        // game manager references
        private UIManager uIManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            // set defaults first so that configuration that runs next will use them
            SetPlayerPrefsDefaults();

            // main buttons
            /*
            soundButton.Configure(systemGameManager);
            controlsButton.Configure(systemGameManager);
            videoButton.Configure(systemGameManager);
            keyBindingsButton.Configure(systemGameManager);
            userInterfaceButton.Configure(systemGameManager);
            returnButton.Configure(systemGameManager);
            */

            // ui buttons
            invertMouseButton.Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            uIManager = systemGameManager.UIManager;
        }

        public void Start() {

            LoadControlsSettings();

        }
       

        private void SetPlayerPrefsDefaults() {
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

            if (!PlayerPrefs.HasKey("GraphicsQualityIndex")) {
                //Debug.Log("MainSettingsMenuController.SetPlayerPrefsDefaults() graphicsQuality is: " + QualitySettings.GetQualityLevel());
                PlayerPrefs.SetInt("GraphicsQualityIndex", QualitySettings.GetQualityLevel());
            }

            if (!PlayerPrefs.HasKey("FullScreen")) {
                PlayerPrefs.SetInt("FullScreen", (Screen.fullScreen == true ? 1 : 0));
            }

            if (!PlayerPrefs.HasKey("VSyncValue")) {
                PlayerPrefs.SetInt("VSyncValue", QualitySettings.vSyncCount);
            }

            if (!PlayerPrefs.HasKey("Textures")) {
                PlayerPrefs.SetInt("Textures", 2);
            }

            if (!PlayerPrefs.HasKey("Shadows")) {
                PlayerPrefs.SetInt("Shadows", 2);
            }

        }

        private void LoadControlsSettings() {
           
            mouseLookSpeedSlider.value = PlayerPrefs.GetFloat("MouseLookSpeed");
            mouseTurnSpeedSlider.value = PlayerPrefs.GetFloat("MouseTurnSpeed");
            keyboardTurnSpeedSlider.value = PlayerPrefs.GetFloat("KeyboardTurnSpeed");

            // check mouse inverse
            if (PlayerPrefs.GetInt("MouseInvert") == 0) {
                invertMouseButton.SetOff();
            } else if (PlayerPrefs.GetInt("MouseInvert") == 1) {
                invertMouseButton.SetOn();
            }
        }

        public void ToggleinvertMouse() {
            if (PlayerPrefs.GetInt("MouseInvert") == 0) {
                PlayerPrefs.SetInt("MouseInvert", 1);
                uIManager.MessageFeedManager.WriteMessage("Invert Mouse: on");
                invertMouseButton.SetOn();
            } else {
                PlayerPrefs.SetInt("MouseInvert", 0);
                uIManager.MessageFeedManager.WriteMessage("Invert Mouse: off");
                invertMouseButton.SetOff();
            }
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

        /*
        public void ResetNavigationHighlights() {
            // turn off all navigation button highlights
            soundButton.DeSelect();
            controlsButton.DeSelect();
            videoButton.DeSelect();
            keyBindingsButton.DeSelect();
            userInterfaceButton.DeSelect();
        }
        */

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

            uINavigationControllers[0].UnHightlightButtons(soundButton);
            SetOpenSubPanel(PanelSound);
            soundButton.Select();
        }

        public void VideoPanel() {
            ResetSettingsPanels();
            PanelVideo.gameObject.SetActive(true);

            uINavigationControllers[0].UnHightlightButtons(videoButton);
            SetOpenSubPanel(PanelVideo);

            videoButton.Select();
        }

        public void ControlsPanel() {
            ResetSettingsPanels();
            PanelControls.gameObject.SetActive(true);

            uINavigationControllers[0].UnHightlightButtons(controlsButton);
            controlsButton.Select();
        }

        public void KeyBindingsPanel() {
            ResetSettingsPanels();
            PanelKeyBindings.gameObject.SetActive(true);

            uINavigationControllers[0].UnHightlightButtons(keyBindingsButton);
            SetOpenSubPanel(PanelKeyBindings);

            keyBindingsButton.Select();
        }


        public void UserInterfacePanel() {
            ResetSettingsPanels();
            PanelUserInterface.gameObject.SetActive(true);

            uINavigationControllers[0].UnHightlightButtons(userInterfaceButton);
            SetOpenSubPanel(PanelUserInterface);
            userInterfaceButton.Select();
        }

        public void CloseMenu() {
            //uIManager.SystemWindowManager.mainMenuWindow.OpenWindow();
            uIManager.settingsMenuWindow.CloseWindow();
        }

        public override void ReceiveOpenWindowNotification() {
            base.ReceiveOpenWindowNotification();
            uINavigationControllers[0].SetCurrentButton(userInterfaceButton);
            userInterfaceButton.HighlightBackground();
            UserInterfacePanel();
        }
    }
}