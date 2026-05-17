using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace AnyRPG {
    public class SyncResourceNameWizard : ScriptableWizard {

        public List<DescribableResource> fileList = new List<DescribableResource>();

        [Tooltip("If there is a common string in the file names that you want to ignore, enter it here. This string will not be included in the updated Resource Name.")]
        public string ignoreString = string.Empty;

        [MenuItem("Tools/AnyRPG/Wizard/Rename/Sync Resource Name")]
        public static void CreateWizard() {
            ScriptableWizard.DisplayWizard<SyncResourceNameWizard>("New Sync Resource Name Wizard", "Sync Name");
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

            EditorUtility.DisplayProgressBar("Sync Resource Name Wizard", "Beginning Conversion...", 0f);

            int i = 0;
            foreach (DescribableResource describableResource in fileList) {
                i++;
                EditorUtility.DisplayProgressBar("Sync Resource Name Wizard", "Beginning Conversion...", (float)i / (float)fileList.Count);
                if (describableResource == null) {
                    continue;
                }
                // set the resource name to the same name as the file, using Pascal Case to determine spaces
                string fileName = System.IO.Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(describableResource));
                // remove ignorestring from fileName
                if (ignoreString.Trim() != string.Empty) {
                    fileName = fileName.Replace(ignoreString, "");
                }
                string resourceName = System.Text.RegularExpressions.Regex.Replace(fileName, "([a-z])([A-Z])", "$1 $2");
                describableResource.ResourceName = resourceName;

                EditorUtility.SetDirty(describableResource);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("Sync Resource Name Wizard", "Conversion Complete!", "OK");

        }

        void OnWizardUpdate() {
            helpString = "Sets the resource name to the same name as the file, replacing Pascal casing with spaces";

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
