using AnyRPG;
using UnityEngine;

namespace AnyRPG {
    public class MusicPlayerComponent : InteractableOptionComponent {

        // game manager references
        MusicPlayerManager musicPlayerManager = null;

        public MusicPlayerProps Props { get => interactableOptionProps as MusicPlayerProps; }

        public MusicPlayerComponent(Interactable interactable, MusicPlayerProps interactableOptionProps, SystemGameManager systemGameManager) : base(interactable, interactableOptionProps, systemGameManager) {
            if (interactionPanelTitle == string.Empty) {
                interactionPanelTitle = "Music Player";
            }
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            musicPlayerManager = systemGameManager.MusicPlayerManager;
        }

        public override bool ProcessInteract(UnitController sourceUnitController, int componentIndex, int choiceIndex) {
            //Debug.Log($"{gameObject.name}.SkillTrainer.Interact(" + source + ")");
            base.ProcessInteract(sourceUnitController, componentIndex, choiceIndex);
            
            return true;
        }

        public override void ClientInteraction(UnitController sourceUnitController, int componentIndex, int choiceIndex) {
            base.ClientInteraction(sourceUnitController, componentIndex, choiceIndex);

            if (!uIManager.musicPlayerWindow.IsOpen) {
                //Debug.Log(source + " interacting with " + gameObject.name);
                musicPlayerManager.SetMusicPlayerProps(Props, this, componentIndex, choiceIndex);
                uIManager.musicPlayerWindow.OpenWindow();
            }
        }

        public override void StopInteract() {
            //Debug.Log($"{gameObject.name}.SkillTrainer.StopInteract()");
            base.StopInteract();
            uIManager.musicPlayerWindow.CloseWindow();
        }

        //public override bool PlayInteractionSound() {
        //    return true;
        //}


    }

}