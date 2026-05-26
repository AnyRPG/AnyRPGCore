using System;
using System.Collections.Generic;

namespace AnyRPG {

    [Serializable]
    public class CharacterAppearanceData {

        public string appearanceString;
        public List<SwappableMeshSaveData> swappableMeshSaveDataList;

        public CharacterAppearanceData(CharacterSaveData saveData) {
            appearanceString = saveData.AppearanceString;
            swappableMeshSaveDataList = saveData.SwappableMeshSaveData;
        }

        public CharacterAppearanceData() {
            appearanceString = string.Empty;
            swappableMeshSaveDataList = new List<SwappableMeshSaveData>();
        }

    }
}

