using AnyRPG;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class CraftingNode : InteractableOption {

        [SerializeField]
        private CraftingNodeProps craftingNodeProps = new CraftingNodeProps();

        [Tooltip("The ability to cast in order to mine this node")]
        [SerializeField]
        private string abilityName = string.Empty;

        public override InteractableOptionProps InteractableOptionProps { get => craftingNodeProps; }
    }

}