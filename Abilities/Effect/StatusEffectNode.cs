using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class StatusEffectNode {

        private StatusEffect statusEffect;

        private Coroutine monitorCoroutine;

        // the reference to the character stats this node sits on
        private CharacterStats characterStats;

        public StatusEffect StatusEffect { get => statusEffect; set => statusEffect = value; }
        public Coroutine MyMonitorCoroutine { get => monitorCoroutine; set => monitorCoroutine = value; }

        //public void Setup(CharacterStats characterStats, StatusEffect _statusEffect, Coroutine newCoroutine) {
        public void Setup(CharacterStats characterStats, StatusEffect _statusEffect) {
            //Debug.Log("StatusEffectNode.Setup(): " + _statusEffect.MyName);
            this.characterStats = characterStats;
            this.statusEffect = _statusEffect;
            //this.monitorCoroutine = newCoroutine;
        }

        public void CancelStatusEffect() {
            //Debug.Log("StatusEffectNode.CancelStatusEffect(): " + MyStatusEffect.MyName);
            StatusEffect.CancelEffect(characterStats.BaseCharacter as BaseCharacter);
            characterStats.HandleStatusEffectRemoval(statusEffect);
        }
    }
}
