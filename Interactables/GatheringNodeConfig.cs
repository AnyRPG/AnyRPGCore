using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [CreateAssetMenu(fileName = "New Gathering Node Config", menuName = "AnyRPG/Interactable/GatheringNodeConfig")]
    public class GatheringNodeConfig : InteractableOptionConfig {

        [SerializeField]
        private GatheringNodeProps interactableOptionProps = new GatheringNodeProps();

        public override InteractableOptionProps InteractableOptionProps { get => interactableOptionProps; }
    }

}