using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class StatusEffectNode {

        private StatusEffect statusEffect = null;

        private Coroutine monitorCoroutine = null;

        private AbilityEffectContext abilityEffectContext = null;

        // the reference to the character stats this node sits on
        private CharacterStats characterStats = null;

        // keep track of any spell effect prefabs associated with this status effect.
        private Dictionary<PrefabProfile, GameObject> prefabObjects = new Dictionary<PrefabProfile, GameObject>();

        public StatusEffect StatusEffect { get => statusEffect; set => statusEffect = value; }
        public Coroutine MyMonitorCoroutine { get => monitorCoroutine; set => monitorCoroutine = value; }
        public AbilityEffectContext AbilityEffectContext { get => abilityEffectContext; set => abilityEffectContext = value; }
        public Dictionary<PrefabProfile, GameObject> PrefabObjects { get => prefabObjects; set => prefabObjects = value; }

        //public void Setup(CharacterStats characterStats, StatusEffect _statusEffect, Coroutine newCoroutine) {
        public void Setup(CharacterStats characterStats, StatusEffect statusEffect, AbilityEffectContext abilityEffectContext) {
            //Debug.Log("StatusEffectNode.Setup(): " + _statusEffect.MyName);
            this.characterStats = characterStats;
            this.statusEffect = statusEffect;
            this.abilityEffectContext = abilityEffectContext;
            //this.monitorCoroutine = newCoroutine;
        }

        public void CancelStatusEffect() {
            //Debug.Log("StatusEffectNode.CancelStatusEffect(): " + StatusEffect.DisplayName);
            if (prefabObjects != null) {
                foreach (GameObject go in prefabObjects.Values) {
                    //Debug.Log(MyName + ".LengthEffect.CancelEffect(" + targetCharacter.MyName + "): Destroy: " + go.name);
                    GameObject.Destroy(go, StatusEffect.PrefabDestroyDelay);
                }
            }
            StatusEffect.CancelEffect(characterStats.BaseCharacter);
            characterStats.HandleStatusEffectRemoval(statusEffect);
        }
    }
}
