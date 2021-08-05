using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class DeleteGameMenuController : WindowContentController {

        // game manager references
        private UIManager uIManager = null;

        public override void Init(SystemGameManager systemGameManager) {
            base.Init(systemGameManager);

            uIManager = systemGameManager.UIManager;
        }

        public void CancelAction() {
            //Debug.Log("NewGameMenuController.CancelAction()");
            uIManager.deleteGameMenuWindow.CloseWindow();
        }

        public void ConfirmAction() {
            //Debug.Log("NewGameMenuController.ConfirmAction()");
            LoadGamePanel.Instance.DeleteGame(true);
        }

    }

}