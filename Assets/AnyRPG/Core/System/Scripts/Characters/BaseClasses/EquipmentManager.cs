using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {

    /// <summary>
    /// This class handles the equipment list and slot logic.  It is not aware of character level restrictions such as weapon affinity
    /// This allows it to be used as an independent equipment list that is not attached to a character that can properly deal with one-hand weapons
    /// </summary>
    public class EquipmentManager : ConfiguredClass {

        //protected Dictionary<EquipmentSlotProfile, InstantiatedEquipment> currentEquipment = new Dictionary<EquipmentSlotProfile, InstantiatedEquipment>();
        protected Dictionary<EquipmentSlotProfile, EquipmentInventorySlot> currentEquipment = new Dictionary<EquipmentSlotProfile, EquipmentInventorySlot>();
        protected Dictionary<EquipmentInventorySlot, EquipmentSlotProfile> currentEquipmentLookup = new Dictionary<EquipmentInventorySlot, EquipmentSlotProfile>();

        //public Dictionary<EquipmentSlotProfile, InstantiatedEquipment> CurrentEquipment { get => currentEquipment; set => currentEquipment = value; }
        public Dictionary<EquipmentSlotProfile, EquipmentInventorySlot> CurrentEquipment { get => currentEquipment; set => currentEquipment = value; }

        public EquipmentManager (SystemGameManager systemGameManager) {
            Configure(systemGameManager);
            foreach (EquipmentSlotProfile equipmentSlotProfile in systemDataFactory.GetResourceList<EquipmentSlotProfile>()) {
                EquipmentInventorySlot tmpSlot = new EquipmentInventorySlot(systemGameManager);
                AddCurrentEquipmentSlot(equipmentSlotProfile, tmpSlot);
            }
        }

        public virtual void AddCurrentEquipmentSlot(EquipmentSlotProfile equipmentSlotProfile, EquipmentInventorySlot equipmentInventorySlot) {
            //Debug.Log($"EquipmentManager.AddCurrentEquipmentSlot({equipmentSlotProfile.DisplayName}, {equipmentInventorySlot})");

            if (currentEquipment.ContainsKey(equipmentSlotProfile)) {
                currentEquipment[equipmentSlotProfile] = equipmentInventorySlot;
            } else {
                currentEquipment.Add(equipmentSlotProfile, equipmentInventorySlot);
            }
            if (currentEquipmentLookup.ContainsKey(equipmentInventorySlot)) {
                currentEquipmentLookup[equipmentInventorySlot] = equipmentSlotProfile;
            } else {
                currentEquipmentLookup.Add(equipmentInventorySlot, equipmentSlotProfile);
            }
        }

        public void ClearCurrentEquipment() {
            currentEquipment.Clear();
            currentEquipmentLookup.Clear();
        }

        public List<EquipmentSlotProfile> GetExclusiveSlotList(EquipmentSlotType equipmentSlotType) {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterEquipmentManager.UnequipExclusiveSlots(" + equipmentSlotType.DisplayName + ")");

            List<EquipmentSlotProfile> exclusiveSlotList = new List<EquipmentSlotProfile>();

            if (equipmentSlotType != null) {
                // unequip exclusive slot profiles
                //Debug.Log(baseCharacter.gameObject.name + ".CharacterEquipmentManager.UnequipExclusiveSlots(" + equipmentSlotType.DisplayName + "): found resource");
                if (equipmentSlotType.ExclusiveSlotProfileList != null && equipmentSlotType.ExclusiveSlotProfileList.Count > 0) {
                    //Debug.Log(baseCharacter.gameObject.name + ".CharacterEquipmentManager.UnequipExclusiveSlots(" + equipmentSlotType.DisplayName + "): has exclusive slots");
                    foreach (EquipmentSlotProfile equipmentSlotProfile in equipmentSlotType.ExclusiveSlotProfileList) {
                        //Debug.Log(baseCharacter.gameObject.name + ".CharacterEquipmentManager.UnequipExclusiveSlots(" + equipmentSlotType.DisplayName + "): exclusive slot: " + equipmentSlotProfile.DisplayName);
                        if (equipmentSlotProfile != null) {
                            exclusiveSlotList.Add(equipmentSlotProfile);
                        }
                    }
                }

                // unequip exclusive slot types
                if (equipmentSlotType.ExclusiveSlotTypeList != null && equipmentSlotType.ExclusiveSlotTypeList.Count > 0) {
                    //Debug.Log(baseCharacter.gameObject.name + ".CharacterEquipmentManager.UnequipExclusiveSlots(" + equipmentSlotType.DisplayName + "): has exclusive slot types");
                    foreach (EquipmentSlotType exclusiveSlotType in equipmentSlotType.ExclusiveSlotTypeList) {
                        //Debug.Log(baseCharacter.gameObject.name + ".CharacterEquipmentManager.UnequipExclusiveSlots(" + equipmentSlotType.DisplayName + "): exclusive slot type: " + exclusiveSlotType);
                        foreach (EquipmentSlotProfile equipmentSlotProfile in currentEquipment.Keys) {
                            if (currentEquipment[equipmentSlotProfile].InstantiatedEquipment != null
                                && currentEquipment[equipmentSlotProfile].InstantiatedEquipment.Equipment.EquipmentSlotType == exclusiveSlotType) {
                                exclusiveSlotList.Add(equipmentSlotProfile);
                            }
                        }
                    }
                }
            }

            return exclusiveSlotList;
        }

        public List<EquipmentSlotProfile> GetCompatibleSlotProfiles(EquipmentSlotType equipmentSlotType) {
            //Debug.Log($"EquipmentManager.GetCompatibleSlotProfiles({equipmentSlotType.ResourceName})");

            List<EquipmentSlotProfile> returnValue = new List<EquipmentSlotProfile>();
            if (equipmentSlotType != null) {
                foreach (EquipmentSlotProfile equipmentSlotProfile in currentEquipment.Keys) {
                    if (equipmentSlotProfile.EquipmentSlotTypeList != null && equipmentSlotProfile.EquipmentSlotTypeList.Contains(equipmentSlotType)) {
                        //Debug.Log($"EquipmentManager.GetCompatibleSlotProfiles({equipmentSlotType.ResourceName}): found compatible slot: {equipmentSlotProfile.DisplayName}");
                        returnValue.Add(equipmentSlotProfile);
                    }
                }
            }

            return returnValue;
        }

        public EquipmentSlotProfile GetFirstEmptySlot(List<EquipmentSlotProfile> slotProfileList) {
            //Debug.Log($"EquipmentManager.GetFirstEmptySlot({slotProfileList.Count})");

            foreach (EquipmentSlotProfile slotProfile in slotProfileList) {
                if (slotProfile != null) {
                    if (currentEquipment[slotProfile].InstantiatedEquipment == null) {
                        //Debug.Log($"EquipmentManager.GetFirstEmptySlot({slotProfileList.Count}): found empty slot: {slotProfile.DisplayName}");
                        return slotProfile;
                    } else {
                        //Debug.Log($"EquipmentManager.GetFirstEmptySlot({slotProfileList.Count}): slot not empty: {slotProfile.DisplayName} equipment: {currentEquipment[slotProfile].InstantiatedEquipment.ResourceName}");
                    }
                }
            }
            return null;
        }

        /*
        public EquipmentSlotProfile GetConflictingSlot(EquipmentSlotProfile equipmentSlotProfile, List<EquipmentSlotProfile> slotProfileList) {
            
            // check if any are empty.  if not, return the first compatible one from the list provided
            EquipmentSlotProfile emptySlotProfile = equipmentSlotProfile;
            if (emptySlotProfile == null) {
                emptySlotProfile = GetFirstEmptySlot(slotProfileList);
            }

            if (emptySlotProfile == null) {
                if (slotProfileList != null && slotProfileList.Count > 0) {
                    return slotProfileList[0];
                }
            }

            return null;
        }
        */

        public virtual EquipmentSlotProfile EquipEquipment(InstantiatedEquipment newItem, EquipmentSlotProfile equipmentSlotProfile = null) {
            //Debug.Log($"EquipmentManager.EquipEquipment({newItem.ResourceName}, {(equipmentSlotProfile == null ? "null" : equipmentSlotProfile.DisplayName)}) (instance: {GetHashCode()})");

            // unequip any item in an exclusive slot for this item
            List<EquipmentSlotProfile> exclusiveSlotList = GetExclusiveSlotList(newItem.Equipment.EquipmentSlotType);
            foreach (EquipmentSlotProfile removeSlotProfile in exclusiveSlotList) {
                UnequipEquipment(removeSlotProfile);
            }

            // get list of compatible slots that can take this slot type
            List<EquipmentSlotProfile> slotProfileList = GetCompatibleSlotProfiles(newItem.Equipment.EquipmentSlotType);
            
            // check if any are empty.  if not, unequip the first one
            EquipmentSlotProfile emptySlotProfile = equipmentSlotProfile;
            if (emptySlotProfile == null) {
                emptySlotProfile = GetFirstEmptySlot(slotProfileList);
            }

            if (emptySlotProfile == null) {
                //Debug.Log($"EquipmentManager.EquipEquipment({newItem.ResourceName}, {equipmentSlotProfile?.DisplayName}): no empty slots found, unequipping first slot");
                if (slotProfileList != null && slotProfileList.Count > 0) {
                    UnequipEquipment(slotProfileList[0]);
                    emptySlotProfile = GetFirstEmptySlot(slotProfileList);
                }
            }

            if (emptySlotProfile != null) {
                EquipToList(newItem, emptySlotProfile);
            }
            //Debug.Log("EquipmentManager.EquipEquipment(): equippping " + newItem.DisplayName + " in slot: " + emptySlotProfile.DisplayName + "; " + emptySlotProfile.GetInstanceID());
            
            return emptySlotProfile;
        }

        public void EquipToList(InstantiatedEquipment equipment, EquipmentSlotProfile equipmentSlotProfile) {
            //Debug.Log($"EquipmentManager.EquipToList({equipment.ResourceName}, {equipmentSlotProfile.DisplayName})");

            currentEquipment[equipmentSlotProfile].AddItem(equipment);
        }


        public virtual void UnequipEquipment(EquipmentSlotProfile equipmentSlotProfile) {
            //Debug.Log($"EquipmentManager.UnequipEquipment({equipmentSlotProfile.DisplayName}) (instance: {GetHashCode()})");

            UnequipFromList(equipmentSlotProfile);
        }

        public virtual InstantiatedEquipment UnequipFromList(EquipmentSlotProfile equipmentSlotProfile) {
            //Debug.Log($"EquipmentManager.UnequipFromList({equipmentSlotProfile.ResourceName}) (instance: {GetHashCode()})");

            if (currentEquipment[equipmentSlotProfile].InstantiatedEquipment != null) {
                InstantiatedEquipment oldItem = currentEquipment[equipmentSlotProfile].InstantiatedEquipment;

                //Debug.Log("zeroing equipment slot: " + equipmentSlot.ToString());
                currentEquipment[equipmentSlotProfile].RemoveItem(oldItem);

                return oldItem;
            }

            return null;
        }

        public int GetEquipmentSetCount(EquipmentSet equipmentSet) {
            int equipmentCount = 0;

            if (equipmentSet != null) {
                foreach (EquipmentInventorySlot tmpEquipmentInventorySlot in currentEquipment.Values) {
                    if (tmpEquipmentInventorySlot.InstantiatedEquipment != null
                        && tmpEquipmentInventorySlot.InstantiatedEquipment.Equipment.EquipmentSet != null
                        && tmpEquipmentInventorySlot.InstantiatedEquipment.Equipment.EquipmentSet == equipmentSet) {
                        equipmentCount++;
                    }
                }
            }

            return equipmentCount;
        }

        /// <summary>
        /// return the equipment slot that a piece of equipment is currently equipped in, or null if not equipped
        /// </summary>
        /// <param name="instantiatedEquipment"></param>
        /// <returns></returns>
        public EquipmentSlotProfile FindEquipmentSlotForEquipment(InstantiatedEquipment instantiatedEquipment) {
            foreach (EquipmentSlotProfile equipmentSlotProfile in currentEquipment.Keys) {
                if (currentEquipment[equipmentSlotProfile].InstantiatedEquipment != null
                    && currentEquipment[equipmentSlotProfile].InstantiatedEquipment == instantiatedEquipment) {
                    return equipmentSlotProfile;
                }
            }
            return null;
        }

        public bool HasEquipment(string equipmentName, bool partialMatch = false) {
            foreach (EquipmentInventorySlot equipmentInventorySlot in currentEquipment.Values) {
                if (equipmentInventorySlot.InstantiatedEquipment != null) {
                    if (SystemDataUtility.MatchResource(equipmentInventorySlot.InstantiatedEquipment.Equipment.ResourceName, equipmentName, partialMatch)) {
                        return true;
                    }
                }
            }
            return false;
        }

        public int GetEquipmentCount(string equipmentName, bool partialMatch = false) {
            int returnValue = 0;
            foreach (EquipmentInventorySlot equipmentInventorySlot in currentEquipment.Values) {
                if (equipmentInventorySlot.InstantiatedEquipment != null) {
                    if (SystemDataUtility.MatchResource(equipmentInventorySlot.InstantiatedEquipment.Equipment.ResourceName, equipmentName, partialMatch)) {
                        returnValue++;
                    }
                }
            }
            return returnValue;
        }

        public void ClearEquipmentList() {
            //Debug.Log($"EquipmentManager.ClearEquipmentList()");

            foreach (EquipmentInventorySlot equipmentInventorySlot in currentEquipment.Values) {
                equipmentInventorySlot.Clear();
            }
        }

    }

}