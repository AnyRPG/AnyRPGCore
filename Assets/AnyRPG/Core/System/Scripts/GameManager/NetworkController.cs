using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AnyRPG {

    /// <summary>
    /// meant to be inherited from by actual network implementations like fish-net, etc.
    /// </summary>
    public abstract class NetworkController : ConfiguredClass {
        
        public virtual bool Login(string username, string password, string server) {
            return false;
        }

        public abstract void Logout();

        public abstract GameObject SpawnPlayer(UnitProfile unitProfile, GameObject playerPrefab, Transform parentTransform, Vector3 position, Vector3 forward, UnitControllerMode unitControllerMode, int unitLevel);
        
        public abstract GameObject SpawnPrefab(GameObject prefab, Transform parentTransform, Vector3 position, Vector3 forward);

        public abstract void LoadScene(string sceneName);
    }

}