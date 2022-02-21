using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New StatusEffect", menuName = "AnyRPG/Abilities/Effects/StatusEffect")]
    public class StatusEffect : AbilityEffect /*, ILearnable*/ {

        [SerializeField]
        private StatusEffectProperties statusEffectProperties = new StatusEffectProperties();

        public override AbilityEffectProperties AbilityEffectProperties { get => statusEffectProperties; }

    }


}
