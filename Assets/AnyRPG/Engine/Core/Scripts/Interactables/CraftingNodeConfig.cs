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

        [SerializeField]
        private CraftingNodeProps interactableOptionProps = new CraftingNodeProps();

        public override InteractableOptionProps InteractableOptionProps { get => interactableOptionProps; }
    }

}