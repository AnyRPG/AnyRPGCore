using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [System.Serializable]
    public class LoadSceneProps : PortalProps {

        [Header("Scene Options")]

        [Tooltip("When interacted with, this scene will load directly.")]
        [SerializeField]
        private string sceneName = string.Empty;

        public string SceneName { get => sceneName; set => sceneName = value; }

        public override InteractableOption GetInteractableOption(Interactable interactable) {
            return new LoadSceneInteractable(interactable, this);
        }
    }

}