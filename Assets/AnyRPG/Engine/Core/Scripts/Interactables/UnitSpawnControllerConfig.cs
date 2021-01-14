using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [CreateAssetMenu(fileName = "New Unit Spawn Controller Config", menuName = "AnyRPG/Interactable/UnitSpawnControllerConfig")]
    public class UnitSpawnControllerConfig : InteractableOptionConfig {

        [SerializeField]
        private UnitSpawnControllerProps interactableOptionProps = new UnitSpawnControllerProps();

        public override InteractableOptionProps InteractableOptionProps { get => interactableOptionProps; }
    }

}