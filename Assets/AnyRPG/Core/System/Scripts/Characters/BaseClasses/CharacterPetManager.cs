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

        public List<UnitProfile> UnitProfiles { get => unitProfiles; set => unitProfiles = value; }
        public Dictionary<UnitProfile, UnitController> ActiveUnitProfiles { get => activeUnitProfiles; set => activeUnitProfiles = value; }
        public List<UnitType> ValidPetTypeList { get => validPetTypeList; set => validPetTypeList = value; }

        public CharacterPetManager(UnitController unitController, SystemGameManager systemGameManager) {
            this.unitController = unitController;
            Configure(systemGameManager);
        }

        public void ProcessCapabilityProviderChange(CapabilityConsumerSnapshot newSnapshot) {
            validPetTypeList.Clear();
            validPetTypeList.AddRange(newSnapshot.GetValidPetTypeList());

            foreach (UnitProfile unitProfile in newSnapshot.GetStartingPetList()) {
                AddPet(unitProfile);
            }
        }

        public void AddTemporaryPet(UnitProfile unitProfile, UnitController petUnitController) {

            if (petUnitController == null) {
                return;
            }

            if (activeUnitProfiles.ContainsKey(unitProfile) == false) {
                activeUnitProfiles.Add(unitProfile, petUnitController);
                petUnitController.SetPetMode(unitController, true);
                petUnitController.UnitEventController.OnUnitDestroy += HandleUnitDestroy;
            }
        }

        public void CapturePet(UnitProfile unitProfile, UnitController unitController) {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterPetManager.CapturePet(" + (unitProfile == null ? "null" : unitProfile.DisplayName) + ", " + (unitController == null ? "null" : unitController.gameObject.name) + ")");

            if (unitController == null) {
                return;
            }

            // you can only have one of the same pet active at a time
            if (activeUnitProfiles.ContainsKey(unitProfile) == false) {
                activeUnitProfiles.Add(unitProfile, unitController);
                unitController.SetPetMode(this.unitController, true);
                unitController.UnitEventController.OnUnitDestroy += HandleUnitDestroy;
            }

            AddPet(unitProfile);
        }

        public virtual void AddPet(UnitProfile unitProfile) {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterPetManager.AddPet(" + unitProfile.DisplayName + ")");
            // need more logic in here about whether this class or spec is allowed to capture this type of pet
            if (unitProfiles != null && unitProfiles.Contains(unitProfile) == false && unitProfile.IsPet == true) {
                unitProfiles.Add(unitProfile);
            }
        }

        public virtual void AddPet(string unitProfileName) {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterPetManager.AddPet(" + unitProfileName + ")");
            UnitProfile unitProfile = systemDataFactory.GetResource<UnitProfile>(unitProfileName);
            if (unitProfile != null) {
                AddPet(unitProfile);
            } else {
                Debug.LogWarning(unitController.gameObject.name + ".CharacterPetManager.AddPet() Could not find unitProfile: " + unitProfileName);
            }
        }

        public virtual void DespawnPet(UnitProfile unitProfile) {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterPetManager.DeSpawnPet(" + unitProfile.DisplayName + ")");
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
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterPetManager.HandleUnitDestroy(" + unitProfile.DisplayName + ")");
            if (activeUnitProfiles.ContainsKey(unitProfile)) {
                activeUnitProfiles[unitProfile].UnitEventController.OnUnitDestroy -= HandleUnitDestroy;
                activeUnitProfiles.Remove(unitProfile);
            }
        }

        public virtual void SpawnPet(UnitProfile unitProfile) {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterPetManager.SpawnPet(" + unitProfile.DisplayName + ")");
            if (activeUnitProfiles.ContainsKey(unitProfile)) {
                // can't add the same dictionary key twice
                return;
            }
            CharacterConfigurationRequest characterConfigurationRequest = new CharacterConfigurationRequest(unitProfile);
            characterConfigurationRequest.unitControllerMode = UnitControllerMode.Pet;
            CharacterRequestData characterRequestData = new CharacterRequestData(this,
                systemGameManager.GameMode,
                characterConfigurationRequest);

            systemGameManager.CharacterManager.SpawnUnitPrefab(characterRequestData, unitController.transform.parent, unitController.transform.position, unitController.transform.forward);
        }

        public void ConfigureSpawnedCharacter(UnitController unitController, CharacterRequestData characterRequestData) {
            unitController.SetPetMode(this.unitController);
        }

        public void PostInit(UnitController unitController, CharacterRequestData characterRequestData) {
            unitController.UnitEventController.OnUnitDestroy += HandleUnitDestroy;
            activeUnitProfiles.Add(characterRequestData.characterConfigurationRequest.unitProfile, unitController);
        }



        //public void ProcessLevelUnload() {
        public void HandleCharacterUnitDespawn() {
            DespawnAllPets();
        }

        public void HandleDie() {
            DespawnAllPets();
        }

    }

}