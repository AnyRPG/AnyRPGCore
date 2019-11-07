using AnyRPG;
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
[CreateAssetMenu(fileName = "New AttackEffect",menuName = "AnyRPG/Abilities/Effects/AttackEffect")]
public class AttackEffect : AmountEffect {

    /// <summary>
    /// Does the actual work of hitting the target with an ability
    /// </summary>
    /// <param name="ability"></param>
    /// <param name="source"></param>
    /// <param name="target"></param>
    public override void PerformAbilityHit(BaseCharacter source, GameObject target, AbilityEffectOutput abilityEffectInput) {
        //Debug.Log(MyAbilityEffectName + ".AttackAbility.PerformAbilityEffect(" + source.name + ", " + target.name + ")");
        if (abilityEffectInput == null) {
            //Debug.Log("AttackEffect.PerformAbilityEffect() abilityEffectInput is null!");
        }
        if (source == null || target == null) {
            // something died or despawned mid cast
            return;
        }
        int abilityFinalAmount = (int)CalculateAbilityAmount(healthBaseAmount, source, target.GetComponent<CharacterUnit>()) + (int)(abilityEffectInput.healthAmount * inputMultiplier);
        AbilityEffectOutput abilityEffectOutput = new AbilityEffectOutput();
        abilityEffectOutput.healthAmount = abilityFinalAmount;
        if (abilityFinalAmount > 0) {
            // this effect may not have any damage and only be here for spawning a prefab or making a sound
            target.GetComponent<CharacterUnit>().MyCharacter.MyCharacterCombat.TakeDamage(abilityFinalAmount, source.MyCharacterUnit.transform.position, source, CombatType.ability, CombatMagnitude.normal, MyName);
        }
        abilityEffectOutput.prefabLocation = abilityEffectInput.prefabLocation;
        base.PerformAbilityHit(source, target, abilityEffectOutput);
    }

    public override bool CanUseOn(GameObject target, BaseCharacter source) {
        //Debug.Log("AttackEffect.CanUseOn(" + (target == null ? " null" : target.name) + ", " + source.gameObject.name + ")");
        return base.CanUseOn(target, source);
    }
}
}
