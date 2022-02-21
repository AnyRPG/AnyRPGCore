using AnyRPG;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New HealEffect", menuName = "AnyRPG/Abilities/Effects/HealEffect")]
    public class HealEffect : AbilityEffect {

        [SerializeField]
        private HealEffectProperties healEffectProperties = new HealEffectProperties();

        public override AbilityEffectProperties AbilityEffectProperties { get => healEffectProperties; }

        


    }
}