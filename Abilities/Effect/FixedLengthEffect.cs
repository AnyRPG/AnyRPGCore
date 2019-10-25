using AnyRPG;
﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
//[CreateAssetMenu(fileName = "New Ability", menuName = "Abilities/Effects/FixedLengthEffect")]
// not using that for now as it will neither tick, nor complete.  that is done by directeffect/children or aoeEffect
// MAKE ABSTRACT IN FUTURE?
public class FixedLengthEffect: LengthEffect {

    /// <summary>
    /// the default amount of time after which we destroy any spawned prefab
    /// </summary>
    public float defaultPrefabLifetime = 10f;

    public float MyAbilityEffectObjectLifetime { get => defaultPrefabLifetime; set => defaultPrefabLifetime = value; }

    protected override void BeginMonitoring(GameObject abilityEffectObject, BaseCharacter source, GameObject target, AbilityEffectOutput abilityEffectInput) {
        //Debug.Log("FixedLengthEffect.BeginMonitoring(" + abilityEffectObject.name + ", " + (target == null ? "null" : target.name) + ")");
        base.BeginMonitoring(abilityEffectObject, source, target, abilityEffectInput);
        //Debug.Log("FixedLengthEffect.BeginMonitoring(); source: " + source.name);
        //source.StartCoroutine(DestroyAbilityEffectObject(abilityEffectObject, source, target, defaultPrefabLifetime, abilityEffectInput));
        source.MyCharacterAbilityManager.BeginDestroyAbilityEffectObject(abilityEffectObject, source, target, defaultPrefabLifetime, abilityEffectInput, this);
    }



}
}