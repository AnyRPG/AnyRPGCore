using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [System.Serializable]
    public class PowerEnhancerNode {

        [SerializeField]
        private float staminaToPowerRatio = 0f;

        [SerializeField]
        private float strengthToPowerRatio = 0f;

        [SerializeField]
        private float agilityToPowerRatio = 0f;

        [SerializeField]
        private float intellectToPowerRatio = 0f;

        [SerializeField]
        private float staminaToCritPerLevel = 0f;

        [SerializeField]
        private float strengthToCritPerLevel = 0f;

        [SerializeField]
        private float agilityToCritPerLevel = 0f;

        [SerializeField]
        float intellectToCritPerLevel = 0f;

        [SerializeField]
        private bool powerToSpellDamage;

        [SerializeField]
        private bool powerToPhysicalDamage;

        public float MyStaminaToPowerRatio { get => staminaToPowerRatio; set => staminaToPowerRatio = value; }
        public float MyStrengthToPowerRatio { get => strengthToPowerRatio; set => strengthToPowerRatio = value; }
        public float MyAgilityToPowerRatio { get => agilityToPowerRatio; set => agilityToPowerRatio = value; }
        public float MyIntellectToPowerRatio { get => intellectToPowerRatio; set => intellectToPowerRatio = value; }
        public float MyStaminaToCritPerLevel { get => staminaToCritPerLevel; set => staminaToCritPerLevel = value; }
        public float MyStrengthToCritPerLevel { get => strengthToCritPerLevel; set => strengthToCritPerLevel = value; }
        public float MyAgilityToCritPerLevel { get => agilityToCritPerLevel; set => agilityToCritPerLevel = value; }
        public float MyIntellectToCritPerLevel { get => intellectToCritPerLevel; set => intellectToCritPerLevel = value; }
        public bool MyPowerToSpellDamage { get => powerToSpellDamage; set => powerToSpellDamage = value; }
        public bool MyPowerToPhysicalDamage { get => powerToPhysicalDamage; set => powerToPhysicalDamage = value; }
    }

}