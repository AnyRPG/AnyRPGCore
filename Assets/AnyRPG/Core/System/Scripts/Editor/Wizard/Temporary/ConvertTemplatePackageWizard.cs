using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace AnyRPG {
    public class ConvertTemplatePackageWizard : ScriptableWizard {

        public List<ScriptableContentTemplate> targetList = new List<ScriptableContentTemplate>();
        public List<DescribableResource> sourceList = new List<DescribableResource>();

        [MenuItem("Tools/AnyRPG/Wizard/Convert/Populate Template Package")]
        public static void CreateWizard() {
            ScriptableWizard.DisplayWizard<ConvertTemplatePackageWizard>("New Convert ScriptableContentTemplate Wizard", "Convert");
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

            EditorUtility.DisplayProgressBar("Convert ScriptableContentTemplate Wizard", "Beginning Conversion...", 0f);

            int i = 0;
            foreach (ScriptableContentTemplate targetTemplate in targetList) {
                i++;
                EditorUtility.DisplayProgressBar("Convert ScriptableContentTemplate Wizard", "Beginning Conversion...", (float)i / (float)targetList.Count);
                if (targetTemplate == null) {
                    continue;
                }
                UpdateScriptableContentTemplate(targetTemplate);

                EditorUtility.SetDirty(targetTemplate);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("Convert ScriptableContentTemplate Wizard", "Conversion Complete!", "OK");

        }

        private void UpdateScriptableContentTemplate(ScriptableContentTemplate targetTemplate) {
            // search inline equipment models for the first model with a valid model prefab and assign it to the dropped item pickup model prefab
            // copy properties from unitprefabprops nameplateprops to unitprefabprops unitpreviewprops
            
            foreach (DescribableResource sourceTemplate in sourceList) {
                if (sourceTemplate == null) {
                    continue;
                }
                if (sourceTemplate.name.Replace("EquipmentModelTemplate", "") == targetTemplate.name.Replace("TemplatePackage", "")) {
                    targetTemplate.Resources.Add(sourceTemplate);
                    break;
                }
            }
        }

        void OnWizardUpdate() {
            helpString = "Populates fields in scriptable content templates based on source templates";

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
