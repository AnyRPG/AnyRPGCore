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

        public override InteractableOptionProps InteractableOptionProps { get => behaviorProps; }
    }

}