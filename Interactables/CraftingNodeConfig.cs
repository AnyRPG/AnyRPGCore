using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [CreateAssetMenu(fileName = "New Crafting Node Config", menuName = "AnyRPG/Interactable/CraftingNodeConfig")]
    [System.Serializable]
    public class CraftingNodeConfig : InteractableOptionConfig {

        [Tooltip("The ability to cast in order to craft with this node")]
        [SerializeField]
        private string abilityName = string.Empty;


        public InteractableOption GetInteractableOption(Interactable interactable) {
            return new CraftingNode(interactable, this);
        }
    }

}