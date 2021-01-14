using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public abstract class InteractableOption : MonoBehaviour {

        protected InteractableOptionProps interactableOptionProps = null;

        // a reference to the interaction option component that was created from this objects properties
        protected InteractableOptionComponent interactableOptionComponent = null;

        public virtual InteractableOptionProps InteractableOptionProps { get => interactableOptionProps; set => interactableOptionProps = value; }
        public InteractableOptionComponent InteractableOptionComponent { get => interactableOptionComponent; }

        public void SetComponent(InteractableOptionComponent interactableOptionComponent) {
            //Debug.Log(gameObject.name + ".InteractableOption.SetComponent(" + (interactableOptionComponent == null ? "null" : interactableOptionComponent.Interactable.gameObject.name) + ")");
            this.interactableOptionComponent = interactableOptionComponent;
        }

        public virtual void SetupScriptableObjects() {
            InteractableOptionProps.SetupScriptableObjects();
        }
    }

}