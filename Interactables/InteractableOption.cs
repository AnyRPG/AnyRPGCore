using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public abstract class InteractableOption : MonoBehaviour, IInteractable, IPrerequisiteOwner {

        public abstract event System.Action<IInteractable> MiniMapStatusUpdateHandler;

        [Header("Interaction Panel")]

        [Tooltip("The text to display for the clickable option in the interaction panel if this object has multiple interaction options.")]
        [SerializeField]
        protected string interactionPanelTitle;

        [Tooltip("The image to display beside the text for the clickable option in the interaction panel if this object has multiple interaction options.")]
        [SerializeField]
        protected Sprite interactionPanelImage;

        [Header("Nameplate")]

        [Tooltip("If there is no system option set for the nameplate image of this interactable option type, this will be used instead.")]
        [SerializeField]
        protected Sprite namePlateImage;

        [Header("Interaction")]

        [Tooltip("These game conditions must be satisfied to be able to interact with this option.")]
        [SerializeField]
        protected List<PrerequisiteConditions> prerequisiteConditions = new List<PrerequisiteConditions>();

        [SerializeField]
        protected INamePlateUnit namePlateUnit;

        protected Interactable interactable;

        protected bool componentReferencesInitialized = false;
        protected bool eventSubscriptionsInitialized = false;

        protected UnitAudioController unitAudio = null;

        public virtual string InteractionPanelTitle { get => interactionPanelTitle; set => interactionPanelTitle = value; }
        public Interactable MyInteractable { get => interactable; set => interactable = value; }

        public virtual bool MyPrerequisitesMet {
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

        public string DisplayName { get => (InteractionPanelTitle != null && InteractionPanelTitle != string.Empty ? InteractionPanelTitle : (interactable != null ? interactable.DisplayName : "interactable is null!")); }
        public UnitAudioController UnitAudio { get => unitAudio; set => unitAudio = value; }

        protected virtual void Awake() {
            //Debug.Log(gameObject.name + ".InteractableOption.Awake(). Setting interactable");
            //GetComponentReferences();
        }

        protected virtual void Start() {
            CreateEventSubscriptions();
        }

        public virtual void OrchestratorStart() {
            //Debug.Log(gameObject.name + ".InteractableOption.OrchestratorStart()");
            SetupScriptableObjects();
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
            unitAudio = GetComponent<UnitAudioController>();
            if (interactable == null) {
                //Debug.Log(gameObject.name + ".InteractableOption.GetComponentReferences(): " + interactable is null);
            }
            namePlateUnit = GetComponent<INamePlateUnit>();

            componentReferencesInitialized = true;
        }

        public virtual void HandleConfirmAction() {
            SystemEventManager.MyInstance.NotifyOnInteractionWithOptionCompleted(this);
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
            (PlayerManager.MyInstance.MyCharacter.CharacterController as PlayerController).StopInteract();
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
            //Debug.Log(gameObject.name + ".InteractableOption.CanShowMiniMapIcon()");
            return (GetCurrentOptionCount() > 0);
        }

        public virtual string GetDescription() {
            return string.Format("<color=#ffff00ff>{0}</color>", GetSummary());
        }

        public virtual string GetSummary() {
            return string.Format("{0}", DisplayName);
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
            if (SystemEventManager.MyInstance == null) {
                Debug.LogError("SystemEventManager not found in the scene.  Is the GameManager in the scene?");
                return;
                //SystemEventManager.MyInstance.OnPlayerUnitSpawn += HandlePlayerUnitSpawn;
            }
            if (PlayerManager.MyInstance == null) {
                Debug.LogError("PlayerManager not found. Is the GameManager in the scene?");
                return;
            }
            eventSubscriptionsInitialized = true;
        }

        public virtual void CleanupEventSubscriptions() {
            if (SystemEventManager.MyInstance != null) {
                //SystemEventManager.MyInstance.OnPlayerUnitSpawn -= HandlePlayerUnitSpawn;
            }
            eventSubscriptionsInitialized = false;
        }

        public virtual void HandlePlayerUnitSpawn() {
            //Debug.Log(gameObject.name + ".InteractableOption.HandlePlayerUnitSpawn()");
            if (prerequisiteConditions != null && prerequisiteConditions.Count > 0) {
                foreach (PrerequisiteConditions tmpPrerequisiteConditions in prerequisiteConditions) {
                    if (tmpPrerequisiteConditions != null) {
                        tmpPrerequisiteConditions.UpdatePrerequisites(false);
                    }
                }
                /*
                if (MyPrerequisitesMet) {
                    HandlePrerequisiteUpdates();
                }
                */
            } else {
                //HandlePrerequisiteUpdates();
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