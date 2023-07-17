using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AnyRPG {
    public class CharacterManager : ConfiguredMonoBehaviour {

        private List<UnitController> localUnits = new List<UnitController>();
        private List<UnitController> networkUnits = new List<UnitController>();

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

        /// <summary>
        /// spawn unit with parent. rotation and position from settings
        /// </summary>
        /// <param name="parentTransform"></param>
        /// <param name="settingsTransform"></param>
        /// <returns></returns>
        public UnitController SpawnUnitPrefab(GameMode spawnMode, UnitProfile unitProfile, Transform parentTransform, Vector3 position, Vector3 forward, UnitControllerMode unitControllerMode, int unitLevel = -1) {
            Debug.Log($"CharacterManager.SpawnUnitPrefab({spawnMode}, {unitProfile.ResourceName})");

            GameObject spawnPrefab;
            if (spawnMode == GameMode.Local) {
                spawnPrefab = unitProfile.UnitPrefabProps.UnitPrefab;
            } else {
                spawnPrefab = unitProfile.UnitPrefabProps.NetworkUnitPrefab;
            }
            GameObject prefabObject = SpawnCharacterPrefab(spawnMode, unitProfile, spawnPrefab, parentTransform, position, forward, unitControllerMode, unitLevel);
            UnitController unitController = null;
            if (spawnMode == GameMode.Local) {
                unitController = ConfigureUnitController(spawnMode, unitProfile, prefabObject, unitControllerMode, unitLevel);
            }

            return unitController;
        }

        public UnitController ConfigureUnitController(GameMode spawnMode, UnitProfile unitProfile, GameObject prefabObject, UnitControllerMode unitControllerMode, int unitLevel) {
            Debug.Log("CharacterManager.ConfigureUnitController()");

            UnitController unitController = null;

            if (prefabObject != null) {
                unitController = prefabObject.GetComponent<UnitController>();
                if (unitController != null) {

                    if (spawnMode == GameMode.Local) {
                        localUnits.Add(unitController);
                    } else {
                        networkUnits.Add(unitController);
                    }

                    // give this unit a unique name
                    unitController.gameObject.name = unitProfile.ResourceName.Replace(" ", "") + systemGameManager.GetSpawnCount();
                    unitController.Configure(systemGameManager);
                    // test - set unitprofile first so we don't overwrite players baseCharacter settings
                    unitController.SetUnitProfile(unitProfile, unitControllerMode, unitLevel);

                }
            }

            return unitController;
        }

        private GameObject LocalSpawnPrefab(GameObject spawnPrefab, Transform parentTransform, Vector3 position, Vector3 forward) {
            Debug.Log("CharacterManager.LocalSpawnPrefab()");

            if (spawnPrefab == null) {
                return null;
            }

            GameObject prefabObject = objectPooler.GetPooledObject(spawnPrefab, position, (forward == Vector3.zero ? Quaternion.identity : Quaternion.LookRotation(forward)), parentTransform);

            return prefabObject;
        }

        private GameObject SpawnCharacterPrefab(GameMode spawnMode, UnitProfile unitProfile, GameObject spawnPrefab, Transform parentTransform, Vector3 position, Vector3 forward, UnitControllerMode unitControllerMode, int unitLevel) {
            Debug.Log($"CharacterManager.SpawnPrefab({spawnMode}, {spawnPrefab.name})");

            if (spawnMode == GameMode.Network) {
                return networkManager.SpawnPlayer(unitProfile, spawnPrefab, parentTransform, position, forward, unitControllerMode, unitLevel);
            }
            return LocalSpawnPrefab(spawnPrefab, parentTransform, position, forward);
        }

        private GameObject SpawnModelPrefab(GameMode spawnMode, GameObject spawnPrefab, Transform parentTransform, Vector3 position, Vector3 forward) {
            Debug.Log($"CharacterManager.SpawnPrefab({spawnMode}, {spawnPrefab.name})");

            if (spawnMode == GameMode.Network) {
                return networkManager.SpawnPrefab(spawnPrefab, parentTransform, position, forward);
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
            Debug.Log($"CharacterManager.SpawnModelPrefab({unitController.gameObject.name}, {unitProfile.ResourceName})");

            if (localUnits.Contains(unitController)) {

                return SpawnModelPrefab(GameMode.Local, unitProfile.UnitPrefabProps.ModelPrefab, parentTransform, position, forward);
            }

            if (networkUnits.Contains(unitController)) {
                return SpawnModelPrefab(GameMode.Network, unitProfile.UnitPrefabProps.NetworkModelPrefab, parentTransform, position, forward);
            }

            return null;
        }
    }

}