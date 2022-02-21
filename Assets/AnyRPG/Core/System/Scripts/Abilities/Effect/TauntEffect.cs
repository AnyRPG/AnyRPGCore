using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Taunt Effect", menuName = "AnyRPG/Abilities/Effects/TauntEffect")]
    public class TauntEffect : AbilityEffect {

        [SerializeField]
        private TauntEffectProperties tauntEffectProperties = new TauntEffectProperties();

        public override AbilityEffectProperties AbilityEffectProperties { get => tauntEffectProperties; }


    }
}
