using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class AbilityManager : MonoBehaviour {

        protected BaseAbility currentCastAbility = null;

        protected Coroutine globalCoolDownCoroutine = null;
        protected Coroutine currentCastCoroutine = null;
        protected Coroutine abilityHitDelayCoroutine = null;
        protected Coroutine destroyAbilityEffectObjectCoroutine = null;

        protected bool eventSubscriptionsInitialized = false;

        protected List<GameObject> abilityEffectGameObjects = new List<GameObject>();
        protected Dictionary<string, AbilityCoolDownNode> abilityCoolDownDictionary = new Dictionary<string, AbilityCoolDownNode>();


        public void BeginPerformAbilityHitDelay(IAbilityCaster source, GameObject target, AbilityEffectOutput abilityEffectInput, ChanneledEffect channeledEffect) {
            abilityHitDelayCoroutine = StartCoroutine(PerformAbilityHitDelay(source, target, abilityEffectInput, channeledEffect));
        }

        public IEnumerator PerformAbilityHitDelay(IAbilityCaster source, GameObject target, AbilityEffectOutput abilityEffectInput, ChanneledEffect channeledEffect) {
            //Debug.Log("ChanelledEffect.PerformAbilityEffectDelay()");
            float timeRemaining = channeledEffect.effectDelay;
            while (timeRemaining > 0f) {
                yield return null;
                timeRemaining -= Time.deltaTime;
            }
            channeledEffect.PerformAbilityHit(source, target, abilityEffectInput);
            abilityHitDelayCoroutine = null;
        }

        public void BeginDestroyAbilityEffectObject(Dictionary<PrefabProfile, GameObject> abilityEffectObjects, IAbilityCaster source, GameObject target, float timer, AbilityEffectOutput abilityEffectInput, FixedLengthEffect fixedLengthEffect) {
            foreach (GameObject go in abilityEffectObjects.Values) {
                abilityEffectGameObjects.Add(go);
            }
            destroyAbilityEffectObjectCoroutine = StartCoroutine(DestroyAbilityEffectObject(abilityEffectObjects, source, target, timer, abilityEffectInput, fixedLengthEffect));
        }

        public IEnumerator DestroyAbilityEffectObject(Dictionary<PrefabProfile, GameObject> abilityEffectObjects, IAbilityCaster source, GameObject target, float timer, AbilityEffectOutput abilityEffectInput, FixedLengthEffect fixedLengthEffect) {
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

                if (fixedLengthEffect.MyPrefabSpawnLocation != PrefabSpawnLocation.Point && fixedLengthEffect.RequiresTarget == true && (target == null || (targetStats.IsAlive == true && fixedLengthEffect.RequireDeadTarget == true) || (targetStats.IsAlive == false && fixedLengthEffect.RequiresLiveTarget == true))) {
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

        public virtual bool AddToAggroTable(CharacterUnit targetCharacterUnit, int usedAgroValue) {
            return false;
        }

        public virtual void GenerateAgro(CharacterUnit targetCharacterUnit, int usedAgroValue) {
            // do nothing for now
        }

        public virtual void ProcessWeaponHitEffects(AttackEffect attackEffect, GameObject target, AbilityEffectOutput abilityEffectOutput) {
            // do nothing.  There is no weapon on the base class
        }

        public virtual void CapturePet(UnitProfile unitProfile, GameObject target) {
            // do nothing.  environment effects cannot have pets
        }

        public virtual void PerformCastingAnimation(AnimationClip animationClip, BaseAbility baseAbility) {
            // do nothing.  environmental effects have no animations for now
        }

        public virtual void DespawnAbilityObjects() {
            // do nothing
        }

        public virtual void InitiateGlobalCooldown(float coolDownToUse) {
            // do nothing
        }

        public virtual void BeginAbilityCoolDown(BaseAbility baseAbility, float coolDownLength = -1f) {
            // do nothing
        }

        public virtual void AddPet(CharacterUnit target) {
            // do nothing, we can't have pets
        }


        public virtual void CleanupAbilityEffectGameObjects() {
            foreach (GameObject go in abilityEffectGameObjects) {
                if (go != null) {
                    Destroy(go);
                }
            }
            abilityEffectGameObjects.Clear();
        }

        public virtual void CleanupCoroutines() {
            //Debug.Log(gameObject.name + ".CharacterAbilitymanager.CleanupCoroutines()");
            if (currentCastCoroutine != null) {
                StopCoroutine(currentCastCoroutine);
                EndCastCleanup();
            }
            if (abilityHitDelayCoroutine != null) {
                StopCoroutine(abilityHitDelayCoroutine);
                abilityHitDelayCoroutine = null;
            }

            if (destroyAbilityEffectObjectCoroutine != null) {
                StopCoroutine(destroyAbilityEffectObjectCoroutine);
                destroyAbilityEffectObjectCoroutine = null;
            }
            CleanupCoolDownRoutines();

            if (globalCoolDownCoroutine != null) {
                StopCoroutine(globalCoolDownCoroutine);
                globalCoolDownCoroutine = null;
            }
        }

        public virtual void EndCastCleanup() {
            currentCastCoroutine = null;
            currentCastAbility = null;
        }

        public void CleanupCoolDownRoutines() {
            foreach (AbilityCoolDownNode abilityCoolDownNode in abilityCoolDownDictionary.Values) {
                if (abilityCoolDownNode.MyCoroutine != null) {
                    StopCoroutine(abilityCoolDownNode.MyCoroutine);
                }
            }
            abilityCoolDownDictionary.Clear();
        }


        public virtual void OnDisable() {
            CleanupEventSubscriptions();
            CleanupCoroutines();
            CleanupAbilityEffectGameObjects();
        }

        public virtual void CleanupEventSubscriptions() {
            if (!eventSubscriptionsInitialized) {
                return;
            }
            eventSubscriptionsInitialized = false;
        }

        public virtual float GetThreatModifiers() {
            return 1f;
        }

        public virtual bool IsPlayerControlled() {
            return false;
        }

    }
}