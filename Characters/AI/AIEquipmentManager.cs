using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UMA;
using UMA.CharacterSystem;

namespace AnyRPG {
    public class AIEquipmentManager : CharacterEquipmentManager {

        public override void GetComponentReferences() {
            //Debug.Log(gameObject.name + "AIEquipmentManager.GetComponentReferences()");
            base.GetComponentReferences();
            if (playerUnitObject == null) {
                playerUnitObject = gameObject;
            }
        }

        public override void OrchestratorFinish() {
            //Debug.Log(gameObject.name + "AIEquipmentManager.OrchestratorFinish()");
            base.OrchestratorFinish();
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
            //Debug.Log(gameObject.name + "AIEquipmentManager.SubscribeToUMACreate()");

            // is this stuff necessary on ai characters?
            if (baseCharacter != null && baseCharacter.MyAnimatedUnit != null && baseCharacter.MyAnimatedUnit.MyCharacterAnimator != null) {
                baseCharacter.MyAnimatedUnit.MyCharacterAnimator.InitializeAnimator();
                dynamicCharacterAvatar.Initialize();
                // is this stuff necessary end

                UMAData umaData = dynamicCharacterAvatar.umaData;
                umaData.OnCharacterCreated += HandleCharacterCreated;
            } else {
                if (baseCharacter == null ) {
                    //Debug.Log(gameObject.name + "AIEquipmentManager.SubscribeToUMACreate(): baseCharacter is null!");
                } else if (baseCharacter.MyAnimatedUnit == null) {
                    //Debug.Log(gameObject.name + "AIEquipmentManager.SubscribeToUMACreate(): baseCharacter.MyAnimatedUnit is null!");
                } else if (baseCharacter.MyAnimatedUnit.MyCharacterAnimator == null) {
                    //Debug.Log(gameObject.name + "AIEquipmentManager.SubscribeToUMACreate(): baseCharacter.MyAnimatedUnit.MyCharacterAnimator is null!");
                }
            }
            
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