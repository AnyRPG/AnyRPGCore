using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace AnyRPG {

    /// <summary>
    /// This class is used when no other appearance controller is configured (such as UMA or swappable mesh)
    /// </summary>
    public class DefaultModelController : ModelAppearanceController {

        public DefaultModelController(UnitController unitController, UnitModelController unitModelController, SystemGameManager systemGameManager)
            : base(unitController, unitModelController, systemGameManager) {
        }

        public override T GetModelAppearanceController<T>() {
            return this as T;
        }

        public override void SaveAppearanceSettings(AnyRPGSaveData saveData) {
        }

        public override void SetInitialSavedAppearance(AnyRPGSaveData saveData) {
            // do nothing
        }

        public override void BuildModelAppearance() {
            // nothing to do here for now
        }

        public override bool IsBuilding() {
            return false;
        }

        public override void ResetSettings() {
            // nothing to do here for now
        }

        public override void RebuildModelAppearance() {
            // nothing to do here for now
        }

        public override void EquipItemModels(CharacterEquipmentManager characterEquipmentManager, Equipment equipment, bool rebuildAppearance) {
            // nothing to do here for now
        }

        public override void UnequipItemModels(Equipment equipment, bool rebuildAppearance) {
            // nothing to do here for now
        }

        public override void DespawnModel() {
            // nothing to do here for now
        }

        public override void ConfigureUnitModel() {
            if (unitModelController.UnitModel == null) {
                return;
            }

            unitModelController.SetModelReady();
        }

        public override bool KeepMonoBehaviorEnabled(MonoBehaviour monoBehaviour) {
            return false;
        }

        public override bool ShouldCalculateFloatHeight() {
            // this is already calculated in the mecanimModelController so no need perform any check here
            return false;
        }

    }

}