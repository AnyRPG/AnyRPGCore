using AnyRPG;
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
[CreateAssetMenu(fileName = "NewChanneledAbility", menuName = "Abilities/ChanneledAbility")]
public class ChanneledAbility : InstantEffectAbility {
    // every x seconds, apply ability effects
    [SerializeField]
    private float tickRate;

    private float nextTickTime = 0f;

    public List<AbilityEffect> channeledAbilityEffects = new List<AbilityEffect>();

    private List<GameObject> activeAbilityEffectObjects = new List<GameObject>();

    /*
    public override void Cast(GameObject source, GameObject target) {
        // do nothing for now
    }
    */

    public override bool CanUseOn(GameObject target, BaseCharacter source) {
        //Debug.Log("ChanneledAbility.CanUseOn(" + target.name + ")");
        if (!base.CanUseOn(target, source)) {
            return false;
        }
        return true;
    }

    public override void StartCasting(BaseCharacter source) {
        //Debug.Log("ChanneledAbility.OnCastStart()");
        base.StartCasting(source);
        nextTickTime = 0;
        //nextTickTime = tickRate;
    }

    public override void OnCastTimeChanged(float currentCastTime, BaseCharacter source, GameObject target) {
        //Debug.Log("ChanneledAbility.OnCastTimeChanged(" + currentCastTime + ", " + source.name + ", " + target.name + ")");
        base.OnCastTimeChanged(currentCastTime, source, target);
        if (currentCastTime >= nextTickTime) {
            PerformChanneledEffect(source, target);
            nextTickTime += tickRate;
        }
    }

    public virtual void PerformChanneledEffect(BaseCharacter source, GameObject target) {
        //Debug.Log("BaseAbility.PerformAbilityEffect(" + abilityName + ", " + (source == null ? "null" : source.name) + ", " + (target == null ? "null" : target.name) + ")");
        foreach (AbilityEffect abilityEffect in channeledAbilityEffects) {
            AbilityEffect _abilityEffect = SystemAbilityEffectManager.MyInstance.GetResource(abilityEffect.MyName);

            // channeled effects need to override the object lifetime so they get destroyed at the tickrate
            //_abilityEffect.MyAbilityEffectObjectLifetime = tickRate;
            _abilityEffect.Cast(source, target, target, null);
        }
    }


}

}