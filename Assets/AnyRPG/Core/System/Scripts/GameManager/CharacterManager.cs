using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AnyRPG {
    public class CharacterManager : ConfiguredMonoBehaviour {

        // keep track of which request spawned something
        private int clientSpawnRequestId;
        private int serverSpawnRequestId;

        private List<UnitController> localUnits = new List<UnitController>();
        private List<UnitController> networkUnownedUnits = new List<UnitController>();
        private List<UnitController> networkOwnedUnits = new List<UnitController>();

        // keep track of spawn requests so that they can be configured after spawning
        private Dictionary<int, CharacterRequestData> unitSpawnRequests = new Dictionary<int, CharacterRequestData>();
        private Dictionary<UnitController, CharacterRequestData> modelSpawnRequests = new Dictionary<UnitController, CharacterRequestData>();

        // game manager references
        private ObjectPooler objectPooler = null;
        private NetworkManagerClient networkManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            objectPooler = systemGameManager.ObjectPooler;
            networkManager = systemGameManager.NetworkManagerClient;
        }

        public int GetSpawnRequestId() {
            return clientSpawnRequestId++;
        }

        public int GetServerSpawnRequestId() {
            return serverSpawnRequestId++;
        }

        private void SetupUnitSpawnRequest(CharacterRequestData characterRequestData) {
            characterRequestData.spawnRequestId = GetSpawnRequestId();
            AddUnitSpawnRequest(characterRequestData.spawnRequestId, characterRequestData);

        }

        public void SpawnPlayer(PlayerCharacterSaveData playerCharacterSaveData, CharacterRequestData characterRequestData, Transform parentTransform, Vector3 position, Vector3 forward) {

            SetupUnitSpawnRequest(characterRequestData);

            if (systemGameManager.GameMode == GameMode.Network) {
                networkManager.SpawnPlayer(playerCharacterSaveData.PlayerCharacterId, characterRequestData, parentTransform);
                return;
            }

            SpawnCharacterPrefab(characterRequestData, parentTransform, position, forward);
        }

        public UnitController SpawnUnitPrefab(CharacterRequestData characterRequestData, Transform parentTransform, Vector3 position, Vector3 forward) {
            //Debug.Log($"CharacterManager.SpawnUnitPrefab({spawnMode}, {unitProfile.ResourceName})");

            SetupUnitSpawnRequest(characterRequestData);

            /*
            if (characterRequestData.requestMode == GameMode.Network) {
                networkManager.SpawnPlayer(characterRequestData, parentTransform, position, forward);
                return null;
            }
            */

            return SpawnCharacterPrefab(characterRequestData, parentTransform, position, forward);
        }


        public void ProcessStopClient(UnitController unitController) {
            Debug.Log($"CharacterManager.ProcessStopClient({unitController.gameObject.name})");
            networkOwnedUnits.Remove(unitController);
            networkUnownedUnits.Remove(unitController);
            unitController.Despawn(0f, false, true);
        }

        public void CompleteCharacterRequest(GameObject characterGameObject, int spawnRequestId, bool isOwner) {
            if (isOwner && unitSpawnRequests.ContainsKey(spawnRequestId) == true) {
                CompleteCharacterRequest(characterGameObject, unitSpawnRequests[spawnRequestId], isOwner);
            }
        }

        public UnitController CompleteCharacterRequest(GameObject characterGameObject, CharacterRequestData characterRequestData, bool isOwner) {
            //Debug.Log($"CharacterManager.CompleteCharacterRequest({gameObject.name}, {isOwner})");

            UnitController unitController = ConfigureUnitController(characterRequestData, characterGameObject, isOwner);
            if (unitController == null) {
                return null;
            }

            if (characterRequestData.requestMode == GameMode.Network) {
                // if this is being spawned over the network, the model is not spawned yet, so return and wait for it to spawn
                return null;
            }

            CompleteModelRequest(characterRequestData.spawnRequestId, unitController, isOwner);

            return unitController;
        }

        public void CompleteNetworkModelRequest(int spawnRequestId, UnitController unitController, GameObject unitModel, bool isOwner) {
            unitController.UnitModelController.SetUnitModel(unitModel);
            CompleteModelRequest(spawnRequestId, unitController, isOwner);
        }

        public void CompleteModelRequest(int spawnRequestId, UnitController unitController, bool isOwner) {
            //Debug.Log($"CharacterManager.CompleteModelRequest({unitController.gameObject.name}, {isOwner})");

            CharacterRequestData characterRequestData;
            if (isOwner && unitSpawnRequests.ContainsKey(spawnRequestId)) {
                characterRequestData = unitSpawnRequests[spawnRequestId];
            } else {
                CharacterConfigurationRequest characterConfigurationRequest = new CharacterConfigurationRequest(unitController.UnitProfile);
                characterConfigurationRequest.unitControllerMode = UnitControllerMode.Preview; // does not matter since it's unused to the CompleteModelRequest() process
                characterRequestData = new CharacterRequestData(null,
                                GameMode.Network,
                                characterConfigurationRequest);
                characterRequestData.spawnRequestId = clientSpawnRequestId;
            }

            unitController.UnitModelController.SetInitialSavedAppearance();

            if (unitSpawnRequests.ContainsKey(spawnRequestId) == true && isOwner) {
                //Debug.Log($"CharacterManager.CompleteModelRequest({characterRequestData.spawnRequestId}, {isOwner}) unitSpawnRequests contains the key");
                characterRequestData.characterRequestor.ConfigureSpawnedCharacter(unitController, characterRequestData);
            }

            unitController.Init();

            if (unitSpawnRequests.ContainsKey(spawnRequestId) == true && isOwner) {
                characterRequestData.characterRequestor.PostInit(unitController, characterRequestData);
                //Debug.Log($"CharacterManager.CompleteModelRequest({characterRequestData.spawnRequestId}, {isOwner}) removing character request id {characterRequestData.spawnRequestId}");
                unitSpawnRequests.Remove(spawnRequestId);
            }

        }

        public UnitController ConfigureUnitController(CharacterRequestData characterRequestData, GameObject prefabObject, bool isOwner) {
            //Debug.Log($"CharacterManager.ConfigureUnitController({prefabObject.name})");

            UnitController unitController = null;

            if (prefabObject != null) {
                unitController = prefabObject.GetComponent<UnitController>();
                if (unitController != null) {
                    characterRequestData.unitController = unitController;
                    //Debug.Log($"CharacterManager.ConfigureUnitController({prefabObject.name}) adding {unitController.gameObject.name} to modelSpawnRequests");
                    modelSpawnRequests.Add(unitController, characterRequestData);

                    if (characterRequestData.requestMode == GameMode.Local) {
                        localUnits.Add(unitController);
                    } else {
                        if (isOwner) {
                            networkOwnedUnits.Add(unitController);
                        } else {
                            networkUnownedUnits.Add(unitController);
                        }
                    }

                    // give this unit a unique name
                    //Debug.Log($"CharacterManager.ConfigureUnitController({unitProfile.ResourceName}, {prefabObject.name}) renaming gameobject from {unitController.gameObject.name}");
                    unitController.gameObject.name = characterRequestData.characterConfigurationRequest.unitProfile.ResourceName.Replace(" ", "") + systemGameManager.GetSpawnCount();
                    unitController.Configure(systemGameManager);
                    unitController.SetCharacterConfiguration(characterRequestData.characterConfigurationRequest);

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

        private UnitController SpawnCharacterPrefab(CharacterRequestData characterRequestData, Transform parentTransform, Vector3 position, Vector3 forward) {
            //Debug.Log($"CharacterManager.SpawnCharacterPrefab({spawnPrefab.name})");

            GameObject prefabObject = LocalSpawnPrefab(characterRequestData.characterConfigurationRequest.unitProfile.UnitPrefabProps.UnitPrefab, parentTransform, position, forward);
            UnitController unitController = null;
            if (characterRequestData.requestMode == GameMode.Local) {
                // this should always be true in this function because it's only called if not network mode
                unitController = CompleteCharacterRequest(prefabObject, characterRequestData, true);
            }
            return unitController;
        }

        public void AddUnitSpawnRequest(int spawnRequestId, CharacterRequestData characterRequestData) {
            unitSpawnRequests.Add(spawnRequestId, characterRequestData);
        }
        
        public bool HasUnitSpawnRequest(int spawnRequestId) {
            return unitSpawnRequests.ContainsKey(spawnRequestId);
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

            if (networkUnownedUnits.Contains(unitController)) {
                return null;
            }

            int usedSpawnRequestId = modelSpawnRequests[unitController].spawnRequestId;
            //Debug.Log($"CharacterManager.SpawnModelPrefab({unitController.gameObject.name}, {unitProfile.ResourceName}) removing unitController from modelSpawnRequests");
            modelSpawnRequests.Remove(unitController);

            if (localUnits.Contains(unitController)) {

                return SpawnModelPrefab(usedSpawnRequestId, GameMode.Local, unitProfile.UnitPrefabProps.ModelPrefab, parentTransform, position, forward);
            }

            if (networkOwnedUnits.Contains(unitController)) {
                return SpawnModelPrefab(usedSpawnRequestId, GameMode.Network, unitProfile.UnitPrefabProps.NetworkModelPrefab, parentTransform, position, forward);
            }

            return null;
        }

        public void PoolUnitController(UnitController unitController) {
            if (localUnits.Contains(unitController)) {
                objectPooler.ReturnObjectToPool(unitController.gameObject);
                return;
            }
            // in network case do nothing because the network manager is handling the pooling
        }
    }

}