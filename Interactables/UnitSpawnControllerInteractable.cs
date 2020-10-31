using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class UnitSpawnControllerInteractable : InteractableOption {

        [SerializeField]
        private UnitSpawnControllerProps unitSpawnControllerProps = new UnitSpawnControllerProps();

        [SerializeField]
        private List<string> unitProfileNames = new List<string>();

        [Tooltip("List of Unit Spawn Nodes to control")]
        [SerializeField]
        private List<UnitSpawnNode> unitSpawnNodeList = new List<UnitSpawnNode>();

        public override InteractableOptionProps InteractableOptionProps { get => unitSpawnControllerProps; }

    }
}