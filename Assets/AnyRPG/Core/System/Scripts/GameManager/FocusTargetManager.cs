using System;
using System.Collections;
using UnityEngine;

namespace AnyRPG {
    public class FocusTargetManager : PreviewManager {

        protected bool eventSubscriptionsInitialized = false;

        private UnitController targetUnitController = null;

        // game manager references
        //protected PlayerManagerClient playerManagerClient = null;
        //protected SaveManager saveManager = null;

        /*
        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            //CreateEventSubscriptions();
        }

        public override void SetGameManagerReferences() {
            //Debug.Log("CharacterPanelManager.SetGameManagerReferences()");

            base.SetGameManagerReferences();
            //playerManagerClient = systemGameManager.PlayerManagerClient;
            saveManager = systemGameManager.SaveManager;
        }
        */

        /*
        protected void CreateEventSubscriptions() {
            //Debug.Log("CharacterPanel.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized) {
                return;
            }
            eventSubscriptionsInitialized = true;
        }

        protected void CleanupEventSubscriptions() {
            //Debug.Log("PlayerManager.CleanupEventSubscriptions()");
            if (!eventSubscriptionsInitialized) {
                return;
            }

            //Debug.Log("PlayerCombat.CleanupEventSubscriptions()");
            eventSubscriptionsInitialized = false;
        }
        */

        public override void SpawnUnit(UnitController targetUnitController) {
            //Debug.Log("CharacterPanel.HandlePlayerUnitSpawn()");
            this.targetUnitController = targetUnitController;

            base.SpawnUnit(targetUnitController);

            unitProfile = targetUnitController.UnitProfile;
            CharacterConfigurationRequest characterConfigurationRequest = new CharacterConfigurationRequest(unitProfile);
            // commented out these next two because they should have come from the unit profile
            //characterConfigurationRequest.unitType = playerManager.UnitController.BaseCharacter.UnitType;
            //characterConfigurationRequest.characterRace = playerManager.UnitController.BaseCharacter.CharacterRace;
            characterConfigurationRequest.faction = targetUnitController.BaseCharacter.Faction;
            characterConfigurationRequest.characterClass = targetUnitController.BaseCharacter.CharacterClass;
            characterConfigurationRequest.classSpecialization = targetUnitController.BaseCharacter.ClassSpecialization;
            characterConfigurationRequest.unitLevel = targetUnitController.CharacterStats.Level;

            // if the game is in lobby mode, there will be no save data
            if (targetUnitController.CharacterSaveManager.SaveData != null) {
                characterConfigurationRequest.characterAppearanceData = new CharacterAppearanceData(targetUnitController.CharacterSaveManager.SaveData);
            }

            SpawnUnit(characterConfigurationRequest);
            targetUnitController.UnitEventController.OnUnitTypeChange += HandleUnitTypeChange;
            targetUnitController.UnitEventController.OnFactionChange += HandleFactionChange;
            targetUnitController.UnitEventController.OnClassChange += HandleClassChange;
            targetUnitController.UnitEventController.OnSpecializationChange += HandleSpecializationChange;
            targetUnitController.UnitEventController.OnLevelChanged += HandleLevelChanged;
            targetUnitController.UnitEventController.OnDespawn += HandleTargetDespawn;
        }

        private void HandleTargetDespawn(UnitController targetUnitController) {
            DespawnUnit();
        }

        public void HandleUnitTypeChange(UnitType newUnitType, UnitType oldUnitType) {
            //Debug.Log("CharacterPanelManager.HandleUnitTypeChange()");
            unitController.BaseCharacter.ChangeUnitType(newUnitType);
        }

        public void HandleFactionChange(Faction newFaction, Faction oldFaction) {
            //Debug.Log("CharacterPanelManager.HandleFactionChange()");
            unitController.BaseCharacter.ChangeCharacterFaction(newFaction);
        }

        public void HandleClassChange(UnitController sourceUnitController, CharacterClass newClass, CharacterClass oldClass) {
            //Debug.Log("CharacterPanelManager.HandleClassChange()");
            unitController.BaseCharacter.ChangeCharacterClass(newClass);
        }

        public void HandleSpecializationChange(UnitController sourceUnitController, ClassSpecialization newSpecialization, ClassSpecialization oldSpecialization) {
            //Debug.Log("CharacterPanelManager.HandleSpecializationChange()");
            unitController.BaseCharacter.ChangeClassSpecialization(newSpecialization);
        }

        public override void DespawnUnit() {

            // unsubscribe from events on the target unit
            targetUnitController.UnitEventController.OnUnitTypeChange -= HandleUnitTypeChange;
            targetUnitController.UnitEventController.OnFactionChange -= HandleFactionChange;
            targetUnitController.UnitEventController.OnClassChange -= HandleClassChange;
            targetUnitController.UnitEventController.OnSpecializationChange -= HandleSpecializationChange;
            targetUnitController.UnitEventController.OnLevelChanged -= HandleLevelChanged;
            targetUnitController.UnitEventController.OnDespawn -= HandleTargetDespawn;

            // unsubscribe from events on the spawned unit
            unitController.UnitEventController.OnAddEquipment += HandleAddEquipment;
            unitController.UnitEventController.OnRemoveEquipment += HandleRemoveEquipment;

            base.DespawnUnit();

            targetUnitController = null;
        }

        public void HandleLevelChanged(int newLevel) {
            unitController.CharacterStats.SetLevelInternal(newLevel);
        }

        public void HandleAddEquipment(EquipmentSlotProfile profile, InstantiatedEquipment equipment) {
            //Debug.Log($"CharacterPanel.HandleAddEquipment({profile.ResourceName}, {equipment.ResourceName})");

            unitController.UnitModelController.RebuildModelAppearance();
        }

        public void HandleRemoveEquipment(EquipmentSlotProfile profile, InstantiatedEquipment equipment) {
            //Debug.Log($"CharacterPanel.HandleRemoveEquipment({profile.ResourceName}, {equipment.ResourceName})");

            unitController.UnitModelController.RebuildModelAppearance();
        }

        protected override void BroadcastUnitCreated() {
            base.BroadcastUnitCreated();
            HandleTargetCreated();
        }

        public void HandleTargetCreated() {
            //Debug.Log("CharacterPanelManager.HandleTargetCreated()");

            //unitController?.UnitModelController.SetInitialSavedAppearance(playerManager.PlayerCharacterSaveData.SaveData);
            CharacterEquipmentManager characterEquipmentManager = unitController.CharacterEquipmentManager;

            if (characterEquipmentManager != null) {
                if (targetUnitController?.CharacterEquipmentManager != null) {

                    //characterEquipmentManager.CurrentEquipment = playerManager.UnitController.CharacterEquipmentManager.CurrentEquipment;
                    // testing new code to avoid just making a pointer to the player gear, which results in equip/unequip not working properly
                    characterEquipmentManager.ClearSubscriptions();
                    foreach (EquipmentSlotProfile equipmentSlotProfile in targetUnitController.CharacterEquipmentManager.CurrentEquipment.Keys) {
                        characterEquipmentManager.AddCurrentEquipmentSlot(equipmentSlotProfile, targetUnitController.CharacterEquipmentManager.CurrentEquipment[equipmentSlotProfile]);
                    }
                    characterEquipmentManager.CreateSubscriptions();
                }
                unitController.UnitEventController.OnAddEquipment += HandleAddEquipment;
                unitController.UnitEventController.OnRemoveEquipment += HandleRemoveEquipment;
            } else {
                Debug.LogWarning("CharacterPanel.HandleTargetCreated(): could not find a characterEquipmentManager");
            }
        }

        /*
        protected virtual void OnDestroy() {
            //Debug.Log("WindowContentController.OnDestroy()");
            CleanupEventSubscriptions();
        }
        */

    }
}