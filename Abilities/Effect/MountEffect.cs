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
            if (PlayerManager.MyInstance == null) {
                // game is in the middle of exiting
                return;
            }
            if (PlayerManager.MyInstance.PlayerUnitObject != null) {
                PlayerManager.MyInstance.PlayerUnitObject.transform.parent = PlayerManager.MyInstance.PlayerUnitParent.transform;

                //PlayerManager.MyInstance.MyPlayerUnitObject.transform.localEulerAngles = Vector3.zero;
                PlayerManager.MyInstance.PlayerUnitObject.transform.localEulerAngles = prefabObjects.Values.ElementAt(0).transform.localEulerAngles;

                // we could skip this and just let the player fall through gravity
                PlayerManager.MyInstance.PlayerUnitObject.transform.position = prefabObjects.Values.ElementAt(0).transform.position;
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

        public override Dictionary<PrefabProfile, GameObject> Cast(IAbilityCaster source, GameObject target, GameObject originalTarget, AbilityEffectContext abilityEffectInput) {
            //Debug.Log("StatusEffect.Cast(" + source.name + ", " + (target? target.name : "null") + ")");
            if (!CanUseOn(target, source)) {
                return null;
            }
            if (PlayerManager.MyInstance.PlayerUnitSpawned == false) {
                // we can't mount anything if the player unit is not spawned
                return null;
            }
            Dictionary<PrefabProfile, GameObject> returnObjects = base.Cast(source, target, originalTarget, abilityEffectInput);
            PrefabProfile prefabProfile = returnObjects.Keys.ElementAt(0);
            GameObject abilityEffectObject = returnObjects[prefabProfile];

            GameObject go = prefabObjects.Values.ElementAt(0);
            if (abilityEffectObject != null) {

                // pass in the ability effect object so we can independently destroy it and let it last as long as the status effect (which could be refreshed).
                abilityEffectObject.transform.parent = PlayerManager.MyInstance.PlayerUnitParent.transform;

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

            string originalPrefabSourceBone = prefabProfile.TargetBone;
            // NOTE: mount effects used sheathed position for character position.  do not use regular position to avoid putting mount below ground when spawning
            Vector3 originalPrefabOffset = prefabProfile.SheathedPosition;

            if (originalPrefabSourceBone != null && originalPrefabSourceBone != string.Empty) {
                Transform mountPoint = abilityEffectObject.transform.FindChildByRecursive(originalPrefabSourceBone);
                if (mountPoint != null) {
                    PlayerManager.MyInstance.PlayerUnitObject.transform.parent = mountPoint;
                    //PlayerManager.MyInstance.MyPlayerUnitObject.transform.localPosition = Vector3.zero;
                    PlayerManager.MyInstance.PlayerUnitObject.transform.position = mountPoint.transform.TransformPoint(originalPrefabOffset);
                    PlayerManager.MyInstance.PlayerUnitObject.transform.localEulerAngles = prefabProfile.SheathedRotation;
                    ActivateMountedState();
                }
            }
        }

        public void DeActivateMountedState() {
            //Debug.Log("MountEffect.DeActivateMountedState()");
            if (prefabObjects != null) {
                GameObject go = prefabObjects.Values.ElementAt(0);
                PlayerUnitMovementController playerUnitMovementController = go.GetComponent<PlayerUnitMovementController>();
                if (playerUnitMovementController != null && PlayerManager.MyInstance.PlayerUnitObject != null) {
                    //Debug.Log("Got Player Unit Movement Controller On Spawned Prefab (mount)");

                    //PlayerManager.MyInstance.MyCharacter.MyAnimatedUnit.MyRigidBody.constraints = RigidbodyConstraints.FreezeAll;

                    //Debug.Log("Setting Animator Values");

                    PlayerManager.MyInstance.ActiveUnitController = PlayerManager.MyInstance.UnitController;
                    ConfigureCharacterRegularPhysics();

                    // set player unit to normal state
                    PlayerManager.MyInstance.ActiveUnitController.Mounted = false;
                    if (PlayerManager.MyInstance.PlayerUnitMovementController) {
                        PlayerManager.MyInstance.PlayerUnitMovementController.enabled = true;
                    }
                    PlayerManager.MyInstance.ActiveUnitController.UnitAnimator.SetRiding(false);
                    //PlayerManager.MyInstance.MyCharacter.MyAnimatedUnit.MyCharacterAnimator.SetBool("Riding", false);
                    CameraManager.MyInstance.ActivateMainCamera();
                    CameraManager.MyInstance.MainCameraController.InitializeCamera(PlayerManager.MyInstance.MyCharacter.CharacterUnit.transform);


                }
            }
        }

        public void ActivateMountedState() {
            if (prefabObjects != null) {
                ConfigureMountPhysics();
                GameObject go = prefabObjects.Values.ElementAt(0);

                // disable movement and input on player unit
                if (PlayerManager.MyInstance.PlayerUnitMovementController) {
                    PlayerManager.MyInstance.PlayerUnitMovementController.enabled = false;
                }
                PlayerManager.MyInstance.ActiveUnitController.FreezeAll();

                // set player animator to riding state
                PlayerManager.MyInstance.ActiveUnitController.UnitAnimator.SetRiding(true);

                // set player unit to riding state
                PlayerManager.MyInstance.ActiveUnitController.Mounted = true;

                ConfigureCharacterMountedPhysics();

                // initialize the mount animator
                PlayerManager.MyInstance.ActiveUnitController = go.GetComponent<UnitController>();
                PlayerManager.MyInstance.ActiveUnitController.SetUnitControllerMode(UnitControllerMode.Mount);
                PlayerManager.MyInstance.ActiveUnitController.CharacterUnit = PlayerManager.MyInstance.MyCharacter.CharacterUnit;

                CameraManager.MyInstance.SwitchToMainCamera();
                CameraManager.MyInstance.MainCameraController.InitializeCamera(go.transform);

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
            UnitController unitController = dynamicCharacterAvatar.gameObject.GetComponent<UnitController>();
            if (unitController != null && unitController.UnitAnimator != null) {
                unitController.UnitAnimator.InitializeAnimator();
            } else {

            }
            dynamicCharacterAvatar.Initialize();
            // is this stuff necessary end

            UMAData umaData = dynamicCharacterAvatar.umaData;
            umaData.OnCharacterCreated += HandleCharacterCreated;
        }

        public void ConfigureCharacterMountedPhysics() {
            PlayerManager.MyInstance.ActiveUnitController.RigidBody.WakeUp();
            //PlayerManager.MyInstance.MyCharacter.MyAnimatedUnit.MyRigidBody.collisionDetectionMode = CollisionDetectionMode.Discrete;
            // DO NOT EVER USE CONTINUOUS SPECULATIVE.  IT WILL MESS THINGS UP EVEN WHEN YOUR RIGIDBODY IS KINEMATIC
            // UNITY ERROR MESSAGE IS MISLEADING AND WRONG HERE....
            //PlayerManager.MyInstance.MyCharacter.MyAnimatedUnit.MyRigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            PlayerManager.MyInstance.ActiveUnitController.RigidBody.interpolation = RigidbodyInterpolation.None;
            PlayerManager.MyInstance.ActiveUnitController.RigidBody.detectCollisions = false;
            PlayerManager.MyInstance.ActiveUnitController.RigidBody.isKinematic = true;
            PlayerManager.MyInstance.ActiveUnitController.RigidBody.useGravity = false;
            PlayerManager.MyInstance.ActiveUnitController.FreezeAll();
            //PlayerManager.MyInstance.MyCharacter.MyAnimatedUnit.MyRigidBody.constraints = RigidbodyConstraints.None;
            Collider collider = PlayerManager.MyInstance.PlayerUnitObject.GetComponent<Collider>();
            if (collider != null) {
                collider.enabled = false;
            }
            PlayerManager.MyInstance.ActiveUnitController.RigidBody.WakeUp();

        }

        public void ConfigureCharacterRegularPhysics() {
            PlayerManager.MyInstance.ActiveUnitController.RigidBody.WakeUp();
            PlayerManager.MyInstance.ActiveUnitController.RigidBody.detectCollisions = true;
            PlayerManager.MyInstance.ActiveUnitController.RigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            PlayerManager.MyInstance.ActiveUnitController.RigidBody.isKinematic = false;
            PlayerManager.MyInstance.ActiveUnitController.RigidBody.useGravity = true;
            PlayerManager.MyInstance.ActiveUnitController.FreezeRotation();
            Collider collider = PlayerManager.MyInstance.PlayerUnitObject.GetComponent<Collider>();
            if (collider != null) {
                collider.enabled = true;
            }
            PlayerManager.MyInstance.ActiveUnitController.RigidBody.WakeUp();
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
