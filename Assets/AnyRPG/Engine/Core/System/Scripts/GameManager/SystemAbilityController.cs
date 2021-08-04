using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class SystemAbilityController : ConfiguredMonoBehaviour, IAbilityCaster {

        private AbilityManager abilityManager = null;

        // game manager references
        ObjectPooler objectPooler = null;

        public IAbilityManager AbilityManager { get => abilityManager; }

        public override  void Init(SystemGameManager systemGame) {
            base.Init(systemGameManager);

            objectPooler = systemGameManager.ObjectPooler;

            abilityManager = new AbilityManager(this);
            SystemEventManager.StartListening("OnLevelUnload", HandleLevelUnload);
        }

        // ensure that no coroutine continues or other spell effects exist past the end of a level
        public void HandleLevelUnload(string eventName, EventParamProperties eventParamProperties) {
            foreach (Coroutine coroutine in abilityManager.DestroyAbilityEffectObjectCoroutines) {
                StopCoroutine(coroutine);
            }
            abilityManager.DestroyAbilityEffectObjectCoroutines.Clear();

            foreach (GameObject go in abilityManager.AbilityEffectGameObjects) {
                if (go != null) {
                    objectPooler.ReturnObjectToPool(go);
                }
            }
            abilityManager.AbilityEffectGameObjects.Clear();
        }

        public void BeginDestroyAbilityEffectObject(Dictionary<PrefabProfile, GameObject> abilityEffectObjects, IAbilityCaster source, Interactable target, float timer, AbilityEffectContext abilityEffectInput, FixedLengthEffect fixedLengthEffect) {
            foreach (GameObject go in abilityEffectObjects.Values) {
                abilityManager.AbilityEffectGameObjects.Add(go);
            }
            abilityManager.AddDestroyAbilityEffectObjectCoroutine(StartCoroutine(DestroyAbilityEffectObject(abilityEffectObjects, source, target, timer, abilityEffectInput, fixedLengthEffect)));
        }

        public IEnumerator DestroyAbilityEffectObject(Dictionary<PrefabProfile, GameObject> abilityEffectObjects, IAbilityCaster source, Interactable target, float timer, AbilityEffectContext abilityEffectInput, FixedLengthEffect fixedLengthEffect) {
            //Debug.Log("SystemAbilityController.DestroyAbilityEffectObject(" + (source == null ? "null" : source.AbilityManager.Name) + ", " + (target == null ? "null" : target.name) + ", " + timer + ")");
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
                    //Debug.Log("SystemAbilityController.DestroyAbilityEffectObject: BREAKING!: fixedLengthEffect: " + (fixedLengthEffect == null ? "null" : fixedLengthEffect.DisplayName) + "; targetstats: " + (targetStats == null ? "null" : targetStats.BaseCharacter.CharacterName));
                    break;
                }

                if (fixedLengthEffect.PrefabSpawnLocation != PrefabSpawnLocation.GroundTarget
                    && fixedLengthEffect.GetTargetOptions(source).RequireTarget == true
                    && (target == null || (targetStats.IsAlive == true && fixedLengthEffect.GetTargetOptions(source).RequireDeadTarget == true) || (targetStats.IsAlive == false && fixedLengthEffect.GetTargetOptions(source).RequireLiveTarget == true))) {
                    //Debug.Log("BREAKING!!!!!!!!!!!!!!!!!");
                    break;
                } else {
                    timeRemaining -= Time.deltaTime;
                    elapsedTime += Time.deltaTime;
                    if (elapsedTime > finalTickRate) {
                        //Debug.Log(fixedLengthEffect.DisplayName + ".FixedLengthEffect.Tick() TickTime!");
                        fixedLengthEffect.CastTick(source, target, abilityEffectInput);
                        elapsedTime -= finalTickRate;
                    }
                }
            }
            //Debug.Log(fixedLengthEffect.DisplayName + ".FixedLengthEffect.Tick() Done ticking and about to perform ability affects.");
            fixedLengthEffect.CastComplete(source, target, abilityEffectInput);
            foreach (GameObject go in abilityEffectObjects.Values) {
                if (abilityManager.AbilityEffectGameObjects.Contains(go)) {
                    abilityManager.AbilityEffectGameObjects.Remove(go);
                }
                if (go != null) {
                    objectPooler.ReturnObjectToPool(go, fixedLengthEffect.PrefabDestroyDelay);
                }
            }
            abilityEffectObjects.Clear();

            abilityManager.DestroyAbilityEffectObjectCoroutine = null;
        }

        public static string GetTimeText(float durationSeconds) {
            string returnText = string.Empty;
            if (durationSeconds < 60f && durationSeconds >= 0f) {
                // less than 1 minute
                returnText = ((int)durationSeconds).ToString() + " second";
                if ((int)durationSeconds != 1) {
                    returnText += "s";
                }
            } else if (durationSeconds < 3600) {
                //less than 1 hour
                returnText = ((int)(durationSeconds / 60)).ToString() + " minute";
                if (((int)durationSeconds / 60) != 1) {
                    returnText += "s";
                }
            } else if (durationSeconds > 3600f) {
                //greater than 1 hour
                returnText = ((int)(durationSeconds / 3600)).ToString() + " hour";
                if (((int)durationSeconds / 3600) != 1) {
                    returnText += "s";
                }
            }
            return returnText;
        }

        public void OnDestroy() {
            StopAllCoroutines();
            SystemEventManager.StopListening("OnLevelUnload", HandleLevelUnload);
        }
    }

}
