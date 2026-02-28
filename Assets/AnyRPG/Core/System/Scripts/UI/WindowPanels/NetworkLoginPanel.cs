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
        private Toggle rememberUsernameToggle = null;

        [SerializeField]
        private Toggle rememberServerToggle = null;

        // game manager references
        private UIManager uIManager = null;
        private PlayerManagerClient playerManagerClient = null;
        private NetworkManagerClient networkManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            serverInput.text = systemConfigurationManager.GameServerAddress;
            passwordInput.inputType = TMP_InputField.InputType.Password;
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            uIManager = systemGameManager.UIManager;
            playerManagerClient = systemGameManager.PlayerManagerClient;
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
            bool rememberUsername = rememberUsernameToggle.isOn;
            if (rememberUsername) {
                PlayerPrefs.SetString("NetworkLoginPanel.username", username);
                PlayerPrefs.SetInt("NetworkLoginPanel.rememberUsername", 1);
            } else {
                PlayerPrefs.SetInt("NetworkLoginPanel.rememberUsername", 0);
            }
            bool rememberServer = rememberServerToggle.isOn;
            if (rememberServer) {
                PlayerPrefs.SetString("NetworkLoginPanel.server", server);
                PlayerPrefs.SetInt("NetworkLoginPanel.rememberServer", 1);
            } else {
                PlayerPrefs.SetInt("NetworkLoginPanel.rememberServer", 0);
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
            if (PlayerPrefs.HasKey("NetworkLoginPanel.rememberUsername")) {
                rememberUsernameToggle.isOn = PlayerPrefs.GetInt("NetworkLoginPanel.rememberUsername") == 1;
                if (rememberUsernameToggle.isOn && PlayerPrefs.HasKey("NetworkLoginPanel.username")) {
                    userNameInput.text = PlayerPrefs.GetString("NetworkLoginPanel.username");
                }
            }
            if (PlayerPrefs.HasKey("NetworkLoginPanel.rememberServer")) {
                rememberServerToggle.isOn = PlayerPrefs.GetInt("NetworkLoginPanel.rememberServer") == 1;
                if (rememberServerToggle.isOn && PlayerPrefs.HasKey("NetworkLoginPanel.server")) {
                    serverInput.text = PlayerPrefs.GetString("NetworkLoginPanel.server");
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