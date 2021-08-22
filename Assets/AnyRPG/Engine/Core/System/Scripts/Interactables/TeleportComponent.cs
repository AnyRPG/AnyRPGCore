using AnyRPG;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class TeleportComponent : PortalComponent {

        public TeleportProps TeleportProps { get => interactableOptionProps as TeleportProps; }

        public TeleportComponent(Interactable interactable, TeleportProps interactableOptionProps, SystemGameManager systemGameManager) : base(interactable, interactableOptionProps, systemGameManager) {
        }

        public override bool Interact(CharacterUnit source, int optionIndex = 0) {
            //Debug.Log(gameObject.name + ".PortalInteractable.Interact()");
            base.Interact(source, optionIndex);

            source.BaseCharacter.CharacterAbilityManager.BeginAbility(TeleportProps.BaseAbility);
            return true;
        }


    }
}