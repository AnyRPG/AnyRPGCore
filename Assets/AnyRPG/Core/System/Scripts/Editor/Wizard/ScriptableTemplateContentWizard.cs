using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace AnyRPG {
    public class ScriptableTemplateContentWizard : ScriptableWizard {

        private const string defaultUnitPrefabPath = "/AnyRPG/Core/System/Prefabs/Character/Unit/DefaultCharacterUnit.prefab";

        // a reference to the systemConfigurationManager found in the currently open scene, for automatic determination of the game name
        // and setting the newly created unit profile as the default if necessary
        private SystemConfigurationManager systemConfigurationManager = null;

        // Will be a subfolder of Application.dataPath and should start with "/"
        private const string newGameParentFolder = "/Games/";

        // the used file path name for the game
        private string fileSystemGameName = string.Empty;

        // the used asset path for the Unit Profile
        private string scriptableObjectPath = string.Empty;

        // user modified variables
        [Header("Game")]
        public string gameName = string.Empty;

        [Header("Content")]

        [Tooltip("If true, replace any content that exists instead of skipping existing items")]
        public bool replaceExisting = false;

        [Tooltip("The scriptable content to copy")]
        public List<ScriptableContentTemplate> scriptableContent = new List<ScriptableContentTemplate>();

        [MenuItem("Tools/AnyRPG/Wizard/Scriptable Template Content Wizard")]
        public static void CreateWizard() {
            ScriptableWizard.DisplayWizard<ScriptableTemplateContentWizard>("Scriptable Template Content Wizard", "Create");
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

            EditorUtility.DisplayProgressBar("Scriptable Template Content Wizard", "Creating Dependency Closure...", 0.1f);

            // create a dependency closure of unique scriptable content templates
            List<ScriptableContentTemplate> scriptableContentTemplates = GetDependencyClosure(scriptableContent);

            EditorUtility.DisplayProgressBar("Scriptable Template Content Wizard", "Getting Unique List of Resources...", 0.2f);

            List<DescribableResource> describableResources = GetDescribableResources(scriptableContentTemplates);

            List<GameObject> gameObjects = GetGameObjects(scriptableContentTemplates);

            EditorUtility.DisplayProgressBar("Scriptable Template Content Wizard", "Copying Resources...", 0.3f);

            CopyResources(describableResources, 0, describableResources.Count + gameObjects.Count);

            CopyPrefabs(gameObjects, describableResources.Count, describableResources.Count + gameObjects.Count);

            AssetDatabase.Refresh();

            //EditorUtility.DisplayProgressBar("New Character Wizard", "Creating Unit Profile...", 0.3f);
            //UnitProfile asset = ScriptableObject.CreateInstance("UnitProfile") as UnitProfile;

            //EditorUtility.DisplayProgressBar("New Character Wizard", "Saving Unit Profile...", 0.5f);

            //scriptableObjectPath = "Assets" + newGameParentFolder + fileSystemGameName + "/Resources/" + fileSystemGameName + "/UnitProfile/" + characterName + "Unit.asset";
            //AssetDatabase.CreateAsset(asset, scriptableObjectPath);

            AssetDatabase.Refresh();

            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("Scriptable Template Content Wizard", "Scriptable Template Content Wizard Complete!", "OK");

        }

        private void CopyResources(List<DescribableResource> describableResources, int beginCount, int totalCount) {

            string newGameFolder = GetNewGameFolder();

            int copyCount = 0;
            foreach (DescribableResource describableResource in describableResources) {

                // get the existing path of the resource to copy
                string assetPath = AssetDatabase.GetAssetPath(describableResource);
                //Debug.Log("Copying Resource at path: " + assetPath);

                // find the resources folder in the path
                string[] assetPathComponents = assetPath.Split('/');
                int basePosition = 0;
                for (int i = 0; i < assetPathComponents.Length; i++) {
                    if (assetPathComponents[i] == "TemplateResources") {
                        basePosition = i;
                        break;
                    }
                }

                string basepath = "";
                // start 2 folders above resources, and end at the folder below the filename
                // eg Assets/AnyRPG/Core/Games/FeaturesDemo/Resources/FeaturesDemoGame/[AbilityEffect/Attack/]AttackEffect.asset
                for (int i = basePosition + 1; i < assetPathComponents.Length -1; i++) {
                    basepath += "/" + assetPathComponents[i];
                    string resourcesFolder = newGameFolder + "/Resources/" + fileSystemGameName + basepath;

                    // create the resources folder
                    //Debug.Log("Creating folder " + resourcesFolder);
                    CreateFolderIfNotExists(resourcesFolder);

                }

                // copy the resource
                string destinationPartialPath = "/Games/" + fileSystemGameName + "/Resources/" + fileSystemGameName + basepath + "/" + assetPathComponents[assetPathComponents.Length - 1].Replace("Template", "");
                string destinationAssetpath = "Assets" + destinationPartialPath;
                string destinationFilesystemPath = Application.dataPath + destinationPartialPath;
                //Debug.Log(destinationFilesystemPath);
                
                if (System.IO.File.Exists(destinationFilesystemPath) == false || replaceExisting == true) {
                    Debug.Log("Copying Resource from '" + assetPath + "' to '" + destinationAssetpath + "'");
                    AssetDatabase.CopyAsset(assetPath, destinationAssetpath);
                } else {
                    Debug.Log("Skipping copy. Resource '" + destinationAssetpath + "' already exists");
                }

                copyCount++;
                EditorUtility.DisplayProgressBar("Scriptable Template Content Wizard", "Copying Resources...", 0.3f + ((((float)beginCount + copyCount) / totalCount) * 0.7f));

            }

        }

        private void CopyPrefabs(List<GameObject> objectResources, int beginCount, int totalCount) {

            string newGameFolder = GetNewGameFolder();

            int copyCount = 0;

            foreach (GameObject prefabObject in objectResources) {

                // get the existing path of the resource to copy
                string assetPath = AssetDatabase.GetAssetPath(prefabObject);
                //Debug.Log("Copying Resource at path: " + assetPath);

                // find the resources folder in the path
                string[] assetPathComponents = assetPath.Split('/');
                int basePosition = 0;
                for (int i = 0; i < assetPathComponents.Length; i++) {
                    if (assetPathComponents[i] == "TemplatePrefabs") {
                        basePosition = i;
                        break;
                    }
                }

                string basepath = "";
                // start 2 folders above resources, and end at the folder below the filename
                // eg Assets/AnyRPG/Core/Games/FeaturesDemo/Resources/FeaturesDemoGame/[AbilityEffect/Attack/]AttackEffect.asset
                for (int i = basePosition + 1; i < assetPathComponents.Length - 1; i++) {
                    basepath += "/" + assetPathComponents[i];
                    string resourcesFolder = newGameFolder + "/Prefab" + basepath;

                    // create the resources folder
                    //Debug.Log("Creating folder " + resourcesFolder);
                    CreateFolderIfNotExists(resourcesFolder);

                }

                // copy the resource
                string destinationPartialPath = "/Games/" + fileSystemGameName + "/Prefab" + basepath + "/" + assetPathComponents[assetPathComponents.Length - 1].Replace("Template", "");
                string destinationAssetpath = "Assets" + destinationPartialPath;
                string destinationFilesystemPath = Application.dataPath + destinationPartialPath;
                //Debug.Log(destinationFilesystemPath);

                if (System.IO.File.Exists(destinationFilesystemPath) == false || replaceExisting == true) {
                    Debug.Log("Copying Resource from '" + assetPath + "' to '" + destinationAssetpath + "'");
                    AssetDatabase.CopyAsset(assetPath, destinationAssetpath);
                } else {
                    Debug.Log("Skipping copy. Prefab '" + destinationAssetpath + "' already exists");
                }

                copyCount++;
                EditorUtility.DisplayProgressBar("Scriptable Template Content Wizard", "Copying Resources...", 0.3f + ((((float)beginCount + copyCount) / totalCount) * 0.7f));

            }

        }

        private List<DescribableResource> GetDescribableResources(List<ScriptableContentTemplate> scriptableContentTemplates) {
            List<DescribableResource> returnList = new List<DescribableResource>();

            // create a list of unique describable resources from the scriptable content templates
            foreach (ScriptableContentTemplate scriptableContentTemplate in scriptableContentTemplates) {
                foreach (DescribableResource describableResource in scriptableContentTemplate.Resources) {
                    if (returnList.Contains(describableResource) == false) {
                        returnList.Add(describableResource);
                    }
                }
            }

            return returnList;
        }

        private List<GameObject> GetGameObjects(List<ScriptableContentTemplate> scriptableContentTemplates) {
            List<GameObject> returnList = new List<GameObject>();

            // create a list of unique describable resources from the scriptable content templates
            foreach (ScriptableContentTemplate scriptableContentTemplate in scriptableContentTemplates) {
                foreach (GameObject gameObject in scriptableContentTemplate.Prefabs) {
                    if (returnList.Contains(gameObject) == false) {
                        returnList.Add(gameObject);
                    }
                }
            }

            return returnList;
        }

        private List<ScriptableContentTemplate> GetDependencyClosure(List<ScriptableContentTemplate> dependencyMaster) {

            // assumption here that the initial list is already unique
            List<ScriptableContentTemplate> returnList = new List<ScriptableContentTemplate>();
            returnList.AddRange(dependencyMaster);

            //List<ScriptableContentTemplate> returnList = new List<ScriptableContentTemplate>();
            foreach (ScriptableContentTemplate dependency in dependencyMaster) {
                
                // create a list of items that is not already in the return list, and crawl them for more dependencies
                List<ScriptableContentTemplate> crawlList = new List<ScriptableContentTemplate>();
                foreach (ScriptableContentTemplate scriptableContentTemplate in dependency.Dependencies) {
                    if (returnList.Contains(scriptableContentTemplate) == false) {
                        crawlList.Add(scriptableContentTemplate);
                    }
                }

                // crawl the unique list and add it to the return list
                returnList.AddRange(GetDependencyClosure(crawlList));
            }

            return returnList;
        }

        void OnWizardUpdate() {
            helpString = "Copies Scriptable Objects and their dependencies to a game";

            MakeFileSystemGameName();

            errorString = Validate();
            isValid = (errorString == null || errorString == "");
        }


        private void MakeFileSystemGameName() {
            fileSystemGameName = gameName.Replace(" ", "");
        }

        string GetNewGameFolder() {
            return Application.dataPath + newGameParentFolder + fileSystemGameName;
        }

        string Validate() {

            // check for empty game name
            if (gameName == null || gameName.Trim() == "") {
                return "Game name must not be empty";
            }
           
            // check for game folder existing
            string newGameFolder = GetNewGameFolder();
            if (System.IO.Directory.Exists(newGameFolder) == false) {
                return "The folder " + newGameFolder + "does not exist.  Please run the new game wizard first to create the game folder structure";
            }

            

            return null;
        }

        private void ShowError(string message) {
            EditorUtility.DisplayDialog("Error", message, "OK");
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
