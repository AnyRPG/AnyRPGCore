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
            SystemGameManager.Instance.UIManager.SystemWindowManager.exitMenuWindow.CloseWindow();
            SystemGameManager.Instance.UIManager.SystemWindowManager.deleteGameMenuWindow.CloseWindow();
            SystemGameManager.Instance.UIManager.SystemWindowManager.settingsMenuWindow.CloseWindow();
            SystemGameManager.Instance.UIManager.SystemWindowManager.playMenuWindow.OpenWindow();
        }

        public void ExitMenu() {
            //Debug.Log("MainMenuController.ExitMenu()");
            SystemGameManager.Instance.UIManager.SystemWindowManager.playMenuWindow.CloseWindow();
            SystemGameManager.Instance.UIManager.SystemWindowManager.deleteGameMenuWindow.CloseWindow();
            SystemGameManager.Instance.UIManager.SystemWindowManager.exitMenuWindow.OpenWindow();
        }

        public void MainMenu() {
            //Debug.Log("MainMenuController.MainMenu()");
            SystemGameManager.Instance.UIManager.SystemWindowManager.exitToMainMenuWindow.OpenWindow();
        }

        public void SettingsMenu() {
            //Debug.Log("MainMenuController.SettingsMenu()");
            SystemGameManager.Instance.UIManager.SystemWindowManager.playMenuWindow.CloseWindow();
            SystemGameManager.Instance.UIManager.SystemWindowManager.deleteGameMenuWindow.CloseWindow();
            //SystemGameManager.Instance.UIManager.SystemWindowManager.mainMenuWindow.CloseWindow();
            SystemGameManager.Instance.UIManager.SystemWindowManager.settingsMenuWindow.OpenWindow();
        }

        public void CreditsMenu() {
            //Debug.Log("MainMenuController.SettingsMenu()");
            SystemGameManager.Instance.UIManager.SystemWindowManager.playMenuWindow.CloseWindow();
            SystemGameManager.Instance.UIManager.SystemWindowManager.deleteGameMenuWindow.CloseWindow();
            //SystemGameManager.Instance.UIManager.SystemWindowManager.mainMenuWindow.CloseWindow();
            SystemGameManager.Instance.UIManager.SystemWindowManager.settingsMenuWindow.CloseWindow();
            SystemGameManager.Instance.UIManager.SystemWindowManager.creditsWindow.OpenWindow();
        }

        public void SaveGame() {
            //Debug.Log("MainMenuController.SaveGame()");
            if (SystemGameManager.Instance.SaveManager.SaveGame()) {
                SystemGameManager.Instance.UIManager.SystemWindowManager.CloseAllWindows();
                SystemGameManager.Instance.UIManager.MessageFeedManager.WriteMessage("Game Saved");
            }

        }

        public void ContinueGame() {
            //Debug.Log("MainMenuController.ContinueGame()");
            SystemGameManager.Instance.UIManager.SystemWindowManager.CloseAllWindows();
        }

    }

}