using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [CreateAssetMenu(fileName = "New AOEEffect", menuName = "AnyRPG/Abilities/Effects/AOEEffect")]
    public class AOEEffect : AbilityEffect {

        [SerializeField]
        private AOEEffectProperties aoeEffectProperties = new AOEEffectProperties();

        public override AbilityEffectProperties AbilityEffectProperties { get => aoeEffectProperties; }

    }

}