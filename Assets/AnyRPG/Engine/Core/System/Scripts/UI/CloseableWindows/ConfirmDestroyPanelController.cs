using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class ConfirmDestroyPanelController : WindowContentController {

        // game manager references
        private UIManager uIManager = null;
        private HandScript handScript = null;

        public override void Init(SystemGameManager systemGameManager) {
            base.Init(systemGameManager);

            uIManager = systemGameManager.UIManager;
            handScript = uIManager.HandScript;
        }

        public void CancelAction() {
            //Debug.Log("NewGameMenuController.CancelAction()");
            uIManager.confirmDestroyMenuWindow.CloseWindow();
        }

        public void ConfirmAction() {
            //Debug.Log("NewGameMenuController.ConfirmAction()");
            handScript.DeleteItem(); ;
            uIManager.confirmDestroyMenuWindow.CloseWindow();
        }

    }

}