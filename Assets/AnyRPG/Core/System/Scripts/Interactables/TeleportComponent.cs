using System;
using System.Collections;
using UnityEngine;

namespace AnyRPG {
    public class TeleportComponent : PortalComponent {

        public TeleportProps TeleportProps { get => interactableOptionProps as TeleportProps; }

        public TeleportComponent(Interactable interactable, TeleportProps interactableOptionProps, SystemGameManager systemGameManager) : base(interactable, interactableOptionProps, systemGameManager) {
        }

        public override bool ProcessInteract(UnitController sourceUnitController, int componentIndex, int choiceIndex) {
            //Debug.Log($"{interactable.gameObject.name}.TeleportComponent.ProcessInteract({sourceUnitController.gameObject.name}, {componentIndex})");

            base.ProcessInteract(sourceUnitController, componentIndex, choiceIndex);

            sourceUnitController.CharacterAbilityManager.BeginAbility(TeleportProps.BaseAbility.AbilityProperties);
            return true;
        }


    }
}