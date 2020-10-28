using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [CreateAssetMenu(fileName = "New Control Switch Config", menuName = "AnyRPG/Interactable/ControlSwitchConfig")]
    public class ControlSwitchConfig : InteractableOptionConfig {

        [SerializeField]
        private ControlSwitchProps interactableOptionProps = new ControlSwitchProps();

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

    }

}