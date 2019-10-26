using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class SystemBarController : MonoBehaviour {

        [SerializeField]
        private SystemPanelButton mainMenuButton;

        [SerializeField]
        private SystemPanelButton abilityBookButton;

        [SerializeField]
        private SystemPanelButton questLogButton;

        [SerializeField]
        private SystemPanelButton characterButton;

        [SerializeField]
        private SystemPanelButton mapButton;

        private void Awake() {
            //Debug.Log("BagBarController.Awake()");
        }

        private void Start() {
            //Debug.Log("BagBarController.Start()");
            if (SystemConfigurationManager.MyInstance.MySystemBarMainMenu != null) {
                mainMenuButton.MyIcon = SystemConfigurationManager.MyInstance.MySystemBarMainMenu;
            }
            if (SystemConfigurationManager.MyInstance.MySystemBarAbilityBook != null) {
                abilityBookButton.MyIcon = SystemConfigurationManager.MyInstance.MySystemBarAbilityBook;
            }
            if (SystemConfigurationManager.MyInstance.MySystemBarQuestLog != null) {
                questLogButton.MyIcon = SystemConfigurationManager.MyInstance.MySystemBarQuestLog;
            }
            if (SystemConfigurationManager.MyInstance.MySystemBarCharacter != null) {
                characterButton.MyIcon = SystemConfigurationManager.MyInstance.MySystemBarCharacter;
            }
            if (SystemConfigurationManager.MyInstance.MySystemBarMap != null) {
                mapButton.MyIcon = SystemConfigurationManager.MyInstance.MySystemBarMap;
            }

        }



    }

}