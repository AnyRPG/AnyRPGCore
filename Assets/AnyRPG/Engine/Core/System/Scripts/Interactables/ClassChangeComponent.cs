using AnyRPG;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class ClassChangeComponent : InteractableOptionComponent {

        public ClassChangeProps Props { get => interactableOptionProps as ClassChangeProps; }

        private bool windowEventSubscriptionsInitialized = false;

        public ClassChangeComponent(Interactable interactable, ClassChangeProps interactableOptionProps) : base(interactable, interactableOptionProps) {
            if (interactableOptionProps.GetInteractionPanelTitle() == string.Empty) {
                interactableOptionProps.InteractionPanelTitle = Props.CharacterClass.DisplayName + " Class";
            }
        }

        public void CleanupEventSubscriptions(ICloseableWindowContents windowContents) {
            //Debug.Log(gameObject.name + ".ClassChangeInteractable.CleanupEventSubscriptions(ICloseableWindowContents)");
            CleanupWindowEventSubscriptions();
        }

        public void CleanupWindowEventSubscriptions() {
            if (PopupWindowManager.Instance != null && PopupWindowManager.Instance.classChangeWindow != null && PopupWindowManager.Instance.classChangeWindow.CloseableWindowContents != null && (PopupWindowManager.Instance.classChangeWindow.CloseableWindowContents as NameChangePanelController) != null) {
                (PopupWindowManager.Instance.classChangeWindow.CloseableWindowContents as ClassChangePanelController).OnConfirmAction -= HandleConfirmAction;
                (PopupWindowManager.Instance.classChangeWindow.CloseableWindowContents as ClassChangePanelController).OnCloseWindow -= CleanupEventSubscriptions;
            }
            windowEventSubscriptionsInitialized = false;
        }

        public override void CleanupEventSubscriptions() {
            //Debug.Log(gameObject.name + ".ClassChangeInteractable.CleanupEventSubscriptions()");
            base.CleanupEventSubscriptions();
            CleanupWindowEventSubscriptions();
            if (SystemEventManager.MyInstance != null) {
                SystemEventManager.MyInstance.OnClassChange -= HandleClassChange;
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
            SystemEventManager.MyInstance.OnClassChange += HandleClassChange;
        }

        public void HandleClassChange(CharacterClass oldCharacterClass, CharacterClass newCharacterClass) {
            HandlePrerequisiteUpdates();
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

            (PopupWindowManager.Instance.classChangeWindow.CloseableWindowContents as ClassChangePanelController).Setup(Props.CharacterClass);
            (PopupWindowManager.Instance.classChangeWindow.CloseableWindowContents as ClassChangePanelController).OnConfirmAction += HandleConfirmAction;
            (PopupWindowManager.Instance.classChangeWindow.CloseableWindowContents as ClassChangePanelController).OnCloseWindow += CleanupEventSubscriptions;
            windowEventSubscriptionsInitialized = true;
            return true;
        }

        /// <summary>
        /// Pick an item up off the ground and put it in the inventory
        /// </summary>

        public override void StopInteract() {
            base.StopInteract();
            PopupWindowManager.Instance.classChangeWindow.CloseWindow();
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
            text.color = Color.cyan;
            return true;
        }

        public override int GetCurrentOptionCount() {
            //Debug.Log(gameObject.name + ".CharacterCreatorInteractable.GetCurrentOptionCount()");
            return GetValidOptionCount();
        }

        // character class is a special type of prerequisite
        public override bool MyPrerequisitesMet {
            get {
                if (PlayerManager.MyInstance.MyCharacter.CharacterClass == Props.CharacterClass) {
                    return false;
                }
                return base.MyPrerequisitesMet;
            }
        }

    }

}