using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace AnyRPG {
    public class ConvertStatusEffectWizard : ScriptableWizard {

        // the used asset path for the Unit Profile
        private string scriptableObjectPath = string.Empty;

        public List<StatusEffect> statusEffects = new List<StatusEffect>();

        //[MenuItem("Tools/AnyRPG/Wizard/Utility/Convert Status Effect Wizard")]
        public static void CreateWizard() {
            ScriptableWizard.DisplayWizard<ConvertStatusEffectWizard>("New Convert Status Effect Wizard", "Create");
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

            EditorUtility.DisplayProgressBar("Convert Status Effect Wizard", "Beginning Conversion...", 0f);

            int i = 0;
            foreach (StatusEffect statusEffect in statusEffects) {
                i++;
                EditorUtility.DisplayProgressBar("Convert Status Effect Wizard", "Beginning Conversion...", (float)i / (float)statusEffects.Count);

                //statusEffect.statusEffectProperties.StatusEffectObjectList = statusEffect.statusEffectProperties.AbilityObjectList;

                EditorUtility.SetDirty(statusEffect);
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
            EditorUtility.DisplayDialog("Convert Status Effect Wizard", "Conversion Complete!", "OK");

        }

        void OnWizardUpdate() {
            helpString = "Converts an Status Effect";

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
