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
    public class ConvertAbilityWizard : ScriptableWizard {

        public List<Ability> abilities = new List<Ability>();
        //public List<GameObject> abilityObjects = new List<GameObject>();

        //[MenuItem("Tools/AnyRPG/Wizard/Convert Ability Wizard")]
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
            string existingAssetPath = string.Empty;
            foreach (Ability ability in abilities) {
                i++;
                EditorUtility.DisplayProgressBar("Convert Ability Wizard", "Beginning Conversion...", (float)i / (float)abilities.Count);

                existingAssetPath = AssetDatabase.GetAssetPath(ability);
                int index = existingAssetPath.LastIndexOf('/');
                string existingAssetPathOnly = existingAssetPath.Substring(0, index);
                string existingAssetFilename = existingAssetPath.Substring(index + 1);
                string existingAssetFilenameOnly = existingAssetFilename.Substring(0, existingAssetFilename.LastIndexOf("."));
                    
                Debug.Log($"Old Asset Path: {existingAssetPath}");

                Ability newAbility = ScriptableObject.CreateInstance("Ability") as Ability;
                newAbility.resourceName = ability.resourceName;
                newAbility.displayName = ability.displayName;
                newAbility.icon = ability.icon;
                newAbility.iconBackgroundImage = ability.iconBackgroundImage;
                newAbility.description = ability.description;
                newAbility.useRegionalDescription = ability.useRegionalDescription;
                newAbility.resourceDescriptionProfile = ability.resourceDescriptionProfile;
                newAbility.optionalOverride = ability.optionalOverride;
                newAbility.abilityProperties = ability.AbilityProperties;

                string scriptableObjectPath = existingAssetPathOnly + "/" + existingAssetFilenameOnly + "2.asset";
                Debug.Log($"New Asset Path: {scriptableObjectPath}");

                AssetDatabase.CreateAsset(newAbility, scriptableObjectPath);
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

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
