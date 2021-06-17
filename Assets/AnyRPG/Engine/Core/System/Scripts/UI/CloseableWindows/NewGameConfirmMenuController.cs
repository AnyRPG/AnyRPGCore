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

        public override void ReceiveOpenWindowNotification() {
            base.ReceiveOpenWindowNotification();
            if (SystemConfigurationManager.Instance.UseNewGameWindow == true) {
                divider.SetActive(false);
                confirmGameButton.gameObject.SetActive(true);
                confirmGameButton.AddSaveData(NewGamePanel.MyInstance.SaveData);
            } else {
                divider.SetActive(true);
                confirmGameButton.gameObject.SetActive(false);
            }
        }

        public void CancelAction() {
            //Debug.Log("NewGameMenuController.CancelAction()");
            SystemWindowManager.MyInstance.confirmNewGameMenuWindow.CloseWindow();
        }

        public void ConfirmAction() {
            //Debug.Log("NewGameMenuController.ConfirmAction()");
            SystemWindowManager.MyInstance.confirmNewGameMenuWindow.CloseWindow();
            SystemWindowManager.MyInstance.loadGameWindow.CloseWindow();
            SystemWindowManager.MyInstance.newGameWindow.CloseWindow();
            if (SystemConfigurationManager.Instance.UseNewGameWindow == true) {
                SaveManager.MyInstance.PerformInventorySetup();
                SaveManager.MyInstance.SaveEquippedBagData(NewGamePanel.MyInstance.SaveData);
                SaveManager.MyInstance.SaveInventorySlotData(NewGamePanel.MyInstance.SaveData);
                SaveManager.MyInstance.LoadGame(NewGamePanel.MyInstance.SaveData);
            } else {
                SaveManager.MyInstance.TryNewGame();
            }
        }

    }

}