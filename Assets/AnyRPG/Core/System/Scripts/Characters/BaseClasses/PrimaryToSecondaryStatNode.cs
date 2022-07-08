using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace AnyRPG {
    
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

}