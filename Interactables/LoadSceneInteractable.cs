using AnyRPG;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class LoadSceneInteractable : PortalInteractable {

        public override event Action<IInteractable> MiniMapStatusUpdateHandler = delegate { };

        private LoadSceneConfig loadSceneConfig = null;

        [Header("Scene Options")]

        [Tooltip("When interacted with, this scene will load directly.")]
        [SerializeField]
        private string sceneName = string.Empty;

        public LoadSceneInteractable(Interactable interactable, LoadSceneConfig interactableOptionConfig) : base(interactable, interactableOptionConfig) {
            this.loadSceneConfig = interactableOptionConfig;
        }

        public override bool Interact(CharacterUnit source) {
            //Debug.Log(gameObject.name + ".PortalInteractable.Interact()");
            base.Interact(source);

            LevelManager.MyInstance.LoadLevel(sceneName);
            return true;
        }

    }
}