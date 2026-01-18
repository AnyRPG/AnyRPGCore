using AnyRPG;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class FactionChangeComponent : InteractableOptionComponent {

        // game manager references
        private FactionChangeManagerClient factionChangeManager = null;

        public FactionChangeProps Props { get => interactableOptionProps as FactionChangeProps; }

        public FactionChangeComponent(Interactable interactable, FactionChangeProps interactableOptionProps, SystemGameManager systemGameManager) : base(interactable, interactableOptionProps, systemGameManager) {
            if (interactionPanelTitle == string.Empty) {
                interactionPanelTitle = Props.Faction.DisplayName + " Faction";
            }
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            factionChangeManager = systemGameManager.FactionChangeManagerClient;
        }

        public override void ProcessCreateEventSubscriptions() {
            //Debug.Log("GatheringNode.CreateEventSubscriptions()");
            base.ProcessCreateEventSubscriptions();

            systemEventManager.OnFactionChange += HandleFactionChange;
        }

        public override void ProcessCleanupEventSubscriptions() {
            //Debug.Log($"{gameObject.name}.FactionChangeInteractable.CleanupEventSubscriptions()");
            base.ProcessCleanupEventSubscriptions();

            systemEventManager.OnFactionChange -= HandleFactionChange;
        }

        public void HandleFactionChange() {
            HandleOptionStateChange();
        }

        public override bool ProcessInteract(UnitController sourceUnitController, int componentIndex, int choiceIndex) {
            //Debug.Log(interactable.gameObject.name + ".FactionChangeInteractable.Interact()");

            base.ProcessInteract(sourceUnitController, componentIndex, choiceIndex);

            return true;
        }

        public override void ClientInteraction(UnitController sourceUnitController, int componentIndex, int choiceIndex) {
            base.ClientInteraction(sourceUnitController, componentIndex, choiceIndex);

            factionChangeManager.SetProps(Props, this, componentIndex, choiceIndex);
            uIManager.factionChangeWindow.OpenWindow();
        }

        public override void StopInteract() {
            base.StopInteract();
            uIManager.factionChangeWindow.CloseWindow();
        }

        public override int GetCurrentOptionCount(UnitController sourceUnitController) {
            //Debug.Log($"{gameObject.name}.CharacterCreatorInteractable.GetCurrentOptionCount()");
            return GetValidOptionCount(sourceUnitController);
        }

        // faction is a special type of prerequisite
        public override bool PrerequisitesMet(UnitController sourceUnitController) {
                if (sourceUnitController.BaseCharacter.Faction == Props.Faction) {
                    return false;
                }
                return base.PrerequisitesMet(sourceUnitController);
        }

        public void ChangeCharacterFaction(UnitController sourceUnitController) {
            sourceUnitController.BaseCharacter.ChangeCharacterFaction(Props.Faction);
            NotifyOnConfirmAction(sourceUnitController);
        }

        //public override bool PlayInteractionSound() {
        //    return true;
        //}


    }

}