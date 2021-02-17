using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;


public class NewGameWizard : ScriptableWizard {
    // Template path/scene that will be used to create the new game
    private const string templateName = "NewGameTemplate";
    private const string pathToNewGameTemplate = "/AnyRPG/Engine/Core/Games/" + templateName;

    private const string pathToGameManagerPrefab = "Assets/AnyRPG/Engine/Core/System/Prefabs/GameManager/GameManager.prefab";

    // Will be a subfolder of Application.dataPath and should start with "/"
    private const string newGameParentFolder = "/";

    public string gameName = "";
    public string gameVersion = "0.1a";
    public string sceneName = "FirstScene";

    [MenuItem("Tools/AnyRPG/New Game Wizard")]
    static void CreateWizard() {
        ScriptableWizard.DisplayWizard<NewGameWizard>("New Game Wizard", "Create");
    }

    // Start is called before the first frame update
    void OnWizardCreate() {

        if (gameVersion == null || gameVersion.Trim() == "") {
            gameVersion = "0.1a";
        }

        if (sceneName == null || sceneName.Trim() == "") {
            sceneName = gameName + "Scene";
        }

        // Create root game folder
        string newGameFolder = GetNewGameFolder();
        // Copy template game to newly created folder
        FileUtil.CopyFileOrDirectory(Application.dataPath + pathToNewGameTemplate, newGameFolder);

        AssetDatabase.Refresh();

        // Find every folder with the name of the new game template "NewGameTemplate" and rename it as necessary
        string[] templateNamedAssets = AssetDatabase.FindAssets(templateName, new string[] { FileUtil.GetProjectRelativePath(newGameFolder) });
        foreach (string templateNamedAssetGuid in templateNamedAssets) {
            string namedAssetPath = AssetDatabase.GUIDToAssetPath(templateNamedAssetGuid);
            if (System.IO.Directory.Exists(namedAssetPath)) { 
                string assetName = System.IO.Path.GetFileName(namedAssetPath);
                string newAssetName = namedAssetPath.Replace(templateName, gameName);
                AssetDatabase.RenameAsset(namedAssetPath, gameName);
            }
        }
        AssetDatabase.Refresh();

        // Rename the template scene to the scene name
        string newSceneFolder = FileUtil.GetProjectRelativePath(newGameFolder + "/Scenes/" + gameName);
        string newSceneName = sceneName + ".unity";
        string newScenePath = newSceneFolder + "/" + newSceneName;
        string templateScenePath = newSceneFolder + "/" + templateName + ".unity";
        AssetDatabase.RenameAsset(templateScenePath, newSceneName);

        AssetDatabase.Refresh();
        // Open the scene to add the necessary elements
        EditorSceneManager.OpenScene(newScenePath);

        // Create a variant of the GameManager
        GameObject gmGO = (GameObject)AssetDatabase.LoadMainAssetAtPath(pathToGameManagerPrefab);
        GameObject instantiatedGM = (GameObject)PrefabUtility.InstantiatePrefab(gmGO);
        GameObject gmVariant = PrefabUtility.SaveAsPrefabAsset(instantiatedGM, newSceneFolder + "/GameManager.prefab");
        gmVariant.name = gameName + "GameManager";

        // Set the properties of the GameManager for this game
        SystemConfigurationManager systemConfigurationManager = gmVariant.GetComponent<SystemConfigurationManager>();
        systemConfigurationManager.GameName = gameName;
        systemConfigurationManager.GameVersion = gameVersion;
        systemConfigurationManager.InitializationScene = gameName;
        systemConfigurationManager.MainMenuScene = "MainMenu";

    }

    void OnWizardUpdate() {
        helpString = "Creates a new game based on the AnyRPG template";
        errorString = Validate();
        isValid = (errorString == null || errorString == "");
    }

    string GetNewGameFolder() {
        return Application.dataPath + newGameParentFolder + gameName;
    }

    string Validate() {
        if (gameName == null || gameName.Trim() == "") {
            return "Game name must not be empty";
        }
        string newGameFolder = GetNewGameFolder();
        if (System.IO.Directory.Exists(newGameFolder))
        {
            return "Folder " + newGameFolder + " already exists.  Please delete this directory or choose a new game name";
        }

        return null;
    }
}
