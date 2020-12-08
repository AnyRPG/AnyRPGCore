using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [CreateAssetMenu(fileName = "New Dialog Config", menuName = "AnyRPG/Interactable/DialogConfig")]
    public class DialogConfig : InteractableOptionConfig {

        [SerializeField]
        private DialogProps interactableOptionProps = new DialogProps();

        public override InteractableOptionProps InteractableOptionProps { get => interactableOptionProps; }
    }

}