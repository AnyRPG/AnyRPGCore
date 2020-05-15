using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using UMA;
using UMA.CharacterSystem;


namespace AnyRPG {
    [CreateAssetMenu(fileName = "New MountEffect", menuName = "AnyRPG/Abilities/Effects/MountEffect")]
    public class MountEffect : StatusEffect {

        // reference to the dynamic character avatar on the mount, if it exists
        private DynamicCharacterAvatar dynamicCharacterAvatar = null;

        public override void CancelEffect(BaseCharacter targetCharacter) {
            //Debug.Log("MountEffect.CancelEffect(" + (targetCharacter != null ? targetCharacter.name : "null") + ")");
            if (PlayerManager.MyInstance.MyPlayerUnitObject != null) {
                PlayerManager.MyInstance.MyPlayerUnitObject.transform.parent = PlayerManager.MyInstance.MyPlayerUnitParent.transform;

                //PlayerManager.MyInstance.MyPlayerUnitObject.transform.localEulerAngles = Vector3.zero;
                PlayerManager.MyInstance.MyPlayerUnitObject.transform.localEulerAngles = prefabObjects.Values.ElementAt(0).transform.localEulerAngles;

                // we could skip this and just let the player fall through gravity
                PlayerManager.MyInstance.MyPlayerUnitObject.transform.position = prefabObjects.Values.ElementAt(0).transform.position;
            }
            DeActivateMountedState();
            UnsubscribeFromUMACreate();
            base.CancelEffect(targetCharacter);
        }

        /*
        // bypass the creation of the status effect and just make its visual prefab
        public void RawCast(BaseCharacter source, GameObject target, GameObject originalTarget, AbilityEffectOutput abilityEffectInput) {
            base.Cast(source, target, originalTarget, abilityEffectInput);
        }
        */

        public override Dictionary<PrefabProfile, GameObject> Cast(IAbilityCaster source, GameObject target, GameObject originalTarget, AbilityEffectOutput abilityEffectInput) {
            //Debug.Log("StatusEffect.Cast(" + source.name + ", " + (target? target.name : "null") + ")");
            if (!CanUseOn(target, source)) {
                return null;
            }
            Dictionary<PrefabProfile, GameObject> returnObjects = base.Cast(source, target, originalTarget, abilityEffectInput);
            PrefabProfile prefabProfile = returnObjects.Keys.ElementAt(0);
            GameObject abilityEffectObject = returnObjects[prefabProfile];

            GameObject go = prefabObjects.Values.ElementAt(0);
            if (abilityEffectObject != null) {

                // pass in the ability effect object so we can independently destroy it and let it last as long as the status effect (which could be refreshed).
                abilityEffectObject.transform.parent = PlayerManager.MyInstance.MyPlayerUnitParent.transform;

                dynamicCharacterAvatar = go.GetComponent<DynamicCharacterAvatar>();
                if (dynamicCharacterAvatar != null) {
                    SubscribeToUMACreate();
                } else {
                    HandleMountUnitSpawn();
                }
            }
            return returnObjects;
        }

        private void HandleMountUnitSpawn() {

            PrefabProfile prefabProfile = prefabObjects.Keys.ElementAt(0);
            GameObject abilityEffectObject = prefabObjects[prefabProfile];

            string originalPrefabSourceBone = prefabProfile.MyTargetBone;
            // NOTE: mount effects used sheathed position for character position.  do not use regular position to avoid putting mount below ground when spawning
            Vector3 originalPrefabOffset = prefabProfile.MySheathedPosition;

            if (originalPrefabSourceBone != null && originalPrefabSourceBone != string.Empty) {
                Transform mountPoint = abilityEffectObject.transform.FindChildByRecursive(originalPrefabSourceBone);
                if (mountPoint != null) {
                    PlayerManager.MyInstance.MyPlayerUnitObject.transform.parent = mountPoint;
                    //PlayerManager.MyInstance.MyPlayerUnitObject.transform.localPosition = Vector3.zero;
                    PlayerManager.MyInstance.MyPlayerUnitObject.transform.position = mountPoint.transform.TransformPoint(originalPrefabOffset);
                    PlayerManager.MyInstance.MyPlayerUnitObject.transform.localEulerAngles = prefabProfile.MySheathedRotation;
                    ActivateMountedState();
                }
            }
        }

        public void DeActivateMountedState() {
            //Debug.Log("MountEffect.DeActivateMountedState()");
            if (prefabObjects != null) {
                GameObject go = prefabObjects.Values.ElementAt(0);
                PlayerUnitMovementController playerUnitMovementController = go.GetComponent<PlayerUnitMovementController>();
                if (playerUnitMovementController != null && PlayerManager.MyInstance.MyPlayerUnitObject != null) {
                    //Debug.Log("Got Player Unit Movement Controller On Spawned Prefab (mount)");

                    //PlayerManager.MyInstance.MyCharacter.MyAnimatedUnit.MyRigidBody.constraints = RigidbodyConstraints.FreezeAll;

                    //Debug.Log("Setting Animator Values");

                    PlayerManager.MyInstance.MyCharacter.AnimatedUnit = PlayerManager.MyInstance.MyPlayerUnitObject.GetComponent<AnimatedUnit>();
                    ConfigureCharacterRegularPhysics();

                    // set player unit to normal state
                    PlayerManager.MyInstance.MyCharacter.CharacterUnit.MyMounted = false;
                    if ((PlayerManager.MyInstance.MyCharacter.AnimatedUnit as AnimatedPlayerUnit).MyPlayerUnitMovementController) {
                        (PlayerManager.MyInstance.MyCharacter.AnimatedUnit as AnimatedPlayerUnit).MyPlayerUnitMovementController.enabled = true;
                    }
                    PlayerManager.MyInstance.MyCharacter.AnimatedUnit.MyCharacterAnimator.SetRiding(false);
                    //PlayerManager.MyInstance.MyCharacter.MyAnimatedUnit.MyCharacterAnimator.SetBool("Riding", false);
                    CameraManager.MyInstance.ActivateMainCamera();
                    CameraManager.MyInstance.MyMainCameraController.InitializeCamera(PlayerManager.MyInstance.MyCharacter.CharacterUnit.transform);


                }
            }
        }

        public void ActivateMountedState() {
            if (prefabObjects != null) {
                ConfigureMountPhysics();
                GameObject go = prefabObjects.Values.ElementAt(0);
                PlayerUnitMovementController playerUnitMovementController = go.GetComponent<PlayerUnitMovementController>();
                if (playerUnitMovementController != null) {
                    // disable movement and input on player unit
                    if ((PlayerManager.MyInstance.MyCharacter.AnimatedUnit as AnimatedPlayerUnit).MyPlayerUnitMovementController) {
                        (PlayerManager.MyInstance.MyCharacter.AnimatedUnit as AnimatedPlayerUnit).MyPlayerUnitMovementController.enabled = false;
                    }
                    PlayerManager.MyInstance.MyCharacter.AnimatedUnit.FreezeAll();

                    //Debug.Log("MountEffect.ActivateMountedState()Setting Animator Values");
                    // set player animator to riding state
                    PlayerManager.MyInstance.MyCharacter.AnimatedUnit.MyCharacterAnimator.SetRiding(true);
                    //PlayerManager.MyInstance.MyCharacter.MyAnimatedUnit.MyCharacterAnimator.SetBool("Riding", true);
                    //PlayerManager.MyInstance.MyCharacter.MyAnimatedUnit.MyCharacterAnimator.SetTrigger("RidingTrigger");

                    // set player unit to riding state
                    PlayerManager.MyInstance.MyCharacter.CharacterUnit.MyMounted = true;

                    ConfigureCharacterMountedPhysics();

                    // initialize the mount animator
                    PlayerManager.MyInstance.MyCharacter.AnimatedUnit = go.GetComponent<AnimatedUnit>();
                    PlayerManager.MyInstance.MyCharacter.AnimatedUnit.OrchestratorStart();
                    PlayerManager.MyInstance.MyCharacter.AnimatedUnit.OrchestratorFinish();
                    PlayerManager.MyInstance.MyCharacter.AnimatedUnit.MyCharacterUnit = PlayerManager.MyInstance.MyCharacter.CharacterUnit;

                    playerUnitMovementController.SetCharacterUnit(PlayerManager.MyInstance.MyCharacter.CharacterUnit);
                    CameraManager.MyInstance.SwitchToMainCamera();
                    CameraManager.MyInstance.MyMainCameraController.InitializeCamera(go.transform);
                }
            }
        }


        public void HandleCharacterCreated(UMAData umaData) {
            //Debug.Log("PlayerManager.CharacterCreatedCallback(): " + umaData);
            UnsubscribeFromUMACreate();
            HandleMountUnitSpawn();
        }

        public void UnsubscribeFromUMACreate() {
            if (dynamicCharacterAvatar != null) {
                dynamicCharacterAvatar.umaData.OnCharacterCreated -= HandleCharacterCreated;
            }
        }

        public void SubscribeToUMACreate() {

            // is this stuff necessary on ai characters?
            AnimatedUnit animatedUnit = dynamicCharacterAvatar.gameObject.GetComponent<AnimatedUnit>();
            animatedUnit.OrchestratorStart();
            animatedUnit.OrchestratorFinish();
            if (animatedUnit != null && animatedUnit.MyCharacterAnimator != null) {
                animatedUnit.MyCharacterAnimator.InitializeAnimator();
            } else {

            }
            dynamicCharacterAvatar.Initialize();
            // is this stuff necessary end

            UMAData umaData = dynamicCharacterAvatar.umaData;
            umaData.OnCharacterCreated += HandleCharacterCreated;
        }

        public void ConfigureCharacterMountedPhysics() {
            PlayerManager.MyInstance.MyCharacter.AnimatedUnit.MyRigidBody.WakeUp();
            //PlayerManager.MyInstance.MyCharacter.MyAnimatedUnit.MyRigidBody.collisionDetectionMode = CollisionDetectionMode.Discrete;
            // DO NOT EVER USE CONTINUOUS SPECULATIVE.  IT WILL MESS THINGS UP EVEN WHEN YOUR RIGIDBODY IS KINEMATIC
            // UNITY ERROR MESSAGE IS MISLEADING AND WRONG HERE....
            //PlayerManager.MyInstance.MyCharacter.MyAnimatedUnit.MyRigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            PlayerManager.MyInstance.MyCharacter.AnimatedUnit.MyRigidBody.interpolation = RigidbodyInterpolation.None;
            PlayerManager.MyInstance.MyCharacter.AnimatedUnit.MyRigidBody.detectCollisions = false;
            PlayerManager.MyInstance.MyCharacter.AnimatedUnit.MyRigidBody.isKinematic = true;
            PlayerManager.MyInstance.MyCharacter.AnimatedUnit.MyRigidBody.useGravity = false;
            PlayerManager.MyInstance.MyCharacter.AnimatedUnit.FreezeAll();
            //PlayerManager.MyInstance.MyCharacter.MyAnimatedUnit.MyRigidBody.constraints = RigidbodyConstraints.None;
            Collider collider = PlayerManager.MyInstance.MyPlayerUnitObject.GetComponent<Collider>();
            if (collider != null) {
                collider.enabled = false;
            }
            PlayerManager.MyInstance.MyCharacter.AnimatedUnit.MyRigidBody.WakeUp();

        }

        public void ConfigureCharacterRegularPhysics() {
            PlayerManager.MyInstance.MyCharacter.AnimatedUnit.MyRigidBody.WakeUp();
            PlayerManager.MyInstance.MyCharacter.AnimatedUnit.MyRigidBody.detectCollisions = true;
            PlayerManager.MyInstance.MyCharacter.AnimatedUnit.MyRigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            PlayerManager.MyInstance.MyCharacter.AnimatedUnit.MyRigidBody.isKinematic = false;
            PlayerManager.MyInstance.MyCharacter.AnimatedUnit.MyRigidBody.useGravity = true;
            PlayerManager.MyInstance.MyCharacter.AnimatedUnit.FreezeRotation();
            Collider collider = PlayerManager.MyInstance.MyPlayerUnitObject.GetComponent<Collider>();
            if (collider != null) {
                collider.enabled = true;
            }
            PlayerManager.MyInstance.MyCharacter.AnimatedUnit.MyRigidBody.WakeUp();
        }

        public void ConfigureMountPhysics() {
            //Debug.Log("MountEffect.ConfigureMountPhysics()");
            GameObject go = prefabObjects.Values.ElementAt(0);
            Collider anyCollider = go.GetComponent<Collider>();
            if (anyCollider != null) {
                //Debug.Log("MountEffect.ConfigureMountPhysics(): configuring trigger");
                anyCollider.isTrigger = false;
            } else {
                //Debug.Log("MountEffect.ConfigureMountPhysics(): could not find collider");
            }
            Rigidbody mountRigidBody = go.GetComponent<Rigidbody>();
            if (mountRigidBody != null) {
                //Debug.Log("MountEffect.ConfigureMountPhysics(): configuring rigidbody");
                mountRigidBody.isKinematic = false;
                mountRigidBody.useGravity = true;
            } else {
                //Debug.Log("MountEffect.ConfigureMountPhysics(): could not find collider");
            }
            NavMeshAgent navMeshAgent = go.GetComponent<NavMeshAgent>();
            if (navMeshAgent != null) {
                navMeshAgent.enabled = false;
            }
        }



    }
}
