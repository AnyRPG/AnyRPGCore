using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace AnyRPG {

    public abstract class ModelAppearanceController : ConfiguredClass {
        
        // reference to unit
        protected UnitController unitController = null;
        protected UnitModelController unitModelController = null;
        protected CharacterEquipmentManager characterEquipmentManager = null;


        // track the equipment that is equipped
        protected Dictionary<EquipmentSlotProfile, Equipment> equippedEquipment = new Dictionary<EquipmentSlotProfile, Equipment>();

        // game manager references
        protected SaveManager saveManager = null;

        public ModelAppearanceController(UnitController unitController, UnitModelController unitModelController, SystemGameManager systemGameManager) {
            this.unitController = unitController;
            this.unitModelController = unitModelController;
            Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            saveManager = systemGameManager.SaveManager;
        }

        public abstract T GetModelAppearanceController<T>() where T : ModelAppearanceController;
        public abstract void SaveAppearanceSettings(/*ISaveDataOwner saveDataOwner, */AnyRPGSaveData saveData);
        public abstract void SetInitialSavedAppearance(AnyRPGSaveData saveData);
        public abstract void BuildModelAppearance();
        public abstract bool IsBuilding();
        public abstract void ResetSettings();
        public abstract void DespawnModel();
        public abstract void ConfigureUnitModel();
        public abstract bool KeepMonoBehaviorEnabled(MonoBehaviour monoBehaviour);
        public abstract bool ShouldCalculateFloatHeight();

        public virtual void Initialize() {
            characterEquipmentManager = unitModelController.CharacterEquipmentManager;
        }

        protected virtual void SynchronizeEquipmentDictionaryKeys() {
            foreach (EquipmentSlotProfile equipmentSlotProfile in characterEquipmentManager.CurrentEquipment.Keys) {
                if (equippedEquipment.ContainsKey(equipmentSlotProfile) == false) {
                    equippedEquipment.Add(equipmentSlotProfile, null);
                }
            }
        }

        protected virtual Equipment GetEquipmentForSlot(EquipmentSlotProfile equipmentSlotProfile) {

            if (unitModelController.SuppressEquipment == true) {
                return null;
            }

            return characterEquipmentManager.CurrentEquipment[equipmentSlotProfile];
        }

        private int RebuildSlotAppearance(EquipmentSlotProfile equipmentSlotProfile, Equipment equipment) {
            if (equipment == equippedEquipment[equipmentSlotProfile]) {
                // equipment spawned is the same as what is the character equipment manager, nothing to do
                return 0;
            }

            // remove unmatching equipment
            UnequipItemModels(equipmentSlotProfile);

            // spawn any needed objects
            EquipItemModels(equipmentSlotProfile, equipment);

            return 1;
        }

        protected virtual void UnequipItemModels(EquipmentSlotProfile equipmentSlotProfile) {
            equippedEquipment[equipmentSlotProfile] = null;
        }

        public virtual void EquipItemModels(EquipmentSlotProfile equipmentSlotProfile, Equipment equipment) {
            equippedEquipment[equipmentSlotProfile] = equipment;
        }

        public virtual int RebuildModelAppearance() {
            //Debug.Log($"{unitController.gameObject.name}.ModelAppearanceController.RebuildModelAppearance()");

            SynchronizeEquipmentDictionaryKeys();

            int updateCount = 0;
            foreach (EquipmentSlotProfile equipmentSlotProfile in characterEquipmentManager.CurrentEquipment.Keys) {
                updateCount += RebuildSlotAppearance(equipmentSlotProfile, GetEquipmentForSlot(equipmentSlotProfile));
            }
            //Debug.Log($"{unitController.gameObject.name}.ModelAppearanceController.RebuildModelAppearance() " + updateCount + " updates");
            return updateCount;
        }


        public virtual void FindUnitModel(GameObject unitModel) {
            // nothing to do here, this is really only necessary for UMA
        }

        public virtual void SetAnimatorOverrideController(AnimatorOverrideController animatorOverrideController) {
            // nothing to do here.  This is really only necessary for UMA
        }
    }

}