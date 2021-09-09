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
        private const string pathToNewGameTemplate = "/AnyRPG/Core/Templates/Game/" + templateName;

        // for now this is necessary due to git not saving empty folders
        private const string pathToResourcesTemplateFolder = "/AnyRPG/Core/Games/FeaturesDemo/Resources/FeaturesDemoGame";

        private const string pathToGameManagerPrefab = "Assets/AnyRPG/Core/System/Prefabs/GameManager/GameManager.prefab";
        private const string pathToUMADCSPrefab = "Getting Started/UMA_GLIB.prefab";

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

        [Header("Third Party Controller")]
        public bool useThirdPartyController = false;

        [Tooltip("This should be a unit in the scene")]
        public GameObject thirdPartyCharacterUnit = null;

        // the version that is saved on disk
        private GameObject thirdPartyCharacterPrefab = null;
        private GameObject thirdPartyCameraPrefab = null;

        [MenuItem("Tools/AnyRPG/Wizard/New Game Wizard")]
        public static void CreateWizard() {
            ScriptableWizard.DisplayWizard<NewGameWizard>("New Game Wizard", "Create");
        }

        void OnEnable() {
            thirdPartyCharacterUnit = Selection.activeGameObject;
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

            EditorUtility.DisplayProgressBar("New Game Wizard", "Creating Game Folder...", 0.2f);
            // Create root game folder
            string newGameFolder = GetNewGameFolder();
            string resourcesFolder = newGameFolder + "/Resources/" + fileSystemGameName;

            // create base games folder
            CreateFolderIfNotExists(Application.dataPath + newGameParentFolder);

            EditorUtility.DisplayProgressBar("New Game Wizard", "Copying Game Template Directory...", 0.3f);

            // create game folder structure
            FileUtil.CopyFileOrDirectory(Application.dataPath + pathToNewGameTemplate, newGameFolder);

            AssetDatabase.Refresh();

            EditorUtility.DisplayProgressBar("New Game Wizard", "Renaming Template Folders...", 0.4f);

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

            EditorUtility.DisplayProgressBar("New Game Wizard", "Create Resource Folder If Necessary...", 0.5f);

            // create resources folder if one didn't already get created by the copy operation
            CreateFolderIfNotExists(resourcesFolder);

            EditorUtility.DisplayProgressBar("New Game Wizard", "Making resource folder structure...", 0.6f);
            // Copy over all folders because git won't commit empty folders
            if (System.IO.Directory.Exists(Application.dataPath + pathToResourcesTemplateFolder)) {
                string[] resourceAssets = AssetDatabase.FindAssets("*", new string[] { "Assets" + pathToResourcesTemplateFolder });
                foreach (string resourceAssetGuid in resourceAssets) {
                    string resourceAssetPath = AssetDatabase.GUIDToAssetPath(resourceAssetGuid);
                    string resourceAssetPathRoot = pathToResourcesTemplateFolder;
                    string relativeAssetPath = resourceAssetPath.Replace("Assets" + resourceAssetPathRoot + "/", "");
                    // Only directories!
                    //Debug.Log("Checking for " + Application.dataPath + resourceAssetPathRoot + "/" + relativeAssetPath);
                    if (System.IO.Directory.Exists(Application.dataPath + resourceAssetPathRoot + "/" + relativeAssetPath)) {
                        System.IO.Directory.CreateDirectory(resourcesFolder + "/" + relativeAssetPath);
                    }
                }
                AssetDatabase.Refresh();
            } else {
                Debug.LogWarning(pathToResourcesTemplateFolder + " was not found.  Resources folder will be empty.");
            }

            // create prefab folder
            string prefabFolder = newGameFolder + "/Prefab";
            string prefabPath = FileUtil.GetProjectRelativePath(prefabFolder);
            CreateFolderIfNotExists(prefabFolder);


            if (useThirdPartyController == true) {
                EditorUtility.DisplayProgressBar("New Game Wizard", "Creating Third Party Prefabs and Resources...", 0.65f);

                // copy unit profile
                string unitProfileTemplatePath = "/AnyRPG/Core/Templates/Resource/UnitProfile/InvectorUMAPlayerUnitTemplate.asset";
                FileUtil.CopyFileOrDirectory(Application.dataPath + unitProfileTemplatePath, resourcesFolder + "/UnitProfile/InvectorUMAPlayerUnit.asset");
                //AssetDatabase.RenameAsset(resourcesFolder + "/UnitProfile/InvectorUMAPlayerUnitTemplate.asset", "InvectorUMAPlayerUnit.asset");
                
                // make prefab on disk
                if (thirdPartyCharacterUnit != null) {
                    string thirdpartyCharacterPrefabPath = FileUtil.GetProjectRelativePath(prefabFolder) + "/" + thirdPartyCharacterUnit.name + ".prefab";
                    thirdPartyCharacterPrefab = PrefabUtility.SaveAsPrefabAsset(thirdPartyCharacterUnit, thirdpartyCharacterPrefabPath);

                    AssetDatabase.Refresh();

                    // link disk prefab into unit profile
                    string unitProfilePath = fileSystemGameName + "/UnitProfile/InvectorUMAPlayerUnit";
                    UnitProfile unitProfile = Resources.Load<UnitProfile>(unitProfilePath);
                    if (unitProfile != null) {
                        unitProfile.UnitPrefabProps.UnitPrefab = thirdPartyCharacterPrefab;
                    } else {
                        Debug.Log("Could not load resource at " + unitProfilePath);
                    }
                }

                AssetDatabase.Refresh();

                // load the invector basic locomotion demo scene and make a prefab out of the camera
                
                EditorSceneManager.OpenScene("Assets/Invector-3rdPersonController/Basic Locomotion/DemoScenes/Invector_BasicLocomotion.unity");
                GameObject thirdPartyCameraGameObject = GameObject.Find("ThirdPersonCamera");
                if (thirdPartyCameraGameObject != null) {
                    string thirdpartyCameraPrefabPath = FileUtil.GetProjectRelativePath(prefabFolder) + "/" + thirdPartyCameraGameObject.name + ".prefab";
                    thirdPartyCameraPrefab = PrefabUtility.SaveAsPrefabAsset(thirdPartyCameraGameObject, thirdpartyCameraPrefabPath);
                }
                

            }

            // setup first scene paths
            string existingFirstSceneFolder = FileUtil.GetProjectRelativePath(newGameFolder + "/Scenes/" + defaultFirstSceneName);
            string newFirstSceneFolder = FileUtil.GetProjectRelativePath(newGameFolder + "/Scenes/" + fileSystemFirstSceneName);
            string newFirstSceneFileName = fileSystemFirstSceneName + ".unity";
            string newFirstScenePath = newFirstSceneFolder + "/" + newFirstSceneFileName;
            string existingFirstScenePath = existingFirstSceneFolder + "/" + defaultFirstSceneName + ".unity";

            // Rename the first scene if necessary
            if (fileSystemFirstSceneName != defaultFirstSceneName) {
                EditorUtility.DisplayProgressBar("New Game Wizard", "Renaming First Scene...", 0.7f);
                AssetDatabase.RenameAsset(existingFirstScenePath, newFirstSceneFileName);
                AssetDatabase.RenameAsset(existingFirstSceneFolder, fileSystemFirstSceneName);
                AssetDatabase.Refresh();
            }

            // add the first scene to the build if the option was chosen
            if (addFirstSceneToBuild) {
                EditorUtility.DisplayProgressBar("New Game Wizard", "Adding First Scene To Build Settings...", 0.75f);
                List<EditorBuildSettingsScene> currentSceneList = EditorBuildSettings.scenes.ToList();
                currentSceneList.Add(new EditorBuildSettingsScene(newFirstScenePath, true));
                EditorBuildSettings.scenes = currentSceneList.ToArray();
            }

            // Rename the game load scene
            EditorUtility.DisplayProgressBar("New Game Wizard", "Renaming Game Load Scene...", 0.8f);
            string gameLoadSceneFolder = FileUtil.GetProjectRelativePath(newGameFolder + "/Scenes/" + fileSystemGameName);
            string newGameLoadSceneFileName = fileSystemGameName + ".unity";
            string newGameLoadScenePath = gameLoadSceneFolder + "/" + newGameLoadSceneFileName;
            string existingGameLoadScenePath = gameLoadSceneFolder + "/" + templateName + ".unity";

            // Rename the game load scene
            AssetDatabase.RenameAsset(existingGameLoadScenePath, newGameLoadSceneFileName);
            AssetDatabase.Refresh();

            EditorUtility.DisplayProgressBar("New Game Wizard", "Modifying scene...", 0.85f);
            // Open the scene to add the necessary elements
            Debug.Log("Loading Scene at " + newGameLoadScenePath);
            Scene newScene = EditorSceneManager.OpenScene(newGameLoadScenePath);

            // Create a variant of the GameManager & UMA prefabs
            EditorUtility.DisplayProgressBar("New Game Wizard", "Making prefab variants...", 0.9f);
            MakeGameManagerPrefabVariant(gameManagerGameObject, prefabPath + "/GameManager.prefab");
            MakeUMAPrefabVariant(umaGameObject, prefabPath + "/UMA_GLIB.prefab", fileSystemGameName);

            EditorUtility.DisplayProgressBar("New Game Wizard", "Saving scene...", 0.95f);
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
            EditorUtility.DisplayProgressBar("New Game Wizard", "Configuration Game Settings...", 0.91f);
            SystemConfigurationManager systemConfigurationManager = instantiatedGO.GetComponent<SystemConfigurationManager>();
            systemConfigurationManager.GameName = gameName;
            systemConfigurationManager.GameVersion = gameVersion;
            systemConfigurationManager.InitializationScene = fileSystemGameName;
            systemConfigurationManager.MainMenuScene = "MainMenu";
            systemConfigurationManager.DefaultStartingZone = fileSystemFirstSceneName;

            if (useThirdPartyController == true) {
                systemConfigurationManager.UseThirdPartyMovementControl = true;
                systemConfigurationManager.AllowAutoAttack = false;
                systemConfigurationManager.UseThirdPartyCameraControl = true;
                systemConfigurationManager.DefaultPlayerUnitProfileName = "Invector UMA Player";
                if (thirdPartyCameraPrefab != null) {
                    systemConfigurationManager.ThirdPartyCamera = thirdPartyCameraPrefab;
                }
                //systemConfigurationManager.CharacterCreatorProfileNames = new List<string>() { "Invector UMA Player" };
            }

            EditorUtility.DisplayProgressBar("New Game Wizard", "Copying resources...", 0.92f);
            // Create a Resources folder and point the game manager to it
            if (systemConfigurationManager.LoadResourcesFolders == null) {
                systemConfigurationManager.LoadResourcesFolders = new List<string>();
            }

            string newResourcesFolderName = fileSystemGameName;
            systemConfigurationManager.LoadResourcesFolders.Add(newResourcesFolderName);

            // make variant on disk
            Debug.Log("saving " + newPath);
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
                Debug.Log("Create folder " + folderName);
                System.IO.Directory.CreateDirectory(folderName);
            }

            AssetDatabase.Refresh();
        }
    }

}
