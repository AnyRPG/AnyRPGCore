using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class ItemPickup : LootableNode {

        [SerializeField]
        private ItemPickupProps itemPickupProps = new ItemPickupProps();

        public override InteractableOptionProps InteractableOptionProps { get => itemPickupProps; }
    }

}