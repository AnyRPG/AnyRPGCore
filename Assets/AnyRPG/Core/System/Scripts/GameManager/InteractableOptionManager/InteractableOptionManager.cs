using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class InteractableOptionManager : ConfiguredClass {

        protected InteractableOptionComponent interactableOptionComponent = null;
        protected int componentIndex;

        // game manager references
        protected InteractionManagerClient interactionManagerClient = null;

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            interactionManagerClient = systemGameManager.InteractionManagerClient;
        }

        public virtual void EndInteraction() {
            interactableOptionComponent?.ProcessStopInteract();
            interactableOptionComponent = null;
        }

        public virtual void BeginInteraction(InteractableOptionComponent interactableOptionComponent, int componentIndex, int choiceIndex) {
            BeginInteraction(interactableOptionComponent, componentIndex, choiceIndex, true);
        }

        public virtual void BeginInteraction(InteractableOptionComponent interactableOptionComponent, int componentIndex, int choiceIndex, bool notify) {
            this.interactableOptionComponent = interactableOptionComponent;
            this.componentIndex = componentIndex;
            interactionManagerClient.BeginInteractionWithOption(interactableOptionComponent, this);
            if (notify == true) {
                interactableOptionComponent?.ProcessStartInteract(componentIndex, choiceIndex);
            }
        }


    }

}