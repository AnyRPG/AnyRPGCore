using AnyRPG;
using UnityEngine;
using UnityEngine.AI;

namespace AnyRPG {
    public class UnitMountManager {

        // unit controller of controlling unit
        private UnitController unitController;

        // properties of mount
        private UnitController mountUnitController = null;
        private UnitProfile mountUnitProfile = null;

        public UnitController MountUnitController { get => mountUnitController; }

        public UnitMountManager(UnitController unitController) {
            this.unitController = unitController;
        }

        public void SetMountedState(UnitController mountUnitController, UnitProfile mountUnitProfile) {
            this.mountUnitController = mountUnitController;
            this.mountUnitProfile = mountUnitProfile;
            mountUnitController.SetRider(unitController);
            if (mountUnitController != null && mountUnitController.ModelReady == false) {
                SubscribeToMountModelReady();
            } else {
                HandleMountUnitSpawn();
            }
        }

        public void SubscribeToMountModelReady() {
            if (mountUnitController != null) {
                mountUnitController.OnModelReady += HandleMountModelReady;
            }
        }

        public void HandleMountModelReady() {
            //Debug.Log("PlayerManager.CharacterCreatedCallback(): " + umaData);
            UnsubscribeFromMountModelReady();
            HandleMountUnitSpawn();
        }

        public void UnsubscribeFromMountModelReady() {
            if (mountUnitController != null) {
                mountUnitController.OnModelReady -= HandleMountModelReady;
            }
        }


        private void HandleMountUnitSpawn() {

            string originalPrefabSourceBone = mountUnitProfile.UnitPrefabProps.TargetBone;
            // NOTE: mount effects used sheathed position for character position.  do not use regular position to avoid putting mount below ground when spawning
            Vector3 originalPrefabOffset = mountUnitProfile.UnitPrefabProps.Position;

            if (originalPrefabSourceBone != null && originalPrefabSourceBone != string.Empty) {
                Transform mountPoint = mountUnitController.transform.FindChildByRecursive(originalPrefabSourceBone);
                if (mountPoint != null) {
                    unitController.transform.parent = mountPoint;
                    //PlayerManager.MyInstance.MyPlayerUnitObject.transform.localPosition = Vector3.zero;
                    unitController.transform.position = mountPoint.transform.TransformPoint(originalPrefabOffset);
                    unitController.transform.localEulerAngles = mountUnitProfile.UnitPrefabProps.Rotation;
                    ActivateMountedState();
                }
            }
        }

        public void ActivateMountedState() {

            unitController.FreezeAll();

            // set player animator to riding state
            unitController.UnitAnimator.SetRiding(true);

            // set player unit to riding state
            unitController.Mounted = true;

            ConfigureCharacterMountedPhysics();

            // set the mount character Unit to be the player unit that is on the mount.
            // this will allow the character to be attacked while mounted.
            mountUnitController.CharacterUnit = unitController.CharacterUnit;

            unitController.NotifyOnActivateMountedState(mountUnitController);
        }

        public void ConfigureCharacterMountedPhysics() {
            unitController.RigidBody.WakeUp();
            //PlayerManager.MyInstance.MyCharacter.MyAnimatedUnit.MyRigidBody.collisionDetectionMode = CollisionDetectionMode.Discrete;
            // DO NOT EVER USE CONTINUOUS SPECULATIVE.  IT WILL MESS THINGS UP EVEN WHEN YOUR RIGIDBODY IS KINEMATIC
            // UNITY ERROR MESSAGE IS MISLEADING AND WRONG HERE....
            //PlayerManager.MyInstance.MyCharacter.MyAnimatedUnit.MyRigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            unitController.RigidBody.interpolation = RigidbodyInterpolation.None;
            unitController.RigidBody.detectCollisions = false;
            unitController.RigidBody.isKinematic = true;
            unitController.RigidBody.useGravity = false;
            unitController.FreezeAll();
            //PlayerManager.MyInstance.MyCharacter.MyAnimatedUnit.MyRigidBody.constraints = RigidbodyConstraints.None;

            // TODO : should this just set to trigger instead so player go through portals and be attacked on mount?
            //PlayerManager.MyInstance.ActiveUnitController.Collider.enabled = false;
            // TESTING IT NOW
            // duplicate collider triggers since mount is redirected - disabling
            //unitController.Collider.isTrigger = true;
            unitController.Collider.enabled = false;


            unitController.RigidBody.WakeUp();
        }

        public void DeActivateMountedState() {
            //Debug.Log(unitController.gameObject.name + ".UnitMountManager.DeActivateMountedState()");
            UnsubscribeFromMountModelReady();
            if (mountUnitController != null && unitController != null && unitController.enabled == true) {

                unitController.transform.parent = PlayerManager.MyInstance.PlayerUnitParent.transform;

                //PlayerManager.MyInstance.MyPlayerUnitObject.transform.localEulerAngles = Vector3.zero;
                unitController.transform.localEulerAngles = mountUnitController.transform.localEulerAngles;

                // we could skip this and just let the player fall through gravity
                unitController.transform.position = mountUnitController.transform.position;

                ConfigureCharacterRegularPhysics();

                // set player unit to normal state
                unitController.Mounted = false;
                // testing disabled now since there is only one of those
                /*
                if (PlayerManager.MyInstance.PlayerUnitMovementController) {
                    PlayerManager.MyInstance.PlayerUnitMovementController.enabled = true;
                }
                */
                unitController.UnitAnimator.SetRiding(false);
                //PlayerManager.MyInstance.MyCharacter.MyAnimatedUnit.MyCharacterAnimator.SetBool("Riding", false);

                unitController.NotifyOnDeActivateMountedState();
            }
            if (mountUnitController != null) { 
                GameObject.Destroy(mountUnitController.gameObject);

                mountUnitController = null;
                mountUnitProfile = null;

            }
        }

        public void ConfigureCharacterRegularPhysics() {
            //Debug.Log(unitController.gameObject.name + ".UnitMountManager.ConfigureCharacterRegularPhysics()");
            unitController.RigidBody.WakeUp();
            unitController.RigidBody.detectCollisions = true;
            unitController.RigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            unitController.RigidBody.isKinematic = false;
            unitController.RigidBody.useGravity = true;
            unitController.FreezeRotation();

            // testing - this used to disable the collider
            // since mounts redirect to character, this results in 2 collider triggers
            //unitController.Collider.isTrigger = false;
            unitController.Collider.enabled = true;

            unitController.RigidBody.WakeUp();
        }

    }

}