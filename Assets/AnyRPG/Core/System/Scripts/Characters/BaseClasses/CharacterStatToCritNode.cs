using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace AnyRPG {

    [System.Serializable]
    public class CharacterStatToCritNode {

        [Tooltip("The name of the stat to convert")]
        [SerializeField]
        private string statName = string.Empty;

        [Tooltip("The crit chance at any level is equal to this value multiplied by (Total Stat Amount / current level).")]
        [SerializeField]
        private float statToCritRatingRatio = 0;

        public string StatName { get => statName; set => statName = value; }
        public float StatToCritRatingRatio { get => statToCritRatingRatio; set => statToCritRatingRatio = value; }
    }

}