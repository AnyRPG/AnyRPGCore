using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AnyRPG {
    public class NetworkCharacterUnit : SpawnedNetworkObject {
        // This is an empty class that is just there to ensure the object gets moved for everyone

        [SyncVar]
        public string unitProfileName = string.Empty;

        [SyncVar]
        public int unitLevel;

        [SyncVar]
        public UnitControllerMode unitControllerMode = UnitControllerMode.Preview;

        [SyncVar(OnChange = nameof(HandleNameSync), ReadPermissions = ReadPermission.ExcludeOwner)]
        public string characterName = string.Empty;

        
        private UnitProfile unitProfile = null;
        private UnitController unitController = null;

        // game manager references
        SystemGameManager systemGameManager = null;

        private void Configure() {
            // call character manager with spawnRequestId to complete configuration
            systemGameManager = GameObject.FindObjectOfType<SystemGameManager>();
            unitController = GetComponent<UnitController>();
            //if (base.IsOwner && unitController != null) {
            //    unitController.UnitEventController.OnNameChange += HandleUnitNameChange;
            //}
        }

        private void HandleUnitNameChange(string characterName) {
            Debug.Log($"{gameObject.name}.NetworkCharacterUnit.HandleUnitNameChange({characterName})");

            HandleUnitNameChangeServer(characterName);
        }

        [ServerRpc]
        private void HandleUnitNameChangeServer(string characterName) {
            Debug.Log($"{gameObject.name}.NetworkCharacterUnit.HandleUnitNameChangeServer({characterName})");

            this.characterName = characterName;
        }

        private void HandleNameSync(string oldValue, string newValue, bool asServer) {
            Debug.Log($"{gameObject.name}.NetworkCharacterUnit.HandleNameSync({oldValue}, {newValue}, {asServer})");

            unitController.BaseCharacter.ChangeCharacterName(newValue);
        }

        private void CompleteCharacterRequest(bool isOwner) {
            //Debug.Log($"{gameObject.name}.NetworkCharacterUnit.CompleteCharacterRequest({isOwner})");

            unitProfile = systemGameManager.SystemDataFactory.GetResource<UnitProfile>(unitProfileName);
            CharacterConfigurationRequest characterConfigurationRequest;
            if (isOwner && systemGameManager.CharacterManager.HasUnitSpawnRequest(clientSpawnRequestId)) {
                systemGameManager.CharacterManager.CompleteCharacterRequest(gameObject, clientSpawnRequestId, isOwner);
            } else {
                characterConfigurationRequest = new CharacterConfigurationRequest(unitProfile);
                characterConfigurationRequest.characterName = characterName;
                characterConfigurationRequest.unitLevel = unitLevel;
                characterConfigurationRequest.unitControllerMode = unitControllerMode;
                CharacterRequestData characterRequestData = new CharacterRequestData(null, GameMode.Network, characterConfigurationRequest);
                characterRequestData.spawnRequestId = clientSpawnRequestId;
                systemGameManager.CharacterManager.CompleteCharacterRequest(gameObject, characterRequestData, isOwner);
            }

            if (base.IsOwner && unitController != null) {
                unitController.UnitEventController.OnNameChange += HandleUnitNameChange;

                // OnNameChange is not called during initialization, so we have to pass the proper name to the network manually
                HandleUnitNameChange(unitController.BaseCharacter.CharacterName);
            }
        }

        public override void OnStopClient() {
            base.OnStopClient();
            Debug.Log($"{gameObject.name}.NetworkCharacterUnit.OnStopClient()");
            systemGameManager.NetworkManager.ProcessStopClient(unitController);
        }

        public override void OnStartClient() {
            base.OnStartClient();
            //Debug.Log($"{gameObject.name}.NetworkCharacterUnit.OnStartClient()");

            Configure();
            if (systemGameManager == null) {
                return;
            }
            CompleteCharacterRequest(base.IsOwner);
            //systemGameManager.CharacterManager.CompleteCharacterRequest(gameObject, spawnRequestId, base.isOwner);
        }

        public override void OnStartServer() {
            base.OnStartServer();
            Debug.Log($"{gameObject.name}.NetworkCharacterUnit.OnStartServer()");

            Configure();
            if (systemGameManager == null) {
                return;
            }
            CompleteCharacterRequest(false);
            //systemGameManager.CharacterManager.CompleteCharacterRequest(gameObject, serverRequestId, false);
        }

        void OnDisable() {
            Debug.Log($"{gameObject.name}.NetworkCharacterUnit.OnDisable()");
        }

    }
}

