using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class PlayMenuController : WindowContentController {

        [SerializeField]
        private HighlightButton continueButton = null;

        [SerializeField]
        private HighlightButton newGameButton = null;

        [SerializeField]
        private HighlightButton loadGameButton = null;

        [Header("Navigation")]

        [SerializeField]
        protected UINavigationController navigationController = new UINavigationController();

        // game manager references
        protected UIManager uIManager = null;
        protected SaveManager saveManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            continueButton.Configure(systemGameManager);
            newGameButton.Configure(systemGameManager);
            loadGameButton.Configure(systemGameManager);

            navigationController.Configure(systemGameManager);
            navigationController.SetOwner(this);

        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            uIManager = systemGameManager.UIManager;
            saveManager = systemGameManager.SaveManager;
        }

        public void NewGame() {
            //Debug.Log("PlayMenuController.NewGame()");
            if (systemConfigurationManager.UseNewGameWindow == true) {
                uIManager.newGameWindow.OpenWindow();
                uIManager.playMenuWindow.CloseWindow();
            } else {
                uIManager.confirmNewGameMenuWindow.OpenWindow();
            }
        }

        public void ContinueGame() {
            //Debug.Log("PlayMenuController.ContinueGame()");
            saveManager.LoadGame();
        }

        public void LoadGame() {
            //Debug.Log("PlayMenuController.LoadGame()");
            uIManager.loadGameWindow.OpenWindow();
            uIManager.playMenuWindow.CloseWindow();
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