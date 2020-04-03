using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class MainMenuManager : MonoBehaviour {

        [SerializeField]
        private Text gameNameText = null;

        [SerializeField]
        private Text gameVersionText = null;

        private void Awake() {
            CheckMissingInspectorValues();
        }

        private void Start() {
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
            if (SystemConfigurationManager.MyInstance != null) {
                if (gameNameText != null) {
                    gameNameText.text = SystemConfigurationManager.MyInstance.MyGameName;
                }
                if (gameVersionText != null) {
                    gameVersionText.text = SystemConfigurationManager.MyInstance.MyGameVersion;
                }
            } else {
                //Debug.Log("SystemConfigurationManager Does Not Exist!");
                return;
            }
        }

    }

}