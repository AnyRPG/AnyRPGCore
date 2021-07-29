using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class PlayMenuController : WindowContentController {

        public void NewGame() {
            //Debug.Log("PlayMenuController.NewGame()");
            if (SystemConfigurationManager.Instance.UseNewGameWindow == true) {
                SystemGameManager.Instance.UIManager.SystemWindowManager.newGameWindow.OpenWindow();
                SystemGameManager.Instance.UIManager.SystemWindowManager.playMenuWindow.CloseWindow();
            } else {
                SystemGameManager.Instance.UIManager.SystemWindowManager.confirmNewGameMenuWindow.OpenWindow();
            }
        }

        public void ContinueGame() {
            //Debug.Log("PlayMenuController.ContinueGame()");
            SystemGameManager.Instance.SaveManager.LoadGame();
        }

        public void LoadGame() {
            //Debug.Log("PlayMenuController.LoadGame()");
            SystemGameManager.Instance.UIManager.SystemWindowManager.loadGameWindow.OpenWindow();
            SystemGameManager.Instance.UIManager.SystemWindowManager.playMenuWindow.CloseWindow();
        }

    }

}