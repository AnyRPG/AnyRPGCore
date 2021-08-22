using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class ItemPickupComponent : LootableNodeComponent {

        public ItemPickupProps ItemPickupProps { get => interactableOptionProps as ItemPickupProps; }

        public ItemPickupComponent(Interactable interactable, ItemPickupProps interactableOptionProps, SystemGameManager systemGameManager) : base(interactable, interactableOptionProps, systemGameManager) {
        }

        public override int GetValidOptionCount() {
            //Debug.Log(interactable.gameObject.name + ".ItemPickupComponent.GetValidOptionCount()");
            int returnValue = base.GetValidOptionCount();
            if (returnValue == 0) {
                return returnValue;
            }
            if ((ItemPickupProps.SpawnTimer == -1  && pickupCount > 0) || spawnCoroutine != null) {
                return 0;
            }
            return returnValue;
        }
    }

}