using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class DisconnectedPanelController : WindowContentController {

        /*
        [SerializeField]
        private HighlightButton okButton = null;
        */

        // game manager references
        private UIManager uIManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            //noButton.Configure(systemGameManager);
            //yesButton.Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            uIManager = systemGameManager.UIManager;
        }

        public void ConfirmAction() {
            //Debug.Log("DisconnectedPanelController.ConfirmAction()");
            uIManager.disconnectedWindow.CloseWindow();
        }

    }

}