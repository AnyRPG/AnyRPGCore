using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public abstract class InteractableOption : MonoBehaviour, IInteractable, IPrerequisiteOwner {

        public abstract event System.Action<IInteractable> MiniMapStatusUpdateHandler;

        [SerializeField]
        protected string interactionPanelTitle;

        [SerializeField]
        protected Sprite interactionPanelImage;

        [SerializeField]
        protected Sprite namePlateImage;

        [SerializeField]
        protected List<PrerequisiteConditions> prerequisiteConditions = new List<PrerequisiteConditions>();

        [SerializeField]
        protected INamePlateUnit namePlateUnit;

        protected Interactable interactable;

        protected bool componentReferencesInitialized = false;
        protected bool eventSubscriptionsInitialized = false;

        protected UnitAudio unitAudio = null;

        public virtual string MyInteractionPanelTitle { get => interactionPanelTitle; set => interactionPanelTitle = value; }
        public Interactable MyInteractable { get => interactable; set => interactable = value; }

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

        public virtual Sprite MyIcon { get => interactionPanelImage; }
        public virtual Sprite MyNamePlateImage { get => namePlateImage; }

        public string MyName { get => (MyInteractionPanelTitle != null && MyInteractionPanelTitle != string.Empty ? MyInteractionPanelTitle : (interactable != null ? interactable.MyName : "interactable is null!")); }
        public UnitAudio MyUnitAudio { get => unitAudio; set => unitAudio = value; }

        protected virtual void Awake() {
            //Debug.Log(gameObject.name + ".InteractableOption.Awake(). Setting interactable");
            //GetComponentReferences();
        }

        protected virtual void Start() {
            CreateEventSubscriptions();
        }

        public virtual void OrchestratorStart() {
            SetupScriptableObjects();
            GetComponentReferences();
        }

        public virtual void OrchestratorFinish() {

        }

        public virtual void HandleConfirmAction() {
            SystemEventManager.MyInstance.NotifyOnInteractionWithOptionCompleted(this);
        }

        public virtual void GetComponentReferences() {
            //Debug.Log(gameObject.name + ".InteractableOption.GetComponentReferences()");
            if (componentReferencesInitialized) {
                //Debug.Log("InteractableOption.GetComponentReferences(): already initialized. exiting!");
                return;
            }
            interactable = GetComponent<Interactable>();
            unitAudio = GetComponent<UnitAudio>();
            if (interactable == null) {
                //Debug.Log(gameObject.name + ".InteractableOption.GetComponentReferences(): " + interactable is null);
            }
            namePlateUnit = GetComponent<INamePlateUnit>();

            componentReferencesInitialized = true;
        }

        public virtual bool CanInteract() {
            return MyPrerequisitesMet;
        }

        public virtual bool Interact(CharacterUnit source) {
            //Debug.Log(gameObject.name + ".InteractableOption.Interact()");
            SystemEventManager.MyInstance.NotifyOnInteractionWithOptionStarted(this);
            return true;
        }

        public virtual void StopInteract() {
            //Debug.Log(gameObject.name + ".InanimateUnit.StopInteract()");
            (PlayerManager.MyInstance.MyCharacter.MyCharacterController as PlayerController).StopInteract();
        }

        public virtual bool HasMiniMapText() {
            return false;
        }

        public virtual bool HasMiniMapIcon() {
            return (MyNamePlateImage != null);
        }

        public virtual bool SetMiniMapText(TextMeshProUGUI text) {
            return (GetCurrentOptionCount() > 0);
        }

        public virtual void SetMiniMapIcon(Image icon) {
            //Debug.Log(gameObject.name + ".InteractableOption.SetMiniMapIcon()");
            if (CanShowMiniMapIcon()) {
                icon.sprite = MyNamePlateImage;
                icon.color = Color.white;
            } else {
                icon.sprite = null;
                icon.color = new Color32(0, 0, 0, 0);
            }
            return;
        }

        public virtual bool CanShowMiniMapIcon() {
            return (GetCurrentOptionCount() > 0);
        }

        public virtual string GetDescription() {
            return string.Format("<color=#ffff00ff>{0}</color>", GetSummary());
        }

        public virtual string GetSummary() {
            return string.Format("{0}", MyName);
        }

        public virtual void OnDisable() {
            CleanupEventSubscriptions();
            CleanupScriptableObjects();
        }

        public virtual void CreateEventSubscriptions() {
            if (eventSubscriptionsInitialized) {
                return;
            }
            //Debug.Log(gameObject.name + ".InteractableOption.CreateEventSubscriptions(): subscribing to player unit spawn");
            SystemEventManager.MyInstance.OnPlayerUnitSpawn += HandlePlayerUnitSpawn;
            if (PlayerManager.MyInstance.MyPlayerUnitSpawned == true) {
                //Debug.Log(gameObject.name + ".InteractableOption.CreateEventSubscriptions(): player unit is already spawned.");
                HandlePlayerUnitSpawn();
            }
            eventSubscriptionsInitialized = true;
        }

        public virtual void CleanupEventSubscriptions() {
            if (SystemEventManager.MyInstance != null) {
                SystemEventManager.MyInstance.OnPlayerUnitSpawn -= HandlePlayerUnitSpawn;
            }
            eventSubscriptionsInitialized = false;
        }

        public virtual void HandlePlayerUnitSpawn() {
            //Debug.Log(gameObject.name + ".InteractableOption.HandlePlayerUnitSpawn()");
            if (prerequisiteConditions != null && prerequisiteConditions.Count > 0) {
                foreach (PrerequisiteConditions tmpPrerequisiteConditions in prerequisiteConditions) {
                    if (tmpPrerequisiteConditions != null) {
                        tmpPrerequisiteConditions.UpdatePrerequisites();
                    }
                }
            } else {
                HandlePrerequisiteUpdates();
            }
            //HandlePrerequisiteUpdates();
        }


        public virtual int GetValidOptionCount() {
            // overwrite me if this type of interactable option has a list of options instead of just one
            return (MyPrerequisitesMet == true ? 1 : 0);
        }

        public virtual int GetCurrentOptionCount() {
            // overwrite me or everything is valid as long as prerequisites are met, which isn't the case for things like dialog, which have multiple options
            //Debug.Log(gameObject.name + ".CharacterCreatorInteractable.GetCurrentOptionCount()");
            return GetValidOptionCount();
        }

        public virtual void HandlePrerequisiteUpdates() {
            //Debug.Log(gameObject.name + ".InteractableOption.HandlePrerequisiteUpdates()");
            if (interactable != null) {
                interactable.HandlePrerequisiteUpdates();
            } else {
                //Debug.Log(gameObject.name + ".InteractableOption.HandlePrerequisiteUpdates(): interactable was null");
            }
        }

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


    }

}