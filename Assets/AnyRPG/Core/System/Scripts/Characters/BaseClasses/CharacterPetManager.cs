using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace AnyRPG {
    public class CharacterPetManager : ConfiguredClass, ICharacterRequestor {

        private List<UnitProfile> unitProfiles = new List<UnitProfile>();

        protected Dictionary<UnitProfile, UnitController> activeUnitProfiles = new Dictionary<UnitProfile, UnitController>();

        protected UnitController unitController;

        private List<UnitType> validPetTypeList = new List<UnitType>();

        protected bool eventSubscriptionsInitialized = false;

        // game manager references
        protected CharacterManager characterManager = null;
        protected LevelManager levelManager = null;

        public List<UnitProfile> UnitProfiles { get => unitProfiles; set => unitProfiles = value; }
        public Dictionary<UnitProfile, UnitController> ActiveUnitProfiles { get => activeUnitProfiles; set => activeUnitProfiles = value; }
        public List<UnitType> ValidPetTypeList { get => validPetTypeList; set => validPetTypeList = value; }

        public CharacterPetManager(UnitController unitController, SystemGameManager systemGameManager) {
            this.unitController = unitController;
            Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            characterManager = systemGameManager.CharacterManager;
            levelManager = systemGameManager.LevelManager;
        }

        public void ProcessCapabilityProviderChange(CapabilityConsumerSnapshot newSnapshot) {
            validPetTypeList.Clear();
            validPetTypeList.AddRange(newSnapshot.GetValidPetTypeList());

            foreach (UnitProfile unitProfile in newSnapshot.GetStartingPetList()) {
                AddPet(unitProfile);
            }
        }

        public void AddActivePet(UnitProfile unitProfile, UnitController petUnitController) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterPetManager.AddActivePet({unitProfile.DisplayName}, {petUnitController.gameObject.name})");

            if (petUnitController == null) {
                return;
            }

            if (activeUnitProfiles.ContainsKey(unitProfile) == false) {
                activeUnitProfiles.Add(unitProfile, petUnitController);
                petUnitController.SetPetMode(unitController);
                petUnitController.UnitEventController.OnUnitDestroy += HandleUnitDestroy;
                unitController.UnitEventController.NotifyOnAddActivePet(unitProfile, petUnitController);
            }
        }

        public void CapturePet(UnitProfile unitProfile, UnitController petUnitController) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterPetManager.CapturePet({unitProfile.DisplayName}, {petUnitController.gameObject.name})");

            if (petUnitController == null) {
                return;
            }

            AddActivePet(unitProfile, petUnitController);

            AddPet(unitProfile);
        }

        public virtual void AddPet(UnitProfile unitProfile) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterPetManager.AddPet({unitProfile.DisplayName})");

            // need more logic in here about whether this class or spec is allowed to capture this type of pet
            if (unitProfiles != null && unitProfiles.Contains(unitProfile) == false && unitProfile.IsPet == true) {
                unitProfiles.Add(unitProfile);
            }
            unitController.UnitEventController.NotifyOnAddPet(unitProfile);
        }

        public virtual void AddPet(string unitProfileName) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterPetManager.AddPet({unitProfileName})");

            UnitProfile unitProfile = systemDataFactory.GetResource<UnitProfile>(unitProfileName);
            if (unitProfile != null) {
                AddPet(unitProfile);
            } else {
                Debug.LogWarning($"{unitController.gameObject.name}.CharacterPetManager.AddPet({unitProfileName}) Could not find unitProfile");
            }
        }

        public virtual void DespawnPet(UnitProfile unitProfile) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterPetManager.DespawnPet({unitProfile.DisplayName})");

            if (activeUnitProfiles.ContainsKey(unitProfile)) {
                if (activeUnitProfiles[unitProfile] != null) {
                    activeUnitProfiles[unitProfile].Despawn(0f, false, true);
                }
            }
            activeUnitProfiles.Remove(unitProfile);
        }

        public void DespawnAllPets() {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterPetManager.DespawnAllPets()");

            foreach (UnitProfile unitProfile in activeUnitProfiles.Keys.ToArray()) {
                // check that the controller is not null.  It may have already been destroyed somehow
                if (activeUnitProfiles[unitProfile] != null) {
                    activeUnitProfiles[unitProfile].Despawn(0f, false, true);
                }
            }
            activeUnitProfiles.Clear();
        }

        public virtual void HandleUnitDestroy(UnitProfile unitProfile) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterPetManager.HandleUnitDestroy({unitProfile.ResourceName})");

            if (activeUnitProfiles.ContainsKey(unitProfile)) {
                activeUnitProfiles[unitProfile].UnitEventController.OnUnitDestroy -= HandleUnitDestroy;
                activeUnitProfiles.Remove(unitProfile);
                unitController.UnitEventController.NotifyOnRemoveActivePet(unitProfile);
            }
        }

        public virtual void SpawnPet(UnitProfile unitProfile) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterPetManager.SpawnPet({unitProfile.ResourceName})");

            if (activeUnitProfiles.ContainsKey(unitProfile)) {
                // can't add the same dictionary key twice
                return;
            }
            CharacterConfigurationRequest characterConfigurationRequest = new CharacterConfigurationRequest(unitProfile);
            characterConfigurationRequest.unitControllerMode = UnitControllerMode.Pet;
            characterConfigurationRequest.faction = unitController.BaseCharacter.Faction;
            characterConfigurationRequest.unitLevel = unitController.CharacterStats.Level;
            CharacterRequestData characterRequestData = new CharacterRequestData(this,
                systemGameManager.GameMode,
                characterConfigurationRequest);
            characterRequestData.characterId = characterManager.GetNewCharacterId(UnitControllerMode.Pet);
            if (networkManagerServer.ServerModeActive == true) {
                characterRequestData.isServerOwned = true;
                characterRequestData.requestMode = GameMode.Network;
                characterManager.SpawnUnitPrefab(characterRequestData, null, unitController.transform.position, unitController.transform.forward, unitController.gameObject.scene);
            } else {
                characterManager.SpawnUnitPrefabLocal(characterRequestData, unitController.transform.parent, unitController.transform.position, unitController.transform.forward);
            }
        }

        public void ConfigureSpawnedCharacter(UnitController unitController) {
            //unitController.SetPetMode(this.unitController);
        }

        public void PostInit(UnitController unitController) {
            AddActivePet(unitController.CharacterRequestData.characterConfigurationRequest.unitProfile, unitController);
        }

        public void HandleCharacterUnitDespawn() {
            if (systemGameManager.GameMode == GameMode.Local || networkManagerServer.ServerModeActive == true || levelManager.IsCutscene()) {
                DespawnAllPets();
            }
        }

        public void HandleDie() {
            if (systemGameManager.GameMode == GameMode.Local || networkManagerServer.ServerModeActive == true || levelManager.IsCutscene()) {
                DespawnAllPets();
            }
        }

        public void UpdatePetList(int currentLevel) {
            CapabilityConsumerSnapshot capabilityConsumerSnapshot = new CapabilityConsumerSnapshot(unitController.BaseCharacter, systemGameManager);

            ProcessCapabilityProviderChange(capabilityConsumerSnapshot);
        }
    }

}