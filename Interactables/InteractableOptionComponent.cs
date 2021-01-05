using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public abstract class InteractableOptionComponent : IPrerequisiteOwner {

        public abstract event System.Action<InteractableOptionComponent> MiniMapStatusUpdateHandler;

        protected Interactable interactable = null;
        protected InteractableOptionProps interactableOptionProps = null;

        protected bool eventSubscriptionsInitialized = false;

        public Interactable Interactable { get => interactable; set => interactable = value; }
        public virtual InteractableOptionProps InteractableOptionProps { get => interactableOptionProps; }
        public virtual string DisplayName {
            get {
                if (interactableOptionProps.InteractionPanelTitle != null && interactableOptionProps.InteractionPanelTitle != string.Empty) {
                    return interactableOptionProps.InteractionPanelTitle;
                }
                if (interactable != null) {
                    return interactable.DisplayName;
                }
                return "interactable is null!";
            }
        }

        public virtual bool MyPrerequisitesMet {
            get {
                //Debug.Log(gameObject.name + ".InteractableOption.MyPrerequisitesMet");
                foreach (PrerequisiteConditions prerequisiteCondition in interactableOptionProps.PrerequisiteConditions) {
                    if (!prerequisiteCondition.IsMet()) {
                        return false;
                    }
                }
                // there are no prerequisites, or all prerequisites are complete
                return true;
            }
        }

        public InteractableOptionComponent(Interactable interactable, InteractableOptionProps interactableOptionProps) {
            this.interactable = interactable;
            this.interactableOptionProps = interactableOptionProps;
            SetupScriptableObjects();
            CreateEventSubscriptions();
        }

        /*
        public virtual void Init() {
            //AddUnitProfileSettings();
            CreateEventSubscriptions();
        }
        */

        public virtual void Cleanup() {
            CleanupEventSubscriptions();
            CleanupScriptableObjects();
        }

        /*
        protected virtual void AddUnitProfileSettings() {
            // do nothing here
        }
        */

        public virtual void ProcessStatusIndicatorSourceInit() {
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

        public virtual void HandleConfirmAction() {
            SystemEventManager.MyInstance.NotifyOnInteractionWithOptionCompleted(this);
        }

        public virtual bool ProcessFactionValue(float factionValue) {
            return (factionValue >= 0f ? true : false);
        }

        public virtual bool ProcessCombatOnly() {
            if (interactable.CombatOnly == true) {
                return false;
            }
            return true;
        }

        public virtual bool CanInteract(bool processRangeCheck = false, bool passedRangeCheck = false, float factionValue = 0f) {
            //Debug.Log(interactable.gameObject.name + this.ToString() + ".InteractableOptionComponent.CanInteract(" + processRangeCheck + ", " + passedRangeCheck + ", " + factionValue + ")");
            if (processRangeCheck == true && passedRangeCheck == false) {
                //Debug.Log(interactable.gameObject.name + ".InteractableOptionComponent.Interact(): range check failed");
                return false;
            }
            if (ProcessFactionValue(factionValue) == false) {
                //Debug.Log(interactable.gameObject.name + ".InteractableOptionComponent.Interact(): faction check failed");
                return false;
            }
            if (ProcessCombatOnly() == false) {
                //Debug.Log(interactable.gameObject.name + ".InteractableOptionComponent.Interact(): combatOnly check failed");
                return false;
            }

            bool returnValue = MyPrerequisitesMet;
            if (returnValue == false) {
                //Debug.Log(interactable.gameObject.name + this.ToString() + ".InteractableOptionComponent.Interact(): prerequisites not met");
            }
            return returnValue;
        }

        public virtual bool Interact(CharacterUnit source) {
            //Debug.Log(interactable.gameObject.name + ".InteractableOptionComponent.Interact()");
            //source.CancelMountEffects();
            SystemEventManager.MyInstance.NotifyOnInteractionWithOptionStarted(this);
            return true;
        }

        public virtual void StopInteract() {
            //Debug.Log(gameObject.name + ".InanimateUnit.StopInteract()");
            PlayerManager.MyInstance.PlayerController.StopInteract();
        }

        public virtual bool HasMiniMapText() {
            return false;
        }

        public virtual bool HasMiniMapIcon() {
            return (interactableOptionProps.NamePlateImage != null);
        }

        public virtual bool SetMiniMapText(TextMeshProUGUI text) {
            return (GetCurrentOptionCount() > 0);
        }

        public virtual void SetMiniMapIcon(Image icon) {
            //Debug.Log(gameObject.name + ".InteractableOption.SetMiniMapIcon()");
            if (CanShowMiniMapIcon()) {
                icon.sprite = interactableOptionProps.NamePlateImage;
                icon.color = Color.white;
            } else {
                icon.sprite = null;
                icon.color = new Color32(0, 0, 0, 0);
            }
            return;
        }

        public virtual bool CanShowMiniMapIcon() {
            //Debug.Log(gameObject.name + ".InteractableOption.CanShowMiniMapIcon()");
            if (interactable.CombatOnly) {
                return false;
            }
            return (GetCurrentOptionCount() > 0);
        }

        public virtual string GetDescription() {
            return string.Format("<color=#ffff00ff>{0}</color>", GetSummary());
        }

        public virtual string GetSummary() {
            return string.Format("{0}", interactableOptionProps.InteractionPanelTitle);
        }
        

        public virtual void HandlePlayerUnitSpawn() {
            //Debug.Log(interactable.gameObject.name + ".InteractableOption.HandlePlayerUnitSpawn()");

            if (interactableOptionProps.PrerequisiteConditions != null && interactableOptionProps.PrerequisiteConditions.Count > 0) {
                foreach (PrerequisiteConditions tmpPrerequisiteConditions in interactableOptionProps.PrerequisiteConditions) {
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
            /*
            if (processRangeCheck == true && passedRangeCheck == false) {
                return 0;
            }
            */
            if (interactable.CombatOnly) {
                return 0;
            }
            return (MyPrerequisitesMet == true ? 1 : 0);
        }

        public virtual int GetCurrentOptionCount() {
            // overwrite me or everything is valid as long as prerequisites are met, which isn't the case for things like dialog, which have multiple options
            //Debug.Log(gameObject.name + ".CharacterCreatorInteractable.GetCurrentOptionCount()");
            if (interactable.CombatOnly) {
                return 0;
            }
            return GetValidOptionCount();
        }

        public virtual void HandlePrerequisiteUpdates() {
            Debug.Log(interactable.gameObject.name + this.ToString() + ".InteractableOption.HandlePrerequisiteUpdates()");
            if (interactable != null) {
                interactable.HandlePrerequisiteUpdates();
            }
            CallMiniMapStatusUpdateHandler();
        }

        public virtual void CallMiniMapStatusUpdateHandler() {
            //MiniMapStatusUpdateHandler(this);
        }

        public virtual void SetupScriptableObjects() {
            //Debug.Log(interactable.gameObject.name + ".InteractableOptionComponent.SetupScriptableObjects()");
            if (interactableOptionProps.PrerequisiteConditions != null) {
                foreach (PrerequisiteConditions tmpPrerequisiteConditions in interactableOptionProps.PrerequisiteConditions) {
                    if (tmpPrerequisiteConditions != null) {
                        tmpPrerequisiteConditions.SetupScriptableObjects(this);
                    }
                }
            }

            //interactableOptionProps.SetupScriptableObjects();
        }

        public virtual void CleanupScriptableObjects() {
            if (interactableOptionProps.PrerequisiteConditions != null) {
                foreach (PrerequisiteConditions tmpPrerequisiteConditions in interactableOptionProps.PrerequisiteConditions) {
                    if (tmpPrerequisiteConditions != null) {
                        tmpPrerequisiteConditions.CleanupScriptableObjects(this);
                    }
                }
            }

        }


    }

}