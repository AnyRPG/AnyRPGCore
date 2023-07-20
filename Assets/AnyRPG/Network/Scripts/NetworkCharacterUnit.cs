using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AnyRPG {
    public class NetworkCharacterUnit : SpawnedNetworkObject {
        // This is an empty class that is just there to ensure the object gets moved for everyone

        private void CompleteCharacterRequest() {
            // call character manager with spawnRequestId to complete configuration
            SystemGameManager systemGameManager = GameObject.FindObjectOfType<SystemGameManager>();
            if (systemGameManager == null) {
                return;
            }
            systemGameManager.CharacterManager.CompleteCharacterRequest(gameObject, spawnRequestId, base.IsOwner);
        }

        public override void OnStartClient() {
            base.OnStartClient();
            Debug.Log($"{gameObject.name}.NetworkCharacterUnit.OnStartClient()");

            //CompleteCharacterRequest();
        }

        public override void OnStartServer() {
            base.OnStartServer();
            Debug.Log($"{gameObject.name}.NetworkCharacterUnit.OnStartServer()");
            
            // doing this here because OnStartClient() does not get called if this is a client and a server
            CompleteCharacterRequest();
        }

    }
}

