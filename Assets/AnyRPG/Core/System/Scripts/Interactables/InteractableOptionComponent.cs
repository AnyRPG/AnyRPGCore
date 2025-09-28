using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public abstract class InteractableOptionComponent : ConfiguredClass, IPrerequisiteOwner {

        protected Interactable interactable = null;
        protected InteractableOptionProps interactableOptionProps = null;

        protected bool eventSubscriptionsInitialized = false;

        protected string interactionPanelTitle = string.Empty;

        // game manager references
        protected SystemEventManager systemEventManager = null;
        protected PlayerManager playerManager = null;
        protected PlayerManagerServer playerManagerServer = null;
        protected UIManager uIManager = null;
        protected InteractionManager interactionManager = null;

        public Interactable Interactable { get => interactable; set => interactable = value; }
        public virtual InteractableOptionProps InteractableOptionProps { get => interactableOptionProps; }
        public virtual int PriorityValue { get => 0; }
        public virtual bool BlockTooltip { get => false; }
        public virtual string DisplayName {
            get {
                if (interactionPanelTitle != string.Empty) {
                    return interactionPanelTitle;
                }
                if (interactable != null) {
                    return interactable.DisplayName;
                }
                return "interactable is null!";
            }
        }

        public virtual bool PrerequisitesMet(UnitController sourceUnitController) {
                //Debug.Log($"{gameObject.name}.InteractableOption.MyPrerequisitesMet");
                foreach (PrerequisiteConditions prerequisiteCondition in interactableOptionProps.PrerequisiteConditions) {
                    if (!prerequisiteCondition.IsMet(sourceUnitController)) {
                        return false;
                    }
                }
                // there are no prerequisites, or all prerequisites are complete
                return true;
        }

        public InteractableOptionComponent(Interactable interactable, InteractableOptionProps interactableOptionProps, SystemGameManager systemGameManager) {
            //Debug.Log(interactable.gameObject.name + ".InteractableOptionComponent(" + interactable.gameObject.name + ", " + (systemGameManager == null ? "null" : systemGameManager.gameObject.name) + ")");
            this.interactable = interactable;
            this.interactableOptionProps = interactableOptionProps;
            interactionPanelTitle = interactableOptionProps.InteractionPanelTitle;
            Configure(systemGameManager);
            SetupScriptableObjects();
            CreateEventSubscriptions();
        }

        public override void SetGameManagerReferences() {
            //Debug.Log(interactable.gameObject.name + ".InteractableOptionComponent.SetGameManagerReferences");
            base.SetGameManagerReferences();
            systemEventManager = systemGameManager.SystemEventManager;
            playerManager = systemGameManager.PlayerManager;
            uIManager = systemGameManager.UIManager;
            playerManagerServer = systemGameManager.PlayerManagerServer;
            interactionManager = systemGameManager.InteractionManager;
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

        public void CreateEventSubscriptions() {
            if (eventSubscriptionsInitialized) {
                return;
            }
            ProcessCreateEventSubscriptions();
            eventSubscriptionsInitialized = true;
        }

        public virtual void ProcessCreateEventSubscriptions() {
        }

        public void CleanupEventSubscriptions() {
            /*
            if (!eventSubscriptionsInitialized) {
                return;
            }
            */
            ProcessCleanupEventSubscriptions();
            eventSubscriptionsInitialized = false;
        }

        public virtual void ProcessCleanupEventSubscriptions() {
        }

        public virtual string GetOptionChoiceName(UnitController sourceUnitController, int choiceIndex) {
            return DisplayName;
        }

        public virtual void NotifyOnConfirmAction(UnitController sourceUnitController) {
            //Debug.Log($"{interactable.gameObject.name}.InteractableOptionComponent.NotifyOnConfirmAction({sourceUnitController?.gameObject.name})");

            sourceUnitController.UnitEventController.NotifyOnCompleteInteractWithOption(this);
            systemEventManager.NotifyOnCompleteInteractWithOption(sourceUnitController, this);
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

        public virtual bool NonCombatOptionsAvailable() {
            if (interactable.NonCombatOptionsAvailable == false) {
                return false;
            }
            return true;
        }

        public virtual bool CanInteract(UnitController sourceUnitController, bool processRangeCheck, bool passedRangeCheck, bool processNonCombatCheck, bool viaSwitch = false) {
            //Debug.Log(interactable.gameObject.name + this.ToString() + ".InteractableOptionComponent.CanInteract(" + processRangeCheck + ", " + passedRangeCheck + ", " + factionValue + ")");
            if (processRangeCheck == true && passedRangeCheck == false) {
                //Debug.Log($"{interactable.gameObject.name}.InteractableOptionComponent.Interact(): range check failed");
                return false;
            }
            if (ProcessCombatOnly() == false) {
                //Debug.Log($"{interactable.gameObject.name}.InteractableOptionComponent.Interact(): combatOnly check failed");
                return false;
            }
            if (processNonCombatCheck == true && NonCombatOptionsAvailable() == false) {
                //Debug.Log($"{interactable.gameObject.name}.InteractableOptionComponent.Interact(): non-combat options not available");
                return false;
            }

            bool returnValue = PrerequisitesMet(sourceUnitController);
            if (returnValue == false) {
                //Debug.Log(interactable.gameObject.name + this.ToString() + ".InteractableOptionComponent.Interact(): prerequisites not met");
            }
            return returnValue;
        }

        public virtual bool Interact(UnitController sourceUnitController, int componentIndex, int choiceIndex) {
            //Debug.Log($"{interactable.gameObject.name}.InteractableOptionComponent.Interact({sourceUnitController?.gameObject.name}, {componentIndex}, {choiceIndex}) : {this.GetType()}");

            //source.CancelMountEffects();
            bool returnValue = ProcessInteract(sourceUnitController, componentIndex, choiceIndex);
            if (returnValue == true) {
                ProcessClientNotifications(sourceUnitController, componentIndex, choiceIndex);
            }
            return returnValue;
        }

        public virtual void ProcessClientNotifications(UnitController sourceUnitController, int componentIndex, int choiceIndex) {
            //Debug.Log($"{interactable.gameObject.name}.InteractableOptionComponent.ProcessClientNotifications({(sourceUnitController == null ? "null" : sourceUnitController.gameObject.name)}, {componentIndex}, {choiceIndex})");

            if (sourceUnitController != null) {
                // trigger network client interaction
                interactable.NotifyOnInteractionWithOptionStarted(sourceUnitController, componentIndex, choiceIndex);
                // trigger local client interaction
                sourceUnitController.UnitEventController.NotifyOnStartInteractWithOption(this, componentIndex, choiceIndex);
                // trigger system event notification
                systemEventManager.NotifyOnStartInteractWithOption(sourceUnitController, this, componentIndex, choiceIndex);
            }
        }

        public virtual bool ProcessInteract(UnitController sourceUnitController, int componentIndex, int choiceIndex) {
            return true;
        }

        public virtual void StopInteract() {
            //Debug.Log($"{gameObject.name}.InanimateUnit.StopInteract()");
            playerManager.PlayerController.StopInteract();
        }

        public virtual void ProcessStartInteract(int componentIndex, int choiceIndex) {
            interactable.ProcessStartInteractWithOption(this, componentIndex, choiceIndex);
        }

        public virtual void ProcessStopInteract() {
            interactable.ProcessStopInteractWithOption(this);
        }

        public virtual bool PlayInteractionSound() {
            return false;
        }

        /// <summary>
        /// called by the player manager on the client when the player interacts
        /// </summary>
        /// <param name="sourceUnitController"></param>
        /// <param name="componentIndex"></param>
        public virtual void ClientInteraction(UnitController sourceUnitController, int componentIndex, int choiceIndex) {
            //Debug.Log($"{interactable.gameObject.name}.InteractableOptionComponent.ClientInteraction({sourceUnitController?.gameObject.name}, {componentIndex}, {choiceIndex})");
            // handle client-only stuff in child classes
        }

        public virtual AudioClip GetInteractionSound(VoiceProps voiceProps) {
            return voiceProps.RandomStartInteract;
        }


        public virtual bool HasMiniMapText() {
            return false;
        }

        public virtual bool HasMainMapText() {
            return false;
        }

        public virtual bool HasMiniMapIcon() {
            return (interactableOptionProps.NamePlateImage != null);
        }

        public virtual bool HasMainMapIcon() {
            return false;
        }

        public virtual bool SetMiniMapText(TextMeshProUGUI text) {
            //Debug.Log($"{interactable.gameObject.name}.InteractableOptionComponent.SetMiniMapText()");
            if (playerManager.UnitController == null) {
                return false;
            }
            return (GetCurrentOptionCount(playerManager.UnitController) > 0);
        }

        public virtual void SetMiniMapIcon(Image icon) {
            //Debug.Log($"{gameObject.name}.InteractableOption.SetMiniMapIcon()");
            if (CanShowMiniMapIcon(playerManager.UnitController)) {
                icon.sprite = GetMiniMapIcon();
                icon.color = GetMiniMapIconColor();
            } else {
                icon.sprite = null;
                icon.color = new Color32(0, 0, 0, 0);
            }
            return;
        }

        public virtual Sprite GetMiniMapIcon() {
            return interactableOptionProps.NamePlateImage;
        }

        public virtual Color GetMiniMapIconColor() {
            return Color.white;
        }

        public virtual bool CanShowMiniMapIcon(UnitController sourceUnitController) {
            //Debug.Log($"{gameObject.name}.InteractableOption.CanShowMiniMapIcon()");
            if (sourceUnitController == null) {
                return false;
            }
            return (GetCurrentOptionCount(sourceUnitController) > 0);
        }

        public virtual string GetDescription() {
            return string.Format("<color=#ffff00ff>{0}</color>", GetSummary(playerManager.UnitController));
        }

        public virtual string GetSummary(UnitController sourceUnitController) {
            return string.Format("{0}", GetInteractionButtonText(sourceUnitController));
        }

        public virtual string GetInteractionButtonText(UnitController sourceUnitController, int componentIndex = 0, int choiceIndex = 0) {
            return interactionPanelTitle;
        }

        public virtual void HandlePlayerUnitSpawn(UnitController sourceUnitController) {
            //Debug.Log(interactable.gameObject.name + ".InteractableOption.HandlePlayerUnitSpawn()");

            if (interactableOptionProps.PrerequisiteConditions != null && interactableOptionProps.PrerequisiteConditions.Count > 0) {
                foreach (PrerequisiteConditions tmpPrerequisiteConditions in interactableOptionProps.PrerequisiteConditions) {
                    if (tmpPrerequisiteConditions != null) {
                        tmpPrerequisiteConditions.UpdatePrerequisites(sourceUnitController, false);
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

            CallMiniMapStatusUpdateHandler();
        }


        public virtual int GetValidOptionCount(UnitController sourceUnitController) {
            // overwrite me if this type of interactable option has a list of options instead of just one
            /*
            if (processRangeCheck == true && passedRangeCheck == false) {
                return 0;
            }
            */
            if (interactable.CombatOnly) {
                return 0;
            }
            return (PrerequisitesMet(sourceUnitController) == true ? 1 : 0);
        }

        public virtual int GetCurrentOptionCount(UnitController sourceUnitController) {
            // overwrite me or everything is valid as long as prerequisites are met, which isn't the case for things like dialog, which have multiple options
            //Debug.Log($"{gameObject.name}.CharacterCreatorInteractable.GetCurrentOptionCount()");
            if (interactable.CombatOnly) {
                return 0;
            }
            return GetValidOptionCount(sourceUnitController);
        }

        public virtual void HandlePrerequisiteUpdates(UnitController sourceUnitController) {
            //Debug.Log(interactable.gameObject.name + this.ToString() + ".InteractableOption.HandlePrerequisiteUpdates()");
            HandleOptionStateChange();
        }

        /// <summary>
        /// trigger to update minimap
        /// </summary>
        public void HandleOptionStateChange() {
            //Debug.Log($"{interactable.gameObject.name}.InteractableOptionComponent.HandleOptionStateChange()");

            if (interactable != null) {
                interactable.HandlePrerequisiteUpdates();
            }
            CallMiniMapStatusUpdateHandler();
        }

        public void CallMiniMapStatusUpdateHandler() {
            //Debug.Log($"{interactable.gameObject.name}.InteractableOptionComponent.CallMiniMapStatusUpdateHandler()");

            interactable?.HandleMiniMapStatusUpdate(this);
        }

        public int GetSwitchOptionIndex(UnitController sourceUnitController) {
            //Debug.Log($"{interactable.gameObject.name}.InteractableOptionComponent.GetSwitchOptionIndex(): {this.GetType()}");

            Dictionary<int, InteractableOptionComponent> allOptions = interactable.GetSwitchInteractables(sourceUnitController);
            foreach (int optionIndex in allOptions.Keys) {
                //Debug.Log($"{interactable.gameObject.name}.InteractableOptionComponent.GetSwitchOptionIndex() : checking option {optionIndex} : {allOptions[optionIndex].GetType()}");
                if (allOptions[optionIndex] == this) {
                    return optionIndex;
                }
            }
            //Debug.Log($"{interactable.gameObject.name}.InteractableOptionComponent.GetSwitchOptionIndex() : no match found return -1");
            return -1;
        }

        public int GetOptionIndex() {
            foreach (int optionIndex in interactable.Interactables.Keys) {
                if (interactable.Interactables[optionIndex] == this) {
                    return optionIndex;
                }
            }
            return -1;
        }

        public virtual void SetupScriptableObjects() {
            //Debug.Log(interactable.gameObject.name + ".InteractableOptionComponent.SetupScriptableObjects()");
            if (interactableOptionProps.PrerequisiteConditions != null) {
                foreach (PrerequisiteConditions tmpPrerequisiteConditions in interactableOptionProps.PrerequisiteConditions) {
                    if (tmpPrerequisiteConditions != null) {
                        tmpPrerequisiteConditions.SetupScriptableObjects(systemGameManager, this);
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