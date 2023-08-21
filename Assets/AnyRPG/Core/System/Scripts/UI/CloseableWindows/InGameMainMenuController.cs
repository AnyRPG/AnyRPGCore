using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class InGameMainMenuController : WindowContentController {

        [Header("Main Menu")]

        /*
        [SerializeField]
        private HighlightButton settingsButton = null;

        [SerializeField]
        private HighlightButton continueButton = null;
        */

        [SerializeField]
        private HighlightButton mainMenuButton = null;

        [SerializeField]
        private HighlightButton logoutButton = null;

        [SerializeField]
        private HighlightButton saveGameButton = null;

        /*
        [SerializeField]
        private HighlightButton exitGameButton = null;
        */

        private UIManager uIManager = null;
        private SaveManager saveManager = null;
        private MessageFeedManager messageFeedManager = null;
        private PlayerManager playerManager = null;

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
            playerManager = systemGameManager.PlayerManager;
        }

        public override void ProcessOpenWindowNotification() {
            base.ProcessOpenWindowNotification();
            if (systemGameManager.GameMode == GameMode.Local) {
                mainMenuButton.gameObject.SetActive(true);
                saveGameButton.gameObject.SetActive(true);
                logoutButton.gameObject.SetActive(false);
            } else if (systemGameManager.GameMode == GameMode.Network) {
                mainMenuButton.gameObject.SetActive(false);
                saveGameButton.gameObject.SetActive(false);
                logoutButton.gameObject.SetActive(true);
            }
            foreach (UINavigationController uINavigation in uINavigationControllers) {
                uINavigation.UpdateNavigationList();
            }
        }


        public void ExitMenu() {
            //Debug.Log("MainMenuController.ExitMenu()");
            currentNavigationController?.CurrentNavigableElement?.DeSelect();
            uIManager.CloseSystemPopupWindows();
            uIManager.exitMenuWindow.OpenWindow();
        }

        public void MainMenu() {
            //Debug.Log("MainMenuController.MainMenu()");
            currentNavigationController?.CurrentNavigableElement?.DeSelect();
            uIManager.CloseSystemPopupWindows();
            uIManager.exitToMainMenuWindow.OpenWindow();
        }

        public void SettingsMenu() {
            //Debug.Log("MainMenuController.SettingsMenu()");
            currentNavigationController?.CurrentNavigableElement?.DeSelect();
            uIManager.CloseSystemPopupWindows();
            uIManager.settingsMenuWindow.OpenWindow();
        }

        public void HelpMenu() {
            //Debug.Log("MainMenuController.SettingsMenu()");
            currentNavigationController?.CurrentNavigableElement?.DeSelect();
            uIManager.CloseSystemPopupWindows();
            uIManager.helpMenuWindow.OpenWindow();
        }


        public void SaveGame() {
            //Debug.Log("MainMenuController.SaveGame()");
            currentNavigationController?.CurrentNavigableElement?.DeSelect();
            if (saveManager.SaveGame(playerManager.PlayerCharacterSaveData.SaveData)) {
                uIManager.CloseSystemPopupWindows();
                messageFeedManager.WriteMessage("Game Saved");
            }
        }

        public void Logout() {
            //Debug.Log("MainMenuController.ContinueGame()");
            currentNavigationController?.CurrentNavigableElement?.DeSelect();
            uIManager.CloseSystemPopupWindows();
            uIManager.confirmLogoutWindow.OpenWindow();
        }

        public void ContinueGame() {
            //Debug.Log("MainMenuController.ContinueGame()");
            currentNavigationController?.CurrentNavigableElement?.DeSelect();
            uIManager.CloseSystemPopupWindows();
        }

        public void CharacterDetails() {
            currentNavigationController.CurrentNavigableElement.DeSelect();
            uIManager.CloseSystemPopupWindows();
            uIManager.characterPanelWindow.OpenWindow();
        }

        public void CharacterAbilities() {
            currentNavigationController.CurrentNavigableElement.DeSelect();
            uIManager.CloseSystemPopupWindows();
            uIManager.abilityBookWindow.OpenWindow();
        }

        public void CharacterQuestLog() {
            currentNavigationController.CurrentNavigableElement.DeSelect();
            uIManager.CloseSystemPopupWindows();
            uIManager.questLogWindow.OpenWindow();
        }

        public void CharacterMap() {
            currentNavigationController.CurrentNavigableElement.DeSelect();
            uIManager.CloseSystemPopupWindows();
            uIManager.mainMapWindow.OpenWindow();
        }

        public void CharacterInventory() {
            currentNavigationController.CurrentNavigableElement.DeSelect();
            uIManager.CloseSystemPopupWindows();
            uIManager.inventoryWindow.ToggleOpenClose();
        }

        public void CharacterSkills() {
            currentNavigationController.CurrentNavigableElement.DeSelect();
            uIManager.CloseSystemPopupWindows();
            uIManager.skillBookWindow.ToggleOpenClose();
        }

        public void CharacterReputations() {
            currentNavigationController.CurrentNavigableElement.DeSelect();
            uIManager.CloseSystemPopupWindows();
            uIManager.reputationBookWindow.ToggleOpenClose();
        }

        public void CharacterCurrencies() {
            currentNavigationController.CurrentNavigableElement.DeSelect();
            uIManager.CloseSystemPopupWindows();
            uIManager.currencyListWindow.ToggleOpenClose();
        }

        public void CharacterAchievements() {
            currentNavigationController.CurrentNavigableElement.DeSelect();
            uIManager.CloseSystemPopupWindows();
            uIManager.achievementListWindow.ToggleOpenClose();
        }


    }

}