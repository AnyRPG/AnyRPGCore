using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace AnyRPG {
    public class NewGameWizard : ScriptableWizard {

        // Template path/scene that will be used to create the new game
        private const string templateName = "TemplateGame";
        private const string pathToNewGameTemplate = "/AnyRPG/Engine/Core/Templates/Game/" + templateName;

        private const string pathToGameManagerPrefab = "Assets/AnyRPG/Engine/Core/System/Prefabs/GameManager/GameManager.prefab";
        private const string pathToUMADCSPrefab = "Getting Started/UMA_DCS.prefab";

        // Will be a subfolder of Application.dataPath and should start with "/"
        private const string newGameParentFolder = "/Games/";

        // compare the default first scene directory to any user picked scene name
        private const string defaultFirstSceneName = "FirstScene";

        // the used file path name for the game
        private string fileSystemGameName = string.Empty;
        private string fileSystemFirstSceneName = string.Empty;

        // user modified variables
        public string gameName = "";
        public string gameVersion = "0.1a";
        public string firstSceneName = "FirstScene";
        public bool addFirstSceneToBuild = true;
        public string umaRoot = "Assets/UMA/";

        [MenuItem("Tools/AnyRPG/New Game Wizard")]
        static void CreateWizard() {
            ScriptableWizard.DisplayWizard<NewGameWizard>("New Game Wizard", "Create");
        }

        void OnWizardCreate() {

            EditorUtility.DisplayProgressBar("New Game Wizard", "Checking parameters...", 0.1f);

            MakeFileSystemGameName();
            MakeFileSystemFirstSceneName();

            // Check for presence of necessary prefabs
            GameObject gameManagerGameObject = (GameObject)AssetDatabase.LoadMainAssetAtPath(pathToGameManagerPrefab);
            if (gameManagerGameObject == null) {
                ShowError("Missing GameManager prefab at " + pathToGameManagerPrefab + ".  Aborting...");
                return;
            }

            string umaPrefabPath = umaRoot + pathToUMADCSPrefab;
            GameObject umaGameObject = (GameObject)AssetDatabase.LoadMainAssetAtPath(umaPrefabPath);
            if (umaGameObject == null) {
                // NOTE: This could be changed to automagically search for the UMA prefab by asset name
                ShowError("Missing UMA prefab at " + umaPrefabPath + ".  Aborting...");
                return;
            }


            // Set default values of game properties just in case they are somehow missing
            if (gameVersion == null || gameVersion.Trim() == "") {
                gameVersion = "0.1a";
                Debug.Log("Empty game version.  Defaulting to " + gameVersion);
            }

            if (firstSceneName == null || firstSceneName.Trim() == "") {
                firstSceneName = fileSystemGameName + "Scene";
                fileSystemFirstSceneName = firstSceneName;
                Debug.Log("Empty first scene name.  Defaulting to \"" + fileSystemFirstSceneName + "\"");
            }

            EditorUtility.DisplayProgressBar("New Game Wizard", "Creating folders...", 0.2f);
            // Create root game folder
            string newGameFolder = GetNewGameFolder();
            // Copy template game to newly created folder
            CreateFolderIfNotExists(Application.dataPath + newGameParentFolder);
            FileUtil.CopyFileOrDirectory(Application.dataPath + pathToNewGameTemplate, newGameFolder);

            AssetDatabase.Refresh();

            EditorUtility.DisplayProgressBar("New Game Wizard", "Renaming folders...", 0.3f);

            // Find every folder with the name of the new game template "TemplateGame" and rename it as necessary
            // This assumes that all folders that have the template name should have their name changed and that only 
            // folders will have their name changed
            string[] templateNamedAssets = AssetDatabase.FindAssets(templateName, new string[] { FileUtil.GetProjectRelativePath(newGameFolder) });
            foreach (string templateNamedAssetGuid in templateNamedAssets) {
                string namedAssetPath = AssetDatabase.GUIDToAssetPath(templateNamedAssetGuid);
                if (System.IO.Directory.Exists(namedAssetPath)) {
                    string assetName = System.IO.Path.GetFileName(namedAssetPath);
                    string newAssetName = namedAssetPath.Replace(templateName, fileSystemGameName);
                    AssetDatabase.RenameAsset(namedAssetPath, fileSystemGameName);
                }
            }
            AssetDatabase.Refresh();

            // setup first scene paths
            string existingFirstSceneFolder = FileUtil.GetProjectRelativePath(newGameFolder + "/Scenes/" + defaultFirstSceneName);
            string newFirstSceneFolder = FileUtil.GetProjectRelativePath(newGameFolder + "/Scenes/" + fileSystemFirstSceneName);
            string newFirstSceneFileName = fileSystemFirstSceneName + ".unity";
            string newFirstScenePath = newFirstSceneFolder + "/" + newFirstSceneFileName;
            string existingFirstScenePath = existingFirstSceneFolder + "/" + defaultFirstSceneName + ".unity";

            // Rename the first scene if necessary
            if (fileSystemFirstSceneName != defaultFirstSceneName) {
                EditorUtility.DisplayProgressBar("New Game Wizard", "Renaming First Scene...", 0.4f);
                AssetDatabase.RenameAsset(existingFirstScenePath, newFirstSceneFileName);
                AssetDatabase.RenameAsset(existingFirstSceneFolder, fileSystemFirstSceneName);
                AssetDatabase.Refresh();
            }

            // add the first scene to the build if the option was chosen
            if (addFirstSceneToBuild) {
                EditorUtility.DisplayProgressBar("New Game Wizard", "Adding First Scene To Build Settings...", 0.5f);
                List<EditorBuildSettingsScene> currentSceneList = EditorBuildSettings.scenes.ToList();
                currentSceneList.Add(new EditorBuildSettingsScene(newFirstScenePath, true));
                EditorBuildSettings.scenes = currentSceneList.ToArray();
            }

            // Rename the game load scene
            EditorUtility.DisplayProgressBar("New Game Wizard", "Renaming Game Load Scene...", 0.55f);
            string gameLoadSceneFolder = FileUtil.GetProjectRelativePath(newGameFolder + "/Scenes/" + fileSystemGameName);
            string newGameLoadSceneFileName = fileSystemGameName + ".unity";
            string newGameLoadScenePath = gameLoadSceneFolder + "/" + newGameLoadSceneFileName;
            string existingGameLoadScenePath = gameLoadSceneFolder + "/" + templateName + ".unity";

            // Rename the game load scene
            AssetDatabase.RenameAsset(existingGameLoadScenePath, newGameLoadSceneFileName);
            AssetDatabase.Refresh();

            EditorUtility.DisplayProgressBar("New Game Wizard", "Modifying scene...", 0.6f);
            // Open the scene to add the necessary elements
            Scene newScene = EditorSceneManager.OpenScene(newGameLoadScenePath);

            // Create a variant of the GameManager & UMA prefabs
            EditorUtility.DisplayProgressBar("New Game Wizard", "Making prefab variants...", 0.7f);
            string prefabPath = FileUtil.GetProjectRelativePath(newGameFolder + "/Prefab");
            MakeGameManagerPrefabVariant(gameManagerGameObject, prefabPath + "/GameManager.prefab");
            MakeUMAPrefabVariant(umaGameObject, prefabPath + "/UMA_DCS.prefab", fileSystemGameName);

            /*
            // Copy over all folders
            if (System.IO.Directory.Exists(Application.dataPath + pathToResourcesFolder)) {
                string[] resourceAssets = AssetDatabase.FindAssets("*", new string[] { "Assets" + pathToResourcesFolder });
                foreach (string resourceAssetGuid in resourceAssets) {
                    string resourceAssetPath = AssetDatabase.GUIDToAssetPath(resourceAssetGuid);
                    string resourceAssetPathRoot = pathToResourcesFolder;
                    string relativeAssetPath = resourceAssetPath.Replace("Assets" + resourceAssetPathRoot + "/", "");
                    // Only directories!
                    if (System.IO.Directory.Exists(Application.dataPath + resourceAssetPathRoot + "/" + relativeAssetPath)) {
                        System.IO.Directory.CreateDirectory(newResourcesRootPath + "/" + relativeAssetPath);
                    }
                }
                AssetDatabase.Refresh();
            } else {
                Debug.LogWarning(pathToResourcesFolder + " was not found.  Resources folder will be empty.");
            }
            */

            EditorUtility.DisplayProgressBar("New Game Wizard", "Saving scene...", 0.9f);
            // Save changes to the scene
            EditorSceneManager.SaveScene(newScene);

            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("New Game Wizard", "New Game Wizard Complete! The game loading scene can be found at " + newGameLoadScenePath, "OK");

        }

        void OnWizardUpdate() {
            helpString = "Creates a new game based on the AnyRPG template";
            MakeFileSystemGameName();
            errorString = Validate();
            isValid = (errorString == null || errorString == "");
        }

        private void MakeFileSystemGameName() {
            fileSystemGameName = gameName.Replace(" ", "");
        }

        private void MakeFileSystemFirstSceneName() {
            fileSystemFirstSceneName = firstSceneName.Replace(" ", "");
        }

        string GetNewGameFolder() {
            return Application.dataPath + newGameParentFolder + fileSystemGameName;
        }

        string Validate() {
            if (gameName == null || gameName.Trim() == "") {
                return "Game name must not be empty";
            }
            if (umaRoot == null || umaRoot.Trim() == "") {
                return "UMA Root directory must not be empty.  UMA must be installed.";
            } else {
                if (!umaRoot.EndsWith("/")) {
                    return "UMA Root directory must end in \"/\".";
                }
                string strippedUMARoot = umaRoot.Substring(0, umaRoot.Length - 1);
                if (!System.IO.Directory.Exists(Application.dataPath + "/../" + strippedUMARoot)) {
                    return "UMA Root directory not found.  UMA must be installed.";
                }
            }
            string newGameFolder = GetNewGameFolder();
            if (System.IO.Directory.Exists(newGameFolder)) {
                return "Folder " + newGameFolder + " already exists.  Please delete this directory or choose a new game name";
            }

            return null;
        }

        private void ShowError(string message) {
            EditorUtility.DisplayDialog("Error", message, "OK");
        }

        private void MakeGameManagerPrefabVariant(GameObject goToMakeVariantOf, string newPath) {

            // instantiate original
            GameObject instantiatedGO = (GameObject)PrefabUtility.InstantiatePrefab(goToMakeVariantOf);
            instantiatedGO.name = fileSystemGameName + instantiatedGO.name;

            // Set the properties of the GameManager for this game
            EditorUtility.DisplayProgressBar("New Game Wizard", "Configuration Game Settings...", 0.75f);
            SystemConfigurationManager systemConfigurationManager = instantiatedGO.GetComponent<SystemConfigurationManager>();
            systemConfigurationManager.GameName = gameName;
            systemConfigurationManager.GameVersion = gameVersion;
            systemConfigurationManager.InitializationScene = fileSystemGameName;
            systemConfigurationManager.MainMenuScene = "MainMenu";
            systemConfigurationManager.DefaultStartingZone = fileSystemFirstSceneName;

            EditorUtility.DisplayProgressBar("New Game Wizard", "Copying resources...", 0.8f);
            // Create a Resources folder and point the game manager to it
            if (systemConfigurationManager.LoadResourcesFolders == null) {
                systemConfigurationManager.LoadResourcesFolders = new List<string>();
            }

            string newResourcesFolderName = fileSystemGameName;
            systemConfigurationManager.LoadResourcesFolders.Add(newResourcesFolderName);

            // make variant on disk
            GameObject variant = PrefabUtility.SaveAsPrefabAsset(instantiatedGO, newPath);

            // remove original from scene
            GameObject.DestroyImmediate(instantiatedGO);

            // instantiate new variant in scene
            //PrefabUtility.InstantiatePrefab(variant);
            GameObject sceneVariant = (GameObject)PrefabUtility.InstantiatePrefab(variant);
            sceneVariant.name = goToMakeVariantOf.name;
        }

        private GameObject MakeUMAPrefabVariant(GameObject goToMakeVariantOf, string newPath, string gameName) {

            // instantiate original
            GameObject instantiatedGO = (GameObject)PrefabUtility.InstantiatePrefab(goToMakeVariantOf);
            instantiatedGO.name = gameName + instantiatedGO.name;
            
            // make variant on disk
            GameObject variant = PrefabUtility.SaveAsPrefabAsset(instantiatedGO, newPath);

            // remove original from scene
            GameObject.DestroyImmediate(instantiatedGO);

            // instantiate new variant in scene
            //PrefabUtility.InstantiatePrefab(variant);
            GameObject sceneVariant = (GameObject)PrefabUtility.InstantiatePrefab(variant);
            sceneVariant.name = goToMakeVariantOf.name;

            return variant;
        }

        private void CreateFolderIfNotExists(string folderName) {
            if (!System.IO.Directory.Exists(folderName)) {
                System.IO.Directory.CreateDirectory(folderName);
            }

            AssetDatabase.Refresh();
        }
    }

}
