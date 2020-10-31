using AnyRPG;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class SpecializationChangeInteractable : InteractableOption {

        [SerializeField]
        private SpecializationChangeProps specializationChangeProps = new SpecializationChangeProps();

        [Tooltip("the class Specialization that this interactable option offers")]
        [SerializeField]
        private string specializationName = string.Empty;

        public override InteractableOptionProps InteractableOptionProps { get => specializationChangeProps; }
    }

}