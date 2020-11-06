using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class UnitAnimator {

        // events
        public event System.Action OnReviveComplete = delegate { };
        public event System.Action<bool> OnStartCasting = delegate { };
        public event System.Action<bool> OnEndCasting = delegate { };
        public event System.Action<bool> OnStartAttacking = delegate { };
        public event System.Action<bool> OnEndAttacking = delegate { };
        public event System.Action OnStartRiding = delegate { };
        public event System.Action OnEndRiding = delegate { };
        public event System.Action OnStartLevitated = delegate { };
        public event System.Action<bool> OnEndLevitated = delegate { };
        public event System.Action OnStartStunned = delegate { };
        public event System.Action<bool> OnEndStunned = delegate { };
        public event System.Action OnStartRevive = delegate { };
        public event System.Action OnDeath = delegate { };

        // components
        private Animator animator = null;

        // unarmed default animation profile
        private AnimationProfile defaultAnimationProfile = null;

        // current animation profile with any overrides from weapons etc
        private AnimationProfile currentAnimationProfile = null;

        private RuntimeAnimatorController animatorController = null;
        private AnimatorOverrideController overrideController = null;

        private RuntimeAnimatorController thirdPartyAnimatorController = null;
        private AnimatorOverrideController thirdPartyOverrideController = null;

        private UnitController unitController = null;

        protected bool initialized = false;

        // keep track of the number of hits in the last animation for normalizing damage to animation length
        private float lastAnimationLength = 0f;

        // keep track of the number of hits in the last animation for normalizing multi-hit abilities
        private int lastAnimationHits = 0;

        private AnimationProfile currentAnimations = null;
        private AnimationProfile systemAnimations = null;

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

        protected bool componentReferencesInitialized = false;

        public bool applyRootMotion { get => (animator != null ? animator.applyRootMotion : false); }
        public Animator MyAnimator { get => animator; }
        public AbilityEffectContext MyCurrentAbilityEffectContext { get => currentAbilityEffectContext; set => currentAbilityEffectContext = value; }
        public RuntimeAnimatorController MyAnimatorController {
            get => animatorController;
            set => animatorController = value;
        }
        public float LastAnimationLength { get => lastAnimationLength; set => lastAnimationLength = value; }
        public int LastAnimationHits { get => lastAnimationHits; set => lastAnimationHits = value; }

        public UnitAnimator(UnitController unitController) {
            this.unitController = unitController;
            systemAnimations = SystemConfigurationManager.MyInstance.MySystemAnimationProfile;
            currentAnimations = UnityEngine.Object.Instantiate(SystemConfigurationManager.MyInstance.MySystemAnimationProfile);
            animatorController = SystemConfigurationManager.MyInstance.MyDefaultAnimatorController;
            defaultAnimationProfile = SystemConfigurationManager.MyInstance.MyDefaultAnimationProfile;

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
            //Debug.Log(gameObject.name + ": CharacterAnimator.InitializeAnimator()");
            if (initialized) {
                return;
            }
            if (animator == null) {
                //Debug.Log(gameObject.name + ": CharacterAnimator.InitializeAnimator(): Could not find animator in children");
                return;
            }
            if (SystemConfigurationManager.MyInstance.MyUseThirdPartyMovementControl == true) {
                if (thirdPartyAnimatorController == null) {
                    thirdPartyAnimatorController = animator.runtimeAnimatorController;
                }
                if (thirdPartyAnimatorController != null) {
                    thirdPartyOverrideController = new AnimatorOverrideController(thirdPartyAnimatorController);
                }
            }

            if (overrideController == null) {
                //Debug.Log(gameObject.name + ": override controller was null. creating new override controller");
                if (animatorController == null) {
                    //Debug.Log(gameObject.name + ".CharacterAnimator.InitializeAnimator() animatorController is null");
                } else {
                    overrideController = new AnimatorOverrideController(animatorController);
                    //SetOverrideController(overrideController);
                    SetCorrectOverrideController(false);
                }
            }
            //Debug.Log(gameObject.name + ": setting override controller to: " + overrideController.name);

            // before finishing initialization, search for a valid unit profile and try to get an animation profile from it
            if (unitController.UnitProfile != null && unitController.UnitProfile.UnitPrefabProfile != null && unitController.UnitProfile.UnitPrefabProfile.AnimationProfile != null) {
                defaultAnimationProfile = unitController.UnitProfile.UnitPrefabProfile.AnimationProfile;
            }
            SetAnimationProfileOverride(defaultAnimationProfile);

            initialized = true;
        }

        public void SetCorrectOverrideController(bool runUpdate = true) {
            //Debug.Log(gameObject.name + ".CharacterAnimator.SetCorrectOverrideController()");
            if (unitController.UnitControllerMode == UnitControllerMode.Player && SystemConfigurationManager.MyInstance.MyUseThirdPartyMovementControl == true) {
                SetOverrideController(thirdPartyOverrideController, runUpdate);
                return;
            }

            // AI or no third party movement control case
            SetOverrideController(overrideController, runUpdate);
        }

        public void SetDefaultOverrideController(bool runUpdate = true) {
            //Debug.Log(gameObject.name + ".CharacterAnimator.SetDefaultOverrideController()");
            SetOverrideController(overrideController, runUpdate);
        }


        public void SetOverrideController(AnimatorOverrideController animatorOverrideController, bool runUpdate = true) {

            if (animator.runtimeAnimatorController != animatorOverrideController && animatorOverrideController != null) {
                animator.runtimeAnimatorController = animatorOverrideController;

                // set animator on UMA if one exists
                if (unitController.DynamicCharacterAvatar != null) {
                    unitController.DynamicCharacterAvatar.raceAnimationControllers.defaultAnimationController = animatorOverrideController;
                }
                //animator.updateMode = AnimatorUpdateMode.
                if (runUpdate) {
                    animator.Update(0f);
                }
            }
        }

        public void SetAnimationProfileOverride(AnimationProfile animationProfile) {
            //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationProfileOverride(" + (animationProfile == null ? "null" : animationProfile.MyProfileName) + ")");
            //AnimationProfile oldAnimationProfile = currentAnimationProfile;
            currentAnimationProfile = animationProfile;
            SetAnimationClipOverrides();
        }

        public void ResetAnimationProfile() {
            //Debug.Log(gameObject.name + ".CharacterAnimator.ResetAnimationProfile()");
            //AnimationProfile oldAnimationProfile = currentAnimationProfile;
            currentAnimationProfile = defaultAnimationProfile;
            // change back to the original animations
            SetAnimationClipOverrides();
        }

        protected void SetAnimationClipOverrides() {
            //Debug.Log(gameObject.name + ": CharacterAnimator.SetAnimationClipOverrides()");
            if (SystemConfigurationManager.MyInstance == null) {
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
            if (currentAnimationProfile == null) {
                // can't do anything since we don't have any clips
                return;
            }

            if (overrideControllerClipList.Contains(systemAnimations.AnimationProps.MoveForwardClip.name)) {
                if (currentAnimationProfile.AnimationProps.MoveForwardClip != null) {
                    if (overrideController[systemAnimations.AnimationProps.MoveForwardClip.name] != currentAnimationProfile.AnimationProps.MoveForwardClip) {
                        overrideController[systemAnimations.AnimationProps.MoveForwardClip.name] = currentAnimationProfile.AnimationProps.MoveForwardClip;
                        currentAnimations.AnimationProps.MoveForwardClip = currentAnimationProfile.AnimationProps.MoveForwardClip;
                    }
                } else {
                    if (defaultAnimationProfile.AnimationProps.MoveForwardClip != null && overrideController[systemAnimations.AnimationProps.MoveForwardClip.name] != defaultAnimationProfile.AnimationProps.MoveForwardClip) {
                        overrideController[systemAnimations.AnimationProps.MoveForwardClip.name] = defaultAnimationProfile.AnimationProps.MoveForwardClip;
                        currentAnimations.AnimationProps.MoveForwardClip = defaultAnimationProfile.AnimationProps.MoveForwardClip;
                    }
                }
                //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): MyMoveForwardClip." + currentAttackAnimationProfile.MyMoveForwardClip.averageSpeed);
                if (currentAnimations.AnimationProps.MoveForwardClip.averageSpeed.z > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 1
                    baseWalkAnimationSpeed = currentAnimations.AnimationProps.MoveForwardClip.averageSpeed.z;
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation walk speed: " + baseWalkAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.AnimationProps.CombatMoveForwardClip.name)) {
                if (currentAnimationProfile.AnimationProps.CombatMoveForwardClip != null) {
                    if (overrideController[systemAnimations.AnimationProps.CombatMoveForwardClip.name] != currentAnimationProfile.AnimationProps.CombatMoveForwardClip) {
                        overrideController[systemAnimations.AnimationProps.CombatMoveForwardClip.name] = currentAnimationProfile.AnimationProps.CombatMoveForwardClip;
                        currentAnimations.AnimationProps.CombatMoveForwardClip = currentAnimationProfile.AnimationProps.CombatMoveForwardClip;
                    }
                } else {
                    if (defaultAnimationProfile.AnimationProps.CombatMoveForwardClip != null && overrideController[systemAnimations.AnimationProps.CombatMoveForwardClip.name] != defaultAnimationProfile.AnimationProps.CombatMoveForwardClip) {
                        overrideController[systemAnimations.AnimationProps.CombatMoveForwardClip.name] = defaultAnimationProfile.AnimationProps.CombatMoveForwardClip;
                        currentAnimations.AnimationProps.CombatMoveForwardClip = defaultAnimationProfile.AnimationProps.CombatMoveForwardClip;
                    }
                }
                //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): MyCombatMoveForwardClip.averageSpeed: " + currentAttackAnimationProfile.MyMoveForwardClip.averageSpeed + "; apparentSpeed: " + currentAttackAnimationProfile.MyCombatMoveForwardClip.apparentSpeed + "; averageAngularSpeed: " + currentAttackAnimationProfile.MyCombatMoveForwardClip.averageAngularSpeed);
                if (currentAnimations.AnimationProps.CombatMoveForwardClip.averageSpeed.z > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 1
                    baseCombatWalkAnimationSpeed = currentAnimations.AnimationProps.CombatMoveForwardClip.averageSpeed.z;
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation walk speed: " + baseWalkAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.AnimationProps.MoveForwardFastClip.name)) {
                if (currentAnimationProfile.AnimationProps.MoveForwardFastClip != null) {
                    if (overrideController[systemAnimations.AnimationProps.MoveForwardFastClip.name] != currentAnimationProfile.AnimationProps.MoveForwardFastClip) {
                        overrideController[systemAnimations.AnimationProps.MoveForwardFastClip.name] = currentAnimationProfile.AnimationProps.MoveForwardFastClip;
                        currentAnimations.AnimationProps.MoveForwardFastClip = currentAnimationProfile.AnimationProps.MoveForwardFastClip;
                    }
                } else {
                    if (defaultAnimationProfile.AnimationProps.MoveForwardFastClip != null && overrideController[systemAnimations.AnimationProps.MoveForwardFastClip.name] != defaultAnimationProfile.AnimationProps.MoveForwardFastClip) {
                        overrideController[systemAnimations.AnimationProps.MoveForwardFastClip.name] = defaultAnimationProfile.AnimationProps.MoveForwardFastClip;
                        currentAnimations.AnimationProps.MoveForwardFastClip = defaultAnimationProfile.AnimationProps.MoveForwardFastClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.AnimationProps.MoveForwardFastClip.averageSpeed.z) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseRunAnimationSpeed = Mathf.Abs(currentAnimations.AnimationProps.MoveForwardFastClip.averageSpeed.z);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.AnimationProps.CombatMoveForwardFastClip.name)) {
                if (currentAnimationProfile.AnimationProps.CombatMoveForwardFastClip != null) {
                    if (overrideController[systemAnimations.AnimationProps.CombatMoveForwardFastClip.name] != currentAnimationProfile.AnimationProps.CombatMoveForwardFastClip) {
                        overrideController[systemAnimations.AnimationProps.CombatMoveForwardFastClip.name] = currentAnimationProfile.AnimationProps.CombatMoveForwardFastClip;
                        currentAnimations.AnimationProps.CombatMoveForwardFastClip = currentAnimationProfile.AnimationProps.CombatMoveForwardFastClip;
                    }
                } else {
                    if (defaultAnimationProfile.AnimationProps.CombatMoveForwardFastClip != null && overrideController[systemAnimations.AnimationProps.CombatMoveForwardFastClip.name] != defaultAnimationProfile.AnimationProps.CombatMoveForwardFastClip) {
                        overrideController[systemAnimations.AnimationProps.CombatMoveForwardFastClip.name] = defaultAnimationProfile.AnimationProps.CombatMoveForwardFastClip;
                        currentAnimations.AnimationProps.CombatMoveForwardFastClip = defaultAnimationProfile.AnimationProps.CombatMoveForwardFastClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.AnimationProps.CombatMoveForwardFastClip.averageSpeed.z) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseCombatRunAnimationSpeed = Mathf.Abs(currentAnimations.AnimationProps.CombatMoveForwardFastClip.averageSpeed.z);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.AnimationProps.MoveBackClip.name)) {
                if (currentAnimationProfile.AnimationProps.MoveBackClip != null) {
                    if (overrideController[systemAnimations.AnimationProps.MoveBackClip.name] != currentAnimationProfile.AnimationProps.MoveBackClip) {
                        overrideController[systemAnimations.AnimationProps.MoveBackClip.name] = currentAnimationProfile.AnimationProps.MoveBackClip;
                        currentAnimations.AnimationProps.MoveBackClip = currentAnimationProfile.AnimationProps.MoveBackClip;
                    }
                } else {
                    if (defaultAnimationProfile.AnimationProps.MoveBackClip != null && overrideController[systemAnimations.AnimationProps.MoveBackClip.name] != defaultAnimationProfile.AnimationProps.MoveBackClip) {
                        overrideController[systemAnimations.AnimationProps.MoveBackClip.name] = defaultAnimationProfile.AnimationProps.MoveBackClip;
                        currentAnimations.AnimationProps.MoveBackClip = defaultAnimationProfile.AnimationProps.MoveBackClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.AnimationProps.MoveBackClip.averageSpeed.z) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseWalkBackAnimationSpeed = Mathf.Abs(currentAnimations.AnimationProps.MoveBackClip.averageSpeed.z);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.AnimationProps.CombatMoveBackClip.name)) {
                if (currentAnimationProfile.AnimationProps.CombatMoveBackClip != null) {
                    if (overrideController[systemAnimations.AnimationProps.CombatMoveBackClip.name] != currentAnimationProfile.AnimationProps.CombatMoveBackClip) {
                        overrideController[systemAnimations.AnimationProps.CombatMoveBackClip.name] = currentAnimationProfile.AnimationProps.CombatMoveBackClip;
                        currentAnimations.AnimationProps.CombatMoveBackClip = currentAnimationProfile.AnimationProps.CombatMoveBackClip;
                    }
                } else {
                    if (defaultAnimationProfile.AnimationProps.CombatMoveBackClip != null && overrideController[systemAnimations.AnimationProps.CombatMoveBackClip.name] != defaultAnimationProfile.AnimationProps.CombatMoveBackClip) {
                        overrideController[systemAnimations.AnimationProps.CombatMoveBackClip.name] = defaultAnimationProfile.AnimationProps.CombatMoveBackClip;
                        currentAnimations.AnimationProps.CombatMoveBackClip = defaultAnimationProfile.AnimationProps.CombatMoveBackClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.AnimationProps.CombatMoveBackClip.averageSpeed.z) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseCombatWalkBackAnimationSpeed = Mathf.Abs(currentAnimations.AnimationProps.CombatMoveBackClip.averageSpeed.z);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.AnimationProps.MoveBackFastClip.name)) {
                if (currentAnimationProfile.AnimationProps.MoveBackFastClip != null) {
                    if (overrideController[systemAnimations.AnimationProps.MoveBackFastClip.name] != currentAnimationProfile.AnimationProps.MoveBackFastClip) {
                        overrideController[systemAnimations.AnimationProps.MoveBackFastClip.name] = currentAnimationProfile.AnimationProps.MoveBackFastClip;
                        currentAnimations.AnimationProps.MoveBackFastClip = currentAnimationProfile.AnimationProps.MoveBackFastClip;
                    }
                } else {
                    if (defaultAnimationProfile.AnimationProps.MoveBackFastClip != null && overrideController[systemAnimations.AnimationProps.MoveBackFastClip.name] != defaultAnimationProfile.AnimationProps.MoveBackFastClip) {
                        overrideController[systemAnimations.AnimationProps.MoveBackFastClip.name] = defaultAnimationProfile.AnimationProps.MoveBackFastClip;
                        currentAnimations.AnimationProps.MoveBackFastClip = defaultAnimationProfile.AnimationProps.MoveBackFastClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.AnimationProps.MoveBackFastClip.averageSpeed.z) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseRunBackAnimationSpeed = Mathf.Abs(currentAnimations.AnimationProps.MoveBackFastClip.averageSpeed.z);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.AnimationProps.CombatMoveBackFastClip.name)) {
                if (currentAnimationProfile.AnimationProps.CombatMoveBackFastClip != null) {
                    if (overrideController[systemAnimations.AnimationProps.CombatMoveBackFastClip.name] != currentAnimationProfile.AnimationProps.CombatMoveBackFastClip) {
                        overrideController[systemAnimations.AnimationProps.CombatMoveBackFastClip.name] = currentAnimationProfile.AnimationProps.CombatMoveBackFastClip;
                        currentAnimations.AnimationProps.CombatMoveBackFastClip = currentAnimationProfile.AnimationProps.CombatMoveBackFastClip;
                    }
                } else {
                    if (defaultAnimationProfile.AnimationProps.CombatMoveBackFastClip != null && overrideController[systemAnimations.AnimationProps.CombatMoveBackFastClip.name] != defaultAnimationProfile.AnimationProps.CombatMoveBackFastClip) {
                        overrideController[systemAnimations.AnimationProps.CombatMoveBackFastClip.name] = defaultAnimationProfile.AnimationProps.CombatMoveBackFastClip;
                        currentAnimations.AnimationProps.CombatMoveBackFastClip = defaultAnimationProfile.AnimationProps.CombatMoveBackFastClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.AnimationProps.CombatMoveBackFastClip.averageSpeed.z) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseCombatRunBackAnimationSpeed = Mathf.Abs(currentAnimations.AnimationProps.CombatMoveBackFastClip.averageSpeed.z);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.AnimationProps.JumpClip.name)) {
                if (currentAnimationProfile.AnimationProps.JumpClip != null) {
                    if (overrideController[systemAnimations.AnimationProps.JumpClip.name] != currentAnimationProfile.AnimationProps.JumpClip) {
                        overrideController[systemAnimations.AnimationProps.JumpClip.name] = currentAnimationProfile.AnimationProps.JumpClip;
                        currentAnimations.AnimationProps.JumpClip = currentAnimationProfile.AnimationProps.JumpClip;
                    }
                } else {
                    if (defaultAnimationProfile.AnimationProps.JumpClip != null && overrideController[systemAnimations.AnimationProps.JumpClip.name] != defaultAnimationProfile.AnimationProps.JumpClip) {
                        overrideController[systemAnimations.AnimationProps.JumpClip.name] = defaultAnimationProfile.AnimationProps.JumpClip;
                        currentAnimations.AnimationProps.JumpClip = defaultAnimationProfile.AnimationProps.JumpClip;
                    }
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.AnimationProps.CombatJumpClip.name)) {
                if (currentAnimationProfile.AnimationProps.CombatJumpClip != null) {
                    if (overrideController[systemAnimations.AnimationProps.CombatJumpClip.name] != currentAnimationProfile.AnimationProps.CombatJumpClip) {
                        overrideController[systemAnimations.AnimationProps.CombatJumpClip.name] = currentAnimationProfile.AnimationProps.CombatJumpClip;
                        currentAnimations.AnimationProps.CombatJumpClip = currentAnimationProfile.AnimationProps.CombatJumpClip;
                    }
                } else {
                    if (defaultAnimationProfile.AnimationProps.CombatJumpClip != null && overrideController[systemAnimations.AnimationProps.CombatJumpClip.name] != defaultAnimationProfile.AnimationProps.CombatJumpClip) {
                        overrideController[systemAnimations.AnimationProps.CombatJumpClip.name] = defaultAnimationProfile.AnimationProps.CombatJumpClip;
                        currentAnimations.AnimationProps.CombatJumpClip = defaultAnimationProfile.AnimationProps.CombatJumpClip;
                    }
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.AnimationProps.IdleClip.name)) {
                if (currentAnimationProfile.AnimationProps.IdleClip != null) {
                    if (overrideController[systemAnimations.AnimationProps.IdleClip.name] != currentAnimationProfile.AnimationProps.IdleClip) {
                        overrideController[systemAnimations.AnimationProps.IdleClip.name] = currentAnimationProfile.AnimationProps.IdleClip;
                        currentAnimations.AnimationProps.IdleClip = currentAnimationProfile.AnimationProps.IdleClip;
                    }
                } else {
                    if (defaultAnimationProfile.AnimationProps.IdleClip != null && overrideController[systemAnimations.AnimationProps.IdleClip.name] != defaultAnimationProfile.AnimationProps.IdleClip) {
                        overrideController[systemAnimations.AnimationProps.IdleClip.name] = defaultAnimationProfile.AnimationProps.IdleClip;
                        currentAnimations.AnimationProps.IdleClip = defaultAnimationProfile.AnimationProps.IdleClip;
                    }
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.AnimationProps.CombatIdleClip.name)) {
                if (currentAnimationProfile.AnimationProps.CombatIdleClip != null) {
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): combat idle clip is not null");
                    if (overrideController[systemAnimations.AnimationProps.CombatIdleClip.name] != currentAnimationProfile.AnimationProps.CombatIdleClip) {
                        overrideController[systemAnimations.AnimationProps.CombatIdleClip.name] = currentAnimationProfile.AnimationProps.CombatIdleClip;
                        currentAnimations.AnimationProps.CombatIdleClip = currentAnimationProfile.AnimationProps.CombatIdleClip;
                    }
                } else {
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): combat idle clip is null");
                    if (defaultAnimationProfile.AnimationProps.CombatIdleClip != null && overrideController[systemAnimations.AnimationProps.CombatIdleClip.name] != defaultAnimationProfile.AnimationProps.CombatIdleClip) {
                        overrideController[systemAnimations.AnimationProps.CombatIdleClip.name] = defaultAnimationProfile.AnimationProps.CombatIdleClip;
                        currentAnimations.AnimationProps.CombatIdleClip = defaultAnimationProfile.AnimationProps.CombatIdleClip;
                    }
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.AnimationProps.LandClip.name)) {
                if (currentAnimationProfile.AnimationProps.LandClip != null) {
                    if (overrideController[systemAnimations.AnimationProps.LandClip.name] != currentAnimationProfile.AnimationProps.LandClip) {
                        overrideController[systemAnimations.AnimationProps.LandClip.name] = currentAnimationProfile.AnimationProps.LandClip;
                        currentAnimations.AnimationProps.LandClip = currentAnimationProfile.AnimationProps.LandClip;
                    }
                } else {
                    if (defaultAnimationProfile.AnimationProps.LandClip != null && overrideController[systemAnimations.AnimationProps.LandClip.name] != defaultAnimationProfile.AnimationProps.LandClip) {
                        overrideController[systemAnimations.AnimationProps.LandClip.name] = defaultAnimationProfile.AnimationProps.LandClip;
                        currentAnimations.AnimationProps.LandClip = defaultAnimationProfile.AnimationProps.LandClip;
                    }
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.AnimationProps.CombatLandClip.name)) {
                if (currentAnimationProfile.AnimationProps.CombatLandClip != null) {
                    if (overrideController[systemAnimations.AnimationProps.CombatLandClip.name] != currentAnimationProfile.AnimationProps.CombatLandClip) {
                        overrideController[systemAnimations.AnimationProps.CombatLandClip.name] = currentAnimationProfile.AnimationProps.CombatLandClip;
                        currentAnimations.AnimationProps.CombatLandClip = currentAnimationProfile.AnimationProps.CombatLandClip;
                    }
                } else {
                    if (defaultAnimationProfile.AnimationProps.CombatLandClip != null && overrideController[systemAnimations.AnimationProps.CombatLandClip.name] != defaultAnimationProfile.AnimationProps.CombatLandClip) {
                        overrideController[systemAnimations.AnimationProps.CombatLandClip.name] = defaultAnimationProfile.AnimationProps.CombatLandClip;
                        currentAnimations.AnimationProps.CombatLandClip = defaultAnimationProfile.AnimationProps.CombatLandClip;
                    }
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.AnimationProps.FallClip.name)) {
                if (currentAnimationProfile.AnimationProps.FallClip != null) {
                    if (overrideController[systemAnimations.AnimationProps.FallClip.name] != currentAnimationProfile.AnimationProps.FallClip) {
                        overrideController[systemAnimations.AnimationProps.FallClip.name] = currentAnimationProfile.AnimationProps.FallClip;
                        currentAnimations.AnimationProps.FallClip = currentAnimationProfile.AnimationProps.FallClip;
                    }
                } else {
                    if (defaultAnimationProfile.AnimationProps.FallClip != null && overrideController[systemAnimations.AnimationProps.FallClip.name] != defaultAnimationProfile.AnimationProps.FallClip) {
                        overrideController[systemAnimations.AnimationProps.FallClip.name] = defaultAnimationProfile.AnimationProps.FallClip;
                        currentAnimations.AnimationProps.FallClip = defaultAnimationProfile.AnimationProps.FallClip;
                    }
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.AnimationProps.CombatFallClip.name)) {
                if (currentAnimationProfile.AnimationProps.CombatFallClip != null) {
                    if (overrideController[systemAnimations.AnimationProps.CombatFallClip.name] != currentAnimationProfile.AnimationProps.CombatFallClip) {
                        overrideController[systemAnimations.AnimationProps.CombatFallClip.name] = currentAnimationProfile.AnimationProps.CombatFallClip;
                        currentAnimations.AnimationProps.CombatFallClip = currentAnimationProfile.AnimationProps.CombatFallClip;
                    }
                } else {
                    if (defaultAnimationProfile.AnimationProps.CombatFallClip != null && overrideController[systemAnimations.AnimationProps.CombatFallClip.name] != defaultAnimationProfile.AnimationProps.CombatFallClip) {
                        overrideController[systemAnimations.AnimationProps.CombatFallClip.name] = defaultAnimationProfile.AnimationProps.CombatFallClip;
                        currentAnimations.AnimationProps.CombatFallClip = defaultAnimationProfile.AnimationProps.CombatFallClip;
                    }
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.AnimationProps.StrafeLeftClip.name)) {
                if (currentAnimationProfile.AnimationProps.StrafeLeftClip != null) {
                    if (overrideController[systemAnimations.AnimationProps.StrafeLeftClip.name] != currentAnimationProfile.AnimationProps.StrafeLeftClip) {
                        overrideController[systemAnimations.AnimationProps.StrafeLeftClip.name] = currentAnimationProfile.AnimationProps.StrafeLeftClip;
                        currentAnimations.AnimationProps.StrafeLeftClip = currentAnimationProfile.AnimationProps.StrafeLeftClip;
                    }
                } else {
                    if (defaultAnimationProfile.AnimationProps.StrafeLeftClip != null && overrideController[systemAnimations.AnimationProps.StrafeLeftClip.name] != defaultAnimationProfile.AnimationProps.StrafeLeftClip) {
                        overrideController[systemAnimations.AnimationProps.StrafeLeftClip.name] = defaultAnimationProfile.AnimationProps.StrafeLeftClip;
                        currentAnimations.AnimationProps.StrafeLeftClip = defaultAnimationProfile.AnimationProps.StrafeLeftClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.AnimationProps.StrafeLeftClip.averageSpeed.x) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseWalkStrafeLeftAnimationSpeed = Mathf.Abs(currentAnimations.AnimationProps.StrafeLeftClip.averageSpeed.x);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.AnimationProps.JogStrafeLeftClip.name)) {
                if (currentAnimationProfile.AnimationProps.JogStrafeLeftClip != null) {
                    if (overrideController[systemAnimations.AnimationProps.JogStrafeLeftClip.name] != currentAnimationProfile.AnimationProps.JogStrafeLeftClip) {
                        overrideController[systemAnimations.AnimationProps.JogStrafeLeftClip.name] = currentAnimationProfile.AnimationProps.JogStrafeLeftClip;
                        currentAnimations.AnimationProps.JogStrafeLeftClip = currentAnimationProfile.AnimationProps.JogStrafeLeftClip;
                    }
                } else {
                    if (defaultAnimationProfile.AnimationProps.JogStrafeLeftClip != null && overrideController[systemAnimations.AnimationProps.JogStrafeLeftClip.name] != defaultAnimationProfile.AnimationProps.JogStrafeLeftClip) {
                        overrideController[systemAnimations.AnimationProps.JogStrafeLeftClip.name] = defaultAnimationProfile.AnimationProps.JogStrafeLeftClip;
                        currentAnimations.AnimationProps.JogStrafeLeftClip = defaultAnimationProfile.AnimationProps.JogStrafeLeftClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.AnimationProps.JogStrafeLeftClip.averageSpeed.x) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseJogStrafeLeftAnimationSpeed = Mathf.Abs(currentAnimations.AnimationProps.JogStrafeLeftClip.averageSpeed.x);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.AnimationProps.StrafeRightClip.name)) {
                if (currentAnimationProfile.AnimationProps.StrafeRightClip != null) {
                    if (overrideController[systemAnimations.AnimationProps.StrafeRightClip.name] != currentAnimationProfile.AnimationProps.StrafeRightClip) {
                        overrideController[systemAnimations.AnimationProps.StrafeRightClip.name] = currentAnimationProfile.AnimationProps.StrafeRightClip;
                        currentAnimations.AnimationProps.StrafeRightClip = currentAnimationProfile.AnimationProps.StrafeRightClip;
                    }
                } else {
                    if (defaultAnimationProfile.AnimationProps.StrafeRightClip != null && overrideController[systemAnimations.AnimationProps.StrafeRightClip.name] != defaultAnimationProfile.AnimationProps.StrafeRightClip) {
                        overrideController[systemAnimations.AnimationProps.StrafeRightClip.name] = defaultAnimationProfile.AnimationProps.StrafeRightClip;
                        currentAnimations.AnimationProps.StrafeRightClip = defaultAnimationProfile.AnimationProps.StrafeRightClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.AnimationProps.StrafeRightClip.averageSpeed.x) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseWalkStrafeRightAnimationSpeed = Mathf.Abs(currentAnimations.AnimationProps.StrafeRightClip.averageSpeed.x);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.AnimationProps.JogStrafeRightClip.name)) {
                if (currentAnimationProfile.AnimationProps.JogStrafeRightClip != null) {
                    if (overrideController[systemAnimations.AnimationProps.JogStrafeRightClip.name] != currentAnimationProfile.AnimationProps.JogStrafeRightClip) {
                        overrideController[systemAnimations.AnimationProps.JogStrafeRightClip.name] = currentAnimationProfile.AnimationProps.JogStrafeRightClip;
                        currentAnimations.AnimationProps.JogStrafeRightClip = currentAnimationProfile.AnimationProps.JogStrafeRightClip;
                    }
                } else {
                    if (defaultAnimationProfile.AnimationProps.JogStrafeRightClip != null && overrideController[systemAnimations.AnimationProps.JogStrafeRightClip.name] != defaultAnimationProfile.AnimationProps.JogStrafeRightClip) {
                        overrideController[systemAnimations.AnimationProps.JogStrafeRightClip.name] = defaultAnimationProfile.AnimationProps.JogStrafeRightClip;
                        currentAnimations.AnimationProps.JogStrafeRightClip = defaultAnimationProfile.AnimationProps.JogStrafeRightClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.AnimationProps.JogStrafeRightClip.averageSpeed.x) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseJogStrafeRightAnimationSpeed = Mathf.Abs(currentAnimations.AnimationProps.JogStrafeRightClip.averageSpeed.x);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.AnimationProps.StrafeForwardRightClip.name)) {
                if (currentAnimationProfile.AnimationProps.StrafeForwardRightClip != null) {
                    if (overrideController[systemAnimations.AnimationProps.StrafeForwardRightClip.name] != currentAnimationProfile.AnimationProps.StrafeForwardRightClip) {
                        overrideController[systemAnimations.AnimationProps.StrafeForwardRightClip.name] = currentAnimationProfile.AnimationProps.StrafeForwardRightClip;
                        currentAnimations.AnimationProps.StrafeForwardRightClip = currentAnimationProfile.AnimationProps.StrafeForwardRightClip;
                    }
                } else {
                    if (defaultAnimationProfile.AnimationProps.StrafeForwardRightClip != null && overrideController[systemAnimations.AnimationProps.StrafeForwardRightClip.name] != defaultAnimationProfile.AnimationProps.StrafeForwardRightClip) {
                        overrideController[systemAnimations.AnimationProps.StrafeForwardRightClip.name] = defaultAnimationProfile.AnimationProps.StrafeForwardRightClip;
                        currentAnimations.AnimationProps.StrafeForwardRightClip = defaultAnimationProfile.AnimationProps.StrafeForwardRightClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.AnimationProps.StrafeForwardRightClip.averageSpeed.magnitude) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseWalkStrafeForwardRightAnimationSpeed = Mathf.Abs(currentAnimations.AnimationProps.StrafeForwardRightClip.averageSpeed.magnitude);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.AnimationProps.JogStrafeForwardRightClip.name)) {
                if (currentAnimationProfile.AnimationProps.JogStrafeForwardRightClip != null) {
                    if (overrideController[systemAnimations.AnimationProps.JogStrafeForwardRightClip.name] != currentAnimationProfile.AnimationProps.JogStrafeForwardRightClip) {
                        overrideController[systemAnimations.AnimationProps.JogStrafeForwardRightClip.name] = currentAnimationProfile.AnimationProps.JogStrafeForwardRightClip;
                        currentAnimations.AnimationProps.JogStrafeForwardRightClip = currentAnimationProfile.AnimationProps.JogStrafeForwardRightClip;
                    }
                } else {
                    if (defaultAnimationProfile.AnimationProps.JogStrafeForwardRightClip != null && overrideController[systemAnimations.AnimationProps.JogStrafeForwardRightClip.name] != defaultAnimationProfile.AnimationProps.JogStrafeForwardRightClip) {
                        overrideController[systemAnimations.AnimationProps.JogStrafeForwardRightClip.name] = defaultAnimationProfile.AnimationProps.JogStrafeForwardRightClip;
                        currentAnimations.AnimationProps.JogStrafeForwardRightClip = defaultAnimationProfile.AnimationProps.JogStrafeForwardRightClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.AnimationProps.JogStrafeForwardRightClip.averageSpeed.magnitude) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseJogStrafeForwardRightAnimationSpeed = Mathf.Abs(currentAnimations.AnimationProps.JogStrafeForwardRightClip.averageSpeed.magnitude);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.AnimationProps.StrafeForwardLeftClip.name)) {
                if (currentAnimationProfile.AnimationProps.StrafeForwardLeftClip != null) {
                    if (overrideController[systemAnimations.AnimationProps.StrafeForwardLeftClip.name] != currentAnimationProfile.AnimationProps.StrafeForwardLeftClip) {
                        overrideController[systemAnimations.AnimationProps.StrafeForwardLeftClip.name] = currentAnimationProfile.AnimationProps.StrafeForwardLeftClip;
                        currentAnimations.AnimationProps.StrafeForwardLeftClip = currentAnimationProfile.AnimationProps.StrafeForwardLeftClip;
                    }
                } else {
                    if (defaultAnimationProfile.AnimationProps.StrafeForwardLeftClip != null && overrideController[systemAnimations.AnimationProps.StrafeForwardLeftClip.name] != defaultAnimationProfile.AnimationProps.StrafeForwardLeftClip) {
                        overrideController[systemAnimations.AnimationProps.StrafeForwardLeftClip.name] = defaultAnimationProfile.AnimationProps.StrafeForwardLeftClip;
                        currentAnimations.AnimationProps.StrafeForwardLeftClip = defaultAnimationProfile.AnimationProps.StrafeForwardLeftClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.AnimationProps.StrafeForwardLeftClip.averageSpeed.magnitude) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseWalkStrafeForwardLeftAnimationSpeed = Mathf.Abs(currentAnimations.AnimationProps.StrafeForwardLeftClip.averageSpeed.magnitude);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.AnimationProps.JogStrafeForwardLeftClip.name)) {
                if (currentAnimationProfile.AnimationProps.JogStrafeForwardLeftClip != null) {
                    if (overrideController[systemAnimations.AnimationProps.JogStrafeForwardLeftClip.name] != currentAnimationProfile.AnimationProps.JogStrafeForwardLeftClip) {
                        overrideController[systemAnimations.AnimationProps.JogStrafeForwardLeftClip.name] = currentAnimationProfile.AnimationProps.JogStrafeForwardLeftClip;
                        currentAnimations.AnimationProps.JogStrafeForwardLeftClip = currentAnimationProfile.AnimationProps.JogStrafeForwardLeftClip;
                    }
                } else {
                    if (defaultAnimationProfile.AnimationProps.JogStrafeForwardLeftClip != null && overrideController[systemAnimations.AnimationProps.JogStrafeForwardLeftClip.name] != defaultAnimationProfile.AnimationProps.JogStrafeForwardLeftClip) {
                        overrideController[systemAnimations.AnimationProps.JogStrafeForwardLeftClip.name] = defaultAnimationProfile.AnimationProps.JogStrafeForwardLeftClip;
                        currentAnimations.AnimationProps.JogStrafeForwardLeftClip = defaultAnimationProfile.AnimationProps.JogStrafeForwardLeftClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.AnimationProps.JogStrafeForwardLeftClip.averageSpeed.magnitude) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseJogStrafeForwardLeftAnimationSpeed = Mathf.Abs(currentAnimations.AnimationProps.JogStrafeForwardLeftClip.averageSpeed.magnitude);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.AnimationProps.StrafeBackLeftClip.name)) {
                if (currentAnimationProfile.AnimationProps.StrafeBackLeftClip != null) {
                    if (overrideController[systemAnimations.AnimationProps.StrafeBackLeftClip.name] != currentAnimationProfile.AnimationProps.StrafeBackLeftClip) {
                        overrideController[systemAnimations.AnimationProps.StrafeBackLeftClip.name] = currentAnimationProfile.AnimationProps.StrafeBackLeftClip;
                        currentAnimations.AnimationProps.StrafeBackLeftClip = currentAnimationProfile.AnimationProps.StrafeBackLeftClip;
                    }
                } else {
                    if (defaultAnimationProfile.AnimationProps.StrafeBackLeftClip != null && overrideController[systemAnimations.AnimationProps.StrafeBackLeftClip.name] != defaultAnimationProfile.AnimationProps.StrafeBackLeftClip) {
                        overrideController[systemAnimations.AnimationProps.StrafeBackLeftClip.name] = defaultAnimationProfile.AnimationProps.StrafeBackLeftClip;
                        currentAnimations.AnimationProps.StrafeBackLeftClip = defaultAnimationProfile.AnimationProps.StrafeBackLeftClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.AnimationProps.StrafeBackLeftClip.averageSpeed.magnitude) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseWalkStrafeBackLeftAnimationSpeed = Mathf.Abs(currentAnimations.AnimationProps.StrafeBackLeftClip.averageSpeed.magnitude);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.AnimationProps.JogStrafeBackLeftClip.name)) {
                if (currentAnimationProfile.AnimationProps.JogStrafeBackLeftClip != null) {
                    if (overrideController[systemAnimations.AnimationProps.JogStrafeBackLeftClip.name] != currentAnimationProfile.AnimationProps.JogStrafeBackLeftClip) {
                        overrideController[systemAnimations.AnimationProps.JogStrafeBackLeftClip.name] = currentAnimationProfile.AnimationProps.JogStrafeBackLeftClip;
                        currentAnimations.AnimationProps.JogStrafeBackLeftClip = currentAnimationProfile.AnimationProps.JogStrafeBackLeftClip;
                    }
                } else {
                    if (defaultAnimationProfile.AnimationProps.JogStrafeBackLeftClip != null && overrideController[systemAnimations.AnimationProps.JogStrafeBackLeftClip.name] != defaultAnimationProfile.AnimationProps.JogStrafeBackLeftClip) {
                        overrideController[systemAnimations.AnimationProps.JogStrafeBackLeftClip.name] = defaultAnimationProfile.AnimationProps.JogStrafeBackLeftClip;
                        currentAnimations.AnimationProps.JogStrafeBackLeftClip = defaultAnimationProfile.AnimationProps.JogStrafeBackLeftClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.AnimationProps.JogStrafeBackLeftClip.averageSpeed.magnitude) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseJogStrafeBackLeftAnimationSpeed = Mathf.Abs(currentAnimations.AnimationProps.JogStrafeBackLeftClip.averageSpeed.magnitude);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.AnimationProps.StrafeBackRightClip.name)) {
                if (currentAnimationProfile.AnimationProps.StrafeBackRightClip != null) {
                    if (overrideController[systemAnimations.AnimationProps.StrafeBackRightClip.name] != currentAnimationProfile.AnimationProps.StrafeBackRightClip) {
                        overrideController[systemAnimations.AnimationProps.StrafeBackRightClip.name] = currentAnimationProfile.AnimationProps.StrafeBackRightClip;
                        currentAnimations.AnimationProps.StrafeBackRightClip = currentAnimationProfile.AnimationProps.StrafeBackRightClip;
                    }
                } else {
                    if (defaultAnimationProfile.AnimationProps.StrafeBackRightClip != null && overrideController[systemAnimations.AnimationProps.StrafeBackRightClip.name] != defaultAnimationProfile.AnimationProps.StrafeBackRightClip) {
                        overrideController[systemAnimations.AnimationProps.StrafeBackRightClip.name] = defaultAnimationProfile.AnimationProps.StrafeBackRightClip;
                        currentAnimations.AnimationProps.StrafeBackRightClip = defaultAnimationProfile.AnimationProps.StrafeBackRightClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.AnimationProps.StrafeBackRightClip.averageSpeed.magnitude) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseWalkStrafeBackRightAnimationSpeed = Mathf.Abs(currentAnimations.AnimationProps.StrafeBackRightClip.averageSpeed.magnitude);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.AnimationProps.JogStrafeBackRightClip.name)) {
                if (currentAnimationProfile.AnimationProps.JogStrafeBackRightClip != null) {
                    if (overrideController[systemAnimations.AnimationProps.JogStrafeBackRightClip.name] != currentAnimationProfile.AnimationProps.JogStrafeBackRightClip) {
                        overrideController[systemAnimations.AnimationProps.JogStrafeBackRightClip.name] = currentAnimationProfile.AnimationProps.JogStrafeBackRightClip;
                        currentAnimations.AnimationProps.JogStrafeBackRightClip = currentAnimationProfile.AnimationProps.JogStrafeBackRightClip;
                    }
                } else {
                    if (defaultAnimationProfile.AnimationProps.JogStrafeBackRightClip != null && overrideController[systemAnimations.AnimationProps.JogStrafeBackRightClip.name] != defaultAnimationProfile.AnimationProps.JogStrafeBackRightClip) {
                        overrideController[systemAnimations.AnimationProps.JogStrafeBackRightClip.name] = defaultAnimationProfile.AnimationProps.JogStrafeBackRightClip;
                        currentAnimations.AnimationProps.JogStrafeBackRightClip = defaultAnimationProfile.AnimationProps.JogStrafeBackRightClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.AnimationProps.JogStrafeBackRightClip.averageSpeed.magnitude) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseJogStrafeBackRightAnimationSpeed = Mathf.Abs(currentAnimations.AnimationProps.JogStrafeBackRightClip.averageSpeed.magnitude);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.AnimationProps.CombatStrafeLeftClip.name)) {
                if (currentAnimationProfile.AnimationProps.CombatStrafeLeftClip != null) {
                    if (overrideController[systemAnimations.AnimationProps.CombatStrafeLeftClip.name] != currentAnimationProfile.AnimationProps.CombatStrafeLeftClip) {
                        overrideController[systemAnimations.AnimationProps.CombatStrafeLeftClip.name] = currentAnimationProfile.AnimationProps.CombatStrafeLeftClip;
                        currentAnimations.AnimationProps.CombatStrafeLeftClip = currentAnimationProfile.AnimationProps.CombatStrafeLeftClip;
                    }
                } else {
                    if (defaultAnimationProfile.AnimationProps.CombatStrafeLeftClip != null && overrideController[systemAnimations.AnimationProps.CombatStrafeLeftClip.name] != defaultAnimationProfile.AnimationProps.CombatStrafeLeftClip) {
                        overrideController[systemAnimations.AnimationProps.CombatStrafeLeftClip.name] = defaultAnimationProfile.AnimationProps.CombatStrafeLeftClip;
                        currentAnimations.AnimationProps.CombatStrafeLeftClip = defaultAnimationProfile.AnimationProps.CombatStrafeLeftClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.AnimationProps.CombatStrafeLeftClip.averageSpeed.x) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseCombatWalkStrafeLeftAnimationSpeed = Mathf.Abs(currentAnimations.AnimationProps.CombatStrafeLeftClip.averageSpeed.x);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.AnimationProps.CombatJogStrafeLeftClip.name)) {
                if (currentAnimationProfile.AnimationProps.CombatJogStrafeLeftClip != null) {
                    if (overrideController[systemAnimations.AnimationProps.CombatJogStrafeLeftClip.name] != currentAnimationProfile.AnimationProps.CombatJogStrafeLeftClip) {
                        overrideController[systemAnimations.AnimationProps.CombatJogStrafeLeftClip.name] = currentAnimationProfile.AnimationProps.CombatJogStrafeLeftClip;
                        currentAnimations.AnimationProps.CombatJogStrafeLeftClip = currentAnimationProfile.AnimationProps.CombatJogStrafeLeftClip;
                    }
                } else {
                    if (defaultAnimationProfile.AnimationProps.CombatJogStrafeLeftClip != null && overrideController[systemAnimations.AnimationProps.CombatJogStrafeLeftClip.name] != defaultAnimationProfile.AnimationProps.CombatJogStrafeLeftClip) {
                        overrideController[systemAnimations.AnimationProps.CombatJogStrafeLeftClip.name] = defaultAnimationProfile.AnimationProps.CombatJogStrafeLeftClip;
                        currentAnimations.AnimationProps.CombatJogStrafeLeftClip = defaultAnimationProfile.AnimationProps.CombatJogStrafeLeftClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.AnimationProps.CombatJogStrafeLeftClip.averageSpeed.x) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseCombatJogStrafeLeftAnimationSpeed = Mathf.Abs(currentAnimations.AnimationProps.CombatJogStrafeLeftClip.averageSpeed.x);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.AnimationProps.CombatStrafeRightClip.name)) {
                if (currentAnimationProfile.AnimationProps.CombatStrafeRightClip != null) {
                    if (overrideController[systemAnimations.AnimationProps.CombatStrafeRightClip.name] != currentAnimationProfile.AnimationProps.CombatStrafeRightClip) {
                        overrideController[systemAnimations.AnimationProps.CombatStrafeRightClip.name] = currentAnimationProfile.AnimationProps.CombatStrafeRightClip;
                        currentAnimations.AnimationProps.CombatStrafeRightClip = currentAnimationProfile.AnimationProps.CombatStrafeRightClip;
                    }
                } else {
                    if (defaultAnimationProfile.AnimationProps.CombatStrafeRightClip != null && overrideController[systemAnimations.AnimationProps.CombatStrafeRightClip.name] != defaultAnimationProfile.AnimationProps.CombatStrafeRightClip) {
                        overrideController[systemAnimations.AnimationProps.CombatStrafeRightClip.name] = defaultAnimationProfile.AnimationProps.CombatStrafeRightClip;
                        currentAnimations.AnimationProps.CombatStrafeRightClip = defaultAnimationProfile.AnimationProps.CombatStrafeRightClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.AnimationProps.CombatStrafeRightClip.averageSpeed.x) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseCombatWalkStrafeRightAnimationSpeed = Mathf.Abs(currentAnimations.AnimationProps.CombatStrafeRightClip.averageSpeed.x);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.AnimationProps.CombatJogStrafeRightClip.name)) {
                if (currentAnimationProfile.AnimationProps.CombatJogStrafeRightClip != null) {
                    if (overrideController[systemAnimations.AnimationProps.CombatJogStrafeRightClip.name] != currentAnimationProfile.AnimationProps.CombatJogStrafeRightClip) {
                        overrideController[systemAnimations.AnimationProps.CombatJogStrafeRightClip.name] = currentAnimationProfile.AnimationProps.CombatJogStrafeRightClip;
                        currentAnimations.AnimationProps.CombatJogStrafeRightClip = currentAnimationProfile.AnimationProps.CombatJogStrafeRightClip;
                    }
                } else {
                    if (defaultAnimationProfile.AnimationProps.CombatJogStrafeRightClip != null && overrideController[systemAnimations.AnimationProps.CombatJogStrafeRightClip.name] != defaultAnimationProfile.AnimationProps.CombatJogStrafeRightClip) {
                        overrideController[systemAnimations.AnimationProps.CombatJogStrafeRightClip.name] = defaultAnimationProfile.AnimationProps.CombatJogStrafeRightClip;
                        currentAnimations.AnimationProps.CombatJogStrafeRightClip = defaultAnimationProfile.AnimationProps.CombatJogStrafeRightClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.AnimationProps.CombatJogStrafeRightClip.averageSpeed.x) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseCombatJogStrafeRightAnimationSpeed = Mathf.Abs(currentAnimations.AnimationProps.CombatJogStrafeRightClip.averageSpeed.x);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.AnimationProps.CombatStrafeForwardRightClip.name)) {
                if (currentAnimationProfile.AnimationProps.CombatStrafeForwardRightClip != null) {
                    if (overrideController[systemAnimations.AnimationProps.CombatStrafeForwardRightClip.name] != currentAnimationProfile.AnimationProps.CombatStrafeForwardRightClip) {
                        overrideController[systemAnimations.AnimationProps.CombatStrafeForwardRightClip.name] = currentAnimationProfile.AnimationProps.CombatStrafeForwardRightClip;
                        currentAnimations.AnimationProps.CombatStrafeForwardRightClip = currentAnimationProfile.AnimationProps.CombatStrafeForwardRightClip;
                    }
                } else {
                    if (defaultAnimationProfile.AnimationProps.CombatStrafeForwardRightClip != null && overrideController[systemAnimations.AnimationProps.CombatStrafeForwardRightClip.name] != defaultAnimationProfile.AnimationProps.CombatStrafeForwardRightClip) {
                        overrideController[systemAnimations.AnimationProps.CombatStrafeForwardRightClip.name] = defaultAnimationProfile.AnimationProps.CombatStrafeForwardRightClip;
                        currentAnimations.AnimationProps.CombatStrafeForwardRightClip = defaultAnimationProfile.AnimationProps.CombatStrafeForwardRightClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.AnimationProps.CombatStrafeForwardRightClip.averageSpeed.magnitude) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseCombatWalkStrafeForwardRightAnimationSpeed = Mathf.Abs(currentAnimations.AnimationProps.CombatStrafeForwardRightClip.averageSpeed.magnitude);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.AnimationProps.CombatJogStrafeForwardRightClip.name)) {
                if (currentAnimationProfile.AnimationProps.CombatJogStrafeForwardRightClip != null) {
                    if (overrideController[systemAnimations.AnimationProps.CombatJogStrafeForwardRightClip.name] != currentAnimationProfile.AnimationProps.CombatJogStrafeForwardRightClip) {
                        overrideController[systemAnimations.AnimationProps.CombatJogStrafeForwardRightClip.name] = currentAnimationProfile.AnimationProps.CombatJogStrafeForwardRightClip;
                        currentAnimations.AnimationProps.CombatJogStrafeForwardRightClip = currentAnimationProfile.AnimationProps.CombatJogStrafeForwardRightClip;
                    }
                } else {
                    if (defaultAnimationProfile.AnimationProps.CombatJogStrafeForwardRightClip != null && overrideController[systemAnimations.AnimationProps.CombatJogStrafeForwardRightClip.name] != defaultAnimationProfile.AnimationProps.CombatJogStrafeForwardRightClip) {
                        overrideController[systemAnimations.AnimationProps.CombatJogStrafeForwardRightClip.name] = defaultAnimationProfile.AnimationProps.CombatJogStrafeForwardRightClip;
                        currentAnimations.AnimationProps.CombatJogStrafeForwardRightClip = defaultAnimationProfile.AnimationProps.CombatJogStrafeForwardRightClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.AnimationProps.CombatJogStrafeForwardRightClip.averageSpeed.magnitude) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseCombatJogStrafeForwardRightAnimationSpeed = Mathf.Abs(currentAnimations.AnimationProps.CombatJogStrafeForwardRightClip.averageSpeed.magnitude);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.AnimationProps.CombatStrafeForwardLeftClip.name)) {
                if (currentAnimationProfile.AnimationProps.CombatStrafeForwardLeftClip != null) {
                    if (overrideController[systemAnimations.AnimationProps.CombatStrafeForwardLeftClip.name] != currentAnimationProfile.AnimationProps.CombatStrafeForwardLeftClip) {
                        overrideController[systemAnimations.AnimationProps.CombatStrafeForwardLeftClip.name] = currentAnimationProfile.AnimationProps.CombatStrafeForwardLeftClip;
                        currentAnimations.AnimationProps.CombatStrafeForwardLeftClip = currentAnimationProfile.AnimationProps.CombatStrafeForwardLeftClip;
                    }
                } else {
                    if (defaultAnimationProfile.AnimationProps.CombatStrafeForwardLeftClip != null && overrideController[systemAnimations.AnimationProps.CombatStrafeForwardLeftClip.name] != defaultAnimationProfile.AnimationProps.CombatStrafeForwardLeftClip) {
                        overrideController[systemAnimations.AnimationProps.CombatStrafeForwardLeftClip.name] = defaultAnimationProfile.AnimationProps.CombatStrafeForwardLeftClip;
                        currentAnimations.AnimationProps.CombatStrafeForwardLeftClip = defaultAnimationProfile.AnimationProps.CombatStrafeForwardLeftClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.AnimationProps.CombatStrafeForwardLeftClip.averageSpeed.magnitude) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseCombatWalkStrafeForwardLeftAnimationSpeed = Mathf.Abs(currentAnimations.AnimationProps.CombatStrafeForwardLeftClip.averageSpeed.magnitude);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.AnimationProps.CombatJogStrafeForwardLeftClip.name)) {
                if (currentAnimationProfile.AnimationProps.CombatJogStrafeForwardLeftClip != null) {
                    if (overrideController[systemAnimations.AnimationProps.CombatJogStrafeForwardLeftClip.name] != currentAnimationProfile.AnimationProps.CombatJogStrafeForwardLeftClip) {
                        overrideController[systemAnimations.AnimationProps.CombatJogStrafeForwardLeftClip.name] = currentAnimationProfile.AnimationProps.CombatJogStrafeForwardLeftClip;
                        currentAnimations.AnimationProps.CombatJogStrafeForwardLeftClip = currentAnimationProfile.AnimationProps.CombatJogStrafeForwardLeftClip;
                    }
                } else {
                    if (defaultAnimationProfile.AnimationProps.CombatJogStrafeForwardLeftClip != null && overrideController[systemAnimations.AnimationProps.CombatJogStrafeForwardLeftClip.name] != defaultAnimationProfile.AnimationProps.CombatJogStrafeForwardLeftClip) {
                        overrideController[systemAnimations.AnimationProps.CombatJogStrafeForwardLeftClip.name] = defaultAnimationProfile.AnimationProps.CombatJogStrafeForwardLeftClip;
                        currentAnimations.AnimationProps.CombatJogStrafeForwardLeftClip = defaultAnimationProfile.AnimationProps.CombatJogStrafeForwardLeftClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.AnimationProps.CombatJogStrafeForwardLeftClip.averageSpeed.magnitude) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseCombatJogStrafeForwardLeftAnimationSpeed = Mathf.Abs(currentAnimations.AnimationProps.CombatJogStrafeForwardLeftClip.averageSpeed.magnitude);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.AnimationProps.CombatStrafeBackLeftClip.name)) {
                if (currentAnimationProfile.AnimationProps.CombatStrafeBackLeftClip != null) {
                    if (overrideController[systemAnimations.AnimationProps.CombatStrafeBackLeftClip.name] != currentAnimationProfile.AnimationProps.CombatStrafeBackLeftClip) {
                        overrideController[systemAnimations.AnimationProps.CombatStrafeBackLeftClip.name] = currentAnimationProfile.AnimationProps.CombatStrafeBackLeftClip;
                        currentAnimations.AnimationProps.CombatStrafeBackLeftClip = currentAnimationProfile.AnimationProps.CombatStrafeBackLeftClip;
                    }
                } else {
                    if (defaultAnimationProfile.AnimationProps.CombatStrafeBackLeftClip != null && overrideController[systemAnimations.AnimationProps.CombatStrafeBackLeftClip.name] != defaultAnimationProfile.AnimationProps.CombatStrafeBackLeftClip) {
                        overrideController[systemAnimations.AnimationProps.CombatStrafeBackLeftClip.name] = defaultAnimationProfile.AnimationProps.CombatStrafeBackLeftClip;
                        currentAnimations.AnimationProps.CombatStrafeBackLeftClip = defaultAnimationProfile.AnimationProps.CombatStrafeBackLeftClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.AnimationProps.CombatStrafeBackLeftClip.averageSpeed.magnitude) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseCombatWalkStrafeBackLeftAnimationSpeed = Mathf.Abs(currentAnimations.AnimationProps.CombatStrafeBackLeftClip.averageSpeed.magnitude);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.AnimationProps.CombatJogStrafeBackLeftClip.name)) {
                if (currentAnimationProfile.AnimationProps.CombatJogStrafeBackLeftClip != null) {
                    if (overrideController[systemAnimations.AnimationProps.CombatJogStrafeBackLeftClip.name] != currentAnimationProfile.AnimationProps.CombatJogStrafeBackLeftClip) {
                        overrideController[systemAnimations.AnimationProps.CombatJogStrafeBackLeftClip.name] = currentAnimationProfile.AnimationProps.CombatJogStrafeBackLeftClip;
                        currentAnimations.AnimationProps.CombatJogStrafeBackLeftClip = currentAnimationProfile.AnimationProps.CombatJogStrafeBackLeftClip;
                    }
                } else {
                    if (defaultAnimationProfile.AnimationProps.CombatJogStrafeBackLeftClip != null && overrideController[systemAnimations.AnimationProps.CombatJogStrafeBackLeftClip.name] != defaultAnimationProfile.AnimationProps.CombatJogStrafeBackLeftClip) {
                        overrideController[systemAnimations.AnimationProps.CombatJogStrafeBackLeftClip.name] = defaultAnimationProfile.AnimationProps.CombatJogStrafeBackLeftClip;
                        currentAnimations.AnimationProps.CombatJogStrafeBackLeftClip = defaultAnimationProfile.AnimationProps.CombatJogStrafeBackLeftClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.AnimationProps.CombatJogStrafeBackLeftClip.averageSpeed.magnitude) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseCombatJogStrafeBackLeftAnimationSpeed = Mathf.Abs(currentAnimations.AnimationProps.CombatJogStrafeBackLeftClip.averageSpeed.magnitude);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.AnimationProps.CombatStrafeBackRightClip.name)) {
                if (currentAnimationProfile.AnimationProps.CombatStrafeBackRightClip != null) {
                    if (overrideController[systemAnimations.AnimationProps.CombatStrafeBackRightClip.name] != currentAnimationProfile.AnimationProps.CombatStrafeBackRightClip) {
                        overrideController[systemAnimations.AnimationProps.CombatStrafeBackRightClip.name] = currentAnimationProfile.AnimationProps.CombatStrafeBackRightClip;
                        currentAnimations.AnimationProps.CombatStrafeBackRightClip = currentAnimationProfile.AnimationProps.CombatStrafeBackRightClip;
                    }
                } else {
                    if (defaultAnimationProfile.AnimationProps.CombatStrafeBackRightClip != null && overrideController[systemAnimations.AnimationProps.CombatStrafeBackRightClip.name] != defaultAnimationProfile.AnimationProps.CombatStrafeBackRightClip) {
                        overrideController[systemAnimations.AnimationProps.CombatStrafeBackRightClip.name] = defaultAnimationProfile.AnimationProps.CombatStrafeBackRightClip;
                        currentAnimations.AnimationProps.CombatStrafeBackRightClip = defaultAnimationProfile.AnimationProps.CombatStrafeBackRightClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.AnimationProps.CombatStrafeBackRightClip.averageSpeed.magnitude) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseCombatWalkStrafeBackRightAnimationSpeed = Mathf.Abs(currentAnimations.AnimationProps.CombatStrafeBackRightClip.averageSpeed.magnitude);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.AnimationProps.CombatJogStrafeBackRightClip.name)) {
                if (currentAnimationProfile.AnimationProps.CombatJogStrafeBackRightClip != null) {
                    if (overrideController[systemAnimations.AnimationProps.CombatJogStrafeBackRightClip.name] != currentAnimationProfile.AnimationProps.CombatJogStrafeBackRightClip) {
                        overrideController[systemAnimations.AnimationProps.CombatJogStrafeBackRightClip.name] = currentAnimationProfile.AnimationProps.CombatJogStrafeBackRightClip;
                        currentAnimations.AnimationProps.CombatJogStrafeBackRightClip = currentAnimationProfile.AnimationProps.CombatJogStrafeBackRightClip;
                    }
                } else {
                    if (defaultAnimationProfile.AnimationProps.CombatJogStrafeBackRightClip != null && overrideController[systemAnimations.AnimationProps.CombatJogStrafeBackRightClip.name] != defaultAnimationProfile.AnimationProps.CombatJogStrafeBackRightClip) {
                        overrideController[systemAnimations.AnimationProps.CombatJogStrafeBackRightClip.name] = defaultAnimationProfile.AnimationProps.CombatJogStrafeBackRightClip;
                        currentAnimations.AnimationProps.CombatJogStrafeBackRightClip = defaultAnimationProfile.AnimationProps.CombatJogStrafeBackRightClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.AnimationProps.CombatJogStrafeBackRightClip.averageSpeed.magnitude) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseCombatJogStrafeBackRightAnimationSpeed = Mathf.Abs(currentAnimations.AnimationProps.CombatJogStrafeBackRightClip.averageSpeed.magnitude);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): Death is not null.");
            if (overrideControllerClipList.Contains(systemAnimations.AnimationProps.DeathClip.name)) {
                if (currentAnimationProfile.AnimationProps.DeathClip != null) {
                    if (overrideController[systemAnimations.AnimationProps.DeathClip.name] != currentAnimationProfile.AnimationProps.DeathClip) {
                        overrideController[systemAnimations.AnimationProps.DeathClip.name] = currentAnimationProfile.AnimationProps.DeathClip;
                        currentAnimations.AnimationProps.DeathClip = currentAnimationProfile.AnimationProps.DeathClip;
                    }
                } else {
                    if (defaultAnimationProfile.AnimationProps.DeathClip != null && overrideController[systemAnimations.AnimationProps.DeathClip.name] != defaultAnimationProfile.AnimationProps.DeathClip) {
                        overrideController[systemAnimations.AnimationProps.DeathClip.name] = defaultAnimationProfile.AnimationProps.DeathClip;
                        currentAnimations.AnimationProps.DeathClip = defaultAnimationProfile.AnimationProps.DeathClip;
                    }
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.AnimationProps.StunnedClip.name)) {
                if (currentAnimationProfile.AnimationProps.StunnedClip != null) {
                    if (overrideController[systemAnimations.AnimationProps.StunnedClip.name] != currentAnimationProfile.AnimationProps.StunnedClip) {
                        overrideController[systemAnimations.AnimationProps.StunnedClip.name] = currentAnimationProfile.AnimationProps.StunnedClip;
                        currentAnimations.AnimationProps.StunnedClip = currentAnimationProfile.AnimationProps.StunnedClip;
                    }
                } else {
                    if (defaultAnimationProfile.AnimationProps.StunnedClip != null && overrideController[systemAnimations.AnimationProps.StunnedClip.name] != defaultAnimationProfile.AnimationProps.StunnedClip) {
                        overrideController[systemAnimations.AnimationProps.StunnedClip.name] = defaultAnimationProfile.AnimationProps.StunnedClip;
                        currentAnimations.AnimationProps.StunnedClip = defaultAnimationProfile.AnimationProps.StunnedClip;
                    }
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.AnimationProps.LevitatedClip.name)) {
                if (currentAnimationProfile.AnimationProps.LevitatedClip != null) {
                    if (overrideController[systemAnimations.AnimationProps.LevitatedClip.name] != currentAnimationProfile.AnimationProps.LevitatedClip) {
                        overrideController[systemAnimations.AnimationProps.LevitatedClip.name] = currentAnimationProfile.AnimationProps.LevitatedClip;
                        currentAnimations.AnimationProps.LevitatedClip = currentAnimationProfile.AnimationProps.LevitatedClip;
                    }
                } else {
                    if (defaultAnimationProfile.AnimationProps.LevitatedClip != null && overrideController[systemAnimations.AnimationProps.LevitatedClip.name] != defaultAnimationProfile.AnimationProps.LevitatedClip) {
                        overrideController[systemAnimations.AnimationProps.LevitatedClip.name] = defaultAnimationProfile.AnimationProps.LevitatedClip;
                        currentAnimations.AnimationProps.LevitatedClip = defaultAnimationProfile.AnimationProps.LevitatedClip;
                    }
                }
            }

            //Debug.Log("CharacterAnimator.SetAnimationClipOverrides() Current Animation Profile Contains Revive Clip");
            if (overrideControllerClipList.Contains(systemAnimations.AnimationProps.ReviveClip.name)) {
                if (currentAnimationProfile.AnimationProps.ReviveClip != null) {
                    if (overrideController[systemAnimations.AnimationProps.ReviveClip.name] != currentAnimationProfile.AnimationProps.ReviveClip) {
                        overrideController[systemAnimations.AnimationProps.ReviveClip.name] = currentAnimationProfile.AnimationProps.ReviveClip;
                        currentAnimations.AnimationProps.ReviveClip = currentAnimationProfile.AnimationProps.ReviveClip;
                    }
                } else {
                    if (defaultAnimationProfile.AnimationProps.ReviveClip != null && overrideController[systemAnimations.AnimationProps.ReviveClip.name] != defaultAnimationProfile.AnimationProps.ReviveClip) {
                        overrideController[systemAnimations.AnimationProps.ReviveClip.name] = defaultAnimationProfile.AnimationProps.ReviveClip;
                        currentAnimations.AnimationProps.ReviveClip = defaultAnimationProfile.AnimationProps.ReviveClip;
                    }
                }
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
            //Debug.Log(gameObject.name + ".CharacterAnimator.HandleAbility(" + baseAbility.MyName + ")");
            if (animator == null) {
                //Debug.Log(gameObject.name + ".CharacterAnimator.HandleAbility(" + baseAbility.MyName + ") ANIMATOR IS NULL!!!");
                return 0f;
            }
            unitController.CharacterUnit.BaseCharacter.CharacterCombat.SwingTarget = targetCharacterUnit;

            if (SystemConfigurationManager.MyInstance != null) {
                // override the default attack animation
                overrideController[SystemConfigurationManager.MyInstance.MySystemAnimationProfile.AnimationProps.AttackClips[0].name] = animationClip;
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
            if (ParameterExists("AnimationSpeed")) {
                animator.SetFloat("AnimationSpeed", (unitController.CharacterUnit.BaseCharacter.CharacterStats.GetSpeedModifiers() / 100f));
            }

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

            if (SystemConfigurationManager.MyInstance != null) {
                // override the default attack animation
                //Debug.Log(gameObject.name + ".CharacterAnimator.HandleCastingAbility() animationClip: " + animationClip.name);
                foreach (AnimationClip tmpAnimationClip in overrideController.animationClips) {
                    //Debug.Log(gameObject.name + ".CharacterAnimator.HandleCastingAbility() Found clip from overrideController: " + tmpAnimationClip.name);
                }

                overrideController[SystemConfigurationManager.MyInstance.MySystemAnimationProfile.AnimationProps.CastClips[0].name] = animationClip;
                //Debug.Log(gameObject.name + ".CharacterAnimator.HandleCastingAbility() current casting clip: " + overrideController[SystemConfigurationManager.MyInstance.MySystemAnimationProfile.MyCastClips[0].name].name);
                float animationLength = animationClip.length;
                //Debug.Log(gameObject.name + ".CharacterAnimator.HandleCastingAbility() animationlength: " + animationLength);

                // save animation length for damage normalization
                //lastAnimationLength = animationLength;

            }
            if (baseAbility.AnimationProfile.AnimationProps.UseRootMotion == true) {
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
            //Debug.Log(gameObject.name + ".WaitForAnimation(" + animationLength + ")");
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
            animator.SetFloat("AnimationSpeed", 1);
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

        public void ClearAutoAttack() {
            if (unitController != null && unitController.CharacterUnit.BaseCharacter != null && unitController.CharacterUnit.BaseCharacter.CharacterCombat != null) {
                unitController.CharacterUnit.BaseCharacter.CharacterCombat.SetWaitingForAutoAttack(false);
            }
            SetAttacking(false);
        }

        public void ClearAnimatedAttack(BaseAbility baseAbility) {
            //Debug.Log(gameObject.name + ".CharacterAnimator.ClearAnimatedAttack()");
            unitController.CharacterUnit.BaseCharacter.CharacterAbilityManager.WaitingForAnimatedAbility = false;
            (baseAbility as AnimatedAbility).CleanupEventSubscriptions(unitController.CharacterUnit.BaseCharacter);
            SetAttacking(false);
            currentAbilityEffectContext = null;
            if (unitController.CharacterUnit.BaseCharacter != null && unitController.CharacterUnit.BaseCharacter.CharacterEquipmentManager != null) {
                unitController.CharacterUnit.BaseCharacter.CharacterAbilityManager.DespawnAbilityObjects();
            }
        }

        public void ClearCasting() {
            //Debug.Log(gameObject.name + ".CharacterAnimator.ClearCasting()");

            //unitController.MyBaseCharacter.MyCharacterAbilityManager.StopCasting();
            if (unitController != null) {
                unitController.SetUseRootMotion(false);
            }
            SetCasting(false);

        }

        public void ClearAnimationBlockers() {
            //Debug.Log(gameObject.name + ".CharacterAnimator.ClearAnimationBlockers()");
            ClearAutoAttack();
            if (currentAbilityEffectContext != null && currentAbilityEffectContext.baseAbility is AnimatedAbility) {
                //Debug.Log(gameObject.name + ".CharacterAnimator.ClearAnimationBlockers() WE HAVE AN ANIMATED ABILITY");
                ClearAnimatedAttack(currentAbilityEffectContext.baseAbility);
            } else {
                //Debug.Log(gameObject.name + ".CharacterAnimator.ClearAnimationBlockers() WE DO NOT HAVE AN ANIMATED ABILITY");
            }
            ClearCasting();
            if (attackCoroutine != null) {
                //Debug.Log(gameObject.name + ".CharacterAnimator.HandleCastingAbility(): STOPPING OUTSTANDING CAST OR REGULAR ATTACK FOR CAST");
                unitController.StopCoroutine(attackCoroutine);
                attackCoroutine = null;
            }
            //Debug.Log(gameObject.name + ".CharacterAnimator.ClearAnimationBlockers(): setting speed to 1");
            if (animator != null && ParameterExists("AnimationSpeed")) {
                animator.SetFloat("AnimationSpeed", 1);
            }
        }

        private bool ParameterExists(string parameterName) {
            if (animator != null) {
                AnimatorControllerParameter[] animatorControllerParameters = animator.parameters;
                foreach (AnimatorControllerParameter animatorControllerParameter in animatorControllerParameters) {
                    if (animatorControllerParameter.name == parameterName) {
                        return true;
                    }
                }
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

            if (ParameterExists("AnimationSpeed")) {
                animator.SetFloat("AnimationSpeed", 1);
            }

            SetAttacking(false, false);
            SetCasting(false, false);

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
            if (SystemConfigurationManager.MyInstance != null) {
                float animationLength = overrideController[SystemConfigurationManager.MyInstance.MySystemAnimationProfile.AnimationProps.ReviveClip.name].length + 2;
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
            if (ParameterExists("AnimationSpeed")) {
                animator.SetFloat("AnimationSpeed", castingSpeed);
            }
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
            //Debug.Log(gameObject.name + ".SetAttacking(" + varValue + ")");
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
                OnStartRiding();
            }
            if (ParameterExists("Riding")) {
                animator.SetBool("Riding", varValue);
            }
            if (varValue == true) {
                SetTrigger("RidingTrigger");
            }
            if (varValue == false) {
                OnEndRiding();
            }
        }

        public void SetStrafing(bool varValue) {
            if (animator == null) {
                return;
            }
            animator.SetBool("Strafing", varValue);
        }

        public void SetMoving(bool varValue) {
            if (animator == null) {
                return;
            }
            if (ParameterExists("Moving")) {
                animator.SetBool("Moving", varValue);
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

            if (unitController.UnitProfile.UnitPrefabProfile.RotateModel && unitController.UnitControllerMode == UnitControllerMode.Player) {
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
                animator.SetFloat("Velocity X", 0f);
                animator.SetFloat("Velocity Z", Mathf.Abs(varValue.magnitude));
            } else {
                // if model is not rotated, send through the normal values
                animator.SetFloat("Velocity X", varValue.x);
                animator.SetFloat("Velocity Z", varValue.z);
            }
            animator.SetFloat("Velocity Y", varValue.y);




            float absXValue = Mathf.Abs(varValue.x);
            float absYValue = Mathf.Abs(varValue.y);
            float absZValue = Mathf.Abs(varValue.z);
            float absValue = Mathf.Abs(varValue.magnitude);

            float animationSpeed = 1;
            float usedBaseAnimationSpeed = 1;
            float multiplier = 1;

            if (!currentAnimationProfile.AnimationProps.SuppressAdjustAnimatorSpeed) {
                // nothing more to do if we are leaving animations at normal speed

                float usedBaseMoveForwardAnimationSpeed;
                float usedbaseWalkBackAnimationSpeed;
                float usedBaseStrafeLeftAnimationSpeed;
                float usedBaseStrafeRightAnimationSpeed;
                float usedBaseWalkStrafeBackRightAnimationSpeed;
                float usedBaseWalkStrafeBackLeftAnimationSpeed;
                float usedBaseStrafeForwardLeftAnimationSpeed;
                float usedBaseStrafeForwardRightAnimationSpeed;


                if (unitController != null && unitController.CharacterUnit.BaseCharacter != null && unitController.CharacterUnit.BaseCharacter.CharacterCombat != null && unitController.CharacterUnit.BaseCharacter.CharacterCombat.GetInCombat() == true) {
                    // in combat
                    usedBaseMoveForwardAnimationSpeed = (absZValue >= 2 ? baseCombatRunAnimationSpeed : baseCombatWalkAnimationSpeed);
                    usedbaseWalkBackAnimationSpeed = (absZValue >= 2 ? baseCombatRunBackAnimationSpeed : baseCombatWalkBackAnimationSpeed);
                    usedBaseStrafeLeftAnimationSpeed = (absValue > baseCombatJogStrafeLeftAnimationSpeed ? baseCombatJogStrafeLeftAnimationSpeed : baseCombatWalkStrafeLeftAnimationSpeed);
                    usedBaseStrafeRightAnimationSpeed = (absValue > baseCombatJogStrafeRightAnimationSpeed ? baseCombatJogStrafeRightAnimationSpeed : baseCombatWalkStrafeRightAnimationSpeed);
                    usedBaseWalkStrafeBackRightAnimationSpeed = (absValue > baseCombatJogStrafeBackRightAnimationSpeed ? baseCombatJogStrafeBackRightAnimationSpeed : baseCombatWalkStrafeBackRightAnimationSpeed);
                    usedBaseWalkStrafeBackLeftAnimationSpeed = (absValue > baseCombatJogStrafeBackLeftAnimationSpeed ? baseCombatJogStrafeBackLeftAnimationSpeed : baseCombatWalkStrafeBackLeftAnimationSpeed);
                    usedBaseStrafeForwardLeftAnimationSpeed = (absValue > baseCombatJogStrafeForwardLeftAnimationSpeed ? baseCombatJogStrafeForwardLeftAnimationSpeed : baseCombatWalkStrafeForwardLeftAnimationSpeed);
                    usedBaseStrafeForwardRightAnimationSpeed = (absValue > baseCombatJogStrafeForwardRightAnimationSpeed ? baseCombatJogStrafeForwardRightAnimationSpeed : baseCombatWalkStrafeForwardRightAnimationSpeed);
                } else {
                    // out of combat
                    usedBaseMoveForwardAnimationSpeed = (absZValue >= 2 ? baseRunAnimationSpeed : baseWalkAnimationSpeed);
                    usedbaseWalkBackAnimationSpeed = (absZValue >= 2 ? baseRunBackAnimationSpeed : baseWalkBackAnimationSpeed);
                    usedBaseStrafeLeftAnimationSpeed = (absValue > baseJogStrafeLeftAnimationSpeed ? baseJogStrafeLeftAnimationSpeed : baseWalkStrafeLeftAnimationSpeed);
                    usedBaseStrafeRightAnimationSpeed = (absValue > baseJogStrafeRightAnimationSpeed ? baseJogStrafeRightAnimationSpeed : baseWalkStrafeRightAnimationSpeed);
                    usedBaseWalkStrafeBackRightAnimationSpeed = (absValue > baseJogStrafeBackRightAnimationSpeed ? baseJogStrafeBackRightAnimationSpeed : baseWalkStrafeBackRightAnimationSpeed);
                    usedBaseWalkStrafeBackLeftAnimationSpeed = (absValue > baseJogStrafeBackLeftAnimationSpeed ? baseJogStrafeBackLeftAnimationSpeed : baseWalkStrafeBackLeftAnimationSpeed);
                    usedBaseStrafeForwardLeftAnimationSpeed = (absValue > baseJogStrafeForwardLeftAnimationSpeed ? baseJogStrafeForwardLeftAnimationSpeed : baseWalkStrafeForwardLeftAnimationSpeed);
                    usedBaseStrafeForwardRightAnimationSpeed = (absValue > baseJogStrafeForwardRightAnimationSpeed ? baseJogStrafeForwardRightAnimationSpeed : baseWalkStrafeForwardRightAnimationSpeed);
                }

                if (absXValue < (absZValue / 2) && varValue.z > 0) {
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
                //Debug.Log(gameObject.name + ".CharacterAnimator.SetVelocityZ(" + varValue + "): used: " + usedBaseAnimationSpeed + "; walk: " + baseWalkAnimationSpeed + "; run: " + baseRunAnimationSpeed);

                if (varValue.magnitude != 0) {
                    //animationSpeed = (1 / usedBaseAnimationSpeed) * Mathf.Abs(multiplier);
                    animationSpeed = multiplier;
                    //animationSpeed = (1 / baseWalkAnimationSpeed);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetVelocityZ(" + varValue + "): animationSpeed: " + animationSpeed);
                }
            }

            //Debug.Log(gameObject.name + ".CharacterAnimator.SetVelocityZ(" + varValue + "): animationSpeed: " + animationSpeed);
            animator.SetFloat("AnimationSpeed", animationSpeed);
        }

        public void SetVelocityZ(float varValue) {
            //Debug.Log(gameObject.name + ".CharacterAnimator.SetVelocityZ(" + varValue + ")");
            if (animator == null) {
                return;
            }
            animator.SetFloat("Velocity Z", varValue);
            float absValue = Mathf.Abs(varValue);

            float animationSpeed = 1;
            float usedBaseAnimationSpeed = (absValue <= 1 ? baseWalkAnimationSpeed : baseRunAnimationSpeed);
            //Debug.Log(gameObject.name + ".CharacterAnimator.SetVelocityZ(" + varValue + "): used: " + usedBaseAnimationSpeed + "; walk: " + baseWalkAnimationSpeed + "; run: " + baseRunAnimationSpeed);

            if (absValue != 0) {
                animationSpeed = (1 / usedBaseAnimationSpeed) * absValue;
                //animationSpeed = (1 / baseWalkAnimationSpeed);
                //Debug.Log(gameObject.name + ".CharacterAnimator.SetVelocityZ(" + varValue + "): animationSpeed: " + animationSpeed);
            }
            //Debug.Log("CharacterAnimator.SetVelocityZ(" + varValue + "): animationSpeed: " + animationSpeed);

            animator.SetFloat("AnimationSpeed", animationSpeed);
            if (absValue != 0) {
                //Debug.Log(gameObject.name + ": SetVelocityZ: " + varValue + "; Setting animationSpeed: " + animationSpeed);
            }
        }

        public void SetVelocityX(float varValue) {
            if (animator == null) {
                return;
            }
            animator.SetFloat("Velocity X", varValue);
        }

        public void SetVelocityY(float varValue) {
            if (animator == null) {
                return;
            }
            animator.SetFloat("Velocity Y", varValue);
        }

        public void SetTurnVelocity(float varValue) {
            if (animator == null) {
                return;
            }
            //Debug.Log(gameObject.name + ".CharacterAnimator.SetTurnVelocity(" + varValue + ")");
            animator.SetFloat("TurnVelocity", varValue);
        }

        public void SetBool(string varName, bool varValue) {

            if (animator != null) {
                if (ParameterExists(varName)) {
                    animator.SetBool(varName, varValue);
                }
            }
        }


        public void SetInteger(string varName, int varValue) {
            //Debug.Log(gameObject.name + ".CharacterAnimator.SetFloat(" + varName + ", " + varValue + ")");
            if (animator != null) {
                animator.SetInteger(varName, varValue);
            }
        }

        public void SetJumping(int varValue) {
            if (animator == null) {
                return;
            }
            animator.SetInteger("Jumping", varValue);
        }


        public void SetTrigger(string varName) {
            //Debug.Log(gameObject.name + ".CharacterAnimator.SetTrigger(" + varName + ")");
            if (animator != null) {
                if (ParameterExists(varName)) {
                    // testing unity bug, so reset trigger in case character is triggered twice in a frame
                    animator.ResetTrigger(varName);

                    animator.SetTrigger(varName);
                }
            }
        }

        public AnimatorClipInfo[] GetCurrentAnimatorClipInfo(int layerIndex) {
            return animator.GetCurrentAnimatorClipInfo(layerIndex);
        }

        public void PerformEquipmentChange(Equipment newItem) {
            HandleEquipmentChanged(newItem, null, -1);
        }

        public void HandleEquipmentChanged(Equipment newItem, Equipment oldItem, int slotIndex = -1) {
            //Debug.Log(gameObject.name + ".CharacterAnimator.PerformEquipmentChange(" + (newItem == null ? "null" : newItem.DisplayName) + ", " + (oldItem == null ? "null" : oldItem.DisplayName) + ")");
            if (animator == null) {
                // this unit isn't animated
                return;
            }

            // Animate grip for weapon when an item is added or removed from hand
            if (newItem != null && newItem is Weapon && (newItem as Weapon).MyDefaultAttackAnimationProfile != null) {
                //Debug.Log(gameObject.name + ".CharacterAnimator.PerformEquipmentChange: we are animating the weapon");
                //animator.SetLayerWeight(1, 1);
                //if (weaponAnimationsDict.ContainsKey(newItem)) {
                SetAnimationProfileOverride((newItem as Weapon).MyDefaultAttackAnimationProfile);
            } else if (newItem == null && oldItem != null && oldItem is Weapon && (oldItem as Weapon).MyDefaultAttackAnimationProfile != null) {
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


    }

}