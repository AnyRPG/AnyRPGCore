using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class SystemGameManager : MonoBehaviour {

        #region Singleton
        private static SystemGameManager instance;

        public static SystemGameManager Instance {
            get {
                return instance;
            }
        }

        private void Awake() {
            instance = this;
            Init();
        }
        #endregion

        [Header("Configuration")]

        // configuration monobehavior
        [SerializeField]
        private SystemConfigurationManager systemConfigurationManager = null;

        [Header("Monobehavior Managers")]

        // sub manager monobehaviors
        [SerializeField]
        private CameraManager cameraManager = null;

        [SerializeField]
        private AudioManager audioManager = null;

        [SerializeField]
        private PetPreviewManager petPreviewManager = null;

        [SerializeField]
        private UnitPreviewManager unitPreviewManager = null;

        [SerializeField]
        private CharacterCreatorManager characterCreatorManager = null;

        [SerializeField]
        private UIManager uIManager = null;

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
        private ObjectPooler objectPooler = null;

        [SerializeField]
        private NewGameManager newGameManager = null;

        [Header("Data Resource Factory")]

        [SerializeField]
        private SystemDataFactory systemDataFactory = null;

        // system scripts
        private SystemEventManager systemEventManager = null;
        private SystemEnvironmentManager systemEnvironmentManager = null;
        private CraftingManager craftingManager = null;
        private InteractionManager interactionManager = null;
        private LootManager lootManager = null;
        private SystemPlayableDirectorManager systemPlayableDirectorManager = null;
        private SaveManager saveManager = null;
        private KeyBindManager keyBindManager = null;
        private QuestLog questLog = null;

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
        public CameraManager CameraManager { get => cameraManager; set => cameraManager = value; }
        public AudioManager AudioManager { get => audioManager; set => audioManager = value; }
        public PetPreviewManager PetPreviewManager { get => petPreviewManager; set => petPreviewManager = value; }
        public UnitPreviewManager UnitPreviewManager { get => unitPreviewManager; set => unitPreviewManager = value; }
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

        private void Init() {
            //Debug.Log("SystemGameManager.Init()");
            SetupPermanentObjects();

            // we are going to handle the initialization of all system managers here so we can control the start order and it isn't random
            // things are initialized here instead of in their declarations to prevent them from being initialized in the Unity Editor and restrict to play mode
            // initialize event manager first because everything else uses it
            systemEventManager = new SystemEventManager();

            // system data factory next for access to data resources
            SystemDataFactory.Init(this);

            // configuration manager next because it will need access to resources from the factory
            systemConfigurationManager.Init(this);

            // then everything else that relies on system configuration and data resources
            // starting with the non monobehavior managers
            systemEnvironmentManager = new SystemEnvironmentManager();
            craftingManager = new CraftingManager(this);
            interactionManager = new InteractionManager();
            lootManager = new LootManager(this);
            systemPlayableDirectorManager = new SystemPlayableDirectorManager();
            saveManager = new SaveManager(this);
            keyBindManager = new KeyBindManager(this);
            questLog = new QuestLog(this);

            // and finally monobehavior managers
            cameraManager.Init(this);
            audioManager.Init(this);
            petPreviewManager.Init(this);
            unitPreviewManager.Init(this);
            characterCreatorManager.Init(this);
            systemAchievementManager.Init(this);

            systemAbilityController.Init(this);
            castTargettingManager.Init(this);
            inputManager.Init(this);
            levelManager.Init(this);
            inventoryManager.Init(this);
            playerManager.Init(this);
            systemItemManager.Init(this);
            logManager.Init(this);
            ObjectPooler.Init(this);
            uIManager.Init(this);

        }

        private void SetupPermanentObjects() {
            DontDestroyOnLoad(this.gameObject);
            GameObject umaDCS = GameObject.Find("UMA_DCS");
            if (umaDCS == null) {
                //Debug.LogError("SystemGameManager.SetupPermanentObjects(): AnyRPG requires uma.  Ensure that the UMA_DCS prefab is in your loading scene.");
                Debug.Log("SystemGameManager.SetupPermanentObjects(): UMA_DCS prefab not found in scene. UMA will be unavailable");
            } else {
                DontDestroyOnLoad(umaDCS);
            }
        }

        private void Start() {
            //Debug.Log("SystemGameManager.Start()");

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

    }

}