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
        public abstract void SaveAppearanceSettings(AnyRPGSaveData saveData);
        public abstract void SetInitialSavedAppearance();
        public abstract void BuildModelAppearance();
        public abstract bool IsBuilding();
        public abstract void ResetSettings();
        public abstract void RebuildModelAppearance();
        public abstract void EquipItemModels(CharacterEquipmentManager characterEquipmentManager, Equipment equipment, bool rebuildAppearance);
        public abstract void UnequipItemModels(Equipment equipment, bool rebuildAppearance);
        public abstract void DespawnModel();
        public abstract void ConfigureUnitModel();
        public abstract bool KeepMonoBehaviorEnabled(MonoBehaviour monoBehaviour);
        public abstract bool ShouldCalculateFloatHeight();

        public virtual void FindUnitModel(GameObject unitModel) {
            // nothing to do here, this is really only necessary for UMA
        }

        public virtual void SetAnimatorOverrideController(AnimatorOverrideController animatorOverrideController) {
            // nothing to do here.  This is really only necessary for UMA
        }
    }

}