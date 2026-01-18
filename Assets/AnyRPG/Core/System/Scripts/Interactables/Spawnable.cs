using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

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
        [FormerlySerializedAs("prerequisiteConditions")]
        protected List<PrerequisiteConditions> spawnPrerequisites = new List<PrerequisiteConditions>();

        protected bool initialized = false;
        protected bool eventSubscriptionsInitialized = false;
        //protected bool spawnedByPrefab = false;

        // game manager references

        protected PlayerManager playerManager = null;
        protected SystemDataFactory systemDataFactory = null;
        protected ObjectPooler objectPooler = null;

        public GameObject SpawnReference { get => spawnReference; set => spawnReference = value; }
        public PrefabProfile PrefabProfile { get => prefabProfile; set => prefabProfile = value; }
        public virtual string DisplayName {
            get {
                return gameObject.name;
            }
            set {
            }
        }

        public virtual bool SpawnPrerequisitesMet(UnitController sourceUnitController) {
            //Debug.Log($"{gameObject.name}.Spawnable.MyPrerequisitesMet");
            foreach (PrerequisiteConditions prerequisiteCondition in spawnPrerequisites) {
                if (!prerequisiteCondition.IsMet(sourceUnitController)) {
                    return false;
                }
            }
            // there are no prerequisites, or all prerequisites are complete
            return true;
        }

        public override void Configure(SystemGameManager systemGameManager) {
            //Debug.Log($"{gameObject.name}.Spawnable.Configure()");

            base.Configure(systemGameManager);

            SetupScriptableObjects();
            CreateEventSubscriptions();
            if (playerManager.PlayerUnitSpawned == false) {
                // this allows us to spawn things with no prerequisites that don't need to check against the player
                PrerequisiteCheck(null);
            }
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            playerManager = systemGameManager.PlayerManager;
            systemDataFactory = systemGameManager.SystemDataFactory;
            objectPooler = systemGameManager.ObjectPooler;
        }

        protected override void PostConfigure() {
            base.PostConfigure();
            Init();
        }

        public virtual void Init() {
            //Debug.Log($"{gameObject.name}.Spawnable.Init()");

            if (initialized == true) {
                return;
            }

            // moved here from CreateEventSubscriptions.  Init should have time to occur before processing this
            if (playerManager.PlayerUnitSpawned) {
                //Debug.Log($"{gameObject.name}.Spawnable.CreateEventSubscriptions(): Player Unit is spawned.  Handling immediate spawn!");
                ProcessPlayerUnitSpawn(playerManager.UnitController);
            } else {
                //Debug.Log($"{gameObject.name}.Spawnable.CreateEventSubscriptions(): Player Unit is not spawned. Added Handle Spawn listener");
            }
            initialized = true;
        }


        public void CreateEventSubscriptions() {
            //Debug.Log($"{gameObject.name}.Spawnable.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized) {
                return;
            }
            ProcessCreateEventSubscriptions();
            eventSubscriptionsInitialized = true;
        }

        public virtual void ProcessCreateEventSubscriptions() {
            systemEventManager.OnPlayerUnitSpawn += HandlePlayerUnitSpawn;
        }

        public void CleanupEventSubscriptions() {
            //Debug.Log($"{gameObject.name}Spawnable.CleanupEventSubscriptions()");
            if (!eventSubscriptionsInitialized) {
                return;
            }
            ProcessCleanupEventSubscriptions();
            eventSubscriptionsInitialized = false;
        }

        public virtual void ProcessCleanupEventSubscriptions() {
            systemEventManager.OnPlayerUnitSpawn -= HandlePlayerUnitSpawn;
        }

        public void HandlePlayerUnitSpawn(UnitController sourceUnitController) {
            //Debug.Log($"{gameObject.name}.InanimateUnit.HandlePlayerUnitSpawn()");
            ProcessPlayerUnitSpawn(sourceUnitController);
        }

        public virtual void OnDestroy() {
            //Debug.Log($"{gameObject.name}.Spawnable.OnDisable()");
            if (SystemGameManager.IsShuttingDown) {
                return;
            }
            CleanupEventSubscriptions();
            CleanupEverything();
        }

        public virtual void ResetSettings() {
            //Debug.Log($"{gameObject.name}.Spawnable.ResetSettings()");

            CleanupEventSubscriptions();
            CleanupEverything();

            initialized = false;
            eventSubscriptionsInitialized = false;
        }

        public virtual void CleanupEverything() {
            //Debug.Log($"{gameObject.name}.Spawnable.CleanupEverything()");
            // moved to OnDestroy so disabled object can still respond to levelUnload
            // which disabled objects?  had to move back to ondisable for object pooling
            // testing - moved back to OnDestroy because spawnables should never be pooled because they are static items in the level unless they are
            // Units in which case they call their own cleanup before despawning themselves
            //CleanupEventSubscriptions();
            CleanupScriptableObjects();
        }

        

        public virtual void HandlePrerequisiteUpdates(UnitController sourceUnitController) {
            //Debug.Log($"{gameObject.name}.Spawnable.HandlePrerequisiteUpdates()");
            //InitializeComponents();
            if (CanSpawn(sourceUnitController)) {
                StartSpawn();
            }
            if (despawnObject && CanDespawn(sourceUnitController)) {
                DestroySpawn();
            }
        }

        protected virtual bool CanDespawn(UnitController sourceUnitController) {
            //Debug.Log($"{gameObject.name}.Spawnable.CanDespawn()");
            if (!SpawnPrerequisitesMet(sourceUnitController)) {
                return true;
            }
            return false;
        }

        public virtual bool CanSpawn(UnitController sourceUnitController) {
            //Debug.Log($"{gameObject.name}.Spawnable.CanSpawn()");
            if (SpawnPrerequisitesMet(sourceUnitController) && (prefabProfile?.Prefab != null || spawnReference != null)) {
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
            //Debug.Log($"{gameObject.name}.Spawnable.Spawn()");

            if (spawnReference == null && prefabProfile?.Prefab != null) {
                //Debug.Log($"{gameObject.name}.Spawnable.Spawn() reference is null but prefab is not");
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
                //spawnedByPrefab = true;
            } else {
                if (spawnReference != null && spawnReference.activeSelf == false) {
                    //Debug.Log($"{gameObject.name}.Spawnable.Spawn() setting {spawnReference.name} active");
                    spawnReference.SetActive(true);
                }
            }
            if (spawnReference != null) {
                AutoConfiguredMonoBehaviour autoConfiguredMonoBehaviour = spawnReference.GetComponent<AutoConfiguredMonoBehaviour>();
                if (autoConfiguredMonoBehaviour != null) {
                    //Debug.Log($"{gameObject.name}.Spawnable.Spawn() found autoconfiguredmonobehaviour: configuring");
                    autoConfiguredMonoBehaviour.AutoConfigure(systemGameManager);
                }
            } else {
                Debug.LogError($"{gameObject.name}.Spawnable.Spawn(): COULD NOT SPAWN OBJECT");
            }

        }

        public virtual void DestroySpawn() {
            //Debug.Log($"{gameObject.name}.Spawnable.DestroySpawn()");

            if (spawnReference != null) {
                //Debug.Log($"{gameObject.name}.Spawnable.DestroySpawn(): destroying spawn");

                // this code has been replaced with just deactivating the gameobject
                //objectPooler.ReturnObjectToPool(spawnReference);
                //spawnReference = null;

                spawnReference.SetActive(false);
            }
        }

        public virtual void ProcessPlayerUnitSpawn(UnitController sourceUnitController) {
            UpdateOnPlayerUnitSpawn(sourceUnitController);
        }

        public bool PrerequisiteCheck(UnitController sourceUnitController) {
            //Debug.Log($"{gameObject.name}.Spawnable.PrerequisiteCheck({(sourceUnitController == null ? "null" : sourceUnitController.gameObject.name)})");

            if (spawnPrerequisites.Count > 0 && sourceUnitController != null) {
                foreach (PrerequisiteConditions tmpPrerequisiteConditions in spawnPrerequisites) {
                    if (tmpPrerequisiteConditions != null) {
                        tmpPrerequisiteConditions.UpdatePrerequisites(sourceUnitController, false);
                    }
                }
                if (SpawnPrerequisitesMet(sourceUnitController)) {
                    HandlePrerequisiteUpdates(sourceUnitController);
                    return true;
                }
            } else if (spawnPrerequisites.Count == 0) {
                if (SpawnPrerequisitesMet(sourceUnitController)) {
                    HandlePrerequisiteUpdates(sourceUnitController);
                    return true;
                }
            }
            return false;

        }

        public virtual bool UpdateOnPlayerUnitSpawn(UnitController sourceUnitController) {
            //Debug.Log($"{gameObject.name}.Spawnable.UpdateOnPlayerUnitSpawn({sourceUnitController.gameObject.name})");

            return PrerequisiteCheck(sourceUnitController);
            //HandlePrerequisiteUpdates();
        }

        public virtual void SetupScriptableObjects() {
            //Debug.Log($"{gameObject.name}.Spawnable.SetupScriptableObjects()");

            if (prefabProfileName != null && prefabProfileName != string.Empty) {
                PrefabProfile tmpPrefabProfile = systemDataFactory.GetResource<PrefabProfile>(prefabProfileName);
                if (tmpPrefabProfile != null && tmpPrefabProfile.Prefab != null) {
                    prefabProfile = tmpPrefabProfile;
                } else {
                    Debug.LogError(gameObject.name + ".Spawnable.SetupScriptableObjects(): COULD NOT FIND PREFAB PROFILE: " + prefabProfileName + " OR ITS PREFAB WHILE INITIALIZING " + gameObject.name);
                }
            }

            if (spawnPrerequisites != null) {
                foreach (PrerequisiteConditions tmpPrerequisiteConditions in spawnPrerequisites) {
                    if (tmpPrerequisiteConditions != null) {
                        tmpPrerequisiteConditions.SetupScriptableObjects(systemGameManager, this);
                    }
                }
            }

        }

        public virtual void CleanupScriptableObjects() {
            if (spawnPrerequisites != null) {
                foreach (PrerequisiteConditions tmpPrerequisiteConditions in spawnPrerequisites) {
                    if (tmpPrerequisiteConditions != null) {
                        tmpPrerequisiteConditions.CleanupScriptableObjects(this);
                    }
                }
            }

        }


    }

}