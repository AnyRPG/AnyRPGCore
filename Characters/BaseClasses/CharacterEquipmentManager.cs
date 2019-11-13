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

        protected Dictionary<EquipmentSlot, Equipment> currentEquipment = new Dictionary<EquipmentSlot, Equipment>();

        protected Dictionary<EquipmentSlot, GameObject> currentEquipmentPhysicalObjects = new Dictionary<EquipmentSlot, GameObject>();

        protected Transform targetBone;

        // the holdable object spawned during an ability cast and removed when the cast is complete
        protected GameObject abilityObject;

        protected bool startHasRun = false;
        protected bool eventSubscriptionsInitialized = false;
        protected bool componentReferencesInitialized = false;
        protected bool subscribedToCombatEvents = false;

        [SerializeField]
        protected string equipmentProfileName;

        public Dictionary<EquipmentSlot, Equipment> MyCurrentEquipment { get => currentEquipment; set => currentEquipment = value; }

        protected virtual void Awake() {
            baseCharacter = GetComponent<BaseCharacter>();
        }

        protected virtual void Start() {
            int numSlots = System.Enum.GetNames(typeof(EquipmentSlot)).Length;
            startHasRun = true;
            CreateEventSubscriptions();
            //LoadDefaultEquipment();
        }

        public virtual void CreateComponentReferences() {
            //Debug.Log(gameObject.name + ".CharacterEquipmentManager.CreateComponentReferences()");
            /*
            if (componentReferencesInitialized) {
                return;
            }
            */

            //componentReferencesInitialized = true;
        }

        public virtual void OnDisable() {
            //Debug.Log("PlayerManager.OnDisable()");
            CleanupEventSubscriptions();
        }

        protected virtual void CreateEventSubscriptions() {
            //Debug.Log("PlayerManager.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized || !startHasRun) {
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


        public virtual void LoadDefaultEquipment() {
            Debug.Log(gameObject.name + ".CharacterEquipmentManager.LoadDefaultEquipment()");
            if (equipmentProfileName != null && equipmentProfileName != string.Empty && SystemEquipmentProfileManager.MyInstance != null) {
                EquipmentProfile equipmentProfile = SystemEquipmentProfileManager.MyInstance.GetResource(equipmentProfileName);
                if (equipmentProfile != null) {
                    Debug.Log(gameObject.name + ".CharacterEquipmentManager.LoadDefaultEquipment() found equipment profile for: " + equipmentProfileName);
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
            currentEquipment = new Dictionary<EquipmentSlot, Equipment>();
        }

        // This method does not actually equip the character, just apply stats??? or not ??? and models from already equipped equipment
        public virtual void EquipCharacter() {
            Debug.Log(gameObject.name + ".CharacterEquipmentManager.EquipCharacter()");
            //public void EquipCharacter(GameObject playerUnitObject = null, bool updateCharacterButton = true) {
            if (currentEquipment == null) {
                //Debug.Log(gameObject.name + ".CharacterEquipmentManager.EquipCharacter(): currentEquipment == null!");
                return;
            }
            //Debug.Log(gameObject.name + ".CharacterEquipmentManager.EquipCharacter(): currentEquipment is not null");
            foreach (Equipment equipment in currentEquipment.Values) {
                if (equipment != null) {
                    //Debug.Log("EquipmentManager.EquipCharacter(): Equipment is not null: " + equipment.MyName);

                    // armor and weapon models handling
                    HandleEquipmentModels(equipment);

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

        public void HandleEquipmentModels(Equipment newItem) {
            //Debug.Log(gameObject.name + ".CharacterEquipmentManager.HandleEquipmentModels(" + (newItem == null ? "null" : newItem.MyName) + ")");
            //HandleItemUMARecipe(newItem);
            HandleWeaponSlot(newItem);
        }

        public virtual void HandleWeaponSlot(Equipment newItem) {
            Debug.Log(gameObject.name + ".CharacterEquipmentManager.HandleWeaponSlot(" + (newItem == null ? "null" : newItem.MyName) + ")");
            if (newItem.MyHoldableObjectName == null || newItem.MyHoldableObjectName == string.Empty || playerUnitObject == null) {
                //Debug.Log(gameObject.name + ".CharacterEquipmentManager.HandleWeaponSlot(): MyHoldableObjectName is empty on " + newItem.MyName);
                return;
            }
            //CreateComponentReferences();
            HoldableObject holdableObject = SystemHoldableObjectManager.MyInstance.GetResource(newItem.MyHoldableObjectName);
            if (holdableObject == null) {
                Debug.Log(gameObject.name + ".CharacterEquipmentManager.HandleWeaponSlot(): holdableObject is null");
                return;
            }
            if (holdableObject.MyPhysicalPrefab != null) {
                //Debug.Log("EquipmentManager.HandleWeaponSlot(): " + newItem.name + " has a physical prefab");
                // attach a mesh to a bone for weapons
                targetBone = playerUnitObject.transform.FindChildByRecursive(holdableObject.MySheathedTargetBone);
                if (targetBone != null) {
                    Debug.Log("EquipmentManager.HandleWeaponSlot(): " + newItem.name + " has a physical prefab. targetbone is not null: equipSlot: " + newItem.equipSlot);
                    GameObject newEquipmentPrefab = Instantiate(holdableObject.MyPhysicalPrefab, targetBone, false);
                    currentEquipmentPhysicalObjects[newItem.equipSlot] = newEquipmentPrefab;
                    newEquipmentPrefab.transform.localScale = holdableObject.MyPhysicalScale;
                    if (baseCharacter != null && baseCharacter.MyCharacterCombat != null && baseCharacter.MyCharacterCombat.GetInCombat() == true) {
                        HoldObject(newEquipmentPrefab, newItem.MyHoldableObjectName, playerUnitObject);
                    } else {
                        SheathObject(newEquipmentPrefab, newItem.MyHoldableObjectName, playerUnitObject);
                    }
                } else {
                    Debug.Log(gameObject + ".CharacterEquipmentManager.HandleWeaponSlot(). We could not find the target bone " + holdableObject.MySheathedTargetBone + " when trying to Equip " + newItem.MyName);
                }
                CharacterAnimator characterAnimator = null;
                if (baseCharacter != null && baseCharacter.MyCharacterUnit != null && baseCharacter.MyCharacterUnit.MyCharacterAnimator != null) {
                    characterAnimator = baseCharacter.MyCharacterUnit.MyCharacterAnimator;
                }
                if (characterAnimator == null) {
                    characterAnimator = GetComponent<CharacterAnimator>();
                }
                if (characterAnimator != null) {
                    characterAnimator.InitializeAnimator();
                    Debug.Log(gameObject.name + ".EquipmentManager.HandleWeaponSlot(): about to animate equipment");
                    characterAnimator.PerformEquipmentChange(newItem, null);
                }
            }
        }

        public void SpawnAbilityObject(string holdableObjectName) {
            HoldableObject holdableObject = SystemHoldableObjectManager.MyInstance.GetResource(holdableObjectName);
            if (holdableObject == null) {
                //Debug.Log("EquipmentManager.SpawnAbilityObject(): holdableObject is null");
                return;
            }

            if (holdableObject.MyPhysicalPrefab != null) {
                targetBone = playerUnitObject.transform.FindChildByRecursive(holdableObject.MyTargetBone);
                if (targetBone != null) {
                    //Debug.Log("EquipmentManager.HandleWeaponSlot(): " + newItem.name + " has a physical prefab. targetbone is not null: equipSlot: " + newItem.equipSlot);
                    abilityObject = Instantiate(holdableObject.MyPhysicalPrefab, targetBone, false);
                    abilityObject.transform.localScale = holdableObject.MyPhysicalScale;
                    HoldObject(abilityObject, holdableObject.MyName, playerUnitObject);
                } else {
                    //Debug.Log(gameObject.name + ".CharacterEquipmentManager.SpawnAbilityObject(): We could not find the target bone " + holdableObject.MySheathedTargetBone);
                }

            }
        }

        public void DespawnAbilityObject() {
            if (abilityObject != null) {
                Destroy(abilityObject);
            }
        }

        public void SheathWeapons() {
            if (currentEquipment.ContainsKey(EquipmentSlot.MainHand) && currentEquipment[EquipmentSlot.MainHand] != null && currentEquipmentPhysicalObjects.ContainsKey(EquipmentSlot.MainHand)) {
                SheathObject(currentEquipmentPhysicalObjects[EquipmentSlot.MainHand], currentEquipment[EquipmentSlot.MainHand].MyHoldableObjectName, playerUnitObject);
            }
            if (currentEquipment.ContainsKey(EquipmentSlot.OffHand) && currentEquipment[EquipmentSlot.OffHand] != null && currentEquipmentPhysicalObjects.ContainsKey(EquipmentSlot.OffHand)) {
                SheathObject(currentEquipmentPhysicalObjects[EquipmentSlot.OffHand], currentEquipment[EquipmentSlot.OffHand].MyHoldableObjectName, playerUnitObject);
            }
        }

        public void HoldWeapons() {
            if (currentEquipment.ContainsKey(EquipmentSlot.MainHand) && currentEquipment[EquipmentSlot.MainHand] != null && currentEquipmentPhysicalObjects.ContainsKey(EquipmentSlot.MainHand)) {
                HoldObject(currentEquipmentPhysicalObjects[EquipmentSlot.MainHand], currentEquipment[EquipmentSlot.MainHand].MyHoldableObjectName, playerUnitObject);
            }
            if (currentEquipment.ContainsKey(EquipmentSlot.OffHand) && currentEquipment[EquipmentSlot.OffHand] != null && currentEquipmentPhysicalObjects.ContainsKey(EquipmentSlot.OffHand)) {
                HoldObject(currentEquipmentPhysicalObjects[EquipmentSlot.OffHand], currentEquipment[EquipmentSlot.OffHand].MyHoldableObjectName, playerUnitObject);
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
                Debug.Log(gameObject + ".CharacterEquipmentManager.SheathObject(): targetBone is null: " + holdableObject.MySheathedTargetBone);
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

        public virtual void Equip(Equipment newItem) {
            //Debug.Log(gameObject.name + ".CharacterEquipmentManager.Equip(" + (newItem != null ? newItem.MyName : "null") + ")");
            if (newItem == null) {
                //Debug.Log("Instructed to Equip a null item!");
                return;
            }
            if (currentEquipment.ContainsKey(newItem.equipSlot) && currentEquipment[newItem.equipSlot] != null) {
                //currentEquipment[newItem.equipSlot].MyCharacterButton.DequipEquipment();
                Unequip(newItem.equipSlot);
            }

            // for now manually handle exclusive slots
            if (newItem is Weapon) {
                // deal with 2h weapons, and unequip offhand
                if ((newItem as Weapon).MyWeaponAffinity == AnyRPGWeaponAffinity.Staff || (newItem as Weapon).MyWeaponAffinity == AnyRPGWeaponAffinity.Sword2H || (newItem as Weapon).MyWeaponAffinity == AnyRPGWeaponAffinity.Mace2H) {
                    if (currentEquipment.ContainsKey(EquipmentSlot.OffHand) && currentEquipment[EquipmentSlot.OffHand] != null) {
                        Unequip(EquipmentSlot.OffHand);
                    }
                }
            }

            // deal with offhands, and unequip any 2h mainhand
            if (newItem.equipSlot == EquipmentSlot.OffHand) {
                if (currentEquipment.ContainsKey(EquipmentSlot.MainHand) && currentEquipment[EquipmentSlot.MainHand] != null && ((currentEquipment[EquipmentSlot.MainHand] as Weapon).MyWeaponAffinity == AnyRPGWeaponAffinity.Staff || (currentEquipment[EquipmentSlot.MainHand] as Weapon).MyWeaponAffinity == AnyRPGWeaponAffinity.Sword2H || (currentEquipment[EquipmentSlot.MainHand] as Weapon).MyWeaponAffinity == AnyRPGWeaponAffinity.Mace2H)) {
                    if (currentEquipment.ContainsKey(EquipmentSlot.MainHand) && currentEquipment[EquipmentSlot.MainHand] != null) {
                        Unequip(EquipmentSlot.MainHand);
                    }
                }
            }

            //Debug.Log("Putting " + newItem.GetUMASlotType() + " in slot " + newItem.UMARecipe.wardrobeSlot);
            currentEquipment[newItem.equipSlot] = newItem;
            //newItem.MySlot.Clear();

            // both of these not needed if character unit not yet spawned?
            HandleItemUMARecipe(newItem);
            HandleWeaponSlot(newItem);

            // DO THIS LAST OR YOU WILL SAVE THE UMA DATA BEFORE ANYTHING IS EQUIPPED!
            // updated oldItem to null here because this call is already done in Unequip.
            // having it here also was leading to duplicate stat removal when gear was changed.
            OnEquipmentChanged(newItem, null);

        }

        public virtual Equipment Unequip(EquipmentSlot equipmentSlot, int slotIndex = -1) {
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
            List<EquipmentSlot> tmpList = new List<EquipmentSlot>();
            foreach (EquipmentSlot equipmentSlot in currentEquipment.Keys) {
                tmpList.Add(equipmentSlot);
            }

            foreach (EquipmentSlot equipmentSlot in tmpList) {
                Unequip(equipmentSlot);
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
            if (subscribedToCombatEvents || !startHasRun) {
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