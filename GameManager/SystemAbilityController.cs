using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class SystemAbilityController : MonoBehaviour, IAbilityCaster {

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

        private AbilityManager abilityManager = null;

        public IAbilityManager AbilityManager { get => abilityManager; }

        private void Awake() {
            abilityManager = new AbilityManager(this);
    }

        public void BeginDestroyAbilityEffectObject(Dictionary<PrefabProfile, GameObject> abilityEffectObjects, IAbilityCaster source, Interactable target, float timer, AbilityEffectContext abilityEffectInput, FixedLengthEffect fixedLengthEffect) {
            foreach (GameObject go in abilityEffectObjects.Values) {
                abilityManager.AbilityEffectGameObjects.Add(go);
            }
            abilityManager.DestroyAbilityEffectObjectCoroutine = StartCoroutine(DestroyAbilityEffectObject(abilityEffectObjects, source, target, timer, abilityEffectInput, fixedLengthEffect));
        }

        public IEnumerator DestroyAbilityEffectObject(Dictionary<PrefabProfile, GameObject> abilityEffectObjects, IAbilityCaster source, Interactable target, float timer, AbilityEffectContext abilityEffectInput, FixedLengthEffect fixedLengthEffect) {
            //Debug.Log("CharacterAbilityManager.DestroyAbilityEffectObject(" + (source == null ? "null" : source.name) + ", " + (target == null ? "null" : target.name) + ", " + timer + ")");
            float timeRemaining = timer;

            // keep track of temporary elapsed time between ticks
            float elapsedTime = 0f;

            bool nullTarget = false;
            CharacterStats targetStats = null;
            if (target != null) {
                CharacterUnit _characterUnit = CharacterUnit.GetCharacterUnit(target);
                if (_characterUnit != null) {
                    targetStats = _characterUnit.BaseCharacter.CharacterStats;
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

                if (fixedLengthEffect.MyPrefabSpawnLocation != PrefabSpawnLocation.GroundTarget
                    && fixedLengthEffect.GetTargetOptions(source).RequireTarget == true
                    && (target == null || (targetStats.IsAlive == true && fixedLengthEffect.GetTargetOptions(source).RequireDeadTarget == true) || (targetStats.IsAlive == false && fixedLengthEffect.GetTargetOptions(source).RequireLiveTarget == true))) {
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
                if (abilityManager.AbilityEffectGameObjects.Contains(go)) {
                    abilityManager.AbilityEffectGameObjects.Remove(go);
                }
                Destroy(go, fixedLengthEffect.MyPrefabDestroyDelay);
            }
            abilityEffectObjects.Clear();

            abilityManager.DestroyAbilityEffectObjectCoroutine = null;
        }

        public void OnDestroy() {
            StopAllCoroutines();
        }
    }

}
