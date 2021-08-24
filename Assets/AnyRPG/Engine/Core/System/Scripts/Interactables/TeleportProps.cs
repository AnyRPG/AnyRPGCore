using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [System.Serializable]
    public class TeleportProps : PortalProps {

        [Header("Teleport")]

        [Tooltip("When interacted with, the player will cast this ability. Only applies if Portal Type is Ability.")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(BaseAbility))]
        private string abilityName = string.Empty;

        private BaseAbility ability = null;

        public BaseAbility BaseAbility { get => ability; }

        public override InteractableOptionComponent GetInteractableOption(Interactable interactable, InteractableOption interactableOption = null) {
            InteractableOptionComponent returnValue = new TeleportComponent(interactable, this, systemGameManager);
            if (interactableOption != null) {
                interactableOption.SetComponent(returnValue);
            }
            return returnValue;
        }

        public override void SetupScriptableObjects(SystemGameManager systemGameManager) {
            base.SetupScriptableObjects(systemGameManager);

            if (abilityName != null && abilityName != string.Empty) {
                BaseAbility baseAbility = systemDataFactory.GetResource<BaseAbility>(abilityName);
                if (baseAbility != null) {
                    ability = baseAbility;
                } else {
                    Debug.LogError("TeleportComponent.SetupScriptableObjects(): COULD NOT FIND ABILITY " + abilityName + " while initializing.");
                }
            }

        }

    }

}