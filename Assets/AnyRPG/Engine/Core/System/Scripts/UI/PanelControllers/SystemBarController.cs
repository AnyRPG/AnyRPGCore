using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class SystemBarController : ConfiguredMonoBehaviour {

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

        // game manager references
        SystemConfigurationManager systemConfigurationManager = null;
        UIManager uIManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            systemConfigurationManager = systemGameManager.SystemConfigurationManager;
            uIManager = systemGameManager.UIManager;

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



    }

}