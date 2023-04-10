using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace AnyRPG {
    public class ConvertUMARecipeProfileWizard : ScriptableWizard {

        public List<EquipmentModelProfile> convertList = new List<EquipmentModelProfile>();

        //[MenuItem("Tools/AnyRPG/Wizard/Convert/Convert EquipmentModelProfile to 0.16")]
        public static void CreateWizard() {
            ScriptableWizard.DisplayWizard<ConvertUMARecipeProfileWizard>("New Convert UMARecipeProfile Wizard", "Convert");
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

            EditorUtility.DisplayProgressBar("Convert UMARecipeProfile Wizard", "Beginning Conversion...", 0f);

            int i = 0;
            foreach (EquipmentModelProfile convertItem in convertList) {
                i++;
                EditorUtility.DisplayProgressBar("Convert UMARecipeProfile Wizard", "Beginning Conversion...", (float)i / (float)convertList.Count);

                UMAEquipmentModel umaEquipmentModel = new UMAEquipmentModel();
                //umaEquipmentModel.Properties = convertItem.DeprecatedUMARecipeProfileProperties;
                convertItem.Properties.EquipmentModels.Add(umaEquipmentModel);

                EditorUtility.SetDirty(convertItem);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("Convert UMARecipeProfile Wizard", "Conversion Complete!", "OK");

        }

        void OnWizardUpdate() {
            helpString = "Converts older UMARecipeProfile to EquipmentModelProfile";

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
