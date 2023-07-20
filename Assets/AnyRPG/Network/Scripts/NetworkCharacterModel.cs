using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AnyRPG {
    public class NetworkCharacterModel : SpawnedNetworkObject {

        private void CompleteModelRequest() {
            // call character manager with spawnRequestId to complete configuration
            SystemGameManager systemGameManager = GameObject.FindObjectOfType<SystemGameManager>();
            if (systemGameManager == null) {
                return;
            }
            systemGameManager.CharacterManager.CompleteModelRequest(spawnRequestId, base.IsOwner);
        }

        public override void OnStartClient() {
            base.OnStartClient();
            //Debug.Log($"{gameObject.name}.NetworkCharacterModel.OnStartClient()");

            //CompleteModelRequest();
        }


        public override void OnStartServer() {
            base.OnStartClient();
            Debug.Log($"{gameObject.name}.NetworkCharacterModel.OnStartServer()");

            // doing this here because OnStartClient() does not get called if this is a client and a server
            CompleteModelRequest();
        }

    }
}

