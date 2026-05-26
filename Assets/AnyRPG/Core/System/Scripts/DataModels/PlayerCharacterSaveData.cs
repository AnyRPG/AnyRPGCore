using System;
using UnityEngine;

namespace AnyRPG {
    
    [Serializable]
    public class PlayerCharacterSaveData {
        
        public CharacterSaveData CharacterSaveData = new CharacterSaveData();
        public ItemInstanceListSaveData ItemInstanceListSaveData = new ItemInstanceListSaveData();

        public PlayerCharacterSaveData() { }

        public PlayerCharacterSaveData(CharacterSaveData characterSaveData, SystemItemManager systemItemManager) { 
            CharacterSaveData = characterSaveData;
            foreach (InventorySlotSaveData inventorySlotSaveData in characterSaveData.InventorySlotSaveData) {
                foreach (long itemInstanceId in inventorySlotSaveData.ItemInstanceIds) {
                    InstantiatedItem instantiatedItem = systemItemManager.GetExistingInstantiatedItem(itemInstanceId);
                    if (instantiatedItem == null) {
                        Debug.LogWarning($"PlayerCharacterSaveData.PlayerCharacterSaveData(): Could not find instantiated item with id {itemInstanceId} in inventory for character {characterSaveData.CharacterName}");
                        continue;
                    }
                    ItemInstanceListSaveData.ItemInstances.Add(instantiatedItem.GetItemSaveData());
                }
            }
            foreach (InventorySlotSaveData inventorySlotSaveData in characterSaveData.BankSlotSaveData) {
                foreach (long itemInstanceId in inventorySlotSaveData.ItemInstanceIds) {
                    InstantiatedItem instantiatedItem = systemItemManager.GetExistingInstantiatedItem(itemInstanceId);
                    if (instantiatedItem == null) {
                        Debug.LogWarning($"PlayerCharacterSaveData.PlayerCharacterSaveData(): Could not find instantiated item with id {itemInstanceId} in bank for character {characterSaveData.CharacterName}");
                        continue;
                    }
                    ItemInstanceListSaveData.ItemInstances.Add(instantiatedItem.GetItemSaveData());
                }
            }
            foreach (EquipmentInventorySlotSaveData equipmentInventorySlotSaveData in characterSaveData.EquipmentSaveData) {
                //Debug.Log($"PlayerCharacterSaveData.Constructor() equipmentId: {equipmentSaveData.ItemInstanceId}");
                if (equipmentInventorySlotSaveData.HasItem == false) {
                    continue;
                }
                InstantiatedItem instantiatedItem = systemItemManager.GetExistingInstantiatedItem(equipmentInventorySlotSaveData.ItemInstanceId);
                if (instantiatedItem == null) {
                    Debug.LogWarning($"PlayerCharacterSaveData.PlayerCharacterSaveData(): Could not find instantiated item with id {equipmentInventorySlotSaveData.ItemInstanceId} in equipment for character {characterSaveData.CharacterName}");
                    continue;
                }
                ItemInstanceListSaveData.ItemInstances.Add(instantiatedItem.GetItemSaveData());
            }
            foreach (EquippedBagSaveData equippedBagSaveData in characterSaveData.EquippedBagSaveData) {
                if (equippedBagSaveData.HasItem == false) {
                    continue;
                }
                InstantiatedItem instantiatedItem = systemItemManager.GetExistingInstantiatedItem(equippedBagSaveData.ItemInstanceId);
                if (instantiatedItem == null) {
                    Debug.LogWarning($"PlayerCharacterSaveData.PlayerCharacterSaveData(): Could not find instantiated item with id {equippedBagSaveData.ItemInstanceId} in equipped bags for character {characterSaveData.CharacterName}");
                    continue;
                }
                ItemInstanceListSaveData.ItemInstances.Add(instantiatedItem.GetItemSaveData());
            }
            foreach (EquippedBagSaveData equippedBagSaveData in characterSaveData.EquippedBankBagSaveData) {
                if (equippedBagSaveData.HasItem == false) {
                    continue;
                }
                InstantiatedItem instantiatedItem = systemItemManager.GetExistingInstantiatedItem(equippedBagSaveData.ItemInstanceId);
                if (instantiatedItem == null) {
                    Debug.LogWarning($"PlayerCharacterSaveData.PlayerCharacterSaveData(): Could not find instantiated item with id {equippedBagSaveData.ItemInstanceId} in equipped bank bags for character {characterSaveData.CharacterName}");
                    continue;
                }
                ItemInstanceListSaveData.ItemInstances.Add(instantiatedItem.GetItemSaveData());
            }
        }

    }

}