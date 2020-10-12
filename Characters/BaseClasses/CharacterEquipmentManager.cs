using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UMA;
using UMA.CharacterSystem;

namespace AnyRPG {
    public abstract class CharacterEquipmentManager : MonoBehaviour {

        public System.Action<Equipment, Equipment> OnEquipmentChanged = delegate { };

        // component references
        protected BaseCharacter baseCharacter;
        protected GameObject playerUnitObject = null;
        protected DynamicCharacterAvatar dynamicCharacterAvatar = null;

        protected Dictionary<EquipmentSlotProfile, Equipment> currentEquipment = new Dictionary<EquipmentSlotProfile, Equipment>();

        //protected Dictionary<EquipmentSlotProfile, Dictionary<PrefabProfile, GameObject>> currentEquipmentPhysicalObjects = new Dictionary<EquipmentSlotProfile, Dictionary<PrefabProfile, GameObject>>();
        protected Dictionary<EquipmentSlotProfile, Dictionary<AttachmentNode, GameObject>> currentEquipmentPhysicalObjects = new Dictionary<EquipmentSlotProfile, Dictionary<AttachmentNode, GameObject>>();

        protected bool eventSubscriptionsInitialized = false;
        protected bool componentReferencesInitialized = false;
        protected bool subscribedToCombatEvents = false;

        protected string equipmentProfileName;

        // need a local reference to this for preview characters which don't have a way to reference back to the base character to find this
        protected AttachmentProfile attachmentProfile;

        public Dictionary<EquipmentSlotProfile, Equipment> CurrentEquipment { get => currentEquipment; set => currentEquipment = value; }
        public GameObject MyPlayerUnitObject { get => playerUnitObject; set => playerUnitObject = value; }
        public AttachmentProfile AttachmentProfile { get => attachmentProfile; set => attachmentProfile = value; }

        protected virtual void Start() {
            int numSlots = SystemEquipmentSlotProfileManager.MyInstance.MyResourceList.Count;
        }

        public void OrchestratorStart() {
            GetComponentReferences();
            CreateEventSubscriptions();
        }

        public virtual void OrchestratorFinish() {
            // overwrite me
        }

        public virtual void GetComponentReferences() {
            baseCharacter = GetComponent<BaseCharacter>();
        }

        public virtual void OnDisable() {
            //Debug.Log("PlayerManager.OnDisable()");
            CleanupEventSubscriptions();
        }

        protected virtual void CreateEventSubscriptions() {
            //Debug.Log("PlayerManager.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized) {
                return;
            }
            eventSubscriptionsInitialized = true;
        }

        protected virtual void CleanupEventSubscriptions() {
            //Debug.Log("PlayerManager.CleanupEventSubscriptions()");
            if (!eventSubscriptionsInitialized) {
                return;
            }
            eventSubscriptionsInitialized = false;
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


        public virtual void LoadDefaultEquipment() {
            //Debug.Log(gameObject.name + ".CharacterEquipmentManager.LoadDefaultEquipment()");
            if (baseCharacter == null || baseCharacter.UnitProfile == null || baseCharacter.UnitProfile.EquipmentNameList == null) {
                return;
            }
            bool skipModels = false;
            if (baseCharacter.UnitProfile.IsUMAUnit == true) {
                // quick check to avoid lookup
                skipModels = true;
            } else {
                // try lookup just in case unit profile wasn't set properly or unit is uma / non uma in different regions (which profile doesn't handle yet)
                // this avoids annoying message in console for now
                DynamicCharacterAvatar dynamicCharacterAvatar = baseCharacter.CharacterUnit.GetComponent<DynamicCharacterAvatar>();
                if (dynamicCharacterAvatar != null) {
                    skipModels = true;
                }
            }


            foreach (string equipmentName in baseCharacter.UnitProfile.EquipmentNameList) {
                Equipment equipment = SystemItemManager.MyInstance.GetNewResource(equipmentName) as Equipment;
                if (equipment != null) {
                    Equip(equipment, null, skipModels);
                }
            }
        }

        public void ClearEquipment() {
            currentEquipment = new Dictionary<EquipmentSlotProfile, Equipment>();
        }

        // This method does not actually equip the character, just apply stats and models from already equipped equipment
        public virtual void EquipCharacter() {
            //Debug.Log(gameObject.name + ".CharacterEquipmentManager.EquipCharacter()");
            //public void EquipCharacter(GameObject playerUnitObject = null, bool updateCharacterButton = true) {
            if (currentEquipment == null) {
                //Debug.Log(gameObject.name + ".CharacterEquipmentManager.EquipCharacter(): currentEquipment == null!");
                return;
            }
            //Debug.Log(gameObject.name + ".CharacterEquipmentManager.EquipCharacter(): currentEquipment is not null");
            //foreach (Equipment equipment in currentEquipment.Values) {
            foreach (EquipmentSlotProfile equipmentSlotProfile in currentEquipment.Keys) {
                if (currentEquipment[equipmentSlotProfile] != null) {
                    //Debug.Log("EquipmentManager.EquipCharacter(): Equipment is not null: " + equipment.MyName);

                    // armor and weapon models handling
                    //HandleEquipmentModels(equipment);
                    HandleEquipmentModels(equipmentSlotProfile);

                } else {
                    //Debug.Log(gameObject.name + ".CharacterEquipmentManager.EquipCharacter(): Equipment is null");
                }
            }
        }

        public void HandleItemUMARecipe(Equipment newItem) {
            //Debug.Log("EquipmentManager.HandleItemUMARecipe()");
            if (newItem == null) {
                //Debug.Log("newItem is null. returning");
                return;
            }

            if (newItem.MyUMARecipes != null && newItem.MyUMARecipes.Count > 0 && dynamicCharacterAvatar != null) {
                //Debug.Log("EquipmentManager.HandleItemUMARecipe(): " + newItem.MyName);
                // Put the item in the UMA slot on the UMA character
                //Debug.Log("Putting " + newItem.UMARecipe.name + " in slot " + newItem.UMARecipe.wardrobeSlot);
                foreach (UMATextRecipe uMARecipe in newItem.MyUMARecipes) {
                    if (uMARecipe != null && uMARecipe.compatibleRaces.Contains(dynamicCharacterAvatar.activeRace.name)) {
                        dynamicCharacterAvatar.SetSlot(uMARecipe.wardrobeSlot, uMARecipe.name);
                    }
                }
                dynamicCharacterAvatar.BuildCharacter();
            } else {
                //Debug.Log("EquipmentManager.HandleItemUMARecipe() No UMA recipe to handle");
            }
        }

        public void HandleEquipmentModels(EquipmentSlotProfile equipmentSlotProfile) {
            //Debug.Log(gameObject.name + ".CharacterEquipmentManager.HandleEquipmentModels(" + equipmentSlotProfileName + ")");
            //public void HandleEquipmentModels(Equipment newItem) {
            //HandleItemUMARecipe(newItem);
            HandleWeaponSlot(equipmentSlotProfile);
        }

        public virtual void HandleWeaponSlot(EquipmentSlotProfile equipmentSlotProfile) {
            //Debug.Log(gameObject.name + ".CharacterEquipmentManager.HandleWeaponSlot(" + equipmentSlotProfile.MyName + ")");
            if (currentEquipment == null) {
                Debug.LogError(gameObject.name + ".CharacterEquipmentManager.HandleWeaponSlot(" + equipmentSlotProfile.DisplayName + "): currentEquipment is null!");
                return;
            }
            if (!currentEquipment.ContainsKey(equipmentSlotProfile)) {
                Debug.LogError(gameObject.name + ".CharacterEquipmentManager.HandleWeaponSlot(" + equipmentSlotProfile.DisplayName + "): currentEquipment does not have key");
                return;
            }
            Equipment newItem = currentEquipment[equipmentSlotProfile];
            //public virtual void HandleWeaponSlot(Equipment newItem) {
            if (newItem == null || playerUnitObject == null) {
                //Debug.Log(gameObject.name + ".CharacterEquipmentManager.HandleWeaponSlot(): MyHoldableObjectName is empty on " + newItem.MyName);
                return;
            }
            SpawnEquipmentObjects(equipmentSlotProfile, newItem);
            CharacterAnimator characterAnimator = null;
            if (baseCharacter != null && baseCharacter.CharacterUnit != null && baseCharacter.AnimatedUnit.MyCharacterAnimator != null) {
                characterAnimator = baseCharacter.AnimatedUnit.MyCharacterAnimator;
                if (characterAnimator != null) {
                    //Debug.Log(gameObject.name + ".EquipmentManager.HandleWeaponSlot(): about to animate equipment");
                    characterAnimator.PerformEquipmentChange(newItem, null);
                }
            }
        }

        public void SpawnEquipmentObjects(EquipmentSlotProfile equipmentSlotProfile, Equipment newEquipment) {
            if (newEquipment == null || newEquipment.HoldableObjectList == null || equipmentSlotProfile == null) {
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
                                    Transform targetBone = playerUnitObject.transform.FindChildByRecursive(attachmentPointNode.TargetBone);

                                    if (targetBone != null) {
                                        //Debug.Log("EquipmentManager.HandleWeaponSlot(): " + newItem.name + " has a physical prefab. targetbone is not null: equipSlot: " + newItem.equipSlot);
                                        GameObject newEquipmentPrefab = Instantiate(attachmentNode.HoldableObject.Prefab, targetBone, false);
                                        //holdableObjects.Add(attachmentNode.MyHoldableObject, newEquipmentPrefab);
                                        holdableObjects.Add(attachmentNode, newEquipmentPrefab);
                                        //currentEquipmentPhysicalObjects[equipmentSlotProfile] = newEquipmentPrefab;

                                        newEquipmentPrefab.transform.localScale = attachmentNode.HoldableObject.Scale;
                                        if (baseCharacter != null && baseCharacter.CharacterCombat != null && baseCharacter.CharacterCombat.GetInCombat() == true) {
                                            HoldObject(newEquipmentPrefab, attachmentNode, playerUnitObject);
                                        } else {
                                            SheathObject(newEquipmentPrefab, attachmentNode, playerUnitObject);
                                        }
                                    } else {
                                        Debug.Log(gameObject + ".CharacterEquipmentManager.SpawnEquipmentObjects(). We could not find the target bone " + attachmentPointNode.TargetBone + " when trying to Equip " + newEquipment.DisplayName);
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
            foreach (EquipmentSlotProfile equipmentSlotProfile in currentEquipment.Keys) {
                if (currentEquipment[equipmentSlotProfile] != null && currentEquipmentPhysicalObjects.ContainsKey(equipmentSlotProfile)) {
                    //foreach (KeyValuePair<PrefabProfile, GameObject> holdableObjectReference in currentEquipmentPhysicalObjects[equipmentSlotProfile]) {
                    foreach (KeyValuePair<AttachmentNode, GameObject> holdableObjectReference in currentEquipmentPhysicalObjects[equipmentSlotProfile]) {
                        SheathObject(holdableObjectReference.Value, holdableObjectReference.Key, playerUnitObject);
                        //SheathObject(currentEquipmentPhysicalObjects[equipmentSlotProfileName], currentEquipment[equipmentSlotProfileName].MyHoldableObjectName, playerUnitObject);
                    }
                    
                }
            }
        }

        public void HoldWeapons() {
            foreach (EquipmentSlotProfile equipmentSlotProfile in currentEquipment.Keys) {
                if (currentEquipment[equipmentSlotProfile] != null && currentEquipmentPhysicalObjects.ContainsKey(equipmentSlotProfile)) {
                    //foreach (KeyValuePair<PrefabProfile, GameObject> holdableObjectReference in currentEquipmentPhysicalObjects[equipmentSlotProfile]) {
                    foreach (KeyValuePair<AttachmentNode, GameObject> holdableObjectReference in currentEquipmentPhysicalObjects[equipmentSlotProfile]) {
                        HoldObject(holdableObjectReference.Value, holdableObjectReference.Key, playerUnitObject);
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
                if (baseCharacter != null && baseCharacter.UnitProfile != null && baseCharacter.UnitProfile.PrefabProfile != null && baseCharacter.UnitProfile.PrefabProfile.AttachmentProfile != null) {
                    if (baseCharacter.UnitProfile.PrefabProfile.AttachmentProfile.AttachmentPointDictionary.ContainsKey(attachmentNode.PrimaryAttachmentName)) {
                        return baseCharacter.UnitProfile.PrefabProfile.AttachmentProfile.AttachmentPointDictionary[attachmentNode.PrimaryAttachmentName];
                    }
                } else if (attachmentProfile != null) {
                    if (attachmentProfile.AttachmentPointDictionary.ContainsKey(attachmentNode.PrimaryAttachmentName)) {
                        return attachmentProfile.AttachmentPointDictionary[attachmentNode.PrimaryAttachmentName];
                    }
                } else {
                    Debug.Log(gameObject.name + ".CharacterEquipmentManager.GetSheathedAttachmentPointNode(): could not get attachment profile from prefabprofile");
                }
            }

            Debug.Log(gameObject.name + ".CharacterEquipmentManager.GetSheathedAttachmentPointNode(): Unable to return attachment point node!");
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
                if (baseCharacter != null && baseCharacter.UnitProfile != null && baseCharacter.UnitProfile.PrefabProfile != null && baseCharacter.UnitProfile.PrefabProfile.AttachmentProfile != null) {
                    if (baseCharacter.UnitProfile.PrefabProfile.AttachmentProfile.AttachmentPointDictionary.ContainsKey(attachmentNode.UnsheathedAttachmentName)) {
                        return baseCharacter.UnitProfile.PrefabProfile.AttachmentProfile.AttachmentPointDictionary[attachmentNode.UnsheathedAttachmentName];
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
                    Debug.Log(gameObject + ".CharacterEquipmentManager.HoldObject(): Unable to find target bone : " + attachmentPointNode.TargetBone);
                }
            } else {
                // disabled message because some equipment (like quivers) does not have held attachment points intentionally because it should stay in the same place in combat
                //Debug.Log(gameObject + ".CharacterEquipmentManager.HoldObject(): Unable to get attachment point " + attachmentNode.UnsheathedAttachmentName);
            }
        }

        public virtual void UnequipExclusiveSlots(EquipmentSlotType equipmentSlotType) {
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

        public virtual EquipmentSlotProfile GetFirstEmptySlot(List<EquipmentSlotProfile> slotProfileList) {
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

        public virtual void Equip(Equipment newItem, EquipmentSlotProfile equipmentSlotProfile = null, bool skipModels = false) {
            //Debug.Log(gameObject.name + ".CharacterEquipmentManager.Equip(" + (newItem != null ? newItem.MyName : "null") + ", " + (equipmentSlotProfile == null ? "null" : equipmentSlotProfile.MyName)+ ")");
            //Debug.Break();
            if (newItem == null) {
                Debug.Log("Instructed to Equip a null item!");
                return;
            }
            //currentEquipment[newItem.equipSlot].MyCharacterButton.DequipEquipment();
            //Unequip(newItem.equipSlot);
            if (newItem.EquipmentSlotType == null) {
                Debug.LogError(gameObject + ".CharacterEquipmentManager.Equip() " + newItem.DisplayName + " could not be equipped because it had no equipment slot.  CHECK INSPECTOR.");
                return;
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
                    Debug.LogError(gameObject + ".CharacterEquipmentManager.Equip() " + newItem.DisplayName + " emptyslotProfile is null.  CHECK INSPECTOR.");
                    return;
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
            HandleItemUMARecipe(newItem);

            // testing new code to prevent UKMA characters from trying to find bones before they are created.
            if (skipModels == false) {
                HandleWeaponSlot(emptySlotProfile);
            }

            // DO THIS LAST OR YOU WILL SAVE THE UMA DATA BEFORE ANYTHING IS EQUIPPED!
            // updated oldItem to null here because this call is already done in Unequip.
            // having it here also was leading to duplicate stat removal when gear was changed.
            //Debug.Log(gameObject.name + ".CharacterEquipmentManager.Equip() FIRING ONEQUIPMENTCHANGED");
            OnEquipmentChanged(newItem, null);

        }

        public virtual int GetEquipmentSetCount(EquipmentSet equipmentSet) {
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
        public virtual EquipmentSlotProfile FindEquipmentSlotForEquipment(Equipment equipment) {
            foreach (EquipmentSlotProfile equipmentSlotProfile in currentEquipment.Keys) {
                if (currentEquipment[equipmentSlotProfile] == equipment) {
                    return equipmentSlotProfile;
                }
            }
            return null;
        }

        public virtual Equipment Unequip(Equipment equipment) {
            foreach (EquipmentSlotProfile equipmentSlotProfile in currentEquipment.Keys) {
                if (currentEquipment[equipmentSlotProfile] == equipment) {
                    return Unequip(equipmentSlotProfile);
                }
            }
            return null;
        }

        /*
        public virtual Equipment Unequip(EquipmentSlotProfile equipmentSlotProfile) {
            Debug.Log(gameObject.name + ".CharacterEquipmentManager.Unequip(" + equipmentSlotProfileName + ")");

            if (equipmentSlotProfile != null) {
                return Unequip(equipmentSlotProfile);
            }
            return null;
        }
        */

        public virtual Equipment Unequip(EquipmentSlotProfile equipmentSlot, int slotIndex = -1) {
            //Debug.Log(gameObject.name + ".CharacterEquipmentManager.Unequip(" + equipmentSlot.ToString() + ", " + slotIndex + ")");
            if (currentEquipment.ContainsKey(equipmentSlot) && currentEquipment[equipmentSlot] != null) {
                //Debug.Log("equipment manager trying to unequip item in slot " + equipmentSlot.ToString() + "; currentEquipment has this slot key");
                if (currentEquipmentPhysicalObjects.ContainsKey(equipmentSlot)) {
                    // LOOP THOUGH THEM INSTEAD
                    foreach (KeyValuePair<AttachmentNode, GameObject> holdableObjectReference in currentEquipmentPhysicalObjects[equipmentSlot]) {
                        GameObject destroyObject = holdableObjectReference.Value;
                        //Debug.Log("equipment manager trying to unequip item in slot " + equipmentSlot.ToString() + "; destroying object: " + destroyObject.name);
                        Destroy(destroyObject);
                    }
                }
                Equipment oldItem = currentEquipment[equipmentSlot];

                if (oldItem.MyUMARecipes != null && oldItem.MyUMARecipes.Count > 0 && dynamicCharacterAvatar != null) {
                    // Clear the item from the UMA slot on the UMA character
                    //Debug.Log("Clearing UMA slot " + oldItem.UMARecipe.wardrobeSlot);
                    //avatar.SetSlot(newItem.UMARecipe.wardrobeSlot, newItem.UMARecipe.name);
                    foreach (UMATextRecipe uMARecipe in oldItem.MyUMARecipes) {
                        if (uMARecipe != null && uMARecipe.compatibleRaces.Contains(dynamicCharacterAvatar.activeRace.name)) {
                            dynamicCharacterAvatar.ClearSlot(uMARecipe.wardrobeSlot);
                        }
                    }
                    dynamicCharacterAvatar.BuildCharacter();
                }

                //Debug.Log("zeroing equipment slot: " + equipmentSlot.ToString());
                currentEquipment[equipmentSlot] = null;
                OnEquipmentChanged(null, oldItem);
                return oldItem;
            }
            return null;
        }

        public void UnequipAll() {
            //Debug.Log("EquipmentManager.UnequipAll()");
            List<EquipmentSlotProfile> tmpList = new List<EquipmentSlotProfile>();
            foreach (EquipmentSlotProfile equipmentSlotProfile in currentEquipment.Keys) {
                tmpList.Add(equipmentSlotProfile);
            }

            foreach (EquipmentSlotProfile equipmentSlotProfile in tmpList) {
                Unequip(equipmentSlotProfile);
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
                    if (weaponAffinity == (equipment as Weapon).MyWeaponSkill) {
                        return true;
                    }
                }
            }
            if (weaponCount == 0) {
                // there are no weapons equipped
                // check if the character class is set and contains a weapon skill that is considered to be active when no weapon is equipped
                if (baseCharacter.CharacterClass != null) {
                    if (baseCharacter.CharacterClass.WeaponSkillList.Contains(weaponAffinity)) {
                        if (weaponAffinity.MyDefaultWeaponSkill) {
                            return true;
                        }
                    }
                }

                // check if the unit profile is set and contains a weapon skill that is considered to be active when no weapon is equipped
                if (baseCharacter.UnitProfile != null) {
                    if (baseCharacter.UnitProfile.WeaponSkillList.Contains(weaponAffinity)) {
                        if (weaponAffinity.MyDefaultWeaponSkill) {
                            return true;
                        }
                    }
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

        public virtual void HandleCharacterUnitSpawn() {
            //Debug.Log(gameObject.name + ".EquipmentManager.OnPlayerUnitSpawn()");
            // handled differently in player, and already in ai
            //CreateComponentReferences();
            EquipCharacter();
            SubscribeToCombatEvents();
        }

        protected void SubscribeToCombatEvents() {
            //Debug.Log("PlayerManager.CreateEventSubscriptions()");
            if (subscribedToCombatEvents) {
                return;
            }
            if (baseCharacter != null && baseCharacter.CharacterCombat != null) {
                baseCharacter.CharacterCombat.OnEnterCombat += HoldWeapons;
                baseCharacter.CharacterCombat.OnDropCombat += SheathWeapons;

            }
            subscribedToCombatEvents = true;
        }

        protected void UnSubscribeFromCombatEvents() {
            //Debug.Log("PlayerManager.CleanupEventSubscriptions()");
            if (!subscribedToCombatEvents) {
                return;
            }
            if (baseCharacter != null && baseCharacter.CharacterCombat != null) {
                baseCharacter.CharacterCombat.OnEnterCombat -= HoldWeapons;
                baseCharacter.CharacterCombat.OnDropCombat -= SheathWeapons;
            }
            subscribedToCombatEvents = false;
        }
    }

}