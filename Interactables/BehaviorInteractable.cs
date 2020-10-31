using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class BehaviorInteractable : InteractableOption {

        [SerializeField]
        private BehaviorProps behaviorProps = new BehaviorProps();

        [Header("Behavior")]

        [SerializeField]
        private List<string> behaviorNames = new List<string>();

        [Tooltip("instantiate a new behavior profile or not when loading behavior profiles")]
        [SerializeField]
        private bool useBehaviorCopy = false;

        public override InteractableOptionProps InteractableOptionProps { get => behaviorProps; }
    }

}