using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class MainMenuManager : MonoBehaviour {

        [SerializeField]
        private TextMeshProUGUI gameNameText = null;

        [SerializeField]
        private TextMeshProUGUI gameVersionText = null;

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
            if (SystemConfigurationManager.Instance != null) {
                if (gameNameText != null) {
                    gameNameText.text = SystemConfigurationManager.Instance.GameName;
                }
                if (gameVersionText != null) {
                    gameVersionText.text = SystemConfigurationManager.Instance.GameVersion;
                }
            } else {
                //Debug.Log("SystemConfigurationManager Does Not Exist!");
                return;
            }
        }

    }

}