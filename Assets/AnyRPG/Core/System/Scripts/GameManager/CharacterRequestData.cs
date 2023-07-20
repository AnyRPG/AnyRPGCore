using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class CharacterRequestData {

        public int spawnRequestId;
        public ICharacterRequestor characterRequestor;
        public GameMode requestMode;
        public UnitProfile unitProfile;
        public UnitControllerMode unitControllerMode;
        public int unitLevel;
        public UnitController unitController;

        public CharacterRequestData(ICharacterRequestor characterRequestor, GameMode requestMode, UnitProfile unitProfile, UnitControllerMode unitControllerMode) {
            this.characterRequestor = characterRequestor;
            this.requestMode = requestMode;
            this.unitProfile = unitProfile;
            this.unitControllerMode = unitControllerMode;
            unitLevel = -1;
        }

        public CharacterRequestData(ICharacterRequestor characterRequestor, GameMode requestMode, UnitProfile unitProfile, UnitControllerMode unitControllerMode, int unitLevel) {
            this.characterRequestor = characterRequestor;
            this.requestMode = requestMode;
            this.unitProfile = unitProfile;
            this.unitControllerMode = unitControllerMode;
            this.unitLevel = unitLevel;

        }
    }
}

