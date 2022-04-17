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
    public class NewGameWizard : ScriptableWizard {

        // Template path/scene that will be used to create the new game
        private const string templateName = "TemplateGame";
        private const string pathToNewGameTemplate = "/AnyRPG/Core/Templates/Game/" + templateName;
        //private const string firstSceneTemplateAssetpath = "Assets/AnyRPG/Core/Templates/Game/Scenes/FirstScene/FirstScene.unity";

        // for now this is necessary due to git not saving empty folders
        private const string pathToResourcesTemplateFolder = "/AnyRPG/Core/Games/FeaturesDemo/Resources/FeaturesDemoGame";

        // required template prefabs
        private const string pathToGameManagerPrefab = "/AnyRPG/Core/System/Prefabs/GameManager/GameManager.prefab";
        private const string pathToSceneConfigPrefab = "/AnyRPG/Core/System/Prefabs/GameManager/SceneConfig.prefab";
        private const string pathToUMAGLIBPrefab = "/UMA/Getting Started/UMA_GLIB.prefab";

        private const string pathToSystemAbilitiesTemplate = "/AnyRPG/Core/Content/TemplatePackages/DefaultSystemEffectsTemplatePackage.asset";
        private const string pathToHealthPowerResourceTemplate = "/AnyRPG/Core/Content/TemplatePackages/PowerResource/HealthPowerResourceTemplatePackage.asset";
        private const string pathToAttackAbilityTemplate = "/AnyRPG/Core/Content/TemplatePackages/AttackAbilityTemplatePackage.asset";
        private const string pathToPlayerUnitsTemplate = "/AnyRPG/Core/Content/TemplatePackages/DefaultPlayerUnitsTemplatePackage.asset";

        // optional template prefabs
        private const string pathToGoldCurrencyGroupTemplate = "/AnyRPG/Core/Content/TemplatePackages/GoldCurrencyGroupTemplatePackage.asset";
        private const string pathToArmorClassesTemplate = "/AnyRPG/Core/Content/TemplatePackages/RPGArmorClassesTemplatePackage.asset";
        private const string pathToItemQualitiesTemplate = "/AnyRPG/Core/Content/TemplatePackages/RPGItemQualitiesTemplatePackage.asset";
        private const string pathToPowerResourcesTemplate = "/AnyRPG/Core/Content/TemplatePackages/RPGPowerResourcesTemplatePackage.asset";
        private const string pathToCharacterStatsTemplate = "/AnyRPG/Core/Content/TemplatePackages/RPGCharacterStatsTemplatePackage.asset";
        private const string pathToUnitToughnessesTemplate = "/AnyRPG/Core/Content/TemplatePackages/RPGUnitToughnessesTemplatePackage.asset";
        private const string pathToWeaponSkillsTemplate = "/AnyRPG/Core/Content/TemplatePackages/RPGWeaponSkillsTemplatePackage.asset";

        // sill be a subfolder of Application.dataPath and should start with "/"
        private const string newGameParentFolder = "/Games/";

        // compare the default first scene directory to any user picked scene name
        //private const string templateFirstSceneName = "FirstScene";

        // the used file path name for the game
        //private string fileSystemGameName = string.Empty;
        //private string fileSystemFirstSceneName = string.Empty;

        // USER MODIFIED VARIABLES
        public string gameName = "";
        public string gameVersion = "0.1a";

        // main menu options
        public AudioClip mainMenuMusic = null;

        public AudioClip newGameMusic = null;

        // first scene options
        public bool copyExistingScene = false;
        public string firstSceneName = "First Scene";

        public SceneAsset existingScene = null;

        public AudioClip firstSceneAmbientSounds = null;

        public AudioClip firstSceneMusic = null;

        // player type
        public DefaultPlayerUnitType defaultPlayerUnitType = DefaultPlayerUnitType.UMA;

        // rpg building blocks
        public bool installGoldCurrencyGroup = true;
        public bool installArmorClasses = true;
        public bool installCharacterStats = true;
        public bool installItemQualities = true;
        public bool installPowerResources = true;
        public bool installUnitToughnesses = true;
        public bool installWeaponSkills = true;

        //private bool addFirstSceneToBuild = true;
        //private string umaRoot = "Assets/UMA/";

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

            string fileSystemGameName = WizardUtilities.GetFileSystemGameName(gameName);
            //fileSystemFirstSceneName = WizardUtilities.GetFilesystemSceneName(firstSceneName);

            // check for presence of template prefabs and resources
            if (CheckFilesExist() == false) {
                return;
            }

            // check that the templates used by the new scene wizard exist
            if (NewSceneWizard.CheckRequiredTemplatesExist() == false) {
                return;
            }

            // Set default values of game properties just in case they are somehow missing
            if (gameVersion == null || gameVersion.Trim() == "") {
                gameVersion = "0.1a";
                Debug.Log("Empty game version.  Defaulting to " + gameVersion);
            }

            EditorUtility.DisplayProgressBar("New Game Wizard", "Creating Game Folder...", 0.2f);
            // Create root game folder
            string fileSystemNewGameFolder = GetFileSystemNewGameFolder(fileSystemGameName);
            string fileSystemResourcesFolder = fileSystemNewGameFolder + "/Resources/" + fileSystemGameName;

            // create base games folder
            WizardUtilities.CreateFolderIfNotExists(Application.dataPath + newGameParentFolder);

            EditorUtility.DisplayProgressBar("New Game Wizard", "Copying Game Template Directory...", 0.3f);

            // create game folder structure
            FileUtil.CopyFileOrDirectory(Application.dataPath + pathToNewGameTemplate, fileSystemNewGameFolder);

            AssetDatabase.Refresh();

            EditorUtility.DisplayProgressBar("New Game Wizard", "Renaming Template Folders...", 0.4f);

            // Find every folder with the name of the new game template "TemplateGame" and rename it as necessary
            // This assumes that all folders that have the template name should have their name changed and that only 
            // folders will have their name changed
            string[] templateNamedAssets = AssetDatabase.FindAssets(templateName, new string[] { FileUtil.GetProjectRelativePath(fileSystemNewGameFolder) });
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
            WizardUtilities.CreateFolderIfNotExists(fileSystemResourcesFolder);

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
                        System.IO.Directory.CreateDirectory(fileSystemResourcesFolder + "/" + relativeAssetPath);
                    }
                }
                AssetDatabase.Refresh();
            } else {
                Debug.LogWarning(pathToResourcesTemplateFolder + " was not found.  Resources folder will be empty.");
            }

            // create prefab folder
            string fileSystemPrefabFolder = fileSystemNewGameFolder + "/Prefab";
            string prefabPath = FileUtil.GetProjectRelativePath(fileSystemPrefabFolder);
            WizardUtilities.CreateFolderIfNotExists(fileSystemPrefabFolder);
            WizardUtilities.CreateFolderIfNotExists(fileSystemPrefabFolder + "/GameManager");

            if (useThirdPartyController == true) {
                ConfigureThirdPartyController(fileSystemGameName, fileSystemResourcesFolder, fileSystemPrefabFolder);
            }

            // Rename the game load scene
            EditorUtility.DisplayProgressBar("New Game Wizard", "Renaming Game Load Scene...", 0.7f);
            string gameLoadSceneFolder = FileUtil.GetProjectRelativePath(fileSystemNewGameFolder + "/Scenes/" + fileSystemGameName);
            string newGameLoadSceneFileName = fileSystemGameName + ".unity";
            string newGameLoadScenePath = gameLoadSceneFolder + "/" + newGameLoadSceneFileName;
            string existingGameLoadScenePath = gameLoadSceneFolder + "/" + templateName + ".unity";

            AssetDatabase.RenameAsset(existingGameLoadScenePath, newGameLoadSceneFileName);
            AssetDatabase.Refresh();

            // add game load scene to build settings
            EditorUtility.DisplayProgressBar("New Game Wizard", "Adding Game Load Scene To Build Settings...", 0.8f);
            List<EditorBuildSettingsScene> currentSceneList = EditorBuildSettings.scenes.ToList();
            Debug.Log("Adding " + newGameLoadScenePath + " to build settings");
            currentSceneList.Add(new EditorBuildSettingsScene(newGameLoadScenePath, true));
            EditorBuildSettings.scenes = currentSceneList.ToArray();

            // Open the scene to add the necessary elements
            EditorUtility.DisplayProgressBar("New Game Wizard", "Modifying loading scene...", 0.85f);
            Debug.Log("Loading Scene at " + newGameLoadScenePath);
            Scene loadGameScene = EditorSceneManager.OpenScene(newGameLoadScenePath);

            // Create a variant of the GameManager
            EditorUtility.DisplayProgressBar("New Game Wizard", "Making prefab variants...", 0.9f);
            GameObject gameManagerGameObject = (GameObject)AssetDatabase.LoadMainAssetAtPath("Assets" + pathToGameManagerPrefab);
            GameObject gameManagerVariant = MakeGameManagerPrefabVariant(fileSystemGameName, gameManagerGameObject, prefabPath + "/GameManager/" + fileSystemGameName + "GameManager.prefab");
            
            // create a variant of the UMA GLIB prefab
            MakeUMAPrefabVariant(prefabPath + "/GameManager/UMA_GLIB.prefab", fileSystemGameName);

            // create a variant of the SceneConfig prefab
            MakeSceneConfigPrefabVariant(fileSystemGameName, gameManagerVariant, prefabPath + "/GameManager/" + fileSystemGameName + "SceneConfig.prefab");

            // Save changes to the load game scene
            EditorUtility.DisplayProgressBar("New Game Wizard", "Saving Load Game Scene...", 0.95f);
            EditorSceneManager.SaveScene(loadGameScene);

            // install default templates
            EditorUtility.DisplayProgressBar("New Game Wizard", "Installing Default Templates...", 0.96f);
            InstallDefaultTemplateContent(fileSystemGameName, newGameParentFolder);

            // install optional templates
            EditorUtility.DisplayProgressBar("New Game Wizard", "Installing Optional Templates...", 0.97f);
            InstallOptionalTemplateContent(fileSystemGameName, newGameParentFolder);

            // make audio profiles and scene nodes for main menu
            EditorUtility.DisplayProgressBar("New Game Wizard", "Configuring Main Menu...", 0.98f);
            ConfigureMainMenuScriptableObjects(fileSystemGameName);

            // create first scene
            NewSceneWizard.CreateScene(newGameParentFolder, gameName, firstSceneName, copyExistingScene, existingScene, firstSceneAmbientSounds, firstSceneMusic);

            AssetDatabase.Refresh();

            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("New Game Wizard", "New Game Wizard Complete! The game loading scene can be found at " + newGameLoadScenePath, "OK");

        }

        private void InstallDefaultTemplateContent(string fileSystemGameName, string newGameParentFolder) {

            List<ScriptableContentTemplate> contentTemplates = new List<ScriptableContentTemplate>();

            contentTemplates.Add((ScriptableContentTemplate)AssetDatabase.LoadMainAssetAtPath("Assets" + pathToSystemAbilitiesTemplate));
            contentTemplates.Add((ScriptableContentTemplate)AssetDatabase.LoadMainAssetAtPath("Assets" + pathToHealthPowerResourceTemplate));
            contentTemplates.Add((ScriptableContentTemplate)AssetDatabase.LoadMainAssetAtPath("Assets" + pathToAttackAbilityTemplate));
            contentTemplates.Add((ScriptableContentTemplate)AssetDatabase.LoadMainAssetAtPath("Assets" + pathToPlayerUnitsTemplate));

            if (contentTemplates.Count > 0) {
                TemplateContentWizard.RunWizard(fileSystemGameName, newGameParentFolder, contentTemplates, true, true);
            }

        }

        private void InstallOptionalTemplateContent(string fileSystemGameName, string newGameParentFolder) {

            List<ScriptableContentTemplate> contentTemplates = new List<ScriptableContentTemplate>();

            if (installGoldCurrencyGroup) {
                //ScriptableContentTemplate goldCurrencyGroupTemplate = (ScriptableContentTemplate)AssetDatabase.LoadMainAssetAtPath("Assets" + pathToGoldCurrencyGroupTemplate);
                contentTemplates.Add((ScriptableContentTemplate)AssetDatabase.LoadMainAssetAtPath("Assets" + pathToGoldCurrencyGroupTemplate));
            }

            if (installArmorClasses) {
                //ScriptableContentTemplate goldCurrencyGroupTemplate = (ScriptableContentTemplate)AssetDatabase.LoadMainAssetAtPath("Assets" + pathToGoldCurrencyGroupTemplate);
                contentTemplates.Add((ScriptableContentTemplate)AssetDatabase.LoadMainAssetAtPath("Assets" + pathToArmorClassesTemplate));
            }

            if (installCharacterStats) {
                contentTemplates.Add((ScriptableContentTemplate)AssetDatabase.LoadMainAssetAtPath("Assets" + pathToCharacterStatsTemplate));
            }

            if (installItemQualities) {
                contentTemplates.Add((ScriptableContentTemplate)AssetDatabase.LoadMainAssetAtPath("Assets" + pathToItemQualitiesTemplate));
            }

            if (installPowerResources) {
                contentTemplates.Add((ScriptableContentTemplate)AssetDatabase.LoadMainAssetAtPath("Assets" + pathToPowerResourcesTemplate));
            }

            if (installUnitToughnesses) {
                contentTemplates.Add((ScriptableContentTemplate)AssetDatabase.LoadMainAssetAtPath("Assets" + pathToUnitToughnessesTemplate));
            }

            if (installWeaponSkills) {
                contentTemplates.Add((ScriptableContentTemplate)AssetDatabase.LoadMainAssetAtPath("Assets" + pathToWeaponSkillsTemplate));
            }

            if (contentTemplates.Count > 0) {
                TemplateContentWizard.RunWizard(fileSystemGameName, newGameParentFolder, contentTemplates, true, true);
            }

        }

        private bool CheckFilesExist() {

            // Check for presence of GameManager prefab
            if (WizardUtilities.CheckFileExists(pathToGameManagerPrefab, "GameManager prefab") == false) {
                return false;
            }

            // Check for presence of SceneConfig prefab
            if (WizardUtilities.CheckFileExists(pathToSceneConfigPrefab, "SceneConfig prefab") == false) {
                return false;
            }

            // check for presence of the UMA GLIB prefab
            if (WizardUtilities.CheckFileExists(pathToUMAGLIBPrefab, "UMA GLIB Prefab") == false) {
                return false;
            }

            // check for presence of default effects template
            if (WizardUtilities.CheckFileExists(pathToSystemAbilitiesTemplate, "Default System Effects Template") == false) {
                return false;
            }

            // check for presence of health power resource template
            if (WizardUtilities.CheckFileExists(pathToHealthPowerResourceTemplate, "Health Power Resource Template") == false) {
                return false;
            }

            // check for presence of health power resource template
            if (WizardUtilities.CheckFileExists(pathToAttackAbilityTemplate, "Attack Ability Template") == false) {
                return false;
            }

            // check for presence of default player units template
            if (WizardUtilities.CheckFileExists(pathToPlayerUnitsTemplate, "Default Player Units Template") == false) {
                return false;
            }

            // check for presence of the gold currency group template
            if (WizardUtilities.CheckFileExists(pathToGoldCurrencyGroupTemplate, "Gold Currency Group Template") == false) {
                return false;
            }

            // check for presence of the armor classes template
            if (WizardUtilities.CheckFileExists(pathToArmorClassesTemplate, "Armor Classes Template") == false) {
                return false;
            }

            // check for presence of the item qualities template
            if (WizardUtilities.CheckFileExists(pathToItemQualitiesTemplate, "Item Qualities Template") == false) {
                return false;
            }

            // check for presence of the power resources template
            if (WizardUtilities.CheckFileExists(pathToPowerResourcesTemplate, "Power Resources Template") == false) {
                return false;
            }

            // check for presence of the character stats template
            if (WizardUtilities.CheckFileExists(pathToCharacterStatsTemplate, "Character Stats Template") == false) {
                return false;
            }

            // check for presence of the unit toughnesses template
            if (WizardUtilities.CheckFileExists(pathToUnitToughnessesTemplate, "Unit Toughnesses Template") == false) {
                return false;
            }

            // check for presence of the weapon skills template
            if (WizardUtilities.CheckFileExists(pathToWeaponSkillsTemplate, "Weapon Skills Template") == false) {
                return false;
            }

            return true;
        }

        private void ConfigureMainMenuScriptableObjects(string fileSystemGameName) {

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

        private void ConfigureThirdPartyController(string fileSystemGameName, string resourcesFolder, string prefabFolder) {
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

        private GameObject MakeGameManagerPrefabVariant(string fileSystemGameName, GameObject goToMakeVariantOf, string newPath) {
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

            // health power resource
            systemConfigurationManager.PowerResources = new List<string>() { "Health" };

            // attack ability
            systemConfigurationManager.Capabilities.AbilityNames = new List<string>() { "Attack" };

            // player unit
            if (defaultPlayerUnitType == DefaultPlayerUnitType.Mecanim) {
                systemConfigurationManager.DefaultPlayerUnitProfileName = "Mecanim Player";
            } else {
                systemConfigurationManager.DefaultPlayerUnitProfileName = "UMA Player";
            }


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

        private GameObject MakeSceneConfigPrefabVariant(string fileSystemGameName, GameObject gameManagerVariant, string newPath) {

            GameObject sceneConfigGameObject = (GameObject)AssetDatabase.LoadMainAssetAtPath("Assets" + pathToSceneConfigPrefab);

            // instantiate original
            GameObject instantiatedGO = (GameObject)PrefabUtility.InstantiatePrefab(sceneConfigGameObject);
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
            //PrefabUtility.InstantiatePrefab(variant);
            GameObject sceneVariant = (GameObject)PrefabUtility.InstantiatePrefab(variant);
            sceneVariant.name = umaGameObject.name;

            return variant;
        }

        private string GetFileSystemNewGameFolder(string fileSystemGameName) {
            return Application.dataPath + newGameParentFolder + fileSystemGameName;
        }

        void OnWizardUpdate() {
            helpString = "Creates a new game based on the AnyRPG template";
            errorString = Validate(WizardUtilities.GetFileSystemGameName(gameName));
            isValid = (errorString == null || errorString == "");
        }

        string Validate(string fileSystemGameName) {

            if (fileSystemGameName == "") {
                return "Game name must not be empty";
            }

            /*
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
            */

            // check that game with same name doesn't already exist
            string newGameFolder = GetFileSystemNewGameFolder(fileSystemGameName);
            if (System.IO.Directory.Exists(newGameFolder)) {
                return "Folder " + newGameFolder + " already exists.  Please delete this directory or choose a new game name";
            }

            // check that first scene name is not empty
            string filesystemSceneName = WizardUtilities.GetFilesystemSceneName(firstSceneName);
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



        protected override bool DrawWizardGUI() {
            //return base.DrawWizardGUI();

            //NewGameWizard myScript = target as NewGameWizard;

            EditorGUILayout.LabelField("Game Options", EditorStyles.boldLabel);

            gameName = EditorGUILayout.TextField("Game Name", gameName);
            gameVersion = EditorGUILayout.TextField("Game Version", gameVersion);

            EditorGUILayout.LabelField("Player Options", EditorStyles.boldLabel);

            defaultPlayerUnitType = (DefaultPlayerUnitType)EditorGUILayout.EnumPopup("Default Player Type", defaultPlayerUnitType);

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

            EditorGUILayout.LabelField("Common RPG Building Blocks", EditorStyles.boldLabel);

            installGoldCurrencyGroup = EditorGUILayout.Toggle(new GUIContent("Install Gold Currency Group", "Includes Gold, Silver, and Copper currencies"), installGoldCurrencyGroup);
            installArmorClasses = EditorGUILayout.Toggle(new GUIContent("Install Armor Classes", "Includes Plate, Leather, and Cloth armor classes"), installArmorClasses);
            installCharacterStats = EditorGUILayout.Toggle(new GUIContent("Install Character Stats", "Includes Stamina, Intellect, Strength, and Agility stats"), installCharacterStats);
            installItemQualities = EditorGUILayout.Toggle(new GUIContent("Install Item Qualities", "Includes Poor, Common, Uncommon, Rare, Epic, and Legendary Item Qualities"), installItemQualities);
            installPowerResources = EditorGUILayout.Toggle(new GUIContent("Install Power Resources", "Includes Health, Mana, Rage, and Energy resources"), installPowerResources);
            installUnitToughnesses = EditorGUILayout.Toggle(new GUIContent("Install Unit Toughnesses", "Includes 2/5/10/25 man group toughnesses, and minion/boss solo toughnesses"), installUnitToughnesses);
            installWeaponSkills = EditorGUILayout.Toggle(new GUIContent("Install Weapon Skills", "Includes animations, sounds, and hit effects for all included weapon types such as bow, sword, etc"), installWeaponSkills);

            return true;
        }
        
    }

    public enum DefaultPlayerUnitType { Mecanim, UMA }

    

}
