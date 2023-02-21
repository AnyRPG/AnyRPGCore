using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace AnyRPG {
    public class SwappableMeshModelController : ModelAppearanceController {

        protected SwappableMeshModelOptions modelOptions = null;

        public SwappableMeshModelController(UnitController unitController, UnitModelController unitModelController, SystemGameManager systemGameManager, SwappableMeshModelOptions modelOptions)
            : base(unitController, unitModelController, systemGameManager) {
            this.modelOptions = modelOptions;
        }

        public SwappableMeshModelOptions ModelOptions { get => modelOptions; }

        public override T GetModelAppearanceController<T>() {
            return this as T;
        }

        /*
        public void SetModelOptions(SwappableMeshModelOptions modelOptions) {
            this.modelOptions = modelOptions;
        }
        */


    }

}