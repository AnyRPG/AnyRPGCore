using AnyRPG;
using UnityEditor;
using UnityEngine;

namespace AnyRPG {
    public class WizardUtilities {

        public static string GetFileSystemGameName(string gameName) {
            return gameName.Replace(" ", "");
        }

        public static string GetGameFolder(string gameParentFolder, string fileSystemGameName) {
            return Application.dataPath + gameParentFolder + fileSystemGameName;
        }

        public static string GetFilesystemSceneName(string sceneName) {
            if (sceneName == null) {
                return string.Empty;
            }
            return sceneName.Replace(" ", "");
        }

        public static void CreateFolderIfNotExists(string folderName) {
            if (!System.IO.Directory.Exists(folderName)) {
                Debug.Log("Create folder " + folderName);
                System.IO.Directory.CreateDirectory(folderName);
            }

            AssetDatabase.Refresh();
        }

        public static void ShowError(string message) {
            EditorUtility.DisplayDialog("Error", message, "OK");
        }


    }
}
