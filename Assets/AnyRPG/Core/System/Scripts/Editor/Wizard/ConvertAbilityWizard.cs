using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace AnyRPG {
    public class ConvertAbilityWizard : ScriptableWizard {

        public List<BaseAbility> baseAbilities = new List<BaseAbility>();

        [MenuItem("Tools/AnyRPG/Wizard/Convert Ability Wizard")]
        public static void CreateWizard() {
            ScriptableWizard.DisplayWizard<ConvertAbilityWizard>("New Convert Ability Wizard", "Convert");
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

            EditorUtility.DisplayProgressBar("Convert Ability Wizard", "Beginning Conversion...", 0f);

            int i = 0;
            foreach (BaseAbility baseAbility in baseAbilities) {
                i++;
                EditorUtility.DisplayProgressBar("Convert Ability Effect Wizard", "Beginning Conversion...", (float)i / (float)baseAbilities.Count);


                //baseAbility.Convert();

                EditorUtility.SetDirty(baseAbility);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            //AssetDatabase.Refresh();

            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("Convert Ability Wizard", "Conversion Complete!", "OK");

        }

        void OnWizardUpdate() {
            helpString = "Converts an Ability";

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
