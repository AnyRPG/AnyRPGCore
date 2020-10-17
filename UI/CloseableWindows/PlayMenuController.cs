using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class PlayMenuController : WindowContentController {

        public void NewGame() {
            //Debug.Log("PlayMenuController.NewGame()");
            if (SystemConfigurationManager.MyInstance.UseNewGameWindow == true) {
                SystemWindowManager.MyInstance.newGameWindow.OpenWindow();
            } else {
                SystemWindowManager.MyInstance.confirmNewGameMenuWindow.OpenWindow();
            }
        }

        public void ContinueGame() {
            //Debug.Log("PlayMenuController.ContinueGame()");
            SaveManager.MyInstance.LoadGame();
        }

        public void LoadGame() {
            //Debug.Log("PlayMenuController.LoadGame()");
            SystemWindowManager.MyInstance.loadGameWindow.OpenWindow();
        }

    }

}