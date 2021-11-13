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
        //private InventoryManager inventoryManager = null;

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
            //inventoryManager = systemGameManager.InventoryManager;
        }


        public void ExitMenu() {
            //Debug.Log("MainMenuController.ExitMenu()");
            currentNavigationController.CurrentNavigableElement.DeSelect();
            uIManager.CloseAllSystemWindows();
            uIManager.exitMenuWindow.OpenWindow();
        }

        public void MainMenu() {
            //Debug.Log("MainMenuController.MainMenu()");
            currentNavigationController.CurrentNavigableElement.DeSelect();
            uIManager.CloseAllSystemWindows();
            uIManager.exitToMainMenuWindow.OpenWindow();
        }

        public void SettingsMenu() {
            //Debug.Log("MainMenuController.SettingsMenu()");
            currentNavigationController.CurrentNavigableElement.DeSelect();
            uIManager.CloseAllSystemWindows();
            uIManager.settingsMenuWindow.OpenWindow();
        }


        public void SaveGame() {
            //Debug.Log("MainMenuController.SaveGame()");
            currentNavigationController.CurrentNavigableElement.DeSelect();
            if (saveManager.SaveGame()) {
                uIManager.CloseAllSystemWindows();
                messageFeedManager.WriteMessage("Game Saved");
            }

        }

        public void ContinueGame() {
            //Debug.Log("MainMenuController.ContinueGame()");
            currentNavigationController.CurrentNavigableElement.DeSelect();
            uIManager.CloseAllSystemWindows();
        }

        public void CharacterDetails() {
            currentNavigationController.CurrentNavigableElement.DeSelect();
            uIManager.CloseAllSystemWindows();
            uIManager.characterPanelWindow.OpenWindow();
        }

        public void CharacterAbilities() {
            currentNavigationController.CurrentNavigableElement.DeSelect();
            uIManager.CloseAllSystemWindows();
            uIManager.abilityBookWindow.OpenWindow();
        }

        public void CharacterQuestLog() {
            currentNavigationController.CurrentNavigableElement.DeSelect();
            uIManager.CloseAllSystemWindows();
            uIManager.questLogWindow.OpenWindow();
        }

        public void CharacterMap() {
            currentNavigationController.CurrentNavigableElement.DeSelect();
            uIManager.CloseAllSystemWindows();
            uIManager.mainMapWindow.OpenWindow();
        }

        public void CharacterInventory() {
            currentNavigationController.CurrentNavigableElement.DeSelect();
            uIManager.CloseAllSystemWindows();
            uIManager.inventoryWindow.ToggleOpenClose();
        }

        public void CharacterSkills() {
            currentNavigationController.CurrentNavigableElement.DeSelect();
            uIManager.CloseAllSystemWindows();
            uIManager.skillBookWindow.ToggleOpenClose();
        }

        public void CharacterReputations() {
            currentNavigationController.CurrentNavigableElement.DeSelect();
            uIManager.CloseAllSystemWindows();
            uIManager.reputationBookWindow.ToggleOpenClose();
        }

        public void CharacterCurrencies() {
            currentNavigationController.CurrentNavigableElement.DeSelect();
            uIManager.CloseAllSystemWindows();
            uIManager.currencyListWindow.ToggleOpenClose();
        }

        public void CharacterAchievements() {
            currentNavigationController.CurrentNavigableElement.DeSelect();
            uIManager.CloseAllSystemWindows();
            uIManager.achievementListWindow.ToggleOpenClose();
        }


    }

}