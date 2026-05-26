using System.Collections;
using UnityEngine;

namespace AnyRPG {
    public class UnitMountManager : ConfiguredClass, ICharacterRequestor {

        // unit controller of controlling unit
        private UnitController unitController;

        // properties of mount
        private UnitController mountUnitController = null;
        private UnitProfile mountUnitProfile = null;

        // state tracking
        private bool lateJoin = false;

        // game manager references
        private PlayerManagerServer playerManagerServer = null;
        private CharacterManager characterManager = null;

        public UnitController MountUnitController { get => mountUnitController; }
        public bool LateJoin { get => lateJoin; set => lateJoin = value; }

        public UnitMountManager(UnitController unitController, SystemGameManager systemGameManager) {
            this.unitController = unitController;
            Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            characterManager = systemGameManager.CharacterManager;
            playerManagerServer = systemGameManager.PlayerManagerServer;
        }

        public void SummonMount(UnitProfile mountUnitProfile) {
            //Debug.Log($"{unitController.gameObject.name}.UnitMountManager.SummonMount({mountUnitProfile.ResourceName})");

            CharacterConfigurationRequest characterConfigurationRequest = new CharacterConfigurationRequest(mountUnitProfile);
            characterConfigurationRequest.unitControllerMode = UnitControllerMode.Mount;
            CharacterRequestData characterRequestData = new CharacterRequestData(this,
                systemGameManager.GameMode,
                characterConfigurationRequest);
            characterRequestData.characterId = characterManager.GetNewCharacterId(UnitControllerMode.Mount);
            if (networkManagerServer.ServerModeActive == true) {
                characterRequestData.requestMode = GameMode.Network;
                if (playerManagerServer.ActiveUnitControllerLookup.ContainsKey(unitController)) {
                    characterRequestData.accountId = playerManagerServer.ActiveUnitControllerLookup[unitController];
                }
                characterManager.SpawnUnitPrefab(characterRequestData, null, unitController.transform.position, unitController.transform.forward, unitController.gameObject.scene);
            } else {
                characterManager.SpawnUnitPrefabLocal(characterRequestData, unitController.transform.parent, unitController.transform.position, unitController.transform.forward);
            }

        }

        public void ConfigureSpawnedCharacter(UnitController mountUnitController) {
            //Debug.Log($"{unitController.gameObject.name}.UnitMountManager.ConfigureSpawnedCharacter({mountUnitController.gameObject.name})");
        }

        public void PostInit(UnitController mountUnitController) {
            //Debug.Log($"{unitController.gameObject.name}.UnitMountManager.PostInit(mountUnitController: {mountUnitController.gameObject.name})");

            SetMountedState(mountUnitController, mountUnitController.CharacterRequestData.characterConfigurationRequest.unitProfile);
        }

        public void SetMountedState(UnitController mountUnitController, UnitProfile mountUnitProfile) {
            //Debug.Log($"{unitController.gameObject.name}.UnitMountManager.SetMountedState({mountUnitController.gameObject.name}, {mountUnitProfile.ResourceName})");

            unitController.CharacterPetManager.DespawnAllPets();

            this.mountUnitController = mountUnitController;
            this.mountUnitProfile = mountUnitProfile;
            mountUnitController.SetRider(unitController);

            unitController.UnitEventController.NotifyOnSetMountedState(mountUnitController, mountUnitProfile);

            //if (systemGameManager.GameMode == GameMode.Local || networkManagerServer.ServerModeActive == true) {
                if (mountUnitController?.UnitModelController != null && mountUnitController.UnitModelController.ModelCreated == false) {
                    SubscribeToMountModelReady();
                } else {
                    HandleMountUnitSpawn();
                }
            //}
        }

        public void SubscribeToMountModelReady() {
            //Debug.Log($"{unitController.gameObject.name}.UnitMountManager.SubscribeToMountModelReady()");

            if (mountUnitController?.UnitModelController != null) {
                //mountUnitController.UnitModelController.OnModelUpdated += HandleMountModelReady;
                mountUnitController.UnitModelController.OnModelCreated += HandleMountModelReady;
            } else {
                Debug.LogError($"{unitController.gameObject.name}.UnitMountManager.SubscribeToMountModelReady() mountUnitController or UnitModelController is null");
            }
        }

        public void HandleMountModelReady() {
            //Debug.Log($"{unitController.gameObject.name}.UnitMountManager.HandleMountModelReady(lateJoin: {lateJoin})");

            UnsubscribeFromMountModelReady();
            //if (lateJoin == true) {
            //    ActivateMountedState();
            //} else {
                // for now we are doing this all the time due to FishNet CSP code requiring us to parent the model because
                // it interferes with the networkTransform parenting
                HandleMountUnitSpawn();
            //}
        }

        public void UnsubscribeFromMountModelReady() {
            //Debug.Log($"{unitController.gameObject.name}.UnitMountManager.UnsubscribeFromMountModelReady()");

            if (mountUnitController?.UnitModelController != null) {
                //mountUnitController.UnitModelController.OnModelUpdated -= HandleMountModelReady;
                mountUnitController.UnitModelController.OnModelCreated -= HandleMountModelReady;
            }
        }

        public void HandleMountUnitSpawn() {
            //Debug.Log($"{unitController.gameObject.name}.UnitMountManager.HandleMountUnitSpawn() lateJoin: {lateJoin}");

            string originalPrefabSourceBone = mountUnitProfile.UnitPrefabProps.TargetBone;
            // NOTE: mount effects used sheathed position for character position.  do not use regular position to avoid putting mount below ground when spawning
            Vector3 originalPrefabOffset = mountUnitProfile.UnitPrefabProps.Position;

            if (originalPrefabSourceBone != null && originalPrefabSourceBone != string.Empty) {
                Transform mountPoint = mountUnitController.transform.FindChildByRecursive(originalPrefabSourceBone);
                if (mountPoint != null) {
                    /*
                    if (lateJoin == true) {
                        unitController.transform.SetParent(null);
                        unitController.transform.localScale = Vector3.one;
                    }
                    */
                    unitController.UnitEventController.NotifyOnSetParent(mountPoint);
                    // print scale and lossy scale for unit and mount point
                    //Debug.Log($"{unitController.gameObject.name}.UnitMountManager.HandleMountUnitSpawn() before mounting scale: {unitController.transform.localScale} lossyScale: {unitController.transform.lossyScale} mScale: {mountPoint.localScale} mLossyScale: {mountPoint.lossyScale}");
                    if (systemGameManager.GameMode == GameMode.Local || networkManagerServer.ServerModeActive == false) {
                        //if (systemGameManager.GameMode == GameMode.Local || (unitController.IsOwner == true && networkManagerServer.ServerModeActive == false)) {
                        //if (systemGameManager.GameMode == GameMode.Local) {
                        if (lateJoin == false) {
                            unitController.transform.parent = mountPoint;
                        } else {
                            unitController.transform.SetParent(mountPoint, false);
                            unitController.transform.localScale = new Vector3(
                                1f / mountPoint.lossyScale.x,
                                1f / mountPoint.lossyScale.y,
                                1f / mountPoint.lossyScale.z
                            );
                            // recalculate nameplate vector since we are changing scale and it is based on lossy scale
                            unitController.NameplateController.SetNameplatePosition();
                        }
                    }
                    //Debug.Log($"{unitController.gameObject.name}.UnitMountManager.HandleMountUnitSpawn() after mounting scale: {unitController.transform.localScale} lossyScale: {unitController.transform.lossyScale} mScale: {mountPoint.localScale} mLossyScale: {mountPoint.lossyScale}");
                    //if (systemGameManager.GameMode == GameMode.Local || networkManagerServer.ServerModeActive == true || (unitController.IsOwner == true && networkManagerServer.ServerModeActive == false)) {
                    unitController.transform.position = mountPoint.transform.TransformPoint(originalPrefabOffset);
                    //unitController.transform.localEulerAngles = mountUnitProfile.UnitPrefabProps.Rotation;
                    //unitController.transform.rotation = Quaternion.identity;
                    unitController.transform.localRotation = Quaternion.identity;
                    //}

                    // testing - is there a reason we wouldn't want to activemounted state on all server and clients?
                    //if (systemGameManager.GameMode == GameMode.Local || networkManagerServer.ServerModeActive == true) {
                    ActivateMountedState();
                    //}
                }
            }
        }

        public void ActivateMountedState() {
            //Debug.Log($"{unitController.gameObject.name}.UnitMountManager.ActivateMountedState(lateJoin: {lateJoin})");

            unitController?.UnitModelController?.SheathWeapons();

            // set player animator to riding state
            if (systemGameManager.GameMode == GameMode.Local
                || networkManagerServer.ServerModeActive == true
                || unitController.IsOwner == true
                || lateJoin == true) {
                unitController.UnitAnimator.SetRiding(true);
            }

            // set player unit to riding state
            unitController.IsMounted = true;

            ConfigureCharacterMountedPhysics();

            // set the mount character Unit to be the player unit that is on the mount.
            // this will allow the character to be attacked while mounted.
            mountUnitController.CharacterUnit = unitController.CharacterUnit;

            unitController.UnitEventController.NotifyOnActivateMountedState(mountUnitController);
            lateJoin = false;
            if ((systemGameManager.GameMode == GameMode.Local || networkManagerServer.ServerModeActive == false) && unitController.IsTargeted == true) {
                unitController.SetUnTargeted();
                mountUnitController.SetTargeted();
            }
            unitController.UnitMovementController.ChangeState(CharacterMovementState.Riding, false);
        }

        public void ConfigureCharacterMountedPhysics() {
            //Debug.Log($"{unitController.gameObject.name}.UnitMountManager.ConfigureCharacterMountedPhysics()");

            unitController.RigidBody.WakeUp();
            //playerManager.UnitController.MyAnimatedUnit.MyRigidBody.collisionDetectionMode = CollisionDetectionMode.Discrete;
            // DO NOT EVER USE CONTINUOUS SPECULATIVE.  IT WILL MESS THINGS UP EVEN WHEN YOUR RIGIDBODY IS KINEMATIC
            // UNITY ERROR MESSAGE IS MISLEADING AND WRONG HERE....
            //playerManager.UnitController.MyAnimatedUnit.MyRigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            unitController.RigidBody.interpolation = RigidbodyInterpolation.None;
            unitController.RigidBody.detectCollisions = false;
            //Debug.Log($"{unitController.gameObject.name}.UnitMountManager.ConfigureCharacterMountedPhysics() set kinematic true");
            unitController.RigidBody.isKinematic = true;
            unitController.RigidBody.useGravity = false;
            unitController.FreezeAll();
            //playerManager.UnitController.MyAnimatedUnit.MyRigidBody.constraints = RigidbodyConstraints.None;

            // TODO : should this just set to trigger instead so player go through portals and be attacked on mount?
            //playerManager.ActiveUnitController.Collider.enabled = false;
            // TESTING IT NOW
            // duplicate collider triggers since mount is redirected - disabling
            //unitController.Collider.isTrigger = true;
            unitController.DisableCollider();


            unitController.RigidBody.WakeUp();
        }

        public void DeactivateMountedState() {
            //Debug.Log($"{unitController.gameObject.name}.UnitMountManager.DeactivateMountedState() frame: {Time.frameCount}");

            lateJoin = false;
            UnsubscribeFromMountModelReady();
            if (mountUnitController != null && unitController != null && unitController.enabled == true) {

                // disable the mount capsule collider to prevent it from bouncing the player when the player dismounts.
                mountUnitController.DisableCollider();

                //unitController.transform.parent = playerManager.PlayerUnitParent.transform;
                unitController.UnitEventController.NotifyOnUnsetParent();
                //if (systemGameManager.GameMode == GameMode.Local || (unitController.IsOwner == true && networkManagerServer.ServerModeActive == false)) {
                //if (systemGameManager.GameMode == GameMode.Local) {
                if (systemGameManager.GameMode == GameMode.Local || (networkManagerServer.ServerModeActive == false)) {
                    unitController.transform.parent = null;
                    //Debug.Break();
                }
                //Debug.Log($"{unitController.gameObject.name}.UnitMountManager.DeactivateMountedState() frame: {Time.frameCount} setting position and rotation to mount {mountUnitController.UnitMotor.MovementBody.GetPosition()}");
                unitController.transform.position = mountUnitController.UnitMotor.MovementBody.GetPosition();
                unitController.transform.rotation = mountUnitController.UnitMotor.MovementBody.GetRotation();
                unitController.UnitModelController.UnitModel.transform.localPosition = Vector3.zero;
                unitController.UnitModelController.UnitModel.transform.localRotation = Quaternion.identity;
                //unitController.UnitMotor.MovementBody.SetPosition(mountUnitController.UnitMotor.MovementBody.GetPosition());
                //Debug.Log($"{unitController.gameObject.name}.UnitMountManager.DeactivateMountedState() frame: {Time.frameCount} after set: tPosition {unitController.transform.position} rPosition: {unitController.UnitMotor.MovementBody.GetPosition()} mPosition: {unitController.UnitModelController.UnitModel.transform.position}");
                Physics.SyncTransforms();
                ConfigureCharacterRegularPhysics();

                //if (systemGameManager.GameMode == GameMode.Local || networkManagerServer.ServerModeActive == true || (unitController.IsOwner == true && networkManagerServer.ServerModeActive == false)) {
                    //Debug.Log($"{unitController.gameObject.name}.UnitMountManager.DeactivateMountedState() setting position and rotation to mount {mountUnitController.UnitMotor.MovementBody.GetPosition()}");
                    /*
                    unitController.UnitMotor.SetPosition(mountUnitController.UnitMotor.MovementBody.GetPosition());
                    unitController.UnitMotor.FaceDirection(mountUnitController.UnitMotor.MovementBody.GetForward());
                    */
                    //Physics.SyncTransforms();
                //unitController.UnitMotor.MovementBody.SetPosition(mountUnitController.UnitMotor.MovementBody.GetPosition());

                //Debug.Log($"{unitController.gameObject.name}.UnitMountManager.DeactivateMountedState() after sync: tPosition: {unitController.transform.position} rPosition: {unitController.UnitMotor.MovementBody.GetPosition()} mPosition: {unitController.UnitModelController.UnitModel.transform.position}");
                //unitController.transform.localEulerAngles = mountUnitController.transform.localEulerAngles;
                // we could skip this and just let the player fall through gravity
                //unitController.transform.position = mountUnitController.transform.position;
                //}


                // set player unit to normal state
                unitController.IsMounted = false;
                // testing disabled now since there is only one of those
                /*
                if (playerManager.PlayerUnitMovementController) {
                    playerManager.PlayerUnitMovementController.enabled = true;
                }
                */
                if (systemGameManager.GameMode == GameMode.Local
                    || (networkManagerServer.ServerModeActive == false && unitController.IsOwner == true)
                    || networkManagerServer.ServerModeActive == true) {
                    unitController.UnitAnimator.SetRiding(false);
                }
                //playerManager.UnitController.MyAnimatedUnit.MyCharacterAnimator.SetBool("Riding", false);

                unitController.UnitEventController.NotifyOnDeactivateMountedState();
            }
            if ((systemGameManager.GameMode == GameMode.Local || networkManagerServer.ServerModeActive == false) && mountUnitController.IsTargeted == true) {
                unitController.SetTargeted();
                mountUnitController.SetUnTargeted();
            }
            //if (systemGameManager.GameMode == GameMode.Local) {
                DespawnMountUnit();
            //}
            if (unitController.CharacterCombat.GetInCombat() == true) {
                unitController.UnitModelController.HoldWeapons();
            }
            unitController.UnitMovementController.ChangeState(CharacterMovementState.Idle, false);
        }

        public void DespawnMountUnit() {
            //Debug.Log($"{unitController.gameObject.name}.UnitMountManager.DespawnMountUnit() frame: {Time.frameCount} parent: {unitController.transform.parent?.gameObject.name}");

            if (mountUnitController != null) {
                //if (systemGameManager.GameMode == GameMode.Local || networkManagerServer.ServerModeActive == true) {
                    unitController.StartCoroutine(DespawnMountDelay());
                //} else {
                    //mountUnitController = null;
                    //mountUnitProfile = null;
                //}
            }
            //unitController.UnitEventController.NotifyOnDespawnMountUnit();
        }

        /// <summary>
        /// add a delay before mount despawn because the parent unset of the player doesn't happen in the same frame
        /// so if you despawn the mount the same frame it despawns the player too
        /// </summary>
        /// <returns></returns>
        public IEnumerator DespawnMountDelay () {
            //Debug.Log($"{unitController.gameObject.name}.UnitMountManager.DespawnMountDelay() frame: {Time.frameCount} parent: {unitController.transform.parent?.gameObject.name}");

            while (unitController.transform.parent != null) {
                //Debug.Log($"{unitController.gameObject.name}.UnitMountManager.DespawnMountDelay() frame: {Time.frameCount} parent: {unitController.transform.parent?.gameObject.name}");
                yield return null;
            }

            // reset the character unit before despawn so the mount doesn't send despawn events to the player that was riding it
            mountUnitController.CharacterUnit = mountUnitController.GetFirstInteractableOption(typeof(CharacterUnit)) as CharacterUnit;
            mountUnitController.Despawn(0f, false, true);
            mountUnitController = null;
            mountUnitProfile = null;
        }

        public void ConfigureCharacterRegularPhysics() {
            //Debug.Log($"{unitController.gameObject.name}.UnitMountManager.ConfigureCharacterRegularPhysics() position: {unitController.transform.position} parent: {unitController.transform.parent?.gameObject.name}");

            unitController.RigidBody.WakeUp();
            unitController.RigidBody.detectCollisions = true;
            unitController.RigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            //Debug.Log($"{unitController.gameObject.name}.UnitMountManager.ConfigureCharacterRegularPhysics() set kinematic false");
            unitController.RigidBody.isKinematic = false;
            if (systemGameManager.GameMode == GameMode.Local || networkManagerServer.ServerModeActive == true || (systemGameManager.GameMode == GameMode.Network && unitController.IsOwner == true)) {
                unitController.RigidBody.useGravity = true;
            } else {
                unitController.RigidBody.useGravity = false;
            }
            if (systemGameManager.GameMode == GameMode.Local) {
                // in network mode, we never interpolate because it interferes with tick smoother component
                unitController.RigidBody.interpolation = RigidbodyInterpolation.Interpolate;
            }
            if (systemGameManager.GameMode == GameMode.Local || (systemGameManager.GameMode == GameMode.Network && unitController.IsOwner == true)) {
                // only local clients or authoritative network clients should unfreeze gravity
                unitController.FreezePositionXZ();
            }

            // testing - this used to disable the collider
            // since mounts redirect to character, this results in 2 collider triggers
            //unitController.Collider.isTrigger = false;
            unitController.EnableCollider();

            unitController.RigidBody.WakeUp();
        }

        public void ProcessUnsetParent() {
            /*
            if (mountUnitController != null) {
                mountUnitController.gameObject.SetActive(false);
            }
            */
        }

        /*
        public void PostInit() {
            //Debug.Log($"{unitController.gameObject.name}.UnitMountManager.PostInit()");

            if (unitController.CharacterSaveManager.SaveData.IsMounted == false) {
                //Debug.Log($"{unitController.gameObject.name}.UnitMountManager.ProcessModelCreated() isMounted = false");
                return;
            }
            //Debug.Log($"{unitController.gameObject.name}.UnitMountManager.ProcessModelCreated() isMounted = TRUE");

            UnitController mountUnitController = null;
            if (unitController.transform.parent != null) {
                mountUnitController = unitController.transform.parent.GetComponentInParent<UnitController>();
            }
            if (mountUnitController == null) {
                //Debug.Log($"{unitController.gameObject.name}.UnitMountManager.PostInit() could not find mount unit controller on parent {unitController.transform.parent?.gameObject.name}");
                return;
            }
            //Debug.Log($"{unitController.gameObject.name}.UnitMountManager.ProcessModelCreated() mountUnitController: {mountUnitController.gameObject.name}");
            lateJoin = true;
            SetMountedState(mountUnitController, mountUnitController.CharacterRequestData.characterConfigurationRequest.unitProfile);
            if (mountUnitController?.UnitModelController != null && mountUnitController.UnitModelController.ModelCreated == false) {
                SubscribeToMountModelReady();
            } else {
                ActivateMountedState();
            }
        }
        */
    }

}