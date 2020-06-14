using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

namespace AnyRPG {
    //public class InanimateUnit : InteractableOption, INamePlateUnit {
    public class InanimateUnit : MonoBehaviour, INamePlateUnit {

        public event System.Action OnInitializeNamePlate = delegate { };
        public event Action<INamePlateUnit> NamePlateNeedsRemoval = delegate { };
        public event Action<int, int> ResourceBarNeedsUpdate = delegate { };

        public event Action<IInteractable> MiniMapStatusUpdateHandler = delegate { };

        [Header("NAMEPLATE SETTINGS")]

        [Tooltip("This is what will be printed on the nameplate above the object.  It will also override whatever value is set for the Interactable mouseover display name.")]
        [SerializeField]
        private string displayName = string.Empty;

        [Tooltip("If true, the faction will not be shown on the nameplate")]
        [SerializeField]
        private bool suppressFaction = true;

        [Tooltip("Set a transform to override the default nameplate placement above the interactable.  Useful for interactables that are not 2m tall.")]
        [SerializeField]
        private Transform namePlateTransform = null;

        private NamePlateController namePlate;

        [Header("UNIT FRAME SETTINGS")]

        [Tooltip("An object or bone in the heirarchy to use as the camera target.")]
        [SerializeField]
        private string unitFrameTarget = string.Empty;

        [Tooltip("The position the camera is looking at, relative to the target")]
        [SerializeField]
        private Vector3 unitFrameCameraLookOffset = Vector3.zero;

        [Tooltip("The position of the camera relative to the target")]
        [SerializeField]
        private Vector3 unitFrameCameraPositionOffset = Vector3.zero;

        protected Interactable interactable;

        protected bool componentReferencesInitialized = false;
        protected bool eventSubscriptionsInitialized = false;


        public Interactable MyInteractable { get => interactable; set => interactable = value; }
        public NamePlateController MyNamePlate { get => namePlate; set => namePlate = value; }
        public string UnitDisplayName { get => displayName; }
        public string Title { get => string.Empty; }
        public Faction Faction { get => null; }
        public string MyUnitFrameTarget { get => unitFrameTarget; }
        public Vector3 MyUnitFrameCameraLookOffset { get => unitFrameCameraLookOffset; set => unitFrameCameraLookOffset = value; }
        public Vector3 MyUnitFrameCameraPositionOffset { get => unitFrameCameraPositionOffset; set => unitFrameCameraPositionOffset = value; }
        public Transform MyNamePlateTransform {
            get {
                if (namePlateTransform != null) {
                    return namePlateTransform;
                }
                return transform;
            }
        }

        public bool SuppressFaction { get => suppressFaction; set => suppressFaction = value; }

        public int Level {
            get {
                return 1;
            }
        }

        public bool HasHealth() {
            return false;
        }

        public bool HasPrimaryResource() {
            return false;
        }

        public bool HasSecondaryResource() {
            return false;
        }

        public int CurrentHealth() {
            return 1;
        }

        public int MaxHealth() {
            return 1;
        }

        private void OnEnable() {
            //Debug.Log(gameObject.name + ": running OnEnable()");
            //InitializeNamePlate();
        }

        public void OnDisable() {
            if (NamePlateManager.MyInstance != null) {
                NamePlateManager.MyInstance.RemoveNamePlate(this as INamePlateUnit);
            }
            CleanupEventSubscriptions();
            //CleanupScriptableObjects();
        }

        protected void Awake() {
            //Debug.Log(gameObject.name + ": Awake() about to get references to all local components");
            OrchestratorStart();
        }

        protected void Start() {
            //Debug.Log(gameObject.name + ".InanimateUnit.Start()");
            CreateEventSubscriptions();
            InitializeNamePlate();
        }

        public virtual void OrchestratorStart() {
            //Debug.Log(gameObject.name + ".InteractableOption.OrchestratorStart()");
            //SetupScriptableObjects();
            GetComponentReferences();
        }

        public virtual void OrchestratorFinish() {

        }

        public virtual void GetComponentReferences() {
            //Debug.Log(gameObject.name + ".InteractableOption.GetComponentReferences()");
            if (componentReferencesInitialized) {
                //Debug.Log("InteractableOption.GetComponentReferences(): already initialized. exiting!");
                return;
            }
            interactable = GetComponent<Interactable>();
            if (interactable == null) {
                //Debug.Log(gameObject.name + ".InteractableOption.GetComponentReferences(): " + interactable is null);
            }

            componentReferencesInitialized = true;
        }

        public virtual void CreateEventSubscriptions() {
            if (eventSubscriptionsInitialized) {
                return;
            }
            //Debug.Log(gameObject.name + ".InteractableOption.CreateEventSubscriptions(): subscribing to player unit spawn");
            if (SystemEventManager.MyInstance == null) {
                Debug.LogError("SystemEventManager not found in the scene.  Is the GameManager in the scene?");
                return;
            }
            if (PlayerManager.MyInstance == null) {
                Debug.LogError("PlayerManager not found. Is the GameManager in the scene?");
                return;
            } else {
                SystemEventManager.StartListening("OnPlayerUnitSpawn", HandlePlayerUnitSpawn);

                if (PlayerManager.MyInstance.MyPlayerUnitSpawned == true) {
                    //Debug.Log(gameObject.name + ".InteractableOption.CreateEventSubscriptions(): player unit is already spawned.");
                    ProcessPlayerUnitSpawn();
                }
            }
            eventSubscriptionsInitialized = true;
        }

        public virtual void CleanupEventSubscriptions() {
            if (SystemEventManager.MyInstance != null) {
                SystemEventManager.StopListening("OnPlayerUnitSpawn", HandlePlayerUnitSpawn);
            }
            eventSubscriptionsInitialized = false;
        }

        public void InitializeNamePlate() {
            //Debug.Log(gameObject.name + ".InanimateUnit.InitializeNamePlate()");
            if (NamePlateManager.MyInstance == null) {
                Debug.LogError(gameObject.name + ": NamePlateManager not found.  Is the GameManager in the scene?");
                return;
            }
            //if (interactable != null && interactable.CanInteract() && namePlate == null) {
            if (interactable != null && interactable.CanSpawn() == true && namePlate == null) {
                //Debug.Log(gameObject.name + ".InanimateUnit.InitializeNamePlate(): can interact with interactable");
                NamePlateController _namePlate = NamePlateManager.MyInstance.AddNamePlate(this, (namePlateTransform == null ? true : false));
                if (_namePlate != null) {
                    namePlate = _namePlate;
                    //Debug.Log(gameObject.name + ".InanimateUnit.InitializeNamePlate(): set nameplate reference successfully");
                } else {
                    //Debug.Log(gameObject.name + ".InanimateUnit.InitializeNamePlate(): nameplate was null!!!");
                }
                OnInitializeNamePlate();
            } else {
                //Debug.Log(gameObject.name + ".InanimateUnit.InitializeNamePlate(): isStarted && interactable.CanInteract() == false");
                return;
            }
        }

        /*
        public bool CanInteract() {
            return false;
        }

        public bool Interact(CharacterUnit source) {
            Debug.Log(gameObject.name + ".InanimateUnit.Interact()");
            return false;
        }

        public void StopInteract() {
            // nothing needed tobe done
        }
        */

        public void OnDestroy() {
            CleanupEventSubscriptions();
        }

        public void HandlePrerequisiteUpdates() {
            //Debug.Log(gameObject.name + ".InanimateUnit.HandlePrerequisiteUpdates()");
            InitializeNamePlate();
            if (interactable != null) {
                interactable.HandlePrerequisiteUpdates();
            } else {
                //Debug.Log(gameObject.name + ".InteractableOption.HandlePrerequisiteUpdates(): interactable was null");
            }
        }

        public void HandlePlayerUnitSpawn(string eventName, EventParamProperties eventParamProperties) {
            //Debug.Log(gameObject.name + ".InanimateUnit.HandlePlayerUnitSpawn()");
            ProcessPlayerUnitSpawn();
        }

        public void ProcessPlayerUnitSpawn() {
            HandlePrerequisiteUpdates();
        }


        /*
        public virtual void SetupScriptableObjects() {
            //Debug.Log(gameObject.name + ".InteractableOption.SetupScriptableObjects()");
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
        */

    }

}