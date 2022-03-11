using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AnyRPG {
    public class UnitAnimator : ConfiguredClass {

        // events
        public event System.Action OnReviveComplete = delegate { };
        public event System.Action<bool> OnStartCasting = delegate { };
        public event System.Action<bool> OnEndCasting = delegate { };
        public event System.Action<bool> OnStartAttacking = delegate { };
        public event System.Action<bool> OnEndAttacking = delegate { };
        public event System.Action OnStartLevitated = delegate { };
        public event System.Action<bool> OnEndLevitated = delegate { };
        public event System.Action OnStartStunned = delegate { };
        public event System.Action<bool> OnEndStunned = delegate { };
        public event System.Action OnStartRevive = delegate { };
        public event System.Action OnDeath = delegate { };

        // components
        private Animator animator = null;

        // reference to default animation profile
        private AnimationProps defaultAnimationProps = null;

        // reference to current animation profile
        private AnimationProps currentAnimationProps = null;

        private RuntimeAnimatorController animatorController = null;
        private AnimatorOverrideController overrideController = null;

        private RuntimeAnimatorController thirdPartyAnimatorController = null;
        private AnimatorOverrideController thirdPartyOverrideController = null;

        // have to keep track of current override controller separately
        private AnimatorOverrideController currentOverrideController = null;

        private UnitController unitController = null;

        protected bool initialized = false;

        // keep track of the number of hits in the last animation for normalizing damage to animation length
        private float lastAnimationLength = 0f;

        // keep track of the number of hits in the last animation for normalizing multi-hit abilities
        private int lastAnimationHits = 0;

        // the full set of current animations (defaultAnimationProps + currentAnimationProps)
        private AnimationProps currentAnimations = null;

        // reference to system animation profile to use in lookups for overrides
        private AnimationProps systemAnimations = null;

        protected bool eventSubscriptionsInitialized = false;

        private Dictionary<string, float> animationSpeeds = new Dictionary<string, float>();
        
        // configure which direction animations should get their speeds from
        private List<string> zList = new List<string>();
        private List<string> xList = new List<string>();
        private List<string> magnitudeList = new List<string>();

        // in combat animations
        private float baseWalkAnimationSpeed = 1f;
        private float baseRunAnimationSpeed = 3.4f;
        private float baseWalkBackAnimationSpeed = 1.6f;
        private float baseRunBackAnimationSpeed = 3.4f;
        private float baseWalkStrafeRightAnimationSpeed = 1f;
        private float baseJogStrafeRightAnimationSpeed = 2.4f;
        private float baseWalkStrafeBackRightAnimationSpeed = 1f;
        private float baseJogStrafeBackRightAnimationSpeed = 1f;
        private float baseWalkStrafeForwardRightAnimationSpeed = 1f;
        private float baseJogStrafeForwardRightAnimationSpeed = 2.67f;
        private float baseWalkStrafeLeftAnimationSpeed = 1f;
        private float baseJogStrafeLeftAnimationSpeed = 2.4f;
        private float baseWalkStrafeBackLeftAnimationSpeed = 1f;
        private float baseJogStrafeBackLeftAnimationSpeed = 2.67f;
        private float baseWalkStrafeForwardLeftAnimationSpeed = 1f;
        private float baseJogStrafeForwardLeftAnimationSpeed = 2.67f;

        // in combat animations
        private float baseCombatWalkAnimationSpeed = 1f;
        private float baseCombatRunAnimationSpeed = 3.4f;
        private float baseCombatWalkBackAnimationSpeed = 1.6f;
        private float baseCombatRunBackAnimationSpeed = 3.4f;
        private float baseCombatWalkStrafeRightAnimationSpeed = 1f;
        private float baseCombatJogStrafeRightAnimationSpeed = 2.4f;
        private float baseCombatWalkStrafeBackRightAnimationSpeed = 1f;
        private float baseCombatJogStrafeBackRightAnimationSpeed = 2.67f;
        private float baseCombatWalkStrafeForwardRightAnimationSpeed = 1f;
        private float baseCombatJogStrafeForwardRightAnimationSpeed = 2.67f;
        private float baseCombatWalkStrafeLeftAnimationSpeed = 1f;
        private float baseCombatJogStrafeLeftAnimationSpeed = 2.4f;
        private float baseCombatWalkStrafeBackLeftAnimationSpeed = 1f;
        private float baseCombatJogStrafeBackLeftAnimationSpeed = 2.67f;
        private float baseCombatWalkStrafeForwardLeftAnimationSpeed = 1f;
        private float baseCombatJogStrafeForwardLeftAnimationSpeed = 2.67f;

        private Coroutine attackCoroutine = null;
        private Coroutine resurrectionCoroutine = null;

        // a reference to any current ability we are casting
        private AbilityEffectContext currentAbilityEffectContext = null;

        private Dictionary<AnimatorOverrideController, List<string>> animatorParameters = new Dictionary<AnimatorOverrideController, List<string>>();

        protected bool componentReferencesInitialized = false;

        // game manager references

        protected ControlsManager controlsManager = null;
        protected CameraManager cameraManager = null;

        public bool applyRootMotion { get => (animator != null ? animator.applyRootMotion : false); }
        public Animator Animator { get => animator; }
        public AbilityEffectContext CurrentAbilityEffectContext { get => currentAbilityEffectContext; set => currentAbilityEffectContext = value; }
        public RuntimeAnimatorController AnimatorController {
            get => animatorController;
            set => animatorController = value;
        }
        public float LastAnimationLength { get => lastAnimationLength; set => lastAnimationLength = value; }
        public int LastAnimationHits { get => lastAnimationHits; set => lastAnimationHits = value; }
        public AnimationProps CurrentAnimations { get => currentAnimations; }

        public UnitAnimator(UnitController unitController, SystemGameManager systemGameManager) {
            this.unitController = unitController;
            Configure(systemGameManager);
            systemAnimations = systemConfigurationManager.SystemAnimationProfile.AnimationProps;
            currentAnimationProps = new AnimationProps();
            currentAnimationProps.Configure();

            // set current animations to the system animations
            // these have to be deep copied because we don't want to overwrite the system animations when we replace them
            currentAnimations = new AnimationProps();
            currentAnimations.Configure();
            foreach (string keyName in systemAnimations.AnimationClips.Keys) {
                currentAnimations.AnimationClips[keyName] = systemAnimations.AnimationClips[keyName];
            }
            currentAnimations.AttackClips = systemAnimations.AttackClips;
            currentAnimations.CastClips = systemAnimations.CastClips;
            currentAnimations.TakeDamageClips = systemAnimations.TakeDamageClips;

            animatorController = systemConfigurationManager.DefaultAnimatorController;
            defaultAnimationProps = systemConfigurationManager.DefaultAnimationProfile.AnimationProps;

            // out of combat animations
            animationSpeeds.Add("MoveForwardClip", 1f);
            animationSpeeds.Add("MoveForwardFastClip", 3.4f);
            animationSpeeds.Add("MoveBackClip", 1.6f);
            animationSpeeds.Add("MoveBackFastClip", 3.4f);
            animationSpeeds.Add("StrafeLeftClip", 1f);
            animationSpeeds.Add("StrafeRightClip", 1f);
            animationSpeeds.Add("StrafeForwardLeftClip", 1f);
            animationSpeeds.Add("StrafeForwardRightClip", 1f);
            animationSpeeds.Add("StrafeBackLeftClip", 1f);
            animationSpeeds.Add("StrafeBackRightClip", 1f);
            animationSpeeds.Add("JogStrafeLeftClip", 2.67f);
            animationSpeeds.Add("JogStrafeRightClip", 2.4f);
            animationSpeeds.Add("JogStrafeForwardLeftClip", 2.67f);
            animationSpeeds.Add("JogStrafeForwardRightClip", 2.67f);
            animationSpeeds.Add("JogStrafeBackLeftClip", 2.4f);
            animationSpeeds.Add("JogStrafeBackRightClip", 1f);

            // in combat animations
            animationSpeeds.Add("CombatMoveForwardClip", 1f);
            animationSpeeds.Add("CombatMoveForwardFastClip", 3.4f);
            animationSpeeds.Add("CombatMoveBackClip", 1.6f);
            animationSpeeds.Add("CombatMoveBackFastClip", 3.4f);
            animationSpeeds.Add("CombatStrafeLeftClip", 1f);
            animationSpeeds.Add("CombatJogStrafeLeftClip", 2.4f);
            animationSpeeds.Add("CombatStrafeRightClip", 1f);
            animationSpeeds.Add("CombatJogStrafeRightClip", 2.4f);
            animationSpeeds.Add("CombatStrafeForwardLeftClip", 1f);
            animationSpeeds.Add("CombatJogStrafeForwardLeftClip", 2.67f);
            animationSpeeds.Add("CombatStrafeForwardRightClip", 1f);
            animationSpeeds.Add("CombatJogStrafeForwardRightClip", 2.67f);
            animationSpeeds.Add("CombatStrafeBackLeftClip", 1f);
            animationSpeeds.Add("CombatJogStrafeBackLeftClip", 2.67f);
            animationSpeeds.Add("CombatStrafeBackRightClip", 1f);
            animationSpeeds.Add("CombatJogStrafeBackRightClip", 2.67f);

            // animation speeds
            zList.Add("MoveForwardClip");
            zList.Add("MoveForwardFastClip");
            zList.Add("CombatMoveForwardClip");
            zList.Add("CombatMoveForwardFastClip");
            zList.Add("MoveBackClip");
            zList.Add("MoveBackFastClip");
            zList.Add("CombatMoveBackClip");
            zList.Add("CombatMoveBackFastClip");
            xList.Add("StrafeLeftClip");
            xList.Add("StrafeRightClip");
            xList.Add("JogStrafeLeftClip");
            xList.Add("JogStrafeRightClip");
            xList.Add("CombatStrafeLeftClip");
            xList.Add("CombatJogStrafeLeftClip");
            xList.Add("CombatStrafeRightClip");
            xList.Add("CombatJogStrafeRightClip");
            magnitudeList.Add("StrafeForwardLeftClip");
            magnitudeList.Add("StrafeForwardRightClip");
            magnitudeList.Add("StrafeBackLeftClip");
            magnitudeList.Add("StrafeBackRightClip");
            magnitudeList.Add("JogStrafeForwardLeftClip");
            magnitudeList.Add("JogStrafeForwardRightClip");
            magnitudeList.Add("JogStrafeBackLeftClip");
            magnitudeList.Add("JogStrafeBackRightClip");
            magnitudeList.Add("CombatStrafeForwardLeftClip");
            magnitudeList.Add("CombatJogStrafeForwardLeftClip");
            magnitudeList.Add("CombatStrafeForwardRightClip");
            magnitudeList.Add("CombatJogStrafeForwardRightClip");
            magnitudeList.Add("CombatStrafeBackLeftClip");
            magnitudeList.Add("CombatJogStrafeBackLeftClip");
            magnitudeList.Add("CombatStrafeBackRightClip");
            magnitudeList.Add("CombatJogStrafeBackRightClip");
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            controlsManager = systemGameManager.ControlsManager;
            cameraManager = systemGameManager.CameraManager;
        }

        public void Init(Animator animator) {
            //animator = unitController.gameObject.GetComponentInChildren<Animator>();
            if (animator == null) {
                return;
            }
            this.animator = animator;

            // configure character animation event receiver
            UnitAnimationEventReceiver characterAnimationEventReceiver = animator.GetComponent<UnitAnimationEventReceiver>();
            if (characterAnimationEventReceiver == null) {
                characterAnimationEventReceiver = animator.gameObject.AddComponent<UnitAnimationEventReceiver>();
            }
            characterAnimationEventReceiver.Setup(unitController);
            InitializeAnimator();
        }


        public void DisableRootMotion() {
            //Debug.Log(gameObject.name + ": CharacterAnimator.DisableRootMotion()");
            if (animator != null) {
                animator.applyRootMotion = false;
            }
        }

        public void EnableRootMotion() {
            //Debug.Log(gameObject.name + ": CharacterAnimator.EnableRootMotion()");
            if (animator != null) {
                animator.applyRootMotion = true;
            }
        }

        public void EnableAnimator() {
            if (animator != null) {
                animator.enabled = true;
            }
        }

        public void InitializeAnimator() {
            //Debug.Log(unitController.gameObject.name + ".UnitAnimator.InitializeAnimator()");
            if (initialized) {
                return;
            }
            if (animator == null) {
                //Debug.Log(gameObject.name + ".UnitAnimator.InitializeAnimator(): Could not find animator in children");
                return;
            }
            if (systemConfigurationManager.UseThirdPartyMovementControl == true) {
                if (thirdPartyAnimatorController == null) {
                    thirdPartyAnimatorController = animator.runtimeAnimatorController;
                }
                if (thirdPartyAnimatorController != null) {
                    thirdPartyOverrideController = new AnimatorOverrideController(thirdPartyAnimatorController);
                }
            }

            if (overrideController == null) {
                //Debug.Log(unitController.gameObject.name + ".UnitAnimator.InitializeAnimator() override controller was null");
                if (animatorController == null) {
                    //Debug.Log(unitController.gameObject.name + ".UnitAnimator.InitializeAnimator() animatorController is null");
                } else {
                    //Debug.Log(unitController.gameObject.name + ".UnitAnimator.InitializeAnimator() creating new override controller");
                    overrideController = new AnimatorOverrideController(animatorController);
                    //SetOverrideController(overrideController);
                    SetCorrectOverrideController(false);
                }
            }
            //Debug.Log(gameObject.name + ": setting override controller to: " + overrideController.name);

            // before finishing initialization, search for a valid unit profile and try to get an animation profile from it
            if (unitController.UnitProfile != null && unitController.UnitProfile != null && unitController.UnitProfile.UnitPrefabProps.AnimationProps != null) {
                defaultAnimationProps = unitController.UnitProfile.UnitPrefabProps.AnimationProps;
            }
            SetAnimationProfileOverride(defaultAnimationProps);

            initialized = true;
        }

        public void SetCorrectOverrideController(bool runUpdate = true) {
            //Debug.Log(unitController.gameObject.name + ".UnitAnimator.SetCorrectOverrideController()");
            if (unitController.UnitControllerMode == UnitControllerMode.Player && systemConfigurationManager.UseThirdPartyMovementControl == true) {
                SetOverrideController(thirdPartyOverrideController, runUpdate);
                return;
            }

            // AI or no third party movement control case
            SetOverrideController(overrideController, runUpdate);
        }

        public void SetDefaultOverrideController(bool runUpdate = true) {
            //Debug.Log(unitController.gameObject.name + ".UnitAnimator.SetDefaultOverrideController()");
            SetOverrideController(overrideController, runUpdate);
        }

        public void SetOverrideController(AnimatorOverrideController animatorOverrideController, bool runUpdate = true) {
            //Debug.Log(unitController.gameObject.name + ".UnitAnimator.SetOverrideController(" + animatorOverrideController.name + ", " + runUpdate + ")");

            if (animator.runtimeAnimatorController != animatorOverrideController && animatorOverrideController != null) {
                //Debug.Log(unitController.gameObject.name + ".UnitAnimator.SetOverrideController(): setting animator override");
                currentOverrideController = animatorOverrideController;
                animator.runtimeAnimatorController = animatorOverrideController;

                // since getting animator parameters is expensive, we only want to do it the first time an override controller is set
                if (animatorParameters.ContainsKey(animatorOverrideController) == false) {
                    animatorParameters.Add(animatorOverrideController, new List<string>());
                    foreach (AnimatorControllerParameter animatorControllerParameter in animator.parameters) {
                        animatorParameters[animatorOverrideController].Add(animatorControllerParameter.name);
                    }
                }

                // set animator on UMA if one exists
                if (unitController.UnitModelController != null) {
                    //Debug.Log(unitController.gameObject.name + ".UnitAnimator.SetOverrideController(" + animatorOverrideController.name + ") setting override controller on UMA");
                    unitController.UnitModelController.SetAnimatorOverrideController(animatorOverrideController);
                }
                //animator.updateMode = AnimatorUpdateMode.
                if (runUpdate) {
                    animator.Update(0f);
                }
            }
        }

        public void SetAnimationProfileOverride(AnimationProps animationProps) {
            //Debug.Log(unitController.gameObject.name + ".UnitAnimator.SetAnimationProfileOverride()");
            currentAnimationProps = animationProps;
            SetAnimationClipOverrides();
        }

        public void ResetAnimationProfile() {
            //Debug.Log(gameObject.name + ".UnitAnimator.ResetAnimationProfile()");
            currentAnimationProps = defaultAnimationProps;

            // change back to the original animations
            SetAnimationClipOverrides();
        }

        protected void SetAnimationClipOverrides() {
            //Debug.Log(gameObject.name + ": CharacterAnimator.SetAnimationClipOverrides()");
            if (systemConfigurationManager == null) {
                return;
            }

            //AnimatorOverrideController tempOverrideController = new AnimatorOverrideController(animatorController);

            List<string> overrideControllerClipList = new List<string>();
            List<KeyValuePair<AnimationClip, AnimationClip>> animationClipPairs = new List<KeyValuePair<AnimationClip, AnimationClip>>();
            overrideController.GetOverrides(animationClipPairs);
            foreach (KeyValuePair<AnimationClip, AnimationClip> animationClipPair in animationClipPairs) {
                //foreach (AnimationClip animationClip in overrideController.) {
                //Debug.Log("Found clip from overrideController: " + animationClipPair.Key);
                overrideControllerClipList.Add(animationClipPair.Key.name);
            }
            if (currentAnimationProps == null) {
                // can't do anything since we don't have any clips
                return;
            }

            // handle single animations
            foreach (string keyName in systemAnimations.AnimationClips.Keys) {
                if (systemAnimations.AnimationClips[keyName] != null && overrideControllerClipList.Contains(systemAnimations.AnimationClips[keyName].name)) {
                    if (currentAnimationProps.AnimationClips[keyName] != null) {
                        if (overrideController[systemAnimations.AnimationClips[keyName].name] != currentAnimationProps.AnimationClips[keyName]) {
                            overrideController[systemAnimations.AnimationClips[keyName].name] = currentAnimationProps.AnimationClips[keyName];
                            currentAnimations.AnimationClips[keyName] = currentAnimationProps.AnimationClips[keyName];
                        }
                    } else {
                        if (defaultAnimationProps.AnimationClips[keyName] != null && overrideController[systemAnimations.AnimationClips[keyName].name] != defaultAnimationProps.AnimationClips[keyName]) {
                            overrideController[systemAnimations.AnimationClips[keyName].name] = defaultAnimationProps.AnimationClips[keyName];
                            currentAnimations.AnimationClips[keyName] = defaultAnimationProps.AnimationClips[keyName];
                        }
                    }
                }
            }

            foreach (string keyName in zList) {
                if (overrideControllerClipList.Contains(systemAnimations.AnimationClips[keyName].name)) {
                    if (animationSpeeds.ContainsKey(keyName) && Mathf.Abs(currentAnimations.AnimationClips[keyName].averageSpeed.z) > 0.1) {
                        // our clip has forward motion.  override the default animation motion speed of 2
                        animationSpeeds[keyName] = Mathf.Abs(currentAnimations.AnimationClips[keyName].averageSpeed.z);
                        //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                    }
                }
            }
            foreach (string keyName in xList) {
                if (overrideControllerClipList.Contains(systemAnimations.AnimationClips[keyName].name)) {
                    if (animationSpeeds.ContainsKey(keyName) && Mathf.Abs(currentAnimations.AnimationClips[keyName].averageSpeed.x) > 0.1) {
                        // our clip has forward motion.  override the default animation motion speed of 2
                        animationSpeeds[keyName] = Mathf.Abs(currentAnimations.AnimationClips[keyName].averageSpeed.x);
                        //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                    }
                }
            }
            foreach (string keyName in magnitudeList) {
                if (overrideControllerClipList.Contains(systemAnimations.AnimationClips[keyName].name)) {
                    if (animationSpeeds.ContainsKey(keyName) && Mathf.Abs(currentAnimations.AnimationClips[keyName].averageSpeed.magnitude) > 0.1) {
                        // our clip has forward motion.  override the default animation motion speed of 2
                        animationSpeeds[keyName] = Mathf.Abs(currentAnimations.AnimationClips[keyName].averageSpeed.magnitude);
                        //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                    }
                }
            }

            // handle animation lists

            // attacks
            if (currentAnimationProps.AttackClips != null && currentAnimationProps.AttackClips.Count > 0) {
                currentAnimations.AttackClips = currentAnimationProps.AttackClips;
            } else {
                currentAnimations.AttackClips = defaultAnimationProps.AttackClips;
            }

            // casting
            if (currentAnimationProps.CastClips != null && currentAnimationProps.CastClips.Count > 0) {
                currentAnimations.CastClips = currentAnimationProps.CastClips;
            } else {
                currentAnimations.CastClips = defaultAnimationProps.CastClips;
            }

            //overrideController = tempOverrideController;
            //Debug.Log(gameObject.name + ": setting override controller to: " + overrideController.name);
            //SetOverrideController(overrideController);

        }

        public int GetAnimationHitCount(AnimationClip animationClip) {
            int hitCount = 0;
            foreach (AnimationEvent animationEvent in animationClip.events) {
                if (animationEvent.functionName == "Hit") {
                    hitCount++;
                }
            }
            //Debug.Log(gameObject.name + ".CharacterAnimator.GetAnimationHitCount(): " + hitCount);
            return hitCount;
        }

        // special melee attack
        public float HandleAbility(AnimationClip animationClip, BaseAbilityProperties baseAbility, BaseCharacter targetCharacterUnit, AbilityEffectContext abilityEffectContext) {
            //Debug.Log(unitController.gameObject.name + ".CharacterAnimator.HandleAbility(" + baseAbility.DisplayName + ")");
            if (animator == null) {
                return 0f;
            }
            unitController.CharacterUnit.BaseCharacter.CharacterCombat.SwingTarget = targetCharacterUnit;

            if (systemConfigurationManager != null) {
                // override the default attack animation
                overrideController[systemConfigurationManager.SystemAnimationProfile.AnimationProps.AttackClips[0].name] = animationClip;
            }
            float animationLength = animationClip.length;

            // save animation length for weapon damage normalization
            lastAnimationLength = animationLength;

            // save animation number of hits for multi hit weapon damage normalization
            lastAnimationHits = GetAnimationHitCount(animationClip);

            //Debug.Log(gameObject.name + ".CharacterAnimator.HandleAbility(): animationlength: " + animationLength);
            currentAbilityEffectContext = abilityEffectContext;

            float speedNormalizedAnimationLength = 1f;
            if (unitController != null && unitController.CharacterUnit.BaseCharacter != null && unitController.CharacterUnit.BaseCharacter.CharacterStats != null) {
                speedNormalizedAnimationLength = (1f / (unitController.CharacterUnit.BaseCharacter.CharacterStats.GetSpeedModifiers() / 100f)) * animationLength;
            }
            SetAnimationSpeed(unitController.CharacterUnit.BaseCharacter.CharacterStats.GetSpeedModifiers() / 100f);

            // wait for the animation to play before allowing the character to attack again
            attackCoroutine = unitController.StartCoroutine(WaitForAnimation(baseAbility, speedNormalizedAnimationLength, (baseAbility as AnimatedAbilityProperties).IsAutoAttack, !(baseAbility as AnimatedAbilityProperties).IsAutoAttack, false));

            // tell the animator to play the animation
            SetAttacking(true);

            return speedNormalizedAnimationLength;
        }

        // non melee ability (spell) cast
        public void HandleCastingAbility(AnimationClip animationClip, BaseAbilityProperties baseAbility) {
            //Debug.Log(gameObject.name + ".CharacterAnimator.HandleCastingAbility()");
            if (animator == null) {
                return;
            }

            if (systemConfigurationManager != null) {
                // override the default attack animation
                //Debug.Log(gameObject.name + ".CharacterAnimator.HandleCastingAbility() animationClip: " + animationClip.name);
                foreach (AnimationClip tmpAnimationClip in overrideController.animationClips) {
                    //Debug.Log(gameObject.name + ".CharacterAnimator.HandleCastingAbility() Found clip from overrideController: " + tmpAnimationClip.name);
                }

                overrideController[systemConfigurationManager.SystemAnimationProfile.AnimationProps.CastClips[0].name] = animationClip;
                //Debug.Log(gameObject.name + ".CharacterAnimator.HandleCastingAbility() current casting clip: " + overrideController[systemConfigurationManager.MySystemAnimationProfile.MyCastClips[0].name].name);
                float animationLength = animationClip.length;
                //Debug.Log(gameObject.name + ".CharacterAnimator.HandleCastingAbility() animationlength: " + animationLength);

                // save animation length for damage normalization
                //lastAnimationLength = animationLength;

            }
            if (baseAbility.GetUnitAnimationProps(unitController.CharacterUnit.BaseCharacter).UseRootMotion == true) {
                unitController.SetUseRootMotion(true);
            } else {
                unitController.SetUseRootMotion(false);
            }


            if (baseAbility.GetAbilityCastingTime(unitController.CharacterUnit.BaseCharacter) > 0f) {
                SetCasting(true, true, (baseAbility.UseSpeedMultipliers == true ? (unitController.CharacterUnit.BaseCharacter.CharacterStats.GetSpeedModifiers() / 100f) : 1f));
            } else {
                //Debug.Log(gameObject.name + ".CharacterAnimator.HandleCastingAbility() ability was instant cast, not setting casting variable");
            }
            // this should not be necessary since we track the length of animation through the casting time
            // regular hits and animated abilities are instant attack and so need to track their downtime through animation length
            // attackCoroutine = StartCoroutine(WaitForAnimation(baseAbility, animationLength, false, false, true));
        }

        // non melee ability (spell) cast
        public void HandleAction(AnimationClip animationClip, AnimatedActionProperties animatedActionProperties) {
            //Debug.Log(gameObject.name + ".CharacterAnimator.HandleCastingAbility()");
            if (animator == null) {
                return;
            }

            if (systemConfigurationManager != null) {
                // override the default cast animation

                overrideController[systemConfigurationManager.SystemAnimationProfile.AnimationProps.CastClips[0].name] = animationClip;
                //Debug.Log(gameObject.name + ".CharacterAnimator.HandleCastingAbility() current casting clip: " + overrideController[systemConfigurationManager.MySystemAnimationProfile.MyCastClips[0].name].name);
                float animationLength = animationClip.length;
                //Debug.Log(gameObject.name + ".CharacterAnimator.HandleCastingAbility() animationlength: " + animationLength);
            }

            /*
            if (baseAbility.GetUnitAnimationProps(unitController.CharacterUnit.BaseCharacter).UseRootMotion == true) {
                unitController.SetUseRootMotion(true);
            } else {
                unitController.SetUseRootMotion(false);
            }
            */

            SetCasting(true, true);
        }

        public bool WaitingForAnimation() {
            if (attackCoroutine != null) {
                return true;
            }
            return false;
        }

        public IEnumerator WaitForAnimation(BaseAbilityProperties baseAbility, float animationLength, bool clearAutoAttack, bool clearAnimatedAttack, bool clearCasting) {
            //Debug.Log(unitController.gameObject.name + ".WaitForAnimation(" + baseAbility + ", " + animationLength + ", " + clearAutoAttack + ", " + clearAnimatedAttack + ", " + clearCasting + ")");
            float remainingTime = animationLength;
            //Debug.Log(gameObject.name + "waitforanimation remainingtime: " + remainingTime + "; MyWaitingForHits: " + unitController.BaseCharacter.MyCharacterCombat.MyWaitingForAutoAttack + "; myWaitingForAnimatedAbility: " + unitController.BaseCharacter.MyCharacterAbilityManager.MyWaitingForAnimatedAbility + "; iscasting: " + unitController.BaseCharacter.MyCharacterAbilityManager.MyIsCasting);
            while (remainingTime > 0f
                && (unitController.CharacterUnit.BaseCharacter.CharacterAbilityManager.WaitingForAnimatedAbility == true || unitController.CharacterUnit.BaseCharacter.CharacterCombat.WaitingForAutoAttack == true || unitController.CharacterUnit.BaseCharacter.CharacterAbilityManager.IsCasting)) {
                //Debug.Log(gameObject.name + ".WaitForAttackAnimation(" + animationLength + "): remainingTime: " + remainingTime + "; MyWaitingForHits: " + unitController.BaseCharacter.MyCharacterCombat.MyWaitingForAutoAttack + "; myWaitingForAnimatedAbility: " + unitController.BaseCharacter.MyCharacterAbilityManager.MyWaitingForAnimatedAbility + "; iscasting: " + unitController.BaseCharacter.MyCharacterAbilityManager.MyIsCasting + "animationSpeed: " + animator.GetFloat("AnimationSpeed"));
                //Debug.Log(gameObject.name + ".WaitForAttackAnimation(" + animationLength + "): animationSpeed: " + animator.GetFloat("AnimationSpeed"));
                yield return null;
                remainingTime -= Time.deltaTime;
            }
            //Debug.Log(gameObject.name + "Setting MyWaitingForAutoAttack to false after countdown (" + remainingTime + ") MyWaitingForAutoAttack: " + unitController.BaseCharacter.MyCharacterCombat.MyWaitingForAutoAttack + "; myWaitingForAnimatedAbility: " + unitController.BaseCharacter.MyCharacterAbilityManager.MyWaitingForAnimatedAbility + "; iscasting: " + unitController.BaseCharacter.MyCharacterAbilityManager.MyIsCasting + "animationSpeed: " + animator.GetFloat("AnimationSpeed"));
            attackCoroutine = null;
            SetAnimationSpeed(1);
            if (clearAutoAttack) {
                ClearAutoAttack();
            }
            if (clearAnimatedAttack) {
                ClearAnimatedAttack(baseAbility);
            }
            if (clearCasting) {
                ClearCasting();
            }
            //ClearAnimationBlockers();
        }

        /// <summary>
        /// return true if an auto attack was cleared
        /// </summary>
        /// <returns></returns>
        public bool ClearAutoAttack() {
            if (unitController?.CharacterUnit?.BaseCharacter != null && unitController.CharacterUnit.BaseCharacter.CharacterCombat.WaitingForAutoAttack == true) {
                if (unitController.CharacterUnit.BaseCharacter.CharacterCombat != null) {
                    unitController.CharacterUnit.BaseCharacter.CharacterCombat.SetWaitingForAutoAttack(false);
                }
                ClearAttackCommon();
                return true;
            }
            return false;
        }

        /// <summary>
        /// return true if an animated ability was cleared
        /// </summary>
        /// <param name="baseAbility"></param>
        /// <returns></returns>
        public bool ClearAnimatedAttack(BaseAbilityProperties baseAbility) {
            //Debug.Log(unitController.gameObject.name + ".CharacterAnimator.ClearAnimatedAttack()");
            if (unitController?.CharacterUnit?.BaseCharacter != null && unitController.CharacterUnit.BaseCharacter.CharacterAbilityManager.WaitingForAnimatedAbility == true) {
                unitController.CharacterUnit.BaseCharacter.CharacterAbilityManager.WaitingForAnimatedAbility = false;
                (baseAbility as AnimatedAbilityProperties).CleanupEventSubscriptions(unitController.CharacterUnit.BaseCharacter);
                ClearAttackCommon();
                return true;
            }
            return false;
        }

        private void ClearAttackCommon() {
            SetAttacking(false);
            currentAbilityEffectContext = null;
            if (unitController?.CharacterUnit?.BaseCharacter?.CharacterAbilityManager != null) {
                unitController.CharacterUnit.BaseCharacter.CharacterAbilityManager.DespawnAbilityObjects();
            }
        }

        public void CheckClearAction() {
            if (unitController.UnitActionManager.CurrentActionCoroutine != null) {
                unitController.UnitActionManager.StopAction();
            }
        }

        public void ClearAction() {
            SetCasting(false);
        }

        public bool ClearCasting(bool stoppedCast = false) {
            //Debug.Log(gameObject.name + ".CharacterAnimator.ClearCasting()");

            //unitController.BaseCharacter.MyCharacterAbilityManager.StopCasting();
            if (unitController?.CharacterUnit?.BaseCharacter != null
                && (unitController.CharacterUnit.BaseCharacter.CharacterAbilityManager.MyCurrentCastCoroutine != null || stoppedCast)) {
                if (unitController != null) {
                    unitController.SetUseRootMotion(false);
                }
                SetCasting(false);
                return true;
            }

            return false;
        }

        public void ClearAnimationBlockers(bool clearAnimatedAbility = true, bool clearAutoAttack = true, bool stoppedCast = false) {
            //Debug.Log(unitController.gameObject.name + ".UnitAnimator.ClearAnimationBlockers()");
            bool clearedAnimation = false;
            if (clearAnimatedAbility && currentAbilityEffectContext != null && currentAbilityEffectContext.baseAbility is AnimatedAbilityProperties) {
                //Debug.Log(gameObject.name + ".CharacterAnimator.ClearAnimationBlockers() WE HAVE AN ANIMATED ABILITY");
                if (ClearAnimatedAttack(currentAbilityEffectContext.baseAbility)) {
                    clearedAnimation = true;
                }
            }
            if (clearAutoAttack == true && ClearAutoAttack()) {
                clearedAnimation = true;
            }
            if (ClearCasting(stoppedCast)) {
                clearedAnimation = true;
            }
            if ((clearAnimatedAbility || clearAutoAttack) && attackCoroutine != null) {
                //Debug.Log(unitController.gameObject.name + ".UnitAnimator.ClearAnimationBlockers(): calling stopCoroutine");
                unitController.StopCoroutine(attackCoroutine);
                attackCoroutine = null;
            }
            //Debug.Log(gameObject.name + ".CharacterAnimator.ClearAnimationBlockers(): setting speed to 1");

            // clear any outstanding actions
            CheckClearAction();

            // if the unit was doing an attack or cast animation, it was likely not moving
            // that means we can reset the animation speed to normal because it shouldn't interfere with the movement animation speed
            if (clearedAnimation) {
                SetAnimationSpeed(1);
            }
        }

        private bool ParameterExists(string parameterName) {
            if (animator != null
                && animatorParameters.ContainsKey(currentOverrideController)
                && animatorParameters[currentOverrideController].Contains(parameterName)) {
                return true;
            }
            return false;
        }

        public void HandleDie(CharacterStats characterStats) {
            //Debug.Log(gameObject.name + ".CharacterAnimator.HandleDeath()");

            OnDeath();

            if (currentAbilityEffectContext != null && currentAbilityEffectContext.baseAbility is AnimatedAbilityProperties) {
                (currentAbilityEffectContext.baseAbility as AnimatedAbilityProperties).CleanupEventSubscriptions(unitController.CharacterUnit.BaseCharacter);
            }

            // add these to prevent characters from dying floating or upright
            HandleUnLevitated(false);
            HandleUnStunned(false);

            SetAnimationSpeed(1);

            SetAttacking(false, false);
            SetCasting(false, false);
            SetJumping(0);

            SetTrigger("DeathTrigger");
            SetBool("IsDead", true);
        }

        public IEnumerator WaitForResurrectionAnimation(float animationLength) {
            //Debug.Log(gameObject.name + ".WaitForAttackAnimation(" + attackLength + ")");
            float remainingTime = animationLength;
            while (remainingTime > 0f) {
                yield return null;
                remainingTime -= Time.deltaTime;
            }
            //Debug.Log(gameObject.name + "Setting waitingforhits to false after countdown down");
            SetBool("IsDead", false);
            if (unitController != null && unitController.CharacterUnit.BaseCharacter != null && unitController.CharacterUnit.BaseCharacter.CharacterStats != null) {
                unitController.CharacterUnit.BaseCharacter.CharacterStats.ReviveComplete();
            }
            OnReviveComplete();
            SetCorrectOverrideController();
            resurrectionCoroutine = null;
        }

        public void HandleReviveBegin() {
            OnStartRevive();
            SetTrigger("ReviveTrigger");
            // add 1 to account for the transition
            if (systemConfigurationManager != null) {
                float animationLength = overrideController[systemConfigurationManager.SystemAnimationProfile.AnimationProps.ReviveClip.name].length + 2;
                resurrectionCoroutine = unitController.StartCoroutine(WaitForResurrectionAnimation(animationLength));
            }
        }

        public void HandleLevitated() {
            //Debug.Log(gameObject.name + ".CharacterAnimator.HandleDeath()");
            OnStartLevitated();
            SetTrigger("LevitateTrigger");
            SetBool("Levitated", true);
        }
        public void HandleUnLevitated(bool swapAnimator = true) {
            SetBool("Levitated", false);
            OnEndLevitated(swapAnimator);
        }

        public void HandleStunned() {
            //Debug.Log(gameObject.name + ".CharacterAnimator.HandleStunned()");
            OnStartStunned();
            SetTrigger("StunTrigger");
            SetBool("Stunned", true);
        }

        public void HandleUnStunned(bool swapAnimator = true) {
            //Debug.Log(gameObject.name + ".CharacterAnimator.HandleUnStunned()");
            SetBool("Stunned", false);
            OnEndStunned(swapAnimator);
        }

        public void SetCasting(bool varValue, bool swapAnimator = true, float castingSpeed = 1f) {
            //Debug.Log(gameObject.name + ".CharacterAnimator.SetCasting(" + varValue + ")");
            if (animator == null) {
                return;
            }
            if (varValue == true) {
                OnStartCasting(swapAnimator);
            }
            SetAnimationSpeed(castingSpeed);
            if (unitController != null && unitController.CharacterUnit.BaseCharacter != null && unitController.CharacterUnit.BaseCharacter.CharacterAbilityManager != null) {
                unitController.CharacterUnit.BaseCharacter.CharacterAbilityManager.IsCasting = varValue;
            }
            if (ParameterExists("Casting")) {
                animator.SetBool("Casting", varValue);
            }

            if (varValue == true) {
                SetTrigger("CastingTrigger");
                //unitController.BaseCharacter.CharacterCombat.ResetAttackCoolDown();
            }
            if (varValue == false) {
                OnEndCasting(swapAnimator);
            }

        }

        public void SetAttacking(bool varValue, bool swapAnimator = true) {
            //Debug.Log(unitController.gameObject.name + ".SetAttacking(" + varValue + ")");
            if (animator == null) {
                return;
            }
            if (varValue == true) {
                OnStartAttacking(swapAnimator);
            }
            if (ParameterExists("Attacking")) {
                animator.SetBool("Attacking", varValue);
            }
            if (varValue == true) {
                float animationSpeed = 1f;
                if (unitController != null && unitController.CharacterUnit.BaseCharacter != null && unitController.CharacterUnit.BaseCharacter.CharacterStats != null) {
                    animationSpeed = 1f / (unitController.CharacterUnit.BaseCharacter.CharacterStats.GetSpeedModifiers() / 100f);
                }
                SetTrigger("AttackTrigger");
            }

            if (varValue == false) {
                OnEndAttacking(swapAnimator);
            }
        }

        public void SetRiding(bool varValue) {
            //Debug.Log(gameObject.name + ".SetRiding(" + varValue + ")");
            if (animator == null) {
                return;
            }
            if (varValue == true) {
                SetDefaultOverrideController();

            }
            if (ParameterExists("Riding")) {
                animator.SetBool("Riding", varValue);
            }
            if (varValue == true) {
                SetTrigger("RidingTrigger");
            }
            /*
            if (varValue == false) {
                OnEndRiding();
            }
            */
        }

        public void SetStrafing(bool varValue) {
            if (animator == null) {
                return;
            }
            SetBool("Strafing", varValue);
        }

        public void SetMoving(bool varValue) {
            if (animator == null) {
                return;
            }
            if (ParameterExists("Moving")) {
                SetBool("Moving", varValue);
            }
            if (varValue) {
                //Debug.Log(gameObject.name + ".CharacterAnimator.SetMoving()");
            }
        }

        public void SetVelocity(Vector3 varValue) {
            //Debug.Log(unitController.gameObject.name + ".CharacterAnimator.SetVelocity(" + varValue + ")");
            // receives velocity in LOCAL SPACE

            if (animator == null) {
                return;
            }

            // testing, no real need to restrict this to player.  anything should be able to rotate instead of strafe?
            //if (unitController.UnitProfile.UnitPrefabProps.RotateModel && unitController.UnitControllerMode == UnitControllerMode.Player) {
            if (unitController.UnitProfile.UnitPrefabProps.RotateModel || controlsManager.GamePadModeActive == true) {
                //Debug.Log(gameObject.name + ".CharacterAnimator.SetVelocity(" + varValue + "): rotating model");

                if (varValue == Vector3.zero) {
                    if (controlsManager.GamePadModeActive == false) {
                        animator.transform.forward = unitController.transform.forward;
                    }
                } else {
                    Vector3 normalizedVector = varValue.normalized;
                    if (normalizedVector.x != 0 || normalizedVector.z != 0) {
                        Vector3 newDirection;
                        //if (controlsManager.GamePadModeActive == true && unitController.UnitControllerMode == UnitControllerMode.Player) {
                            //newDirection = Quaternion.LookRotation(new Vector3(cameraManager.ActiveMainCamera.transform.forward.x, 0f, cameraManager.ActiveMainCamera.transform.forward.z).normalized) * new Vector3(normalizedVector.x, 0, normalizedVector.z);
                            //newDirection = cameraManager.MainCameraGameObject.transform.TransformDirection
                        //} else {
                            newDirection = unitController.transform.TransformDirection(new Vector3(normalizedVector.x, 0, normalizedVector.z));
                        //}
                        if (newDirection != Vector3.zero) {
                            //animator.transform.forward = newDirection;
                            unitController.transform.forward = newDirection;
                        }
                        //Debug.Log(gameObject.name + ".CharacterAnimator.SetVelocity(" + varValue + "): setting forward to: " + transform.TransformDirection(new Vector3(normalizedVector.x, 0, normalizedVector.z)));
                    }
                    //animator.transform.forward = varValue.normalized;
                }
                // if model is rotated, send through the magnitude so that all movement is considered in the forward direction
                SetFloat("Velocity X", 0f);
                SetFloat("Velocity Z", Mathf.Abs(varValue.magnitude));
            } else {
                // if model is not rotated, send through the normal values
                SetFloat("Velocity X", varValue.x);
                SetFloat("Velocity Z", varValue.z);
            }
            SetFloat("Velocity Y", varValue.y);

            float absXValue = Mathf.Abs(varValue.x);
            float absYValue = Mathf.Abs(varValue.y);
            float absZValue = Mathf.Abs(varValue.z);
            float absValue = Mathf.Abs(varValue.magnitude);

            float animationSpeed = 1f;
            float usedBaseAnimationSpeed = 1f;
            float multiplier = 1f;

            if (currentAnimationProps.SuppressAdjustAnimatorSpeed == false) {
                // nothing more to do if we are leaving animations at normal speed

                float usedBaseMoveForwardAnimationSpeed;
                float usedbaseWalkBackAnimationSpeed;
                float usedBaseStrafeLeftAnimationSpeed;
                float usedBaseStrafeRightAnimationSpeed;
                float usedBaseWalkStrafeBackRightAnimationSpeed;
                float usedBaseWalkStrafeBackLeftAnimationSpeed;
                float usedBaseStrafeForwardLeftAnimationSpeed;
                float usedBaseStrafeForwardRightAnimationSpeed;


                if (unitController != null
                    && unitController.CharacterUnit.BaseCharacter != null
                    && unitController.CharacterUnit.BaseCharacter.CharacterCombat != null
                    && unitController.CharacterUnit.BaseCharacter.CharacterCombat.GetInCombat() == true) {
                    // in combat
                    usedBaseMoveForwardAnimationSpeed = (absValue >= 2 ? baseCombatRunAnimationSpeed : baseCombatWalkAnimationSpeed);
                    usedbaseWalkBackAnimationSpeed = (absValue >= 2 ? baseCombatRunBackAnimationSpeed : baseCombatWalkBackAnimationSpeed);
                    usedBaseStrafeLeftAnimationSpeed = (absValue > baseCombatJogStrafeLeftAnimationSpeed ? baseCombatJogStrafeLeftAnimationSpeed : baseCombatWalkStrafeLeftAnimationSpeed);
                    usedBaseStrafeRightAnimationSpeed = (absValue > baseCombatJogStrafeRightAnimationSpeed ? baseCombatJogStrafeRightAnimationSpeed : baseCombatWalkStrafeRightAnimationSpeed);
                    usedBaseWalkStrafeBackRightAnimationSpeed = (absValue > baseCombatJogStrafeBackRightAnimationSpeed ? baseCombatJogStrafeBackRightAnimationSpeed : baseCombatWalkStrafeBackRightAnimationSpeed);
                    usedBaseWalkStrafeBackLeftAnimationSpeed = (absValue > baseCombatJogStrafeBackLeftAnimationSpeed ? baseCombatJogStrafeBackLeftAnimationSpeed : baseCombatWalkStrafeBackLeftAnimationSpeed);
                    usedBaseStrafeForwardLeftAnimationSpeed = (absValue > baseCombatJogStrafeForwardLeftAnimationSpeed ? baseCombatJogStrafeForwardLeftAnimationSpeed : baseCombatWalkStrafeForwardLeftAnimationSpeed);
                    usedBaseStrafeForwardRightAnimationSpeed = (absValue > baseCombatJogStrafeForwardRightAnimationSpeed ? baseCombatJogStrafeForwardRightAnimationSpeed : baseCombatWalkStrafeForwardRightAnimationSpeed);
                } else {
                    // out of combat
                    usedBaseMoveForwardAnimationSpeed = (absValue >= 2 ? baseRunAnimationSpeed : baseWalkAnimationSpeed);
                    usedbaseWalkBackAnimationSpeed = (absValue >= 2 ? baseRunBackAnimationSpeed : baseWalkBackAnimationSpeed);
                    usedBaseStrafeLeftAnimationSpeed = (absValue > baseJogStrafeLeftAnimationSpeed ? baseJogStrafeLeftAnimationSpeed : baseWalkStrafeLeftAnimationSpeed);
                    usedBaseStrafeRightAnimationSpeed = (absValue > baseJogStrafeRightAnimationSpeed ? baseJogStrafeRightAnimationSpeed : baseWalkStrafeRightAnimationSpeed);
                    usedBaseWalkStrafeBackRightAnimationSpeed = (absValue > baseJogStrafeBackRightAnimationSpeed ? baseJogStrafeBackRightAnimationSpeed : baseWalkStrafeBackRightAnimationSpeed);
                    usedBaseWalkStrafeBackLeftAnimationSpeed = (absValue > baseJogStrafeBackLeftAnimationSpeed ? baseJogStrafeBackLeftAnimationSpeed : baseWalkStrafeBackLeftAnimationSpeed);
                    usedBaseStrafeForwardLeftAnimationSpeed = (absValue > baseJogStrafeForwardLeftAnimationSpeed ? baseJogStrafeForwardLeftAnimationSpeed : baseWalkStrafeForwardLeftAnimationSpeed);
                    usedBaseStrafeForwardRightAnimationSpeed = (absValue > baseJogStrafeForwardRightAnimationSpeed ? baseJogStrafeForwardRightAnimationSpeed : baseWalkStrafeForwardRightAnimationSpeed);
                }

                // if the model is being rotated, animation speed is always based on the forward animation since that is what will be playing
                if (unitController.UnitProfile.UnitPrefabProps.RotateModel || (absXValue < (absZValue / 2) && varValue.z > 0)) {
                    // the new condition above should account for any animations with extra sideways movement because you have to pass 22.5 degrees in either direction to be considered to be going sideways
                    //} else if (varValue.x == 0 && varValue.z > 0) {
                    // run forward
                    //usedBaseAnimationSpeed = (absZValue <= 1 ? baseWalkAnimationSpeed : baseRunAnimationSpeed);
                    //usedBaseAnimationSpeed = (absZValue > baseWalkAnimationSpeed ? baseRunAnimationSpeed : baseWalkAnimationSpeed);
                    // since jog forward animation is hardcoded to 2 or more in animator, switched condition below to match
                    usedBaseAnimationSpeed = usedBaseMoveForwardAnimationSpeed;
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetVelocity(" + varValue + "): run: " + baseRunAnimationSpeed + "; walk: " + baseWalkAnimationSpeed + "; used: " + usedBaseAnimationSpeed);
                    //multiplier = varValue.z;
                    multiplier = (absValue / usedBaseAnimationSpeed);
                } else if (absXValue < (absZValue / 2) && varValue.z < 0) {
                    //} else if (varValue.x == 0 && varValue.z < 0) {
                    // run back
                    usedBaseAnimationSpeed = usedbaseWalkBackAnimationSpeed;
                    multiplier = (absValue / usedBaseAnimationSpeed);
                } else if (varValue.x > 0 && absZValue < (absXValue / 2)) {
                    // strafe right
                    usedBaseAnimationSpeed = usedBaseStrafeRightAnimationSpeed;
                    multiplier = (absValue / usedBaseAnimationSpeed);
                } else if (varValue.x < 0 && absZValue < (absXValue / 2)) {
                    // strafe left
                    usedBaseAnimationSpeed = usedBaseStrafeLeftAnimationSpeed;
                    multiplier = (absValue / usedBaseAnimationSpeed);
                } else if (varValue.x > 0 && varValue.z < 0) {
                    // strafe back right
                    usedBaseAnimationSpeed = usedBaseWalkStrafeBackRightAnimationSpeed;
                    multiplier = (absValue / usedBaseAnimationSpeed);
                } else if (varValue.x < 0 && varValue.z < 0) {
                    // strafe back left
                    usedBaseAnimationSpeed = usedBaseWalkStrafeBackLeftAnimationSpeed;
                    multiplier = (absValue / usedBaseAnimationSpeed);
                } else if (varValue.x < 0 && varValue.z > 0) {
                    // strafe forward left
                    usedBaseAnimationSpeed = usedBaseStrafeForwardLeftAnimationSpeed;
                    multiplier = (absValue / usedBaseAnimationSpeed);
                } else if (varValue.x > 0 && varValue.z > 0) {
                    // strafe forward right
                    usedBaseAnimationSpeed = usedBaseStrafeForwardRightAnimationSpeed;
                    multiplier = (absValue / usedBaseAnimationSpeed);
                }
                //Debug.Log(unitController.gameObject.name + ".CharacterAnimator.SetVelocity(" + varValue + "): used: " + usedBaseAnimationSpeed + "; walk: " + baseWalkAnimationSpeed + "; run: " + baseRunAnimationSpeed + "; multiplier: " + multiplier);

                // if velocity is zero, the unit is stopping and the default animation speed of 1 should be used
                // if the velocity is greater than zero, and animation speed sync is enabled, use the correct multiplier calculated above
                if (varValue.magnitude != 0f && systemConfigurationManager.SyncMovementAnimationSpeed == true) {
                    animationSpeed = multiplier;
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetVelocityZ(" + varValue + "): animationSpeed: " + animationSpeed);
                }
            }

            //Debug.Log(unitController.gameObject.name + ".CharacterAnimator.SetVelocity(" + varValue + "): animationSpeed: " + animationSpeed);
            SetAnimationSpeed(animationSpeed);
        }

        public void SetAnimationSpeed(float animationSpeed) {
            //Debug.Log(unitController.gameObject.name + ".UnitAnimation.SetAnimationSpeed(" + animationSpeed + ")");
            if (animator != null && ParameterExists("AnimationSpeed")) {
                SetFloat("AnimationSpeed", animationSpeed);
            }
        }

        public void SetVelocityX(float varValue) {
            if (animator == null) {
                return;
            }
            SetFloat("Velocity X", varValue);
        }

        public void SetVelocityY(float varValue) {
            if (animator == null) {
                return;
            }
            SetFloat("Velocity Y", varValue);
        }

        public void SetTurnVelocity(float varValue) {
            if (animator == null) {
                return;
            }
            //Debug.Log(gameObject.name + ".CharacterAnimator.SetTurnVelocity(" + varValue + ")");
            SetFloat("TurnVelocity", varValue);
        }

        public int GetInt(string varName) {
            if (animator != null) {
                if (ParameterExists(varName)) {
                    return animator.GetInteger(varName);
                }
            }
            return 0;
        }

        public bool GetBool(string varName) {
            if (animator != null) {
                if (ParameterExists(varName)) {
                    return animator.GetBool(varName);
                }
            }
            return false;
        }

        public void SetBool(string varName, bool varValue) {

            if (animator != null) {
                if (ParameterExists(varName)) {
                    animator.SetBool(varName, varValue);
                }
            }
        }

        public void SetFloat(string varName, float varValue) {
            //Debug.Log(gameObject.name + ".CharacterAnimator.SetFloat(" + varName + ", " + varValue + ")");
            if (animator != null && ParameterExists(varName)) {
                animator.SetFloat(varName, varValue);
            }
        }

        public void SetInteger(string varName, int varValue) {
            //Debug.Log(gameObject.name + ".CharacterAnimator.SetFloat(" + varName + ", " + varValue + ")");
            if (animator != null && ParameterExists(varName)) {
                animator.SetInteger(varName, varValue);
            }
        }

        public bool IsInAir() {
            if (GetBool("falling") == true || GetInt("Jumping") != 0) {
                return true;
            }
            return false;
        }

        public void SetFalling(bool varValue) {
            if (animator == null) {
                return;
            }
            SetBool("Falling", varValue);
            if (varValue == true) {
                SetTrigger("FallTrigger");
            }
        }

        public void SetJumping(int varValue) {
            if (animator == null) {
                return;
            }
            SetInteger("Jumping", varValue);
            if (varValue == 0) {
                SetFalling(false);
            }
        }

        public void SetTrigger(string varName) {
            //Debug.Log(gameObject.name + ".CharacterAnimator.SetTrigger(" + varName + ")");
            if (animator != null && ParameterExists(varName)) {
                animator.ResetTrigger(varName);
                animator.SetTrigger(varName);
            }
        }

        public AnimatorClipInfo[] GetCurrentAnimatorClipInfo(int layerIndex) {
            return animator.GetCurrentAnimatorClipInfo(layerIndex);
        }

        public void PerformEquipmentChange(Equipment newItem) {
            //Debug.Log(unitController.gameObject.name + ".CharacterAnimator.PerformEquipmentChange(" + newItem.DisplayName + ")");
            HandleEquipmentChanged(newItem, null, -1);
        }

        public void HandleEquipmentChanged(Equipment newItem, Equipment oldItem, int slotIndex = -1) {
            //Debug.Log(unitController.gameObject.name + ".UnitAnimator.HandleEquipmentChanged(" + (newItem == null ? "null" : newItem.DisplayName) + ", " + (oldItem == null ? "null" : oldItem.DisplayName) + ")");
            if (animator == null) {
                // this unit isn't animated
                return;
            }

            // Animate grip for weapon when an item is added or removed from hand
            if (newItem != null
                && newItem is Weapon
                && (newItem as Weapon).AnimationProfile != null
                && (newItem as Weapon).AnimationProfile.AnimationProps != null) {
                //Debug.Log(gameObject.name + ".CharacterAnimator.PerformEquipmentChange: we are animating the weapon");
                //animator.SetLayerWeight(1, 1);
                //Debug.Log(unitController.gameObject.name + ".UnitAnimator.HandleEquipmentChanged() animation profile: " + (newItem as Weapon).AnimationProfile.DisplayName);
                SetAnimationProfileOverride((newItem as Weapon).AnimationProfile.AnimationProps);
            } else if (newItem == null
                && oldItem != null
                && oldItem is Weapon
                && (oldItem as Weapon).AnimationProfile != null
                && (oldItem as Weapon).AnimationProfile.AnimationProps != null) {
                //animator.SetLayerWeight(1, 0);
                //Debug.Log(gameObject.name + ".CharacterAnimator.PerformEquipmentChange: resetting the animation profile");
                ResetAnimationProfile();
            }

            /*
            // Animate grip for weapon when a shield is added or removed from hand
            if (newItem != null && newItem.equipSlot == EquipmentSlot.OffHand) {
                //Debug.Log("we are animating the shield");
                //animator.SetLayerWeight(2, 1);
            } else if (newItem == null && oldItem != null && oldItem.equipSlot == EquipmentSlot.OffHand) {
                //animator.SetLayerWeight(2, 0);
            }
            */
        }

        public void ResetSettings() {
            //Debug.Log(unitController.gameObject.name + ".UnitAnimator.ResetSettings()");

            // return settings to how they were when the unit was initialized in case a third party animator is used and this unit was in preview mode
            if (systemConfigurationManager.UseThirdPartyMovementControl == true) {
                if (thirdPartyAnimatorController != null) {
                    animator.runtimeAnimatorController = thirdPartyAnimatorController;
                }
            }
        }


    }

}