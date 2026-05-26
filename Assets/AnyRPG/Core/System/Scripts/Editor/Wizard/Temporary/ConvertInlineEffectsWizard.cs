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
    public class ConvertInlineEffectsWizard : ScriptableWizard {

        public List<Ability> abilities = new List<Ability>();
        //public List<GameObject> abilityObjects = new List<GameObject>();

        //[MenuItem("Tools/AnyRPG/Wizard/Utility/Convert Inline Effects Wizard")]
        public static void CreateWizard() {
            ScriptableWizard.DisplayWizard<ConvertInlineEffectsWizard>("New Convert InlineEffects Wizard", "Convert");
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

            EditorUtility.DisplayProgressBar("Convert Inline Effects Wizard", "Beginning Conversion...", 0f);

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
                
                /*
                foreach (AbilityEffectConfig abilityEffectConfig in ability.abilityProperties.inlineChannelingEffects) {
                    ability.abilityProperties.channeledAbilityEffectnames.Add(abilityEffectConfig.Convert(ability, existingAssetPathOnly));
                }
                */
                
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                //ability.abilityProperties.inlineChannelingEffects.Clear();
                
                /*
                foreach (AbilityEffectConfig abilityEffectConfig in ability.abilityProperties.inlineAbilityEffects) {
                    ability.abilityProperties.castEndAbilityEffectNames.Add(abilityEffectConfig.Convert(ability, existingAssetPathOnly));
                }
                */
                EditorUtility.SetDirty(ability);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                //ability.abilityProperties.inlineAbilityEffects.Clear();

            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("Convert Inline Effects Wizard", "Conversion Complete!", "OK");

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
