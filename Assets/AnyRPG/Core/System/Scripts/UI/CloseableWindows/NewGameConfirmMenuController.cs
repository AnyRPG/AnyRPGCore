using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class NewGameConfirmMenuController : WindowContentController {

        [SerializeField]
        private GameObject divider = null;

        [SerializeField]
        private ConfirmGameButton confirmGameButton = null;

        /*
        [SerializeField]
        private HighlightButton confirmButton = null;

        [SerializeField]
        private HighlightButton cancelButton = null;
        */

        // game manager references
        private UIManager uIManager = null;
        private SaveManager saveManager = null;
        private NewGameManager newGameManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            confirmGameButton.Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            uIManager = systemGameManager.UIManager;
            saveManager = systemGameManager.SaveManager;
            newGameManager = systemGameManager.NewGameManager;
        }

        public override void ReceiveOpenWindowNotification() {
            base.ReceiveOpenWindowNotification();
            if (systemConfigurationManager.UseNewGameWindow == true) {
                divider.SetActive(false);
                confirmGameButton.gameObject.SetActive(true);
                confirmGameButton.AddSaveData(newGameManager.SaveData);
            } else {
                divider.SetActive(true);
                confirmGameButton.gameObject.SetActive(false);
            }
        }

        public void CancelAction() {
            //Debug.Log("NewGameMenuController.CancelAction()");
            uIManager.confirmNewGameMenuWindow.CloseWindow();
        }

        public void ConfirmAction() {
            //Debug.Log("NewGameMenuController.ConfirmAction()");
            uIManager.confirmNewGameMenuWindow.CloseWindow();
            uIManager.loadGameWindow.CloseWindow();
            uIManager.newGameWindow.CloseWindow();
            if (systemConfigurationManager.UseNewGameWindow == true) {
                saveManager.PerformInventorySetup();
                saveManager.SaveEquippedBagData(newGameManager.SaveData);
                saveManager.SaveInventorySlotData(newGameManager.SaveData);
                saveManager.LoadGame(newGameManager.SaveData);
            } else {
                saveManager.TryNewGame();
            }
        }

    }

}