using AnyRPG;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace AnyRPG {
    public class SystemControlsPanelController : WindowContentController {

        [Header("CONTROLS SETTINGS")]

        public Slider mouseLookSpeedSlider;
        public Slider mouseTurnSpeedSlider;
        public Slider keyboardTurnSpeedSlider;
        public OnOffTextButton invertMouseButton;

        private float defaultMouseLookSpeed = 0.5f;
        private float defaultMouseTurnSpeed = 0.5f;
        private float defaultKeyboardTurnSpeed = 0.5f;
        private int defaultInvertMouse = 0;

        // game manager references
        private UIManager uIManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            SetPlayerPrefsDefaults();

            LoadControlsSettings();
            // ui buttons
            invertMouseButton.Configure(systemGameManager);
        }

        /*
        public override void Init() {
            base.Init();
            LoadControlsSettings();
        }
        */

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            uIManager = systemGameManager.UIManager;
        }

        /*
        public void Start() {

            LoadControlsSettings();
        }
        */
       

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


    }
}