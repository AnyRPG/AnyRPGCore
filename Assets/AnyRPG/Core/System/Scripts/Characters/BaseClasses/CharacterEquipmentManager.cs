using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class CharacterEquipmentManager : EquipmentManager {

        public System.Action<Equipment, Equipment, int> OnEquipmentChanged = delegate { };

        // component references
        protected BaseCharacter baseCharacter = null;

        //protected EquipmentManager equipmentManager = null;

        // keep track of holdable objects to be used during weapon attack animations such as arrows, glowing hand effects, weapon trails, etc
        private List<AbilityAttachmentNode> weaponAbilityAnimationObjects = new List<AbilityAttachmentNode>();

        // keep track of holdable objects to be used during weapon attacks such as arrows, glowing hand effects, weapon trails, etc
        private List<AbilityAttachmentNode> weaponAbilityObjects = new List<AbilityAttachmentNode>();


        //public Dictionary<EquipmentSlotProfile, Equipment> CurrentEquipment { get => equipmentManager.CurrentEquipment; set => equipmentManager.CurrentEquipment = value; }
        public List<AbilityAttachmentNode> WeaponAbilityAnimationObjects { get => weaponAbilityAnimationObjects; }
        public List<AbilityAttachmentNode> WeaponAbilityObjects { get => weaponAbilityObjects; }

        public CharacterEquipmentManager (BaseCharacter baseCharacter, SystemGameManager systemGameManager) : base(systemGameManager) {
            this.baseCharacter = baseCharacter;
            //Configure(systemGameManager);

            //equipmentManager = new EquipmentManager(systemGameManager);
        }

        public void HandleCapabilityConsumerChange() {
            List<Equipment> equipmentToRemove = new List<Equipment>();
            foreach (Equipment equipment in CurrentEquipment.Values) {
                if (equipment != null && equipment.CanEquip(baseCharacter) == false) {
                    equipmentToRemove.Add(equipment);
                }
            }
            foreach (Equipment equipment in equipmentToRemove) {
                Unequip(equipment);
            }

            // since all status effects were cancelled on the change, it is necessary to re-apply set bonuses
            foreach (Equipment equipment in CurrentEquipment.Values) {
                if (equipment != null) {
                    baseCharacter.CharacterAbilityManager.UpdateEquipmentTraits(equipment);
                }
            }
        }

        public float GetWeaponDamage() {
            float returnValue = 0f;
            foreach (EquipmentSlotProfile equipmentSlotProfile in CurrentEquipment.Keys) {
                if (CurrentEquipment[equipmentSlotProfile] != null && CurrentEquipment[equipmentSlotProfile] is Weapon) {
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

            // load the unit profile equipment
            foreach (Equipment equipment in baseCharacter.UnitProfile.EquipmentList) {
                if (equipment != null) {
                    Equip(equipment, null);
                }
            }

            if (loadProviderEquipment == false || baseCharacter.UnitProfile.UseProviderEquipment == false) {
                return;
            }

            if (baseCharacter.Faction != null) {
                foreach (Equipment equipment in baseCharacter.Faction.EquipmentList) {
                    Equip(equipment, null);
                }
            }

            if (baseCharacter.CharacterRace != null) {
                foreach (Equipment equipment in baseCharacter.CharacterRace.EquipmentList) {
                    Equip(equipment, null);
                }
            }

            if (baseCharacter.CharacterClass != null) {
                foreach (Equipment equipment in baseCharacter.CharacterClass.EquipmentList) {
                    Equip(equipment, null);
                }
                if (baseCharacter.ClassSpecialization != null) {
                    foreach (Equipment equipment in baseCharacter.ClassSpecialization.EquipmentList) {
                        Equip(equipment, null);
                    }
                }
            }

        }

        public bool Equip(Equipment newItem, EquipmentSlotProfile equipmentSlotProfile = null) {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterEquipmentManager.Equip(" + (newItem != null ? newItem.DisplayName : "null") + ", " + (equipmentSlotProfile == null ? "null" : equipmentSlotProfile.DisplayName) + ")");

            if (newItem == null) {
                Debug.Log("Instructed to Equip a null item!");
                return false;
            }

            if (newItem.EquipmentSlotType == null) {
                Debug.LogError(baseCharacter.gameObject.name + "CharacterEquipmentManager.Equip() " + newItem.DisplayName + " could not be equipped because it had no equipment slot.  CHECK INSPECTOR.");
                return false;
            }

            if (newItem.CanEquip(baseCharacter) == false) {
                //Debug.Log(baseCharacter.gameObject.name + "CharacterEquipmentManager.Equip(" + (newItem != null ? newItem.DisplayName : "null") + "; could not equip");
                return false;
            }

            
            equipmentSlotProfile = base.EquipEquipment(newItem, equipmentSlotProfile);
            
            if (equipmentSlotProfile == null) {
                Debug.LogError(baseCharacter.gameObject.name + "CharacterEquipmentManager.Equip() " + newItem.DisplayName + " equipmentSlotProfile is null.  CHECK INSPECTOR.");
                return false;
            }

            // DO THIS LAST OR YOU WILL SAVE THE UMA DATA BEFORE ANYTHING IS EQUIPPED!
            NotifyEquipmentChanged(newItem, null, -1, equipmentSlotProfile);

            //Debug.Log("CharacterEquipmentManager.Equip(" + (newItem != null ? newItem.DisplayName : "null") + "; successfully equipped");

            return true;
        }

        public override void UnequipEquipment(EquipmentSlotProfile equipmentSlotProfile) {
            // intentionally not calling base
            // this override exists to intercept a list-only update and perform more character level log
            //base.UnequipFromList(equipmentSlotProfile);
            Unequip(equipmentSlotProfile);
        }

        public void RemoveWeaponAbilityAnimationObjects(AbilityAttachmentNode abilityAttachmentNode) {
            // animation phase objects
            if (weaponAbilityAnimationObjects.Contains(abilityAttachmentNode)) {
                weaponAbilityAnimationObjects.Remove(abilityAttachmentNode);
            }
        }

        public void RemoveWeaponAbilityObjects(AbilityAttachmentNode abilityAttachmentNode) {
            // attack phase objects
            if (weaponAbilityObjects.Contains(abilityAttachmentNode)) {
                weaponAbilityObjects.Remove(abilityAttachmentNode);
            }
        }

        public void AddWeaponAbilityAnimationObjects(AbilityAttachmentNode abilityAttachmentNode) {
            // animation phase objects
            if (!weaponAbilityAnimationObjects.Contains(abilityAttachmentNode)) {
                weaponAbilityAnimationObjects.Add(abilityAttachmentNode);
            }
        }

        public void AddWeaponAbilityObjects(AbilityAttachmentNode abilityAttachmentNode) {
            // attack phase objects
            if (!weaponAbilityObjects.Contains(abilityAttachmentNode)) {
                weaponAbilityObjects.Add(abilityAttachmentNode);
            }
        }

        public void HandleWeaponHoldableObjects(Equipment newItem, Equipment oldItem) {
            //Debug.Log(gameObject.name + ".CharacterAbilityManager.HandleEquipmentChanged(" + (newItem != null ? newItem.DisplayName : "null") + ", " + (oldItem != null ? oldItem.DisplayName : "null") + ")");

            oldItem?.HandleUnequip(this);

            newItem?.HandleEquip(this);
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

        /*
        public int GetEquipmentSetCount(EquipmentSet equipmentSet) {
            return equipmentManager.GetEquipmentSetCount(equipmentSet);
        }

        /// <summary>
        /// return the equipment slot that a piece of equipment is currently equipped in, or null if not equipped
        /// </summary>
        /// <param name="equipment"></param>
        /// <returns></returns>
        public EquipmentSlotProfile FindEquipmentSlotForEquipment(Equipment equipment) {
            return equipmentManager.FindEquipmentSlotForEquipment(equipment);
        }
        */

        public Equipment Unequip(Equipment equipment) {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterEquipmentManager.Unequip(" + (equipment == null ? "null" : equipment.DisplayName) + ", " + unequipModels + ", " + unequipAppearance + ", " + rebuildAppearance + ")");

            EquipmentSlotProfile equipmentSlotProfile = FindEquipmentSlotForEquipment(equipment);
            if (equipmentSlotProfile != null) {
                return Unequip(equipmentSlotProfile, -1);
            }
            return null;
        }

        public Equipment Unequip(EquipmentSlotProfile equipmentSlot, int slotIndex = -1) {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterEquipmentManager.Unequip(" + equipmentSlot.ToString() + ", " + slotIndex + ", " + unequipModels + ", " + unequipAppearance + ", " + rebuildAppearance + ")");
            
            if (CurrentEquipment.ContainsKey(equipmentSlot) && CurrentEquipment[equipmentSlot] != null) {
                //Debug.Log("equipment manager trying to unequip item in slot " + equipmentSlot.ToString() + "; currentEquipment has this slot key");

                Equipment oldItem = base.UnequipFromList(equipmentSlot);

                NotifyEquipmentChanged(null, oldItem, slotIndex, equipmentSlot);
                return oldItem;
            }
            return null;
        }

        /*
        public void UnequipAll(bool rebuildUMA = true) {
            //Debug.Log("EquipmentManager.UnequipAll()");
            List<EquipmentSlotProfile> tmpList = new List<EquipmentSlotProfile>();
            foreach (EquipmentSlotProfile equipmentSlotProfile in CurrentEquipment.Keys) {
                tmpList.Add(equipmentSlotProfile);
            }

            foreach (EquipmentSlotProfile equipmentSlotProfile in tmpList) {
                Unequip(equipmentSlotProfile, -1, true, true, rebuildUMA);
            }
        }
        */

        public bool HasAffinity(WeaponSkill weaponAffinity) {
            //Debug.Log("EquipmentManager.HasAffinity(" + weaponAffinity.ToString() + ")");
            int weaponCount = 0;
            foreach (Equipment equipment in CurrentEquipment.Values) {
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

        /*
        public bool HasEquipment(string equipmentName, bool partialMatch = false) {
            return equipmentManager.HasEquipment(equipmentName, partialMatch);
        }

        public int GetEquipmentCount(string equipmentName, bool partialMatch = false) {
            return GetEquipmentCount(equipmentName, partialMatch);
        }
        */

    }

}