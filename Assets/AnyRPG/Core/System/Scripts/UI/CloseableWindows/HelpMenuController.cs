using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class HelpMenuController : WindowContentController {

        /*
        [SerializeField]
        private HighlightButton mainMenuButton = null;
        */

        private UIManager uIManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            uIManager = systemGameManager.UIManager;
        }


        public void KeyboardHints() {
            //Debug.Log("MainMenuController.ExitMenu()");
            currentNavigationController?.CurrentNavigableElement?.DeSelect();
            uIManager.CloseAllSystemWindows();
            uIManager.exitMenuWindow.OpenWindow();
        }

        public void GamepadHints() {
            //Debug.Log("MainMenuController.MainMenu()");
            currentNavigationController?.CurrentNavigableElement?.DeSelect();
            uIManager.CloseAllSystemWindows();
            uIManager.gamepadHintWindow.OpenWindow();
        }

        public void CharacterStuck() {
            //Debug.Log("MainMenuController.SettingsMenu()");
            currentNavigationController?.CurrentNavigableElement?.DeSelect();
            uIManager.CloseAllSystemWindows();
            uIManager.confirmCharacterStuckWindow.OpenWindow();
        }

     

    }

}