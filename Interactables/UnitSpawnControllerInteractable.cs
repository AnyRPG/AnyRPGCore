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

        public override InteractableOptionProps InteractableOptionProps { get => unitSpawnControllerProps; }

    }
}