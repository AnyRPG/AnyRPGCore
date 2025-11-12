using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class InteractableOptionManager : ConfiguredClass {

        //public event System.Action OnConfirmAction = delegate { };
        //public event System.Action OnEndInteraction = delegate { };
        //public event System.Action OnBeginInteraction = delegate { };

        protected InteractableOptionComponent interactableOptionComponent = null;
        protected int componentIndex;

        // game manager references
        protected InteractionManager interactionManager = null;

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            interactionManager = systemGameManager.InteractionManager;
        }

        public virtual void EndInteraction() {
            interactableOptionComponent?.ProcessStopInteract();
            interactableOptionComponent = null;
            //OnEndInteraction();
        }

        public virtual void BeginInteraction(InteractableOptionComponent interactableOptionComponent, int componentIndex, int choiceIndex) {
            BeginInteraction(interactableOptionComponent, componentIndex, choiceIndex, true);
        }

        public virtual void BeginInteraction(InteractableOptionComponent interactableOptionComponent, int componentIndex, int choiceIndex, bool notify) {
            this.interactableOptionComponent = interactableOptionComponent;
            this.componentIndex = componentIndex;
            interactionManager.BeginInteractionWithOption(interactableOptionComponent, this);
            if (notify == true) {
                interactableOptionComponent?.ProcessStartInteract(componentIndex, choiceIndex);
            }
            //OnBeginInteraction();
        }


    }

}