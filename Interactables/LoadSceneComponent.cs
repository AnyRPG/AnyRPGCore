using AnyRPG;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class LoadSceneComponent : PortalComponent {

        public override event Action<IInteractable> MiniMapStatusUpdateHandler = delegate { };

        public LoadSceneComponent(Interactable interactable, LoadSceneProps interactableOptionProps) : base(interactable, interactableOptionProps) {
            this.interactableOptionProps = interactableOptionProps;
        }

        public override bool Interact(CharacterUnit source) {
            //Debug.Log(gameObject.name + ".PortalInteractable.Interact()");
            base.Interact(source);

            LevelManager.MyInstance.LoadLevel((interactableOptionProps as LoadSceneProps).SceneName);
            return true;
        }

    }
}