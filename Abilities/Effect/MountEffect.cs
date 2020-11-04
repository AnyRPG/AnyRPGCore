using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;


namespace AnyRPG {
    [CreateAssetMenu(fileName = "New MountEffect", menuName = "AnyRPG/Abilities/Effects/MountEffect")]
    public class MountEffect : StatusEffect {

        [Header("Mount")]

        [Tooltip("Unit Prefab Profile to use for the mount object")]
        [SerializeField]
        private string unitProfileName = string.Empty;

        // reference to actual unitProfile
        private UnitProfile unitProfile = null;

        // reference to spawned mount UnitController
        private UnitController mountUnitController;

        public override void CancelEffect(BaseCharacter targetCharacter) {
            //Debug.Log("MountEffect.CancelEffect(" + (targetCharacter != null ? targetCharacter.name : "null") + ")");
            if (PlayerManager.MyInstance == null) {
                // game is in the middle of exiting
                return;
            }
            if (PlayerManager.MyInstance.UnitController != null) {
                PlayerManager.MyInstance.UnitController.transform.parent = PlayerManager.MyInstance.PlayerUnitParent.transform;

                //PlayerManager.MyInstance.MyPlayerUnitObject.transform.localEulerAngles = Vector3.zero;
                PlayerManager.MyInstance.UnitController.transform.localEulerAngles = mountUnitController.transform.localEulerAngles;

                // we could skip this and just let the player fall through gravity
                PlayerManager.MyInstance.UnitController.transform.position = mountUnitController.transform.position;
            }
            DeActivateMountedState();
            UnsubscribeFromModelReady();
            base.CancelEffect(targetCharacter);
        }

        /*
        // bypass the creation of the status effect and just make its visual prefab
        public void RawCast(BaseCharacter source, GameObject target, GameObject originalTarget, AbilityEffectOutput abilityEffectInput) {
            base.Cast(source, target, originalTarget, abilityEffectInput);
        }
        */

        public override Dictionary<PrefabProfile, GameObject> Cast(IAbilityCaster source, Interactable target, Interactable originalTarget, AbilityEffectContext abilityEffectInput) {
            //Debug.Log("StatusEffect.Cast(" + source.name + ", " + (target? target.name : "null") + ")");
            if (!CanUseOn(target, source)) {
                return null;
            }
            if (PlayerManager.MyInstance.PlayerUnitSpawned == false) {
                // we can't mount anything if the player unit is not spawned
                return null;
            }
            Dictionary<PrefabProfile, GameObject> returnObjects = base.Cast(source, target, originalTarget, abilityEffectInput);

            mountUnitController = unitProfile.SpawnUnitPrefab(source.transform.parent, source.transform.position, source.transform.forward, UnitControllerMode.Mount);
            if (mountUnitController != null) {
                //mountUnitController.SetMountMode();
                if (mountUnitController != null && mountUnitController.ModelReady == false) {
                    SubscribeToModelReady();
                } else {
                    HandleMountUnitSpawn();
                }
            }
            return returnObjects;
        }

        private void HandleMountUnitSpawn() {

            string originalPrefabSourceBone = unitProfile.UnitPrefabProfile.TargetBone;
            // NOTE: mount effects used sheathed position for character position.  do not use regular position to avoid putting mount below ground when spawning
            Vector3 originalPrefabOffset = unitProfile.UnitPrefabProfile.Position;

            if (originalPrefabSourceBone != null && originalPrefabSourceBone != string.Empty) {
                Transform mountPoint = mountUnitController.transform.FindChildByRecursive(originalPrefabSourceBone);
                if (mountPoint != null) {
                    PlayerManager.MyInstance.UnitController.transform.parent = mountPoint;
                    //PlayerManager.MyInstance.MyPlayerUnitObject.transform.localPosition = Vector3.zero;
                    PlayerManager.MyInstance.UnitController.transform.position = mountPoint.transform.TransformPoint(originalPrefabOffset);
                    PlayerManager.MyInstance.UnitController.transform.localEulerAngles = unitProfile.UnitPrefabProfile.Rotation;
                    ActivateMountedState();
                }
            }
        }

        public void DeActivateMountedState() {
            //Debug.Log("MountEffect.DeActivateMountedState()");
            if (mountUnitController != null) {
                if (PlayerManager.MyInstance.UnitController != null) {

                    PlayerManager.MyInstance.SetActiveUnitController(PlayerManager.MyInstance.UnitController);
                    ConfigureCharacterRegularPhysics();

                    // set player unit to normal state
                    PlayerManager.MyInstance.ActiveUnitController.Mounted = false;
                    if (PlayerManager.MyInstance.PlayerUnitMovementController) {
                        PlayerManager.MyInstance.PlayerUnitMovementController.enabled = true;
                    }
                    PlayerManager.MyInstance.ActiveUnitController.UnitAnimator.SetRiding(false);
                    //PlayerManager.MyInstance.MyCharacter.MyAnimatedUnit.MyCharacterAnimator.SetBool("Riding", false);
                    CameraManager.MyInstance.ActivateMainCamera();
                    CameraManager.MyInstance.MainCameraController.InitializeCamera(PlayerManager.MyInstance.ActiveUnitController.transform);
                }
            }
        }

        public void ActivateMountedState() {
            if (prefabObjects != null) {

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

                PlayerManager.MyInstance.SetActiveUnitController(mountUnitController);

                // set the mount character Unit to be the player unit that is on the mount.
                // this will theoretically allow the character to be attacked while mounted.
                // TODO : test that this works
                PlayerManager.MyInstance.ActiveUnitController.CharacterUnit = PlayerManager.MyInstance.UnitController.CharacterUnit;

                CameraManager.MyInstance.SwitchToMainCamera();
                CameraManager.MyInstance.MainCameraController.InitializeCamera(mountUnitController.transform);

            }
        }

        public void HandleModelReady() {
            //Debug.Log("PlayerManager.CharacterCreatedCallback(): " + umaData);
            UnsubscribeFromModelReady();
            HandleMountUnitSpawn();
        }

        public void UnsubscribeFromModelReady() {
            if (mountUnitController != null) {
                mountUnitController.OnModelReady -= HandleModelReady;
            }
        }

        public void SubscribeToModelReady() {
            if (mountUnitController != null) {
                mountUnitController.OnModelReady += HandleModelReady;
            }
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

            // TODO : should this just set to trigger instead so player go through portals and be attacked on mount?
            //PlayerManager.MyInstance.ActiveUnitController.Collider.enabled = false;
            // TESTING IT NOW
            PlayerManager.MyInstance.ActiveUnitController.Collider.isTrigger = true;

            PlayerManager.MyInstance.ActiveUnitController.RigidBody.WakeUp();

        }

        public void ConfigureCharacterRegularPhysics() {
            PlayerManager.MyInstance.ActiveUnitController.RigidBody.WakeUp();
            PlayerManager.MyInstance.ActiveUnitController.RigidBody.detectCollisions = true;
            PlayerManager.MyInstance.ActiveUnitController.RigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            PlayerManager.MyInstance.ActiveUnitController.RigidBody.isKinematic = false;
            PlayerManager.MyInstance.ActiveUnitController.RigidBody.useGravity = true;
            PlayerManager.MyInstance.ActiveUnitController.FreezeRotation();

            // testing - this used to disable the collider
            PlayerManager.MyInstance.ActiveUnitController.Collider.isTrigger = false;

            PlayerManager.MyInstance.ActiveUnitController.RigidBody.WakeUp();
        }

        public override void SetupScriptableObjects() {
            base.SetupScriptableObjects();
            if (unitProfileName != null && unitProfileName != string.Empty) {
                UnitProfile tmpUnitProfile = SystemUnitProfileManager.MyInstance.GetResource(unitProfileName);
                if (tmpUnitProfile != null) {
                    unitProfile = tmpUnitProfile;
                } else {
                    Debug.LogError("MountEffect.SetupScriptableObjects(): Could not find prefab Profile : " + unitProfileName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                }
            } else {
                Debug.LogError("MountEffect.SetupScriptableObjects(): Mount effect requires a unit prefab profile but non was configured while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
            }

        }



    }
}
