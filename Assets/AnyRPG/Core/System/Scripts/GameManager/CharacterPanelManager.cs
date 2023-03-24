using AnyRPG;
using System.Collections;
using UnityEngine;

namespace AnyRPG {
    public class CharacterPanelManager : PreviewManager {

        protected bool eventSubscriptionsInitialized = false;

        // game manager references
        protected PlayerManager playerManager = null;
        protected SystemEventManager systemEventManager = null;
        protected SaveManager saveManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            CreateEventSubscriptions();
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            playerManager = systemGameManager.PlayerManager;
            systemEventManager = systemGameManager.SystemEventManager;
            saveManager = systemGameManager.SaveManager;
        }

        protected void CreateEventSubscriptions() {
            //Debug.Log("CharacterPanel.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized) {
                return;
            }
            SystemEventManager.StartListening("OnPlayerUnitSpawn", HandlePlayerUnitSpawn);
            SystemEventManager.StartListening("OnPlayerUnitDespawn", HandlePlayerUnitDespawn);
            if (playerManager.PlayerUnitSpawned == true) {
                ProcessPlayerUnitSpawn();
            }
            eventSubscriptionsInitialized = true;
        }

        protected void CleanupEventSubscriptions() {
            //Debug.Log("PlayerManager.CleanupEventSubscriptions()");
            if (!eventSubscriptionsInitialized) {
                return;
            }

            //Debug.Log("PlayerCombat.CleanupEventSubscriptions()");
            SystemEventManager.StopListening("OnPlayerUnitSpawn", HandlePlayerUnitSpawn);
            SystemEventManager.StopListening("OnPlayerUnitDespawn", HandlePlayerUnitDespawn);
            eventSubscriptionsInitialized = false;
        }

        public void HandlePlayerUnitSpawn(string eventName, EventParamProperties eventParamProperties) {
            //Debug.Log($"{gameObject.name}.InanimateUnit.HandlePlayerUnitSpawn()");
            ProcessPlayerUnitSpawn();
        }

        public void ProcessPlayerUnitSpawn() {
            //Debug.Log("CharacterPanel.HandlePlayerUnitSpawn()");
            unitProfile = playerManager.ActiveUnitController.UnitProfile;
            SpawnUnit();
            systemEventManager.OnEquipmentChanged += HandleEquipmentChanged;
            playerManager.ActiveUnitController.UnitEventController.OnUnitTypeChange += HandleUnitTypeChange;
            playerManager.ActiveUnitController.UnitEventController.OnRaceChange += HandleRaceChange;
            playerManager.ActiveUnitController.UnitEventController.OnFactionChange += HandleFactionChange;
            playerManager.ActiveUnitController.UnitEventController.OnClassChange += HandleClassChange;
            playerManager.ActiveUnitController.UnitEventController.OnSpecializationChange += HandleSpecializationChange;
            playerManager.ActiveUnitController.UnitEventController.OnLevelChanged += HandleLevelChanged;
        }

        public void HandleUnitTypeChange(UnitType newUnitType, UnitType oldUnitType) {
            //Debug.Log("CharacterPanelManager.HandleUnitTypeChange()");
            unitController.CharacterUnit.BaseCharacter.SetUnitType(newUnitType, true, false, false);
        }

        public void HandleRaceChange(CharacterRace newRace, CharacterRace oldRace) {
            //Debug.Log("CharacterPanelManager.HandleRaceChange()");
            unitController.CharacterUnit.BaseCharacter.SetCharacterRace(newRace, true, false, false);
        }

        public void HandleFactionChange(Faction newFaction, Faction oldFaction) {
            //Debug.Log("CharacterPanelManager.HandleFactionChange()");
            unitController.CharacterUnit.BaseCharacter.SetCharacterFaction(newFaction, true, false, false);
        }

        public void HandleClassChange(CharacterClass newClass, CharacterClass oldClass) {
            //Debug.Log("CharacterPanelManager.HandleClassChange()");
            unitController.CharacterUnit.BaseCharacter.SetCharacterClass(newClass, true, false, false);
        }

        public void HandleSpecializationChange(ClassSpecialization newSpecialization, ClassSpecialization oldSpecialization) {
            //Debug.Log("CharacterPanelManager.HandleSpecializationChange()");
            unitController.CharacterUnit.BaseCharacter.SetClassSpecialization(newSpecialization, true, false, false);
        }

        public void HandlePlayerUnitDespawn(string eventName, EventParamProperties eventParamProperties) {
            //Debug.Log("CharacterPanel.HandlePlayerUnitDespawn()");
            systemEventManager.OnEquipmentChanged -= HandleEquipmentChanged;
            playerManager.ActiveUnitController.UnitEventController.OnUnitTypeChange -= HandleUnitTypeChange;
            playerManager.ActiveUnitController.UnitEventController.OnRaceChange -= HandleRaceChange;
            playerManager.ActiveUnitController.UnitEventController.OnFactionChange -= HandleFactionChange;
            playerManager.ActiveUnitController.UnitEventController.OnClassChange -= HandleClassChange;
            playerManager.ActiveUnitController.UnitEventController.OnSpecializationChange -= HandleSpecializationChange;
            playerManager.ActiveUnitController.UnitEventController.OnLevelChanged -= HandleLevelChanged;

            DespawnUnit();
        }

        public void HandleLevelChanged(int newLevel) {
            unitController.CharacterUnit.BaseCharacter.CharacterStats.SetLevel(newLevel);
        }

        public void HandleEquipmentChanged(Equipment newEquipment, Equipment oldEquipment) {
            //Debug.Log("CharacterPanelManager.HandleEquipmentChanged(" + (newEquipment == null ? "null" : newEquipment.DisplayName) + ", " + (oldEquipment == null ? "null" : oldEquipment.DisplayName) + ")");
            if (oldEquipment != null) {
                unitController.CharacterUnit.BaseCharacter.CharacterEquipmentManager.Unequip(oldEquipment);
            }
            if (newEquipment != null) {
                unitController.CharacterUnit.BaseCharacter.CharacterEquipmentManager.Equip(newEquipment, null);
            }
            //unitController.UnitModelController.BuildModelAppearance();
            unitController.UnitModelController.RebuildModelAppearance();
        }

        protected override void BroadcastUnitCreated() {
            base.BroadcastUnitCreated();
            HandleTargetCreated();
        }

        public void HandleTargetCreated() {
            //Debug.Log("CharacterPanel.HandleTargetCreated()");
            unitController?.UnitModelController.SetInitialSavedAppearance(saveManager.CurrentSaveData);
            CharacterEquipmentManager characterEquipmentManager = unitController.CharacterUnit.BaseCharacter.CharacterEquipmentManager;

            // providers need to be set or equipment won't be able to be equipped
            unitController.CharacterUnit.BaseCharacter.SetUnitType(playerManager.MyCharacter.UnitType, true, false, false);
            unitController.CharacterUnit.BaseCharacter.SetCharacterRace(playerManager.MyCharacter.CharacterRace, true, false, false);
            unitController.CharacterUnit.BaseCharacter.SetCharacterFaction(playerManager.MyCharacter.Faction, true, false, false);
            unitController.CharacterUnit.BaseCharacter.SetCharacterClass(playerManager.MyCharacter.CharacterClass, true, false, false);
            unitController.CharacterUnit.BaseCharacter.SetClassSpecialization(playerManager.MyCharacter.ClassSpecialization, true, false, false);
            unitController.CharacterUnit.BaseCharacter.CharacterStats.SetLevel(playerManager.MyCharacter.CharacterStats.Level);

            if (characterEquipmentManager != null) {
                if (playerManager.MyCharacter?.CharacterEquipmentManager != null) {

                    //characterEquipmentManager.CurrentEquipment = playerManager.MyCharacter.CharacterEquipmentManager.CurrentEquipment;
                    // testing new code to avoid just making a pointer to the player gear, which results in equip/unequip not working properly
                    characterEquipmentManager.CurrentEquipment.Clear();
                    foreach (EquipmentSlotProfile equipmentSlotProfile in playerManager.MyCharacter.CharacterEquipmentManager.CurrentEquipment.Keys) {
                        characterEquipmentManager.CurrentEquipment.Add(equipmentSlotProfile, playerManager.MyCharacter.CharacterEquipmentManager.CurrentEquipment[equipmentSlotProfile]);
                    }
                }
            } else {
                Debug.Log("CharacterPanel.HandleTargetCreated(): could not find a characterEquipmentManager");
            }
        }

        protected virtual void OnDestroy() {
            //Debug.Log("WindowContentController.OnDestroy()");
            CleanupEventSubscriptions();
        }

    }
}