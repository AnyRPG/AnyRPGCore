using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [System.Serializable]
    public class ClickSwitchProps : ControlSwitchProps {

        public override InteractableOptionComponent GetInteractableOption(Interactable interactable, InteractableOption interactableOption = null) {
            InteractableOptionComponent returnValue = new ClickSwitchComponent(interactable, this);
            if (interactableOption != null) {
                interactableOption.SetComponent(returnValue);
            }
            return returnValue;
        }
    }

}