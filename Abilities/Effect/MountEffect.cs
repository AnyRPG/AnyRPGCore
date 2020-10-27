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

            mountUnitController = unitProfile.SpawnUnitPrefab(source.transform.parent, source.transform.position, source.transform.forward);
            if (mountUnitController != null) {
                mountUnitController.SetMountMode();
                if (mountUnitController != null && mountUnitController.DynamicCharacterAvatar != null) {
                    SubscribeToUMACreate();
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
                    PlayerManager.MyInstance.PlayerUnitObject.transform.parent = mountPoint;
                    //PlayerManager.MyInstance.MyPlayerUnitObject.transform.localPosition = Vector3.zero;
                    PlayerManager.MyInstance.PlayerUnitObject.transform.position = mountPoint.transform.TransformPoint(originalPrefabOffset);
                    PlayerManager.MyInstance.PlayerUnitObject.transform.localEulerAngles = unitProfile.UnitPrefabProfile.Rotation;
                    ActivateMountedState();
                }
            }
        }

        public void DeActivateMountedState() {
            //Debug.Log("MountEffect.DeActivateMountedState()");
            if (mountUnitController != null) {
                if (PlayerManager.MyInstance.PlayerUnitObject != null) {

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
                PlayerManager.MyInstance.ActiveUnitController.CharacterUnit = PlayerManager.MyInstance.MyCharacter.CharacterUnit;

                CameraManager.MyInstance.SwitchToMainCamera();
                CameraManager.MyInstance.MainCameraController.InitializeCamera(mountUnitController.transform);

            }
        }

        public void HandleCharacterCreated(UMAData umaData) {
            //Debug.Log("PlayerManager.CharacterCreatedCallback(): " + umaData);
            UnsubscribeFromUMACreate();
            HandleMountUnitSpawn();
        }

        public void UnsubscribeFromUMACreate() {
            if (mountUnitController.DynamicCharacterAvatar != null) {
                mountUnitController.DynamicCharacterAvatar.umaData.OnCharacterCreated -= HandleCharacterCreated;
            }
        }

        public void SubscribeToUMACreate() {
            if (mountUnitController.DynamicCharacterAvatar != null && mountUnitController.DynamicCharacterAvatar.umaData != null) {
                mountUnitController.DynamicCharacterAvatar.umaData.OnCharacterCreated += HandleCharacterCreated;
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
