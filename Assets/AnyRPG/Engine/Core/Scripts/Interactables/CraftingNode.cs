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

        public override InteractableOptionProps InteractableOptionProps { get => craftingNodeProps; }
    }

}