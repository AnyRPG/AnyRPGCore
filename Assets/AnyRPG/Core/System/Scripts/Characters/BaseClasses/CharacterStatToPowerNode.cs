using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace AnyRPG {

    [System.Serializable]
    public class CharacterStatToPowerNode {

        [Tooltip("The name of the stat to convert")]
        [SerializeField]
        private string statName = string.Empty;

        [Tooltip("The stat is multiplied by this amount when converting it to power")]
        [SerializeField]
        private float statToPowerRatio = 0;

        public string StatName { get => statName; set => statName = value; }
        public float StatToPowerRatio { get => statToPowerRatio; set => statToPowerRatio = value; }
    }

}