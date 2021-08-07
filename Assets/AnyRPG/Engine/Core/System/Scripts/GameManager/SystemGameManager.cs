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

        [Header("Objects and Data")]

        [SerializeField]
        private SystemDataFactory systemDataFactory = null;

        [SerializeField]
        private ObjectPooler objectPooler = null;

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
        public LoadGameManager LoadGameManager { get => loadGameManager; set => loadGameManager = value; }

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
            systemEnvironmentManager.Configure(this);
            craftingManager.Configure(this);
            interactionManager.Configure(this);
            lootManager.Configure(this);
            systemPlayableDirectorManager.Configure(this);
            questLog.Configure(this);
            KeyBindManager.Configure(this);
            saveManager.Configure(this);
            cameraManager.Configure(this);
            audioManager.Configure(this);
            petPreviewManager.Configure(this);
            unitPreviewManager.Configure(this);
            characterCreatorManager.Configure(this);
            systemAchievementManager.Configure(this);
            systemAbilityController.Configure(this);
            castTargettingManager.Configure(this);
            inputManager.Configure(this);
            levelManager.Configure(this);
            inventoryManager.Configure(this);
            playerManager.Configure(this);
            systemItemManager.Configure(this);
            logManager.Configure(this);
            ObjectPooler.Configure(this);
            uIManager.Configure(this);

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