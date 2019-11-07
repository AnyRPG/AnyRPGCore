using AnyRPG;
﻿using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
[CreateAssetMenu(fileName = "New ResurrectEffect",menuName = "AnyRPG/Abilities/Effects/ResurrectEffect")]
public class ResurrectEffect : InstantEffect {

    /// <summary>
    /// Does the actual work of hitting the target with an ability
    /// </summary>
    /// <param name="ability"></param>
    /// <param name="source"></param>
    /// <param name="target"></param>
    public override void PerformAbilityHit(BaseCharacter source, GameObject target, AbilityEffectOutput abilityEffectInput) {
        //Debug.Log(resourceName + ".ResurrectEffect.PerformAbilityEffect(" + source.name + ", " + (target == null ? "null" : target.name) + ") effect: " + resourceName);
        AbilityEffectOutput abilityEffectOutput = new AbilityEffectOutput();
        abilityEffectOutput.prefabLocation = abilityEffectInput.prefabLocation;
        ResurrectTarget(target);
        base.PerformAbilityHit(source, target, abilityEffectOutput);
    }

    private void ResurrectTarget(GameObject target) {
        CharacterUnit characterUnit = target.GetComponent<CharacterUnit>();
        if (characterUnit == null) {
            //Debug.Log("CharacterUnit is null? target despawn during cast?");
            return;
        }
        characterUnit.MyCharacter.MyCharacterStats.Revive();
    }

    public override bool CanUseOn(GameObject target, BaseCharacter source) {
        if (target == null) {
            return false;
        }
        CharacterUnit characterUnit = target.GetComponent<CharacterUnit>();
        if (characterUnit == null) {
            return false;
        }
        if (characterUnit.MyCharacter.MyCharacterStats.IsAlive == false) {
            return true;
        }
        return false;
    }

    public override CharacterUnit ReturnTarget(CharacterUnit source, CharacterUnit target) {
        if (target == null) {
            //Debug.Log("Ressurect spell cast, but there was no target");
            return null;
        }
        CharacterUnit characterUnit = target.GetComponent<CharacterUnit>();
        if (characterUnit == null) {
            return null;
        }
        if (characterUnit.MyCharacter.MyCharacterStats.IsAlive == false) {
            return target;
        }
        return null;
    }

    /*
    public override void CastComplete(CharacterUnit source, GameObject target, AbilityEffectOutput abilityEffectInput) {
        base.CastComplete(source, target, abilityEffectInput);
        ResurrectTarget(target);
    }
    */


}
}