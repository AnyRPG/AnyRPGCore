using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace AnyRPG {
    
    [System.Serializable]
    public class ItemLevelProperties {

        [Tooltip("If true, this item level will scale to match the character level")]
        public bool dynamicLevel = false;

        [Tooltip("If true, and dynamic level is true, the item level will be frozen at the level it dropped at")]
        public bool freezeDropLevel = false;

        [Tooltip("If dynamic level is true and this value is greater than zero, the item scaling will be capped at this level")]
        public int levelCap = 0;

        [Tooltip("If dynamic level is not true, this value will be used for the static level")]
        public int itemLevel = 1;

        [Tooltip("The level the character must be to use this item")]
        public int useLevel = 1;
    }

}