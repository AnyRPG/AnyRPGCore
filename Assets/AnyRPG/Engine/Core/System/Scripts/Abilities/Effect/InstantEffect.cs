using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New InstantEffect", menuName = "AnyRPG/Abilities/Effects/InstantEffect")]
    public class InstantEffect : DirectEffect {

        public override Dictionary<PrefabProfile, GameObject> Cast(IAbilityCaster source, Interactable target, Interactable originalTarget, AbilityEffectContext abilityEffectContext) {
            //Debug.Log(DisplayName + ".InstantEffect.Cast()");
            if (abilityEffectContext == null) {
                abilityEffectContext = new AbilityEffectContext(source);
            }
            Dictionary<PrefabProfile, GameObject> returnObjects = base.Cast(source, target, originalTarget, abilityEffectContext);

            PerformAbilityHit(source, target, abilityEffectContext);
            return returnObjects;
        }

    }
}