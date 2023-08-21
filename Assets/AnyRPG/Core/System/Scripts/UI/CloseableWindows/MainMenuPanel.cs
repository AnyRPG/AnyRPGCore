using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class MainMenuPanel : WindowContentController {

        [Header("Main Menu")]

        [SerializeField]
        private HighlightButton playOnlineButton = null;

        [SerializeField]
        private HighlightButton playOfflineButton = null;

        /*

        [SerializeField]
        private HighlightButton settingsButton = null;

        [SerializeField]
        private HighlightButton creditsButton = null;

        [SerializeField]
        private HighlightButton exitGameButton = null;
        */

        // game manager references
        protected UIManager uIManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            /*
            playButton.Configure(systemGameManager);
            settingsButton.Configure(systemGameManager);
            creditsButton.Configure(systemGameManager);
            exitGameButton.Configure(systemGameManager);
            */
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            uIManager = systemGameManager.UIManager;
        }

        public override void ProcessOpenWindowNotification() {
            base.ProcessOpenWindowNotification();
            if (systemConfigurationManager.AllowOfflinePlay == true) {
                playOfflineButton.gameObject.SetActive(true);
            } else {
                playOfflineButton.gameObject.SetActive(false);
            }
            if (systemConfigurationManager.AllowOnlinePlay == true) {
                playOnlineButton.gameObject.SetActive(true);
            } else {
                playOnlineButton.gameObject.SetActive(false);
            }
        }

        public void PlayMenu() {
            //Debug.Log("MainMenuController.PlayMenu()");
            uIManager.exitMenuWindow.CloseWindow();
            uIManager.deleteGameMenuWindow.CloseWindow();
            uIManager.settingsMenuWindow.CloseWindow();
            uIManager.networkLoginWindow.CloseWindow();
            uIManager.playMenuWindow.OpenWindow();
        }

        public void NetworkLoginMenu() {
            //Debug.Log("MainMenuController.NetworkMenu()");
            uIManager.exitMenuWindow.CloseWindow();
            uIManager.playMenuWindow.CloseWindow();
            uIManager.deleteGameMenuWindow.CloseWindow();
            uIManager.settingsMenuWindow.CloseWindow();
            uIManager.networkLoginWindow.OpenWindow();
        }

        public void ExitMenu() {
            //Debug.Log("MainMenuController.ExitMenu()");
            uIManager.playMenuWindow.CloseWindow();
            uIManager.deleteGameMenuWindow.CloseWindow();
            uIManager.networkLoginWindow.CloseWindow();
            uIManager.exitMenuWindow.OpenWindow();
        }

        public void SettingsMenu() {
            //Debug.Log("MainMenuController.SettingsMenu()");
            uIManager.playMenuWindow.CloseWindow();
            uIManager.deleteGameMenuWindow.CloseWindow();
            //systemWindowManager.mainMenuWindow.CloseWindow();
            uIManager.networkLoginWindow.CloseWindow();
            uIManager.settingsMenuWindow.OpenWindow();
        }

        public void CreditsMenu() {
            //Debug.Log("MainMenuController.SettingsMenu()");
            uIManager.playMenuWindow.CloseWindow();
            uIManager.deleteGameMenuWindow.CloseWindow();
            //systemWindowManager.mainMenuWindow.CloseWindow();
            uIManager.settingsMenuWindow.CloseWindow();
            uIManager.networkLoginWindow.CloseWindow();
            uIManager.creditsWindow.OpenWindow();
        }


    }

}