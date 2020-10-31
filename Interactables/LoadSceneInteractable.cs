using AnyRPG;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class LoadSceneInteractable : PortalInteractable {

        [SerializeField]
        private LoadSceneProps loadSceneProps = new LoadSceneProps();

        [Header("Scene Options")]

        [Tooltip("When interacted with, this scene will load directly.")]
        [SerializeField]
        private string sceneName = string.Empty;

        public override InteractableOptionProps InteractableOptionProps { get => loadSceneProps; }
    }
}