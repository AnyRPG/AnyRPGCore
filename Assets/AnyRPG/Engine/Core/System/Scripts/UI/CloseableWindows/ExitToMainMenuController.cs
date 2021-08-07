using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class ExitToMainMenuController : WindowContentController {

        // game manager references
        private UIManager uIManager = null;
        private LevelManager levelManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            uIManager = systemGameManager.UIManager;
            levelManager = systemGameManager.LevelManager;
        }

        public void CancelExit() {
            //Debug.Log("ExitMenuController.CancelExit()");
            uIManager.exitToMainMenuWindow.CloseWindow();
        }

        public void ConfirmExit() {
            //Debug.Log("ExitMenuController.ConfirmExit()");
            uIManager.exitToMainMenuWindow.CloseWindow();
            uIManager.playerOptionsMenuWindow.CloseWindow();
            levelManager.LoadMainMenu();
        }

    }

}