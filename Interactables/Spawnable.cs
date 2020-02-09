using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
    public class Spawnable : MonoBehaviour {

        [SerializeField]
        protected string prefabProfileName;

        protected PrefabProfile prefabProfile;

        [SerializeField]
        protected float spawnDelay = 0f;

        [SerializeField]
        protected GameObject spawnReference;

        // if there is an object spawned, and the prerequisite conditions are no longer met, despawn it
        [SerializeField]
        private bool despawnObject = false;

        [SerializeField]
        protected List<PrerequisiteConditions> prerequisiteConditions = new List<PrerequisiteConditions>();

        protected bool componentsInitialized = false;

        protected bool eventSubscriptionsInitialized = false;

        public GameObject MySpawnReference { get => spawnReference; set => spawnReference = value; }

        public PrefabProfile MyPrefabProfile { get => prefabProfile; set => prefabProfile = value; }

        public bool MyPrerequisitesMet {
            get {
                //Debug.Log(gameObject.name + ".InteractableOption.MyPrerequisitesMet");
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
            //Debug.Log(gameObject.name + ".Interactable.Awake()");
            if (SystemGameManager.MyInstance == null) {
                Debug.LogError(gameObject.name + "Interactable.Awake(): Could not find System Game Manager.  Is Game Manager Prefab in Scene?!!!");
                return;
            }
            InitializeComponents();
            SetupScriptableObjects();
        }

        public virtual void Start() {
            //Debug.Log(gameObject.name + ".Interactable.Start()");
            InitializeComponents();
            //interactionTransform = transform;
            //InitializeMaterials();
            CreateEventSubscriptions();
        }

        public virtual void CreateEventSubscriptions() {
            //Debug.Log(gameObject.name + ".Interactable.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized) {
                return;
            }
            SystemEventManager.MyInstance.OnPlayerUnitSpawn += HandlePlayerUnitSpawn;
            if (PlayerManager.MyInstance.MyPlayerUnitSpawned) {
                //Debug.Log(gameObject.name + ".Interactable.Start(): Player Unit is spawned.  Handling immediate spawn!");
                HandlePlayerUnitSpawn();
            } else {
                //Debug.Log(gameObject.name + ".Interactable.Start(): Player Unit is not spawned. Added Handle Spawn listener");
            }
            SystemEventManager.MyInstance.OnPrerequisiteUpdated += HandlePrerequisiteUpdates;
            eventSubscriptionsInitialized = true;
        }

        private void CleanupEventSubscriptions() {
            //Debug.Log("PlayerManager.CleanupEventSubscriptions()");
            if (!eventSubscriptionsInitialized) {
                return;
            }
            if (SystemEventManager.MyInstance != null) {
                SystemEventManager.MyInstance.OnPrerequisiteUpdated -= HandlePrerequisiteUpdates;
                SystemEventManager.MyInstance.OnPlayerUnitSpawn -= HandlePlayerUnitSpawn;
            }
            eventSubscriptionsInitialized = false;
        }

        public void OnDisable() {
            //Debug.Log(gameObject.name + ".Interactable.OnDisable()");
            CleanupEverything();
        }

        public virtual void CleanupEverything() {
            //Debug.Log(gameObject.name + ".Interactable.CleanupEverything()");
            CleanupEventSubscriptions();
        }

        public virtual void InitializeComponents() {
            //Debug.Log(gameObject.name + ".Interactable.InitializeComponents()");

            if (componentsInitialized == true) {
                return;
            }

            componentsInitialized = true;
        }

        public virtual void HandlePrerequisiteUpdates() {
            //Debug.Log(gameObject.name + ".Interactable.HandlePrerequisiteUpdates()");
            InitializeComponents();
            if (CanSpawn()) {
                StartSpawn();
            }
            if (despawnObject && CanDespawn()) {
                DestroySpawn();
            }
        }

        protected virtual bool CanDespawn() {
            if (!MyPrerequisitesMet) {
                return true;
            }
            return false;
        }

        protected virtual bool CanSpawn() {
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
                accumulatedTime += Time.deltaTime;
                yield return null;
            }
            Spawn();
        }


        public virtual void Spawn() {
            //Debug.Log(gameObject.name + ".Interactable.Spawn()");

            if (spawnReference == null && prefabProfile != null && prefabProfile.MyPrefab != null) {
                //Debug.Log(gameObject.name + ".Interactable.Spawn(): Spawning " + spawnPrefab.name);
                spawnReference = Instantiate(prefabProfile.MyPrefab, transform.TransformPoint(prefabProfile.MyPosition), Quaternion.LookRotation(transform.forward), transform);
                spawnReference.transform.Rotate(prefabProfile.MyRotation);
            } else {
                if (spawnReference != null) {
                    //Debug.Log(gameObject.name + ".Interactable.Spawn(): Already spawned");
                }
                if (prefabProfile == null) {
                    //Debug.Log(gameObject.name + ".Interactable.Spawn(): PrefabProfile is null");
                } else {
                    if (prefabProfile.MyPrefab == null) {
                        //Debug.Log(gameObject.name + ".Interactable.Spawn(): PrefabProfile.myprefab is null");
                    }
                }
            }

        }

        public virtual void DestroySpawn() {
            //Debug.Log(gameObject.name + ".Interactable.DestroySpawn()");
            if (spawnReference != null) {
                Destroy(spawnReference);
                spawnReference = null;
            }
        }

        public virtual void HandlePlayerUnitSpawn() {
            //Debug.Log(gameObject.name + ".Interactable.HandlePlayerUnitSpawn()");
            HandlePrerequisiteUpdates();
        }



        protected virtual void OnDestroy() {
            //Debug.Log(gameObject.name + ".Interactable.OnDestroy()");
        }

        public virtual void SetupScriptableObjects() {
            //Debug.Log(gameObject.name + ".Interactable.SetupScriptableObjects()");
            if (prefabProfileName != null && prefabProfileName != string.Empty) {
                PrefabProfile tmpPrefabProfile = SystemPrefabProfileManager.MyInstance.GetResource(prefabProfileName);
                if (tmpPrefabProfile != null && tmpPrefabProfile.MyPrefab != null) {
                    prefabProfile = tmpPrefabProfile;
                } else {
                    Debug.LogError(gameObject.name + ".Interactable.SetupScriptableObjects(): COULD NOT FIND PREFAB PROFILE: " + prefabProfileName + " OR ITS PREFAB WHILE INITIALIZING " + gameObject.name);
                }
            }
        }


    }

}