using AnyRPG;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class ClassChangeComponent : InteractableOptionComponent {

        // game manager references
        private ClassChangeManagerClient classChangeManagerClient = null;

        public ClassChangeProps Props { get => interactableOptionProps as ClassChangeProps; }

        public ClassChangeComponent(Interactable interactable, ClassChangeProps interactableOptionProps, SystemGameManager systemGameManager) : base(interactable, interactableOptionProps, systemGameManager) {
            if (interactionPanelTitle == string.Empty) {
                interactionPanelTitle = Props.CharacterClass.DisplayName + " Class";
            }
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            classChangeManagerClient = systemGameManager.ClassChangeManager;
        }

        public override void ProcessCreateEventSubscriptions() {
            //Debug.Log("GatheringNode.CreateEventSubscriptions()");
            base.ProcessCreateEventSubscriptions();

            systemEventManager.OnClassChange += HandleClassChange;
        }

        public override void ProcessCleanupEventSubscriptions() {
            //Debug.Log($"{gameObject.name}.ClassChangeInteractable.CleanupEventSubscriptions()");
            base.ProcessCleanupEventSubscriptions();
            systemEventManager.OnClassChange -= HandleClassChange;
        }

        public void HandleClassChange(UnitController sourceUnitController, CharacterClass oldCharacterClass, CharacterClass newCharacterClass) {
            HandlePrerequisiteUpdates(sourceUnitController);
        }

        public override bool ProcessInteract(UnitController sourceUnitController, int componentIndex, int choiceIndex = 0) {
            //Debug.Log($"{gameObject.name}.ClassChangeInteractable.Interact()");
            base.ProcessInteract(sourceUnitController, componentIndex, choiceIndex);

            //interactionManager.InteractWithClassChangeComponent(sourceUnitController, this, optionIndex);

            return true;
        }

        public override void ClientInteraction(UnitController sourceUnitController, int componentIndex, int choiceIndex) {
            //Debug.Log($"{interactable.gameObject.name}.ClassChangeComponent.ClientInteraction({sourceUnitController.gameObject.name}, {componentIndex}, {choiceIndex})");

            base.ClientInteraction(sourceUnitController, componentIndex, choiceIndex);
            classChangeManagerClient.SetProps(Props, this, componentIndex, choiceIndex);

            uIManager.classChangeWindow.OpenWindow();
        }

        public override void StopInteract() {
            base.StopInteract();
            uIManager.classChangeWindow.CloseWindow();
        }

        public override int GetCurrentOptionCount(UnitController sourceUnitController) {
            //Debug.Log($"{gameObject.name}.CharacterCreatorInteractable.GetCurrentOptionCount()");
            return GetValidOptionCount(sourceUnitController);
        }

        // character class is a special type of prerequisite
        public override bool PrerequisitesMet(UnitController sourceUnitController) {
                if (sourceUnitController.BaseCharacter.CharacterClass == Props.CharacterClass) {
                    return false;
                }
                return base.PrerequisitesMet(sourceUnitController);
        }

        public void ChangeCharacterClass(UnitController sourceUnitController) {
            sourceUnitController.BaseCharacter.ChangeCharacterClass(Props.CharacterClass);
            NotifyOnConfirmAction(sourceUnitController);
        }

        //public override bool PlayInteractionSound() {
        //    return true;
        //}


    }

}