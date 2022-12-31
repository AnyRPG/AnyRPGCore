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
    public class NewSceneWizard : ScriptableWizard {

        // Will be a subfolder of Application.dataPath and should start with "/"
        private string gameParentFolder = "/Games/";

        // user modified variables
        [Header("Game")]
        public string gameName = string.Empty;

        private const string sceneTemplatePath = "/AnyRPG/Core/Templates/Game/Scenes/FirstScene/FirstScene.unity";
        private const string lightingSettingsTemplatePath = "/AnyRPG/Core/Templates/Game/Scenes/FirstScene/FirstSceneSettings.lighting";
        private const string portalTemplatePath = "/AnyRPG/Core/Templates/Prefabs/Portal/StonePortal.prefab";
        private const string defaultSpawnLocationPath = "/AnyRPG/Core/Templates/Prefabs/SpawnPoints/DefaultSpawnLocation.prefab";
        private const string zoneCollidersPath = "/AnyRPG/Core/Templates/Prefabs/Colliders/ZoneColliders.prefab";

        // first scene options
        public bool copyExistingScene = false;
        public string sceneName = "New Scene";

        public SceneAsset existingScene = null;

        public AudioClip newSceneDayAmbientSounds = null;

        public AudioClip newSceneNightAmbientSounds = null;

        public AudioClip newSceneMusic = null;

        [MenuItem("Tools/AnyRPG/Wizard/New Scene Wizard")]
        public static void CreateWizard() {
            ScriptableWizard.DisplayWizard<NewSceneWizard>("New Scene Wizard", "Create");
        }

        void OnEnable() {
            SystemConfigurationManager systemConfigurationManager = WizardUtilities.GetSystemConfigurationManager();
            gameName = WizardUtilities.GetGameName(systemConfigurationManager);
            gameParentFolder = WizardUtilities.GetGameParentFolder(systemConfigurationManager, gameName);

            sceneName = GetNewSceneTitle(sceneName);
        }

        void OnWizardCreate() {

            EditorUtility.DisplayProgressBar("New Scene Wizard", "Checking parameters...", 0.1f);

            string fileSystemGameName = WizardUtilities.GetFileSystemGameName(gameName);

            // check that templates exist
            if (CheckRequiredTemplatesExist() == false) {
                return;
            }

            // Check for presence of sceneconfig prefab
            if (WizardUtilities.CheckFileExists(gameParentFolder + fileSystemGameName + "/Prefab/GameManager/" + fileSystemGameName + "SceneConfig.prefab", "SceneConfig prefab") == false) {
                return;
            }

            string newSceneAssetPath = CreateScene(gameParentFolder, gameName, sceneName, copyExistingScene, existingScene, newSceneDayAmbientSounds, newSceneNightAmbientSounds, newSceneMusic);

            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("New Scene Wizard", "New Scene Wizard Complete! The scene can be found at " + newSceneAssetPath, "OK");

        }

        public static bool CheckRequiredTemplatesExist() {

            // Check for presence of scene template
            if (WizardUtilities.CheckFileExists(sceneTemplatePath, "scene template") == false) {
                return false;
            }

            // Check for presence of lighting template
            if (WizardUtilities.CheckFileExists(lightingSettingsTemplatePath, "lighting settings template") == false) {
                return false;
            }

            // Check for presence of portal template
            if (WizardUtilities.CheckFileExists(portalTemplatePath, "portal template") == false) {
                return false;
            }

            // Check for presence of default spawn location prefab
            if (WizardUtilities.CheckFileExists(defaultSpawnLocationPath, "default spawn location prefab") == false) {
                return false;
            }

            // Check for presence of zone colliders prefab
            if (WizardUtilities.CheckFileExists(zoneCollidersPath, "zone colliders prefab") == false) {
                return false;
            }


            return true;
        }

        public static string GetNewSceneTitle(string sceneName) {

            // attempt to create a unique scene name if the default is already used
            string testSceneName = sceneName;
            string filesystemSceneName = WizardUtilities.GetFilesystemSceneName(testSceneName);

            if (SceneExists(filesystemSceneName)) {
                for (int i = 2; i < 100; i++) {
                    testSceneName = sceneName + " " + i.ToString();
                    filesystemSceneName = WizardUtilities.GetFilesystemSceneName(testSceneName);
                    if (SceneExists(filesystemSceneName) == false) {
                        sceneName = testSceneName;
                        break;
                    }
                }
            }

            return sceneName;
        }

        public static bool SceneExists(string filesystemSceneName) {

            EditorBuildSettingsScene[] editorBuildSettingsScenes = EditorBuildSettings.scenes;
            foreach (EditorBuildSettingsScene editorBuildSettingsScene in editorBuildSettingsScenes) {
                //Debug.Log(Path.GetFileName(editorBuildSettingsScene.path).Replace(".unity", ""));
                if (Path.GetFileName(editorBuildSettingsScene.path).Replace(".unity", "") == filesystemSceneName) {
                    return true;
                }
            }

            return false;
        }

        public static string CreateScene(string gameParentFolder,
            string gameName,
            string sceneName,
            bool copyExistingScene,
            SceneAsset existingScene,
            AudioClip newSceneDayAmbientSounds,
            AudioClip newSceneNightAmbientSounds,
            AudioClip newSceneMusic) {

            string fileSystemGameName = WizardUtilities.GetFileSystemGameName(gameName);
            string fileSystemSceneName = WizardUtilities.GetFilesystemSceneName(sceneName);

            // Determine root game folder
            string gameFileSystemFolder = WizardUtilities.GetGameFileSystemFolder(gameParentFolder, fileSystemGameName);

            // create resources folder if one doesn't already exist
            EditorUtility.DisplayProgressBar("New Scene Wizard", "Create Resource Folder If Necessary...", 0.5f);
            WizardUtilities.CreateFolderIfNotExists(gameFileSystemFolder + "/Resources/" + fileSystemGameName);

            // create prefab folder
            string prefabFolder = gameFileSystemFolder + "/Prefab";
            //string prefabPath = FileUtil.GetProjectRelativePath(prefabFolder);
            WizardUtilities.CreateFolderIfNotExists(prefabFolder + "/Portal");


            // setup first scene paths
            string newSceneFolder = FileUtil.GetProjectRelativePath(gameFileSystemFolder + "/Scenes/" + fileSystemSceneName);
            string newSceneFileName = fileSystemSceneName + ".unity";
            string newSceneAssetPath = newSceneFolder + "/" + newSceneFileName;
            string newLightingSettingsFileName = fileSystemSceneName + "LightingSettings.lighting";
            string newLightingSettingsAssetPath = newSceneFolder + "/" + newLightingSettingsFileName;

            // create the first scene folder
            WizardUtilities.CreateFolderIfNotExists(gameFileSystemFolder + "/Scenes/" + fileSystemSceneName);

            // if copying existing scene, use existing scene, otherwise use template first scene
            if (copyExistingScene == true) {
                //AssetDatabase.GetAssetPath(existingScene);
                AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(existingScene), newSceneAssetPath);
            } else {
                AssetDatabase.CopyAsset("Assets" + sceneTemplatePath, newSceneAssetPath);
            }
            AssetDatabase.CopyAsset("Assets" + lightingSettingsTemplatePath, newLightingSettingsAssetPath);
            AssetDatabase.Refresh();

            // add scene to build settings
            EditorUtility.DisplayProgressBar("New Scene Wizard", "Adding Scene To Build Settings...", 0.8f);
            List<EditorBuildSettingsScene> currentSceneList = EditorBuildSettings.scenes.ToList();
            Debug.Log("Adding " + newSceneAssetPath + " to build settings");
            currentSceneList.Add(new EditorBuildSettingsScene(newSceneAssetPath, true));
            EditorBuildSettings.scenes = currentSceneList.ToArray();

            // add zonecolliders to scene
            EditorUtility.DisplayProgressBar("New Scene Wizard", "Adding Zone Colliders to Scene...", 0.85f);
            string zoneCollidersPrefabAssetPath = "Assets" + zoneCollidersPath;
            GameObject zoneCollidersGameObject = (GameObject)AssetDatabase.LoadMainAssetAtPath(zoneCollidersPrefabAssetPath);
            Scene firstScene = EditorSceneManager.OpenScene(newSceneAssetPath);
            GameObject instantiatedCollidersGO = (GameObject)PrefabUtility.InstantiatePrefab(zoneCollidersGameObject);
            instantiatedCollidersGO.transform.SetAsFirstSibling();

            // add defaultSpawnLocation to scene
            EditorUtility.DisplayProgressBar("New Scene Wizard", "Adding Default Spawn Location to Scene...", 0.9f);
            string defaultSpawnLocationPrefabAssetPath = "Assets" + defaultSpawnLocationPath;
            GameObject defaultSpawnLocationGameObject = (GameObject)AssetDatabase.LoadMainAssetAtPath(defaultSpawnLocationPrefabAssetPath);
            GameObject instantiatedSpawnLocationGO = (GameObject)PrefabUtility.InstantiatePrefab(defaultSpawnLocationGameObject);
            instantiatedSpawnLocationGO.transform.SetAsFirstSibling();

            // add sceneconfig to scene
            EditorUtility.DisplayProgressBar("New Scene Wizard", "Adding SceneConfig to Scene...", 0.97f);
            string sceneConfigPrefabAssetPath = "Assets" + gameParentFolder + fileSystemGameName + "/Prefab/GameManager/" + fileSystemGameName + "SceneConfig.prefab";
            GameObject sceneConfigGameObject = (GameObject)AssetDatabase.LoadMainAssetAtPath(sceneConfigPrefabAssetPath);
            GameObject instantiatedGO = (GameObject)PrefabUtility.InstantiatePrefab(sceneConfigGameObject);
            instantiatedGO.transform.SetAsFirstSibling();


            // assign lighting settings
            LightingSettings lightingSettings = (LightingSettings)AssetDatabase.LoadMainAssetAtPath(newLightingSettingsAssetPath);
            Lightmapping.lightingSettings = lightingSettings;

            EditorSceneManager.SaveScene(firstScene);

            // creating portal
            EditorUtility.DisplayProgressBar("New Scene Wizard", "Creating Portal...", 0.98f);
            CreatePortal(gameParentFolder, gameName, sceneName, fileSystemGameName, fileSystemSceneName);

            // create scenenode and audio profiles
            EditorUtility.DisplayProgressBar("New Scene Wizard", "Configuring Scene...", 0.99f);
            ConfigureSceneScriptableObjects(gameParentFolder, sceneName, fileSystemGameName, fileSystemSceneName, newSceneDayAmbientSounds, newSceneNightAmbientSounds, newSceneMusic);

            AssetDatabase.Refresh();

            return newSceneAssetPath;
        }

        public static void CreatePortal(string gameParentFolder, string gameName, string sceneName, string fileSystemGameName, string fileSystemSceneName) {
            
            string destinationPartialPath = gameParentFolder + fileSystemGameName + "/Prefab/Portal/" + fileSystemSceneName + "StonePortal.prefab";
            string destinationAssetpath = "Assets" + destinationPartialPath;
            string destinationFilesystemPath = Application.dataPath + destinationPartialPath;

            WizardUtilities.CreateFolderIfNotExists(Application.dataPath + gameParentFolder + fileSystemGameName + "/Prefab/Portal");
            if (System.IO.File.Exists(destinationFilesystemPath) == false) {
                Debug.Log("Copying Resource from '" + portalTemplatePath + "' to '" + destinationAssetpath + "'");
                if (AssetDatabase.CopyAsset("Assets" + portalTemplatePath, destinationAssetpath)) {
                }
            } else {
                Debug.Log("Skipping copy. Prefab '" + destinationAssetpath + "' already exists");
            }

            GameObject portalObject = (GameObject)AssetDatabase.LoadMainAssetAtPath(destinationAssetpath);
            if (portalObject != null) {
                Interactable interactable = portalObject.GetComponentInChildren<Interactable>();
                if (interactable != null) {
                    interactable.DisplayName = "Travel to " + sceneName;
                }
                LoadSceneInteractable loadSceneInteractable = portalObject.GetComponentInChildren<LoadSceneInteractable>();
                if (loadSceneInteractable != null) {
                    loadSceneInteractable.LoadSceneProps.SceneName = sceneName;
                }

                EditorUtility.SetDirty(portalObject);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }


        }

        private static void ConfigureSceneScriptableObjects(string gameParentFolder,
            string sceneName,
            string fileSystemGameName,
            string fileSystemSceneName,
            AudioClip newSceneDayAmbientSounds,
            AudioClip newSceneNightAmbientSounds,
            AudioClip newSceneMusic) {

            // create ambient audio profile
            /*
            if (newSceneAmbientSounds != null) {
                AudioProfile audioProfile = ScriptableObject.CreateInstance("AudioProfile") as AudioProfile;
                audioProfile.ResourceName = sceneName + " Ambient";
                audioProfile.AudioClips = new List<AudioClip>() { newSceneAmbientSounds };

                string scriptableObjectPath = "Assets" + gameParentFolder + fileSystemGameName + "/Resources/" + fileSystemGameName + "/AudioProfile/" + fileSystemSceneName + "Ambient.asset";
                AssetDatabase.CreateAsset(audioProfile, scriptableObjectPath);
            }
            */

            // create background music profile
            if (newSceneMusic != null) {
                AudioProfile audioProfile = ScriptableObject.CreateInstance("AudioProfile") as AudioProfile;
                audioProfile.ResourceName = sceneName + " Music";
                audioProfile.AudioClips = new List<AudioClip>() { newSceneMusic };

                string scriptableObjectPath = "Assets" + gameParentFolder + fileSystemGameName + "/Resources/" + fileSystemGameName + "/AudioProfile/" + fileSystemSceneName + "Music.asset";
                AssetDatabase.CreateAsset(audioProfile, scriptableObjectPath);
            }

            // create scene node
            SceneNode sceneNode = ScriptableObject.CreateInstance("SceneNode") as SceneNode;
            sceneNode.ResourceName = sceneName;
            sceneNode.SuppressCharacterSpawn = false;
            sceneNode.SuppressMainCamera = false;
            sceneNode.SceneFile = fileSystemSceneName;
            if (newSceneDayAmbientSounds != null) {
                //sceneNode.AmbientMusicProfileName = sceneName + " Ambient";
                sceneNode.DayAmbientSound = newSceneDayAmbientSounds;
            }
            if (newSceneNightAmbientSounds != null) {
                //sceneNode.AmbientMusicProfileName = sceneName + " Ambient";
                sceneNode.NightAmbientSound = newSceneNightAmbientSounds;
            }
            if (newSceneMusic != null) {
                sceneNode.BackgroundMusicProfileName = sceneName + " Music";
            }

            string sceneNodeAssetPath = "Assets" + gameParentFolder + fileSystemGameName + "/Resources/" + fileSystemGameName + "/SceneNode/" + fileSystemSceneName + "SceneNode.asset";
            AssetDatabase.CreateAsset(sceneNode, sceneNodeAssetPath);

        }

        void OnWizardUpdate() {
            helpString = "Creates a new scene based on the AnyRPG template";
            string fileSystemGameName = WizardUtilities.GetFileSystemGameName(gameName);
            errorString = Validate(fileSystemGameName);
            isValid = (errorString == null || errorString == "");
        }

        string Validate(string fileSystemGameName) {

            if (fileSystemGameName == "") {
                return "Game name must not be empty";
            }

            // check for game folder existing
            string newGameFolder = WizardUtilities.GetGameFileSystemFolder(gameParentFolder, fileSystemGameName);
            if (System.IO.Directory.Exists(newGameFolder) == false) {
                return "The folder " + newGameFolder + "does not exist.  Please run the new game wizard first to create the game folder structure";
            }

            // check that scene name is not empty
            string filesystemSceneName = WizardUtilities.GetFilesystemSceneName(sceneName);
            if (filesystemSceneName == "") {
                return "Scene Name must not be empty";
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


        
        protected override bool DrawWizardGUI() {
            //return base.DrawWizardGUI();

            //NewGameWizard myScript = target as NewGameWizard;

            EditorGUILayout.LabelField("Game Options", EditorStyles.boldLabel);

            gameName = EditorGUILayout.TextField("Game Name", gameName);

            EditorGUILayout.LabelField("Scene Options", EditorStyles.boldLabel);

            sceneName = EditorGUILayout.TextField("Scene Name", sceneName);
            copyExistingScene = EditorGUILayout.Toggle("Copy Existing Scene", copyExistingScene);

            if (copyExistingScene) {
                existingScene = EditorGUILayout.ObjectField("Existing Scene", existingScene, typeof(SceneAsset), false) as SceneAsset;
            }

            newSceneDayAmbientSounds = EditorGUILayout.ObjectField("New Scene Day Ambient Sounds", newSceneDayAmbientSounds, typeof(AudioClip), false) as AudioClip;
            newSceneNightAmbientSounds = EditorGUILayout.ObjectField("New Scene Night Ambient Sounds", newSceneNightAmbientSounds, typeof(AudioClip), false) as AudioClip;
            newSceneMusic = EditorGUILayout.ObjectField("New Scene Music", newSceneMusic, typeof(AudioClip), false) as AudioClip;

            return true;
        }
        
    }


}
