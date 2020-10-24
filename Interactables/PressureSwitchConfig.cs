using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [CreateAssetMenu(fileName = "New Pressure Switch Config", menuName = "AnyRPG/Interactable/PressureSwitchConfig")]
    [System.Serializable]
    public class PressureSwitchConfig : ControlSwitchConfig {

        [Tooltip("the minimum amount of weight needed to activate this switch")]
        [SerializeField]
        private float minimumWeight = 0f;

        public override InteractableOption GetInteractableOption(Interactable interactable) {
            return new PressureSwitch(interactable, this);
        }
    }

}