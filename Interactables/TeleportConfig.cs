using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [CreateAssetMenu(fileName = "New Teleport Config", menuName = "AnyRPG/Interactable/TeleportConfig")]
    [System.Serializable]
    public class TeleportConfig : PortalConfig {

        [Header("Teleport")]

        [Tooltip("When interacted with, the player will cast this ability. Only applies if Portal Type is Ability.")]
        [SerializeField]
        private string abilityName = string.Empty;

        public string AbilityName { get => abilityName; set => abilityName = value; }

        public override InteractableOption GetInteractableOption(Interactable interactable) {
            return new TeleportInteractable(interactable, this);
        }
    }

}