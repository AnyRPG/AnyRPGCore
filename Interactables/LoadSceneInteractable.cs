using AnyRPG;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class LoadSceneInteractable : PortalInteractable {

        public override event Action<IInteractable> MiniMapStatusUpdateHandler = delegate { };

        [SerializeField]
        private LoadSceneConfig loadSceneConfig = new LoadSceneConfig();

        private LoadSceneProps interactableOptionProps = null;

        [Header("Scene Options")]

        [Tooltip("When interacted with, this scene will load directly.")]
        [SerializeField]
        private string sceneName = string.Empty;

        public LoadSceneInteractable(Interactable interactable, LoadSceneProps interactableOptionProps) : base(interactable, interactableOptionProps) {
            this.interactableOptionProps = interactableOptionProps;
        }

        public override bool Interact(CharacterUnit source) {
            //Debug.Log(gameObject.name + ".PortalInteractable.Interact()");
            base.Interact(source);

            LevelManager.MyInstance.LoadLevel(sceneName);
            return true;
        }

    }
}