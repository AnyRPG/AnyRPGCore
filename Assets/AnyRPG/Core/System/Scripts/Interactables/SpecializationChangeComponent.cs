using AnyRPG;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class SpecializationChangeComponent : InteractableOptionComponent {

        // game manager references
        private SpecializationChangeManager specializationChangeManager = null;

        public SpecializationChangeProps Props { get => interactableOptionProps as SpecializationChangeProps; }

        public SpecializationChangeComponent(Interactable interactable, SpecializationChangeProps interactableOptionProps, SystemGameManager systemGameManager) : base(interactable, interactableOptionProps, systemGameManager) {
            if (interactableOptionProps.GetInteractionPanelTitle() == string.Empty) {
                interactableOptionProps.InteractionPanelTitle = Props.ClassSpecialization.DisplayName + " Specialization";
            }
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            specializationChangeManager = systemGameManager.SpecializationChangeManager;
        }

        public override void ProcessCreateEventSubscriptions() {
            //Debug.Log("GatheringNode.CreateEventSubscriptions()");
            base.ProcessCreateEventSubscriptions();

            // because the class is a special type of prerequisite, we need to be notified when it changes
            SystemEventManager.StartListening("OnSpecializationChange", HandleSpecializationChange);
            systemEventManager.OnClassChange += HandleClassChange;
        }

        public override void ProcessCleanupEventSubscriptions() {
            //Debug.Log(gameObject.name + ".ClassChangeInteractable.CleanupEventSubscriptions()");
            base.ProcessCleanupEventSubscriptions();

            SystemEventManager.StopListening("OnSpecializationChange", HandleSpecializationChange);
            if (systemEventManager != null) {
                systemEventManager.OnClassChange -= HandleClassChange;
            }
        }

        public override bool Interact(CharacterUnit source, int optionIndex = 0) {
            //Debug.Log(gameObject.name + ".ClassChangeInteractable.Interact()");
            base.Interact(source, optionIndex);

            specializationChangeManager.SetDisplaySpecialization(Props.ClassSpecialization, this);
            uIManager.specializationChangeWindow.OpenWindow();

            return true;
        }

        public override void StopInteract() {
            base.StopInteract();
            uIManager.specializationChangeWindow.CloseWindow();
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

        public void HandleSpecializationChange(string eventName, EventParamProperties eventParamProperties) {
            HandlePrerequisiteUpdates();
        }

        public void HandleClassChange(CharacterClass oldCharacterClass, CharacterClass newCharacterClass) {
            HandlePrerequisiteUpdates();
        }

        // specialization is a special type of prerequisite
        public override bool PrerequisitesMet {
            get {
                if (playerManager.MyCharacter.ClassSpecialization == Props.ClassSpecialization) {
                    return false;
                }
                if (Props.ClassSpecialization.CharacterClasses.Contains(playerManager.MyCharacter.CharacterClass) == false) {
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