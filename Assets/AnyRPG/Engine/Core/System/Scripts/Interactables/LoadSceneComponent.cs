using AnyRPG;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class LoadSceneComponent : PortalComponent {

        public override event Action<InteractableOptionComponent> MiniMapStatusUpdateHandler = delegate { };

        public LoadSceneProps LoadSceneProps { get => interactableOptionProps as LoadSceneProps; }

        public LoadSceneComponent(Interactable interactable, LoadSceneProps interactableOptionProps) : base(interactable, interactableOptionProps) {
        }

        public override bool Interact(CharacterUnit source, int optionIndex = 0) {
            //Debug.Log(interactable.gameObject.name + ".LoadSceneComponent.Interact()");
            base.Interact(source, optionIndex);

            LevelManager.MyInstance.LoadLevel(LoadSceneProps.SceneName);
            return true;
        }

    }
}