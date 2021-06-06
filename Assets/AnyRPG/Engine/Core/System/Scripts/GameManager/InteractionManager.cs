using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class InteractionManager : MonoBehaviour {

        public event System.Action<Interactable> OnSetInteractable = delegate { };

        #region Singleton
        private static InteractionManager instance;

        public static InteractionManager Instance {
            get {
                return instance;
            }
        }

        private void Awake() {
            instance = this;
        }

        #endregion

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