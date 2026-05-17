using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AnyRPG {
    public interface ICharacterRequestor {
        public void ConfigureSpawnedCharacter(UnitController unitController);
        public void PostInit(UnitController unitController);
    }
}

