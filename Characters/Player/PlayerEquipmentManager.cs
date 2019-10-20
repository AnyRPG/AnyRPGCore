using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UMA;
using UMA.CharacterSystem;

public class PlayerEquipmentManager : CharacterEquipmentManager {

    protected override void CreateEventReferences() {
        //Debug.Log("PlayerManager.CreateEventReferences()");
        if (eventReferencesInitialized || !startHasRun) {
            return;
        }
        SystemEventManager.MyInstance.OnPlayerUnitSpawn += OnPlayerUnitSpawn;
        SystemEventManager.MyInstance.OnPlayerUnitDespawn += OnPlayerUnitDespawn;
        base.CreateEventReferences();
    }

    protected override void CleanupEventReferences() {
        //Debug.Log("PlayerManager.CleanupEventReferences()");
        if (!eventReferencesInitialized) {
            return;
        }
        SystemEventManager.MyInstance.OnPlayerUnitSpawn -= OnPlayerUnitSpawn;
        SystemEventManager.MyInstance.OnPlayerUnitDespawn -= OnPlayerUnitDespawn;
        base.CleanupEventReferences();
    }

    public void OnPlayerUnitSpawn() {
        //Debug.Log("EquipmentManager.OnPlayerUnitSpawn()");
        EquipCharacter();
        SubscribeToCombatEvents();
    }

    public void OnPlayerUnitDespawn() {
        //Debug.Log("EquipmentManager.OnPlayerUnitDespawn()");
        UnSubscribeFromCombatEvents();
    }

    protected void SubscribeToCombatEvents() {
        //Debug.Log("PlayerManager.CreateEventReferences()");
        if (subscribedToCombatEvents || !startHasRun) {
            return;
        }
        if (PlayerManager.MyInstance != null && PlayerManager.MyInstance.MyCharacter && PlayerManager.MyInstance.MyCharacter.MyCharacterCombat) {
            PlayerManager.MyInstance.MyCharacter.MyCharacterCombat.OnEnterCombat += HoldWeapons;
            PlayerManager.MyInstance.MyCharacter.MyCharacterCombat.OnDropCombat += SheathWeapons;

        }
        subscribedToCombatEvents = true;
    }

    protected void UnSubscribeFromCombatEvents() {
        //Debug.Log("PlayerManager.CleanupEventReferences()");
        if (!subscribedToCombatEvents) {
            return;
        }
        if (PlayerManager.MyInstance != null && PlayerManager.MyInstance.MyCharacter && PlayerManager.MyInstance.MyCharacter.MyCharacterCombat) {
            PlayerManager.MyInstance.MyCharacter.MyCharacterCombat.OnEnterCombat -= HoldWeapons;
            PlayerManager.MyInstance.MyCharacter.MyCharacterCombat.OnDropCombat -= SheathWeapons;
        }
        subscribedToCombatEvents = false;
    }

    public override void EquipCharacter() {
        //public void EquipCharacter(GameObject playerUnitObject = null, bool updateCharacterButton = true) {
        //Debug.Log("EquipmentManager.EquipCharacter(" + (playerUnitObject == null ? "null" : playerUnitObject.name) + ")");
        if (currentEquipment == null) {
            return;
        }
        base.EquipCharacter();

        // MOVE TO PLAYER MANAGER
        //SystemEventManager.MyInstance.NotifyOnEquipmentRefresh(equipment);
    }

    public override void Equip(Equipment newItem) {
        //Debug.Log("EquipmentManager.Equip()");
        if (newItem == null) {
            //Debug.Log("Instructed to Equip a null item!");
            return;
        }
        base.Equip(newItem);

        // DO THIS LAST OR YOU WILL SAVE THE UMA DATA BEFORE ANYTHING IS EQUIPPED!
        // updated oldItem to null here because this call is already done in Unequip.
        // having it here also was leading to duplicate stat removal when gear was changed.
        SystemEventManager.MyInstance.NotifyOnEquipmentChanged(newItem, null);
    }

    public override Equipment Unequip(EquipmentSlot equipmentSlot, int slotIndex = -1) {
        //Debug.Log("equipment manager trying to unequip item in slot " + equipmentSlot.ToString());
        Equipment returnValue = base.Unequip(equipmentSlot, slotIndex);
        if (returnValue != null) {
            if (PlayerManager.MyInstance.MyPlayerUnitSpawned) {

                if (slotIndex != -1) {
                    InventoryManager.MyInstance.AddItem(returnValue, slotIndex);
                } else {
                    InventoryManager.MyInstance.AddItem(returnValue);
                }
                SystemEventManager.MyInstance.NotifyOnEquipmentChanged(null, returnValue);
            }

        }
        return returnValue;
    }


}
