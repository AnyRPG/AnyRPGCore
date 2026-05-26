using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AnyRPG {
    public class RenameWizard : ScriptableWizard {

        public string findString = string.Empty;
        public string replaceString = string.Empty;
        public ReplaceMode replaceMode = ReplaceMode.Replace;

        public List<Object> fileList = new List<Object>();

        [MenuItem("Tools/AnyRPG/Wizard/Rename/Rename Resources")]
        public static void CreateWizard() {
            ScriptableWizard.DisplayWizard<RenameWizard>("New Rename Wizard", "Rename");
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

            EditorUtility.DisplayProgressBar("Convert Equipment Wizard", "Beginning Conversion...", 0f);

            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
            try {
                int i = 0;
                foreach (Object fileObject in fileList) {
                    i++;
                    EditorUtility.DisplayProgressBar("Rename Wizard", "Beginning Rename...", (float)i / (float)fileList.Count);
                    if (fileObject == null) {
                        continue;
                    }
                    string newName = fileObject.name;
                    if (replaceMode == ReplaceMode.Replace) {
                        newName = fileObject.name.Replace(findString, replaceString);
                    } else if (replaceMode == ReplaceMode.Append) {
                        newName = newName + replaceString;
                    } else if (replaceMode == ReplaceMode.Prepend) {
                        newName = replaceString + newName;
                    }
                    Debug.Log($"Renaming {AssetDatabase.GetAssetPath(fileObject)} to {newName}");

                    AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(fileObject), newName);
                }
            } catch {
                EditorUtility.DisplayDialog("Rename Wizard", "Error Encountered!", "OK");
                Cleanup();
                throw;
            }

            EditorUtility.DisplayDialog("Rename Wizard", "Rename Complete!", "OK");
            Cleanup();
        }

        private void Cleanup() {
            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.ScriptOnly);
            EditorUtility.ClearProgressBar();
            AssetDatabase.Refresh();

        }

        void OnWizardUpdate() {
            helpString = "Mass Rename objects";

            errorString = Validate();
            isValid = (errorString == null || errorString == "");
        }

        string Validate() {

            if (findString == string.Empty && replaceMode == ReplaceMode.Replace) {
                return "Find String must not be empty";
            }
            if (replaceString == string.Empty && replaceMode != ReplaceMode.Replace) {
                return "Replace String must not be empty for Append or Prepend";
            }

            return null;
        }

        private void ShowError(string message) {
            EditorUtility.DisplayDialog("Error", message, "OK");
        }
       
    }

    public enum ReplaceMode { Replace, Append, Prepend }

}
