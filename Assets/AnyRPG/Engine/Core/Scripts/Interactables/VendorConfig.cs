using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [CreateAssetMenu(fileName = "New Vendor Config", menuName = "AnyRPG/Interactable/VendorConfig")]
    public class VendorConfig : InteractableOptionConfig {

        [SerializeField]
        private VendorProps interactableOptionProps = new VendorProps();

        public override InteractableOptionProps InteractableOptionProps { get => interactableOptionProps; }
    }

}