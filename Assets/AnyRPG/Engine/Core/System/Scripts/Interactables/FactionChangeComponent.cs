using AnyRPG;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class FactionChangeComponent : InteractableOptionComponent {

        public FactionChangeProps Props { get => interactableOptionProps as FactionChangeProps; }

        private bool windowEventSubscriptionsInitialized = false;

        public FactionChangeComponent(Interactable interactable, FactionChangeProps interactableOptionProps) : base(interactable, interactableOptionProps) {
            if (interactableOptionProps.GetInteractionPanelTitle() == string.Empty) {
                interactableOptionProps.InteractionPanelTitle = Props.Faction.DisplayName + " Faction";
            }
        }

        public override void CreateEventSubscriptions() {
            //Debug.Log("GatheringNode.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized) {
                return;
            }
            base.CreateEventSubscriptions();

            // because the class is a special type of prerequisite, we need to be notified when it changes
            if (SystemEventManager.Instance == null) {
                Debug.LogError("SystemEventManager Not Found.  Is the GameManager prefab in the scene?");
                return;
            }
            SystemEventManager.StartListening("OnFactionChange", HandleFactionChange);
        }

        public void CleanupEventSubscriptions(ICloseableWindowContents windowContents) {
            //Debug.Log(gameObject.name + ".FactionChangeInteractable.CleanupEventSubscriptions(ICloseableWindowContents)");
            CleanupWindowEventSubscriptions();
        }

        public void CleanupWindowEventSubscriptions() {
            if (PopupWindowManager.Instance != null
                && PopupWindowManager.Instance.factionChangeWindow != null
                && PopupWindowManager.Instance.factionChangeWindow.CloseableWindowContents != null
                && (PopupWindowManager.Instance.factionChangeWindow.CloseableWindowContents as NameChangePanelController) != null) {
                (PopupWindowManager.Instance.factionChangeWindow.CloseableWindowContents as FactionChangePanelController).OnConfirmAction -= HandleConfirmAction;
                (PopupWindowManager.Instance.factionChangeWindow.CloseableWindowContents as FactionChangePanelController).OnCloseWindow -= CleanupEventSubscriptions;
            }
        }

        public override void CleanupEventSubscriptions() {
            //Debug.Log(gameObject.name + ".FactionChangeInteractable.CleanupEventSubscriptions()");
            base.CleanupEventSubscriptions();
            CleanupWindowEventSubscriptions();
            SystemEventManager.StopListening("OnFactionChange", HandleFactionChange);
        }

        public override void HandleConfirmAction() {
            //Debug.Log(gameObject.name + ".FactionChangeInteractable.HandleConfirmAction()");
            base.HandleConfirmAction();

            // just to be safe
            CleanupWindowEventSubscriptions();
        }

        public void HandleFactionChange(string eventName, EventParamProperties eventParamProperties) {
            HandlePrerequisiteUpdates();
        }

        public override bool Interact(CharacterUnit source, int optionIndex = 0) {
            //Debug.Log(gameObject.name + ".FactionChangeInteractable.Interact()");
            if (windowEventSubscriptionsInitialized == true) {
                return false;
            }
            base.Interact(source, optionIndex);
            (PopupWindowManager.Instance.factionChangeWindow.CloseableWindowContents as FactionChangePanelController).Setup(Props.Faction);
            (PopupWindowManager.Instance.factionChangeWindow.CloseableWindowContents as FactionChangePanelController).OnConfirmAction += HandleConfirmAction;
            (PopupWindowManager.Instance.factionChangeWindow.CloseableWindowContents as FactionChangePanelController).OnCloseWindow += CleanupEventSubscriptions;
            windowEventSubscriptionsInitialized = true;
            return true;
        }

        /// <summary>
        /// Pick an item up off the ground and put it in the inventory
        /// </summary>

        public override void StopInteract() {
            base.StopInteract();
            PopupWindowManager.Instance.factionChangeWindow.CloseWindow();
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

        // faction is a special type of prerequisite
        public override bool MyPrerequisitesMet {
            get {
                if (PlayerManager.Instance.MyCharacter.Faction == Props.Faction) {
                    return false;
                }
                return base.MyPrerequisitesMet;
            }
        }

    }

}