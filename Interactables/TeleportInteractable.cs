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

        public override InteractableOptionProps InteractableOptionProps { get => teleportProps; }
    }
}