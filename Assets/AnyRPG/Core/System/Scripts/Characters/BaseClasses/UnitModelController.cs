using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace AnyRPG {
    public class UnitModelController : ConfiguredClass {

        public event System.Action OnModelReady = delegate { };

        // reference to unit
        private UnitController unitController = null;
        private GameObject unitModel = null;

        // track model
        private bool modelReady = false;

        // specific controllers
        private UMAModelController umaModelController = null;
        private MecanimModelController mecanimModelController = null;

        // game manager references
        private ObjectPooler objectPooler = null;
        private UIManager uIManager = null;

        // properties
        private Transform floatTransform = null;

        public UMAModelController UMAModelController { get => umaModelController; }
        public MecanimModelController MecanimModelController { get => mecanimModelController; }
        public bool ModelReady { get => modelReady; }

        public UnitModelController(UnitController unitController, SystemGameManager systemGameManager) {
            this.unitController = unitController;
            Configure(systemGameManager);

            umaModelController = new UMAModelController(unitController, this, systemGameManager);
            mecanimModelController = new MecanimModelController(unitController, this, systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            objectPooler = systemGameManager.ObjectPooler;
            uIManager = systemGameManager.UIManager;
        }

        public bool isBuilding() {
            return umaModelController.IsBuilding();
        }

        public void ResetSettings() {
            umaModelController.ResetSettings();
        }

        public void SpawnUnitModel() {
            //Debug.Log(gameObject.name + ".UnitController.SpawnUnitModel()");
            if (unitController.UnitProfile?.UnitPrefabProps?.ModelPrefab != null) {
                unitModel = unitController.UnitProfile.SpawnModelPrefab(unitController.transform, unitController.transform.position, unitController.transform.forward);
            }
        }

        public void FindUnitModel(Animator animator) {
            //Debug.Log(unitController.gameObject.name + ".UnitController.FindUnitModel()");
            // this may have been called from a unit which already had a model attached
            // if so, the model is the animator gameobject, since no model will have been passed to this call
            if (animator != null && unitModel == null) {
                unitModel = animator.gameObject;
            }

            umaModelController.FindUnitModel(unitModel);

        }

        public void BuildModelAppearance() {
            umaModelController.BuildModelAppearance();
        }

        public void RebuildModelAppearance() {
            // not yet implemented
            mecanimModelController.RebuildModelAppearance();
            umaModelController.RebuildModelAppearance();
        }

        /*
        public void LoadSavedAppearanceSettings(string recipeString = null, bool rebuildAppearance = false) {
            umaModelController.LoadSavedAppearanceSettings(recipeString, rebuildAppearance);
        }
        */

        public void SaveAppearanceSettings() {
            umaModelController.SaveAppearanceSettings();
        }

        public string GetAppearanceSettings() {
            if (umaModelController.DynamicCharacterAvatar != null) {
                return umaModelController.GetAppearanceString();
            }
            return string.Empty;
        }

        public void SetAnimatorOverrideController(AnimatorOverrideController animatorOverrideController) {
            umaModelController.SetAnimatorOverrideController(animatorOverrideController);
        }

        // This method does not actually equip the character, just apply models from already equipped equipment
        public void EquipEquipmentModels(CharacterEquipmentManager characterEquipmentManager) {
            //Debug.Log(unitController.gameObject.name + ".UnitModelController.EquipEquipmentModels()");
            if (characterEquipmentManager.CurrentEquipment == null || characterEquipmentManager.CurrentEquipment.Count == 0) {
                //Debug.Log(unitController.gameObject.name + ".UnitModelController.EquipCharacter(): currentEquipment == null!");
                // no point building model appearance if there was nothing equipped
                return;
            }
            foreach (EquipmentSlotProfile equipmentSlotProfile in characterEquipmentManager.CurrentEquipment.Keys) {
                if (characterEquipmentManager.CurrentEquipment[equipmentSlotProfile] != null) {
                    // armor and weapon models handling
                    EquipItemModels(characterEquipmentManager, equipmentSlotProfile, characterEquipmentManager.CurrentEquipment[equipmentSlotProfile], true, false, false);
                }
            }

            //umaModelController.BuildModelAppearance();
        }

        public void EquipItemModels(CharacterEquipmentManager characterEquipmentManager, EquipmentSlotProfile equipmentSlotProfile, Equipment equipment, bool equipModels, bool setAppearance, bool rebuildAppearance) {
            //Debug.Log(unitController.gameObject.name + ".UnitModelController.EquipItemModels(" + equipment.DisplayName + ", " + equipModels + ", " + setAppearance + ", " + rebuildAppearance + ")");
            if (characterEquipmentManager.CurrentEquipment == null) {
                Debug.LogError("UnitModelController.EquipItemModels(" + equipmentSlotProfile.DisplayName + "): currentEquipment is null!");
                return;
            }
            if (!characterEquipmentManager.CurrentEquipment.ContainsKey(equipmentSlotProfile)) {
                Debug.LogError("UnitModelController.EquipItemModels(" + equipmentSlotProfile.DisplayName + "): currentEquipment does not have key");
                return;
            }
            //Equipment equipment = characterEquipmentManager.CurrentEquipment[equipmentSlotProfile];
            //public void HandleWeaponSlot(Equipment newItem) {
            if (equipment == null || unitController == null) {
                //Debug.Log(gameObject.name + ".CharacterEquipmentManager.HandleWeaponSlot(): MyHoldableObjectName is empty on " + newItem.DisplayName);
                return;
            }

            if (setAppearance == true) {
                // both of these not needed if character unit not yet spawned?
                // re-enabled for newGamePanel
                // removed because this is only supposed to equip weapons and other physical prefabs
                // having it here resulted in a second application of the uma gear
                // this code now inside if condition should be safe
                umaModelController.EquipItemModels(characterEquipmentManager, equipment, rebuildAppearance);
            }

            if (equipModels == true) {
                // testing new code to prevent UKMA characters from trying to find bones before they are created.
                mecanimModelController.EquipItemModels(characterEquipmentManager, equipmentSlotProfile, equipment);
            }
        }

        public void UnequipItemModels(EquipmentSlotProfile equipmentSlot, Equipment equipment, bool unequipModels = true, bool unequipAppearance = true, bool rebuildAppearance = true) {
            //Debug.Log(unitController.gameObject.name + ".UnitModelController.UnequipItemModels(" + equipment.DisplayName + ", " + unequipModels + ", " + unequipAppearance + ", " + rebuildAppearance + ")");

            if (unequipModels == true) {
                mecanimModelController.UnequipItemModels(equipmentSlot);
            }

            if (unequipAppearance == true) {
                umaModelController.UnequipItemModels(equipment, rebuildAppearance);
            }
        }

        /*
        public void SetInitialAppearance(string appearance) {
            umaModelController.SetInitialAppearance(appearance);
        }
        */

        public void SetInitialSavedAppearance() {
            umaModelController.SetInitialSavedAppearance();
        }

        public void ConfigureUnitModel() {
            //Debug.Log(unitController.gameObject.name + "UnitModelController.ConfigureUnitModel()");

            if (unitModel != null || umaModelController.DynamicCharacterAvatar != null) {
                if (umaModelController.DynamicCharacterAvatar != null) {

                    umaModelController.InitializeModel();
                } else {
                    // this is not an UMA model, therefore it is ready and its bone structure is already created
                    SetModelReady();
                }
            }
        }

        public bool KeepMonoBehaviorEnabled(MonoBehaviour monoBehaviour) {
            return umaModelController.KeepMonoBehaviorEnabled(monoBehaviour);
        }

        public void SetAttachmentProfile(AttachmentProfile attachmentProfile) {
            mecanimModelController.SetAttachmentProfile(attachmentProfile);
        }

        public void DespawnModel() {
            //Debug.Log(unitController.gameObject.name + "UnitModelController.DespawnModel()");
            mecanimModelController.DespawnModel();
            umaModelController.DespawnModel();
            if (unitController.UnitProfile?.UnitPrefabProps?.ModelPrefab != null) {
                objectPooler.ReturnObjectToPool(unitModel);
            }
        }

        public static bool IsInLayerMask(int layer, LayerMask layermask) {
            return layermask == (layermask | (1 << layer));
        }

        public void SetDefaultLayer(string layerName) {
            //Debug.Log(unitController.gameObject.name + ".UnitModelController.SetDefaultLayer(" + layerName + ")");
            if (layerName != null && layerName != string.Empty) {
                int defaultLayer = LayerMask.NameToLayer(layerName);
                int finalmask = (1 << defaultLayer);
                if (!IsInLayerMask(unitController.gameObject.layer, finalmask)) {
                    unitController.gameObject.layer = defaultLayer;
                    //Debug.Log(gameObject.name + ".UnitController.SetDefaultLayer(): object was not set to correct layer: " + layerName + ". Setting automatically");
                }
                //Debug.Log(gameObject.name + ".UnitController.SetDefaultLayer(): unitModel: " + (unitModel == null ? "null" : unitModel.name));

                //if (unitModel != null && !IsInLayerMask(unitModel.layer, finalmask)) {
                if (unitModel != null) {
                    //if (unitModel != null && unitModel.gameObject != unitController.gameObject) {
                        // the unit model is separate from the unit controller.  It is safe to do a recursive layer set
                        SetLayerRecursive(unitModel, defaultLayer);
                    //} else if (unitModel.gameObject == unitController.gameObject) {
                        // the unit model is the gameObject.  Only renderers should have the layer set
                    //}
                }
                //Debug.Log(gameObject.name + ".UnitController.SetDefaultLayer(): model was not set to correct layer: " + layerName + ". Setting automatically");
                /*
                if (unitModel != null) {
                    Renderer[] renderers = unitModel.GetComponentsInChildren<Renderer>();
                    for (int i = 0; i < renderers.Length; i++) {
                        if (renderers[i].gameObject.layer != uIManager.IgnoreChangeLayer) {
                            //Debug.Log(unitController.gameObject.name + ".UnitController.SetDefaultLayer(): renderer " + renderers[i].gameObject.name + " was not set to correct layer: " + layerName + ". Setting automatically");
                            renderers[i].gameObject.layer = defaultLayer;
                        }
                    }
                }
                */
            }
        }

        public void SetLayerRecursive(GameObject objectName, int newLayer) {
            // set the preview unit layer to the PlayerPreview layer so the preview camera can see it and all other cameras will ignore it
            int equipmentMask = 1 << LayerMask.NameToLayer("Equipment");
            int spellMask = 1 << LayerMask.NameToLayer("SpellEffects");
            int raycastmask = 1 << LayerMask.NameToLayer("Ignore Raycast");
            int ignoreMask = (equipmentMask | spellMask | raycastmask);

            objectName.layer = newLayer;
            foreach (Transform childTransform in objectName.gameObject.GetComponentsInChildren<Transform>(true)) {
                if (!IsInLayerMask(childTransform.gameObject.layer, ignoreMask)) {
                    childTransform.gameObject.layer = newLayer;
                }
            }

        }

        public void SheathWeapons() {
            mecanimModelController.SheathWeapons();
        }

        public void HoldWeapons() {
            mecanimModelController.HoldWeapons();
        }

        public void SetModelReady() {
            //Debug.Log(unitController.gameObject.name + ".UnitModelController.SetModelReady()");
            if (modelReady == false) {

                unitController.CharacterUnit.BaseCharacter.HandleCharacterUnitSpawn();
                EquipEquipmentModels(unitController.CharacterUnit.BaseCharacter.CharacterEquipmentManager);
            }
            if (mecanimModelController.ShouldCalculateFloatHeight() || umaModelController.ShouldCalculateFloatHeight()) {
                CalculateFloatHeight();
            }
            modelReady = true;
            OnModelReady();
            unitController.SetModelReady();
        }

        public void CalculateFloatHeight() {
            unitController.FloatHeight += unitController.UnitProfile.UnitPrefabProps.FloatHeight;
            if (unitController.UnitProfile?.UnitPrefabProps?.FloatTransform != string.Empty) {
                floatTransform = unitController.transform.FindChildByRecursive(unitController.UnitProfile.UnitPrefabProps.FloatTransform);
                if (floatTransform != null) {
                    unitController.FloatHeight = floatTransform.position.y - unitController.transform.position.y;
                    if (unitController.UnitProfile.UnitPrefabProps.AddFloatHeightToTransform == true) {
                        unitController.FloatHeight += unitController.UnitProfile.UnitPrefabProps.FloatHeight;
                    }
                }
            }
            //Debug.Log(unitController.gameObject.name + ".UnitModelController.CalculateFloatHeight() new float height: " + unitController.FloatHeight);
        }
    }

}