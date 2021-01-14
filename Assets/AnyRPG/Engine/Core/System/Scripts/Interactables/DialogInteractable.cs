using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class DialogInteractable : InteractableOption {

        [SerializeField]
        private DialogProps dialogProps = new DialogProps();

        public override InteractableOptionProps InteractableOptionProps { get => dialogProps; }
    }

}