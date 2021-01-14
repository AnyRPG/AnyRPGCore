using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class PressureSwitch : ControlSwitch {

        [SerializeField]
        private PressureSwitchProps pressureSwitchProps = new PressureSwitchProps();

        public override InteractableOptionProps InteractableOptionProps { get => pressureSwitchProps; }
    }

}