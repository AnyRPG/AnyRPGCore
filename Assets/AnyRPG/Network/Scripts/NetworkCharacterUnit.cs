using FishNet.Object;
using FishNet.Object.Synchronizing;
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

        UnitProfile unitProfile = null;

        // game manager references
        SystemGameManager systemGameManager = null;

        private void FindSystemGameManager() {
            // call character manager with spawnRequestId to complete configuration
            systemGameManager = GameObject.FindObjectOfType<SystemGameManager>();
        }

        private void CompleteCharacterRequest(bool isOwner) {
            unitProfile = systemGameManager.SystemDataFactory.GetResource<UnitProfile>(unitProfileName);
            CharacterRequestData characterRequestData = new CharacterRequestData(null, GameMode.Network, unitProfile, unitControllerMode, unitLevel);
            characterRequestData.spawnRequestId = clientSpawnRequestId;
            systemGameManager.CharacterManager.CompleteCharacterRequest(gameObject, characterRequestData, isOwner);
        }

        public override void OnStartClient() {
            base.OnStartClient();
            Debug.Log($"{gameObject.name}.NetworkCharacterUnit.OnStartClient()");

            FindSystemGameManager();
            if (systemGameManager == null) {
                return;
            }
            CompleteCharacterRequest(base.IsOwner);
            //systemGameManager.CharacterManager.CompleteCharacterRequest(gameObject, spawnRequestId, base.isOwner);
        }

        public override void OnStartServer() {
            base.OnStartServer();
            Debug.Log($"{gameObject.name}.NetworkCharacterUnit.OnStartServer()");

            FindSystemGameManager();
            if (systemGameManager == null) {
                return;
            }
            CompleteCharacterRequest(false);
            //systemGameManager.CharacterManager.CompleteCharacterRequest(gameObject, serverRequestId, false);
        }

    }
}

