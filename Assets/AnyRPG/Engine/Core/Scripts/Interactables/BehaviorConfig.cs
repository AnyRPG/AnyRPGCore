using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [CreateAssetMenu(fileName = "New Behavior Config", menuName = "AnyRPG/Interactable/BehaviorConfig")]
    public class BehaviorConfig : InteractableOptionConfig {

        [SerializeField]
        private BehaviorProps interactableOptionProps = new BehaviorProps();

        public override InteractableOptionProps InteractableOptionProps { get => interactableOptionProps; }

    }

}