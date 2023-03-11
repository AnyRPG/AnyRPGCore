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

        protected Dictionary<EquipmentSlotProfile, Equipment> currentEquipment = new Dictionary<EquipmentSlotProfile, Equipment>();
        
        public Dictionary<EquipmentSlotProfile, Equipment> CurrentEquipment { get => currentEquipment; set => currentEquipment = value; }
        
        public EquipmentManager (SystemGameManager systemGameManager) {
            Configure(systemGameManager);
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
                            if (currentEquipment[equipmentSlotProfile] != null && currentEquipment[equipmentSlotProfile].EquipmentSlotType == exclusiveSlotType) {
                                exclusiveSlotList.Add(equipmentSlotProfile);
                            }
                        }
                    }
                }
            }

            return exclusiveSlotList;
        }

        public List<EquipmentSlotProfile> GetCompatibleSlotProfiles(EquipmentSlotType equipmentSlotType) {
            List<EquipmentSlotProfile> returnValue = new List<EquipmentSlotProfile>();
            if (equipmentSlotType != null) {
                foreach (EquipmentSlotProfile equipmentSlotProfile in systemDataFactory.GetResourceList<EquipmentSlotProfile>()) {
                    if (equipmentSlotProfile.EquipmentSlotTypeList != null && equipmentSlotProfile.EquipmentSlotTypeList.Contains(equipmentSlotType)) {
                        returnValue.Add(equipmentSlotProfile);
                    }
                }
            }

            return returnValue;
        }

        public EquipmentSlotProfile GetFirstEmptySlot(List<EquipmentSlotProfile> slotProfileList) {
            //Debug.Log("EquipmentManager.GetFirstEmptySlot()");
            foreach (EquipmentSlotProfile slotProfile in slotProfileList) {
                if (slotProfile != null) {
                    if (currentEquipment.ContainsKey(slotProfile) == false || (currentEquipment.ContainsKey(slotProfile) == true && currentEquipment[slotProfile] == null)) {
                        //Debug.Log("EquipmentManager.GetFirstEmptySlot(): " + slotProfile);
                        return slotProfile;
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

        public EquipmentSlotProfile EquipEquipment(Equipment newItem, EquipmentSlotProfile equipmentSlotProfile = null) {
            //Debug.Log("EquipmentManager.EquipEquipment(" + newItem.DisplayName + ", " + (equipmentSlotProfile == null ? "null" : equipmentSlotProfile.DisplayName) + ")");

            // unequip any item in an exclusive slot for this item
            List<EquipmentSlotProfile> exclusiveSlotList = GetExclusiveSlotList(newItem.EquipmentSlotType);
            foreach (EquipmentSlotProfile removeSlotProfile in exclusiveSlotList) {
                UnequipEquipment(removeSlotProfile);
            }

            // get list of compatible slots that can take this slot type
            List<EquipmentSlotProfile> slotProfileList = GetCompatibleSlotProfiles(newItem.EquipmentSlotType);
            
            // check if any are empty.  if not, unequip the first one
            EquipmentSlotProfile emptySlotProfile = equipmentSlotProfile;
            if (emptySlotProfile == null) {
                emptySlotProfile = GetFirstEmptySlot(slotProfileList);
            }

            if (emptySlotProfile == null) {
                if (slotProfileList != null && slotProfileList.Count > 0) {
                    UnequipEquipment(slotProfileList[0]);
                    emptySlotProfile = GetFirstEmptySlot(slotProfileList);
                }
            }

            if (emptySlotProfile != null) {
                EquipToList(newItem, emptySlotProfile);
            }
            //Debug.Log(gameObject.name + ".CharacterEquipmentManager.Equip(): equippping " + newItem.DisplayName + " in slot: " + emptySlotProfile + "; " + emptySlotProfile.GetInstanceID());
            
            return emptySlotProfile;
        }

        public void EquipToList(Equipment equipment, EquipmentSlotProfile equipmentSlotProfile) {
            currentEquipment[equipmentSlotProfile] = equipment;
        }


        public virtual void UnequipEquipment(EquipmentSlotProfile equipmentSlotProfile) {
            UnequipFromList(equipmentSlotProfile);
        }

        public Equipment UnequipFromList(EquipmentSlotProfile equipmentSlotProfile) {
            //Debug.Log("EquipmentManager.UnequipFromList(" + equipmentSlotProfile.DisplayName + ")");

            if (currentEquipment.ContainsKey(equipmentSlotProfile) && currentEquipment[equipmentSlotProfile] != null) {
                Equipment oldItem = currentEquipment[equipmentSlotProfile];

                //Debug.Log("zeroing equipment slot: " + equipmentSlot.ToString());
                currentEquipment[equipmentSlotProfile] = null;

                return oldItem;
            }

            return null;
        }

        public int GetEquipmentSetCount(EquipmentSet equipmentSet) {
            int equipmentCount = 0;

            if (equipmentSet != null) {
                foreach (Equipment tmpEquipment in currentEquipment.Values) {
                    if (tmpEquipment?.EquipmentSet != null && tmpEquipment.EquipmentSet == equipmentSet) {
                        equipmentCount++;
                    }
                }
            }

            return equipmentCount;
        }

        /// <summary>
        /// return the equipment slot that a piece of equipment is currently equipped in, or null if not equipped
        /// </summary>
        /// <param name="equipment"></param>
        /// <returns></returns>
        public EquipmentSlotProfile FindEquipmentSlotForEquipment(Equipment equipment) {
            foreach (EquipmentSlotProfile equipmentSlotProfile in currentEquipment.Keys) {
                if (currentEquipment[equipmentSlotProfile] != null && currentEquipment[equipmentSlotProfile] == equipment) {
                    return equipmentSlotProfile;
                }
            }
            return null;
        }

        public bool HasEquipment(string equipmentName, bool partialMatch = false) {
            foreach (Equipment equipment in currentEquipment.Values) {
                if (equipment != null) {
                    if (SystemDataFactory.MatchResource(equipment.DisplayName, equipmentName, partialMatch)) {
                        return true;
                    }
                }
            }
            return false;
        }

        public int GetEquipmentCount(string equipmentName, bool partialMatch = false) {
            int returnValue = 0;
            foreach (Equipment equipment in currentEquipment.Values) {
                if (equipment != null) {
                    if (SystemDataFactory.MatchResource(equipment.DisplayName, equipmentName, partialMatch)) {
                        returnValue++;
                    }
                }
            }
            return returnValue;
        }

        public void ClearEquipmentList() {
            currentEquipment.Clear();
        }

    }

}