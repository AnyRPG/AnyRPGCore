using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
    public class NetworkLoginPanel : WindowContentController {

        [SerializeField]
        private TMP_InputField userNameInput = null;

        [SerializeField]
        private TMP_InputField passwordInput = null;

        [SerializeField]
        private TMP_InputField serverInput = null;

        // game manager references
        private UIManager uIManager = null;
        private PlayerManager playerManager = null;
        private NetworkManagerClient networkManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            serverInput.text = systemConfigurationManager.GameServerAddress;
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
        }

        public override void ReceiveClosedWindowNotification() {
            //Debug.Log("NetworkLoginPanelController.ReceiveClosedWindowNotification()");
            base.ReceiveClosedWindowNotification();
            //nameChangeManager.EndInteraction();
        }

        public void HandlePointerClick() {
            EventParamProperties eventParam = new EventParamProperties();
            SystemEventManager.TriggerEvent("OnDisableMovement", eventParam);
            controlsManager.ActivateTextInput();
        }

        public void HandleEndEdit() {
            EventParamProperties eventParam = new EventParamProperties();
            SystemEventManager.TriggerEvent("OnEnableMovement", eventParam);
            controlsManager.DeactivateTextInput();
        }

    }

}