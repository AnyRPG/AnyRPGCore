using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New RainEffect", menuName = "AnyRPG/Abilities/Effects/RainEffect")]
    public class RainEffect : AOEEffect {

        [SerializeField]
        private RainEffectProperties rainEffectProperties = new RainEffectProperties();

        public override AbilityEffectProperties EffectProperties { get => rainEffectProperties; }


    }
}