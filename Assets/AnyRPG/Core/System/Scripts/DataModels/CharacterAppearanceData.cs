using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {

    [Serializable]
    public class CharacterAppearanceData {

        public string appearanceString;
        public List<SwappableMeshSaveData> swappableMeshSaveDataList;

        public CharacterAppearanceData(AnyRPGSaveData saveData) {
            appearanceString = saveData.appearanceString;
            swappableMeshSaveDataList = saveData.swappableMeshSaveData;
        }

        public CharacterAppearanceData() {
            appearanceString = string.Empty;
            swappableMeshSaveDataList = new List<SwappableMeshSaveData>();
        }

    }
}

