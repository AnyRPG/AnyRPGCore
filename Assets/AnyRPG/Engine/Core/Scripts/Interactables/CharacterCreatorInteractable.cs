using AnyRPG;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class CharacterCreatorInteractable : InteractableOption {

        [SerializeField]
        private CharacterCreatorProps characterCreatorProps = new CharacterCreatorProps();

        public override InteractableOptionProps InteractableOptionProps { get => characterCreatorProps; }
    }
}