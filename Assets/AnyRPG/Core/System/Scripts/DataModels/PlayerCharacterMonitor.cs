using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class PlayerCharacterMonitor : ConfiguredClass {

        public int accountId;
        public PlayerCharacterSaveData playerCharacterSaveData;
        public UnitController unitController;
        public bool saveDataDirty;
        public bool disconnected = false;

        // game manager references
        private SaveManager saveManager = null;

        public PlayerCharacterMonitor(SystemGameManager systemGameManager, int accountId, PlayerCharacterSaveData playerCharacterSaveData, UnitController unitController) {
            Configure(systemGameManager);

            this.accountId = accountId;
            this.playerCharacterSaveData = playerCharacterSaveData;
            this.unitController = unitController;
            saveDataDirty = false;
            //unitController.OnCameraTargetReady += HandleCameraTargetReady;
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            saveManager = systemGameManager.SaveManager;
        }

        public void ProcessBeforeDespawn() {
            //Debug.Log($"PlayerCharacterMonitor.ProcessBeforeDespawn() for account {accountId}");

            unitController.CharacterSaveManager.SaveGameData();
            unitController.UnitEventController.OnSaveDataUpdated -= HandleSaveDataUpdated;
            unitController = null;
        }

        public void SavePlayerLocation() {
            if (unitController?.CharacterSaveManager == null) {
                return;
            }
            if (unitController.transform.position.x != playerCharacterSaveData.SaveData.PlayerLocationX
                || unitController.transform.position.y != playerCharacterSaveData.SaveData.PlayerLocationY
                || unitController.transform.position.z != playerCharacterSaveData.SaveData.PlayerLocationZ
                || unitController.transform.forward.x != playerCharacterSaveData.SaveData.PlayerRotationX
                || unitController.transform.forward.y != playerCharacterSaveData.SaveData.PlayerRotationY
                || unitController.transform.forward.z != playerCharacterSaveData.SaveData.PlayerRotationZ) {
                unitController.CharacterSaveManager.SavePlayerLocation();
                saveDataDirty = true;
            }
        }

        /*
        public void HandleCameraTargetReady() {
            unitController.OnCameraTargetReady -= HandleCameraTargetReady;
            SubscribeToUnitEvents();
        }
        */

        /*
        public void HandleDespawn(UnitController unitController) {
            networkManagerServer.StopMonitoringPlayerUnit(playerCharacterSaveData.PlayerCharacterId);
        }
        */

        /*
        private void SubscribeToUnitEvents() {
            //unitController.UnitEventController.OnDespawn += HandleDespawn;
        }
        */

        /*
        public void StopMonitoring() {
            unitController.OnCameraTargetReady -= HandleCameraTargetReady;
        }
        */

        public void SetUnitController(UnitController unitController) {
            //Debug.Log($"PlayerCharacterMonitor.SetUnitController({unitController.gameObject.name})");

            this.unitController = unitController;
            disconnected = false;
            unitController.UnitEventController.OnSaveDataUpdated += HandleSaveDataUpdated;
        }

        private void HandleSaveDataUpdated() {
            saveDataDirty = true;
        }

        public void SetDisconnected() {
            //Debug.Log($"PlayerCharacterMonitor.SetDisconnected() for account {accountId}");

            ProcessBeforeDespawn();
            this.disconnected = true;
        }
    }
}

