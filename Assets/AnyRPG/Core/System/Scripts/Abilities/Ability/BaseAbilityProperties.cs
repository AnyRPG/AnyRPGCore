using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace AnyRPG {

    [Serializable]
    public abstract class BaseAbilityProperties : ConfiguredClass, IDescribable, IUseable, IMoveable, ITargetable, ILearnable {

        public event System.Action OnAbilityLearn = delegate { };
        public event System.Action OnAbilityUsed = delegate { };

        [Header("Casting Requirements")]

        [Tooltip("If true, this ability cannot be cast in combat.")]
        [SerializeField]
        protected bool requireOutOfCombat = false;

        [Tooltip("If this list is not empty, this ability will require the character to have the following weapons equipped to use it.")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(WeaponSkill))]
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
        protected bool animatorCreatePrefabs = false;

        [Tooltip("Delay to destroy casting effect prefabs after casting completes")]
        [SerializeField]
        protected float prefabDestroyDelay = 0f;

        [Header("Animation")]

        [Tooltip("The animation clip the character will perform")]
        [SerializeField]
        protected AnimationClip animationClip = null;

        [Tooltip("The name of an animation profile to get animations for the character to perform while casting this ability")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(AnimationProfile))]
        protected string animationProfileName = string.Empty;

        protected AnimationProfile animationProfile;

        [Tooltip("If true, the ability will use the casting animations from the caster.")]
        [SerializeField]
        protected bool useUnitCastAnimations = false;

        [Header("Audio")]

        [Tooltip("If the animation has hit events while it is playing (such as when a hammer strike occurs), this audio profile will be played in response to those events.")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(AudioProfile))]
        protected string animationHitAudioProfileName;

        protected AudioProfile animationHitAudioProfile;

        [Tooltip("An audio clip to play while the ability is casting")]
        [SerializeField]
        protected AudioClip castingAudioClip = null;

        [Tooltip("An audio profile to play while the ability is casting")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(AudioProfile))]
        protected string castingAudioProfileName = string.Empty;

        protected AudioProfile castingAudioProfile;

        [Header("Learning")]

        [Tooltip("The minimum level a character must be to cast this ability")]
        [SerializeField]
        protected int requiredLevel = 1;

        [Tooltip("If not empty, the character must be one of these classes to use this item.")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(CharacterClass))]
        private List<string> characterClassRequirements = new List<string>();

        private List<CharacterClass> characterClassRequirementList = new List<CharacterClass>();

        [Tooltip("If true, this ability does not have to be learned to cast. For abilities that anyone can use, like scrolls or crafting")]
        [SerializeField]
        protected bool useableWithoutLearning = false;

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
        [ResourceSelector(resourceType = typeof(PowerResource))]
        protected string powerResourceName = string.Empty;

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
        [ResourceSelector(resourceType = typeof(PowerResource))]
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
        [SerializeReference]
        [SerializeReferenceButton]
        protected List<AbilityEffectConfig> inlineAbilityEffects = new List<AbilityEffectConfig>();

        [Tooltip("When casting is complete, these ability effects will be triggered.")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(AbilityEffect))]
        protected List<string> abilityEffectNames = new List<string>();

        [Header("Channeling")]

        [Tooltip("During casting, this ability will perform its tick effects, every x seconds")]
        [SerializeField]
        private float tickRate = 1f;

        [Header("Chanelling Effects")]

        [Tooltip("During casting, these ability effects will be triggered on every tick.")]
        [SerializeReference]
        [SerializeReferenceButton]
        protected List<AbilityEffectConfig> inlineChannelingEffects = new List<AbilityEffectConfig>();

        [Tooltip("During casting, these ability effects will be triggered on every tick.")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(AbilityEffect))]
        protected List<string> channeledAbilityEffectnames = new List<string>();

        protected List<AbilityEffectProperties> channeledAbilityEffects = new List<AbilityEffectProperties>();

        private List<AbilityEffectProperties> abilityEffects = new List<AbilityEffectProperties>();

        protected IDescribable describableData = null;

        // game manager references
        protected PlayerManager playerManager = null;
        protected UIManager uIManager = null;
        protected SystemAbilityController systemAbilityController = null;

        public string DisplayName { get => describableData.DisplayName; }
        public Sprite Icon { get => describableData.Icon; }
        public string Description { get => describableData.Description; }


        public AnimationClip CastingAnimationClip {
            /*
            get {
                if (animationClip != null) {
                    return animationClip;
                }
                if (animationProfile?.AnimationProps?.CastClips != null
                    && animationProfile.AnimationProps.CastClips.Count > 0) {
                    return animationProfile.AnimationProps.CastClips[0];
                }
                return null;
            }
            */
            set {
                animationClip = value;
            }
        }
        public int RequiredLevel { get => requiredLevel; }
        public bool AutoAddToBars { get => autoAddToBars; }
        public bool UseableWithoutLearning { get => useableWithoutLearning; set => useableWithoutLearning = value; }

        /// <summary>
        /// return the casting time of the ability without any speed modifiers applied
        /// </summary>
        public virtual float GetBaseAbilityCastingTime(IAbilityCaster source) {
            if (useAnimationCastTime == false) {
                return abilityCastingTime;
            } else {
                if (GetCastClips(source).Count > 0) {
                    return GetCastClips(source)[0].length;
                }
                return abilityCastingTime;
            }
        }
        public bool CanSimultaneousCast { get => canSimultaneousCast; set => canSimultaneousCast = value; }
        public bool IgnoreGlobalCoolDown { get => ignoreGlobalCoolDown; set => ignoreGlobalCoolDown = value; }
        public AudioClip CastingAudioClip {
            get {
                if (castingAudioClip != null) {
                    return castingAudioClip;
                }
                if (castingAudioProfile != null) {
                    return castingAudioProfile.AudioClip;
                }
                return null;
            }
            set {
                castingAudioClip = value;
            }
        }
        public AudioClip AnimationHitAudioClip { get => (animationHitAudioProfile == null ? null : animationHitAudioProfile.AudioClip); }
        public bool AnimatorCreatePrefabs { get => animatorCreatePrefabs; set => animatorCreatePrefabs = value; }
        public List<AnimationClip> AttackClips { get => (animationProfile != null ? animationProfile.AnimationProps.AttackClips : null); }
        
        public List<AnimationClip> CastClips {
            get {
                if (animationClip != null) {
                    return new List<AnimationClip>() { animationClip };
                }
                if (animationProfile != null) {
                    return animationProfile.AnimationProps.CastClips;
                }
                return new List<AnimationClip>();
            }
        }
        
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
        public float CoolDown { get => abilityCoolDown; set => abilityCoolDown = value; }
        public AbilityPrefabSource AbilityPrefabSource { get => abilityPrefabSource; set => abilityPrefabSource = value; }
        public string AnimationProfileName { get => animationProfileName; set => animationProfileName = value; }
        public List<AbilityAttachmentNode> HoldableObjectList { set => holdableObjectList = value; }
        public float TickRate { get => tickRate; set => tickRate = value; }
        public AbilityTargetProps TargetOptions { set => targetOptions = value; }
        public List<AbilityEffectProperties> ChanneledAbilityEffects { get => channeledAbilityEffects; set => channeledAbilityEffects = value; }
        public string CastingAudioProfileName { get => castingAudioProfileName; set => castingAudioProfileName = value; }
        public bool UseAnimationCastTime { get => useAnimationCastTime; set => useAnimationCastTime = value; }

        /*
        public void GetBaseAbilityProperties(BaseAbility effect) {

            requireOutOfCombat = effect.RequireOutOfCombat;
            weaponAffinityNames = effect.WeaponAffinityNames;
            holdableObjectList = effect.HoldableObjectList;
            abilityPrefabSource = effect.AbilityPrefabSource;
            animatorCreatePrefabs = effect.AnimatorCreatePrefabs;
            prefabDestroyDelay = effect.PrefabDestroyDelay;
            animationProfileName = effect.AnimationProfileName;
            useUnitCastAnimations = effect.UseUnitCastAnimations;
            animationHitAudioProfileName = effect.AnimationHitAudioProfileName;
            castingAudioProfileName = effect.CastingAudioProfileName;
            requiredLevel = effect.RequiredLevel;
            characterClassRequirements = effect.CharacterClassRequirements;
            useableWithoutLearning = effect.UseableWithoutLearning;
            autoAddToBars = effect.AutoAddToBars;
            canSimultaneousCast = effect.CanSimultaneousCast;
            canCastWhileMoving = effect.CanCastWhileMoving;
            ignoreGlobalCoolDown = effect.IgnoreGlobalCoolDown;
            powerResourceName = effect.PowerResourceName;
            baseResourceCost = effect.BaseResourceCost;
            resourceCostPerLevel = effect.ResourceCostPerLevel;
            spendDelay = effect.SpendDelay;
            generatePowerResourceName = effect.GeneratePowerResourceName;
            baseResourceGain = effect.BaseResourceGain;
            resourceGainPerLevel = effect.ResourceGainPerLevel;
            useSpeedMultipliers = effect.UseSpeedMultipliers;
            useAnimationCastTime = effect.UseAnimationCastTime;
            abilityCastingTime = effect.AbilityCastingTime;
            abilityCoolDown = effect.AbilityCoolDown;
            coolDownOnCast = effect.CoolDownOnCast;
            useAbilityEffectTargetting = effect.UseAbilityEffectTargetting;
            targetOptions = effect.TargetOptions;
            inlineAbilityEffects = effect.InlineAbilityEffects;
            abilityEffectNames = effect.AbilityEffectNames;
            tickRate = effect.TickRate;
            channeledAbilityEffectnames = effect.ChanneledAbilityEffectnames;
        }
        */

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            playerManager = systemGameManager.PlayerManager;
            uIManager = systemGameManager.UIManager;
            systemAbilityController = systemGameManager.SystemAbilityController;
        }

        public virtual bool IsUseableStale() {
            if (playerManager.MyCharacter.CharacterAbilityManager.HasAbility(DisplayName)) {
                return false;
            }
            return true;
        }

        public void AssignToActionButton(ActionButton actionButton) {
            actionButton.BackgroundImage.color = new Color32(0, 0, 0, 255);
        }

        public void AssignToHandScript(Image backgroundImage) {
            backgroundImage.color = new Color32(0, 0, 0, 255);
        }

        public bool ActionButtonUse() {
            return Use();
        }

        public IUseable GetFactoryUseable() {
            return systemDataFactory.GetResource<BaseAbility>(DisplayName).AbilityProperties;
        }

        public virtual void UpdateChargeCount(ActionButton actionButton) {
            uIManager.UpdateStackSize(actionButton, 0, false);
        }

        public virtual void ProcessUnLearnAbility(CharacterAbilityManager abilityManager) {
            // do nothing here
        }

        public virtual void PrepareToLearnAbility(CharacterAbilityManager abilityManager) {
            // do nothing here
        }

        public virtual void ProcessLearnAbility(CharacterAbilityManager abilityManager) {
            // do nothing here
        }

        public virtual bool CanLearnAbility(CharacterAbilityManager characterAbilityManager) {
            return true;
        }

        public virtual void ProcessLoadAbility(CharacterAbilityManager abilityManager) {
            // do nothing here
        }

        public virtual bool HadSpecialIcon(ActionButton actionButton) {
            return false;
        }

        /// <summary>
        /// determine if an ability is on an animation length cooldown
        /// </summary>
        /// <param name="characterCombat"></param>
        /// <returns></returns>
        public virtual bool ReadyToCast(CharacterCombat characterCombat) {
            return true;
        }

        public void UpdateTargetRange(ActionBarManager actionBarManager, ActionButton actionButton) {
            actionBarManager.UpdateAbilityTargetRange(this, actionButton);
        }

        public virtual void UpdateActionButtonVisual(ActionButton actionButton) {
            //Debug.Log(DisplayName + ".BaseAbility.UpdateActionButtonVisual()");
            // set cooldown icon on abilities that don't have enough resources to cast
            if (PowerResource != null
                && (GetResourceCost(playerManager.ActiveCharacter) >= playerManager.ActiveCharacter.CharacterStats.GetPowerResourceAmount(PowerResource))) {
                //Debug.Log(DisplayName + ".BaseAbility.UpdateActionButtonVisual(): not enough resources to cast this ability.  enabling full cooldown");
                actionButton.EnableFullCoolDownIcon();
                return;
            }

            if (HadSpecialIcon(actionButton)) {
                return;
            }

            if (RequireOutOfCombat) {
                if (playerManager.MyCharacter.CharacterCombat.GetInCombat() == true) {
                    //Debug.Log("ActionButton.UpdateVisual(): can't cast due to being in combat");
                    actionButton.EnableFullCoolDownIcon();
                    return;
                }
            }

            if (!CanCast(playerManager.MyCharacter)) {
                //Debug.Log(DisplayName + ".BaseAbility.UpdateActionButtonVisual(): can't cast due to spell restrictions");
                actionButton.EnableFullCoolDownIcon();
                return;
            }


            if (playerManager.MyCharacter.CharacterAbilityManager.RemainingGlobalCoolDown > 0f
                || playerManager.MyCharacter.CharacterAbilityManager.MyAbilityCoolDownDictionary.ContainsKey(DisplayName)) {
                //Debug.Log(DisplayName + ".BaseAbility.UpdateActionButtonVisual(): Ability is on cooldown");
                if (actionButton.CoolDownIcon.isActiveAndEnabled != true) {
                    //Debug.Log("ActionButton.UpdateVisual(): coolDownIcon is not enabled: " + (useable == null ? "null" : useable.DisplayName));
                    actionButton.CoolDownIcon.enabled = true;
                }
                if (actionButton.CoolDownIcon.sprite != actionButton.Icon.sprite) {
                    actionButton.CoolDownIcon.sprite = actionButton.Icon.sprite;
                    actionButton.CoolDownIcon.color = new Color32(0, 0, 0, 230);
                    actionButton.CoolDownIcon.fillMethod = Image.FillMethod.Radial360;
                    actionButton.CoolDownIcon.fillClockwise = false;
                }
                float remainingAbilityCoolDown = 0f;
                float initialCoolDown = 0f;
                if (playerManager.MyCharacter.CharacterAbilityManager.MyAbilityCoolDownDictionary.ContainsKey(DisplayName)) {
                    remainingAbilityCoolDown = playerManager.MyCharacter.CharacterAbilityManager.MyAbilityCoolDownDictionary[DisplayName].RemainingCoolDown;
                    initialCoolDown = playerManager.MyCharacter.CharacterAbilityManager.MyAbilityCoolDownDictionary[DisplayName].InitialCoolDown;
                } else {
                    initialCoolDown = abilityCoolDown;
                }
                //float globalCoolDown
                float fillAmount = Mathf.Max(remainingAbilityCoolDown, playerManager.MyCharacter.CharacterAbilityManager.RemainingGlobalCoolDown) /
                    (remainingAbilityCoolDown > playerManager.MyCharacter.CharacterAbilityManager.RemainingGlobalCoolDown ? initialCoolDown : playerManager.MyCharacter.CharacterAbilityManager.InitialGlobalCoolDown);
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
                //Debug.Log("ActionButton.OnUseableUse(" + ability.DisplayName + "): WAS NOT ANIMATED AUTO ATTACK");
                //if (abilityCoRoutine == null) {
                //if (monitorCoroutine == null) {
                    return systemAbilityController.StartCoroutine(actionButton.MonitorAbility(DisplayName));
                //}
            //return null;
        }

        public TargetProps GetTargetOptions(IAbilityCaster abilityCaster) {
            if (useAbilityEffectTargetting == true && GetAbilityEffects(abilityCaster).Count > 0) {
                return GetAbilityEffects(abilityCaster)[0].GetTargetOptions(abilityCaster);
            }
            return targetOptions;
        }

        public virtual List<AbilityEffectProperties> GetAbilityEffects(IAbilityCaster abilityCaster) {
            return abilityEffects;
        }

        public virtual List<AbilityAttachmentNode> GetHoldableObjectList(IAbilityCaster abilityCaster) {
            return holdableObjectList;
        }

        public virtual float GetAbilityCastingTime(IAbilityCaster abilityCaster) {
            if (useSpeedMultipliers) {
                return GetBaseAbilityCastingTime(abilityCaster) * (1f / abilityCaster.AbilityManager.GetSpeed());
            }
            return GetBaseAbilityCastingTime(abilityCaster);
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
                return animationProfile?.AnimationProps;
            }
        }

        public virtual string GetName() {
            return string.Format("<color=yellow>{0}</color>", DisplayName);
        }

        public virtual string GetDescription() {
            return string.Format("{0}\n{1}", GetName(), GetSummary());
        }

        public virtual string GetSummary() {
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
                    if (playerManager.MyCharacter.CharacterEquipmentManager.HasAffinity(_weaponAffinity)) {
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

            string abilityRange = (GetTargetOptions(playerManager.MyCharacter).UseMeleeRange == true ? "melee" : GetTargetOptions(playerManager.MyCharacter).MaxRange + " meters");

            string costString = string.Empty;
            if (powerResource != null) {
                costString = "\nCost: " + GetResourceCost(playerManager.MyCharacter) + " " + powerResource.DisplayName;
            }

            string coolDownString = GetCooldownString();

            return string.Format("Cast time: {0} second(s)\nCooldown: {1} second(s){2}\nRange: {3}\n<color=#ffff00ff>{4}</color>{5}{6}",
                GetAbilityCastingTime(playerManager.MyCharacter).ToString("F1"),
                abilityCoolDown,
                costString,
                abilityRange,
                Description,
                addString,
                coolDownString);
        }

        public string GetCooldownString() {
            string coolDownString = string.Empty;
            if (playerManager?.MyCharacter?.CharacterAbilityManager != null
                && (playerManager.MyCharacter.CharacterAbilityManager.RemainingGlobalCoolDown > 0f
                || playerManager.MyCharacter.CharacterAbilityManager.MyAbilityCoolDownDictionary.ContainsKey(DisplayName))) {
                float dictionaryCooldown = 0f;
                if (playerManager.MyCharacter.CharacterAbilityManager.MyAbilityCoolDownDictionary.ContainsKey(DisplayName)) {
                    dictionaryCooldown = playerManager.MyCharacter.CharacterAbilityManager.MyAbilityCoolDownDictionary[DisplayName].RemainingCoolDown;
                }
                coolDownString = "\n\nCooldown Remaining: " + SystemAbilityController.GetTimeText(Mathf.Max(dictionaryCooldown, playerManager.MyCharacter.CharacterAbilityManager.RemainingGlobalCoolDown)); ;
            }
            return coolDownString;
        }

        public string GetShortDescription() {
            return Description;
        }

        public virtual float GetResourceCost(IAbilityCaster abilityCaster) {
            if (abilityCaster != null && powerResource != null) {
                return baseResourceCost + (abilityCaster.AbilityManager.Level * resourceCostPerLevel);
            }
            return baseResourceCost;
        }

        public virtual float GetResourceGain(IAbilityCaster abilityCaster) {
            //Debug.Log(DisplayName + ".BaseAbility.GetResourceGain(" + (abilityCaster == null ? "null" : abilityCaster.Name) + ")");
            if (abilityCaster != null) {
                //Debug.Log(DisplayName + ".BaseAbility.GetResourceGain() level: " + abilityCaster.Level + "; gainperLevel: " + resourceGainPerLevel + "; base: " + baseResourceGain);

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
            //Debug.Log("BaseAbility.PerformChanneledEffect(" + DisplayName + ", " + (source == null ? "null" : source.AbilityManager.Name) + ", " + (target == null ? "null" : target.name) + ")");
            foreach (AbilityEffectProperties abilityEffect in channeledAbilityEffects) {

                // channeled effects need to override the object lifetime so they get destroyed at the tickrate
                if (abilityEffect.ChanceToCast >= 100f || abilityEffect.ChanceToCast >= UnityEngine.Random.Range(0f, 100f)) {
                    abilityEffect.Cast(source, target, target, abilityEffectContext);
                }
            }
        }

        public bool CanCast(IAbilityCaster sourceCharacter, bool playerInitiated = false) {
            // cannot cast due to being stunned
            if (sourceCharacter.AbilityManager.ControlLocked) {
                return false;
            }
            if (useAbilityEffectTargetting) {
                List<AbilityEffectProperties> abilityEffects = GetAbilityEffects(sourceCharacter);
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
            if (CanCast(playerManager.MyCharacter, true)) {
                //Debug.Log(DisplayName + ".BaseAbility.Use(): cancast is true");
                playerManager.MyCharacter.CharacterAbilityManager.BeginAbility(this, true);
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
            //Debug.Log(DisplayName + ".BaseAbility.ProcessGCDManual()");
            ProcessGCDManual(sourceCharacter);
        }

        public virtual void ProcessGCDManual(IAbilityCaster sourceCharacter, float usedCoolDown = 0f) {
            //Debug.Log(DisplayName + ".BaseAbility.ProcessGCDManual(" + usedCoolDown + ")");
            if (CanSimultaneousCast == false && IgnoreGlobalCoolDown == false && GetAbilityCastingTime(sourceCharacter) == 0f) {
                sourceCharacter.AbilityManager.InitiateGlobalCooldown(usedCoolDown);
            }
        }

        public virtual void ProcessAbilityPrefabs(IAbilityCaster sourceCharacter) {
            //Debug.Log(DisplayName + ".BaseAbility.ProcessAbilityPrefabs()");
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

            return targetOptions.CanUseOn(this, target, sourceCharacter, abilityEffectContext, playerInitiated, performRangeCheck);

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

            foreach (AbilityEffectProperties abilityEffect in GetAbilityEffects(source)) {
                if (abilityEffect == null) {
                    Debug.Log("Forgot to set ability affect in inspector?");
                }
                AbilityEffectContext abilityEffectOutput = abilityEffectContext.GetCopy();
                if (abilityEffect != null
                    && abilityEffect.CanUseOn(target, source, abilityEffectContext)
                    && (abilityEffect.ChanceToCast >= 100f || abilityEffect.ChanceToCast >= UnityEngine.Random.Range(0f, 100f))) {
                    abilityEffect.Cast(source, target, target, abilityEffectOutput);
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
            //source.RigidBody.constraints = RigidbodyConstraints.FreezeAll;
            
        }

        public virtual float OnCastTimeChanged(float currentCastPercent, float nextTickPercent, IAbilityCaster source, Interactable target, AbilityEffectContext abilityEffectContext) {
            //Debug.Log(DisplayName + ".BaseAbility.OnCastTimeChanged(" + currentCastPercent + ", " + nextTickPercent + ")");
            // overwrite me
            if (currentCastPercent >= nextTickPercent) {
                PerformChanneledEffect(source, target, abilityEffectContext);
                nextTickPercent += (tickRate / GetBaseAbilityCastingTime(source));
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
                if (!CharacterClassRequirementList.Contains(playerManager.MyCharacter.CharacterClass)) {
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


        public void SetupScriptableObjects(SystemGameManager systemGameManager, IDescribable describable) {
            //base.SetupScriptableObjects(systemGameManager);
            describableData = describable;

            Configure(systemGameManager);

            // add inline effects
            foreach (AbilityEffectConfig abilityEffectConfig in inlineAbilityEffects) {
                if (abilityEffectConfig != null) {
                    abilityEffectConfig.SetupScriptableObjects(systemGameManager, this);
                    abilityEffects.Add(abilityEffectConfig.AbilityEffectProperties);
                } else {
                    Debug.LogWarning("Null inline AbilityEffect detected while initializing BaseAbility Properties for " + describable.DisplayName);
                }
            }

            // add named effects
            if (AbilityEffectNames != null) {
                foreach (string abilityEffectName in AbilityEffectNames) {
                    AbilityEffect abilityEffect = systemDataFactory.GetResource<AbilityEffect>(abilityEffectName);
                    if (abilityEffect != null) {
                        abilityEffects.Add(abilityEffect.AbilityEffectProperties);
                    } else {
                        Debug.LogError("BaseAbility.SetupScriptableObjects(): Could not find ability effect: " + abilityEffectName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                    }
                }
            }
            if (holdableObjectList != null) {
                foreach (AbilityAttachmentNode holdableObjectAttachment in holdableObjectList) {
                    if (holdableObjectAttachment != null) {
                        holdableObjectAttachment.SetupScriptableObjects(DisplayName, systemGameManager);
                    }
                }
            }
            weaponAffinityList = new List<WeaponSkill>();
            if (weaponAffinityNames != null) {
                foreach (string weaponAffinityName in weaponAffinityNames) {
                    WeaponSkill tmpWeaponSkill = systemDataFactory.GetResource<WeaponSkill>(weaponAffinityName);
                    if (tmpWeaponSkill != null) {
                        weaponAffinityList.Add(tmpWeaponSkill);
                    } else {
                        Debug.LogError("BaseAbility.SetupScriptableObjects(): Could not find weapon skill: " + weaponAffinityName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                    }
                }
            }

            castingAudioProfile = null;
            if (castingAudioProfileName != null && castingAudioProfileName != string.Empty) {
                AudioProfile audioProfile = systemDataFactory.GetResource<AudioProfile>(castingAudioProfileName);
                if (audioProfile != null) {
                    castingAudioProfile = audioProfile;
                } else {
                    Debug.LogError("BaseAbility.SetupScriptableObjects(): Could not find audio profile: " + castingAudioProfileName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                }
            }

            animationHitAudioProfile = null;
            if (animationHitAudioProfileName != null && animationHitAudioProfileName != string.Empty) {
                AudioProfile audioProfile = systemDataFactory.GetResource<AudioProfile>(animationHitAudioProfileName);
                if (audioProfile != null) {
                    animationHitAudioProfile = audioProfile;
                } else {
                    Debug.LogError("BaseAbility.SetupScriptableObjects(): Could not find audio profile: " + animationHitAudioProfileName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                }
            }

            animationProfile = null;
            if (animationProfileName != null && animationProfileName != string.Empty) {
                AnimationProfile tmpAnimationProfile = systemDataFactory.GetResource<AnimationProfile>(animationProfileName);
                if (tmpAnimationProfile != null) {
                    animationProfile = tmpAnimationProfile;
                } else {
                    Debug.LogError("BaseAbility.SetupScriptableObjects(): Could not find animation profile: " + animationProfileName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                }
            }

            powerResource = null;
            if (powerResourceName != null && powerResourceName != string.Empty) {
                PowerResource tmpPowerResource = systemDataFactory.GetResource<PowerResource>(powerResourceName);
                if (tmpPowerResource != null) {
                    powerResource = tmpPowerResource;
                } else {
                    Debug.LogError("BaseAbility.SetupScriptableObjects(): Could not find power resource: " + powerResourceName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                }
            }

            generatePowerResource = null;
            if (generatePowerResourceName != null && generatePowerResourceName != string.Empty) {
                PowerResource tmpPowerResource = systemDataFactory.GetResource<PowerResource>(generatePowerResourceName);
                if (tmpPowerResource != null) {
                    generatePowerResource = tmpPowerResource;
                } else {
                    Debug.LogError("BaseAbility.SetupScriptableObjects(): Could not find power resource: " + powerResourceName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                }
            }


            //channeledAbilityEffects = new List<AbilityEffectProperties>();
            
            // add inline effects
            foreach (AbilityEffectConfig abilityEffectConfig in inlineChannelingEffects) {
                abilityEffectConfig.SetupScriptableObjects(systemGameManager, this);
                channeledAbilityEffects.Add(abilityEffectConfig.AbilityEffectProperties);
            }

            // add named effects
            if (channeledAbilityEffectnames != null) {
                foreach (string abilityEffectName in channeledAbilityEffectnames) {
                    AbilityEffect abilityEffect = systemDataFactory.GetResource<AbilityEffect>(abilityEffectName);
                    if (abilityEffect != null) {
                        channeledAbilityEffects.Add(abilityEffect.AbilityEffectProperties);
                    } else {
                        Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find ability effect: " + abilityEffectName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                    }
                }
            }

            characterClassRequirementList = new List<CharacterClass>();
            if (characterClassRequirementList != null) {
                foreach (string characterClassName in characterClassRequirements) {
                    CharacterClass tmpCharacterClass = systemDataFactory.GetResource<CharacterClass>(characterClassName);
                    if (tmpCharacterClass != null) {
                        characterClassRequirementList.Add(tmpCharacterClass);
                    } else {
                        Debug.LogError("SystemAbilityManager.SetupScriptableObjects(): Could not find character class : " + characterClassName + " while inititalizing " + DisplayName + ".  CHECK INSPECTOR");
                    }
                }
            }


        }

    }

    //public enum PrefabSpawnLocation { None, Caster, Target, GroundTarget, OriginalTarget, TargetPoint, CasterPoint }
    //public enum AbilityPrefabSource { Both, Ability, Weapon }

}