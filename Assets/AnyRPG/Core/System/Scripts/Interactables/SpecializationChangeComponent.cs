using AnyRPG;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class SpecializationChangeComponent : InteractableOptionComponent {

        // game manager references
        private SpecializationChangeManagerClient specializationChangeManager = null;

        public SpecializationChangeProps Props { get => interactableOptionProps as SpecializationChangeProps; }

        public SpecializationChangeComponent(Interactable interactable, SpecializationChangeProps interactableOptionProps, SystemGameManager systemGameManager) : base(interactable, interactableOptionProps, systemGameManager) {
            if (interactionPanelTitle == string.Empty) {
                interactionPanelTitle = Props.ClassSpecialization.DisplayName + " Specialization";
            }
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            specializationChangeManager = systemGameManager.SpecializationChangeManagerClient;
        }

        public override void ProcessCreateEventSubscriptions() {
            //Debug.Log("GatheringNode.CreateEventSubscriptions()");
            base.ProcessCreateEventSubscriptions();

            // because the class is a special type of prerequisite, we need to be notified when it changes
            systemEventManager.OnSpecializationChange += HandleSpecializationChange;
            systemEventManager.OnClassChange += HandleClassChange;
        }

        public override void ProcessCleanupEventSubscriptions() {
            //Debug.Log($"{gameObject.name}.ClassChangeInteractable.CleanupEventSubscriptions()");
            base.ProcessCleanupEventSubscriptions();

            if (systemEventManager != null) {
                systemEventManager.OnSpecializationChange -= HandleSpecializationChange;
                systemEventManager.OnClassChange -= HandleClassChange;
            }
        }

        public override bool ProcessInteract(UnitController sourceUnitController, int componentIndex, int choiceIndex) {
            //Debug.Log($"{gameObject.name}.ClassChangeInteractable.Interact()");
            base.ProcessInteract(sourceUnitController, componentIndex, choiceIndex);

            return true;
        }

        public override void ClientInteraction(UnitController sourceUnitController, int componentIndex, int choiceIndex) {
            base.ClientInteraction(sourceUnitController, componentIndex, choiceIndex);

            specializationChangeManager.SetProps(Props, this, componentIndex, choiceIndex);
            uIManager.specializationChangeWindow.OpenWindow();
        }

        public override void StopInteract() {
            base.StopInteract();
            uIManager.specializationChangeWindow.CloseWindow();
        }

        public override int GetCurrentOptionCount(UnitController sourceUnitController) {
            //Debug.Log($"{gameObject.name}.CharacterCreatorInteractable.GetCurrentOptionCount()");
            return GetValidOptionCount(sourceUnitController);
        }

        public void HandleSpecializationChange(UnitController sourceUnitController, ClassSpecialization newClassSpecialization, ClassSpecialization oldClassSpecialization) {
            HandlePrerequisiteUpdates(sourceUnitController);
        }

        public void HandleClassChange(UnitController sourceUnitController, CharacterClass oldCharacterClass, CharacterClass newCharacterClass) {
            HandlePrerequisiteUpdates(sourceUnitController);
        }

        // specialization is a special type of prerequisite
        public override bool PrerequisitesMet(UnitController sourceUnitController) {
                if (sourceUnitController.BaseCharacter.ClassSpecialization == Props.ClassSpecialization) {
                    return false;
                }
                if (Props.ClassSpecialization.CharacterClasses.Contains(sourceUnitController.BaseCharacter.CharacterClass) == false) {
                    return false;
                }
                return base.PrerequisitesMet(sourceUnitController);
        }

        public void ChangeCharacterSpecialization(UnitController sourceUnitController) {
            sourceUnitController.BaseCharacter.ChangeClassSpecialization(Props.ClassSpecialization);
            NotifyOnConfirmAction(sourceUnitController);
        }

        //public override bool PlayInteractionSound() {
        //    return true;
        //}



    }

}