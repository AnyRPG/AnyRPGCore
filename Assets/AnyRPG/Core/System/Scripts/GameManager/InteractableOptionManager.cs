using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class InteractableOptionManager : ConfiguredMonoBehaviour {

        //public event System.Action OnConfirmAction = delegate { };
        //public event System.Action OnEndInteraction = delegate { };
        //public event System.Action OnBeginInteraction = delegate { };

        protected InteractableOptionComponent interactableOptionComponent = null;

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

        public virtual void ConfirmAction() {
            interactableOptionComponent?.NotifyOnConfirmAction();
        }

        public virtual void BeginInteraction(InteractableOptionComponent interactableOptionComponent) {
            BeginInteraction(interactableOptionComponent, true);
        }

        public virtual void BeginInteraction(InteractableOptionComponent interactableOptionComponent, bool notify) {
            this.interactableOptionComponent = interactableOptionComponent;
            interactionManager.BeginInteraction(interactableOptionComponent, this);
            if (notify == true) {
                interactableOptionComponent?.ProcessStartInteract();
            }
            //OnBeginInteraction();
        }


    }

}