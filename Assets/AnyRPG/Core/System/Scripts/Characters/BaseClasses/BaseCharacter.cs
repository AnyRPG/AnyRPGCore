using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class BaseCharacter : ConfiguredClass, ICapabilityConsumer {

        // properties that come from the unit profile
        private string characterName = string.Empty;
        private string title = string.Empty;
        private Faction faction = null;
        private UnitType unitType = null;
        private CharacterRace characterRace = null;
        private CharacterClass characterClass = null;
        private ClassSpecialization classSpecialization = null;
        private bool spawnDead = false;
        private UnitToughness unitToughness = null;

        // common access to properties that provide stats
        private List<IStatProvider> statProviders = new List<IStatProvider>();

        // components
        private UnitController unitController = null;

        // track state
        //private bool characterInitialized = false;

        // logic for processing capabilities lives here
        private CapabilityConsumerProcessor capabilityConsumerProcessor = null;

        //public UnitController UnitController { get => unitController; }

        public string CharacterName { get => characterName; }

        public UnitType UnitType { get => unitType; set => unitType = value; }
        public CharacterRace CharacterRace { get => characterRace; }
        public CharacterClass CharacterClass { get => characterClass; }
        public ClassSpecialization ClassSpecialization { get => classSpecialization; }
        public Faction Faction {
            get {
                if (unitController.UnderControl) {
                    //Debug.Log($"{gameObject.name}.MyFactionName: return master unit faction name");
                    return unitController.MasterUnit.BaseCharacter.Faction;
                }
                return faction;
            }
        }
        public bool SpawnDead { get => spawnDead; set => spawnDead = value; }
        public string Title { get => title; set => title = value; }
        public List<IStatProvider> StatProviders { get => statProviders; set => statProviders = value; }
        public UnitToughness UnitToughness { get => unitToughness; set => unitToughness = value; }
        public UnitProfile UnitProfile { get => unitController.UnitProfile; }
        public CapabilityConsumerProcessor CapabilityConsumerProcessor { get => capabilityConsumerProcessor; }

        public BaseCharacter(UnitController unitController, SystemGameManager systemGameManager) {
            this.unitController = unitController;
            Configure(systemGameManager);
            capabilityConsumerProcessor = new CapabilityConsumerProcessor(this, systemGameManager);
        }

        /*
        public void ChangeCharacterRace(CharacterRace newCharacterRace) {
            //Debug.Log($"{gameObject.name}.PlayerCharacter.Joinfaction(" + newFaction + ")");
            if (newCharacterRace != null && newCharacterRace != characterRace) {
                SetCharacterRace(newCharacterRace);
            }
        }
        */

        public void UpdateStatProviderList() {
            statProviders = new List<IStatProvider>();
            statProviders.Add(systemConfigurationManager);
            if (unitController.UnitProfile != null) {
                statProviders.Add(unitController.UnitProfile);
            }
            if (unitType != null) {
                statProviders.Add(unitType);
            }
            if (characterRace != null) {
                statProviders.Add(characterRace);
            }
            if (characterClass != null) {
                statProviders.Add(characterClass);
            }
            if (classSpecialization != null) {
                statProviders.Add(classSpecialization);
            }

            unitController.CharacterStats.HandleUpdateStatProviders();
        }

        public void Initialize() {
            //Debug.Log($"{gameObject.name}: BaseCharacter.Initialize()");
            unitController.CharacterInventoryManager.PerformSetupActivities();
        }

        public void SetUnitToughness(UnitToughness newUnitToughness) {
            //Debug.Log($"{gameObject.name}: BaseCharacter.SetUnitToughness(" + (newUnitToughness == null ? "null" : newUnitToughness.DisplayName) + ")");

            unitToughness = newUnitToughness;
        }

        public void SetCharacterName(string newName) {
            Debug.Log($"{unitController.gameObject.name}.BaseCharacter.SetCharactername({newName})");

            if (string.IsNullOrEmpty(newName)) {
                return;
            }
            characterName = newName;
        }

        public void ChangeCharacterName(string newName) {
            Debug.Log($"{unitController.gameObject.name}.BaseCharacter.ChangeCharactername({newName})");

            SetCharacterName(newName);
            unitController.UnitEventController.NotifyOnNameChange(newName);
        }

        public void SetCharacterTitle(string newTitle) {
            //Debug.Log($"{gameObject.name}.BaseCharacter.SetCharacterFaction(" + newFaction + ")");
            if (newTitle == null) {
                return;
            }
            title = newTitle;
        }

        public void ChangeCharacterTitle(string newTitle) {
            SetCharacterTitle(newTitle);
            unitController.UnitEventController.NotifyOnTitleChange(newTitle);
        }

        public void SetCharacterFaction(Faction newFaction) {
            //Debug.Log($"{gameObject.name}.BaseCharacter.SetCharacterFaction(" + newFaction + ")");

            if (newFaction == null) {
                return;
            }
            faction = newFaction;
            unitController.CharacterFactionManager.SetReputation(newFaction);
        }

        public void ChangeCharacterFaction(Faction newFaction) {
            //Debug.Log($"{gameObject.name}.PlayerCharacter.Joinfaction(" + newFaction + ")");
            if (newFaction != null && newFaction != faction) {
                CapabilityConsumerSnapshot oldSnapshot = new CapabilityConsumerSnapshot(this, systemGameManager);
                Faction oldFaction = faction;
                unitController.CharacterStats.ClearStatusEffects();
                unitController.CharacterPetManager.DespawnAllPets();
                SetCharacterFaction(newFaction);
                capabilityConsumerProcessor.UpdateCapabilityProviderList();
                // get a snapshot of the new state
                CapabilityConsumerSnapshot newSnapshot = new CapabilityConsumerSnapshot(this, systemGameManager);
                UpdateStatProviderList();
                // update capabilities based on the difference between old and new snapshots
                ProcessCapabilityConsumerChange(oldSnapshot, newSnapshot);
                // this will reset the stats
                unitController.CharacterStats.SetLevel(unitController.CharacterStats.Level);
                unitController.UnitEventController.NotifyOnFactionChange(newFaction, oldFaction);
            }
        }

        public void SetClassSpecialization(ClassSpecialization newClassSpecialization) {
            //Debug.Log($"{gameObject.name}.BaseCharacter.SetClassSpecialization({newCharacterClassName})");

            classSpecialization = newClassSpecialization;
        }

        public void ChangeClassSpecialization(ClassSpecialization newClassSpecialization) {
            //Debug.Log($"{gameObject.name}.PlayerCharacter.Joinfaction(" + newFaction + ")");
            if (newClassSpecialization != classSpecialization) {
                unitController.CharacterStats.ClearStatusEffects();
                unitController.CharacterPetManager.DespawnAllPets();
                CapabilityConsumerSnapshot oldSnapshot = new CapabilityConsumerSnapshot(this, systemGameManager);
                ClassSpecialization oldClassSpecialization = classSpecialization;
                SetClassSpecialization(newClassSpecialization);
                capabilityConsumerProcessor.UpdateCapabilityProviderList();
                // get a snapshot of the new state
                CapabilityConsumerSnapshot newSnapshot = new CapabilityConsumerSnapshot(this, systemGameManager);
                UpdateStatProviderList();
                // update capabilities based on the difference between old and new snapshots
                ProcessCapabilityConsumerChange(oldSnapshot, newSnapshot);
                // this will reset the stats
                unitController.CharacterStats.SetLevel(unitController.CharacterStats.Level);
                unitController.UnitEventController.NotifyOnSpecializationChange(newClassSpecialization, oldClassSpecialization);
            }
        }


        public void SetCharacterClass(CharacterClass newCharacterClass) {
            //Debug.Log($"{gameObject.name}.BaseCharacter.SetCharacterClass(" + (newCharacterClass != null ? newCharacterClass.DisplayName : "null") + ", " + notify + ", " + resetStats + ", " + processEquipmentRestrictions + ")");

            //if (newCharacterClass == null) {
            //    return;
            //}

            characterClass = newCharacterClass;
        }

        public void ChangeCharacterClass(CharacterClass newCharacterClass) {
            //Debug.Log($"{gameObject.name}.PlayerCharacter.ChangeCharacterClass(" + newFaction + ")");
            if (newCharacterClass != null && newCharacterClass != characterClass) {
                unitController.CharacterStats.ClearStatusEffects();
                unitController.CharacterPetManager.DespawnAllPets();
                CharacterClass oldCharacterClass = characterClass;
                CapabilityConsumerSnapshot oldSnapshot = new CapabilityConsumerSnapshot(this, systemGameManager);
                ChangeClassSpecialization(null);
                SetCharacterClass(newCharacterClass);
                capabilityConsumerProcessor.UpdateCapabilityProviderList();
                // get a snapshot of the new state
                CapabilityConsumerSnapshot newSnapshot = new CapabilityConsumerSnapshot(this, systemGameManager);
                UpdateStatProviderList();
                // update capabilities based on the difference between old and new snapshots
                ProcessCapabilityConsumerChange(oldSnapshot, newSnapshot, true);
                // this will reset the stats
                unitController.UnitEventController.NotifyOnClassChange(newCharacterClass, oldCharacterClass);
                unitController.CharacterStats.SetLevel(unitController.CharacterStats.Level);
            }
        }

        public void SetCharacterRace(CharacterRace newCharacterRace, bool notify = true, bool processEquipmentRestrictions = true) {
            //Debug.Log($"{gameObject.name}.BaseCharacter.SetCharacterClass(" + (newCharacterClass != null ? newCharacterClass.DisplayName : "null") + ", " + notify + ")");

            if (newCharacterRace == null) {
                return;
            }
            characterRace = newCharacterRace;
        }

        public void ChangeCharacterRace(CharacterRace newCharacterRace) {
            //Debug.Log($"{gameObject.name}.BaseCharacter.ChangeCharacterRace(" + newUnitType + ")");
            if (newCharacterRace != null && newCharacterRace != characterRace) {
                unitController.CharacterStats.ClearStatusEffects();
                unitController.CharacterPetManager.DespawnAllPets();
                CharacterRace oldCharacterRace = characterRace;
                CapabilityConsumerSnapshot oldSnapshot = new CapabilityConsumerSnapshot(this, systemGameManager);
                SetCharacterRace(newCharacterRace);
                capabilityConsumerProcessor.UpdateCapabilityProviderList();
                // get a snapshot of the new state
                CapabilityConsumerSnapshot newSnapshot = new CapabilityConsumerSnapshot(this, systemGameManager);
                UpdateStatProviderList();
                // update capabilities based on the difference between old and new snapshots
                ProcessCapabilityConsumerChange(oldSnapshot, newSnapshot, true);
                unitController.UnitEventController.NotifyOnRaceChange(newCharacterRace, oldCharacterRace);
                unitController.CharacterStats.SetLevel(unitController.CharacterStats.Level);
            }
        }

        public void SetUnitType(UnitType newUnitType) {
            //Debug.Log($"{gameObject.name}.BaseCharacter.SetUnitType(" + (newUnitType != null ? newUnitType.DisplayName : "null") + ", " + notify + ")");

            if (newUnitType == null) {
                return;
            }
            unitType = newUnitType;
        }

        public void ChangeUnitType(UnitType newUnitType) {
            //Debug.Log($"{gameObject.name}.BaseCharacter.ChangeUnitType(" + newUnitType + ")");
            if (newUnitType != null && newUnitType != unitType) {
                unitController.CharacterStats.ClearStatusEffects();
                unitController.CharacterPetManager.DespawnAllPets();
                CapabilityConsumerSnapshot oldSnapshot = new CapabilityConsumerSnapshot(this, systemGameManager);
                SetUnitType(newUnitType);
                capabilityConsumerProcessor.UpdateCapabilityProviderList();
                // get a snapshot of the new state
                CapabilityConsumerSnapshot newSnapshot = new CapabilityConsumerSnapshot(this, systemGameManager);
                UpdateStatProviderList();
                // update capabilities based on the difference between old and new snapshots
                ProcessCapabilityConsumerChange(oldSnapshot, newSnapshot, true);
                unitController.UnitEventController.NotifyOnUnitTypeChange(newUnitType, oldSnapshot.UnitType);
                unitController.CharacterStats.SetLevel(unitController.CharacterStats.Level);
            }
        }

        public void ProcessCapabilityConsumerChange(CapabilityConsumerSnapshot oldSnapshot, CapabilityConsumerSnapshot newSnapshot, bool processEquipmentRestrictions = true) {
            //Debug.Log($"{gameObject.name}.BaseCharacter.ProcessCapabilityConsumerChange()");
            if (processEquipmentRestrictions == true) {
                unitController.CharacterEquipmentManager.HandleCapabilityConsumerChange();
            }
            unitController.CharacterAbilityManager.HandleCapabilityProviderChange(oldSnapshot, newSnapshot);
            unitController.CharacterPetManager.ProcessCapabilityProviderChange(newSnapshot);
        }

        public void HandleCharacterUnitSpawn() {
            //Debug.Log($"{gameObject.name}.BaseCharacter.HandleCharacterUnitSpawn()");
            // no longer necessary - moved to UnitModel -> UnitModelController
            //characterEquipmentManager.HandleCharacterUnitSpawn();
            unitController.CharacterStats.HandleCharacterUnitSpawn();
        }

        public void DespawnImmediate() {
            //Debug.Log($"{gameObject.name}.BaseCharacter.DespawnImmediate()");
            if (unitController != null && unitController.CharacterUnit != null) {
                unitController.CharacterUnit.Despawn(0, false, true);
            }
        }

        public void Despawn() {
            //Debug.Log($"{gameObject.name}.BaseCharacter.Despawn()");

            if (unitController != null && unitController.CharacterUnit != null) {
                unitController.CharacterUnit.Despawn();
            }
        }

        public void TryToDespawn() {
            //Debug.Log($"{gameObject.name}.BaseCharacter.TryToDespawn()");

            if (unitController.UnitProfile != null && unitController.UnitProfile.PreventAutoDespawn == true) {
                return;
            }
            if (unitController != null && unitController.LootableCharacter != null) {
                // lootable character handles its own despawn logic
                return;
            } else {

            }

            Despawn();
        }

        private void ResetSettings() {
            //Debug.Log($"{gameObject.name}.BaseCharacter.ResetSettings()");
            characterName = string.Empty;
            title = string.Empty;
            faction = null;
            unitType = null;
            characterRace = null;
            characterClass = null;
            classSpecialization = null;
            spawnDead = false;
            unitToughness = null;

            statProviders = new List<IStatProvider>();

            capabilityConsumerProcessor = null;

            unitController = null;

            //characterInitialized = false;
        }

    }

}