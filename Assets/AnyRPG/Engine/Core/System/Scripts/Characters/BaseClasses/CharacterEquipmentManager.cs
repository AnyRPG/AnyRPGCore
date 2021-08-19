using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class CharacterEquipmentManager {

        public System.Action<Equipment, Equipment, int> OnEquipmentChanged = delegate { };

        // component references
        protected BaseCharacter baseCharacter = null;

        protected Dictionary<EquipmentSlotProfile, Equipment> currentEquipment = new Dictionary<EquipmentSlotProfile, Equipment>();

        // keep track of holdable objects to be used during weapon attacks such as arrows, glowing hand effects, weapon trails, etc
        private List<AbilityAttachmentNode> weaponHoldableObjects = new List<AbilityAttachmentNode>();

        public Dictionary<EquipmentSlotProfile, Equipment> CurrentEquipment { get => currentEquipment; set => currentEquipment = value; }
        public List<AbilityAttachmentNode> WeaponHoldableObjects { get => weaponHoldableObjects; }

        public CharacterEquipmentManager (BaseCharacter baseCharacter) {
            this.baseCharacter = baseCharacter;
        }

        public void HandleCapabilityConsumerChange() {
            List<Equipment> equipmentToRemove = new List<Equipment>();
            foreach (Equipment equipment in currentEquipment.Values) {
                if (equipment != null && equipment.CanEquip(baseCharacter) == false) {
                    equipmentToRemove.Add(equipment);
                }
            }
            foreach (Equipment equipment in equipmentToRemove) {
                Unequip(equipment);
            }

            // since all status effects were cancelled on the change, it is necessary to re-apply set bonuses
            foreach (Equipment equipment in currentEquipment.Values) {
                if (equipment != null) {
                    baseCharacter.CharacterAbilityManager.UpdateEquipmentTraits(equipment);
                }
            }
        }

        public float GetWeaponDamage() {
            float returnValue = 0f;
            foreach (EquipmentSlotProfile equipmentSlotProfile in currentEquipment.Keys) {
                if (currentEquipment[equipmentSlotProfile] != null && currentEquipment[equipmentSlotProfile] is Weapon) {
                    returnValue += (CurrentEquipment[equipmentSlotProfile] as Weapon).GetDamagePerSecond(baseCharacter.CharacterStats.Level);
                }
            }
            return returnValue;
        }

        /// <summary>
        /// meant to be called by SetUnitProfile since it relies on that for the equipment list
        /// </summary>
        public void LoadDefaultEquipment(bool loadProviderEquipment) {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterEquipmentManager.LoadDefaultEquipment(" + loadProviderEquipment + ")");

            if (baseCharacter?.UnitProfile?.EquipmentList == null) {
                return;
            }

            // testing skip this if loadProviderEquipment is set to false to allow new game panel to work
            // since new game panel already sets up the equipment
            // monitor to see if it breaks anything else
            if (loadProviderEquipment == true) {
                // load the unit profile equipment
                foreach (Equipment equipment in baseCharacter.UnitProfile.EquipmentList) {
                    if (equipment != null) {
                        Equip(equipment, null, false, false, false);
                    }
                }
            }

            if (loadProviderEquipment == false || baseCharacter.UnitProfile.UseProviderEquipment == false) {
                return;
            }

            if (baseCharacter.CharacterRace != null) {
                foreach (Equipment equipment in baseCharacter.CharacterRace.EquipmentList) {
                    Equip(equipment, null, false, false, false);
                }
            }

            if (baseCharacter.CharacterClass != null) {
                foreach (Equipment equipment in baseCharacter.CharacterClass.EquipmentList) {
                    Equip(equipment, null, false, false, false);
                }
                if (baseCharacter.ClassSpecialization != null) {
                    foreach (Equipment equipment in baseCharacter.ClassSpecialization.EquipmentList) {
                        Equip(equipment, null, false, false, false);
                    }
                }
            }

            if (baseCharacter.Faction != null) {
                foreach (Equipment equipment in baseCharacter.Faction.EquipmentList) {
                    Equip(equipment, null, false, false, false);
                }
            }

        }

        public void ClearEquipment() {
            currentEquipment = new Dictionary<EquipmentSlotProfile, Equipment>();
        }


        public void UnequipExclusiveSlots(EquipmentSlotType equipmentSlotType) {
            //Debug.Log(gameObject + ".CharacterEquipmentManager.UnequipExclusiveSlots(" + equipmentSlotTypeName + ")");
            if (equipmentSlotType != null) {
                //Debug.Log(gameObject + ".CharacterEquipmentManager.UnequipExclusiveSlots(" + equipmentSlotTypeName + "): found resource");
                if (equipmentSlotType.MyExclusiveSlotProfileList != null && equipmentSlotType.MyExclusiveSlotProfileList.Count > 0) {
                    //Debug.Log(gameObject + ".CharacterEquipmentManager.UnequipExclusiveSlots(" + equipmentSlotTypeName + "): has exclusive slots");
                    foreach (EquipmentSlotProfile equipmentSlotProfile in equipmentSlotType.MyExclusiveSlotProfileList) {
                        //Debug.Log(gameObject + ".CharacterEquipmentManager.UnequipExclusiveSlots(" + equipmentSlotTypeName + "): exclusive slot: " + equipmentSlotProfileName);
                        if (equipmentSlotProfile != null) {
                            Unequip(equipmentSlotProfile);
                        }
                    }
                }
            }
        }

        public List<EquipmentSlotProfile> GetCompatibleSlotProfiles(EquipmentSlotType equipmentSlotType) {
            List<EquipmentSlotProfile> returnValue = new List<EquipmentSlotProfile>();
            if (equipmentSlotType != null) {
                foreach (EquipmentSlotProfile equipmentSlotProfile in SystemDataFactory.Instance.GetResourceList<EquipmentSlotProfile>()) {
                    if (equipmentSlotProfile.MyEquipmentSlotTypeList != null && equipmentSlotProfile.MyEquipmentSlotTypeList.Contains(equipmentSlotType)) {
                        returnValue.Add(equipmentSlotProfile);
                    }
                }
            }

            return returnValue;
        }

        public EquipmentSlotProfile GetFirstEmptySlot(List<EquipmentSlotProfile> slotProfileList) {
            //Debug.Log(gameObject.name + ".CharacterEquipmentManager.GetFirstEmptySlot()");
            foreach (EquipmentSlotProfile slotProfile in slotProfileList) {
                if (slotProfile != null) {
                    if (currentEquipment.ContainsKey(slotProfile) == false || (currentEquipment.ContainsKey(slotProfile) == true && currentEquipment[slotProfile] == null)) {
                        //Debug.Log(gameObject.name + ".CharacterEquipmentManager.GetFirstEmptySlot(): " + equipmentSlotProfile + "; " + equipmentSlotProfile.GetInstanceID());
                        return slotProfile;
                    }
                }
            }
            return null;
        }

        public bool Equip(Equipment newItem, EquipmentSlotProfile equipmentSlotProfile = null, bool equipModels = true, bool setAppearance = true, bool rebuildAppearance = true) {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterEquipmentManager.Equip(" + (newItem != null ? newItem.DisplayName : "null") + ", " + (equipmentSlotProfile == null ? "null" : equipmentSlotProfile.DisplayName) + ", " + equipModels + ", " + setAppearance + ", " + rebuildAppearance + ")");
            //Debug.Break();
            if (newItem == null) {
                Debug.Log("Instructed to Equip a null item!");
                return false;
            }
            //currentEquipment[newItem.equipSlot].MyCharacterButton.DequipEquipment();
            //Unequip(newItem.equipSlot);
            if (newItem.EquipmentSlotType == null) {
                Debug.LogError(baseCharacter.gameObject.name + "CharacterEquipmentManager.Equip() " + newItem.DisplayName + " could not be equipped because it had no equipment slot.  CHECK INSPECTOR.");
                return false;
            }

            if (newItem.CanEquip(baseCharacter) == false) {
                //Debug.Log(baseCharacter.gameObject.name + "CharacterEquipmentManager.Equip(" + (newItem != null ? newItem.DisplayName : "null") + "; could not equip");
                return false;
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
                    Unequip(slotProfileList[0]);
                    emptySlotProfile = GetFirstEmptySlot(slotProfileList);
                }
                if (emptySlotProfile == null) {
                    Debug.LogError(baseCharacter.gameObject.name + "CharacterEquipmentManager.Equip() " + newItem.DisplayName + " emptyslotProfile is null.  CHECK INSPECTOR.");
                    return false;
                }
            }

            // unequip any item in an exclusive slot for this item
            UnequipExclusiveSlots(newItem.EquipmentSlotType);

            //Debug.Log(gameObject.name + ".CharacterEquipmentManager.Equip(): equippping " + newItem.MyName + " in slot: " + emptySlotProfile + "; " + emptySlotProfile.GetInstanceID());
            currentEquipment[emptySlotProfile] = newItem;

            baseCharacter?.UnitController?.UnitModelController.EquipItemModels(this, emptySlotProfile, newItem, equipModels, setAppearance, rebuildAppearance);
           
            // DO THIS LAST OR YOU WILL SAVE THE UMA DATA BEFORE ANYTHING IS EQUIPPED!
            // updated oldItem to null here because this call is already done in Unequip.
            // having it here also was leading to duplicate stat removal when gear was changed.
            //Debug.Log(gameObject.name + ".CharacterEquipmentManager.Equip() FIRING ONEQUIPMENTCHANGED");
            NotifyEquipmentChanged(newItem, null, -1, emptySlotProfile);

            //Debug.Log("CharacterEquipmentManager.Equip(" + (newItem != null ? newItem.DisplayName : "null") + "; successfully equipped");

            return true;
        }

        public void HandleWeaponHoldableObjects(Equipment newItem, Equipment oldItem) {
            //Debug.Log(gameObject.name + ".CharacterAbilityManager.HandleEquipmentChanged(" + (newItem != null ? newItem.MyName : "null") + ", " + (oldItem != null ? oldItem.MyName : "null") + ")");
            if (oldItem != null && (oldItem is Weapon) && (oldItem as Weapon).AbilityObjectList != null && (oldItem as Weapon).AbilityObjectList.Count > 0) {
                foreach (AbilityAttachmentNode abilityAttachmentNode in (oldItem as Weapon).AbilityObjectList) {
                    if (weaponHoldableObjects.Contains(abilityAttachmentNode)) {
                        weaponHoldableObjects.Remove(abilityAttachmentNode);
                    }
                }
            }

            if (newItem != null && (newItem is Weapon) && (newItem as Weapon).AbilityObjectList != null && (newItem as Weapon).AbilityObjectList.Count > 0) {
                foreach (AbilityAttachmentNode abilityAttachmentNode in (newItem as Weapon).AbilityObjectList) {
                    if (!weaponHoldableObjects.Contains(abilityAttachmentNode)) {
                        weaponHoldableObjects.Add(abilityAttachmentNode);
                    }
                }
            }
        }

        public void NotifyEquipmentChanged(Equipment newItem, Equipment oldItem, int slotIndex, EquipmentSlotProfile equipmentSlotProfile) {
            HandleWeaponHoldableObjects(newItem, oldItem);
            //OnEquipmentChanged(newItem, oldItem, slotIndex);
            baseCharacter.CharacterStats.HandleEquipmentChanged(newItem, oldItem, slotIndex);
            baseCharacter.CharacterCombat.HandleEquipmentChanged(newItem, oldItem, slotIndex, equipmentSlotProfile);
            baseCharacter.CharacterAbilityManager.HandleEquipmentChanged(newItem, oldItem, slotIndex);
            if (baseCharacter.UnitController != null) {
                baseCharacter.UnitController.UnitAnimator.HandleEquipmentChanged(newItem, oldItem, slotIndex);
            }

            // now that all stats have been recalculated, it's safe to fire this event, so things that listen will show the correct values
            OnEquipmentChanged(newItem, oldItem, slotIndex);
        }

        public int GetEquipmentSetCount(EquipmentSet equipmentSet) {
            int equipmentCount = 0;

            if (equipmentSet != null) {
                foreach (Equipment tmpEquipment in CurrentEquipment.Values) {
                    if (tmpEquipment != null && tmpEquipment.EquipmentSet != null && tmpEquipment.EquipmentSet == equipmentSet) {
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
                if (currentEquipment[equipmentSlotProfile] == equipment) {
                    return equipmentSlotProfile;
                }
            }
            return null;
        }

        public Equipment Unequip(Equipment equipment, bool unequipModels = true, bool unequipAppearance = true, bool rebuildAppearance = true) {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterEquipmentManager.Unequip(" + (equipment == null ? "null" : equipment.DisplayName) + ", " + unequipModels + ", " + unequipAppearance + ", " + rebuildAppearance + ")");
            foreach (EquipmentSlotProfile equipmentSlotProfile in currentEquipment.Keys) {
                if (currentEquipment[equipmentSlotProfile] != null && currentEquipment[equipmentSlotProfile] == equipment) {
                    return Unequip(equipmentSlotProfile, -1, unequipModels, unequipAppearance, rebuildAppearance);
                }
            }
            return null;
        }

        /*
        public Equipment Unequip(EquipmentSlotProfile equipmentSlotProfile) {
            Debug.Log(gameObject.name + ".CharacterEquipmentManager.Unequip(" + equipmentSlotProfileName + ")");

            if (equipmentSlotProfile != null) {
                return Unequip(equipmentSlotProfile);
            }
            return null;
        }
        */

       

        public Equipment Unequip(EquipmentSlotProfile equipmentSlot, int slotIndex = -1, bool unequipModels = true, bool unequipAppearance = true, bool rebuildAppearance = true) {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterEquipmentManager.Unequip(" + equipmentSlot.ToString() + ", " + slotIndex + ", " + unequipModels + ", " + unequipAppearance + ", " + rebuildAppearance + ")");
            if (currentEquipment.ContainsKey(equipmentSlot) && currentEquipment[equipmentSlot] != null) {
                //Debug.Log("equipment manager trying to unequip item in slot " + equipmentSlot.ToString() + "; currentEquipment has this slot key");

                baseCharacter?.UnitController?.UnitModelController?.UnequipItemModels(equipmentSlot, currentEquipment[equipmentSlot], unequipModels, unequipAppearance, rebuildAppearance);
                
                Equipment oldItem = currentEquipment[equipmentSlot];

                //Debug.Log("zeroing equipment slot: " + equipmentSlot.ToString());
                currentEquipment[equipmentSlot] = null;
                NotifyEquipmentChanged(null, oldItem, slotIndex, equipmentSlot);
                return oldItem;
            }
            return null;
        }

        public void UnequipAll(bool rebuildUMA = true) {
            //Debug.Log("EquipmentManager.UnequipAll()");
            List<EquipmentSlotProfile> tmpList = new List<EquipmentSlotProfile>();
            foreach (EquipmentSlotProfile equipmentSlotProfile in currentEquipment.Keys) {
                tmpList.Add(equipmentSlotProfile);
            }

            foreach (EquipmentSlotProfile equipmentSlotProfile in tmpList) {
                Unequip(equipmentSlotProfile, -1, true, true, rebuildUMA);
            }

            /*
            for (int i = 0; i < currentEquipment.Count; i++) {
                Unequip(currentEquipment[i].);
            }
            */
        }

        public bool HasAffinity(WeaponSkill weaponAffinity) {
            //Debug.Log("EquipmentManager.HasAffinity(" + weaponAffinity.ToString() + ")");
            int weaponCount = 0;
            foreach (Equipment equipment in currentEquipment.Values) {
                if (equipment is Weapon) {
                    weaponCount++;
                    if (weaponAffinity == (equipment as Weapon).WeaponSkill) {
                        return true;
                    }
                }
            }
            if (weaponCount == 0) {
                // there are no weapons equipped
                // check if the character class is set and contains a weapon skill that is considered to be active when no weapon is equipped
                if (weaponAffinity.WeaponSkillProps.DefaultWeaponSkill && baseCharacter.CapabilityConsumerProcessor.IsWeaponSkillSupported(weaponAffinity)) {
                    return true;
                }
            }
            return false;
        }

        /*
        void SetEquipmentBlendShapes(Equipment item, int weight) {
            foreach (EquipmentMeshRegion blendShape in item.coveredMeshRegions) {
                targetMesh.SetBlendShapeWeight((int)blendShape, weight);
            }
        }
        */

        public bool HasEquipment(string equipmentName) {
            foreach (Equipment equipment in currentEquipment.Values) {
                if (equipment != null) {
                    if (SystemDataFactory.MatchResource(equipment.DisplayName, equipmentName)) {
                        return true;
                    }
                }
            }
            return false;
        }

        public int GetEquipmentCount(string equipmentName) {
            int returnValue = 0;
            foreach (Equipment equipment in currentEquipment.Values) {
                if (equipment != null) {
                    if (SystemDataFactory.MatchResource(equipment.DisplayName, equipmentName)) {
                        returnValue++;
                    }
                }
            }
            return returnValue;
        }


    }

}