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

        public virtual bool IsDead {
            get {
                return false;
            }
        }

        public virtual GameObject UnitGameObject {
            get {
                return gameObject;
            }
        }

        public virtual bool PerformingAbility {
            get {
                return false;
            }
        }

        // for now, all environmental effects will calculate their ability damage as if they were level 1
        public virtual int Level {
            get {
                return 1;
            }
        }

        public virtual string Name {
            get {
                return gameObject.name;
            }
        }

        public virtual void GeneratePower(IAbility ability) {
            // do nothing
        }

        public virtual AudioClip GetAnimatedAbilityHitSound() {
            return null;
        }

        public virtual List<AnimationClip> GetDefaultAttackAnimations() {
            return new List<AnimationClip>();
        }

        public virtual bool PerformLOSCheck(GameObject target, ITargetable targetable, AbilityEffectContext abilityEffectContext = null) {
            return true;
        }


        public virtual float GetMeleeRange() {
            return 1f;
        }

        public void BeginPerformAbilityHitDelay(IAbilityCaster source, GameObject target, AbilityEffectContext abilityEffectInput, ChanneledEffect channeledEffect) {
            abilityHitDelayCoroutine = StartCoroutine(PerformAbilityHitDelay(source, target, abilityEffectInput, channeledEffect));
        }

        public IEnumerator PerformAbilityHitDelay(IAbilityCaster source, GameObject target, AbilityEffectContext abilityEffectInput, ChanneledEffect channeledEffect) {
            //Debug.Log("ChanelledEffect.PerformAbilityEffectDelay()");
            float timeRemaining = channeledEffect.effectDelay;
            while (timeRemaining > 0f) {
                yield return null;
                timeRemaining -= Time.deltaTime;
            }
            channeledEffect.PerformAbilityHit(source, target, abilityEffectInput);
            abilityHitDelayCoroutine = null;
        }

        

        public virtual bool AddToAggroTable(CharacterUnit targetCharacterUnit, int usedAgroValue) {
            return false;
        }

        public virtual void GenerateAgro(CharacterUnit targetCharacterUnit, int usedAgroValue) {
            // do nothing for now
        }

        public virtual void ProcessWeaponHitEffects(AttackEffect attackEffect, GameObject target, AbilityEffectContext abilityEffectOutput) {
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

        public virtual void ProcessAbilityCoolDowns(AnimatedAbility baseAbility, float animationLength, float abilityCoolDown) {
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


        public virtual float GetAnimationLengthMultiplier() {
            // environmental effects don't need casting animations
            // this is a multiplier, so needs to be one for normal damage
            return 1f;
        }

        public virtual float GetOutgoingDamageModifiers() {
            // this is a multiplier, so needs to be one for normal damage
            return 1f;
        }

        public virtual float GetPhysicalDamage() {
            return 0f;
        }

        public virtual float GetPhysicalPower() {
            return 0f;
        }

        public virtual float GetSpellPower() {
            return 0f;
        }

        public virtual float GetCritChance() {
            return 0f;
        }

        public virtual float GetSpeed() {
            return 1f;
        }

        public virtual bool IsTargetInMeleeRange(GameObject target) {
            return true;
        }

        public virtual bool PerformFactionCheck(ITargetable targetableEffect, CharacterUnit targetCharacterUnit, bool targetIsSelf) {
            // environmental effects should be cast on all units, regardless of faction
            return true;
        }

        public virtual bool IsTargetInAbilityRange(BaseAbility baseAbility, GameObject target, AbilityEffectContext abilityEffectContext = null) {
            // environmental effects only target things inside their collider, so everything is always in range
            return true;
        }

        public virtual bool IsTargetInAbilityEffectRange(AbilityEffect abilityEffect, GameObject target, AbilityEffectContext abilityEffectContext = null) {
            // environmental effects only target things inside their collider, so everything is always in range
            return true;
        }

        public virtual bool PerformWeaponAffinityCheck(BaseAbility baseAbility) {
            return true;
        }

        public virtual bool PerformAnimatedAbilityCheck(AnimatedAbility animatedAbility) {
            return true;
        }

        public virtual bool ProcessAnimatedAbilityHit(GameObject target, bool deactivateAutoAttack) {
            // we can now continue because everything beyond this point is single target oriented and it's ok if we cancel attacking due to lack of alive/unfriendly target
            // check for friendly target in case it somehow turned friendly mid swing
            if (target == null || deactivateAutoAttack == true) {
                //baseCharacter.MyCharacterCombat.DeActivateAutoAttack();
                return false;
            }
            return true;
        }

        public virtual GameObject ReturnTarget(AbilityEffect abilityEffect, GameObject target) {
            return target;
        }


        public virtual float PerformAnimatedAbility(AnimationClip animationClip, AnimatedAbility animatedAbility, BaseCharacter targetBaseCharacter, AbilityEffectContext abilityEffectContext) {

            // do nothing for now
            return 0f;
        }

        public virtual bool AbilityHit(GameObject target, AbilityEffectContext abilityEffectContext) {
            return true;
        }

        public virtual void OnDestroy() {

        }

    }
}