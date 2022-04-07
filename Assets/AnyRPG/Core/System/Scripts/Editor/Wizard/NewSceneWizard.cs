using AnyRPG;
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

        // a reference to the systemConfigurationManager found in the currently open scene, for automatic determination of the game name
        // and setting the newly created unit profile as the default if necessary
        private SystemConfigurationManager systemConfigurationManager = null;

        // Will be a subfolder of Application.dataPath and should start with "/"
        private const string gameParentFolder = "/Games/";

        // user modified variables
        [Header("Game")]
        public string gameName = string.Empty;

        private const string firstSceneTemplateAssetpath = "Assets/AnyRPG/Core/Templates/Game/Scenes/FirstScene/FirstScene.unity";
        private const string portalAssetPath = "Assets/AnyRPG/Core/Templates/Prefabs/Portal/StonePortal.prefab";

        // first scene options
        public bool copyExistingScene = false;
        public string newSceneName = "New Scene";

        public SceneAsset existingScene = null;

        public AudioClip newSceneAmbientSounds = null;

        public AudioClip newSceneMusic = null;

        [MenuItem("Tools/AnyRPG/Wizard/New Scene Wizard")]
        public static void CreateWizard() {
            ScriptableWizard.DisplayWizard<NewSceneWizard>("New Scene Wizard", "Create");
        }

        void OnEnable() {
            systemConfigurationManager = GameObject.FindObjectOfType<SystemConfigurationManager>();
            if (systemConfigurationManager == null) {
                SceneConfig sceneConfig = GameObject.FindObjectOfType<SceneConfig>();
                if (sceneConfig != null) {
                    systemConfigurationManager = sceneConfig.systemConfigurationManager;
                }
            }
            if (systemConfigurationManager != null) {
                gameName = systemConfigurationManager.GameName;
            }
        }

        void OnWizardCreate() {

            EditorUtility.DisplayProgressBar("New Scene Wizard", "Checking parameters...", 0.1f);

            string fileSystemGameName = WizardUtilities.GetFileSystemGameName(gameName);
            string fileSystemSceneName = WizardUtilities.GetFilesystemSceneName(newSceneName);

            // Check for presence of sceneconfig prefab
            string pathToSceneConfigPrefab = "Assets" + gameParentFolder + fileSystemGameName + "/Prefab/GameManager/" + fileSystemGameName + "SceneConfig.prefab";
            GameObject sceneConfigGameObject = (GameObject)AssetDatabase.LoadMainAssetAtPath(pathToSceneConfigPrefab);
            if (sceneConfigGameObject == null) {
                ShowError("Missing SceneConfig prefab at " + pathToSceneConfigPrefab + ".  Aborting...");
                return;
            }

            // Determine root game folder
            string gameFileSystemFolder = WizardUtilities.GetGameFolder(gameParentFolder, fileSystemGameName);
            string resourcesFileSystemFolder = gameFileSystemFolder + "/Resources/" + fileSystemGameName;

            // create resources folder if one doesn't already exist
            EditorUtility.DisplayProgressBar("New Game Wizard", "Create Resource Folder If Necessary...", 0.5f);
            WizardUtilities.CreateFolderIfNotExists(resourcesFileSystemFolder);

            // create prefab folder
            string prefabFolder = gameFileSystemFolder + "/Prefab";
            //string prefabPath = FileUtil.GetProjectRelativePath(prefabFolder);
            WizardUtilities.CreateFolderIfNotExists(prefabFolder);
            WizardUtilities.CreateFolderIfNotExists(prefabFolder + "/Portal");


            // setup first scene paths
            string newSceneFolder = FileUtil.GetProjectRelativePath(gameFileSystemFolder + "/Scenes/" + fileSystemSceneName);
            string newSceneFileName = fileSystemSceneName + ".unity";
            string newSceneAssetPath = newSceneFolder + "/" + newSceneFileName;

            // create the first scene folder
            WizardUtilities.CreateFolderIfNotExists(gameFileSystemFolder + "/Scenes/" + fileSystemSceneName);

            // if copying existing scene, use existing scene, otherwise use template first scene
            if (copyExistingScene == true) {
                //AssetDatabase.GetAssetPath(existingScene);
                AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(existingScene), newSceneAssetPath); 
            } else {
                AssetDatabase.CopyAsset(firstSceneTemplateAssetpath, newSceneAssetPath);
            }
            AssetDatabase.Refresh();

            // add scene to build settings
            EditorUtility.DisplayProgressBar("New Game Wizard", "Adding Scene To Build Settings...", 0.8f);
            List<EditorBuildSettingsScene> currentSceneList = EditorBuildSettings.scenes.ToList();
            Debug.Log("Adding " + newSceneAssetPath + " to build settings");
            currentSceneList.Add(new EditorBuildSettingsScene(newSceneAssetPath, true));
            EditorBuildSettings.scenes = currentSceneList.ToArray();

            // add sceneconfig to scene
            EditorUtility.DisplayProgressBar("New Scene Wizard", "Adding SceneConfig to Scene...", 0.97f);
            Scene firstScene = EditorSceneManager.OpenScene(newSceneAssetPath);
            GameObject instantiatedGO = (GameObject)PrefabUtility.InstantiatePrefab(sceneConfigGameObject);
            instantiatedGO.transform.SetAsFirstSibling();
            EditorSceneManager.SaveScene(firstScene);

            // creating portal
            EditorUtility.DisplayProgressBar("New Scene Wizard", "Creating Portal...", 0.98f);
            CreatePortal(gameName, newSceneName, fileSystemGameName, fileSystemSceneName);

            // create scenenode and audio profiles
            EditorUtility.DisplayProgressBar("New Scene Wizard", "Configuring Scene...", 0.99f);
            ConfigureSceneScriptableObjects(fileSystemGameName, fileSystemSceneName);

            AssetDatabase.Refresh();

            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("New Scene Wizard", "New Scene Wizard Complete! The scene can be found at " + newSceneAssetPath, "OK");

        }

        public static void CreatePortal(string gameName, string sceneName, string fileSystemGameName, string fileSystemSceneName) {
            string destinationPartialPath = gameParentFolder + fileSystemGameName + "/Prefab/Portal/" + fileSystemSceneName + "StonePortal.prefab";
            string destinationAssetpath = "Assets" + destinationPartialPath;
            string destinationFilesystemPath = Application.dataPath + destinationPartialPath;

            WizardUtilities.CreateFolderIfNotExists(Application.dataPath + gameParentFolder + fileSystemGameName + "/Prefab/Portal");
            if (System.IO.File.Exists(destinationFilesystemPath) == false) {
                Debug.Log("Copying Resource from '" + portalAssetPath + "' to '" + destinationAssetpath + "'");
                if (AssetDatabase.CopyAsset(portalAssetPath, destinationAssetpath)) {
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

        private void ConfigureSceneScriptableObjects(string fileSystemGameName, string fileSystemSceneName) {

            // create ambient audio profile
            if (newSceneAmbientSounds != null) {
                AudioProfile audioProfile = ScriptableObject.CreateInstance("AudioProfile") as AudioProfile;
                audioProfile.ResourceName = newSceneName + " Ambient";
                audioProfile.AudioClips = new List<AudioClip>() { newSceneAmbientSounds };

                string scriptableObjectPath = "Assets" + gameParentFolder + fileSystemGameName + "/Resources/" + fileSystemGameName + "/AudioProfile/" + fileSystemSceneName + "Ambient.asset";
                AssetDatabase.CreateAsset(audioProfile, scriptableObjectPath);
            }

            // create background music profile
            if (newSceneMusic != null) {
                AudioProfile audioProfile = ScriptableObject.CreateInstance("AudioProfile") as AudioProfile;
                audioProfile.ResourceName = newSceneName + " Music";
                audioProfile.AudioClips = new List<AudioClip>() { newSceneMusic };

                string scriptableObjectPath = "Assets" + gameParentFolder + fileSystemGameName + "/Resources/" + fileSystemGameName + "/AudioProfile/" + fileSystemSceneName + "Music.asset";
                AssetDatabase.CreateAsset(audioProfile, scriptableObjectPath);
            }

            // create scene node
            SceneNode sceneNode = ScriptableObject.CreateInstance("SceneNode") as SceneNode;
            sceneNode.ResourceName = newSceneName;
            sceneNode.SuppressCharacterSpawn = false;
            sceneNode.SuppressMainCamera = false;
            sceneNode.SceneFile = fileSystemSceneName;
            if (newSceneAmbientSounds != null) {
                sceneNode.AmbientMusicProfileName = newSceneName + " Ambient";
            }
            if (newSceneMusic != null) {
                sceneNode.BackgroundMusicProfileName = newSceneName + " Music";
            }

            string sceneNodeObjectPath = "Assets" + gameParentFolder + fileSystemGameName + "/Resources/" + fileSystemGameName + "/SceneNode/" + fileSystemSceneName + "SceneNode.asset";
            AssetDatabase.CreateAsset(sceneNode, sceneNodeObjectPath);

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
            string newGameFolder = WizardUtilities.GetGameFolder(gameParentFolder, fileSystemGameName);
            if (System.IO.Directory.Exists(newGameFolder) == false) {
                return "The folder " + newGameFolder + "does not exist.  Please run the new game wizard first to create the game folder structure";
            }

            // check that scene name is not empty
            string filesystemSceneName = WizardUtilities.GetFilesystemSceneName(newSceneName);
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

        private void ShowError(string message) {
            EditorUtility.DisplayDialog("Error", message, "OK");
        }

        
        protected override bool DrawWizardGUI() {
            //return base.DrawWizardGUI();

            //NewGameWizard myScript = target as NewGameWizard;

            EditorGUILayout.LabelField("Game Options", EditorStyles.boldLabel);

            gameName = EditorGUILayout.TextField("Game Name", gameName);

            EditorGUILayout.LabelField("Scene Options", EditorStyles.boldLabel);

            newSceneName = EditorGUILayout.TextField("Scene Name", newSceneName);
            copyExistingScene = EditorGUILayout.Toggle("Copy Existing Scene", copyExistingScene);

            if (copyExistingScene) {
                existingScene = EditorGUILayout.ObjectField("Existing Scene", existingScene, typeof(SceneAsset), false) as SceneAsset;
            }

            newSceneAmbientSounds = EditorGUILayout.ObjectField("First Scene Ambient Sounds", newSceneAmbientSounds, typeof(AudioClip), false) as AudioClip;
            newSceneMusic = EditorGUILayout.ObjectField("First Scene Music", newSceneMusic, typeof(AudioClip), false) as AudioClip;

            return true;
        }
        
    }


}
