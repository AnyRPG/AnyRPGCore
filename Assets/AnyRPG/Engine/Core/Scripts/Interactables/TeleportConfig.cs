using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [CreateAssetMenu(fileName = "New Teleport Config", menuName = "AnyRPG/Interactable/TeleportConfig")]
    public class TeleportConfig : InteractableOptionConfig {

        [SerializeField]
        private TeleportProps interactableOptionProps = new TeleportProps();

        public override InteractableOptionProps InteractableOptionProps { get => interactableOptionProps; }
    }

}