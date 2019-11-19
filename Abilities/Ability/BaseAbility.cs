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

        [SerializeField]
        private List<AnyRPGWeaponAffinity> weaponAffinity = new List<AnyRPGWeaponAffinity>();

        public List<AnyRPGWeaponAffinity> MyWeaponAffinity { get => weaponAffinity; set => weaponAffinity = value; }

        [SerializeField]
        protected string holdableObjectName;

        // now we have multiple objects
        [SerializeField]
        private List<string> holdableObjectNames = new List<string>();


        // on hit animation
        [SerializeField]
        protected AnimationClip animationClip = null;

        [SerializeField]
        protected AudioClip animationHitAudioClip;

        // on hit animation
        [SerializeField]
        protected AnimationClip castingAnimationClip = null;

        [SerializeField]
        protected AudioClip castingAudioClip;

        public AnimationClip MyAnimationClip { get => animationClip; set => animationClip = value; }
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

        public int maxRange = 0;

        [SerializeField]
        protected bool autoLearn = false;

        [SerializeField]
        protected bool autoAddToBars = true;

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

        public override string GetSummary() {
            string requireString = string.Empty;
            bool affinityMet = false;
            string colorString = string.Empty;
            string addString = string.Empty;
            if (weaponAffinity.Count == 0) {
                // no restrictions, automatically true
                affinityMet = true;

            } else {
                List<string> requireStrings = new List<string>();
                foreach (AnyRPGWeaponAffinity _weaponAffinity in weaponAffinity) {
                    requireStrings.Add(_weaponAffinity.ToString());
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
            if (weaponAffinity.Count == 0) {
                // no restrictions, automatically true
                return true;
            } else {
                foreach (AnyRPGWeaponAffinity _weaponAffinity in weaponAffinity) {

                    if (sourceCharacter != null && sourceCharacter.MyCharacterEquipmentManager != null && sourceCharacter.MyCharacterEquipmentManager.HasAffinity(_weaponAffinity)) {
                        return true;
                    }
                }
            }
            return false;
        }

        public void Use() {
            //Debug.Log("BaseAbility.Use()");
            // prevent casting any ability without the proper weapon affinity
            if (CanCast(PlayerManager.MyInstance.MyCharacter)) {
                PlayerManager.MyInstance.MyCharacter.MyCharacterAbilityManager.BeginAbility(this);
            }
        }

        public virtual bool Cast(BaseCharacter source, GameObject target, Vector3 groundTarget) {
            //Debug.Log(resourceName + ".BaseAbility.Cast(" + source.name + ", " + (target == null ? "null" : target.name) + ", " + groundTarget + ")");
            if (!CanCast(source)) {
                //CombatLogUI.MyInstance.WriteCombatMessage("BaseAbility.Cast(): You do not have the right weapon to cast: " + MyName);
                return false;
            }

            if (source != null && source.MyCharacterAbilityManager != null) {
                source.MyCharacterAbilityManager.BeginAbilityCoolDown(this);
            }

            return true;
            // notify subscribers
            //OnAbilityCast(this);
        }

        public virtual bool CanUseOn(GameObject target, BaseCharacter source) {
            //Debug.Log(MyName + ".BaseAbility.CanUseOn(" + (target != null ? target.name : "null") + ", " + (source != null ? source.name : "null") + ")");
            if (requiresTarget == false) {
                //Debug.Log("BaseAbility.CanUseOn(): target not required, returning true");
                return true;
            }

            CharacterUnit targetCharacterUnit = null;
            if (target != null) {
                targetCharacterUnit = target.GetComponent<CharacterUnit>();
                if (targetCharacterUnit != null) {
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

            // deal with targetting
            if (target == null && autoSelfCast != true) {
                //Debug.Log(resourceName + " requires a target!");
                CombatLogUI.MyInstance.WriteCombatMessage(resourceName + " requires a target!");
                return false;
            }

            if (target != null && targetCharacterUnit != null) {
                if (maxRange > 0 && Vector3.Distance(source.MyCharacterUnit.transform.position, target.transform.position) > maxRange) {
                    //Debug.Log(target.name + " is out of range");
                    if (CombatLogUI.MyInstance != null && source != null && PlayerManager.MyInstance.MyCharacter != null && source == (PlayerManager.MyInstance.MyCharacter as BaseCharacter)) {
                        CombatLogUI.MyInstance.WriteCombatMessage(target.name + " is out of range of " + (MyName == null ? "null" : MyName));
                    }
                    return false;
                }
            }

            return true;
        }

        //public virtual void PerformAbilityEffect(BaseAbility ability, GameObject source, GameObject target) {
        public virtual void PerformAbilityEffects(BaseCharacter source, GameObject target, Vector3 groundTarget) {
            //Debug.Log(MyName + ".BaseAbility.PerformAbilityEffects(" + source.name + ", " + (target ? target.name : "null") + ", " + groundTarget + ")");
            if (abilityEffects.Count == 0) {
                //Debug.Log(resourceName + ".BaseAbility.PerformAbilityEffects(" + source.name + ", " + (target ? target.name : "null") + "): THERE ARE NO EFFECTS ATTACHED TO THIS ABILITY!");
                // this is fine for channeled abilities
            }
            foreach (AbilityEffect abilityEffect in abilityEffects) {
                if (abilityEffect == null) {
                    Debug.Log("Forgot to set ability affect in inspector?");
                }
                AbilityEffectOutput abilityEffectOutput = new AbilityEffectOutput();
                abilityEffectOutput.prefabLocation = groundTarget;
                AbilityEffect _abilityEffect = SystemAbilityEffectManager.MyInstance.GetResource(abilityEffect.MyName);
                if (_abilityEffect != null) {
                    _abilityEffect.Cast(source, target, target, abilityEffectOutput);
                } else {
                    //Debug.Log(MyName + ".BaseAbility.PerformAbilityEffects(" + source.name + ", " + (target ? target.name : "null") + ", " + groundTarget + ") COULD NOT FIND " + abilityEffect.MyName);
                    return;
                }
            }
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

            if (sourceCharacter == null) {
                //Debug.Log("BaseAbility.ReturnTarget(): source is null! This should never happen!!!!!");
            }

            // create target booleans
            bool targetIsFriendly = false;
            bool targetIsEnemy = false;
            bool targetIsSelf = false;
            if (target != null) {
                CharacterUnit targetCharacterUnit = target.GetComponent<CharacterUnit>();
                if (targetCharacterUnit != null) {
                    if (Faction.RelationWith(targetCharacterUnit.MyCharacter, sourceCharacter) <= -1) {
                        targetIsEnemy = true;
                    }
                    if (Faction.RelationWith(targetCharacterUnit.MyCharacter, sourceCharacter) >= 0) {
                        targetIsFriendly = true;
                    }
                }
            }
            if (target == sourceCharacter.MyCharacterUnit.gameObject) {
                targetIsSelf = true;
            }

            // correct match conditions.  if any of these are met, the target is already valid
            if (canCastOnFriendly && targetIsFriendly) {
                return target;
            }
            if (canCastOnEnemy && targetIsEnemy) {
                return target;
            }

            // we don't have valid friendly or enemy target.  see if we need to auto-target self or not
            if (canCastOnSelf) {
                if (targetIsSelf) {
                    return target;
                }
                // self is a valid targe, but target was not self.  check for auto-self cast
                if (autoSelfCast) {
                    target = sourceCharacter.MyCharacterUnit.gameObject;
                    return target;
                }
            }

            // if we reached here, the target was not redirected to self, so we have to return the original target
            // and since it wasn't friendly or an enemy or self, it is likely a trade/crafting node
            return target;
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