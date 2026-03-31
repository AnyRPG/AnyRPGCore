using System;
using System.Collections;
using System.Collections.Generic;

namespace AnyRPG {
    
    [Serializable]
    public class PersistentObjectSaveData {

        public string UUID = string.Empty;
        public float LocationX;
        public float LocationY;
        public float LocationZ;
        public float DirectionX;
        public float DirectionY;
        public float DirectionZ;

        //public CharacterSaveData CharacterSaveData = null;
        public InteractableSaveData InteractableSaveData = null;

    }


}