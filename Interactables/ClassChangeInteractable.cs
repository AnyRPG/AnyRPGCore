using AnyRPG;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class ClassChangeInteractable : InteractableOption {

        [SerializeField]
        private ClassChangeProps classChangeProps = new ClassChangeProps();

        public override InteractableOptionProps InteractableOptionProps { get => classChangeProps; }
    }

}