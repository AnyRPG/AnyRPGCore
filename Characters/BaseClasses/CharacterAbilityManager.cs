using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAbilityManager : MonoBehaviour, ICharacterAbilityManager {
    public event System.Action<IAbility, float> OnCastTimeChanged = delegate { };
    public event System.Action<BaseCharacter> OnCastStop = delegate { };

    protected ICharacter baseCharacter;

    protected Coroutine currentCast = null;
    protected Coroutine abilityHitDelayCoroutine = null;
    protected Coroutine destroyAbilityEffectObjectCoroutine = null;

    protected Dictionary<string, IAbility> abilityList = new Dictionary<string, IAbility>();

    protected bool isCasting = false;

    private Vector3 groundTarget = Vector3.zero;

    private bool targettingModeActive = false;

    // does killing the player you are currently targetting stop your cast.  gets set to false when channeling aoe.
    private bool killStopCast = true;

    protected float remainingGlobalCoolDown = 0f;

    protected bool startHasRun = false;

    protected bool eventReferencesInitialized = false;

    // we need a reference to the total length of the current global cooldown to properly calculate radial fill on the action buttons
    protected float initialGlobalCoolDown;

    public float MyInitialGlobalCoolDown { get => initialGlobalCoolDown; set => initialGlobalCoolDown = value; }

    public float MyRemainingGlobalCoolDown { get => remainingGlobalCoolDown; set => remainingGlobalCoolDown = value; }

    private bool waitingForAnimatedAbility = false;

    public ICharacter MyBaseCharacter {
        get => baseCharacter;
        set => baseCharacter = value;
    }

    public Dictionary<string, IAbility> MyAbilityList { get => abilityList;}
    public bool MyWaitingForAnimatedAbility { get => waitingForAnimatedAbility; set => waitingForAnimatedAbility = value; }
    public bool MyIsCasting { get => isCasting; set => isCasting = value; }

    protected virtual void Awake() {
        //Debug.Log("CharacterAbilityManager.Awake()");
        baseCharacter = GetComponent<BaseCharacter>() as ICharacter;
        //abilityList = SystemAbilityManager.MyInstance.GetResourceList();
    }

    protected virtual void Start() {
        //Debug.Log("CharacterAbilityManager.Start()");
        startHasRun = true;
        UpdateAbilityList(baseCharacter.MyCharacterStats.MyLevel);
        CreateEventReferences();
    }

    public virtual void CreateEventReferences() {
        if (eventReferencesInitialized || !startHasRun) {
            return;
        }
        SystemEventManager.MyInstance.OnLevelChanged += UpdateAbilityList;
        baseCharacter.MyCharacterCombat.OnKillEvent += ReceiveKillDetails;
        SystemEventManager.MyInstance.OnLevelUnload += HandleLevelUnload;
        if (baseCharacter != null && baseCharacter.MyCharacterStats != null) {
            baseCharacter.MyCharacterStats.OnDie += OnDieHandler;
        }
        eventReferencesInitialized = true;
    }

    public virtual void CleanupEventReferences() {
        if (!eventReferencesInitialized) {
            return;
        }
        if (SystemEventManager.MyInstance != null) {
            SystemEventManager.MyInstance.OnLevelChanged -= UpdateAbilityList;
            SystemEventManager.MyInstance.OnLevelUnload -= HandleLevelUnload;
        }
        if (baseCharacter != null && baseCharacter.MyCharacterCombat != null) {
            baseCharacter.MyCharacterCombat.OnKillEvent -= ReceiveKillDetails;
        }
        if (baseCharacter != null && baseCharacter.MyCharacterStats != null) {
            baseCharacter.MyCharacterStats.OnDie -= OnDieHandler;
        }
        OnCharacterUnitDespawn();
        eventReferencesInitialized = false;
    }

    public virtual void OnDisable() {
        CleanupEventReferences();
        CleanupCoroutines();
    }

    public virtual void CleanupCoroutines() {
        //Debug.Log(gameObject.name + ".CharacterAbilitymanager.CleanupCoroutines()");
        if (currentCast != null) {
            StopCoroutine(currentCast);
            currentCast = null;
        }
        if (abilityHitDelayCoroutine != null) {
            StopCoroutine(abilityHitDelayCoroutine);
            abilityHitDelayCoroutine = null;
        }

        if (destroyAbilityEffectObjectCoroutine != null) {
            StopCoroutine(destroyAbilityEffectObjectCoroutine);
            destroyAbilityEffectObjectCoroutine = null;
        }
    }

    public virtual void OnDieHandler(CharacterStats _characterStats) {
        //Debug.Log(gameObject.name + ".OnDieHandler()");

        MyWaitingForAnimatedAbility = false;
    }


    /// <summary>
    /// Called when the type of cast should not be interrupted by the death of your current mob target
    /// </summary>
    public void KillStopCastOverride() {
        //Debug.Log("CharacterAbilityManager.KillStopCastOverride()");

        killStopCast = false;
    }

    /// <summary>
    /// Called when the type of cast should be interrupted by the death of your current mob target
    /// </summary>
    public void KillStopCastNormal() {
        //Debug.Log("CharacterAbilityManager.KillStopCastNormal()");
        killStopCast = true;
    }

    public bool HasAbility (string abilityName) {
        //Debug.Log(gameObject.name + ".CharacterAbilitymanager.HasAbility(" + abilityName + ")");
        string keyName = SystemResourceManager.prepareStringForMatch(abilityName);
        //Debug.Log(gameObject.name + ".CharacterAbilitymanager.HasAbility(" + abilityName + "): keyname: " + keyName);
        if (MyAbilityList.ContainsKey(keyName)) {
            //Debug.Log(gameObject.name + ".CharacterAbilitymanager.HasAbility( " + abilityName + "): keyname: " + keyName + " TRUE!");
            return true;
        }
        //Debug.Log(gameObject.name + ".CharacterAbilitymanager.HasAbility( " + abilityName + "): keyname: " + keyName + " FALSE!");
        return false;
    }

    public void ActivateTargettingMode(Color groundTargetColor) {
        Debug.Log("CharacterAbilityManager.ActivateTargettingMode()");
        targettingModeActive = true;
        CastTargettingManager.MyInstance.EnableProjector(groundTargetColor);
    }

    public bool WaitingForTarget() {
        //Debug.Log("CharacterAbilityManager.WaitingForTarget(): returning: " + targettingModeActive);
        return targettingModeActive;
    }

    private Vector3 GetGroundTarget() {
        //Debug.Log("CharacterAbilityManager.GetGroundTarget(): returning: " + groundTarget);
        return groundTarget;
    }

    public void SetGroundTarget(Vector3 newGroundTarget) {
        Debug.Log("CharacterAbilityManager.SetGroundTarget(" + newGroundTarget + ")");
        groundTarget = newGroundTarget;
        DeActivateTargettingMode();
    }

    public void DeActivateTargettingMode() {
        //Debug.Log("CharacterAbilityManager.DeActivateTargettingMode()");
        targettingModeActive = false;
        CastTargettingManager.MyInstance.DisableProjector();
    }

    public void OnCharacterUnitSpawn() {
        //Debug.Log("CharacterAbilityManager.OnCharacterUnitSpawn()");
        PlayerUnitMovementController movementController = MyBaseCharacter.MyCharacterUnit.GetComponent<PlayerUnitMovementController>();
        //CharacterMotor characterMotor = MyBaseCharacter.MyCharacterUnit.MyCharacterMotor;
        if (movementController != null) {
            movementController.OnMovement += OnManualMovement;
        }
        if (MyBaseCharacter.MyCharacterUnit.MyCharacterMotor != null) {
            //Debug.Log("CharacterAbilityManager.OnCharacterUnitSpawn(): CharacterMotor is not null");
            MyBaseCharacter.MyCharacterUnit.MyCharacterMotor.OnMovement += OnManualMovement;
        } else {
            Debug.Log("CharacterAbilityManager.OnCharacterUnitSpawn(): CharacterMotor is null!");
        }
    }

    public void OnCharacterUnitDespawn() {
        //Debug.Log(gameObject.name + ".CharacterAbilityManager.OnCharacterUnitDespawn()");
        if (MyBaseCharacter != null && MyBaseCharacter.MyCharacterUnit != null) {
            PlayerUnitMovementController movementController = MyBaseCharacter.MyCharacterUnit.GetComponent<PlayerUnitMovementController>();
            if (movementController != null) {
                movementController.OnMovement -= OnManualMovement;
            }
        }
    }

    public virtual void UpdateAbilityList(int newLevel) {
        //Debug.Log(gameObject.name + ".CharacterAbilityManager.UpdateAbilityList(). length: " + abilityList.Count);
        foreach (BaseAbility ability in SystemAbilityManager.MyInstance.GetResourceList()) {
            if (ability.MyRequiredLevel <= newLevel && ability.MyAutoLearn == true) {
                if (!HasAbility(ability.MyName)) {
                    LearnAbility(ability.MyName);
                } else {
                    //Debug.Log(ability.MyName + " already known, no need to re-learn");
                }
            }
        }
    }

    public virtual bool LearnAbility(string abilityName) {
        //Debug.Log(gameObject.name + ".CharacterAbilityManager.LearnAbility()");
        string keyName = SystemResourceManager.prepareStringForMatch(abilityName);
        BaseAbility baseAbility = SystemAbilityManager.MyInstance.GetResource(abilityName);
        if (!HasAbility(abilityName) && baseAbility.MyRequiredLevel <= MyBaseCharacter.MyCharacterStats.MyLevel) {
            abilityList[keyName] = baseAbility;
            return true;
        }
        return false;
    }

    public void UnlearnAbility(string abilityName) {
        string keyName = SystemResourceManager.prepareStringForMatch(abilityName);
        if (abilityList.ContainsKey(keyName)) {
            abilityList.Remove(keyName);
            /*
             * Fix this so we remove abilities we don't have from our bars ?  or just keep them there but disabled?
            if (OnAbilityListChanged != null) {
                OnAbilityListChanged(ability);
            }
            */
        }
        // attemp to remove from bars
        UIManager.MyInstance.MyActionBarManager.UpdateVisuals(true);
    }

    /// <summary>
    /// Cast a spell with a cast timer
    /// </summary>
    /// <param name="ability"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public IEnumerator PerformAbilityCast(IAbility ability, GameObject target) {
        float startTime = Time.time;
        //Debug.Log("CharacterAbilitymanager.PerformAbilityCast(" + ability.MyName + ") Enter Ienumerator with tag: " + startTime);
        bool canCast = true;
        if (ability.MyRequiresTarget == false || ability.MyCanCastOnEnemy == false) {
            // prevent the killing of your enemy target from stopping aoe casts and casts that cannot be cast on an ememy
            KillStopCastOverride();
        } else {
            KillStopCastNormal();
        }
        if (ability.MyRequiresGroundTarget == true) {
            //Debug.Log("CharacterAbilitymanager.PerformAbilityCast() Ability requires a ground target.");
            ActivateTargettingMode(ability.MyGroundTargetColor);
            while (WaitingForTarget() == true) {
                //Debug.Log("CharacterAbilitymanager.PerformAbilityCast() waiting for target");
                yield return null;
            }
            if (GetGroundTarget() == Vector3.zero) {
                //Debug.Log("Ground Targetting: groundtarget is vector3.zero, cannot cast");
                canCast = false;
            }
        }
        if (canCast == true) {
            //Debug.Log("Ground Targetting: cancast is true");
            // TESTING ORDERING
            if (!ability.MyCanSimultaneousCast) {
                //Debug.Log("CharacterAbilitymanager.PerformAbilityCast() ability: " + ability.MyName + " can simultaneous cast is false, setting casting to true");
                // i think this should work
                //isCasting = true;
                //isCasting = true;
                ability.StartCasting(baseCharacter as BaseCharacter);
            }
            float currentCastTime = 0f;
            //Debug.Log("CharacterAbilitymanager.PerformAbilityCast() currentCastTime: " + currentCastTime + "; MyAbilityCastingTime: " + ability.MyAbilityCastingTime);
            while (currentCastTime < ability.MyAbilityCastingTime) {
                currentCastTime += Time.deltaTime;

                // call this first because it updates the cast bar
                //Debug.Log("CharacterAbilitymanager.PerformAbilityCast() currentCastTime: " + currentCastTime + "; MyAbilityCastingTime: " + ability.MyAbilityCastingTime + "; calling OnCastTimeChanged()");
                OnCastTimeChanged(ability, currentCastTime);

                // now call the ability on casttime changed (really only here for channeled stuff to do damage)
                ability.OnCastTimeChanged(currentCastTime, baseCharacter as BaseCharacter, target);

                yield return null;
            }
        }

        //Debug.Log(gameObject + ".CharacterAbilityManager.PerformAbilityCast(). nulling tag: " + startTime);
        currentCast = null;

        // I REALLY HOPE THIS DOESN'T BREAK SHIT.  BECAUSE UNITY IS RETARDED AND DOESN'T ACTUALLY STOP A COROUTINE WHEN YOU CALL STOP FUCKING COROUTINE, WE HAVE TO SET THAT SHIT TO NULL AND THEN COMPLETE THE ROUTINE
        // OTHERWISE WE WILL ATTEMPT TO PERFORM A CAST AND IT WILL NOT BE STOPPED EVEN THOUGH WE TOLD IT TO STOP AND EVERYTHING ELSE AFTER THAT DEPENDS ON IT ACTUALLY BEING FUCKING STOPPED WILL FAIL.
        if (canCast) {
            //Debug.Log(gameObject.name + ".CharacterAbilitymanager.PerformAbilityCast(): Cast Complete currentCastTime: " + currentCastTime + "; abilitycastintime: " + ability.MyAbilityCastingTime);
            if (!ability.MyCanSimultaneousCast) {
                OnCastStop(MyBaseCharacter as BaseCharacter);
                MyBaseCharacter.MyCharacterUnit.MyCharacterAnimator.SetCasting(false);
            }
            PerformAbility(ability, target, GetGroundTarget());

        }
    }

    public void ReceiveKillDetails(BaseCharacter killedcharacter, float creditPercent) {
        //Debug.Log("CharacterAbilityManager.ReceiveKillDetails()");
        if (MyBaseCharacter.MyCharacterController.MyTarget == killedcharacter.MyCharacterUnit.gameObject) {
            if (killStopCast) {
                StopCasting();
            }
        }
    }

    /// <summary>
    /// The entrypoint to Casting a spell.  handles all logic such as instant/timed cast, current cast in progress, enough mana, target being alive etc
    /// </summary>
    /// <param name="ability"></param>
    public void BeginAbility(IAbility ability) {
        //Debug.Log("CharacterAbilitymanager.BeginAbility()");
        if (ability == null) {
            //Debug.Log("CharacterAbilityManager.BeginAbility(): ability is null! Exiting!");
            return;
        } else {
            //Debug.Log("CharacterAbilityManager.BeginAbility(" + ability.MyName + ")");
        }
        BeginAbilityCommon(ability, baseCharacter.MyCharacterController.MyTarget);
    }

    public void BeginAbility(IAbility ability, GameObject target) {
        //Debug.Log("CharacterAbilityManager.BeginAbility(" + ability.MyName + ")");
        BeginAbilityCommon(ability, target);
    }

    private void BeginAbilityCommon(IAbility ability, GameObject target) {
        //Debug.Log("CharacterAbilityManager.BeginAbilityCommon(" + ability.MyName + ", " + (target == null ? "null" : target.name) + ")");
        IAbility usedAbility = SystemAbilityManager.MyInstance.GetResource(ability.MyName);
        string keyName = SystemResourceManager.prepareStringForMatch(ability.MyName);

        // check if the ability is learned yet
        if (!usedAbility.MyUseableWithoutLearning && !abilityList.ContainsKey(keyName)) {
            //Debug.Log("ability.MyUseableWithoutLearning: " + ability.MyUseableWithoutLearning + "; abilityList.Contains(" + usedAbility.MyName + "): " + abilityList.Contains(usedAbility));
            return;
        }

        // check if the ability is on cooldown
        if (usedAbility.MyRemainingCoolDown > 0f) {
            //CombatLogUI.MyInstance.WriteCombatMessage(ability.MyName + " is on cooldown: " + SystemAbilityManager.MyInstance.GetResource(ability.MyName).MyRemainingCoolDown);
            // write some common notify method here that only has content in it in playerabilitymanager to show messages so don't get spammed with npc messages
            return;
        }

        // check if we have enough mana
        if (MyBaseCharacter.MyCharacterStats.currentMana < usedAbility.MyAbilityManaCost) {
            //CombatLogUI.MyInstance.WriteCombatMessage("Not enough mana to perform " + ability.MyName + " at a cost of " + ability.MyAbilityManaCost.ToString());
            return;
        }

        // get final target before beginning casting
        GameObject finalTarget = usedAbility.ReturnTarget(baseCharacter as BaseCharacter, target);

        // perform ability dependent checks
        if (!usedAbility.CanUseOn(finalTarget, baseCharacter as BaseCharacter) == true) {
            //Debug.Log("ability.CanUseOn(" + ability.MyName + ", " + (target != null ? target.name : "null") + " was false.  exiting");
            return;
        }

        if (usedAbility.MyCanSimultaneousCast) {
            // directly performing to avoid interference with other abilities being casted
            PerformAbility(usedAbility, finalTarget, GetGroundTarget());
        } else {
            if (currentCast == null) {
                //Debug.Log("Performing Ability " + ability.MyName + " at a cost of " + ability.MyAbilityManaCost.ToString() + ": ABOUT TO START COROUTINE");

                // we need to do this because we are allowed to stop an outstanding auto-attack to start this cast
                MyBaseCharacter.MyCharacterUnit.MyCharacterAnimator.ClearAnimationBlockers();

                // start the cast (or cast targetting projector)
                currentCast = StartCoroutine(PerformAbilityCast(usedAbility, finalTarget));
            } else {
                //CombatLogUI.MyInstance.WriteCombatMessage("A cast was already in progress WE SHOULD NOT BE HERE BECAUSE WE CHECKED FIRST! iscasting: " + isCasting + "; currentcast==null? " + (currentCast == null));
                // unless.... we got here from the crafting queue, which launches the next item as the last step of the currently in progress cast
                //Debug.Log("A cast was already in progress!");
            }
        }
    }

    /// <summary>
    /// Casts a spell.  Note that this does not do the actual damage yet since the ability may have a travel time
    /// </summary>
    /// <param name="ability"></param>
    /// <param name="target"></param>
    public virtual void PerformAbility(IAbility ability, GameObject target, Vector3 groundTarget) {
        //Debug.Log(gameObject.name + ".CharacterAbilityManager.PerformAbility(" + ability.MyName + ")");
        GameObject finalTarget = target;
        if (finalTarget != null) {
            //Debug.Log(gameObject.name + ": performing ability: " + ability.MyName + " on " + finalTarget.name);
        } else {
            //Debug.Log(gameObject.name + ": performing ability: " + ability.MyName + ": finalTarget is null");
        }

        if (MyBaseCharacter.MyCharacterStats.currentMana < ability.MyAbilityManaCost) {
            CombatLogUI.MyInstance.WriteCombatMessage("Not enough mana to perform " + ability.MyName + " at a cost of " + ability.MyAbilityManaCost.ToString());
            //Debug.Log("Not enough mana to perform " + ability.MyName + " at a cost of " + ability.MyAbilityManaCost.ToString());
            return;
        }

        if (ability.MyAbilityManaCost != 0) {
            MyBaseCharacter.MyCharacterStats.UseMana(ability.MyAbilityManaCost);
        }

        // cast the system manager version so we can track globally the spell cooldown
        SystemAbilityManager.MyInstance.GetResource(ability.MyName).Cast(baseCharacter as BaseCharacter, finalTarget, groundTarget);
        //ability.Cast(MyBaseCharacter.MyCharacterUnit.gameObject, finalTarget);
    }

    /// <summary>
    /// Stop casting if the character is manually moved with the movement keys
    /// </summary>
    public void OnManualMovement() {
        //Debug.Log("CharacterAbilityManager.OnmanualMovement(): Received On Manual Movement Handler");
        StopCasting();
    }

    public virtual void StopCasting() {
        //Debug.Log(gameObject.name + ".CharacterAbilityManager.StopCasting()");
        // TESTING - REMOVED ISCASTING == TRUE BECAUSE IT WAS PREVENTING THE CRAFTING QUEUE FROM WORKING.  TECHNICALLY THIS GOT CALLED RIGHT AFTER ISCASTING WAS SET TO FALSE, BUT BEFORE CURRENTCAST WAS NULLED
        if (currentCast != null) {
            //if (currentCast != null && isCasting == true) {
            //Debug.Log(gameObject.name + ".CharacterAbilityManager.StopCasting(): currentCast is not null, stopping coroutine");
            StopCoroutine(currentCast);
            currentCast = null;
        } else {
            //Debug.Log(gameObject.name + ".currentCast is null, nothing to stop");
        }
        MyBaseCharacter.MyCharacterUnit.MyCharacterAnimator.ClearAnimationBlockers();
        OnCastStop(MyBaseCharacter as BaseCharacter);
    }

    public void HandleLevelUnload() {
        StopCasting();
        MyWaitingForAnimatedAbility = false;
    }

    public void BeginPerformAbilityHitDelay(BaseCharacter source, GameObject target, AbilityEffectOutput abilityEffectInput, ChanneledEffect channeledEffect) {
        abilityHitDelayCoroutine = StartCoroutine(PerformAbilityHitDelay(source, target, abilityEffectInput, channeledEffect));
    }

    public IEnumerator PerformAbilityHitDelay(BaseCharacter source, GameObject target, AbilityEffectOutput abilityEffectInput, ChanneledEffect channeledEffect) {
        //Debug.Log("ChanelledEffect.PerformAbilityEffectDelay()");
        float timeRemaining = channeledEffect.effectDelay;
        while (timeRemaining > 0f) {
            timeRemaining -= Time.deltaTime;
            yield return null;
        }
        channeledEffect.PerformAbilityHit(source, target, abilityEffectInput);
        abilityHitDelayCoroutine = null;
    }

    public void BeginDestroyAbilityEffectObject(GameObject abilityEffectObject, BaseCharacter source, GameObject target, float timer, AbilityEffectOutput abilityEffectInput, FixedLengthEffect fixedLengthEffect) {
        destroyAbilityEffectObjectCoroutine = StartCoroutine(DestroyAbilityEffectObject(abilityEffectObject, source, target, timer, abilityEffectInput, fixedLengthEffect));
    }

    public IEnumerator DestroyAbilityEffectObject(GameObject abilityEffectObject, BaseCharacter source, GameObject target, float timer, AbilityEffectOutput abilityEffectInput, FixedLengthEffect fixedLengthEffect) {
        //Debug.Log("FixedLengthEffect.DestroyAbilityEffectObject(" + timer + ")");
        float timeRemaining = timer;

        ICharacterStats targetStats = null;
        if (target != null) {
            targetStats = target.GetComponent<CharacterUnit>().MyCharacter.MyCharacterStats;
        }

        int milliseconds = (int)((fixedLengthEffect.MyTickRate - (int)fixedLengthEffect.MyTickRate) * 1000);
        float finalTickRate = fixedLengthEffect.MyTickRate;
        if (finalTickRate == 0) {
            finalTickRate = timer + 1;
        }
        //Debug.Log(abilityEffectName + ".StatusEffect.Tick() milliseconds: " + milliseconds);
        TimeSpan tickRateTimeSpan = new TimeSpan(0, 0, 0, (int)finalTickRate, milliseconds);
        //Debug.Log(abilityEffectName + ".StatusEffect.Tick() tickRateTimeSpan: " + tickRateTimeSpan);
        fixedLengthEffect.MyNextTickTime = System.DateTime.Now + tickRateTimeSpan;
        //Debug.Log(abilityEffectName + ".FixedLengthEffect.Tick() nextTickTime: " + nextTickTime);

        while (timeRemaining > 0f) {
            if (fixedLengthEffect.MyPrefabSpawnLocation != PrefabSpawnLocation.Point && fixedLengthEffect.MyRequiresTarget == true && (target == null || (targetStats.IsAlive == true && fixedLengthEffect.MyRequireDeadTarget == true) || (targetStats.IsAlive == false && fixedLengthEffect.MyRequiresLiveTarget == true))) {
                //Debug.Log("BREAKING!!!!!!!!!!!!!!!!!");
                break;
            } else {
                timeRemaining -= Time.deltaTime;
                if (System.DateTime.Now > fixedLengthEffect.MyNextTickTime) {
                    //Debug.Log(abilityEffectName + ".FixedLengthEffect.Tick() TickTime!");
                    fixedLengthEffect.CastTick(source, target, abilityEffectInput);
                    fixedLengthEffect.MyNextTickTime += tickRateTimeSpan;
                }
            }
            yield return null;
        }
        //Debug.Log(abilityEffectName + ".FixedLengthEffect.Tick() Done ticking and about to perform ability affects.");
        fixedLengthEffect.CastComplete(source, target, abilityEffectInput);
        Destroy(abilityEffectObject, fixedLengthEffect.MyPrefabDestroyDelay);

        destroyAbilityEffectObjectCoroutine = null;
    }

}
