using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class MainMenuController : WindowContentController {

        [SerializeField]
        private Button mainMenuButton = null;

        public override void Awake() {
            base.Awake();
            if (mainMenuButton != null && SystemConfigurationManager.MyInstance.MainMenuSceneNode == null) {
                mainMenuButton.interactable = false;
            }
        }

        public void PlayMenu() {
            //Debug.Log("MainMenuController.PlayMenu()");
            SystemWindowManager.MyInstance.exitMenuWindow.CloseWindow();
            SystemWindowManager.MyInstance.deleteGameMenuWindow.CloseWindow();
            SystemWindowManager.MyInstance.settingsMenuWindow.CloseWindow();
            SystemWindowManager.MyInstance.playMenuWindow.OpenWindow();
        }

        public void ExitMenu() {
            //Debug.Log("MainMenuController.ExitMenu()");
            SystemWindowManager.MyInstance.playMenuWindow.CloseWindow();
            SystemWindowManager.MyInstance.deleteGameMenuWindow.CloseWindow();
            SystemWindowManager.MyInstance.exitMenuWindow.OpenWindow();
        }

        public void MainMenu() {
            //Debug.Log("MainMenuController.MainMenu()");
            SystemWindowManager.MyInstance.exitToMainMenuWindow.OpenWindow();
        }

        public void SettingsMenu() {
            //Debug.Log("MainMenuController.SettingsMenu()");
            SystemWindowManager.MyInstance.playMenuWindow.CloseWindow();
            SystemWindowManager.MyInstance.deleteGameMenuWindow.CloseWindow();
            //SystemWindowManager.MyInstance.mainMenuWindow.CloseWindow();
            SystemWindowManager.MyInstance.settingsMenuWindow.OpenWindow();
        }

        public void CreditsMenu() {
            //Debug.Log("MainMenuController.SettingsMenu()");
            SystemWindowManager.MyInstance.playMenuWindow.CloseWindow();
            SystemWindowManager.MyInstance.deleteGameMenuWindow.CloseWindow();
            //SystemWindowManager.MyInstance.mainMenuWindow.CloseWindow();
            SystemWindowManager.MyInstance.settingsMenuWindow.CloseWindow();
            SystemWindowManager.MyInstance.creditsWindow.OpenWindow();
        }

        public void SaveGame() {
            //Debug.Log("MainMenuController.SaveGame()");
            if (SaveManager.MyInstance.SaveGame()) {
                SystemWindowManager.MyInstance.CloseAllWindows();
                MessageFeedManager.MyInstance.WriteMessage("Game Saved");
            }

        }

        public void ContinueGame() {
            //Debug.Log("MainMenuController.ContinueGame()");
            SystemWindowManager.MyInstance.CloseAllWindows();
        }

    }

}