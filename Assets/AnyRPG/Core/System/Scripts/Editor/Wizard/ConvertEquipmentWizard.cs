using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace AnyRPG {
    public class ConvertEquipmentWizard : ScriptableWizard {

        public List<Equipment> equipmentList = new List<Equipment>();

        [MenuItem("Tools/AnyRPG/Wizard/Convert/Convert Equipment to 0.14.2a")]
        public static void CreateWizard() {
            ScriptableWizard.DisplayWizard<ConvertEquipmentWizard>("New Convert Equipment Wizard", "Convert");
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

            EditorUtility.DisplayProgressBar("Convert Equipment Wizard", "Beginning Conversion...", 0f);

            int i = 0;
            foreach (Equipment equipment in equipmentList) {
                i++;
                EditorUtility.DisplayProgressBar("Convert Equipment Wizard", "Beginning Conversion...", (float)i / (float)equipmentList.Count);


                if (equipment.DeprecatedUseUMARecipe == true) {
                    equipment.UmaRecipeProfileName = equipment.ResourceName;
                }

                EditorUtility.SetDirty(equipment);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("Convert Equipment Wizard", "Conversion Complete!", "OK");

        }

        void OnWizardUpdate() {
            helpString = "Converts older equipment to 0.14.2a compatible";

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
