using AnyRPG;
using UnityEngine;
using UnityEngine.AI;

namespace AnyRPG {
    public class UnitModelController {

        public event System.Action OnModelReady = delegate { };

        // reference to unit
        private UnitController unitController = null;
        private GameObject unitModel = null;

        // track model
        private bool modelReady = false;

        // specific controllers
        private UMAModelController umaModelController = null;
        private MecanimModelController mecanimModelController = null;

        public UMAModelController UMAModelController { get => umaModelController; }
        public MecanimModelController MecanimModelController { get => mecanimModelController; }
        public bool ModelReady { get => modelReady; }

        public UnitModelController(UnitController unitController) {
            this.unitController = unitController;

            umaModelController = new UMAModelController(unitController, this);
            mecanimModelController = new MecanimModelController(unitController);
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

        public void FindUnitModel(Animator  animator) {
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

        public void LoadSavedAppearanceSettings(string recipeString = null, bool rebuildAppearance = false) {
            umaModelController.LoadSavedAppearanceSettings(recipeString, rebuildAppearance);
        }

        public void SaveAppearanceSettings() {
            umaModelController.SaveAppearanceSettings();
        }

        public string GetAppearanceSettings() {
            if (umaModelController.DynamicCharacterAvatar != null) {
                return umaModelController.GetAppearanceSettings();
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
                //Debug.Log(gameObject.name + ".CharacterEquipmentManager.EquipCharacter(): currentEquipment == null!");
                // no point building model appearance if there was nothing equipped
                return;
            }
            foreach (EquipmentSlotProfile equipmentSlotProfile in characterEquipmentManager.CurrentEquipment.Keys) {
                if (characterEquipmentManager.CurrentEquipment[equipmentSlotProfile] != null) {
                    // armor and weapon models handling
                    EquipItemModels(characterEquipmentManager, equipmentSlotProfile, characterEquipmentManager.CurrentEquipment[equipmentSlotProfile], false);
                }
            }
            
            umaModelController.BuildModelAppearance();
        }

        public void EquipItemModels(CharacterEquipmentManager characterEquipmentManager, EquipmentSlotProfile equipmentSlotProfile, Equipment equipment, bool rebuildAppearance) {
            //Debug.Log(unitController.gameObject.name + ".UnitModelController.EquipItemModels(" + equipment.DisplayName + ", " + rebuildAppearance + ")");
            if (characterEquipmentManager.CurrentEquipment == null) {
                Debug.LogError("CharacterEquipmentManager.HandleWeaponSlot(" + equipmentSlotProfile.DisplayName + "): currentEquipment is null!");
                return;
            }
            if (!characterEquipmentManager.CurrentEquipment.ContainsKey(equipmentSlotProfile)) {
                Debug.LogError("CharacterEquipmentManager.HandleWeaponSlot(" + equipmentSlotProfile.DisplayName + "): currentEquipment does not have key");
                return;
            }
            //Equipment equipment = characterEquipmentManager.CurrentEquipment[equipmentSlotProfile];
            //public void HandleWeaponSlot(Equipment newItem) {
            if (equipment == null || unitController == null) {
                //Debug.Log(gameObject.name + ".CharacterEquipmentManager.HandleWeaponSlot(): MyHoldableObjectName is empty on " + newItem.MyName);
                return;
            }

            // both of these not needed if character unit not yet spawned?
            umaModelController.EquipItemModels(characterEquipmentManager, equipment, rebuildAppearance);

            // testing new code to prevent UKMA characters from trying to find bones before they are created.
            mecanimModelController.EquipItemModels(characterEquipmentManager, equipmentSlotProfile, equipment);
        }

        public void UnequipItemModels(EquipmentSlotProfile equipmentSlot, Equipment equipment, bool rebuildAppearance = true) {
            mecanimModelController.UnequipItemModels(equipmentSlot);
            umaModelController.UnequipItemModels(equipment, rebuildAppearance);
        }

        public void SetInitialAppearance(string appearance) {
            umaModelController.SetInitialAppearance(appearance);
        }

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
                ObjectPooler.Instance.ReturnObjectToPool(unitModel);
            }
        }

        public static bool IsInLayerMask(int layer, LayerMask layermask) {
            return layermask == (layermask | (1 << layer));
        }

        public void SetDefaultLayer(string layerName) {
            if (layerName != null && layerName != string.Empty) {
                int defaultLayer = LayerMask.NameToLayer(layerName);
                int finalmask = (1 << defaultLayer);
                if (!IsInLayerMask(unitController.gameObject.layer, finalmask)) {
                    unitController.gameObject.layer = defaultLayer;
                    //Debug.Log(gameObject.name + ".UnitController.SetDefaultLayer(): object was not set to correct layer: " + layerName + ". Setting automatically");
                }
                //Debug.Log(gameObject.name + ".UnitController.SetDefaultLayer(): unitModel: " + (unitModel == null ? "null" : unitModel.name));
                if (unitModel != null && !IsInLayerMask(unitModel.layer, finalmask)) {
                    SystemGameManager.Instance.UIManager.SetLayerRecursive(unitModel, defaultLayer);
                    //Debug.Log(gameObject.name + ".UnitController.SetDefaultLayer(): model was not set to correct layer: " + layerName + ". Setting automatically");
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
            modelReady = true;
            OnModelReady();
            unitController.SetModelReady();
        }
    }

}