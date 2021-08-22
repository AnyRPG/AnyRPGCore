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

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            CheckMissingInspectorValues();
            SetupGameLabels();
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
            if (systemConfigurationManager != null) {
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