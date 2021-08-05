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
        private UIManager uIManager = null;
        private SaveManager saveManager = null;
        private MessageFeedManager messageFeedManager = null;

        public override void Init(SystemGameManager systemGameManager) {
            base.Init(systemGameManager);

            systemConfigurationManager = systemGameManager.SystemConfigurationManager;
            saveManager = systemGameManager.SaveManager;
            uIManager = systemGameManager.UIManager;
            messageFeedManager = uIManager.MessageFeedManager;

            if (mainMenuButton != null
                && systemConfigurationManager.MainMenuSceneNode == null
                && systemConfigurationManager.MainMenuScene == string.Empty) {
                mainMenuButton.interactable = false;
            }
        }

        public void PlayMenu() {
            //Debug.Log("MainMenuController.PlayMenu()");
            uIManager.exitMenuWindow.CloseWindow();
            uIManager.deleteGameMenuWindow.CloseWindow();
            uIManager.settingsMenuWindow.CloseWindow();
            uIManager.playMenuWindow.OpenWindow();
        }

        public void ExitMenu() {
            //Debug.Log("MainMenuController.ExitMenu()");
            uIManager.playMenuWindow.CloseWindow();
            uIManager.deleteGameMenuWindow.CloseWindow();
            uIManager.exitMenuWindow.OpenWindow();
        }

        public void MainMenu() {
            //Debug.Log("MainMenuController.MainMenu()");
            uIManager.exitToMainMenuWindow.OpenWindow();
        }

        public void SettingsMenu() {
            //Debug.Log("MainMenuController.SettingsMenu()");
            uIManager.playMenuWindow.CloseWindow();
            uIManager.deleteGameMenuWindow.CloseWindow();
            //systemWindowManager.mainMenuWindow.CloseWindow();
            uIManager.settingsMenuWindow.OpenWindow();
        }

        public void CreditsMenu() {
            //Debug.Log("MainMenuController.SettingsMenu()");
            uIManager.playMenuWindow.CloseWindow();
            uIManager.deleteGameMenuWindow.CloseWindow();
            //systemWindowManager.mainMenuWindow.CloseWindow();
            uIManager.settingsMenuWindow.CloseWindow();
            uIManager.creditsWindow.OpenWindow();
        }

        public void SaveGame() {
            //Debug.Log("MainMenuController.SaveGame()");
            if (saveManager.SaveGame()) {
                uIManager.CloseAllSystemWindows();
                messageFeedManager.WriteMessage("Game Saved");
            }

        }

        public void ContinueGame() {
            //Debug.Log("MainMenuController.ContinueGame()");
            uIManager.CloseAllSystemWindows();
        }

    }

}