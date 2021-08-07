using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class MainMenuManager : AutoConfiguredMonoBehaviour {

        [SerializeField]
        private TextMeshProUGUI gameNameText = null;

        [SerializeField]
        private TextMeshProUGUI gameVersionText = null;

        // game manager references

        private SystemConfigurationManager systemConfigurationManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            CheckMissingInspectorValues();
            SetupGameLabels();
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            systemConfigurationManager = systemGameManager.SystemConfigurationManager;
        }

        private void CheckMissingInspectorValues() {
            if (gameNameText == null) {
                Debug.LogError("MainMenuManager.CheckMissingInspectorValues(): Please Set gameNameText in the inspector");
            }
            if (gameVersionText == null) {
                Debug.LogError("MainMenuManager.CheckMissingInspectorValues(): Please Set gameVersionText in the inspector");
            }
        }

        private void SetupGameLabels() {
            if (SystemGameManager.Instance.SystemConfigurationManager != null) {
                if (gameNameText != null) {
                    gameNameText.text = systemConfigurationManager.GameName;
                }
                if (gameVersionText != null) {
                    gameVersionText.text = systemConfigurationManager.GameVersion;
                }
            } else {
                //Debug.Log("SystemConfigurationManager Does Not Exist!");
                return;
            }
        }

    }

}