using AnyRPG;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class FactionChangeInteractable : InteractableOption {

        [SerializeField]
        private FactionChangeProps factionChangeProps = new FactionChangeProps();

        public override InteractableOptionProps InteractableOptionProps { get => factionChangeProps; }
    }

}