using AnyRPG;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class ClassChangeComponent : InteractableOptionComponent {

        // game manager references
        private ClassChangeManager classChangeManager = null;

        public ClassChangeProps Props { get => interactableOptionProps as ClassChangeProps; }

        public ClassChangeComponent(Interactable interactable, ClassChangeProps interactableOptionProps, SystemGameManager systemGameManager) : base(interactable, interactableOptionProps, systemGameManager) {
            if (interactableOptionProps.GetInteractionPanelTitle() == string.Empty) {
                interactableOptionProps.InteractionPanelTitle = Props.CharacterClass.DisplayName + " Class";
            }
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            classChangeManager = systemGameManager.ClassChangeManager;
        }

        public override void ProcessCreateEventSubscriptions() {
            //Debug.Log("GatheringNode.CreateEventSubscriptions()");
            base.ProcessCreateEventSubscriptions();

            systemEventManager.OnClassChange += HandleClassChange;
        }

        public override void ProcessCleanupEventSubscriptions() {
            //Debug.Log(gameObject.name + ".ClassChangeInteractable.CleanupEventSubscriptions()");
            base.ProcessCleanupEventSubscriptions();
            systemEventManager.OnClassChange -= HandleClassChange;
        }

        public void HandleClassChange(CharacterClass oldCharacterClass, CharacterClass newCharacterClass) {
            HandlePrerequisiteUpdates();
        }

        public override bool Interact(CharacterUnit source, int optionIndex = 0) {
            //Debug.Log(gameObject.name + ".ClassChangeInteractable.Interact()");
            base.Interact(source, optionIndex);

            classChangeManager.SetDisplayClass(Props.CharacterClass, this);
            uIManager.classChangeWindow.OpenWindow();

            return true;
        }

        public override void StopInteract() {
            base.StopInteract();
            uIManager.classChangeWindow.CloseWindow();
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
        public override bool PrerequisitesMet {
            get {
                if (playerManager.MyCharacter.CharacterClass == Props.CharacterClass) {
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