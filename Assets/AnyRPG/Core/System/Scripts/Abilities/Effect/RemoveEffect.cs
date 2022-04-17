using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New RemoveEffect", menuName = "AnyRPG/Abilities/Effects/RemoveEffect")]
    public class RemoveEffect : AbilityEffect {

        [SerializeField]
        private RemoveEffectProperties removeEffectProperties = new RemoveEffectProperties();

        public override AbilityEffectProperties AbilityEffectProperties { get => removeEffectProperties; }


    }
}