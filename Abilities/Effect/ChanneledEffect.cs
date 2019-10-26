using AnyRPG;
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
[CreateAssetMenu(fileName = "New ChanneledEffect", menuName = "Abilities/Effects/ChanneledEffect")]
public class ChanneledEffect : DirectEffect {

    // the amount of time to delay damage after spawning the prefab
    public float effectDelay = 0f;

    public override void Cast(BaseCharacter source, GameObject target, GameObject originalTarget, AbilityEffectOutput abilityEffectInput) {
        //Debug.Log("ChanelledEffect.Cast(" + source + ", " + (target == null ? "null" : target.name) + ")");
        if (abilityEffectInput == null) {
            abilityEffectInput = new AbilityEffectOutput();
        }
        base.Cast(source, target, originalTarget, abilityEffectInput);
        if (abilityEffectObject != null) {
            IChanneledObject lightningBoltScript = abilityEffectObject.GetComponent<IChanneledObject>();
            lightningBoltScript.MyStartObject = source.MyCharacterUnit.gameObject;
            lightningBoltScript.MyStartPosition = source.MyCharacterUnit.GetComponent<Collider>().bounds.center - source.MyCharacterUnit.transform.position;
            lightningBoltScript.MyEndObject = target.gameObject;
            lightningBoltScript.MyEndPosition = target.GetComponent<Collider>().bounds.center - target.transform.position;
            // delayed damage
            //source.StartCoroutine(PerformAbilityHitDelay(source, target, abilityEffectInput));
            source.MyCharacterAbilityManager.BeginPerformAbilityHitDelay(source, target, abilityEffectInput, this);
        }
    }

}
}
