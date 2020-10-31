using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [System.Serializable]
    public class GatheringNodeProps : LootableNodeProps {

        [Header("Gathering Node")]

        [Tooltip("The ability to cast in order to gather from this node")]
        [SerializeField]
        private string abilityName = string.Empty;

        public string AbilityName { get => abilityName; set => abilityName = value; }

        public override InteractableOptionComponent GetInteractableOption(Interactable interactable) {
            return new GatheringNodeComponent(interactable, this);
        }
    }

}