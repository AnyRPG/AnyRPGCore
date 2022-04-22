using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace AnyRPG {
    public class TemplateContentWizard : ScriptableWizard {

        // Will be a subfolder of Application.dataPath and should start with "/"
        private string gameParentFolder = "/Games/";

        private static int totalResourceCount = 0;
        private static int totalPrefabCount = 0;
        private static int copyResourceCount = 0;
        private static int copyPrefabCount = 0;
        private static int skipResourceCount = 0;
        private static int skipPrefabCount = 0;

        // user modified variables
        [Header("Game")]
        public string gameName = string.Empty;

        [Header("Content")]

        [Tooltip("If true, replace any content that exists instead of skipping existing scriptable objects")]
        public bool replaceExistingResources = false;

        [Tooltip("If true, replace any content that exists instead of skipping existing prefabs")]
        public bool replaceExistingPrefabs = false;

        [Tooltip("The scriptable content to copy")]
        public List<ScriptableContentTemplate> scriptableContent = new List<ScriptableContentTemplate>();

        [MenuItem("Tools/AnyRPG/Wizard/Template Content Wizard")]
        public static void CreateWizard() {
            ScriptableWizard.DisplayWizard<TemplateContentWizard>("Template Content Wizard", "Install");
        }

        void OnEnable() {
            SystemConfigurationManager systemConfigurationManager = WizardUtilities.GetSystemConfigurationManager();
            gameName = WizardUtilities.GetGameName(systemConfigurationManager);
            gameParentFolder = WizardUtilities.GetGameParentFolder(systemConfigurationManager, gameName);

        }

        void OnWizardCreate() {

            EditorUtility.DisplayProgressBar("Template Content Wizard", "Creating Dependency Closure...", 0.1f);
            try {
                string fileSystemGameName = WizardUtilities.GetFileSystemGameName(gameName);
                RunWizard(fileSystemGameName, gameParentFolder, scriptableContent, replaceExistingResources, replaceExistingPrefabs);

            } catch {
                // do nothing
                Debug.LogWarning("An error was detected while running wizard.  See console log for details");
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("Template Content Wizard",
                    "Error!  See console log for details",
                    "OK");
                throw;
            }

            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("Template Content Wizard",
                "Template Content Wizard Complete!" +
                "\nCopied " + copyResourceCount + " / " + totalResourceCount + " resources." +
                "\nSkipped " + skipResourceCount + " existing resources." +
                "\nCopied " + copyPrefabCount + " / " + totalPrefabCount + " prefabs." +
                "\nSkipped " + skipPrefabCount + " existing prefabs.",
                "OK");

        }

        public static void RunWizard(string fileSystemGameName, string gameParentFolder, List<ScriptableContentTemplate> scriptableContent, bool replaceExistingResources, bool replaceExistingPrefabs) {
            copyResourceCount = 0;
            copyPrefabCount = 0;
            skipResourceCount = 0;
            skipPrefabCount = 0;

            // create a dependency closure of unique scriptable content templates
            List<ScriptableContentTemplate> scriptableContentTemplates = GetDependencyClosure(scriptableContent);

            EditorUtility.DisplayProgressBar("Template Content Wizard", "Getting Unique List of Resources...", 0.2f);

            List<DescribableResource> describableResources = GetDescribableResources(scriptableContentTemplates);
            totalResourceCount = describableResources.Count;

            List<GameObject> gameObjects = GetGameObjects(scriptableContentTemplates);
            totalPrefabCount = gameObjects.Count;

            EditorUtility.DisplayProgressBar("Template Content Wizard", "Copying Resources...", 0.3f);

            CopyResources(fileSystemGameName, gameParentFolder, describableResources, replaceExistingResources, 0, totalResourceCount + totalPrefabCount);

            CopyPrefabs(fileSystemGameName, gameParentFolder, gameObjects, replaceExistingPrefabs, totalResourceCount, totalResourceCount + totalPrefabCount);

            AssetDatabase.Refresh();

        }

        public static void CopyResources(string fileSystemGameName, string gameParentFolder, List<DescribableResource> describableResources, bool replaceExistingResources, int beginCount, int totalCount) {

            string newGameFolder = WizardUtilities.GetGameFileSystemFolder(gameParentFolder, fileSystemGameName);

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
                    WizardUtilities.CreateFolderIfNotExists(resourcesFolder);

                }

                // copy the resource
                string destinationPartialPath = gameParentFolder + fileSystemGameName + "/Resources/" + fileSystemGameName + basepath + "/" + assetPathComponents[assetPathComponents.Length - 1].Replace("Template", "");
                string destinationAssetpath = "Assets" + destinationPartialPath;
                string destinationFilesystemPath = Application.dataPath + destinationPartialPath;
                //Debug.Log(destinationFilesystemPath);
                
                if (System.IO.File.Exists(destinationFilesystemPath) == false || replaceExistingResources == true) {
                    Debug.Log("Copying Resource from '" + assetPath + "' to '" + destinationAssetpath + "'");
                    if (AssetDatabase.CopyAsset(assetPath, destinationAssetpath)) {
                        copyResourceCount++;
                    }
                } else {
                    Debug.Log("Skipping copy. Resource '" + destinationAssetpath + "' already exists");
                    skipResourceCount++;
                }

                copyCount++;
                EditorUtility.DisplayProgressBar("Template Content Wizard", "Copying Resources...", 0.3f + ((((float)beginCount + copyCount) / totalCount) * 0.7f));

            }

        }

        private static void CopyPrefabs(string fileSystemGameName, string gameParentFolder, List<GameObject> objectResources, bool replaceExistingPrefabs, int beginCount, int totalCount) {

            string newGameFolder = WizardUtilities.GetGameFileSystemFolder(gameParentFolder, fileSystemGameName);

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
                    WizardUtilities.CreateFolderIfNotExists(resourcesFolder);

                }

                // copy the prefab
                string destinationPartialPath = gameParentFolder + fileSystemGameName + "/Prefab" + basepath + "/" + assetPathComponents[assetPathComponents.Length - 1].Replace("Template", "");
                string destinationAssetpath = "Assets" + destinationPartialPath;
                string destinationFilesystemPath = Application.dataPath + destinationPartialPath;
                //Debug.Log(destinationFilesystemPath);

                if (System.IO.File.Exists(destinationFilesystemPath) == false || replaceExistingPrefabs == true) {
                    Debug.Log("Copying Resource from '" + assetPath + "' to '" + destinationAssetpath + "'");
                    if (AssetDatabase.CopyAsset(assetPath, destinationAssetpath)) {
                        copyPrefabCount++;
                    }
                } else {
                    Debug.Log("Skipping copy. Prefab '" + destinationAssetpath + "' already exists");
                    skipPrefabCount++;
                }

                copyCount++;
                EditorUtility.DisplayProgressBar("Template Content Wizard", "Copying Resources...", 0.3f + ((((float)beginCount + copyCount) / totalCount) * 0.7f));

            }

        }

        private static List<DescribableResource> GetDescribableResources(List<ScriptableContentTemplate> scriptableContentTemplates) {
            List<DescribableResource> returnList = new List<DescribableResource>();

            // create a list of unique describable resources from the scriptable content templates
            foreach (ScriptableContentTemplate scriptableContentTemplate in scriptableContentTemplates) {
                foreach (DescribableResource describableResource in scriptableContentTemplate.Resources) {
                    if (describableResource != null) {
                        if (returnList.Contains(describableResource) == false) {
                            returnList.Add(describableResource);
                        }
                    } else {
                        Debug.LogWarning("Null resource found in list for " + scriptableContentTemplate.ResourceName);
                    }
                }
            }

            return returnList;
        }

        private static List<GameObject> GetGameObjects(List<ScriptableContentTemplate> scriptableContentTemplates) {
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

        private static List<ScriptableContentTemplate> GetDependencyClosure(List<ScriptableContentTemplate> dependencyMaster, List<ScriptableContentTemplate> crawledList = null) {

            // assumption here that the initial list is already unique
            List<ScriptableContentTemplate> returnList = new List<ScriptableContentTemplate>();
            returnList.AddRange(dependencyMaster);

            //List<ScriptableContentTemplate> returnList = new List<ScriptableContentTemplate>();
            foreach (ScriptableContentTemplate dependency in dependencyMaster) {
                
                // create a list of items that is not already in the return list, and crawl them for more dependencies
                List<ScriptableContentTemplate> crawlList = new List<ScriptableContentTemplate>();
                foreach (ScriptableContentTemplate scriptableContentTemplate in dependency.Dependencies) {
                    if (returnList.Contains(scriptableContentTemplate) == false &&
                        (crawledList == null || crawledList.Contains(scriptableContentTemplate) == false)) {
                        if (scriptableContentTemplate == null) {
                            Debug.LogWarning("Null dependency found in list for " + dependency.ResourceName);
                        } else {
                            crawlList.Add(scriptableContentTemplate);
                        }
                    }
                }
                List<ScriptableContentTemplate> crawlListResults = GetDependencyClosure(crawlList, returnList);

                foreach (ScriptableContentTemplate scriptableContentTemplate in crawlListResults) {
                    if (returnList.Contains(scriptableContentTemplate) == false) {
                        returnList.Add(scriptableContentTemplate);
                    }
                }
                // crawl the unique list and add it to the return list
                //returnList.AddRange(GetDependencyClosure(crawlList));
            }

            return returnList;
        }

        void OnWizardUpdate() {
            helpString = "Copies Scriptable Objects and their dependencies to a game";

            string fileSystemGameName = WizardUtilities.GetFileSystemGameName(gameName);

            errorString = Validate(fileSystemGameName);
            isValid = (errorString == null || errorString == "");
        }

        

        string Validate(string fileSystemGameName) {

            // check for empty game name
            if (gameName == null || gameName.Trim() == "") {
                return "Game name must not be empty";
            }
           
            // check for game folder existing
            string newGameFolder = WizardUtilities.GetGameFileSystemFolder(gameParentFolder, fileSystemGameName);
            if (System.IO.Directory.Exists(newGameFolder) == false) {
                return "The folder " + newGameFolder + " does not exist.  Please run the new game wizard first to create the game folder structure";
            }

            return null;
        }

    }

}
