using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class ExitToMainMenuController : WindowContentController {

        public void CancelExit() {
            //Debug.Log("ExitMenuController.CancelExit()");
            SystemWindowManager.MyInstance.exitToMainMenuWindow.CloseWindow();
        }

        public void ConfirmExit() {
            //Debug.Log("ExitMenuController.ConfirmExit()");
            SystemWindowManager.MyInstance.exitToMainMenuWindow.CloseWindow();
            SystemWindowManager.MyInstance.playerOptionsMenuWindow.CloseWindow();
            LevelManager.MyInstance.LoadMainMenu();
        }

    }

}