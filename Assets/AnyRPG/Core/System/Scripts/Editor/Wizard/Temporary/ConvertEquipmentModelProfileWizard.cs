using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace AnyRPG {
    public class ConvertEquipmentModelProfileWizard : ScriptableWizard {

        public List<EquipmentModelProfile> equipmentList = new List<EquipmentModelProfile>();

        //[MenuItem("Tools/AnyRPG/Wizard/Convert/Convert Equipment Model Profile")]
        public static void CreateWizard() {
            ScriptableWizard.DisplayWizard<ConvertEquipmentModelProfileWizard>("New Convert EquipmentModelProfile Wizard", "Convert");
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

            EditorUtility.DisplayProgressBar("Convert EquipmentModelProfile Wizard", "Beginning Conversion...", 0f);

            int i = 0;
            foreach (EquipmentModelProfile equipment in equipmentList) {
                i++;
                EditorUtility.DisplayProgressBar("Convert EquipmentModelProfile Wizard", "Beginning Conversion...", (float)i / (float)equipmentList.Count);
                if (equipment == null) {
                    continue;
                }
                if (equipment.Properties.ApplyToEquipmentName != string.Empty) {
                    continue;
                }
                UpdateEquipmentModelProfile(equipment);

                EditorUtility.SetDirty(equipment);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("Convert EquipmentModelProfile Wizard", "Conversion Complete!", "OK");

        }

        private void UpdateEquipmentModelProfile(EquipmentModelProfile equipment) {
            // search inline equipment models for the first model with a valid model prefab and assign it to the dropped item pickup model prefab
            equipment.Properties.ApplyToEquipmentName = equipment.ResourceName;
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
