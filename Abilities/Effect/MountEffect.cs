using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New MountEffect", menuName = "AnyRPG/Abilities/Effects/MountEffect")]
    public class MountEffect : StatusEffect {

        public override void CancelEffect(BaseCharacter targetCharacter) {
            base.CancelEffect(targetCharacter);
        }

        /*
        // bypass the creation of the status effect and just make its visual prefab
        public void RawCast(BaseCharacter source, GameObject target, GameObject originalTarget, AbilityEffectOutput abilityEffectInput) {
            base.Cast(source, target, originalTarget, abilityEffectInput);
        }
        */

        public override void Cast(BaseCharacter source, GameObject target, GameObject originalTarget, AbilityEffectOutput abilityEffectInput) {
            //Debug.Log("StatusEffect.Cast(" + source.name + ", " + (target? target.name : "null") + ")");
            if (!CanUseOn(target, source)) {
                return;
            }
            string originalPrefabSourceBone = prefabSourceBone;
            prefabSourceBone = string.Empty;
            base.Cast(source, target, originalTarget, abilityEffectInput);
            prefabSourceBone = originalPrefabSourceBone;
            if (abilityEffectObject != null) {
                // pass in the ability effect object so we can independently destroy it and let it last as long as the status effect (which could be refreshed).
                abilityEffectObject.transform.parent = PlayerManager.MyInstance.MyPlayerUnitParent.transform;
                if (prefabSourceBone != null && prefabSourceBone != string.Empty) {
                    Transform mountPoint = abilityEffectObject.transform.FindChildByRecursive(prefabSourceBone);
                    if (mountPoint != null) {
                        PlayerManager.MyInstance.MyPlayerUnitObject.transform.parent = mountPoint;
                        PlayerManager.MyInstance.MyPlayerUnitObject.transform.localPosition = Vector3.zero;
                        PerformRedirects();
                    }
                }
            }
        }

        public void PerformRedirects() {
            if (abilityEffectObject != null) {
                PlayerUnitMovementController playerUnitMovementController = abilityEffectObject.GetComponent<PlayerUnitMovementController>();
                if (playerUnitMovementController != null) {
                    Debug.Log("Got Player Unit Movement Controller On Spawned Prefab (mount)");
                    (PlayerManager.MyInstance.MyCharacter.MyCharacterUnit as PlayerUnit).MyPlayerUnitMovementController.enabled = false;
                    PlayerManager.MyInstance.MyCharacter.MyCharacterUnit.MyRigidBody.constraints = RigidbodyConstraints.FreezeAll;

                    Debug.Log("Setting Animator Values");

                    PlayerManager.MyInstance.MyCharacter.MyCharacterUnit.MyCharacterAnimator.SetBool("Riding", true);
                    PlayerManager.MyInstance.MyCharacter.MyCharacterUnit.MyCharacterAnimator.SetTrigger("RidingTrigger");


                    playerUnitMovementController.SetCharacterUnit(PlayerManager.MyInstance.MyCharacter.MyCharacterUnit);

                    Rigidbody mountRigidBody = abilityEffectObject.GetComponent<Rigidbody>();
                    if (mountRigidBody != null) {
                        PlayerManager.MyInstance.MyCharacter.MyCharacterUnit.MyRigidBody = mountRigidBody;
                    }
                    CharacterMotor mountCharacterMotor = abilityEffectObject.GetComponent<CharacterMotor>();
                    if (mountCharacterMotor != null) {
                        PlayerManager.MyInstance.MyCharacter.MyCharacterUnit.MyCharacterMotor = mountCharacterMotor;
                        mountCharacterMotor.MyCharacterUnit = PlayerManager.MyInstance.MyCharacter.MyCharacterUnit;
                    }
                    CharacterAnimator mountCharacterAnimator = abilityEffectObject.GetComponent<CharacterAnimator>();
                    if (mountCharacterAnimator != null) {
                        PlayerManager.MyInstance.MyCharacter.MyCharacterUnit.MyCharacterAnimator = mountCharacterAnimator;
                    }

                    //PlayerManager.MyInstance.MyCharacter.MyCharacterUnit
                }
            }
        }

    }
}
