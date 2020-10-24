using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [CreateAssetMenu(fileName = "New Item Pickup Config", menuName = "AnyRPG/Interactable/ItemPickupConfig")]
    [System.Serializable]
    public class ItemPickupConfig : LootableNodeConfig {


        public override InteractableOption GetInteractableOption(Interactable interactable) {
            return new ItemPickup(interactable, this);
        }
    }

}