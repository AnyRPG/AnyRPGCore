using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace AnyRPG {
    public class WrongClientVersionPanel : WindowContentController {

        [Header("Wrong Client Version")]

        /*
        [SerializeField]
        private HighlightButton okButton = null;
        */

        [SerializeField]
        private TMP_Text versionMessage = null;

        // game manager references
        private UIManager uIManager = null;
        private NetworkManagerClient networkManagerClient = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            //noButton.Configure(systemGameManager);
            //yesButton.Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            uIManager = systemGameManager.UIManager;
            networkManagerClient = systemGameManager.NetworkManagerClient;
        }

        public override void ProcessOpenWindowNotification() {
            base.ProcessOpenWindowNotification();
            networkManagerClient.OnClientVersionFailure += HandleClientVersionFailure;
        }

        public override void ReceiveClosedWindowNotification() {
            base.ReceiveClosedWindowNotification();
            networkManagerClient.OnClientVersionFailure -= HandleClientVersionFailure;
        }

        public void HandleClientVersionFailure(string requiredClientVersion) {
            versionMessage.text = $"Wrong client version.\nDownload version {requiredClientVersion} from {systemConfigurationManager.ClientDownloadUrl}";
        }

        public void ConfirmAction() {
            //Debug.Log("DisconnectedPanelController.ConfirmAction()");
            uIManager.wrongClientVersionWindow.CloseWindow();
        }

    }

}