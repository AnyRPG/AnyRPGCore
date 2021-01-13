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
        List<InteractableOption> controlObjects = new List<InteractableOption>();

        [Tooltip("all these switches must be in the onState for this switch to activate")]
        [SerializeField]
        private List<ControlSwitch> switchGroup = new List<ControlSwitch>();

        [Tooltip("The number of times this object can be activated.  0 is unlimited")]
        [SerializeField]
        private int activationLimit = 0;

        public List<InteractableOptionComponent> ControlObjects {
            get {
                List<InteractableOptionComponent> returnList = new List<InteractableOptionComponent>();
                foreach (InteractableOption interactableOption in controlObjects) {
                    if (interactableOption.InteractableOptionComponent != null) {
                        returnList.Add(interactableOption.InteractableOptionComponent);
                    }
                }
                return returnList;
            }
        }
        public List<ControlSwitchComponent> SwitchGroup {
            get {
                List<ControlSwitchComponent> returnList = new List<ControlSwitchComponent>();
                foreach (ControlSwitch controlSwitch in switchGroup) {
                    if (controlSwitch.InteractableOptionComponent != null) {
                        returnList.Add(controlSwitch.InteractableOptionComponent as ControlSwitchComponent);
                    }
                }
                return returnList;
            }
        }
        public int ActivationLimit { get => activationLimit; set => activationLimit = value; }

        public override InteractableOptionComponent GetInteractableOption(Interactable interactable, InteractableOption interactableOption = null) {
            InteractableOptionComponent returnValue = new ControlSwitchComponent(interactable, this);
            if (interactableOption != null) {
                interactableOption.SetComponent(returnValue);
            }
            return returnValue;
        }
    }

}