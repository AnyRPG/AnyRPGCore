namespace AnyRPG {

    [System.Serializable]
    public class ClickSwitchProps : ControlSwitchProps {

        public override InteractableOptionComponent GetInteractableOption(InteractableBase interactable, InteractableOption interactableOption = null) {
            InteractableOptionComponent returnValue = new ClickSwitchComponent(interactable, this, systemGameManager);
            if (interactableOption != null) {
                interactableOption.SetComponent(returnValue);
            }
            return returnValue;
        }
    }

}