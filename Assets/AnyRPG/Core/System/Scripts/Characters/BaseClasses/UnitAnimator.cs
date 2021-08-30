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
            currentAnimations = UnityEngine.Object.Instantiate(systemConfigurationManager.SystemAnimationProfile).AnimationProps;
            animatorController = systemConfigurationManager.DefaultAnimatorController;
            defaultAnimationProps = systemConfigurationManager.DefaultAnimationProfile.AnimationProps;
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

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

            if (overrideControllerClipList.Contains(systemAnimations.MoveForwardClip.name)) {
                if (currentAnimationProps.MoveForwardClip != null) {
                    if (overrideController[systemAnimations.MoveForwardClip.name] != currentAnimationProps.MoveForwardClip) {
                        overrideController[systemAnimations.MoveForwardClip.name] = currentAnimationProps.MoveForwardClip;
                        currentAnimations.MoveForwardClip = currentAnimationProps.MoveForwardClip;
                    }
                } else {
                    if (defaultAnimationProps.MoveForwardClip != null && overrideController[systemAnimations.MoveForwardClip.name] != defaultAnimationProps.MoveForwardClip) {
                        overrideController[systemAnimations.MoveForwardClip.name] = defaultAnimationProps.MoveForwardClip;
                        currentAnimations.MoveForwardClip = defaultAnimationProps.MoveForwardClip;
                    }
                }
                //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): MyMoveForwardClip." + currentAttackAnimationProfile.MyMoveForwardClip.averageSpeed);
                if (currentAnimations.MoveForwardClip.averageSpeed.z > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 1
                    baseWalkAnimationSpeed = currentAnimations.MoveForwardClip.averageSpeed.z;
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation walk speed: " + baseWalkAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.CombatMoveForwardClip.name)) {
                if (currentAnimationProps.CombatMoveForwardClip != null) {
                    if (overrideController[systemAnimations.CombatMoveForwardClip.name] != currentAnimationProps.CombatMoveForwardClip) {
                        overrideController[systemAnimations.CombatMoveForwardClip.name] = currentAnimationProps.CombatMoveForwardClip;
                        currentAnimations.CombatMoveForwardClip = currentAnimationProps.CombatMoveForwardClip;
                    }
                } else {
                    if (defaultAnimationProps.CombatMoveForwardClip != null && overrideController[systemAnimations.CombatMoveForwardClip.name] != defaultAnimationProps.CombatMoveForwardClip) {
                        overrideController[systemAnimations.CombatMoveForwardClip.name] = defaultAnimationProps.CombatMoveForwardClip;
                        currentAnimations.CombatMoveForwardClip = defaultAnimationProps.CombatMoveForwardClip;
                    }
                }
                //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): MyCombatMoveForwardClip.averageSpeed: " + currentAttackAnimationProfile.MyMoveForwardClip.averageSpeed + "; apparentSpeed: " + currentAttackAnimationProfile.MyCombatMoveForwardClip.apparentSpeed + "; averageAngularSpeed: " + currentAttackAnimationProfile.MyCombatMoveForwardClip.averageAngularSpeed);
                if (currentAnimations.CombatMoveForwardClip.averageSpeed.z > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 1
                    baseCombatWalkAnimationSpeed = currentAnimations.CombatMoveForwardClip.averageSpeed.z;
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation walk speed: " + baseWalkAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.MoveForwardFastClip.name)) {
                if (currentAnimationProps.MoveForwardFastClip != null) {
                    if (overrideController[systemAnimations.MoveForwardFastClip.name] != currentAnimationProps.MoveForwardFastClip) {
                        overrideController[systemAnimations.MoveForwardFastClip.name] = currentAnimationProps.MoveForwardFastClip;
                        currentAnimations.MoveForwardFastClip = currentAnimationProps.MoveForwardFastClip;
                    }
                } else {
                    if (defaultAnimationProps.MoveForwardFastClip != null && overrideController[systemAnimations.MoveForwardFastClip.name] != defaultAnimationProps.MoveForwardFastClip) {
                        overrideController[systemAnimations.MoveForwardFastClip.name] = defaultAnimationProps.MoveForwardFastClip;
                        currentAnimations.MoveForwardFastClip = defaultAnimationProps.MoveForwardFastClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.MoveForwardFastClip.averageSpeed.z) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseRunAnimationSpeed = Mathf.Abs(currentAnimations.MoveForwardFastClip.averageSpeed.z);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.CombatMoveForwardFastClip.name)) {
                if (currentAnimationProps.CombatMoveForwardFastClip != null) {
                    if (overrideController[systemAnimations.CombatMoveForwardFastClip.name] != currentAnimationProps.CombatMoveForwardFastClip) {
                        overrideController[systemAnimations.CombatMoveForwardFastClip.name] = currentAnimationProps.CombatMoveForwardFastClip;
                        currentAnimations.CombatMoveForwardFastClip = currentAnimationProps.CombatMoveForwardFastClip;
                    }
                } else {
                    if (defaultAnimationProps.CombatMoveForwardFastClip != null && overrideController[systemAnimations.CombatMoveForwardFastClip.name] != defaultAnimationProps.CombatMoveForwardFastClip) {
                        overrideController[systemAnimations.CombatMoveForwardFastClip.name] = defaultAnimationProps.CombatMoveForwardFastClip;
                        currentAnimations.CombatMoveForwardFastClip = defaultAnimationProps.CombatMoveForwardFastClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.CombatMoveForwardFastClip.averageSpeed.z) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseCombatRunAnimationSpeed = Mathf.Abs(currentAnimations.CombatMoveForwardFastClip.averageSpeed.z);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.MoveBackClip.name)) {
                if (currentAnimationProps.MoveBackClip != null) {
                    if (overrideController[systemAnimations.MoveBackClip.name] != currentAnimationProps.MoveBackClip) {
                        overrideController[systemAnimations.MoveBackClip.name] = currentAnimationProps.MoveBackClip;
                        currentAnimations.MoveBackClip = currentAnimationProps.MoveBackClip;
                    }
                } else {
                    if (defaultAnimationProps.MoveBackClip != null && overrideController[systemAnimations.MoveBackClip.name] != defaultAnimationProps.MoveBackClip) {
                        overrideController[systemAnimations.MoveBackClip.name] = defaultAnimationProps.MoveBackClip;
                        currentAnimations.MoveBackClip = defaultAnimationProps.MoveBackClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.MoveBackClip.averageSpeed.z) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseWalkBackAnimationSpeed = Mathf.Abs(currentAnimations.MoveBackClip.averageSpeed.z);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.CombatMoveBackClip.name)) {
                if (currentAnimationProps.CombatMoveBackClip != null) {
                    if (overrideController[systemAnimations.CombatMoveBackClip.name] != currentAnimationProps.CombatMoveBackClip) {
                        overrideController[systemAnimations.CombatMoveBackClip.name] = currentAnimationProps.CombatMoveBackClip;
                        currentAnimations.CombatMoveBackClip = currentAnimationProps.CombatMoveBackClip;
                    }
                } else {
                    if (defaultAnimationProps.CombatMoveBackClip != null && overrideController[systemAnimations.CombatMoveBackClip.name] != defaultAnimationProps.CombatMoveBackClip) {
                        overrideController[systemAnimations.CombatMoveBackClip.name] = defaultAnimationProps.CombatMoveBackClip;
                        currentAnimations.CombatMoveBackClip = defaultAnimationProps.CombatMoveBackClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.CombatMoveBackClip.averageSpeed.z) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseCombatWalkBackAnimationSpeed = Mathf.Abs(currentAnimations.CombatMoveBackClip.averageSpeed.z);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.MoveBackFastClip.name)) {
                if (currentAnimationProps.MoveBackFastClip != null) {
                    if (overrideController[systemAnimations.MoveBackFastClip.name] != currentAnimationProps.MoveBackFastClip) {
                        overrideController[systemAnimations.MoveBackFastClip.name] = currentAnimationProps.MoveBackFastClip;
                        currentAnimations.MoveBackFastClip = currentAnimationProps.MoveBackFastClip;
                    }
                } else {
                    if (defaultAnimationProps.MoveBackFastClip != null && overrideController[systemAnimations.MoveBackFastClip.name] != defaultAnimationProps.MoveBackFastClip) {
                        overrideController[systemAnimations.MoveBackFastClip.name] = defaultAnimationProps.MoveBackFastClip;
                        currentAnimations.MoveBackFastClip = defaultAnimationProps.MoveBackFastClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.MoveBackFastClip.averageSpeed.z) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseRunBackAnimationSpeed = Mathf.Abs(currentAnimations.MoveBackFastClip.averageSpeed.z);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.CombatMoveBackFastClip.name)) {
                if (currentAnimationProps.CombatMoveBackFastClip != null) {
                    if (overrideController[systemAnimations.CombatMoveBackFastClip.name] != currentAnimationProps.CombatMoveBackFastClip) {
                        overrideController[systemAnimations.CombatMoveBackFastClip.name] = currentAnimationProps.CombatMoveBackFastClip;
                        currentAnimations.CombatMoveBackFastClip = currentAnimationProps.CombatMoveBackFastClip;
                    }
                } else {
                    if (defaultAnimationProps.CombatMoveBackFastClip != null && overrideController[systemAnimations.CombatMoveBackFastClip.name] != defaultAnimationProps.CombatMoveBackFastClip) {
                        overrideController[systemAnimations.CombatMoveBackFastClip.name] = defaultAnimationProps.CombatMoveBackFastClip;
                        currentAnimations.CombatMoveBackFastClip = defaultAnimationProps.CombatMoveBackFastClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.CombatMoveBackFastClip.averageSpeed.z) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseCombatRunBackAnimationSpeed = Mathf.Abs(currentAnimations.CombatMoveBackFastClip.averageSpeed.z);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.JumpClip.name)) {
                if (currentAnimationProps.JumpClip != null) {
                    if (overrideController[systemAnimations.JumpClip.name] != currentAnimationProps.JumpClip) {
                        overrideController[systemAnimations.JumpClip.name] = currentAnimationProps.JumpClip;
                        currentAnimations.JumpClip = currentAnimationProps.JumpClip;
                    }
                } else {
                    if (defaultAnimationProps.JumpClip != null && overrideController[systemAnimations.JumpClip.name] != defaultAnimationProps.JumpClip) {
                        overrideController[systemAnimations.JumpClip.name] = defaultAnimationProps.JumpClip;
                        currentAnimations.JumpClip = defaultAnimationProps.JumpClip;
                    }
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.CombatJumpClip.name)) {
                if (currentAnimationProps.CombatJumpClip != null) {
                    if (overrideController[systemAnimations.CombatJumpClip.name] != currentAnimationProps.CombatJumpClip) {
                        overrideController[systemAnimations.CombatJumpClip.name] = currentAnimationProps.CombatJumpClip;
                        currentAnimations.CombatJumpClip = currentAnimationProps.CombatJumpClip;
                    }
                } else {
                    if (defaultAnimationProps.CombatJumpClip != null && overrideController[systemAnimations.CombatJumpClip.name] != defaultAnimationProps.CombatJumpClip) {
                        overrideController[systemAnimations.CombatJumpClip.name] = defaultAnimationProps.CombatJumpClip;
                        currentAnimations.CombatJumpClip = defaultAnimationProps.CombatJumpClip;
                    }
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.IdleClip.name)) {
                if (currentAnimationProps.IdleClip != null) {
                    if (overrideController[systemAnimations.IdleClip.name] != currentAnimationProps.IdleClip) {
                        overrideController[systemAnimations.IdleClip.name] = currentAnimationProps.IdleClip;
                        currentAnimations.IdleClip = currentAnimationProps.IdleClip;
                    }
                } else {
                    if (defaultAnimationProps.IdleClip != null && overrideController[systemAnimations.IdleClip.name] != defaultAnimationProps.IdleClip) {
                        overrideController[systemAnimations.IdleClip.name] = defaultAnimationProps.IdleClip;
                        currentAnimations.IdleClip = defaultAnimationProps.IdleClip;
                    }
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.CombatIdleClip.name)) {
                if (currentAnimationProps.CombatIdleClip != null) {
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): combat idle clip is not null");
                    if (overrideController[systemAnimations.CombatIdleClip.name] != currentAnimationProps.CombatIdleClip) {
                        overrideController[systemAnimations.CombatIdleClip.name] = currentAnimationProps.CombatIdleClip;
                        currentAnimations.CombatIdleClip = currentAnimationProps.CombatIdleClip;
                    }
                } else {
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): combat idle clip is null");
                    if (defaultAnimationProps.CombatIdleClip != null && overrideController[systemAnimations.CombatIdleClip.name] != defaultAnimationProps.CombatIdleClip) {
                        overrideController[systemAnimations.CombatIdleClip.name] = defaultAnimationProps.CombatIdleClip;
                        currentAnimations.CombatIdleClip = defaultAnimationProps.CombatIdleClip;
                    }
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.LandClip.name)) {
                if (currentAnimationProps.LandClip != null) {
                    if (overrideController[systemAnimations.LandClip.name] != currentAnimationProps.LandClip) {
                        overrideController[systemAnimations.LandClip.name] = currentAnimationProps.LandClip;
                        currentAnimations.LandClip = currentAnimationProps.LandClip;
                    }
                } else {
                    if (defaultAnimationProps.LandClip != null && overrideController[systemAnimations.LandClip.name] != defaultAnimationProps.LandClip) {
                        overrideController[systemAnimations.LandClip.name] = defaultAnimationProps.LandClip;
                        currentAnimations.LandClip = defaultAnimationProps.LandClip;
                    }
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.CombatLandClip.name)) {
                if (currentAnimationProps.CombatLandClip != null) {
                    if (overrideController[systemAnimations.CombatLandClip.name] != currentAnimationProps.CombatLandClip) {
                        overrideController[systemAnimations.CombatLandClip.name] = currentAnimationProps.CombatLandClip;
                        currentAnimations.CombatLandClip = currentAnimationProps.CombatLandClip;
                    }
                } else {
                    if (defaultAnimationProps.CombatLandClip != null && overrideController[systemAnimations.CombatLandClip.name] != defaultAnimationProps.CombatLandClip) {
                        overrideController[systemAnimations.CombatLandClip.name] = defaultAnimationProps.CombatLandClip;
                        currentAnimations.CombatLandClip = defaultAnimationProps.CombatLandClip;
                    }
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.FallClip.name)) {
                if (currentAnimationProps.FallClip != null) {
                    if (overrideController[systemAnimations.FallClip.name] != currentAnimationProps.FallClip) {
                        overrideController[systemAnimations.FallClip.name] = currentAnimationProps.FallClip;
                        currentAnimations.FallClip = currentAnimationProps.FallClip;
                    }
                } else {
                    if (defaultAnimationProps.FallClip != null && overrideController[systemAnimations.FallClip.name] != defaultAnimationProps.FallClip) {
                        overrideController[systemAnimations.FallClip.name] = defaultAnimationProps.FallClip;
                        currentAnimations.FallClip = defaultAnimationProps.FallClip;
                    }
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.CombatFallClip.name)) {
                if (currentAnimationProps.CombatFallClip != null) {
                    if (overrideController[systemAnimations.CombatFallClip.name] != currentAnimationProps.CombatFallClip) {
                        overrideController[systemAnimations.CombatFallClip.name] = currentAnimationProps.CombatFallClip;
                        currentAnimations.CombatFallClip = currentAnimationProps.CombatFallClip;
                    }
                } else {
                    if (defaultAnimationProps.CombatFallClip != null && overrideController[systemAnimations.CombatFallClip.name] != defaultAnimationProps.CombatFallClip) {
                        overrideController[systemAnimations.CombatFallClip.name] = defaultAnimationProps.CombatFallClip;
                        currentAnimations.CombatFallClip = defaultAnimationProps.CombatFallClip;
                    }
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.StrafeLeftClip.name)) {
                if (currentAnimationProps.StrafeLeftClip != null) {
                    if (overrideController[systemAnimations.StrafeLeftClip.name] != currentAnimationProps.StrafeLeftClip) {
                        overrideController[systemAnimations.StrafeLeftClip.name] = currentAnimationProps.StrafeLeftClip;
                        currentAnimations.StrafeLeftClip = currentAnimationProps.StrafeLeftClip;
                    }
                } else {
                    if (defaultAnimationProps.StrafeLeftClip != null && overrideController[systemAnimations.StrafeLeftClip.name] != defaultAnimationProps.StrafeLeftClip) {
                        overrideController[systemAnimations.StrafeLeftClip.name] = defaultAnimationProps.StrafeLeftClip;
                        currentAnimations.StrafeLeftClip = defaultAnimationProps.StrafeLeftClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.StrafeLeftClip.averageSpeed.x) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseWalkStrafeLeftAnimationSpeed = Mathf.Abs(currentAnimations.StrafeLeftClip.averageSpeed.x);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.JogStrafeLeftClip.name)) {
                if (currentAnimationProps.JogStrafeLeftClip != null) {
                    if (overrideController[systemAnimations.JogStrafeLeftClip.name] != currentAnimationProps.JogStrafeLeftClip) {
                        overrideController[systemAnimations.JogStrafeLeftClip.name] = currentAnimationProps.JogStrafeLeftClip;
                        currentAnimations.JogStrafeLeftClip = currentAnimationProps.JogStrafeLeftClip;
                    }
                } else {
                    if (defaultAnimationProps.JogStrafeLeftClip != null && overrideController[systemAnimations.JogStrafeLeftClip.name] != defaultAnimationProps.JogStrafeLeftClip) {
                        overrideController[systemAnimations.JogStrafeLeftClip.name] = defaultAnimationProps.JogStrafeLeftClip;
                        currentAnimations.JogStrafeLeftClip = defaultAnimationProps.JogStrafeLeftClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.JogStrafeLeftClip.averageSpeed.x) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseJogStrafeLeftAnimationSpeed = Mathf.Abs(currentAnimations.JogStrafeLeftClip.averageSpeed.x);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.StrafeRightClip.name)) {
                if (currentAnimationProps.StrafeRightClip != null) {
                    if (overrideController[systemAnimations.StrafeRightClip.name] != currentAnimationProps.StrafeRightClip) {
                        overrideController[systemAnimations.StrafeRightClip.name] = currentAnimationProps.StrafeRightClip;
                        currentAnimations.StrafeRightClip = currentAnimationProps.StrafeRightClip;
                    }
                } else {
                    if (defaultAnimationProps.StrafeRightClip != null && overrideController[systemAnimations.StrafeRightClip.name] != defaultAnimationProps.StrafeRightClip) {
                        overrideController[systemAnimations.StrafeRightClip.name] = defaultAnimationProps.StrafeRightClip;
                        currentAnimations.StrafeRightClip = defaultAnimationProps.StrafeRightClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.StrafeRightClip.averageSpeed.x) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseWalkStrafeRightAnimationSpeed = Mathf.Abs(currentAnimations.StrafeRightClip.averageSpeed.x);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.JogStrafeRightClip.name)) {
                if (currentAnimationProps.JogStrafeRightClip != null) {
                    if (overrideController[systemAnimations.JogStrafeRightClip.name] != currentAnimationProps.JogStrafeRightClip) {
                        overrideController[systemAnimations.JogStrafeRightClip.name] = currentAnimationProps.JogStrafeRightClip;
                        currentAnimations.JogStrafeRightClip = currentAnimationProps.JogStrafeRightClip;
                    }
                } else {
                    if (defaultAnimationProps.JogStrafeRightClip != null && overrideController[systemAnimations.JogStrafeRightClip.name] != defaultAnimationProps.JogStrafeRightClip) {
                        overrideController[systemAnimations.JogStrafeRightClip.name] = defaultAnimationProps.JogStrafeRightClip;
                        currentAnimations.JogStrafeRightClip = defaultAnimationProps.JogStrafeRightClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.JogStrafeRightClip.averageSpeed.x) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseJogStrafeRightAnimationSpeed = Mathf.Abs(currentAnimations.JogStrafeRightClip.averageSpeed.x);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.StrafeForwardRightClip.name)) {
                if (currentAnimationProps.StrafeForwardRightClip != null) {
                    if (overrideController[systemAnimations.StrafeForwardRightClip.name] != currentAnimationProps.StrafeForwardRightClip) {
                        overrideController[systemAnimations.StrafeForwardRightClip.name] = currentAnimationProps.StrafeForwardRightClip;
                        currentAnimations.StrafeForwardRightClip = currentAnimationProps.StrafeForwardRightClip;
                    }
                } else {
                    if (defaultAnimationProps.StrafeForwardRightClip != null && overrideController[systemAnimations.StrafeForwardRightClip.name] != defaultAnimationProps.StrafeForwardRightClip) {
                        overrideController[systemAnimations.StrafeForwardRightClip.name] = defaultAnimationProps.StrafeForwardRightClip;
                        currentAnimations.StrafeForwardRightClip = defaultAnimationProps.StrafeForwardRightClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.StrafeForwardRightClip.averageSpeed.magnitude) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseWalkStrafeForwardRightAnimationSpeed = Mathf.Abs(currentAnimations.StrafeForwardRightClip.averageSpeed.magnitude);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.JogStrafeForwardRightClip.name)) {
                if (currentAnimationProps.JogStrafeForwardRightClip != null) {
                    if (overrideController[systemAnimations.JogStrafeForwardRightClip.name] != currentAnimationProps.JogStrafeForwardRightClip) {
                        overrideController[systemAnimations.JogStrafeForwardRightClip.name] = currentAnimationProps.JogStrafeForwardRightClip;
                        currentAnimations.JogStrafeForwardRightClip = currentAnimationProps.JogStrafeForwardRightClip;
                    }
                } else {
                    if (defaultAnimationProps.JogStrafeForwardRightClip != null && overrideController[systemAnimations.JogStrafeForwardRightClip.name] != defaultAnimationProps.JogStrafeForwardRightClip) {
                        overrideController[systemAnimations.JogStrafeForwardRightClip.name] = defaultAnimationProps.JogStrafeForwardRightClip;
                        currentAnimations.JogStrafeForwardRightClip = defaultAnimationProps.JogStrafeForwardRightClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.JogStrafeForwardRightClip.averageSpeed.magnitude) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseJogStrafeForwardRightAnimationSpeed = Mathf.Abs(currentAnimations.JogStrafeForwardRightClip.averageSpeed.magnitude);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.StrafeForwardLeftClip.name)) {
                if (currentAnimationProps.StrafeForwardLeftClip != null) {
                    if (overrideController[systemAnimations.StrafeForwardLeftClip.name] != currentAnimationProps.StrafeForwardLeftClip) {
                        overrideController[systemAnimations.StrafeForwardLeftClip.name] = currentAnimationProps.StrafeForwardLeftClip;
                        currentAnimations.StrafeForwardLeftClip = currentAnimationProps.StrafeForwardLeftClip;
                    }
                } else {
                    if (defaultAnimationProps.StrafeForwardLeftClip != null && overrideController[systemAnimations.StrafeForwardLeftClip.name] != defaultAnimationProps.StrafeForwardLeftClip) {
                        overrideController[systemAnimations.StrafeForwardLeftClip.name] = defaultAnimationProps.StrafeForwardLeftClip;
                        currentAnimations.StrafeForwardLeftClip = defaultAnimationProps.StrafeForwardLeftClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.StrafeForwardLeftClip.averageSpeed.magnitude) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseWalkStrafeForwardLeftAnimationSpeed = Mathf.Abs(currentAnimations.StrafeForwardLeftClip.averageSpeed.magnitude);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.JogStrafeForwardLeftClip.name)) {
                if (currentAnimationProps.JogStrafeForwardLeftClip != null) {
                    if (overrideController[systemAnimations.JogStrafeForwardLeftClip.name] != currentAnimationProps.JogStrafeForwardLeftClip) {
                        overrideController[systemAnimations.JogStrafeForwardLeftClip.name] = currentAnimationProps.JogStrafeForwardLeftClip;
                        currentAnimations.JogStrafeForwardLeftClip = currentAnimationProps.JogStrafeForwardLeftClip;
                    }
                } else {
                    if (defaultAnimationProps.JogStrafeForwardLeftClip != null && overrideController[systemAnimations.JogStrafeForwardLeftClip.name] != defaultAnimationProps.JogStrafeForwardLeftClip) {
                        overrideController[systemAnimations.JogStrafeForwardLeftClip.name] = defaultAnimationProps.JogStrafeForwardLeftClip;
                        currentAnimations.JogStrafeForwardLeftClip = defaultAnimationProps.JogStrafeForwardLeftClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.JogStrafeForwardLeftClip.averageSpeed.magnitude) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseJogStrafeForwardLeftAnimationSpeed = Mathf.Abs(currentAnimations.JogStrafeForwardLeftClip.averageSpeed.magnitude);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.StrafeBackLeftClip.name)) {
                if (currentAnimationProps.StrafeBackLeftClip != null) {
                    if (overrideController[systemAnimations.StrafeBackLeftClip.name] != currentAnimationProps.StrafeBackLeftClip) {
                        overrideController[systemAnimations.StrafeBackLeftClip.name] = currentAnimationProps.StrafeBackLeftClip;
                        currentAnimations.StrafeBackLeftClip = currentAnimationProps.StrafeBackLeftClip;
                    }
                } else {
                    if (defaultAnimationProps.StrafeBackLeftClip != null && overrideController[systemAnimations.StrafeBackLeftClip.name] != defaultAnimationProps.StrafeBackLeftClip) {
                        overrideController[systemAnimations.StrafeBackLeftClip.name] = defaultAnimationProps.StrafeBackLeftClip;
                        currentAnimations.StrafeBackLeftClip = defaultAnimationProps.StrafeBackLeftClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.StrafeBackLeftClip.averageSpeed.magnitude) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseWalkStrafeBackLeftAnimationSpeed = Mathf.Abs(currentAnimations.StrafeBackLeftClip.averageSpeed.magnitude);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.JogStrafeBackLeftClip.name)) {
                if (currentAnimationProps.JogStrafeBackLeftClip != null) {
                    if (overrideController[systemAnimations.JogStrafeBackLeftClip.name] != currentAnimationProps.JogStrafeBackLeftClip) {
                        overrideController[systemAnimations.JogStrafeBackLeftClip.name] = currentAnimationProps.JogStrafeBackLeftClip;
                        currentAnimations.JogStrafeBackLeftClip = currentAnimationProps.JogStrafeBackLeftClip;
                    }
                } else {
                    if (defaultAnimationProps.JogStrafeBackLeftClip != null && overrideController[systemAnimations.JogStrafeBackLeftClip.name] != defaultAnimationProps.JogStrafeBackLeftClip) {
                        overrideController[systemAnimations.JogStrafeBackLeftClip.name] = defaultAnimationProps.JogStrafeBackLeftClip;
                        currentAnimations.JogStrafeBackLeftClip = defaultAnimationProps.JogStrafeBackLeftClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.JogStrafeBackLeftClip.averageSpeed.magnitude) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseJogStrafeBackLeftAnimationSpeed = Mathf.Abs(currentAnimations.JogStrafeBackLeftClip.averageSpeed.magnitude);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.StrafeBackRightClip.name)) {
                if (currentAnimationProps.StrafeBackRightClip != null) {
                    if (overrideController[systemAnimations.StrafeBackRightClip.name] != currentAnimationProps.StrafeBackRightClip) {
                        overrideController[systemAnimations.StrafeBackRightClip.name] = currentAnimationProps.StrafeBackRightClip;
                        currentAnimations.StrafeBackRightClip = currentAnimationProps.StrafeBackRightClip;
                    }
                } else {
                    if (defaultAnimationProps.StrafeBackRightClip != null && overrideController[systemAnimations.StrafeBackRightClip.name] != defaultAnimationProps.StrafeBackRightClip) {
                        overrideController[systemAnimations.StrafeBackRightClip.name] = defaultAnimationProps.StrafeBackRightClip;
                        currentAnimations.StrafeBackRightClip = defaultAnimationProps.StrafeBackRightClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.StrafeBackRightClip.averageSpeed.magnitude) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseWalkStrafeBackRightAnimationSpeed = Mathf.Abs(currentAnimations.StrafeBackRightClip.averageSpeed.magnitude);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.JogStrafeBackRightClip.name)) {
                if (currentAnimationProps.JogStrafeBackRightClip != null) {
                    if (overrideController[systemAnimations.JogStrafeBackRightClip.name] != currentAnimationProps.JogStrafeBackRightClip) {
                        overrideController[systemAnimations.JogStrafeBackRightClip.name] = currentAnimationProps.JogStrafeBackRightClip;
                        currentAnimations.JogStrafeBackRightClip = currentAnimationProps.JogStrafeBackRightClip;
                    }
                } else {
                    if (defaultAnimationProps.JogStrafeBackRightClip != null && overrideController[systemAnimations.JogStrafeBackRightClip.name] != defaultAnimationProps.JogStrafeBackRightClip) {
                        overrideController[systemAnimations.JogStrafeBackRightClip.name] = defaultAnimationProps.JogStrafeBackRightClip;
                        currentAnimations.JogStrafeBackRightClip = defaultAnimationProps.JogStrafeBackRightClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.JogStrafeBackRightClip.averageSpeed.magnitude) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseJogStrafeBackRightAnimationSpeed = Mathf.Abs(currentAnimations.JogStrafeBackRightClip.averageSpeed.magnitude);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.CombatStrafeLeftClip.name)) {
                if (currentAnimationProps.CombatStrafeLeftClip != null) {
                    if (overrideController[systemAnimations.CombatStrafeLeftClip.name] != currentAnimationProps.CombatStrafeLeftClip) {
                        overrideController[systemAnimations.CombatStrafeLeftClip.name] = currentAnimationProps.CombatStrafeLeftClip;
                        currentAnimations.CombatStrafeLeftClip = currentAnimationProps.CombatStrafeLeftClip;
                    }
                } else {
                    if (defaultAnimationProps.CombatStrafeLeftClip != null && overrideController[systemAnimations.CombatStrafeLeftClip.name] != defaultAnimationProps.CombatStrafeLeftClip) {
                        overrideController[systemAnimations.CombatStrafeLeftClip.name] = defaultAnimationProps.CombatStrafeLeftClip;
                        currentAnimations.CombatStrafeLeftClip = defaultAnimationProps.CombatStrafeLeftClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.CombatStrafeLeftClip.averageSpeed.x) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseCombatWalkStrafeLeftAnimationSpeed = Mathf.Abs(currentAnimations.CombatStrafeLeftClip.averageSpeed.x);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.CombatJogStrafeLeftClip.name)) {
                if (currentAnimationProps.CombatJogStrafeLeftClip != null) {
                    if (overrideController[systemAnimations.CombatJogStrafeLeftClip.name] != currentAnimationProps.CombatJogStrafeLeftClip) {
                        overrideController[systemAnimations.CombatJogStrafeLeftClip.name] = currentAnimationProps.CombatJogStrafeLeftClip;
                        currentAnimations.CombatJogStrafeLeftClip = currentAnimationProps.CombatJogStrafeLeftClip;
                    }
                } else {
                    if (defaultAnimationProps.CombatJogStrafeLeftClip != null && overrideController[systemAnimations.CombatJogStrafeLeftClip.name] != defaultAnimationProps.CombatJogStrafeLeftClip) {
                        overrideController[systemAnimations.CombatJogStrafeLeftClip.name] = defaultAnimationProps.CombatJogStrafeLeftClip;
                        currentAnimations.CombatJogStrafeLeftClip = defaultAnimationProps.CombatJogStrafeLeftClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.CombatJogStrafeLeftClip.averageSpeed.x) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseCombatJogStrafeLeftAnimationSpeed = Mathf.Abs(currentAnimations.CombatJogStrafeLeftClip.averageSpeed.x);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.CombatStrafeRightClip.name)) {
                if (currentAnimationProps.CombatStrafeRightClip != null) {
                    if (overrideController[systemAnimations.CombatStrafeRightClip.name] != currentAnimationProps.CombatStrafeRightClip) {
                        overrideController[systemAnimations.CombatStrafeRightClip.name] = currentAnimationProps.CombatStrafeRightClip;
                        currentAnimations.CombatStrafeRightClip = currentAnimationProps.CombatStrafeRightClip;
                    }
                } else {
                    if (defaultAnimationProps.CombatStrafeRightClip != null && overrideController[systemAnimations.CombatStrafeRightClip.name] != defaultAnimationProps.CombatStrafeRightClip) {
                        overrideController[systemAnimations.CombatStrafeRightClip.name] = defaultAnimationProps.CombatStrafeRightClip;
                        currentAnimations.CombatStrafeRightClip = defaultAnimationProps.CombatStrafeRightClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.CombatStrafeRightClip.averageSpeed.x) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseCombatWalkStrafeRightAnimationSpeed = Mathf.Abs(currentAnimations.CombatStrafeRightClip.averageSpeed.x);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.CombatJogStrafeRightClip.name)) {
                if (currentAnimationProps.CombatJogStrafeRightClip != null) {
                    if (overrideController[systemAnimations.CombatJogStrafeRightClip.name] != currentAnimationProps.CombatJogStrafeRightClip) {
                        overrideController[systemAnimations.CombatJogStrafeRightClip.name] = currentAnimationProps.CombatJogStrafeRightClip;
                        currentAnimations.CombatJogStrafeRightClip = currentAnimationProps.CombatJogStrafeRightClip;
                    }
                } else {
                    if (defaultAnimationProps.CombatJogStrafeRightClip != null && overrideController[systemAnimations.CombatJogStrafeRightClip.name] != defaultAnimationProps.CombatJogStrafeRightClip) {
                        overrideController[systemAnimations.CombatJogStrafeRightClip.name] = defaultAnimationProps.CombatJogStrafeRightClip;
                        currentAnimations.CombatJogStrafeRightClip = defaultAnimationProps.CombatJogStrafeRightClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.CombatJogStrafeRightClip.averageSpeed.x) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseCombatJogStrafeRightAnimationSpeed = Mathf.Abs(currentAnimations.CombatJogStrafeRightClip.averageSpeed.x);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.CombatStrafeForwardRightClip.name)) {
                if (currentAnimationProps.CombatStrafeForwardRightClip != null) {
                    if (overrideController[systemAnimations.CombatStrafeForwardRightClip.name] != currentAnimationProps.CombatStrafeForwardRightClip) {
                        overrideController[systemAnimations.CombatStrafeForwardRightClip.name] = currentAnimationProps.CombatStrafeForwardRightClip;
                        currentAnimations.CombatStrafeForwardRightClip = currentAnimationProps.CombatStrafeForwardRightClip;
                    }
                } else {
                    if (defaultAnimationProps.CombatStrafeForwardRightClip != null && overrideController[systemAnimations.CombatStrafeForwardRightClip.name] != defaultAnimationProps.CombatStrafeForwardRightClip) {
                        overrideController[systemAnimations.CombatStrafeForwardRightClip.name] = defaultAnimationProps.CombatStrafeForwardRightClip;
                        currentAnimations.CombatStrafeForwardRightClip = defaultAnimationProps.CombatStrafeForwardRightClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.CombatStrafeForwardRightClip.averageSpeed.magnitude) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseCombatWalkStrafeForwardRightAnimationSpeed = Mathf.Abs(currentAnimations.CombatStrafeForwardRightClip.averageSpeed.magnitude);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.CombatJogStrafeForwardRightClip.name)) {
                if (currentAnimationProps.CombatJogStrafeForwardRightClip != null) {
                    if (overrideController[systemAnimations.CombatJogStrafeForwardRightClip.name] != currentAnimationProps.CombatJogStrafeForwardRightClip) {
                        overrideController[systemAnimations.CombatJogStrafeForwardRightClip.name] = currentAnimationProps.CombatJogStrafeForwardRightClip;
                        currentAnimations.CombatJogStrafeForwardRightClip = currentAnimationProps.CombatJogStrafeForwardRightClip;
                    }
                } else {
                    if (defaultAnimationProps.CombatJogStrafeForwardRightClip != null && overrideController[systemAnimations.CombatJogStrafeForwardRightClip.name] != defaultAnimationProps.CombatJogStrafeForwardRightClip) {
                        overrideController[systemAnimations.CombatJogStrafeForwardRightClip.name] = defaultAnimationProps.CombatJogStrafeForwardRightClip;
                        currentAnimations.CombatJogStrafeForwardRightClip = defaultAnimationProps.CombatJogStrafeForwardRightClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.CombatJogStrafeForwardRightClip.averageSpeed.magnitude) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseCombatJogStrafeForwardRightAnimationSpeed = Mathf.Abs(currentAnimations.CombatJogStrafeForwardRightClip.averageSpeed.magnitude);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.CombatStrafeForwardLeftClip.name)) {
                if (currentAnimationProps.CombatStrafeForwardLeftClip != null) {
                    if (overrideController[systemAnimations.CombatStrafeForwardLeftClip.name] != currentAnimationProps.CombatStrafeForwardLeftClip) {
                        overrideController[systemAnimations.CombatStrafeForwardLeftClip.name] = currentAnimationProps.CombatStrafeForwardLeftClip;
                        currentAnimations.CombatStrafeForwardLeftClip = currentAnimationProps.CombatStrafeForwardLeftClip;
                    }
                } else {
                    if (defaultAnimationProps.CombatStrafeForwardLeftClip != null && overrideController[systemAnimations.CombatStrafeForwardLeftClip.name] != defaultAnimationProps.CombatStrafeForwardLeftClip) {
                        overrideController[systemAnimations.CombatStrafeForwardLeftClip.name] = defaultAnimationProps.CombatStrafeForwardLeftClip;
                        currentAnimations.CombatStrafeForwardLeftClip = defaultAnimationProps.CombatStrafeForwardLeftClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.CombatStrafeForwardLeftClip.averageSpeed.magnitude) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseCombatWalkStrafeForwardLeftAnimationSpeed = Mathf.Abs(currentAnimations.CombatStrafeForwardLeftClip.averageSpeed.magnitude);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.CombatJogStrafeForwardLeftClip.name)) {
                if (currentAnimationProps.CombatJogStrafeForwardLeftClip != null) {
                    if (overrideController[systemAnimations.CombatJogStrafeForwardLeftClip.name] != currentAnimationProps.CombatJogStrafeForwardLeftClip) {
                        overrideController[systemAnimations.CombatJogStrafeForwardLeftClip.name] = currentAnimationProps.CombatJogStrafeForwardLeftClip;
                        currentAnimations.CombatJogStrafeForwardLeftClip = currentAnimationProps.CombatJogStrafeForwardLeftClip;
                    }
                } else {
                    if (defaultAnimationProps.CombatJogStrafeForwardLeftClip != null && overrideController[systemAnimations.CombatJogStrafeForwardLeftClip.name] != defaultAnimationProps.CombatJogStrafeForwardLeftClip) {
                        overrideController[systemAnimations.CombatJogStrafeForwardLeftClip.name] = defaultAnimationProps.CombatJogStrafeForwardLeftClip;
                        currentAnimations.CombatJogStrafeForwardLeftClip = defaultAnimationProps.CombatJogStrafeForwardLeftClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.CombatJogStrafeForwardLeftClip.averageSpeed.magnitude) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseCombatJogStrafeForwardLeftAnimationSpeed = Mathf.Abs(currentAnimations.CombatJogStrafeForwardLeftClip.averageSpeed.magnitude);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.CombatStrafeBackLeftClip.name)) {
                if (currentAnimationProps.CombatStrafeBackLeftClip != null) {
                    if (overrideController[systemAnimations.CombatStrafeBackLeftClip.name] != currentAnimationProps.CombatStrafeBackLeftClip) {
                        overrideController[systemAnimations.CombatStrafeBackLeftClip.name] = currentAnimationProps.CombatStrafeBackLeftClip;
                        currentAnimations.CombatStrafeBackLeftClip = currentAnimationProps.CombatStrafeBackLeftClip;
                    }
                } else {
                    if (defaultAnimationProps.CombatStrafeBackLeftClip != null && overrideController[systemAnimations.CombatStrafeBackLeftClip.name] != defaultAnimationProps.CombatStrafeBackLeftClip) {
                        overrideController[systemAnimations.CombatStrafeBackLeftClip.name] = defaultAnimationProps.CombatStrafeBackLeftClip;
                        currentAnimations.CombatStrafeBackLeftClip = defaultAnimationProps.CombatStrafeBackLeftClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.CombatStrafeBackLeftClip.averageSpeed.magnitude) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseCombatWalkStrafeBackLeftAnimationSpeed = Mathf.Abs(currentAnimations.CombatStrafeBackLeftClip.averageSpeed.magnitude);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.CombatJogStrafeBackLeftClip.name)) {
                if (currentAnimationProps.CombatJogStrafeBackLeftClip != null) {
                    if (overrideController[systemAnimations.CombatJogStrafeBackLeftClip.name] != currentAnimationProps.CombatJogStrafeBackLeftClip) {
                        overrideController[systemAnimations.CombatJogStrafeBackLeftClip.name] = currentAnimationProps.CombatJogStrafeBackLeftClip;
                        currentAnimations.CombatJogStrafeBackLeftClip = currentAnimationProps.CombatJogStrafeBackLeftClip;
                    }
                } else {
                    if (defaultAnimationProps.CombatJogStrafeBackLeftClip != null && overrideController[systemAnimations.CombatJogStrafeBackLeftClip.name] != defaultAnimationProps.CombatJogStrafeBackLeftClip) {
                        overrideController[systemAnimations.CombatJogStrafeBackLeftClip.name] = defaultAnimationProps.CombatJogStrafeBackLeftClip;
                        currentAnimations.CombatJogStrafeBackLeftClip = defaultAnimationProps.CombatJogStrafeBackLeftClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.CombatJogStrafeBackLeftClip.averageSpeed.magnitude) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseCombatJogStrafeBackLeftAnimationSpeed = Mathf.Abs(currentAnimations.CombatJogStrafeBackLeftClip.averageSpeed.magnitude);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.CombatStrafeBackRightClip.name)) {
                if (currentAnimationProps.CombatStrafeBackRightClip != null) {
                    if (overrideController[systemAnimations.CombatStrafeBackRightClip.name] != currentAnimationProps.CombatStrafeBackRightClip) {
                        overrideController[systemAnimations.CombatStrafeBackRightClip.name] = currentAnimationProps.CombatStrafeBackRightClip;
                        currentAnimations.CombatStrafeBackRightClip = currentAnimationProps.CombatStrafeBackRightClip;
                    }
                } else {
                    if (defaultAnimationProps.CombatStrafeBackRightClip != null && overrideController[systemAnimations.CombatStrafeBackRightClip.name] != defaultAnimationProps.CombatStrafeBackRightClip) {
                        overrideController[systemAnimations.CombatStrafeBackRightClip.name] = defaultAnimationProps.CombatStrafeBackRightClip;
                        currentAnimations.CombatStrafeBackRightClip = defaultAnimationProps.CombatStrafeBackRightClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.CombatStrafeBackRightClip.averageSpeed.magnitude) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseCombatWalkStrafeBackRightAnimationSpeed = Mathf.Abs(currentAnimations.CombatStrafeBackRightClip.averageSpeed.magnitude);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.CombatJogStrafeBackRightClip.name)) {
                if (currentAnimationProps.CombatJogStrafeBackRightClip != null) {
                    if (overrideController[systemAnimations.CombatJogStrafeBackRightClip.name] != currentAnimationProps.CombatJogStrafeBackRightClip) {
                        overrideController[systemAnimations.CombatJogStrafeBackRightClip.name] = currentAnimationProps.CombatJogStrafeBackRightClip;
                        currentAnimations.CombatJogStrafeBackRightClip = currentAnimationProps.CombatJogStrafeBackRightClip;
                    }
                } else {
                    if (defaultAnimationProps.CombatJogStrafeBackRightClip != null && overrideController[systemAnimations.CombatJogStrafeBackRightClip.name] != defaultAnimationProps.CombatJogStrafeBackRightClip) {
                        overrideController[systemAnimations.CombatJogStrafeBackRightClip.name] = defaultAnimationProps.CombatJogStrafeBackRightClip;
                        currentAnimations.CombatJogStrafeBackRightClip = defaultAnimationProps.CombatJogStrafeBackRightClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.CombatJogStrafeBackRightClip.averageSpeed.magnitude) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseCombatJogStrafeBackRightAnimationSpeed = Mathf.Abs(currentAnimations.CombatJogStrafeBackRightClip.averageSpeed.magnitude);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): Death is not null.");
            if (overrideControllerClipList.Contains(systemAnimations.DeathClip.name)) {
                if (currentAnimationProps.DeathClip != null) {
                    if (overrideController[systemAnimations.DeathClip.name] != currentAnimationProps.DeathClip) {
                        overrideController[systemAnimations.DeathClip.name] = currentAnimationProps.DeathClip;
                        currentAnimations.DeathClip = currentAnimationProps.DeathClip;
                    }
                } else {
                    if (defaultAnimationProps.DeathClip != null && overrideController[systemAnimations.DeathClip.name] != defaultAnimationProps.DeathClip) {
                        overrideController[systemAnimations.DeathClip.name] = defaultAnimationProps.DeathClip;
                        currentAnimations.DeathClip = defaultAnimationProps.DeathClip;
                    }
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.StunnedClip.name)) {
                if (currentAnimationProps.StunnedClip != null) {
                    if (overrideController[systemAnimations.StunnedClip.name] != currentAnimationProps.StunnedClip) {
                        overrideController[systemAnimations.StunnedClip.name] = currentAnimationProps.StunnedClip;
                        currentAnimations.StunnedClip = currentAnimationProps.StunnedClip;
                    }
                } else {
                    if (defaultAnimationProps.StunnedClip != null && overrideController[systemAnimations.StunnedClip.name] != defaultAnimationProps.StunnedClip) {
                        overrideController[systemAnimations.StunnedClip.name] = defaultAnimationProps.StunnedClip;
                        currentAnimations.StunnedClip = defaultAnimationProps.StunnedClip;
                    }
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.LevitatedClip.name)) {
                if (currentAnimationProps.LevitatedClip != null) {
                    if (overrideController[systemAnimations.LevitatedClip.name] != currentAnimationProps.LevitatedClip) {
                        overrideController[systemAnimations.LevitatedClip.name] = currentAnimationProps.LevitatedClip;
                        currentAnimations.LevitatedClip = currentAnimationProps.LevitatedClip;
                    }
                } else {
                    if (defaultAnimationProps.LevitatedClip != null && overrideController[systemAnimations.LevitatedClip.name] != defaultAnimationProps.LevitatedClip) {
                        overrideController[systemAnimations.LevitatedClip.name] = defaultAnimationProps.LevitatedClip;
                        currentAnimations.LevitatedClip = defaultAnimationProps.LevitatedClip;
                    }
                }
            }

            //Debug.Log("CharacterAnimator.SetAnimationClipOverrides() Current Animation Profile Contains Revive Clip");
            if (overrideControllerClipList.Contains(systemAnimations.ReviveClip.name)) {
                if (currentAnimationProps.ReviveClip != null) {
                    if (overrideController[systemAnimations.ReviveClip.name] != currentAnimationProps.ReviveClip) {
                        overrideController[systemAnimations.ReviveClip.name] = currentAnimationProps.ReviveClip;
                        currentAnimations.ReviveClip = currentAnimationProps.ReviveClip;
                    }
                } else {
                    if (defaultAnimationProps.ReviveClip != null && overrideController[systemAnimations.ReviveClip.name] != defaultAnimationProps.ReviveClip) {
                        overrideController[systemAnimations.ReviveClip.name] = defaultAnimationProps.ReviveClip;
                        currentAnimations.ReviveClip = defaultAnimationProps.ReviveClip;
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
        public float HandleAbility(AnimationClip animationClip, BaseAbility baseAbility, BaseCharacter targetCharacterUnit, AbilityEffectContext abilityEffectContext) {
            //Debug.Log(unitController.gameObject.name + ".CharacterAnimator.HandleAbility(" + baseAbility.DisplayName + ")");
            if (animator == null) {
                //Debug.Log(gameObject.name + ".CharacterAnimator.HandleAbility(" + baseAbility.MyName + ") ANIMATOR IS NULL!!!");
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
                //Debug.Log(gameObject.name + ".CharacterAnimator.HandleAbility(" + baseAbility.MyName + "): speedNormalizedAnimationLength: " + speedNormalizedAnimationLength + "; length: " + animationLength);
            }
            SetAnimationSpeed(unitController.CharacterUnit.BaseCharacter.CharacterStats.GetSpeedModifiers() / 100f);

            // wait for the animation to play before allowing the character to attack again
            attackCoroutine = unitController.StartCoroutine(WaitForAnimation(baseAbility, speedNormalizedAnimationLength, (baseAbility as AnimatedAbility).IsAutoAttack, !(baseAbility as AnimatedAbility).IsAutoAttack, false));

            // tell the animator to play the animation
            SetAttacking(true);

            return speedNormalizedAnimationLength;
        }

        // non melee ability (spell) cast
        public void HandleCastingAbility(AnimationClip animationClip, BaseAbility baseAbility) {
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

        public bool WaitingForAnimation() {
            if (attackCoroutine != null) {
                return true;
            }
            return false;
        }

        public IEnumerator WaitForAnimation(BaseAbility baseAbility, float animationLength, bool clearAutoAttack, bool clearAnimatedAttack, bool clearCasting) {
            //Debug.Log(unitController.gameObject.name + ".WaitForAnimation(" + baseAbility + ", " + animationLength + ", " + clearAutoAttack + ", " + clearAnimatedAttack + ", " + clearCasting + ")");
            float remainingTime = animationLength;
            //Debug.Log(gameObject.name + "waitforanimation remainingtime: " + remainingTime + "; MyWaitingForHits: " + unitController.MyBaseCharacter.MyCharacterCombat.MyWaitingForAutoAttack + "; myWaitingForAnimatedAbility: " + unitController.MyBaseCharacter.MyCharacterAbilityManager.MyWaitingForAnimatedAbility + "; iscasting: " + unitController.MyBaseCharacter.MyCharacterAbilityManager.MyIsCasting);
            while (remainingTime > 0f
                && (unitController.CharacterUnit.BaseCharacter.CharacterAbilityManager.WaitingForAnimatedAbility == true || unitController.CharacterUnit.BaseCharacter.CharacterCombat.WaitingForAutoAttack == true || unitController.CharacterUnit.BaseCharacter.CharacterAbilityManager.IsCasting)) {
                //Debug.Log(gameObject.name + ".WaitForAttackAnimation(" + animationLength + "): remainingTime: " + remainingTime + "; MyWaitingForHits: " + unitController.MyBaseCharacter.MyCharacterCombat.MyWaitingForAutoAttack + "; myWaitingForAnimatedAbility: " + unitController.MyBaseCharacter.MyCharacterAbilityManager.MyWaitingForAnimatedAbility + "; iscasting: " + unitController.MyBaseCharacter.MyCharacterAbilityManager.MyIsCasting + "animationSpeed: " + animator.GetFloat("AnimationSpeed"));
                //Debug.Log(gameObject.name + ".WaitForAttackAnimation(" + animationLength + "): animationSpeed: " + animator.GetFloat("AnimationSpeed"));
                yield return null;
                remainingTime -= Time.deltaTime;
            }
            //Debug.Log(gameObject.name + "Setting MyWaitingForAutoAttack to false after countdown (" + remainingTime + ") MyWaitingForAutoAttack: " + unitController.MyBaseCharacter.MyCharacterCombat.MyWaitingForAutoAttack + "; myWaitingForAnimatedAbility: " + unitController.MyBaseCharacter.MyCharacterAbilityManager.MyWaitingForAnimatedAbility + "; iscasting: " + unitController.MyBaseCharacter.MyCharacterAbilityManager.MyIsCasting + "animationSpeed: " + animator.GetFloat("AnimationSpeed"));
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
        public bool ClearAnimatedAttack(BaseAbility baseAbility) {
            //Debug.Log(unitController.gameObject.name + ".CharacterAnimator.ClearAnimatedAttack()");
            if (unitController?.CharacterUnit?.BaseCharacter != null && unitController.CharacterUnit.BaseCharacter.CharacterAbilityManager.WaitingForAnimatedAbility == true) {
                unitController.CharacterUnit.BaseCharacter.CharacterAbilityManager.WaitingForAnimatedAbility = false;
                (baseAbility as AnimatedAbility).CleanupEventSubscriptions(unitController.CharacterUnit.BaseCharacter);
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

        public bool ClearCasting(bool stoppedCast = false) {
            //Debug.Log(gameObject.name + ".CharacterAnimator.ClearCasting()");

            //unitController.MyBaseCharacter.MyCharacterAbilityManager.StopCasting();
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
            if (clearAnimatedAbility && currentAbilityEffectContext != null && currentAbilityEffectContext.baseAbility is AnimatedAbility) {
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

            if (currentAbilityEffectContext != null && currentAbilityEffectContext.baseAbility is AnimatedAbility) {
                (currentAbilityEffectContext.baseAbility as AnimatedAbility).CleanupEventSubscriptions(unitController.CharacterUnit.BaseCharacter);
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
                //unitController.MyBaseCharacter.CharacterCombat.ResetAttackCoolDown();
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
            //Debug.Log(gameObject.name + ".CharacterAnimator.SetVelocity(" + varValue + ", " + rotateModel + ")");
            // receives velocity in LOCAL SPACE

            if (animator == null) {
                return;
            }

            // testing, no real need to restrict this to player.  anything should be able to rotate instead of strafe?
            //if (unitController.UnitProfile.UnitPrefabProps.RotateModel && unitController.UnitControllerMode == UnitControllerMode.Player) {
            if (unitController.UnitProfile.UnitPrefabProps.RotateModel) {
                //Debug.Log(gameObject.name + ".CharacterAnimator.SetVelocity(" + varValue + "): rotating model");

                if (varValue == Vector3.zero) {
                    animator.transform.forward = unitController.transform.forward;
                } else {
                    Vector3 normalizedVector = varValue.normalized;
                    if (normalizedVector.x != 0 || normalizedVector.z != 0) {
                        Vector3 newDirection = unitController.transform.TransformDirection(new Vector3(normalizedVector.x, 0, normalizedVector.z));
                        if (newDirection != Vector3.zero) {
                            animator.transform.forward = newDirection;
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