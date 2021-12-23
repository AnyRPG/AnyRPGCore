using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace AnyRPG {
    public class OnScreenKeyboardController : WindowContentController {


        [SerializeField]
        private TMP_InputField inputField = null;

        [SerializeField]
        private UINavigationController capitalKeys = null;

        [SerializeField]
        private UINavigationController lowercaseKeys = null;

        // the input field to fill with the value of the local input field when accept is pressed
        private TMP_InputField sourceInputField = null;

        // game manager references
        protected UIManager uIManager = null;
        protected SaveManager saveManager = null;
        protected NewGameManager newGameManager = null;
        protected OnScreenKeyboardManager onScreenKeyboardManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);


        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            uIManager = systemGameManager.UIManager;
            saveManager = systemGameManager.SaveManager;
            newGameManager = systemGameManager.NewGameManager;
            onScreenKeyboardManager = uIManager.OnScreenKeyboardManager;
        }

        public void InputCharacter(string newCharacter) {
            inputField.text = inputField.text + newCharacter;
        }

        public override void JoystickButton2() {
            base.JoystickButton2();
            sourceInputField.text = inputField.text;
            Close();
        }

        public override void JoystickButton3() {
            base.JoystickButton3();
            ToggleCaps();
        }

        public override void JoystickButton4() {
            base.JoystickButton4();
            if (inputField.text.Length > 0) {
                inputField.text = inputField.text.Substring(0, inputField.text.Length - 1);
            }
        }


        public void ToggleCaps() {
            //Debug.Log("OnScreenKeyboardController.ToggleCaps()");
            if (capitalKeys.gameObject.activeSelf == true) {
                capitalKeys.gameObject.SetActive(false);
                lowercaseKeys.gameObject.SetActive(true);
                if (currentNavigationController == capitalKeys) {
                    lowercaseKeys.SetCurrentIndex(capitalKeys.CurrentIndex);
                    lowercaseKeys.FocusCurrentButton();
                    SetNavigationController(lowercaseKeys);
                }
            } else {
                lowercaseKeys.gameObject.SetActive(false);
                capitalKeys.gameObject.SetActive(true);
                if (currentNavigationController == lowercaseKeys) {
                    capitalKeys.SetCurrentIndex(lowercaseKeys.CurrentIndex);
                    capitalKeys.FocusCurrentButton();
                    SetNavigationController(capitalKeys);
                }
            }
        }

        public void HandleActivateKeyboard(TMP_InputField sourceInputField) {
            //Debug.Log("OnScreenKeyboardController.HandleActivateKeyboard()");
            this.sourceInputField = sourceInputField;
            inputField.text = sourceInputField.text;
        }

        protected override void ProcessCreateEventSubscriptions() {
            base.ProcessCreateEventSubscriptions();

            onScreenKeyboardManager.OnActivateKeyboard += HandleActivateKeyboard;
        }

        protected override void ProcessCleanupEventSubscriptions() {
            base.ProcessCleanupEventSubscriptions();

            onScreenKeyboardManager.OnActivateKeyboard -= HandleActivateKeyboard;

        }

    }

}