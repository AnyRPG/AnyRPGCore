using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class ItemPickup : LootableNode {

        public override event Action<IInteractable> MiniMapStatusUpdateHandler = delegate { };

        [SerializeField]
        protected string itemName;


        /*
        // track the number of times this item has been picked up
        private int pickupCount = 0;
        */
        /*
        public override bool Interact(CharacterUnit source) {
            bool returnValue = PickUp();
            return returnValue;
        }
        */
        /*
        public override int GetCurrentOptionCount() {
            return (pickupCount > 0 && spawnTimer == -1 ? 0 : 1);
        }
        */
    }

}