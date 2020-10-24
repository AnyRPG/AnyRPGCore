using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [CreateAssetMenu(fileName = "New Gathering Node Config", menuName = "AnyRPG/Interactable/GatheringNodeConfig")]
    [System.Serializable]
    public class GatheringNodeConfig : LootableNodeConfig {

        [Header("Gathering Node")]

        [Tooltip("The ability to cast in order to gather from this node")]
        [SerializeField]
        private string abilityName = string.Empty;

        public override InteractableOption GetInteractableOption(Interactable interactable) {
            return new GatheringNode(interactable, this);
        }
    }

}