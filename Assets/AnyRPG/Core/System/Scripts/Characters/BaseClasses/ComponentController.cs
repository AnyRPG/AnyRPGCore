using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace AnyRPG {
    public class ComponentController : ConfiguredMonoBehaviour {

        [SerializeField]
        protected InteractableRange interactableRange = null;

        protected Interactable Interactable = null;

        public InteractableRange InteractableRange { get => interactableRange; set => interactableRange = value; }

        public override void Configure(SystemGameManager systemGameManager) {

            base.Configure(systemGameManager);
            interactableRange.Configure(systemGameManager);
        }

        public void SetInteractable(Interactable interactable) {
            this.Interactable = interactable;
            interactableRange.SetInteractable(interactable);
        }


    }

}
