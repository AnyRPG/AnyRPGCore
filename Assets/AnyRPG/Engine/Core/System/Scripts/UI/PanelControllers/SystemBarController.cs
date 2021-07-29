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
            if (SystemGameManager.Instance.SystemConfigurationManager.SystemBarMainMenu != null) {
                mainMenuButton.Icon = SystemGameManager.Instance.SystemConfigurationManager.SystemBarMainMenu;
            }
            if (SystemGameManager.Instance.SystemConfigurationManager.SystemBarAbilityBook != null) {
                abilityBookButton.Icon = SystemGameManager.Instance.SystemConfigurationManager.SystemBarAbilityBook;
            }
            if (SystemGameManager.Instance.SystemConfigurationManager.SystemBarQuestLog != null) {
                questLogButton.Icon = SystemGameManager.Instance.SystemConfigurationManager.SystemBarQuestLog;
            }
            if (SystemGameManager.Instance.SystemConfigurationManager.SystemBarCharacter != null) {
                characterButton.Icon = SystemGameManager.Instance.SystemConfigurationManager.SystemBarCharacter;
            }
            if (SystemGameManager.Instance.SystemConfigurationManager.SystemBarMap != null) {
                mapButton.Icon = SystemGameManager.Instance.SystemConfigurationManager.SystemBarMap;
            }

        }

        public void ClickMainMenu() {
            SystemGameManager.Instance.UIManager.SystemWindowManager.inGameMainMenuWindow.ToggleOpenClose();
        }

        public void ClickAbilityBook() {
            SystemGameManager.Instance.UIManager.PopupWindowManager.abilityBookWindow.ToggleOpenClose();
        }

        public void ClickCharacter() {
            SystemGameManager.Instance.UIManager.PopupWindowManager.characterPanelWindow.ToggleOpenClose();
        }

        public void ClickQuestLog() {
            SystemGameManager.Instance.UIManager.PopupWindowManager.questLogWindow.ToggleOpenClose();
        }

        public void ClickMap() {
            SystemGameManager.Instance.UIManager.PopupWindowManager.mainMapWindow.ToggleOpenClose();
        }



    }

}