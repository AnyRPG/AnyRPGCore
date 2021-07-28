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

        [SerializeField]
        private GameObject resourceManagerParent = null;

        private List<SystemResourceManager> systemResourceManagers = new List<SystemResourceManager>();

        // event manager first because everything else will use it for subscriptions
        private SystemEventManager systemEventManager = new SystemEventManager();

        // system scripts
        private SystemEnvironmentManager systemEnvironmentManager = new SystemEnvironmentManager();
        private CraftingManager craftingManager = new CraftingManager();
        private InteractionManager interactionManager = new InteractionManager();
        private LootManager lootManager = new LootManager();
        private SystemPlayableDirectorManager systemPlayableDirectorManager = new SystemPlayableDirectorManager();
        private SaveManager saveManager = null;
        private KeyBindManager keyBindManager = new KeyBindManager();

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

        private void Init() {
            //Debug.Log("SystemGameManager.Awake()");
            SetupPermanentObjects();

            // initialize event manager first because everything else uses it
            //systemEventManager = new SystemEventManager();

            //systemEnvironmentManager = new SystemEnvironmentManager();
            //craftingManager = new CraftingManager();
            //interactionManager = new InteractionManager();
            saveManager = new SaveManager();
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
            UIManager.Instance.PerformSetupActivities();

            // next, load scriptable object resources
            LoadResources();

            // next, verify systemconfiguration manager references to resources
            SystemConfigurationManager.Instance.SetupScriptableObjects();

            PlayerManager.Instance.OrchestratorStart();

            // then launch level manager to start loading the game
            LevelManager.Instance.PerformSetupActivities();

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