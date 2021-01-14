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

        public override InteractableOptionProps InteractableOptionProps { get => specializationChangeProps; }
    }

}