using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [CreateAssetMenu(fileName = "New Item Pickup Config", menuName = "AnyRPG/Interactable/ItemPickupConfig")]
    public class ItemPickupConfig : InteractableOptionConfig {

        [SerializeField]
        private ItemPickupProps interactableOptionProps = new ItemPickupProps();

        public override InteractableOptionProps InteractableOptionProps { get => interactableOptionProps; }
    }

}