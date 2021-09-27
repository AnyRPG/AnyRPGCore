using AnyRPG;
using System.Collections;
using UnityEngine;

namespace AnyRPG {
    public class CharacterPanelManager : PreviewManager {

        protected bool eventSubscriptionsInitialized = false;

        // game manager references
        protected PlayerManager playerManager = null;
        protected SystemEventManager systemEventManager = null;

        public void HandleOpenWindow(UnitProfile unitProfile) {
            //Debug.Log("CharacterPanelManager.HandleOpenWindow()");

            if (unitProfile == null) {
                Debug.Log("CharacterPanelManager.HandleOpenWindow(): unitProfile is null");
                return;
            }
            cloneSource = unitProfile;
            if (cloneSource == null) {
                return;
            }

            OpenWindowCommon();
        }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            CreateEventSubscriptions();
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            playerManager = systemGameManager.PlayerManager;
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
            //Debug.Log(gameObject.name + ".InanimateUnit.HandlePlayerUnitSpawn()");
            ProcessPlayerUnitSpawn();
        }

        public void ProcessPlayerUnitSpawn() {
            //Debug.Log("CharacterPanel.HandlePlayerUnitSpawn()");
            systemEventManager.OnEquipmentChanged += HandleEquipmentChanged;
        }

        public void HandlePlayerUnitDespawn(string eventName, EventParamProperties eventParamProperties) {
            //Debug.Log("CharacterPanel.HandlePlayerUnitDespawn()");
            systemEventManager.OnEquipmentChanged -= HandleEquipmentChanged;
        }

        public void HandleEquipmentChanged(Equipment newEquipment, Equipment oldEquipment) {
            //Debug.Log("CharacterPanel.HandleEquipmentChanged(" + (newEquipment == null ? "null" : newEquipment.DisplayName) + ", " + (oldEquipment == null ? "null" : oldEquipment.DisplayName) + ")");
                if (oldEquipment != null) {
                    unitController.CharacterUnit.BaseCharacter.CharacterEquipmentManager.Unequip(oldEquipment, true, true, false);
                }
                if (newEquipment != null) {
                    unitController.CharacterUnit.BaseCharacter.CharacterEquipmentManager.Equip(newEquipment, null, true, true, false);
                }
                unitController.UnitModelController.BuildModelAppearance();
        }

        protected override void BroadcastTargetCreated() {
            base.BroadcastTargetCreated();
            HandleTargetCreated();
        }

        public void HandleTargetCreated() {
            //Debug.Log("CharacterPanel.HandleTargetCreated()");
            unitController?.UnitModelController.SetInitialSavedAppearance();
            CharacterEquipmentManager characterEquipmentManager = unitController.CharacterUnit.BaseCharacter.CharacterEquipmentManager;

            // providers need to be set or equipment won't be able to be equipped
            unitController.CharacterUnit.BaseCharacter.SetUnitType(playerManager.MyCharacter.UnitType, true, false, false);
            unitController.CharacterUnit.BaseCharacter.SetCharacterRace(playerManager.MyCharacter.CharacterRace, true, false, false);
            unitController.CharacterUnit.BaseCharacter.SetCharacterFaction(playerManager.MyCharacter.Faction, true, false, false);
            unitController.CharacterUnit.BaseCharacter.SetCharacterClass(playerManager.MyCharacter.CharacterClass, true, false, false);
            unitController.CharacterUnit.BaseCharacter.SetClassSpecialization(playerManager.MyCharacter.ClassSpecialization, true, false, false);

            if (characterEquipmentManager != null) {
                if (playerManager != null && playerManager.MyCharacter != null && playerManager.MyCharacter.CharacterEquipmentManager != null) {

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