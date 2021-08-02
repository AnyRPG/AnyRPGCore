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

        [Header("Resource Managers")]

        [SerializeField]
        private GameObject resourceManagerParent = null;

        [SerializeField]
        private SystemDataFactory systemDataFactory = null;

        private List<SystemResourceManager> systemResourceManagers = new List<SystemResourceManager>();

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

        public SystemEventManager EventManager { get => systemEventManager; set => systemEventManager = value; }
        public SystemEnvironmentManager SystemEnvironmentManager { get => systemEnvironmentManager; set => systemEnvironmentManager = value; }
        public CraftingManager CraftingManager { get => craftingManager; set => craftingManager = value; }
        public InteractionManager InteractionManager { get => interactionManager; set => interactionManager = value; }
        public LootManager LootManager { get => lootManager; set => lootManager = value; }
        public SystemPlayableDirectorManager SystemPlayableDirectorManager { get => systemPlayableDirectorManager; set => systemPlayableDirectorManager = value; }
        public SaveManager SaveManager { get => saveManager; set => saveManager = value; }
        public KeyBindManager KeyBindManager { get => keyBindManager; set => keyBindManager = value; }
        public CameraManager CameraManager { get => cameraManager; set => cameraManager = value; }
        public AudioManager AudioManager { get => audioManager; set => audioManager = value; }
        public PetPreviewManager PetPreviewManager { get => petPreviewManager; set => petPreviewManager = value; }
        public UnitPreviewManager UnitPreviewManager { get => unitPreviewManager; set => unitPreviewManager = value; }
        public CharacterCreatorManager CharacterCreatorManager { get => characterCreatorManager; set => characterCreatorManager = value; }
        public UIManager UIManager { get => uIManager; set => uIManager = value; }
        public SystemConfigurationManager SystemConfigurationManager { get => systemConfigurationManager; set => systemConfigurationManager = value; }
        public SystemAbilityController SystemAbilityController { get => systemAbilityController; set => systemAbilityController = value; }
        public CastTargettingManager CastTargettingManager { get => castTargettingManager; set => castTargettingManager = value; }
        public QuestLog QuestLog { get => questLog; set => questLog = value; }
        public InputManager InputManager { get => inputManager; set => inputManager = value; }
        public LevelManager LevelManager { get => levelManager; set => levelManager = value; }
        public InventoryManager InventoryManager { get => inventoryManager; set => inventoryManager = value; }
        public PlayerManager PlayerManager { get => playerManager; set => playerManager = value; }
        public SystemItemManager SystemItemManager { get => systemItemManager; set => systemItemManager = value; }
        public SystemAchievementManager SystemAchievementManager { get => systemAchievementManager; set => systemAchievementManager = value; }

        private void Init() {
            //Debug.Log("SystemGameManager.Init()");
            SetupPermanentObjects();

            // things are initialized here instead of in their declarations to prevent them from being initialized in the Unity Editor and restrict to play mode
            // initialize event manager first because everything else uses it
            systemEventManager = new SystemEventManager();

            systemEnvironmentManager = new SystemEnvironmentManager();
            craftingManager = new CraftingManager();
            interactionManager = new InteractionManager();
            lootManager = new LootManager();
            systemPlayableDirectorManager = new SystemPlayableDirectorManager();
            saveManager = new SaveManager();
            keyBindManager = new KeyBindManager();
            questLog = new QuestLog();

            // sub manager monobehaviors
            cameraManager.Init();
            audioManager.Init();
            petPreviewManager.Init();
            unitPreviewManager.Init();
            characterCreatorManager.Init();
            systemAchievementManager.Init();

            systemAbilityController.Init();
            castTargettingManager.Init();
            inputManager.Init();
            inventoryManager.Init();
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

            // we are going to handle the initialization of all system managers here so we can control the start order and it isn't random

            // first turn off the UI
            SystemGameManager.Instance.UIManager.PerformSetupActivities();

            // next, load scriptable object resources
            LoadResources();

            // next, verify systemconfiguration manager references to resources
            SystemGameManager.Instance.SystemConfigurationManager.SetupScriptableObjects();

            SystemGameManager.Instance.PlayerManager.OrchestratorStart();

            // then launch level manager to start loading the game
            SystemGameManager.Instance.LevelManager.PerformSetupActivities();

        }

        /// <summary>
        /// this function is not currently in use because objects subscribe to events on these so clearing them breaks the event subscriptions
        /// also, there should no longer be mutable properties on these so there should be no need to reload them
        /// </summary>
        public void ReloadResourceLists() {
            //Debug.Log("SystemGameManager.ReloadResourceLists()");

            // this has to be done in two loops to avoid invalidating scriptableobject references as we clear the lists to reload them
            foreach (SystemResourceManager systemResourceManager in systemResourceManagers) {
                systemResourceManager.ReloadResourceList();
            }
            foreach (SystemResourceManager systemResourceManager in systemResourceManagers) {
                systemResourceManager.SetupScriptableObjects();
            }

        }

        public void LoadResources() {
            //Debug.Log("SystemGameManager.LoadResources()");

            // load all resource managers into a list and get them to load their scriptableobjects from disk
            SystemResourceManager[] systemResourceManagerArray = resourceManagerParent.GetComponents<SystemResourceManager>();
            foreach (SystemResourceManager systemResourceManager in systemResourceManagerArray) {
                //Debug.Log("SystemGameManager.LoadResources(): found a child: " + child.name);
                //SystemResourceManager systemResourceManager = child.GetComponent<SystemResourceManager>();
                if (systemResourceManager != null) {
                    systemResourceManagers.Add(systemResourceManager);
                    systemResourceManager.LoadResourceList();
                }
            }

            systemDataFactory.SetupFactory();

            // give each resource manager a chance to loop through their scriptableOjects and create references to other scriptableOjects to avoid costly
            // and repetitive runtime lookups
            foreach (SystemResourceManager systemResourceManager in systemResourceManagers) {
                systemResourceManager.SetupScriptableObjects();
            }
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