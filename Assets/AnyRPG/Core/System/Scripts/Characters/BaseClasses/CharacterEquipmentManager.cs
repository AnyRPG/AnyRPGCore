using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

namespace AnyRPG {
    public class CharacterEquipmentManager : EquipmentManager {


        // component references
        protected UnitController unitController = null;

        //protected EquipmentManager equipmentManager = null;

        // keep track of holdable objects to be used during weapon attack animations such as arrows, glowing hand effects, weapon trails, etc
        private List<AbilityAttachmentNode> weaponAbilityAnimationObjects = new List<AbilityAttachmentNode>();

        // keep track of holdable objects to be used during weapon attacks such as arrows, glowing hand effects, weapon trails, etc
        private List<AbilityAttachmentNode> weaponAbilityObjects = new List<AbilityAttachmentNode>();

        //public Dictionary<EquipmentSlotProfile, Equipment> CurrentEquipment { get => equipmentManager.CurrentEquipment; set => equipmentManager.CurrentEquipment = value; }
        public List<AbilityAttachmentNode> WeaponAbilityAnimationObjects { get => weaponAbilityAnimationObjects; }
        public List<AbilityAttachmentNode> WeaponAbilityObjects { get => weaponAbilityObjects; }
        public UnitController UnitController { get => unitController; }

        public CharacterEquipmentManager(UnitController unitController, SystemGameManager systemGameManager) : base(systemGameManager) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterEquipmentManager.CharacterEquipmentManager()");

            this.unitController = unitController;
            CreateSubscriptions();
        }

        public void ClearSubscriptions() {
            //Debug.Log($"{unitController.gameObject.name}.CharacterEquipmentManager.ClearSubscriptions()");

            unitController.CharacterInventoryManager.EquipmentSlots.Clear();
            foreach (EquipmentInventorySlot equipmentInventorySlot in currentEquipment.Values) {
                equipmentInventorySlot.OnAddEquipment -= HandleAddEquipment;
                equipmentInventorySlot.OnRemoveEquipment -= HandleRemoveEquipment;
            }
            ClearCurrentEquipment();
        }

        public void CreateSubscriptions() {
            foreach (EquipmentInventorySlot equipmentInventorySlot in currentEquipment.Values) {
                unitController.CharacterInventoryManager.EquipmentSlots.Add(equipmentInventorySlot);
                equipmentInventorySlot.OnAddEquipment += HandleAddEquipment;
                equipmentInventorySlot.OnRemoveEquipment += HandleRemoveEquipment;
            }
        }

        public void HandleCapabilityConsumerChange() {
            List<InstantiatedEquipment> equipmentToRemove = new List<InstantiatedEquipment>();
            foreach (EquipmentInventorySlot equipmentInventorySlot in CurrentEquipment.Values) {
                if (equipmentInventorySlot.InstantiatedEquipment != null
                    && equipmentInventorySlot.InstantiatedEquipment.Equipment.CanEquip(equipmentInventorySlot.InstantiatedEquipment.GetItemLevel(unitController.CharacterStats.Level), unitController) == false) {
                    equipmentToRemove.Add(equipmentInventorySlot.InstantiatedEquipment);
                }
            }
            if (equipmentToRemove.Count > 0) {
                foreach (InstantiatedEquipment equipment in equipmentToRemove) {
                    Unequip(equipment);
                }
                unitController.UnitModelController.RebuildModelAppearance();
            }

            // since all status effects were cancelled on the change, it is necessary to re-apply set bonuses
            foreach (EquipmentInventorySlot equipmentInventorySlot in CurrentEquipment.Values) {
                if (equipmentInventorySlot.InstantiatedEquipment != null) {
                    unitController.CharacterAbilityManager.UpdateEquipmentTraits(equipmentInventorySlot.InstantiatedEquipment);
                }
            }
        }

        public float GetWeaponDamage() {
            float returnValue = 0f;
            foreach (EquipmentSlotProfile equipmentSlotProfile in CurrentEquipment.Keys) {
                if (CurrentEquipment[equipmentSlotProfile].InstantiatedEquipment != null
                    && CurrentEquipment[equipmentSlotProfile].InstantiatedEquipment.Equipment is Weapon) {
                    returnValue += (CurrentEquipment[equipmentSlotProfile].InstantiatedEquipment.Equipment as Weapon).GetDamagePerSecond(unitController.CharacterStats.Level);
                }
            }
            return returnValue;
        }

        /// <summary>
        /// meant to be called by SetUnitProfile since it relies on that for the equipment list
        /// </summary>
        public void LoadDefaultEquipment(bool loadProviderEquipment) {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterEquipmentManager.LoadDefaultEquipment(" + loadProviderEquipment + ")");

            if (unitController?.UnitProfile?.EquipmentList == null) {
                return;
            }

            // load the unit profile equipment
            foreach (Equipment equipment in unitController.UnitProfile.EquipmentList) {
                if (equipment != null) {
                    
                    Equip(unitController.CharacterInventoryManager.GetNewInstantiatedItem(equipment) as InstantiatedEquipment, null);
                }
            }

            if (loadProviderEquipment == false || unitController.UnitProfile.UseProviderEquipment == false) {
                return;
            }

            if (unitController.BaseCharacter.Faction != null) {
                foreach (Equipment equipment in unitController.BaseCharacter.Faction.EquipmentList) {
                    Equip(unitController.CharacterInventoryManager.GetNewInstantiatedItem(equipment) as InstantiatedEquipment, null);
                }
            }

            if (unitController.BaseCharacter.CharacterRace != null) {
                foreach (Equipment equipment in unitController.BaseCharacter.CharacterRace.EquipmentList) {
                    Equip(unitController.CharacterInventoryManager.GetNewInstantiatedItem(equipment) as InstantiatedEquipment, null);
                }
            }

            if (unitController.BaseCharacter.CharacterClass != null) {
                foreach (Equipment equipment in unitController.BaseCharacter.CharacterClass.EquipmentList) {
                    Equip(unitController.CharacterInventoryManager.GetNewInstantiatedItem(equipment) as InstantiatedEquipment, null);
                }
                if (unitController.BaseCharacter.ClassSpecialization != null) {
                    foreach (Equipment equipment in unitController.BaseCharacter.ClassSpecialization.EquipmentList) {
                        Equip(unitController.CharacterInventoryManager.GetNewInstantiatedItem(equipment) as InstantiatedEquipment, null);
                    }
                }
            }

        }

        public bool Equip(InstantiatedEquipment newItem, EquipmentSlotProfile equipmentSlotProfile = null) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterEquipmentManager.Equip({(newItem != null ? newItem.ResourceName : "null")}, {(equipmentSlotProfile == null ? "null" : equipmentSlotProfile.DisplayName)})");

            if (newItem == null) {
                Debug.LogWarning("Instructed to Equip a null item!");
                return false;
            }

            if (newItem.Equipment.EquipmentSlotType == null) {
                Debug.LogError(unitController.gameObject.name + "CharacterEquipmentManager.Equip() " + newItem.Equipment.ResourceName + " could not be equipped because it had no equipment slot.  CHECK INSPECTOR.");
                return false;
            }

            if (newItem.Equipment.CanEquip(newItem.GetItemLevel(unitController.CharacterStats.Level), unitController) == false) {
                //Debug.Log(baseCharacter.gameObject.name + "CharacterEquipmentManager.Equip(" + (newItem != null ? newItem.DisplayName : "null") + "; could not equip");
                return false;
            }

            EquipEquipment(newItem, equipmentSlotProfile);

            //Debug.Log("CharacterEquipmentManager.Equip(" + (newItem != null ? newItem.DisplayName : "null") + "; successfully equipped");

            return true;
        }

        public override EquipmentSlotProfile EquipEquipment(InstantiatedEquipment newEquipment, EquipmentSlotProfile equipmentSlotProfile = null) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterEquipmentManager.EquipEquipment({(newEquipment != null ? newEquipment.ResourceName : "null")}, {(equipmentSlotProfile == null ? "null" : equipmentSlotProfile.DisplayName)})");

            return base.EquipEquipment(newEquipment, equipmentSlotProfile);
        }

        public void HandleAddEquipment(EquipmentInventorySlot equipmentInventorySlot, InstantiatedEquipment instantiatedEquipment) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterEquipmentManager.HandleAddEquipment({equipmentInventorySlot.ToString()}, {(instantiatedEquipment != null ? instantiatedEquipment.ResourceName : "null")})");

            EquipmentSlotProfile equipmentSlotProfile = currentEquipmentLookup[equipmentInventorySlot];

            // DO THIS LAST OR YOU WILL SAVE THE UMA DATA BEFORE ANYTHING IS EQUIPPED!
            NotifyEquipmentChanged(instantiatedEquipment, null, -1, equipmentSlotProfile);
            // now that all stats have been recalculated, it's safe to fire this event, so things that listen will show the correct values
            unitController.UnitEventController.NotifyOnAddEquipment(equipmentSlotProfile, instantiatedEquipment);
        }

        public override void UnequipEquipment(EquipmentSlotProfile equipmentSlotProfile) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterEquipmentManager.UnequipEquipment({(equipmentSlotProfile == null ? "null" : equipmentSlotProfile.ResourceName)})");
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

        public void HandleWeaponHoldableObjects(InstantiatedEquipment newItem, InstantiatedEquipment oldItem) {
            //Debug.Log($"{gameObject.name}.CharacterAbilityManager.HandleEquipmentChanged(" + (newItem != null ? newItem.DisplayName : "null") + ", " + (oldItem != null ? oldItem.DisplayName : "null") + ")");

            oldItem?.Equipment.HandleUnequip(this);

            newItem?.Equipment.HandleEquip(this);
        }

        public void NotifyEquipmentChanged(InstantiatedEquipment newItem, InstantiatedEquipment oldItem, int slotIndex, EquipmentSlotProfile equipmentSlotProfile) {

            HandleWeaponHoldableObjects(newItem, oldItem);
            unitController.CharacterStats.HandleEquipmentChanged(newItem, oldItem, slotIndex);
            unitController.CharacterCombat.HandleEquipmentChanged(newItem, oldItem, slotIndex, equipmentSlotProfile);
            unitController.CharacterAbilityManager.HandleEquipmentChanged(newItem, oldItem, slotIndex);
            unitController.UnitAnimator.HandleEquipmentChanged(newItem, oldItem, slotIndex);

            // now that all stats have been recalculated, it's safe to fire this event, so things that listen will show the correct values
            //unitController.UnitEventController.NotifyOnEquipmentChanged(newItem, oldItem, slotIndex);
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

        public InstantiatedEquipment Unequip(InstantiatedEquipment instantiatedEquipment) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterEquipmentManager.Unequip({(instantiatedEquipment != null ? instantiatedEquipment.ResourceName : "null")})");

            EquipmentSlotProfile equipmentSlotProfile = FindEquipmentSlotForEquipment(instantiatedEquipment);
            if (equipmentSlotProfile != null) {
                return Unequip(equipmentSlotProfile, -1);
            }
            return null;
        }

        public InstantiatedEquipment Unequip(EquipmentSlotProfile equipmentSlot, int slotIndex = -1) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterEquipmentManager.Unequip({(equipmentSlot == null ? "null" : equipmentSlot.ResourceName)}, {slotIndex})");

            if (CurrentEquipment.ContainsKey(equipmentSlot) && CurrentEquipment[equipmentSlot].InstantiatedEquipment != null) {
                //Debug.Log("equipment manager trying to unequip item in slot " + equipmentSlot.ToString() + "; currentEquipment has this slot key");

                InstantiatedEquipment oldItem = UnequipFromList(equipmentSlot);
                if (slotIndex != -1) {
                    unitController.CharacterInventoryManager.AddInventoryItem(oldItem, slotIndex);
                } else if (oldItem != null) {
                    unitController.CharacterInventoryManager.AddItem(oldItem, false);
                }
                return oldItem;

            }
            return null;
        }

        public void HandleRemoveEquipment(EquipmentInventorySlot equipmentInventorySlot, InstantiatedEquipment instantiatedEquipment) {
            EquipmentSlotProfile equipmentSlotProfile = currentEquipmentLookup[equipmentInventorySlot];
            // FIX ME - that slotIndex used to come from the Unequip function above so this will go into the first empty slot in the bag instead of the one the old item came from
            // during a swap - maybe not such a big deal ?
            NotifyEquipmentChanged(null, instantiatedEquipment, -1, equipmentSlotProfile);
            // now that all stats have been recalculated, it's safe to fire this event, so things that listen will show the correct values
            unitController.UnitEventController.NotifyOnRemoveEquipment(equipmentSlotProfile, instantiatedEquipment);
        }

        public override InstantiatedEquipment UnequipFromList(EquipmentSlotProfile equipmentSlotProfile) {
            //if (systemGameManager.GameMode == GameMode.Local || networkManagerServer.ServerModeActive == true || unitController.UnitControllerMode == UnitControllerMode.Preview) {
                return base.UnequipFromList(equipmentSlotProfile);
            /*
            } else {
                unitController.UnitEventController.NotifyOnRequestUnequipFromList(equipmentSlotProfile);
                return null;
            }
            */
        }

        public bool HasAffinity(WeaponSkill weaponAffinity) {
            //Debug.Log("EquipmentManager.HasAffinity(" + weaponAffinity.ToString() + ")");

            int weaponCount = 0;
            foreach (EquipmentInventorySlot equipmentInventorySlot in CurrentEquipment.Values) {
                if (equipmentInventorySlot.InstantiatedEquipment != null
                    && equipmentInventorySlot.InstantiatedEquipment.Equipment is Weapon) {
                    weaponCount++;
                    if (weaponAffinity == (equipmentInventorySlot.InstantiatedEquipment.Equipment as Weapon).WeaponSkill) {
                        return true;
                    }
                }
            }
            if (weaponCount == 0) {
                // there are no weapons equipped
                // check if the character class is set and contains a weapon skill that is considered to be active when no weapon is equipped
                if (weaponAffinity.WeaponSkillProps.DefaultWeaponSkill && unitController.BaseCharacter.CapabilityConsumerProcessor.IsWeaponSkillSupported(weaponAffinity)) {
                    return true;
                }
            }
            return false;
        }

        public void RequestSwapInventoryEquipment(InstantiatedEquipment oldEquipment, InstantiatedEquipment newEquipment) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterEquipmentManager.RequestSwapInventoryEquipment()");

            if (systemGameManager.GameMode == GameMode.Local) {
                SwapInventoryEquipment(oldEquipment, newEquipment);
            }
            unitController.UnitEventController.NotifyOnRequestSwapInventoryEquipment(oldEquipment, newEquipment);
        }

        public void SwapInventoryEquipment(InstantiatedEquipment oldEquipment, InstantiatedEquipment newEquipment) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterEquipmentManager.SwapInventoryEquipment()");

            EquipmentSlotProfile equipmentSlotProfile = FindEquipmentSlotForEquipment(oldEquipment);
            if (equipmentSlotProfile != null) {
                newEquipment.Remove();
                Unequip(equipmentSlotProfile);
                Equip(newEquipment, equipmentSlotProfile);
                unitController.UnitModelController.RebuildModelAppearance();
            }
        }

        public void RequestUnequipToSlot(InstantiatedEquipment instantiatedEquipment, int inventorySlotId) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterEquipmentManager.RequestUnequipToSlot({instantiatedEquipment.ResourceName}, {inventorySlotId})");

            if (systemGameManager.GameMode == GameMode.Local) {
                UnequipToSlot(instantiatedEquipment, inventorySlotId);
            }
            unitController.UnitEventController.NotifyOnRequestUnequipToSlot(instantiatedEquipment, inventorySlotId);
        }

        public void UnequipToSlot(InstantiatedEquipment instantiatedEquipment, int inventorySlotId) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterEquipmentManager.UnequipToSlot({instantiatedEquipment.ResourceName}, {inventorySlotId})");

            EquipmentSlotProfile equipmentSlotProfile = FindEquipmentSlotForEquipment(instantiatedEquipment);
            if (equipmentSlotProfile != null) {
                Unequip(equipmentSlotProfile, inventorySlotId);
                unitController.UnitModelController.RebuildModelAppearance();
            }
        }

        public void RequestEquipToSlot(InstantiatedEquipment tmp, EquipmentSlotProfile equipmentSlotProfile) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterEquipmentManager.RequestEquip({tmp.ResourceName}, {(equipmentSlotProfile == null ? "null" : equipmentSlotProfile.DisplayName)})");
            
            if (systemGameManager.GameMode == GameMode.Local) {
                EquipToSlot(tmp, equipmentSlotProfile);
            }
            unitController.UnitEventController.NotifyOnRequestEquipToSlot(tmp, equipmentSlotProfile);
        }

        public void EquipToSlot(InstantiatedEquipment tmp, EquipmentSlotProfile equipmentSlotProfile) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterEquipmentManager.EquipToSlot({tmp.ResourceName}, {(equipmentSlotProfile == null ? "null" : equipmentSlotProfile.DisplayName)})");
            
            if (tmp != null && tmp.Equipment.CanEquip(tmp.GetItemLevel(unitController.CharacterStats.Level), unitController) == true) {
                // unequip any existing item in this slot
                Unequip(equipmentSlotProfile);
                InventorySlot oldSlot = tmp.Slot;
                // equip new item to this slot
                if (Equip(tmp, equipmentSlotProfile)) {
                    tmp.RemoveFrom(oldSlot);
                }
                unitController.UnitModelController.RebuildModelAppearance();
            }
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