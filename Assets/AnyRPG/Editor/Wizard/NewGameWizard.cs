using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;


public class NewGameWizard : ScriptableWizard {
    // Template path/scene that will be used to create the new game
    private const string templateName = "NewGameTemplate";
    private const string pathToNewGameTemplate = "/AnyRPG/Engine/Core/Games/" + templateName;
    private const string pathToResourcesFolder = "/AnyRPG/Engine/Core/Games/CoreGame/Resources";

    private const string pathToGameManagerPrefab = "Assets/AnyRPG/Engine/Core/System/Prefabs/GameManager/GameManager.prefab";
    private const string pathToUMADCSPrefab = "Getting Started/UMA_DCS.prefab";

    // Will be a subfolder of Application.dataPath and should start with "/"
    private const string newGameParentFolder = "/";

    public string gameName = "";
    public string gameVersion = "0.1a";
    public string sceneName = "FirstScene";
    public string umaRoot = "Assets/UMA/";

    [MenuItem("Tools/AnyRPG/New Game Wizard")]
    static void CreateWizard() {
        ScriptableWizard.DisplayWizard<NewGameWizard>("New Game Wizard", "Create");
    }

    void OnWizardCreate() {

        EditorUtility.DisplayProgressBar("New Game Wizard", "Checking parameters...", 0.1f);

        // Check for presence of necessary prefabs
        GameObject gmGO = (GameObject)AssetDatabase.LoadMainAssetAtPath(pathToGameManagerPrefab);
        if (gmGO == null) {
            ShowError("Missing GameManager prefab at " + pathToGameManagerPrefab + ".  Aborting...");
            return;
        }

        string umaPrefabPath = umaRoot + pathToUMADCSPrefab;
        GameObject umaGO = (GameObject)AssetDatabase.LoadMainAssetAtPath(umaPrefabPath);
        if (umaGO == null) {
            // NOTE: This could be changed to automagically search for the UMA prefab by asset name
            ShowError("Missing UMA prefab at " + umaPrefabPath + ".  Aborting...");
            return;
        }


        // Set default values of game properties just in case they are somehow missing
        if (gameVersion == null || gameVersion.Trim() == "") {
            gameVersion = "0.1a";
            Debug.Log("Empty game version.  Defaulting to " + gameVersion);
        }

        if (sceneName == null || sceneName.Trim() == "") {
            sceneName = gameName + "Scene";
            Debug.Log("Empty first scene name.  Defaulting to \"" + sceneName + "\"");
        }

        EditorUtility.DisplayProgressBar("New Game Wizard", "Creating folders...", 0.2f);
        // Create root game folder
        string newGameFolder = GetNewGameFolder();
        // Copy template game to newly created folder
        FileUtil.CopyFileOrDirectory(Application.dataPath + pathToNewGameTemplate, newGameFolder);

        AssetDatabase.Refresh();

        EditorUtility.DisplayProgressBar("New Game Wizard", "Renaming folders...", 0.3f);

        // Find every folder with the name of the new game template "NewGameTemplate" and rename it as necessary
        // This assumes that all folders that have the template name should have their name changed and that only 
        // folders will have their name changed
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

        EditorUtility.DisplayProgressBar("New Game Wizard", "Creating Resources folder...", 0.4f);
        // Create a Resources folder(s) for scriptable objects.  Create directory structure only
        string newResourcesRootPath = newGameFolder + "/Resources";
        if (!System.IO.Directory.Exists(newResourcesRootPath)) {
            System.IO.Directory.CreateDirectory(newResourcesRootPath);
        }

        AssetDatabase.Refresh();

        EditorUtility.DisplayProgressBar("New Game Wizard", "Modifying scene...", 0.6f);
        // Open the scene to add the necessary elements
        Scene newScene = EditorSceneManager.OpenScene(newScenePath);

        // Create a variant of the GameManager & UMA prefabs
        EditorUtility.DisplayProgressBar("New Game Wizard", "Making prefab variants...", 0.7f);
        GameObject gmVariant = MakePrefabVariant(gmGO, newSceneFolder + "/GameManager.prefab", gameName);
        MakePrefabVariant(umaGO, newSceneFolder + "/UMA_DCS.prefab", gameName);

        // Set the properties of the GameManager for this game
        SystemConfigurationManager systemConfigurationManager = gmVariant.GetComponent<SystemConfigurationManager>();
        systemConfigurationManager.GameName = gameName;
        systemConfigurationManager.GameVersion = gameVersion;
        systemConfigurationManager.InitializationScene = gameName;
        systemConfigurationManager.MainMenuScene = "MainMenu";

        EditorUtility.DisplayProgressBar("New Game Wizard", "Copying resources...", 0.8f);
        // Create a Resources folder and point the game manager to it
        if (systemConfigurationManager.LoadResourcesFolders == null) {
            systemConfigurationManager.LoadResourcesFolders = new List<string>();
        }
        string newResourcesFolderName = gameName;
        systemConfigurationManager.LoadResourcesFolders.Add(newResourcesFolderName);

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
        }
        else {
            Debug.LogWarning(pathToResourcesFolder + " was not found.  Resources folder will be empty.");
        }

        // Finally, rename "CoreGame" in the Resources folder to the game name.  Then the GameManager will be pointing to the correct folder
        AssetDatabase.RenameAsset(FileUtil.GetProjectRelativePath(newResourcesRootPath + "/CoreGame"), gameName);

        EditorUtility.DisplayProgressBar("New Game Wizard", "Saving scene...", 0.9f);
        // Save changes to the scene
        EditorSceneManager.SaveScene(newScene);
        EditorUtility.ClearProgressBar();
        EditorUtility.DisplayDialog("New Game Wizard", "New Game Wizard Complete!  New scene found at " + newScenePath, "OK");

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

    private void ShowError(string message)
    {
        EditorUtility.DisplayDialog("Error", message, "OK");
    }

    private GameObject MakePrefabVariant(GameObject goToMakeVariantOf, string newPath, string gameName) {
        GameObject instantiatedGO = (GameObject)PrefabUtility.InstantiatePrefab(goToMakeVariantOf);
        GameObject variant = PrefabUtility.SaveAsPrefabAsset(instantiatedGO, newPath);
        variant.name = gameName + variant.name;
        return variant;
    }
}
