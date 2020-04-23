using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using UMA;
using UMA.CharacterSystem;

namespace AnyRPG {
    public class CharacterAnimator : MonoBehaviour {

        public event System.Action OnReviveComplete = delegate { };

        [SerializeField]
        [FormerlySerializedAs("defaultAttackAnimationProfile")]
        protected AnimationProfile defaultAnimationProfile;

        //public AnimationClip replaceableAttackAnim;
        protected AnimationProfile currentAnimationProfile;
        const float locomotionAnimationSmoothTime = 0.1f;

        protected Animator animator;

        [SerializeField]
        protected RuntimeAnimatorController animatorController;

        [SerializeField]
        protected RuntimeAnimatorController thirdPartyAnimatorController;

        protected RuntimeAnimatorController originalAnimatorController;

        public AnimatorOverrideController overrideController;
        //protected AnimatorOverrideController defaultOverrideController;
        protected AnimatorOverrideController thirdPartyOverrideController;

        protected CharacterUnit characterUnit;

        protected bool initialized = false;

        private float lastAnimationLength;

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
        private BaseAbility currentAbility = null;

        protected bool componentReferencesInitialized = false;

        public bool applyRootMotion { get => (animator != null ? animator.applyRootMotion : false); }
        public Animator MyAnimator { get => animator; }
        public BaseAbility MyCurrentAbility { get => currentAbility; set => currentAbility = value; }
        public RuntimeAnimatorController MyAnimatorController {
            get => animatorController;
            set => animatorController = value;
        }
        public float MyLastAnimationLength { get => lastAnimationLength; set => lastAnimationLength = value; }

        public void OrchestratorStart() {
            //Debug.Log(gameObject.name + ".CharacterAnimator.OrchestratorStart()");
            systemAnimations = SystemConfigurationManager.MyInstance.MySystemAnimationProfile;
            currentAnimations = Instantiate(SystemConfigurationManager.MyInstance.MySystemAnimationProfile);
            GetComponentReferences();
        }

        public void OrchestratorFinish() {
            CreateEventSubscriptions();
            InitializeAnimator();

        }

        public virtual void GetComponentReferences() {
            if (componentReferencesInitialized == true) {
                return;
            }
            originalAnimatorController = animatorController;
            if (characterUnit == null) {
                characterUnit = GetComponent<CharacterUnit>();
            }
            if (characterUnit == null) {
                characterUnit = GetComponentInParent<CharacterUnit>();
            }
            if (characterUnit == null) {
                //Debug.Log(gameObject.name + ".CharacterAnimator.Awake(): Unable to detect characterUnit");
            }
            if (SystemConfigurationManager.MyInstance != null) {
                if (animatorController == null) {
                    animatorController = SystemConfigurationManager.MyInstance.MyDefaultAnimatorController;
                    originalAnimatorController = animatorController;
                }
                if (defaultAnimationProfile == null) {
                    defaultAnimationProfile = SystemConfigurationManager.MyInstance.MyDefaultAnimationProfile;
                }
            }
            componentReferencesInitialized = true;

        }

        public virtual void CreateEventSubscriptions() {
            if (eventSubscriptionsInitialized) {
                return;
            }
            //Debug.Log(gameObject.name + ".CharacterAnimator.CreateEventSubscriptions()");
            if (characterUnit != null && characterUnit.MyCharacter != null) {
                //Debug.Log(gameObject.name + ".CharacterAnimator.CreateEventSubscriptions(): subscribing to HandleDeath");
                //characterUnit.MyCharacter.MyCharacterCombat.OnAttack += HandleAttack;
                characterUnit.MyCharacter.MyCharacterStats.OnDie += HandleDeath;
                characterUnit.MyCharacter.MyCharacterStats.OnReviveBegin += HandleRevive;
                if (characterUnit.MyCharacter.MyCharacterEquipmentManager != null) {
                    //Debug.Log(gameObject.name + ".CharacterAnimator.CreateEventSubscriptions(): subscribing to onequipmentchanged");
                    characterUnit.MyCharacter.MyCharacterEquipmentManager.OnEquipmentChanged += PerformEquipmentChange;
                }
            } else {
                if (characterUnit == null) {
                    //Debug.Log(gameObject.name + ".CharacterAnimator.CreateEventSubscriptions(): characterUnit is null");
                } else if (characterUnit.MyCharacter == null) {
                    //Debug.Log(gameObject.name + ".CharacterAnimator.CreateEventSubscriptions(): characterUnit.mycharacter is null");
                }
            }
            eventSubscriptionsInitialized = true;
        }

        public virtual void CleanupEventSubscriptions() {
            if (characterUnit != null && characterUnit.MyCharacter != null) {
                //characterUnit.MyCharacter.MyCharacterCombat.OnAttack -= HandleAttack;
                characterUnit.MyCharacter.MyCharacterStats.OnDie -= HandleDeath;
                characterUnit.MyCharacter.MyCharacterStats.OnReviveBegin -= HandleRevive;
                if (characterUnit.MyCharacter.MyCharacterEquipmentManager != null) {
                    characterUnit.MyCharacter.MyCharacterEquipmentManager.OnEquipmentChanged -= PerformEquipmentChange;
                }
            }
        }

        public void OnDestroy() {
            // move here from ondisable to prevent from not receiving revive signals properly
            //Debug.Log(gameObject.name + ".CharacterAnimator.OnDestroy()");
            CleanupEventSubscriptions();
        }

        public void OnDisable() {
            //Debug.Log(gameObject.name + ".CharacterAnimator.OnDisable()");
            CleanupCoroutines();
        }

        public void CleanupCoroutines() {
            if (attackCoroutine != null) {
                StopCoroutine(attackCoroutine);
                attackCoroutine = null;
            }
            if (resurrectionCoroutine != null) {
                StopCoroutine(resurrectionCoroutine);
                resurrectionCoroutine = null;
            }
        }

        public void DisableRootMotion() {
            if (animator != null) {
                animator.applyRootMotion = false;
            }
        }

        public void EnableRootMotion() {
            if (animator != null) {
                animator.applyRootMotion = true;
            }
        }

        public void EnableAnimator() {
            if (animator != null) {
                animator.enabled = true;
            }
        }

        public virtual void InitializeAnimator() {
            //Debug.Log(gameObject.name + ": CharacterAnimator.InitializeAnimator()");
            if (initialized) {
                return;
            }
            animator = GetComponentInChildren<Animator>();
            if (animator == null) {
                //Debug.Log(gameObject.name + ": CharacterAnimator.InitializeAnimator(): Could not find animator in children");
                return;
            } else {
                /*
                if (SystemConfigurationManager.MyInstance.MyUseThirdPartyMovementControl == true) {
                    thirdPartyAnimatorController = animator.runtimeAnimatorController;
                    if (thirdPartyAnimatorController != null) {
                        thirdPartyOverrideController = new AnimatorOverrideController(thirdPartyAnimatorController);
                        Debug.Log(gameObject.name + ": CharacterAnimator.InitializeAnimator(): got third party animator: " + thirdPartyAnimatorController.name);
                    } else {
                        Debug.Log(gameObject.name + ": CharacterAnimator.InitializeAnimator(): third party animator was null but use third party movement control was true");
                    }
                }
                */
                //Debug.Log(gameObject.name + ": CharacterAnimator.InitializeAnimator(): found animator attached to: " + animator.gameObject.name);
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

            SetAnimationProfileOverride(defaultAnimationProfile);

            initialized = true;
        }

        public virtual void SetCorrectOverrideController(bool runUpdate = true) {
            //Debug.Log(gameObject.name + ".CharacterAnimator.SetCorrectOverrideController()");
            SetOverrideController(overrideController, runUpdate);
        }

        public virtual void SetDefaultOverrideController(bool runUpdate = true) {
            //Debug.Log(gameObject.name + ".CharacterAnimator.SetOverrideController()");
            SetOverrideController(overrideController, runUpdate);
        }


        public virtual void SetOverrideController(AnimatorOverrideController animatorOverrideController, bool runUpdate = true) {
            //Debug.Log(gameObject.name + ".CharacterAnimator.SetOverrideController()");
            if (animator == null) {
                //Debug.Log(gameObject.name + ".CharacterAnimator.SetOverrideController(): animator is null");
            } else if (animator.runtimeAnimatorController == null) {
                //Debug.Log(gameObject.name + ".CharacterAnimator.SetOverrideController(): animator.runtimeanimatorcontroller is null");
            }

            if (animator.runtimeAnimatorController != animatorOverrideController) {
                animator.runtimeAnimatorController = animatorOverrideController;

                // set animator on UMA if one exists
                DynamicCharacterAvatar myAvatar = GetComponent<DynamicCharacterAvatar>();
                if (myAvatar != null) {
                    myAvatar.raceAnimationControllers.defaultAnimationController = animatorOverrideController;
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

        // Update is called once per frame
        protected virtual void Update() {
            //Debug.Log(gameObject.name + ": CharacterAnimator.Update()");
            if (animator == null) {
                //Debug.Log(gameObject.name + ": CharacterAnimator.Update(). nothing to animate.  exiting!");
                return;
            }
        }

        protected virtual void SetAnimationClipOverrides() {
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

            if (overrideControllerClipList.Contains(systemAnimations.MyMoveForwardClip.name)) {
                if (currentAnimationProfile.MyMoveForwardClip != null) {
                    if (overrideController[systemAnimations.MyMoveForwardClip.name] != currentAnimationProfile.MyMoveForwardClip) {
                        overrideController[systemAnimations.MyMoveForwardClip.name] = currentAnimationProfile.MyMoveForwardClip;
                        currentAnimations.MyMoveForwardClip = currentAnimationProfile.MyMoveForwardClip;
                    }
                } else {
                    if (defaultAnimationProfile.MyMoveForwardClip != null && overrideController[systemAnimations.MyMoveForwardClip.name] != defaultAnimationProfile.MyMoveForwardClip) {
                        overrideController[systemAnimations.MyMoveForwardClip.name] = defaultAnimationProfile.MyMoveForwardClip;
                        currentAnimations.MyMoveForwardClip = defaultAnimationProfile.MyMoveForwardClip;
                    }
                }
                //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): MyMoveForwardClip." + currentAttackAnimationProfile.MyMoveForwardClip.averageSpeed);
                if (currentAnimations.MyMoveForwardClip.averageSpeed.z > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 1
                    baseWalkAnimationSpeed = currentAnimations.MyMoveForwardClip.averageSpeed.z;
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation walk speed: " + baseWalkAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.MyCombatMoveForwardClip.name)) {
                if (currentAnimationProfile.MyCombatMoveForwardClip != null) {
                    if (overrideController[systemAnimations.MyCombatMoveForwardClip.name] != currentAnimationProfile.MyCombatMoveForwardClip) {
                        overrideController[systemAnimations.MyCombatMoveForwardClip.name] = currentAnimationProfile.MyCombatMoveForwardClip;
                        currentAnimations.MyCombatMoveForwardClip = currentAnimationProfile.MyCombatMoveForwardClip;
                    }
                } else {
                    if (defaultAnimationProfile.MyCombatMoveForwardClip != null && overrideController[systemAnimations.MyCombatMoveForwardClip.name] != defaultAnimationProfile.MyCombatMoveForwardClip) {
                        overrideController[systemAnimations.MyCombatMoveForwardClip.name] = defaultAnimationProfile.MyCombatMoveForwardClip;
                        currentAnimations.MyCombatMoveForwardClip = defaultAnimationProfile.MyCombatMoveForwardClip;
                    }
                }
                //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): MyCombatMoveForwardClip.averageSpeed: " + currentAttackAnimationProfile.MyMoveForwardClip.averageSpeed + "; apparentSpeed: " + currentAttackAnimationProfile.MyCombatMoveForwardClip.apparentSpeed + "; averageAngularSpeed: " + currentAttackAnimationProfile.MyCombatMoveForwardClip.averageAngularSpeed);
                if (currentAnimations.MyCombatMoveForwardClip.averageSpeed.z > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 1
                    baseCombatWalkAnimationSpeed = currentAnimations.MyCombatMoveForwardClip.averageSpeed.z;
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation walk speed: " + baseWalkAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.MyMoveForwardFastClip.name)) {
                if (currentAnimationProfile.MyMoveForwardFastClip != null) {
                    if (overrideController[systemAnimations.MyMoveForwardFastClip.name] != currentAnimationProfile.MyMoveForwardFastClip) {
                        overrideController[systemAnimations.MyMoveForwardFastClip.name] = currentAnimationProfile.MyMoveForwardFastClip;
                        currentAnimations.MyMoveForwardFastClip = currentAnimationProfile.MyMoveForwardFastClip;
                    }
                } else {
                    if (defaultAnimationProfile.MyMoveForwardFastClip != null && overrideController[systemAnimations.MyMoveForwardFastClip.name] != defaultAnimationProfile.MyMoveForwardFastClip) {
                        overrideController[systemAnimations.MyMoveForwardFastClip.name] = defaultAnimationProfile.MyMoveForwardFastClip;
                        currentAnimations.MyMoveForwardFastClip = defaultAnimationProfile.MyMoveForwardFastClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.MyMoveForwardFastClip.averageSpeed.z) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseRunAnimationSpeed = Mathf.Abs(currentAnimations.MyMoveForwardFastClip.averageSpeed.z);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.MyCombatMoveForwardFastClip.name)) {
                if (currentAnimationProfile.MyCombatMoveForwardFastClip != null) {
                    if (overrideController[systemAnimations.MyCombatMoveForwardFastClip.name] != currentAnimationProfile.MyCombatMoveForwardFastClip) {
                        overrideController[systemAnimations.MyCombatMoveForwardFastClip.name] = currentAnimationProfile.MyCombatMoveForwardFastClip;
                        currentAnimations.MyCombatMoveForwardFastClip = currentAnimationProfile.MyCombatMoveForwardFastClip;
                    }
                } else {
                    if (defaultAnimationProfile.MyCombatMoveForwardFastClip != null && overrideController[systemAnimations.MyCombatMoveForwardFastClip.name] != defaultAnimationProfile.MyCombatMoveForwardFastClip) {
                        overrideController[systemAnimations.MyCombatMoveForwardFastClip.name] = defaultAnimationProfile.MyCombatMoveForwardFastClip;
                        currentAnimations.MyCombatMoveForwardFastClip = defaultAnimationProfile.MyCombatMoveForwardFastClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.MyCombatMoveForwardFastClip.averageSpeed.z) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseCombatRunAnimationSpeed = Mathf.Abs(currentAnimations.MyCombatMoveForwardFastClip.averageSpeed.z);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.MyMoveBackClip.name)) {
                if (currentAnimationProfile.MyMoveBackClip != null) {
                    if (overrideController[systemAnimations.MyMoveBackClip.name] != currentAnimationProfile.MyMoveBackClip) {
                        overrideController[systemAnimations.MyMoveBackClip.name] = currentAnimationProfile.MyMoveBackClip;
                        currentAnimations.MyMoveBackClip = currentAnimationProfile.MyMoveBackClip;
                    }
                } else {
                    if (defaultAnimationProfile.MyMoveBackClip != null && overrideController[systemAnimations.MyMoveBackClip.name] != defaultAnimationProfile.MyMoveBackClip) {
                        overrideController[systemAnimations.MyMoveBackClip.name] = defaultAnimationProfile.MyMoveBackClip;
                        currentAnimations.MyMoveBackClip = defaultAnimationProfile.MyMoveBackClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.MyMoveBackClip.averageSpeed.z) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseWalkBackAnimationSpeed = Mathf.Abs(currentAnimations.MyMoveBackClip.averageSpeed.z);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.MyCombatMoveBackClip.name)) {
                if (currentAnimationProfile.MyCombatMoveBackClip != null) {
                    if (overrideController[systemAnimations.MyCombatMoveBackClip.name] != currentAnimationProfile.MyCombatMoveBackClip) {
                        overrideController[systemAnimations.MyCombatMoveBackClip.name] = currentAnimationProfile.MyCombatMoveBackClip;
                        currentAnimations.MyCombatMoveBackClip = currentAnimationProfile.MyCombatMoveBackClip;
                    }
                } else {
                    if (defaultAnimationProfile.MyCombatMoveBackClip != null && overrideController[systemAnimations.MyCombatMoveBackClip.name] != defaultAnimationProfile.MyCombatMoveBackClip) {
                        overrideController[systemAnimations.MyCombatMoveBackClip.name] = defaultAnimationProfile.MyCombatMoveBackClip;
                        currentAnimations.MyCombatMoveBackClip = defaultAnimationProfile.MyCombatMoveBackClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.MyCombatMoveBackClip.averageSpeed.z) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseCombatWalkBackAnimationSpeed = Mathf.Abs(currentAnimations.MyCombatMoveBackClip.averageSpeed.z);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.MyMoveBackFastClip.name)) {
                if (currentAnimationProfile.MyMoveBackFastClip != null) {
                    if (overrideController[systemAnimations.MyMoveBackFastClip.name] != currentAnimationProfile.MyMoveBackFastClip) {
                        overrideController[systemAnimations.MyMoveBackFastClip.name] = currentAnimationProfile.MyMoveBackFastClip;
                        currentAnimations.MyMoveBackFastClip = currentAnimationProfile.MyMoveBackFastClip;
                    }
                } else {
                    if (defaultAnimationProfile.MyMoveBackFastClip != null && overrideController[systemAnimations.MyMoveBackFastClip.name] != defaultAnimationProfile.MyMoveBackFastClip) {
                        overrideController[systemAnimations.MyMoveBackFastClip.name] = defaultAnimationProfile.MyMoveBackFastClip;
                        currentAnimations.MyMoveBackFastClip = defaultAnimationProfile.MyMoveBackFastClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.MyMoveBackFastClip.averageSpeed.z) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseRunBackAnimationSpeed = Mathf.Abs(currentAnimations.MyMoveBackFastClip.averageSpeed.z);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.MyCombatMoveBackFastClip.name)) {
                if (currentAnimationProfile.MyCombatMoveBackFastClip != null) {
                    if (overrideController[systemAnimations.MyCombatMoveBackFastClip.name] != currentAnimationProfile.MyCombatMoveBackFastClip) {
                        overrideController[systemAnimations.MyCombatMoveBackFastClip.name] = currentAnimationProfile.MyCombatMoveBackFastClip;
                        currentAnimations.MyCombatMoveBackFastClip = currentAnimationProfile.MyCombatMoveBackFastClip;
                    }
                } else {
                    if (defaultAnimationProfile.MyCombatMoveBackFastClip != null && overrideController[systemAnimations.MyCombatMoveBackFastClip.name] != defaultAnimationProfile.MyCombatMoveBackFastClip) {
                        overrideController[systemAnimations.MyCombatMoveBackFastClip.name] = defaultAnimationProfile.MyCombatMoveBackFastClip;
                        currentAnimations.MyCombatMoveBackFastClip = defaultAnimationProfile.MyCombatMoveBackFastClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.MyCombatMoveBackFastClip.averageSpeed.z) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseCombatRunBackAnimationSpeed = Mathf.Abs(currentAnimations.MyCombatMoveBackFastClip.averageSpeed.z);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.MyJumpClip.name)) {
                if (currentAnimationProfile.MyJumpClip != null) {
                    if (overrideController[systemAnimations.MyJumpClip.name] != currentAnimationProfile.MyJumpClip) {
                        overrideController[systemAnimations.MyJumpClip.name] = currentAnimationProfile.MyJumpClip;
                        currentAnimations.MyJumpClip = currentAnimationProfile.MyJumpClip;
                    }
                } else {
                    if (defaultAnimationProfile.MyJumpClip != null && overrideController[systemAnimations.MyJumpClip.name] != defaultAnimationProfile.MyJumpClip) {
                        overrideController[systemAnimations.MyJumpClip.name] = defaultAnimationProfile.MyJumpClip;
                        currentAnimations.MyJumpClip = defaultAnimationProfile.MyJumpClip;
                    }
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.MyCombatJumpClip.name)) {
                if (currentAnimationProfile.MyCombatJumpClip != null) {
                    if (overrideController[systemAnimations.MyCombatJumpClip.name] != currentAnimationProfile.MyCombatJumpClip) {
                        overrideController[systemAnimations.MyCombatJumpClip.name] = currentAnimationProfile.MyCombatJumpClip;
                        currentAnimations.MyCombatJumpClip = currentAnimationProfile.MyCombatJumpClip;
                    }
                } else {
                    if (defaultAnimationProfile.MyCombatJumpClip != null && overrideController[systemAnimations.MyCombatJumpClip.name] != defaultAnimationProfile.MyCombatJumpClip) {
                        overrideController[systemAnimations.MyCombatJumpClip.name] = defaultAnimationProfile.MyCombatJumpClip;
                        currentAnimations.MyCombatJumpClip = defaultAnimationProfile.MyCombatJumpClip;
                    }
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.MyIdleClip.name)) {
                if (currentAnimationProfile.MyIdleClip != null) {
                    if (overrideController[systemAnimations.MyIdleClip.name] != currentAnimationProfile.MyIdleClip) {
                        overrideController[systemAnimations.MyIdleClip.name] = currentAnimationProfile.MyIdleClip;
                        currentAnimations.MyIdleClip = currentAnimationProfile.MyIdleClip;
                    }
                } else {
                    if (defaultAnimationProfile.MyIdleClip != null && overrideController[systemAnimations.MyIdleClip.name] != defaultAnimationProfile.MyIdleClip) {
                        overrideController[systemAnimations.MyIdleClip.name] = defaultAnimationProfile.MyIdleClip;
                        currentAnimations.MyIdleClip = defaultAnimationProfile.MyIdleClip;
                    }
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.MyCombatIdleClip.name)) {
                if (currentAnimationProfile.MyCombatIdleClip != null) {
                    if (overrideController[systemAnimations.MyCombatIdleClip.name] != currentAnimationProfile.MyCombatIdleClip) {
                        overrideController[systemAnimations.MyCombatIdleClip.name] = currentAnimationProfile.MyCombatIdleClip;
                        currentAnimations.MyCombatIdleClip = currentAnimationProfile.MyCombatIdleClip;
                    }
                } else {
                    if (defaultAnimationProfile.MyCombatIdleClip != null && overrideController[systemAnimations.MyCombatIdleClip.name] != defaultAnimationProfile.MyCombatIdleClip) {
                        overrideController[systemAnimations.MyCombatIdleClip.name] = defaultAnimationProfile.MyCombatIdleClip;
                        currentAnimations.MyCombatIdleClip = defaultAnimationProfile.MyCombatIdleClip;
                    }
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.MyLandClip.name)) {
                if (currentAnimationProfile.MyLandClip != null) {
                    if (overrideController[systemAnimations.MyLandClip.name] != currentAnimationProfile.MyLandClip) {
                        overrideController[systemAnimations.MyLandClip.name] = currentAnimationProfile.MyLandClip;
                        currentAnimations.MyLandClip = currentAnimationProfile.MyLandClip;
                    }
                } else {
                    if (defaultAnimationProfile.MyLandClip != null && overrideController[systemAnimations.MyLandClip.name] != defaultAnimationProfile.MyLandClip) {
                        overrideController[systemAnimations.MyLandClip.name] = defaultAnimationProfile.MyLandClip;
                        currentAnimations.MyLandClip = defaultAnimationProfile.MyLandClip;
                    }
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.MyCombatLandClip.name)) {
                if (currentAnimationProfile.MyCombatLandClip != null) {
                    if (overrideController[systemAnimations.MyCombatLandClip.name] != currentAnimationProfile.MyCombatLandClip) {
                        overrideController[systemAnimations.MyCombatLandClip.name] = currentAnimationProfile.MyCombatLandClip;
                        currentAnimations.MyCombatLandClip = currentAnimationProfile.MyCombatLandClip;
                    }
                } else {
                    if (defaultAnimationProfile.MyCombatLandClip != null && overrideController[systemAnimations.MyCombatLandClip.name] != defaultAnimationProfile.MyCombatLandClip) {
                        overrideController[systemAnimations.MyCombatLandClip.name] = defaultAnimationProfile.MyCombatLandClip;
                        currentAnimations.MyCombatLandClip = defaultAnimationProfile.MyCombatLandClip;
                    }
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.MyFallClip.name)) {
                if (currentAnimationProfile.MyFallClip != null) {
                    if (overrideController[systemAnimations.MyFallClip.name] != currentAnimationProfile.MyFallClip) {
                        overrideController[systemAnimations.MyFallClip.name] = currentAnimationProfile.MyFallClip;
                        currentAnimations.MyFallClip = currentAnimationProfile.MyFallClip;
                    }
                } else {
                    if (defaultAnimationProfile.MyFallClip != null && overrideController[systemAnimations.MyFallClip.name] != defaultAnimationProfile.MyFallClip) {
                        overrideController[systemAnimations.MyFallClip.name] = defaultAnimationProfile.MyFallClip;
                        currentAnimations.MyFallClip = defaultAnimationProfile.MyFallClip;
                    }
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.MyCombatFallClip.name)) {
                if (currentAnimationProfile.MyCombatFallClip != null) {
                    if (overrideController[systemAnimations.MyCombatFallClip.name] != currentAnimationProfile.MyCombatFallClip) {
                        overrideController[systemAnimations.MyCombatFallClip.name] = currentAnimationProfile.MyCombatFallClip;
                        currentAnimations.MyCombatFallClip = currentAnimationProfile.MyCombatFallClip;
                    }
                } else {
                    if (defaultAnimationProfile.MyCombatFallClip != null && overrideController[systemAnimations.MyCombatFallClip.name] != defaultAnimationProfile.MyCombatFallClip) {
                        overrideController[systemAnimations.MyCombatFallClip.name] = defaultAnimationProfile.MyCombatFallClip;
                        currentAnimations.MyCombatFallClip = defaultAnimationProfile.MyCombatFallClip;
                    }
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.MyStrafeLeftClip.name)) {
                if (currentAnimationProfile.MyStrafeLeftClip != null) {
                    if (overrideController[systemAnimations.MyStrafeLeftClip.name] != currentAnimationProfile.MyStrafeLeftClip) {
                        overrideController[systemAnimations.MyStrafeLeftClip.name] = currentAnimationProfile.MyStrafeLeftClip;
                        currentAnimations.MyStrafeLeftClip = currentAnimationProfile.MyStrafeLeftClip;
                    }
                } else {
                    if (defaultAnimationProfile.MyStrafeLeftClip != null && overrideController[systemAnimations.MyStrafeLeftClip.name] != defaultAnimationProfile.MyStrafeLeftClip) {
                        overrideController[systemAnimations.MyStrafeLeftClip.name] = defaultAnimationProfile.MyStrafeLeftClip;
                        currentAnimations.MyStrafeLeftClip = defaultAnimationProfile.MyStrafeLeftClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.MyStrafeLeftClip.averageSpeed.x) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseWalkStrafeLeftAnimationSpeed = Mathf.Abs(currentAnimations.MyStrafeLeftClip.averageSpeed.x);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.MyJogStrafeLeftClip.name)) {
                if (currentAnimationProfile.MyJogStrafeLeftClip != null) {
                    if (overrideController[systemAnimations.MyJogStrafeLeftClip.name] != currentAnimationProfile.MyJogStrafeLeftClip) {
                        overrideController[systemAnimations.MyJogStrafeLeftClip.name] = currentAnimationProfile.MyJogStrafeLeftClip;
                        currentAnimations.MyJogStrafeLeftClip = currentAnimationProfile.MyJogStrafeLeftClip;
                    }
                } else {
                    if (defaultAnimationProfile.MyJogStrafeLeftClip != null && overrideController[systemAnimations.MyJogStrafeLeftClip.name] != defaultAnimationProfile.MyJogStrafeLeftClip) {
                        overrideController[systemAnimations.MyJogStrafeLeftClip.name] = defaultAnimationProfile.MyJogStrafeLeftClip;
                        currentAnimations.MyJogStrafeLeftClip = defaultAnimationProfile.MyJogStrafeLeftClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.MyJogStrafeLeftClip.averageSpeed.x) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseJogStrafeLeftAnimationSpeed = Mathf.Abs(currentAnimations.MyJogStrafeLeftClip.averageSpeed.x);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.MyStrafeRightClip.name)) {
                if (currentAnimationProfile.MyStrafeRightClip != null) {
                    if (overrideController[systemAnimations.MyStrafeRightClip.name] != currentAnimationProfile.MyStrafeRightClip) {
                        overrideController[systemAnimations.MyStrafeRightClip.name] = currentAnimationProfile.MyStrafeRightClip;
                        currentAnimations.MyStrafeRightClip = currentAnimationProfile.MyStrafeRightClip;
                    }
                } else {
                    if (defaultAnimationProfile.MyStrafeRightClip != null && overrideController[systemAnimations.MyStrafeRightClip.name] != defaultAnimationProfile.MyStrafeRightClip) {
                        overrideController[systemAnimations.MyStrafeRightClip.name] = defaultAnimationProfile.MyStrafeRightClip;
                        currentAnimations.MyStrafeRightClip = defaultAnimationProfile.MyStrafeRightClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.MyStrafeRightClip.averageSpeed.x) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseWalkStrafeRightAnimationSpeed = Mathf.Abs(currentAnimations.MyStrafeRightClip.averageSpeed.x);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.MyJogStrafeRightClip.name)) {
                if (currentAnimationProfile.MyJogStrafeRightClip != null) {
                    if (overrideController[systemAnimations.MyJogStrafeRightClip.name] != currentAnimationProfile.MyJogStrafeRightClip) {
                        overrideController[systemAnimations.MyJogStrafeRightClip.name] = currentAnimationProfile.MyJogStrafeRightClip;
                        currentAnimations.MyJogStrafeRightClip = currentAnimationProfile.MyJogStrafeRightClip;
                    }
                } else {
                    if (defaultAnimationProfile.MyJogStrafeRightClip != null && overrideController[systemAnimations.MyJogStrafeRightClip.name] != defaultAnimationProfile.MyJogStrafeRightClip) {
                        overrideController[systemAnimations.MyJogStrafeRightClip.name] = defaultAnimationProfile.MyJogStrafeRightClip;
                        currentAnimations.MyJogStrafeRightClip = defaultAnimationProfile.MyJogStrafeRightClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.MyJogStrafeRightClip.averageSpeed.x) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseJogStrafeRightAnimationSpeed = Mathf.Abs(currentAnimations.MyJogStrafeRightClip.averageSpeed.x);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.MyStrafeForwardRightClip.name)) {
                if (currentAnimationProfile.MyStrafeForwardRightClip != null) {
                    if (overrideController[systemAnimations.MyStrafeForwardRightClip.name] != currentAnimationProfile.MyStrafeForwardRightClip) {
                        overrideController[systemAnimations.MyStrafeForwardRightClip.name] = currentAnimationProfile.MyStrafeForwardRightClip;
                        currentAnimations.MyStrafeForwardRightClip = currentAnimationProfile.MyStrafeForwardRightClip;
                    }
                } else {
                    if (defaultAnimationProfile.MyStrafeForwardRightClip != null && overrideController[systemAnimations.MyStrafeForwardRightClip.name] != defaultAnimationProfile.MyStrafeForwardRightClip) {
                        overrideController[systemAnimations.MyStrafeForwardRightClip.name] = defaultAnimationProfile.MyStrafeForwardRightClip;
                        currentAnimations.MyStrafeForwardRightClip = defaultAnimationProfile.MyStrafeForwardRightClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.MyStrafeForwardRightClip.averageSpeed.magnitude) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseWalkStrafeForwardRightAnimationSpeed = Mathf.Abs(currentAnimations.MyStrafeForwardRightClip.averageSpeed.magnitude);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.MyJogStrafeForwardRightClip.name)) {
                if (currentAnimationProfile.MyJogStrafeForwardRightClip != null) {
                    if (overrideController[systemAnimations.MyJogStrafeForwardRightClip.name] != currentAnimationProfile.MyJogStrafeForwardRightClip) {
                        overrideController[systemAnimations.MyJogStrafeForwardRightClip.name] = currentAnimationProfile.MyJogStrafeForwardRightClip;
                        currentAnimations.MyJogStrafeForwardRightClip = currentAnimationProfile.MyJogStrafeForwardRightClip;
                    }
                } else {
                    if (defaultAnimationProfile.MyJogStrafeForwardRightClip != null && overrideController[systemAnimations.MyJogStrafeForwardRightClip.name] != defaultAnimationProfile.MyJogStrafeForwardRightClip) {
                        overrideController[systemAnimations.MyJogStrafeForwardRightClip.name] = defaultAnimationProfile.MyJogStrafeForwardRightClip;
                        currentAnimations.MyJogStrafeForwardRightClip = defaultAnimationProfile.MyJogStrafeForwardRightClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.MyJogStrafeForwardRightClip.averageSpeed.magnitude) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseJogStrafeForwardRightAnimationSpeed = Mathf.Abs(currentAnimations.MyJogStrafeForwardRightClip.averageSpeed.magnitude);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.MyStrafeForwardLeftClip.name)) {
                if (currentAnimationProfile.MyStrafeForwardLeftClip != null) {
                    if (overrideController[systemAnimations.MyStrafeForwardLeftClip.name] != currentAnimationProfile.MyStrafeForwardLeftClip) {
                        overrideController[systemAnimations.MyStrafeForwardLeftClip.name] = currentAnimationProfile.MyStrafeForwardLeftClip;
                        currentAnimations.MyStrafeForwardLeftClip = currentAnimationProfile.MyStrafeForwardLeftClip;
                    }
                } else {
                    if (defaultAnimationProfile.MyStrafeForwardLeftClip != null && overrideController[systemAnimations.MyStrafeForwardLeftClip.name] != defaultAnimationProfile.MyStrafeForwardLeftClip) {
                        overrideController[systemAnimations.MyStrafeForwardLeftClip.name] = defaultAnimationProfile.MyStrafeForwardLeftClip;
                        currentAnimations.MyStrafeForwardLeftClip = defaultAnimationProfile.MyStrafeForwardLeftClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.MyStrafeForwardLeftClip.averageSpeed.magnitude) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseWalkStrafeForwardLeftAnimationSpeed = Mathf.Abs(currentAnimations.MyStrafeForwardLeftClip.averageSpeed.magnitude);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.MyJogStrafeForwardLeftClip.name)) {
                if (currentAnimationProfile.MyJogStrafeForwardLeftClip != null) {
                    if (overrideController[systemAnimations.MyJogStrafeForwardLeftClip.name] != currentAnimationProfile.MyJogStrafeForwardLeftClip) {
                        overrideController[systemAnimations.MyJogStrafeForwardLeftClip.name] = currentAnimationProfile.MyJogStrafeForwardLeftClip;
                        currentAnimations.MyJogStrafeForwardLeftClip = currentAnimationProfile.MyJogStrafeForwardLeftClip;
                    }
                } else {
                    if (defaultAnimationProfile.MyJogStrafeForwardLeftClip != null && overrideController[systemAnimations.MyJogStrafeForwardLeftClip.name] != defaultAnimationProfile.MyJogStrafeForwardLeftClip) {
                        overrideController[systemAnimations.MyJogStrafeForwardLeftClip.name] = defaultAnimationProfile.MyJogStrafeForwardLeftClip;
                        currentAnimations.MyJogStrafeForwardLeftClip = defaultAnimationProfile.MyJogStrafeForwardLeftClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.MyJogStrafeForwardLeftClip.averageSpeed.magnitude) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseJogStrafeForwardLeftAnimationSpeed = Mathf.Abs(currentAnimations.MyJogStrafeForwardLeftClip.averageSpeed.magnitude);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.MyStrafeBackLeftClip.name)) {
                if (currentAnimationProfile.MyStrafeBackLeftClip != null) {
                    if (overrideController[systemAnimations.MyStrafeBackLeftClip.name] != currentAnimationProfile.MyStrafeBackLeftClip) {
                        overrideController[systemAnimations.MyStrafeBackLeftClip.name] = currentAnimationProfile.MyStrafeBackLeftClip;
                        currentAnimations.MyStrafeBackLeftClip = currentAnimationProfile.MyStrafeBackLeftClip;
                    }
                } else {
                    if (defaultAnimationProfile.MyStrafeBackLeftClip != null && overrideController[systemAnimations.MyStrafeBackLeftClip.name] != defaultAnimationProfile.MyStrafeBackLeftClip) {
                        overrideController[systemAnimations.MyStrafeBackLeftClip.name] = defaultAnimationProfile.MyStrafeBackLeftClip;
                        currentAnimations.MyStrafeBackLeftClip = defaultAnimationProfile.MyStrafeBackLeftClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.MyStrafeBackLeftClip.averageSpeed.magnitude) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseWalkStrafeBackLeftAnimationSpeed = Mathf.Abs(currentAnimations.MyStrafeBackLeftClip.averageSpeed.magnitude);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.MyJogStrafeBackLeftClip.name)) {
                if (currentAnimationProfile.MyJogStrafeBackLeftClip != null) {
                    if (overrideController[systemAnimations.MyJogStrafeBackLeftClip.name] != currentAnimationProfile.MyJogStrafeBackLeftClip) {
                        overrideController[systemAnimations.MyJogStrafeBackLeftClip.name] = currentAnimationProfile.MyJogStrafeBackLeftClip;
                        currentAnimations.MyJogStrafeBackLeftClip = currentAnimationProfile.MyJogStrafeBackLeftClip;
                    }
                } else {
                    if (defaultAnimationProfile.MyJogStrafeBackLeftClip != null && overrideController[systemAnimations.MyJogStrafeBackLeftClip.name] != defaultAnimationProfile.MyJogStrafeBackLeftClip) {
                        overrideController[systemAnimations.MyJogStrafeBackLeftClip.name] = defaultAnimationProfile.MyJogStrafeBackLeftClip;
                        currentAnimations.MyJogStrafeBackLeftClip = defaultAnimationProfile.MyJogStrafeBackLeftClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.MyJogStrafeBackLeftClip.averageSpeed.magnitude) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseJogStrafeBackLeftAnimationSpeed = Mathf.Abs(currentAnimations.MyJogStrafeBackLeftClip.averageSpeed.magnitude);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.MyStrafeBackRightClip.name)) {
                if (currentAnimationProfile.MyStrafeBackRightClip != null) {
                    if (overrideController[systemAnimations.MyStrafeBackRightClip.name] != currentAnimationProfile.MyStrafeBackRightClip) {
                        overrideController[systemAnimations.MyStrafeBackRightClip.name] = currentAnimationProfile.MyStrafeBackRightClip;
                        currentAnimations.MyStrafeBackRightClip = currentAnimationProfile.MyStrafeBackRightClip;
                    }
                } else {
                    if (defaultAnimationProfile.MyStrafeBackRightClip != null && overrideController[systemAnimations.MyStrafeBackRightClip.name] != defaultAnimationProfile.MyStrafeBackRightClip) {
                        overrideController[systemAnimations.MyStrafeBackRightClip.name] = defaultAnimationProfile.MyStrafeBackRightClip;
                        currentAnimations.MyStrafeBackRightClip = defaultAnimationProfile.MyStrafeBackRightClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.MyStrafeBackRightClip.averageSpeed.magnitude) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseWalkStrafeBackRightAnimationSpeed = Mathf.Abs(currentAnimations.MyStrafeBackRightClip.averageSpeed.magnitude);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.MyJogStrafeBackRightClip.name)) {
                if (currentAnimationProfile.MyJogStrafeBackRightClip != null) {
                    if (overrideController[systemAnimations.MyJogStrafeBackRightClip.name] != currentAnimationProfile.MyJogStrafeBackRightClip) {
                        overrideController[systemAnimations.MyJogStrafeBackRightClip.name] = currentAnimationProfile.MyJogStrafeBackRightClip;
                        currentAnimations.MyJogStrafeBackRightClip = currentAnimationProfile.MyJogStrafeBackRightClip;
                    }
                } else {
                    if (defaultAnimationProfile.MyJogStrafeBackRightClip != null && overrideController[systemAnimations.MyJogStrafeBackRightClip.name] != defaultAnimationProfile.MyJogStrafeBackRightClip) {
                        overrideController[systemAnimations.MyJogStrafeBackRightClip.name] = defaultAnimationProfile.MyJogStrafeBackRightClip;
                        currentAnimations.MyJogStrafeBackRightClip = defaultAnimationProfile.MyJogStrafeBackRightClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.MyJogStrafeBackRightClip.averageSpeed.magnitude) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseJogStrafeBackRightAnimationSpeed = Mathf.Abs(currentAnimations.MyJogStrafeBackRightClip.averageSpeed.magnitude);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.MyCombatStrafeLeftClip.name)) {
                if (currentAnimationProfile.MyCombatStrafeLeftClip != null) {
                    if (overrideController[systemAnimations.MyCombatStrafeLeftClip.name] != currentAnimationProfile.MyCombatStrafeLeftClip) {
                        overrideController[systemAnimations.MyCombatStrafeLeftClip.name] = currentAnimationProfile.MyCombatStrafeLeftClip;
                        currentAnimations.MyCombatStrafeLeftClip = currentAnimationProfile.MyCombatStrafeLeftClip;
                    }
                } else {
                    if (defaultAnimationProfile.MyCombatStrafeLeftClip != null && overrideController[systemAnimations.MyCombatStrafeLeftClip.name] != defaultAnimationProfile.MyCombatStrafeLeftClip) {
                        overrideController[systemAnimations.MyCombatStrafeLeftClip.name] = defaultAnimationProfile.MyCombatStrafeLeftClip;
                        currentAnimations.MyCombatStrafeLeftClip = defaultAnimationProfile.MyCombatStrafeLeftClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.MyCombatStrafeLeftClip.averageSpeed.x) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseCombatWalkStrafeLeftAnimationSpeed = Mathf.Abs(currentAnimations.MyCombatStrafeLeftClip.averageSpeed.x);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.MyCombatJogStrafeLeftClip.name)) {
                if (currentAnimationProfile.MyCombatJogStrafeLeftClip != null) {
                    if (overrideController[systemAnimations.MyCombatJogStrafeLeftClip.name] != currentAnimationProfile.MyCombatJogStrafeLeftClip) {
                        overrideController[systemAnimations.MyCombatJogStrafeLeftClip.name] = currentAnimationProfile.MyCombatJogStrafeLeftClip;
                        currentAnimations.MyCombatJogStrafeLeftClip = currentAnimationProfile.MyCombatJogStrafeLeftClip;
                    }
                } else {
                    if (defaultAnimationProfile.MyCombatJogStrafeLeftClip != null && overrideController[systemAnimations.MyCombatJogStrafeLeftClip.name] != defaultAnimationProfile.MyCombatJogStrafeLeftClip) {
                        overrideController[systemAnimations.MyCombatJogStrafeLeftClip.name] = defaultAnimationProfile.MyCombatJogStrafeLeftClip;
                        currentAnimations.MyCombatJogStrafeLeftClip = defaultAnimationProfile.MyCombatJogStrafeLeftClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.MyCombatJogStrafeLeftClip.averageSpeed.x) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseCombatJogStrafeLeftAnimationSpeed = Mathf.Abs(currentAnimations.MyCombatJogStrafeLeftClip.averageSpeed.x);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.MyCombatStrafeRightClip.name)) {
                if (currentAnimationProfile.MyCombatStrafeRightClip != null) {
                    if (overrideController[systemAnimations.MyCombatStrafeRightClip.name] != currentAnimationProfile.MyCombatStrafeRightClip) {
                        overrideController[systemAnimations.MyCombatStrafeRightClip.name] = currentAnimationProfile.MyCombatStrafeRightClip;
                        currentAnimations.MyCombatStrafeRightClip = currentAnimationProfile.MyCombatStrafeRightClip;
                    }
                } else {
                    if (defaultAnimationProfile.MyCombatStrafeRightClip != null && overrideController[systemAnimations.MyCombatStrafeRightClip.name] != defaultAnimationProfile.MyCombatStrafeRightClip) {
                        overrideController[systemAnimations.MyCombatStrafeRightClip.name] = defaultAnimationProfile.MyCombatStrafeRightClip;
                        currentAnimations.MyCombatStrafeRightClip = defaultAnimationProfile.MyCombatStrafeRightClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.MyCombatStrafeRightClip.averageSpeed.x) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseCombatWalkStrafeRightAnimationSpeed = Mathf.Abs(currentAnimations.MyCombatStrafeRightClip.averageSpeed.x);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.MyCombatJogStrafeRightClip.name)) {
                if (currentAnimationProfile.MyCombatJogStrafeRightClip != null) {
                    if (overrideController[systemAnimations.MyCombatJogStrafeRightClip.name] != currentAnimationProfile.MyCombatJogStrafeRightClip) {
                        overrideController[systemAnimations.MyCombatJogStrafeRightClip.name] = currentAnimationProfile.MyCombatJogStrafeRightClip;
                        currentAnimations.MyCombatJogStrafeRightClip = currentAnimationProfile.MyCombatJogStrafeRightClip;
                    }
                } else {
                    if (defaultAnimationProfile.MyCombatJogStrafeRightClip != null && overrideController[systemAnimations.MyCombatJogStrafeRightClip.name] != defaultAnimationProfile.MyCombatJogStrafeRightClip) {
                        overrideController[systemAnimations.MyCombatJogStrafeRightClip.name] = defaultAnimationProfile.MyCombatJogStrafeRightClip;
                        currentAnimations.MyCombatJogStrafeRightClip = defaultAnimationProfile.MyCombatJogStrafeRightClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.MyCombatJogStrafeRightClip.averageSpeed.x) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseCombatJogStrafeRightAnimationSpeed = Mathf.Abs(currentAnimations.MyCombatJogStrafeRightClip.averageSpeed.x);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.MyCombatStrafeForwardRightClip.name)) {
                if (currentAnimationProfile.MyCombatStrafeForwardRightClip != null) {
                    if (overrideController[systemAnimations.MyCombatStrafeForwardRightClip.name] != currentAnimationProfile.MyCombatStrafeForwardRightClip) {
                        overrideController[systemAnimations.MyCombatStrafeForwardRightClip.name] = currentAnimationProfile.MyCombatStrafeForwardRightClip;
                        currentAnimations.MyCombatStrafeForwardRightClip = currentAnimationProfile.MyCombatStrafeForwardRightClip;
                    }
                } else {
                    if (defaultAnimationProfile.MyCombatStrafeForwardRightClip != null && overrideController[systemAnimations.MyCombatStrafeForwardRightClip.name] != defaultAnimationProfile.MyCombatStrafeForwardRightClip) {
                        overrideController[systemAnimations.MyCombatStrafeForwardRightClip.name] = defaultAnimationProfile.MyCombatStrafeForwardRightClip;
                        currentAnimations.MyCombatStrafeForwardRightClip = defaultAnimationProfile.MyCombatStrafeForwardRightClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.MyCombatStrafeForwardRightClip.averageSpeed.magnitude) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseCombatWalkStrafeForwardRightAnimationSpeed = Mathf.Abs(currentAnimations.MyCombatStrafeForwardRightClip.averageSpeed.magnitude);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.MyCombatJogStrafeForwardRightClip.name)) {
                if (currentAnimationProfile.MyCombatJogStrafeForwardRightClip != null) {
                    if (overrideController[systemAnimations.MyCombatJogStrafeForwardRightClip.name] != currentAnimationProfile.MyCombatJogStrafeForwardRightClip) {
                        overrideController[systemAnimations.MyCombatJogStrafeForwardRightClip.name] = currentAnimationProfile.MyCombatJogStrafeForwardRightClip;
                        currentAnimations.MyCombatJogStrafeForwardRightClip = currentAnimationProfile.MyCombatJogStrafeForwardRightClip;
                    }
                } else {
                    if (defaultAnimationProfile.MyCombatJogStrafeForwardRightClip != null && overrideController[systemAnimations.MyCombatJogStrafeForwardRightClip.name] != defaultAnimationProfile.MyCombatJogStrafeForwardRightClip) {
                        overrideController[systemAnimations.MyCombatJogStrafeForwardRightClip.name] = defaultAnimationProfile.MyCombatJogStrafeForwardRightClip;
                        currentAnimations.MyCombatJogStrafeForwardRightClip = defaultAnimationProfile.MyCombatJogStrafeForwardRightClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.MyCombatJogStrafeForwardRightClip.averageSpeed.magnitude) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseCombatJogStrafeForwardRightAnimationSpeed = Mathf.Abs(currentAnimations.MyCombatJogStrafeForwardRightClip.averageSpeed.magnitude);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.MyCombatStrafeForwardLeftClip.name)) {
                if (currentAnimationProfile.MyCombatStrafeForwardLeftClip != null) {
                    if (overrideController[systemAnimations.MyCombatStrafeForwardLeftClip.name] != currentAnimationProfile.MyCombatStrafeForwardLeftClip) {
                        overrideController[systemAnimations.MyCombatStrafeForwardLeftClip.name] = currentAnimationProfile.MyCombatStrafeForwardLeftClip;
                        currentAnimations.MyCombatStrafeForwardLeftClip = currentAnimationProfile.MyCombatStrafeForwardLeftClip;
                    }
                } else {
                    if (defaultAnimationProfile.MyCombatStrafeForwardLeftClip != null && overrideController[systemAnimations.MyCombatStrafeForwardLeftClip.name] != defaultAnimationProfile.MyCombatStrafeForwardLeftClip) {
                        overrideController[systemAnimations.MyCombatStrafeForwardLeftClip.name] = defaultAnimationProfile.MyCombatStrafeForwardLeftClip;
                        currentAnimations.MyCombatStrafeForwardLeftClip = defaultAnimationProfile.MyCombatStrafeForwardLeftClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.MyCombatStrafeForwardLeftClip.averageSpeed.magnitude) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseCombatWalkStrafeForwardLeftAnimationSpeed = Mathf.Abs(currentAnimations.MyCombatStrafeForwardLeftClip.averageSpeed.magnitude);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.MyCombatJogStrafeForwardLeftClip.name)) {
                if (currentAnimationProfile.MyCombatJogStrafeForwardLeftClip != null) {
                    if (overrideController[systemAnimations.MyCombatJogStrafeForwardLeftClip.name] != currentAnimationProfile.MyCombatJogStrafeForwardLeftClip) {
                        overrideController[systemAnimations.MyCombatJogStrafeForwardLeftClip.name] = currentAnimationProfile.MyCombatJogStrafeForwardLeftClip;
                        currentAnimations.MyCombatJogStrafeForwardLeftClip = currentAnimationProfile.MyCombatJogStrafeForwardLeftClip;
                    }
                } else {
                    if (defaultAnimationProfile.MyCombatJogStrafeForwardLeftClip != null && overrideController[systemAnimations.MyCombatJogStrafeForwardLeftClip.name] != defaultAnimationProfile.MyCombatJogStrafeForwardLeftClip) {
                        overrideController[systemAnimations.MyCombatJogStrafeForwardLeftClip.name] = defaultAnimationProfile.MyCombatJogStrafeForwardLeftClip;
                        currentAnimations.MyCombatJogStrafeForwardLeftClip = defaultAnimationProfile.MyCombatJogStrafeForwardLeftClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.MyCombatJogStrafeForwardLeftClip.averageSpeed.magnitude) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseCombatJogStrafeForwardLeftAnimationSpeed = Mathf.Abs(currentAnimations.MyCombatJogStrafeForwardLeftClip.averageSpeed.magnitude);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.MyCombatStrafeBackLeftClip.name)) {
                if (currentAnimationProfile.MyCombatStrafeBackLeftClip != null) {
                    if (overrideController[systemAnimations.MyCombatStrafeBackLeftClip.name] != currentAnimationProfile.MyCombatStrafeBackLeftClip) {
                        overrideController[systemAnimations.MyCombatStrafeBackLeftClip.name] = currentAnimationProfile.MyCombatStrafeBackLeftClip;
                        currentAnimations.MyCombatStrafeBackLeftClip = currentAnimationProfile.MyCombatStrafeBackLeftClip;
                    }
                } else {
                    if (defaultAnimationProfile.MyCombatStrafeBackLeftClip != null && overrideController[systemAnimations.MyCombatStrafeBackLeftClip.name] != defaultAnimationProfile.MyCombatStrafeBackLeftClip) {
                        overrideController[systemAnimations.MyCombatStrafeBackLeftClip.name] = defaultAnimationProfile.MyCombatStrafeBackLeftClip;
                        currentAnimations.MyCombatStrafeBackLeftClip = defaultAnimationProfile.MyCombatStrafeBackLeftClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.MyCombatStrafeBackLeftClip.averageSpeed.magnitude) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseCombatWalkStrafeBackLeftAnimationSpeed = Mathf.Abs(currentAnimations.MyCombatStrafeBackLeftClip.averageSpeed.magnitude);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.MyCombatJogStrafeBackLeftClip.name)) {
                if (currentAnimationProfile.MyCombatJogStrafeBackLeftClip != null) {
                    if (overrideController[systemAnimations.MyCombatJogStrafeBackLeftClip.name] != currentAnimationProfile.MyCombatJogStrafeBackLeftClip) {
                        overrideController[systemAnimations.MyCombatJogStrafeBackLeftClip.name] = currentAnimationProfile.MyCombatJogStrafeBackLeftClip;
                        currentAnimations.MyCombatJogStrafeBackLeftClip = currentAnimationProfile.MyCombatJogStrafeBackLeftClip;
                    }
                } else {
                    if (defaultAnimationProfile.MyCombatJogStrafeBackLeftClip != null && overrideController[systemAnimations.MyCombatJogStrafeBackLeftClip.name] != defaultAnimationProfile.MyCombatJogStrafeBackLeftClip) {
                        overrideController[systemAnimations.MyCombatJogStrafeBackLeftClip.name] = defaultAnimationProfile.MyCombatJogStrafeBackLeftClip;
                        currentAnimations.MyCombatJogStrafeBackLeftClip = defaultAnimationProfile.MyCombatJogStrafeBackLeftClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.MyCombatJogStrafeBackLeftClip.averageSpeed.magnitude) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseCombatJogStrafeBackLeftAnimationSpeed = Mathf.Abs(currentAnimations.MyCombatJogStrafeBackLeftClip.averageSpeed.magnitude);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.MyCombatStrafeBackRightClip.name)) {
                if (currentAnimationProfile.MyCombatStrafeBackRightClip != null) {
                    if (overrideController[systemAnimations.MyCombatStrafeBackRightClip.name] != currentAnimationProfile.MyCombatStrafeBackRightClip) {
                        overrideController[systemAnimations.MyCombatStrafeBackRightClip.name] = currentAnimationProfile.MyCombatStrafeBackRightClip;
                        currentAnimations.MyCombatStrafeBackRightClip = currentAnimationProfile.MyCombatStrafeBackRightClip;
                    }
                } else {
                    if (defaultAnimationProfile.MyCombatStrafeBackRightClip != null && overrideController[systemAnimations.MyCombatStrafeBackRightClip.name] != defaultAnimationProfile.MyCombatStrafeBackRightClip) {
                        overrideController[systemAnimations.MyCombatStrafeBackRightClip.name] = defaultAnimationProfile.MyCombatStrafeBackRightClip;
                        currentAnimations.MyCombatStrafeBackRightClip = defaultAnimationProfile.MyCombatStrafeBackRightClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.MyCombatStrafeBackRightClip.averageSpeed.magnitude) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseCombatWalkStrafeBackRightAnimationSpeed = Mathf.Abs(currentAnimations.MyCombatStrafeBackRightClip.averageSpeed.magnitude);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.MyCombatJogStrafeBackRightClip.name)) {
                if (currentAnimationProfile.MyCombatJogStrafeBackRightClip != null) {
                    if (overrideController[systemAnimations.MyCombatJogStrafeBackRightClip.name] != currentAnimationProfile.MyCombatJogStrafeBackRightClip) {
                        overrideController[systemAnimations.MyCombatJogStrafeBackRightClip.name] = currentAnimationProfile.MyCombatJogStrafeBackRightClip;
                        currentAnimations.MyCombatJogStrafeBackRightClip = currentAnimationProfile.MyCombatJogStrafeBackRightClip;
                    }
                } else {
                    if (defaultAnimationProfile.MyCombatJogStrafeBackRightClip != null && overrideController[systemAnimations.MyCombatJogStrafeBackRightClip.name] != defaultAnimationProfile.MyCombatJogStrafeBackRightClip) {
                        overrideController[systemAnimations.MyCombatJogStrafeBackRightClip.name] = defaultAnimationProfile.MyCombatJogStrafeBackRightClip;
                        currentAnimations.MyCombatJogStrafeBackRightClip = defaultAnimationProfile.MyCombatJogStrafeBackRightClip;
                    }
                }
                if (Mathf.Abs(currentAnimations.MyCombatJogStrafeBackRightClip.averageSpeed.magnitude) > 0.1) {
                    // our clip has forward motion.  override the default animation motion speed of 2
                    baseCombatJogStrafeBackRightAnimationSpeed = Mathf.Abs(currentAnimations.MyCombatJogStrafeBackRightClip.averageSpeed.magnitude);
                    //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
                }
            }

            //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): Death is not null.");
            if (overrideControllerClipList.Contains(systemAnimations.MyDeathClip.name)) {
                if (currentAnimationProfile.MyDeathClip != null) {
                    if (overrideController[systemAnimations.MyDeathClip.name] != currentAnimationProfile.MyDeathClip) {
                        overrideController[systemAnimations.MyDeathClip.name] = currentAnimationProfile.MyDeathClip;
                        currentAnimations.MyDeathClip = currentAnimationProfile.MyDeathClip;
                    }
                } else {
                    if (defaultAnimationProfile.MyDeathClip != null && overrideController[systemAnimations.MyDeathClip.name] != defaultAnimationProfile.MyDeathClip) {
                        overrideController[systemAnimations.MyDeathClip.name] = defaultAnimationProfile.MyDeathClip;
                        currentAnimations.MyDeathClip = defaultAnimationProfile.MyDeathClip;
                    }
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.MyStunnedClip.name)) {
                if (currentAnimationProfile.MyStunnedClip != null) {
                    if (overrideController[systemAnimations.MyStunnedClip.name] != currentAnimationProfile.MyStunnedClip) {
                        overrideController[systemAnimations.MyStunnedClip.name] = currentAnimationProfile.MyStunnedClip;
                        currentAnimations.MyStunnedClip = currentAnimationProfile.MyStunnedClip;
                    }
                } else {
                    if (defaultAnimationProfile.MyStunnedClip != null && overrideController[systemAnimations.MyStunnedClip.name] != defaultAnimationProfile.MyStunnedClip) {
                        overrideController[systemAnimations.MyStunnedClip.name] = defaultAnimationProfile.MyStunnedClip;
                        currentAnimations.MyStunnedClip = defaultAnimationProfile.MyStunnedClip;
                    }
                }
            }

            if (overrideControllerClipList.Contains(systemAnimations.MyLevitatedClip.name)) {
                if (currentAnimationProfile.MyLevitatedClip != null) {
                    if (overrideController[systemAnimations.MyLevitatedClip.name] != currentAnimationProfile.MyLevitatedClip) {
                        overrideController[systemAnimations.MyLevitatedClip.name] = currentAnimationProfile.MyLevitatedClip;
                        currentAnimations.MyLevitatedClip = currentAnimationProfile.MyLevitatedClip;
                    }
                } else {
                    if (defaultAnimationProfile.MyLevitatedClip != null && overrideController[systemAnimations.MyLevitatedClip.name] != defaultAnimationProfile.MyLevitatedClip) {
                        overrideController[systemAnimations.MyLevitatedClip.name] = defaultAnimationProfile.MyLevitatedClip;
                        currentAnimations.MyLevitatedClip = defaultAnimationProfile.MyLevitatedClip;
                    }
                }
            }

            //Debug.Log("CharacterAnimator.SetAnimationClipOverrides() Current Animation Profile Contains Revive Clip");
            if (overrideControllerClipList.Contains(systemAnimations.MyReviveClip.name)) {
                if (currentAnimationProfile.MyReviveClip != null) {
                    if (overrideController[systemAnimations.MyReviveClip.name] != currentAnimationProfile.MyReviveClip) {
                        overrideController[systemAnimations.MyReviveClip.name] = currentAnimationProfile.MyReviveClip;
                        currentAnimations.MyReviveClip = currentAnimationProfile.MyReviveClip;
                    }
                } else {
                    if (defaultAnimationProfile.MyReviveClip != null && overrideController[systemAnimations.MyReviveClip.name] != defaultAnimationProfile.MyReviveClip) {
                        overrideController[systemAnimations.MyReviveClip.name] = defaultAnimationProfile.MyReviveClip;
                        currentAnimations.MyReviveClip = defaultAnimationProfile.MyReviveClip;
                    }
                }
            }

            //overrideController = tempOverrideController;
            //Debug.Log(gameObject.name + ": setting override controller to: " + overrideController.name);
            //SetOverrideController(overrideController);

        }

        // special melee attack
        public virtual float HandleAbility(AnimationClip animationClip, BaseAbility baseAbility, BaseCharacter targetCharacterUnit) {
            //Debug.Log(gameObject.name + ".CharacterAnimator.HandleAbility(" + baseAbility.MyName + ")");
            if (animator == null) {
                //Debug.Log(gameObject.name + ".CharacterAnimator.HandleAbility(" + baseAbility.MyName + ") ANIMATOR IS NULL!!!");
                return 0f;
            }
            characterUnit.MyCharacter.MyCharacterCombat.MySwingTarget = targetCharacterUnit;

            if (SystemConfigurationManager.MyInstance != null) {
                // override the default attack animation
                overrideController[SystemConfigurationManager.MyInstance.MySystemAnimationProfile.MyAttackClips[0].name] = animationClip;
            }
            float animationLength = animationClip.length;

            // save animation length for weapon damage normalization
            lastAnimationLength = animationLength;

            //Debug.Log(gameObject.name + ".CharacterAnimator.HandleAbility(): animationlength: " + animationLength);
            currentAbility = baseAbility;


            float speedNormalizedAnimationLength = 1f;
            if (characterUnit != null && characterUnit.MyCharacter != null && characterUnit.MyCharacter.MyCharacterStats != null) {
                speedNormalizedAnimationLength = characterUnit.MyCharacter.MyCharacterStats.GetSpeedModifiers() * animationLength;
                //Debug.Log(gameObject.name + ".CharacterAnimator.HandleAbility(" + baseAbility.MyName + "): speedNormalizedAnimationLength: " + speedNormalizedAnimationLength + "; length: " + animationLength);
            }
            if (ParameterExists("AnimationSpeed")) {
                animator.SetFloat("AnimationSpeed", 1f / characterUnit.MyCharacter.MyCharacterStats.GetSpeedModifiers());
            }

            // wait for the animation to play before allowing the character to attack again
            attackCoroutine = StartCoroutine(WaitForAnimation(baseAbility, speedNormalizedAnimationLength, (baseAbility as AnimatedAbility).MyIsAutoAttack, !(baseAbility as AnimatedAbility).MyIsAutoAttack, false));

            // SUPPRESS DEFAULT SOUND EFFECT FOR ANIMATED ABILITIES, WHICH ARE NOW RESPONSIBLE FOR THEIR OWN SOUND EFFECTS
            characterUnit.MyCharacter.MyCharacterCombat.MyOverrideHitSoundEffect = null;

            // tell the animator to play the animation
            SetAttacking(true);

            return speedNormalizedAnimationLength;
        }

        // non melee ability (spell) cast
        public virtual void HandleCastingAbility(AnimationClip animationClip, BaseAbility baseAbility) {
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

                overrideController[SystemConfigurationManager.MyInstance.MySystemAnimationProfile.MyCastClips[0].name] = animationClip;
                //Debug.Log(gameObject.name + ".CharacterAnimator.HandleCastingAbility() current casting clip: " + overrideController[SystemConfigurationManager.MyInstance.MySystemAnimationProfile.MyCastClips[0].name].name);
                float animationLength = animationClip.length;
                //Debug.Log(gameObject.name + ".CharacterAnimator.HandleCastingAbility() animationlength: " + animationLength);

                // save animation length for damage normalization
                //lastAnimationLength = animationLength;

            }
            if (baseAbility.MyAnimationProfile.MyUseRootMotion == true) {
                characterUnit.SetUseRootMotion(true);
            } else {
                characterUnit.SetUseRootMotion(false);
            }
            if (baseAbility.MyAbilityCastingTime > 0f) {
                SetCasting(true);
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
            //Debug.Log(gameObject.name + "waitforanimation remainingtime: " + remainingTime + "; MyWaitingForHits: " + characterUnit.MyCharacter.MyCharacterCombat.MyWaitingForAutoAttack + "; myWaitingForAnimatedAbility: " + characterUnit.MyCharacter.MyCharacterAbilityManager.MyWaitingForAnimatedAbility + "; iscasting: " + characterUnit.MyCharacter.MyCharacterAbilityManager.MyIsCasting);
            while (remainingTime > 0f && (characterUnit.MyCharacter.MyCharacterAbilityManager.MyWaitingForAnimatedAbility == true || characterUnit.MyCharacter.MyCharacterCombat.MyWaitingForAutoAttack == true || characterUnit.MyCharacter.MyCharacterAbilityManager.MyIsCasting)) {
                //Debug.Log(gameObject.name + ".WaitForAttackAnimation(" + animationLength + "): remainingTime: " + remainingTime + "; MyWaitingForHits: " + characterUnit.MyCharacter.MyCharacterCombat.MyWaitingForAutoAttack + "; myWaitingForAnimatedAbility: " + characterUnit.MyCharacter.MyCharacterAbilityManager.MyWaitingForAnimatedAbility + "; iscasting: " + characterUnit.MyCharacter.MyCharacterAbilityManager.MyIsCasting + "animationSpeed: " + animator.GetFloat("AnimationSpeed"));
                //Debug.Log(gameObject.name + ".WaitForAttackAnimation(" + animationLength + "): animationSpeed: " + animator.GetFloat("AnimationSpeed"));
                remainingTime -= Time.deltaTime;
                yield return null;
            }
            //Debug.Log(gameObject.name + "Setting MyWaitingForAutoAttack to false after countdown (" + remainingTime + ") MyWaitingForAutoAttack: " + characterUnit.MyCharacter.MyCharacterCombat.MyWaitingForAutoAttack + "; myWaitingForAnimatedAbility: " + characterUnit.MyCharacter.MyCharacterAbilityManager.MyWaitingForAnimatedAbility + "; iscasting: " + characterUnit.MyCharacter.MyCharacterAbilityManager.MyIsCasting + "animationSpeed: " + animator.GetFloat("AnimationSpeed"));
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
            if (characterUnit != null && characterUnit.MyCharacter != null && characterUnit.MyCharacter.MyCharacterCombat != null) {
                characterUnit.MyCharacter.MyCharacterCombat.SetWaitingForAutoAttack(false);
            }
            SetAttacking(false);
        }

        public void ClearAnimatedAttack(BaseAbility baseAbility) {
            //Debug.Log(gameObject.name + ".CharacterAnimator.ClearAnimatedAttack()");
            characterUnit.MyCharacter.MyCharacterAbilityManager.MyWaitingForAnimatedAbility = false;
            (baseAbility as AnimatedAbility).CleanupEventSubscriptions(characterUnit.MyCharacter);
            SetAttacking(false);
            currentAbility = null;
            if (characterUnit.MyCharacter != null && characterUnit.MyCharacter.MyCharacterEquipmentManager != null) {
                characterUnit.MyCharacter.MyCharacterEquipmentManager.DespawnAbilityObjects();
            }
        }

        public void ClearCasting() {
            //Debug.Log(gameObject.name + ".CharacterAnimator.ClearCasting()");

            //characterUnit.MyCharacter.MyCharacterAbilityManager.StopCasting();
            if (characterUnit != null) {
                characterUnit.SetUseRootMotion(false);
            }
            SetCasting(false);

        }

        public virtual void ClearAnimationBlockers() {
            //Debug.Log(gameObject.name + ".CharacterAnimator.ClearAnimationBlockers()");
            ClearAutoAttack();
            if (currentAbility is AnimatedAbility) {
                //Debug.Log(gameObject.name + ".CharacterAnimator.ClearAnimationBlockers() WE HAVE AN ANIMATED ABILITY");
                ClearAnimatedAttack(currentAbility);
            } else {
                //Debug.Log(gameObject.name + ".CharacterAnimator.ClearAnimationBlockers() WE DO NOT HAVE AN ANIMATED ABILITY");
            }
            ClearCasting();
            if (attackCoroutine != null) {
                //Debug.Log(gameObject.name + ".CharacterAnimator.HandleCastingAbility(): STOPPING OUTSTANDING CAST OR REGULAR ATTACK FOR CAST");
                StopCoroutine(attackCoroutine);
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

        public virtual void HandleDeath(CharacterStats characterStats) {
            //Debug.Log(gameObject.name + ".CharacterAnimator.HandleDeath()");
            if (currentAbility != null && currentAbility is AnimatedAbility) {
                (currentAbility as AnimatedAbility).CleanupEventSubscriptions(characterUnit.MyCharacter);
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
                remainingTime -= Time.deltaTime;
                yield return null;
            }
            //Debug.Log(gameObject.name + "Setting waitingforhits to false after countdown down");
            SetBool("IsDead", false);
            OnReviveComplete();
            SetCorrectOverrideController();
            resurrectionCoroutine = null;
        }

        public virtual void HandleRevive() {
            SetTrigger("ReviveTrigger");
            // add 1 to account for the transition
            if (SystemConfigurationManager.MyInstance != null) {
                float animationLength = overrideController[SystemConfigurationManager.MyInstance.MySystemAnimationProfile.MyReviveClip.name].length + 2;
                resurrectionCoroutine = StartCoroutine(WaitForResurrectionAnimation(animationLength));
            }
        }

        public virtual void HandleLevitated() {
            //Debug.Log(gameObject.name + ".CharacterAnimator.HandleDeath()");
            SetTrigger("LevitateTrigger");
            SetBool("Levitated", true);
        }
        public virtual void HandleUnLevitated(bool swapAnimator = true) {
            SetBool("Levitated", false);
        }

        public virtual void HandleStunned() {
            //Debug.Log(gameObject.name + ".CharacterAnimator.HandleStunned()");
            SetTrigger("StunTrigger");
            SetBool("Stunned", true);
        }

        public virtual void HandleUnStunned(bool swapAnimator = true) {
            //Debug.Log(gameObject.name + ".CharacterAnimator.HandleUnStunned()");
            SetBool("Stunned", false);
        }

        public virtual void SetCasting(bool varValue, bool swapAnimator = true) {
            //Debug.Log(gameObject.name + ".CharacterAnimator.SetCasting(" + varValue + ")");
            if (animator == null) {
                return;
            }
            if (characterUnit != null && characterUnit.MyCharacter != null && characterUnit.MyCharacter.MyCharacterAbilityManager != null) {
                characterUnit.MyCharacter.MyCharacterAbilityManager.MyIsCasting = varValue;
            }
            if (ParameterExists("Casting")) {
                animator.SetBool("Casting", varValue);
            }

            if (varValue == true) {
                SetTrigger("CastingTrigger");
                characterUnit.MyCharacter.MyCharacterCombat.ResetAttackCoolDown();
            }
        }

        public virtual void SetAttacking(bool varValue, bool swapAnimator = true) {
            //Debug.Log(gameObject.name + ".SetAttacking(" + varValue + ")");
            if (animator == null) {
                return;
            }
            if (ParameterExists("Attacking")) {
                animator.SetBool("Attacking", varValue);
            }
            if (varValue == true) {
                float animationSpeed = 1f;
                if (characterUnit != null && characterUnit.MyCharacter != null && characterUnit.MyCharacter.MyCharacterStats != null) {
                    animationSpeed = 1f / characterUnit.MyCharacter.MyCharacterStats.GetSpeedModifiers();
                }
                //Debug.Log(gameObject.name + ".CharacterAnimator.SetAttacking(): setting speed to: " + animationSpeed);
                //animator.SetFloat("AnimationSpeed", animationSpeed);
                SetTrigger("AttackTrigger");
                characterUnit.MyCharacter.MyCharacterCombat.ResetAttackCoolDown();
            } else {
                //animator.SetFloat("AnimationSpeed", 1f);
            }
        }

        public virtual void SetRiding(bool varValue) {
            Debug.Log(gameObject.name + ".SetRiding(" + varValue + ")");
            if (animator == null) {
                return;
            }

            if (ParameterExists("Riding")) {
                animator.SetBool("Riding", varValue);
            }
            if (varValue == true) {
                SetTrigger("RidingTrigger");
            } else {
                //animator.SetFloat("AnimationSpeed", 1f);
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

        public void SetVelocity(Vector3 varValue, bool rotateModel = false) {
            //Debug.Log(gameObject.name + ".CharacterAnimator.SetVelocity(" + varValue + ")");
            // receives velocity in LOCAL SPACE

            if (animator == null) {
                return;
            }

            if (rotateModel) {
                if (varValue == Vector3.zero) {
                    animator.transform.forward = transform.forward;
                } else {
                    Vector3 normalizedVector = varValue.normalized;
                    if (normalizedVector.x != 0 || normalizedVector.z != 0) {
                        animator.transform.forward = transform.TransformDirection(new Vector3(normalizedVector.x, 0, normalizedVector.z));
                        //Debug.Log(gameObject.name + ".CharacterAnimator.SetVelocity(" + varValue + "): setting forward to: " + transform.TransformDirection(new Vector3(normalizedVector.x, 0, normalizedVector.z)));
                    }
                    //animator.transform.forward = varValue.normalized;
                }
            }

            animator.SetFloat("Velocity X", varValue.x);
            animator.SetFloat("Velocity Y", varValue.y);
            animator.SetFloat("Velocity Z", varValue.z);

            float absXValue = Mathf.Abs(varValue.x);
            float absYValue = Mathf.Abs(varValue.y);
            float absZValue = Mathf.Abs(varValue.z);
            float absValue = Mathf.Abs(varValue.magnitude);

            float animationSpeed = 1;
            float usedBaseAnimationSpeed = 1;
            float multiplier = 1;

            if (!currentAnimationProfile.MySuppressAdjustAnimatorSpeed) {
                // nothing more to do if we are leaving animations at normal speed

                float usedBaseMoveForwardAnimationSpeed;
                float usedbaseWalkBackAnimationSpeed;
                float usedBaseStrafeLeftAnimationSpeed;
                float usedBaseStrafeRightAnimationSpeed;
                float usedBaseWalkStrafeBackRightAnimationSpeed;
                float usedBaseWalkStrafeBackLeftAnimationSpeed;
                float usedBaseStrafeForwardLeftAnimationSpeed;
                float usedBaseStrafeForwardRightAnimationSpeed;


                if (characterUnit != null && characterUnit.MyCharacter != null && characterUnit.MyCharacter.MyCharacterCombat != null && characterUnit.MyCharacter.MyCharacterCombat.GetInCombat() == true) {
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
            Debug.Log(gameObject.name + ".CharacterAnimator.SetVelocityZ(" + varValue + "): animationSpeed: " + animationSpeed);

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
                    animator.SetTrigger(varName);
                }
            }
        }

        public AnimatorClipInfo[] GetCurrentAnimatorClipInfo(int layerIndex) {
            return animator.GetCurrentAnimatorClipInfo(layerIndex);
        }

        public void PerformEquipmentChange(Equipment newItem) {
            PerformEquipmentChange(newItem, null);
        }

        public void PerformEquipmentChange(Equipment newItem, Equipment oldItem) {
            //Debug.Log(gameObject.name + ".CharacterAnimator.PerformEquipmentChange(" + (newItem == null ? "null" : newItem.MyName) + ", " + (oldItem == null ? "null" : oldItem.MyName) + ")");
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