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
                SystemWindowManager.Instance.newGameWindow.OpenWindow();
                SystemWindowManager.Instance.playMenuWindow.CloseWindow();
            } else {
                SystemWindowManager.Instance.confirmNewGameMenuWindow.OpenWindow();
            }
        }

        public void ContinueGame() {
            //Debug.Log("PlayMenuController.ContinueGame()");
            SaveManager.Instance.LoadGame();
        }

        public void LoadGame() {
            //Debug.Log("PlayMenuController.LoadGame()");
            SystemWindowManager.Instance.loadGameWindow.OpenWindow();
            SystemWindowManager.Instance.playMenuWindow.CloseWindow();
        }

    }

}