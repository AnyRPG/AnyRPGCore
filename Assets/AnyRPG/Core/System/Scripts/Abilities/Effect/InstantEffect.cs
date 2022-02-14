using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New InstantEffect", menuName = "AnyRPG/Abilities/Effects/InstantEffect")]
    public class InstantEffect : DirectEffect {

        [SerializeField]
        private InstantEffectProperties instantEffectProperties = new InstantEffectProperties();

        public override AbilityEffectProperties AbilityEffectProperties { get => instantEffectProperties; }

    }
}