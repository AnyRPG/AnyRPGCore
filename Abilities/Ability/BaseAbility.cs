using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "NewAbility",menuName = "AnyRPG/Abilities/Ability")]
    public abstract class BaseAbility : DescribableResource, IUseable, IMoveable, ITargetable, ILearnable {

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

        [Tooltip("Physical prefabs to attach to bones on the character unit")]
        [SerializeField]
        private List<AbilityAttachmentNode> holdableObjectList = new List<AbilityAttachmentNode>();

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

        [Tooltip("This ability can be cast while moving.")]
        [SerializeField]
        protected bool canCastWhileMoving = false;

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

        [Tooltip("Delay the spending of the resource by this many seconds when the ability is cast.  Useful if the ability can kill the caster to give time to complete final cast.")]
        [SerializeField]
        protected float spendDelay = 0f;

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

        [Tooltip("If true, after the cast time is calculated, it is affected by the Speed secondary stat.")]
        [SerializeField]
        protected bool useSpeedMultipliers = true;

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
        private bool requiresTarget = false;

        [Tooltip("If true, the character must have an uninterrupted line of sight to the target.")]
        [SerializeField]
        private bool requireLineOfSight = false;

        [Tooltip("If true, the target must be a character and must be alive.")]
        [SerializeField]
        private bool requiresLiveTarget = true;

        [Tooltip("If true, the target must be a character and must be dead.")]
        [SerializeField]
        private bool requireDeadTarget = false;

        [Tooltip("Can the character cast this ability on itself?")]
        [SerializeField]
        protected bool canCastOnSelf = false;

        [Tooltip("Can the character cast this ability on others?")]
        [SerializeField]
        protected bool canCastOnOthers = false;

        [Tooltip("Can the character cast this ability on a character belonging to an enemy faction?")]
        [SerializeField]
        protected bool canCastOnEnemy = false;

        [Tooltip("Can the character cast this ability on a character with no relationship?")]
        [SerializeField]
        protected bool canCastOnNeutral = false;

        [Tooltip("Can the character cast this ability on a character belonging to a friendly faction?")]
        [SerializeField]
        protected bool canCastOnFriendly = false;

        [Tooltip("If no target is given, automatically cast on the caster")]
        [SerializeField]
        private bool autoSelfCast = false;

        [Header("Range")]

        [Tooltip("If true, the target must be within melee range (within hitbox) to cast this ability.")]
        [SerializeField]
        protected bool useMeleeRange = false;

        [Tooltip("If melee range is not used, this ability can be cast on targets this many meters away.")]
        [SerializeField]
        protected int maxRange;

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

        public AnimationClip CastingAnimationClip {
            get => (animationProfile != null && animationProfile.MyAttackClips != null && animationProfile.MyAttackClips.Count > 0 ? animationProfile.MyAttackClips[0] : null);
        }
        public int RequiredLevel { get => requiredLevel; }
        public bool AutoLearn { get => autoLearn; }
        public bool AutoAddToBars { get => autoAddToBars; }
        public bool UseableWithoutLearning { get => useableWithoutLearning; }


        public virtual float GetAbilityCastingTime(IAbilityCaster abilityCaster) {
            if (useSpeedMultipliers) {
                return BaseAbilityCastingTime * abilityCaster.AbilityManager.GetSpeed();
            }
            return BaseAbilityCastingTime;
        }

        /// <summary>
        /// return the casting time of the ability without any speed modifiers applied
        /// </summary>
        public virtual float BaseAbilityCastingTime {
            get {
                if (useAnimationCastTime == false) {
                    return abilityCastingTime;
                } else {
                    if (CastingAnimationClip != null) {
                        return CastingAnimationClip.length;
                    }
                    return abilityCastingTime;
                }
            }
            set => abilityCastingTime = value;
        }
        public bool RequiresTarget { get => requiresTarget; set => requiresTarget = value; }
        public bool RequiresGroundTarget { get => requiresGroundTarget; set => requiresGroundTarget = value; }
        public Color GroundTargetColor { get => groundTargetColor; set => groundTargetColor = value; }
        public bool CanCastOnSelf { get => canCastOnSelf; }
        public bool CanCastOnEnemy { get => canCastOnEnemy; }
        public bool CanCastOnFriendly { get => canCastOnFriendly; }
        public bool CanSimultaneousCast { get => canSimultaneousCast; set => canSimultaneousCast = value; }
        public bool RequireDeadTarget { get => requireDeadTarget; set => requireDeadTarget = value; }
        public bool IgnoreGlobalCoolDown { get => ignoreGlobalCoolDown; set => ignoreGlobalCoolDown = value; }
        public AudioClip CastingAudioClip { get => (castingAudioProfile == null ? null : castingAudioProfile.AudioClip); }
        public AudioClip AnimationHitAudioClip { get => (animationHitAudioProfile == null ? null : animationHitAudioProfile.AudioClip); }
        public bool AnimatorCreatePrefabs { get => animatorCreatePrefabs; set => animatorCreatePrefabs = value; }
        public List<AnimationClip> AnimationClips { get => (animationProfile != null ? animationProfile.MyAttackClips : null); }
        public int MaxRange { get => maxRange; set => maxRange = value; }
        public bool UseMeleeRange { get => useMeleeRange; set => useMeleeRange = value; }
        public List<string> WeaponAffinityNames { get => weaponAffinityNames; set => weaponAffinityNames = value; }
        public bool RequireOutOfCombat { get => requireOutOfCombat; set => requireOutOfCombat = value; }
        public List<string> AbilityEffectNames { get => abilityEffectNames; set => abilityEffectNames = value; }
        public List<AbilityEffect> AbilityEffects { get => abilityEffects; set => abilityEffects = value; }
        public AnimationProfile AnimationProfile { get => animationProfile; set => animationProfile = value; }
        public float GroundTargetRadius { get => groundTargetRadius; set => groundTargetRadius = value; }
        public List<WeaponSkill> WeaponAffinityList { get => weaponAffinityList; set => weaponAffinityList = value; }
        public bool RequireLineOfSight { get => requireLineOfSight; set => requireLineOfSight = value; }
        public List<CharacterClass> CharacterClassRequirementList { get => characterClassRequirementList; set => characterClassRequirementList = value; }
        public PowerResource PowerResource { get => powerResource; set => powerResource = value; }
        public PowerResource GeneratePowerResource { get => generatePowerResource; set => generatePowerResource = value; }
        public int BaseResourceGain { get => baseResourceGain; set => baseResourceGain = value; }
        public int ResourceGainPerLevel { get => resourceGainPerLevel; set => resourceGainPerLevel = value; }
        public float SpendDelay { get => spendDelay; set => spendDelay = value; }
        public LineOfSightSourceLocation LineOfSightSourceLocation { get => LineOfSightSourceLocation.Caster; }
        public TargetRangeSourceLocation TargetRangeSourceLocation { get => TargetRangeSourceLocation.Caster; }
        public bool UseSpeedMultipliers { get => useSpeedMultipliers; set => useSpeedMultipliers = value; }
        public bool CanCastWhileMoving { get => canCastWhileMoving; set => canCastWhileMoving = value; }
        public bool CanCastOnNeutral { get => canCastOnNeutral; set => canCastOnNeutral = value; }
        public bool CanCastOnOthers { get => canCastOnOthers; set => canCastOnOthers = value; }
        public virtual List<AbilityAttachmentNode> HoldableObjectList { get => holdableObjectList; set => holdableObjectList = value; }

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
                    requireWeaponSkills.Add(_weaponAffinity.DisplayName);
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
                costString = "\nCost: " + GetResourceCost(PlayerManager.MyInstance.MyCharacter) + " " + powerResource.DisplayName;
            }

            return string.Format("Cast time: {0} second(s)\nCooldown: {1} second(s){2}\nRange: {3}\n<color=#ffff00ff>{4}</color>{5}", GetAbilityCastingTime(PlayerManager.MyInstance.MyCharacter).ToString("F1"), abilityCoolDown, costString, abilityRange, description, addString);
        }

        public string GetShortDescription() {
            return description;
        }

        public virtual float GetResourceCost(IAbilityCaster abilityCaster) {
            if (abilityCaster != null && powerResource != null) {
                return baseResourceCost + (abilityCaster.AbilityManager.Level * resourceCostPerLevel);
            }
            return baseResourceCost;
        }

        public virtual float GetResourceGain(IAbilityCaster abilityCaster) {
            //Debug.Log(MyName + ".BaseAbility.GetResourceGain(" + (abilityCaster == null ? "null" : abilityCaster.Name) + ")");
            if (abilityCaster != null) {
                //Debug.Log(MyName + ".BaseAbility.GetResourceGain() level: " + abilityCaster.Level + "; gainperLevel: " + resourceGainPerLevel + "; base: " + baseResourceGain);

                return baseResourceGain + (abilityCaster.AbilityManager.Level * resourceGainPerLevel);
            }
            return baseResourceCost;
        }

        public virtual AudioClip GetAnimationHitSound() {
            return AnimationHitAudioClip;
        }

        public virtual AudioClip GetHitSound(IAbilityCaster abilityCaster) {
            // only meant for animated Abilities
            return null;
        }

        public virtual float GetLOSMaxRange(IAbilityCaster source, Interactable target) {
            //Debug.Log(MyName + ".BaseAbility.GetLOSMaxRange(" + (source == null ? "null" : source.Name) + ", " + (target == null ? "null" : target.name) + ")");
            if (source.AbilityManager.PerformLOSCheck(target, this)) {
                //Debug.Log(MyName + ".BaseAbility.GetLOSMaxRange(" + (source == null ? "null" : source.Name) + ", " + (target == null ? "null" : target.name) + "): return " + MaxRange);
                return MaxRange;
            }
            //Debug.Log(MyName + ".BaseAbility.GetLOSMaxRange(" + (source == null ? "null" : source.Name) + ", " + (target == null ? "null" : target.name) + "): return " + source.GetMeleeRange());
            return source.AbilityManager.GetMeleeRange();
        }

        public virtual void PerformChanneledEffect(IAbilityCaster source, Interactable target, AbilityEffectContext abilityEffectContext) {
            //Debug.Log("BaseAbility.PerformChanneledEffect(" + MyName + ", " + (source == null ? "null" : source.name) + ", " + (target == null ? "null" : target.name) + ")");
            foreach (AbilityEffect abilityEffect in channeledAbilityEffects) {
                AbilityEffect _abilityEffect = SystemAbilityEffectManager.MyInstance.GetNewResource(abilityEffect.DisplayName);

                // channeled effects need to override the object lifetime so they get destroyed at the tickrate
                //_abilityEffect.MyAbilityEffectObjectLifetime = tickRate;
                _abilityEffect.Cast(source, target, target, abilityEffectContext);
            }
        }

        public bool CanCast(IAbilityCaster sourceCharacter) {
            if (weaponAffinityNames.Count == 0) {
                // no restrictions, automatically true
                return true;
            } else {
                return sourceCharacter.AbilityManager.PerformWeaponAffinityCheck(this);
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

        public virtual bool Cast(IAbilityCaster sourceCharacter, Interactable target, AbilityEffectContext abilityEffectContext) {
            //Debug.Log(resourceName + ".BaseAbility.Cast(" + sourceCharacter.AbilityManager.Name + ", " + (target == null ? "null" : target.name) + ")");
            if (!CanCast(sourceCharacter)) {
                //Debug.Log(resourceName + ".BaseAbility.Cast(" + sourceCharacter.AbilityManager.name + ", " + (target == null ? "null" : target.name) + ", " + groundTarget + "): CAN'T CAST!!!");
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
                sourceCharacter.AbilityManager.BeginAbilityCoolDown(this, animationLength);
            }
        }

        public virtual void ProcessGCDAuto(IAbilityCaster sourceCharacter) {
            //Debug.Log(MyName + ".BaseAbility.ProcessGCDManual()");
            ProcessGCDManual(sourceCharacter);
        }

        public virtual void ProcessGCDManual(IAbilityCaster sourceCharacter, float usedCoolDown = 0f) {
            //Debug.Log(MyName + ".BaseAbility.ProcessGCDManual(" + usedCoolDown + ")");
            if (CanSimultaneousCast == false && IgnoreGlobalCoolDown == false && GetAbilityCastingTime(sourceCharacter) == 0f) {
                sourceCharacter.AbilityManager.InitiateGlobalCooldown(usedCoolDown);
            } else {
                //Debug.Log(gameObject.name + ".PlayerAbilityManager.PerformAbility(" + ability.MyName + "): ability.MyAbilityCastingTime: " + ability.MyAbilityCastingTime);
            }
        }

        public virtual void ProcessAbilityPrefabs(IAbilityCaster sourceCharacter) {
            //Debug.Log(MyName + ".BaseAbility.ProcessAbilityPrefabs()");
            if (holdableObjectList == null || holdableObjectList.Count == 0) {
                return;
            }

            sourceCharacter.AbilityManager.DespawnAbilityObjects();
        }

        public virtual bool CanUseOn(Interactable target, IAbilityCaster sourceCharacter, bool performCooldownChecks = true, AbilityEffectContext abilityEffectContext = null) {
            //Debug.Log(MyName + ".BaseAbility.CanUseOn(" + (target != null ? target.name : "null") + ", " + (sourceCharacter != null ? sourceCharacter.AbilityManager.name : "null") + ")");

            if (abilityEffects != null && abilityEffects.Count > 0 && useAbilityEffectTargetting == true) {
                return abilityEffects[0].CanUseOn(target, sourceCharacter, abilityEffectContext);
            }

            // create target booleans
            bool targetIsSelf = false;
            CharacterUnit targetCharacterUnit = null;

            // special case for ground targeted spells cast by AI since AI currently has to cast a ground targeted spell on its current target
            if (requiresGroundTarget == true
                && maxRange > 0
                && target != null
                && ((sourceCharacter as BaseCharacter) is BaseCharacter) && (sourceCharacter as BaseCharacter).UnitController.UnitControllerMode == UnitControllerMode.AI && Vector3.Distance(sourceCharacter.AbilityManager.UnitGameObject.transform.position, target.transform.position) > maxRange) {
                return false;
            }

            // if this ability requires no target, then we can always cast it
            if (requiresTarget == false) {
                return true;
            }

            // if we got here, we require a target, therefore if we don't have one, we can't cast
            if (target == null) {
                return false;
            }

            // determine if we are casting on ourself
            if (target == sourceCharacter.AbilityManager.UnitGameObject) {
                targetIsSelf = true;
            }

            // first check if the target is ourself
            if (targetIsSelf == true) {
                if (canCastOnSelf == false) {
                    return false;
                } else {
                    return true;
                }
            }

            // if we made it this far, the target is not ourself

            // the target is another unit, but this ability cannot be cast on others
            if (canCastOnOthers == false) {
                return false;
            }

            targetCharacterUnit = target.GetComponent<CharacterUnit>();
            if (targetCharacterUnit != null) {

                // liveness checks
                if (targetCharacterUnit.BaseCharacter.CharacterStats.IsAlive == false && requiresLiveTarget == true) {
                    //Debug.Log("This ability requires a live target");
                    //CombatLogUI.MyInstance.WriteCombatMessage(resourceName + " requires a live target!");
                    return false;
                }
                if (targetCharacterUnit.BaseCharacter.CharacterStats.IsAlive == true && requireDeadTarget == true) {
                    //Debug.Log("This ability requires a dead target");
                    //CombatLogUI.MyInstance.WriteCombatMessage(resourceName + " requires a dead target!");
                    return false;
                }

                if (!sourceCharacter.AbilityManager.PerformFactionCheck(this, targetCharacterUnit, targetIsSelf)) {
                    return false;
                }

            } else {
                if (requiresLiveTarget == true || requireDeadTarget == true) {
                    // something that is not a character unit cannot satisfy the alive or dead conditions because it is inanimate
                    return false;
                }
                if (canCastOnFriendly == true || CanCastOnNeutral == true || canCastOnEnemy == true) {
                    // something that is not a character unit cannot satisfy the relationship conditions because it is inanimate
                    return false;
                }
            }

            // if we made it this far we passed liveness and relationship checks.
            // since the target is not ourself, and it is valid, we should perform a range check

            if (!sourceCharacter.AbilityManager.IsTargetInAbilityRange(this, target, abilityEffectContext)) {
                return false;
            }

            //Debug.Log(MyName + ".BaseAbility.CanUseOn(): returning true");
            return true;
        }

        //public virtual void PerformAbilityEffect(BaseAbility ability, GameObject source, GameObject target) {
        public virtual bool PerformAbilityEffects(IAbilityCaster source, Interactable target, AbilityEffectContext abilityEffectContext) {
            //Debug.Log(MyName + ".BaseAbility.PerformAbilityEffects(" + source.name + ", " + (target ? target.name : "null") + ", " + groundTarget + ")");
            if (abilityEffects.Count == 0) {
                //Debug.Log(resourceName + ".BaseAbility.PerformAbilityEffects(" + source.name + ", " + (target ? target.name : "null") + "): THERE ARE NO EFFECTS ATTACHED TO THIS ABILITY!");
                // this is fine for channeled abilities
            }

            // perform hit / miss check only if baseability requires target and return false if miss
            if (requiresTarget) {
                if (!source.AbilityManager.AbilityHit(target, abilityEffectContext)) {
                    return false;
                }
            }

            // generate power resource
            source.AbilityManager.GeneratePower(this);

            foreach (AbilityEffect abilityEffect in abilityEffects) {
                if (abilityEffect == null) {
                    Debug.Log("Forgot to set ability affect in inspector?");
                }
                AbilityEffectContext abilityEffectOutput = abilityEffectContext.GetCopy();
                AbilityEffect _abilityEffect = SystemAbilityEffectManager.MyInstance.GetNewResource(abilityEffect.DisplayName);
                if (_abilityEffect != null && _abilityEffect.CanUseOn(target, source, abilityEffectContext)) {
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
        public virtual Interactable ReturnTarget(IAbilityCaster sourceCharacter, Interactable target, bool performCooldownChecks = true, AbilityEffectContext abilityEffectContext = null) {
            //Debug.Log(MyName + ".BaseAbility.ReturnTarget(" + (sourceCharacter == null ? "null" : sourceCharacter.AbilityManager.MyName) + ", " + (target == null ? "null" : target.name) + ")");
            // before we get here, a validity check has already been performed, so no need to unset any targets
            // we are only concerned with redirecting the target to self if auto-selfcast is enabled

            if (sourceCharacter == null || sourceCharacter.AbilityManager.UnitGameObject == null) {
                //Debug.Log("BaseAbility.ReturnTarget(): source is null! This should never happen!!!!!");
                return null;
            }

            // perform ability dependent checks
            if (CanUseOn(target, sourceCharacter, performCooldownChecks, abilityEffectContext) == false) {
                //Debug.Log(MyName + ".BaseAbility.CanUseOn(" + (target != null ? target.name : "null") + " was false");
                if (canCastOnSelf && autoSelfCast) {
                    target = (sourceCharacter as MonoBehaviour).GetComponent<Interactable>();
                    //Debug.Log(MyName + ".BaseAbility.ReturnTarget(): returning target as sourcecharacter: " + target.name);
                    return target;
                } else {
                    //Debug.Log(MyName + ".BaseAbility.ReturnTarget(): returning null");
                    return null;
                }
            }

            return target;
        }

        public virtual void StartCasting(IAbilityCaster source) {
            //Debug.Log("BaseAbility.OnCastStart(" + source.name + ")");
            //Debug.Log("setting casting animation");
            if (CastingAnimationClip != null) {
                source.AbilityManager.PerformCastingAnimation(CastingAnimationClip, this);
            }
            // GRAVITY FREEZE FOR CASTING
            // DISABLING SINCE IT IS CAUSING INSTANT CASTS TO STOP CHARACTER WHILE MOVING.  MAYBE CHECK IF CAST TIMER AND THEN DO IT?
            // NEXT LINE NO LONGER NEEDED SINCE WE NOW ACTUALLY CHECK THE REAL DISTANCE MOVED BY THE CHARACTER AND DON'T CANCEL CAST UNTIL DISTANCE IS > 0.1F
            //source.MyRigidBody.constraints = RigidbodyConstraints.FreezeAll;
            /*
            if (abilityCastingPrefab != null) {
                if (abilityCastingPrefabRef == null) {
                    //Vector3 relativePrefabOffset = source.AbilityManager.UnitGameObject.transform.TransformPoint(prefabOffset);
                    //Vector3 spawnLocation = new Vector3(source.AbilityManager.UnitGameObject.transform.position.x + relativePrefabOffset.x, source.MyCharacterUnit.transform.position.y + relativePrefabOffset.y, source.MyCharacterUnit.transform.position.z + relativePrefabOffset.z);
                    Vector3 spawnLocation = source.AbilityManager.UnitGameObject.transform.TransformPoint(prefabOffset);
                    //Debug.Log("BaseAbility.OnCastStart(): Instantiating spell casting prefab at " + source.transform.position + "; spawnLocation is : " + spawnLocation);
                    //abilityCastingPrefabRef = Instantiate(abilityCastingPrefab, spawnLocation, source.AbilityManager.UnitGameObject.transform.rotation * Quaternion.Euler(source.MyCharacterUnit.transform.TransformDirection(prefabRotation)), source.transform);
                    abilityCastingPrefabRef = Instantiate(abilityCastingPrefab, spawnLocation, Quaternion.LookRotation(source.AbilityManager.UnitGameObject.transform.forward) * Quaternion.Euler(prefabRotation), source.MyCharacterUnit.transform);
                }
            }
            */
            //source.MyCharacterAbilityManager.OnCastStop += HandleCastStop;
        }

        public virtual float OnCastTimeChanged(float currentCastPercent, float nextTickPercent, IAbilityCaster source, Interactable target, AbilityEffectContext abilityEffectContext) {
            //Debug.Log("BaseAbility.OnCastTimeChanged()");
            // overwrite me
            if (currentCastPercent >= nextTickPercent) {
                PerformChanneledEffect(source, target, abilityEffectContext);
                nextTickPercent += (tickRate / BaseAbilityCastingTime);
            }
            return nextTickPercent;
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
                if (!CharacterClassRequirementList.Contains(PlayerManager.MyInstance.MyCharacter.CharacterClass)) {
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


        public override void SetupScriptableObjects() {
            base.SetupScriptableObjects();
            abilityEffects = new List<AbilityEffect>();
            if (AbilityEffectNames != null) {
                foreach (string abilityEffectName in AbilityEffectNames) {
                    AbilityEffect abilityEffect = SystemAbilityEffectManager.MyInstance.GetResource(abilityEffectName);
                    if (abilityEffect != null) {
                        abilityEffects.Add(abilityEffect);
                    } else {
                        Debug.LogError("BaseAbility.SetupScriptableObjects(): Could not find ability effect: " + abilityEffectName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                    }
                }
            }
            if (holdableObjectList != null) {
                foreach (AbilityAttachmentNode holdableObjectAttachment in holdableObjectList) {
                    if (holdableObjectAttachment != null) {
                        holdableObjectAttachment.SetupScriptableObjects();
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
                        Debug.LogError("BaseAbility.SetupScriptableObjects(): Could not find weapon skill: " + weaponAffinityName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                    }
                }
            }

            castingAudioProfile = null;
            if (castingAudioProfileName != null && castingAudioProfileName != string.Empty) {
                AudioProfile audioProfile = SystemAudioProfileManager.MyInstance.GetResource(castingAudioProfileName);
                if (audioProfile != null) {
                    castingAudioProfile = audioProfile;
                } else {
                    Debug.LogError("BaseAbility.SetupScriptableObjects(): Could not find audio profile: " + castingAudioProfileName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                }
            }

            animationHitAudioProfile = null;
            if (animationHitAudioProfileName != null && animationHitAudioProfileName != string.Empty) {
                AudioProfile audioProfile = SystemAudioProfileManager.MyInstance.GetResource(animationHitAudioProfileName);
                if (audioProfile != null) {
                    animationHitAudioProfile = audioProfile;
                } else {
                    Debug.LogError("BaseAbility.SetupScriptableObjects(): Could not find audio profile: " + animationHitAudioProfileName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                }
            }

            animationProfile = null;
            if (animationProfileName != null && animationProfileName != string.Empty) {
                AnimationProfile tmpAnimationProfile = SystemAnimationProfileManager.MyInstance.GetResource(animationProfileName);
                if (tmpAnimationProfile != null) {
                    animationProfile = tmpAnimationProfile;
                } else {
                    Debug.LogError("BaseAbility.SetupScriptableObjects(): Could not find animation profile: " + animationProfileName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                }
            }

            powerResource = null;
            if (powerResourceName != null && powerResourceName != string.Empty) {
                PowerResource tmpPowerResource = SystemPowerResourceManager.MyInstance.GetResource(powerResourceName);
                if (tmpPowerResource != null) {
                    powerResource = tmpPowerResource;
                } else {
                    Debug.LogError("BaseAbility.SetupScriptableObjects(): Could not find power resource: " + powerResourceName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                }
            }

            generatePowerResource = null;
            if (generatePowerResourceName != null && generatePowerResourceName != string.Empty) {
                PowerResource tmpPowerResource = SystemPowerResourceManager.MyInstance.GetResource(generatePowerResourceName);
                if (tmpPowerResource != null) {
                    generatePowerResource = tmpPowerResource;
                } else {
                    Debug.LogError("BaseAbility.SetupScriptableObjects(): Could not find power resource: " + powerResourceName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                }
            }


            channeledAbilityEffects = new List<AbilityEffect>();
            if (channeledAbilityEffectnames != null) {
                foreach (string abilityEffectName in channeledAbilityEffectnames) {
                    AbilityEffect abilityEffect = SystemAbilityEffectManager.MyInstance.GetResource(abilityEffectName);
                    if (abilityEffect != null) {
                        channeledAbilityEffects.Add(abilityEffect);
                    } else {
                        Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find ability effect: " + abilityEffectName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
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
                        Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find character class : " + characterClassName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                    }
                }
            }


        }

    }

    public enum PrefabSpawnLocation { None, Caster, Target, GroundTarget, OriginalTarget, targetPoint }

}