using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public class UMAModelProvider : CharacterModelProvider {

        public override ModelAppearanceController GetAppearanceController(UnitController unitController, UnitModelController unitModelController, SystemGameManager systemGameManager) {
            return new UMAModelController(unitController, unitModelController, systemGameManager);
        }

    }

}

