using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [CreateAssetMenu(fileName = "New AOEEffect", menuName = "AnyRPG/Abilities/Effects/AOEEffect")]
    [System.Serializable]
    public class AOEEffect : FixedLengthEffect {

        /*
        [Header("AOE")]


        [SerializeField]
        private AOEEffectPropertiesNode aoeProperties = new AOEEffectPropertiesNode();

        public AOEEffectPropertiesNode AoeProperties { get => aoeProperties; set => aoeProperties = value; }
        */

        [SerializeField]
        private AOEEffectProperties aoeEffectProperties = new AOEEffectProperties();

        public virtual AbilityEffectProperties EffectProperties { get => aoeEffectProperties; }

    }

}