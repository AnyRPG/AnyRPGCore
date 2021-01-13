using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [CreateAssetMenu(fileName = "New Cutscene Config", menuName = "AnyRPG/Interactable/CutsceneConfig")]
    public class CutsceneConfig : InteractableOptionConfig {

        [SerializeField]
        private CutsceneProps interactableOptionProps = new CutsceneProps();

        public override InteractableOptionProps InteractableOptionProps { get => interactableOptionProps; }
    }

}