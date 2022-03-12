using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "New Action", menuName = "AnyRPG/AnimatedAction")]
    public class AnimatedAction : DescribableResource, IUseable, IMoveable /*, ITargetable,*/ /*ILearnable*/ {

        public event System.Action OnAbilityLearn = delegate { };
        public event System.Action OnAbilityUsed = delegate { };


        [Header("Action")]

        [SerializeField]
        private AnimatedActionProperties actionProperties = new AnimatedActionProperties();

        // game manager references
        protected PlayerManager playerManager = null;
        protected UIManager uIManager = null;
        protected SystemAbilityController systemAbilityController = null;

        public AnimatedActionProperties ActionProperties { get => actionProperties; }
        public float CoolDown { get => 0f; }
        public virtual bool RequireOutOfCombat { get => false; }

        /// <summary>
        /// return the casting time of the ability without any speed modifiers applied
        /// </summary>
        public virtual float GetBaseAbilityCastingTime(IAbilityCaster source) {
            if (GetCastClips(source).Count > 0) {
                return GetCastClips(source)[0].length;
            }
            return 0f;
        }


        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            playerManager = systemGameManager.PlayerManager;
            uIManager = systemGameManager.UIManager;
            systemAbilityController = systemGameManager.SystemAbilityController;
        }

        public void UpdateTargetRange(ActionBarManager actionBarManager, ActionButton actionButton) {
            // do nothing
        }

        public virtual bool IsUseableStale() {
            return false;
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

        public virtual bool HadSpecialIcon(ActionButton actionButton) {
            return false;
        }

        public virtual void UpdateActionButtonVisual(ActionButton actionButton) {
            //Debug.Log(DisplayName + ".BaseAbility.UpdateActionButtonVisual()");
            // set cooldown icon on abilities that don't have enough resources to cast

            if (HadSpecialIcon(actionButton)) {
                return;
            }

            if (!CanCast(playerManager.MyCharacter)) {
                //Debug.Log(DisplayName + ".BaseAbility.UpdateActionButtonVisual(): can't cast due to spell restrictions");
                actionButton.EnableFullCoolDownIcon();
                return;
            }

            actionButton.DisableCoolDownIcon();
        }

        public virtual Coroutine ChooseMonitorCoroutine(ActionButton actionButton) {
            // actionbuttons can be disabled, but the systemability manager will not.  That's why the ability is monitored here
                //Debug.Log("ActionButton.OnUseableUse(" + ability.DisplayName + "): WAS NOT ANIMATED AUTO ATTACK");
                //if (abilityCoRoutine == null) {
                //if (monitorCoroutine == null) {
                    //return systemAbilityController.StartCoroutine(actionButton.MonitorAbility(this));
                //}
            return null;
        }

        /*
        public TargetProps GetTargetOptions(IAbilityCaster abilityCaster) {
            return targetOptions;
        }
        */

        public virtual float GetAbilityCastingTime(IAbilityCaster abilityCaster) {
            return GetBaseAbilityCastingTime(abilityCaster);
        }

        public List<AnimationClip> GetCastClips(IAbilityCaster sourceCharacter) {
            List<AnimationClip> animationClips = new List<AnimationClip>();
            /*
            if (useUnitCastAnimations == true) {
                animationClips = sourceCharacter.AbilityManager.GetUnitCastAnimations();
            } else {
            */
                animationClips.Add(actionProperties.AnimationClip);
            //}
            return animationClips;
        }

        /*
        public AnimationProps GetUnitAnimationProps(IAbilityCaster sourceCharacter) {
            
            if (useUnitCastAnimations == true) {
                return sourceCharacter.AbilityManager.GetUnitAnimationProps();
            } else {
            
                return animationProfile.AnimationProps;
            //}
        }
    */


        public override string GetSummary() {
            string requireString = string.Empty;
            string colorString = string.Empty;

            //string abilityRange = (GetTargetOptions(playerManager.MyCharacter).UseMeleeRange == true ? "melee" : GetTargetOptions(playerManager.MyCharacter).MaxRange + " meters");

            return string.Format("<color=#ffff00ff>{0}</color>",
                description);
        }

        public string GetShortDescription() {
            return description;
        }

        public bool CanCast(IAbilityCaster sourceCharacter, bool playerInitiated = false) {
            // cannot cast due to being stunned
            if (sourceCharacter.AbilityManager.ControlLocked) {
                return false;
            }
            return true;
        }

        public bool Use() {
            //Debug.Log(DisplayName + ".BaseAbility.Use()");
            // prevent casting any ability without the proper weapon affinity
            if (CanCast(playerManager.MyCharacter, true)) {
                //Debug.Log(DisplayName + ".BaseAbility.Use(): cancast is true");
                //playerManager.MyCharacter.CharacterAbilityManager.BeginAbility(this, true);
                //playerManager.ActiveUnitController.UnitActionManager.BeginAction(this, true);
                return true;
            }
            return false;
        }

        public void NotifyOnLearn() {
            OnAbilityLearn();
        }

        public void NotifyOnAbilityUsed() {
            OnAbilityUsed();
        }

        public override void SetupScriptableObjects(SystemGameManager systemGameManager) {
            base.SetupScriptableObjects(systemGameManager);

            actionProperties.SetupScriptableObjects(systemGameManager, DisplayName);

        }

    }


    [System.Serializable]
    public class AnimatedActionProperties : ConfiguredClass {

        [Header("Animation")]

        [Tooltip("The animation clip the character will perform")]
        [SerializeField]
        protected AnimationClip animationClip = null;

        [Tooltip("The name of an animation profile to get animations for the character to perform")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(AnimationProfile))]
        protected string animationProfileName = string.Empty;

        protected AnimationProfile animationProfile;

        [Header("Audio")]

        [Tooltip("An audio profile to play while the ability is casting")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(AudioProfile))]
        protected string castingAudioProfileName;

        protected AudioProfile castingAudioProfile;

        [Header("Prefabs")]

        [Tooltip("Physical prefabs to attach to bones on the character unit")]
        [SerializeField]
        private List<AbilityAttachmentNode> holdableObjectList = new List<AbilityAttachmentNode>();

        /*
        [Tooltip("holdable object prefabs are created by the animator from an animation event, not from the ability manager during cast start")]
        [SerializeField]
        protected bool animatorCreatePrefabs;
        */

        /*
        [Tooltip("Delay to destroy casting effect prefabs after casting completes")]
        [SerializeField]
        protected float prefabDestroyDelay = 0f;
        */

        /*
        [Header("Learning")]

        [Tooltip("The minimum level a character must be to cast this ability")]
        [SerializeField]
        protected int requiredLevel = 1;

        [Tooltip("If true, this ability does not have to be learned to cast. For abilities that anyone can use, like scrolls or crafting")]
        [SerializeField]
        protected bool useableWithoutLearning = false;

        [Header("Casting Restrictions")]

        [Tooltip("This ability can be cast while moving.")]
        [SerializeField]
        protected bool canCastWhileMoving = false;
        */

        private string displayName = string.Empty;

        public AudioClip CastingAudioClip { get => (castingAudioProfile == null ? null : castingAudioProfile.AudioClip); }
        //public bool AnimatorCreatePrefabs { get => animatorCreatePrefabs; set => animatorCreatePrefabs = value; }
        public List<AnimationClip> AttackClips { get => (animationProfile != null ? animationProfile.AnimationProps.AttackClips : null); }
        public List<AnimationClip> CastClips { get => (animationProfile != null ? animationProfile.AnimationProps.CastClips : new List<AnimationClip>()); }
        //public bool RequireOutOfCombat { get => requireOutOfCombat; set => requireOutOfCombat = value; }
        //public LineOfSightSourceLocation LineOfSightSourceLocation { get => LineOfSightSourceLocation.Caster; }
        //public TargetRangeSourceLocation TargetRangeSourceLocation { get => TargetRangeSourceLocation.Caster; }
        //public bool CanCastWhileMoving { get => canCastWhileMoving; set => canCastWhileMoving = value; }
        //public int RequiredLevel { get => requiredLevel; }
        //public bool UseableWithoutLearning { get => useableWithoutLearning; }
        public string DisplayName { get => displayName; }
        
        public AnimationClip AnimationClip {
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
            set => animationClip = value;
        }

        public float ActionCastingTime {
            get {
                if (AnimationClip != null) {
                    return animationClip.length;
                }
                return 0f;
            }
        }

        public List<AbilityAttachmentNode> HoldableObjectList { get => holdableObjectList; set => holdableObjectList = value; }

        /*
        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
        }
        */

        public void SetupScriptableObjects(SystemGameManager systemGameManager, string ownerName) {
            Configure(systemGameManager);

            displayName = ownerName;
       
            if (holdableObjectList != null) {
                foreach (AbilityAttachmentNode holdableObjectAttachment in holdableObjectList) {
                    if (holdableObjectAttachment != null) {
                        holdableObjectAttachment.SetupScriptableObjects(ownerName, systemGameManager);
                    }
                }
            }

            castingAudioProfile = null;
            if (castingAudioProfileName != null && castingAudioProfileName != string.Empty) {
                AudioProfile audioProfile = systemDataFactory.GetResource<AudioProfile>(castingAudioProfileName);
                if (audioProfile != null) {
                    castingAudioProfile = audioProfile;
                } else {
                    Debug.LogError("BaseAbility.SetupScriptableObjects(): Could not find audio profile: " + castingAudioProfileName + " while inititalizing " + ownerName + ".  CHECK INSPECTOR");
                }
            }

            animationProfile = null;
            if (animationProfileName != null && animationProfileName != string.Empty) {
                AnimationProfile tmpAnimationProfile = systemDataFactory.GetResource<AnimationProfile>(animationProfileName);
                if (tmpAnimationProfile != null) {
                    animationProfile = tmpAnimationProfile;
                } else {
                    Debug.LogError("BaseAbility.SetupScriptableObjects(): Could not find animation profile: " + animationProfileName + " while inititalizing " + ownerName + ".  CHECK INSPECTOR");
                }
            }

        }
    }

}