using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterStats))]
public class CharacterCombat : MonoBehaviour, ICharacterCombat {

    //events
    public virtual event System.Action<BaseCharacter, float> OnKillEvent = delegate { };
    public virtual event System.Action<BaseCharacter> OnAttack = delegate { };
    public virtual event System.Action OnDropCombat = delegate { };
    public virtual event System.Action OnEnterCombat = delegate { };
    public virtual event System.Action<BaseCharacter, GameObject> OnHitEvent = delegate { };

    protected bool startHasRun = false;
    protected bool eventReferencesInitialized = false;

    /// <summary>
    /// Attack speed is based on animation speed.  This is a limiter in case animations are too fast.
    /// </summary>
    public float attackSpeed = 1f;

    public bool autoAttack = true;
    public bool autoAttackActive = false;
    public float combatCooldown = 10f;
    protected float lastCombatEvent;

    //public bool isAttacking { get; private set; }
    protected bool inCombat = false;

    protected float attackCooldown = 0f;

    // components
    protected ICharacter baseCharacter;
    //protected CharacterCombat opponentCombat;

    protected BaseAbility onHitAbility = null;

    protected AggroTable aggroTable = null;

    // this is what the current weapon defaults to.  I't
    protected AudioClip defaultHitSoundEffect = null;

    // this holds the current weapon default, is null when special ability going, then resets back to default value after.
    // if this value is null, and default is not, no sound is played, if both null, system default played
    protected AudioClip overrideHitSoundEffect = null;

    /// <summary>
    ///  waiting for the animator to let us know we can hit again
    /// </summary>
    private bool waitingForAutoAttack = false;

    protected Coroutine regenRoutine = null;

    // the target we swung at, in case we try to change target mid swing and we don't put an animation on something too far away
    protected BaseCharacter swingTarget = null;

    public AggroTable MyAggroTable {
        get {
            return aggroTable;
        }
    }

    public ICharacter MyBaseCharacter { get => baseCharacter; set => baseCharacter = value; }
    public bool MyWaitingForAutoAttack {
        get => waitingForAutoAttack;
        set {
            //Debug.Log(gameObject.name + ".CharacterCombat.MyWaitingForHits Setting waitingforHits to: " + value);
            waitingForAutoAttack = value;
        } 
    }

    public AudioClip MyDefaultHitSoundEffect { get => defaultHitSoundEffect; set => defaultHitSoundEffect = value; }
    public AudioClip MyOverrideHitSoundEffect { get => overrideHitSoundEffect; set => overrideHitSoundEffect = value; }
    public BaseCharacter MySwingTarget { get => swingTarget; set => swingTarget = value; }

    private void Awake() {
        //Debug.Log(gameObject.name + ".CharacterCombat.Awake()");
        baseCharacter = GetComponent<BaseCharacter>();
        aggroTable = new AggroTable(baseCharacter as BaseCharacter);
    }

    public virtual void Start() {
        startHasRun = true;
        CreateEventReferences();

        //GetComponent<CharacterStats>().OnTakeDamage += OnTakeDamage;
    }

    protected virtual void CreateEventReferences() {
        if (eventReferencesInitialized || !startHasRun) {
            return;
        }
        if (baseCharacter!= null && baseCharacter.MyCharacterStats != null) {
            baseCharacter.MyCharacterStats.OnDie += OnDieHandler;
        }
        SystemEventManager.MyInstance.OnLevelUnload += HandleLevelUnload;
        eventReferencesInitialized = true;
    }

    protected virtual void CleanupEventReferences() {
        //Debug.Log("PlayerCombat.CleanupEventReferences()");
        if (baseCharacter != null && baseCharacter.MyCharacterStats != null) {
            baseCharacter.MyCharacterStats.OnDie -= OnDieHandler;
        }
        if (SystemEventManager.MyInstance != null) {
            SystemEventManager.MyInstance.OnLevelUnload -= HandleLevelUnload;
        }
    }

    public virtual void OnEnable() {
        CreateEventReferences();
    }

    public virtual void OnDisable() {
        //Debug.Log(gameObject.name + ".CharacterCombat.OnDisable()");
        CleanupEventReferences();
        AttemptStopRegen();
    }

    public void HandleLevelUnload() {
        SetWaitingForAutoAttack(false);
        DropCombat();
        AttemptStopRegen();
    }

    protected virtual void Update() {
        if (!baseCharacter.MyCharacterStats.IsAlive) {
            return;
        }

        // reduce the autoattack cooldown
        if (attackCooldown > 0f ) {
            //Debug.Log(gameObject.name + ".CharacterCombat.Update(): attackCooldown: " + attackCooldown);
            attackCooldown -= Time.deltaTime;
        }

        if (inCombat && baseCharacter.MyCharacterController.MyTarget == null) {
            TryToDropCombat();
        }
    }

    public IEnumerator outOfCombatRegen() {
        //Debug.Log(gameObject.name + ".CharacterCombat.outOfCombatRegen() beginning");
        if (baseCharacter != null && baseCharacter.MyCharacterStats != null ) {
            while (baseCharacter.MyCharacterStats.currentHealth < baseCharacter.MyCharacterStats.MyMaxHealth || baseCharacter.MyCharacterStats.currentMana < baseCharacter.MyCharacterStats.MyMaxMana) {
                yield return new WaitForSeconds(1);
                int healthAmount = baseCharacter.MyCharacterStats.MyMaxHealth / 100;
                int manaAmount = baseCharacter.MyCharacterStats.MyMaxMana / 100;
                //Debug.Log(gameObject.name + ".CharacterCombat.outOfCombatRegen() beginning; about to recover health: " + healthAmount + "; mana: " + manaAmount);
                baseCharacter.MyCharacterStats.RecoverHealth(healthAmount, baseCharacter as BaseCharacter, false);
                baseCharacter.MyCharacterStats.RecoverMana(manaAmount, baseCharacter as BaseCharacter, false);
            }
        }
        //Debug.Log(gameObject.name + ".CharacterCombat.outOfCombatRegen() ending naturally on full health");
        regenRoutine = null;
    }

    public virtual void OnDieHandler(CharacterStats _characterStats) {
        //Debug.Log(gameObject.name + ".OnDieHandler()");
        SetWaitingForAutoAttack(false);

        BroadcastCharacterDeath();
    }

    public void SetWaitingForAutoAttack(bool newValue) {
        //Debug.Log(gameObject.name + ".CharacterCombat.SetWaitingForAutoAttack(" + newValue + ")");
        MyWaitingForAutoAttack = newValue;
    }

    public virtual void ProcessTakeDamage(int damage, BaseCharacter target, CombatType combatType, CombatMagnitude combatMagnitude, string abilityName) {
        //Debug.Log("OnTakeDamageHandler activated on " + gameObject.name);
        AbilityEffectOutput abilityAffectInput = new AbilityEffectOutput();
        abilityAffectInput.healthAmount = damage;
        foreach (StatusEffectNode statusEffectNode in MyBaseCharacter.MyCharacterStats.MyStatusEffects.Values) {
            //Debug.Log("Casting Reflection On Take Damage");
            // this could maybe be done better through an event subscription
            if (statusEffectNode.MyStatusEffect.MyReflectAbilityEffectList.Count > 0) {
                statusEffectNode.MyStatusEffect.CastReflect(MyBaseCharacter as BaseCharacter, target.MyCharacterUnit.gameObject, abilityAffectInput);
            }
        }

        if (target != null && PlayerManager.MyInstance && PlayerManager.MyInstance.MyCharacter != null && PlayerManager.MyInstance.MyCharacter.MyCharacterUnit != null && PlayerManager.MyInstance.MyPlayerUnitObject != null && baseCharacter != null && baseCharacter.MyCharacterUnit != null) {
            if (target == PlayerManager.MyInstance.MyCharacter || (PlayerManager.MyInstance.MyCharacter as BaseCharacter) == (baseCharacter as BaseCharacter)) {
                // spawn text over enemies damaged by the player and over the player itself
                CombatTextManager.MyInstance.SpawnCombatText(baseCharacter.MyCharacterUnit.gameObject, damage, combatType, combatMagnitude);
                SystemEventManager.MyInstance.NotifyOnTakeDamage(target, MyBaseCharacter.MyCharacterUnit, damage, abilityName);
            }
            lastCombatEvent = Time.time;
            if (target.MyCharacterStats.IsAlive) {
                aggroTable.AddToAggroTable(target.MyCharacterUnit, damage);
                EnterCombat(target);
            } else {
                //Debug.Log(gameObject.name + ".CharacterCombat.OnTakeDamage(): Received damage from dead character.  Ignoring aggro mechanics.");
            }
        }

    }

    public virtual void TryToDropCombat() {
        if (inCombat == false) {
            //Debug.Log(gameObject.name + ".TryToDropCombat(): incombat = false. returning");
            return;
        }
        //Debug.Log(gameObject.name + " trying to drop combat.");
        if (aggroTable.MyTopAgroNode == null) {
            //Debug.Log(gameObject.name + ".TryToDropCombat(): topAgroNode is null. Dropping combat.");
            DropCombat();
        } else {
            //Debug.Log(gameObject.name + ".TryToDropCombat(): topAgroNode was not null");
            // this next condition should prevent crashes as a result of level unloads
            if (MyBaseCharacter.MyCharacterUnit != null) {
                foreach (AggroNode aggroNode in MyAggroTable.MyAggroNodes) {
                    AIController _aiController = aggroNode.aggroTarget.MyCharacter.MyCharacterController as AIController;
                    // since players don't have an agro radius, we can skip the check and drop combat automatically
                    if (_aiController != null) {
                        if (Vector3.Distance(MyBaseCharacter.MyCharacterUnit.transform.position, aggroNode.aggroTarget.transform.position) < _aiController.MyAggroRange) {
                            // fail if we are inside the agro radius of an AI on our agro table
                            return;
                        }
                    }
                }
            }

            // we made it through the loop without returning.  we are allowed to leave combat.
            // FYI THIS CODE ALLOWS YOU TO DROP COMBAT WHILE STILL ON SOMETHING'S AGRO TABLE
            // THIS SHOULD NOT BE AN ISSUE THOUGH, BECAUSE OnTriggerEnter IN AGGRONODE WILL RE-ADD WHEN THE THING CATCHES UP TO YOU AND GETS BACK IN RANGE
            // DOING THIS SO YOU DON'T LOSE KILL CREDIT IF YOU WALK AWAY TO DO OUT OF COMBAT REGEN
            DropCombat();
        }
    }

    protected virtual void DropCombat() {
        //Debug.Log(gameObject.name + ".CharacterCombat.DropCombat()");
        inCombat = false;
        SetWaitingForAutoAttack(false);
        if (baseCharacter != null && baseCharacter.MyCharacterAbilityManager != null) {
            baseCharacter.MyCharacterAbilityManager.MyWaitingForAnimatedAbility = false;
        }
        if (baseCharacter != null && baseCharacter.MyCharacterUnit != null && baseCharacter.MyCharacterUnit.MyCharacterAnimator != null) {
            baseCharacter.MyCharacterUnit.MyCharacterAnimator.SetBool("InCombat", false);
        }
        DeActivateAutoAttack();
        //Debug.Log(gameObject.name + ".CharacterCombat.DropCombat(): dropped combat.");
        OnDropCombat();
    }

    public void ActivateAutoAttack() {
        autoAttackActive = true;
    }

    public void DeActivateAutoAttack() {
        autoAttackActive = false;
    }

    public bool GetInCombat() {
        return inCombat;
    }

    /// <summary>
    /// Adds the target to the aggro table with 0 agro to ensure you have something to send a drop combat message to if you did 0 damage to them during combat.
    /// Set inCombat to true.
    /// </summary>
    /// <param name="target"></param>
    /// return true if this is a new entry, false if not
    public virtual bool EnterCombat(BaseCharacter target) {
        //Debug.Log(gameObject.name + ".CharacterCombat.EnterCombat(" + (target != null && target.MyName != null ? target.MyName : "null") + ")");
        if (MyBaseCharacter.MyCharacterStats.IsAlive == false) {
            //Debug.Log(gameObject.name + ".CharacterCombat.EnterCombat(" + (target != null && target.MyName != null ? target.MyName : "null") + "): character is not alive, returning!");
            return false;
        }
        // try commenting this out to fix bug where things that have agrod but done no damage don't get death notifications
        //if (!inCombat) {
        //Debug.Log(gameObject.name + " Entering Combat with " + target.name);
        //}
        lastCombatEvent = Time.time;
        // maybe do this in update?
        baseCharacter.MyCharacterUnit.MyCharacterAnimator.SetBool("InCombat", true);
        inCombat = true;
        OnEnterCombat();
        if (aggroTable.AddToAggroTable(target.MyCharacterUnit, 0)) {
            return true;
        }
        return false;
    }

    protected virtual bool CanPerformAutoAttack(BaseCharacter characterTarget) {
        //Debug.Log(gameObject.name + ".CharacterCombat.CanPerformAutoAttack(" + characterTarget.MyCharacterName + ")");
        if (!AutoAttackTargetIsValid(characterTarget)) {
            return false;
        }
        if (!baseCharacter.MyCharacterController.IsTargetInHitBox(characterTarget.MyCharacterUnit.gameObject)) {
            // target is too far away, can't attack
            //Debug.Log(gameObject.name + ".CharacterCombat.CanPerformAutoAttack(" + characterTarget.MyCharacterName + ") target is too far away, can't attack");
            return false;
        }
        if (attackCooldown > 0f) {
            // still waiting for attack cooldown, can't attack
            //Debug.Log(gameObject.name + ".CharacterCombat.CanPerformAutoAttack(" + characterTarget.MyCharacterName + ") still waiting for attack cooldown (" + attackCooldown + "), can't attack");
            return false;
        }
        if (MyWaitingForAutoAttack == true) {
            // there is an existing autoattack in progress, can't start a new auto-attack
            //Debug.Log(gameObject.name + ".CharacterCombat.CanPerformAutoAttack(" + characterTarget.MyCharacterName + ") autoattack in progress, can't start a new auto-attack");
            return false;
        }
        if (MyBaseCharacter.MyCharacterAbilityManager != null && MyBaseCharacter.MyCharacterAbilityManager.MyWaitingForAnimatedAbility == true) {
            // if we have an ability manager and there is an outstanding special attack in progress, can't auto-attack
            //Debug.Log(gameObject.name + ".CharacterCombat.CanPerformAutoAttack(" + characterTarget.MyCharacterName + ") special attack in progress, can't auto-attack");
            return false;
        }
        if (MyBaseCharacter.MyCharacterAbilityManager != null && MyBaseCharacter.MyCharacterAbilityManager.MyIsCasting == true) {
            // there is a spell cast in progress, can't start a new auto-attack
            //Debug.Log(gameObject.name + ".CharacterCombat.CanPerformAutoAttack(" + characterTarget.MyCharacterName + ") there is a spell cast in progress, can't start a new auto-attack");
            return false;
        }
        if (MyBaseCharacter.MyCharacterUnit != null && MyBaseCharacter.MyCharacterUnit.MyCharacterAnimator != null && MyBaseCharacter.MyCharacterUnit.MyCharacterAnimator.WaitingForAnimation() == true) {
            // all though there are no casts in progress, a current animation is still finishing, so we can't start a new auto-attack yet
            // this can happen when an animation for a casted ability lasts longer than the actual cast time, for abilities that do their damage part way through the animation
            //Debug.Log(gameObject.name + ".CharacterCombat.CanPerformAutoAttack(" + characterTarget.MyCharacterName + ") WaitingForAnimation() == true");
            return false;
        }
        // there are no blockers to an attack, we can start an auto-attack
        return true;
    }

    protected virtual bool AutoAttackTargetIsValid(BaseCharacter characterTarget) {
        // ensure the current target is the target we swung at in case we switched target mid swing via tab/agro etc
        // this helps prevent spell hit effects from triggering on the wrong unit
        if (characterTarget != swingTarget) {
            return false;
        }
        if (Faction.RelationWith(characterTarget, MyBaseCharacter as BaseCharacter) > -1) {
            return false;
        }
        if (!characterTarget.MyCharacterStats.IsAlive) {
            return false;
        }
        return true;
    }


    /// <summary>
    /// This is the entrypoint to a manual attack.
    /// </summary>
    /// <param name="characterTarget"></param>
    public virtual void Attack (BaseCharacter characterTarget) {
        //Debug.Log(gameObject.name + ": Attack(" + characterTarget.name + ")");
        if (characterTarget == null) {
            //Debug.Log("You must have a target to attack");
            //CombatLogUI.MyInstance.WriteCombatMessage("You must have a target to attack");
        } else {
            // add this here to prevent characters from not being able to attack
            swingTarget = characterTarget;

            // perform a faction/liveness check and disable auto-attack if it is not valid
            if (!AutoAttackTargetIsValid(characterTarget)) {
                DeActivateAutoAttack();
                return;
            }
            if (!inCombat) {
                EnterCombat(characterTarget);
            }
            ActivateAutoAttack();
            if (CanPerformAutoAttack(characterTarget)) {
                // block further auto-attacks while this one is outstanding
                //Debug.Log(gameObject.name + ": Attack(" + characterTarget.name + ") canperformattack: setwaitingforautoattack");
                SetWaitingForAutoAttack(true);

                // Perform the attack. OnAttack should have been populated by the animator to begin an attack animation and send us an AttackHitEvent to respond to
                OnAttack(characterTarget);

                lastCombatEvent = Time.time;
            }
        }
    }

    /// <summary>
    /// receive the AttackHitEvent from the attack animation so damage can be triggered against the enemy
    /// </summary>
    public void AttackHitEvent() {
        AttackHit_AnimationEvent();
    }

    /// <summary>
    /// After the attack animation reaches the point where it contacts the enemy, do damage to it
    /// </summary>
    public virtual bool AttackHit_AnimationEvent() {
        //Debug.Log(gameObject.name + ".CharacterCombat.AttackHit_AnimationEvent()");
        //bool hitSucceeded = false;
        // The character could die mid swing before the attack event fires.  We can't let a dead character do damage
        if (!baseCharacter.MyCharacterStats.IsAlive) {
            //Debug.Log(gameObject.name + ".CharacterCombat.AttackHit_AnimationEvent() Character is not alive!");
            return false;
        }
        CharacterUnit targetCharacterUnit = null;
        //stats.TakeDamage(myStats.damage.GetValue());
        if (MyBaseCharacter.MyCharacterController.MyTarget != null) {
            targetCharacterUnit = MyBaseCharacter.MyCharacterController.MyTarget.GetComponent<CharacterUnit>();
        }

        if (MyBaseCharacter.MyCharacterController.MyTarget != null && targetCharacterUnit != null) {

            // OnHitEvent is responsible for performing ability effects for animated abilities, and needs to fire no matter what because those effects may not require targets
            OnHitEvent(baseCharacter as BaseCharacter, MyBaseCharacter.MyCharacterController.MyTarget);

            // we can now continue because everything beyond this point is single target oriented and it's ok if we cancel attacking due to lack of alive/unfriendly target
            // check for friendly target in case it somehow turned friendly mid swing
            BaseCharacter targetCharacter = targetCharacterUnit.MyBaseCharacter;
            if (targetCharacter != null && !AutoAttackTargetIsValid(targetCharacter)) {
                DeActivateAutoAttack();
                return false;
            }
            //Debug.Log(gameObject.name + ".CharacterCombat.AttackHit_AnimationEvent() OpponentCombat is not null. About to deal damage");
            targetCharacterUnit.MyCharacter.MyCharacterCombat.TakeDamage(baseCharacter.MyCharacterStats.MyMeleeDamage, baseCharacter.MyCharacterUnit.transform.position, baseCharacter as BaseCharacter, CombatType.normal, CombatMagnitude.normal, "Attack");

            if (autoAttackActive == false) {
                ActivateAutoAttack();
            }

            // onHitAbility is only for weapons, not for special moves
            // OnHitAbility will not fire if target is dead. This is ok because regular weapon onhit ability should be set to something that requires a target anyway
            if (onHitAbility != null) {
                //Debug.Log(gameObject.name + ".CharacterCombat.AttackHit_AnimationEvent() onHitAbility is not null!");
                baseCharacter.MyCharacterAbilityManager.BeginAbility(onHitAbility);
            }
            return true;
        } else {
            if (baseCharacter != null && baseCharacter.MyCharacterUnit != null && baseCharacter.MyCharacterUnit.MyCharacterAnimator != null && baseCharacter.MyCharacterUnit.MyCharacterAnimator.MyCurrentAbility != null) {
                if (baseCharacter.MyCharacterUnit.MyCharacterAnimator.MyCurrentAbility.MyRequiresTarget == false) {
                    OnHitEvent(baseCharacter as BaseCharacter, MyBaseCharacter.MyCharacterController.MyTarget);
                    return true;
                }
            }
        }
        return false;
    }

    public void ResetAttackCoolDown() {
        attackCooldown = attackSpeed;
    }

    private void TakeDamageCommon(int damage, BaseCharacter source, CombatType combatType, CombatMagnitude combatMagnitude, string abilityName) {

        damage = (int)(damage * MyBaseCharacter.MyCharacterStats.GetDamageModifiers());

        ProcessTakeDamage(damage, source, combatType, combatMagnitude, abilityName);
        //Debug.Log(gameObject.name + " sending " + damage.ToString() + " to character stats");
        baseCharacter.MyCharacterStats.ReduceHealth(damage);
    }

    public virtual bool TakeDamage(int damage, Vector3 sourcePosition, BaseCharacter source, CombatType combatType, CombatMagnitude combatMagnitude, string abilityName) {
        //Debug.Log(gameObject.name + ".TakeDamage(" + damage + ", " + sourcePosition + ", " + source.name + ")");
        if (baseCharacter.MyCharacterStats.IsAlive) {
            //Debug.Log(gameObject.name + " about to take " + damage.ToString() + " damage. Character is alive");
            //float distance = Vector3.Distance(transform.position, sourcePosition);
            // replace with hitbox check
            bool canPerformAbility = true;
            if (combatType == CombatType.normal) {
                if (!source.MyCharacterController.IsTargetInHitBox(baseCharacter.MyCharacterUnit.gameObject)) {
                    canPerformAbility = false;
                }
                damage -= baseCharacter.MyCharacterStats.MyArmor;
                damage = Mathf.Clamp(damage, 0, int.MaxValue);

            }
            if (canPerformAbility) {
                TakeDamageCommon(damage, source, combatType, combatMagnitude, abilityName);
                return true;
            }
        } else {
            //Debug.Log("Something is trying to damage our dead character!!!");
        }
        return false;
    }

    public virtual void OnKillConfirmed(BaseCharacter sourceCharacter, float creditPercent) {
        //Debug.Log(gameObject.name + " received death broadcast from " + sourceCharacter.name);
        if (sourceCharacter != null) {
            OnKillEvent(sourceCharacter, creditPercent);
        }
        aggroTable.ClearSingleTarget(sourceCharacter.MyCharacterUnit);
        TryToDropCombat();
    }

    public virtual void BroadcastCharacterDeath() {
        if (!baseCharacter.MyCharacterStats.IsAlive) {
            // putting this here because it can be overwritten easier than the event handler that calls it
            //Debug.Log(gameObject.name + " broadcasting death to aggro table");
            foreach (AggroNode _aggroNode in MyAggroTable.MyAggroNodes) {
                if (_aggroNode.aggroTarget == null) {
                    //Debug.Log(gameObject.name + ": aggronode.aggrotarget is null!");
                } else {
                    CharacterCombat _otherCharacterCombat = _aggroNode.aggroTarget.GetComponent<CharacterUnit>().MyCharacter.MyCharacterCombat as CharacterCombat;
                    if (_otherCharacterCombat != null) {
                        _otherCharacterCombat.OnKillConfirmed(baseCharacter as BaseCharacter, (_aggroNode.aggroValue > 0) ? 1 : 0);
                    } else {
                        //Debug.Log(gameObject.name + ": aggronode.aggrotarget(" + _aggroNode.aggroTarget.name + ") had no character combat!");
                    }
                }
            }
            aggroTable.ClearTable();
            DropCombat();
        }
    }

    public void AttemptRegen(int MaxAmount, int currentAmount) {
        //Debug.Log(gameObject.name + ".CharacterCombat.AttemptRegen(); startHasRun: " + startHasRun);
        if (currentAmount != MaxAmount && regenRoutine == null) {
            //Debug.Log(gameObject.name + ".CharacterCombat.AttemptRegen(" + MaxAmount + ", " + currentAmount + "); regenRoutine == null");
            AttemptRegen();
        }
    }


    public void AttemptRegen() {
        //Debug.Log(gameObject.name + ".CharacterCombat.AttemptRegen(); startHasRun: " + startHasRun);
        if (regenRoutine == null && GetInCombat() == false && isActiveAndEnabled == true && startHasRun == true) {
            //Debug.Log(gameObject.name + ".CharacterCombat.AttemptRegen(): starting coroutine");
            regenRoutine = StartCoroutine(outOfCombatRegen());
        }
    }

    public void AttemptStopRegen() {
        //Debug.Log(gameObject.name + ".CharacterCombat.AttemptStopRegen()");
        if (regenRoutine != null) {
            //Debug.Log(gameObject.name + ".CharacterCombat.AttemptStopRegen(): regen routine was not null, stopping now");
            StopCoroutine(regenRoutine);
            regenRoutine = null;
        }
    }

}
