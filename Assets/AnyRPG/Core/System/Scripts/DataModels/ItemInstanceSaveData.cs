using System;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [Serializable]
    public class ItemInstanceSaveData {

        public string ItemName = string.Empty;
        public string DisplayName = string.Empty;
        public string ItemQuality = string.Empty;
        public int DropLevel;
        public long ItemInstanceId;

        public List<int> RandomSecondaryStatIndexes = new List<int>();
        public string GainCurrencyName;
        public int GainCurrencyAmount;
    }

}