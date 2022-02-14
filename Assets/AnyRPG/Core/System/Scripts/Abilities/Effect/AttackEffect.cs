using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New AttackEffect", menuName = "AnyRPG/Abilities/Effects/AttackEffect")]
    public class AttackEffect : AmountEffect {

        [SerializeField]
        private AttackEffectProperties attackEffectProperties = new AttackEffectProperties();

        public override AbilityEffectProperties AbilityEffectProperties { get => attackEffectProperties; }

    }
}
