using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AnyRPG {
    public class SpawnedNetworkObject : NetworkBehaviour {

        [SyncVar]
        public int spawnRequestId;

        public override void OnStartClient() {
            base.OnStartClient();
            //Debug.Log($"{gameObject.name}.SpawnedNetworkObject.OnStartClient()");
        }


        public override void OnStartServer() {
            base.OnStartClient();
            //Debug.Log($"{gameObject.name}.SpawnedNetworkObject.OnStartServer()");
        }
    }
}

