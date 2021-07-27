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
                confirmGameButton.AddSaveData(NewGamePanel.Instance.SaveData);
            } else {
                divider.SetActive(true);
                confirmGameButton.gameObject.SetActive(false);
            }
        }

        public void CancelAction() {
            //Debug.Log("NewGameMenuController.CancelAction()");
            SystemWindowManager.Instance.confirmNewGameMenuWindow.CloseWindow();
        }

        public void ConfirmAction() {
            //Debug.Log("NewGameMenuController.ConfirmAction()");
            SystemWindowManager.Instance.confirmNewGameMenuWindow.CloseWindow();
            SystemWindowManager.Instance.loadGameWindow.CloseWindow();
            SystemWindowManager.Instance.newGameWindow.CloseWindow();
            if (SystemConfigurationManager.Instance.UseNewGameWindow == true) {
                SaveManager.Instance.PerformInventorySetup();
                SaveManager.Instance.SaveEquippedBagData(NewGamePanel.Instance.SaveData);
                SaveManager.Instance.SaveInventorySlotData(NewGamePanel.Instance.SaveData);
                SaveManager.Instance.LoadGame(NewGamePanel.Instance.SaveData);
            } else {
                SaveManager.Instance.TryNewGame();
            }
        }

    }

}