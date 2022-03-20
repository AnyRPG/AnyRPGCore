using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class PatrolInteractable : InteractableOption {

        [SerializeField]
        private PatrolProps patrolProps = new PatrolProps();

        public override InteractableOptionProps InteractableOptionProps { get => patrolProps; }
    }

}