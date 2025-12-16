using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class CharacterConfigurationRequest {

        public UnitProfile unitProfile;
        public string characterName = string.Empty;
        public string characterTitle = string.Empty;
        public UnitType unitType = null;
        public CharacterRace characterRace = null;
        public CharacterClass characterClass = null;
        public ClassSpecialization classSpecialization = null;
        public Faction faction = null;
        public UnitToughness unitToughness = null;
        public int unitLevel = 1;
        public int currentExperience = 0;
        public bool isDead = false;
        public UnitControllerMode unitControllerMode;
        public CharacterAppearanceData characterAppearanceData = null;

        public CharacterConfigurationRequest(ICapabilityConsumer capabilityConsumer) {
            SetUnitProfileProperties(capabilityConsumer.UnitProfile);
            unitType = capabilityConsumer.UnitType;
            characterRace = capabilityConsumer.CharacterRace;
            characterClass = capabilityConsumer.CharacterClass;
            classSpecialization = capabilityConsumer.ClassSpecialization;
            faction = capabilityConsumer.Faction;
        }

        public CharacterConfigurationRequest(UnitProfile unitProfile) {
            SetUnitProfileProperties(unitProfile);
        }

        public CharacterConfigurationRequest(SystemDataFactory systemDataFactory, CharacterSaveData saveData) {
            //Debug.Log($"CharacterConfigurationRequest.CharacterConfigurationRequest() faction: {saveData.playerFaction}");

            SetUnitProfileProperties(systemDataFactory.GetResource<UnitProfile>(saveData.UnitProfileName));
            characterName = saveData.CharacterName;
            characterClass = systemDataFactory.GetResource<CharacterClass>(saveData.CharacterClass);
            classSpecialization = systemDataFactory.GetResource<ClassSpecialization>(saveData.ClassSpecialization);
            faction = systemDataFactory.GetResource<Faction>(saveData.CharacterFaction);
            unitLevel = saveData.CharacterLevel;
            currentExperience = saveData.CurrentExperience;
            isDead = saveData.IsDead;
            characterAppearanceData = new CharacterAppearanceData(saveData);
        }

        private void SetUnitProfileProperties(UnitProfile unitProfile) {
            //Debug.Log($"CharacterConfigurationRequest.SetUnitProfileProperties({(unitProfile == null ? "null" : unitProfile.name)})");

            this.unitProfile = unitProfile;
            if (unitProfile == null) {
                // this could potentially happen if loading old save data after an upgrade where a unit profile is renamed
                return;
            }
            characterName = unitProfile.CharacterName;
            characterTitle = unitProfile.Title;
            unitType = unitProfile.UnitType;
            characterRace = unitProfile.CharacterRace;
            characterClass = unitProfile.CharacterClass;
            classSpecialization = unitProfile.ClassSpecialization;
            faction = unitProfile.Faction;
            unitToughness = unitProfile.DefaultToughness;
            isDead = unitProfile.SpawnDead;
        }


    }
}

