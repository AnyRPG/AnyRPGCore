using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class ConfirmDestroyPanelController : WindowContentController {

        public void CancelAction() {
            //Debug.Log("NewGameMenuController.CancelAction()");
            SystemGameManager.Instance.UIManager.SystemWindowManager.confirmDestroyMenuWindow.CloseWindow();
        }

        public void ConfirmAction() {
            //Debug.Log("NewGameMenuController.ConfirmAction()");
            HandScript.Instance.DeleteItem(); ;
            SystemGameManager.Instance.UIManager.SystemWindowManager.confirmDestroyMenuWindow.CloseWindow();
        }

    }

}