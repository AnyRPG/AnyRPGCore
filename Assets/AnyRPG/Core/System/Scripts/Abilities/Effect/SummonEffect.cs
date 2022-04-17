using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New SummonEffect", menuName = "AnyRPG/Abilities/Effects/SummonEffect")]
    public class SummonEffect : AbilityEffect {

        [SerializeField]
        private SummonEffectProperties summonEffectProperties = new SummonEffectProperties();

        public override AbilityEffectProperties AbilityEffectProperties { get => summonEffectProperties; }



    }

}
