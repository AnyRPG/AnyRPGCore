using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class ExitToMainMenuController : WindowContentController {

        public void CancelExit() {
            //Debug.Log("ExitMenuController.CancelExit()");
            SystemWindowManager.Instance.exitToMainMenuWindow.CloseWindow();
        }

        public void ConfirmExit() {
            //Debug.Log("ExitMenuController.ConfirmExit()");
            SystemWindowManager.Instance.exitToMainMenuWindow.CloseWindow();
            SystemWindowManager.Instance.playerOptionsMenuWindow.CloseWindow();
            LevelManager.Instance.LoadMainMenu();
        }

    }

}