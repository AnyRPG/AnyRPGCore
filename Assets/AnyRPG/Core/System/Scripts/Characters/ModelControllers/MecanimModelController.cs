using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace AnyRPG {
    public class MecanimModelController : ConfiguredClass {
        
        // reference to unit
        private UnitController unitController = null;
        private UnitModelController unitModelController = null;
        private CharacterEquipmentManager characterEquipmentManager = null;

        // need a local reference to this for preview characters which don't have a way to reference back to the base character to find this
        protected AttachmentProfile attachmentProfile;

        private int unitPreviewLayer = 0;
        private int equipmentLayer = 0;
        private int setLayerIgnoreMask = 0;

        // rebuilds should be queued in the case that an UMA update is in progress to avoid a situation where the skeleton is not available
        //private bool rebuildQueued = false;

        // game manager references
        private ObjectPooler objectPooler = null;

        // track the actual prefabs that are equipped
        private Dictionary<EquipmentSlotProfile, Dictionary<AttachmentNode, GameObject>> currentEquipmentPhysicalObjects = new Dictionary<EquipmentSlotProfile, Dictionary<AttachmentNode, GameObject>>();

        // track the equipment that is equipped
        private Dictionary<EquipmentSlotProfile, Equipment> equippedEquipment = new Dictionary<EquipmentSlotProfile, Equipment>();

        public MecanimModelController(UnitController unitController, UnitModelController unitModelController, SystemGameManager systemGameManager) {
            this.unitController = unitController;
            this.unitModelController = unitModelController;

            Configure(systemGameManager);

            unitPreviewLayer = LayerMask.NameToLayer("UnitPreview");
            equipmentLayer = LayerMask.NameToLayer("Equipment");

            int spellMask = 1 << LayerMask.NameToLayer("SpellEffects");
            int raycastmask = 1 << LayerMask.NameToLayer("Ignore Raycast");
            setLayerIgnoreMask = (spellMask | raycastmask);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            objectPooler = systemGameManager.ObjectPooler;
        }

        public void Initialize() {
            characterEquipmentManager = unitModelController.CharacterEquipmentManager;
        }

        public void SetAttachmentProfile(AttachmentProfile attachmentProfile) {
            this.attachmentProfile = attachmentProfile;
        }

        private void EquipItemModels(EquipmentSlotProfile equipmentSlotProfile, Equipment equipment) {
            //Debug.Log(unitController.gameObject.name + ".MecanimModelController.EquipItemModels(" + equipmentSlotProfile.DisplayName + ", " + (equipment == null ? "null" : equipment.DisplayName) +")");

            SpawnEquipmentObjects(equipmentSlotProfile, equipment);

            if (unitController?.UnitAnimator != null) {
                unitController.UnitAnimator.HandleEquipmentChanged(equipment, null);
            }
        }

        private void SpawnEquipmentObjects(EquipmentSlotProfile equipmentSlotProfile, Equipment newEquipment) {
            //Debug.Log(unitController.gameObject.name + ".MecanimModelController.SpawnEquipmentObjects(" + equipmentSlotProfile.DisplayName + ", " + (newEquipment == null ? "null" : newEquipment.DisplayName) + ")");

            if (newEquipment == null || newEquipment.HoldableObjectList == null || newEquipment.HoldableObjectList.Count == 0|| equipmentSlotProfile == null) {
                //Debug.Log("MecanimModelController.SpawnEquipmentObjects() : FAILED TO SPAWN OBJECTS");
                return;
            }
            //Dictionary<PrefabProfile, GameObject> holdableObjects = new Dictionary<PrefabProfile, GameObject>();
            Dictionary<AttachmentNode, GameObject> holdableObjects = new Dictionary<AttachmentNode, GameObject>();
            foreach (HoldableObjectAttachment holdableObjectAttachment in newEquipment.HoldableObjectList) {
                //Debug.Log("MecanimModelController.SpawnEquipmentObjects(): " + newEquipment.ResourceName + " has an attachment");
                if (holdableObjectAttachment != null && holdableObjectAttachment.AttachmentNodes != null) {
                    //Debug.Log("MecanimModelController.SpawnEquipmentObjects(): " + newEquipment.ResourceName + " has attachment nodes");
                    foreach (AttachmentNode attachmentNode in holdableObjectAttachment.AttachmentNodes) {
                        //Debug.Log("MecanimModelController.SpawnEquipmentObjects(): " + newEquipment.ResourceName + " cycling attachment node");
                        if (attachmentNode != null && attachmentNode.EquipmentSlotProfile != null && equipmentSlotProfile == attachmentNode.EquipmentSlotProfile) {
                            //Debug.Log("MecanimModelController.SpawnEquipmentObjects(): " + newEquipment.ResourceName + " found equipmentSlotProfile");
                            if (attachmentNode.HoldableObject != null && attachmentNode.HoldableObject.Prefab != null) {
                                //Debug.Log("MecanimModelController.SpawnEquipmentObjects(): " + newEquipment.ResourceName + " has a physical prefab");
                                // attach a mesh to a bone for weapons

                                AttachmentPointNode attachmentPointNode = GetSheathedAttachmentPointNode(attachmentNode);
                                if (attachmentPointNode != null) {
                                    //Debug.Log("MecanimModelController.SpawnEquipmentObjects(): " + newEquipment.ResourceName + " found attachment point");
                                    Transform targetBone = unitController.gameObject.transform.FindChildByRecursive(attachmentPointNode.TargetBone);

                                    if (targetBone != null) {
                                        //Debug.Log(unitController.gameObject.name + ".MecanimModelController.SpawnEquipmentObjects(): " + newEquipment.name + " has a physical prefab. targetbone is not null: equipSlot: " + newEquipment.EquipmentSlotType.DisplayName);
                                        GameObject newEquipmentPrefab = objectPooler.GetPooledObject(attachmentNode.HoldableObject.Prefab, targetBone);
                                        //holdableObjects.Add(attachmentNode.MyHoldableObject, newEquipmentPrefab);
                                        holdableObjects.Add(attachmentNode, newEquipmentPrefab);
                                        //currentEquipmentPhysicalObjects[equipmentSlotProfile] = newEquipmentPrefab;

                                        if (unitController.UnitControllerMode == UnitControllerMode.Preview) {
                                            //Debug.Log("unit preview layer");
                                            LayerUtility.SetMeshRendererLayerRecursive(newEquipmentPrefab, unitPreviewLayer, setLayerIgnoreMask);
                                        } else {
                                            //Debug.Log("not unit preview layer");
                                            LayerUtility.SetMeshRendererLayerRecursive(newEquipmentPrefab, equipmentLayer, setLayerIgnoreMask);
                                        }
                                        newEquipmentPrefab.transform.localScale = attachmentNode.HoldableObject.Scale;
                                        if (unitController?.CharacterUnit?.BaseCharacter.CharacterCombat != null && unitController?.CharacterUnit?.BaseCharacter.CharacterCombat.GetInCombat() == true) {
                                            HoldObject(newEquipmentPrefab, attachmentNode, unitController.gameObject);
                                        } else {
                                            SheathObject(newEquipmentPrefab, attachmentNode, unitController.gameObject);
                                        }
                                    } else {
                                        Debug.Log("MecanimModelController.SpawnEquipmentObjects(). We could not find the target bone " + attachmentPointNode.TargetBone + " when trying to Equip " + newEquipment.ResourceName);
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
            equippedEquipment[equipmentSlotProfile] = newEquipment;
        }

        public void SetLayerRecursive(GameObject objectName, int newLayer) {
            // set the preview unit layer to the PlayerPreview layer so the preview camera can see it and all other cameras will ignore it
            int spellMask = 1 << LayerMask.NameToLayer("SpellEffects");
            int raycastmask = 1 << LayerMask.NameToLayer("Ignore Raycast");
            int ignoreMask = (spellMask | raycastmask);

            objectName.layer = newLayer;
            foreach (MeshRenderer meshRenderer in objectName.gameObject.GetComponentsInChildren<MeshRenderer>(true)) {
                if (!LayerUtility.IsInLayerMask(meshRenderer.gameObject.layer, ignoreMask)) {
                    meshRenderer.gameObject.layer = newLayer;
                }
            }

        }

        public void SheathObject(GameObject go, AttachmentNode attachmentNode, GameObject searchObject) {
            if (searchObject == null) {
                //Debug.Log(gameObject + ".MecanimModelController.SheathObject(): searchObject is null");
                return;
            }
            if (attachmentNode == null || attachmentNode.HoldableObject == null) {
                //Debug.Log(gameObject + ".MecanimModelController.SheathObject(): MyHoldableObjectName is empty");
                return;
            }
            if (go == null) {
                //Debug.Log(gameObject + ".MecanimModelController.SheathObject(): gameObject is null is null");
                return;
            }
            AttachmentPointNode attachmentPointNode = GetSheathedAttachmentPointNode(attachmentNode);
            if (attachmentPointNode != null) {
                Transform targetBone = searchObject.transform.FindChildByRecursive(attachmentPointNode.TargetBone);
                if (targetBone != null) {
                    //Debug.Log(gameObject + ".MecanimModelController.SheathObject(): targetBone is NOT null: " + holdableObject.MySheathedTargetBone);
                    go.transform.parent = targetBone;
                    go.transform.localPosition = attachmentPointNode.Position;
                    go.transform.localEulerAngles = attachmentPointNode.Rotation;
                } else {
                    //Debug.Log(gameObject + ".MecanimModelController.SheathObject(): targetBone is null: " + holdableObject.MySheathedTargetBone);
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
                    //Debug.Log(gameObject.name + ".MecanimModelController.GetSheathedAttachmentPointNode(): could not get attachment profile from prefabprofile");
                }
            }
            // enable for troubleshooting only.  It gets spammy with beast units that don't have attachments.
            //Debug.Log("MecanimModelController.GetSheathedAttachmentPointNode(): Unable to return attachment point node!");
            return null;
        }

        public AttachmentPointNode GetHeldAttachmentPointNode(AttachmentNode attachmentNode) {
            //Debug.Log(gameObject.name + ".MecanimModelController.GetHeldAttachmentPointNode()");
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
            //Debug.Log(gameObject + ".MecanimModelController.HoldObject(" + go.name + ", " + holdableObjectName + ", " + searchObject.name + ")");

            if (attachmentNode == null || attachmentNode.HoldableObject == null || go == null || searchObject == null) {
                //Debug.Log(gameObject + ".MecanimModelController.HoldObject(): MyHoldableObjectName is empty");
                return;
            }

            AttachmentPointNode attachmentPointNode = GetHeldAttachmentPointNode(attachmentNode);
            if (attachmentPointNode?.TargetBone == null || attachmentPointNode.TargetBone == string.Empty) {
                // disabled message because some equipment (like quivers) does not have held attachment points intentionally because it should stay in the same place in combat
                //Debug.Log(gameObject + ".MecanimModelController.HoldObject(): Unable to get attachment point " + attachmentNode.UnsheathedAttachmentName);
                return;
            }

            Transform targetBone = searchObject.transform.FindChildByRecursive(attachmentPointNode.TargetBone);
            if (targetBone == null) {
                Debug.Log("MecanimModelController.HoldObject(): Unable to find target bone : " + attachmentPointNode.TargetBone + " while holding " + attachmentNode.HoldableObject.ResourceName);
                return;
            }

            //Debug.Log(gameObject + ".MecanimModelController.HoldObject(): targetBone: " + targetBone + "; position: " + holdableObject.MyPosition + "; holdableObject.MyPhysicalRotation: " + holdableObject.MyRotation);
            go.transform.parent = targetBone;
            go.transform.localPosition = attachmentPointNode.Position;
            if (attachmentPointNode.RotationIsGlobal) {
                go.transform.rotation = Quaternion.LookRotation(targetBone.transform.forward) * Quaternion.Euler(attachmentPointNode.Rotation);
            } else {
                go.transform.localEulerAngles = attachmentPointNode.Rotation;
            }

        }

        public void SheathWeapons() {
            // loop through all the equipmentslots and check if they have equipment that is of type weapon
            //if they do, run sheathobject on that slot

            foreach (EquipmentSlotProfile equipmentSlotProfile in characterEquipmentManager.CurrentEquipment.Keys) {
                SheathWeapon(equipmentSlotProfile);
            }
        }

        private void SheathWeapon(EquipmentSlotProfile equipmentSlotProfile) {
            if (characterEquipmentManager.CurrentEquipment[equipmentSlotProfile] == null || currentEquipmentPhysicalObjects.ContainsKey(equipmentSlotProfile) == false) {
                return;
            }

            foreach (KeyValuePair<AttachmentNode, GameObject> holdableObjectReference in currentEquipmentPhysicalObjects[equipmentSlotProfile]) {
                SheathObject(holdableObjectReference.Value, holdableObjectReference.Key, unitController.gameObject);
            }
        }

        public void HoldWeapons() {
            //Debug.Log(baseCharacter.gameObject.name + ".MecanimModelController.HoldWeapons()");

            // when mounted, weapons should stay sheathed
            if (unitController?.Mounted == true) {
                return;
            }

            foreach (EquipmentSlotProfile equipmentSlotProfile in characterEquipmentManager.CurrentEquipment.Keys) {
                HoldWeapon(equipmentSlotProfile);
            }
        }

        private void HoldWeapon(EquipmentSlotProfile equipmentSlotProfile) {
            if (characterEquipmentManager.CurrentEquipment[equipmentSlotProfile] == null || currentEquipmentPhysicalObjects.ContainsKey(equipmentSlotProfile) == false) {
                return;
            }

            foreach (KeyValuePair<AttachmentNode, GameObject> holdableObjectReference in currentEquipmentPhysicalObjects[equipmentSlotProfile]) {
                HoldObject(holdableObjectReference.Value, holdableObjectReference.Key, unitController.gameObject);
            }
        }

        private void SynchronizeEquipmentDictionaryKeys() {
            foreach (EquipmentSlotProfile equipmentSlotProfile in characterEquipmentManager.CurrentEquipment.Keys) {
                if (equippedEquipment.ContainsKey(equipmentSlotProfile) == false) {
                    equippedEquipment.Add(equipmentSlotProfile, null);
                }
            }
        }

        private Equipment GetEquipmentForSlot(EquipmentSlotProfile equipmentSlotProfile) {
            
            if (unitModelController.SuppressEquipment == true) {
                return null;
            }

            return characterEquipmentManager.CurrentEquipment[equipmentSlotProfile];
        }

        public void RebuildModelAppearance() {
            //Debug.Log(unitController.gameObject.name + ".MecanimModelController.RebuildModelAppearance()");

            if (unitModelController.IsBuilding() == true) {
                // let model appearance get built first (in case of UMA without bones being ready)
                return;
            }

            SynchronizeEquipmentDictionaryKeys();

            foreach (EquipmentSlotProfile equipmentSlotProfile in characterEquipmentManager.CurrentEquipment.Keys) {
                RebuildSlotAppearance(equipmentSlotProfile, GetEquipmentForSlot(equipmentSlotProfile));
            }
        }

        private void RebuildSlotAppearance(EquipmentSlotProfile equipmentSlotProfile, Equipment equipment) {
            //Debug.Log(unitController.gameObject.name + ".MecanimModelController.RebuildSlotAppearance(" + equipmentSlotProfile.ResourceName + ", " + (equipment == null ? "null" : equipment.ResourceName) + ")");

            if (equipment == equippedEquipment[equipmentSlotProfile]) {
                // equipment spawned is the same as what is the character equipment manager, nothing to do
                return;
            }

            // remove unmatching equipment
            UnequipItemModels(equipmentSlotProfile);

            // spawn any needed objects
            EquipItemModels(equipmentSlotProfile, equipment);
        }

        private void RemoveEquipmentObjects() {
            //Debug.Log(unitController.gameObject.name + ".MecanimModelController.RemoveEquipmentObjects()");
            
            List<EquipmentSlotProfile> equipmentSlots = new List<EquipmentSlotProfile>();
            equipmentSlots.AddRange(equippedEquipment.Keys);

            foreach (EquipmentSlotProfile equipmentSlotProfile in equipmentSlots) {
                UnequipItemModels(equipmentSlotProfile);
            }
        }

        private void UnequipItemModels(EquipmentSlotProfile equipmentSlot) {
            
            if (equippedEquipment[equipmentSlot] == null) {
                // nothing equipped in this slot, nothing to do
                return;
            }

            if (currentEquipmentPhysicalObjects.ContainsKey(equipmentSlot)) {
                foreach (KeyValuePair<AttachmentNode, GameObject> holdableObjectReference in currentEquipmentPhysicalObjects[equipmentSlot]) {
                    objectPooler.ReturnObjectToPool(holdableObjectReference.Value);
                }
                currentEquipmentPhysicalObjects[equipmentSlot].Clear();
            }
            equippedEquipment[equipmentSlot] = null;
        }

        public void DespawnModel() {
            //Debug.Log(unitController.gameObject.name + ".MecanimModelController.DespawnModel()");

            RemoveEquipmentObjects();
        }

    }

}