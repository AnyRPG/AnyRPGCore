using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "NewAbility",menuName = "AnyRPG/Abilities/Ability")]
    public abstract class BaseAbility : DescribableResource, IUseable, IMoveable, IAbility, ITargetable {

        public event System.Action OnAbilityLearn = delegate { };
        public event System.Action OnAbilityUsed = delegate { };

        [Header("Casting Requirements")]

        [Tooltip("If true, this ability cannot be cast in combat.")]
        [SerializeField]
        protected bool requireOutOfCombat = false;

        [Tooltip("If this list is not empty, this ability will require the character to have the following weapons equipped to use it.")]
        [SerializeField]
        private List<string> weaponAffinityNames = new List<string>();

        private List<WeaponSkill> weaponAffinityList = new List<WeaponSkill>();

        [Header("Prefabs")]

        [Tooltip("The names of items to spawn while casting this ability")]
        [SerializeField]
        private List<string> holdableObjectNames = new List<string>();

        //[SerializeField]
        private List<PrefabProfile> holdableObjects = new List<PrefabProfile>();

        [Header("Prefab Control")]

        [Tooltip("holdable object prefabs are created by the animator from an animation event, not from the ability manager during cast start")]
        [SerializeField]
        protected bool animatorCreatePrefabs;

        [Tooltip("Delay to destroy casting effect prefabs after casting completes")]
        [SerializeField]
        protected float prefabDestroyDelay = 0f;

        [Header("Animation")]

        [Tooltip("The name of an animation profile to get animations for the character to perform while casting this ability")]
        [SerializeField]
        protected string animationProfileName = string.Empty;

        protected AnimationProfile animationProfile;

        [Header("Audio")]

        [Tooltip("If the animation has hit events while it is playing (such as when a hammer strike occurs), this audio profile will be played in response to those events.")]
        [SerializeField]
        protected string animationHitAudioProfileName;

        protected AudioProfile animationHitAudioProfile;

        [Tooltip("An audio profile to play while the ability is casting")]
        [SerializeField]
        protected string castingAudioProfileName;

        protected AudioProfile castingAudioProfile;

        [Header("Learning")]

        [Tooltip("The minimum level a character must be to cast this ability")]
        [SerializeField]
        protected int requiredLevel = 1;

        [Tooltip("If not empty, the character must be one of these classes to use this item.")]
        [SerializeField]
        private List<string> characterClassRequirements = new List<string>();

        private List<CharacterClass> characterClassRequirementList = new List<CharacterClass>();

        [Tooltip("If true, this ability does not have to be learned to cast. For abilities that anyone can use, like scrolls or crafting")]
        [SerializeField]
        protected bool useableWithoutLearning = false;

        [Tooltip("Will this ability be automatically added to the player spellbook if they are of the required level?")]
        [SerializeField]
        protected bool autoLearn = false;

        [Tooltip("When learned, should the ability be automatically placed on the player action bars in an available slot?")]
        [SerializeField]
        protected bool autoAddToBars = true;

        [Header("Casting Restrictions")]

        [Tooltip("This spell can be cast while other spell casts are in progress. Use this option for things like system abilities (level up, achievement, take damage effect, etc) that should not be blocked by an active spell cast in progress.")]
        [SerializeField]
        private bool canSimultaneousCast = false;

        [Tooltip("This spell can be cast while the global cooldown is active. Use this option for things like system abilities (level up, achievement, take damage effect, etc) that should not be blocked by an active spell cast in progress.")]
        [SerializeField]
        private bool ignoreGlobalCoolDown = false;

        [Header("Cost")]

        [Tooltip("The resource to use when casting this ability")]
        [SerializeField]
        protected string powerResourceName = string.Empty;

        /// <summary>
        /// the resource to spend when casting
        /// </summary>
        protected PowerResource powerResource = null;

        [Tooltip("A fixed amount of the resource to use per cast")]
        [SerializeField]
        protected int baseResourceCost = 0;

        [Tooltip("A fixed amount of the resource to use per cast")]
        [SerializeField]
        protected int resourceCostPerLevel = 5;

        [Header("Power Generation")]

        [Tooltip("The resource to refill when this ability hits the target")]
        [SerializeField]
        protected string generatePowerResourceName = string.Empty;

        protected PowerResource generatePowerResource = null;

        [Tooltip("A fixed amount of the resource to gain")]
        [SerializeField]
        protected int baseResourceGain = 0;

        [Tooltip("An amount of the resource to gain that is multiplied by the caster level")]
        [SerializeField]
        protected int resourceGainPerLevel = 0;


        [Header("Cast Time")]

        [Tooltip("If true, the cast time is based on the time of the animation played while casting.")]
        [SerializeField]
        protected bool useAnimationCastTime = true;

        [Tooltip("If the animation cast time is not used, the number of seconds to spend casting")]
        [SerializeField]
        protected float abilityCastingTime = 0f;

        [Tooltip("The cooldown in seconds before this ability can be cast again.  0 means no cooldown.")]
        [SerializeField]
        public float abilityCoolDown = 0f;


        [Header("Ground Target")]

        [Tooltip("If true, casting this spell will require choosing a target on the ground, instead of a target character.")]
        [SerializeField]
        private bool requiresGroundTarget = false;

        [Tooltip("If this is a ground targeted spell, tint it with this color.")]
        [SerializeField]
        protected Color groundTargetColor = new Color32(255, 255, 255, 255);

        [Tooltip("How big should the projector be on the ground if this is ground targeted. Used to show accurate effect size.")]
        [SerializeField]
        protected float groundTargetRadius = 0f;

        [Header("Standard Target")]

        [Tooltip("Ignore requireTarget and canCast variables and use the check from the first ability effect instead")]
        [SerializeField]
        private bool useAbilityEffectTargetting = false;

        [Tooltip("If true, the character must have a target selected to cast this ability.")]
        [SerializeField]
        private bool requiresTarget;

        [Tooltip("If true, the character must have an uninterrupted line of sight to the target.")]
        [SerializeField]
        private bool requireLineOfSight;

        [Tooltip("If true, the target must be a character and must be alive.")]
        [SerializeField]
        private bool requiresLiveTarget = true;

        [Tooltip("If true, the target must be a character and must be dead.")]
        [SerializeField]
        private bool requireDeadTarget;

        [Tooltip("Can the character cast this ability on itself?")]
        [SerializeField]
        protected bool canCastOnSelf = false;

        [Tooltip("Can the character cast this ability on a character belonging to an enemy faction?")]
        [SerializeField]
        protected bool canCastOnEnemy = false;

        [Tooltip("Can the character cast this ability on a character belonging to a friendly faction?")]
        [SerializeField]
        protected bool canCastOnFriendly = false;

        [Tooltip("If no target is given, automatically cast on the caster")]
        [SerializeField]
        private bool autoSelfCast = false;

        [Header("Range")]

        [Tooltip("If true, the target must be within melee range (within hitbox) to cast this ability.")]
        [SerializeField]
        protected bool useMeleeRange;

        [Tooltip("If melee range is not used, this ability can be cast on targets this many meters away.")]
        [SerializeField]
        protected int maxRange;

        // this will be set to the ability casting length only for direct cast abilities
        protected float castTimeMultiplier = 1f;

        [Header("Cast Complete Ability Effects")]

        [Tooltip("When casting is complete, these ability effects will be triggered.")]
        [SerializeField]
        protected List<string> abilityEffectNames = new List<string>();

        [Header("Channeling")]

        [Tooltip("During casting, this ability will perform its tick effects, every x seconds")]
        [SerializeField]
        private float tickRate = 1f;

        [Header("Chanelling Effects")]

        [Tooltip("During casting, these ability effects will be triggered on every tick.")]
        [SerializeField]
        protected List<string> channeledAbilityEffectnames = new List<string>();

        protected List<AbilityEffect> channeledAbilityEffects = new List<AbilityEffect>();


        protected List<AbilityEffect> abilityEffects = new List<AbilityEffect>();

        protected Vector3 groundTarget = Vector3.zero;

        public AnimationClip MyCastingAnimationClip {
            get => (animationProfile != null && animationProfile.MyAttackClips != null && animationProfile.MyAttackClips.Count > 0 ? animationProfile.MyAttackClips[0] : null);
        }
        public int MyRequiredLevel { get => requiredLevel; }
        public bool MyAutoLearn { get => autoLearn; }
        public bool MyAutoAddToBars { get => autoAddToBars; }
        public bool MyUseableWithoutLearning { get => useableWithoutLearning; }
        public virtual float MyAbilityCastingTime {
            get {
                if (useAnimationCastTime == false) {
                    return abilityCastingTime;
                } else {
                    if (MyCastingAnimationClip != null) {
                        return MyCastingAnimationClip.length;
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
        public bool CanCastOnEnemy { get => canCastOnEnemy; }
        public bool CanCastOnFriendly { get => canCastOnFriendly; }
        public bool CanSimultaneousCast { get => canSimultaneousCast; set => canSimultaneousCast = value; }
        public bool MyRequireDeadTarget { get => requireDeadTarget; set => requireDeadTarget = value; }
        public bool MyIgnoreGlobalCoolDown { get => ignoreGlobalCoolDown; set => ignoreGlobalCoolDown = value; }
        public AudioClip MyCastingAudioClip { get => (castingAudioProfile == null ? null : castingAudioProfile.MyAudioClip); }
        public AudioClip MyAnimationHitAudioClip { get => (animationHitAudioProfile == null ? null : animationHitAudioProfile.MyAudioClip); }
        public virtual List<PrefabProfile> MyHoldableObjects { get => holdableObjects; set => holdableObjects = value; }
        public bool MyAnimatorCreatePrefabs { get => animatorCreatePrefabs; set => animatorCreatePrefabs = value; }
        public List<AnimationClip> AnimationClips { get => (animationProfile != null ? animationProfile.MyAttackClips : null); }
        public int MaxRange { get => maxRange; set => maxRange = value; }
        public bool UseMeleeRange { get => useMeleeRange; set => useMeleeRange = value; }
        public List<string> MyWeaponAffinityNames { get => weaponAffinityNames; set => weaponAffinityNames = value; }
        public bool MyRequireOutOfCombat { get => requireOutOfCombat; set => requireOutOfCombat = value; }
        public List<string> MyAbilityEffectNames { get => abilityEffectNames; set => abilityEffectNames = value; }
        public List<AbilityEffect> MyAbilityEffects { get => abilityEffects; set => abilityEffects = value; }
        public AnimationProfile MyAnimationProfile { get => animationProfile; set => animationProfile = value; }
        public float MyGroundTargetRadius { get => groundTargetRadius; set => groundTargetRadius = value; }
        public List<WeaponSkill> WeaponAffinityList { get => weaponAffinityList; set => weaponAffinityList = value; }
        public bool RequireLineOfSight { get => requireLineOfSight; set => requireLineOfSight = value; }
        public List<CharacterClass> CharacterClassRequirementList { get => characterClassRequirementList; set => characterClassRequirementList = value; }
        public PowerResource PowerResource { get => powerResource; set => powerResource = value; }
        public PowerResource GeneratePowerResource { get => generatePowerResource; set => generatePowerResource = value; }
        public int BaseResourceGain { get => baseResourceGain; set => baseResourceGain = value; }
        public int ResourceGainPerLevel { get => resourceGainPerLevel; set => resourceGainPerLevel = value; }

        public override string GetSummary() {
            string requireString = string.Empty;
            bool affinityMet = false;
            string colorString = string.Empty;
            string addString = string.Empty;
            if (weaponAffinityNames.Count == 0) {
                // no restrictions, automatically true
                affinityMet = true;

            } else {
                List<string> requireWeaponSkills = new List<string>();
                foreach (WeaponSkill _weaponAffinity in weaponAffinityList) {
                    requireWeaponSkills.Add(_weaponAffinity.MyName);
                    if (PlayerManager.MyInstance.MyCharacter.CharacterEquipmentManager.HasAffinity(_weaponAffinity)) {
                        affinityMet = true;
                    }
                }
                if (affinityMet) {
                    colorString = "#ffffffff";
                } else {
                    colorString = "#ff0000ff";
                }
                addString = string.Format("\n<color={0}>Requires: {1}</color>", colorString, string.Join(",", requireWeaponSkills));
            }

            string abilityRange = (useMeleeRange == true ? "melee" : MaxRange + " meters");

            string costString = string.Empty;
            if (powerResource != null) {
                costString = "\nCost: " + GetResourceCost(PlayerManager.MyInstance.MyCharacter.CharacterAbilityManager) + " " + powerResource.MyName;
            }

            return string.Format("Cast time: {0} second(s)\nCooldown: {1} second(s){2}\nRange: {3}\n<color=#ffff00ff>{4}</color>{5}", MyAbilityCastingTime.ToString("F1"), abilityCoolDown, costString, abilityRange, description, addString);
        }

        public virtual float GetResourceCost(IAbilityCaster abilityCaster) {
            if (abilityCaster != null && powerResource != null) {
                return baseResourceCost + (abilityCaster.Level * resourceCostPerLevel);
            }
            return baseResourceCost;
        }

        public virtual float GetResourceGain(IAbilityCaster abilityCaster) {
            //Debug.Log(MyName + ".BaseAbility.GetResourceGain(" + (abilityCaster == null ? "null" : abilityCaster.Name) + ")");
            if (abilityCaster != null) {
                //Debug.Log(MyName + ".BaseAbility.GetResourceGain() level: " + abilityCaster.Level + "; gainperLevel: " + resourceGainPerLevel + "; base: " + baseResourceGain);

                return baseResourceGain + (abilityCaster.Level * resourceGainPerLevel);
            }
            return baseResourceCost;
        }

        public virtual AudioClip GetAnimationHitSound() {
            return MyAnimationHitAudioClip;
        }

        public virtual AudioClip GetHitSound(IAbilityCaster abilityCaster) {
            // only meant for animated Abilities
            return null;
        }

        public virtual float GetLOSMaxRange(IAbilityCaster source, GameObject target) {
            //Debug.Log(MyName + ".BaseAbility.GetLOSMaxRange(" + (source == null ? "null" : source.Name) + ", " + (target == null ? "null" : target.name) + ")");
            if (source.PerformLOSCheck(target, this)) {
                //Debug.Log(MyName + ".BaseAbility.GetLOSMaxRange(" + (source == null ? "null" : source.Name) + ", " + (target == null ? "null" : target.name) + "): return " + MaxRange);
                return MaxRange;
            }
            //Debug.Log(MyName + ".BaseAbility.GetLOSMaxRange(" + (source == null ? "null" : source.Name) + ", " + (target == null ? "null" : target.name) + "): return " + source.GetMeleeRange());
            return source.GetMeleeRange();
        }

        public virtual void PerformChanneledEffect(IAbilityCaster source, GameObject target) {
            //Debug.Log("BaseAbility.PerformChanneledEffect(" + MyName + ", " + (source == null ? "null" : source.name) + ", " + (target == null ? "null" : target.name) + ")");
            foreach (AbilityEffect abilityEffect in channeledAbilityEffects) {
                AbilityEffect _abilityEffect = SystemAbilityEffectManager.MyInstance.GetNewResource(abilityEffect.MyName);

                // channeled effects need to override the object lifetime so they get destroyed at the tickrate
                //_abilityEffect.MyAbilityEffectObjectLifetime = tickRate;
                _abilityEffect.Cast(source, target, target, null);
            }
        }


        public bool CanCast(IAbilityCaster sourceCharacter) {
            if (weaponAffinityNames.Count == 0) {
                // no restrictions, automatically true
                return true;
            } else {
                return sourceCharacter.PerformWeaponAffinityCheck(this);
            }
        }

        public bool Use() {
            //Debug.Log("BaseAbility.Use()");
            // prevent casting any ability without the proper weapon affinity
            if (CanCast(PlayerManager.MyInstance.MyCharacter.CharacterAbilityManager as IAbilityCaster)) {
                PlayerManager.MyInstance.MyCharacter.CharacterAbilityManager.BeginAbility(this);
                return true;
            }
            return false;
        }

        public virtual bool Cast(IAbilityCaster sourceCharacter, GameObject target, Vector3 groundTarget) {
            //Debug.Log(resourceName + ".BaseAbility.Cast(" + sourceCharacter.name + ", " + (target == null ? "null" : target.name) + ", " + groundTarget + ")");
            if (!CanCast(sourceCharacter)) {
                //Debug.Log(resourceName + ".BaseAbility.Cast(" + sourceCharacter.name + ", " + (target == null ? "null" : target.name) + ", " + groundTarget + "): CAN'T CAST!!!");
                //CombatLogUI.MyInstance.WriteCombatMessage("BaseAbility.Cast(): You do not have the right weapon to cast: " + MyName);
                return false;
            }

            BeginAbilityCoolDown(sourceCharacter);

            ProcessAbilityPrefabs(sourceCharacter);
            ProcessGCDAuto(sourceCharacter);

            return true;
            // notify subscribers
            //OnAbilityCast(this);
        }

        public virtual void BeginAbilityCoolDown(IAbilityCaster sourceCharacter, float animationLength = -1f) {
            if (sourceCharacter != null) {
                sourceCharacter.BeginAbilityCoolDown(this, animationLength);
            }
        }

        public virtual void ProcessGCDAuto(IAbilityCaster sourceCharacter) {
            ProcessGCDManual(sourceCharacter);
        }

        public virtual void ProcessGCDManual(IAbilityCaster sourceCharacter, float usedCoolDown = 0f) {
            if (CanSimultaneousCast == false && MyIgnoreGlobalCoolDown == false && MyAbilityCastingTime == 0f) {
                sourceCharacter.InitiateGlobalCooldown(usedCoolDown);
            } else {
                //Debug.Log(gameObject.name + ".PlayerAbilityManager.PerformAbility(" + ability.MyName + "): ability.MyAbilityCastingTime: " + ability.MyAbilityCastingTime);
            }
        }

        public virtual void ProcessAbilityPrefabs(IAbilityCaster sourceCharacter) {
            //Debug.Log(MyName + ".BaseAbilitiy.ProcessAbilityPrefabs()");
            if (MyHoldableObjects.Count == 0) {
                return;
            }

            sourceCharacter.DespawnAbilityObjects();
        }

        public virtual bool CanUseOn(GameObject target, IAbilityCaster sourceCharacter, bool performCooldownChecks = true) {
            //Debug.Log(MyName + ".BaseAbility.CanUseOn(" + (target != null ? target.name : "null") + ", " + (sourceCharacter != null ? sourceCharacter.name : "null") + ")");

            if (abilityEffects != null && abilityEffects.Count > 0 && useAbilityEffectTargetting == true) {
                return abilityEffects[0].CanUseOn(target, sourceCharacter);
            }

            // create target booleans
            bool targetIsSelf = false;
            CharacterUnit targetCharacterUnit = null;

            // special case for ground targeted spells cast by AI since AI currently has to cast a ground targeted spell on its current target
            if (requiresGroundTarget == true && maxRange > 0 && target != null && (sourceCharacter as AICharacter) is AICharacter && Vector3.Distance(sourceCharacter.UnitGameObject.transform.position, target.transform.position) > maxRange) {
                return false;
            }

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

            if (target == sourceCharacter.UnitGameObject) {
                targetIsSelf = true;
            }

            if (target != null) {
                targetCharacterUnit = target.GetComponent<CharacterUnit>();
                if (targetCharacterUnit != null) {

                    if (!sourceCharacter.PerformFactionCheck(this, targetCharacterUnit, targetIsSelf)) {
                        return false;
                    }

                    // liveness checks
                    if (targetCharacterUnit.MyCharacter.CharacterStats.IsAlive == false && requiresLiveTarget == true) {
                        //Debug.Log("This ability requires a live target");
                        //CombatLogUI.MyInstance.WriteCombatMessage(resourceName + " requires a live target!");
                        return false;
                    }
                    if (targetCharacterUnit.MyCharacter.CharacterStats.IsAlive == true && requireDeadTarget == true) {
                        //Debug.Log("This ability requires a dead target");
                        //CombatLogUI.MyInstance.WriteCombatMessage(resourceName + " requires a dead target!");
                        return false;
                    }
                } else {
                    if (requiresLiveTarget == true || requireDeadTarget == true) {
                        // something that is not a character unit cannot satisfy the alive or dead conditions because it is inanimate
                        return false;
                    }
                }
            }
            
            if (!canCastOnSelf && targetIsSelf) {
                //Debug.Log(MyName + ": Can't cast on self. return false");
                return false;
            }

            if (target != null) {
                if (canCastOnSelf && targetIsSelf) {
                    return true;
                }

                if (!sourceCharacter.IsTargetInAbilityRange(this, target)) {
                    return false;
                }
            }

            //Debug.Log(MyName + ".BaseAbility.CanUseOn(): returning true");
            return true;
        }

        //public virtual void PerformAbilityEffect(BaseAbility ability, GameObject source, GameObject target) {
        public virtual bool PerformAbilityEffects(IAbilityCaster source, GameObject target, Vector3 groundTarget) {
            //Debug.Log(MyName + ".BaseAbility.PerformAbilityEffects(" + source.name + ", " + (target ? target.name : "null") + ", " + groundTarget + ")");
            if (abilityEffects.Count == 0) {
                //Debug.Log(resourceName + ".BaseAbility.PerformAbilityEffects(" + source.name + ", " + (target ? target.name : "null") + "): THERE ARE NO EFFECTS ATTACHED TO THIS ABILITY!");
                // this is fine for channeled abilities
            }

            // perform hit / miss check only if baseability requires target and return false if miss
            if (requiresTarget) {
                if (!source.AbilityHit(target)) {
                    return false;
                }
            }

            // generate power resource
            source.GeneratePower(this);

            foreach (AbilityEffect abilityEffect in abilityEffects) {
                if (abilityEffect == null) {
                    Debug.Log("Forgot to set ability affect in inspector?");
                }
                AbilityEffectOutput abilityEffectOutput = new AbilityEffectOutput();
                abilityEffectOutput.prefabLocation = groundTarget;

                abilityEffectOutput.castTimeMultipler = castTimeMultiplier;
                AbilityEffect _abilityEffect = SystemAbilityEffectManager.MyInstance.GetNewResource(abilityEffect.MyName);
                if (_abilityEffect != null && _abilityEffect.CanUseOn(target, source)) {
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
        public virtual GameObject ReturnTarget(IAbilityCaster sourceCharacter, GameObject target, bool performCooldownChecks = true) {
            //Debug.Log(MyName + ".BaseAbility.ReturnTarget(" + (sourceCharacter == null ? "null" : sourceCharacter.MyName) + ", " + (target == null ? "null" : target.name) + ")");
            // before we get here, a validity check has already been performed, so no need to unset any targets
            // we are only concerned with redirecting the target to self if auto-selfcast is enabled

            if (sourceCharacter == null || sourceCharacter.UnitGameObject == null) {
                //Debug.Log("BaseAbility.ReturnTarget(): source is null! This should never happen!!!!!");
                return null;
            }

            // perform ability dependent checks
            if (CanUseOn(target, sourceCharacter, performCooldownChecks) == false) {
                //Debug.Log(MyName + ".BaseAbility.CanUseOn(" + (target != null ? target.name : "null") + " was false");
                if (canCastOnSelf && autoSelfCast) {
                    target = sourceCharacter.UnitGameObject;
                    //Debug.Log(MyName + ".BaseAbility.ReturnTarget(): returning target as sourcecharacter: " + target.name);
                    return target;
                } else {
                    //Debug.Log(MyName + ".BaseAbility.ReturnTarget(): returning null");
                    return null;
                }
            } else {
                //Debug.Log(MyName + ".BaseAbility.ReturnTarget(): returning original target: " + (target == null ? "null" : target.name));
                if (canCastOnSelf && autoSelfCast && target == null) {
                    target = sourceCharacter.UnitGameObject;
                    return target;
                }
                return target;
            }
        }

        public virtual void StartCasting(IAbilityCaster source) {
            //Debug.Log("BaseAbility.OnCastStart(" + source.name + ")");
            //Debug.Log("setting casting animation");
            if (MyCastingAnimationClip != null) {
                source.PerformCastingAnimation(MyCastingAnimationClip, this);
            }
            // GRAVITY FREEZE FOR CASTING
            // DISABLING SINCE IT IS CAUSING INSTANT CASTS TO STOP CHARACTER WHILE MOVING.  MAYBE CHECK IF CAST TIMER AND THEN DO IT?
            // NEXT LINE NO LONGER NEEDED SINCE WE NOW ACTUALLY CHECK THE REAL DISTANCE MOVED BY THE CHARACTER AND DON'T CANCEL CAST UNTIL DISTANCE IS > 0.1F
            //source.MyRigidBody.constraints = RigidbodyConstraints.FreezeAll;
            /*
            if (abilityCastingPrefab != null) {
                if (abilityCastingPrefabRef == null) {
                    //Vector3 relativePrefabOffset = source.UnitGameObject.transform.TransformPoint(prefabOffset);
                    //Vector3 spawnLocation = new Vector3(source.UnitGameObject.transform.position.x + relativePrefabOffset.x, source.MyCharacterUnit.transform.position.y + relativePrefabOffset.y, source.MyCharacterUnit.transform.position.z + relativePrefabOffset.z);
                    Vector3 spawnLocation = source.UnitGameObject.transform.TransformPoint(prefabOffset);
                    //Debug.Log("BaseAbility.OnCastStart(): Instantiating spell casting prefab at " + source.transform.position + "; spawnLocation is : " + spawnLocation);
                    //abilityCastingPrefabRef = Instantiate(abilityCastingPrefab, spawnLocation, source.UnitGameObject.transform.rotation * Quaternion.Euler(source.MyCharacterUnit.transform.TransformDirection(prefabRotation)), source.transform);
                    abilityCastingPrefabRef = Instantiate(abilityCastingPrefab, spawnLocation, Quaternion.LookRotation(source.UnitGameObject.transform.forward) * Quaternion.Euler(prefabRotation), source.MyCharacterUnit.transform);
                }
            }
            */
            //source.MyCharacterAbilityManager.OnCastStop += HandleCastStop;
        }

        public virtual float OnCastTimeChanged(float currentCastTime, float nextTickTime, IAbilityCaster source, GameObject target) {
            //Debug.Log("BaseAbility.OnCastTimeChanged()");
            // overwrite me
            if (currentCastTime >= nextTickTime) {
                PerformChanneledEffect(source, target);
                nextTickTime += tickRate;
            }
            return nextTickTime;
        }

        public void NotifyOnLearn() {
            OnAbilityLearn();
        }

        public void NotifyOnAbilityUsed() {
            OnAbilityUsed();
        }

        /// <summary>
        /// are the character class requirements met to learn or use this ability
        /// </summary>
        /// <returns></returns>
        public bool CharacterClassRequirementIsMet() {
            // only used when changing class or for action bars, so hard coding player character is ok for now
            if (CharacterClassRequirementList != null && CharacterClassRequirementList.Count > 0) {
                if (!CharacterClassRequirementList.Contains(PlayerManager.MyInstance.MyCharacter.MyCharacterClass)) {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// are all requirements met to learn or use this ability
        /// </summary>
        /// <returns></returns>
        public virtual bool RequirementsAreMet() {
            if (!CharacterClassRequirementIsMet()) {
                return false;
            }

            return true;
        }


        /*
        public virtual void HandleCastStop(BaseCharacter source) {
            //Debug.Log("BaseAbility.OnCastComplete()");
            //Destroy(abilityCastingPrefabRef, prefabDestroyDelay);

            // this may lead to bugs, not sure...
            // taking out for now, seems to be messing with attacks?
            //source.MyCharacterCombat.SetWaitingForHits(false);

            source.MyCharacterAbilityManager.OnCastStop -= HandleCastStop;
        }
        */

        public override void SetupScriptableObjects() {
            base.SetupScriptableObjects();
            abilityEffects = new List<AbilityEffect>();
            if (MyAbilityEffectNames != null) {
                foreach (string abilityEffectName in MyAbilityEffectNames) {
                    AbilityEffect abilityEffect = SystemAbilityEffectManager.MyInstance.GetResource(abilityEffectName);
                    if (abilityEffect != null) {
                        abilityEffects.Add(abilityEffect);
                    } else {
                        Debug.LogError("BaseAbility.SetupScriptableObjects(): Could not find ability effect: " + abilityEffectName + " while inititalizing " + MyName + ".  CHECK INSPECTOR");
                    }
                }
            }
            holdableObjects = new List<PrefabProfile>();
            if (holdableObjectNames != null) {
                foreach (string holdableObjectName in holdableObjectNames) {
                    PrefabProfile holdableObject = SystemPrefabProfileManager.MyInstance.GetResource(holdableObjectName);
                    if (holdableObject != null) {
                        holdableObjects.Add(holdableObject);
                    } else {
                        Debug.LogError("BaseAbility.SetupScriptableObjects(): Could not find holdableObject: " + holdableObjectName + " while inititalizing " + MyName + ".  CHECK INSPECTOR");
                    }
                }
            }
            weaponAffinityList = new List<WeaponSkill>();
            if (weaponAffinityNames != null) {
                foreach (string weaponAffinityName in weaponAffinityNames) {
                    WeaponSkill tmpWeaponSkill = SystemWeaponSkillManager.MyInstance.GetResource(weaponAffinityName);
                    if (tmpWeaponSkill != null) {
                        weaponAffinityList.Add(tmpWeaponSkill);
                    } else {
                        Debug.LogError("BaseAbility.SetupScriptableObjects(): Could not find weapon skill: " + weaponAffinityName + " while inititalizing " + MyName + ".  CHECK INSPECTOR");
                    }
                }
            }

            castingAudioProfile = null;
            if (castingAudioProfileName != null && castingAudioProfileName != string.Empty) {
                AudioProfile audioProfile = SystemAudioProfileManager.MyInstance.GetResource(castingAudioProfileName);
                if (audioProfile != null) {
                    castingAudioProfile = audioProfile;
                } else {
                    Debug.LogError("BaseAbility.SetupScriptableObjects(): Could not find audio profile: " + castingAudioProfileName + " while inititalizing " + MyName + ".  CHECK INSPECTOR");
                }
            }

            animationHitAudioProfile = null;
            if (animationHitAudioProfileName != null && animationHitAudioProfileName != string.Empty) {
                AudioProfile audioProfile = SystemAudioProfileManager.MyInstance.GetResource(animationHitAudioProfileName);
                if (audioProfile != null) {
                    animationHitAudioProfile = audioProfile;
                } else {
                    Debug.LogError("BaseAbility.SetupScriptableObjects(): Could not find audio profile: " + animationHitAudioProfileName + " while inititalizing " + MyName + ".  CHECK INSPECTOR");
                }
            }

            animationProfile = null;
            if (animationProfileName != null && animationProfileName != string.Empty) {
                AnimationProfile tmpAnimationProfile = SystemAnimationProfileManager.MyInstance.GetResource(animationProfileName);
                if (tmpAnimationProfile != null) {
                    animationProfile = tmpAnimationProfile;
                } else {
                    Debug.LogError("BaseAbility.SetupScriptableObjects(): Could not find animation profile: " + animationProfileName + " while inititalizing " + MyName + ".  CHECK INSPECTOR");
                }
            }

            powerResource = null;
            if (powerResourceName != null && powerResourceName != string.Empty) {
                PowerResource tmpPowerResource = SystemPowerResourceManager.MyInstance.GetResource(powerResourceName);
                if (tmpPowerResource != null) {
                    powerResource = tmpPowerResource;
                } else {
                    Debug.LogError("BaseAbility.SetupScriptableObjects(): Could not find power resource: " + powerResourceName + " while inititalizing " + MyName + ".  CHECK INSPECTOR");
                }
            }

            generatePowerResource = null;
            if (generatePowerResourceName != null && generatePowerResourceName != string.Empty) {
                PowerResource tmpPowerResource = SystemPowerResourceManager.MyInstance.GetResource(generatePowerResourceName);
                if (tmpPowerResource != null) {
                    generatePowerResource = tmpPowerResource;
                } else {
                    Debug.LogError("BaseAbility.SetupScriptableObjects(): Could not find power resource: " + powerResourceName + " while inititalizing " + MyName + ".  CHECK INSPECTOR");
                }
            }


            channeledAbilityEffects = new List<AbilityEffect>();
            if (channeledAbilityEffectnames != null) {
                foreach (string abilityEffectName in channeledAbilityEffectnames) {
                    AbilityEffect abilityEffect = SystemAbilityEffectManager.MyInstance.GetResource(abilityEffectName);
                    if (abilityEffect != null) {
                        channeledAbilityEffects.Add(abilityEffect);
                    } else {
                        Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find ability effect: " + abilityEffectName + " while inititalizing " + MyName + ".  CHECK INSPECTOR");
                    }
                }
            }

            characterClassRequirementList = new List<CharacterClass>();
            if (characterClassRequirementList != null) {
                foreach (string characterClassName in characterClassRequirements) {
                    CharacterClass tmpCharacterClass = SystemCharacterClassManager.MyInstance.GetResource(characterClassName);
                    if (tmpCharacterClass != null) {
                        characterClassRequirementList.Add(tmpCharacterClass);
                    } else {
                        Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find character class : " + characterClassName + " while inititalizing " + MyName + ".  CHECK INSPECTOR");
                    }
                }
            }


        }

    }

    public enum PrefabSpawnLocation { None, Caster, Target, Point, OriginalTarget, targetPoint }

}