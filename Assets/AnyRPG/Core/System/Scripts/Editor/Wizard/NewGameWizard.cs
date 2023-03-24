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

        // Template scene that will be used to create the new game load scene
        private const string pathToLoadSceneTemplate = "/AnyRPG/Core/Templates/Game/Scenes/LoadScene/LoadScene.unity";
        private const string pathToMainMenuSceneTemplate = "/AnyRPG/Core/Templates/Game/Scenes/MainMenu/MainMenu.unity";

        // required template prefabs
        private const string pathToGameManagerPrefab = "/AnyRPG/Core/System/Prefabs/GameManager/GameManager.prefab";
        private const string pathToSceneConfigPrefab = "/AnyRPG/Core/System/Prefabs/GameManager/SceneConfig.prefab";
        private const string pathToUMAGLIBPrefab = "/UMA/Getting Started/UMA_GLIB.prefab";

        private const string pathToSystemAbilitiesTemplate = "/AnyRPG/Core/Content/TemplatePackages/AbilityEffect/DefaultSystemEffectsTemplatePackage.asset";
        private const string pathToHealthPowerResourceTemplate = "/AnyRPG/Core/Content/TemplatePackages/PowerResource/HealthPowerResourceTemplatePackage.asset";
        private const string pathToAttackAbilityTemplate = "/AnyRPG/Core/Content/TemplatePackages/BaseAbility/Attack/AttackAbilityTemplatePackage.asset";
        private const string pathToPlayerUnitsTemplate = "/AnyRPG/Core/Content/TemplatePackages/UnitProfile/Player/DefaultPlayerUnitsTemplatePackage.asset";

        // optional template prefabs
        private const string pathToGoldCurrencyGroupTemplate = "/AnyRPG/Core/Content/TemplatePackages/Currency/GoldCurrencyGroupTemplatePackage.asset";
        private const string pathToArmorClassesTemplate = "/AnyRPG/Core/Content/TemplatePackages/ArmorClass/AllArmorClassesTemplatePackage.asset";
        private const string pathToItemQualitiesTemplate = "/AnyRPG/Core/Content/TemplatePackages/ItemQuality/AllItemQualitiesTemplatePackage.asset";
        private const string pathToPowerResourcesTemplate = "/AnyRPG/Core/Content/TemplatePackages/PowerResource/AllPowerResourcesTemplatePackage.asset";
        private const string pathToCharacterStatsTemplate = "/AnyRPG/Core/Content/TemplatePackages/CharacterStat/AllCharacterStatsTemplatePackage.asset";
        private const string pathToUnitToughnessesTemplate = "/AnyRPG/Core/Content/TemplatePackages/UnitToughness/AllUnitToughnessesTemplatePackage.asset";
        private const string pathToWeaponSkillsTemplate = "/AnyRPG/Core/Content/TemplatePackages/WeaponSkill/AllWeaponSkillsTemplatePackage.asset";
        private const string pathToFootstepSoundsTemplate = "/AnyRPG/Core/Content/TemplatePackages/AudioProfile/FootstepSoundEffectsTemplatePackage.asset";
        private const string pathToWeatherProfilesTemplate = "/AnyRPG/Core/Content/TemplatePackages/WeatherProfile/AllWeatherProfilesTemplatePackage.asset";
        private const string pathToHumanVoiceProfilesTemplate = "/AnyRPG/Core/Content/TemplatePackages/VoiceProfile/CoreVoiceProfilesTemplatePackage.asset";

        // sill be a subfolder of Application.dataPath and should start with "/"
        private const string gameParentFolder = "/Games/";

        private List<string> resourceFolders = new List<string>() {
            "AbilityEffect",
            "Achievement",
            "AnimatedAction",
            "AnimationProfile",
            "ArmorClass",
            "AudioProfile",
            "BaseAbility",
            "BehaviorProfile",
            "CharacterClass",
            "CharacterRace",
            "CharacterStat",
            "ChatCommand",
            "ClassSpecialization",
            "CombatStrategy",
            "Currency",
            "CurrencyGroup",
            "Cutscene",
            "Dialog",
            "EnvironmentStateProfile",
            "EquipmentSet",
            "Faction",
            "InteractableOptionConfig",
            "Item",
            "ItemQuality",
            "LootTable",
            "MaterialProfile",
            "PatrolProfile",
            "PowerResource",
            "PrefabProfile",
            "Quest",
            "QuestGiverProfile",
            "Recipe",
            "SceneNode",
            "Skill",
            "StatusEffectGroup",
            "StatusEffectType",
            "SwappableMeshModelProfile",
            "UMARecipeProfile",
            "UnitProfile",
            "UnitToughness",
            "UnitType",
            "VendorCollection",
            "VoiceProfile",
            "WeaponSkill",
            "WeatherProfile"
        };

        // compare the default first scene directory to any user picked scene name
        //private const string templateFirstSceneName = "FirstScene";

        // the used file path name for the game
        //private string fileSystemGameName = string.Empty;
        //private string fileSystemFirstSceneName = string.Empty;

        // #######################
        // USER MODIFIED VARIABLES
        // #######################

        public string gameName = "New Game";
        public string gameVersion = "0.1a";

        // main menu options
        public AudioClip mainMenuMusic = null;

        public AudioClip newGameMusic = null;

        // first scene options
        public bool copyExistingScene = false;
        public string firstSceneName = "New Scene";

        public SceneAsset existingScene = null;

        public AudioClip firstSceneDayAmbientSounds = null;

        public AudioClip firstSceneNightAmbientSounds = null;

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
        public bool installFootstepSounds = true;
        public bool installWeatherProfiles = true;
        public bool installHumanVoiceProfiles = true;

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

            SetNewGameTitle();
            firstSceneName = NewSceneWizard.GetNewSceneTitle(firstSceneName);
        }

        void OnWizardCreate() {

            string gameLoadScenePath = string.Empty;

            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);

            try {
                gameLoadScenePath = CreateNewGame();
            } catch {
                Debug.LogWarning("Error detected while running wizard");

                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("New Game Wizard", "New Game Wizard encountered an error.  Check the console log for details.", "OK");
                Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.ScriptOnly);
                throw;
            }

            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("New Game Wizard", "New Game Wizard Complete! The game loading scene can be found at " + gameLoadScenePath, "OK");
            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.ScriptOnly);

        }

        private void SetNewGameTitle() {

            // attempt to create unique game name if the default is already used
            string testGameName = gameName;
            string newGameFolder = WizardUtilities.GetGameFileSystemFolder(gameParentFolder, WizardUtilities.GetFileSystemGameName(testGameName));

            if (GameFolderExists(newGameFolder)) {
                for (int i = 2; i < 100; i++) {
                    testGameName = gameName + " " + i.ToString();
                    newGameFolder = WizardUtilities.GetGameFileSystemFolder(gameParentFolder, WizardUtilities.GetFileSystemGameName(testGameName));
                    if (GameFolderExists(newGameFolder) == false) {
                        gameName = testGameName;
                        break;
                    }
                }
            }
        }

        private string CreateNewGame() {

            EditorUtility.DisplayProgressBar("New Game Wizard", "Checking parameters...", 0.1f);

            string fileSystemGameName = WizardUtilities.GetFileSystemGameName(gameName);
            //fileSystemFirstSceneName = WizardUtilities.GetFilesystemSceneName(firstSceneName);

            // check for presence of template prefabs and resources
            if (CheckFilesExist() == false) {
                return string.Empty;
            }

            // check that the templates used by the new scene wizard exist
            if (NewSceneWizard.CheckRequiredTemplatesExist() == false) {
                return string.Empty;
            }

            // Set default values of game properties just in case they are somehow missing
            if (gameVersion == null || gameVersion.Trim() == "") {
                gameVersion = "0.1a";
                Debug.Log("Empty game version.  Defaulting to " + gameVersion);
            }

            // Create root game folder
            EditorUtility.DisplayProgressBar("New Game Wizard", "Creating Game Folder...", 0.2f);
            string fileSystemNewGameFolder = WizardUtilities.GetGameFileSystemFolder(gameParentFolder, fileSystemGameName);
            WizardUtilities.CreateFolderIfNotExists(fileSystemNewGameFolder);

            // create resources folders
            EditorUtility.DisplayProgressBar("New Game Wizard", "Creating Resources Folders...", 0.3f);
            string fileSystemResourcesFolder = fileSystemNewGameFolder + "/Resources/" + fileSystemGameName;
            WizardUtilities.CreateFolderIfNotExists(fileSystemResourcesFolder);
            foreach (string resourceFolder in resourceFolders) {
                WizardUtilities.CreateFolderIfNotExists(fileSystemResourcesFolder + "/" + resourceFolder);
            }

            AssetDatabase.Refresh();

            // copy game load scene
            EditorUtility.DisplayProgressBar("New Game Wizard", "Copying Load Scene...", 0.4f);
            string loadSceneFolder = gameParentFolder + fileSystemGameName + "/Scenes/" + fileSystemGameName;
            string loadSceneAssetPath = "Assets" + loadSceneFolder + "/" + fileSystemGameName + ".unity";
            WizardUtilities.CreateFolderIfNotExists(Application.dataPath + loadSceneFolder);
            AssetDatabase.CopyAsset("Assets" + pathToLoadSceneTemplate, loadSceneAssetPath);

            // add game load scene to build settings
            EditorUtility.DisplayProgressBar("New Game Wizard", "Adding Game Load Scene To Build Settings...", 0.45f);
            List<EditorBuildSettingsScene> currentSceneList = EditorBuildSettings.scenes.ToList();
            Debug.Log("Adding " + loadSceneAssetPath + " to build settings");
            currentSceneList.Add(new EditorBuildSettingsScene(loadSceneAssetPath, true));
            EditorBuildSettings.scenes = currentSceneList.ToArray();

            // copy main mneu scene
            EditorUtility.DisplayProgressBar("New Game Wizard", "Copying Main Menu Scene...", 0.5f);
            string mainMenuSceneFolder = gameParentFolder + fileSystemGameName + "/Scenes/" + fileSystemGameName + "MainMenu";
            string mainMenuSceneAssetPath = "Assets" + mainMenuSceneFolder + "/" + fileSystemGameName + "MainMenu.unity";
            WizardUtilities.CreateFolderIfNotExists(Application.dataPath + mainMenuSceneFolder);
            AssetDatabase.CopyAsset("Assets" + pathToMainMenuSceneTemplate, mainMenuSceneAssetPath);

            // add main menu scene to build settings
            EditorUtility.DisplayProgressBar("New Game Wizard", "Adding Main Menu Scene To Build Settings...", 0.55f);
            currentSceneList = EditorBuildSettings.scenes.ToList();
            Debug.Log("Adding " + mainMenuSceneAssetPath + " to build settings");
            currentSceneList.Add(new EditorBuildSettingsScene(mainMenuSceneAssetPath, true));
            EditorBuildSettings.scenes = currentSceneList.ToArray();

            // Open the load scene to add the necessary elements
            EditorUtility.DisplayProgressBar("New Game Wizard", "Modifying loading scene...", 0.6f);
            Debug.Log("Loading Scene at " + loadSceneAssetPath);
            Scene loadGameScene = EditorSceneManager.OpenScene(loadSceneAssetPath);

            // create prefab folder
            string fileSystemPrefabFolder = fileSystemNewGameFolder + "/Prefab";
            string prefabPath = FileUtil.GetProjectRelativePath(fileSystemPrefabFolder);
            WizardUtilities.CreateFolderIfNotExists(fileSystemPrefabFolder + "/GameManager");

            if (useThirdPartyController == true) {
                ConfigureThirdPartyController(fileSystemGameName, fileSystemResourcesFolder, fileSystemPrefabFolder);
            }

            // Create a variant of the GameManager
            EditorUtility.DisplayProgressBar("New Game Wizard", "Making prefab variants...", 0.7f);
            GameObject gameManagerGameObject = (GameObject)AssetDatabase.LoadMainAssetAtPath("Assets" + pathToGameManagerPrefab);
            GameObject gameManagerVariant = MakeGameManagerPrefabVariant(fileSystemGameName, gameManagerGameObject, prefabPath + "/GameManager/" + fileSystemGameName + "GameManager.prefab");

            // create a variant of the UMA GLIB prefab
            MakeUMAPrefabVariant(prefabPath + "/GameManager/UMA_GLIB.prefab", fileSystemGameName);

            // create a variant of the SceneConfig prefab
            MakeSceneConfigPrefabVariant(fileSystemGameName, gameManagerVariant, prefabPath + "/GameManager/" + fileSystemGameName + "SceneConfig.prefab");

            // Save changes to the load game scene
            EditorUtility.DisplayProgressBar("New Game Wizard", "Saving Load Game Scene...", 0.8f);
            EditorSceneManager.SaveScene(loadGameScene);

            // Open the main menu scene to add the scene config
            EditorUtility.DisplayProgressBar("New Game Wizard", "Modifying main menu scene...", 0.85f);
            Debug.Log("Loading Scene at " + mainMenuSceneAssetPath);
            Scene mainMenuScene = EditorSceneManager.OpenScene(mainMenuSceneAssetPath);

            // add sceneconfig to scene
            EditorUtility.DisplayProgressBar("New Game Wizard", "Adding SceneConfig to Scene...", 0.87f);
            string sceneConfigPrefabAssetPath = "Assets" + gameParentFolder + fileSystemGameName + "/Prefab/GameManager/" + fileSystemGameName + "SceneConfig.prefab";
            GameObject sceneConfigGameObject = (GameObject)AssetDatabase.LoadMainAssetAtPath(sceneConfigPrefabAssetPath);
            GameObject instantiatedGO = (GameObject)PrefabUtility.InstantiatePrefab(sceneConfigGameObject);
            instantiatedGO.transform.SetAsFirstSibling();
            EditorSceneManager.SaveScene(mainMenuScene);

            // install default templates
            EditorUtility.DisplayProgressBar("New Game Wizard", "Installing Default Templates...", 0.9f);
            InstallDefaultTemplateContent(fileSystemGameName, gameParentFolder);

            // install optional templates
            EditorUtility.DisplayProgressBar("New Game Wizard", "Installing Optional Templates...", 0.95f);
            InstallOptionalTemplateContent(fileSystemGameName, gameParentFolder);

            // make audio profiles and scene nodes for main menu
            EditorUtility.DisplayProgressBar("New Game Wizard", "Configuring Main Menu...", 0.99f);
            ConfigureMainMenuScriptableObjects(fileSystemGameName);

            // create first scene
            NewSceneWizard.CreateScene(gameParentFolder, gameName, firstSceneName, copyExistingScene, existingScene, firstSceneDayAmbientSounds, firstSceneNightAmbientSounds, firstSceneMusic);

            AssetDatabase.Refresh();

            return loadSceneAssetPath;
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

            if (installFootstepSounds) {
                contentTemplates.Add((ScriptableContentTemplate)AssetDatabase.LoadMainAssetAtPath("Assets" + pathToFootstepSoundsTemplate));
            }

            if (installWeatherProfiles) {
                contentTemplates.Add((ScriptableContentTemplate)AssetDatabase.LoadMainAssetAtPath("Assets" + pathToWeatherProfilesTemplate));
            }

            if (installHumanVoiceProfiles) {
                contentTemplates.Add((ScriptableContentTemplate)AssetDatabase.LoadMainAssetAtPath("Assets" + pathToHumanVoiceProfilesTemplate));
            }

            if (contentTemplates.Count > 0) {
                TemplateContentWizard.RunWizard(fileSystemGameName, newGameParentFolder, contentTemplates, true, true);
            }

        }

        private bool CheckFilesExist() {

            // Check for presence of Load Scene template
            if (WizardUtilities.CheckFileExists(pathToLoadSceneTemplate, "Load Scene Template") == false) {
                return false;
            }

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

            // check for presence of the footstep sounds template
            if (WizardUtilities.CheckFileExists(pathToFootstepSoundsTemplate, "Footstep Sounds Template") == false) {
                return false;
            }

            // check for presence of the weather profiles template
            if (WizardUtilities.CheckFileExists(pathToWeatherProfilesTemplate, "Weather Profiles Template") == false) {
                return false;
            }

            // check for presence of the human voice profiles template
            if (WizardUtilities.CheckFileExists(pathToHumanVoiceProfilesTemplate, "Human Voice Profiles Template") == false) {
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

                string scriptableObjectPath = "Assets" + gameParentFolder + fileSystemGameName + "/Resources/" + fileSystemGameName + "/AudioProfile/MainMenuAudio.asset";
                AssetDatabase.CreateAsset(audioProfile, scriptableObjectPath);
            }

            if (newGameMusic != null) {
                AudioProfile audioProfile = ScriptableObject.CreateInstance("AudioProfile") as AudioProfile;
                audioProfile.ResourceName = "New Game";
                audioProfile.AudioClips = new List<AudioClip>() { newGameMusic };

                string scriptableObjectPath = "Assets" + gameParentFolder + fileSystemGameName + "/Resources/" + fileSystemGameName + "/AudioProfile/NewGameAudio.asset";
                AssetDatabase.CreateAsset(audioProfile, scriptableObjectPath);
            }

            // create scene node
            SceneNode sceneNode = ScriptableObject.CreateInstance("SceneNode") as SceneNode;
            sceneNode.ResourceName = "Main Menu";
            sceneNode.SceneFile = $"{fileSystemGameName}MainMenu";
            sceneNode.AllowMount = false;
            sceneNode.SuppressCharacterSpawn = true;
            if (mainMenuMusic != null) {
                sceneNode.BackgroundMusicProfileName = "Main Menu";
            }

            string sceneNodeObjectPath = "Assets" + gameParentFolder + fileSystemGameName + "/Resources/" + fileSystemGameName + "/SceneNode/MainMenuSceneNode.asset";
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
            string newGameFolder = WizardUtilities.GetGameFileSystemFolder(gameParentFolder, fileSystemGameName);
            if (GameFolderExists(newGameFolder)) {
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
            if (NewSceneWizard.SceneExists(filesystemSceneName)) {
                return "A scene with the name " + filesystemSceneName + " already exists in the build settings. Please choose a unique first scene name.";
            }

            return null;
        }

        private bool GameFolderExists(string newGameFolder) {
            if (System.IO.Directory.Exists(newGameFolder)) {
                return true;
            }

            return false;
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

            firstSceneDayAmbientSounds = EditorGUILayout.ObjectField("Day Ambient Sounds", firstSceneDayAmbientSounds, typeof(AudioClip), false) as AudioClip;
            firstSceneNightAmbientSounds = EditorGUILayout.ObjectField("Night Ambient Sounds", firstSceneNightAmbientSounds, typeof(AudioClip), false) as AudioClip;
            firstSceneMusic = EditorGUILayout.ObjectField("Background Music", firstSceneMusic, typeof(AudioClip), false) as AudioClip;

            EditorGUILayout.LabelField("Common RPG Building Blocks", EditorStyles.boldLabel);

            installGoldCurrencyGroup = EditorGUILayout.Toggle(new GUIContent("Install Gold Currency Group", "Includes Gold, Silver, and Copper currencies"), installGoldCurrencyGroup);
            installArmorClasses = EditorGUILayout.Toggle(new GUIContent("Install Armor Classes", "Includes Plate, Leather, and Cloth armor classes"), installArmorClasses);
            installCharacterStats = EditorGUILayout.Toggle(new GUIContent("Install Character Stats", "Includes Stamina, Intellect, Strength, and Agility stats"), installCharacterStats);
            installItemQualities = EditorGUILayout.Toggle(new GUIContent("Install Item Qualities", "Includes Poor, Common, Uncommon, Rare, Epic, and Legendary Item Qualities"), installItemQualities);
            installPowerResources = EditorGUILayout.Toggle(new GUIContent("Install Power Resources", "Includes Health, Mana, Rage, and Energy resources"), installPowerResources);
            installUnitToughnesses = EditorGUILayout.Toggle(new GUIContent("Install Unit Toughnesses", "Includes 2/5/10/25 man group toughnesses, and minion/boss solo toughnesses"), installUnitToughnesses);
            installWeaponSkills = EditorGUILayout.Toggle(new GUIContent("Install Weapon Skills", "Includes animations, sounds, and hit effects for all included weapon types such as bow, sword, etc"), installWeaponSkills);
            installFootstepSounds = EditorGUILayout.Toggle(new GUIContent("Install Footstep Sounds", "Includes many different types of footstep sounds including gravel, sand, snow, etc"), installFootstepSounds);
            installWeatherProfiles = EditorGUILayout.Toggle(new GUIContent("Install Weather", "Includes snow, rain, and fog weather"), installWeatherProfiles);
            installHumanVoiceProfiles = EditorGUILayout.Toggle(new GUIContent("Install Human Voices", "Includes multiple types of male and female voices"), installHumanVoiceProfiles);

            return true;
        }
        
    }

    public enum DefaultPlayerUnitType { Mecanim, UMA }

    

}
