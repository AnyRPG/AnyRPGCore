using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [System.Serializable]
    public class ItemPickupProps : LootableNodeProps {


        public InteractableOption GetInteractableOption(Interactable interactable) {
            return new ItemPickup(interactable, this);
        }
    }

}