using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UMA;
using UMA.CharacterSystem;

public class EquipmentManager : MonoBehaviour {

    #region Singleton
    private static EquipmentManager instance;

    public static EquipmentManager MyInstance {
        get {
            if (instance == null) {
                instance = FindObjectOfType<EquipmentManager>();
            }

            return instance;
        }
    }

    #endregion

    private Dictionary<EquipmentSlot, Equipment> currentEquipment = new Dictionary<EquipmentSlot, Equipment>();

    private Dictionary<EquipmentSlot, GameObject> currentEquipmentPhysicalObjects = new Dictionary<EquipmentSlot, GameObject>();

    private Transform targetBone;

    protected bool startHasRun = false;
    protected bool eventReferencesInitialized = false;

    public Dictionary<EquipmentSlot, Equipment> MyCurrentEquipment { get => currentEquipment; set => currentEquipment = value; }


    private void Start() {
        int numSlots = System.Enum.GetNames(typeof(EquipmentSlot)).Length;
        startHasRun = true;
        CreateEventReferences();
    }

    private void CreateEventReferences() {
        //Debug.Log("PlayerManager.CreateEventReferences()");
        if (eventReferencesInitialized || !startHasRun) {
            return;
        }
        SystemEventManager.MyInstance.OnPlayerUnitSpawn += OnPlayerUnitSpawn;
        SystemEventManager.MyInstance.OnPlayerUnitDespawn += OnPlayerUnitDespawn;
        eventReferencesInitialized = true;
    }

    private void CleanupEventReferences() {
        //Debug.Log("PlayerManager.CleanupEventReferences()");
        if (!eventReferencesInitialized) {
            return;
        }
        SystemEventManager.MyInstance.OnPlayerUnitSpawn -= OnPlayerUnitSpawn;
        SystemEventManager.MyInstance.OnPlayerUnitDespawn -= OnPlayerUnitDespawn;
        eventReferencesInitialized = false;
    }

    public void OnDisable() {
        //Debug.Log("PlayerManager.OnDisable()");
        CleanupEventReferences();
    }

    public void ClearEquipment() {
        currentEquipment = new Dictionary<EquipmentSlot, Equipment>();
    }


    public void OnPlayerUnitSpawn() {
        //Debug.Log("EquipmentManager.OnPlayerUnitSpawn()");
        EquipCharacter();
    }

    public void OnPlayerUnitDespawn() {
        //Debug.Log("EquipmentManager.OnPlayerUnitDespawn()");
    }

    // This method does not actually equip the character, just apply stats and models from already equipped equipment
    public void EquipCharacter(GameObject playerUnitObject = null, bool updateCharacterButton = true) {
        //Debug.Log("EquipmentManager.EquipCharacter(" + (playerUnitObject == null ? "null" : playerUnitObject.name) + ")");
        if (currentEquipment == null) {
            return;
        }
        foreach (Equipment equipment in currentEquipment.Values) {
            if (equipment != null) {
                //Debug.Log("EquipmentManager.EquipCharacter(): Equipment is not null: " + equipment.MyName);

                // armor and weapon models handling
                HandleEquipmentModels(equipment, playerUnitObject);

                if (updateCharacterButton) {
                    // put the items in the character panel because we are equipping an actual character, not the character panel character
                    if (CharacterPanel.MyInstance != null) {
                        CharacterPanel.MyInstance.EquipEquipment(equipment, true);
                    }
                }
                // new code to trigger only external items that are idempotent but lost on load (on hit abilities and animation profiles)
                SystemEventManager.MyInstance.NotifyOnEquipmentRefresh(equipment);
            } else {
                //Debug.Log("Equipment is null");
            }
        }
    }

    public void HandleItemUMARecipe(Equipment newItem) {
        //Debug.Log("EquipmentManager.HandleItemUMARecipe()");
        if (newItem == null) {
            //Debug.Log("newItem is null. returning");
            return;
        }
        if (newItem.UMARecipe != null && PlayerManager.MyInstance.MyAvatar != null) {
            //Debug.Log("EquipmentManager.HandleItemUMARecipe(): " + newItem.MyName);
            // Put the item in the UMA slot on the UMA character
            //Debug.Log("Putting " + newItem.UMARecipe.name + " in slot " + newItem.UMARecipe.wardrobeSlot);
            PlayerManager.MyInstance.MyAvatar.SetSlot(newItem.UMARecipe.wardrobeSlot, newItem.UMARecipe.name);
            PlayerManager.MyInstance.MyAvatar.BuildCharacter();
        } else {
            //Debug.Log("EquipmentManager.HandleItemUMARecipe() No UMA recipe to handle");
        }
    }

    public void HandleEquipmentModels(Equipment newItem, GameObject playerUnitObject = null) {
        //Debug.Log("EquipmentManager.HandleEquipmentModels(" + (newItem == null ? "null" : newItem.MyName) + ", " + (playerUnitObject == null ? "null" : playerUnitObject.name) + ")");
        //HandleItemUMARecipe(newItem);
        HandleWeaponSlot(newItem, playerUnitObject);
    }

    public void HandleWeaponSlot(Equipment newItem, GameObject playerUnitObject = null) {
        //Debug.Log("EquipmentManager.HandleWeaponSlot(" + (newItem == null ? "null" : newItem.MyName) + ", " + (playerUnitObject == null ? "null" : playerUnitObject.name) + ")");
        if (PlayerManager.MyInstance.MyPlayerUnitSpawned == false && playerUnitObject == null) {
            // nothing to do since there is no object to attach to right now.  It will be handled automatically when he spawns anyway
            //Debug.Log("EquipmentManager.HandleWeaponSlot(): playerUnitObject is null and player unit is not spawned.  returning.");
            return;
        }
        GameObject usedObject = (playerUnitObject == null ? PlayerManager.MyInstance.MyPlayerUnitObject : playerUnitObject);
        if (newItem.PhysicalPrefab != null) {
            //Debug.Log("EquipmentManager.HandleWeaponSlot(): " + newItem.name + " has a physical prefab");
            // attach a mesh to a bone for weapons
            targetBone = usedObject.transform.Find(newItem.TargetBone);
            if (targetBone != null) {
                //Debug.Log("EquipmentManager.HandleWeaponSlot(): " + newItem.name + " has a physical prefab. targetbone is not null: equipSlot: " + newItem.equipSlot);
                GameObject newEquipmentPrefab = Instantiate(newItem.PhysicalPrefab, targetBone, false);
                if (PlayerManager.MyInstance.MyPlayerUnitSpawned && usedObject == PlayerManager.MyInstance.MyPlayerUnitObject) {
                    currentEquipmentPhysicalObjects[newItem.equipSlot] = newEquipmentPrefab;
                }
                newEquipmentPrefab.transform.localPosition = newItem.PhysicalPosition;
                newEquipmentPrefab.transform.localEulerAngles = newItem.PhysicalRotation;
                newEquipmentPrefab.transform.localScale = newItem.PhysicalScale;
            } else {
                //Debug.Log("We could not find the target bone " + newItem.TargetBone + " when trying to Equip " + newItem.MyName);
            }
            //(PlayerManager.MyInstance.MyCharacter.MyCharacterUnit.MyCharacterAnimator as PlayerAnimator).OnEquipmentChanged(null, newItem);
            // testing was that above line why animations weren't set on zone load?
            if (PlayerManager.MyInstance.MyPlayerUnitSpawned == true && usedObject == PlayerManager.MyInstance.MyPlayerUnitObject) {
                //Debug.Log("EquipmentManager.HandleWeaponSlot(): Player Unit is spawned and the object we are using as the player unit, go ahead and animate attacks");
                (PlayerManager.MyInstance.MyCharacter.MyCharacterUnit.MyCharacterAnimator as PlayerAnimator).OnEquipmentChanged(newItem, null);
            }
        }
    }

    public void Equip (Equipment newItem) {
        //Debug.Log("EquipmentManager.Equip()");
        if (newItem == null) {
            //Debug.Log("Instructed to Equip a null item!");
            return;
        }
        //Equipment oldItem = Unequip(newItem.equipSlot);
        // TESTING, THIS STUFF NEEDS TO BE HANDLED THROUGH CHARACTER PANEL?
        //CharacterPanel.MyInstance.
        if (currentEquipment.ContainsKey(newItem.equipSlot) && currentEquipment[newItem.equipSlot] != null) {
            currentEquipment[newItem.equipSlot].MyCharacterButton.DequipEquipment();
            //Unequip(newItem.equipSlot);
        }

        // for now manually handle exclusive slots
        if (newItem is Weapon) {
            // deal with 2h weapons, and unequip offhand
            if ((newItem as Weapon).MyWeaponAffinity == AnyRPGWeaponAffinity.Staff || (newItem as Weapon).MyWeaponAffinity == AnyRPGWeaponAffinity.Sword2H || (newItem as Weapon).MyWeaponAffinity == AnyRPGWeaponAffinity.Mace2H) {
                if (currentEquipment.ContainsKey(EquipmentSlot.OffHand) && currentEquipment[EquipmentSlot.OffHand] != null) {
                    currentEquipment[EquipmentSlot.OffHand].MyCharacterButton.DequipEquipment();
                    //Unequip(newItem.equipSlot);
                    //Unequip(EquipmentSlot.OffHand);
                }
            }
        }

        // deal with offhands, and unequip any 2h mainhand
        if (newItem.equipSlot == EquipmentSlot.OffHand) {
            if (currentEquipment.ContainsKey(EquipmentSlot.MainHand) && currentEquipment[EquipmentSlot.MainHand] != null && ((currentEquipment[EquipmentSlot.MainHand] as Weapon).MyWeaponAffinity == AnyRPGWeaponAffinity.Staff || (currentEquipment[EquipmentSlot.MainHand] as Weapon).MyWeaponAffinity == AnyRPGWeaponAffinity.Sword2H || (currentEquipment[EquipmentSlot.MainHand] as Weapon).MyWeaponAffinity == AnyRPGWeaponAffinity.Mace2H)) {
                if (currentEquipment[EquipmentSlot.MainHand] != null && currentEquipment[EquipmentSlot.MainHand].MyCharacterButton != null) {
                    currentEquipment[EquipmentSlot.MainHand].MyCharacterButton.DequipEquipment();
                    //Unequip(newItem.equipSlot);
                    //Unequip(EquipmentSlot.MainHand);
                }
            }
        }

        //Debug.Log("Putting " + newItem.GetUMASlotType() + " in slot " + newItem.UMARecipe.wardrobeSlot);
        currentEquipment[newItem.equipSlot] = newItem;
        HandleItemUMARecipe(newItem);
        HandleWeaponSlot(newItem);

        // DO THIS LAST OR YOU WILL SAVE THE UMA DATA BEFORE ANYTHING IS EQUIPPED!
        // updated oldItem to null here because this call is already done in Unequip.
        // having it here also was leading to duplicate stat removal when gear was changed.
        SystemEventManager.MyInstance.NotifyOnEquipmentChanged(newItem, null);
    }

    public Equipment Unequip(EquipmentSlot equipmentSlot, int slotIndex = -1) {
        //Debug.Log("equipment manager trying to unequip item in slot " + equipmentSlot.ToString());
        if (currentEquipment.ContainsKey(equipmentSlot) && currentEquipment[equipmentSlot] != null) {
            //Debug.Log("equipment manager trying to unequip item in slot " + equipmentSlot.ToString() + "; currentEquipment has this slot key");
            if (currentEquipmentPhysicalObjects.ContainsKey(equipmentSlot)) {
                GameObject destroyObject = currentEquipmentPhysicalObjects[equipmentSlot];
                //Debug.Log("equipment manager trying to unequip item in slot " + equipmentSlot.ToString() + "; destroying object: " + destroyObject.name);
                Destroy(destroyObject);
            }
            Equipment oldItem = currentEquipment[equipmentSlot];
            // TESTING SKIP THIS STUFF IF THE PLAYER UNIT IS NOT SPAWNED BECAUSE WE ARE UNEQUIPPING A PREVIEW UNIT
            if (PlayerManager.MyInstance.MyPlayerUnitSpawned) {
                if (oldItem.UMARecipe != null && PlayerManager.MyInstance.MyAvatar != null) {
                    // Clear the item from the UMA slot on the UMA character
                    //Debug.Log("Clearing UMA slot " + oldItem.UMARecipe.wardrobeSlot);
                    //avatar.SetSlot(newItem.UMARecipe.wardrobeSlot, newItem.UMARecipe.name);
                    PlayerManager.MyInstance.MyAvatar.ClearSlot(oldItem.UMARecipe.wardrobeSlot);
                    PlayerManager.MyInstance.MyAvatar.BuildCharacter();
                }

                if (slotIndex != -1) {
                    InventoryManager.MyInstance.AddItem(oldItem, slotIndex);
                } else {
                    InventoryManager.MyInstance.AddItem(oldItem);
                }
            }
            //Debug.Log("zeroing equipment slot: " + equipmentSlot.ToString());
            currentEquipment[equipmentSlot] = null;
            if (PlayerManager.MyInstance.MyPlayerUnitSpawned) {
                SystemEventManager.MyInstance.NotifyOnEquipmentChanged(null, oldItem);
            }
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
                if ((equipment as Weapon).MyWeaponAffinity != AnyRPGWeaponAffinity.Unarmed ) {
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
}
