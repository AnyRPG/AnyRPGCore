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

        public ModelAppearanceController(UnitController unitController, UnitModelController unitModelController, SystemGameManager systemGameManager) {
            this.unitController = unitController;
            this.unitModelController = unitModelController;
            Configure(systemGameManager);
        }

        public abstract T GetModelAppearanceController<T>() where T : ModelAppearanceController;

    }

}