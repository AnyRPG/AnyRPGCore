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

        protected Dictionary<EquipmentSlotProfile, GameObject> currentEquipmentPhysicalObjects = new Dictionary<EquipmentSlotProfile, GameObject>();

        protected Transform targetBone;

        // the holdable objects spawned during an ability cast and removed when the cast is complete
        protected List<GameObject> abilityObjects = new List<GameObject>();

        protected bool eventSubscriptionsInitialized = false;
        protected bool componentReferencesInitialized = false;
        protected bool subscribedToCombatEvents = false;

        [SerializeField]
        protected string equipmentProfileName;

        public Dictionary<EquipmentSlotProfile, Equipment> MyCurrentEquipment { get => currentEquipment; set => currentEquipment = value; }

        protected virtual void Start() {
            int numSlots = SystemEquipmentSlotProfileManager.MyInstance.MyResourceList.Count;
        }

        public void OrchestratorStart() {
            CreateComponentReferences();
            CreateEventSubscriptions();
        }

        public virtual void CreateComponentReferences() {
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
                    returnValue += (MyCurrentEquipment[equipmentSlotProfile] as Weapon).MyDamagePerSecond();
                }
            }
            return returnValue;
        }


        public virtual void LoadDefaultEquipment() {
            //Debug.Log(gameObject.name + ".CharacterEquipmentManager.LoadDefaultEquipment()");
            if (equipmentProfileName != null && equipmentProfileName != string.Empty && SystemEquipmentProfileManager.MyInstance != null) {
                EquipmentProfile equipmentProfile = SystemEquipmentProfileManager.MyInstance.GetResource(equipmentProfileName);
                if (equipmentProfile != null) {
                    //Debug.Log(gameObject.name + ".CharacterEquipmentManager.LoadDefaultEquipment() found equipment profile for: " + equipmentProfileName);
                    foreach (string equipmentName in equipmentProfile.MyEquipmentNameList) {
                        Equipment equipment = SystemItemManager.MyInstance.GetNewResource(equipmentName) as Equipment;
                        if (equipment != null) {
                            Equip(equipment);
                        }
                    }
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

            if (newItem.UMARecipe != null && dynamicCharacterAvatar != null) {
                //Debug.Log("EquipmentManager.HandleItemUMARecipe(): " + newItem.MyName);
                // Put the item in the UMA slot on the UMA character
                //Debug.Log("Putting " + newItem.UMARecipe.name + " in slot " + newItem.UMARecipe.wardrobeSlot);
                dynamicCharacterAvatar.SetSlot(newItem.UMARecipe.wardrobeSlot, newItem.UMARecipe.name);
                dynamicCharacterAvatar.BuildCharacter();
            } else {
                //Debug.Log("EquipmentManager.HandleItemUMARecipe() No UMA recipe to handle");
            }
        }

        public void HandleEquipmentModels(EquipmentSlotProfile equipmentSlotProfile) {
            //public void HandleEquipmentModels(Equipment newItem) {
            //Debug.Log(gameObject.name + ".CharacterEquipmentManager.HandleEquipmentModels(" + (newItem == null ? "null" : newItem.MyName) + ")");
            //HandleItemUMARecipe(newItem);
            HandleWeaponSlot(equipmentSlotProfile);
        }

        public virtual void HandleWeaponSlot(EquipmentSlotProfile equipmentSlotProfile) {

            Equipment newItem = currentEquipment[equipmentSlotProfile];
            //public virtual void HandleWeaponSlot(Equipment newItem) {
            //Debug.Log(gameObject.name + ".CharacterEquipmentManager.HandleWeaponSlot(" + (newItem == null ? "null" : newItem.MyName) + ")");
            if (newItem.MyHoldableObjectName == null || newItem.MyHoldableObjectName == string.Empty || playerUnitObject == null) {
                //Debug.Log(gameObject.name + ".CharacterEquipmentManager.HandleWeaponSlot(): MyHoldableObjectName is empty on " + newItem.MyName);
                return;
            }
            //CreateComponentReferences();
            HoldableObject holdableObject = SystemHoldableObjectManager.MyInstance.GetResource(newItem.MyHoldableObjectName);
            if (holdableObject == null) {
                //Debug.Log(gameObject.name + ".CharacterEquipmentManager.HandleWeaponSlot(): holdableObject is null");
                return;
            }
            if (holdableObject.MyPhysicalPrefab != null) {
                //Debug.Log("EquipmentManager.HandleWeaponSlot(): " + newItem.name + " has a physical prefab");
                // attach a mesh to a bone for weapons
                targetBone = playerUnitObject.transform.FindChildByRecursive(holdableObject.MySheathedTargetBone);
                if (targetBone != null) {
                    //Debug.Log("EquipmentManager.HandleWeaponSlot(): " + newItem.name + " has a physical prefab. targetbone is not null: equipSlot: " + newItem.equipSlot);
                    GameObject newEquipmentPrefab = Instantiate(holdableObject.MyPhysicalPrefab, targetBone, false);
                    currentEquipmentPhysicalObjects[equipmentSlotProfile] = newEquipmentPrefab;
                    newEquipmentPrefab.transform.localScale = holdableObject.MyPhysicalScale;
                    if (baseCharacter != null && baseCharacter.MyCharacterCombat != null && baseCharacter.MyCharacterCombat.GetInCombat() == true) {
                        HoldObject(newEquipmentPrefab, newItem.MyHoldableObjectName, playerUnitObject);
                    } else {
                        SheathObject(newEquipmentPrefab, newItem.MyHoldableObjectName, playerUnitObject);
                    }
                } else {
                    //Debug.Log(gameObject + ".CharacterEquipmentManager.HandleWeaponSlot(). We could not find the target bone " + holdableObject.MySheathedTargetBone + " when trying to Equip " + newItem.MyName);
                }
                CharacterAnimator characterAnimator = null;
                if (baseCharacter != null && baseCharacter.MyCharacterUnit != null && baseCharacter.MyAnimatedUnit.MyCharacterAnimator != null) {
                    characterAnimator = baseCharacter.MyAnimatedUnit.MyCharacterAnimator;
                    if (characterAnimator != null) {
                        //Debug.Log(gameObject.name + ".EquipmentManager.HandleWeaponSlot(): about to animate equipment");
                        characterAnimator.PerformEquipmentChange(newItem, null);
                    }
                }
                /*
                if (characterAnimator == null) {
                    characterAnimator = GetComponent<CharacterAnimator>();
                }
                */
            }
        }

        public void SpawnAbilityObject(List<string> holdableObjectNames) {
            foreach (string holdableObjectName in holdableObjectNames) {
                HoldableObject holdableObject = SystemHoldableObjectManager.MyInstance.GetResource(holdableObjectName);
                if (holdableObject != null) {

                    if (holdableObject.MyPhysicalPrefab != null) {
                        targetBone = playerUnitObject.transform.FindChildByRecursive(holdableObject.MyTargetBone);
                        if (targetBone != null) {
                            //Debug.Log("EquipmentManager.HandleWeaponSlot(): " + newItem.name + " has a physical prefab. targetbone is not null: equipSlot: " + newItem.equipSlot);
                            GameObject abilityObject = Instantiate(holdableObject.MyPhysicalPrefab, targetBone, false);
                            abilityObject.transform.localScale = holdableObject.MyPhysicalScale;
                            HoldObject(abilityObject, holdableObject.MyName, playerUnitObject);
                            abilityObjects.Add(abilityObject);
                        } else {
                            //Debug.Log(gameObject.name + ".CharacterEquipmentManager.SpawnAbilityObject(): We could not find the target bone " + holdableObject.MySheathedTargetBone);
                        }

                    }

                }

            }

        }

        public void DespawnAbilityObjects() {
            //Debug.Log(gameObject + ".CharacterEquipmentManager.DespawnAbilityObjects()");
            if (abilityObjects == null || abilityObjects.Count == 0) {
                return;
            }

            foreach (GameObject abilityObject in abilityObjects) {
                if (abilityObject != null) {
                    Destroy(abilityObject);
                }
            }
            abilityObjects.Clear();
        }

        public void SheathWeapons() {
            // loop through all the equipmentslots and check if they have equipment that is of type weapon
            //if they do, run sheathobject on that slot
            foreach (EquipmentSlotProfile equipmentSlotProfile in currentEquipment.Keys) {
                if (currentEquipment[equipmentSlotProfile] != null && currentEquipmentPhysicalObjects.ContainsKey(equipmentSlotProfile)) {
                    SheathObject(currentEquipmentPhysicalObjects[equipmentSlotProfile], currentEquipment[equipmentSlotProfile].MyHoldableObjectName, playerUnitObject);
                }
            }
        }

        public void HoldWeapons() {
            foreach (EquipmentSlotProfile equipmentSlotProfile in currentEquipment.Keys) {
                if (currentEquipment[equipmentSlotProfile] != null && currentEquipmentPhysicalObjects.ContainsKey(equipmentSlotProfile)) {
                    //SheathObject(currentEquipmentPhysicalObjects[equipmentSlotProfile], currentEquipment[equipmentSlotProfile].MyHoldableObjectName, playerUnitObject);
                    HoldObject(currentEquipmentPhysicalObjects[equipmentSlotProfile], currentEquipment[equipmentSlotProfile].MyHoldableObjectName, playerUnitObject);
                }
            }
        }

        public void SheathObject(GameObject go, string holdableObjectName, GameObject searchObject) {
            if (searchObject == null) {
                //Debug.Log(gameObject + ".CharacterEquipmentManager.SheathObject(): searchObject is null");
                return;
            }
            if (holdableObjectName == null || holdableObjectName == string.Empty) {
                //Debug.Log(gameObject + ".CharacterEquipmentManager.SheathObject(): MyHoldableObjectName is empty");
                return;
            }
            HoldableObject holdableObject = SystemHoldableObjectManager.MyInstance.GetResource(holdableObjectName);
            if (holdableObject == null) {
                //Debug.Log(gameObject + ".CharacterEquipmentManager.SheathObject(): holdableObject is null");
                return;
            }
            targetBone = searchObject.transform.FindChildByRecursive(holdableObject.MySheathedTargetBone);
            if (targetBone != null) {
                //Debug.Log(gameObject + ".CharacterEquipmentManager.SheathObject(): targetBone is NOT null: " + holdableObject.MySheathedTargetBone);
                go.transform.parent = targetBone;
                go.transform.localPosition = holdableObject.MySheathedPhysicalPosition;
                go.transform.localEulerAngles = holdableObject.MySheathedPhysicalRotation;
            } else {
                //Debug.Log(gameObject + ".CharacterEquipmentManager.SheathObject(): targetBone is null: " + holdableObject.MySheathedTargetBone);
            }

        }

        public void HoldObject(GameObject go, string holdableObjectName, GameObject searchObject) {
            //Debug.Log(gameObject + ".CharacterEquipmentManager.HoldObject(" + go.name + ", " + holdableObjectName + ", " + searchObject.name + ")");
            if (holdableObjectName == null || holdableObjectName == string.Empty) {
                //Debug.Log(gameObject + ".CharacterEquipmentManager.HoldObject(): MyHoldableObjectName is empty");
                return;
            }
            HoldableObject holdableObject = SystemHoldableObjectManager.MyInstance.GetResource(holdableObjectName);
            if (holdableObject == null) {
                //Debug.Log(gameObject + ".CharacterEquipmentManager.HoldObject(): holdableObject is null");
                return;
            }
            targetBone = searchObject.transform.FindChildByRecursive(holdableObject.MyTargetBone);
            if (targetBone != null) {
                //Debug.Log(gameObject + ".CharacterEquipmentManager.HoldObject(): targetBone: " + targetBone + "; position: " + holdableObject.MyPhysicalPosition + "; holdableObject.MyPhysicalRotation: " + holdableObject.MyPhysicalRotation);
                go.transform.parent = targetBone;
                go.transform.localPosition = holdableObject.MyPhysicalPosition;
                go.transform.localEulerAngles = holdableObject.MyPhysicalRotation;
            }
        }

        public virtual void UnequipExclusiveSlots(string equipmentSlotTypeName) {
            if (equipmentSlotTypeName != null && equipmentSlotTypeName != string.Empty) {
                EquipmentSlotType equipmentSlotType = SystemEquipmentSlotTypeManager.MyInstance.GetResource(equipmentSlotTypeName);
                if (equipmentSlotType != null) {
                    if (equipmentSlotType.MyExclusiveSlotProfileList != null && equipmentSlotType.MyExclusiveSlotProfileList.Count > 0) {
                        foreach (string equipmentSlotProfileName in equipmentSlotType.MyExclusiveSlotProfileList) {
                            if (equipmentSlotProfileName != null && equipmentProfileName != string.Empty) {
                                Unequip(equipmentSlotProfileName);
                            }
                        }
                    }
                }
            }
        }

        public List<string> GetCompatibleSlotProfiles(string equipmentSlotTypeName) {
            List<string> returnValue = new List<string>();
            if (equipmentSlotTypeName != null && equipmentSlotTypeName != string.Empty) {
                foreach (EquipmentSlotProfile equipmentSlotProfile in SystemEquipmentSlotProfileManager.MyInstance.MyResourceList.Values) {
                    if (equipmentSlotProfile.MyEquipmentSlotTypeList != null && equipmentSlotProfile.MyEquipmentSlotTypeList.Contains(equipmentSlotTypeName)) {
                        returnValue.Add(equipmentSlotProfile.MyName);
                    }
                }
            }

            return returnValue;
        }

        public virtual EquipmentSlotProfile GetFirstEmptySlot(List<string> slotProfileList) {
            //Debug.Log(gameObject.name + ".CharacterEquipmentManager.GetFirstEmptySlot()");
            foreach (string slotProfileName in slotProfileList) {
                EquipmentSlotProfile equipmentSlotProfile = SystemEquipmentSlotProfileManager.MyInstance.GetResource(slotProfileName);
                if (equipmentSlotProfile != null) {
                    if (currentEquipment.ContainsKey(equipmentSlotProfile) == false || (currentEquipment.ContainsKey(equipmentSlotProfile) == true && currentEquipment[equipmentSlotProfile] == null)) {
                        //Debug.Log(gameObject.name + ".CharacterEquipmentManager.GetFirstEmptySlot(): " + equipmentSlotProfile + "; " + equipmentSlotProfile.GetInstanceID());
                        return equipmentSlotProfile;
                    }
                }
            }
            return null;
        }

        public virtual void Equip(Equipment newItem, EquipmentSlotProfile equipmentSlotProfile = null) {
            //Debug.Log(gameObject.name + ".CharacterEquipmentManager.Equip(" + (newItem != null ? newItem.MyName : "null") + ", " + (equipmentSlotProfile == null ? "null" : equipmentSlotProfile.MyName)+ ")");
            if (newItem == null) {
                Debug.Log("Instructed to Equip a null item!");
                return;
            }
            //currentEquipment[newItem.equipSlot].MyCharacterButton.DequipEquipment();
            //Unequip(newItem.equipSlot);
            if (newItem.MyEquipmentSlotType == null || newItem.MyEquipmentSlotType == string.Empty) {
                Debug.LogError(gameObject + ".CharacterEquipmentManager.Equip() " + newItem.MyName + " could not be equipped because it had no equipment slot.  CHECK INSPECTOR.");
            }

            // get list of compatible slots that can take this slot type
            List<string> slotProfileList = GetCompatibleSlotProfiles(newItem.MyEquipmentSlotType);
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
            }
            // unequip any item in an exclusive slot for this item
            UnequipExclusiveSlots(newItem.MyEquipmentSlotType);

            Debug.Log(gameObject.name + ".CharacterEquipmentManager.Equip(): equippping " + newItem.MyName + " in slot: " + emptySlotProfile + "; " + emptySlotProfile.GetInstanceID());
            currentEquipment[emptySlotProfile] = newItem;
            //newItem.MySlot.Clear();

            //Debug.Log("Putting " + newItem.GetUMASlotType() + " in slot " + newItem.UMARecipe.wardrobeSlot);
            // both of these not needed if character unit not yet spawned?
            HandleItemUMARecipe(newItem);
            HandleWeaponSlot(emptySlotProfile);

            // DO THIS LAST OR YOU WILL SAVE THE UMA DATA BEFORE ANYTHING IS EQUIPPED!
            // updated oldItem to null here because this call is already done in Unequip.
            // having it here also was leading to duplicate stat removal when gear was changed.
            //Debug.Log(gameObject.name + ".CharacterEquipmentManager.Equip() FIRING ONEQUIPMENTCHANGED");
            OnEquipmentChanged(newItem, null);

        }

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

        public virtual Equipment Unequip(string equipmentSlotProfileName) {
            if (equipmentSlotProfileName != null && equipmentSlotProfileName != string.Empty) {
                EquipmentSlotProfile equipmentSlotProfile = SystemEquipmentSlotProfileManager.MyInstance.GetResource(equipmentSlotProfileName);
                if (equipmentSlotProfile != null) {
                    return Unequip(equipmentSlotProfile);
                }
            }
            return null;
        }

        public virtual Equipment Unequip(EquipmentSlotProfile equipmentSlot, int slotIndex = -1) {
            //Debug.Log(gameObject.name + ".CharacterEquipmentManager.Unequip(" + equipmentSlot.ToString() + ", " + slotIndex + ")");
            if (currentEquipment.ContainsKey(equipmentSlot) && currentEquipment[equipmentSlot] != null) {
                //Debug.Log("equipment manager trying to unequip item in slot " + equipmentSlot.ToString() + "; currentEquipment has this slot key");
                if (currentEquipmentPhysicalObjects.ContainsKey(equipmentSlot)) {
                    GameObject destroyObject = currentEquipmentPhysicalObjects[equipmentSlot];
                    //Debug.Log("equipment manager trying to unequip item in slot " + equipmentSlot.ToString() + "; destroying object: " + destroyObject.name);
                    Destroy(destroyObject);
                }
                Equipment oldItem = currentEquipment[equipmentSlot];

                if (oldItem.UMARecipe != null && dynamicCharacterAvatar != null) {
                    // Clear the item from the UMA slot on the UMA character
                    //Debug.Log("Clearing UMA slot " + oldItem.UMARecipe.wardrobeSlot);
                    //avatar.SetSlot(newItem.UMARecipe.wardrobeSlot, newItem.UMARecipe.name);
                    dynamicCharacterAvatar.ClearSlot(oldItem.UMARecipe.wardrobeSlot);
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

        public bool HasAffinity(AnyRPGWeaponAffinity weaponAffinity) {
            //Debug.Log("EquipmentManager.HasAffinity(" + weaponAffinity.ToString() + ")");
            bool unarmed = true;
            foreach (Equipment equipment in currentEquipment.Values) {
                if (equipment is Weapon) {
                    if ((equipment as Weapon).MyWeaponAffinity != AnyRPGWeaponAffinity.Unarmed) {
                        unarmed = false;
                        if (weaponAffinity == (equipment as Weapon).MyWeaponAffinity) {
                            return true;
                        }
                    }
                }
            }
            if (weaponAffinity == AnyRPGWeaponAffinity.Unarmed && unarmed == true) {
                return true;
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
                    if (SystemResourceManager.MatchResource(equipment.MyName, equipmentName)) {
                        return true;
                    }
                }
            }
            return false;
        }

        protected void SubscribeToCombatEvents() {
            //Debug.Log("PlayerManager.CreateEventSubscriptions()");
            if (subscribedToCombatEvents) {
                return;
            }
            if (baseCharacter != null && baseCharacter.MyCharacterCombat != null) {
                baseCharacter.MyCharacterCombat.OnEnterCombat += HoldWeapons;
                baseCharacter.MyCharacterCombat.OnDropCombat += SheathWeapons;

            }
            subscribedToCombatEvents = true;
        }

        protected void UnSubscribeFromCombatEvents() {
            //Debug.Log("PlayerManager.CleanupEventSubscriptions()");
            if (!subscribedToCombatEvents) {
                return;
            }
            if (baseCharacter != null && baseCharacter.MyCharacterCombat != null) {
                baseCharacter.MyCharacterCombat.OnEnterCombat -= HoldWeapons;
                baseCharacter.MyCharacterCombat.OnDropCombat -= SheathWeapons;
            }
            subscribedToCombatEvents = false;
        }
    }

}