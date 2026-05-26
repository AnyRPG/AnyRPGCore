using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public class PressureSwitchProps : ControlSwitchProps {

        [Tooltip("the minimum amount of weight needed to activate this switch")]
        [SerializeField]
        private float minimumWeight = 0f;

        public float MinimumWeight { get => minimumWeight; set => minimumWeight = value; }

        public override InteractableOptionComponent GetInteractableOption(Interactable interactable, InteractableOption interactableOption = null) {
            InteractableOptionComponent returnValue = new PressureSwitchComponent(interactable, this, systemGameManager);
            if (interactableOption != null) {
                interactableOption.SetComponent(returnValue);
            }
            return returnValue;
        }
    }

}