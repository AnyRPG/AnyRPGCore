using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class SystemGameManager : MonoBehaviour {

        private void Awake() {
            Init();
        }

        [Header("Configuration")]

        // configuration monobehavior
        [SerializeField]
        private SystemConfigurationManager systemConfigurationManager = null;

        [Header("Objects and Data")]

        [SerializeField]
        private SystemDataFactory systemDataFactory = null;

        [SerializeField]
        private ObjectPooler objectPooler = null;

        [Header("Monobehavior Managers")]

        // sub manager monobehaviors

        [SerializeField]
        private ControlsManager controlsManager = null;

        [SerializeField]
        private WindowManager windowManager = null;

        [SerializeField]
        private CameraManager cameraManager = null;

        [SerializeField]
        private AudioManager audioManager = null;

        [SerializeField]
        private PetPreviewManager petPreviewManager = null;

        [SerializeField]
        private UnitPreviewManager unitPreviewManager = null;

        [SerializeField]
        private CharacterPanelManager characterPanelManager = null;

        [SerializeField]
        private CharacterCreatorManager characterCreatorManager = null;

        [SerializeField]
        private SystemAbilityController systemAbilityController = null;

        [SerializeField]
        private CastTargettingManager castTargettingManager = null;

        [SerializeField]
        private InputManager inputManager = null;

        [SerializeField]
        private LevelManager levelManager = null;

        [SerializeField]
        private InventoryManager inventoryManager = null;

        [SerializeField]
        private PlayerManager playerManager = null;

        [SerializeField]
        private SystemItemManager systemItemManager = null;

        [SerializeField]
        private SystemAchievementManager systemAchievementManager = null;

        [SerializeField]
        private LogManager logManager = null;

        [SerializeField]
        private NewGameManager newGameManager = null;

        [SerializeField]
        private LoadGameManager loadGameManager = null;

        [SerializeField]
        private SaveManager saveManager = null;

        [SerializeField]
        private KeyBindManager keyBindManager = null;

        [SerializeField]
        private QuestLog questLog = null;

        [SerializeField]
        private SystemEnvironmentManager systemEnvironmentManager = null;

        [SerializeField]
        private CraftingManager craftingManager = null;

        [SerializeField]
        private InteractionManager interactionManager = null;

        [SerializeField]
        private LootManager lootManager = null;

        [SerializeField]
        private SystemPlayableDirectorManager systemPlayableDirectorManager = null;

        [SerializeField]
        private UIManager uIManager = null;

        [SerializeField]
        private CurrencyConverter currencyConverter = null;

        [SerializeField]
        private ChatCommandManager chatCommandManager = null;

        [SerializeField]
        private TimeOfDayManager timeOfDayManager = null;

        [SerializeField]
        private WeatherManager weatherManager = null;

        [SerializeField]
        private DialogManager dialogManager = null;

        [SerializeField]
        private ClassChangeManager classChangeManager = null;

        [SerializeField]
        private FactionChangeManager factionChangeManager = null;

        [SerializeField]
        private SpecializationChangeManager specializationChangeManager = null;

        [SerializeField]
        private MusicPlayerManager musicPlayerManager = null;

        [SerializeField]
        private NameChangeManager nameChangeManager = null;

        [SerializeField]
        private SkillTrainerManager skillTrainerManager = null;

        [SerializeField]
        private UnitSpawnManager unitSpawnManager = null;

        [SerializeField]
        private VendorManager vendorManager = null;

        // system scripts
        private SystemEventManager systemEventManager = null;

        // application state
        private int spawnCount = 0;
        private static bool isShuttingDown = false;

        public static bool IsShuttingDown { get => isShuttingDown; }

        public SystemEventManager SystemEventManager { get => systemEventManager; set => systemEventManager = value; }
        public SystemEnvironmentManager SystemEnvironmentManager { get => systemEnvironmentManager; set => systemEnvironmentManager = value; }
        public CraftingManager CraftingManager { get => craftingManager; set => craftingManager = value; }
        public InteractionManager InteractionManager { get => interactionManager; set => interactionManager = value; }
        public LootManager LootManager { get => lootManager; set => lootManager = value; }
        public SystemPlayableDirectorManager SystemPlayableDirectorManager { get => systemPlayableDirectorManager; set => systemPlayableDirectorManager = value; }
        public SaveManager SaveManager { get => saveManager; set => saveManager = value; }
        public KeyBindManager KeyBindManager { get => keyBindManager; set => keyBindManager = value; }
        public QuestLog QuestLog { get => questLog; set => questLog = value; }

        public SystemConfigurationManager SystemConfigurationManager { get => systemConfigurationManager; set => systemConfigurationManager = value; }
        public ControlsManager ControlsManager { get => controlsManager; }
        public WindowManager WindowManager { get => windowManager; set => windowManager = value; }
        public CameraManager CameraManager { get => cameraManager; set => cameraManager = value; }
        public AudioManager AudioManager { get => audioManager; set => audioManager = value; }
        public PetPreviewManager PetPreviewManager { get => petPreviewManager; set => petPreviewManager = value; }
        public UnitPreviewManager UnitPreviewManager { get => unitPreviewManager; set => unitPreviewManager = value; }
        public CharacterPanelManager CharacterPanelManager { get => characterPanelManager; set => characterPanelManager = value; }
        public CharacterCreatorManager CharacterCreatorManager { get => characterCreatorManager; set => characterCreatorManager = value; }
        public SystemAchievementManager SystemAchievementManager { get => systemAchievementManager; set => systemAchievementManager = value; }
        public UIManager UIManager { get => uIManager; set => uIManager = value; }
        public SystemAbilityController SystemAbilityController { get => systemAbilityController; set => systemAbilityController = value; }
        public CastTargettingManager CastTargettingManager { get => castTargettingManager; set => castTargettingManager = value; }
        public InputManager InputManager { get => inputManager; set => inputManager = value; }
        public LevelManager LevelManager { get => levelManager; set => levelManager = value; }
        public InventoryManager InventoryManager { get => inventoryManager; set => inventoryManager = value; }
        public PlayerManager PlayerManager { get => playerManager; set => playerManager = value; }
        public SystemItemManager SystemItemManager { get => systemItemManager; set => systemItemManager = value; }
        public LogManager LogManager { get => logManager; set => logManager = value; }
        public ObjectPooler ObjectPooler { get => objectPooler; set => objectPooler = value; }
        public SystemDataFactory SystemDataFactory { get => systemDataFactory; set => systemDataFactory = value; }
        public NewGameManager NewGameManager { get => newGameManager; set => newGameManager = value; }
        public LoadGameManager LoadGameManager { get => loadGameManager; set => loadGameManager = value; }
        public CurrencyConverter CurrencyConverter { get => currencyConverter; set => currencyConverter = value; }
        public ChatCommandManager ChatCommandManager { get => chatCommandManager; set => chatCommandManager = value; }
        public TimeOfDayManager TimeOfDayManager { get => timeOfDayManager; set => timeOfDayManager = value; }
        public WeatherManager WeatherManager { get => weatherManager; set => weatherManager = value; }
        public DialogManager DialogManager { get => dialogManager; set => dialogManager = value; }
        public ClassChangeManager ClassChangeManager { get => classChangeManager; set => classChangeManager = value; }
        public FactionChangeManager FactionChangeManager { get => factionChangeManager; set => factionChangeManager = value; }
        public SpecializationChangeManager SpecializationChangeManager { get => specializationChangeManager; set => specializationChangeManager = value; }
        public MusicPlayerManager MusicPlayerManager { get => musicPlayerManager; set => musicPlayerManager = value; }
        public NameChangeManager NameChangeManager { get => nameChangeManager; set => nameChangeManager = value; }
        public SkillTrainerManager SkillTrainerManager { get => skillTrainerManager; set => skillTrainerManager = value; }
        public UnitSpawnManager UnitSpawnManager { get => unitSpawnManager; set => unitSpawnManager = value; }
        public VendorManager VendorManager { get => vendorManager; set => vendorManager = value; }

        private void Init() {
            //Debug.Log("SystemGameManager.Init()");
            SetupPermanentObjects();

            // we are going to handle the initialization of all system managers here so we can control the start order and it isn't random
            // things are initialized here instead of in their declarations to prevent them from being initialized in the Unity Editor and restrict to play mode
            // initialize event manager first because everything else uses it
            systemEventManager = new SystemEventManager();

            // system data factory next for access to data resources
            SystemDataFactory.Configure(this);

            // configuration manager next because it will need access to resources from the factory
            systemConfigurationManager.Configure(this);

            // then everything else that relies on system configuration and data resources
            objectPooler.Configure(this);


            controlsManager.Configure(this);
            windowManager.Configure(this);
            cameraManager.Configure(this);
            //audioManager.Configure(this);
            petPreviewManager.Configure(this);
            unitPreviewManager.Configure(this);
            characterPanelManager.Configure(this);
            characterCreatorManager.Configure(this);
            systemAbilityController.Configure(this);
            castTargettingManager.Configure(this);
            inputManager.Configure(this);
            levelManager.Configure(this);
            inventoryManager.Configure(this);
            playerManager.Configure(this);
            systemItemManager.Configure(this);
            systemAchievementManager.Configure(this);
            logManager.Configure(this);
            newGameManager.Configure(this);
            loadGameManager.Configure(this);
            saveManager.Configure(this);
            KeyBindManager.Configure(this);
            questLog.Configure(this);
            systemEnvironmentManager.Configure(this);
            craftingManager.Configure(this);
            interactionManager.Configure(this);
            lootManager.Configure(this);
            systemPlayableDirectorManager.Configure(this);
            uIManager.Configure(this);
            currencyConverter.Configure(this);
            chatCommandManager.Configure(this);
            timeOfDayManager.Configure(this);
            weatherManager.Configure(this);
            dialogManager.Configure(this);
            classChangeManager.Configure(this);
            factionChangeManager.Configure(this);
            specializationChangeManager.Configure(this);
            musicPlayerManager.Configure(this);
            nameChangeManager.Configure(this);
            skillTrainerManager.Configure(this);
            unitSpawnManager.Configure(this);
            vendorManager.Configure(this);
        }

        private void SetupPermanentObjects() {
            //Debug.Log("SystemGameManager.SetupPermanentObjects()");
            DontDestroyOnLoad(this.gameObject);
            GameObject umaDCS = GameObject.Find("UMA_GLIB");
            if (umaDCS == null) {
                umaDCS = GameObject.Find("UMA_DCS");
                if (umaDCS != null) {
                    Debug.Log("SystemGameManager.SetupPermanentObjects(): UMA_DCS is deprecated.  Please replace the UMA_DCS prefab with the UMA_GLIB prefab");
                }
            }
            if (umaDCS == null) {
                //Debug.LogError("SystemGameManager.SetupPermanentObjects(): AnyRPG requires uma.  Ensure that the UMA_DCS prefab is in your loading scene.");
                Debug.Log("SystemGameManager.SetupPermanentObjects(): Neither UMA_GLIB nor UMA_DCS prefab could be found in the scene. UMA will be unavailable");
            } else {
                DontDestroyOnLoad(umaDCS);
            }
        }

        private void Start() {
            //Debug.Log("SystemGameManager.Start()");

            // due to "intended" but not officially documented behavior, audio updates will be overwritten if called in Awake() so they must be called in Start()
            // https://fogbugz.unity3d.com/default.asp?1197165_nik4gg1io942ae13#bugevent_1071843210

            audioManager.Configure(this);

            // first turn off the UI
            UIManager.PerformSetupActivities();

            // then launch level manager to start loading the game
            LevelManager.PerformSetupActivities();

        }

        public int GetSpawnCount() {
            spawnCount++;
            return spawnCount;
        }

        private void OnApplicationQuit() {
            //Debug.Log("SystemGameManager.OnApplicationQuit()");
            isShuttingDown = true;
        }

        /// <summary>
        /// configure all classes of type AutoConfiguredMonoBehavior in the scene
        /// </summary>
        public void AutoConfigureMonoBehaviours() {
            foreach (AutoConfiguredMonoBehaviour autoConfiguredMonoBehaviour in GameObject.FindObjectsOfType<AutoConfiguredMonoBehaviour>()) {
                autoConfiguredMonoBehaviour.Configure(this);
            }
        }

    }

}