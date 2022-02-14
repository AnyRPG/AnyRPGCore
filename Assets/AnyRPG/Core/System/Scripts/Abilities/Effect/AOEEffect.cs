using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [CreateAssetMenu(fileName = "New AOEEffect", menuName = "AnyRPG/Abilities/Effects/AOEEffect")]
    [System.Serializable]
    public class AOEEffect : FixedLengthEffect {

        [Header("AOE")]

        [SerializeField]
        private AOEEffectPropertiesNode aoeProperties = new AOEEffectPropertiesNode();

        [SerializeField]
        private AOEEffectProperties aoeEffectProperties = new AOEEffectProperties();

        public virtual AbilityEffectProperties EffectProperties { get => aoeEffectProperties; }
        public AOEEffectPropertiesNode AoeProperties { get => aoeProperties; set => aoeProperties = value; }

        public override void Convert() {

            aoeEffectProperties.GetAOEEffectProperties(this);

        }
    }

}