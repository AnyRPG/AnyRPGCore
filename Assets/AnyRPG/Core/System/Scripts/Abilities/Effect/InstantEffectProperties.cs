using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    [System.Serializable]
    public class InstantEffectProperties : DirectEffectProperties {

        /*
        public void GetInstantEffectProperties(InstantEffect effect) {

            GetDirectEffectProperties(effect);
        }
        */

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