using AnyRPG;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class CharacterCreatorComponent : InteractableOptionComponent {

        private CharacterCreatorManager characterCreatorManager = null;
        private CharacterCreatorInteractableManager characterCreatorInteractableManager = null;

        public CharacterCreatorProps Props { get => interactableOptionProps as CharacterCreatorProps; }

        public CharacterCreatorComponent(Interactable interactable, CharacterCreatorProps interactableOptionProps, SystemGameManager systemGameManager) : base(interactable, interactableOptionProps, systemGameManager) {
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            characterCreatorManager = systemGameManager.CharacterCreatorManager;
            characterCreatorInteractableManager = systemGameManager.CharacterCreatorInteractableManager;
        }

        public override bool Interact(CharacterUnit source, int optionIndex = 0) {
            // was there a reason why we didn't have base.Interact here before or just an oversight?
            base.Interact(source, optionIndex);
            // moved to coroutine because UMA will crash here due to its use of DestroyImmediate in the case where an UMAData was attached to the model.
            characterCreatorInteractableManager.SetCharacterCreator(this);
            interactable.StartCoroutine(OpenWindowWait());
            
            return true;
        }

        public IEnumerator OpenWindowWait() {
            yield return null;
            OpenWindow();
        }

        public void OpenWindow() {
            uIManager.characterCreatorWindow.OpenWindow();
        }

        public override void StopInteract() {
            base.StopInteract();
            uIManager.characterCreatorWindow.CloseWindow();
        }

        public override bool HasMiniMapText() {
            return true;
        }

        public override bool SetMiniMapText(TextMeshProUGUI text) {
            //Debug.Log(gameObject.name + ".CharacterCreatorInteractable.SetMiniMapText(" + text + ")");
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

    }

}