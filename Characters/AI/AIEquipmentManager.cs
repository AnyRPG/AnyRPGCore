using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UMA;
using UMA.CharacterSystem;

namespace AnyRPG {
    public class AIEquipmentManager : CharacterEquipmentManager {

        public override void CreateComponentReferences() {
            base.CreateComponentReferences();
            if (playerUnitObject == null) {
                playerUnitObject = gameObject;
            }

            // NPC case
            if (dynamicCharacterAvatar == null) {
                dynamicCharacterAvatar = GetComponent<DynamicCharacterAvatar>();
                if (dynamicCharacterAvatar != null) {
                    SubscribeToUMACreate();
                }
            }
        }

        public void HandleCharacterCreated(UMAData umaData) {
            //Debug.Log("PlayerManager.CharacterCreatedCallback(): " + umaData);
            UnsubscribeFromUMACreate();
            HandleCharacterUnitSpawn();
        }

        public void SubscribeToUMACreate() {

            // is this stuff necessary on ai characters?
            baseCharacter.MyAnimatedUnit.MyCharacterAnimator.InitializeAnimator();
            dynamicCharacterAvatar.Initialize();
            // is this stuff necessary end

            UMAData umaData = dynamicCharacterAvatar.umaData;
            umaData.OnCharacterCreated += HandleCharacterCreated;
        }

        public void UnsubscribeFromUMACreate() {
            if (dynamicCharacterAvatar != null) {
                dynamicCharacterAvatar.umaData.OnCharacterCreated -= HandleCharacterCreated;
            }
        }

        protected override void Start() {
            base.Start();
            SubscribeToCombatEvents();
        }

        public override void OnDisable() {
            base.OnDisable();
            UnSubscribeFromCombatEvents();
            UnsubscribeFromUMACreate();
        }

    }

}