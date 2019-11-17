using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New MountEffect", menuName = "AnyRPG/Abilities/Effects/MountEffect")]
    public class MountEffect : StatusEffect {

        public override void CancelEffect(BaseCharacter targetCharacter) {
            //Debug.Log("MountEffect.CancelEffect(" + (targetCharacter != null ? targetCharacter.name : "null") + ")");
            PlayerManager.MyInstance.MyPlayerUnitObject.transform.parent = PlayerManager.MyInstance.MyPlayerUnitParent.transform;

            // we could skip this and just let the player fall through gravity
            PlayerManager.MyInstance.MyPlayerUnitObject.transform.position = abilityEffectObject.transform.position;
            DeActivateMountedState();
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
                        //PlayerManager.MyInstance.MyPlayerUnitObject.transform.localPosition = Vector3.zero;
                        PlayerManager.MyInstance.MyPlayerUnitObject.transform.position = mountPoint.transform.position;
                        ActivateMountedState();
                    }
                }
            }
        }

        public void DeActivateMountedState() {
            //Debug.Log("MountEffect.DeActivateMountedState()");
            if (abilityEffectObject != null) {
                PlayerUnitMovementController playerUnitMovementController = abilityEffectObject.GetComponent<PlayerUnitMovementController>();
                if (playerUnitMovementController != null) {
                    //Debug.Log("Got Player Unit Movement Controller On Spawned Prefab (mount)");

                    //PlayerManager.MyInstance.MyCharacter.MyAnimatedUnit.MyRigidBody.constraints = RigidbodyConstraints.FreezeAll;

                    //Debug.Log("Setting Animator Values");

                    PlayerManager.MyInstance.MyCharacter.MyAnimatedUnit = PlayerManager.MyInstance.MyPlayerUnitObject.GetComponent<AnimatedUnit>();
                    ConfigureCharacterRegularPhysics();

                    (PlayerManager.MyInstance.MyCharacter.MyAnimatedUnit as AnimatedPlayerUnit).MyPlayerUnitMovementController.enabled = true;

                    PlayerManager.MyInstance.MyCharacter.MyAnimatedUnit.MyCharacterAnimator.SetBool("Riding", false);

                }
            }
        }

        public void ActivateMountedState() {
            if (abilityEffectObject != null) {
                ConfigureMountPhysics();
                PlayerUnitMovementController playerUnitMovementController = abilityEffectObject.GetComponent<PlayerUnitMovementController>();
                if (playerUnitMovementController != null) {

                    //Debug.Log("Got Player Unit Movement Controller On Spawned Prefab (mount)");

                    (PlayerManager.MyInstance.MyCharacter.MyAnimatedUnit as AnimatedPlayerUnit).MyPlayerUnitMovementController.enabled = false;
                    PlayerManager.MyInstance.MyCharacter.MyAnimatedUnit.MyRigidBody.constraints = RigidbodyConstraints.FreezeAll;

                    //Debug.Log("MountEffect.ActivateMountedState()Setting Animator Values");

                    PlayerManager.MyInstance.MyCharacter.MyAnimatedUnit.MyCharacterAnimator.SetBool("Riding", true);
                    PlayerManager.MyInstance.MyCharacter.MyAnimatedUnit.MyCharacterAnimator.SetTrigger("RidingTrigger");

                    ConfigureCharacterMountedPhysics();
                    PlayerManager.MyInstance.MyCharacter.MyAnimatedUnit = abilityEffectObject.GetComponent<AnimatedUnit>();

                    playerUnitMovementController.SetCharacterUnit(PlayerManager.MyInstance.MyCharacter.MyCharacterUnit);
                }
            }
        }

        public void ConfigureCharacterMountedPhysics() {
            PlayerManager.MyInstance.MyCharacter.MyAnimatedUnit.MyRigidBody.WakeUp();
            //PlayerManager.MyInstance.MyCharacter.MyAnimatedUnit.MyRigidBody.collisionDetectionMode = CollisionDetectionMode.Discrete;
            // DO NOT EVER USE CONTINUOUS SPECULATIVE.  IT WILL MESS THINGS UP EVEN WHEN YOUR RIGIDBODY IS KINEMATIC
            // UNITY ERROR MESSAGE IS MISLEADING AND WRONG HERE....
            //PlayerManager.MyInstance.MyCharacter.MyAnimatedUnit.MyRigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            PlayerManager.MyInstance.MyCharacter.MyAnimatedUnit.MyRigidBody.interpolation = RigidbodyInterpolation.None;
            PlayerManager.MyInstance.MyCharacter.MyAnimatedUnit.MyRigidBody.detectCollisions = false;
            PlayerManager.MyInstance.MyCharacter.MyAnimatedUnit.MyRigidBody.isKinematic = true;
            PlayerManager.MyInstance.MyCharacter.MyAnimatedUnit.MyRigidBody.useGravity = false;
            PlayerManager.MyInstance.MyCharacter.MyAnimatedUnit.MyRigidBody.constraints = RigidbodyConstraints.FreezeAll;
            //PlayerManager.MyInstance.MyCharacter.MyAnimatedUnit.MyRigidBody.constraints = RigidbodyConstraints.None;
            Collider collider = PlayerManager.MyInstance.MyPlayerUnitObject.GetComponent<Collider>();
            if (collider != null) {
                collider.enabled = false;
            }
            PlayerManager.MyInstance.MyCharacter.MyAnimatedUnit.MyRigidBody.WakeUp();

        }

        public void ConfigureCharacterRegularPhysics() {
            PlayerManager.MyInstance.MyCharacter.MyAnimatedUnit.MyRigidBody.WakeUp();
            PlayerManager.MyInstance.MyCharacter.MyAnimatedUnit.MyRigidBody.detectCollisions = true;
            PlayerManager.MyInstance.MyCharacter.MyAnimatedUnit.MyRigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            PlayerManager.MyInstance.MyCharacter.MyAnimatedUnit.MyRigidBody.isKinematic = false;
            PlayerManager.MyInstance.MyCharacter.MyAnimatedUnit.MyRigidBody.useGravity = true;
            PlayerManager.MyInstance.MyCharacter.MyAnimatedUnit.MyRigidBody.constraints = RigidbodyConstraints.FreezeRotation;
            Collider collider = PlayerManager.MyInstance.MyPlayerUnitObject.GetComponent<Collider>();
            if (collider != null) {
                collider.enabled = true;
            }
            PlayerManager.MyInstance.MyCharacter.MyAnimatedUnit.MyRigidBody.WakeUp();
        }

        public void ConfigureMountPhysics() {
            Debug.Log("MountEffect.ConfigureMountPhysics()");
            Collider anyCollider = abilityEffectObject.GetComponent<Collider>();
            if (anyCollider != null) {
                Debug.Log("MountEffect.ConfigureMountPhysics(): configuring trigger");
                anyCollider.isTrigger = false;
            } else {
                Debug.Log("MountEffect.ConfigureMountPhysics(): could not find collider");
            }
            Rigidbody mountRigidBody = abilityEffectObject.GetComponent<Rigidbody>();
            if (mountRigidBody != null) {
                Debug.Log("MountEffect.ConfigureMountPhysics(): configuring rigidbody");
                mountRigidBody.isKinematic = false;
                mountRigidBody.useGravity = true;
            } else {
                Debug.Log("MountEffect.ConfigureMountPhysics(): could not find collider");
            }
            NavMeshAgent navMeshAgent = abilityEffectObject.GetComponent<NavMeshAgent>();
            if (navMeshAgent != null) {
                navMeshAgent.enabled = false;
            }
        }



    }
}
