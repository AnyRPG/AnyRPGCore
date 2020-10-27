using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class Spawnable : MonoBehaviour, IPrerequisiteOwner {

        [Header("Spawn Object")]

        [Tooltip("The name of the prefab profile to use.  The object referenced in the prefab profile will be spawned.")]
        [SerializeField]
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

        protected bool componentsInitialized = false;

        protected bool eventSubscriptionsInitialized = false;

        public GameObject MySpawnReference { get => spawnReference; set => spawnReference = value; }

        public PrefabProfile MyPrefabProfile { get => prefabProfile; set => prefabProfile = value; }

        public virtual bool MyPrerequisitesMet {
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

        protected virtual void Awake() {
            //Debug.Log(gameObject.name + ".Spawnable.Awake()");
            if (SystemGameManager.MyInstance == null) {
                Debug.LogError(gameObject.name + ": SystemGameManager not found. Is the Game Manager in the scene?");
                return;
            }
            SetupScriptableObjects();
            OrchestrateStartup();
            if (PlayerManager.MyInstance.PlayerUnitSpawned == false) {
                // this allows us to spawn things with no prerequisites that don't need to check against the player
                PrerequisiteCheck();
            }
        }

        public virtual void OrchestrateStartup() {
            OrchestratorStart();
            OrchestratorFinish();
        }

        public virtual void OrchestratorStart() {
            GetComponentReferences();
        }

        public virtual void OrchestratorFinish() {
            CreateEventSubscriptions();
        }

        public virtual void Start() {
            //Debug.Log(gameObject.name + ".Spawnable.Start()");
            //InitializeComponents();
            //interactionTransform = transform;
            //InitializeMaterials();
        }

        public virtual void CreateEventSubscriptions() {
            //Debug.Log(gameObject.name + ".Spawnable.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized) {
                return;
            }
            SystemEventManager.StartListening("OnPlayerUnitSpawn", HandlePlayerUnitSpawn);
            if (PlayerManager.MyInstance.PlayerUnitSpawned) {
                //Debug.Log(gameObject.name + ".Spawnable.CreateEventSubscriptions(): Player Unit is spawned.  Handling immediate spawn!");
                ProcessPlayerUnitSpawn();
            } else {
                //Debug.Log(gameObject.name + ".Spawnable.CreateEventSubscriptions(): Player Unit is not spawned. Added Handle Spawn listener");
            }
            //SystemEventManager.MyInstance.OnPrerequisiteUpdated += HandlePrerequisiteUpdates;
            eventSubscriptionsInitialized = true;
        }

        private void CleanupEventSubscriptions() {
            //Debug.Log(gameObject.name + "Spawnable.CleanupEventSubscriptions()");
            if (!eventSubscriptionsInitialized) {
                return;
            }
            if (SystemEventManager.MyInstance != null) {
                //SystemEventManager.MyInstance.OnPrerequisiteUpdated -= HandlePrerequisiteUpdates;
                SystemEventManager.StopListening("OnPlayerUnitSpawn", HandlePlayerUnitSpawn);
            }
            eventSubscriptionsInitialized = false;
        }

        public void HandlePlayerUnitSpawn(string eventName, EventParamProperties eventParamProperties) {
            //Debug.Log(gameObject.name + ".InanimateUnit.HandlePlayerUnitSpawn()");
            ProcessPlayerUnitSpawn();
        }

        public virtual void OnDisable() {
            //Debug.Log(gameObject.name + ".Spawnable.OnDisable()");
            CleanupEverything();
        }

        public virtual void CleanupEverything() {
            //Debug.Log(gameObject.name + ".Spawnable.CleanupEverything()");
            CleanupEventSubscriptions();
            CleanupScriptableObjects();
        }

        public virtual void GetComponentReferences() {
            //Debug.Log(gameObject.name + ".Spawnable.InitializeComponents()");

            if (componentsInitialized == true) {
                return;
            }

            componentsInitialized = true;
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
            if (!MyPrerequisitesMet) {
                return true;
            }
            return false;
        }

        public virtual bool CanSpawn() {
            //Debug.Log(gameObject.name + ".Spawnable.CanSpawn()");
            if (MyPrerequisitesMet) {
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

            if (spawnReference == null && prefabProfile != null && prefabProfile.Prefab != null) {
                //Debug.Log(gameObject.name + ".Spawnable.Spawn(): Spawning " + prefabProfile.MyName);
                Vector3 usedPosition = prefabProfile.SheathedPosition;
                Vector3 usedScale = prefabProfile.SheathedScale;
                Vector3 usedRotation = prefabProfile.SheathedRotation;

                if (prefabProfile.UseItemPickup) {
                    usedPosition = prefabProfile.PickupPosition;
                    usedScale = prefabProfile.PickupScale;
                    usedRotation = prefabProfile.PickupRotation;
                }

                //spawnReference = Instantiate(prefabProfile.MyPrefab, transform.TransformPoint(usedPosition), Quaternion.LookRotation(transform.forward), transform);
                spawnReference = Instantiate(prefabProfile.Prefab, transform.TransformPoint(usedPosition), transform.localRotation, transform);

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
                Destroy(spawnReference);
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
                if (MyPrerequisitesMet) {
                    HandlePrerequisiteUpdates();
                    return true;
                }
            } else {
                HandlePrerequisiteUpdates();
                return true;
            }
            return false;

        }

        public virtual bool UpdateOnPlayerUnitSpawn() {
            //Debug.Log(gameObject.name + ".Spawnable.UpdateOnPlayerUnitSpawn()");
            return PrerequisiteCheck();
            //HandlePrerequisiteUpdates();
        }

        protected virtual void OnDestroy() {
            //Debug.Log(gameObject.name + ".Spawnable.OnDestroy()");
        }

        public virtual void SetupScriptableObjects() {
            //Debug.Log(gameObject.name + ".Spawnable.SetupScriptableObjects()");
            if (prefabProfileName != null && prefabProfileName != string.Empty) {
                PrefabProfile tmpPrefabProfile = SystemPrefabProfileManager.MyInstance.GetResource(prefabProfileName);
                if (tmpPrefabProfile != null && tmpPrefabProfile.Prefab != null) {
                    prefabProfile = tmpPrefabProfile;
                } else {
                    Debug.LogError(gameObject.name + ".Spawnable.SetupScriptableObjects(): COULD NOT FIND PREFAB PROFILE: " + prefabProfileName + " OR ITS PREFAB WHILE INITIALIZING " + gameObject.name);
                }
            }

            if (prerequisiteConditions != null) {
                foreach (PrerequisiteConditions tmpPrerequisiteConditions in prerequisiteConditions) {
                    if (tmpPrerequisiteConditions != null) {
                        tmpPrerequisiteConditions.SetupScriptableObjects(this);
                    }
                }
            }

        }

        public virtual void CleanupScriptableObjects() {
            if (prerequisiteConditions != null) {
                foreach (PrerequisiteConditions tmpPrerequisiteConditions in prerequisiteConditions) {
                    if (tmpPrerequisiteConditions != null) {
                        tmpPrerequisiteConditions.CleanupScriptableObjects();
                    }
                }
            }

        }


    }

}