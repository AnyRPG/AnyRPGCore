using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class LoginInProgressPanel : WindowContentController {

        /*
        [SerializeField]
        private HighlightButton okButton = null;
        */

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

        public void CancelAction() {
            //Debug.Log("DisconnectedPanelController.ConfirmAction()");
            networkManagerClient.Logout();
            uIManager.loginInProgressWindow.CloseWindow();
        }

    }

}