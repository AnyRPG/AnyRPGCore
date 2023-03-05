using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace AnyRPG {
    public class ConvertUMARecipeProfileWizard : ScriptableWizard {

        public List<UMARecipeProfile> convertList = new List<UMARecipeProfile>();

        //[MenuItem("Tools/AnyRPG/Wizard/Convert/Convert UMARecipeProfile to 0.14.2a")]
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
            foreach (UMARecipeProfile convertItem in convertList) {
                i++;
                EditorUtility.DisplayProgressBar("Convert UMARecipeProfile Wizard", "Beginning Conversion...", (float)i / (float)convertList.Count);

                //convertItem.Properties.SharedColors = convertItem.DeprecatedSharedColors;
                //convertItem.Properties.UMARecipes = convertItem.DeprecatedUMARecipes;

                EditorUtility.SetDirty(convertItem);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("Convert UMARecipeProfile Wizard", "Conversion Complete!", "OK");

        }

        void OnWizardUpdate() {
            helpString = "Converts older UMARecipeProfile to 0.14.2a compatible";

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
