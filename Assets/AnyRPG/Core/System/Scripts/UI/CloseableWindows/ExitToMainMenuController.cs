using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class ExitToMainMenuController : WindowContentController {

        [SerializeField]
        private HighlightButton noButton = null;

        [SerializeField]
        private HighlightButton yesButton = null;

        // game manager references
        private UIManager uIManager = null;
        private LevelManager levelManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            noButton.Configure(systemGameManager);
            yesButton.Configure(systemGameManager);

        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
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