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
            SystemEventManager.MyInstance.OnPlayerUnitSpawn += HandlePlayerUnitSpawn;
            SystemEventManager.MyInstance.OnPlayerUnitDespawn += HandlePlayerUnitDespawn;
            base.CreateEventSubscriptions();
        }

        protected override void CleanupEventSubscriptions() {
            //Debug.Log("PlayerManager.CleanupEventSubscriptions()");
            if (!eventSubscriptionsInitialized) {
                return;
            }
            SystemEventManager.MyInstance.OnPlayerUnitSpawn -= HandlePlayerUnitSpawn;
            SystemEventManager.MyInstance.OnPlayerUnitDespawn -= HandlePlayerUnitDespawn;
            base.CleanupEventSubscriptions();
        }

        public void HandlePlayerUnitSpawn() {
            //Debug.Log(gameObject.name + ".EquipmentManager.OnPlayerUnitSpawn()");
            CreateComponentReferences();
            EquipCharacter();
            SubscribeToCombatEvents();
        }

        public void HandlePlayerUnitDespawn() {
            //Debug.Log("EquipmentManager.OnPlayerUnitDespawn()");
            UnSubscribeFromCombatEvents();
        }

        public override void CreateComponentReferences() {
            //Debug.Log(gameObject.name + ".PlayerEquipmentManager.CreateComponentReferences()");
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

}