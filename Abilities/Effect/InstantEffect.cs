using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New InstantEffect", menuName = "AnyRPG/Abilities/Effects/InstantEffect")]
    public class InstantEffect : DirectEffect {

        public override Dictionary<PrefabProfile, GameObject> Cast(IAbilityCaster source, GameObject target, GameObject originalTarget, AbilityEffectContext abilityEffectInput) {
            //Debug.Log(MyName + ".InstantEffect.Cast()");
            if (abilityEffectInput == null) {
                abilityEffectInput = new AbilityEffectContext();
            }
            Dictionary<PrefabProfile, GameObject> returnObjects = base.Cast(source, target, originalTarget, abilityEffectInput);

            PerformAbilityHit(source, target, abilityEffectInput);
            return returnObjects;
        }

    }
}