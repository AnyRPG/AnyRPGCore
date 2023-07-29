using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class CharacterCombat : ConfiguredClass {

        //events
        /*
        public event System.Action<UnitController, float> OnKillEvent = delegate { };
        public event System.Action<Interactable> OnEnterCombat = delegate { };
        public event System.Action<UnitController, Interactable> OnHitEvent = delegate { };
        public event System.Action OnCombatUpdate = delegate { };
        public event System.Action<Interactable, AbilityEffectContext> OnReceiveCombatMiss = delegate { };
        */

        protected bool eventSubscriptionsInitialized = false;

        private bool autoAttackActive = false;

        //[Tooltip("The amount of seconds after the last combat event to wait before dropping combat")]
        protected float combatCooldown = 10f;

        // the time at which the last attack was given or received
        protected float lastCombatEvent;

        // the time at which the last animated attack began
        protected float lastAttackBegin;

        //public bool isAttacking { get; private set; }
        protected bool inCombat = false;

        protected float attackSpeed = 1f;

        // components
        protected UnitController unitController;

        // track equipped weapons for managing default hit effects
        protected List<Weapon> equippedWeapons = new List<Weapon>();

        // list of on hit effects to cast on weapon hit if the weapon hit is an auto attack
        private List<AbilityEffectProperties> defaultHitEffects = new List<AbilityEffectProperties>();

        // list of on hit effects to cast on weapon hit from currently equipped weapons
        protected List<AbilityEffectProperties> onHitEffects = new List<AbilityEffectProperties>();

        // the weapon skill from the weapon equipped in the main weapon slot
        protected WeaponSkill mainWeaponSkill = null;

        protected AggroTable aggroTable = null;

        // this is what the current weapon defaults to
        protected List<AudioClip> defaultHitSoundEffects = new List<AudioClip>();

        // the target we swung at, in case we try to change target mid swing and we don't put an animation on something too far away
        protected UnitController swingTarget = null;

        private Coroutine waitForCastsCoroutine = null;
        private Coroutine dropCombatCoroutine = null;

        // game manager references
        protected PlayerManager playerManager = null;
        protected UIManager uIManager = null;
        protected SystemEventManager systemEventManager = null;

        public AggroTable AggroTable {
            get {
                return aggroTable;
            }
        }

        public List<AudioClip> DefaultHitSoundEffects { get => defaultHitSoundEffects; set => defaultHitSoundEffects = value; }
        public UnitController SwingTarget { get => swingTarget; set => swingTarget = value; }
        public bool AutoAttackActive { get => autoAttackActive; set => autoAttackActive = value; }
        public List<AbilityEffectProperties> OnHitEffects { get => onHitEffects; set => onHitEffects = value; }
        public List<AbilityEffectProperties> DefaultHitEffects { get => defaultHitEffects; set => defaultHitEffects = value; }
        public float AttackSpeed { get => attackSpeed; set => attackSpeed = value; }
        public float LastAttackBegin { get => lastAttackBegin; }

        public CharacterCombat(UnitController unitController, SystemGameManager systemGameManager) {
            this.unitController = unitController;
            Configure(systemGameManager);
            aggroTable = new AggroTable(unitController);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            playerManager = systemGameManager.PlayerManager;
            uIManager = systemGameManager.UIManager;
            systemEventManager = systemGameManager.SystemEventManager;
        }

        public void HandleAutoAttack() {
            //Debug.Log($"{gameObject.name}.PlayerCombat.HandleAutoAttack()");
            if (unitController == null) {
                // can't attack without a character
                return;
            }

            if (unitController.Target == null && AutoAttackActive == true) {
                //Debug.Log($"{gameObject.name}.PlayerCombat.HandleAutoAttack(): target is null.  deactivate autoattack");
                DeActivateAutoAttack();
                return;
            }
            if (unitController.CharacterAbilityManager.PerformingAnyAbility() == true) {
                // can't auto-attack during auto-attack, animated attack, or cast
                return;
            }

            if (OnAutoAttackCooldown()) {
                // ensure attacks aren't happening too fast
                return;
            }/* else {
                Debug.Log(baseCharacter.gameObject.name + ".CharacterCombat.HandleAutoAttack() time is " + (Time.time - lastAttackBegin));
            }*/


            if (AutoAttackActive == true && unitController.Target != null) {
                //Debug.Log("player controller is in combat and target is not null");
                //Interactable _interactable = controller.MyTarget.GetComponent<Interactable>();
                CharacterUnit _characterUnit = CharacterUnit.GetCharacterUnit(unitController.Target);
                if (_characterUnit?.UnitController != null) {
                    //Debug.Log($"{gameObject.name}.PlayerCombat.HandleAutoAttack(). targetCharacter is not null.  Attacking");
                    Attack(_characterUnit.UnitController);
                    return;
                }
                // autoattack is active, but we were unable to attack the target because they were dead, or not a lootable character, or didn't have an interactable.
                // There is no reason for autoattack to remain active under these circumstances
                //Debug.Log($"{gameObject.name}: target is not attackable.  deactivate autoattack");
                DeActivateAutoAttack();
            }
        }

        //public void ProcessLevelUnload() {
        public void HandleCharacterUnitDespawn() {
            DropCombat(true);
            if (waitForCastsCoroutine != null) {
                unitController.StopCoroutine(waitForCastsCoroutine);
            }
            if (dropCombatCoroutine != null) {
                unitController.StopCoroutine(dropCombatCoroutine);
            }
        }

        public void Update() {
            if (inCombat == false
                || unitController == null
                || unitController.CharacterStats.IsAlive == false) {
                return;
            }

            if (unitController.Target == null
                || aggroTable.TopAgroNode == null) {
                TryToDropCombat();
            }

            // leave combat if the combat cooldown has expired
            if (Time.time - lastCombatEvent > combatCooldown) {
                //Debug.Log($"{gameObject.name} Leaving Combat");
                TryToDropCombat();
            }

            if (inCombat) {
                unitController.UnitEventController.NotifyOnCombatUpdate();
            }
        }

        public void HandleDie() {
            //Debug.Log($"{gameObject.name}.OnDieHandler()");

            BroadcastCharacterDeath();

            if ((unitController.UnitControllerMode == UnitControllerMode.AI || unitController.UnitControllerMode == UnitControllerMode.Pet)
                && !(unitController.CurrentState is DeathState)) {
                (unitController as UnitController).ChangeState(new DeathState());
                return;
            }
        }

        public void RegisterAnimatedAbilityBegin() {
            lastAttackBegin = Time.time;
        }

        

        /// <summary>
        /// This is the entrypoint to a manual attack.
        /// </summary>
        /// <param name="characterTarget"></param>
        public void Attack(UnitController characterTarget, bool playerInitiated = false) {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterCombat.Attack(" + characterTarget.name + ")");

            if (characterTarget == null) {
                //Debug.Log("You must have a target to attack");
                //logManager.WriteCombatMessage("You must have a target to attack");
            } else {
                // add this here to prevent characters from not being able to attack
                swingTarget = characterTarget;

                // Perform the attack. OnAttack should have been populated by the animator to begin an attack animation and send us an AttackHitEvent to respond to
                if (unitController.CharacterAbilityManager.PerformingAnyAbility() == false) {
                    // in order to support attacks from bows (or wands in the future), the weapon needs to be unsheathed
                    unitController.CharacterAbilityManager.AttemptAutoAttack(playerInitiated);
                }
            }
        }

        public void ProcessTakeDamage(AbilityEffectContext abilityEffectContext, PowerResource powerResource, int damage, IAbilityCaster target, CombatMagnitude combatMagnitude, AbilityEffectProperties abilityEffect) {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterCombat.ProcessTakeDamage(" + damage + ", " + (target == null ? "null" : target.AbilityManager.UnitGameObject.name) + ", " + combatMagnitude.ToString() + ", " + abilityEffect.DisplayName);
            /*
            if (abilityEffectContext == null) {
                abilityEffectContext = new AbilityEffectContext();
                abilityEffectContext.AbilityCaster = source;
            }
            */
            abilityEffectContext.powerResource = powerResource;
            abilityEffectContext.SetResourceAmount(powerResource.ResourceName, damage);

            // prevent infinite reflect loops
            if (abilityEffectContext.reflectDamage == false) {
                foreach (StatusEffectNode statusEffectNode in unitController.CharacterStats.StatusEffects.Values) {
                    //Debug.Log("Casting Reflection On Take Damage");
                    // this could maybe be done better through an event subscription
                    if (statusEffectNode.StatusEffect.ReflectAbilityEffectList.Count > 0) {
                        // we can't reflect on system attackers, so check if this is an interactable
                        Interactable targetInteractable = target.AbilityManager.UnitGameObject.GetComponent<Interactable>();
                        if (targetInteractable != null) {
                            statusEffectNode.StatusEffect.CastReflect(unitController, targetInteractable, abilityEffectContext);
                        }
                    }
                }
            }

            if (target != null
                && playerManager.ActiveUnitController != null
                && unitController.CharacterUnit != null) {
                if (target == (playerManager.ActiveUnitController as IAbilityCaster) ||
                    playerManager.ActiveUnitController == unitController ||
                    target.AbilityManager.IsPlayerControlled()) {
                    // spawn text over enemies damaged by the player and over the player itself
                    CombatTextType combatTextType = CombatTextType.normal;
                    /*
                    if ((abilityEffect as AttackEffect).DamageType == DamageType.physical) {
                        combatTextType = CombatTextType.normal;
                    } else if ((abilityEffect as AttackEffect).DamageType == DamageType.ability) {
                        combatTextType = CombatTextType.ability;
                    }
                    */
                    // this code has issues.  status effects spawned by auto-attacks show wrong color
                    // on-hit effects spawned by auto-attacks show wrong color
                    // testing - add physical requirement
                    //if ((abilityEffectContext.baseAbility is AnimatedAbility) && (abilityEffectContext.baseAbility as AnimatedAbility).IsAutoAttack) {
                    if ((abilityEffectContext.baseAbility is AnimatedAbilityProperties)
                        && (abilityEffectContext.baseAbility as AnimatedAbilityProperties).IsAutoAttack
                        && (abilityEffect as AttackEffectProperties).DamageType == DamageType.physical) {
                        combatTextType = CombatTextType.normal;
                    } else {
                        combatTextType = CombatTextType.ability;
                    }
                    uIManager.CombatTextManager.SpawnCombatText(unitController, damage, combatTextType, combatMagnitude, abilityEffectContext);
                    systemEventManager.NotifyOnTakeDamage(target, unitController.CharacterUnit, damage, abilityEffect.DisplayName);
                }
                lastCombatEvent = Time.time;
                float totalThreat = damage;
                totalThreat *= abilityEffect.ThreatMultiplier * target.AbilityManager.GetThreatModifiers();


                // determine if this target is capable of fighting (ie, not environmental effect), and if so, enter combat
                CharacterUnit _characterUnit = target.AbilityManager.GetCharacterUnit();
                if (_characterUnit != null) {
                    AggroTable.AddToAggroTable(_characterUnit, (int)totalThreat);
                    // commented out and moved to TakeDamage
                    //EnterCombat(_interactable);
                }
            }

            unitController?.UnitEventController.NotifyOnTakeDamage();

        }

        public virtual void TryToDropCombat() {
            if (inCombat == false) {
                //Debug.Log($"{gameObject.name}.TryToDropCombat(): incombat = false. returning");
                return;
            }
            //Debug.Log($"{gameObject.name} trying to drop combat.");
            if (aggroTable.TopAgroNode == null) {
                //Debug.Log($"{gameObject.name}.TryToDropCombat(): topAgroNode is null. Dropping combat.");
                DropCombat();
            } else {
                //Debug.Log($"{gameObject.name}.TryToDropCombat(): topAgroNode was not null");
                // this next condition should prevent crashes as a result of level unloads
                foreach (AggroNode aggroNode in AggroTable.AggroNodes) {
                    UnitController _aiController = aggroNode.aggroTarget.UnitController;
                    // since players don't have an agro radius, we can skip the check and drop combat automatically
                    if (_aiController != null) {
                        if (Vector3.Distance(unitController.transform.position, aggroNode.aggroTarget.Interactable.transform.position) < _aiController.AggroRadius) {
                            // fail if we are inside the agro radius of an AI on our agro table
                            return;
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

        protected virtual void DropCombat(bool immediate = false) {
            //Debug.Log($"{gameObject.name}.CharacterCombat.DropCombat()");
            if (inCombat) {
                inCombat = false;

                /*
                if (waitingForAutoAttack == true) {
                    unitController.CharacterAbilityManager.StopCasting();
                }
                if (baseCharacter?.UnitController?.UnitAnimator != null) {
                    baseCharacter.UnitController.UnitAnimator.SetBool("InCombat", false);
                }
                */
                if (immediate) {
                    ProcessDropCombat();
                } else {
                    if (waitForCastsCoroutine == null) {
                        waitForCastsCoroutine = unitController.StartCoroutine(WaitForCastsToFinish());
                    }
                }
                DeActivateAutoAttack();
                //Debug.Log($"{gameObject.name}.CharacterCombat.DropCombat(): dropped combat.");
                //baseCharacter.UnitController?.UnitModelController?.SheathWeapons();
                unitController.UnitEventController.NotifyOnDropCombat();
            }
        }

        public IEnumerator WaitForCastsToFinish() {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterCombat.WaitForCastsToFinish()");

            while (inCombat == false && unitController.CharacterAbilityManager.PerformingAnyAbility() == true) {
                //Debug.Log(baseCharacter.gameObject.name + ".CharacterCombat.WaitForCastsToFinish() waitingforAutoAttack: " + waitingForAutoAttack + "; currentcastAbility: " + (unitController.CharacterAbilityManager.CurrentCastAbility == null ? "null" : unitController.CharacterAbilityManager.CurrentCastAbility.DisplayName));

                yield return null;
            }
            /*
            if (waitingForAutoAttack == true) {
                unitController.CharacterAbilityManager.StopCasting();
            }
            */
            if (inCombat == false && dropCombatCoroutine == null) {
                dropCombatCoroutine = unitController.StartCoroutine(WaitForDropCombat());
            }

            waitForCastsCoroutine = null;
        }

        public IEnumerator WaitForDropCombat() {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterCombat.WaitForDropCombat()");
            yield return new WaitForSeconds(2);
            if (inCombat == false) {
                ProcessDropCombat();
            }
            dropCombatCoroutine = null;
        }

        private void ProcessDropCombat() {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterCombat.ProcessDropCombat()");
            unitController.UnitAnimator.SetBool("InCombat", false);

            unitController.UnitModelController.SheathWeapons();
        }

        public void ActivateAutoAttack() {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterCombat.ActivateAutoAttack()");

            if (systemConfigurationManager.AllowAutoAttack == true) {
                autoAttackActive = true;
            }
        }

        public void DeActivateAutoAttack() {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterCombat.DeActivateAutoAttack()");

            autoAttackActive = false;
        }

        public bool GetInCombat() {
            return inCombat;
        }

        /*
        public bool EnterCombat(CharacterUnit _characterUnit) {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterCombat.EnterCombat(" + (target == null ? "null" : target.AbilityManager.Name) + ")");
            //Interactable _interactable = target.AbilityManager.UnitGameObject.GetComponent<Interactable>();
            //if (_interactable != null) {
                //CharacterUnit _characterUnit =  CharacterUnit.GetCharacterUnit(_interactable);
                if (_characterUnit != null) {
                    return EnterCombat(_characterUnit.Interactable);
                }
            //}
            return false;
        }
        */

        public bool OnAutoAttackCooldown() {
            return (Time.time - lastAttackBegin < attackSpeed);
        }

        public virtual bool PullIntoCombat(UnitController targetUnitController) {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterCombat.PullIntoCombat()");

            // cancel things like stealth
            if (inCombat == false) {
                targetUnitController.CharacterStats.CancelNonCombatEffects();
            }

            return EnterCombat(targetUnitController);
        }

        /*
        public virtual bool PullIntoCombat(Interactable target) {
            
            return EnterCombat(target);
        }
        */

        /// <summary>
        /// Adds the target to the aggro table with 0 agro to ensure you have something to send a drop combat message to if you did 0 damage to them during combat.
        /// Set inCombat to true.
        /// </summary>
        /// <param name="target"></param>
        /// return true if this is a new entry, false if not
        public virtual bool EnterCombat(Interactable target) {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterCombat.EnterCombat()");

            CharacterUnit _characterUnit = CharacterUnit.GetCharacterUnit(target);
            if (_characterUnit == null || _characterUnit.UnitController.CharacterStats.IsAlive == false || unitController.CharacterStats.IsAlive == false) {
                return false;
            }

            // If we do not have a focus, set the target as the focus
            if (unitController != null && unitController.Target == null) {
                unitController.SetTarget(target);
            }

            lastCombatEvent = Time.time;
            // maybe do this in update?
            unitController.UnitAnimator.SetBool("InCombat", true);

            // moved to PullIntoCombat so that stealth doesn't get canceled the minute an attack is started, causing things like backstab to fail
            /*
            if (inCombat == false) {
                baseCharacter?.CharacterStats.CancelNonCombatEffects();
            }
            */

            inCombat = true;
            if (!aggroTable.AggroTableContains(CharacterUnit.GetCharacterUnit(target))) {
                unitController.UnitEventController.NotifyOnEnterCombat(target);
            }
            unitController.UnitModelController.HoldWeapons();
            return aggroTable.AddToAggroTable(CharacterUnit.GetCharacterUnit(target), 0);
        }

        public BaseAbilityProperties GetValidAttackAbility() {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterCombat.GetValidAttackAbility()");

            List<BaseAbilityProperties> returnList = new List<BaseAbilityProperties>();

            foreach (BaseAbilityProperties baseAbility in unitController.CharacterAbilityManager.AbilityList.Values) {
                //Debug.Log(baseCharacter.gameObject.name + ".CharacterCombat.GetValidAttackAbility(): Checking ability: " + baseAbility.DisplayName);
                if (baseAbility.GetTargetOptions(unitController).CanCastOnEnemy &&
                    unitController.CharacterAbilityManager.CanCastAbility(baseAbility) &&
                    baseAbility.CanUseOn(unitController.Target, unitController) &&
                    // check weapon affinity
                    baseAbility.CanCast(unitController) &&
                    unitController.CharacterAbilityManager.PerformLOSCheck(unitController.Target, baseAbility)) {
                    //Debug.Log(baseCharacter.gameObject.name + ".AICombat.GetValidAttackAbility(): ADDING AN ABILITY TO LIST");

                    returnList.Add(baseAbility);
                }
            }
            if (returnList.Count > 0) {
                int randomIndex = Random.Range(0, returnList.Count);
                //Debug.Log(baseCharacter.gameObject.name + ".AICombat.GetValidAttackAbility(): returnList.Count: " + returnList.Count + "; randomIndex: " + randomIndex);
                return returnList[randomIndex];
            }
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterCombat.GetValidAttackAbility(): ABOUT TO RETURN NULL!");
            return null;
        }

        public BaseAbilityProperties GetMeleeAbility() {
            foreach (BaseAbilityProperties baseAbility in unitController.CharacterAbilityManager.AbilityList.Values) {
                if (baseAbility.GetTargetOptions(unitController).CanCastOnEnemy && baseAbility.GetTargetOptions(unitController).UseMeleeRange == true) {
                    return baseAbility;
                }
            }
            return null;
        }

        public List<BaseAbilityProperties> GetAttackRangeAbilityList() {
            List<BaseAbilityProperties> returnList = new List<BaseAbilityProperties>();
            foreach (BaseAbilityProperties baseAbility in unitController.CharacterAbilityManager.AbilityList.Values) {
                returnList.Add(baseAbility);
            }
            return returnList;
        }

        public float GetMinAttackRange(List<BaseAbilityProperties> baseAbilityList) {
            float returnValue = 0f;

            foreach (BaseAbilityProperties baseAbility in baseAbilityList) {
                if (baseAbility.GetTargetOptions(unitController).CanCastOnEnemy
                    && baseAbility.GetTargetOptions(unitController).UseMeleeRange == false
                    && baseAbility.GetTargetOptions(unitController).MaxRange > 0f) {
                    float returnedMaxRange = baseAbility.GetLOSMaxRange(unitController, unitController.Target);
                    if (returnValue == 0f || returnedMaxRange < returnValue) {
                        returnValue = returnedMaxRange;
                    }
                }
            }
            return returnValue;
        }

        /// <summary>
        /// receive the AttackHitEvent from the attack animation so damage can be triggered against the enemy
        /// </summary>
        public void AttackHitEvent() {
            AttackHitAnimationEvent();
        }

        public bool ProcessAttackHit() {
            if (!unitController.CharacterStats.IsAlive) {
                //Debug.Log($"{gameObject.name}.CharacterCombat.AttackHit_AnimationEvent() Character is not alive!");
                return false;
            }
            CharacterUnit targetCharacterUnit = null;
            //stats.TakeDamage(myStats.damage.GetValue());
            if (unitController.Target != null) {
                targetCharacterUnit = CharacterUnit.GetCharacterUnit(unitController.Target);
            }

            // some attacks can hit more than once.
            // in case this is one of those attacks, get a copy of the ability effect context so subsequent hits do not get input power from each other
            AbilityEffectContext usedAbilityEffectContext = null;
            if (unitController.CharacterAbilityManager.CurrentAbilityEffectContext != null) {
                usedAbilityEffectContext = unitController.CharacterAbilityManager.CurrentAbilityEffectContext.GetCopy();
            } else {
                return false;
            }

            //BaseAbilityProperties animatorCurrentAbility = null;
            if ((unitController.Target != null && targetCharacterUnit != null) || usedAbilityEffectContext.baseAbility.GetTargetOptions(unitController).RequireTarget == false) {

                //bool attackLanded = true;
                if (usedAbilityEffectContext != null) {
                    //animatorCurrentAbility = usedAbilityEffectContext.baseAbility;
                    if (usedAbilityEffectContext.baseAbility is AnimatedAbilityProperties) {
                        /*attackLanded =*/ (usedAbilityEffectContext.baseAbility as AnimatedAbilityProperties).HandleAbilityHit(
                            unitController,
                            unitController.Target,
                            usedAbilityEffectContext);
                    }
                }

                // onHitAbility is only for weapons, not for special moves
                /*
                if (!attackLanded) {
                    return false;
                }
                */

                return true;
            } else {
                // this appears to be some old code since nothing is subscribed to OnHitEvent() !
                //Debug.Log("Processing hit on null target");
                // OnHitEvent is responsible for performing ability effects for animated abilities, and needs to fire no matter what because those effects may not require targets
                if (usedAbilityEffectContext != null) {
                    if (usedAbilityEffectContext.baseAbility.GetTargetOptions(unitController).RequireTarget == false) {
                        unitController.UnitEventController.NotifyOnHitEvent(unitController, unitController.Target);
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// After the attack animation reaches the point where it contacts the enemy, do damage to it
        /// </summary>
        public void AttackHitAnimationEvent() {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterCombat.AttackHit_AnimationEvent()");
            ProcessAttackHit();
        }

        public virtual void ReceiveCombatMiss(Interactable targetObject, AbilityEffectContext abilityEffectContext) {
            //Debug.Log($"{gameObject.name}.CharacterCombat.ReceiveCombatMiss()");
            lastCombatEvent = Time.time;
            unitController.UnitEventController.NotifyOnReceiveCombatMiss(targetObject, abilityEffectContext);
            
            // miss sound should only be played on attacking unit
            if (targetObject != unitController) {
                unitController.UnitEventController.NotifyOnCombatMiss();
            }
        }

        public bool DidAttackMiss() {
            //Debug.Log($"{gameObject.name}.CharacterCombat.DidAttackMiss()");
            int randomNumber = UnityEngine.Random.Range(0, 100);
            int randomCutoff = (int)Mathf.Clamp(unitController.CharacterStats.GetAccuracyModifiers(), 0, 100);
            //Debug.Log($"{gameObject.name}.CharacterCombat.DidAttackMiss(): number: " + randomNumber + "; accuracy = " + randomCutoff);
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
        private bool TakeDamageCommon(AbilityEffectContext abilityEffectContext, PowerResource powerResource, int damage, IAbilityCaster source, CombatMagnitude combatMagnitude, AbilityEffectProperties abilityEffect) {
            //Debug.Log($"{gameObject.name}.TakeDamageCommon(" + damage + ")");

            // perform check to see if this character has the resource to be reduced.  if not, it is immune to this type of damage
            if (unitController.CharacterStats.WasImmuneToDamageType(powerResource, source, abilityEffectContext)) {
                return false;
            }

            damage = (int)(damage * unitController.CharacterStats.GetIncomingDamageModifiers());

            ProcessTakeDamage(abilityEffectContext, powerResource, damage, source, combatMagnitude, abilityEffect);
            //Debug.Log($"{gameObject.name} sending " + damage.ToString() + " to character stats");
            unitController.CharacterStats.ReducePowerResource(powerResource, damage);
            
            // check if dead.  if alive, then play take damage animation
            return true;
        }

        public virtual bool TakeDamage(AbilityEffectContext abilityEffectContext, PowerResource powerResource, int damage, IAbilityCaster sourceCharacter, CombatMagnitude combatMagnitude, AttackEffectProperties abilityEffect) {
            //public virtual bool TakeDamage(AbilityEffectContext abilityEffectContext, PowerResource powerResource, int damage, IAbilityCaster sourceCharacter, CombatMagnitude combatMagnitude, AbilityEffectProperties abilityEffect) {
            //Debug.Log($"{gameObject.name}.TakeDamage(" + damage + ", " + sourcePosition + ", " + source.name + ")");
            if (unitController.UnitControllerMode == UnitControllerMode.AI || unitController.UnitControllerMode == UnitControllerMode.Pet) {
                if (unitController.CurrentState is EvadeState || unitController.CurrentState is DeathState) {
                    return false;
                }
            }

            if (unitController.CharacterStats.IsAlive) {

                CharacterUnit _characterUnit = sourceCharacter.AbilityManager.GetCharacterUnit();
                if (_characterUnit != null) {
                    unitController.Aggro(_characterUnit);
                }
                //Debug.Log($"{gameObject.name} about to take " + damage.ToString() + " damage. Character is alive");
                //float distance = Vector3.Distance(transform.position, sourcePosition);
                // replace with hitbox check
                bool canPerformAbility = true;
                if (abilityEffect.DamageType == DamageType.physical) {
                    damage -= (int)unitController.CharacterStats.SecondaryStats[SecondaryStatType.Armor].CurrentValue - (int)(unitController.CharacterStats.SecondaryStats[SecondaryStatType.Armor].CurrentValue * Mathf.Clamp(abilityEffect.IgnoreArmorPercent / 100f, 0f, 1f));
                    damage = Mathf.Clamp(damage, 0, int.MaxValue);
                }
                if (abilityEffect.GetTargetOptions(sourceCharacter).UseMeleeRange) {
                    if (!sourceCharacter.AbilityManager.IsTargetInMeleeRange(unitController)) {
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

        public virtual void OnKillConfirmed(UnitController sourceCharacter, float creditPercent) {
            //Debug.Log($"{gameObject.name} received death broadcast from " + sourceCharacter.AbilityManager.name);
            if (sourceCharacter != null) {
                unitController.UnitEventController.NotifyOnKillEvent(sourceCharacter, creditPercent);
            }
            aggroTable.ClearSingleTarget(sourceCharacter.CharacterUnit);
            TryToDropCombat();
            unitController?.UnitEventController.NotifyOnKillTarget();
        }

        public virtual void BroadcastCharacterDeath() {
            if (!unitController.CharacterStats.IsAlive) {
                // putting this here because it can be overwritten easier than the event handler that calls it
                //Debug.Log($"{gameObject.name} broadcasting death to aggro table");
                Dictionary<CharacterCombat, float> broadcastDictionary = new Dictionary<CharacterCombat, float>();
                foreach (AggroNode _aggroNode in AggroTable.AggroNodes) {
                    if (_aggroNode.aggroTarget == null) {
                        //Debug.Log($"{gameObject.name}: aggronode.aggrotarget is null!");
                    } else {
                        CharacterCombat _otherCharacterCombat = _aggroNode.aggroTarget.UnitController.CharacterCombat as CharacterCombat;
                        if (_otherCharacterCombat != null) {
                            broadcastDictionary.Add(_otherCharacterCombat, (_aggroNode.aggroValue > 0 ? 1 : 0));
                        } else {
                            //Debug.Log($"{gameObject.name}: aggronode.aggrotarget(" + _aggroNode.aggroTarget.name + ") had no character combat!");
                        }
                    }
                }
                foreach (CharacterCombat characterCombat in broadcastDictionary.Keys) {
                    characterCombat.OnKillConfirmed(unitController, broadcastDictionary[characterCombat]);
                }
                aggroTable.ClearTable();
                DropCombat();
            }
        }

        public virtual bool GetWeaponSkillAttackVoiceSetting() {
            if (mainWeaponSkill != null) {
                return mainWeaponSkill.WeaponSkillProps.PlayAttackVoice;
            }
            return false;
        }

        public virtual AudioClip GetWeaponSkillAnimationHitSound() {
            return mainWeaponSkill?.WeaponSkillProps.AnimationEventAudioProfile?.RandomAudioClip;
        }

        public virtual void SetMainWeaponSkill(WeaponSkill weaponSkill) {
            this.mainWeaponSkill = weaponSkill;
        }

        public virtual void AddOnHitEffects(List<AbilityEffectProperties> abilityEffectProperties) {
            onHitEffects.AddRange(abilityEffectProperties);
        }

        public virtual void RemoveOnHitEffect(AbilityEffectProperties abilityEffect) {
            if (onHitEffects.Contains(abilityEffect)) {
                //Debug.Log(baseCharacter.gameObject.name + ".CharacterCombat.HandleEquipmentChanged(): olditem (" + oldItem.DisplayName + ") was weapon and removing hit effect: " + abilityEffect.DisplayName);
                onHitEffects.Remove(abilityEffect);
            }
        }

        public virtual void AddDefaultHitEffects(List<AbilityEffectProperties> abilityEffectProperties) {
            defaultHitEffects.AddRange(abilityEffectProperties);
        }

        public virtual void ClearUnitDefaultHitEffects() {
            if (unitController?.UnitProfile != null) {
                foreach (AbilityEffectProperties abilityEffectProperties in unitController?.UnitProfile.DefaultHitEffectList) {
                    RemoveDefaultHitEffect(abilityEffectProperties);
                }
            }
        }

        public virtual void RemoveDefaultHitEffect(AbilityEffectProperties abilityEffect) {
            if (defaultHitEffects.Contains(abilityEffect)) {
                //Debug.Log(baseCharacter.gameObject.name + ".CharacterCombat.HandleEquipmentChanged(): olditem (" + oldItem.DisplayName + ") was weapon and removing hit effect: " + abilityEffect.DisplayName);
                defaultHitEffects.Remove(abilityEffect);
            }
        }

        public virtual void AddDefaultHitSoundEffects(List<AudioClip> audioClips) {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterCombat.AddDefaultHitSoundEffects(" + audioClips.Count + ")");
            defaultHitSoundEffects.AddRange(audioClips);
        }

        public virtual void ClearDefaultHitSoundEffects() {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterCombat.ClearDefaultHitSoundEffects()");
            defaultHitSoundEffects.Clear();
        }

        public virtual void HandleEquipmentChanged(Equipment newItem, Equipment oldItem, int slotIndex, EquipmentSlotProfile equipmentSlotProfile) {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterCombat.HandleEquipmentChanged(" + (newItem == null ? "null" : newItem.DisplayName) + ", " + (oldItem == null ? "null" : oldItem.DisplayName) + ", " + slotIndex + ")");

            if (equipmentSlotProfile.MainWeaponSlot == false) {
                // to avoid hitting twice with each attack, only main hand weapons are counted for on hit effects
                return;
            }

            if (oldItem != null) {
                oldItem.HandleUnequip(this, equipmentSlotProfile);
            }

            if (newItem != null) {
                newItem.HandleEquip(this, equipmentSlotProfile);
            }
        }

        public virtual void WeaponEquipped(Weapon weapon, EquipmentSlotProfile equipmentSlotProfile) {
            
            equippedWeapons.Add(weapon);
            ClearUnitDefaultHitEffects();
            unitController.CharacterAbilityManager.WeaponEquipped(weapon);

            if (weapon.OnHitEffectList.Count > 0) {
                AddOnHitEffects(weapon.OnHitEffectList);
            }
            if (weapon.DefaultHitEffectList.Count > 0) {
                AddDefaultHitEffects(weapon.DefaultHitEffectList);
            }
            if (equipmentSlotProfile != null) {
                if (equipmentSlotProfile.MainWeaponSlot == true) {
                    SetMainWeaponSkill(weapon.WeaponSkill);
                }
                if (equipmentSlotProfile.SetOnHitAudio == true) {
                    if (weapon.DefaultHitSoundEffects != null) {
                        AddDefaultHitSoundEffects(weapon.DefaultHitSoundEffects);
                    }
                }
            }

            SetAttackSpeed();
        }

        public virtual void AddUnitProfileHitEffects() {
            if (equippedWeapons.Count == 0 && unitController.UnitProfile != null) {
                AddDefaultHitEffects(unitController.UnitProfile.DefaultHitEffectList);
            }
        }

        public virtual void WeaponUnequipped(Weapon weapon, EquipmentSlotProfile equipmentSlotProfile) {
            if (equippedWeapons.Contains(weapon)) {
                equippedWeapons.Remove(weapon);
            }
            AddUnitProfileHitEffects();
            unitController.CharacterAbilityManager.WeaponUnequipped(weapon);

            if (weapon.OnHitEffectList != null && weapon.OnHitEffectList.Count > 0) {
                foreach (AbilityEffectProperties abilityEffect in weapon.OnHitEffectList) {
                    RemoveOnHitEffect(abilityEffect);
                }
            }
            if (weapon.DefaultHitEffectList != null && weapon.DefaultHitEffectList.Count > 0) {
                foreach (AbilityEffectProperties abilityEffect in weapon.DefaultHitEffectList) {
                    RemoveDefaultHitEffect(abilityEffect);
                }
            }
            if (equipmentSlotProfile != null) {
                if (equipmentSlotProfile.MainWeaponSlot == true) {
                    SetMainWeaponSkill(null);
                }
                if (equipmentSlotProfile.SetOnHitAudio == true) {
                    //Debug.Log(baseCharacter.gameObject.name + ".CharacterCombat.HandleEquipmentChanged(): clearing default hit effects");
                    ClearDefaultHitSoundEffects();
                }
            }

            SetAttackSpeed();
        }

        public virtual void SetAttackSpeed() {
            float maxAttackSpeed = 0f;
            foreach (Equipment equipment in unitController.CharacterEquipmentManager.CurrentEquipment.Values) {
                if ((equipment is Weapon) && (equipment as Weapon).WeaponSkill != null && (equipment as Weapon).WeaponSkill.WeaponSkillProps.AttackSpeed > maxAttackSpeed) {
                    maxAttackSpeed = (equipment as Weapon).WeaponSkill.WeaponSkillProps.AttackSpeed;
                }
            }
            attackSpeed = maxAttackSpeed;
        }

    }

}