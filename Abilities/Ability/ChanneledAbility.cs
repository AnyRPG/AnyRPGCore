using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "NewChanneledAbility", menuName = "AnyRPG/Abilities/ChanneledAbility")]
    public class ChanneledAbility : InstantEffectAbility {


        public override bool CanUseOn(GameObject target, BaseCharacter source) {
            //Debug.Log("ChanneledAbility.CanUseOn(" + (target == null ? "null" : target.name) + ")");
            if (!base.CanUseOn(target, source)) {
                return false;
            }
            return true;
        }

        /*
        public override float OnCastTimeChanged(float currentCastTime,  BaseCharacter source, GameObject target) {
            //Debug.Log(MyName + "ChanneledAbility.OnCastTimeChanged(" + currentCastTime + ", " + source.name + ", " + (target == null ? "null" : target.name) + ")");
            base.OnCastTimeChanged(currentCastTime, source, target);
            if (currentCastTime >= nextTickTime) {
                PerformChanneledEffect(source, target);
                nextTickTime += tickRate;
            }
            return nextTickTime;
        }
        */



    }

}