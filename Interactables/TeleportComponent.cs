using AnyRPG;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class TeleportComponent : PortalComponent {

        public override event Action<IInteractable> MiniMapStatusUpdateHandler = delegate { };

        private BaseAbility ability = null;

        public BaseAbility BaseAbility { get => ability; }

        public TeleportComponent(Interactable interactable, TeleportProps interactableOptionProps) : base(interactable, interactableOptionProps) {
            this.interactableOptionProps = interactableOptionProps;
        }


        public override bool Interact(CharacterUnit source) {
            //Debug.Log(gameObject.name + ".PortalInteractable.Interact()");
            base.Interact(source);

            source.BaseCharacter.CharacterAbilityManager.BeginAbility(ability);
            return true;
        }

        public override void SetupScriptableObjects() {
            base.SetupScriptableObjects();
            if ((interactableOptionProps as TeleportProps).AbilityName != null && (interactableOptionProps as TeleportProps).AbilityName != string.Empty) {
                BaseAbility baseAbility = SystemAbilityManager.MyInstance.GetResource((interactableOptionProps as TeleportProps).AbilityName);
                if (baseAbility != null) {
                    ability = baseAbility;
                } else {
                    Debug.LogError("TeleportComponent.SetupScriptableObjects(): COULD NOT FIND ABILITY " + (interactableOptionProps as TeleportProps).AbilityName + " while initializing.");
                }
            }
        }


    }
}