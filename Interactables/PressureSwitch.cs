using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class PressureSwitch : ControlSwitch {

        [SerializeField]
        private PressureSwitchProps pressureSwitchProps = new PressureSwitchProps();

        [Tooltip("the minimum amount of weight needed to activate this switch")]
        [SerializeField]
        private float minimumWeight = 0f;

        public override InteractableOptionProps InteractableOptionProps { get => pressureSwitchProps; }
    }

}