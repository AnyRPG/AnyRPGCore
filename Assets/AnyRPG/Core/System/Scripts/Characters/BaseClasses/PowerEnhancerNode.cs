using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace AnyRPG {
    [System.Serializable]
    public class PowerEnhancerNode {

        [Tooltip("Convert stats to spellpower and attack power")]
        [SerializeField]
        private List<CharacterStatToPowerNode> statToPowerConversion = new List<CharacterStatToPowerNode>();

        [Tooltip("Convert stats to critical strike percentage")]
        [SerializeField]
        private List<CharacterStatToCritNode> statToCritRatingConversion = new List<CharacterStatToCritNode>();

        [Tooltip("Convert primary stats to secondary stats")]
        [SerializeField]
        private List<PrimaryToSecondaryStatNode> primaryToSecondaryConversion = new List<PrimaryToSecondaryStatNode>();

        [SerializeField]
        private bool powerToSpellDamage;

        [SerializeField]
        private bool powerToPhysicalDamage;

        public bool PowerToSpellDamage { get => powerToSpellDamage; set => powerToSpellDamage = value; }
        public bool PowerToPhysicalDamage { get => powerToPhysicalDamage; set => powerToPhysicalDamage = value; }
        public List<CharacterStatToPowerNode> StatToPowerConversion { get => statToPowerConversion; set => statToPowerConversion = value; }
        public List<CharacterStatToCritNode> StatToCritRatingConversion { get => statToCritRatingConversion; set => statToCritRatingConversion = value; }
        public List<PrimaryToSecondaryStatNode> PrimaryToSecondaryConversion { get => primaryToSecondaryConversion; set => primaryToSecondaryConversion = value; }
    }

}