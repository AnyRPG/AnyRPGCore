using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UMA;
using UMA.CharacterSystem;

namespace AnyRPG {
    public class PlayerEquipmentManager : CharacterEquipmentManager {

        protected override void CreateEventSubscriptions() {
            //Debug.Log(gameObject.name + ".PlayerEquipmentManager.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized) {
                return;
            }
            if (baseCharacter != null) {
                baseCharacter.OnClassChange += HandleClassChange;
            }
            SystemEventManager.MyInstance.OnPlayerUnitSpawn += HandleCharacterUnitSpawn;
            SystemEventManager.MyInstance.OnPlayerUnitDespawn += HandlePlayerUnitDespawn;
            base.CreateEventSubscriptions();
        }

        protected override void CleanupEventSubscriptions() {
            //Debug.Log("PlayerManager.CleanupEventSubscriptions()");
            if (!eventSubscriptionsInitialized) {
                return;
            }
            if (baseCharacter != null) {
                baseCharacter.OnClassChange -= HandleClassChange;
            }
            SystemEventManager.MyInstance.OnPlayerUnitSpawn -= HandleCharacterUnitSpawn;
            SystemEventManager.MyInstance.OnPlayerUnitDespawn -= HandlePlayerUnitDespawn;
            base.CleanupEventSubscriptions();
        }

        public override void HandleCharacterUnitSpawn() {
            GetComponentReferences();
            base.HandleCharacterUnitSpawn();
        }

        public void HandleClassChange(CharacterClass newClass, CharacterClass oldClass) {
            List<Equipment> equipmentToRemove = new List<Equipment>();
            foreach (Equipment equipment in currentEquipment.Values) {
                if (equipment != null && equipment.CanEquip(baseCharacter) == false) {
                    equipmentToRemove.Add(equipment);
                }
            }
            foreach (Equipment equipment in equipmentToRemove) {
                Unequip(equipment);
            }
        }
        

        public void HandlePlayerUnitDespawn() {
            //Debug.Log("EquipmentManager.OnPlayerUnitDespawn()");
            UnSubscribeFromCombatEvents();
        }

        public override void GetComponentReferences() {
            //Debug.Log(gameObject.name + ".PlayerEquipmentManager.CreateComponentReferences()");
            base.GetComponentReferences();
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
                    //Debug.Log(gameObject.name + ".CharacterEquipmentManager.CreateComponentReferences(): baseCharacter.MyCharacterUnit == null!");
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
            //Debug.Log(gameObject.name + ".PlayerEquipmentManager.EquipCharacter()");
            //public void EquipCharacter(GameObject playerUnitObject = null, bool updateCharacterButton = true) {
            if (currentEquipment == null) {
                //Debug.Log(gameObject.name + ".PlayerEquipmentManager.EquipCharacter(): currentEquipment == null!");
                return;
            }
            base.EquipCharacter();

            // MOVE TO PLAYER MANAGER
            //SystemEventManager.MyInstance.NotifyOnEquipmentRefresh(equipment);
        }

        public override void Equip(Equipment newItem, EquipmentSlotProfile equipmentSlotProfile = null) {
            //Debug.Log(gameObject.name + ".PlayerEquipmentManager.Equip(" + (newItem == null ? "null" : newItem.MyName)+ ", " + (equipmentSlotProfile == null ? "null" : equipmentSlotProfile.MyName) + ")");
            if (newItem == null) {
                Debug.Log("Instructed to Equip a null item!");
                return;
            }
            base.Equip(newItem, equipmentSlotProfile);

            // DO THIS LAST OR YOU WILL SAVE THE UMA DATA BEFORE ANYTHING IS EQUIPPED!
            // updated oldItem to null here because this call is already done in Unequip.
            // having it here also was leading to duplicate stat removal when gear was changed.
            SystemEventManager.MyInstance.NotifyOnEquipmentChanged(newItem, null);
        }

        public override Equipment Unequip(EquipmentSlotProfile equipmentSlotProfile, int slotIndex = -1) {
            //Debug.Log("equipment manager trying to unequip item in slot " + equipmentSlot.ToString());
            Equipment returnValue = base.Unequip(equipmentSlotProfile, slotIndex);
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

}