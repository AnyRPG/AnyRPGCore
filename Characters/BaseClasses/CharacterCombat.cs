using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [RequireComponent(typeof(CharacterStats))]
    public class CharacterCombat : MonoBehaviour {

        //events
        public virtual event System.Action<BaseCharacter, float> OnKillEvent = delegate { };
        public virtual event System.Action OnDropCombat = delegate { };
        public virtual event System.Action OnEnterCombat = delegate { };
        public virtual event System.Action<BaseCharacter, GameObject> OnHitEvent = delegate { };

        protected bool eventSubscriptionsInitialized = false;

        /// <summary>
        /// Attack speed is based on animation speed.  This is a limiter in case animations are too fast.
        /// </summary>
        public float attackSpeed = 1f;

        public bool autoAttack = true;

        private bool autoAttackActive = false;

        public float combatCooldown = 10f;
        protected float lastCombatEvent;

        //public bool isAttacking { get; private set; }
        protected bool inCombat = false;

        protected float attackCooldown = 0f;

        // components
        protected BaseCharacter baseCharacter;
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

        public BaseCharacter MyBaseCharacter { get => baseCharacter; set => baseCharacter = value; }
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
        public bool MyAutoAttackActive { get => autoAttackActive; set => autoAttackActive = value; }

        public void OrchestratorStart() {
            //Debug.Log(gameObject.name + ".CharacterCombat.OrchestratorStart()");
            GetComponentReferences();
            aggroTable = new AggroTable(baseCharacter as BaseCharacter);
            CreateEventSubscriptions();
        }

        public virtual void GetComponentReferences() {
            //Debug.Log(gameObject.name + ".CharacterCombat.GetComponentReferences()");
            baseCharacter = GetComponent<BaseCharacter>();
        }

        protected virtual void CreateEventSubscriptions() {
            if (eventSubscriptionsInitialized) {
                return;
            }
            if (baseCharacter != null && baseCharacter.MyCharacterStats != null) {
                baseCharacter.MyCharacterStats.OnDie += OnDieHandler;
            }
            if (baseCharacter != null && baseCharacter.MyCharacterEquipmentManager != null) {
                //Debug.Log(gameObject.name + ".CharacterCombat.CreateEventSubscriptions(): subscribing to onequipmentchanged");
                baseCharacter.MyCharacterEquipmentManager.OnEquipmentChanged += HandleEquipmentChanged;
            }
            SystemEventManager.MyInstance.OnLevelUnload += HandleLevelUnload;
            eventSubscriptionsInitialized = true;
        }

        protected virtual void CleanupEventSubscriptions() {
            //Debug.Log("PlayerCombat.CleanupEventSubscriptions()");
            if (baseCharacter != null && baseCharacter.MyCharacterStats != null) {
                baseCharacter.MyCharacterStats.OnDie -= OnDieHandler;
            }
            if (baseCharacter != null && baseCharacter.MyCharacterEquipmentManager != null) {
                baseCharacter.MyCharacterEquipmentManager.OnEquipmentChanged -= HandleEquipmentChanged;
            }
            if (SystemEventManager.MyInstance != null) {
                SystemEventManager.MyInstance.OnLevelUnload -= HandleLevelUnload;
            }
        }

        public virtual void OnEnable() {
            //CreateEventSubscriptions();
        }

        public virtual void OnDestroy() {
            CleanupEventSubscriptions();
        }

        public virtual void OnDisable() {
            //Debug.Log(gameObject.name + ".CharacterCombat.OnDisable()");
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
            if (attackCooldown > 0f) {
                //Debug.Log(gameObject.name + ".CharacterCombat.Update(): attackCooldown: " + attackCooldown);
                attackCooldown -= Time.deltaTime;
            }

            if (inCombat && baseCharacter.MyCharacterController.MyTarget == null) {
                TryToDropCombat();
            }
        }

        public IEnumerator outOfCombatRegen() {
            //Debug.Log(gameObject.name + ".CharacterCombat.outOfCombatRegen() beginning");
            if (baseCharacter != null && baseCharacter.MyCharacterStats != null) {
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

        public virtual void ProcessTakeDamage(int damage, BaseCharacter target, CombatMagnitude combatMagnitude, AbilityEffect abilityEffect) {
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
                    CombatTextType combatTextType = CombatTextType.normal;
                    if ((abilityEffect as AttackEffect).MyDamageType == DamageType.physical) {
                        combatTextType = CombatTextType.normal;
                    } else if ((abilityEffect as AttackEffect).MyDamageType == DamageType.ability) {
                        combatTextType = CombatTextType.ability;
                    }
                    CombatTextManager.MyInstance.SpawnCombatText(baseCharacter.MyCharacterUnit.gameObject, damage, combatTextType, combatMagnitude);
                    SystemEventManager.MyInstance.NotifyOnTakeDamage(target, MyBaseCharacter.MyCharacterUnit, damage, abilityEffect.MyName);
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
            if (baseCharacter != null && baseCharacter.MyCharacterUnit != null && baseCharacter.MyAnimatedUnit.MyCharacterAnimator != null) {
                baseCharacter.MyAnimatedUnit.MyCharacterAnimator.SetBool("InCombat", false);
            }
            DeActivateAutoAttack();
            //Debug.Log(gameObject.name + ".CharacterCombat.DropCombat(): dropped combat.");
            OnDropCombat();
        }

        public void ActivateAutoAttack() {
            //Debug.Log(gameObject.name + ".CharacterCombat.ActivateAutoAttack()");
            autoAttackActive = true;
        }

        public void DeActivateAutoAttack() {
            //Debug.Log(gameObject.name + ".CharacterCombat.DeActivateAutoAttack()");
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
            baseCharacter.MyAnimatedUnit.MyCharacterAnimator.SetBool("InCombat", true);
            inCombat = true;
            OnEnterCombat();
            if (aggroTable.AddToAggroTable(target.MyCharacterUnit, 0)) {
                return true;
            }
            return false;
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


                // onHitAbility is only for weapons, not for special moves

                AbilityEffectOutput abilityAffectInput = new AbilityEffectOutput();
                foreach (StatusEffectNode statusEffectNode in MyBaseCharacter.MyCharacterStats.MyStatusEffects.Values) {
                    //Debug.Log(gameObject.name + ".CharacterCombat.AttackHit_AnimationEvent(): Casting OnHit Ability On Take Damage");
                    // this could maybe be done better through an event subscription
                    if (statusEffectNode.MyStatusEffect.MyWeaponHitAbilityEffectList.Count > 0) {
                        statusEffectNode.MyStatusEffect.CastWeaponHit(MyBaseCharacter as BaseCharacter, targetCharacterUnit.gameObject, abilityAffectInput);
                    }
                }


                // OnHitAbility will not fire if target is dead. This is ok because regular weapon onhit ability should be set to something that requires a target anyway
                if (onHitAbility != null) {
                    //Debug.Log(gameObject.name + ".CharacterCombat.AttackHit_AnimationEvent() onHitAbility is not null!");
                    baseCharacter.MyCharacterAbilityManager.BeginAbility(onHitAbility);
                } else {
                    //Debug.Log(gameObject.name + ".CharacterCombat.AttackHit_AnimationEvent() onHitAbility is null!!!");
                }
                return true;
            } else {
                if (baseCharacter != null && baseCharacter.MyCharacterUnit != null && baseCharacter.MyAnimatedUnit.MyCharacterAnimator != null && baseCharacter.MyAnimatedUnit.MyCharacterAnimator.MyCurrentAbility != null) {
                    if (baseCharacter.MyAnimatedUnit.MyCharacterAnimator.MyCurrentAbility.MyRequiresTarget == false) {
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

        private void TakeDamageCommon(int damage, BaseCharacter source, CombatMagnitude combatMagnitude, AbilityEffect abilityEffect) {

            damage = (int)(damage * MyBaseCharacter.MyCharacterStats.GetIncomingDamageModifiers());

            ProcessTakeDamage(damage, source, combatMagnitude, abilityEffect);
            //Debug.Log(gameObject.name + " sending " + damage.ToString() + " to character stats");
            baseCharacter.MyCharacterStats.ReduceHealth(damage);
        }

        public virtual bool TakeDamage(int damage, Vector3 sourcePosition, BaseCharacter sourceCharacter, CombatMagnitude combatMagnitude, AbilityEffect abilityEffect) {
            //Debug.Log(gameObject.name + ".TakeDamage(" + damage + ", " + sourcePosition + ", " + source.name + ")");
            if (baseCharacter.MyCharacterStats.IsAlive) {
                //Debug.Log(gameObject.name + " about to take " + damage.ToString() + " damage. Character is alive");
                //float distance = Vector3.Distance(transform.position, sourcePosition);
                // replace with hitbox check
                bool canPerformAbility = true;
                if ((abilityEffect as AttackEffect).MyDamageType == DamageType.physical) {
                    damage -= baseCharacter.MyCharacterStats.MyArmor;
                    damage = Mathf.Clamp(damage, 0, int.MaxValue);
                }
                if (abilityEffect.MyUseMeleeRange) {
                    if (!sourceCharacter.MyCharacterController.IsTargetInHitBox(baseCharacter.MyCharacterUnit.gameObject)) {
                        canPerformAbility = false;
                    }
                }

                // CHARACTER WILL HAVE RANGED ABILITIES HIT WHEN RUNNING AWAY OUT OF RANGE AS LONG AS IT WAS IN RANGE WHEN ABILITY WAS CAST
                // MAY WANT TO CHANGE THIS IF DODGE MECHANICS ARE A IMPORTANT PART OF GAMEPLAY

                if (canPerformAbility) {
                    TakeDamageCommon(damage, sourceCharacter, combatMagnitude, abilityEffect);
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
            //Debug.Log(gameObject.name + ".CharacterCombat.AttemptRegen()");
            if (currentAmount != MaxAmount && regenRoutine == null) {
                //Debug.Log(gameObject.name + ".CharacterCombat.AttemptRegen(" + MaxAmount + ", " + currentAmount + "); regenRoutine == null");
                AttemptRegen();
            }
        }


        public void AttemptRegen() {
            //Debug.Log(gameObject.name + ".CharacterCombat.AttemptRegen()");
            if (regenRoutine == null && GetInCombat() == false && isActiveAndEnabled == true) {
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

        public virtual void HandleEquipmentChanged(Equipment newItem, Equipment oldItem) {
            //Debug.Log(gameObject.name + ".CharacterCombat.HandleEquipmentChanged(" + (newItem == null ? "null" : newItem.MyName) + ", " + (oldItem == null ? "null" : oldItem.MyName) + ")");
            if (oldItem != null) {
                if (oldItem.equipSlot == EquipmentSlot.MainHand) {
                    onHitAbility = null;
                }
            }
            if (newItem != null) {
                if (newItem.equipSlot == EquipmentSlot.MainHand) {
                    onHitAbility = null;
                    //Debug.Log(gameObject.name + ".CharacterCombat.HandleEquipmentChanged(): item is a weapon");
                    //overrideHitSoundEffect = null;
                    //defaultHitSoundEffect = null;
                    if (newItem is Weapon && (newItem as Weapon).OnHitAbility != null) {
                        //Debug.Log(gameObject.name + ".CharacterCombat.HandleEquipmentChanged(): New item is a weapon and has the on hit ability " + (newItem as Weapon).OnHitAbility.name);
                        onHitAbility = (newItem as Weapon).OnHitAbility;
                    }
                    overrideHitSoundEffect = null;
                    defaultHitSoundEffect = null;
                    if (newItem is Weapon && (newItem as Weapon).MyDefaultHitSoundEffect != null) {
                        //Debug.Log("New item is a weapon and has the on hit ability " + (newItem as Weapon).OnHitAbility.name);
                        overrideHitSoundEffect = (newItem as Weapon).MyDefaultHitSoundEffect;
                        defaultHitSoundEffect = (newItem as Weapon).MyDefaultHitSoundEffect;
                    }
                }
            }
            AttemptRegen();
        }

    }

}