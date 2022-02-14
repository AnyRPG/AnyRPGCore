using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace AnyRPG {
    public class ConvertAbilityEffectWizard : ScriptableWizard {

        // the used asset path for the Unit Profile
        private string scriptableObjectPath = string.Empty;

        public List<AbilityEffect> abilityEffects = new List<AbilityEffect>();

        //[MenuItem("Tools/AnyRPG/Wizard/Convert Wizard")]
        public static void CreateWizard() {
            ScriptableWizard.DisplayWizard<ConvertAbilityEffectWizard>("New Convert Ability Effect Wizard", "Create");
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

            EditorUtility.DisplayProgressBar("Convert Ability Effect Wizard", "Beginning Conversion...", 0f);

            int i = 0;
            foreach (AbilityEffect abilityEffect in abilityEffects) {
                i++;
                EditorUtility.DisplayProgressBar("Convert Ability Effect Wizard", "Beginning Conversion...", (float)i / (float)abilityEffects.Count);


                //abilityEffect.Convert();

                EditorUtility.SetDirty(abilityEffect);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }



            /*
            AssetDatabase.Refresh();

            EditorUtility.DisplayProgressBar("New Character Wizard", "Checking Default Player Unit Setting...", 0.6f);
            if (setAsDefaultPlayerCharacter == true && systemConfigurationManager != null) {
                SystemConfigurationManager diskSystemConfigurationManager = PrefabUtility.GetCorrespondingObjectFromSource<SystemConfigurationManager>(systemConfigurationManager);
                if (diskSystemConfigurationManager != null) {
                    diskSystemConfigurationManager.DefaultPlayerUnitProfileName = characterName;
                    EditorUtility.SetDirty(diskSystemConfigurationManager);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();

                   
                }
            }
            */

            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("Convert Ability Effect Wizard", "Conversion Complete!", "OK");

        }

        void OnWizardUpdate() {
            helpString = "Converts an Ability Effect";

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
