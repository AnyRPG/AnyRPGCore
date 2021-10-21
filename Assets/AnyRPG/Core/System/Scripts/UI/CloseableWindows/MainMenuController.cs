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

        [Header("Navigation")]

        [SerializeField]
        protected UINavigationController navigationController = new UINavigationController();

        protected UIManager uIManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            playButton.Configure(systemGameManager);
            settingsButton.Configure(systemGameManager);
            creditsButton.Configure(systemGameManager);
            exitGameButton.Configure(systemGameManager);

            navigationController.Configure(systemGameManager);
            navigationController.SetOwner(this);
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

        public override void ReceiveOpenWindowNotification() {
            navigationController.RegisterNavigationController();
            if (systemConfigurationManager.DefaultControllerConfiguration == DefaultControllerConfiguration.GamePad) {
                navigationController.FocusInitialButton();
            }
            base.ReceiveOpenWindowNotification();
        }

        public override void RecieveClosedWindowNotification() {
            navigationController.UnRegisterNavigationController();
            base.RecieveClosedWindowNotification();
        }


    }

}