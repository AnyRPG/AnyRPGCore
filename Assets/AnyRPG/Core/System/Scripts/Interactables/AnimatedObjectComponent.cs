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
            interactionPanelTitle = "Interactable";
        }

        public override bool CanInteract(UnitController sourceUnitController, bool processRangeCheck, bool passedRangeCheck, bool processNonCombatCheck, bool viaSwitch = false) {

            if (Props.SwitchOnly == true && viaSwitch == false) {
                return false;
            }
            return base.CanInteract(sourceUnitController, processRangeCheck, passedRangeCheck, processNonCombatCheck);
        }

        public override bool ProcessInteract(UnitController sourceUnitController, int componentIndex, int choiceIndex = 0) {
            //Debug.Log($"{gameObject.name}.AnimatedObject.Interact(" + (source == null ? "null" : source.name) +")");
            base.ProcessInteract(sourceUnitController, componentIndex, choiceIndex);

            if (Props.AnimationComponent == null) {
                //Debug.Log("AnimatedObjectComponent.Interact(): Animation component was null");
                return false;
            }
            ChooseMovement(sourceUnitController, componentIndex);

            return true;
        }

        public override void ClientInteraction(UnitController sourceUnitController, int componentIndex, int choiceIndex) {
            uIManager.interactionWindow.CloseWindow();
        }

        public void ChooseMovement(UnitController sourceUnitController, int componentIndex) {
            //interactable.InteractableEventController.NotifyOnAnimatedObjectChooseMovement(sourceUnitController, optionIndex);
            if (objectOpen) {
                if (systemGameManager.GameMode == GameMode.Local || networkManagerServer.ServerModeActive == true) {
                    Props.AnimationComponent.Play(Props.CloseAnimationClip.name);
                }
                if (Props.OpenAudioClip != null) {
                    interactable.UnitComponentController.PlayEffectSound(Props.CloseAudioClip);
                }
                objectOpen = false;
            } else {
                if (systemGameManager.GameMode == GameMode.Local || networkManagerServer.ServerModeActive == true) {
                    Props.AnimationComponent.Play(Props.OpenAnimationClip.name);
                }
                if (Props.CloseAudioClip != null) {
                    interactable.UnitComponentController.PlayEffectSound(Props.OpenAudioClip);
                }
                objectOpen = true;
            }
        }


    }

}