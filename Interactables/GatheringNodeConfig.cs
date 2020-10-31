using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [CreateAssetMenu(fileName = "New Gathering Node Config", menuName = "AnyRPG/Interactable/GatheringNodeConfig")]
    [System.Serializable]
    public class GatheringNodeConfig : InteractableOptionConfig {

        [SerializeField]
        private GatheringNodeProps interactableOptionProps = new GatheringNodeProps();

        [Header("Gathering Node")]

        [Tooltip("The ability to cast in order to gather from this node")]
        [SerializeField]
        private string abilityName = string.Empty;

        public GatheringNodeProps InteractableOptionProps { get => interactableOptionProps; set => interactableOptionProps = value; }
    }

}