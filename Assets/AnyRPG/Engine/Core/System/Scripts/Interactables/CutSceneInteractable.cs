using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class CutSceneInteractable : InteractableOption {

        [SerializeField]
        private CutsceneProps cutsceneProps = new CutsceneProps();

        public override InteractableOptionProps InteractableOptionProps { get => cutsceneProps; }
    }

}