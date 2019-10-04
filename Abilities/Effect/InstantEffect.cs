using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "New InstantEffect", menuName = "Abilities/Effects/InstantEffect")]
public class InstantEffect : DirectEffect {

    public override void Cast(BaseCharacter source, GameObject target, GameObject originalTarget, AbilityEffectOutput abilityEffectInput) {
        //Debug.Log(MyName + ".InstantEffect.Cast()");
        if (abilityEffectInput == null) {
            abilityEffectInput = new AbilityEffectOutput();
        }
        base.Cast(source, target, originalTarget, abilityEffectInput);

        PerformAbilityHit(source, target, abilityEffectInput);
    }

}