using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [CreateAssetMenu(fileName = "New Patrol Config", menuName = "AnyRPG/Interactable/Patrol Config")]
    public class PatrolConfig : InteractableOptionConfig {

        [SerializeField]
        private PatrolProps interactableOptionProps = new PatrolProps();

        public override InteractableOptionProps InteractableOptionProps { get => interactableOptionProps; }
    }

}