using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New InstantEffect", menuName = "AnyRPG/Abilities/Effects/InstantEffect")]
    public class InstantEffect : DirectEffect {

        public override GameObject Cast(BaseCharacter source, GameObject target, GameObject originalTarget, AbilityEffectOutput abilityEffectInput) {
            //Debug.Log(MyName + ".InstantEffect.Cast()");
            if (abilityEffectInput == null) {
                abilityEffectInput = new AbilityEffectOutput();
            }
            GameObject returnObject = base.Cast(source, target, originalTarget, abilityEffectInput);

            PerformAbilityHit(source, target, abilityEffectInput);
            return returnObject;
        }

    }
}