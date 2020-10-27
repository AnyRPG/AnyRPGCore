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
        private ItemPickupConfig itemPickupConfig = new ItemPickupConfig();

        public ItemPickup(Interactable interactable, ItemPickupConfig interactableOptionConfig) : base(interactable, interactableOptionConfig) {
            this.itemPickupConfig = interactableOptionConfig;
        }

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