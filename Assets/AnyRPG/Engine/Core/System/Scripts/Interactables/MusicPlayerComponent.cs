using AnyRPG;
using UnityEngine;

namespace AnyRPG {
    public class MusicPlayerComponent : InteractableOptionComponent {

        public MusicPlayerProps Props { get => interactableOptionProps as MusicPlayerProps; }

        public MusicPlayerComponent(Interactable interactable, MusicPlayerProps interactableOptionProps, SystemGameManager systemGameManager) : base(interactable, interactableOptionProps, systemGameManager) {
            if (interactableOptionProps.GetInteractionPanelTitle() == string.Empty) {
                interactableOptionProps.InteractionPanelTitle = "Music Player";
            }
        }

        public override bool Interact(CharacterUnit source, int optionIndex = 0) {
            //Debug.Log(gameObject.name + ".SkillTrainer.Interact(" + source + ")");
            base.Interact(source, optionIndex);
            if (!uIManager.musicPlayerWindow.IsOpen) {
                //Debug.Log(source + " interacting with " + gameObject.name);
                uIManager.musicPlayerWindow.OpenWindow();
                (uIManager.musicPlayerWindow.CloseableWindowContents as MusicPlayerUI).ShowMusicProfiles(this);
                return true;
            }
            return false;
        }

        public override void StopInteract() {
            //Debug.Log(gameObject.name + ".SkillTrainer.StopInteract()");
            base.StopInteract();
            //vendorUI.ClearPages();
            uIManager.musicPlayerWindow.CloseWindow();
        }

    }

}