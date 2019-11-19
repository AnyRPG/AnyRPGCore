using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UMA;
using UMA.CharacterSystem;

namespace AnyRPG {
    public class AIEquipmentManager : CharacterEquipmentManager {

        protected void Awake() {
            if (playerUnitObject == null) {
                playerUnitObject = gameObject;
            }

            // NPC case
            if (dynamicCharacterAvatar == null) {
                dynamicCharacterAvatar = GetComponent<DynamicCharacterAvatar>();
            }

        }

        protected override void Start() {
            base.Start();
            SubscribeToCombatEvents();
        }

        public override void OnDisable() {
            base.OnDisable();
            UnSubscribeFromCombatEvents();
        }

    }

}