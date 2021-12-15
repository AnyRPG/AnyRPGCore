using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class SystemBarController : ConfiguredMonoBehaviour {

        /*
        [SerializeField]
        private RectTransform rectTransform = null;
        */

        [SerializeField]
        private SystemPanelButton mainMenuButton = null;

        [SerializeField]
        private SystemPanelButton abilityBookButton = null;

        [SerializeField]
        private SystemPanelButton questLogButton = null;

        [SerializeField]
        private SystemPanelButton characterButton = null;

        [SerializeField]
        private SystemPanelButton mapButton = null;

        [SerializeField]
        private SystemPanelButton skillsButton = null;

        [SerializeField]
        private SystemPanelButton reputationsButton = null;

        [SerializeField]
        private SystemPanelButton currenciesButton = null;

        [SerializeField]
        private SystemPanelButton achievementsButton = null;

        [SerializeField]
        private SystemPanelButton inventoryButton = null;

        // game manager references
        UIManager uIManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            mainMenuButton.Configure(systemGameManager);
            abilityBookButton.Configure(systemGameManager);
            questLogButton.Configure(systemGameManager);
            characterButton.Configure(systemGameManager);
            mapButton.Configure(systemGameManager);
            skillsButton.Configure(systemGameManager);
            reputationsButton.Configure(systemGameManager);
            currenciesButton.Configure(systemGameManager);
            achievementsButton.Configure(systemGameManager);
            inventoryButton.Configure(systemGameManager);

            mainMenuButton.SetTooltipTransform(uIManager.BottomPanel.RectTransform);
            abilityBookButton.SetTooltipTransform(uIManager.BottomPanel.RectTransform);
            questLogButton.SetTooltipTransform(uIManager.BottomPanel.RectTransform);
            characterButton.SetTooltipTransform(uIManager.BottomPanel.RectTransform);
            mapButton.SetTooltipTransform(uIManager.BottomPanel.RectTransform);
            skillsButton.SetTooltipTransform(uIManager.BottomPanel.RectTransform);
            reputationsButton.SetTooltipTransform(uIManager.BottomPanel.RectTransform);
            currenciesButton.SetTooltipTransform(uIManager.BottomPanel.RectTransform);
            achievementsButton.SetTooltipTransform(uIManager.BottomPanel.RectTransform);
            inventoryButton.SetTooltipTransform(uIManager.BottomPanel.RectTransform);


            if (systemConfigurationManager.SystemBarMainMenu != null) {
                mainMenuButton.Icon = systemConfigurationManager.SystemBarMainMenu;
            }
            if (systemConfigurationManager.SystemBarAbilityBook != null) {
                abilityBookButton.Icon = systemConfigurationManager.SystemBarAbilityBook;
            }
            if (systemConfigurationManager.SystemBarQuestLog != null) {
                questLogButton.Icon = systemConfigurationManager.SystemBarQuestLog;
            }
            if (systemConfigurationManager.SystemBarCharacter != null) {
                characterButton.Icon = systemConfigurationManager.SystemBarCharacter;
            }
            if (systemConfigurationManager.SystemBarMap != null) {
                mapButton.Icon = systemConfigurationManager.SystemBarMap;
            }
            if (systemConfigurationManager.SystemBarSkills != null) {
                skillsButton.Icon = systemConfigurationManager.SystemBarSkills;
            }
            if (systemConfigurationManager.SystemBarReputations != null) {
                reputationsButton.Icon = systemConfigurationManager.SystemBarReputations;
            }
            if (systemConfigurationManager.SystemBarCurrencies != null) {
                currenciesButton.Icon = systemConfigurationManager.SystemBarCurrencies;
            }
            if (systemConfigurationManager.SystemBarAchievements != null) {
                achievementsButton.Icon = systemConfigurationManager.SystemBarAchievements;
            }
            if (systemConfigurationManager.SystemBarInventory != null) {
                inventoryButton.Icon = systemConfigurationManager.SystemBarInventory;
            }
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            uIManager = systemGameManager.UIManager;
        }


        public void ClickMainMenu() {
            uIManager.inGameMainMenuWindow.ToggleOpenClose();
        }

        public void ClickAbilityBook() {
            uIManager.abilityBookWindow.ToggleOpenClose();
        }

        public void ClickCharacter() {
            uIManager.characterPanelWindow.ToggleOpenClose();
        }

        public void ClickQuestLog() {
            uIManager.questLogWindow.ToggleOpenClose();
        }

        public void ClickMap() {
            uIManager.mainMapWindow.ToggleOpenClose();
        }

        public void ClickSkills() {
            uIManager.skillBookWindow.ToggleOpenClose();
        }

        public void ClickReputations() {
            uIManager.reputationBookWindow.ToggleOpenClose();
        }

        public void ClickCurrencies() {
            uIManager.currencyListWindow.ToggleOpenClose();
        }

        public void ClickAchievements() {
            uIManager.achievementListWindow.ToggleOpenClose();
        }

        public void ClickInventory() {
            uIManager.inventoryWindow.ToggleOpenClose();
        }


    }

}