using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class EnvironmentalEffectArea : AbilityManager, IAbilityCaster {

        [Tooltip("Every x seconds, the effect will be applied to everyone within the effect radius")]
        [SerializeField]
        private float tickRate = 1f;

        [Tooltip("The name of the ability effect to cast on valid targets every tick")]
        [SerializeField]
        private List<string> abilityEffectNames = new List<string>();

        // a reference to the ability effect to apply to targets on tick
        private List<AbilityEffect> abilityEffects = new List<AbilityEffect>();

        // a counter to keep track of the amount of time passed since the last tick
        private float elapsedTime = 0f;

        private BoxCollider boxCollider = null;

        public GameObject UnitGameObject {
            get {
                return gameObject;
            }
        }

        public bool PerformingAbility {
            get {
                return false;
            }
        }

        // for now, all environmental effects will calculate their ability damage as if they were level 1
        public int Level {
            get {
                return 1;
            }
        }

        public string Name {
            get {
                return gameObject.name;
            }
        }

        private void Awake() {
            //Debug.Log(gameObject.name + ".EnvironmentalEffectArea.Awake()");
            GetComponentReferences();
            SetupScriptableObjects();
        }

        public void GetComponentReferences() {
            boxCollider = GetComponent<BoxCollider>();
        }


        private void FixedUpdate() {
            elapsedTime += Time.fixedDeltaTime;
            if (elapsedTime > tickRate) {
                //Debug.Log(gameObject.name + ".EnvironmentalEffectArea.FixedUpdate()");
                elapsedTime -= tickRate;
                PerformAbilityEffects();
            }
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

        public float GetPhysicalDamage() {
            return 0f;
        }

        public float GetPhysicalPower() {
            return 0f;
        }

        public float GetSpellPower() {
            return 0f;
        }

        public virtual float GetCritChance() {
            return 0f;
        }

        public bool IsTargetInMeleeRange(GameObject target) {
            return true;
        }

        public bool PerformFactionCheck(ITargetable targetableEffect, CharacterUnit targetCharacterUnit, bool targetIsSelf) {
            // environmental effects should be cast on all units, regardless of faction
            return true;
        }

        public bool IsTargetInAbilityRange(BaseAbility baseAbility, GameObject target) {
            // environmental effects only target things inside their collider, so everything is always in range
            return true;
        }

        public bool IsTargetInAbilityEffectRange(AbilityEffect abilityEffect, GameObject target) {
            // environmental effects only target things inside their collider, so everything is always in range
            return true;
        }

        public virtual bool PerformWeaponAffinityCheck(BaseAbility baseAbility) {
            return true;
        }

        public bool PerformAnimatedAbilityCheck(AnimatedAbility animatedAbility) {
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

        private void PerformAbilityEffects() {
            //Debug.Log(gameObject.name + ".EnvironmentalEffectArea.PerformAbilityEffects()");

            List<AOETargetNode> validTargets = GetValidTargets();
            foreach (AOETargetNode validTarget in validTargets) {
                foreach (AbilityEffect abilityEffect in abilityEffects) {
                    //Debug.Log(gameObject.name + ".EnvironmentalEffectArea.PerformAbilityEffects(): casting " + abilityEffect.MyName);

                    abilityEffect.Cast(this, validTarget.targetGameObject, null, new AbilityEffectOutput());
                }
            }
        }

        public GameObject ReturnTarget(AbilityEffect abilityEffect, GameObject target) {
            return target;
        }

        protected virtual List<AOETargetNode> GetValidTargets() {
            //Debug.Log(gameObject.name + ".EnvironmentalEffectArea.GetValidTargets()");

            Vector3 aoeSpawnCenter = transform.position;

            Collider[] colliders = new Collider[0];
            int playerMask = 1 << LayerMask.NameToLayer("Player");
            int characterMask = 1 << LayerMask.NameToLayer("CharacterUnit");
            int validMask = (playerMask | characterMask);

            //Debug.Log(MyName + ".AOEEffect.GetValidTargets(): using aoeSpawnCenter: " + aoeSpawnCenter + ", extents: " + aoeExtents);
            colliders = Physics.OverlapBox(aoeSpawnCenter, boxCollider.bounds.extents, Quaternion.identity, validMask);

            //Debug.Log("AOEEffect.Cast(): Casting OverlapSphere with radius: " + aoeRadius);
            List<AOETargetNode> validTargets = new List<AOETargetNode>();
            foreach (Collider collider in colliders) {
                //Debug.Log(gameObject.name + ".EnvironmentalEffectArea.GetValidTargets() hit: " + collider.gameObject.name + "; layer: " + collider.gameObject.layer);

                bool canAdd = true;
                if (collider.gameObject.GetComponent<CharacterUnit>() == null) {
                    canAdd = false;
                }
                /*
                foreach (AbilityEffect abilityEffect in abilityEffects) {
                    if (abilityEffect.CanUseOn(collider.gameObject, source) == false) {
                        canAdd = false;
                    }
                }
                */
                //Debug.Log(MyName + "performing AOE ability  on " + collider.gameObject);
                if (canAdd) {
                    AOETargetNode validTargetNode = new AOETargetNode();
                    validTargetNode.targetGameObject = collider.gameObject;
                    validTargets.Add(validTargetNode);
                }
            }
            //Debug.Log(gameObject.name + ".EnvironmentalEffectArea.GetValidTargets(). Valid targets count: " + validTargets.Count);
            return validTargets;
        }

        public float PerformAnimatedAbility(AnimationClip animationClip, AnimatedAbility animatedAbility, BaseCharacter targetBaseCharacter) {

            // do nothing for now
            return 0f;
        }

        public bool AbilityHit(GameObject target) {
            return true;
        }

        private void SetupScriptableObjects() {
            Debug.Log(gameObject.name + ".EnvironmentalEffectArea.SetupScriptableObjects()");
            if (SystemAbilityEffectManager.MyInstance == null) {
                Debug.LogError(gameObject.name + ": SystemAbilityEffectManager not found.  Is the GameManager in the scene?");
                return;
            }

            if (abilityEffectNames != null) {
                foreach (string abilityEffectName in abilityEffectNames) {
                    if (abilityEffectName != string.Empty) {
                        AbilityEffect tmpAbilityEffect = SystemAbilityEffectManager.MyInstance.GetResource(abilityEffectName);
                        if (tmpAbilityEffect != null) {
                            abilityEffects.Add(tmpAbilityEffect);
                        } else {
                            Debug.LogError(gameObject.name + ".EnvironmentalEffectArea.SetupScriptableObjects(): Could not find ability effect " + abilityEffectName + " while initializing " + gameObject.name + ". Check inspector.");
                        }
                    } else {
                        Debug.LogError(gameObject.name + ".EnvironmentalEffectArea.SetupScriptableObjects(): Ability Effect name was empty while initializing " + gameObject.name + ". Check inspector.");
                    }
                }
            }
        }

    }

}
