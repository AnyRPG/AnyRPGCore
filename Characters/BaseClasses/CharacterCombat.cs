using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class CharacterCombat {

        //events
        public event System.Action<BaseCharacter, float> OnKillEvent = delegate { };
        public event System.Action OnDropCombat = delegate { };
        public event System.Action<Interactable> OnEnterCombat = delegate { };
        public event System.Action<BaseCharacter, Interactable> OnHitEvent = delegate { };
        public event System.Action OnUpdate = delegate { };
        public event System.Action<Interactable, AbilityEffectContext> OnReceiveCombatMiss = delegate { };

        protected bool eventSubscriptionsInitialized = false;

        private bool autoAttackActive = false;

        //[Tooltip("The amount of seconds after the last combat event to wait before dropping combat")]
        protected float combatCooldown = 10f;

        protected float lastCombatEvent;

        //public bool isAttacking { get; private set; }
        protected bool inCombat = false;

        // components
        protected BaseCharacter baseCharacter;

        // list of on hit effects to cast on weapon hit from currently equipped weapons
        protected List<AbilityEffect> onHitEffects = null;

        protected AggroTable aggroTable = null;

        // this is what the current weapon defaults to
        protected List<AudioClip> defaultHitSoundEffects = new List<AudioClip>();

        /// <summary>
        ///  waiting for the animator to let us know we can hit again
        /// </summary>
        private bool waitingForAutoAttack = false;

        // the target we swung at, in case we try to change target mid swing and we don't put an animation on something too far away
        protected BaseCharacter swingTarget = null;

        public AggroTable AggroTable {
            get {
                return aggroTable;
            }
        }

        public BaseCharacter BaseCharacter { get => baseCharacter; set => baseCharacter = value; }
        public bool WaitingForAutoAttack {
            get => waitingForAutoAttack;
            set {
                //Debug.Log(gameObject.name + ".CharacterCombat.MyWaitingForHits Setting waitingforHits to: " + value);
                waitingForAutoAttack = value;
            }
        }

        public List<AudioClip> DefaultHitSoundEffects { get => defaultHitSoundEffects; set => defaultHitSoundEffects = value; }
        public BaseCharacter SwingTarget { get => swingTarget; set => swingTarget = value; }
        public bool AutoAttackActive { get => autoAttackActive; set => autoAttackActive = value; }
        public List<AbilityEffect> OnHitEffects { get => onHitEffects; set => onHitEffects = value; }

        public CharacterCombat(BaseCharacter baseCharacter) {
            this.baseCharacter = baseCharacter;
            aggroTable = new AggroTable(baseCharacter as BaseCharacter);
        }

        public void HandleAutoAttack() {
            //Debug.Log(gameObject.name + ".PlayerCombat.HandleAutoAttack()");
            if (baseCharacter.UnitController == null) {
                // can't attack without a character
                return;
            }

            if (baseCharacter.UnitController.Target == null && AutoAttackActive == true) {
                //Debug.Log(gameObject.name + ".PlayerCombat.HandleAutoAttack(): target is null.  deactivate autoattack");
                DeActivateAutoAttack();
                return;
            }
            if (baseCharacter.CharacterAbilityManager.WaitingForAnimatedAbility == true || baseCharacter.CharacterCombat.WaitingForAutoAttack == true || baseCharacter.CharacterAbilityManager.IsCasting) {
                // can't auto-attack during auto-attack, animated attack, or cast
                return;
            }


            if (AutoAttackActive == true && baseCharacter.UnitController.Target != null) {
                //Debug.Log("player controller is in combat and target is not null");
                //Interactable _interactable = controller.MyTarget.GetComponent<Interactable>();
                CharacterUnit _characterUnit = CharacterUnit.GetCharacterUnit(baseCharacter.UnitController.Target);
                if (_characterUnit != null) {
                    BaseCharacter targetCharacter = _characterUnit.BaseCharacter;
                    if (targetCharacter != null) {
                        //Debug.Log(gameObject.name + ".PlayerCombat.HandleAutoAttack(). targetCharacter is not null.  Attacking");
                        Attack(targetCharacter);
                        return;
                    } else {
                        //Debug.Log(gameObject.name + ".PlayerCombat.HandleAutoAttack(). targetCharacter is null. deactivating auto attack");
                    }
                }
                // autoattack is active, but we were unable to attack the target because they were dead, or not a lootable character, or didn't have an interactable.
                // There is no reason for autoattack to remain active under these circumstances
                //Debug.Log(gameObject.name + ": target is not attackable.  deactivate autoattack");
                DeActivateAutoAttack();
            }
        }

        public void ProcessLevelUnload() {
            SetWaitingForAutoAttack(false);
            DropCombat();
        }

        public void Update() {
            if (baseCharacter == null) {
                return;
            }
            if (!baseCharacter.CharacterStats.IsAlive) {
                return;
            }

            if (inCombat && baseCharacter.UnitController.Target == null) {
                TryToDropCombat();
            }

            // leave combat if the combat cooldown has expired
            if ((Time.time - lastCombatEvent > combatCooldown) && inCombat) {
                //Debug.Log(gameObject.name + " Leaving Combat");
                TryToDropCombat();
            }

            OnUpdate();
        }

        public bool WaitingForAction() {
            if (baseCharacter.CharacterAbilityManager.WaitingForAnimatedAbility == true || baseCharacter.CharacterCombat.WaitingForAutoAttack == true || baseCharacter.CharacterAbilityManager.IsCasting) {
                // can't auto-attack during auto-attack, animated attack, or cast
                return true;
            }
            return false;
        }

        public void HandleDie() {
            //Debug.Log(gameObject.name + ".OnDieHandler()");
            SetWaitingForAutoAttack(false);

            BroadcastCharacterDeath();

            if (baseCharacter.UnitController.UnitControllerMode == UnitControllerMode.AI && !(baseCharacter.UnitController.CurrentState is DeathState)) {
                (baseCharacter.UnitController as UnitController).ChangeState(new DeathState());
            }

        }

        public void SetWaitingForAutoAttack(bool newValue) {
            //Debug.Log(gameObject.name + ".CharacterCombat.SetWaitingForAutoAttack(" + newValue + ")");
            WaitingForAutoAttack = newValue;
        }

        /// <summary>
        /// This is the entrypoint to a manual attack.
        /// </summary>
        /// <param name="characterTarget"></param>
        public void Attack(BaseCharacter characterTarget) {
            //Debug.Log(gameObject.name + ".PlayerCombat.Attack(" + characterTarget.name + ")");
            if (characterTarget == null) {
                //Debug.Log("You must have a target to attack");
                //CombatLogUI.MyInstance.WriteCombatMessage("You must have a target to attack");
            } else {
                // add this here to prevent characters from not being able to attack
                swingTarget = characterTarget;

                // Perform the attack. OnAttack should have been populated by the animator to begin an attack animation and send us an AttackHitEvent to respond to
                if (WaitingForAction() == false) {
                    baseCharacter.CharacterAbilityManager.AttemptAutoAttack();
                }
            }
        }

        public void ProcessTakeDamage(AbilityEffectContext abilityEffectContext, PowerResource powerResource, int damage, IAbilityCaster target, CombatMagnitude combatMagnitude, AbilityEffect abilityEffect) {
            //Debug.Log(gameObject.name + ".CharacterCombat.ProcessTakeDamage(" + damage + ", " + (target == null ? "null" : target.Name) + ", " + combatMagnitude.ToString() + ", " + abilityEffect.MyName);

            if (abilityEffectContext == null) {
                abilityEffectContext = new AbilityEffectContext();
            }
            abilityEffectContext.powerResource = powerResource;
            abilityEffectContext.SetResourceAmount(powerResource.DisplayName, damage);

            // prevent infinite reflect loops
            if (abilityEffectContext.reflectDamage == false) {
                foreach (StatusEffectNode statusEffectNode in BaseCharacter.CharacterStats.StatusEffects.Values) {
                    //Debug.Log("Casting Reflection On Take Damage");
                    // this could maybe be done better through an event subscription
                    if (statusEffectNode.StatusEffect.ReflectAbilityEffectList.Count > 0) {
                        // we can't reflect on system attackers, so check if this is an interactable
                        Interactable targetInteractable = target.gameObject.GetComponent<Interactable>();
                        if (targetInteractable != null) {
                            statusEffectNode.StatusEffect.CastReflect(BaseCharacter, targetInteractable, abilityEffectContext);
                        }
                    }
                }
            }

            if (target != null
                && PlayerManager.MyInstance != null
                && PlayerManager.MyInstance.ActiveUnitController != null
                && baseCharacter != null
                && baseCharacter.UnitController != null
                && baseCharacter.UnitController.CharacterUnit != null) {
                if (target == (PlayerManager.MyInstance.MyCharacter as IAbilityCaster) ||
                    (PlayerManager.MyInstance.MyCharacter as BaseCharacter) == (baseCharacter as BaseCharacter) ||
                    target.AbilityManager.IsPlayerControlled()) {
                    // spawn text over enemies damaged by the player and over the player itself
                    CombatTextType combatTextType = CombatTextType.normal;
                    if ((abilityEffect as AttackEffect).DamageType == DamageType.physical) {
                        combatTextType = CombatTextType.normal;
                    } else if ((abilityEffect as AttackEffect).DamageType == DamageType.ability) {
                        combatTextType = CombatTextType.ability;
                    }
                    CombatTextManager.MyInstance.SpawnCombatText(baseCharacter.UnitController, damage, combatTextType, combatMagnitude, abilityEffectContext);
                    SystemEventManager.MyInstance.NotifyOnTakeDamage(target, BaseCharacter.UnitController.CharacterUnit, damage, abilityEffect.DisplayName);
                }
                lastCombatEvent = Time.time;
                float totalThreat = damage;
                totalThreat *= abilityEffect.ThreatMultiplier * target.AbilityManager.GetThreatModifiers();

                // determine if this target is capable of fighting (ie, not environmental effect), and if so, enter combat
                Interactable _interactable = target.gameObject.GetComponent<Interactable>();
                if (_interactable != null) {
                    CharacterUnit _characterUnit = CharacterUnit.GetCharacterUnit(_interactable);
                    if (_characterUnit != null) {
                        AggroTable.AddToAggroTable(_characterUnit, (int)totalThreat);
                        EnterCombat(_interactable);
                    }
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
                if (BaseCharacter.UnitController.CharacterUnit != null) {
                    foreach (AggroNode aggroNode in AggroTable.MyAggroNodes) {
                        UnitController _aiController = aggroNode.aggroTarget.BaseCharacter.UnitController as UnitController;
                        // since players don't have an agro radius, we can skip the check and drop combat automatically
                        if (_aiController != null) {
                            if (Vector3.Distance(BaseCharacter.UnitController.transform.position, aggroNode.aggroTarget.Interactable.transform.position) < _aiController.AggroRadius) {
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
            if (baseCharacter != null && baseCharacter.CharacterAbilityManager != null) {
                baseCharacter.CharacterAbilityManager.WaitingForAnimatedAbility = false;
            }
            if (baseCharacter != null && baseCharacter.UnitController != null && baseCharacter.UnitController.UnitAnimator != null) {
                baseCharacter.UnitController.UnitAnimator.SetBool("InCombat", false);
            }
            DeActivateAutoAttack();
            //Debug.Log(gameObject.name + ".CharacterCombat.DropCombat(): dropped combat.");
            baseCharacter.CharacterEquipmentManager.SheathWeapons();
            OnDropCombat();
        }

        public void ActivateAutoAttack() {
            //Debug.Log(gameObject.name + ".CharacterCombat.ActivateAutoAttack()");
            if (SystemConfigurationManager.MyInstance.MyAllowAutoAttack == true) {
                autoAttackActive = true;
            }
        }

        public void DeActivateAutoAttack() {
            //Debug.Log(gameObject.name + ".CharacterCombat.DeActivateAutoAttack()");
            autoAttackActive = false;
        }

        public bool GetInCombat() {
            return inCombat;
        }

        public bool EnterCombat(IAbilityCaster target) {
            Interactable _interactable = target.gameObject.GetComponent<Interactable>();
            if (_interactable != null) {
                CharacterUnit _characterUnit = CharacterUnit.GetCharacterUnit(_interactable);
                if (_characterUnit != null) {
                    return EnterCombat(_interactable);
                }
            }
            return false;
        }

        /// <summary>
        /// Adds the target to the aggro table with 0 agro to ensure you have something to send a drop combat message to if you did 0 damage to them during combat.
        /// Set inCombat to true.
        /// </summary>
        /// <param name="target"></param>
        /// return true if this is a new entry, false if not
        public virtual bool EnterCombat(Interactable target) {
            //Debug.Log(gameObject.name + ".CharacterCombat.EnterCombat(" + (target != null && target.MyName != null ? target.MyName : "null") + ")");
            CharacterUnit _characterUnit = CharacterUnit.GetCharacterUnit(target);
            if (_characterUnit == null || _characterUnit.BaseCharacter.CharacterStats.IsAlive == false || BaseCharacter.CharacterStats.IsAlive == false) {
                //Debug.Log(gameObject.name + ".CharacterCombat.EnterCombat(" + (target != null && target.MyName != null ? target.MyName : "null") + "): character is not alive, returning!");
                return false;
            }

            // If we do not have a focus, set the target as the focus
            if (baseCharacter.UnitController != null && baseCharacter.UnitController.Target == null) {
                baseCharacter.UnitController.SetTarget(target);
            }

            lastCombatEvent = Time.time;
            // maybe do this in update?
            if (baseCharacter != null && baseCharacter.UnitController != null && baseCharacter.UnitController.UnitAnimator != null) {
                baseCharacter.UnitController.UnitAnimator.SetBool("InCombat", true);
            }
            inCombat = true;
            if (!aggroTable.AggroTableContains(CharacterUnit.GetCharacterUnit(target))) {
                OnEnterCombat(target);
            }
            baseCharacter.CharacterEquipmentManager.HoldWeapons();
            return aggroTable.AddToAggroTable(CharacterUnit.GetCharacterUnit(target) , 0);
        }

        public BaseAbility GetValidAttackAbility() {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterCombat.GetValidAttackAbility()");

            List<BaseAbility> returnList = new List<BaseAbility>();

            if (BaseCharacter != null && BaseCharacter.CharacterAbilityManager != null) {
                //Debug.Log(gameObject.name + ".AICombat.GetValidAttackAbility(): CHARACTER HAS ABILITY MANAGER");

                foreach (BaseAbility baseAbility in BaseCharacter.CharacterAbilityManager.AbilityList.Values) {
                    //Debug.Log(baseCharacter.gameObject.name + ".AICombat.GetValidAttackAbility(): Checking ability: " + baseAbility.DisplayName);
                    if (baseAbility.CanCastOnEnemy &&
                        BaseCharacter.CharacterAbilityManager.CanCastAbility(baseAbility) &&
                        baseAbility.CanUseOn(BaseCharacter.UnitController.Target, BaseCharacter) &&
                        baseCharacter.CharacterAbilityManager.PerformLOSCheck(baseCharacter.UnitController.Target, baseAbility)) {
                        //Debug.Log(baseCharacter.gameObject.name + ".AICombat.GetValidAttackAbility(): ADDING AN ABILITY TO LIST");
                        returnList.Add(baseAbility);
                    }
                }
            }
            if (returnList.Count > 0) {
                int randomIndex = Random.Range(0, returnList.Count);
                //Debug.Log(baseCharacter.gameObject.name + ".AICombat.GetValidAttackAbility(): returnList.Count: " + returnList.Count + "; randomIndex: " + randomIndex);
                return returnList[randomIndex];
            }
            //Debug.Log(baseCharacter.gameObject.name + ".AICombat.GetValidAttackAbility(): ABOUT TO RETURN NULL!");
            return null;
        }

        public BaseAbility GetMeleeAbility() {
            //Debug.Log(gameObject.name + ".AICombat.GetValidAttackAbility()");

            if (BaseCharacter != null && BaseCharacter.CharacterAbilityManager != null) {
                //Debug.Log(gameObject.name + ".AICombat.GetValidAttackAbility(): CHARACTER HAS ABILITY MANAGER");

                foreach (BaseAbility baseAbility in BaseCharacter.CharacterAbilityManager.AbilityList.Values) {
                    //Debug.Log(gameObject.name + ".AICombat.GetValidAttackAbility(): Checking ability: " + baseAbility.MyName);
                    //if (baseAbility.maxRange == 0 || Vector3.Distance(aiController.MyBaseCharacter.MyCharacterUnit.transform.position, aiController.MyTarget.transform.position) < baseAbility.maxRange) {
                    if (baseAbility.CanCastOnEnemy && baseAbility.UseMeleeRange == true) {
                        //Debug.Log(gameObject.name + ".AICombat.GetValidAttackAbility(): ADDING AN ABILITY TO LIST");
                        //if (baseAbility.MyCanCastOnEnemy) {
                        return baseAbility;
                    }
                    //}
                }
            }
            //Debug.Log(gameObject.name + ".AICombat.GetValidAttackAbility(): ABOUT TO RETURN NULL!");
            return null;
        }

        public List<BaseAbility> GetAttackRangeAbilityList() {
            List<BaseAbility> returnList = new List<BaseAbility>();

            if (BaseCharacter != null && BaseCharacter.CharacterAbilityManager != null) {
                //Debug.Log(gameObject.name + ".AICombat.GetValidAttackAbility(): CHARACTER HAS ABILITY MANAGER");

                foreach (BaseAbility baseAbility in BaseCharacter.CharacterAbilityManager.AbilityList.Values) {
                    returnList.Add(baseAbility);
                }
            }
            return returnList;
        }

        public float GetMinAttackRange(List<BaseAbility> baseAbilityList) {
            //Debug.Log(gameObject.name + ".AICombat.GetValidAttackAbility()");

            float returnValue = 0f;

            if (BaseCharacter != null && BaseCharacter.CharacterAbilityManager != null) {
                //Debug.Log(gameObject.name + ".AICombat.GetValidAttackAbility(): CHARACTER HAS ABILITY MANAGER");

                foreach (BaseAbility baseAbility in baseAbilityList) {
                    //Debug.Log(gameObject.name + ".AICombat.GetValidAttackAbility(): Checking ability: " + baseAbility.MyName);
                    if (baseAbility.CanCastOnEnemy && baseAbility.UseMeleeRange == false && baseAbility.MaxRange > 0f) {
                        float returnedMaxRange = baseAbility.GetLOSMaxRange(baseCharacter, baseCharacter.UnitController.Target);
                        if (returnValue == 0f || returnedMaxRange < returnValue) {
                            //Debug.Log(sourceCharacter.AbilityManager.MyName + ".AICombat.GetValidAttackAbility(): ADDING AN ABILITY TO LIST: " + baseAbility.MyName);
                            returnValue = returnedMaxRange;
                        }
                    }
                }
            }
            //Debug.Log(gameObject.name + ".AICombat.GetMinAttackRange(): return " + returnValue);
            return returnValue;
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
            if (!baseCharacter.CharacterStats.IsAlive) {
                //Debug.Log(gameObject.name + ".CharacterCombat.AttackHit_AnimationEvent() Character is not alive!");
                return false;
            }
            CharacterUnit targetCharacterUnit = null;
            //stats.TakeDamage(myStats.damage.GetValue());
            if (BaseCharacter.UnitController.Target != null) {
                targetCharacterUnit = CharacterUnit.GetCharacterUnit(BaseCharacter.UnitController.Target);
            }

            if (BaseCharacter.UnitController.Target != null && targetCharacterUnit != null) {

                // OnHitEvent is responsible for performing ability effects for animated abilities, and needs to fire no matter what because those effects may not require targets
                //OnHitEvent(baseCharacter as BaseCharacter, MyBaseCharacter.MyCharacterController.MyTarget);

                BaseAbility animatorCurrentAbility = null;
                bool attackLanded = true;
                if (BaseCharacter != null && BaseCharacter.UnitController != null && BaseCharacter.UnitController.UnitAnimator != null && BaseCharacter.UnitController.UnitAnimator.MyCurrentAbilityEffectContext != null) {
                    animatorCurrentAbility = BaseCharacter.UnitController.UnitAnimator.MyCurrentAbilityEffectContext.baseAbility;
                    if (animatorCurrentAbility is AnimatedAbility) {
                        attackLanded = (BaseCharacter.UnitController.UnitAnimator.MyCurrentAbilityEffectContext.baseAbility as AnimatedAbility).HandleAbilityHit(
                            BaseCharacter,
                            BaseCharacter.UnitController.Target,
                            BaseCharacter.UnitController.UnitAnimator.MyCurrentAbilityEffectContext);
                    }
                }

                // onHitAbility is only for weapons, not for special moves
                if (!attackLanded) {
                    return false;
                }

                if (animatorCurrentAbility != null) {
                    AudioClip audioClip = animatorCurrentAbility.GetHitSound(baseCharacter);
                    if (audioClip != null) {
                        baseCharacter.UnitController.UnitComponentController.PlayEffect(audioClip);
                    }
                    //AudioManager.MyInstance.PlayEffect(overrideHitSoundEffect);
                }

                AbilityEffectContext abilityAffectInput = new AbilityEffectContext();
                foreach (StatusEffectNode statusEffectNode in BaseCharacter.CharacterStats.StatusEffects.Values) {
                    //Debug.Log(gameObject.name + ".CharacterCombat.AttackHit_AnimationEvent(): Casting OnHit Ability On Take Damage");
                    // this could maybe be done better through an event subscription
                    if (statusEffectNode.StatusEffect.WeaponHitAbilityEffectList.Count > 0) {
                        statusEffectNode.StatusEffect.CastWeaponHit(BaseCharacter, targetCharacterUnit.Interactable, abilityAffectInput);
                    }
                }

                // moved inside attackEffect
                /*
                // OnHitAbility will not fire if target is dead. This is ok because regular weapon onhit ability should be set to something that requires a target anyway
                if (onHitEffect != null) {
                    //Debug.Log(gameObject.name + ".CharacterCombat.AttackHit_AnimationEvent() onHitAbility is not null!");
                    baseCharacter.MyCharacterAbilityManager.BeginAbility(onHitEffect);
                } else {
                    //Debug.Log(gameObject.name + ".CharacterCombat.AttackHit_AnimationEvent() onHitAbility is null!!!");
                }
                */
                return true;
            } else {
                if (baseCharacter != null && baseCharacter.UnitController != null && baseCharacter.UnitController.UnitAnimator != null && baseCharacter.UnitController.UnitAnimator.MyCurrentAbilityEffectContext != null) {
                    if (baseCharacter.UnitController.UnitAnimator.MyCurrentAbilityEffectContext.baseAbility.RequiresTarget == false) {
                        OnHitEvent(baseCharacter as BaseCharacter, BaseCharacter.UnitController.Target);
                        return true;
                    }
                }
            }
            return false;
        }

        public virtual void ReceiveCombatMiss(Interactable targetObject, AbilityEffectContext abilityEffectContext) {
            //Debug.Log(gameObject.name + ".CharacterCombat.ReceiveCombatMiss()");
            OnReceiveCombatMiss(targetObject, abilityEffectContext);
        }

        public bool DidAttackMiss() {
            //Debug.Log(gameObject.name + ".CharacterCombat.DidAttackMiss()");
            int randomNumber = UnityEngine.Random.Range(0, 100);
            int randomCutoff = (int)Mathf.Clamp(baseCharacter.CharacterStats.GetAccuracyModifiers(), 0, 100);
            //Debug.Log(gameObject.name + ".CharacterCombat.DidAttackMiss(): number: " + randomNumber + "; accuracy = " + randomCutoff);
            if (randomNumber >= randomCutoff) {
                return true;
            }
            return false;
        }

        /// <summary>
        /// return true if damage was taken, false if not
        /// </summary>
        /// <param name="abilityEffectContext"></param>
        /// <param name="powerResource"></param>
        /// <param name="damage"></param>
        /// <param name="source"></param>
        /// <param name="combatMagnitude"></param>
        /// <param name="abilityEffect"></param>
        /// <returns></returns>
        private bool TakeDamageCommon(AbilityEffectContext abilityEffectContext, PowerResource powerResource, int damage, IAbilityCaster source, CombatMagnitude combatMagnitude, AbilityEffect abilityEffect) {
            //Debug.Log(gameObject.name + ".TakeDamageCommon(" + damage + ")");

            // perform check to see if this character has the resource to be reduced.  if not, it is immune to this type of damage
            if (baseCharacter.CharacterStats.WasImmuneToDamageType(powerResource, source, abilityEffectContext)) {
                return false;
            }

            damage = (int)(damage * BaseCharacter.CharacterStats.GetIncomingDamageModifiers());

            ProcessTakeDamage(abilityEffectContext, powerResource, damage, source, combatMagnitude, abilityEffect);
            //Debug.Log(gameObject.name + " sending " + damage.ToString() + " to character stats");
            baseCharacter.CharacterStats.ReducePowerResource(powerResource, damage);
            return true;
        }

        public virtual bool TakeDamage(AbilityEffectContext abilityEffectContext, PowerResource powerResource, int damage, IAbilityCaster sourceCharacter, CombatMagnitude combatMagnitude, AbilityEffect abilityEffect) {
            //Debug.Log(gameObject.name + ".TakeDamage(" + damage + ", " + sourcePosition + ", " + source.name + ")");
            if (baseCharacter.UnitController.UnitControllerMode == UnitControllerMode.AI) {
                if (baseCharacter.UnitController.CurrentState is EvadeState || baseCharacter.UnitController.CurrentState is DeathState) {
                    return false;
                }
            }

            if (baseCharacter.CharacterStats.IsAlive) {
                EnterCombat(sourceCharacter);
                //Debug.Log(gameObject.name + " about to take " + damage.ToString() + " damage. Character is alive");
                //float distance = Vector3.Distance(transform.position, sourcePosition);
                // replace with hitbox check
                bool canPerformAbility = true;
                if ((abilityEffect as AttackEffect).DamageType == DamageType.physical) {
                    damage -= (int)baseCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Armor].CurrentValue;
                    damage = Mathf.Clamp(damage, 0, int.MaxValue);
                }
                if (abilityEffect.UseMeleeRange) {
                    if (!sourceCharacter.AbilityManager.IsTargetInMeleeRange(baseCharacter.UnitController)) {
                        canPerformAbility = false;
                    }
                }

                // CHARACTER WILL HAVE RANGED ABILITIES HIT WHEN RUNNING AWAY OUT OF RANGE AS LONG AS IT WAS IN RANGE WHEN ABILITY WAS CAST
                // MAY WANT TO CHANGE THIS IF DODGE MECHANICS ARE A IMPORTANT PART OF GAMEPLAY

                if (canPerformAbility) {
                    return TakeDamageCommon(abilityEffectContext, powerResource, damage, sourceCharacter, combatMagnitude, abilityEffect);
                }
            } else {
                //Debug.Log("Something is trying to damage our dead character!!!");
            }
            return false;
        }

        public virtual void OnKillConfirmed(BaseCharacter sourceCharacter, float creditPercent) {
            //Debug.Log(gameObject.name + " received death broadcast from " + sourceCharacter.AbilityManager.name);
            if (sourceCharacter != null) {
                OnKillEvent(sourceCharacter, creditPercent);
                baseCharacter.CharacterAbilityManager.ReceiveKillDetails(sourceCharacter, creditPercent);
            }
            aggroTable.ClearSingleTarget(sourceCharacter.UnitController.CharacterUnit);
            TryToDropCombat();
        }

        public virtual void BroadcastCharacterDeath() {
            if (!baseCharacter.CharacterStats.IsAlive) {
                // putting this here because it can be overwritten easier than the event handler that calls it
                //Debug.Log(gameObject.name + " broadcasting death to aggro table");
                Dictionary<CharacterCombat, float> broadcastDictionary = new Dictionary<CharacterCombat, float>();
                foreach (AggroNode _aggroNode in AggroTable.MyAggroNodes) {
                    if (_aggroNode.aggroTarget == null) {
                        //Debug.Log(gameObject.name + ": aggronode.aggrotarget is null!");
                    } else {
                        CharacterCombat _otherCharacterCombat = _aggroNode.aggroTarget.BaseCharacter.CharacterCombat as CharacterCombat;
                        if (_otherCharacterCombat != null) {
                            broadcastDictionary.Add(_otherCharacterCombat, (_aggroNode.aggroValue > 0 ? 1 : 0));
                        } else {
                            //Debug.Log(gameObject.name + ": aggronode.aggrotarget(" + _aggroNode.aggroTarget.name + ") had no character combat!");
                        }
                    }
                }
                foreach (CharacterCombat characterCombat in broadcastDictionary.Keys) {
                    characterCombat.OnKillConfirmed(baseCharacter as BaseCharacter, broadcastDictionary[characterCombat]);
                }
                aggroTable.ClearTable();
                DropCombat();
            }
        }

        public virtual void HandleEquipmentChanged(Equipment newItem, Equipment oldItem, int slotIndex) {
            //Debug.Log(gameObject.name + ".CharacterCombat.HandleEquipmentChanged(" + (newItem == null ? "null" : newItem.DisplayName) + ", " + (oldItem == null ? "null" : oldItem.DisplayName) + ")");

            if (oldItem != null && oldItem is Weapon) {
                if ((oldItem as Weapon).OnHitEffectList != null && (oldItem as Weapon).OnHitEffectList.Count > 0) {
                    onHitEffects = null;
                }
                EquipmentSlotProfile equipmentSlotProfile = baseCharacter.CharacterEquipmentManager.FindEquipmentSlotForEquipment(oldItem);
                if (equipmentSlotProfile != null && equipmentSlotProfile.SetOnHitAudio == true) {
                    defaultHitSoundEffects.Clear();
                }
            }

            if (newItem != null) {
                if (newItem is Weapon) {
                    onHitEffects = new List<AbilityEffect>();
                    //Debug.Log(gameObject.name + ".CharacterCombat.HandleEquipmentChanged(): item is a weapon");
                    //overrideHitSoundEffect = null;
                    //defaultHitSoundEffect = null;
                    if ((newItem as Weapon).OnHitEffectList != null && (newItem as Weapon).OnHitEffectList.Count > 0) {
                        //Debug.Log(gameObject.name + ".CharacterCombat.HandleEquipmentChanged(): New item is a weapon and has the on hit effect " + (newItem as Weapon).MyOnHitEffect.MyName);
                        onHitEffects = (newItem as Weapon).OnHitEffectList;
                    }
                    EquipmentSlotProfile equipmentSlotProfile = baseCharacter.CharacterEquipmentManager.FindEquipmentSlotForEquipment(newItem);
                    if (equipmentSlotProfile != null && equipmentSlotProfile.SetOnHitAudio == true) {
                        if ((newItem as Weapon).DefaultHitSoundEffects != null) {
                            //Debug.Log(gameObject.name + ".CharacterCombat.HandleEquipmentChanged(): setting default hit sound");
                            defaultHitSoundEffects.AddRange((newItem as Weapon).DefaultHitSoundEffects);
                        }
                    }
                }
            }
        }

    }

}