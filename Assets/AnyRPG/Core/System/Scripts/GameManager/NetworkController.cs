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
    public abstract class NetworkController : ConfiguredMonoBehaviour {
        
        public virtual bool Login(string username, string password, string server) {
            return false;
        }
        public abstract void Logout();
        public abstract void SpawnPlayer(int playerCharacterId, CharacterRequestData characterRequestData, Transform parentTransform);
        public abstract GameObject SpawnModelPrefab(int spawnRequestId, GameObject prefab, Transform parentTransform, Vector3 position, Vector3 forward);
        public abstract void LoadScene(string sceneName);
        public abstract bool CanSpawnCharacterOverNetwork();
        public abstract bool OwnPlayer(UnitController unitController);
        public abstract void CreatePlayerCharacter(AnyRPGSaveData anyRPGSaveData);
        public abstract void DeletePlayerCharacter(int playerCharacterId);
        public abstract void LoadCharacterList();
    }

}