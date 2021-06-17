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
            if (SystemConfigurationManager.Instance.SystemBarMainMenu != null) {
                mainMenuButton.Icon = SystemConfigurationManager.Instance.SystemBarMainMenu;
            }
            if (SystemConfigurationManager.Instance.SystemBarAbilityBook != null) {
                abilityBookButton.Icon = SystemConfigurationManager.Instance.SystemBarAbilityBook;
            }
            if (SystemConfigurationManager.Instance.SystemBarQuestLog != null) {
                questLogButton.Icon = SystemConfigurationManager.Instance.SystemBarQuestLog;
            }
            if (SystemConfigurationManager.Instance.SystemBarCharacter != null) {
                characterButton.Icon = SystemConfigurationManager.Instance.SystemBarCharacter;
            }
            if (SystemConfigurationManager.Instance.SystemBarMap != null) {
                mapButton.Icon = SystemConfigurationManager.Instance.SystemBarMap;
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