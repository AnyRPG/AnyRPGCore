using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace AnyRPG {
    /// <summary>
    /// Creates a new ability using the Ability class from one of its previous children
    /// </summary>
    public class MigrateAbilityEffectWizard : ScriptableWizard {

        public List<Ability> abilities = new List<Ability>();
        //public List<GameObject> abilityObjects = new List<GameObject>();

        //[MenuItem("Tools/AnyRPG/Wizard/Utility/Migrate Ability Effect Wizard")]
        public static void CreateWizard() {
            ScriptableWizard.DisplayWizard<MigrateAbilityEffectWizard>("New Migrate Ability Effect Wizard", "Convert");
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
            string existingAssetPath = string.Empty;
            foreach (Ability ability in abilities) {
                i++;
                EditorUtility.DisplayProgressBar("Convert Ability Wizard", "Beginning Conversion...", (float)i / (float)abilities.Count);

                // don't overwrite if they are already moved
                if (ability.abilityProperties.castEndAbilityEffectNames.Count > 0 && ability.abilityProperties.actionHitAbilityEffectNames.Count == 0) {
                    ability.abilityProperties.actionHitAbilityEffectNames = new List<string>(ability.abilityProperties.castEndAbilityEffectNames);
                    ability.abilityProperties.castEndAbilityEffectNames.Clear();
                }

                EditorUtility.SetDirty(ability);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

            }

            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("Convert Ability Wizard", "Conversion Complete!", "OK");

        }

        void OnWizardUpdate() {
            helpString = "Moves ability effects from cast end to on hit";

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
