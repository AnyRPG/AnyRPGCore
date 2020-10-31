using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [CreateAssetMenu(fileName = "New Specialization Change Config", menuName = "AnyRPG/Interactable/SpecializationChangeConfig")]
    public class SpecializationChangeConfig : InteractableOptionConfig {

        [SerializeField]
        private SpecializationChangeProps interactableOptionProps = new SpecializationChangeProps();

        [Tooltip("the class Specialization that this interactable option offers")]
        [SerializeField]
        private string specializationName = string.Empty;

        public string SpecializationName { get => specializationName; set => specializationName = value; }
        public SpecializationChangeProps InteractableOptionProps { get => interactableOptionProps; set => interactableOptionProps = value; }
    }

}