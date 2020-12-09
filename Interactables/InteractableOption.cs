using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public abstract class InteractableOption : MonoBehaviour {

        protected InteractableOptionProps interactableOptionProps = null;

        public virtual InteractableOptionProps InteractableOptionProps { get => interactableOptionProps; set => interactableOptionProps = value; }

        public virtual void SetupScriptableObjects() {
            InteractableOptionProps.SetupScriptableObjects();
        }
    }

}