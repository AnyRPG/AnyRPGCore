using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class CopyGameMenuController : WindowContentController {

        public void CancelAction() {
            //Debug.Log("NewGameMenuController.CancelAction()");
            SystemGameManager.Instance.UIManager.SystemWindowManager.copyGameMenuWindow.CloseWindow();
        }

        public void ConfirmAction() {
            //Debug.Log("NewGameMenuController.ConfirmAction()");
            LoadGamePanel.Instance.CopyGame(true);
        }

    }

}