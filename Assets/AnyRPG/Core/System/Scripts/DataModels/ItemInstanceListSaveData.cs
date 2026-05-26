using System;
using System.Collections.Generic;

namespace AnyRPG {

    [Serializable]
    public class ItemInstanceListSaveData {
        public List<ItemInstanceSaveData> ItemInstances = new List<ItemInstanceSaveData>();

        public ItemInstanceListSaveData() {
        }
    }

}