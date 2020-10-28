using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [System.Serializable]
    public class PressureSwitchProps : ControlSwitchProps {

        [Tooltip("the minimum amount of weight needed to activate this switch")]
        [SerializeField]
        private float minimumWeight = 0f;

        public override InteractableOption GetInteractableOption(Interactable interactable) {
            return new PressureSwitch(interactable, this);
        }
    }

}