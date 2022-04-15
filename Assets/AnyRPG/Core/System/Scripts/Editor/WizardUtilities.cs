using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace AnyRPG {
    public class WizardUtilities {

        public static string GetFileSystemGameName(string gameName) {
            return gameName.Replace(" ", "");
        }

        public static string GetGameFileSystemFolder(string gameParentFolder, string fileSystemGameName) {
            return Application.dataPath + gameParentFolder + fileSystemGameName;
        }

        public static string GetFilesystemSceneName(string sceneName) {
            if (sceneName == null) {
                return string.Empty;
            }
            return sceneName.Replace(" ", "");
        }

        public static string GetScriptableObjectFileSystemName(string itemName) {
            if (itemName == null) {
                return string.Empty;
            }
            return itemName.Replace(" ", "");
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

        public static string GetGameParentFolder(SystemConfigurationManager systemConfigurationManager, string gameName) {
            string gameParentFolder = "/Games/";
            string fileSystemGameName = GetFileSystemGameName(gameName);
            string assetPath = AssetDatabase.GetAssetPath(systemConfigurationManager);

            Debug.Log("SystemConfigurationManager path is " + assetPath);

            string[] pathParts = assetPath.Split('/');
            bool foundGameFolder = false;
            int gameFolderIndex = 0;

            // remove Assets from beginning of path
            if (pathParts[0] == "Assets") {
                pathParts = pathParts.Skip(1).ToArray();
            }

            // start at the end of the path and work downward until the game name is found
            for (int i = pathParts.Length - 1; i > 0; i--) {
                if (pathParts[i] == fileSystemGameName) {
                    gameFolderIndex = i;
                    foundGameFolder = true;
                    break;
                }
            }

            // if the game name was found, reconstruct the path with the parent folder of the game name being the final element
            if (foundGameFolder == true) {
                gameParentFolder = "/";
                for (int i = 0; i < gameFolderIndex; i++) {
                    gameParentFolder = gameParentFolder + pathParts[i] + "/";
                }
            }

            Debug.Log("Game parent folder is " + gameParentFolder);

            return gameParentFolder;
        }

        public static string GetGameName(SystemConfigurationManager systemConfigurationManager) {
            if (systemConfigurationManager != null) {
                return systemConfigurationManager.GameName;
            }

            // SceneConfig or SystemConfigurationManager not found.  Return empty string
            return string.Empty;
        }

        public static SystemConfigurationManager GetSystemConfigurationManager() {

            SystemConfigurationManager systemConfigurationManager = GetSceneSystemConfigurationManager();

            // this next check is to ensure we don't accidentally edit the base game manager prefab and affect all games in this Unity project
            // we only want to edit the game manager prefab variant for the current game
            if (systemConfigurationManager == null) {
                SceneConfig sceneConfig = GameObject.FindObjectOfType<SceneConfig>();
                if (sceneConfig != null) {
                    // if no system configuration manager was in the scene, but we did find a sceneConfig, then we already have a direct reference to the
                    // correct prefab variant on disk through the sceneConfig.  In this case, we can return it directly
                    systemConfigurationManager = sceneConfig.systemConfigurationManager;
                }
            } else {
                // if we did find a system configuration manager in the scene, we have a reference to the scene version of it, not the prefab variant on disk
                // in this case we need to get the prefab variant on disk
                systemConfigurationManager = PrefabUtility.GetCorrespondingObjectFromSource<SystemConfigurationManager>(systemConfigurationManager);
            }

            return systemConfigurationManager;
        }

        public static SystemConfigurationManager GetSceneSystemConfigurationManager() {
            return GameObject.FindObjectOfType<SystemConfigurationManager>();
        }

        public static bool CheckFileExists(string partialFilePath, string messageString) {

            string templateAssetPath = "Assets" + partialFilePath;
            string templateFileSystemPath = Application.dataPath + partialFilePath;

            if (System.IO.File.Exists(templateFileSystemPath) == false) {
                WizardUtilities.ShowError("Missing " + messageString + " at " + templateAssetPath + ".  Aborting...");
                return false;
            }

            return true;

        }


    }
}
