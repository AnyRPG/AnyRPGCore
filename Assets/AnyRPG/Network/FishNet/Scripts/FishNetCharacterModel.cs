using FishNet.Component.Animating;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AnyRPG {
    public class FishNetCharacterModel : SpawnedNetworkObject {

        private SystemGameManager systemGameManager = null;
        private NetworkAnimator networkAnimator = null;
        private UnitController unitController = null;
        private Animator animator = null;

        private bool isClient = false;

        private void FindGameManager() {
            //Debug.Log($"{gameObject.name}.FishNetCharacterModel.FindGameManager() ownerId: {base.OwnerId}");

            // call character manager with spawnRequestId to complete configuration
            systemGameManager = GameObject.FindAnyObjectByType<SystemGameManager>();
            if (systemGameManager == null) {
                return;
            }
            networkAnimator = GetComponent<NetworkAnimator>();
            animator = GetComponent<Animator>();
            //Debug.Log($"{gameObject.name}.FishNetCharacterModel.FindGameManager(): animator: {animator.GetInstanceID()}");
            unitController = GetComponentInParent<UnitController>();
            if (unitController == null) {
                StartCoroutine(WaitForParent());
                return;
            }
            CompleteSharedConfiguration();
        }

        private void CompleteSharedConfiguration() {
            //Debug.Log($"{gameObject.name}.FishNetCharacterModel.CompleteSharedConfiguration(): ownerId: {base.OwnerId}");

            unitController.UnitEventController.OnInitializeAnimator += HandleInitializeAnimator;
            //unitController.UnitModelController.OnModelCreated += HandleModelCreated;

            // clients are authoritative for their own animators, and server is authoritative for all others
            //Debug.Log($"{gameObject.name}.FishNetCharacterModel.FindGameManager(): IsOwner: {base.IsOwner}, OwnerId: {base.OwnerId}, ServerModeActive: {systemGameManager.NetworkManagerServer.ServerModeActive}");
            if (base.IsOwner || (systemGameManager.NetworkManagerServer.ServerModeActive == true && base.OwnerId == -1)) {
                unitController.UnitEventController.OnAnimatorSetTrigger += HandleSetTrigger;
                unitController.UnitEventController.OnAnimatorResetTrigger += HandleResetTrigger;
            }
            if (isClient == true) {
                CompleteClientConfiguration();
            } else {
                CompleteServerConfiguration();
            }
        }

        private IEnumerator WaitForParent() {
            //Debug.Log($"{gameObject.name}.FishNetCharacterModel.WaitForParent(): ownerId: {base.OwnerId} frame: {Time.frameCount}");

            while (unitController == null) {
                yield return null;
                //Debug.Log($"{gameObject.name}.FishNetCharacterModel.WaitForParent(): ownerId: {base.OwnerId} frame: {Time.frameCount}");
                unitController = GetComponentInParent<UnitController>();
            }
            CompleteSharedConfiguration();
        }

        /*
        private void HandleModelCreated() {
            animator = GetComponent<Animator>();
            //Debug.Log($"{gameObject.name}.FishNetCharacterModel.HandleModelCreated(): animator: {animator.GetInstanceID()}");

            networkAnimator.SetAnimator(animator);
        }
        */

        private void HandleSetTrigger(string triggerName) {
            //Debug.Log($"{gameObject.name}.FishNetCharacterModel.HandleSetTrigger({triggerName})");

            networkAnimator.SetTrigger(triggerName);
        }

        private void HandleResetTrigger(string triggerName) {
            //Debug.Log($"{gameObject.name}.FishNetCharacterModel.HandleResetTrigger({triggerName})");

            networkAnimator.ResetTrigger(triggerName);
        }

        private void HandleInitializeAnimator() {
            //Debug.Log($"{gameObject.name}.FishNetCharacterModel.HandleInitializeAnimator()");

            networkAnimator.SetAnimator(animator);
        }

        private void CompleteModelRequest(bool isOwner) {
            //Debug.Log($"{gameObject.name}.FishNetCharacterModel.CompleteModelRequest() isOwner: {isOwner}");

            systemGameManager.CharacterManager.CompleteNetworkModelRequest(unitController, gameObject, base.OwnerId == -1);
        }

        public override void OnStartClient() {
            //Debug.Log($"{gameObject.name}.FishNetCharacterModel.OnStartClient() owner: {base.OwnerId}");

            base.OnStartClient();
            isClient = true;
            FindGameManager();
        }

        private void CompleteClientConfiguration() {
            //Debug.Log($"{gameObject.name}.FishNetCharacterModel.CompleteClientConfiguration() owner: {base.OwnerId}");

            if (unitController.CharacterConfigured == true) {
                CompleteModelRequest(base.IsOwner);
            } else {
                SubscribeToUnitConfigured();
            }
        }

        public override void OnStartServer() {
            //Debug.Log($"{gameObject.name}.FishNetCharacterModel.OnStartServer()");
            base.OnStartClient();
            isClient = false;
            FindGameManager();
        }

        public override void OnSpawnServer(NetworkConnection connection) {
            //Debug.Log($"{gameObject.name}.FishNetCharacterModel.OnSpawnServer() owner: {base.OwnerId}");

            base.OnSpawnServer(connection);

            HandleSpawnServerClient(connection);
        }

        [TargetRpc]
        private void HandleSpawnServerClient(NetworkConnection networkConnection) {
            //Debug.Log($"{gameObject.name}.FishNetCharacterModel.HandleSpawnServerClient() owner: {base.OwnerId}");
        }


        private void CompleteServerConfiguration() {
            //Debug.Log($"{gameObject.name}.FishNetCharacterModel.CompleteServerConfiguration() owner: {base.OwnerId}");

            if (unitController.CharacterConfigured == true) {
                CompleteModelRequest(base.OwnerId == -1);
            } else {
                SubscribeToUnitConfigured();
            }
        }

        private void SubscribeToUnitConfigured() {
            //Debug.Log($"{gameObject.name}.FishNetCharacterModel.SubscribeToUnitConfigured()");

            unitController.UnitEventController.OnCharacterConfigured += HandleCharacterConfigured;
        }

        private void HandleCharacterConfigured() {
            //Debug.Log($"{gameObject.name}.FishNetCharacterModel.HandleCharacterConfigured()");

            unitController.UnitEventController.OnCharacterConfigured -= HandleCharacterConfigured;
            if (systemGameManager.NetworkManagerServer.ServerModeActive == false) {
                CompleteModelRequest(base.IsOwner);
            } else {
                CompleteModelRequest(base.OwnerId == -1);
            }
        }

        /*
        [ServerRpc(RequireOwnership = false)]
        public void GetClientSaveData(NetworkConnection networkConnection = null) {
            //Debug.Log($"{gameObject.name}.NetworkCharacterUnit.GetClientSaveData()");

            PutClientSaveData(networkConnection, unitController.CharacterSaveManager.SaveData);
        }

        [TargetRpc]
        public void PutClientSaveData(NetworkConnection networkConnection, AnyRPGSaveData saveData) {
            CompleteModelRequest(base.IsOwner, saveData);
        }
        */


    }
}

