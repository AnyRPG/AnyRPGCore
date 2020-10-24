using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class SystemBarController : MonoBehaviour {

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

        private void Awake() {
            //Debug.Log("BagBarController.Awake()");
        }

        private void Start() {
            //Debug.Log("BagBarController.Start()");
            if (SystemConfigurationManager.MyInstance.MySystemBarMainMenu != null) {
                mainMenuButton.Icon = SystemConfigurationManager.MyInstance.MySystemBarMainMenu;
            }
            if (SystemConfigurationManager.MyInstance.MySystemBarAbilityBook != null) {
                abilityBookButton.Icon = SystemConfigurationManager.MyInstance.MySystemBarAbilityBook;
            }
            if (SystemConfigurationManager.MyInstance.MySystemBarQuestLog != null) {
                questLogButton.Icon = SystemConfigurationManager.MyInstance.MySystemBarQuestLog;
            }
            if (SystemConfigurationManager.MyInstance.MySystemBarCharacter != null) {
                characterButton.Icon = SystemConfigurationManager.MyInstance.MySystemBarCharacter;
            }
            if (SystemConfigurationManager.MyInstance.MySystemBarMap != null) {
                mapButton.Icon = SystemConfigurationManager.MyInstance.MySystemBarMap;
            }

        }

        public void ClickMainMenu() {
            SystemWindowManager.MyInstance.inGameMainMenuWindow.ToggleOpenClose();
        }

        public void ClickAbilityBook() {
            PopupWindowManager.MyInstance.abilityBookWindow.ToggleOpenClose();
        }

        public void ClickCharacter() {
            PopupWindowManager.MyInstance.characterPanelWindow.ToggleOpenClose();
        }

        public void ClickQuestLog() {
            PopupWindowManager.MyInstance.questLogWindow.ToggleOpenClose();
        }

        public void ClickMap() {
            PopupWindowManager.MyInstance.mainMapWindow.ToggleOpenClose();
        }



    }

}