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

        public override InteractableOptionProps InteractableOptionProps { get => loadSceneProps; }
        public LoadSceneProps LoadSceneProps { get => loadSceneProps; }
    }
}