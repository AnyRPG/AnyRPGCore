using AnyRPG;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class TeleportComponent : PortalComponent {

        public override event Action<InteractableOptionComponent> MiniMapStatusUpdateHandler = delegate { };

        public TeleportProps TeleportProps { get => interactableOptionProps as TeleportProps; }


        public TeleportComponent(Interactable interactable, TeleportProps interactableOptionProps) : base(interactable, interactableOptionProps) {
        }


        public override bool Interact(CharacterUnit source) {
            //Debug.Log(gameObject.name + ".PortalInteractable.Interact()");
            base.Interact(source);

            source.BaseCharacter.CharacterAbilityManager.BeginAbility(TeleportProps.BaseAbility);
            return true;
        }


    }
}