using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "NewAbility",menuName = "AnyRPG/Abilities/Ability")]
    public abstract class BaseAbility : DescribableResource, IUseable, IMoveable, IAbility {

        //public event System.Action<IAbility> OnAbilityCast = delegate { };

        // ability cannot be cast in combat if true
        [SerializeField]
        protected bool requireOutOfCombat = false;

        [SerializeField]
        private List<string> weaponAffinityNames = new List<string>();

        [SerializeField]
        protected string holdableObjectName;

        // now we have multiple objects
        [SerializeField]
        private List<string> holdableObjectNames = new List<string>();

        // holdable object prefabs are created by the animator from an animation event, not from the ability manager during cast start
        [SerializeField]
        protected bool animatorCreatePrefabs;

        // will randomly rotate through these
        [SerializeField]
        protected List<AnimationClip> animationClips = new List<AnimationClip>();

        [SerializeField]
        protected AudioClip animationHitAudioClip;

        // on hit animation
        [SerializeField]
        protected AnimationClip castingAnimationClip = null;

        [SerializeField]
        protected AudioClip castingAudioClip;

        //public AnimationClip MyAnimationClip { get => animationClip; set => animationClip = value; }
        public AnimationClip MyCastingAnimationClip { get => castingAnimationClip; set => castingAnimationClip = value; }

        [SerializeField]
        protected int requiredLevel = 1;

        // for abilities that anyone can use, like scrolls or crafting
        [SerializeField]
        protected bool useableWithoutLearning = false;

        // this spell can be cast while other spell casts are in progress
        [SerializeField]
        private bool canSimultaneousCast = false;

        // prevent special effects from triggering gcd
        // could possibly try just casting effects directly in future instead of casting the actual ability
        [SerializeField]
        private bool ignoreGlobalCoolDown = false;

        [SerializeField]
        protected int abilityManaCost = 0;

        // the cooldown in seconds before we can use this ability again.  0 means no cooldown.
        public float abilityCoolDown = 0f;

        [SerializeField]
        protected bool useAnimationCastTime = true;

        [SerializeField]
        protected float abilityCastingTime = 0f;

        // a prefab to spawn while casting
        [SerializeField]
        protected GameObject abilityCastingPrefab;

        // delay to destroy prefab after casting completes
        [SerializeField]
        protected float prefabDestroyDelay = 0f;

        [SerializeField]
        protected Vector3 prefabOffset = Vector3.zero;

        [SerializeField]
        protected Vector3 prefabRotation = Vector3.zero;

        protected GameObject abilityCastingPrefabRef;

        [SerializeField]
        private bool requiresGroundTarget = false;

        [SerializeField]
        protected Color groundTargetColor = new Color32(255, 255, 255, 255);

        // ignore requireTarget and canCast variables and use the check from the first ability effect instead
        [SerializeField]
        private bool useAbilityEffectTargetting = false;

        public bool requiresTarget;
        public bool requiresLiveTarget = true;

        [SerializeField]
        private bool requireDeadTarget;

        [SerializeField]
        protected bool canCastOnSelf = false;

        [SerializeField]
        protected bool canCastOnEnemy = false;

        [SerializeField]
        protected bool canCastOnFriendly = false;

        // if no target is given, automatically cast on the caster
        public bool autoSelfCast = false;

        [SerializeField]
        protected bool useMeleeRange;

        [SerializeField]
        protected int maxRange;

        [SerializeField]
        protected bool autoLearn = false;

        [SerializeField]
        protected bool autoAddToBars = true;

        // this will be set to the ability casting length only for direct cast abilities
        protected float castTimeMultiplier = 1f;

        public List<AbilityEffect> abilityEffects = new List<AbilityEffect>();

        protected Vector3 groundTarget = Vector3.zero;

        public int MyRequiredLevel { get => requiredLevel; }
        public bool MyAutoLearn { get => autoLearn; }
        public bool MyAutoAddToBars { get => autoAddToBars; }
        public bool MyUseableWithoutLearning { get => useableWithoutLearning; }
        public int MyAbilityManaCost { get => abilityManaCost; set => abilityManaCost = value; }
        public virtual float MyAbilityCastingTime {
            get {
                if (useAnimationCastTime == false) {
                    return abilityCastingTime;
                } else {
                    if (castingAnimationClip != null) {
                        return castingAnimationClip.length;
                    }
                    return abilityCastingTime;
                }
            }
            set => abilityCastingTime = value;
        }
        public bool MyRequiresTarget { get => requiresTarget; set => requiresTarget = value; }
        public bool MyRequiresGroundTarget { get => requiresGroundTarget; set => requiresGroundTarget = value; }
        public Color MyGroundTargetColor { get => groundTargetColor; set => groundTargetColor = value; }
        public bool MyCanCastOnSelf { get => canCastOnSelf; }
        public bool MyCanCastOnEnemy { get => canCastOnEnemy; }
        public bool MyCanCastOnFriendly { get => canCastOnFriendly; }
        public bool MyCanSimultaneousCast { get => canSimultaneousCast; set => canSimultaneousCast = value; }
        public bool MyRequireDeadTarget { get => requireDeadTarget; set => requireDeadTarget = value; }
        public bool MyIgnoreGlobalCoolDown { get => ignoreGlobalCoolDown; set => ignoreGlobalCoolDown = value; }
        public AudioClip MyCastingAudioClip { get => castingAudioClip; set => castingAudioClip = value; }
        public AudioClip MyAnimationHitAudioClip { get => animationHitAudioClip; set => animationHitAudioClip = value; }
        public List<string> MyHoldableObjectNames { get => holdableObjectNames; set => holdableObjectNames = value; }
        public bool MyAnimatorCreatePrefabs { get => animatorCreatePrefabs; set => animatorCreatePrefabs = value; }
        protected List<AnimationClip> MyAnimationClips { get => animationClips; set => animationClips = value; }
        public int MyMaxRange { get => maxRange; set => maxRange = value; }
        public bool MyUseMeleeRange { get => useMeleeRange; set => useMeleeRange = value; }
        public List<string> MyWeaponAffinityNames { get => weaponAffinityNames; set => weaponAffinityNames = value; }
        public bool MyRequireOutOfCombat { get => requireOutOfCombat; set => requireOutOfCombat = value; }

        public override string GetSummary() {
            string requireString = string.Empty;
            bool affinityMet = false;
            string colorString = string.Empty;
            string addString = string.Empty;
            if (weaponAffinityNames.Count == 0) {
                // no restrictions, automatically true
                affinityMet = true;

            } else {
                List<string> requireStrings = new List<string>();
                foreach (string _weaponAffinity in weaponAffinityNames) {
                    requireStrings.Add(_weaponAffinity);
                    if (PlayerManager.MyInstance.MyCharacter.MyCharacterEquipmentManager.HasAffinity(_weaponAffinity)) {
                        affinityMet = true;
                    }
                }
                if (affinityMet) {
                    colorString = "#ffffffff";
                } else {
                    colorString = "#ff0000ff";
                }
                addString = string.Format("\n<color={0}>Requires: {1}</color>", colorString, string.Join(",", requireStrings));
            }

            return string.Format("Cast time: {0} second(s)\nCooldown: {1} second(s)\nCost: {2} Mana\n<color=#ffff00ff>{3}</color>{4}", MyAbilityCastingTime.ToString("F1"), abilityCoolDown, abilityManaCost, description, addString);
        }

        public bool CanCast(BaseCharacter sourceCharacter) {
            if (weaponAffinityNames.Count == 0) {
                // no restrictions, automatically true
                return true;
            } else {
                if (true) {

                }
                foreach (string _weaponAffinity in weaponAffinityNames) {
                    if (sourceCharacter != null && sourceCharacter.MyCharacterEquipmentManager != null && sourceCharacter.MyCharacterEquipmentManager.HasAffinity(_weaponAffinity)) {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool Use() {
            //Debug.Log("BaseAbility.Use()");
            // prevent casting any ability without the proper weapon affinity
            if (CanCast(PlayerManager.MyInstance.MyCharacter)) {
                PlayerManager.MyInstance.MyCharacter.MyCharacterAbilityManager.BeginAbility(this);
                return true;
            }
            return false;
        }

        public virtual bool Cast(BaseCharacter sourceCharacter, GameObject target, Vector3 groundTarget) {
            //Debug.Log(resourceName + ".BaseAbility.Cast(" + sourceCharacter.name + ", " + (target == null ? "null" : target.name) + ", " + groundTarget + ")");
            if (!CanCast(sourceCharacter)) {
                //Debug.Log(resourceName + ".BaseAbility.Cast(" + sourceCharacter.name + ", " + (target == null ? "null" : target.name) + ", " + groundTarget + "): CAN'T CAST!!!");
                //CombatLogUI.MyInstance.WriteCombatMessage("BaseAbility.Cast(): You do not have the right weapon to cast: " + MyName);
                return false;
            }

            if (sourceCharacter != null && sourceCharacter.MyCharacterAbilityManager != null) {
                sourceCharacter.MyCharacterAbilityManager.BeginAbilityCoolDown(this);
            }

            ProcessAbilityPrefabs(sourceCharacter);
            ProcessGCDAuto(sourceCharacter);

            return true;
            // notify subscribers
            //OnAbilityCast(this);
        }

        public virtual void ProcessGCDAuto(BaseCharacter sourceCharacter) {
            ProcessGCDManual(sourceCharacter);
        }

        public virtual void ProcessGCDManual(BaseCharacter sourceCharacter, float usedCoolDown = 0f) {
            if (MyCanSimultaneousCast == false && MyIgnoreGlobalCoolDown != true && MyAbilityCastingTime == 0f) {
                sourceCharacter.MyCharacterAbilityManager.InitiateGlobalCooldown(usedCoolDown);
            } else {
                //Debug.Log(gameObject.name + ".PlayerAbilityManager.PerformAbility(" + ability.MyName + "): ability.MyAbilityCastingTime: " + ability.MyAbilityCastingTime);
            }
        }

        public virtual void ProcessAbilityPrefabs(BaseCharacter sourceCharacter) {
            //Debug.Log(MyName + ".BaseAbilitiy.ProcessAbilityPrefabs()");
            if (MyHoldableObjectNames.Count == 0) {
                return;
            }
            if (sourceCharacter != null && sourceCharacter.MyCharacterEquipmentManager != null) {
                sourceCharacter.MyCharacterEquipmentManager.DespawnAbilityObjects();
            }
        }

        public virtual bool CanUseOn(GameObject target, BaseCharacter sourceCharacter) {
            //Debug.Log(MyName + ".BaseAbility.CanUseOn(" + (target != null ? target.name : "null") + ", " + (source != null ? source.name : "null") + ")");

            if (abilityEffects != null && abilityEffects.Count > 0 && useAbilityEffectTargetting == true) {
                return abilityEffects[0].CanUseOn(target, sourceCharacter);
            }

            // create target booleans
            bool targetIsFriendly = false;
            bool targetIsEnemy = false;
            bool targetIsSelf = false;
            CharacterUnit targetCharacterUnit = null;

            if (requiresTarget == false) {
                //Debug.Log("BaseAbility.CanUseOn(): target not required, returning true");
                return true;
            }

            // deal with targetting
            if (target == null && autoSelfCast != true) {
                //Debug.Log(resourceName + " requires a target!");
                CombatLogUI.MyInstance.WriteCombatMessage(resourceName + " requires a target!");
                return false;
            }

            if (target != null) {
                targetCharacterUnit = target.GetComponent<CharacterUnit>();
                if (targetCharacterUnit != null) {
                    if (Faction.RelationWith(targetCharacterUnit.MyCharacter, sourceCharacter) <= -1) {
                        targetIsEnemy = true;
                    }
                    if (Faction.RelationWith(targetCharacterUnit.MyCharacter, sourceCharacter) >= 0) {
                        targetIsFriendly = true;
                    }

                    // liveness checks
                    if (targetCharacterUnit.MyCharacter.MyCharacterStats.IsAlive == false && requiresLiveTarget == true) {
                        //Debug.Log("This ability requires a live target");
                        //CombatLogUI.MyInstance.WriteCombatMessage(resourceName + " requires a live target!");
                        return false;
                    }
                    if (targetCharacterUnit.MyCharacter.MyCharacterStats.IsAlive == true && requireDeadTarget == true) {
                        //Debug.Log("This ability requires a dead target");
                        //CombatLogUI.MyInstance.WriteCombatMessage(resourceName + " requires a dead target!");
                        return false;
                    }

                }
            }
            if (target == sourceCharacter.MyCharacterUnit.gameObject) {
                targetIsSelf = true;
            }

            // correct match conditions.  if any of these are met, the target is already valid
            if (!(canCastOnFriendly && targetIsFriendly || canCastOnEnemy && targetIsEnemy || canCastOnSelf && targetIsSelf)) {
                return false;
            }

            
            // range checks
            if (target != null && targetCharacterUnit != null) {
                if (canCastOnFriendly && targetIsFriendly || canCastOnEnemy && targetIsEnemy) {
                    // if none of those is true, then we are casting on ourselves, so don't need to do range check
                    if (MyUseMeleeRange) {
                        if (!sourceCharacter.MyCharacterController.IsTargetInHitBox(target)) {
                            return false;
                        }
                    } else {
                        if (maxRange > 0 && Vector3.Distance(sourceCharacter.MyCharacterUnit.transform.position, target.transform.position) > maxRange) {
                            //Debug.Log(target.name + " is out of range");
                            if (CombatLogUI.MyInstance != null && sourceCharacter != null && PlayerManager.MyInstance.MyCharacter != null && sourceCharacter == (PlayerManager.MyInstance.MyCharacter as BaseCharacter)) {
                                CombatLogUI.MyInstance.WriteCombatMessage(target.name + " is out of range of " + (MyName == null ? "null" : MyName));
                            }
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        //public virtual void PerformAbilityEffect(BaseAbility ability, GameObject source, GameObject target) {
        public virtual bool PerformAbilityEffects(BaseCharacter source, GameObject target, Vector3 groundTarget) {
            //Debug.Log(MyName + ".BaseAbility.PerformAbilityEffects(" + source.name + ", " + (target ? target.name : "null") + ", " + groundTarget + ")");
            if (abilityEffects.Count == 0) {
                //Debug.Log(resourceName + ".BaseAbility.PerformAbilityEffects(" + source.name + ", " + (target ? target.name : "null") + "): THERE ARE NO EFFECTS ATTACHED TO THIS ABILITY!");
                // this is fine for channeled abilities
            }

            // perform hit / miss check only if baseability requires target and return false if miss
            if (requiresTarget) {
                if (source.MyCharacterCombat.DidAttackMiss() == true) {
                    //Debug.Log(MyName + ".BaseAbility.PerformAbilityHit(" + source.name + ", " + target.name + "): attack missed");
                    source.MyCharacterCombat.ReceiveCombatMiss(target);
                    return false;
                }
            }

            foreach (AbilityEffect abilityEffect in abilityEffects) {
                if (abilityEffect == null) {
                    Debug.Log("Forgot to set ability affect in inspector?");
                }
                AbilityEffectOutput abilityEffectOutput = new AbilityEffectOutput();
                abilityEffectOutput.prefabLocation = groundTarget;

                abilityEffectOutput.castTimeMultipler = castTimeMultiplier;
                AbilityEffect _abilityEffect = SystemAbilityEffectManager.MyInstance.GetResource(abilityEffect.MyName);
                if (_abilityEffect != null) {
                    _abilityEffect.Cast(source, target, target, abilityEffectOutput);
                } else {
                    //Debug.Log(MyName + ".BaseAbility.PerformAbilityEffects(" + source.name + ", " + (target ? target.name : "null") + ", " + groundTarget + ") COULD NOT FIND " + abilityEffect.MyName);
                    //return;
                }
            }
            return true;
        }

        /// <summary>
        /// Return the proper target for this type of ability
        /// </summary>
        /// <param name="sourceCharacter"></param>
        /// <returns></returns>
        public virtual GameObject ReturnTarget(BaseCharacter sourceCharacter, GameObject target) {
            //Debug.Log("BaseAbility.ReturnTarget()");
            // before we get here, a validity check has already been performed, so no need to unset any targets
            // we are only concerned with redirecting the target to self if auto-selfcast is enabled

            if (sourceCharacter == null || sourceCharacter.MyCharacterUnit == null) {
                //Debug.Log("BaseAbility.ReturnTarget(): source is null! This should never happen!!!!!");
                return null;
            }

            // perform ability dependent checks
            if (!CanUseOn(target, sourceCharacter) == true) {
                //Debug.Log("ability.CanUseOn(" + ability.MyName + ", " + (target != null ? target.name : "null") + " was false.  exiting");
                if (canCastOnSelf && autoSelfCast) {
                    target = sourceCharacter.MyCharacterUnit.gameObject;
                    return target;
                } else {
                    return null;
                }
            } else {
                return target;
            }
        }

        public virtual void StartCasting(BaseCharacter source) {
            //Debug.Log("BaseAbility.OnCastStart(" + source.name + ")");
            //Debug.Log("setting casting animation");
            if (castingAnimationClip != null) {
                source.MyAnimatedUnit.MyCharacterAnimator.HandleCastingAbility(castingAnimationClip, this);
            }
            // GRAVITY FREEZE FOR CASTING
            // DISABLING SINCE IT IS CAUSING INSTANT CASTS TO STOP CHARACTER WHILE MOVING.  MAYBE CHECK IF CAST TIMER AND THEN DO IT?
            // NEXT LINE NO LONGER NEEDED SINCE WE NOW ACTUALLY CHECK THE REAL DISTANCE MOVED BY THE CHARACTER AND DON'T CANCEL CAST UNTIL DISTANCE IS > 0.1F
            //source.MyRigidBody.constraints = RigidbodyConstraints.FreezeAll;

            if (abilityCastingPrefab != null) {
                if (abilityCastingPrefabRef == null) {
                    //Vector3 relativePrefabOffset = source.MyCharacterUnit.transform.TransformPoint(prefabOffset);
                    //Vector3 spawnLocation = new Vector3(source.MyCharacterUnit.transform.position.x + relativePrefabOffset.x, source.MyCharacterUnit.transform.position.y + relativePrefabOffset.y, source.MyCharacterUnit.transform.position.z + relativePrefabOffset.z);
                    Vector3 spawnLocation = source.MyCharacterUnit.transform.TransformPoint(prefabOffset);
                    //Debug.Log("BaseAbility.OnCastStart(): Instantiating spell casting prefab at " + source.transform.position + "; spawnLocation is : " + spawnLocation);
                    //abilityCastingPrefabRef = Instantiate(abilityCastingPrefab, spawnLocation, source.MyCharacterUnit.transform.rotation * Quaternion.Euler(source.MyCharacterUnit.transform.TransformDirection(prefabRotation)), source.transform);
                    abilityCastingPrefabRef = Instantiate(abilityCastingPrefab, spawnLocation, Quaternion.LookRotation(source.MyCharacterUnit.transform.forward) * Quaternion.Euler(prefabRotation), source.MyCharacterUnit.transform);
                }
            }
            source.MyCharacterAbilityManager.OnCastStop += HandleCastStop;
        }

        public virtual void OnCastTimeChanged(float currentCastTime, BaseCharacter source, GameObject target) {
            //Debug.Log("BaseAbility.OnCastTimeChanged()");
            // overwrite me
        }

        public virtual void HandleCastStop(BaseCharacter source) {
            //Debug.Log("BaseAbility.OnCastComplete()");
            Destroy(abilityCastingPrefabRef, prefabDestroyDelay);

            // this may lead to bugs, not sure...
            // taking out for now, seems to be messing with attacks?
            //source.MyCharacterCombat.SetWaitingForHits(false);

            source.MyCharacterAbilityManager.OnCastStop -= HandleCastStop;
        }

    }

    public enum PrefabSpawnLocation { None, Caster, Target, Point, OriginalTarget }

}