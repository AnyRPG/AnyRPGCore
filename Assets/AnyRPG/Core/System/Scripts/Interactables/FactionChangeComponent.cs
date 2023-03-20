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

        public override void ProcessCleanupEventSubscriptions() {
            //Debug.Log($"{gameObject.name}.FactionChangeInteractable.CleanupEventSubscriptions()");
            base.ProcessCleanupEventSubscriptions();

            SystemEventManager.StopListening("OnFactionChange", HandleFactionChange);
        }

        public void HandleFactionChange(string eventName, EventParamProperties eventParamProperties) {
            HandlePrerequisiteUpdates();
        }

        public override bool Interact(CharacterUnit source, int optionIndex = 0) {
            //Debug.Log(interactable.gameObject.name + ".FactionChangeInteractable.Interact()");

            base.Interact(source, optionIndex);

            factionChangeManager.SetDisplayFaction(Props.Faction, this);
            uIManager.factionChangeWindow.OpenWindow();

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
            //Debug.Log($"{gameObject.name}.CharacterCreatorInteractable.GetCurrentOptionCount()");
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

        //public override bool PlayInteractionSound() {
        //    return true;
        //}


    }

}