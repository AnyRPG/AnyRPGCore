using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UMA;
using UMA.CharacterSystem;

public class CharacterAnimator : MonoBehaviour {

    public event System.Action OnReviveComplete = delegate { };

    [SerializeField]
    protected AnimationProfile defaultAttackAnimationProfile;

    //public AnimationClip replaceableAttackAnim;
    protected AnimationProfile currentAttackAnimationProfile;
    const float locomotionAnimationSmoothTime = 0.1f;
    const string replaceableAnimationName = "AnyRPGDefaultAttack";

    protected Animator animator;
    public RuntimeAnimatorController animatorController;
    public AnimatorOverrideController overrideController;

    protected ICharacterUnit characterUnit;

    protected bool initialized = false;

    // in combat animations
    private float baseWalkAnimationSpeed = 1f;
    private float baseRunAnimationSpeed = 3.4f;
    private float baseWalkStrafeRightAnimationSpeed = 1f;
    private float baseJogStrafeRightAnimationSpeed = 2.4f;
    private float baseWalkStrafeBackRightAnimationSpeed = 1f;
    private float baseWalkStrafeForwardRightAnimationSpeed = 1f;
    private float baseJogStrafeForwardRightAnimationSpeed = 2.67f;
    private float baseWalkStrafeLeftAnimationSpeed = 1f;
    private float baseJogStrafeLeftAnimationSpeed = 2.4f;
    private float baseWalkStrafeBackLeftAnimationSpeed = 1f;
    private float baseWalkStrafeForwardLeftAnimationSpeed = 1f;
    private float baseJogStrafeForwardLeftAnimationSpeed = 2.67f;
    private float baseWalkBackAnimationSpeed = 1.6f;

    // in combat animations
    private float baseCombatWalkAnimationSpeed = 1f;
    private float baseCombatRunAnimationSpeed = 3.4f;
    private float baseCombatWalkStrafeRightAnimationSpeed = 1f;
    private float baseCombatJogStrafeRightAnimationSpeed = 2.4f;
    private float baseCombatWalkStrafeBackRightAnimationSpeed = 1f;
    private float baseCombatWalkStrafeForwardRightAnimationSpeed = 1f;
    private float baseCombatJogStrafeForwardRightAnimationSpeed = 2.67f;
    private float baseCombatWalkStrafeLeftAnimationSpeed = 1f;
    private float baseCombatJogStrafeLeftAnimationSpeed = 2.4f;
    private float baseCombatWalkStrafeBackLeftAnimationSpeed = 1f;
    private float baseCombatWalkStrafeForwardLeftAnimationSpeed = 1f;
    private float baseCombatJogStrafeForwardLeftAnimationSpeed = 2.67f;
    private float baseCombatWalkBackAnimationSpeed = 1.6f;


    private Coroutine attackCoroutine = null;
    private Coroutine resurrectionCoroutine = null;

    // a reference to any current ability we are casting
    private BaseAbility currentAbility = null;

    public bool applyRootMotion { get => (animator != null ? animator.applyRootMotion : false) ; }
    public Animator MyAnimator { get => animator; }
    public BaseAbility MyCurrentAbility { get => currentAbility; set => currentAbility = value; }

    protected virtual void Awake() {
        //Debug.Log(gameObject.name + ".CharacterAnimator.Awake()");
        if (characterUnit == null) {
            characterUnit = GetComponent<CharacterUnit>();
        }
        if (characterUnit == null) {
            characterUnit = GetComponentInParent<CharacterUnit>();
        }
        if (characterUnit == null) {
            Debug.Log(gameObject.name + ".CharacterAnimator.Awake(): Unable to detect characterUnit!");
        }
    }

    protected virtual void Start() {
        //Debug.Log(gameObject.name + ".CharacterAnimator.Start()");
        CreateEventReferences();
        InitializeAnimator();
    }

    public virtual void CreateEventReferences() {
        if (characterUnit.MyCharacter != null) {
            characterUnit.MyCharacter.MyCharacterCombat.OnAttack += HandleAttack;
            characterUnit.MyCharacter.MyCharacterStats.OnDie += HandleDeath;
            characterUnit.MyCharacter.MyCharacterStats.OnReviveBegin += HandleRevive;
        }
    }

    public virtual void CleanupEventReferences() {
        if (characterUnit != null && characterUnit.MyCharacter != null) {
            characterUnit.MyCharacter.MyCharacterCombat.OnAttack -= HandleAttack;
            characterUnit.MyCharacter.MyCharacterStats.OnDie -= HandleDeath;
            characterUnit.MyCharacter.MyCharacterStats.OnReviveBegin -= HandleRevive;
        }
    }

    public void OnDisable() {
        CleanupEventReferences();
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

    public virtual void InitializeAnimator() {
        //Debug.Log(gameObject.name + ": CharacterAnimator.InitializeAnimator()");
        if (initialized) {
            return;
        }
        animator = GetComponentInChildren<Animator>();
        if (animator == null) {
            //Debug.Log(gameObject.name + ": CharacterAnimator.InitializeAnimator(): Could not find animator in children");
            return;
        }
        if (overrideController == null) {
            //Debug.Log(gameObject.name + ": override controller was null. creating new override controller");
            overrideController = new AnimatorOverrideController(animatorController);
        }
        //Debug.Log(gameObject.name + ": setting override controller to: " + overrideController.name);
        animator.runtimeAnimatorController = overrideController;

        // set animator on UMA if one exists
        DynamicCharacterAvatar myAvatar = GetComponent<DynamicCharacterAvatar>();
        if (myAvatar != null) {
            myAvatar.raceAnimationControllers.defaultAnimationController = overrideController;
        }

        SetAnimationProfileOverride(defaultAttackAnimationProfile);

        initialized = true;
    }

    public void SetAnimationProfileOverride(AnimationProfile animationProfile) {
        //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationProfileOverride()");
        currentAttackAnimationProfile = animationProfile;
        SetAnimationClipOverrides();
    }

    public void ResetAnimationProfile() {
        //Debug.Log(gameObject.name + ".CharacterAnimator.ResetAnimationProfile()");
        currentAttackAnimationProfile = defaultAttackAnimationProfile;
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

        if (currentAttackAnimationProfile.MyMoveForwardClip != null) {
            //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): WalkForward is not null.");
            overrideController["AnyRPGWalkForward"] = currentAttackAnimationProfile.MyMoveForwardClip;
            //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): MyMoveForwardClip." + currentAttackAnimationProfile.MyMoveForwardClip.averageSpeed);
            if (currentAttackAnimationProfile.MyMoveForwardClip.averageSpeed.z > 0.1) {
                // our clip has forward motion.  override the default animation motion speed of 1
                baseWalkAnimationSpeed = currentAttackAnimationProfile.MyMoveForwardClip.averageSpeed.z;
                //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation walk speed: " + baseWalkAnimationSpeed);
            }
        }
        if (currentAttackAnimationProfile.MyCombatMoveForwardClip != null) {
            //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): WalkForward is not null.");
            overrideController["AnyRPGCombatUnarmedStrafeForward"] = currentAttackAnimationProfile.MyCombatMoveForwardClip;
            //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): MyMoveForwardClip." + currentAttackAnimationProfile.MyMoveForwardClip.averageSpeed);
            if (currentAttackAnimationProfile.MyCombatMoveForwardClip.averageSpeed.z > 0.1) {
                // our clip has forward motion.  override the default animation motion speed of 1
                baseCombatWalkAnimationSpeed = currentAttackAnimationProfile.MyCombatMoveForwardClip.averageSpeed.z;
                //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation walk speed: " + baseWalkAnimationSpeed);
            }
        }
        if (currentAttackAnimationProfile.MyMoveForwardFastClip != null) {
            //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): JogForward is not null.");
            overrideController["AnyRPGJogForward"] = currentAttackAnimationProfile.MyMoveForwardFastClip;
            if (Mathf.Abs(currentAttackAnimationProfile.MyMoveForwardFastClip.averageSpeed.z) > 0.1) {
                // our clip has forward motion.  override the default animation motion speed of 2
                baseRunAnimationSpeed = Mathf.Abs(currentAttackAnimationProfile.MyMoveForwardFastClip.averageSpeed.z);
                //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
            }
        }
        if (currentAttackAnimationProfile.MyCombatMoveForwardFastClip != null) {
            //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): JogForward is not null.");
            overrideController["AnyRPGCombatUnarmedRunForward"] = currentAttackAnimationProfile.MyCombatMoveForwardFastClip;
            if (Mathf.Abs(currentAttackAnimationProfile.MyCombatMoveForwardFastClip.averageSpeed.z) > 0.1) {
                // our clip has forward motion.  override the default animation motion speed of 2
                baseCombatRunAnimationSpeed = Mathf.Abs(currentAttackAnimationProfile.MyCombatMoveForwardFastClip.averageSpeed.z);
                //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
            }
        }
        if (currentAttackAnimationProfile.MyMoveBackClip != null) {
            overrideController["AnyRPGWalkBackward"] = currentAttackAnimationProfile.MyMoveBackClip;
            if (Mathf.Abs(currentAttackAnimationProfile.MyMoveBackClip.averageSpeed.z) > 0.1) {
                // our clip has forward motion.  override the default animation motion speed of 2
                baseWalkBackAnimationSpeed = Mathf.Abs(currentAttackAnimationProfile.MyMoveBackClip.averageSpeed.z);
                //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
            }
        }
        if (currentAttackAnimationProfile.MyCombatMoveBackClip != null) {
            overrideController["AnyRPGCombatUnarmedMoveBack"] = currentAttackAnimationProfile.MyCombatMoveBackClip;
            if (Mathf.Abs(currentAttackAnimationProfile.MyCombatMoveBackClip.averageSpeed.z) > 0.1) {
                // our clip has forward motion.  override the default animation motion speed of 2
                baseCombatWalkBackAnimationSpeed = Mathf.Abs(currentAttackAnimationProfile.MyCombatMoveBackClip.averageSpeed.z);
                //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
            }
        }


        if (currentAttackAnimationProfile.MyJumpClip != null) {
            overrideController["AnyRPGUnarmedJump"] = currentAttackAnimationProfile.MyJumpClip;
        }
        if (currentAttackAnimationProfile.MyCombatJumpClip != null) {
            overrideController["AnyRPGCombatUnarmedJump"] = currentAttackAnimationProfile.MyCombatJumpClip;
        }
        if (currentAttackAnimationProfile.MyIdleClip != null) {
            overrideController["AnyRPGIdleNeutral"] = currentAttackAnimationProfile.MyIdleClip;
        }
        if (currentAttackAnimationProfile.MyCombatIdleClip != null) {
            overrideController["AnyRPGCombatUnarmedIdle"] = currentAttackAnimationProfile.MyCombatIdleClip;
        }
        if (currentAttackAnimationProfile.MyLandClip != null) {
            overrideController["AnyRPGUnarmedLand"] = currentAttackAnimationProfile.MyLandClip;
        }
        if (currentAttackAnimationProfile.MyCombatLandClip != null) {
            overrideController["AnyRPGCombatUnarmedLand"] = currentAttackAnimationProfile.MyCombatLandClip;
        }
        if (currentAttackAnimationProfile.MyFallClip != null) {
            overrideController["AnyRPGUnarmedFall"] = currentAttackAnimationProfile.MyFallClip;
        }
        if (currentAttackAnimationProfile.MyCombatFallClip != null) {
            overrideController["AnyRPGCombatUnarmedFall"] = currentAttackAnimationProfile.MyCombatFallClip;
        }

        // out of combat strafing
        if (currentAttackAnimationProfile.MyStrafeLeftClip != null) {
            overrideController["AnyRPGWalkStrafeLeft"] = currentAttackAnimationProfile.MyStrafeLeftClip;
            if (Mathf.Abs(currentAttackAnimationProfile.MyStrafeLeftClip.averageSpeed.x) > 0.1) {
                // our clip has forward motion.  override the default animation motion speed of 2
                baseWalkStrafeLeftAnimationSpeed = Mathf.Abs(currentAttackAnimationProfile.MyStrafeLeftClip.averageSpeed.x);
                //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
            }
        }
        if (currentAttackAnimationProfile.MyJogStrafeLeftClip != null) {
            overrideController["AnyRPGJogStrafeLeft"] = currentAttackAnimationProfile.MyJogStrafeLeftClip;
            if (Mathf.Abs(currentAttackAnimationProfile.MyJogStrafeLeftClip.averageSpeed.x) > 0.1) {
                // our clip has forward motion.  override the default animation motion speed of 2
                baseJogStrafeLeftAnimationSpeed = Mathf.Abs(currentAttackAnimationProfile.MyJogStrafeLeftClip.averageSpeed.x);
                //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
            }
        }
        if (currentAttackAnimationProfile.MyStrafeRightClip != null) {
            overrideController["AnyRPGWalkStrafeRight"] = currentAttackAnimationProfile.MyStrafeRightClip;
            if (Mathf.Abs(currentAttackAnimationProfile.MyStrafeRightClip.averageSpeed.x) > 0.1) {
                // our clip has forward motion.  override the default animation motion speed of 2
                baseWalkStrafeRightAnimationSpeed = Mathf.Abs(currentAttackAnimationProfile.MyStrafeRightClip.averageSpeed.x);
                //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
            }
        }
        if (currentAttackAnimationProfile.MyJogStrafeRightClip != null) {
            overrideController["AnyRPGJogStrafeRight"] = currentAttackAnimationProfile.MyJogStrafeRightClip;
            if (Mathf.Abs(currentAttackAnimationProfile.MyJogStrafeRightClip.averageSpeed.x) > 0.1) {
                // our clip has forward motion.  override the default animation motion speed of 2
                baseJogStrafeRightAnimationSpeed = Mathf.Abs(currentAttackAnimationProfile.MyJogStrafeRightClip.averageSpeed.x);
                //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
            }
        }
        if (currentAttackAnimationProfile.MyStrafeForwardRightClip != null) {
            overrideController["AnyRPGWalkStrafeForwardRight"] = currentAttackAnimationProfile.MyStrafeForwardRightClip;
            if (Mathf.Abs(currentAttackAnimationProfile.MyStrafeForwardRightClip.averageSpeed.magnitude) > 0.1) {
                // our clip has forward motion.  override the default animation motion speed of 2
                baseWalkStrafeForwardRightAnimationSpeed = Mathf.Abs(currentAttackAnimationProfile.MyStrafeForwardRightClip.averageSpeed.magnitude);
                //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
            }
        }
        if (currentAttackAnimationProfile.MyJogStrafeForwardRightClip != null) {
            overrideController["AnyRPGJogStrafeForwardRight"] = currentAttackAnimationProfile.MyJogStrafeForwardRightClip;
            if (Mathf.Abs(currentAttackAnimationProfile.MyJogStrafeForwardRightClip.averageSpeed.magnitude) > 0.1) {
                // our clip has forward motion.  override the default animation motion speed of 2
                baseJogStrafeForwardRightAnimationSpeed = Mathf.Abs(currentAttackAnimationProfile.MyJogStrafeForwardRightClip.averageSpeed.magnitude);
                //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
            }
        }
        if (currentAttackAnimationProfile.MyStrafeForwardLeftClip != null) {
            overrideController["AnyRPGWalkStrafeForwardLeft"] = currentAttackAnimationProfile.MyStrafeForwardLeftClip;
            if (Mathf.Abs(currentAttackAnimationProfile.MyStrafeForwardLeftClip.averageSpeed.magnitude) > 0.1) {
                // our clip has forward motion.  override the default animation motion speed of 2
                baseWalkStrafeForwardLeftAnimationSpeed = Mathf.Abs(currentAttackAnimationProfile.MyStrafeForwardLeftClip.averageSpeed.magnitude);
                //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
            }
        }
        if (currentAttackAnimationProfile.MyJogStrafeForwardLeftClip != null) {
            overrideController["AnyRPGJogStrafeForwardLeft"] = currentAttackAnimationProfile.MyJogStrafeForwardLeftClip;
            if (Mathf.Abs(currentAttackAnimationProfile.MyJogStrafeForwardLeftClip.averageSpeed.magnitude) > 0.1) {
                // our clip has forward motion.  override the default animation motion speed of 2
                baseJogStrafeForwardLeftAnimationSpeed = Mathf.Abs(currentAttackAnimationProfile.MyJogStrafeForwardLeftClip.averageSpeed.magnitude);
                //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
            }
        }
        if (currentAttackAnimationProfile.MyStrafeBackLeftClip != null) {
            overrideController["AnyRPGWalkStrafeBackLeft"] = currentAttackAnimationProfile.MyStrafeBackLeftClip;
            if (Mathf.Abs(currentAttackAnimationProfile.MyStrafeBackLeftClip.averageSpeed.magnitude) > 0.1) {
                // our clip has forward motion.  override the default animation motion speed of 2
                baseWalkStrafeBackLeftAnimationSpeed = Mathf.Abs(currentAttackAnimationProfile.MyStrafeBackLeftClip.averageSpeed.magnitude);
                //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
            }
        }
        if (currentAttackAnimationProfile.MyStrafeBackRightClip != null) {
            overrideController["AnyRPGWalkStrafeBackRight"] = currentAttackAnimationProfile.MyStrafeBackRightClip;
            if (Mathf.Abs(currentAttackAnimationProfile.MyStrafeBackRightClip.averageSpeed.magnitude) > 0.1) {
                // our clip has forward motion.  override the default animation motion speed of 2
                baseWalkStrafeBackRightAnimationSpeed = Mathf.Abs(currentAttackAnimationProfile.MyStrafeBackRightClip.averageSpeed.magnitude);
                //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
            }
        }


        // combat strafing
        if (currentAttackAnimationProfile.MyCombatStrafeLeftClip != null) {
            overrideController["AnyRPGCombatUnarmedStrafeLeft"] = currentAttackAnimationProfile.MyCombatStrafeLeftClip;
            if (Mathf.Abs(currentAttackAnimationProfile.MyCombatStrafeLeftClip.averageSpeed.x) > 0.1) {
                // our clip has forward motion.  override the default animation motion speed of 2
                baseCombatWalkStrafeLeftAnimationSpeed = Mathf.Abs(currentAttackAnimationProfile.MyCombatStrafeLeftClip.averageSpeed.x);
                //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
            }
        }
        if (currentAttackAnimationProfile.MyCombatJogStrafeLeftClip != null) {
            overrideController["AnyRPGCombatUnarmedJogStrafeLeft"] = currentAttackAnimationProfile.MyCombatJogStrafeLeftClip;
            if (Mathf.Abs(currentAttackAnimationProfile.MyCombatJogStrafeLeftClip.averageSpeed.x) > 0.1) {
                // our clip has forward motion.  override the default animation motion speed of 2
                baseCombatJogStrafeLeftAnimationSpeed = Mathf.Abs(currentAttackAnimationProfile.MyCombatJogStrafeLeftClip.averageSpeed.x);
                //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
            }
        }
        if (currentAttackAnimationProfile.MyCombatStrafeRightClip != null) {
            overrideController["AnyRPGCombatUnarmedStrafeRight"] = currentAttackAnimationProfile.MyCombatStrafeRightClip;
            if (Mathf.Abs(currentAttackAnimationProfile.MyCombatStrafeRightClip.averageSpeed.x) > 0.1) {
                // our clip has forward motion.  override the default animation motion speed of 2
                baseCombatWalkStrafeRightAnimationSpeed = Mathf.Abs(currentAttackAnimationProfile.MyCombatStrafeRightClip.averageSpeed.x);
                //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
            }
        }
        if (currentAttackAnimationProfile.MyCombatJogStrafeRightClip != null) {
            overrideController["AnyRPGCombatUnarmedJogStrafeRight"] = currentAttackAnimationProfile.MyCombatJogStrafeRightClip;
            if (Mathf.Abs(currentAttackAnimationProfile.MyCombatJogStrafeRightClip.averageSpeed.x) > 0.1) {
                // our clip has forward motion.  override the default animation motion speed of 2
                baseCombatJogStrafeRightAnimationSpeed = Mathf.Abs(currentAttackAnimationProfile.MyCombatJogStrafeRightClip.averageSpeed.x);
                //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
            }
        }
        if (currentAttackAnimationProfile.MyCombatStrafeForwardRightClip != null) {
            overrideController["AnyRPGCombatUnarmedStrafeForwardRight"] = currentAttackAnimationProfile.MyCombatStrafeForwardRightClip;
            if (Mathf.Abs(currentAttackAnimationProfile.MyCombatStrafeForwardRightClip.averageSpeed.magnitude) > 0.1) {
                // our clip has forward motion.  override the default animation motion speed of 2
                baseCombatWalkStrafeForwardRightAnimationSpeed = Mathf.Abs(currentAttackAnimationProfile.MyCombatStrafeForwardRightClip.averageSpeed.magnitude);
                //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
            }
        }
        if (currentAttackAnimationProfile.MyCombatJogStrafeForwardRightClip != null) {
            overrideController["AnyRPGCombatUnarmedStrafeForwardRight"] = currentAttackAnimationProfile.MyCombatJogStrafeForwardRightClip;
            if (Mathf.Abs(currentAttackAnimationProfile.MyCombatJogStrafeForwardRightClip.averageSpeed.magnitude) > 0.1) {
                // our clip has forward motion.  override the default animation motion speed of 2
                baseCombatJogStrafeForwardRightAnimationSpeed = Mathf.Abs(currentAttackAnimationProfile.MyCombatJogStrafeForwardRightClip.averageSpeed.magnitude);
                //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
            }
        }
        if (currentAttackAnimationProfile.MyCombatStrafeForwardLeftClip != null) {
            overrideController["AnyRPGCombatUnarmedStrafeForwardLeft"] = currentAttackAnimationProfile.MyCombatStrafeForwardLeftClip;
            if (Mathf.Abs(currentAttackAnimationProfile.MyCombatStrafeForwardLeftClip.averageSpeed.magnitude) > 0.1) {
                // our clip has forward motion.  override the default animation motion speed of 2
                baseCombatWalkStrafeForwardLeftAnimationSpeed = Mathf.Abs(currentAttackAnimationProfile.MyCombatStrafeForwardLeftClip.averageSpeed.magnitude);
                //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
            }
        }
        if (currentAttackAnimationProfile.MyCombatJogStrafeForwardLeftClip != null) {
            overrideController["AnyRPGCombatUnarmedJogStrafeForwardLeft"] = currentAttackAnimationProfile.MyCombatJogStrafeForwardLeftClip;
            if (Mathf.Abs(currentAttackAnimationProfile.MyCombatJogStrafeForwardLeftClip.averageSpeed.magnitude) > 0.1) {
                // our clip has forward motion.  override the default animation motion speed of 2
                baseCombatJogStrafeForwardLeftAnimationSpeed = Mathf.Abs(currentAttackAnimationProfile.MyCombatJogStrafeForwardLeftClip.averageSpeed.magnitude);
                //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
            }
        }
        if (currentAttackAnimationProfile.MyCombatStrafeBackLeftClip != null) {
            overrideController["AnyRPGCombatUnarmedStrafeBackLeft"] = currentAttackAnimationProfile.MyCombatStrafeBackLeftClip;
            if (Mathf.Abs(currentAttackAnimationProfile.MyCombatStrafeBackLeftClip.averageSpeed.magnitude) > 0.1) {
                // our clip has forward motion.  override the default animation motion speed of 2
                baseCombatWalkStrafeBackLeftAnimationSpeed = Mathf.Abs(currentAttackAnimationProfile.MyCombatStrafeBackLeftClip.averageSpeed.magnitude);
                //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
            }
        }
        if (currentAttackAnimationProfile.MyCombatStrafeBackRightClip != null) {
            overrideController["AnyRPGCombatUnarmedStrafeBackRight"] = currentAttackAnimationProfile.MyCombatStrafeBackRightClip;
            if (Mathf.Abs(currentAttackAnimationProfile.MyCombatStrafeBackRightClip.averageSpeed.magnitude) > 0.1) {
                // our clip has forward motion.  override the default animation motion speed of 2
                baseCombatWalkStrafeBackRightAnimationSpeed = Mathf.Abs(currentAttackAnimationProfile.MyCombatStrafeBackRightClip.averageSpeed.magnitude);
                //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): set base animation run speed: " + baseRunAnimationSpeed);
            }
        }


        if (currentAttackAnimationProfile.MyDeathClip != null) {
            //Debug.Log(gameObject.name + ".CharacterAnimator.SetAnimationClipOverrides(): Death is not null.");
            overrideController["AnyRPGUnarmedDeath1"] = currentAttackAnimationProfile.MyDeathClip;
        }
        if (currentAttackAnimationProfile.MyStunnedClip != null) {
            overrideController["AnyRPGUnarmedStunned"] = currentAttackAnimationProfile.MyStunnedClip;
        }
        if (currentAttackAnimationProfile.MyLevitatedClip != null) {
            overrideController["AnyRPGLevitated"] = currentAttackAnimationProfile.MyLevitatedClip;
        }
    }


    // regular melee auto-attack
    protected virtual void HandleAttack(BaseCharacter targetCharacterUnit) {
        //Debug.Log(gameObject.name + ".CharacterAnimator.HandleAttack()");
        if (animator == null) {
            return;
        }

        characterUnit.MyCharacter.MyCharacterCombat.MySwingTarget = targetCharacterUnit;

        // pick a random attack animation
        int attackIndex = Random.Range(0, currentAttackAnimationProfile.MyProfileNodes.Length);
        //Debug.Log(gameObject.name + ".CharacterAnimator: OnAttack(): attack index set to: " + attackIndex);

        // override the default attack animation
        overrideController[replaceableAnimationName] = currentAttackAnimationProfile.MyProfileNodes[attackIndex].animationClip;
        float animationLength = currentAttackAnimationProfile.MyProfileNodes[attackIndex].animationClip.length;

        // start a coroutine to unlock the auto-attack blocker boolean when the animation completes
        attackCoroutine = StartCoroutine(WaitForAnimation(null, animationLength, true, false, false));

        // tell the animator to play the animation
        SetAttacking(true);
    }

    // special melee attack
    public virtual void HandleAbility(AnimationClip animationClip, BaseAbility baseAbility, BaseCharacter targetCharacterUnit) {
        //Debug.Log(gameObject.name + ".CharacterAnimator.HandleAbility(" + baseAbility.MyName + ")");
        if (animator == null) {
            return;
        }
        characterUnit.MyCharacter.MyCharacterCombat.MySwingTarget = targetCharacterUnit;
        // override the default attack animation
        overrideController[replaceableAnimationName] = animationClip;
        float animationLength = animationClip.length;
        //Debug.Log(gameObject.name + ".CharacterAnimator.HandleAbility(): animationlength: " + animationLength);
        currentAbility = baseAbility;
        // wait for the animation to play before allowing the character to attack again
        attackCoroutine = StartCoroutine(WaitForAnimation(baseAbility, animationLength, false, true, false));
        
        // SUPPRESS DEFAULT SOUND EFFECT FOR ANIMATED ABILITIES, WHICH ARE NOW RESPONSIBLE FOR THEIR OWN SOUND EFFECTS
        characterUnit.MyCharacter.MyCharacterCombat.MyOverrideHitSoundEffect = null;

        // tell the animator to play the animation
        SetAttacking(true);
    }

    // non melee ability (spell) cast
    public virtual void HandleCastingAbility(AnimationClip animationClip, BaseAbility baseAbility) {
        //Debug.Log(gameObject.name + ".CharacterAnimator.HandleCastingAbility()");
        if (animator == null) {
            return;
        }

        // override the default attack animation
        overrideController["AnyRPGMagicSlowCasting"] = animationClip;
        float animationLength = animationClip.length;
        //Debug.Log(gameObject.name + ".CharacterAnimator.HandleCastingAbility() animationlength: " + animationLength);

        SetCasting(true);
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
            //Debug.Log(gameObject.name + ".WaitForAttackAnimation(" + animationLength + "): inside loop: " + remainingTime + "; MyWaitingForHits: " + characterUnit.MyCharacter.MyCharacterCombat.MyWaitingForAutoAttack + "; myWaitingForAnimatedAbility: " + characterUnit.MyCharacter.MyCharacterAbilityManager.MyWaitingForAnimatedAbility + "; iscasting: " + characterUnit.MyCharacter.MyCharacterAbilityManager.MyIsCasting);
            remainingTime -= Time.deltaTime;
            yield return null;
        }
        //Debug.Log(gameObject.name + "Setting MyWaitingForAutoAttack to false after countdown (" + remainingTime + ") MyWaitingForAutoAttack: " + characterUnit.MyCharacter.MyCharacterCombat.MyWaitingForAutoAttack + "; myWaitingForAnimatedAbility: " + characterUnit.MyCharacter.MyCharacterAbilityManager.MyWaitingForAnimatedAbility + "; iscasting: " + characterUnit.MyCharacter.MyCharacterAbilityManager.MyIsCasting);
        attackCoroutine = null;
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
        characterUnit.MyCharacter.MyCharacterCombat.SetWaitingForAutoAttack(false);
        SetAttacking(false);
    }

    public void ClearAnimatedAttack(BaseAbility baseAbility) {
        //Debug.Log(gameObject.name + ".CharacterAnimator.ClearAnimatedAttack()");
        characterUnit.MyCharacter.MyCharacterAbilityManager.MyWaitingForAnimatedAbility = false;
        (baseAbility as AnimatedAbility).CleanupEventReferences(characterUnit.MyCharacter);
        SetAttacking(false);
        currentAbility = null;
    }

    public void ClearCasting() {
        //characterUnit.MyCharacter.MyCharacterAbilityManager.StopCasting();
        // no need to do this here, as it is done in the stopcasting method now
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
    }

    private void HandleDeath(CharacterStats characterStats) {
        //Debug.Log(gameObject.name + ".CharacterAnimator.HandleDeath()");
        if (currentAbility != null && currentAbility is AnimatedAbility) {
            (currentAbility as AnimatedAbility).CleanupEventReferences(characterUnit.MyCharacter);
        }
        // add these to prevent characters from dying floating or upright
        HandleUnLevitated();
        HandleUnStunned();

        SetAttacking(false);
        SetCasting(false);
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
        resurrectionCoroutine = null;
    }


    private void HandleRevive() {
        SetTrigger("ReviveTrigger");
        // add 1 to account for the transition
        float animationLength = overrideController["AnyRPGResurrection2"].length + 2;
        resurrectionCoroutine = StartCoroutine(WaitForResurrectionAnimation(animationLength));
    }

    public void HandleLevitated() {
        //Debug.Log(gameObject.name + ".CharacterAnimator.HandleDeath()");
        SetTrigger("LevitateTrigger");
        SetBool("Levitated", true);
    }
    public void HandleUnLevitated() {
        SetBool("Levitated", false);
    }

    public void HandleStunned() {
        //Debug.Log(gameObject.name + ".CharacterAnimator.HandleStunned()");
        SetTrigger("StunTrigger");
        SetBool("Stunned", true);
    }

    public void HandleUnStunned() {
        //Debug.Log(gameObject.name + ".CharacterAnimator.HandleUnStunned()");
        SetBool("Stunned", false);
    }

    public virtual void SetCasting(bool varValue) {
        //Debug.Log(gameObject.name + ".CharacterAnimator.SetCasting(" + varValue + ")");
        if (animator == null) {
            return;
        }
        characterUnit.MyCharacter.MyCharacterAbilityManager.MyIsCasting = varValue;
        animator.SetBool("Casting", varValue);
        if (varValue == true) {
            SetTrigger("CastingTrigger");
            characterUnit.MyCharacter.MyCharacterCombat.ResetAttackCoolDown();
        }
    }

    public void SetAttacking(bool varValue) {
        //Debug.Log(gameObject.name + ".SetAttacking(" + varValue + ")");
        if (animator == null) {
            return;
        }
        animator.SetBool("Attacking", varValue);
        if (varValue == true) {
            SetTrigger("AttackTrigger");
            characterUnit.MyCharacter.MyCharacterCombat.ResetAttackCoolDown();
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
        animator.SetBool("Moving", varValue);
        if (varValue) {
            //Debug.Log(gameObject.name + ".CharacterAnimator.SetMoving()");
        }
    }

    public void SetVelocity(Vector3 varValue) {
        //Debug.Log(gameObject.name + ".CharacterAnimator.SetVelocity(" + varValue + ")");

        if (animator == null) {
            return;
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

        float usedBaseMoveForwardAnimationSpeed;
        float usedbaseWalkBackAnimationSpeed;
        float usedBaseStrafeLeftAnimationSpeed;
        float usedBaseStrafeRightAnimationSpeed;
        float usedBaseWalkStrafeBackRightAnimationSpeed;
        float usedBaseWalkStrafeBackLeftAnimationSpeed;
        float usedBaseStrafeForwardLeftAnimationSpeed;
        float usedBaseStrafeForwardRightAnimationSpeed;


        if (characterUnit.MyCharacter.MyCharacterCombat != null && characterUnit.MyCharacter.MyCharacterCombat.GetInCombat() == true) {
            // in combat
            usedBaseMoveForwardAnimationSpeed = (absZValue >= 2 ? baseCombatRunAnimationSpeed : baseCombatWalkAnimationSpeed);
            usedbaseWalkBackAnimationSpeed = baseCombatWalkBackAnimationSpeed;
            usedBaseStrafeLeftAnimationSpeed = (absValue > baseCombatJogStrafeLeftAnimationSpeed ? baseCombatJogStrafeLeftAnimationSpeed : baseCombatWalkStrafeLeftAnimationSpeed);
            usedBaseStrafeRightAnimationSpeed = (absValue > baseCombatJogStrafeRightAnimationSpeed ? baseCombatJogStrafeRightAnimationSpeed : baseCombatWalkStrafeRightAnimationSpeed);
            usedBaseWalkStrafeBackRightAnimationSpeed = baseCombatWalkStrafeBackRightAnimationSpeed;
            usedBaseWalkStrafeBackLeftAnimationSpeed = baseCombatWalkStrafeBackLeftAnimationSpeed;
            usedBaseStrafeForwardLeftAnimationSpeed = (absValue > baseCombatJogStrafeForwardLeftAnimationSpeed ? baseCombatJogStrafeForwardLeftAnimationSpeed : baseCombatWalkStrafeForwardLeftAnimationSpeed);
            usedBaseStrafeForwardRightAnimationSpeed = (absValue > baseCombatJogStrafeForwardRightAnimationSpeed ? baseCombatJogStrafeForwardRightAnimationSpeed : baseCombatWalkStrafeForwardRightAnimationSpeed);
        } else {
            // out of combat
            usedBaseMoveForwardAnimationSpeed = (absZValue >= 2 ? baseRunAnimationSpeed : baseWalkAnimationSpeed);
            usedbaseWalkBackAnimationSpeed = baseWalkBackAnimationSpeed;
            usedBaseStrafeLeftAnimationSpeed = (absValue > baseJogStrafeLeftAnimationSpeed ? baseJogStrafeLeftAnimationSpeed : baseWalkStrafeLeftAnimationSpeed);
            usedBaseStrafeRightAnimationSpeed = (absValue > baseJogStrafeRightAnimationSpeed ? baseJogStrafeRightAnimationSpeed : baseWalkStrafeRightAnimationSpeed);
            usedBaseWalkStrafeBackRightAnimationSpeed = baseWalkStrafeBackRightAnimationSpeed;
            usedBaseWalkStrafeBackLeftAnimationSpeed = baseWalkStrafeBackLeftAnimationSpeed;
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
            usedBaseAnimationSpeed = usedBaseStrafeLeftAnimationSpeed;
            multiplier = (absValue / usedBaseAnimationSpeed);
        } else if (varValue.x < 0 && absZValue < (absXValue / 2)) {
            // strafe left
            usedBaseAnimationSpeed = usedBaseStrafeRightAnimationSpeed;
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
        float usedBaseAnimationSpeed = (absValue <= 1 ? baseWalkAnimationSpeed : baseRunAnimationSpeed );
        //Debug.Log(gameObject.name + ".CharacterAnimator.SetVelocityZ(" + varValue + "): used: " + usedBaseAnimationSpeed + "; walk: " + baseWalkAnimationSpeed + "; run: " + baseRunAnimationSpeed);

        if (absValue != 0) {
            animationSpeed = (1 / usedBaseAnimationSpeed) * absValue;
            //animationSpeed = (1 / baseWalkAnimationSpeed);
            //Debug.Log(gameObject.name + ".CharacterAnimator.SetVelocityZ(" + varValue + "): animationSpeed: " + animationSpeed);
        }

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
            animator.SetBool(varName, varValue);
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
            animator.SetTrigger(varName);
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
        if (newItem != null && newItem.equipSlot == EquipmentSlot.MainHand && (newItem as Weapon).MyDefaultAttackAnimationProfile != null) {
            //Debug.Log(gameObject.name + ".CharacterAnimator.PerformEquipmentChange: we are animating the weapon");
            //animator.SetLayerWeight(1, 1);
            //if (weaponAnimationsDict.ContainsKey(newItem)) {
            SetAnimationProfileOverride((newItem as Weapon).MyDefaultAttackAnimationProfile);
        } else if (newItem == null && oldItem != null && oldItem.equipSlot == EquipmentSlot.MainHand) {
            //animator.SetLayerWeight(1, 0);
            //Debug.Log(gameObject.name + ".CharacterAnimator.PerformEquipmentChange: resetting the animation profile");
            ResetAnimationProfile();
        }

        // Animate grip for weapon when a shield is added or removed from hand
        if (newItem != null && newItem.equipSlot == EquipmentSlot.OffHand) {
            //Debug.Log("we are animating the shield");
            //animator.SetLayerWeight(2, 1);
        } else if (newItem == null && oldItem != null && oldItem.equipSlot == EquipmentSlot.OffHand) {
            //animator.SetLayerWeight(2, 0);
        }
    }


}
