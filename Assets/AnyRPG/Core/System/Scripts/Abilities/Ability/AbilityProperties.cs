using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace AnyRPG {

    [Serializable]
    public class AbilityProperties : ConfiguredClass, IRewardable, IDescribable, IUseable, IMoveable, ITargetable, ILearnable {

        public event System.Action<UnitController> OnAbilityLearn = delegate { };

        [Header("Casting Requirements")]

        [Tooltip("If true, this ability cannot be cast in combat.")]
        [SerializeField]
        protected bool requireOutOfCombat = false;

        [Tooltip("If true, the caster must be stealthed to perform this ability.")]
        [SerializeField]
        protected bool requireStealth = false;

        [Tooltip("If this list is not empty, this ability will require the character to have the following weapons equipped to use it.")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(WeaponSkill))]
        private List<string> weaponAffinityNames = new List<string>();

        private List<WeaponSkill> weaponAffinityList = new List<WeaponSkill>();


        [Header("Prefabs")]

        [Tooltip("Physical prefabs to attach to bones on the character unit")]
        [SerializeField]
        private List<AbilityAttachmentNode> holdableObjectList = new List<AbilityAttachmentNode>();

        [Tooltip("If true, the prefabs will be despawned when the cast phase ends, instead of during or at the end of the action phase")]
        [SerializeField]
        private bool despawnPrefabsOnCastEnd = false;


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

        //[Tooltip("The animation clip the character will perform while casting")]
        //[SerializeField]
        //protected AnimationClip animationClip = null;

        [Tooltip("The name of an animation profile to get animations for the character to perform while casting this ability")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(AnimationProfile))]
        protected string animationProfileName = string.Empty;

        protected AnimationProfile animationProfile = null;

        [Tooltip("If true, the ability will use the casting animations from the caster.")]
        [SerializeField]
        protected bool useUnitCastAnimations = false;

        
        [Header("Animated Ability")]

        [Tooltip("Is this an auto attack ability")]
        [SerializeField]
        private bool isAutoAttack = false;

        [Tooltip("If true, a random animation from the unit attack animations will be used")]
        [SerializeField]
        private bool useUnitAttackAnimations = false;

        [Tooltip("This option is only valid if this is not an auto attack ability.  If true, it will use the current auto-attack animations so it looks good with any weapon.")]
        [SerializeField]
        private bool useAutoAttackAnimations = false;

        [Tooltip("If true, the current weapon default hit sound will be played when this ability hits an enemy.")]
        [SerializeField]
        private bool useWeaponHitSound = false;

        [Tooltip("If true, the choice of whether or not to play the attack voice is controlled by the weapon skill.")]
        [SerializeField]
        private bool useWeaponSkillAttackVoiceSetting = false;

        [Tooltip("If true, the character will play their attack voice clip when this ability is used.")]
        [SerializeField]
        private bool playAttackVoice = false;


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

        [Tooltip("If true, the audio will be looped until the cast is complete.")]
        [SerializeField]
        protected bool loopAudio = false;


        [Header("Learning")]

        [Tooltip("The minimum level a character must be to cast this ability")]
        [SerializeField]
        protected int requiredLevel = 1;

        [Tooltip("If not empty, the character must be one of these classes to cast this ability.")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(CharacterClass))]
        private List<string> characterClassRequirements = new List<string>();

        private List<CharacterClass> characterClassRequirementList = new List<CharacterClass>();

        [Tooltip("If not empty, the character must be one of these class specializations to cast this ability.")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(ClassSpecialization))]
        private List<string> classSpecializationRequirements = new List<string>();

        private List<ClassSpecialization> classSpecializationRequirementList = new List<ClassSpecialization>();

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

        [Tooltip("If Ability Effect Targetting is used, the first ability effect from this stage will be used.")]
        [SerializeField]
        private AbilityStage abilityEffectTargetStage = AbilityStage.CastEnd;

        [SerializeField]
        private AbilityTargetProps targetOptions = new AbilityTargetProps();

        [Header("Channeling")]

        [Tooltip("During casting, this ability will perform its tick effects, every x seconds")]
        [SerializeField]
        private float tickRate = 1f;


        [Header("Chanelling Effects")]

        /*
        [Tooltip("During casting, these ability effects will be triggered on every tick.")]
        [SerializeReference]
        [SerializeReferenceButton]
        public List<AbilityEffectConfig> inlineChannelingEffects = new List<AbilityEffectConfig>();
        */

        [Tooltip("During casting, these ability effects will be triggered on every tick.")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(AbilityEffect))]
        public List<string> channeledAbilityEffectnames = new List<string>();

        protected List<AbilityEffectProperties> channeledAbilityEffects = new List<AbilityEffectProperties>();


        [Header("Cast Complete Ability Effects")]

        /*
        [Tooltip("When casting is complete, these ability effects will be triggered.")]
        [SerializeReference]
        [SerializeReferenceButton]
        public List<AbilityEffectConfig> inlineAbilityEffects = new List<AbilityEffectConfig>();
        */

        [Tooltip("When casting is complete, these ability effects will be triggered.")]
        [FormerlySerializedAs("abilityEffectNames")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(AbilityEffect))]
        public List<string> castEndAbilityEffectNames = new List<string>();

        private List<AbilityEffectProperties> castEndAbilityEffects = new List<AbilityEffectProperties>();


        [Header("Action Hit Ability Effects")]

        [Tooltip("In response to Hit events during the animation, these ability effects will be triggered.")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(AbilityEffect))]
        public List<string> actionHitAbilityEffectNames = new List<string>();

        private List<AbilityEffectProperties> actionHitAbilityEffects = new List<AbilityEffectProperties>();


        [Header("Action End Ability Effects")]

        [Tooltip("When the action is complete, these ability effects will be triggered.")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(AbilityEffect))]
        protected List<string> actionEndAbilityEffectNames = new List<string>();

        private List<AbilityEffectProperties> actionEndAbilityEffects = new List<AbilityEffectProperties>();


        protected IDescribable describableData = null;
        protected List<AnimationClip> actionClips = new List<AnimationClip>();
        protected List<AnimationClip> castClips = new List<AnimationClip>();

        // game manager references
        protected PlayerManager playerManager = null;
        protected UIManager uIManager = null;
        protected SystemAbilityController systemAbilityController = null;

        public string ResourceName { get => describableData.ResourceName; }
        public string DisplayName { get => describableData.DisplayName; }
        public Sprite Icon { get => describableData.Icon; }
        public string Description { get => describableData.Description; }
        public bool IsAutoAttack { get => isAutoAttack; set => isAutoAttack = value; }
        public bool UseWeaponHitSound { get => useWeaponHitSound; set => useWeaponHitSound = value; }
        public bool UseWeaponSkillAttackVoiceSetting { get => useWeaponSkillAttackVoiceSetting; set => useWeaponSkillAttackVoiceSetting = value; }


        /*
        public AnimationClip CastingAnimationClip {
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
            set {
                animationClip = value;
            }
        }
*/
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
                if (GetAbilityCastClips(source).Count > 0) {
                    return GetAbilityCastClips(source)[0].length;
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
                    return castingAudioProfile.RandomAudioClip;
                }
                return null;
            }
            set {
                castingAudioClip = value;
            }
        }
        public AudioClip AnimationHitAudioClip { get => animationHitAudioProfile?.RandomAudioClip; }
        public bool AnimatorCreatePrefabs { get => animatorCreatePrefabs; set => animatorCreatePrefabs = value; }
        public List<AnimationClip> ActionClips { get => actionClips; }
        public List<AnimationClip> CastClips { get => castClips; }
        
        public List<string> WeaponAffinityNames { get => weaponAffinityNames; set => weaponAffinityNames = value; }
        public bool RequireOutOfCombat { get => requireOutOfCombat; set => requireOutOfCombat = value; }
        public List<string> AbilityEffectNames { get => castEndAbilityEffectNames; set => castEndAbilityEffectNames = value; }
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
        public List<ClassSpecialization> ClassSpecializationRequirementList { get => classSpecializationRequirementList; set => classSpecializationRequirementList = value; }
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
        public bool RequireStealth { get => requireStealth; set => requireStealth = value; }
        public bool LoopAudio { get => loopAudio; set => loopAudio = value; }
        public bool AlwaysDisplayCount { get => false; }

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

        public virtual float GetTimeMultiplier(IAbilityCaster sourceCharacter, AbilityEffectContext abilityEffectContext) {
            
            // casting phase multiplier
            return Mathf.Clamp(abilityEffectContext.castTimeMultiplier, 1f, Mathf.Infinity);

            // FIX ME - figure out which one to return
            // action phase multiplier
            //return Mathf.Clamp(sourceCharacter.AbilityManager.GetAnimationLengthMultiplier(), 1f, Mathf.Infinity);

        }

        public void GiveReward(UnitController sourceUnitController) {
            sourceUnitController.CharacterAbilityManager.LearnAbility(this);
        }

        public bool HasReward(UnitController sourceUnitController) {
            return sourceUnitController.CharacterAbilityManager.HasAbility(this);
        }

        public virtual bool IsUseableStale(UnitController sourceUnitController) {
            if (sourceUnitController.CharacterAbilityManager.HasAbility(ResourceName)) {
                //Debug.Log(DisplayName + " is not stale");
                return false;
            }
            //Debug.Log(DisplayName + " is stale");
            return true;
        }

        public void AssignToActionButton(ActionButton actionButton) {
            actionButton.BackgroundImage.color = new Color32(0, 0, 0, 255);
            if (isAutoAttack && systemConfigurationManager.AllowAutoAttack == true) {
                actionButton.SubscribeToAutoAttackEvents();
            }
        }

        public void HandleRemoveFromActionButton(ActionButton actionButton) {
            //Debug.Log(DisplayName + ".BaseAbility.HandleRemoveFromActionButton()");
            if (isAutoAttack && systemConfigurationManager.AllowAutoAttack == true) {
                actionButton.UnsubscribeFromAutoAttackEvents();
            }
        }

        public void AssignToHandScript(Image backgroundImage) {
            backgroundImage.color = new Color32(0, 0, 0, 255);
        }

        public bool ActionButtonUse(UnitController sourceUnitController) {
            return Use(sourceUnitController);
        }

        public IUseable GetFactoryUseable() {
            return systemDataFactory.GetResource<Ability>(DisplayName).AbilityProperties;
        }

        public virtual int GetChargeCount() {
            return 0;
        }

        public virtual void ProcessUnLearnAbility(CharacterAbilityManager abilityManager) {
            if (isAutoAttack) {
                abilityManager.UnsetAutoAttackAbility();
            }
        }

        public virtual void PrepareToLearnAbility(CharacterAbilityManager abilityManager) {
            if (IsAutoAttack == true) {
                abilityManager.UnLearnDefaultAutoAttackAbility();
            }
        }

        public virtual void ProcessLearnAbility(CharacterAbilityManager abilityManager) {
            if (isAutoAttack) {
                abilityManager.SetAutoAttackAbility(this);
            }
        }

        public virtual bool CanLearnAbility(CharacterAbilityManager characterAbilityManager) {
            if (isAutoAttack == true && characterAbilityManager.AutoAttackAbility != null) {
                return false;
            }
            return true;
        }

        public virtual void ProcessLoadAbility(CharacterAbilityManager abilityManager) {
            if (isAutoAttack == true) {
                abilityManager.LearnAutoAttack(this);
            }
        }

        public virtual bool HadSpecialIcon(ActionButton actionButton) {
            if (systemConfigurationManager.AllowAutoAttack == true && IsAutoAttack == true) {

                if (playerManager.UnitController.CharacterCombat.GetInCombat() == true
                    && playerManager.UnitController.CharacterCombat.AutoAttackActive == true) {
                    if (actionButton.CoolDownIcon.isActiveAndEnabled == false) {
                        actionButton.CoolDownIcon.enabled = true;
                    }
                    if (actionButton.CoolDownIcon.color == new Color32(255, 0, 0, 155)) {
                        actionButton.CoolDownIcon.color = new Color32(255, 146, 146, 155);
                    } else {
                        actionButton.CoolDownIcon.color = new Color32(255, 0, 0, 155);
                    }

                    if (actionButton.CoolDownIcon.fillMethod != Image.FillMethod.Radial360) {
                        actionButton.CoolDownIcon.fillMethod = Image.FillMethod.Radial360;
                    }
                    if (actionButton.CoolDownIcon.fillAmount != 1f) {
                        actionButton.CoolDownIcon.fillAmount = 1f;
                    }
                } else {
                    //Debug.Log("ActionButton.UpdateVisual(): Player is not in combat");
                    actionButton.DisableCoolDownIcon();
                }
                // don't need to continue on and do radial fill on auto-attack icons
                return true;
            }

            return false;
        }

        /// <summary>
        /// determine if an ability is on an animation length cooldown
        /// </summary>
        /// <param name="characterCombat"></param>
        /// <returns></returns>
        public virtual bool ReadyToCast(CharacterCombat characterCombat) {
            if (isAutoAttack == true && characterCombat.OnAutoAttackCooldown() == true) {
                return false;
            }

            return true;
        }

        public void UpdateTargetRange(ActionBarManager actionBarManager, ActionButton actionButton) {
            actionBarManager.UpdateAbilityTargetRange(this, actionButton);
        }

        public virtual void UpdateActionButtonVisual(ActionButton actionButton) {
            //Debug.Log($"{ResourceName}.AbilityProperties.UpdateActionButtonVisual()");

            // this must happen first because it's an image update that doesn't rely on cooldowns
            // auto-attack buttons are special and display the current weapon of the character
            if (IsAutoAttack == true) {
                //Debug.Log("ActionButton.UpdateVisual(): updating auto-attack ability");
                foreach (EquipmentSlotProfile equipmentSlotProfile in playerManager.UnitController.CharacterEquipmentManager.CurrentEquipment.Keys) {
                    //Debug.Log("ActionButton.UpdateVisual(): updating auto-attack ability");
                    if (equipmentSlotProfile.MainWeaponSlot == true
                        && playerManager.UnitController.CharacterEquipmentManager.CurrentEquipment[equipmentSlotProfile].InstantiatedEquipment != null
                        && playerManager.UnitController.CharacterEquipmentManager.CurrentEquipment[equipmentSlotProfile].InstantiatedEquipment.Equipment is Weapon) {
                        if (actionButton.Icon.sprite != playerManager.UnitController.CharacterEquipmentManager.CurrentEquipment[equipmentSlotProfile].InstantiatedEquipment.Icon) {
                            actionButton.Icon.sprite = playerManager.UnitController.CharacterEquipmentManager.CurrentEquipment[equipmentSlotProfile].InstantiatedEquipment.Icon;
                            break;
                        }
                    }
                }
            }

            // set cooldown icon on abilities that don't have enough resources to cast
            if (PowerResource != null
                && (GetResourceCost(playerManager.UnitController) > playerManager.UnitController.CharacterStats.GetPowerResourceAmount(PowerResource))) {
                //Debug.Log(DisplayName + ".BaseAbility.UpdateActionButtonVisual(): not enough resources to cast this ability.  enabling full cooldown");
                actionButton.EnableFullCoolDownIcon();
                return;
            }

            if (HadSpecialIcon(actionButton)) {
                return;
            }

            if (RequireOutOfCombat) {
                if (playerManager.UnitController.CharacterCombat.GetInCombat() == true) {
                    //Debug.Log("ActionButton.UpdateVisual(): can't cast due to being in combat");
                    actionButton.EnableFullCoolDownIcon();
                    return;
                }
            }

            if (RequireStealth) {
                if (playerManager.UnitController.CharacterStats.IsStealthed == false) {
                    //Debug.Log("ActionButton.UpdateVisual(): can't cast due to not being stealthed");
                    actionButton.EnableFullCoolDownIcon();
                    return;
                }
            }

            if (!CanCast(playerManager.UnitController)) {
                //Debug.Log(DisplayName + ".BaseAbility.UpdateActionButtonVisual(): can't cast due to spell restrictions");
                actionButton.EnableFullCoolDownIcon();
                return;
            }


            if (playerManager.UnitController.CharacterAbilityManager.RemainingGlobalCoolDown > 0f
                || playerManager.UnitController.CharacterAbilityManager.AbilityCoolDownDictionary.ContainsKey(DisplayName)) {
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
                if (playerManager.UnitController.CharacterAbilityManager.AbilityCoolDownDictionary.ContainsKey(DisplayName)) {
                    remainingAbilityCoolDown = playerManager.UnitController.CharacterAbilityManager.AbilityCoolDownDictionary[DisplayName].RemainingCoolDown;
                    initialCoolDown = playerManager.UnitController.CharacterAbilityManager.AbilityCoolDownDictionary[DisplayName].InitialCoolDown;
                } else {
                    initialCoolDown = abilityCoolDown;
                }
                //float globalCoolDown
                float fillAmount = Mathf.Max(remainingAbilityCoolDown, playerManager.UnitController.CharacterAbilityManager.RemainingGlobalCoolDown) /
                    (remainingAbilityCoolDown > playerManager.UnitController.CharacterAbilityManager.RemainingGlobalCoolDown ? initialCoolDown : playerManager.UnitController.CharacterAbilityManager.InitialGlobalCoolDown);
                //Debug.Log("Setting fill amount to: " + fillAmount);
                if (actionButton.CoolDownIcon.fillAmount != fillAmount) {
                    actionButton.CoolDownIcon.fillAmount = fillAmount;
                }
            } else {
                actionButton.DisableCoolDownIcon();
            }
        }

        public virtual Coroutine ChooseMonitorCoroutine(ActionButton actionButton) {

            if (systemConfigurationManager.AllowAutoAttack == true && IsAutoAttack == true) {
                //Debug.Log("ActionButton.OnUseableUse(" + ability.DisplayName + "): WAS ANIMATED AUTO ATTACK");
                //if (autoAttackCoRoutine == null) {
                //if (monitorCoroutine == null) {
                return systemAbilityController.StartCoroutine(actionButton.MonitorAutoAttack());
                //}
            }
            // actionbuttons can be disabled, but the systemability manager will not.  That's why the ability is monitored here
            return systemAbilityController.StartCoroutine(actionButton.MonitorAbility(DisplayName));
        }

        public TargetProps GetTargetOptions(IAbilityCaster abilityCaster) {
            // FIX ME - action hits should be able to target by their effects
            if (useAbilityEffectTargetting == true) {
                List<AbilityEffectProperties> abilityEffects = null;
                if (abilityEffectTargetStage == AbilityStage.CastChannel) {
                    abilityEffects = channeledAbilityEffects;
                }
                if (abilityEffectTargetStage == AbilityStage.CastEnd) {
                    abilityEffects = GetCastEndEffects(abilityCaster);
                }
                if (abilityEffectTargetStage == AbilityStage.ActionHit) {
                    abilityEffects = GetActionHitEffects(abilityCaster);
                }
                if (abilityEffectTargetStage == AbilityStage.ActionEnd) {
                    abilityEffects = GetActionEndEffects(abilityCaster);
                }
                if (abilityEffects.Count > 0) {
                    return abilityEffects[0].GetTargetOptions(abilityCaster);
                }
            }
            return targetOptions;
        }

        public bool PlayAttackVoice(CharacterCombat characterCombat) {
            if (useWeaponSkillAttackVoiceSetting == true) {
                return characterCombat.GetWeaponSkillAttackVoiceSetting();
            }

            return playAttackVoice;
        }

        public virtual List<AbilityEffectProperties> GetCastEndEffects(IAbilityCaster abilityCaster) {
            return castEndAbilityEffects;
        }

        public virtual List<AbilityEffectProperties> GetActionHitEffects(IAbilityCaster abilityCaster) {
            if (isAutoAttack) {
                List<AbilityEffectProperties> weaponAbilityList = abilityCaster.AbilityManager.GetDefaultHitEffects();
                if (weaponAbilityList != null && weaponAbilityList.Count > 0) {
                    return weaponAbilityList;
                }
            }
            return actionHitAbilityEffects;
        }

        public virtual List<AbilityEffectProperties> GetActionEndEffects(IAbilityCaster abilityCaster) {
            /*
            // disabled because default hit effects should only happen on action hit, not action end
            if (isAutoAttack) {
                List<AbilityEffectProperties> weaponAbilityList = abilityCaster.AbilityManager.GetDefaultHitEffects();
                if (weaponAbilityList != null && weaponAbilityList.Count > 0) {
                    return weaponAbilityList;
                }
            }
            */
            return actionEndAbilityEffects;
        }

        public virtual List<AbilityAttachmentNode> GetHoldableObjectList(IAbilityCaster abilityCaster) {
            if (abilityPrefabSource == AbilityPrefabSource.Both) {
                List<AbilityAttachmentNode> returnList = new List<AbilityAttachmentNode>();
                returnList.AddRange(holdableObjectList);
                returnList.AddRange(abilityCaster.AbilityManager.GetWeaponAbilityAnimationObjectList());
                return returnList;
            }
            if (abilityPrefabSource == AbilityPrefabSource.Weapon) {
                return abilityCaster.AbilityManager.GetWeaponAbilityAnimationObjectList();
            }

            // abilityPrefabSource is Ability
            return holdableObjectList;
        }

        public virtual float GetAbilityCastingTime(IAbilityCaster abilityCaster) {
            if (useSpeedMultipliers) {
                return GetBaseAbilityCastingTime(abilityCaster) * (1f / abilityCaster.AbilityManager.GetSpeed());
            }
            return GetBaseAbilityCastingTime(abilityCaster);
        }

        public List<AnimationClip> GetAbilityCastClips(IAbilityCaster sourceCharacter) {
            //Debug.Log($"{ResourceName}.AbilityProperties.GetAbilityCastClips()");

            if (useUnitCastAnimations == true) {
                return sourceCharacter.AbilityManager.GetUnitCastAnimations();
            } else {
                return CastClips;
            }
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

        public virtual string GetSummary() {
            //Debug.Log(DisplayName + ".BaseAbilityProperties.GetSummary()");

            return string.Format("{0}\n{1}", GetName(), GetDescription());
        }

        public virtual string GetDescription() {
            string requireString = string.Empty;
            bool affinityMet = false;
            string colorString = string.Empty;
            string addString = string.Empty;
            if (requireStealth == true) {
                if (playerManager.UnitController.CharacterStats.IsStealthed == false) {
                    addString = string.Format("\n<color={0}>Requires Stealth</color>", "#ff0000ff");
                }
            }
            if (weaponAffinityNames.Count == 0) {
                // no restrictions, automatically true
                affinityMet = true;

            } else {
                List<string> requireWeaponSkills = new List<string>();
                foreach (WeaponSkill _weaponAffinity in weaponAffinityList) {
                    requireWeaponSkills.Add(_weaponAffinity.DisplayName);
                    if (playerManager.UnitController.CharacterEquipmentManager.HasAffinity(_weaponAffinity)) {
                        affinityMet = true;
                    }
                }
                if (affinityMet) {
                    colorString = "#ffffffff";
                } else {
                    colorString = "#ff0000ff";
                }
                addString += string.Format("\n<color={0}>Requires: {1}</color>", colorString, string.Join(",", requireWeaponSkills));
            }

            string abilityRange = (GetTargetOptions(playerManager.UnitController).UseMeleeRange == true ? "melee" : GetTargetOptions(playerManager.UnitController).MaxRange + " meters");

            string costString = string.Empty;
            if (powerResource != null) {
                costString = "\nCost: " + GetResourceCost(playerManager.UnitController) + " " + powerResource.DisplayName;
            }

            string coolDownString = GetCooldownString();

            return string.Format("Cast time: {0} second(s)\nCooldown: {1} second(s){2}\nRange: {3}\n<color=#ffff00ff>{4}</color>{5}{6}",
                GetAbilityCastingTime(playerManager.UnitController).ToString("F1"),
                abilityCoolDown,
                costString,
                abilityRange,
                Description,
                addString,
                coolDownString);
        }

        public string GetCooldownString() {
            string coolDownString = string.Empty;
            if (playerManager?.UnitController?.CharacterAbilityManager != null
                && (playerManager.UnitController.CharacterAbilityManager.RemainingGlobalCoolDown > 0f
                || playerManager.UnitController.CharacterAbilityManager.AbilityCoolDownDictionary.ContainsKey(DisplayName))) {
                float dictionaryCooldown = 0f;
                if (playerManager.UnitController.CharacterAbilityManager.AbilityCoolDownDictionary.ContainsKey(DisplayName)) {
                    dictionaryCooldown = playerManager.UnitController.CharacterAbilityManager.AbilityCoolDownDictionary[DisplayName].RemainingCoolDown;
                }
                coolDownString = "\n\nCooldown Remaining: " + SystemAbilityController.GetTimeText(Mathf.Max(dictionaryCooldown, playerManager.UnitController.CharacterAbilityManager.RemainingGlobalCoolDown)); ;
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

        public virtual AudioClip GetAnimationEventSound(CharacterCombat characterCombat) {
            AudioClip characterClip = characterCombat.GetWeaponSkillAnimationHitSound();
            if (characterClip != null) {
                return characterClip;
            }
            return GetAnimationEventSound();
        }

        public virtual AudioClip GetAnimationEventSound() {
            return AnimationHitAudioClip;
        }

        public List<AnimationClip> GetAbilityActionClips(IAbilityCaster sourceCharacter) {
            if (useUnitAttackAnimations == true) {
                return sourceCharacter.AbilityManager.GetUnitAttackAnimations();
            } else if (useAutoAttackAnimations == true) {
                return sourceCharacter.AbilityManager.GetDefaultAttackAnimations();
            } else {
                return ActionClips;
            }
        }

        public virtual AudioClip GetHitSound(IAbilityCaster abilityCaster) {
            //Debug.Log(DisplayName + ".AnimatedAbility.GetHitSound(" + abilityCaster.AbilityManager.Name + ")");
            if (useWeaponHitSound == true) {
                //Debug.Log(DisplayName + ".AnimatedAbility.GetHitSound(" + abilityCaster.Name + "): using weapon hit sound");
                return abilityCaster.AbilityManager.GetAnimatedAbilityHitSound();
            }
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
                // FIX ME - this should take into account which of the ability effects (cast/hit/end) are used for targeting
                List<AbilityEffectProperties> abilityEffects = null;
                if (abilityEffectTargetStage == AbilityStage.CastChannel) {
                    abilityEffects = channeledAbilityEffects;
                }
                if (abilityEffectTargetStage == AbilityStage.CastEnd) {
                    abilityEffects = GetCastEndEffects(sourceCharacter);
                }
                if (abilityEffectTargetStage == AbilityStage.ActionHit) {
                    abilityEffects = GetActionHitEffects(sourceCharacter);
                }
                if (abilityEffectTargetStage == AbilityStage.ActionEnd) {
                    abilityEffects = GetActionEndEffects(sourceCharacter);
                }

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

        public bool Use(UnitController sourceUnitController) {
            //Debug.Log(DisplayName + ".BaseAbility.Use()");
            // prevent casting any ability without the proper weapon affinity
            if (CanCast(sourceUnitController, true)) {
                //Debug.Log(DisplayName + ".BaseAbility.Use(): cancast is true");
                sourceUnitController.CharacterAbilityManager.BeginAbility(this, true);
                return true;
            }
            return false;
        }

        public virtual bool Cast(IAbilityCaster sourceCharacter, Interactable target, AbilityEffectContext abilityEffectContext) {
            //Debug.Log($"{ResourceName}.AbilityProperties.Cast({(sourceCharacter == null ? "null" : sourceCharacter.AbilityManager.Name)}, {(target == null ? "null" : target.name)})");
            
            if (!CanCast(sourceCharacter)) {
                //Debug.Log(resourceName + ".BaseAbility.Cast(" + sourceCharacter.AbilityManager.Name + ", " + (target == null ? "null" : target.name) + " CAN'T CAST!!!");
                return false;
            }

            if (coolDownOnCast == false) {
                BeginAbilityCoolDown(sourceCharacter);
            }

            ProcessCleanupAbilityPrefabs(sourceCharacter);
            ProcessGCDAuto(sourceCharacter);

            List<AbilityEffectProperties> abilityEffectProperties = GetCastEndEffects(sourceCharacter);
            PerformAbilityEffects(sourceCharacter, target, abilityEffectContext, abilityEffectProperties);

            List<AnimationClip> usedAnimationClips = GetAbilityActionClips(sourceCharacter);
            if (usedAnimationClips.Count > 0) {
                //Debug.Log("AnimatedAbility.Cast(): animationClip is not null, setting animator");

                CharacterUnit targetCharacterUnit = null;
                if (target != null) {
                    targetCharacterUnit = CharacterUnit.GetCharacterUnit(target);
                }
                UnitController targetUnitController = null;
                if (targetCharacterUnit != null) {
                    targetUnitController = targetCharacterUnit.UnitController;
                }

                int attackIndex = UnityEngine.Random.Range(0, usedAnimationClips.Count);
                if (usedAnimationClips[attackIndex] != null) {
                    // perform the actual animation
                    float animationLength = sourceCharacter.AbilityManager.PerformAbilityAction(this, usedAnimationClips[attackIndex], attackIndex, targetUnitController, abilityEffectContext);

                    sourceCharacter.AbilityManager.ProcessAbilityCoolDowns(this, animationLength, abilityCoolDown);
                }

            } else {
                // since there is no action phase to despawn the objects, do it here
                sourceCharacter.AbilityManager.DespawnAbilityObjects();
            }
            return true;
            // notify subscribers
            //OnAbilityCast(this);
        }

        
        public void HandleAbilityHit(IAbilityCaster source, Interactable target, AbilityEffectContext abilityEffectContext) {
            //Debug.Log($"{ResourceName}.HandleAbilityHit()");

            List<AbilityEffectProperties> abilityEffectProperties = GetActionHitEffects(source);
            HandleAbilityHitCommon(source, target, abilityEffectContext, abilityEffectProperties);
        }

        public void HandleAbilityEndHit(IAbilityCaster source, Interactable target, AbilityEffectContext abilityEffectContext) {
            List<AbilityEffectProperties> abilityEffectProperties = GetActionEndEffects(source);
            if (abilityEffectProperties.Count == 0) {
                return;
            }
            HandleAbilityHitCommon(source, target, abilityEffectContext, abilityEffectProperties);
        }

        public void HandleAbilityHitCommon(IAbilityCaster source, Interactable target, AbilityEffectContext abilityEffectContext, List<AbilityEffectProperties> abilityEffectProperties) {
            //Debug.Log($"{ResourceName}.AbilityProperties.HandleAbilityHitCommon()");

            // perform a check that includes range to target, and does not include in progress ability action check
            bool rangeResult = CanUseOnBase(target, source);
            bool deactivateAutoAttack = false;
            if (rangeResult == false) {
                // if the range to target check failed, perform another check without range
                // if that passes, do not deactivate auto-attack.  target just moved away mid swing and didn't die/change faction etc
                if (!CanUseOnBase(target, source, true, null, false, false)) {
                    deactivateAutoAttack = true;
                }
            }
            source.AbilityManager.ProcessAnimatedAbilityHit(target, deactivateAutoAttack);

            // since ability and effects can have their own individual range and LOS requirements, check if the effects are allowed to hit
            // as long as only the LOS and range check failed from the ability (meaning no faction, liveness etc violations)
            if (deactivateAutoAttack == false || abilityEffectContext.baseAbility.GetTargetOptions(source).RequireTarget == false) {
                bool missResult = PerformAbilityEffects(source, target, abilityEffectContext, abilityEffectProperties);
            }
        }


        public virtual void BeginAbilityCoolDown(IAbilityCaster sourceCharacter, float animationLength = -1f) {
            if (sourceCharacter != null) {
                sourceCharacter.AbilityManager.BeginAbilityCoolDown(this, animationLength);
            }
        }

        /// <summary>
        /// give the cooldowns a chance to trigger
        /// </summary>
        /// <param name="sourceCharacter"></param>
        public virtual void ProcessGCDAuto(IAbilityCaster sourceCharacter) {
            //Debug.Log($"{ResourceName}.AbilityProperties.ProcessGCDAuto(" + (sourceCharacter == null ? "null" : sourceCharacter.gameObject.name) + ")");

            if (GetAbilityActionClips(sourceCharacter).Count > 0) {
                // cooldown length will be based on action animation length
                return;
            }

            // cooldown length 
            ProcessGCDManual(sourceCharacter);
        }

        public virtual void ProcessGCDManual(IAbilityCaster sourceCharacter, float usedCoolDown = 0f) {
            //Debug.Log(DisplayName + ".BaseAbility.ProcessGCDManual(" + usedCoolDown + ")");
            if (CanSimultaneousCast == false && IgnoreGlobalCoolDown == false && GetAbilityCastingTime(sourceCharacter) == 0f) {
                // if cast time was zero, initiate global cooldown to prevent spamming
                sourceCharacter.AbilityManager.InitiateGlobalCooldown(usedCoolDown);
            }
        }

        public virtual void ProcessCleanupAbilityPrefabs(IAbilityCaster sourceCharacter) {
            //Debug.Log($"{DisplayName}.BaseAbility.ProcessCleanupAbilityPrefabs()");

            if (despawnPrefabsOnCastEnd == false) {
                return;
            }
            
            if (GetHoldableObjectList(sourceCharacter) == null || GetHoldableObjectList(sourceCharacter).Count == 0) {
                return;
            }

            sourceCharacter.AbilityManager.DespawnAbilityObjects();
        }

        public virtual bool CanUseOn(Interactable target, IAbilityCaster sourceCharacter, bool performCooldownChecks = true, AbilityEffectContext abilityEffectContext = null, bool playerInitiated = false, bool performRangeCheck = true) {
            //Debug.Log($"{ResourceName}.AbilityProperties.CanUseOn({(target != null ? target.name : "null")}, {(sourceCharacter != null ? sourceCharacter.AbilityManager.Name : "null")}, {performCooldownChecks})");
            
            if (performCooldownChecks && !sourceCharacter.AbilityManager.PerformAbilityActionCheck(this)) {
                //Debug.Log($"{ResourceName}.AbilityProperties.CanUseOn(): failed cooldown check");
                return false;
            }

            if (!CanUseOnBase(target, sourceCharacter, performCooldownChecks, abilityEffectContext, playerInitiated, performRangeCheck)) {
                //Debug.Log($"{ResourceName}.AbilityProperties.CanUseOn(): failed base check");
                return false;
            }

            if (!CanSimultaneousCast) {
                if (performCooldownChecks == true && sourceCharacter.AbilityManager.PerformingAbility) {
                    if (playerInitiated) {
                        sourceCharacter.AbilityManager.ReceiveCombatMessage($"Cannot cast {describableData.DisplayName}. Another cast is in progress");
                    }
                    //Debug.Log($"{ResourceName}.AbilityProperties.CanUseOn(): failed in-progress check");
                    return false;
                }
            }
            return true;

        }

        // to be used at the end of ability hit because it doesn't perform in progress check
        public bool CanUseOnBase(Interactable target, IAbilityCaster sourceCharacter, bool performCooldownChecks = true, AbilityEffectContext abilityEffectContext = null, bool playerInitiated = false, bool performRangeCheck = true) {
            if (useAbilityEffectTargetting == true) {
                List<AbilityEffectProperties> abilityEffects = null;
                if (abilityEffectTargetStage == AbilityStage.CastChannel) {
                    abilityEffects = channeledAbilityEffects;
                }
                if (abilityEffectTargetStage == AbilityStage.CastEnd) {
                    abilityEffects = GetCastEndEffects(sourceCharacter);
                }
                if (abilityEffectTargetStage == AbilityStage.ActionHit) {
                    abilityEffects = GetActionHitEffects(sourceCharacter);
                }
                if (abilityEffectTargetStage == AbilityStage.ActionEnd) {
                    abilityEffects = GetActionEndEffects(sourceCharacter);
                }
                if (abilityEffects.Count > 0) {
                    return abilityEffects[0].CanUseOn(target, sourceCharacter, abilityEffectContext, playerInitiated, performRangeCheck);
                }
            }

            return targetOptions.CanUseOn(this, target, sourceCharacter, abilityEffectContext, playerInitiated, performRangeCheck);

        }

        public virtual void PerformCastEndEffects(IAbilityCaster source, Interactable target, AbilityEffectContext abilityEffectContext) {
            List<AbilityEffectProperties> usedCastEndEffects = GetCastEndEffects(source);

        }

        public virtual void PerformActionHitEffects(IAbilityCaster source, Interactable target, AbilityEffectContext abilityEffectContext) {
            List<AbilityEffectProperties> usedActionHitEffects = GetActionHitEffects(source);
        }

        public virtual void PerformActionEndEffects(IAbilityCaster source, Interactable target, AbilityEffectContext abilityEffectContext) {
            List<AbilityEffectProperties> usedActionEndEffects = GetActionEndEffects(source);
        }

        public virtual bool PerformAbilityEffects(IAbilityCaster source, Interactable target, AbilityEffectContext abilityEffectContext, List<AbilityEffectProperties> abilityEffectProperties) {
            //Debug.Log($"{ResourceName}.AbilityProperties.PerformAbilityEffects({source.AbilityManager.Name}, {(target ? target.name : "null")})");
            
            // FIX ME - this line existed only for DirectAbility - does it break anything else by being here ?
            abilityEffectContext.castTimeMultiplier = GetBaseAbilityCastingTime(source);

            // perform hit / miss check only if baseability requires target and return false if miss
            if (GetTargetOptions(source).RequireTarget) {
                if (!source.AbilityManager.DidAbilityHit(target, abilityEffectContext)) {
                    //Debug.Log($"{ResourceName}.AbilityProperties.PerformAbilityEffects({source.AbilityManager.Name}, {(target ? target.name : "null")}) missed!");
                    return false;
                }
            }

            // generate power resource
            source.AbilityManager.GeneratePower(this);

            foreach (AbilityEffectProperties abilityEffect in abilityEffectProperties) {
                if (abilityEffect == null) {
                    Debug.LogWarning("Forgot to set ability affect in inspector?");
                }
                //Debug.Log($"{ResourceName}.AbilityProperties.PerformAbilityEffects({source.AbilityManager.Name}, {(target ? target.name : "null")}) processing abilityEffect {abilityEffect.ResourceName}");
                AbilityEffectContext abilityEffectOutput = abilityEffectContext.GetCopy();
                if (abilityEffect != null
                    && abilityEffect.CanUseOn(target, source, abilityEffectContext)
                    && (abilityEffect.ChanceToCast >= 100f || abilityEffect.ChanceToCast >= UnityEngine.Random.Range(0f, 100f))) {
                    abilityEffect.Cast(source, target, target, abilityEffectOutput);
                } else {
                    //Debug.Log($"{ResourceName}.AbilityProperties.PerformAbilityEffects({source.AbilityManager.Name}, {(target ? target.name : "null")}) NULL ABILITYEFFECT OR COULD NOT USE ON");
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

        /*
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
        }
        */

        public virtual float OnCastTimeChanged(float currentCastPercent, float nextTickPercent, IAbilityCaster source, Interactable target, AbilityEffectContext abilityEffectContext) {
            //Debug.Log(DisplayName + ".BaseAbility.OnCastTimeChanged(" + currentCastPercent + ", " + nextTickPercent + ")");
            // overwrite me
            if (currentCastPercent >= nextTickPercent) {
                PerformChanneledEffect(source, target, abilityEffectContext);
                nextTickPercent += (tickRate / GetBaseAbilityCastingTime(source));
            }
            return nextTickPercent;
        }

        public void NotifyOnLearn(UnitController unitController) {
            OnAbilityLearn(unitController);
        }

        /// <summary>
        /// are the character class requirements met to learn or use this ability
        /// </summary>
        /// <returns></returns>
        public bool CharacterClassRequirementIsMet(CharacterClass characterClass) {
            // only used when changing class or for action bars, so hard coding player character is ok for now
            if (CharacterClassRequirementList != null && CharacterClassRequirementList.Count > 0) {
                if (!CharacterClassRequirementList.Contains(characterClass)) {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// are the class specialization requirements met to learn or use this ability
        /// </summary>
        /// <returns></returns>
        public bool ClassSpecializationRequirementIsMet(ClassSpecialization classSpecialization) {
            // only used when changing class or for action bars, so hard coding player character is ok for now
            if (ClassSpecializationRequirementList != null && ClassSpecializationRequirementList.Count > 0) {
                if (!ClassSpecializationRequirementList.Contains(classSpecialization)) {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// are all requirements met to learn or use this ability
        /// </summary>
        /// <returns></returns>
        public virtual bool RequirementsAreMet(UnitController unitController) {
            if (!CharacterClassRequirementIsMet(unitController.BaseCharacter.CharacterClass)) {
                return false;
            }
            if (!ClassSpecializationRequirementIsMet(unitController.BaseCharacter.ClassSpecialization)) {
                return false;
            }

            return true;
        }


        public void SetupScriptableObjects(SystemGameManager systemGameManager, IDescribable describable) {
            //base.SetupScriptableObjects(systemGameManager);
            describableData = describable;

            Configure(systemGameManager);

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
                        Debug.LogError($"AbilityProperties.SetupScriptableObjects(): Could not find weapon skill: {weaponAffinityName} while inititalizing {ResourceName}.  CHECK INSPECTOR");
                    }
                }
            }

            if (castingAudioClip != null) {
                systemGameManager.AudioManager.RegisterAudioClip(castingAudioClip);
            }

            castingAudioProfile = null;
            if (castingAudioProfileName != null && castingAudioProfileName != string.Empty) {
                AudioProfile audioProfile = systemDataFactory.GetResource<AudioProfile>(castingAudioProfileName);
                if (audioProfile != null) {
                    castingAudioProfile = audioProfile;
                } else {
                    Debug.LogError($"AbilityProperties.SetupScriptableObjects(): Could not find audio profile: {castingAudioProfileName} while inititalizing {ResourceName}.  CHECK INSPECTOR");
                }
            }

            animationHitAudioProfile = null;
            if (animationHitAudioProfileName != null && animationHitAudioProfileName != string.Empty) {
                AudioProfile audioProfile = systemDataFactory.GetResource<AudioProfile>(animationHitAudioProfileName);
                if (audioProfile != null) {
                    animationHitAudioProfile = audioProfile;
                } else {
                    Debug.LogError($"AbilityProperties.SetupScriptableObjects(): Could not find audio profile: {animationHitAudioProfileName} while inititalizing {ResourceName}.  CHECK INSPECTOR");
                }
            }

            if (animationProfileName != null && animationProfileName != string.Empty) {
                animationProfile = systemDataFactory.GetResource<AnimationProfile>(animationProfileName);
                if (animationProfile == null) {
                    Debug.LogError($"AbilityProperties.SetupScriptableObjects(): Could not find animation profile: {animationProfileName} while inititalizing {ResourceName}.  CHECK INSPECTOR");
                }
            }
            if (animationProfile == null) {
                actionClips = new List<AnimationClip>();
                castClips = new List<AnimationClip>();
            } else {
                actionClips = animationProfile.AnimationProps.AttackClips;
                castClips = animationProfile.AnimationProps.CastClips;
            }

            powerResource = null;
            if (powerResourceName != null && powerResourceName != string.Empty) {
                PowerResource tmpPowerResource = systemDataFactory.GetResource<PowerResource>(powerResourceName);
                if (tmpPowerResource != null) {
                    powerResource = tmpPowerResource;
                } else {
                    Debug.LogError($"AbilityProperties.SetupScriptableObjects(): Could not find power resource: {powerResourceName} while inititalizing {ResourceName}.  CHECK INSPECTOR");
                }
            }

            generatePowerResource = null;
            if (generatePowerResourceName != null && generatePowerResourceName != string.Empty) {
                PowerResource tmpPowerResource = systemDataFactory.GetResource<PowerResource>(generatePowerResourceName);
                if (tmpPowerResource != null) {
                    generatePowerResource = tmpPowerResource;
                } else {
                    Debug.LogError($"AbilityProperties.SetupScriptableObjects(): Could not find power resource: {powerResourceName} while inititalizing {ResourceName}.  CHECK INSPECTOR");
                }
            }


            /*
            // add inline channeled effects
            foreach (AbilityEffectConfig abilityEffectConfig in inlineChannelingEffects) {
                abilityEffectConfig.SetupScriptableObjects(systemGameManager, this);
                channeledAbilityEffects.Add(abilityEffectConfig.AbilityEffectProperties);
            }
            */

            // add named channeled effects
            if (channeledAbilityEffectnames != null) {
                foreach (string abilityEffectName in channeledAbilityEffectnames) {
                    AbilityEffect abilityEffect = systemDataFactory.GetResource<AbilityEffect>(abilityEffectName);
                    if (abilityEffect != null) {
                        channeledAbilityEffects.Add(abilityEffect.AbilityEffectProperties);
                    } else {
                        Debug.LogError($"AbilityProperties.SetupScriptableObjects(): Could not find ability effect: {abilityEffectName} while inititalizing {ResourceName}.  CHECK INSPECTOR");
                    }
                }
            }

            /*
            // add inline cast end effects
            foreach (AbilityEffectConfig abilityEffectConfig in inlineAbilityEffects) {
                if (abilityEffectConfig != null) {
                    abilityEffectConfig.SetupScriptableObjects(systemGameManager, this);
                    castEndAbilityEffects.Add(abilityEffectConfig.AbilityEffectProperties);
                } else {
                    Debug.LogWarning("Null inline AbilityEffect detected while initializing BaseAbility Properties for " + describable.ResourceName);
                }
            }
            */

            // add named cast end effects
            if (AbilityEffectNames != null) {
                foreach (string abilityEffectName in AbilityEffectNames) {
                    AbilityEffect abilityEffect = systemDataFactory.GetResource<AbilityEffect>(abilityEffectName);
                    if (abilityEffect != null) {
                        castEndAbilityEffects.Add(abilityEffect.AbilityEffectProperties);
                    } else {
                        Debug.LogError($"AbilityProperties.SetupScriptableObjects(): Could not find ability effect: {abilityEffectName} while inititalizing {ResourceName}.  CHECK INSPECTOR");
                    }
                }
            }

            // add action hit effects
            if (actionHitAbilityEffectNames != null) {
                foreach (string abilityEffectName in actionHitAbilityEffectNames) {
                    AbilityEffect abilityEffect = systemDataFactory.GetResource<AbilityEffect>(abilityEffectName);
                    if (abilityEffect != null) {
                        actionHitAbilityEffects.Add(abilityEffect.AbilityEffectProperties);
                    } else {
                        Debug.LogError($"AbilityProperties.SetupScriptableObjects(): Could not find ability effect: {abilityEffectName} while inititalizing {ResourceName}.  CHECK INSPECTOR");
                    }
                }
            }

            // add action end effects
            if (actionEndAbilityEffectNames != null) {
                foreach (string abilityEffectName in actionEndAbilityEffectNames) {
                    AbilityEffect abilityEffect = systemDataFactory.GetResource<AbilityEffect>(abilityEffectName);
                    if (abilityEffect != null) {
                        actionEndAbilityEffects.Add(abilityEffect.AbilityEffectProperties);
                    } else {
                        Debug.LogError($"AbilityProperties.SetupScriptableObjects(): Could not find ability effect: {abilityEffectName} while inititalizing {ResourceName}.  CHECK INSPECTOR");
                    }
                }
            }


            characterClassRequirementList = new List<CharacterClass>();
            if (characterClassRequirements != null) {
                foreach (string characterClassName in characterClassRequirements) {
                    CharacterClass tmpCharacterClass = systemDataFactory.GetResource<CharacterClass>(characterClassName);
                    if (tmpCharacterClass != null) {
                        characterClassRequirementList.Add(tmpCharacterClass);
                    } else {
                        Debug.LogError($"AbilityProperties.SetupScriptableObjects(): Could not find character class : {characterClassName} while inititalizing {ResourceName}.  CHECK INSPECTOR");
                    }
                }
            }

            if (classSpecializationRequirements != null) {
                foreach (string classSpecializationName in classSpecializationRequirements) {
                    ClassSpecialization tmpClassSpecialization = systemDataFactory.GetResource<ClassSpecialization>(classSpecializationName);
                    if (tmpClassSpecialization != null) {
                        classSpecializationRequirementList.Add(tmpClassSpecialization);
                    } else {
                        Debug.LogError($"AbilityProperties.SetupScriptableObjects(): Could not find class specialization : {classSpecializationName} while inititalizing {ResourceName}.  CHECK INSPECTOR");
                    }
                }
            }



        }

    }

}