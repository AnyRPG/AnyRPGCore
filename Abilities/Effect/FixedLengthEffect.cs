using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    //[CreateAssetMenu(fileName = "New Ability",menuName = "AnyRPG/Abilities/Effects/FixedLengthEffect")]
    // not using that for now as it will neither tick, nor complete.  that is done by directeffect/children or aoeEffect
    // MAKE ABSTRACT IN FUTURE?
    public class FixedLengthEffect : LengthEffect {

        /// <summary>
        /// the default amount of time after which we destroy any spawned prefab
        /// </summary>
        public float defaultPrefabLifetime = 10f;

        public float MyAbilityEffectObjectLifetime { get => defaultPrefabLifetime; set => defaultPrefabLifetime = value; }

        protected override void BeginMonitoring(Dictionary<PrefabProfile, GameObject> abilityEffectObjects, IAbilityCaster source, GameObject target, AbilityEffectContext abilityEffectInput) {
            //Debug.Log("FixedLengthEffect.BeginMonitoring(" + (target == null ? "null" : target.name) + ")");
            base.BeginMonitoring(abilityEffectObjects, source, target, abilityEffectInput);
            //Debug.Log("FixedLengthEffect.BeginMonitoring(); source: " + source.name);
            //source.StartCoroutine(DestroyAbilityEffectObject(abilityEffectObject, source, target, defaultPrefabLifetime, abilityEffectInput));
            CheckDestroyObjects(abilityEffectObjects, source, target, abilityEffectInput);
        }

        protected virtual void CheckDestroyObjects(Dictionary<PrefabProfile, GameObject> abilityEffectObjects, IAbilityCaster source, GameObject target, AbilityEffectContext abilityEffectInput) {
            if (source != null) {
                SystemAbilityController.MyInstance.BeginDestroyAbilityEffectObject(abilityEffectObjects, source, target, defaultPrefabLifetime, abilityEffectInput, this);
            }
        }

    }
}