using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class InteractionManager : ConfiguredMonoBehaviour {

        public event System.Action<Interactable> OnSetInteractable = delegate { };

        private Interactable currentInteractable = null;
        private InteractableOptionComponent currentInteractableOptionComponent = null;
        private InteractableOptionManager interactableOptionManager = null;

        /*
        public Interactable CurrentInteractable {
            get => currentInteractable;
            set {
                //Debug.Log("CurrentInteractable");
                currentInteractable = value;
                OnSetInteractable(currentInteractable);
            }
        }
        */

        public void SetInteractable(Interactable interactable) {
            currentInteractable = interactable;
            OnSetInteractable(currentInteractable);
        }

        public void BeginInteraction(InteractableOptionComponent interactableOptionComponent, InteractableOptionManager interactableOptionManager) {
            this.interactableOptionManager = interactableOptionManager;
            currentInteractableOptionComponent = interactableOptionComponent;
            SetInteractable(interactableOptionComponent.Interactable);
        }

        //public void SetInteractableOptionManager(InteractableOptionManager interactableOptionManager) {
        //}



    }

}