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
        public override int GetValidOptionCount() {
            int returnValue = base.GetValidOptionCount();
            if (returnValue == 0) {
                return returnValue;
            }
            if ((spawnTimer == -1  && pickupCount > 0) || spawnCoroutine != null) {
                return 0;
            }
            return returnValue;
        }
    }

}