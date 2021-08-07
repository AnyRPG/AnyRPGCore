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

        public Interactable CurrentInteractable {
            get => currentInteractable;
            set {
                Debug.Log("CurrentInteractable");
                currentInteractable = value;
                OnSetInteractable(currentInteractable);
            }
        }

    }

}