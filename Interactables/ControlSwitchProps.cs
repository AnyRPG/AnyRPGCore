using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [System.Serializable]
    public class ControlSwitchProps : InteractableOptionProps {

        [Header("Control Switch")]

        [Tooltip("When successfully activated, this switch will call Interact() on the following interactables")]
        [SerializeField]
        List<InteractableOptionComponent> controlObjects = new List<InteractableOptionComponent>();

        [Tooltip("all these switches must be in the onState for this switch to activate")]
        [SerializeField]
        private List<ControlSwitchComponent> switchGroup = new List<ControlSwitchComponent>();

        [Tooltip("The number of times this object can be activated.  0 is unlimited")]
        [SerializeField]
        private int activationLimit = 0;

        public List<InteractableOptionComponent> ControlObjects { get => controlObjects; set => controlObjects = value; }
        public List<ControlSwitchComponent> SwitchGroup { get => switchGroup; set => switchGroup = value; }
        public int ActivationLimit { get => activationLimit; set => activationLimit = value; }

        public override InteractableOptionComponent GetInteractableOption(Interactable interactable) {
            return new ControlSwitchComponent(interactable, this);
        }
    }

}