using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Item Quality", menuName = "AnyRPG/ItemQuality")]
    [System.Serializable]
    public class ItemQuality : DescribableResource {

        // the amount of stats that you would get naturally at a level
        [SerializeField]
        private float statMultiplier;

        [SerializeField]
        private Color qualityColor;

        [SerializeField]
        private bool dynamicItemLevel;

        public float MyStatMultiplier { get => statMultiplier; set => statMultiplier = value; }
        public Color MyQualityColor { get => qualityColor; set => qualityColor = value; }
        public bool MyDynamicItemLevel { get => dynamicItemLevel; set => dynamicItemLevel = value; }
    }

}