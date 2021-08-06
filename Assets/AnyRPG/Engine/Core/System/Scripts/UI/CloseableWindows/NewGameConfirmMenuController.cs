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

        // game manager references
        private UIManager uIManager = null;
        private SystemConfigurationManager systemConfigurationManager = null;
        private SaveManager saveManager = null;
        private NewGameManager newGameManager = null;

        public override void Init(SystemGameManager systemGameManager) {
            base.Init(systemGameManager);

            uIManager = systemGameManager.UIManager;
            systemConfigurationManager = systemGameManager.SystemConfigurationManager;
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