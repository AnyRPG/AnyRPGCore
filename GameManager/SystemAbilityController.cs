using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class SystemAbilityController : AbilityManager, IAbilityCaster {

        #region Singleton
        private static SystemAbilityController instance;

        public static SystemAbilityController MyInstance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<SystemAbilityController>();
                }

                return instance;
            }
        }
        #endregion

        public void BeginDestroyAbilityEffectObject(Dictionary<PrefabProfile, GameObject> abilityEffectObjects, IAbilityCaster source, GameObject target, float timer, AbilityEffectContext abilityEffectInput, FixedLengthEffect fixedLengthEffect) {
            foreach (GameObject go in abilityEffectObjects.Values) {
                abilityEffectGameObjects.Add(go);
            }
            destroyAbilityEffectObjectCoroutine = StartCoroutine(DestroyAbilityEffectObject(abilityEffectObjects, source, target, timer, abilityEffectInput, fixedLengthEffect));
        }

        public IEnumerator DestroyAbilityEffectObject(Dictionary<PrefabProfile, GameObject> abilityEffectObjects, IAbilityCaster source, GameObject target, float timer, AbilityEffectContext abilityEffectInput, FixedLengthEffect fixedLengthEffect) {
            //Debug.Log("CharacterAbilityManager.DestroyAbilityEffectObject(" + (source == null ? "null" : source.name) + ", " + (target == null ? "null" : target.name) + ", " + timer + ")");
            float timeRemaining = timer;

            // keep track of temporary elapsed time between ticks
            float elapsedTime = 0f;

            bool nullTarget = false;
            CharacterStats targetStats = null;
            if (target != null) {
                CharacterUnit _characterUnit = target.GetComponent<CharacterUnit>();
                if (_characterUnit != null) {
                    targetStats = _characterUnit.MyCharacter.CharacterStats;
                }
            } else {
                nullTarget = true;
            }

            int milliseconds = (int)((fixedLengthEffect.TickRate - (int)fixedLengthEffect.TickRate) * 1000);
            float finalTickRate = fixedLengthEffect.TickRate;
            if (finalTickRate == 0) {
                finalTickRate = timer + 1;
            }
            //Debug.Log(abilityEffectName + ".StatusEffect.Tick() milliseconds: " + milliseconds);
            //TimeSpan tickRateTimeSpan = new TimeSpan(0, 0, 0, (int)finalTickRate, milliseconds);
            //Debug.Log(abilityEffectName + ".StatusEffect.Tick() tickRateTimeSpan: " + tickRateTimeSpan);
            //fixedLengthEffect.MyNextTickTime = System.DateTime.Now + tickRateTimeSpan;
            //Debug.Log(abilityEffectName + ".FixedLengthEffect.Tick() nextTickTime: " + nextTickTime);

            while (timeRemaining > 0f) {
                yield return null;

                if (nullTarget == false && (targetStats == null || fixedLengthEffect == null)) {
                    //Debug.Log(gameObject.name + ".CharacterAbilityManager.DestroyAbilityEffectObject: BREAKING!!!!!!!!!!!!!!!!!: fixedLengthEffect: " + (fixedLengthEffect == null ? "null" : fixedLengthEffect.MyName) + "; targetstats: " + (targetStats == null ? "null" : targetStats.name));
                    break;
                }

                if (fixedLengthEffect.MyPrefabSpawnLocation != PrefabSpawnLocation.GroundTarget && fixedLengthEffect.RequiresTarget == true && (target == null || (targetStats.IsAlive == true && fixedLengthEffect.RequireDeadTarget == true) || (targetStats.IsAlive == false && fixedLengthEffect.RequiresLiveTarget == true))) {
                    //Debug.Log("BREAKING!!!!!!!!!!!!!!!!!");
                    break;
                } else {
                    timeRemaining -= Time.deltaTime;
                    if (elapsedTime > finalTickRate) {
                        //Debug.Log(abilityEffectName + ".FixedLengthEffect.Tick() TickTime!");
                        fixedLengthEffect.CastTick(source, target, abilityEffectInput);
                        elapsedTime -= finalTickRate;
                    }
                }
            }
            //Debug.Log(fixedLengthEffect.MyName + ".FixedLengthEffect.Tick() Done ticking and about to perform ability affects.");
            fixedLengthEffect.CastComplete(source, target, abilityEffectInput);
            foreach (GameObject go in abilityEffectObjects.Values) {
                if (abilityEffectGameObjects.Contains(go)) {
                    abilityEffectGameObjects.Remove(go);
                }
                Destroy(go, fixedLengthEffect.MyPrefabDestroyDelay);
            }
            abilityEffectObjects.Clear();

            destroyAbilityEffectObjectCoroutine = null;
        }

        public override void OnDestroy() {
            base.OnDestroy();
            StopAllCoroutines();
        }
    }

}
