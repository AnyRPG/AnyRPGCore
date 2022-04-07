﻿using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace AnyRPG {
    public class NewGameWizard : ScriptableWizard {

        // Template path/scene that will be used to create the new game
        private const string templateName = "TemplateGame";
        private const string pathToNewGameTemplate = "/AnyRPG/Core/Templates/Game/" + templateName;
        private const string firstSceneTemplateAssetpath = "Assets/AnyRPG/Core/Templates/Game/Scenes/FirstScene/FirstScene.unity";

        // for now this is necessary due to git not saving empty folders
        private const string pathToResourcesTemplateFolder = "/AnyRPG/Core/Games/FeaturesDemo/Resources/FeaturesDemoGame";

        private const string pathToGameManagerPrefab = "Assets/AnyRPG/Core/System/Prefabs/GameManager/GameManager.prefab";
        private const string pathToSceneConfigPrefab = "Assets/AnyRPG/Core/System/Prefabs/GameManager/SceneConfig.prefab";
        private const string pathToUMADCSPrefab = "Getting Started/UMA_GLIB.prefab";

        private const string pathToSystemAbilitiesTemplate = "Assets/AnyRPG/Core/Content/TemplatePackages/DefaultSystemEffectsTemplatePackage.asset";
        private const string pathToGoldCurrencyGroupTemplate = "Assets/AnyRPG/Core/Content/TemplatePackages/GoldCurrencyGroupTemplatePackage.asset";


        // Will be a subfolder of Application.dataPath and should start with "/"
        private const string newGameParentFolder = "/Games/";

        // compare the default first scene directory to any user picked scene name
        //private const string templateFirstSceneName = "FirstScene";

        // the used file path name for the game
        private string fileSystemGameName = string.Empty;
        private string fileSystemFirstSceneName = string.Empty;

        // user modified variables
        public string gameName = "";
        public string gameVersion = "0.1a";

        // main menu options
        public AudioClip mainMenuMusic = null;

        public AudioClip newGameMusic = null;

        // first scene options
        public bool copyExistingScene = false;
        public string firstSceneName = "FirstScene";

        public SceneAsset existingScene = null;

        public AudioClip firstSceneAmbientSounds = null;

        public AudioClip firstSceneMusic = null;

        // game options
        public bool installGoldCurrencyGroup = true;

        //private bool addFirstSceneToBuild = true;
        private string umaRoot = "Assets/UMA/";

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
            //existingScene = AssetDatabase.LoadAssetAtPath
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

            GameObject sceneConfigGameObject = (GameObject)AssetDatabase.LoadMainAssetAtPath(pathToSceneConfigPrefab);
            if (sceneConfigGameObject == null) {
                ShowError("Missing SceneConfig prefab at " + pathToSceneConfigPrefab + ".  Aborting...");
                return;
            }

            ScriptableContentTemplate defaultEffectsTemplate = (ScriptableContentTemplate)AssetDatabase.LoadMainAssetAtPath(pathToSystemAbilitiesTemplate);
            if (defaultEffectsTemplate == null) {
                ShowError("Missing Default System Effects Content Template at " + pathToSystemAbilitiesTemplate + ".  Aborting...");
                return;
            }

            ScriptableContentTemplate goldCurrencyGroupTemplate = (ScriptableContentTemplate)AssetDatabase.LoadMainAssetAtPath(pathToGoldCurrencyGroupTemplate);
            if (goldCurrencyGroupTemplate == null) {
                ShowError("Missing Gold Currency Group Content Template at " + pathToGoldCurrencyGroupTemplate + ".  Aborting...");
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

            /*
            if (firstSceneName == null || firstSceneName.Trim() == "") {
                firstSceneName = fileSystemGameName + "Scene";
                fileSystemFirstSceneName = firstSceneName;
                Debug.Log("Empty first scene name.  Defaulting to \"" + fileSystemFirstSceneName + "\"");
            }
            */

            EditorUtility.DisplayProgressBar("New Game Wizard", "Creating Game Folder...", 0.2f);
            // Create root game folder
            string newGameFolder = GetNewGameFolder();
            string resourcesFolder = newGameFolder + "/Resources/" + fileSystemGameName;

            // create base games folder
            WizardUtilities.CreateFolderIfNotExists(Application.dataPath + newGameParentFolder);

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
            WizardUtilities.CreateFolderIfNotExists(resourcesFolder);

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
            WizardUtilities.CreateFolderIfNotExists(prefabFolder);
            WizardUtilities.CreateFolderIfNotExists(prefabFolder + "/GameManager");


            if (useThirdPartyController == true) {
                ConfigureThirdPartyController(resourcesFolder, prefabFolder);
            }

            // setup first scene paths
            //string existingFirstSceneFolder = FileUtil.GetProjectRelativePath(newGameFolder + "/Scenes/" + defaultFirstSceneName);
            string newFirstSceneFolder = FileUtil.GetProjectRelativePath(newGameFolder + "/Scenes/" + fileSystemFirstSceneName);
            string newFirstSceneFileName = fileSystemFirstSceneName + ".unity";
            string newFirstSceneAssetPath = newFirstSceneFolder + "/" + newFirstSceneFileName;
            //string existingFirstScenePath = existingFirstSceneFolder + "/" + defaultFirstSceneName + ".unity";

            // create the first scene folder
            WizardUtilities.CreateFolderIfNotExists(newGameFolder + "/Scenes/" + fileSystemFirstSceneName);

            // if copying existing scene, use existing scene, otherwise use template first scene
            if (copyExistingScene == true) {
                //AssetDatabase.GetAssetPath(existingScene);
                AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(existingScene), newFirstSceneAssetPath); 
            } else {
                AssetDatabase.CopyAsset(firstSceneTemplateAssetpath, newFirstSceneAssetPath);
            }
            AssetDatabase.Refresh();


            // Rename the game load scene
            EditorUtility.DisplayProgressBar("New Game Wizard", "Renaming Game Load Scene...", 0.75f);
            string gameLoadSceneFolder = FileUtil.GetProjectRelativePath(newGameFolder + "/Scenes/" + fileSystemGameName);
            string newGameLoadSceneFileName = fileSystemGameName + ".unity";
            string newGameLoadScenePath = gameLoadSceneFolder + "/" + newGameLoadSceneFileName;
            string existingGameLoadScenePath = gameLoadSceneFolder + "/" + templateName + ".unity";

            // Rename the game load scene
            AssetDatabase.RenameAsset(existingGameLoadScenePath, newGameLoadSceneFileName);
            AssetDatabase.Refresh();

            EditorUtility.DisplayProgressBar("New Game Wizard", "Adding Scenes To Build Settings...", 0.8f);
            List<EditorBuildSettingsScene> currentSceneList = EditorBuildSettings.scenes.ToList();
            Debug.Log("Adding " + newFirstSceneAssetPath + " to build settings");
            currentSceneList.Add(new EditorBuildSettingsScene(newFirstSceneAssetPath, true));
            Debug.Log("Adding " + newGameLoadScenePath + " to build settings");
            currentSceneList.Add(new EditorBuildSettingsScene(newGameLoadScenePath, true));
            EditorBuildSettings.scenes = currentSceneList.ToArray();


            // Open the scene to add the necessary elements
            EditorUtility.DisplayProgressBar("New Game Wizard", "Modifying loading scene...", 0.85f);
            Debug.Log("Loading Scene at " + newGameLoadScenePath);
            Scene newScene = EditorSceneManager.OpenScene(newGameLoadScenePath);

            // install gold currency group
            if (installGoldCurrencyGroup) {
                EditorUtility.DisplayProgressBar("New Game Wizard", "Installing Gold Currency Group...", 0.89f);
                TemplateContentWizard.RunWizard(fileSystemGameName, newGameParentFolder, new List<ScriptableContentTemplate>() { goldCurrencyGroupTemplate }, true, true);
            }

            // Create a variant of the GameManager & UMA prefabs
            EditorUtility.DisplayProgressBar("New Game Wizard", "Making prefab variants...", 0.9f);
            GameObject gameManagerVariant = MakeGameManagerPrefabVariant(gameManagerGameObject, prefabPath + "/GameManager/" + fileSystemGameName + "GameManager.prefab");
            MakeUMAPrefabVariant(umaGameObject, prefabPath + "/GameManager/UMA_GLIB.prefab", fileSystemGameName);
            GameObject sceneConfigVariant = MakeSceneConfigPrefabVariant(gameManagerVariant, sceneConfigGameObject, prefabPath + "/GameManager/" + fileSystemGameName + "SceneConfig.prefab");

            // Save changes to the load game scene
            EditorUtility.DisplayProgressBar("New Game Wizard", "Saving Load Game Scene...", 0.95f);
            EditorSceneManager.SaveScene(newScene);

            // install system default effects
            EditorUtility.DisplayProgressBar("New Game Wizard", "Installing System Default Effects...", 0.96f);
            TemplateContentWizard.RunWizard(fileSystemGameName, newGameParentFolder, new List<ScriptableContentTemplate>() { defaultEffectsTemplate }, true, true);

            // add sceneconfig to first scene
            EditorUtility.DisplayProgressBar("New Game Wizard", "Adding SceneConfig to First Scene...", 0.97f);
            Scene firstScene = EditorSceneManager.OpenScene(newFirstSceneAssetPath);
            GameObject instantiatedGO = (GameObject)PrefabUtility.InstantiatePrefab(sceneConfigVariant);
            instantiatedGO.transform.SetAsFirstSibling();
            EditorSceneManager.SaveScene(firstScene);

            // load loading scene
            //EditorSceneManager.OpenScene(newGameLoadScenePath);

            EditorUtility.DisplayProgressBar("New Game Wizard", "Creating Portal For First Scene...", 0.975f);
            NewSceneWizard.CreatePortal(gameName, firstSceneName, fileSystemGameName, fileSystemFirstSceneName);

            // make audio profiles and scene nodes
            EditorUtility.DisplayProgressBar("New Game Wizard", "Configuring Main Menu...", 0.98f);
            ConfigureMainMenuScriptableObjects();

            EditorUtility.DisplayProgressBar("New Game Wizard", "Configuring First Scene...", 0.99f);
            ConfigureFirstSceneScriptableObjects();

            AssetDatabase.Refresh();

            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("New Game Wizard", "New Game Wizard Complete! The game loading scene can be found at " + newGameLoadScenePath, "OK");

        }

        private void ConfigureMainMenuScriptableObjects() {

            // create audio profile
            if (mainMenuMusic != null) {
                AudioProfile audioProfile = ScriptableObject.CreateInstance("AudioProfile") as AudioProfile;
                audioProfile.ResourceName = "Main Menu";
                audioProfile.AudioClips = new List<AudioClip>() { mainMenuMusic };

                string scriptableObjectPath = "Assets" + newGameParentFolder + fileSystemGameName + "/Resources/" + fileSystemGameName + "/AudioProfile/MainMenuAudio.asset";
                AssetDatabase.CreateAsset(audioProfile, scriptableObjectPath);
            }

            if (newGameMusic != null) {
                AudioProfile audioProfile = ScriptableObject.CreateInstance("AudioProfile") as AudioProfile;
                audioProfile.ResourceName = "New Game";
                audioProfile.AudioClips = new List<AudioClip>() { newGameMusic };

                string scriptableObjectPath = "Assets" + newGameParentFolder + fileSystemGameName + "/Resources/" + fileSystemGameName + "/AudioProfile/NewGameAudio.asset";
                AssetDatabase.CreateAsset(audioProfile, scriptableObjectPath);
            }

            // create scene node
            SceneNode sceneNode = ScriptableObject.CreateInstance("SceneNode") as SceneNode;
            sceneNode.ResourceName = "Main Menu";
            sceneNode.SceneFile = "MainMenu";
            sceneNode.AllowMount = false;
            sceneNode.SuppressCharacterSpawn = true;
            if (mainMenuMusic != null) {
                sceneNode.BackgroundMusicProfileName = "Main Menu";
            }

            string sceneNodeObjectPath = "Assets" + newGameParentFolder + fileSystemGameName + "/Resources/" + fileSystemGameName + "/SceneNode/MainMenuSceneNode.asset";
            AssetDatabase.CreateAsset(sceneNode, sceneNodeObjectPath);

        }

        private void ConfigureFirstSceneScriptableObjects() {

            // create ambient audio profile
            if (firstSceneAmbientSounds != null) {
                AudioProfile audioProfile = ScriptableObject.CreateInstance("AudioProfile") as AudioProfile;
                audioProfile.ResourceName = firstSceneName + " Ambient";
                audioProfile.AudioClips = new List<AudioClip>() { firstSceneAmbientSounds };

                string scriptableObjectPath = "Assets" + newGameParentFolder + fileSystemGameName + "/Resources/" + fileSystemGameName + "/AudioProfile/" + fileSystemFirstSceneName + "Ambient.asset";
                AssetDatabase.CreateAsset(audioProfile, scriptableObjectPath);
            }

            // create background music profile
            if (firstSceneMusic != null) {
                AudioProfile audioProfile = ScriptableObject.CreateInstance("AudioProfile") as AudioProfile;
                audioProfile.ResourceName = firstSceneName + " Music";
                audioProfile.AudioClips = new List<AudioClip>() { firstSceneMusic };

                string scriptableObjectPath = "Assets" + newGameParentFolder + fileSystemGameName + "/Resources/" + fileSystemGameName + "/AudioProfile/" + fileSystemFirstSceneName + "Music.asset";
                AssetDatabase.CreateAsset(audioProfile, scriptableObjectPath);
            }

            // create scene node
            SceneNode sceneNode = ScriptableObject.CreateInstance("SceneNode") as SceneNode;
            sceneNode.ResourceName = firstSceneName;
            sceneNode.SuppressCharacterSpawn = false;
            sceneNode.SuppressMainCamera = false;
            sceneNode.SceneFile = fileSystemFirstSceneName;
            if (firstSceneAmbientSounds != null) {
                sceneNode.AmbientMusicProfileName = firstSceneName + " Ambient";
            }
            if (firstSceneMusic != null) {
                sceneNode.BackgroundMusicProfileName = firstSceneName + " Music";
            }

            string sceneNodeObjectPath = "Assets" + newGameParentFolder + fileSystemGameName + "/Resources/" + fileSystemGameName + "/SceneNode/" + fileSystemFirstSceneName + "SceneNode.asset";
            AssetDatabase.CreateAsset(sceneNode, sceneNodeObjectPath);

        }

        private void ConfigureThirdPartyController(string resourcesFolder, string prefabFolder) {
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
            MakeFileSystemGameName();

            if (fileSystemGameName == "") {
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

            // check that game with same name doesn't already exist
            string newGameFolder = GetNewGameFolder();
            if (System.IO.Directory.Exists(newGameFolder)) {
                return "Folder " + newGameFolder + " already exists.  Please delete this directory or choose a new game name";
            }

            // check that first scene name is not empty
            string filesystemSceneName = GetFilesystemSceneName(firstSceneName);
            if (filesystemSceneName == "") {
                return "First Scene Name must not be empty";
            }

            // check that first scene name is different than game name
            if (filesystemSceneName == fileSystemGameName) {
                return "First Scene Name must be different than Game Name";
            }

            // check that scene with same name doesn't already exist in build settings
            EditorBuildSettingsScene[] editorBuildSettingsScenes = EditorBuildSettings.scenes;
            foreach (EditorBuildSettingsScene editorBuildSettingsScene in editorBuildSettingsScenes) {
                //Debug.Log(Path.GetFileName(editorBuildSettingsScene.path).Replace(".unity", ""));
                if (Path.GetFileName(editorBuildSettingsScene.path).Replace(".unity", "") == filesystemSceneName) {
                    return "A scene with the name " + filesystemSceneName + " already exists in the build settings. Please choose a unique first scene name.";
                }
            }

            return null;
        }

        private string GetFilesystemSceneName(string sceneName) {
            if (sceneName == null) {
                return string.Empty;
            }
            return sceneName.Replace(" ", "");
        }

        private void ShowError(string message) {
            EditorUtility.DisplayDialog("Error", message, "OK");
        }

        private GameObject MakeGameManagerPrefabVariant(GameObject goToMakeVariantOf, string newPath) {
            Debug.Log("NewGameWizard.MakeGameManagerPrefabVariant(" + goToMakeVariantOf.name + ", " + newPath + ")");

            // make prefab variant of game manager
            //GameObject instantiatedGO = (GameObject)PrefabUtility.InstantiatePrefab(goToMakeVariantOf);

            // instantiate original
            GameObject instantiatedGO = (GameObject)PrefabUtility.InstantiatePrefab(goToMakeVariantOf);
            instantiatedGO.name = fileSystemGameName + instantiatedGO.name;

            // Set the properties of the GameManager for this game
            EditorUtility.DisplayProgressBar("New Game Wizard", "Configuration Game Settings...", 0.91f);
            SystemConfigurationManager systemConfigurationManager = instantiatedGO.GetComponent<SystemConfigurationManager>();
            systemConfigurationManager.GameName = gameName;
            systemConfigurationManager.GameVersion = gameVersion;

            // scene configuration
            systemConfigurationManager.InitializationScene = fileSystemGameName;
            systemConfigurationManager.MainMenuScene = "Main Menu";
            systemConfigurationManager.DefaultStartingZone = firstSceneName;

            // default system effects
            systemConfigurationManager.LevelUpEffectName = "Level Up";
            systemConfigurationManager.DeathEffectName = "Death";
            systemConfigurationManager.LootSparkleEffectName = "Loot Sparkle";

            // gold currency group
            if (installGoldCurrencyGroup) {
                systemConfigurationManager.CurrencyGroupName = "Gold";
                systemConfigurationManager.KillCurrencyName = "Silver";
                systemConfigurationManager.QuestCurrencyName = "Gold";
            }

            // name game music
            if (newGameMusic != null) {
                systemConfigurationManager.NewGameAudio = "New Game";
            }

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
            //sceneVariant.name = goToMakeVariantOf.name;

            return variant;
        }

        private GameObject MakeSceneConfigPrefabVariant(GameObject gameManagerVariant, GameObject goToMakeVariantOf, string newPath) {
            
            // instantiate original
            GameObject instantiatedGO = (GameObject)PrefabUtility.InstantiatePrefab(goToMakeVariantOf);
            instantiatedGO.name = fileSystemGameName + instantiatedGO.name;

            // Set the properties of the SceneConfig for this game
            EditorUtility.DisplayProgressBar("New Game Wizard", "Configuration SceneConfig Settings...", 0.91f);
            SceneConfig sceneConfig = instantiatedGO.GetComponent<SceneConfig>();
            sceneConfig.systemConfigurationManager = gameManagerVariant.GetComponent<SystemConfigurationManager>();
            sceneConfig.loadGameOnPlay = true;

            // save as prefab variant
            Debug.Log("saving " + newPath);
            GameObject variant = PrefabUtility.SaveAsPrefabAsset(instantiatedGO, newPath);

            // remove original from scene
            GameObject.DestroyImmediate(instantiatedGO);

            return variant;
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

        protected override bool DrawWizardGUI() {
            //return base.DrawWizardGUI();

            //NewGameWizard myScript = target as NewGameWizard;

            EditorGUILayout.LabelField("Game Options", EditorStyles.boldLabel);

            gameName = EditorGUILayout.TextField("Game Name", gameName);
            gameVersion = EditorGUILayout.TextField("Game Version", gameVersion);

            EditorGUILayout.LabelField("Main Menu Options", EditorStyles.boldLabel);

            mainMenuMusic = EditorGUILayout.ObjectField("Main Menu Music", mainMenuMusic, typeof(AudioClip), false) as AudioClip;

            newGameMusic = EditorGUILayout.ObjectField("New Game Music", newGameMusic, typeof(AudioClip), false) as AudioClip;

            EditorGUILayout.LabelField("First Scene Options", EditorStyles.boldLabel);

            firstSceneName = EditorGUILayout.TextField("First Scene Name", firstSceneName);
            copyExistingScene = EditorGUILayout.Toggle("Copy Existing Scene", copyExistingScene);

            if (copyExistingScene) {
                existingScene = EditorGUILayout.ObjectField("Existing Scene", existingScene, typeof(SceneAsset), false) as SceneAsset;
            }

            firstSceneAmbientSounds = EditorGUILayout.ObjectField("First Scene Ambient Sounds", firstSceneAmbientSounds, typeof(AudioClip), false) as AudioClip;
            firstSceneMusic = EditorGUILayout.ObjectField("First Scene Music", firstSceneMusic, typeof(AudioClip), false) as AudioClip;

            EditorGUILayout.LabelField("Common Options", EditorStyles.boldLabel);

            installGoldCurrencyGroup = EditorGUILayout.Toggle("Install Gold Currency Group", installGoldCurrencyGroup);

            return true;
        }
        
    }

    /*
    [CustomEditor(typeof(NewGameWizard))]
    public class NewGameWizardEditor : Editor {

        public override void OnInspectorGUI() {
            //base.OnInspectorGUI();

            NewGameWizard myScript = target as NewGameWizard;

            //DrawDefaultInspector();

            myScript.createFirstScene = EditorGUILayout.Toggle("Create First Scene", myScript.createFirstScene);

            if (myScript.createFirstScene) {
                myScript.firstSceneName = EditorGUILayout.TextField("First Scene Name", myScript.firstSceneName);
            }// else {
               // myScript.firstSceneName = EditorGUILayout.field("First Scene Name", myScript.firstSceneName);
            //}
        }
    }
*/

}
