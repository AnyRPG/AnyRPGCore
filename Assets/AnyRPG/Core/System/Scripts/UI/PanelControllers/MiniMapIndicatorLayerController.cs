using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace AnyRPG {
    public abstract class MiniMapIndicatorLayerController : ConfiguredMonoBehaviour {

        protected bool isActive = false;
        protected InteractableOptionComponent interactableOptionComponent = null;

        public bool IsActive { get => isActive; }

        public virtual void Setup(InteractableOptionComponent interactableOptionComponent) {
            this.interactableOptionComponent = interactableOptionComponent;
        }

        public abstract void ConfigureDisplay();
    }

}