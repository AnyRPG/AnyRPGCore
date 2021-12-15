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
        public Slider joystickLookSpeedSlider;
        public Slider joystickTurnSpeedSlider;
        public OnOffTextButton invertJoystickButton;
        public OnOffTextButton gamepadModeButton;


        private float defaultMouseLookSpeed = 0.5f;
        private float defaultMouseTurnSpeed = 0.5f;
        private float defaultKeyboardTurnSpeed = 0.5f;
        private int defaultInvertMouse = 0;
        private float defaultJoystickLookSpeed = 0.5f;
        private float defaultJoystickTurnSpeed = 0.5f;
        private int defaultInvertJoystick = 0;

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
            if (!PlayerPrefs.HasKey("JoystickLookSpeed")) {
                PlayerPrefs.SetFloat("JoystickLookSpeed", defaultJoystickLookSpeed);
            }
            if (!PlayerPrefs.HasKey("JoystickTurnSpeed")) {
                PlayerPrefs.SetFloat("JoystickTurnSpeed", defaultJoystickTurnSpeed);
            }
            if (!PlayerPrefs.HasKey("JoystickInvert")) {
                PlayerPrefs.SetInt("JoystickInvert", defaultInvertJoystick);
            }
            if (!PlayerPrefs.HasKey("GamepadMode")) {
                PlayerPrefs.SetInt("GamepadMode", systemConfigurationManager.DefaultControllerConfiguration == DefaultControllerConfiguration.GamePad ? 1 : 0);
            }
        }

        public void ResetToDefaults() {
            PlayerPrefs.SetFloat("MouseLookSpeed", defaultMouseLookSpeed);
            PlayerPrefs.SetFloat("MouseTurnSpeed", defaultMouseTurnSpeed);
            PlayerPrefs.SetFloat("KeyboardTurnSpeed", defaultKeyboardTurnSpeed);
            PlayerPrefs.SetInt("MouseInvert", defaultInvertMouse);
            PlayerPrefs.SetFloat("JoystickLookSpeed", defaultJoystickLookSpeed);
            PlayerPrefs.SetFloat("JoystickTurnSpeed", defaultJoystickTurnSpeed);
            PlayerPrefs.SetInt("JoystickInvert", defaultInvertJoystick);
            PlayerPrefs.SetInt("GamepadMode", systemConfigurationManager.DefaultControllerConfiguration == DefaultControllerConfiguration.GamePad ? 1 : 0);
            LoadControlsSettings();
        }

        private void LoadControlsSettings() {
           
            mouseLookSpeedSlider.value = PlayerPrefs.GetFloat("MouseLookSpeed");
            mouseTurnSpeedSlider.value = PlayerPrefs.GetFloat("MouseTurnSpeed");
            keyboardTurnSpeedSlider.value = PlayerPrefs.GetFloat("KeyboardTurnSpeed");

            joystickLookSpeedSlider.value = PlayerPrefs.GetFloat("JoystickLookSpeed");
            joystickTurnSpeedSlider.value = PlayerPrefs.GetFloat("JoystickTurnSpeed");

            // check mouse inverse
            if (PlayerPrefs.GetInt("MouseInvert") == 0) {
                invertMouseButton.SetOff();
            } else if (PlayerPrefs.GetInt("MouseInvert") == 1) {
                invertMouseButton.SetOn();
            }

            // check joystick inverse
            if (PlayerPrefs.GetInt("JoystickInvert") == 0) {
                invertJoystickButton.SetOff();
            } else if (PlayerPrefs.GetInt("JoystickInvert") == 1) {
                invertJoystickButton.SetOn();
            }

            // check joystick inverse
            if (PlayerPrefs.GetInt("GamepadMode") == 0) {
                gamepadModeButton.SetOff();
                controlsManager.DeActivateGamepadMode(false);
            } else if (PlayerPrefs.GetInt("GamepadMode") == 1) {
                gamepadModeButton.SetOn();
                controlsManager.ActivateGamepadMode(false);
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

        public void ToggleInvertJoystick() {
            if (PlayerPrefs.GetInt("JoystickInvert") == 0) {
                PlayerPrefs.SetInt("JoystickInvert", 1);
                uIManager.MessageFeedManager.WriteMessage("Invert Joystick: on");
                invertJoystickButton.SetOn();
            } else {
                PlayerPrefs.SetInt("JoystickInvert", 0);
                uIManager.MessageFeedManager.WriteMessage("Invert Joystick: off");
                invertJoystickButton.SetOff();
            }
        }

        public void ToggleGamepadMode() {
            if (PlayerPrefs.GetInt("GamepadMode") == 0) {
                PlayerPrefs.SetInt("GamepadMode", 1);
                uIManager.MessageFeedManager.WriteMessage("Gamepad Mode: on");
                gamepadModeButton.SetOn();
                controlsManager.ActivateGamepadMode(true);
            } else {
                PlayerPrefs.SetInt("GamepadMode", 0);
                uIManager.MessageFeedManager.WriteMessage("Gamepad Mode: off");
                gamepadModeButton.SetOff();
                controlsManager.DeActivateGamepadMode(true);
            }
        }


        public void MouseLookSpeedSlider() {
            if (configureCount == 0) {
                return;
            }
            PlayerPrefs.SetFloat("MouseLookSpeed", mouseLookSpeedSlider.value);
        }

        public void MouseTurnSpeedSlider() {
            if (configureCount == 0) {
                return;
            }
            PlayerPrefs.SetFloat("MouseTurnSpeed", mouseTurnSpeedSlider.value);
        }

        public void KeyboardTurnSpeedSlider() {
            if (configureCount == 0) {
                return;
            }
            PlayerPrefs.SetFloat("KeyboardTurnSpeed", keyboardTurnSpeedSlider.value);
        }

        public void JoystickLookSpeedSlider() {
            if (configureCount == 0) {
                return;
            }
            PlayerPrefs.SetFloat("JoystickLookSpeed", joystickLookSpeedSlider.value);
        }

        public void JoystickTurnSpeedSlider() {
            if (configureCount == 0) {
                return;
            }
            PlayerPrefs.SetFloat("JoystickTurnSpeed", joystickTurnSpeedSlider.value);
        }


    }
}