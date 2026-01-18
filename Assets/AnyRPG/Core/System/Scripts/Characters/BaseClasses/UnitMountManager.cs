using AnyRPG;
using System;
using System.Collections;
using System.Globalization;
using UnityEngine;
using UnityEngine.AI;

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
        private PlayerManager playerManager = null;
        private PlayerManagerServer playerManagerServer = null;
        private CharacterManager characterManager = null;

        public UnitController MountUnitController { get => mountUnitController; }

        public UnitMountManager(UnitController unitController, SystemGameManager systemGameManager) {
            this.unitController = unitController;
            Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            playerManager = systemGameManager.PlayerManager;
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
            //Debug.Log($"{mountUnitController.gameObject.name}.CharacterAbilityManager.PostInit()");

            SetMountedState(mountUnitController, mountUnitController.CharacterRequestData.characterConfigurationRequest.unitProfile);
        }

        public void SetMountedState(UnitController mountUnitController, UnitProfile mountUnitProfile) {
            //Debug.Log($"{unitController.gameObject.name}.UnitMountManager.SetMountedState({mountUnitController.gameObject.name}, {mountUnitProfile.ResourceName})");

            unitController.CharacterPetManager.DespawnAllPets();

            this.mountUnitController = mountUnitController;
            this.mountUnitProfile = mountUnitProfile;
            mountUnitController.SetRider(unitController);

            unitController.UnitEventController.NotifyOnSetMountedState(mountUnitController, mountUnitProfile);

            if (systemGameManager.GameMode == GameMode.Local || networkManagerServer.ServerModeActive == true || unitController.IsOwner == true) {
                if (mountUnitController?.UnitModelController != null && mountUnitController.UnitModelController.ModelCreated == false) {
                    SubscribeToMountModelReady();
                } else {
                    HandleMountUnitSpawn();
                }
            }
        }

        public void SubscribeToMountModelReady() {
            if (mountUnitController?.UnitModelController != null) {
                //mountUnitController.UnitModelController.OnModelUpdated += HandleMountModelReady;
                mountUnitController.UnitModelController.OnModelCreated += HandleMountModelReady;
            }
        }

        public void HandleMountModelReady() {
            //Debug.Log($"{unitController.gameObject.name}.UnitMountManager.HandleMountModelReady()");

            UnsubscribeFromMountModelReady();
            if (lateJoin == true) {
                ActivateMountedState(true);
            } else {
                HandleMountUnitSpawn();
            }
        }

        public void UnsubscribeFromMountModelReady() {
            //Debug.Log($"{unitController.gameObject.name}.UnitMountManager.UnsubscribeFromMountModelReady()");

            if (mountUnitController?.UnitModelController != null) {
                //mountUnitController.UnitModelController.OnModelUpdated -= HandleMountModelReady;
                mountUnitController.UnitModelController.OnModelCreated -= HandleMountModelReady;
            }
        }


        public void HandleMountUnitSpawn() {
            //Debug.Log($"{unitController.gameObject.name}.UnitMountManager.HandleMountUnitSpawn()");

            string originalPrefabSourceBone = mountUnitProfile.UnitPrefabProps.TargetBone;
            // NOTE: mount effects used sheathed position for character position.  do not use regular position to avoid putting mount below ground when spawning
            Vector3 originalPrefabOffset = mountUnitProfile.UnitPrefabProps.Position;

            if (originalPrefabSourceBone != null && originalPrefabSourceBone != string.Empty) {
                Transform mountPoint = mountUnitController.transform.FindChildByRecursive(originalPrefabSourceBone);
                if (mountPoint != null) {
                    unitController.UnitEventController.NotifyOnSetParent(mountPoint);
                    if (systemGameManager.GameMode == GameMode.Local) {
                        unitController.transform.parent = mountPoint;
                    }
                    if (systemGameManager.GameMode == GameMode.Local || networkManagerServer.ServerModeActive == false) {
                        unitController.transform.position = mountPoint.transform.TransformPoint(originalPrefabOffset);
                        unitController.transform.localEulerAngles = mountUnitProfile.UnitPrefabProps.Rotation;
                    }
                    if (systemGameManager.GameMode == GameMode.Local || networkManagerServer.ServerModeActive == true) {
                        ActivateMountedState();
                    }
                }
            }
            unitController.UnitEventController.NotifyOnMountUnitSpawn();
        }

        public void ActivateMountedState(bool lateJoin = false) {
            //Debug.Log($"{unitController.gameObject.name}.UnitMountManager.ActivateMountedState()");

            unitController?.UnitModelController?.SheathWeapons();

            // set player animator to riding state
            if (systemGameManager.GameMode == GameMode.Local
                || (networkManagerServer.ServerModeActive == false && unitController.IsOwner == true)
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
        }

        public void ConfigureCharacterMountedPhysics() {
            unitController.RigidBody.WakeUp();
            //playerManager.UnitController.MyAnimatedUnit.MyRigidBody.collisionDetectionMode = CollisionDetectionMode.Discrete;
            // DO NOT EVER USE CONTINUOUS SPECULATIVE.  IT WILL MESS THINGS UP EVEN WHEN YOUR RIGIDBODY IS KINEMATIC
            // UNITY ERROR MESSAGE IS MISLEADING AND WRONG HERE....
            //playerManager.UnitController.MyAnimatedUnit.MyRigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            unitController.RigidBody.interpolation = RigidbodyInterpolation.None;
            unitController.RigidBody.detectCollisions = false;
            unitController.RigidBody.isKinematic = true;
            unitController.RigidBody.useGravity = false;
            unitController.FreezeAll();
            //playerManager.UnitController.MyAnimatedUnit.MyRigidBody.constraints = RigidbodyConstraints.None;

            // TODO : should this just set to trigger instead so player go through portals and be attacked on mount?
            //playerManager.ActiveUnitController.Collider.enabled = false;
            // TESTING IT NOW
            // duplicate collider triggers since mount is redirected - disabling
            //unitController.Collider.isTrigger = true;
            unitController.Collider.enabled = false;


            unitController.RigidBody.WakeUp();
        }

        public void DeactivateMountedState() {
            //Debug.Log($"{unitController.gameObject.name}.UnitMountManager.DeactivateMountedState()");

            lateJoin = false;
            UnsubscribeFromMountModelReady();
            if (mountUnitController != null && unitController != null && unitController.enabled == true) {

                //unitController.transform.parent = playerManager.PlayerUnitParent.transform;
                unitController.UnitEventController.NotifyOnUnsetParent();
                if (systemGameManager.GameMode == GameMode.Local) {
                    unitController.transform.parent = null;
                }

                if (systemGameManager.GameMode == GameMode.Local || (networkManagerServer.ServerModeActive == false && unitController.IsOwner == true)) {
                    unitController.transform.localEulerAngles = mountUnitController.transform.localEulerAngles;
                    // we could skip this and just let the player fall through gravity
                    unitController.transform.position = mountUnitController.transform.position;
                }

                ConfigureCharacterRegularPhysics();

                // set player unit to normal state
                unitController.IsMounted = false;
                // testing disabled now since there is only one of those
                /*
                if (playerManager.PlayerUnitMovementController) {
                    playerManager.PlayerUnitMovementController.enabled = true;
                }
                */
                if (systemGameManager.GameMode == GameMode.Local || (networkManagerServer.ServerModeActive == false && unitController.IsOwner == true)) {
                    unitController.UnitAnimator.SetRiding(false);
                }
                //playerManager.UnitController.MyAnimatedUnit.MyCharacterAnimator.SetBool("Riding", false);

                unitController.UnitEventController.NotifyOnDeactivateMountedState();
            }
            if ((systemGameManager.GameMode == GameMode.Local || networkManagerServer.ServerModeActive == false) && mountUnitController.IsTargeted == true) {
                unitController.SetTargeted();
                mountUnitController.SetUnTargeted();
            }
            if (systemGameManager.GameMode == GameMode.Local) {
                DespawnMountUnit();
            }
            if (unitController.CharacterCombat.GetInCombat() == true) {
                unitController.UnitModelController.HoldWeapons();
            }
        }

        public void DespawnMountUnit() {
            //Debug.Log($"{unitController.gameObject.name}.UnitMountManager.DespawnMountUnit() frame: {Time.frameCount} parent: {unitController.transform.parent?.gameObject.name}");

            if (mountUnitController != null) {
                if (systemGameManager.GameMode == GameMode.Local || networkManagerServer.ServerModeActive == true) {
                    unitController.StartCoroutine(DespawnMountDelay());
                } else {
                    mountUnitController = null;
                    mountUnitProfile = null;
                }
            }
            unitController.UnitEventController.NotifyOnDespawnMountUnit();
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
            /*
            yield return null;
            //Debug.Log($"{unitController.gameObject.name}.UnitMountManager.DespawnMountDelay() frame: {Time.frameCount} parent: {unitController.transform.parent?.gameObject.name}");
            //Debug.Break();
            yield return null;
            //Debug.Log($"{unitController.gameObject.name}.UnitMountManager.DespawnMountDelay() frame: {Time.frameCount} parent: {unitController.transform.parent?.gameObject.name}");
            yield return null;
            //Debug.Log($"{unitController.gameObject.name}.UnitMountManager.DespawnMountDelay() frame: {Time.frameCount} parent: {unitController.transform.parent?.gameObject.name}");
            yield return null;
            //Debug.Log($"{unitController.gameObject.name}.UnitMountManager.DespawnMountDelay() frame: {Time.frameCount} parent: {unitController.transform.parent?.gameObject.name}");
            yield return null;
            //Debug.Log($"{unitController.gameObject.name}.UnitMountManager.DespawnMountDelay() frame: {Time.frameCount} parent: {unitController.transform.parent?.gameObject.name}");
            */
            //Debug.Log($"{unitController.gameObject.name}.UnitMountManager.DespawnMountDelay() frame: {Time.frameCount} parent: {(unitController.transform.parent == null ? "null" : unitController.transform.parent.gameObject.name)}");

            // reset the character unit before despawn so the mount doesn't send despawn events to the player that was riding it
            mountUnitController.CharacterUnit = mountUnitController.GetFirstInteractableOption(typeof(CharacterUnit)) as CharacterUnit;
            mountUnitController.Despawn(0f, false, true);
            mountUnitController = null;
            mountUnitProfile = null;
        }

        public void ConfigureCharacterRegularPhysics() {
            //Debug.Log($"{unitController.gameObject.name}.UnitMountManager.ConfigureCharacterRegularPhysics()");

            unitController.RigidBody.WakeUp();
            unitController.RigidBody.detectCollisions = true;
            unitController.RigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            unitController.RigidBody.isKinematic = false;
            if (networkManagerServer.ServerModeActive == true || (systemGameManager.GameMode == GameMode.Network && unitController.IsOwner == false)) {
                // movement is client authoritative, so gravity should not be applied on the server
                //Debug.Log($"{unitController.gameObject.name}.UnitMountManager.ConfigureCharacterRegularPhysics() turn OFF gravity");
                unitController.RigidBody.useGravity = false;
            } else {
                //Debug.Log($"{unitController.gameObject.name}.UnitMountManager.ConfigureCharacterRegularPhysics() turn ON gravity");
                unitController.RigidBody.useGravity = true;
            }
            unitController.RigidBody.interpolation = RigidbodyInterpolation.Interpolate;
            if (systemGameManager.GameMode == GameMode.Local || (systemGameManager.GameMode == GameMode.Network && unitController.IsOwner == true)) {
                // only local clients or authoritative network clients should unfreeze gravity
                unitController.FreezePositionXZ();
            }

            // testing - this used to disable the collider
            // since mounts redirect to character, this results in 2 collider triggers
            //unitController.Collider.isTrigger = false;
            unitController.Collider.enabled = true;

            unitController.RigidBody.WakeUp();
        }

        public void ProcessUnsetParent() {
            if (mountUnitController != null) {
                mountUnitController.gameObject.SetActive(false);
            }
        }

        public void ProcessModelCreated() {
            //Debug.Log($"{unitController.gameObject.name}.UnitMountManager.ProcessModelCreated()");

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
                return;
            }
            //Debug.Log($"{unitController.gameObject.name}.UnitMountManager.ProcessModelCreated() mountUnitController: {mountUnitController.gameObject.name}");
            lateJoin = true;
            SetMountedState(mountUnitController, mountUnitController.CharacterRequestData.characterConfigurationRequest.unitProfile);
            if (mountUnitController?.UnitModelController != null && mountUnitController.UnitModelController.ModelCreated == false) {
                SubscribeToMountModelReady();
            } else {
                ActivateMountedState(true);
            }
        }
    }

}