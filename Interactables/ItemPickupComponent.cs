using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class ItemPickupComponent : LootableNodeComponent {

        public override event Action<InteractableOptionComponent> MiniMapStatusUpdateHandler = delegate { };

        public ItemPickupComponent(Interactable interactable, ItemPickupProps interactableOptionProps) : base(interactable, interactableOptionProps) {
            this.interactableOptionProps = interactableOptionProps;
        }

        public override int GetValidOptionCount() {
            int returnValue = base.GetValidOptionCount();
            if (returnValue == 0) {
                return returnValue;
            }
            if ((interactableOptionProps.SpawnTimer == -1  && pickupCount > 0) || spawnCoroutine != null) {
                return 0;
            }
            return returnValue;
        }
    }

}