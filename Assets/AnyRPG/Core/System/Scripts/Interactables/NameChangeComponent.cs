using AnyRPG;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class NameChangeComponent : InteractableOptionComponent {

        // game manager references
        NameChangeManager nameChangeManager = null;

        public NameChangeProps Props { get => interactableOptionProps as NameChangeProps; }

        public NameChangeComponent(Interactable interactable, NameChangeProps interactableOptionProps, SystemGameManager systemGameManager) : base(interactable, interactableOptionProps, systemGameManager) {
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            nameChangeManager = systemGameManager.NameChangeManager;
        }

        public override bool Interact(CharacterUnit source, int optionIndex = 0) {
            //Debug.Log(gameObject.name + ".NameChangeInteractable.Interact()");
            
            base.Interact(source, optionIndex);

            nameChangeManager.BeginInteraction(this);
            uIManager.nameChangeWindow.OpenWindow();
            return true;
        }

        public override void StopInteract() {
            base.StopInteract();
            uIManager.nameChangeWindow.CloseWindow();
        }

        public override bool HasMiniMapText() {
            return true;
        }

        public override bool SetMiniMapText(TextMeshProUGUI text) {
            //Debug.Log(gameObject.name + ".NameChangeInteractable.SetMiniMapText(" + text + ")");
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
            //Debug.Log(interactable.gameObject.name + ".NameChangeInteractable.GetCurrentOptionCount(): returning " + GetValidOptionCount());
            return GetValidOptionCount();
        }

        //public override bool PlayInteractionSound() {
        //    return true;
        //}


    }

}