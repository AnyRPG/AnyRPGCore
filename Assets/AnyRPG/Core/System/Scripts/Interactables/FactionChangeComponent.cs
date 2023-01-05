using AnyRPG;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class FactionChangeComponent : InteractableOptionComponent {

        // game manager references
        private FactionChangeManager factionChangeManager = null;

        public FactionChangeProps Props { get => interactableOptionProps as FactionChangeProps; }

        private bool windowEventSubscriptionsInitialized = false;

        public FactionChangeComponent(Interactable interactable, FactionChangeProps interactableOptionProps, SystemGameManager systemGameManager) : base(interactable, interactableOptionProps, systemGameManager) {
            if (interactableOptionProps.GetInteractionPanelTitle() == string.Empty) {
                interactableOptionProps.InteractionPanelTitle = Props.Faction.DisplayName + " Faction";
            }
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            factionChangeManager = systemGameManager.FactionChangeManager;
        }

        public override void ProcessCreateEventSubscriptions() {
            //Debug.Log("GatheringNode.CreateEventSubscriptions()");
            base.ProcessCreateEventSubscriptions();

            SystemEventManager.StartListening("OnFactionChange", HandleFactionChange);
        }

        //public void CleanupEventSubscriptions(ICloseableWindowContents windowContents) {
        public void CleanupEventSubscriptions(CloseableWindowContents windowContents) {
            //Debug.Log(gameObject.name + ".FactionChangeInteractable.CleanupEventSubscriptions(ICloseableWindowContents)");
            CleanupWindowEventSubscriptions();
        }

        public void CleanupWindowEventSubscriptions() {

            factionChangeManager.OnConfirmAction -= HandleConfirmAction;
            factionChangeManager.OnEndInteraction -= CleanupWindowEventSubscriptions;

            windowEventSubscriptionsInitialized = false;
        }

        public override void ProcessCleanupEventSubscriptions() {
            //Debug.Log(gameObject.name + ".FactionChangeInteractable.CleanupEventSubscriptions()");
            base.ProcessCleanupEventSubscriptions();
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
            //Debug.Log(interactable.gameObject.name + ".FactionChangeInteractable.Interact()");
            if (windowEventSubscriptionsInitialized == true) {
                return false;
            }
            base.Interact(source, optionIndex);
            factionChangeManager.SetDisplayFaction(Props.Faction);
            uIManager.factionChangeWindow.OpenWindow();
            factionChangeManager.OnConfirmAction += HandleConfirmAction;
            factionChangeManager.OnEndInteraction += CleanupWindowEventSubscriptions;
            windowEventSubscriptionsInitialized = true;
            return true;
        }

        public override void StopInteract() {
            base.StopInteract();
            uIManager.factionChangeWindow.CloseWindow();
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
        public override bool PrerequisitesMet {
            get {
                if (playerManager.MyCharacter.Faction == Props.Faction) {
                    return false;
                }
                return base.PrerequisitesMet;
            }
        }

        public override bool PlayInteractionSound() {
            return true;
        }


    }

}