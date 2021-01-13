using AnyRPG;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class NameChangeInteractable : InteractableOption {

        [SerializeField]
        private NameChangeProps nameChangeProps = new NameChangeProps();

        public override InteractableOptionProps InteractableOptionProps { get => nameChangeProps; }
    }

}