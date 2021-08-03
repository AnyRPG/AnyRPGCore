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

        private SystemConfigurationManager systemConfigurationManager = null;
        private SystemWindowManager systemWindowManager = null;
        private SaveManager saveManager = null;
        private MessageFeedManager messageFeedManager = null;

        public override void Init(SystemGameManager systemGameManager) {
            base.Init(systemGameManager);

            systemConfigurationManager = systemGameManager.SystemConfigurationManager;
            systemWindowManager = systemGameManager.UIManager.SystemWindowManager;
            saveManager = systemGameManager.SaveManager;
            messageFeedManager = systemGameManager.UIManager.MessageFeedManager;

            if (mainMenuButton != null
                && systemConfigurationManager.MainMenuSceneNode == null
                && systemConfigurationManager.MainMenuScene == string.Empty) {
                mainMenuButton.interactable = false;
            }
        }

        public void PlayMenu() {
            //Debug.Log("MainMenuController.PlayMenu()");
            systemWindowManager.exitMenuWindow.CloseWindow();
            systemWindowManager.deleteGameMenuWindow.CloseWindow();
            systemWindowManager.settingsMenuWindow.CloseWindow();
            systemWindowManager.playMenuWindow.OpenWindow();
        }

        public void ExitMenu() {
            //Debug.Log("MainMenuController.ExitMenu()");
            systemWindowManager.playMenuWindow.CloseWindow();
            systemWindowManager.deleteGameMenuWindow.CloseWindow();
            systemWindowManager.exitMenuWindow.OpenWindow();
        }

        public void MainMenu() {
            //Debug.Log("MainMenuController.MainMenu()");
            systemWindowManager.exitToMainMenuWindow.OpenWindow();
        }

        public void SettingsMenu() {
            //Debug.Log("MainMenuController.SettingsMenu()");
            systemWindowManager.playMenuWindow.CloseWindow();
            systemWindowManager.deleteGameMenuWindow.CloseWindow();
            //systemWindowManager.mainMenuWindow.CloseWindow();
            systemWindowManager.settingsMenuWindow.OpenWindow();
        }

        public void CreditsMenu() {
            //Debug.Log("MainMenuController.SettingsMenu()");
            systemWindowManager.playMenuWindow.CloseWindow();
            systemWindowManager.deleteGameMenuWindow.CloseWindow();
            //systemWindowManager.mainMenuWindow.CloseWindow();
            systemWindowManager.settingsMenuWindow.CloseWindow();
            systemWindowManager.creditsWindow.OpenWindow();
        }

        public void SaveGame() {
            //Debug.Log("MainMenuController.SaveGame()");
            if (saveManager.SaveGame()) {
                systemWindowManager.CloseAllWindows();
                messageFeedManager.WriteMessage("Game Saved");
            }

        }

        public void ContinueGame() {
            //Debug.Log("MainMenuController.ContinueGame()");
            systemWindowManager.CloseAllWindows();
        }

    }

}