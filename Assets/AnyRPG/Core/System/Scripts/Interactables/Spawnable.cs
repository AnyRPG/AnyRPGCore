using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class Spawnable : AutoConfiguredMonoBehaviour, IPrerequisiteOwner {

        [Header("Spawn Object")]

        [Tooltip("The name of the prefab profile to use.  The object referenced in the prefab profile will be spawned.")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(PrefabProfile))]
        protected string prefabProfileName = string.Empty;

        protected PrefabProfile prefabProfile;

        [Header("Spawn Control")]

        [Tooltip("The amount of time to delay spawn once all the prerequisites are met and the object can spawn")]
        [SerializeField]
        protected float spawnDelay = 0f;

        [Tooltip("Set this to a static object to prevent the object in the prefabprofile from spawning")]
        [SerializeField]
        protected GameObject spawnReference = null;

        [Tooltip("if there is an object spawned, and the prerequisite conditions are no longer met, despawn it")]
        [SerializeField]
        private bool despawnObject = false;

        [Tooltip("Game conditions that must be satisfied for the object to spawn")]
        [SerializeField]
        protected List<PrerequisiteConditions> prerequisiteConditions = new List<PrerequisiteConditions>();

        // state management
        protected bool startHasRun = false;

        // get references step tracker
        protected bool componentReferencesInitialized = false;

        // initialize step tracker
        protected bool initialized = false;

        // subscriptions step tracker
        protected bool eventSubscriptionsInitialized = false;

        // game manager references

        protected PlayerManager playerManager = null;
        protected SystemDataFactory systemDataFactory = null;
        protected ObjectPooler objectPooler = null;

        public GameObject SpawnReference { get => spawnReference; set => spawnReference = value; }
        public PrefabProfile PrefabProfile { get => prefabProfile; set => prefabProfile = value; }

        public virtual bool PrerequisitesMet {
            get {
                //Debug.Log(gameObject.name + ".Spawnable.MyPrerequisitesMet");
                foreach (PrerequisiteConditions prerequisiteCondition in prerequisiteConditions) {
                    if (!prerequisiteCondition.IsMet()) {
                        return false;
                    }
                }
                // there are no prerequisites, or all prerequisites are complete
                return true;
            }
        }

        public override void Configure(SystemGameManager systemGameManager) {
            //Debug.Log(gameObject.name + ".Spawnable.Configure()");
            base.Configure(systemGameManager);

            GetComponentReferences();
            SetupScriptableObjects();
            CreateEventSubscriptions();
            if (playerManager.PlayerUnitSpawned == false) {
                // this allows us to spawn things with no prerequisites that don't need to check against the player
                PrerequisiteCheck();
            }
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            playerManager = systemGameManager.PlayerManager;
            systemDataFactory = systemGameManager.SystemDataFactory;
            objectPooler = systemGameManager.ObjectPooler;
        }

        public virtual void Init() {
            if (initialized == true) {
                return;
            }
            ProcessInit();

            // moved here from CreateEventSubscriptions.  Init should have time to occur before processing this
            if (playerManager.PlayerUnitSpawned) {
                //Debug.Log(gameObject.name + ".Spawnable.CreateEventSubscriptions(): Player Unit is spawned.  Handling immediate spawn!");
                ProcessPlayerUnitSpawn();
            } else {
                //Debug.Log(gameObject.name + ".Spawnable.CreateEventSubscriptions(): Player Unit is not spawned. Added Handle Spawn listener");
            }
            startHasRun = true;
            initialized = true;
        }

        public virtual void ProcessInit() {
            // do nothing here
        }

        protected virtual void Start() {
            //Debug.Log(gameObject.name + ".Spawnable.Start()");
            if (systemGameManager == null) {
                Debug.LogError(gameObject.name + ": SystemGameManager not found. Is the Game Manager in the scene?");
                return;
            }

            Init();
        }

        public void CreateEventSubscriptions() {
            //Debug.Log(gameObject.name + ".Spawnable.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized) {
                return;
            }
            ProcessCreateEventSubscriptions();
            eventSubscriptionsInitialized = true;
        }

        public virtual void ProcessCreateEventSubscriptions() {
            SystemEventManager.StartListening("OnLevelUnload", HandleLevelUnload);
            SystemEventManager.StartListening("OnPlayerUnitSpawn", HandlePlayerUnitSpawn);
        }

        public void CleanupEventSubscriptions() {
            //Debug.Log(gameObject.name + "Spawnable.CleanupEventSubscriptions()");
            if (!eventSubscriptionsInitialized) {
                return;
            }
            ProcessCleanupEventSubscriptions();
            eventSubscriptionsInitialized = false;
        }

        public virtual void ProcessCleanupEventSubscriptions() {
            SystemEventManager.StopListening("OnLevelUnload", HandleLevelUnload);
            SystemEventManager.StopListening("OnPlayerUnitSpawn", HandlePlayerUnitSpawn);
        }

        public void HandleLevelUnload(string eventName, EventParamProperties eventParamProperties) {
            ProcessLevelUnload();
        }

        public virtual void ProcessLevelUnload() {
            // nothing here
        }

        public void HandlePlayerUnitSpawn(string eventName, EventParamProperties eventParamProperties) {
            //Debug.Log(gameObject.name + ".InanimateUnit.HandlePlayerUnitSpawn()");
            ProcessPlayerUnitSpawn();
        }

        /*
        public virtual void OnDisable() {
            //Debug.Log(gameObject.name + ".Spawnable.OnDisable()");
            if (SystemGameManager.IsShuttingDown) {
                return;
            }
            ResetSettings();
        }
        */

        public virtual void OnDestroy() {
            //Debug.Log(gameObject.name + ".Spawnable.OnDisable()");
            if (SystemGameManager.IsShuttingDown) {
                return;
            }
            CleanupEventSubscriptions();
            CleanupEverything();
        }

        public virtual void ResetSettings() {
            //Debug.Log(gameObject.name + ".Spawnable.ResetSettings()");
            CleanupEventSubscriptions();
            CleanupEverything();

            startHasRun = false;
            componentReferencesInitialized = false;
            initialized = false;
            eventSubscriptionsInitialized = false;
        }

        public virtual void CleanupEverything() {
            //Debug.Log(gameObject.name + ".Spawnable.CleanupEverything()");
            // moved to OnDestroy so disabled object can still respond to levelUnload
            // which disabled objects?  had to move back to ondisable for object pooling
            // testing - moved back to OnDestroy because spawnables should never be pooled because they are static items in the level unless they are
            // Units in which case they call their own cleanup before despawning themselves
            //CleanupEventSubscriptions();
            CleanupScriptableObjects();
        }

        public virtual void GetComponentReferences() {
            //Debug.Log(gameObject.name + ".Spawnable.InitializeComponents()");

            if (componentReferencesInitialized == true) {
                return;
            }
            componentReferencesInitialized = true;
        }

        public virtual void HandlePrerequisiteUpdates() {
            //Debug.Log(gameObject.name + ".Spawnable.HandlePrerequisiteUpdates()");
            //InitializeComponents();
            if (CanSpawn()) {
                StartSpawn();
            }
            if (despawnObject && CanDespawn()) {
                DestroySpawn();
            }
        }

        protected virtual bool CanDespawn() {
            //Debug.Log(gameObject.name + ".Spawnable.CanDespawn()");
            if (!PrerequisitesMet) {
                return true;
            }
            return false;
        }

        public virtual bool CanSpawn() {
            //Debug.Log(gameObject.name + ".Spawnable.CanSpawn()");
            if (PrerequisitesMet && prefabProfile?.Prefab != null) {
                return true;
            }
            return false;
        }

        protected virtual void StartSpawn() {
            StartCoroutine(WaitForSpawn());
        }

        private IEnumerator WaitForSpawn() {
            float accumulatedTime = 0f;
            while (accumulatedTime < spawnDelay) {
                yield return null;
                accumulatedTime += Time.deltaTime;
            }
            Spawn();
        }


        public virtual void Spawn() {
            //Debug.Log(gameObject.name + ".Spawnable.Spawn()");

            if (spawnReference == null && prefabProfile?.Prefab != null) {
                Vector3 usedPosition = prefabProfile.SheathedPosition;
                Vector3 usedScale = prefabProfile.SheathedScale;
                Vector3 usedRotation = prefabProfile.SheathedRotation;

                if (prefabProfile.UseItemPickup) {
                    usedPosition = prefabProfile.PickupPosition;
                    usedScale = prefabProfile.PickupScale;
                    usedRotation = prefabProfile.PickupRotation;
                }

                spawnReference = objectPooler.GetPooledObject(prefabProfile.Prefab, transform.TransformPoint(usedPosition), transform.localRotation, transform);

                // updated scale from normal to sheathed this allows pickup nodes for things you can't equip to show a different size in hand than on the ground
                spawnReference.transform.localScale = usedScale;

                //spawnReference.transform.Rotate(usedRotation);
                spawnReference.transform.localRotation = Quaternion.Euler(usedRotation);
            } else {
                if (spawnReference != null) {
                    //Debug.Log(gameObject.name + ".Spawnable.Spawn(): Already spawned");
                }
                if (prefabProfile == null) {
                    //Debug.Log(gameObject.name + ".Spawnable.Spawn(): PrefabProfile is null");
                } else {
                    if (prefabProfile.Prefab == null) {
                        //Debug.Log(gameObject.name + ".Spawnable.Spawn(): PrefabProfile.myprefab is null");
                    }
                }
            }

        }

        public virtual void DestroySpawn() {
            //Debug.Log(gameObject.name + ".Spawnable.DestroySpawn()");
            if (spawnReference != null) {
                //Debug.Log(gameObject.name + ".Spawnable.DestroySpawn(): destroying spawn");
                objectPooler.ReturnObjectToPool(spawnReference);
                spawnReference = null;
            }
        }

        public virtual void ProcessPlayerUnitSpawn() {
            UpdateOnPlayerUnitSpawn();
        }

        public bool PrerequisiteCheck() {
            if (prerequisiteConditions != null && prerequisiteConditions.Count > 0) {
                foreach (PrerequisiteConditions tmpPrerequisiteConditions in prerequisiteConditions) {
                    if (tmpPrerequisiteConditions != null) {
                        tmpPrerequisiteConditions.UpdatePrerequisites(false);
                    }
                }
                if (PrerequisitesMet) {
                    HandlePrerequisiteUpdates();
                    return true;
                }
            } else {
                if (PrerequisitesMet) {
                    HandlePrerequisiteUpdates();
                    return true;
                }
            }
            return false;

        }

        public virtual bool UpdateOnPlayerUnitSpawn() {
            //Debug.Log(gameObject.name + ".Spawnable.UpdateOnPlayerUnitSpawn()");
            return PrerequisiteCheck();
            //HandlePrerequisiteUpdates();
        }

        public virtual void SetupScriptableObjects() {
            //Debug.Log(gameObject.name + ".Spawnable.SetupScriptableObjects()");
            if (prefabProfileName != null && prefabProfileName != string.Empty) {
                PrefabProfile tmpPrefabProfile = systemDataFactory.GetResource<PrefabProfile>(prefabProfileName);
                if (tmpPrefabProfile != null && tmpPrefabProfile.Prefab != null) {
                    prefabProfile = tmpPrefabProfile;
                } else {
                    Debug.LogError(gameObject.name + ".Spawnable.SetupScriptableObjects(): COULD NOT FIND PREFAB PROFILE: " + prefabProfileName + " OR ITS PREFAB WHILE INITIALIZING " + gameObject.name);
                }
            }

            if (prerequisiteConditions != null) {
                foreach (PrerequisiteConditions tmpPrerequisiteConditions in prerequisiteConditions) {
                    if (tmpPrerequisiteConditions != null) {
                        tmpPrerequisiteConditions.SetupScriptableObjects(systemGameManager, this);
                    }
                }
            }

        }

        public virtual void CleanupScriptableObjects() {
            if (prerequisiteConditions != null) {
                foreach (PrerequisiteConditions tmpPrerequisiteConditions in prerequisiteConditions) {
                    if (tmpPrerequisiteConditions != null) {
                        tmpPrerequisiteConditions.CleanupScriptableObjects(this);
                    }
                }
            }

        }


    }

}