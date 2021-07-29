using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class DeleteGameMenuController : WindowContentController {

        public void CancelAction() {
            //Debug.Log("NewGameMenuController.CancelAction()");
            SystemGameManager.Instance.UIManager.SystemWindowManager.deleteGameMenuWindow.CloseWindow();
        }

        public void ConfirmAction() {
            //Debug.Log("NewGameMenuController.ConfirmAction()");
            LoadGamePanel.Instance.DeleteGame(true);
        }

    }

}