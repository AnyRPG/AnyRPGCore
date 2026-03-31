using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace AnyRPG {
    public class UMASupportWizard : ScriptableWizard {

        private const string pathToUMAGLIBPrefab = "/UMA/Getting Started/UMA_GLIB.prefab";
        private const string pathToPlayerUnitsTemplate = "/AnyRPG/UMA/Content/TemplatePackages/UnitProfile/Player/UMAHumanPlayerUnitsTemplatePackage.asset";

        // Will be a subfolder of Application.dataPath and should start with "/"
        private string gameParentFolder = "/Games/";

        // user modified variables
        [Header("Game")]
        private string gameName = string.Empty;

        [MenuItem("Tools/AnyRPG/Wizard/UMA/Add UMA Support")]
        public static void CreateWizard() {
            ScriptableWizard.DisplayWizard<UMASupportWizard>("Add UMA Support Wizard", "Add Support");
        }

        private void OnEnable() {
            SystemConfigurationManager systemConfigurationManager = WizardUtilities.GetSystemConfigurationManager();
            gameName = WizardUtilities.GetGameName(systemConfigurationManager);
            gameParentFolder = WizardUtilities.GetGameParentFolder(systemConfigurationManager, gameName);
        }

        private void OnWizardCreate() {

            EditorUtility.DisplayProgressBar("UMA Support Wizard", "Checking parameters...", 0.1f);

            string fileSystemGameName = WizardUtilities.GetFileSystemGameName(gameName);

            // check that templates exist
            if (CheckFilesExist() == false) {
                EditorUtility.ClearProgressBar();
                return;
            }

            // Check for presence of GameManager prefab
            string gameManagerPrefabPath = "Assets" + gameParentFolder + fileSystemGameName + "/Prefab/GameManager/" + fileSystemGameName + "GameManager.prefab";
            if (WizardUtilities.CheckFileExists(gameParentFolder + fileSystemGameName + "/Prefab/GameManager/" + fileSystemGameName + "GameManager.prefab", "GameManager prefab") == false) {
                EditorUtility.ClearProgressBar();
                return;
            }

            // Determine game load scene path
            string loadSceneFolder = gameParentFolder + fileSystemGameName + "/Scenes/" + fileSystemGameName;
            string loadSceneAssetPath = "Assets" + loadSceneFolder + "/" + fileSystemGameName + ".unity";

            // Check for presence of game load scene
            if (WizardUtilities.CheckFileExists(loadSceneFolder + "/" + fileSystemGameName + ".unity", "game load scene") == false) {
                EditorUtility.ClearProgressBar();
                return;
            }

            // Open the game load scene to add the UMA GLIB prefab
            EditorUtility.DisplayProgressBar("UMA Support Wizard", "Opening game load scene...", 0.3f);
            Scene loadGameScene = EditorSceneManager.OpenScene(loadSceneAssetPath);

            // create prefab folder
            string fileSystemPrefabFolder = Application.dataPath + gameParentFolder + fileSystemGameName + "/Prefab";
            string prefabPath = FileUtil.GetProjectRelativePath(fileSystemPrefabFolder);
            WizardUtilities.CreateFolderIfNotExists(fileSystemPrefabFolder + "/GameManager");

            // create a variant of the UMA GLIB prefab and add it to the scene
            EditorUtility.DisplayProgressBar("UMA Support Wizard", "Creating UMA GLIB prefab variant...", 0.5f);
            GameObject umaGlibVariant = MakeUMAPrefabVariant(prefabPath + "/GameManager/UMA_GLIB.prefab", fileSystemGameName);

            // Save changes to the load game scene
            EditorUtility.DisplayProgressBar("UMA Support Wizard", "Saving load game scene...", 0.7f);
            EditorSceneManager.SaveScene(loadGameScene);

            // install UMA player units template
            EditorUtility.DisplayProgressBar("UMA Support Wizard", "Installing UMA player units...", 0.8f);
            InstallUMAPlayerUnitsTemplate(fileSystemGameName);

            // update game manager configuration
            EditorUtility.DisplayProgressBar("UMA Support Wizard", "Configuring game options...", 0.9f);
            UpdateGameManagerConfiguration(gameManagerPrefabPath);

            AssetDatabase.Refresh();

            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("UMA Support Wizard", "UMA Support successfully added to " + gameName + "!", "OK");
        }

        private bool CheckFilesExist() {

            // check for presence of the UMA GLIB prefab
            if (WizardUtilities.CheckFileExists(pathToUMAGLIBPrefab, "UMA GLIB Prefab") == false) {
                return false;
            }

            // check for presence of UMA player units template
            if (WizardUtilities.CheckFileExists(pathToPlayerUnitsTemplate, "UMA Player Units Template") == false) {
                return false;
            }

            return true;
        }

        private GameObject MakeUMAPrefabVariant(string newPath, string gameName) {

            GameObject umaGameObject = (GameObject)AssetDatabase.LoadMainAssetAtPath("Assets" + pathToUMAGLIBPrefab);

            // instantiate original
            GameObject instantiatedGO = (GameObject)PrefabUtility.InstantiatePrefab(umaGameObject);
            instantiatedGO.name = gameName + instantiatedGO.name;

            // make variant on disk
            GameObject variant = PrefabUtility.SaveAsPrefabAsset(instantiatedGO, newPath);

            // remove original from scene
            GameObject.DestroyImmediate(instantiatedGO);

            // instantiate new variant in scene
            GameObject sceneVariant = (GameObject)PrefabUtility.InstantiatePrefab(variant);
            sceneVariant.name = umaGameObject.name;

            return variant;
        }

        private void InstallUMAPlayerUnitsTemplate(string fileSystemGameName) {

            List<ScriptableContentTemplate> contentTemplates = new List<ScriptableContentTemplate>();
            contentTemplates.Add((ScriptableContentTemplate)AssetDatabase.LoadMainAssetAtPath("Assets" + pathToPlayerUnitsTemplate));

            TemplateContentWizard.RunWizard(fileSystemGameName, gameParentFolder, contentTemplates, true, true);
        }

        private void UpdateGameManagerConfiguration(string gameManagerPrefabPath) {

            GameObject gameManagerPrefab = (GameObject)AssetDatabase.LoadMainAssetAtPath(gameManagerPrefabPath);
            
            if (gameManagerPrefab != null) {
                SystemConfigurationManager systemConfigurationManager = gameManagerPrefab.GetComponent<SystemConfigurationManager>();
                
                if (systemConfigurationManager != null) {
                    systemConfigurationManager.DefaultPlayerUnitProfileName = "UMA Human Male";
                    
                    EditorUtility.SetDirty(gameManagerPrefab);
                    AssetDatabase.SaveAssets();
                }
            }
        }

        private void OnWizardUpdate() {
            helpString = "Adds UMA support to an existing game";
            string fileSystemGameName = WizardUtilities.GetFileSystemGameName(gameName);
            errorString = Validate(fileSystemGameName);
            isValid = (errorString == null || errorString == "");
        }

        private string Validate(string fileSystemGameName) {

            if (fileSystemGameName == "") {
                return "Game name must not be empty";
            }

            // check for game folder existing
            string gameFolder = WizardUtilities.GetGameFileSystemFolder(gameParentFolder, fileSystemGameName);
            if (System.IO.Directory.Exists(gameFolder) == false) {
                return "The folder " + gameFolder + " does not exist. Please ensure you have opened a scene from an existing game.";
            }

            // check that GameManager prefab exists
            string gameManagerPrefabPath = Application.dataPath + gameParentFolder + fileSystemGameName + "/Prefab/GameManager/" + fileSystemGameName + "GameManager.prefab";
            if (System.IO.File.Exists(gameManagerPrefabPath) == false) {
                return "GameManager prefab not found. Please ensure you have opened a scene from an existing game.";
            }

            // check that game load scene exists
            string loadScenePath = Application.dataPath + gameParentFolder + fileSystemGameName + "/Scenes/" + fileSystemGameName + "/" + fileSystemGameName + ".unity";
            if (System.IO.File.Exists(loadScenePath) == false) {
                return "Game load scene not found. Please ensure you have opened a scene from an existing game.";
            }

            // check if UMA GLIB prefab already exists
            string umaGlibPath = Application.dataPath + gameParentFolder + fileSystemGameName + "/Prefab/GameManager/UMA_GLIB.prefab";
            if (System.IO.File.Exists(umaGlibPath)) {
                return "UMA GLIB prefab already exists. UMA support may already be installed for this game.";
            }

            return null;
        }

        protected override bool DrawWizardGUI() {
            
            EditorGUILayout.LabelField("Game Options", EditorStyles.boldLabel);
            
            EditorGUILayout.LabelField("Game Name", gameName);
            EditorGUILayout.HelpBox("The wizard will add UMA support to the currently loaded game.", MessageType.Info);

            return true;
        }

    }

}
