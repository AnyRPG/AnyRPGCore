namespace AnyRPG {
    public class InteractionManagerClient : ConfiguredClass {

        public event System.Action<Interactable> OnSetInteractable = delegate { };

        private Interactable currentInteractable = null;
        private InteractableOptionComponent currentInteractableOptionComponent = null;
        private InteractableOptionManager interactableOptionManager = null;

        private PlayerManagerClient playerManagerClient = null;
        private UIManager uIManager = null;
        private InteractionManagerServer interactionManagerServer = null;

        public override void SetGameManagerReferences() {
            //Debug.Log($"InteractionManager.SetGameManagerReferences()");

            base.SetGameManagerReferences();
            playerManagerClient = systemGameManager.PlayerManagerClient;
            uIManager = systemGameManager.UIManager;
            interactionManagerServer = systemGameManager.InteractionManagerServer;
        }

        public void InteractWithInteractable(UnitController sourceUnitController, Interactable target) {
            //Debug.Log($"InteractionManager.Interact({sourceUnitController.gameObject.name}, {target.gameObject.name})");
            
            if (systemGameManager.GameMode == GameMode.Local) {
                interactionManagerServer.InteractWithInteractable(sourceUnitController, target);
            } else {
                networkManagerClient.RequestInteractWithInteractable(target);
            }
        }

        public void InteractWithOption(UnitController sourceUnitController, Interactable targetInteractable, InteractableOptionComponent interactableOptionComponent, int componentIndex, int choiceIndex) {
            //Debug.Log($"InteractionManager.InteractWithOptionClient({sourceUnitController.gameObject.name}, {targetInteractable.gameObject.name}, {componentIndex}, {choiceIndex})");

            if (systemGameManager.GameMode == GameMode.Local) {
                interactionManagerServer.InteractWithOption(sourceUnitController, targetInteractable, interactableOptionComponent, componentIndex, choiceIndex);
            } else {
                networkManagerClient.RequestInteractWithOption(sourceUnitController, targetInteractable, componentIndex, choiceIndex);
            }
        }

        public void OpenInteractionWindow(Interactable targetInteractable) {
            //Debug.Log($"InteractionManager.OpenInteractionWindow");

            BeginInteraction(targetInteractable);
            uIManager.craftingWindow.CloseWindow();
            uIManager.interactionWindow.OpenWindow();
        }


        public void BeginInteraction(Interactable interactable) {
            SetInteractable(interactable);
            interactable.ProcessStartInteract();
        }

        public void EndInteraction() {
            currentInteractable.ProcessStopInteract();
            SetInteractable(null);
        }

        public void SetInteractable(Interactable interactable) {
            currentInteractable = interactable;
            OnSetInteractable(currentInteractable);
        }

        public void BeginInteractionWithOption(InteractableOptionComponent interactableOptionComponent, InteractableOptionManager interactableOptionManager) {
            this.interactableOptionManager = interactableOptionManager;
            currentInteractableOptionComponent = interactableOptionComponent;
            SetInteractable(interactableOptionComponent.Interactable);
        }

    }
}