using AnyRPG;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class TeleportInteractable : PortalInteractable {

        [SerializeField]
        private TeleportProps teleportProps = new TeleportProps();

        [Header("Teleport")]

        [Tooltip("When interacted with, the player will cast this ability. Only applies if Portal Type is Ability.")]
        [SerializeField]
        private string abilityName = string.Empty;

        public override InteractableOptionProps InteractableOptionProps { get => teleportProps; }
    }
}