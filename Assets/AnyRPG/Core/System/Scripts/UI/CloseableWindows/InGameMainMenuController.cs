using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class InGameMainMenuController : WindowContentController {

        /*
        [SerializeField]
        private HighlightButton saveButton = null;

        [SerializeField]
        private HighlightButton settingsButton = null;

        [SerializeField]
        private HighlightButton continueButton = null;
        */

        [SerializeField]
        private HighlightButton mainMenuButton = null;

        /*
        [SerializeField]
        private HighlightButton exitGameButton = null;
        */

        private UIManager uIManager = null;
        private SaveManager saveManager = null;
        private MessageFeedManager messageFeedManager = null;
        private InventoryManager inventoryManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            if (mainMenuButton != null
                && systemConfigurationManager.MainMenuSceneNode == null
                && systemConfigurationManager.MainMenuScene == string.Empty) {
                mainMenuButton.Button.interactable = false;
            }

            /*
            saveButton.Configure(systemGameManager);
            settingsButton.Configure(systemGameManager);
            continueButton.Configure(systemGameManager);
            mainMenuButton.Configure(systemGameManager);
            exitGameButton.Configure(systemGameManager);
            */
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            saveManager = systemGameManager.SaveManager;
            uIManager = systemGameManager.UIManager;
            messageFeedManager = uIManager.MessageFeedManager;
            inventoryManager = systemGameManager.InventoryManager;
        }


        public void ExitMenu() {
            //Debug.Log("MainMenuController.ExitMenu()");
            uIManager.CloseAllSystemWindows();
            uIManager.exitMenuWindow.OpenWindow();
        }

        public void MainMenu() {
            //Debug.Log("MainMenuController.MainMenu()");
            uIManager.CloseAllSystemWindows();
            uIManager.exitToMainMenuWindow.OpenWindow();
        }

        public void SettingsMenu() {
            //Debug.Log("MainMenuController.SettingsMenu()");
            uIManager.CloseAllSystemWindows();
            uIManager.settingsMenuWindow.OpenWindow();
        }


        public void SaveGame() {
            //Debug.Log("MainMenuController.SaveGame()");
            if (saveManager.SaveGame()) {
                uIManager.CloseAllSystemWindows();
                messageFeedManager.WriteMessage("Game Saved");
            }

        }

        public void ContinueGame() {
            //Debug.Log("MainMenuController.ContinueGame()");
            uIManager.CloseAllSystemWindows();
        }

        public void CharacterDetails() {
            uIManager.CloseAllSystemWindows();
            uIManager.characterPanelWindow.OpenWindow();
        }

        public void CharacterAbilities() {
            uIManager.CloseAllSystemWindows();
            uIManager.abilityBookWindow.OpenWindow();
        }

        public void CharacterQuestLog() {
            uIManager.CloseAllSystemWindows();
            uIManager.questLogWindow.OpenWindow();
        }

        public void CharacterMap() {
            uIManager.CloseAllSystemWindows();
            uIManager.mainMapWindow.OpenWindow();
        }

        public void CharacterInventory() {
            uIManager.CloseAllSystemWindows();
            inventoryManager.OpenClose();
        }


    }

}