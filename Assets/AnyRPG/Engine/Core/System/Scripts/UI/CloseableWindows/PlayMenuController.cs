using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class PlayMenuController : WindowContentController {

        [SerializeField]
        private HighlightButton continueButton = null;

        [SerializeField]
        private HighlightButton newGameButton = null;

        [SerializeField]
        private HighlightButton loadGameButton = null;

        // game manager references
        private UIManager uIManager = null;
        private SaveManager saveManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            continueButton.Configure(systemGameManager);
            newGameButton.Configure(systemGameManager);
            loadGameButton.Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            uIManager = systemGameManager.UIManager;
            saveManager = systemGameManager.SaveManager;
        }

        public void NewGame() {
            //Debug.Log("PlayMenuController.NewGame()");
            if (systemConfigurationManager.UseNewGameWindow == true) {
                uIManager.newGameWindow.OpenWindow();
                uIManager.playMenuWindow.CloseWindow();
            } else {
                uIManager.confirmNewGameMenuWindow.OpenWindow();
            }
        }

        public void ContinueGame() {
            //Debug.Log("PlayMenuController.ContinueGame()");
            saveManager.LoadGame();
        }

        public void LoadGame() {
            //Debug.Log("PlayMenuController.LoadGame()");
            uIManager.loadGameWindow.OpenWindow();
            uIManager.playMenuWindow.CloseWindow();
        }

    }

}