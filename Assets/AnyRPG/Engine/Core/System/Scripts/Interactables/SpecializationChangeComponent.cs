using AnyRPG;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class SpecializationChangeComponent : InteractableOptionComponent {

        public override event Action<InteractableOptionComponent> MiniMapStatusUpdateHandler = delegate { };

        public SpecializationChangeProps Props { get => interactableOptionProps as SpecializationChangeProps; }

        private bool windowEventSubscriptionsInitialized = false;

        public SpecializationChangeComponent(Interactable interactable, SpecializationChangeProps interactableOptionProps) : base(interactable, interactableOptionProps) {
            if (interactableOptionProps.GetInteractionPanelTitle() == string.Empty) {
                interactableOptionProps.InteractionPanelTitle = Props.ClassSpecialization.DisplayName + " Specialization";
            }
        }

        public override void CreateEventSubscriptions() {
            //Debug.Log("GatheringNode.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized) {
                return;
            }
            base.CreateEventSubscriptions();

            // because the class is a special type of prerequisite, we need to be notified when it changes
            if (SystemEventManager.MyInstance == null) {
                Debug.LogError("SystemEventManager Not Found.  Is the GameManager prefab in the scene?");
                return;
            }
            SystemEventManager.StartListening("OnSpecializationChange", HandleSpecializationChange);
            if (SystemEventManager.MyInstance != null) {
                SystemEventManager.MyInstance.OnClassChange += HandleClassChange;
            }

        }

        public void CleanupEventSubscriptions(ICloseableWindowContents windowContents) {
            //Debug.Log(gameObject.name + ".ClassChangeInteractable.CleanupEventSubscriptions(ICloseableWindowContents)");
            CleanupWindowEventSubscriptions();
        }

        public void CleanupWindowEventSubscriptions() {
            if (PopupWindowManager.MyInstance != null && PopupWindowManager.MyInstance.specializationChangeWindow != null && PopupWindowManager.MyInstance.specializationChangeWindow.CloseableWindowContents != null && (PopupWindowManager.MyInstance.specializationChangeWindow.CloseableWindowContents as NameChangePanelController) != null) {
                (PopupWindowManager.MyInstance.specializationChangeWindow.CloseableWindowContents as SpecializationChangePanelController).OnConfirmAction -= HandleConfirmAction;
                (PopupWindowManager.MyInstance.specializationChangeWindow.CloseableWindowContents as SpecializationChangePanelController).OnCloseWindow -= CleanupEventSubscriptions;
            }
            windowEventSubscriptionsInitialized = false;
        }

        public override void CleanupEventSubscriptions() {
            //Debug.Log(gameObject.name + ".ClassChangeInteractable.CleanupEventSubscriptions()");
            base.CleanupEventSubscriptions();
            CleanupWindowEventSubscriptions();
            SystemEventManager.StopListening("OnSpecializationChange", HandleSpecializationChange);
            if (SystemEventManager.MyInstance != null) {
                SystemEventManager.MyInstance.OnClassChange -= HandleClassChange;
            }
        }

        public override void HandleConfirmAction() {
            //Debug.Log(gameObject.name + ".ClassChangeInteractable.HandleConfirmAction()");
            base.HandleConfirmAction();

            // just to be safe
            CleanupWindowEventSubscriptions();
        }

        public override bool Interact(CharacterUnit source, int optionIndex = 0) {
            //Debug.Log(gameObject.name + ".ClassChangeInteractable.Interact()");
            if (windowEventSubscriptionsInitialized == true) {
                return false;
            }
            base.Interact(source, optionIndex);

            (PopupWindowManager.MyInstance.specializationChangeWindow.CloseableWindowContents as SpecializationChangePanelController).Setup(Props.ClassSpecialization);
            (PopupWindowManager.MyInstance.specializationChangeWindow.CloseableWindowContents as SpecializationChangePanelController).OnConfirmAction += HandleConfirmAction;
            (PopupWindowManager.MyInstance.specializationChangeWindow.CloseableWindowContents as SpecializationChangePanelController).OnCloseWindow += CleanupEventSubscriptions;
            windowEventSubscriptionsInitialized = true;
            return true;
        }

        /// <summary>
        /// Pick an item up off the ground and put it in the inventory
        /// </summary>

        public override void StopInteract() {
            base.StopInteract();
            PopupWindowManager.MyInstance.specializationChangeWindow.CloseWindow();
        }

        public override bool HasMiniMapText() {
            return true;
        }

        public override bool SetMiniMapText(TextMeshProUGUI text) {
            if (!base.SetMiniMapText(text)) {
                text.text = "";
                text.color = new Color32(0, 0, 0, 0);
                return false;
            }
            text.text = "o";
            text.fontSize = 50;
            text.color = Color.cyan;
            return true;
        }

        public override int GetCurrentOptionCount() {
            //Debug.Log(gameObject.name + ".CharacterCreatorInteractable.GetCurrentOptionCount()");
            return GetValidOptionCount();
        }

        public override void CallMiniMapStatusUpdateHandler() {
            MiniMapStatusUpdateHandler(this);
        }

        public void HandleSpecializationChange(string eventName, EventParamProperties eventParamProperties) {
            HandlePrerequisiteUpdates();
        }

        public void HandleClassChange(CharacterClass oldCharacterClass, CharacterClass newCharacterClass) {
            HandlePrerequisiteUpdates();
        }

        public override void HandlePlayerUnitSpawn() {
            base.HandlePlayerUnitSpawn();
            MiniMapStatusUpdateHandler(this);
        }

        // specialization is a special type of prerequisite
        public override bool MyPrerequisitesMet {
            get {
                if (PlayerManager.MyInstance.MyCharacter.ClassSpecialization == Props.ClassSpecialization) {
                    return false;
                }
                if (Props.ClassSpecialization.CharacterClasses.Contains(PlayerManager.MyInstance.MyCharacter.CharacterClass) == false) {
                    return false;
                }
                return base.MyPrerequisitesMet;
            }
        }


    }

}