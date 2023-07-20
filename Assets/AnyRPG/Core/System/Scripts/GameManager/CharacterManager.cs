using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AnyRPG {
    public class CharacterManager : ConfiguredMonoBehaviour {

        // keep track of which request spawned something
        private int spawnRequestId;

        private List<UnitController> localUnits = new List<UnitController>();
        private List<UnitController> networkUnits = new List<UnitController>();

        // keep track of spawn requests so that they can be configured after spawning
        private Dictionary<int, CharacterRequestData> networkSpawnRequests = new Dictionary<int, CharacterRequestData>();
        private Dictionary<UnitController, CharacterRequestData> modelSpawnRequests = new Dictionary<UnitController, CharacterRequestData>();

        // game manager references
        private ObjectPooler objectPooler = null;
        private NetworkManager networkManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            objectPooler = systemGameManager.ObjectPooler;
            networkManager = systemGameManager.NetworkManager;
        }

        public int GetSpawnRequestId() {
            return spawnRequestId++;
        }

        /// <summary>
        /// spawn unit with parent. rotation and position from settings
        /// </summary>
        /// <param name="parentTransform"></param>
        /// <param name="settingsTransform"></param>
        /// <returns></returns>
        public UnitController SpawnUnitPrefab(CharacterRequestData characterRequestData, Transform parentTransform, Vector3 position, Vector3 forward) {
            //Debug.Log($"CharacterManager.SpawnUnitPrefab({spawnMode}, {unitProfile.ResourceName})");
            characterRequestData.spawnRequestId = GetSpawnRequestId();
            GameObject spawnPrefab;
            if (characterRequestData.requestMode == GameMode.Local) {
                spawnPrefab = characterRequestData.unitProfile.UnitPrefabProps.UnitPrefab;
            } else {
                spawnPrefab = characterRequestData.unitProfile.UnitPrefabProps.NetworkUnitPrefab;
            }
            GameObject prefabObject = SpawnCharacterPrefab(characterRequestData, spawnPrefab, parentTransform, position, forward);
            UnitController unitController = null;
            if (characterRequestData.requestMode == GameMode.Local) {
                unitController = ConfigureUnitController(characterRequestData, prefabObject);
            }

            return unitController;
        }

        public void CompleteCharacterRequest(GameObject gameObject, int characterRequestId, bool isOwner) {
            Debug.Log($"CharacterManager.CompleteCharacterRequest({gameObject.name}, {characterRequestId}, {isOwner})");

            if (networkSpawnRequests.ContainsKey(characterRequestId) == false) {
                Debug.Log($"CharacterManager.CompleteCharacterRequest({gameObject.name}, {characterRequestId}, {isOwner}) network spawn requests does not contain request id");
                return;
            }
            // gamemode is always network here because this call only happens when a network object is spawned
            UnitController unitController = ConfigureUnitController(networkSpawnRequests[characterRequestId], gameObject);
            if (unitController == null) {
                return;
            }

            // can't run ConfigureSpawnedCharacter before the model is spawned.
            // check if UnitProfile contains a network model.  If not, go ahead since the model is probably integrated with the unit
            // If a model is specified, then complete configuration when it spawns so that Init can properly find the animator
            if (networkSpawnRequests[characterRequestId].unitProfile.UnitPrefabProps.NetworkModelPrefab != null) {
                return;
            }
            CompleteModelRequest(characterRequestId, isOwner);
        }

        public void CompleteModelRequest(int characterRequestId, bool isOwner) {
            Debug.Log($"CharacterManager.CompleteModelRequest({characterRequestId}, {isOwner})");

            if (networkSpawnRequests.ContainsKey(characterRequestId) == false) {
                Debug.Log($"CharacterManager.CompleteModelRequest({characterRequestId}, {isOwner}) networkSpawnRequests does not contain key");
                return;
            }

            networkSpawnRequests[characterRequestId].characterRequestor.ConfigureSpawnedCharacter(networkSpawnRequests[characterRequestId].unitController, networkSpawnRequests[characterRequestId]);

            Debug.Log($"CharacterManager.CompleteModelRequest({characterRequestId}, {isOwner}) removing character request id {characterRequestId}");
            networkSpawnRequests.Remove(characterRequestId);
        }

        public UnitController ConfigureUnitController(CharacterRequestData characterRequestData, GameObject prefabObject) {
            Debug.Log($"CharacterManager.ConfigureUnitController({prefabObject.name})");

            UnitController unitController = null;

            if (prefabObject != null) {
                unitController = prefabObject.GetComponent<UnitController>();
                if (unitController != null) {
                    characterRequestData.unitController = unitController;
                    Debug.Log($"CharacterManager.ConfigureUnitController({prefabObject.name}) adding {unitController.gameObject.name} to modelSpawnRequests");
                    modelSpawnRequests.Add(unitController, characterRequestData);

                    if (characterRequestData.requestMode == GameMode.Local) {
                        localUnits.Add(unitController);
                    } else {
                        networkUnits.Add(unitController);
                    }

                    // give this unit a unique name
                    //Debug.Log($"CharacterManager.ConfigureUnitController({unitProfile.ResourceName}, {prefabObject.name}) renaming gameobject from {unitController.gameObject.name}");
                    unitController.gameObject.name = characterRequestData.unitProfile.ResourceName.Replace(" ", "") + systemGameManager.GetSpawnCount();
                    unitController.Configure(systemGameManager);
                    // test - set unitprofile first so we don't overwrite players baseCharacter settings
                    unitController.SetUnitProfile(characterRequestData.unitProfile, characterRequestData.unitControllerMode, characterRequestData.unitLevel);

                }
            }

            return unitController;
        }

        private GameObject LocalSpawnPrefab(GameObject spawnPrefab, Transform parentTransform, Vector3 position, Vector3 forward) {
            //Debug.Log("CharacterManager.LocalSpawnPrefab()");

            if (spawnPrefab == null) {
                return null;
            }

            GameObject prefabObject = objectPooler.GetPooledObject(spawnPrefab, position, (forward == Vector3.zero ? Quaternion.identity : Quaternion.LookRotation(forward)), parentTransform);

            return prefabObject;
        }

        private GameObject SpawnCharacterPrefab(CharacterRequestData characterRequestData, GameObject spawnPrefab, Transform parentTransform, Vector3 position, Vector3 forward) {
            Debug.Log($"CharacterManager.SpawnCharacterPrefab({spawnPrefab.name})");

            if (characterRequestData.requestMode == GameMode.Network) {
                //int spawnRequestId = GetSpawnRequestId();
                Debug.Log($"CharacterManager.SpawnCharacterPrefab({spawnPrefab.name}) adding {characterRequestData.spawnRequestId} to networkSpawnRequests");
                networkSpawnRequests.Add(characterRequestData.spawnRequestId, characterRequestData);
                networkManager.SpawnPlayer(characterRequestData, spawnPrefab, parentTransform, position, forward);
                return null;
            }
            return LocalSpawnPrefab(spawnPrefab, parentTransform, position, forward);
        }

        private GameObject SpawnModelPrefab(int spawnRequestId, GameMode spawnMode, GameObject spawnPrefab, Transform parentTransform, Vector3 position, Vector3 forward) {
            //Debug.Log($"CharacterManager.SpawnPrefab({spawnMode}, {spawnPrefab.name})");

            if (spawnMode == GameMode.Network) {
                return networkManager.SpawnModelPrefab(spawnRequestId, spawnPrefab, parentTransform, position, forward);
            }
            return LocalSpawnPrefab(spawnPrefab, parentTransform, position, forward);
        }


        /// <summary>
        /// spawn unit with parent. rotation and position from settings
        /// </summary>
        /// <param name="parentTransform"></param>
        /// <param name="settingsTransform"></param>
        /// <returns></returns>
        public GameObject SpawnModelPrefab(UnitController unitController, UnitProfile unitProfile, Transform parentTransform, Vector3 position, Vector3 forward) {
            //Debug.Log($"CharacterManager.SpawnModelPrefab({unitController.gameObject.name}, {unitProfile.ResourceName})");

            if (modelSpawnRequests.ContainsKey(unitController) == false) {
                return null;
            }
            int usedSpawnRequestId = modelSpawnRequests[unitController].spawnRequestId;
            Debug.Log($"CharacterManager.SpawnModelPrefab({unitController.gameObject.name}, {unitProfile.ResourceName}) removing unitController from modelSpawnRequests");
            modelSpawnRequests.Remove(unitController);

            if (localUnits.Contains(unitController)) {

                return SpawnModelPrefab(usedSpawnRequestId, GameMode.Local, unitProfile.UnitPrefabProps.ModelPrefab, parentTransform, position, forward);
            }

            if (networkUnits.Contains(unitController)) {
                return SpawnModelPrefab(usedSpawnRequestId, GameMode.Network, unitProfile.UnitPrefabProps.NetworkModelPrefab, parentTransform, position, forward);
            }

            return null;
        }
    }

}