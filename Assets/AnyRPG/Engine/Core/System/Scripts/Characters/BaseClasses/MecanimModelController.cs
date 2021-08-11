using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace AnyRPG {
    public class MecanimModelController {
        
        // reference to unit
        private UnitController unitController = null;

        // need a local reference to this for preview characters which don't have a way to reference back to the base character to find this
        protected AttachmentProfile attachmentProfile;

        private Dictionary<EquipmentSlotProfile, Dictionary<AttachmentNode, GameObject>> currentEquipmentPhysicalObjects = new Dictionary<EquipmentSlotProfile, Dictionary<AttachmentNode, GameObject>>();

        public MecanimModelController(UnitController unitController) {
            this.unitController = unitController;
        }

        public void SetAttachmentProfile(AttachmentProfile attachmentProfile) {
            this.attachmentProfile = attachmentProfile;
        }

        public void EquipItemModels(CharacterEquipmentManager characterEquipmentManager, EquipmentSlotProfile equipmentSlotProfile, Equipment equipment) {
                
                SpawnEquipmentObjects(equipmentSlotProfile, equipment);
                if (unitController?.UnitAnimator != null) {
                    unitController.UnitAnimator.HandleEquipmentChanged(equipment, null);
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
                                    Transform targetBone = unitController.gameObject.transform.FindChildByRecursive(attachmentPointNode.TargetBone);

                                    if (targetBone != null) {
                                        //Debug.Log("EquipmentManager.HandleWeaponSlot(): " + newItem.name + " has a physical prefab. targetbone is not null: equipSlot: " + newItem.equipSlot);
                                        GameObject newEquipmentPrefab = ObjectPooler.Instance.GetPooledObject(attachmentNode.HoldableObject.Prefab, targetBone);
                                        //holdableObjects.Add(attachmentNode.MyHoldableObject, newEquipmentPrefab);
                                        holdableObjects.Add(attachmentNode, newEquipmentPrefab);
                                        //currentEquipmentPhysicalObjects[equipmentSlotProfile] = newEquipmentPrefab;

                                        newEquipmentPrefab.transform.localScale = attachmentNode.HoldableObject.Scale;
                                        if (unitController?.CharacterUnit?.BaseCharacter.CharacterCombat != null && unitController?.CharacterUnit?.BaseCharacter.CharacterCombat.GetInCombat() == true) {
                                            HoldObject(newEquipmentPrefab, attachmentNode, unitController.gameObject);
                                        } else {
                                            SheathObject(newEquipmentPrefab, attachmentNode, unitController.gameObject);
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

        public void SheathObject(GameObject go, AttachmentNode attachmentNode, GameObject searchObject) {
            if (searchObject == null) {
                //Debug.Log(gameObject + ".CharacterEquipmentManager.SheathObject(): searchObject is null");
                return;
            }
            if (attachmentNode == null || attachmentNode.HoldableObject == null) {
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
                if (unitController?.CharacterUnit?.BaseCharacter?.UnitProfile?.UnitPrefabProps?.AttachmentProfile != null) {
                    if (unitController.CharacterUnit.BaseCharacter.UnitProfile.UnitPrefabProps.AttachmentProfile.AttachmentPointDictionary.ContainsKey(attachmentNode.PrimaryAttachmentName)) {
                        return unitController.CharacterUnit.BaseCharacter.UnitProfile.UnitPrefabProps.AttachmentProfile.AttachmentPointDictionary[attachmentNode.PrimaryAttachmentName];
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
                if (unitController?.CharacterUnit?.BaseCharacter?.UnitProfile?.UnitPrefabProps?.AttachmentProfile != null) {
                    if (unitController.CharacterUnit.BaseCharacter.UnitProfile.UnitPrefabProps.AttachmentProfile.AttachmentPointDictionary.ContainsKey(attachmentNode.UnsheathedAttachmentName)) {
                        return unitController.CharacterUnit.BaseCharacter.UnitProfile.UnitPrefabProps.AttachmentProfile.AttachmentPointDictionary[attachmentNode.UnsheathedAttachmentName];
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

        public void SheathWeapons() {
            // loop through all the equipmentslots and check if they have equipment that is of type weapon
            //if they do, run sheathobject on that slot

            foreach (EquipmentSlotProfile equipmentSlotProfile in unitController.CharacterUnit.BaseCharacter.CharacterEquipmentManager.CurrentEquipment.Keys) {
                if (unitController.CharacterUnit.BaseCharacter.CharacterEquipmentManager.CurrentEquipment[equipmentSlotProfile] != null
                    && currentEquipmentPhysicalObjects.ContainsKey(equipmentSlotProfile)) {
                    //foreach (KeyValuePair<PrefabProfile, GameObject> holdableObjectReference in currentEquipmentPhysicalObjects[equipmentSlotProfile]) {
                    foreach (KeyValuePair<AttachmentNode, GameObject> holdableObjectReference in currentEquipmentPhysicalObjects[equipmentSlotProfile]) {
                        SheathObject(holdableObjectReference.Value, holdableObjectReference.Key, unitController.gameObject);
                        //SheathObject(currentEquipmentPhysicalObjects[equipmentSlotProfileName], currentEquipment[equipmentSlotProfileName].MyHoldableObjectName, playerUnitObject);
                    }

                }
            }
        }

        public void HoldWeapons() {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterEquipmentManager.HoldWeapons()");

            // when mounted, weapons should stay sheathed
            if (unitController?.Mounted == true) {
                return;
            }

            foreach (EquipmentSlotProfile equipmentSlotProfile in unitController.CharacterUnit.BaseCharacter.CharacterEquipmentManager.CurrentEquipment.Keys) {
                if (unitController.CharacterUnit.BaseCharacter.CharacterEquipmentManager.CurrentEquipment[equipmentSlotProfile] != null
                    && currentEquipmentPhysicalObjects.ContainsKey(equipmentSlotProfile)) {
                    //foreach (KeyValuePair<PrefabProfile, GameObject> holdableObjectReference in currentEquipmentPhysicalObjects[equipmentSlotProfile]) {
                    foreach (KeyValuePair<AttachmentNode, GameObject> holdableObjectReference in currentEquipmentPhysicalObjects[equipmentSlotProfile]) {
                        HoldObject(holdableObjectReference.Value, holdableObjectReference.Key, unitController.gameObject);
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

        public void RemoveEquipmentObjects() {
            foreach (Dictionary<AttachmentNode, GameObject> holdableObjectReferences in currentEquipmentPhysicalObjects.Values) {
                Debug.Log("MecanimModelController.RemoveEquipmentObjects(): destroying objects ");
                foreach (GameObject holdableObjectReference in holdableObjectReferences.Values) {
                    Debug.Log("MecanimModelController.RemoveEquipmentObjects(): destroying object: " + holdableObjectReference.name);
                    ObjectPooler.Instance.ReturnObjectToPool(holdableObjectReference);
                }
            }
            currentEquipmentPhysicalObjects.Clear();
        }

        public void UnequipItemModels(EquipmentSlotProfile equipmentSlot) {
            if (currentEquipmentPhysicalObjects.ContainsKey(equipmentSlot)) {
                // LOOP THOUGH THEM INSTEAD
                foreach (KeyValuePair<AttachmentNode, GameObject> holdableObjectReference in currentEquipmentPhysicalObjects[equipmentSlot]) {
                    //GameObject destroyObject = holdableObjectReference.Value;
                    //Debug.Log("equipment manager trying to unequip item in slot " + equipmentSlot.ToString() + "; destroying object: " + destroyObject.name);
                    ObjectPooler.Instance.ReturnObjectToPool(holdableObjectReference.Value);
                }
            }
        }

        public void DespawnModel() {
            Debug.Log(unitController.gameObject.name + ".MecanimModelController.DespawnModel()");
            RemoveEquipmentObjects();
        }

    }

}