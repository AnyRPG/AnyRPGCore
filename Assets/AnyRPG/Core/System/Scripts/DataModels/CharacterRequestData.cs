using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class CharacterRequestData {

        public int spawnRequestId;
        public ICharacterRequestor characterRequestor;
        public GameMode requestMode;
        //public UnitProfile unitProfile;
        //public UnitControllerMode unitControllerMode;
        public int unitLevel;
        public UnitController unitController;
        public CharacterConfigurationRequest characterConfigurationRequest;

        public CharacterRequestData(ICharacterRequestor characterRequestor, GameMode requestMode/*, UnitProfile unitProfile*//*, UnitControllerMode unitControllerMode*/, CharacterConfigurationRequest characterConfigurationRequest) {
            this.characterRequestor = characterRequestor;
            this.requestMode = requestMode;
            //this.unitProfile = unitProfile;
            //this.unitControllerMode = unitControllerMode;
            this.characterConfigurationRequest = characterConfigurationRequest;
            unitLevel = 1;
        }

    }
}

