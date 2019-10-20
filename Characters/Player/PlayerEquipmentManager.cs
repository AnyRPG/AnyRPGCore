﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UMA;
using UMA.CharacterSystem;

public class PlayerEquipmentManager : CharacterEquipmentManager {

    protected override void CreateEventReferences() {
        Debug.Log(gameObject.name + ".PlayerEquipmentManager.CreateEventReferences()");
        if (eventReferencesInitialized || !startHasRun) {
            return;
        }
        SystemEventManager.MyInstance.OnPlayerUnitSpawn += HandlePlayerUnitSpawn;
        SystemEventManager.MyInstance.OnPlayerUnitDespawn += HandlePlayerUnitDespawn;
        base.CreateEventReferences();
    }

    protected override void CleanupEventReferences() {
        //Debug.Log("PlayerManager.CleanupEventReferences()");
        if (!eventReferencesInitialized) {
            return;
        }
        SystemEventManager.MyInstance.OnPlayerUnitSpawn -= HandlePlayerUnitSpawn;
        SystemEventManager.MyInstance.OnPlayerUnitDespawn -= HandlePlayerUnitDespawn;
        base.CleanupEventReferences();
    }

    public void HandlePlayerUnitSpawn() {
        Debug.Log("EquipmentManager.OnPlayerUnitSpawn()");
        CreateComponentReferences();
        EquipCharacter();
        SubscribeToCombatEvents();
    }

    public void HandlePlayerUnitDespawn() {
        //Debug.Log("EquipmentManager.OnPlayerUnitDespawn()");
        UnSubscribeFromCombatEvents();
    }

    public override void CreateComponentReferences() {
        Debug.Log(gameObject.name + ".PlayerEquipmentManager.CreateComponentReferences()");
        base.CreateComponentReferences();
        /*
        if (componentReferencesInitialized) {
            return;
        }
        */

        // player character case
        if (baseCharacter != null) {
            if (baseCharacter.MyCharacterUnit != null) {
                playerUnitObject = baseCharacter.MyCharacterUnit.gameObject;
            } else {
                Debug.Log(gameObject.name + ".CharacterEquipmentManager.CreateComponentReferences(): baseCharacter.MyCharacterUnit == null!");
            }
        }

        // player character case
        if (baseCharacter != null) {
            if (baseCharacter.MyCharacterUnit != null) {
                dynamicCharacterAvatar = baseCharacter.MyCharacterUnit.GetComponent<DynamicCharacterAvatar>();
            }
        }

        //componentReferencesInitialized = true;
    }

    public override void EquipCharacter() {
        Debug.Log(gameObject.name + ".PlayerEquipmentManager.EquipCharacter()");
        //public void EquipCharacter(GameObject playerUnitObject = null, bool updateCharacterButton = true) {
        if (currentEquipment == null) {
            Debug.Log(gameObject.name + ".PlayerEquipmentManager.EquipCharacter(): currentEquipment == null!");
            return;
        }
        Debug.Log(gameObject.name + ".PlayerEquipmentManager.EquipCharacter(): about to call base method");
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
