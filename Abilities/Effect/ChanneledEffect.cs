using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New ChanneledEffect",menuName = "AnyRPG/Abilities/Effects/ChanneledEffect")]
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
                IChanneledObject channeledObjectScript = abilityEffectObject.GetComponent<IChanneledObject>();
                if (channeledObjectScript != null) {
                    channeledObjectScript.MyStartObject = source.MyCharacterUnit.gameObject;
                    channeledObjectScript.MyStartPosition = source.MyCharacterUnit.GetComponent<Collider>().bounds.center - source.MyCharacterUnit.transform.position;
                    channeledObjectScript.MyEndObject = target.gameObject;
                    channeledObjectScript.MyEndPosition = target.GetComponent<Collider>().bounds.center - target.transform.position;
                }
                // delayed damage
                //source.StartCoroutine(PerformAbilityHitDelay(source, target, abilityEffectInput));
                source.MyCharacterAbilityManager.BeginPerformAbilityHitDelay(source, target, abilityEffectInput, this);
            }
        }

    }
}
