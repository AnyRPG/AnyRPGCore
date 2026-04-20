using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace AnyRPG {
    public class ConvertUnitProfileWizard : ScriptableWizard {

        public List<UnitPrefabProfile> unitProfileList = new List<UnitPrefabProfile>();

        //[MenuItem("Tools/AnyRPG/Wizard/Convert/Convert UnitProfile nameplate settings")]
        public static void CreateWizard() {
            ScriptableWizard.DisplayWizard<ConvertUnitProfileWizard>("New Convert UnitProfile Wizard", "Convert");
        }

        void OnEnable() {
            /*
            systemConfigurationManager = GameObject.FindObjectOfType<SystemConfigurationManager>();
            if (systemConfigurationManager != null) {
                gameName = systemConfigurationManager.GameName;
            }
            */
        }

        void OnWizardCreate() {

            EditorUtility.DisplayProgressBar("Convert UnitProfile Wizard", "Beginning Conversion...", 0f);

            int i = 0;
            foreach (UnitPrefabProfile unitProfile in unitProfileList) {
                i++;
                EditorUtility.DisplayProgressBar("Convert UnitProfile Wizard", "Beginning Conversion...", (float)i / (float)unitProfileList.Count);
                if (unitProfile == null) {
                    continue;
                }
                UpdateUnitProfile(unitProfile);

                EditorUtility.SetDirty(unitProfile);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("Convert UnitProfile Wizard", "Conversion Complete!", "OK");

        }

        private void UpdateUnitProfile(UnitPrefabProfile unitProfile) {
            // search inline equipment models for the first model with a valid model prefab and assign it to the dropped item pickup model prefab
            // copy properties from unitprefabprops nameplateprops to unitprefabprops unitpreviewprops
            
            if (unitProfile?.UnitPrefabProps == null) {
                return;
            }
            unitProfile.UnitPrefabProps.UnitPreviewProps.UnitPreviewTarget = unitProfile.UnitPrefabProps.UnitPreviewProps.UnitPreviewTarget;
            unitProfile.UnitPrefabProps.UnitPreviewProps.UnitPreviewCameraLookOffset = unitProfile.UnitPrefabProps.UnitPreviewProps.UnitPreviewCameraLookOffset;
            unitProfile.UnitPrefabProps.UnitPreviewProps.UnitPreviewCameraPositionOffset = unitProfile.UnitPrefabProps.UnitPreviewProps.UnitPreviewCameraPositionOffset;
            unitProfile.UnitPrefabProps.UnitFrameProps.UseSnapShot = unitProfile.UnitPrefabProps.UnitFrameProps.UseSnapShot;
            unitProfile.UnitPrefabProps.UnitFrameProps.UnitFrameTarget = unitProfile.UnitPrefabProps.UnitFrameProps.UnitFrameTarget;
            unitProfile.UnitPrefabProps.UnitFrameProps.UnitFrameCameraLookOffset = unitProfile.UnitPrefabProps.UnitFrameProps.UnitFrameCameraLookOffset;
            unitProfile.UnitPrefabProps.UnitFrameProps.UnitFrameCameraPositionOffset = unitProfile.UnitPrefabProps.UnitFrameProps.UnitFrameCameraPositionOffset;
        }

        void OnWizardUpdate() {
            helpString = "Converts equipment to have dropped items if the equipment has a prefab model associated with it";

            errorString = Validate();
            isValid = (errorString == null || errorString == "");
        }

        string Validate() {

            /*
            // check for empty game name
            if (gameName == null || gameName.Trim() == "") {
                return "Game name must not be empty";
            }
            */

            return null;
        }

        private void ShowError(string message) {
            EditorUtility.DisplayDialog("Error", message, "OK");
        }
       
    }

}
