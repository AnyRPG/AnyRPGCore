using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AnyRPG {
    public class CharacterManager : ConfiguredClass {

        public event Action<UnitController> OnCompleteUnitControllerInit = delegate { };

        // keep track of which request spawned something
        //private int clientSpawnRequestIdCounter;
        //private int serverSpawnRequestId;

        private List<UnitController> localUnits = new List<UnitController>();
        private List<UnitController> networkUnownedUnits = new List<UnitController>();
        private List<UnitController> networkOwnedUnits = new List<UnitController>();
        private List<UnitController> serverOwnedUnits = new List<UnitController>();

        private Dictionary<UnitControllerMode, Dictionary<int, UnitController>> unitControllerIdLookup = new Dictionary<UnitControllerMode, Dictionary<int, UnitController>>();
        private Dictionary<int, UnitController> playerIdLookup = new Dictionary<int, UnitController>();
        private Dictionary<int, UnitController> aiIdLookup = new Dictionary<int, UnitController>();
        private Dictionary<int, UnitController> petIdLookup = new Dictionary<int, UnitController>();
        private Dictionary<int, UnitController> mountIdLookup = new Dictionary<int, UnitController>();
        private Dictionary<int, UnitController> previewIdLookup = new Dictionary<int, UnitController>();
        private Dictionary<int, UnitController> inanimateIdLookup = new Dictionary<int, UnitController>();

        private Dictionary<UnitControllerMode, int> characterIdCounters = new Dictionary<UnitControllerMode, int>();
        /*
        private int aiIdCounter = 1;
        private int petIdCounter = 1;
        private int mountIdCounter = 1;
        private int previewIdCounter = 1;
        private int inanimateIdCounter = 1;
        */

        // keep track of spawn requests so that they can be configured after spawning
        //private Dictionary<int, CharacterRequestData> unitSpawnRequests = new Dictionary<int, CharacterRequestData>();
        //private Dictionary<UnitController, CharacterRequestData> modelSpawnRequests = new Dictionary<UnitController, CharacterRequestData>();

        // game manager references
        private ObjectPooler objectPooler = null;
        private PlayerManager playerManager = null;
        private PlayerManagerServer playerManagerServer = null;

        public List<UnitController> LocalUnits { get => localUnits; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            unitControllerIdLookup[UnitControllerMode.Player] = playerIdLookup;
            unitControllerIdLookup[UnitControllerMode.AI] = aiIdLookup;
            unitControllerIdLookup[UnitControllerMode.Pet] = petIdLookup;
            unitControllerIdLookup[UnitControllerMode.Preview] = previewIdLookup;
            unitControllerIdLookup[UnitControllerMode.Mount] = mountIdLookup;
            unitControllerIdLookup[UnitControllerMode.Inanimate] = inanimateIdLookup;

            characterIdCounters[UnitControllerMode.Player] = 1;
            characterIdCounters[UnitControllerMode.AI] = 1;
            characterIdCounters[UnitControllerMode.Pet] = 1;
            characterIdCounters[UnitControllerMode.Mount] = 1;
            characterIdCounters[UnitControllerMode.Preview] = 1;
            characterIdCounters[UnitControllerMode.Inanimate] = 1;
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            objectPooler = systemGameManager.ObjectPooler;
            playerManager = systemGameManager.PlayerManager;
            playerManagerServer = systemGameManager.PlayerManagerServer;
        }

        public int GetNewCharacterId(UnitControllerMode unitControllerMode) {
            int returnValue = -1;
            if (characterIdCounters.ContainsKey(unitControllerMode)) {
                returnValue = characterIdCounters[unitControllerMode];
                characterIdCounters[unitControllerMode]++;
            }
            return returnValue;
        }

        /*
        private void SetupUnitSpawnRequest(CharacterRequestData characterRequestData) {
            //Debug.Log($"CharacterManager.SetupUnitSpawnRequest({characterRequestData.characterConfigurationRequest.unitProfile.resourceName})");

            characterRequestData.clientSpawnRequestId = GetClientSpawnRequestId();
            characterRequestData.serverSpawnRequestId = characterRequestData.clientSpawnRequestId;
            AddUnitSpawnRequest(characterRequestData.clientSpawnRequestId, characterRequestData);
        }
        */

        /*
        public void RequestSpawnLobbyGamePlayer(int gameId, CharacterRequestData characterRequestData) {
            //Debug.Log($"CharacterManager.SpawnLobbyGamePlayer({gameId}, {position}, {forward})");

            SetupUnitSpawnRequest(characterRequestData);

            networkManagerClient.RequestSpawnLobbyGamePlayer(gameId, characterRequestData, SceneManager.GetActiveScene().name);
        }
        */

        // on the network
        public UnitController SpawnUnitPrefab(CharacterRequestData characterRequestData, Transform parentTransform, Vector3 position, Vector3 forward, Scene scene) {
            //Debug.Log($"CharacterManager.SpawnUnitPrefab({characterRequestData.characterConfigurationRequest.unitProfile.ResourceName}, {position}, {forward}, {scene.name})");

            return networkManagerServer.SpawnCharacterPrefab(characterRequestData, parentTransform, position, forward, scene);
        }

        // locally
        public UnitController SpawnUnitPrefabLocal(CharacterRequestData characterRequestData, Transform parentTransform, Vector3 position, Vector3 forward) {
            //Debug.Log($"CharacterManager.SpawnUnitPrefab({characterRequestData.characterConfigurationRequest.unitProfile.ResourceName}, {position}, {forward})");

            return SpawnCharacterPrefab(characterRequestData, parentTransform, position, forward);
        }


        public void ProcessStopNetworkUnit(UnitController unitController) {
            //Debug.Log($"CharacterManager.ProcessStopNetworkUnit({unitController.gameObject.name})");

            if (unitController.IsOwner == true && networkOwnedUnits.Contains(unitController)) {
                //HandleNetworkOwnedUnitDespawn(unitController);
                unitController.Despawn(0f, false, true);
                return;
            }
            if (unitController.IsOwner == false && networkUnownedUnits.Contains(unitController)) {
                //HandleNetworkUnownedUnitDespawn(unitController);
                unitController.Despawn(0f, false, true);
                return;
            }

            if (networkManagerServer.ServerModeActive == true) {
                return;
            }


            // add default case because network disconnect could happen before initialization is completed
            unitController.Despawn(0f, false, true);

        }

        /*
        public void CompleteCharacterRequest(GameObject characterGameObject, int spawnRequestId, bool isOwner) {
            //Debug.Log($"CharacterManager.CompleteCharacterRequest({characterGameObject.name}, {spawnRequestId}, {isOwner})");

            if (unitSpawnRequests.ContainsKey(spawnRequestId) == true) {
                CompleteCharacterRequest(characterGameObject, unitSpawnRequests[spawnRequestId], isOwner);
            }
        }
        */

        public void BeginCharacterRequest(UnitController unitController) {
            ConfigureUnitController(unitController);
        }

        public void CompleteNetworkCharacterRequest(UnitController unitController) {
            //Debug.Log($"CharacterManager.CompleteNetworkCharacterRequest({unitController.gameObject.name})");

            SetUnitControllerConfiguration(unitController);
        }

        public void CompleteCharacterRequest(UnitController unitController) {
            //Debug.Log($"CharacterManager.CompleteCharacterRequest({unitController.gameObject.name})");

            SetUnitControllerConfiguration(unitController);

            if (unitController.CharacterRequestData.requestMode == GameMode.Network) {
                // if this is being spawned over the network, the model is not spawned yet, so return and wait for it to spawn
                return;
            }

            CompleteModelRequest(unitController, false);
        }

        public void CompleteNetworkModelRequest(UnitController unitController, GameObject unitModel, bool isServerOwner) {
            //Debug.Log($"CharacterManager.CompleteNetworkModelRequest({clientSpawnRequestId}, {serverSpawnRequestId}, {unitController.gameObject.name}, {isOwner}, {isServerOwner})");

            unitController.UnitModelController.SetUnitModel(unitModel);
            CompleteModelRequest(unitController, isServerOwner);
        }

        public void CompleteModelRequest(UnitController unitController, bool isServerOwner) {
            //Debug.Log($"CharacterManager.CompleteModelRequest({unitController.gameObject.name}, {isServerOwner})");

            unitController.UnitModelController.SetInitialSavedAppearance();

            if (unitController.CharacterRequestData.characterRequestor != null) {
                //Debug.Log($"CharacterManager.CompleteModelRequest({characterRequestData.spawnRequestId}, {isOwner}) unitSpawnRequests contains the key");
                unitController.CharacterRequestData.characterRequestor.ConfigureSpawnedCharacter(unitController);
            }

            unitController.Init();

            if (unitController.CharacterRequestData.characterRequestor != null) {
                unitController.CharacterRequestData.characterRequestor.PostInit(unitController);
                //Debug.Log($"CharacterManager.CompleteModelRequest({characterRequestData.spawnRequestId}, {isOwner}) removing character request id {characterRequestData.spawnRequestId}");
                //unitSpawnRequests.Remove(usedSpawnRequestId);
            }
            OnCompleteUnitControllerInit(unitController);
        }

        public void ConfigureUnitController(UnitController unitController) {
            unitController.Configure(systemGameManager);
        }

        public UnitController GetUnitController(UnitControllerMode unitControllerMode, int characterId) {
            //Debug.Log($"CharacterManager.GetUnitController({unitControllerMode}, {characterId})");
            if (unitControllerIdLookup.ContainsKey(unitControllerMode) == true) {
                if (unitControllerIdLookup[unitControllerMode].ContainsKey(characterId) == true) {
                    return unitControllerIdLookup[unitControllerMode][characterId];
                }
            }
            return null;
        }

        public UnitController SetUnitControllerConfiguration(UnitController unitController) {
            //Debug.Log($"CharacterManager.ConfigureUnitController({unitController.gameObject.name})");

            if (unitController != null) {
                //Debug.Log($"CharacterManager.ConfigureUnitController({prefabObject.name}) adding {unitController.gameObject.name} to modelSpawnRequests");
                //modelSpawnRequests.Add(unitController, characterRequestData);

                // give this unit a unique name
                //Debug.Log($"CharacterManager.ConfigureUnitController({unitProfile.ResourceName}, {prefabObject.name}) renaming gameobject from {unitController.gameObject.name}");
                unitController.gameObject.name = unitController.CharacterRequestData.characterConfigurationRequest.unitProfile.ResourceName.Replace(" ", "") + systemGameManager.GetSpawnCount();
                //ConfigureUnitController(unitController);

                // add to lookup dictionaries
                if (unitControllerIdLookup.ContainsKey(unitController.CharacterRequestData.characterConfigurationRequest.unitControllerMode) == true) {
                    if (unitControllerIdLookup[unitController.CharacterRequestData.characterConfigurationRequest.unitControllerMode].ContainsKey(unitController.CharacterId)) {
                        unitControllerIdLookup[unitController.CharacterRequestData.characterConfigurationRequest.unitControllerMode][unitController.CharacterId] = unitController;
                    } else {
                        unitControllerIdLookup[unitController.CharacterRequestData.characterConfigurationRequest.unitControllerMode].Add(unitController.CharacterId, unitController);
                    }
                }

                if (unitController.CharacterRequestData.requestMode == GameMode.Local) {
                    //Debug.Log($"adding {unitController.gameObject.name} to local owned units");
                    localUnits.Add(unitController);
                    SubscribeToLocalOwnedUnitsEvents(unitController);
                } else {
                    if (unitController.CharacterRequestData.isOwner) {
                        networkOwnedUnits.Add(unitController);
                        unitController.UnitEventController.OnDespawn += HandleNetworkOwnedUnitDespawn;
                    } else if (unitController.CharacterRequestData.isServerOwned) {
                        //Debug.Log($"adding {unitController.gameObject.name} to server owned units");
                        serverOwnedUnits.Add(unitController);
                        unitController.UnitEventController.OnDespawn += HandleServerOwnedUnitDespawn;
                    } else {
                        networkUnownedUnits.Add(unitController);
                        unitController.UnitEventController.OnDespawn += HandleNetworkUnownedUnitDespawn;
                    }
                }

                if (unitController.CharacterRequestData.characterConfigurationRequest.unitControllerMode == UnitControllerMode.Player) {
                    if (unitController.CharacterRequestData.isOwner) {
                        // this is only true on the client
                        playerManager.SetUnitController(unitController);
                        playerManagerServer.AddActivePlayer(networkManagerClient.AccountId, unitController);
                        playerManagerServer.MonitorPlayer(unitController);
                    } else if (networkManagerServer.ServerModeActive) {
                        playerManagerServer.MonitorPlayer(unitController);
                    }
                }
                unitController.SetCharacterConfiguration();

            }

            return unitController;
        }

        private void SubscribeToLocalOwnedUnitsEvents(UnitController unitController) {
            
            unitController.UnitEventController.OnDespawn += HandleLocalUnitDespawn;
            unitController.UnitEventController.OnAfterDie += HandleAfterDie;
        }

        public void HandleAfterDie(CharacterStats deadCharacterStats) {
            if (playerManager.UnitController == null || deadCharacterStats.UnitController.GetCurrentInteractables(playerManager.UnitController).Count == 0) {
                deadCharacterStats.UnitController.OutlineController.TurnOffOutline();
            }
        }

        private void HandleLocalUnitDespawn(UnitController unitController) {
            unitController.UnitEventController.OnDespawn -= HandleLocalUnitDespawn;
            unitController.UnitEventController.OnAfterDie -= HandleAfterDie;
            localUnits.Remove(unitController);
            RemoveUnitControllerFromLookups(unitController);
        }

        private void HandleNetworkOwnedUnitDespawn(UnitController unitController) {
            unitController.UnitEventController.OnDespawn -= HandleNetworkOwnedUnitDespawn;
            networkOwnedUnits.Remove(unitController);
            RemoveUnitControllerFromLookups(unitController);
        }

        private void HandleNetworkUnownedUnitDespawn(UnitController unitController) {
            unitController.UnitEventController.OnDespawn -= HandleNetworkUnownedUnitDespawn;
            networkUnownedUnits.Remove(unitController);
            RemoveUnitControllerFromLookups(unitController);
        }

        private void HandleServerOwnedUnitDespawn(UnitController unitController) {
            unitController.UnitEventController.OnDespawn -= HandleServerOwnedUnitDespawn;
            serverOwnedUnits.Remove(unitController);
            RemoveUnitControllerFromLookups(unitController);
        }

        private void RemoveUnitControllerFromLookups(UnitController unitController) {
            if (unitControllerIdLookup.ContainsKey(unitController.CharacterRequestData.characterConfigurationRequest.unitControllerMode) == true) {
                if (unitControllerIdLookup[unitController.CharacterRequestData.characterConfigurationRequest.unitControllerMode].ContainsKey(unitController.CharacterId)) {
                    unitControllerIdLookup[unitController.CharacterRequestData.characterConfigurationRequest.unitControllerMode].Remove(unitController.CharacterId);
                }
            }
        }

        private GameObject LocalSpawnPrefab(GameObject spawnPrefab, Transform parentTransform, Vector3 position, Vector3 forward) {
            //Debug.Log($"CharacterManager.LocalSpawnPrefab({spawnPrefab.name}, {position}, {forward})");

            if (spawnPrefab == null) {
                return null;
            }

            GameObject prefabObject = objectPooler.GetPooledObject(spawnPrefab, position, (forward == Vector3.zero ? Quaternion.identity : Quaternion.LookRotation(forward)), parentTransform);

            return prefabObject;
        }

        public UnitController SpawnCharacterPrefab(CharacterRequestData characterRequestData, Transform parent, Vector3 position, Vector3 forward) {
            //Debug.Log($"CharacterManager.SpawnCharacterPrefab({characterRequestData.characterConfigurationRequest.unitProfile.ResourceName}, {position}, {forward})");

            GameObject prefabObject = LocalSpawnPrefab(characterRequestData.characterConfigurationRequest.unitProfile.UnitPrefabProps.UnitPrefab, parent, position, forward);
            UnitController unitController = null;
            if (characterRequestData.requestMode == GameMode.Local) {
                //Debug.Log($"CharacterManager.SpawnCharacterPrefab() spawning local unit");
                // this should always be true in this function because it's only called if not network mode
                unitController = prefabObject.GetComponent<UnitController>();
                unitController.SetCharacterRequestData(characterRequestData);
                BeginCharacterRequest(unitController);
                CompleteCharacterRequest(unitController);
            }
            return unitController;
        }

        /*
        public void AddUnitSpawnRequest(int spawnRequestId, CharacterRequestData characterRequestData) {
            //Debug.Log($"CharacterManager.AddUnitSpawnRequest({spawnRequestId}, {characterRequestData.characterConfigurationRequest.unitProfile.ResourceName})");

            unitSpawnRequests.Add(spawnRequestId, characterRequestData);
        }
        
        public bool HasUnitSpawnRequest(int spawnRequestId) {
            return unitSpawnRequests.ContainsKey(spawnRequestId);
        }
        */

        private GameObject SpawnModelPrefab(GameMode spawnMode, GameObject spawnPrefab, Transform parentTransform, Vector3 position, Vector3 forward) {
            //Debug.Log($"CharacterManager.SpawnModelPrefab({spawnMode}, {spawnPrefab.name}, {parentTransform.gameObject.name}, {position}, {forward})");

            if (spawnMode == GameMode.Network) {
                if (networkManagerServer.ServerModeActive == true) {
                    return networkManagerServer.SpawnModelPrefab(spawnPrefab, parentTransform, position, forward);
                } else {
                    return networkManagerClient.RequestSpawnModelPrefab(spawnPrefab, parentTransform, position, forward);
                }
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
            //Debug.Log($"CharacterManager.SpawnModelPrefab({unitController.gameObject.name}, {unitProfile.ResourceName}, {parentTransform.gameObject.name}, {position}, {forward})");

            if (networkUnownedUnits.Contains(unitController)) {
                //Debug.Log($"CharacterManager.SpawnModelPrefab() network unowned unit");
                return null;
            }

            if (localUnits.Contains(unitController)) {
                return SpawnModelPrefab(GameMode.Local, unitProfile.UnitPrefabProps.ModelPrefab, parentTransform, position, forward);
            }

            if (networkOwnedUnits.Contains(unitController)) {
                return SpawnModelPrefab(GameMode.Network, unitProfile.UnitPrefabProps.NetworkModelPrefab, parentTransform, position, forward);
            }

            if (serverOwnedUnits.Contains(unitController)) {
                return SpawnModelPrefab(GameMode.Network, unitProfile.UnitPrefabProps.NetworkModelPrefab, parentTransform, position, forward);
            }

            return null;
        }

        public void PoolUnitController(UnitController unitController) {
            //Debug.Log($"CharacterManager.PoolUnitController({unitController.gameObject.name})");

            if (localUnits.Contains(unitController)) {
                objectPooler.ReturnObjectToPool(unitController.gameObject);
                return;
            }
            if (networkManagerServer.ServerModeActive == true) {
                // this is happening on the server, return the object to the pool
                // disabled because crashing.  On server, network objects are automatically despawned when level unloads
                //unitController.gameObject.SetActive(false);
                if (unitController.IsDisconnected == false) {
                    // if we manually called the despawn method, we want to return it to the pool
                    // if a disconnect happened, we'll get an error on clients if we despawn it now because the network manager will try to despawn it next
                    networkManagerServer.ReturnObjectToPool(unitController.gameObject);
                }
            } else {
                // this is happening on the client
                if (localUnits.Contains(unitController)) {
                    // this unit was requested in a local game, pool it
                    objectPooler.ReturnObjectToPool(unitController.gameObject);
                } else {
                    // this unit was requested in a network game, deactivate it and let it wait for the network pooler to claim it
                    unitController.gameObject.SetActive(false);
                }
            }
        }

        public string GetCharacterName(int characterId) {
            foreach (UnitControllerMode unitControllerMode in unitControllerIdLookup.Keys) {
                if (unitControllerIdLookup[unitControllerMode].ContainsKey(characterId)) {
                    return unitControllerIdLookup[unitControllerMode][characterId].DisplayName;
                }
            }
            return "Unknown";
        }

        /*
        public void AddServerSpawnRequestId(int clientSpawnRequestId, int serverSpawnRequestId) {
            //Debug.Log($"CharacterManager.AddServerSpawnRequestId({clientSpawnRequestId}, {serverSpawnRequestId})");
            if (unitSpawnRequests.ContainsKey(clientSpawnRequestId) == true) {
                unitSpawnRequests[clientSpawnRequestId].serverSpawnRequestId = serverSpawnRequestId;
            } else {
                //Debug.LogError($"CharacterManager.AddServerSpawnRequestId() client spawn request id {clientSpawnRequestId} not found in unit spawn requests");
            }
        }
        */

    }

}