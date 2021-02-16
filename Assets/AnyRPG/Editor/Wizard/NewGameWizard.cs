using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


public class NewGameWizard : ScriptableWizard
{
    // Will be a subfolder of Application.dataPath and should start with "/"
    private const string newGameParentFolder = "/";

    public string gameName = "";
    public string gameVersion = "0.1a";
    public string sceneName = "FirstScene";

    [MenuItem("Tools/AnyRPG/New Game Wizard")]
    static void CreateWizard()
    {
        ScriptableWizard.DisplayWizard<NewGameWizard>("New Game Wizard", "Create");
    }

    // Start is called before the first frame update
    void OnWizardCreate()
    {
        // Create root game folder
        string newGameFolder = GetNewGameFolder();
        if (!System.IO.Directory.Exists(gameName))
        {
            System.IO.Directory.CreateDirectory(newGameFolder);
        }
        AssetDatabase.Refresh();
    }

    void OnWizardUpdate()
    {
        helpString = "Creates a new game based on the AnyRPG template";
        errorString = Validate();
        isValid = (errorString == null || errorString == "");
    }

    string GetNewGameFolder()
    {
        return Application.dataPath + newGameParentFolder + gameName;
    }

    string Validate()
    {
        if (gameName == null || gameName.Trim() == "")
        {
            return "Game name must not be empty";
        }
        string newGameFolder = GetNewGameFolder();
        if (System.IO.Directory.Exists(newGameFolder))
        {
            return "Folder " + newGameFolder + " already exists.  Please delete this directory or choose a new game name";
        }
        return null;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
