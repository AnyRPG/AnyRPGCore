using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
    public class NetworkLoginPanel : WindowPanel {

        [SerializeField]
        private TMP_InputField userNameInput = null;

        [SerializeField]
        private TMP_InputField passwordInput = null;

        [SerializeField]
        private TMP_InputField serverInput = null;

        [SerializeField]
        private Toggle toggle = null;

        // game manager references
        private UIManager uIManager = null;
        private PlayerManager playerManager = null;
        private NetworkManagerClient networkManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            serverInput.text = systemConfigurationManager.GameServerAddress;
            passwordInput.inputType = TMP_InputField.InputType.Password;
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            uIManager = systemGameManager.UIManager;
            playerManager = systemGameManager.PlayerManager;
            networkManager = systemGameManager.NetworkManagerClient;
        }

        /// <summary>
        /// disable hotkeys and movement while text input is active
        /// </summary>
        public void ActivateTextInput() {
            controlsManager.ActivateTextInput();
        }

        public void DeativateTextInput() {
            controlsManager.DeactivateTextInput();
        }


        public void CancelAction() {
            //Debug.Log("NetworkLoginPanelController.CancelAction()");
            uIManager.networkLoginWindow.CloseWindow();
        }

        public void ConfirmAction() {
            //Debug.Log("NetworkLoginPanelController.ConfirmAction()");
            string username = userNameInput.text;
            string password = passwordInput.text;
            string server = serverInput.text;
            bool rememberMe = toggle.isOn;
            if (rememberMe) {
                PlayerPrefs.SetString("NetworkLoginPanel.username", username);
                PlayerPrefs.SetInt("NetworkLoginPanel.rememberMe", 1);
            } else {
                PlayerPrefs.SetInt("NetworkLoginPanel.rememberMe", 0);
            }
            /*
            if (textInput.text != null && textInput.text != string.Empty) {
                nameChangeManager.ChangePlayerName(textInput.text);
                uIManager.nameChangeWindow.CloseWindow();
            }
            */
            uIManager.loginInProgressWindow.OpenWindow();
            networkManager.Login(username, password, server);

        }

        public override void ProcessOpenWindowNotification() {
            //Debug.Log("NetworkLoginPanelController.ProcessOpenWindowNotification()");
            base.ProcessOpenWindowNotification();
            if (PlayerPrefs.HasKey("NetworkLoginPanel.rememberMe")) {
                toggle.isOn = PlayerPrefs.GetInt("NetworkLoginPanel.rememberMe") == 1;
                if (toggle.isOn && PlayerPrefs.HasKey("NetworkLoginPanel.username")) {
                    userNameInput.text = PlayerPrefs.GetString("NetworkLoginPanel.username");
                }
            }
        }

        public override void ReceiveClosedWindowNotification() {
            //Debug.Log("NetworkLoginPanelController.ReceiveClosedWindowNotification()");
            base.ReceiveClosedWindowNotification();
            //nameChangeManager.EndInteraction();
        }

    }

}