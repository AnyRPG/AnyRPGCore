using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UMA;
using UMA.CharacterSystem;

namespace AnyRPG {
    public class CharacterEquipmentManager {

        public System.Action<Equipment, Equipment, int> OnEquipmentChanged = delegate { };

        // component references
        protected BaseCharacter baseCharacter = null;

        protected Dictionary<EquipmentSlotProfile, Equipment> currentEquipment = new Dictionary<EquipmentSlotProfile, Equipment>();

        //protected Dictionary<EquipmentSlotProfile, Dictionary<PrefabProfile, GameObject>> currentEquipmentPhysicalObjects = new Dictionary<EquipmentSlotProfile, Dictionary<PrefabProfile, GameObject>>();
        protected Dictionary<EquipmentSlotProfile, Dictionary<AttachmentNode, GameObject>> currentEquipmentPhysicalObjects = new Dictionary<EquipmentSlotProfile, Dictionary<AttachmentNode, GameObject>>();

        protected string equipmentProfileName;

        // need a local reference to this for preview characters which don't have a way to reference back to the base character to find this
        protected AttachmentProfile attachmentProfile;

        // keep track of holdable objects to be used during weapon attacks such as arrows, glowing hand effects, weapon trails, etc
        private List<AbilityAttachmentNode> weaponHoldableObjects = new List<AbilityAttachmentNode>();

        public Dictionary<EquipmentSlotProfile, Equipment> CurrentEquipment { get => currentEquipment; set => currentEquipment = value; }
        public AttachmentProfile AttachmentProfile { get => attachmentProfile; set => attachmentProfile = value; }
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
            //Debug.Log(gameObject.name + ".CharacterEquipmentManager.LoadDefaultEquipment()");

            if (baseCharacter?.UnitProfile?.EquipmentList == null) {
                return;
            }

            // load the unit profile equipment
            foreach (Equipment equipment in baseCharacter.UnitProfile.EquipmentList) {
                if (equipment != null) {
                    Equip(equipment, null, true);
                }
            }

            if (loadProviderEquipment == false || baseCharacter.UnitProfile.UseProviderEquipment == false) {
                return;
            }

            if (baseCharacter.CharacterRace != null) {
                foreach (Equipment equipment in baseCharacter.CharacterRace.EquipmentList) {
                    Equip(equipment, null, true);
                }
            }

            if (baseCharacter.CharacterClass != null) {
                foreach (Equipment equipment in baseCharacter.CharacterClass.EquipmentList) {
                    Equip(equipment, null, true);
                }
                if (baseCharacter.ClassSpecialization != null) {
                    foreach (Equipment equipment in baseCharacter.ClassSpecialization.EquipmentList) {
                        Equip(equipment, null, true);
                    }
                }
            }

            if (baseCharacter.Faction != null) {
                foreach (Equipment equipment in baseCharacter.Faction.EquipmentList) {
                    Equip(equipment, null, true);
                }
            }

        }

        public void ClearEquipment() {
            currentEquipment = new Dictionary<EquipmentSlotProfile, Equipment>();
        }

        // This method does not actually equip the character, just apply models from already equipped equipment
        public void EquipCharacter() {
            //Debug.Log(gameObject.name + ".CharacterEquipmentManager.EquipCharacter()");
            if (currentEquipment == null) {
                //Debug.Log(gameObject.name + ".CharacterEquipmentManager.EquipCharacter(): currentEquipment == null!");
                return;
            }
            foreach (EquipmentSlotProfile equipmentSlotProfile in currentEquipment.Keys) {
                if (currentEquipment[equipmentSlotProfile] != null) {
                    // armor and weapon models handling
                    HandleEquipmentModels(equipmentSlotProfile);
                }
            }
        }

        public void HandleItemUMARecipe(Equipment newItem, bool rebuildUMA = true) {
            //Debug.Log("EquipmentManager.HandleItemUMARecipe()");
            if (newItem == null) {
                Debug.Log("newItem is null. returning");
                return;
            }

            if (newItem.MyUMARecipes != null && newItem.MyUMARecipes.Count > 0 && baseCharacter.UnitController != null && baseCharacter.UnitController.DynamicCharacterAvatar != null) {
                //Debug.Log("EquipmentManager.HandleItemUMARecipe(): " + newItem.DisplayName);
                // Put the item in the UMA slot on the UMA character
                //Debug.Log("Putting " + newItem.UMARecipe.name + " in slot " + newItem.UMARecipe.wardrobeSlot);
                foreach (UMATextRecipe uMARecipe in newItem.MyUMARecipes) {
                    if (uMARecipe != null && uMARecipe.compatibleRaces.Contains(baseCharacter.UnitController.DynamicCharacterAvatar.activeRace.name)) {
                        //Debug.Log("EquipmentManager.HandleItemUMARecipe(): SetSlot: " + uMARecipe.wardrobeSlot + ", " + uMARecipe.name);
                        baseCharacter.UnitController.DynamicCharacterAvatar.SetSlot(uMARecipe.wardrobeSlot, uMARecipe.name);
                    }
                }
                if (rebuildUMA) {
                    //Debug.Log("CharacterEquipmentManager.HandleItemUMARecipe(): BuildCharacter()");
                    baseCharacter.UnitController.DynamicCharacterAvatar.BuildCharacter();
                }
            }
        }

        public void HandleEquipmentModels(EquipmentSlotProfile equipmentSlotProfile) {
            //Debug.Log(gameObject.name + ".CharacterEquipmentManager.HandleEquipmentModels(" + equipmentSlotProfileName + ")");
            //public void HandleEquipmentModels(Equipment newItem) {
            //HandleItemUMARecipe(newItem);
            HandleWeaponSlot(equipmentSlotProfile);
        }

        public void HandleWeaponSlot(EquipmentSlotProfile equipmentSlotProfile) {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterEquipmentManager.HandleWeaponSlot(" + equipmentSlotProfile.DisplayName + ")");
            if (currentEquipment == null) {
                Debug.LogError("CharacterEquipmentManager.HandleWeaponSlot(" + equipmentSlotProfile.DisplayName + "): currentEquipment is null!");
                return;
            }
            if (!currentEquipment.ContainsKey(equipmentSlotProfile)) {
                Debug.LogError("CharacterEquipmentManager.HandleWeaponSlot(" + equipmentSlotProfile.DisplayName + "): currentEquipment does not have key");
                return;
            }
            Equipment newItem = currentEquipment[equipmentSlotProfile];
            //public void HandleWeaponSlot(Equipment newItem) {
            if (newItem == null || baseCharacter.UnitController == null) {
                //Debug.Log(gameObject.name + ".CharacterEquipmentManager.HandleWeaponSlot(): MyHoldableObjectName is empty on " + newItem.MyName);
                return;
            }
            SpawnEquipmentObjects(equipmentSlotProfile, newItem);
            if (baseCharacter != null && baseCharacter.UnitController != null && baseCharacter.UnitController.UnitAnimator != null) {
                baseCharacter.UnitController.UnitAnimator.HandleEquipmentChanged(newItem, null);
            }
        }

        public void SpawnEquipmentObjects(EquipmentSlotProfile equipmentSlotProfile, Equipment newEquipment) {
            //Debug.Log("CharacterEquipmentManager.SpawnEquipmentObjects()");
            if (newEquipment == null || newEquipment.HoldableObjectList == null || equipmentSlotProfile == null) {
                Debug.Log("CharacterEquipmentManager.SpawnEquipmentObjects() : FAILED TO SPAWN OBJECTS");
                return;
            }
            //Dictionary<PrefabProfile, GameObject> holdableObjects = new Dictionary<PrefabProfile, GameObject>();
            Dictionary<AttachmentNode, GameObject> holdableObjects = new Dictionary<AttachmentNode, GameObject>();
            foreach (HoldableObjectAttachment holdableObjectAttachment in newEquipment.HoldableObjectList) {
                if (holdableObjectAttachment != null && holdableObjectAttachment.MyAttachmentNodes != null) {
                    foreach (AttachmentNode attachmentNode in holdableObjectAttachment.MyAttachmentNodes) {
                        if (attachmentNode != null && attachmentNode.MyEquipmentSlotProfile != null && equipmentSlotProfile == attachmentNode.MyEquipmentSlotProfile) {
                            //CreateComponentReferences();
                            if (attachmentNode.HoldableObject != null && attachmentNode.HoldableObject.Prefab != null) {
                                //Debug.Log("EquipmentManager.HandleWeaponSlot(): " + newItem.name + " has a physical prefab");
                                // attach a mesh to a bone for weapons

                                AttachmentPointNode attachmentPointNode = GetSheathedAttachmentPointNode(attachmentNode);
                                if (attachmentPointNode != null) {
                                    Transform targetBone = baseCharacter.UnitController.gameObject.transform.FindChildByRecursive(attachmentPointNode.TargetBone);

                                    if (targetBone != null) {
                                        //Debug.Log("EquipmentManager.HandleWeaponSlot(): " + newItem.name + " has a physical prefab. targetbone is not null: equipSlot: " + newItem.equipSlot);
                                        GameObject newEquipmentPrefab = UnityEngine.Object.Instantiate(attachmentNode.HoldableObject.Prefab, targetBone, false);
                                        //holdableObjects.Add(attachmentNode.MyHoldableObject, newEquipmentPrefab);
                                        holdableObjects.Add(attachmentNode, newEquipmentPrefab);
                                        //currentEquipmentPhysicalObjects[equipmentSlotProfile] = newEquipmentPrefab;

                                        newEquipmentPrefab.transform.localScale = attachmentNode.HoldableObject.Scale;
                                        if (baseCharacter != null && baseCharacter.CharacterCombat != null && baseCharacter.CharacterCombat.GetInCombat() == true) {
                                            HoldObject(newEquipmentPrefab, attachmentNode, baseCharacter.UnitController.gameObject);
                                        } else {
                                            SheathObject(newEquipmentPrefab, attachmentNode, baseCharacter.UnitController.gameObject);
                                        }
                                    } else {
                                        Debug.Log("CharacterEquipmentManager.SpawnEquipmentObjects(). We could not find the target bone " + attachmentPointNode.TargetBone + " when trying to Equip " + newEquipment.DisplayName);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if (holdableObjects.Count > 0) {
                currentEquipmentPhysicalObjects[equipmentSlotProfile] = holdableObjects;
            }
        }

        public void SheathWeapons() {
            // loop through all the equipmentslots and check if they have equipment that is of type weapon
            //if they do, run sheathobject on that slot

            if (baseCharacter == null || baseCharacter.UnitController == null) {
                // cannot sheath a physical item if there is no unit
                return;
            }
            foreach (EquipmentSlotProfile equipmentSlotProfile in currentEquipment.Keys) {
                if (currentEquipment[equipmentSlotProfile] != null && currentEquipmentPhysicalObjects.ContainsKey(equipmentSlotProfile)) {
                    //foreach (KeyValuePair<PrefabProfile, GameObject> holdableObjectReference in currentEquipmentPhysicalObjects[equipmentSlotProfile]) {
                    foreach (KeyValuePair<AttachmentNode, GameObject> holdableObjectReference in currentEquipmentPhysicalObjects[equipmentSlotProfile]) {
                        SheathObject(holdableObjectReference.Value, holdableObjectReference.Key, baseCharacter.UnitController.gameObject);
                        //SheathObject(currentEquipmentPhysicalObjects[equipmentSlotProfileName], currentEquipment[equipmentSlotProfileName].MyHoldableObjectName, playerUnitObject);
                    }
                    
                }
            }
        }

        public void HoldWeapons() {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterEquipmentManager.HoldWeapons()");

            // when mounted, weapons should stay sheathed
            if (baseCharacter?.UnitController?.Mounted == true) {
                return;
            }

            foreach (EquipmentSlotProfile equipmentSlotProfile in currentEquipment.Keys) {
                if (currentEquipment[equipmentSlotProfile] != null && currentEquipmentPhysicalObjects.ContainsKey(equipmentSlotProfile)) {
                    //foreach (KeyValuePair<PrefabProfile, GameObject> holdableObjectReference in currentEquipmentPhysicalObjects[equipmentSlotProfile]) {
                    foreach (KeyValuePair<AttachmentNode, GameObject> holdableObjectReference in currentEquipmentPhysicalObjects[equipmentSlotProfile]) {
                        HoldObject(holdableObjectReference.Value, holdableObjectReference.Key, baseCharacter.UnitController.gameObject);
                        //SheathObject(currentEquipmentPhysicalObjects[equipmentSlotProfileName], currentEquipment[equipmentSlotProfileName].MyHoldableObjectName, playerUnitObject);
                    }

                }
                /*
                if (currentEquipment[equipmentSlotProfile] != null && currentEquipmentPhysicalObjects.ContainsKey(equipmentSlotProfile)) {
                    //SheathObject(currentEquipmentPhysicalObjects[equipmentSlotProfile], currentEquipment[equipmentSlotProfile].MyHoldableObjectName, playerUnitObject);
                    HoldObject(currentEquipmentPhysicalObjects[equipmentSlotProfile], currentEquipment[equipmentSlotProfile].MyHoldableObjectName, playerUnitObject);
                }
                */
            }
        }

        //public void SheathObject(GameObject go, PrefabProfile holdableObject, GameObject searchObject) {
        public void SheathObject(GameObject go, AttachmentNode attachmentNode, GameObject searchObject) {
            if (searchObject == null) {
                //Debug.Log(gameObject + ".CharacterEquipmentManager.SheathObject(): searchObject is null");
                return;
            }
            if (attachmentNode == null || attachmentNode.HoldableObject == null ) {
                //Debug.Log(gameObject + ".CharacterEquipmentManager.SheathObject(): MyHoldableObjectName is empty");
                return;
            }
            if (go == null) {
                //Debug.Log(gameObject + ".CharacterEquipmentManager.SheathObject(): gameObject is null is null");
                return;
            }
            AttachmentPointNode attachmentPointNode = GetSheathedAttachmentPointNode(attachmentNode);
            if (attachmentPointNode != null) {
                Transform targetBone = searchObject.transform.FindChildByRecursive(attachmentPointNode.TargetBone);
                if (targetBone != null) {
                    //Debug.Log(gameObject + ".CharacterEquipmentManager.SheathObject(): targetBone is NOT null: " + holdableObject.MySheathedTargetBone);
                    go.transform.parent = targetBone;
                    go.transform.localPosition = attachmentPointNode.Position;
                    go.transform.localEulerAngles = attachmentPointNode.Rotation;
                } else {
                    //Debug.Log(gameObject + ".CharacterEquipmentManager.SheathObject(): targetBone is null: " + holdableObject.MySheathedTargetBone);
                }
            }

        }

        public AttachmentPointNode GetSheathedAttachmentPointNode(AttachmentNode attachmentNode) {
            if (attachmentNode.UseUniversalAttachment == false) {
                AttachmentPointNode attachmentPointNode = new AttachmentPointNode();
                attachmentPointNode.TargetBone = attachmentNode.HoldableObject.SheathedTargetBone;
                attachmentPointNode.Position = attachmentNode.HoldableObject.SheathedPosition;
                attachmentPointNode.Rotation = attachmentNode.HoldableObject.SheathedRotation;
                return attachmentPointNode;
            } else {
                // find unit profile, find prefab profile, find universal attachment profile, find universal attachment node
                if (baseCharacter != null && baseCharacter.UnitProfile != null && baseCharacter.UnitProfile != null && baseCharacter.UnitProfile.UnitPrefabProps.AttachmentProfile != null) {
                    if (baseCharacter.UnitProfile.UnitPrefabProps.AttachmentProfile.AttachmentPointDictionary.ContainsKey(attachmentNode.PrimaryAttachmentName)) {
                        return baseCharacter.UnitProfile.UnitPrefabProps.AttachmentProfile.AttachmentPointDictionary[attachmentNode.PrimaryAttachmentName];
                    }
                } else if (attachmentProfile != null) {
                    if (attachmentProfile.AttachmentPointDictionary.ContainsKey(attachmentNode.PrimaryAttachmentName)) {
                        return attachmentProfile.AttachmentPointDictionary[attachmentNode.PrimaryAttachmentName];
                    }
                } else {
                    // enable for troubleshooting only.  It gets spammy with beast units that don't have attachments.
                    //Debug.Log(gameObject.name + ".CharacterEquipmentManager.GetSheathedAttachmentPointNode(): could not get attachment profile from prefabprofile");
                }
            }
            // enable for troubleshooting only.  It gets spammy with beast units that don't have attachments.
            //Debug.Log("CharacterEquipmentManager.GetSheathedAttachmentPointNode(): Unable to return attachment point node!");
            return null;
        }

        public AttachmentPointNode GetHeldAttachmentPointNode(AttachmentNode attachmentNode) {
            //Debug.Log(gameObject.name + ".CharacterEquipmentManager.GetHeldAttachmentPointNode()");
            if (attachmentNode.UseUniversalAttachment == false) {
                AttachmentPointNode attachmentPointNode = new AttachmentPointNode();
                attachmentPointNode.TargetBone = attachmentNode.HoldableObject.TargetBone;
                attachmentPointNode.Position = attachmentNode.HoldableObject.Position;
                attachmentPointNode.Rotation = attachmentNode.HoldableObject.Rotation;
                attachmentPointNode.RotationIsGlobal = attachmentNode.HoldableObject.RotationIsGlobal;
                return attachmentPointNode;
            } else {
                // find unit profile, find prefab profile, find universal attachment profile, find universal attachment node
                if (baseCharacter != null && baseCharacter.UnitProfile != null && baseCharacter.UnitProfile != null && baseCharacter.UnitProfile.UnitPrefabProps.AttachmentProfile != null) {
                    if (baseCharacter.UnitProfile.UnitPrefabProps.AttachmentProfile.AttachmentPointDictionary.ContainsKey(attachmentNode.UnsheathedAttachmentName)) {
                        return baseCharacter.UnitProfile.UnitPrefabProps.AttachmentProfile.AttachmentPointDictionary[attachmentNode.UnsheathedAttachmentName];
                    }
                }
            }

            return null;
        }

        public void HoldObject(GameObject go, AttachmentNode attachmentNode, GameObject searchObject) {
            //public void HoldObject(GameObject go, PrefabProfile holdableObject, GameObject searchObject) {
            //Debug.Log(gameObject + ".CharacterEquipmentManager.HoldObject(" + go.name + ", " + holdableObjectName + ", " + searchObject.name + ")");
            if (attachmentNode == null || attachmentNode.HoldableObject == null || go == null || searchObject == null) {
                //Debug.Log(gameObject + ".CharacterEquipmentManager.HoldObject(): MyHoldableObjectName is empty");
                return;
            }

            AttachmentPointNode attachmentPointNode = GetHeldAttachmentPointNode(attachmentNode);
            if (attachmentPointNode != null && attachmentPointNode.TargetBone != null && attachmentPointNode.TargetBone != string.Empty) {
                Transform targetBone = searchObject.transform.FindChildByRecursive(attachmentPointNode.TargetBone);
                if (targetBone != null) {
                    //Debug.Log(gameObject + ".CharacterEquipmentManager.HoldObject(): targetBone: " + targetBone + "; position: " + holdableObject.MyPosition + "; holdableObject.MyPhysicalRotation: " + holdableObject.MyRotation);
                    go.transform.parent = targetBone;
                    go.transform.localPosition = attachmentPointNode.Position;
                    if (attachmentPointNode.RotationIsGlobal) {
                        go.transform.rotation = Quaternion.LookRotation(targetBone.transform.forward) * Quaternion.Euler(attachmentPointNode.Rotation);
                    } else {
                        go.transform.localEulerAngles = attachmentPointNode.Rotation;
                    }
                } else {
                    Debug.Log("CharacterEquipmentManager.HoldObject(): Unable to find target bone : " + attachmentPointNode.TargetBone);
                }
            } else {
                // disabled message because some equipment (like quivers) does not have held attachment points intentionally because it should stay in the same place in combat
                //Debug.Log(gameObject + ".CharacterEquipmentManager.HoldObject(): Unable to get attachment point " + attachmentNode.UnsheathedAttachmentName);
            }
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
                foreach (EquipmentSlotProfile equipmentSlotProfile in SystemEquipmentSlotProfileManager.MyInstance.MyResourceList.Values) {
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

        public bool Equip(Equipment newItem, EquipmentSlotProfile equipmentSlotProfile = null, bool skipModels = false, bool rebuildUMA = true) {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterEquipmentManager.Equip(" + (newItem != null ? newItem.DisplayName : "null") + ", " + (equipmentSlotProfile == null ? "null" : equipmentSlotProfile.DisplayName) + ")");
            //Debug.Break();
            if (newItem == null) {
                Debug.Log("Instructed to Equip a null item!");
                return false;
            }
            //currentEquipment[newItem.equipSlot].MyCharacterButton.DequipEquipment();
            //Unequip(newItem.equipSlot);
            if (newItem.EquipmentSlotType == null) {
                Debug.LogError("CharacterEquipmentManager.Equip() " + newItem.DisplayName + " could not be equipped because it had no equipment slot.  CHECK INSPECTOR.");
                return false;
            }

            if (newItem.CanEquip(baseCharacter) == false) {
                //Debug.Log("CharacterEquipmentManager.Equip(" + (newItem != null ? newItem.DisplayName : "null") + "; could not equip");
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
                    Debug.LogError("CharacterEquipmentManager.Equip() " + newItem.DisplayName + " emptyslotProfile is null.  CHECK INSPECTOR.");
                    return false;
                }
            }

            // unequip any item in an exclusive slot for this item
            UnequipExclusiveSlots(newItem.EquipmentSlotType);

            //Debug.Log(gameObject.name + ".CharacterEquipmentManager.Equip(): equippping " + newItem.MyName + " in slot: " + emptySlotProfile + "; " + emptySlotProfile.GetInstanceID());
            currentEquipment[emptySlotProfile] = newItem;
            //newItem.MySlot.Clear();

            //Debug.Break();
            //Debug.Log("Putting " + newItem.GetUMASlotType() + " in slot " + newItem.UMARecipe.wardrobeSlot);

            // both of these not needed if character unit not yet spawned?
            HandleItemUMARecipe(newItem, rebuildUMA);

            // testing new code to prevent UKMA characters from trying to find bones before they are created.
            if (skipModels == false) {
                HandleWeaponSlot(emptySlotProfile);
            }

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

        public Equipment Unequip(Equipment equipment, bool rebuildUMA = true) {
            foreach (EquipmentSlotProfile equipmentSlotProfile in currentEquipment.Keys) {
                if (currentEquipment[equipmentSlotProfile] == equipment) {
                    return Unequip(equipmentSlotProfile, -1, rebuildUMA);
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

        public Equipment Unequip(EquipmentSlotProfile equipmentSlot, int slotIndex = -1, bool rebuildUMA = true) {
            //Debug.Log(gameObject.name + ".CharacterEquipmentManager.Unequip(" + equipmentSlot.ToString() + ", " + slotIndex + ")");
            if (currentEquipment.ContainsKey(equipmentSlot) && currentEquipment[equipmentSlot] != null) {
                //Debug.Log("equipment manager trying to unequip item in slot " + equipmentSlot.ToString() + "; currentEquipment has this slot key");
                if (currentEquipmentPhysicalObjects.ContainsKey(equipmentSlot)) {
                    // LOOP THOUGH THEM INSTEAD
                    foreach (KeyValuePair<AttachmentNode, GameObject> holdableObjectReference in currentEquipmentPhysicalObjects[equipmentSlot]) {
                        GameObject destroyObject = holdableObjectReference.Value;
                        //Debug.Log("equipment manager trying to unequip item in slot " + equipmentSlot.ToString() + "; destroying object: " + destroyObject.name);
                        GameObject.Destroy(destroyObject);
                    }
                }
                Equipment oldItem = currentEquipment[equipmentSlot];

                if (oldItem.MyUMARecipes != null && oldItem.MyUMARecipes.Count > 0 && baseCharacter.UnitController != null && baseCharacter.UnitController.DynamicCharacterAvatar != null) {
                    // Clear the item from the UMA slot on the UMA character
                    //Debug.Log("Clearing UMA slot " + oldItem.UMARecipe.wardrobeSlot);
                    //avatar.SetSlot(newItem.UMARecipe.wardrobeSlot, newItem.UMARecipe.name);
                    foreach (UMATextRecipe uMARecipe in oldItem.MyUMARecipes) {
                        if (uMARecipe != null && uMARecipe.compatibleRaces.Contains(baseCharacter.UnitController.DynamicCharacterAvatar.activeRace.name)) {
                            baseCharacter.UnitController.DynamicCharacterAvatar.ClearSlot(uMARecipe.wardrobeSlot);
                        }
                    }
                    if (rebuildUMA) {
                        //Debug.Log("EquipmentManager.Unequip(): BuildCharacter()");
                        baseCharacter.UnitController.DynamicCharacterAvatar.BuildCharacter();
                    }
                }

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
                Unequip(equipmentSlotProfile, -1, rebuildUMA);
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
                    if (SystemResourceManager.MatchResource(equipment.DisplayName, equipmentName)) {
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
                    if (SystemResourceManager.MatchResource(equipment.DisplayName, equipmentName)) {
                        returnValue++;
                    }
                }
            }
            return returnValue;
        }

        public void HandleCharacterUnitSpawn() {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterEquipmentManager.HandleCharacterUnitSpawn()");
            EquipCharacter();
        }

    }

}