using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class MainMenuController : WindowContentController {

        [SerializeField]
        private HighlightButton playButton = null;

        [SerializeField]
        private HighlightButton settingsButton = null;

        [SerializeField]
        private HighlightButton creditsButton = null;

        [SerializeField]
        private HighlightButton exitGameButton = null;

        private UIManager uIManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            playButton.Configure(systemGameManager);
            settingsButton.Configure(systemGameManager);
            creditsButton.Configure(systemGameManager);
            exitGameButton.Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            uIManager = systemGameManager.UIManager;
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


    }

}