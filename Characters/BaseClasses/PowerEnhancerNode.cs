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

        public bool MyPowerToSpellDamage { get => powerToSpellDamage; set => powerToSpellDamage = value; }
        public bool MyPowerToPhysicalDamage { get => powerToPhysicalDamage; set => powerToPhysicalDamage = value; }
        public List<CharacterStatToPowerNode> StatToPowerConversion { get => statToPowerConversion; set => statToPowerConversion = value; }
        public List<CharacterStatToCritNode> StatToCritRatingConversion { get => statToCritRatingConversion; set => statToCritRatingConversion = value; }
        public List<PrimaryToSecondaryStatNode> PrimaryToSecondaryConversion { get => primaryToSecondaryConversion; set => primaryToSecondaryConversion = value; }
    }

    [System.Serializable]
    public class PrimaryToSecondaryStatNode {

        [Tooltip("The name of the stat to convert this resource into")]
        [SerializeField]
        private SecondaryStatType secondaryStatType;

        [Tooltip("The primary stat is multiplied by this amount when converting it to the secondary stat")]
        [SerializeField]
        private float conversionRatio = 0f;

        [Tooltip("A rated conversion is for stats that are percentages.  At level 1, you need 100 points of this secondary stat for 100%, 200 points at level 2, etc. The percent chance at any level is equal to this value multiplied by (Total Stat Amount / current level).  This allows you to increase or decrease the 100 point requirement for 100% chance.")]
        [SerializeField]
        private bool ratedConversion = false;

        public SecondaryStatType SecondaryStatType { get => secondaryStatType; set => secondaryStatType = value; }
        public float ConversionRatio { get => conversionRatio; set => conversionRatio = value; }
        public bool RatedConversion { get => ratedConversion; set => ratedConversion = value; }
    }

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