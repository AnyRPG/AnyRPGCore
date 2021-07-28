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

        public override void Init() {
            base.Init();
            if (mainMenuButton != null
                && SystemConfigurationManager.Instance.MainMenuSceneNode == null
                && SystemConfigurationManager.Instance.MainMenuScene == string.Empty) {
                mainMenuButton.interactable = false;
            }
        }

        public void PlayMenu() {
            //Debug.Log("MainMenuController.PlayMenu()");
            SystemWindowManager.Instance.exitMenuWindow.CloseWindow();
            SystemWindowManager.Instance.deleteGameMenuWindow.CloseWindow();
            SystemWindowManager.Instance.settingsMenuWindow.CloseWindow();
            SystemWindowManager.Instance.playMenuWindow.OpenWindow();
        }

        public void ExitMenu() {
            //Debug.Log("MainMenuController.ExitMenu()");
            SystemWindowManager.Instance.playMenuWindow.CloseWindow();
            SystemWindowManager.Instance.deleteGameMenuWindow.CloseWindow();
            SystemWindowManager.Instance.exitMenuWindow.OpenWindow();
        }

        public void MainMenu() {
            //Debug.Log("MainMenuController.MainMenu()");
            SystemWindowManager.Instance.exitToMainMenuWindow.OpenWindow();
        }

        public void SettingsMenu() {
            //Debug.Log("MainMenuController.SettingsMenu()");
            SystemWindowManager.Instance.playMenuWindow.CloseWindow();
            SystemWindowManager.Instance.deleteGameMenuWindow.CloseWindow();
            //SystemWindowManager.Instance.mainMenuWindow.CloseWindow();
            SystemWindowManager.Instance.settingsMenuWindow.OpenWindow();
        }

        public void CreditsMenu() {
            //Debug.Log("MainMenuController.SettingsMenu()");
            SystemWindowManager.Instance.playMenuWindow.CloseWindow();
            SystemWindowManager.Instance.deleteGameMenuWindow.CloseWindow();
            //SystemWindowManager.Instance.mainMenuWindow.CloseWindow();
            SystemWindowManager.Instance.settingsMenuWindow.CloseWindow();
            SystemWindowManager.Instance.creditsWindow.OpenWindow();
        }

        public void SaveGame() {
            //Debug.Log("MainMenuController.SaveGame()");
            if (SystemGameManager.Instance.SaveManager.SaveGame()) {
                SystemWindowManager.Instance.CloseAllWindows();
                MessageFeedManager.Instance.WriteMessage("Game Saved");
            }

        }

        public void ContinueGame() {
            //Debug.Log("MainMenuController.ContinueGame()");
            SystemWindowManager.Instance.CloseAllWindows();
        }

    }

}