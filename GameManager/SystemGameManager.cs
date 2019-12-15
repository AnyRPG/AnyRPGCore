using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class SystemGameManager : MonoBehaviour {

        #region Singleton
        private static SystemGameManager instance;

        public static SystemGameManager MyInstance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<SystemGameManager>();
                }

                return instance;
            }
        }
        #endregion

        [SerializeField]
        private GameObject resourceManagerParent = null;

        private List<SystemResourceManager> systemResourceManagers = new List<SystemResourceManager>();

        private int spawnCount = 0;

        private bool eventSubscriptionsInitialized = false;

        private void Awake() {
            //Debug.Log("SystemGameManager.Awake()");
        }

        private void Start() {
            //Debug.Log("SystemGameManager.Start()");

            // we are going to handle the initialization of all system managers here so we can control the start order and it isn't random

            // first turn off the UI
            UIManager.MyInstance.PerformSetupActivities();

            // next, load scriptable object resources
            LoadResources();

            // next, verify systemconfiguration manager references to resources
            SystemConfigurationManager.MyInstance.VerifySystemAbilities();

            PlayerManager.MyInstance.OrchestratorStart();

            // subscribe to player connection despawn events for reloading resources
            CreateEventSubscriptions();

            // then launch level manager to start loading the game
            LevelManager.MyInstance.PerformSetupActivities();

        }

        public virtual void CreateEventSubscriptions() {
            //Debug.Log("PlayerManager.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized) {
                return;
            }
            SystemEventManager.MyInstance.OnPlayerConnectionDespawn += ReloadResourceLists;
            eventSubscriptionsInitialized = true;
        }

        public virtual void CleanupEventSubscriptions() {
            //Debug.Log("PlayerManager.CleanupEventSubscriptions()");
            if (!eventSubscriptionsInitialized) {
                return;
            }
            SystemEventManager.MyInstance.OnPlayerConnectionDespawn -= ReloadResourceLists;
            eventSubscriptionsInitialized = false;
        }

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

    }

}