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
            if (SystemGameManager.Instance.SystemConfigurationManager.UseNewGameWindow == true) {
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
            SystemGameManager.Instance.UIManager.SystemWindowManager.confirmNewGameMenuWindow.CloseWindow();
        }

        public void ConfirmAction() {
            //Debug.Log("NewGameMenuController.ConfirmAction()");
            SystemGameManager.Instance.UIManager.SystemWindowManager.confirmNewGameMenuWindow.CloseWindow();
            SystemGameManager.Instance.UIManager.SystemWindowManager.loadGameWindow.CloseWindow();
            SystemGameManager.Instance.UIManager.SystemWindowManager.newGameWindow.CloseWindow();
            if (SystemGameManager.Instance.SystemConfigurationManager.UseNewGameWindow == true) {
                SystemGameManager.Instance.SaveManager.PerformInventorySetup();
                SystemGameManager.Instance.SaveManager.SaveEquippedBagData(NewGamePanel.Instance.SaveData);
                SystemGameManager.Instance.SaveManager.SaveInventorySlotData(NewGamePanel.Instance.SaveData);
                SystemGameManager.Instance.SaveManager.LoadGame(NewGamePanel.Instance.SaveData);
            } else {
                SystemGameManager.Instance.SaveManager.TryNewGame();
            }
        }

    }

}