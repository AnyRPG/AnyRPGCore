using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class ClickSwitchComponent : ControlSwitchComponent {

        public override event System.Action<InteractableOptionComponent> MiniMapStatusUpdateHandler = delegate { };

        public ClickSwitchComponent(Interactable interactable, ControlSwitchProps interactableOptionProps) : base(interactable, interactableOptionProps) {
        }

       
    }

}