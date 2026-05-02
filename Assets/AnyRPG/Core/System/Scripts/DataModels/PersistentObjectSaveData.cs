using System;

namespace AnyRPG {
    
    [Serializable]
    public class PersistentObjectSaveData {

        public string UUID = string.Empty;
        public float LocationX;
        public float LocationY;
        public float LocationZ;
        public float RotationX;
        public float RotationY;
        public float RotationZ;
        public float RotationW;

        //public CharacterSaveData CharacterSaveData = null;
        public InteractableSaveData InteractableSaveData = null;

    }


}