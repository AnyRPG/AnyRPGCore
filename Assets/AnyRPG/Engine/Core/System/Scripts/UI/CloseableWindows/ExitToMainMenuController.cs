using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class ExitToMainMenuController : WindowContentController {

        public void CancelExit() {
            //Debug.Log("ExitMenuController.CancelExit()");
            SystemGameManager.Instance.UIManager.SystemWindowManager.exitToMainMenuWindow.CloseWindow();
        }

        public void ConfirmExit() {
            //Debug.Log("ExitMenuController.ConfirmExit()");
            SystemGameManager.Instance.UIManager.SystemWindowManager.exitToMainMenuWindow.CloseWindow();
            SystemGameManager.Instance.UIManager.SystemWindowManager.playerOptionsMenuWindow.CloseWindow();
            SystemGameManager.Instance.LevelManager.LoadMainMenu();
        }

    }

}