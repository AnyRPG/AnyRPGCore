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

        [Tooltip("Ability to use ability prefabs, both to use Weapon and ability prefabs, weapon to use only weapon prefabs")]
        [SerializeField]
        protected AbilityPrefabSource abilityPrefabSource = AbilityPrefabSource.Both;

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

        [Tooltip("If true, the ability will use the casting animations from the caster.")]
        [SerializeField]
        protected bool useUnitCastAnimations = false;

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
        protected float abilityCoolDown = 0f;

        [Tooltip("By default an ability cooldown is initiated once a cast is complete.  Check this option to start the cooldown at the beginning of the cast")]
        [SerializeField]
        protected bool coolDownOnCast = false;

        [Header("Target Properties")]

        [Tooltip("Ignore the below target options and use the check from the first ability effect instead")]
        [SerializeField]
        private bool useAbilityEffectTargetting = false;

        [SerializeField]
        private AbilityTargetProps targetOptions = new AbilityTargetProps();

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

        private List<AbilityEffect> abilityEffects = new List<AbilityEffect>();

        public AnimationClip CastingAnimationClip {
            get {
                if (animationProfile != null
                    && animationProfile.AnimationProps.CastClips != null
                    && animationProfile.AnimationProps.CastClips.Count > 0) {
                    return animationProfile.AnimationProps.CastClips[0];
                }
                return null;
            }
        }
        public int RequiredLevel { get => requiredLevel; }
        public bool AutoLearn { get => autoLearn; }
        public bool AutoAddToBars { get => autoAddToBars; }
        public bool UseableWithoutLearning { get => useableWithoutLearning; }

        /// <summary>
        /// return the casting time of the ability without any speed modifiers applied
        /// </summary>
        public virtual float BaseAbilityCastingTime {
            get {
                if (useAnimationCastTime == false) {
                    return abilityCastingTime;
                } else {
                    // TODO : FIX : get casting animation clip based on source caster
                    if (CastingAnimationClip != null) {
                        return CastingAnimationClip.length;
                    }
                    return abilityCastingTime;
                }
            }
            set => abilityCastingTime = value;
        }
        public bool CanSimultaneousCast { get => canSimultaneousCast; set => canSimultaneousCast = value; }
        public bool IgnoreGlobalCoolDown { get => ignoreGlobalCoolDown; set => ignoreGlobalCoolDown = value; }
        public AudioClip CastingAudioClip { get => (castingAudioProfile == null ? null : castingAudioProfile.AudioClip); }
        public AudioClip AnimationHitAudioClip { get => (animationHitAudioProfile == null ? null : animationHitAudioProfile.AudioClip); }
        public bool AnimatorCreatePrefabs { get => animatorCreatePrefabs; set => animatorCreatePrefabs = value; }
        public List<AnimationClip> AttackClips { get => (animationProfile != null ? animationProfile.AnimationProps.AttackClips : null); }
        public List<AnimationClip> CastClips { get => (animationProfile != null ? animationProfile.AnimationProps.CastClips : null); }
        public List<string> WeaponAffinityNames { get => weaponAffinityNames; set => weaponAffinityNames = value; }
        public bool RequireOutOfCombat { get => requireOutOfCombat; set => requireOutOfCombat = value; }
        public List<string> AbilityEffectNames { get => abilityEffectNames; set => abilityEffectNames = value; }
        /*
        public List<AbilityEffect> AbilityEffects {
            get {
                 return abilityEffects;
            }
        }
        */
        //public AnimationProfile AnimationProfile { get => animationProfile; set => animationProfile = value; }
        public List<WeaponSkill> WeaponAffinityList { get => weaponAffinityList; set => weaponAffinityList = value; }
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
        public bool CoolDownOnCast { get => coolDownOnCast; set => coolDownOnCast = value; }
        public float AbilityCoolDown { get => abilityCoolDown; set => abilityCoolDown = value; }

        public virtual bool IsUseableStale(ActionButton actionButton) {
            if (PlayerManager.MyInstance.MyCharacter.CharacterAbilityManager.HasAbility(this)) {
                return false;
            }
            return true;
        }

        public bool ActionButtonUse() {
            return Use();
        }

        public IUseable GetFactoryUseable() {
            return SystemAbilityManager.MyInstance.GetResource(DisplayName);
        }

        public virtual void UpdateChargeCount(ActionButton actionButton) {
            UIManager.MyInstance.UpdateStackSize(actionButton, 0, false);
        }

        public virtual bool HadSpecialIcon(ActionButton actionButton) {
            return false;
        }

        public virtual void UpdateActionButtonVisual(ActionButton actionButton) {
            //Debug.Log(DisplayName + ".BaseAbility.UpdateActionButtonVisual()");
            // set cooldown icon on abilities that don't have enough resources to cast
            if (PowerResource != null
                && (GetResourceCost(PlayerManager.MyInstance.ActiveCharacter) >= PlayerManager.MyInstance.ActiveCharacter.CharacterStats.GetPowerResourceAmount(PowerResource))) {
                //Debug.Log(DisplayName + ".BaseAbility.UpdateActionButtonVisual(): not enough resources to cast this ability.  enabling full cooldown");
                actionButton.EnableFullCoolDownIcon();
                return;
            }

            if (HadSpecialIcon(actionButton)) {
                return;
            }

            if (RequireOutOfCombat) {
                if (PlayerManager.MyInstance.MyCharacter.CharacterCombat.GetInCombat() == true) {
                    //Debug.Log("ActionButton.UpdateVisual(): can't cast due to being in combat");
                    actionButton.EnableFullCoolDownIcon();
                    return;
                }
            }

            if (!CanCast(PlayerManager.MyInstance.MyCharacter)) {
                //Debug.Log(DisplayName + ".BaseAbility.UpdateActionButtonVisual(): can't cast due to spell restrictions");
                actionButton.EnableFullCoolDownIcon();
                return;
            }


            if (PlayerManager.MyInstance.MyCharacter.CharacterAbilityManager.MyRemainingGlobalCoolDown > 0f
                || PlayerManager.MyInstance.MyCharacter.CharacterAbilityManager.MyAbilityCoolDownDictionary.ContainsKey(DisplayName)) {
                //Debug.Log(DisplayName + ".BaseAbility.UpdateActionButtonVisual(): Ability is on cooldown");
                if (actionButton.CoolDownIcon.isActiveAndEnabled != true) {
                    //Debug.Log("ActionButton.UpdateVisual(): coolDownIcon is not enabled: " + (useable == null ? "null" : useable.DisplayName));
                    actionButton.CoolDownIcon.enabled = true;
                }
                if (actionButton.CoolDownIcon.sprite != actionButton.MyIcon.sprite) {
                    //Debug.Log("Setting coolDownIcon to match MyIcon");
                    actionButton.CoolDownIcon.sprite = actionButton.MyIcon.sprite;
                    actionButton.CoolDownIcon.color = new Color32(0, 0, 0, 230);
                    actionButton.CoolDownIcon.fillMethod = Image.FillMethod.Radial360;
                    //coolDownIcon.fillOrigin = Image.Origin360.Top;
                    actionButton.CoolDownIcon.fillClockwise = false;
                }
                //Debug.Log("remainingCooldown: " + this.remainingCooldown + "; totalcooldown: " + (MyUseable as BaseAbility).abilityCoolDown);
                float remainingAbilityCoolDown = 0f;
                float initialCoolDown = 0f;
                if (PlayerManager.MyInstance.MyCharacter.CharacterAbilityManager.MyAbilityCoolDownDictionary.ContainsKey(DisplayName)) {
                    remainingAbilityCoolDown = PlayerManager.MyInstance.MyCharacter.CharacterAbilityManager.MyAbilityCoolDownDictionary[DisplayName].MyRemainingCoolDown;
                    initialCoolDown = PlayerManager.MyInstance.MyCharacter.CharacterAbilityManager.MyAbilityCoolDownDictionary[DisplayName].MyInitialCoolDown;
                } else {
                    initialCoolDown = abilityCoolDown;
                }
                //float globalCoolDown
                float fillAmount = Mathf.Max(remainingAbilityCoolDown, PlayerManager.MyInstance.MyCharacter.CharacterAbilityManager.MyRemainingGlobalCoolDown) /
                    (remainingAbilityCoolDown > PlayerManager.MyInstance.MyCharacter.CharacterAbilityManager.MyRemainingGlobalCoolDown ? initialCoolDown : PlayerManager.MyInstance.MyCharacter.CharacterAbilityManager.MyInitialGlobalCoolDown);
                //Debug.Log("Setting fill amount to: " + fillAmount);
                if (actionButton.CoolDownIcon.fillAmount != fillAmount) {
                    actionButton.CoolDownIcon.fillAmount = fillAmount;
                }
            } else {
                actionButton.DisableCoolDownIcon();
            }
        }

        public virtual Coroutine ChooseMonitorCoroutine(ActionButton actionButton) {
            // actionbuttons can be disabled, but the systemability manager will not.  That's why the ability is monitored here
                //Debug.Log("ActionButton.OnUseableUse(" + ability.MyName + "): WAS NOT ANIMATED AUTO ATTACK");
                //if (abilityCoRoutine == null) {
                //if (monitorCoroutine == null) {
                    return SystemAbilityManager.MyInstance.StartCoroutine(actionButton.MonitorAbility(this));
                //}
            //return null;
        }

        public TargetProps GetTargetOptions(IAbilityCaster abilityCaster) {
            if (useAbilityEffectTargetting == true && GetAbilityEffects(abilityCaster).Count > 0) {
                return GetAbilityEffects(abilityCaster)[0].GetTargetOptions(abilityCaster);
            }
            return targetOptions;
        }

        public virtual List<AbilityEffect> GetAbilityEffects(IAbilityCaster abilityCaster) {
            return abilityEffects;
        }

        public virtual List<AbilityAttachmentNode> GetHoldableObjectList(IAbilityCaster abilityCaster) {
            return holdableObjectList;
        }

        public virtual float GetAbilityCastingTime(IAbilityCaster abilityCaster) {
            if (useSpeedMultipliers) {
                return BaseAbilityCastingTime * abilityCaster.AbilityManager.GetSpeed();
            }
            return BaseAbilityCastingTime;
        }

        public List<AnimationClip> GetCastClips(IAbilityCaster sourceCharacter) {
            List<AnimationClip> animationClips = new List<AnimationClip>();
            if (useUnitCastAnimations == true) {
                animationClips = sourceCharacter.AbilityManager.GetUnitCastAnimations();
            } else {
                animationClips = CastClips;
            }
            return animationClips;
        }

        public AnimationProps GetUnitAnimationProps(IAbilityCaster sourceCharacter) {
            if (useUnitCastAnimations == true) {
                return sourceCharacter.AbilityManager.GetUnitAnimationProps();
            } else {
                return animationProfile.AnimationProps;
            }
        }



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

            string abilityRange = (GetTargetOptions(PlayerManager.MyInstance.MyCharacter).UseMeleeRange == true ? "melee" : GetTargetOptions(PlayerManager.MyInstance.MyCharacter).MaxRange + " meters");

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
            //Debug.Log(DisplayName + ".BaseAbility.GetLOSMaxRange(" + (source == null ? "null" : source.AbilityManager.Name) + ", " + (target == null ? "null" : target.name) + ")");
            if (source.AbilityManager.PerformLOSCheck(target, this)) {
                //Debug.Log(DisplayName + ".BaseAbility.GetLOSMaxRange(" + (source == null ? "null" : source.AbilityManager.Name) + ", " + (target == null ? "null" : target.name) + "): return max " + GetTargetOptions(source).MaxRange);
                return GetTargetOptions(source).MaxRange;
            }
            //Debug.Log(DisplayName + ".BaseAbility.GetLOSMaxRange(" + (source == null ? "null" : source.AbilityManager.Name) + ", " + (target == null ? "null" : target.name) + "): return melee " + source.AbilityManager.GetMeleeRange());
            return source.AbilityManager.GetMeleeRange();
        }

        public virtual void PerformChanneledEffect(IAbilityCaster source, Interactable target, AbilityEffectContext abilityEffectContext) {
            //Debug.Log("BaseAbility.PerformChanneledEffect(" + MyName + ", " + (source == null ? "null" : source.name) + ", " + (target == null ? "null" : target.name) + ")");
            foreach (AbilityEffect abilityEffect in channeledAbilityEffects) {
                AbilityEffect _abilityEffect = SystemAbilityEffectManager.MyInstance.GetNewResource(abilityEffect.DisplayName);

                // channeled effects need to override the object lifetime so they get destroyed at the tickrate
                //_abilityEffect.MyAbilityEffectObjectLifetime = tickRate;
                if (_abilityEffect.ChanceToCast >= 100f || _abilityEffect.ChanceToCast >= UnityEngine.Random.Range(0f, 100f)) {
                    _abilityEffect.Cast(source, target, target, abilityEffectContext);
                }
            }
        }

        public bool CanCast(IAbilityCaster sourceCharacter, bool playerInitiated = false) {
            // cannot cast due to being stunned
            if (sourceCharacter.AbilityManager.ControlLocked) {
                return false;
            }
            if (useAbilityEffectTargetting) {
                List<AbilityEffect> abilityEffects = GetAbilityEffects(sourceCharacter);
                if (abilityEffects != null && abilityEffects.Count > 0 && abilityEffects[0].CanCast() == false) {
                    return false;
                }
            }
            if (weaponAffinityNames.Count == 0) {
                // no restrictions, automatically true
                return true;
            } else {
                return sourceCharacter.AbilityManager.PerformWeaponAffinityCheck(this, playerInitiated);
            }
        }

        public bool Use() {
            //Debug.Log(DisplayName + ".BaseAbility.Use()");
            // prevent casting any ability without the proper weapon affinity
            if (CanCast(PlayerManager.MyInstance.MyCharacter, true)) {
                //Debug.Log(DisplayName + ".BaseAbility.Use(): cancast is true");
                PlayerManager.MyInstance.MyCharacter.CharacterAbilityManager.BeginAbility(this, true);
                return true;
            }
            return false;
        }

        public virtual bool Cast(IAbilityCaster sourceCharacter, Interactable target, AbilityEffectContext abilityEffectContext) {
            //Debug.Log(resourceName + ".BaseAbility.Cast(" + sourceCharacter.AbilityManager.Name + ", " + (target == null ? "null" : target.name) + ")");
            if (!CanCast(sourceCharacter)) {
                //Debug.Log(resourceName + ".BaseAbility.Cast(" + sourceCharacter.AbilityManager.Name + ", " + (target == null ? "null" : target.name) + " CAN'T CAST!!!");
                return false;
            }

            if (coolDownOnCast == false) {
                BeginAbilityCoolDown(sourceCharacter);
            }

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
            if (GetHoldableObjectList(sourceCharacter) == null || GetHoldableObjectList(sourceCharacter).Count == 0) {
                return;
            }

            sourceCharacter.AbilityManager.DespawnAbilityObjects();
        }

        public virtual bool CanUseOn(Interactable target, IAbilityCaster sourceCharacter, bool performCooldownChecks = true, AbilityEffectContext abilityEffectContext = null, bool playerInitiated = false, bool performRangeCheck = true) {
            if (playerInitiated) {
                //Debug.Log(DisplayName + ".BaseAbility.CanUseOn(" + (target != null ? target.name : "null") + ", " + (sourceCharacter != null ? sourceCharacter.AbilityManager.Name : "null") + ")");
            }

            if (useAbilityEffectTargetting == true
                && GetAbilityEffects(sourceCharacter).Count > 0) {
                return GetAbilityEffects(sourceCharacter)[0].CanUseOn(target, sourceCharacter, abilityEffectContext, playerInitiated, performRangeCheck);
            }

            return TargetProps.CanUseOn(this, target, sourceCharacter, abilityEffectContext, playerInitiated, performRangeCheck);

        }

        //public virtual void PerformAbilityEffect(BaseAbility ability, GameObject source, GameObject target) {
        public virtual bool PerformAbilityEffects(IAbilityCaster source, Interactable target, AbilityEffectContext abilityEffectContext) {
            //Debug.Log(DisplayName + ".BaseAbility.PerformAbilityEffects(" + source.AbilityManager.Name + ", " + (target ? target.name : "null") + ")");
            if (GetAbilityEffects(source).Count == 0) {
                //Debug.Log(resourceName + ".BaseAbility.PerformAbilityEffects(" + source.name + ", " + (target ? target.name : "null") + "): THERE ARE NO EFFECTS ATTACHED TO THIS ABILITY!");
                // this is fine for channeled abilities
            }

            // perform hit / miss check only if baseability requires target and return false if miss
            if (GetTargetOptions(source).RequireTarget) {
                if (!source.AbilityManager.AbilityHit(target, abilityEffectContext)) {
                    //Debug.Log(DisplayName + ".BaseAbility.PerformAbilityEffects(): miss");
                    return false;
                }
            }

            // generate power resource
            source.AbilityManager.GeneratePower(this);

            foreach (AbilityEffect abilityEffect in GetAbilityEffects(source)) {
                if (abilityEffect == null) {
                    Debug.Log("Forgot to set ability affect in inspector?");
                }
                AbilityEffectContext abilityEffectOutput = abilityEffectContext.GetCopy();
                AbilityEffect _abilityEffect = SystemAbilityEffectManager.MyInstance.GetNewResource(abilityEffect.DisplayName);
                if (_abilityEffect != null
                    && _abilityEffect.CanUseOn(target, source, abilityEffectContext)
                    && (_abilityEffect.ChanceToCast >= 100f || _abilityEffect.ChanceToCast >= UnityEngine.Random.Range(0f, 100f))) {
                    _abilityEffect.Cast(source, target, target, abilityEffectOutput);
                } else {
                    //Debug.Log(DisplayName + ".BaseAbility.PerformAbilityEffects(" + source.AbilityManager.Name + ", " + (target ? target.name : "null") + ") COULD NOT FIND " + abilityEffect.DisplayName);
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
        public virtual Interactable ReturnTarget(IAbilityCaster sourceCharacter, Interactable target, bool performCooldownChecks = true, AbilityEffectContext abilityEffectContext = null, bool playerInitiated = false) {
            if (playerInitiated) {
                //Debug.Log(DisplayName + ".BaseAbility.ReturnTarget(" + (sourceCharacter == null ? "null" : sourceCharacter.AbilityManager.Name) + ", " + (target == null ? "null" : target.name) + ")");
            }
            // before we get here, a validity check has already been performed, so no need to unset any targets
            // we are only concerned with redirecting the target to self if auto-selfcast is enabled

            if (sourceCharacter == null || sourceCharacter.AbilityManager.UnitGameObject == null) {
                //Debug.Log("BaseAbility.ReturnTarget(): source is null! This should never happen!!!!!");
                return null;
            }

            // perform ability dependent checks
            if (CanUseOn(target, sourceCharacter, performCooldownChecks, abilityEffectContext, playerInitiated) == false) {
                //Debug.Log(DisplayName + ".BaseAbility.CanUseOn(" + (target != null ? target.name : "null") + " was false");
                if (GetTargetOptions(sourceCharacter).CanCastOnSelf && GetTargetOptions(sourceCharacter).AutoSelfCast) {
                    target = sourceCharacter.AbilityManager.UnitGameObject.GetComponent<Interactable>();
                    //Debug.Log(DisplayName + ".BaseAbility.ReturnTarget(): returning target as sourcecharacter: " + target.name);
                    return target;
                } else {
                    //Debug.Log(DisplayName + ".BaseAbility.ReturnTarget(): returning null");
                    return null;
                }
            }

            return target;
        }

        public virtual void StartCasting(IAbilityCaster source) {
            //Debug.Log("BaseAbility.OnCastStart(" + source.name + ")");
            List<AnimationClip> usedCastAnimationClips = GetCastClips(source);
            if (usedCastAnimationClips != null && usedCastAnimationClips.Count > 0) {
                int clipIndex = UnityEngine.Random.Range(0, usedCastAnimationClips.Count);
                if (usedCastAnimationClips[clipIndex] != null) {
                    // perform the actual animation
                    source.AbilityManager.PerformCastingAnimation(usedCastAnimationClips[clipIndex], this);
                }

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
    public enum AbilityPrefabSource { Both, Ability, Weapon }

}