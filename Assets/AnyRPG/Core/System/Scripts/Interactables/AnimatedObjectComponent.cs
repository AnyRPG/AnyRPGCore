using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class AnimatedObjectComponent : InteractableOptionComponent {

        public AnimatedObjectProps Props { get => interactableOptionProps as AnimatedObjectProps; }

        // by default it is considered closed when not using the sheathed position
        private bool objectOpen = false;

        public AnimatedObjectComponent(Interactable interactable, AnimatedObjectProps interactableOptionProps, SystemGameManager systemGameManager) : base(interactable, interactableOptionProps, systemGameManager) {
            interactableOptionProps.InteractionPanelTitle = "Interactable";
        }

        public override bool CanInteract(bool processRangeCheck = false, bool passedRangeCheck = false, float factionValue = 0, bool processNonCombatCheck = true) {

            if (Props.SwitchOnly == true) {
                return false;
            }
            return base.CanInteract(processRangeCheck, passedRangeCheck, factionValue, processNonCombatCheck);
        }

        public override bool Interact(CharacterUnit source, int optionIndex = 0) {
            //Debug.Log(gameObject.name + ".AnimatedObject.Interact(" + (source == null ? "null" : source.name) +")");
            base.Interact(source, optionIndex);
            uIManager.interactionWindow.CloseWindow();

            if (Props.AnimationComponent == null) {
                Debug.Log("AnimatedObjectComponent.Interact(): Animation component was null");
                return false;
            }
            ChooseMovement();

            return false;
        }

        private void ChooseMovement() {
            if (objectOpen) {
                Props.AnimationComponent.Play(Props.CloseAnimationClip.name);
                if (Props.OpenAudioClip != null) {
                    interactable.UnitComponentController.PlayEffectSound(Props.CloseAudioClip);
                }
                objectOpen = false;
            } else {
                Props.AnimationComponent.Play(Props.OpenAnimationClip.name);
                if (Props.CloseAudioClip != null) {
                    interactable.UnitComponentController.PlayEffectSound(Props.OpenAudioClip);
                }
                objectOpen = true;
            }
        }


    }

}