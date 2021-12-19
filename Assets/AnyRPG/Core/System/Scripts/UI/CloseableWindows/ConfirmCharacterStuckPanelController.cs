using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class ConfirmCharacterStuckPanelController : WindowContentController {

        // game manager references
        private UIManager uIManager = null;
        private PlayerManager playerManager = null;

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            uIManager = systemGameManager.UIManager;
            playerManager = systemGameManager.PlayerManager;
        }

        public void CancelAction() {
            //Debug.Log("NewGameMenuController.CancelAction()");
            uIManager.confirmCharacterStuckWindow.CloseWindow();
        }

        public void ConfirmAction() {
            //Debug.Log("NewGameMenuController.ConfirmAction()");
            uIManager.confirmCharacterStuckWindow.CloseWindow();
            uIManager.helpMenuWindow.CloseWindow();
            playerManager.RespawnPlayer();
        }

    }

}