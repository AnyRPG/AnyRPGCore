using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class PlayerCharacterMonitor : ConfiguredClass {

        public int clientId;
        public PlayerCharacterSaveData playerCharacterSaveData;
        public UnitController unitController;
        public bool saveDataDirty;

        // game manager references
        private SaveManager saveManager = null;
        private NetworkManagerServer networkManagerServer = null;

        public PlayerCharacterMonitor(SystemGameManager systemGameManager, int clientId, PlayerCharacterSaveData playerCharacterSaveData, UnitController unitController) {
            Configure(systemGameManager);

            this.clientId = clientId;
            this.playerCharacterSaveData = playerCharacterSaveData;
            this.unitController = unitController;
            saveDataDirty = false;
            unitController.OnCameraTargetReady += HandleCameraTargetReady;
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            saveManager = systemGameManager.SaveManager;
            networkManagerServer = systemGameManager.NetworkManagerServer;
        }

        public void SavePlayerLocation() {
            if (unitController.transform.position.x != playerCharacterSaveData.SaveData.PlayerLocationX
                || unitController.transform.position.y != playerCharacterSaveData.SaveData.PlayerLocationY
                || unitController.transform.position.z != playerCharacterSaveData.SaveData.PlayerLocationZ
                || unitController.transform.forward.x != playerCharacterSaveData.SaveData.PlayerRotationX
                || unitController.transform.forward.y != playerCharacterSaveData.SaveData.PlayerRotationY
                || unitController.transform.forward.z != playerCharacterSaveData.SaveData.PlayerRotationZ) {
                saveManager.SavePlayerLocation(playerCharacterSaveData.SaveData, unitController);
                saveDataDirty = true;
            }
        }

        public void HandleCameraTargetReady() {
            unitController.OnCameraTargetReady -= HandleCameraTargetReady;
            SubscribeToUnitEvents();
        }

        public void HandleDespawn(UnitController unitController) {
            networkManagerServer.StopMonitoringPlayerUnit(playerCharacterSaveData.PlayerCharacterId);
        }

        private void SubscribeToUnitEvents() {
            unitController.UnitEventController.OnDespawn += HandleDespawn;
        }

        public void StopMonitoring() {
            unitController.OnCameraTargetReady -= HandleCameraTargetReady;

        }

    }
}

