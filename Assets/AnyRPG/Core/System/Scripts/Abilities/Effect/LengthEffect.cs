using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New LengthEffect", menuName = "AnyRPG/Abilities/Effects/LengthEffect")]
    public class LengthEffect : AbilityEffect {

        [SerializeField]
        private LengthEffectProperties lengthEffectProperties = new LengthEffectProperties();

        public override AbilityEffectProperties AbilityEffectProperties { get => lengthEffectProperties; }

    }
}