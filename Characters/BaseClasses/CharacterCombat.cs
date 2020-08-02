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

        private bool autoAttackActive = false;

        //[Tooltip("The amount of seconds after the last combat event to wait before dropping combat")]
        protected float combatCooldown = 10f;

        protected float lastCombatEvent;

        //public bool isAttacking { get; private set; }
        protected bool inCombat = false;

        // components
        protected BaseCharacter baseCharacter;

        protected AbilityEffect onHitEffect = null;

        protected AggroTable aggroTable = null;

        // this is what the current weapon defaults to
        protected AudioClip defaultHitSoundEffect = null;

        /// <summary>
        ///  waiting for the animator to let us know we can hit again
        /// </summary>
        private bool waitingForAutoAttack = false;

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

        public AudioClip DefaultHitSoundEffect { get => defaultHitSoundEffect; set => defaultHitSoundEffect = value; }
        public BaseCharacter MySwingTarget { get => swingTarget; set => swingTarget = value; }
        public bool MyAutoAttackActive { get => autoAttackActive; set => autoAttackActive = value; }
        public AbilityEffect MyOnHitEffect { get => onHitEffect; set => onHitEffect = value; }

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
            //Debug.Log(gameObject.name + ".CharacterCombat.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized) {
                return;
            }
            if (baseCharacter != null && baseCharacter.CharacterStats != null) {
                baseCharacter.CharacterStats.OnDie += OnDieHandler;
            }
            if (baseCharacter != null && baseCharacter.CharacterEquipmentManager != null) {
                //Debug.Log(gameObject.name + ".CharacterCombat.CreateEventSubscriptions(): subscribing to onequipmentchanged");
                baseCharacter.CharacterEquipmentManager.OnEquipmentChanged += HandleEquipmentChanged;
            }
            SystemEventManager.StartListening("OnLevelUnload", HandleLevelUnload);
            eventSubscriptionsInitialized = true;
        }

        protected virtual void CleanupEventSubscriptions() {
            //Debug.Log("PlayerCombat.CleanupEventSubscriptions()");
            if (baseCharacter != null && baseCharacter.CharacterStats != null) {
                baseCharacter.CharacterStats.OnDie -= OnDieHandler;
            }
            if (baseCharacter != null && baseCharacter.CharacterEquipmentManager != null) {
                baseCharacter.CharacterEquipmentManager.OnEquipmentChanged -= HandleEquipmentChanged;
            }
            if (SystemEventManager.MyInstance != null) {
                SystemEventManager.StopListening("OnLevelUnload", HandleLevelUnload);
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
        }

        public void HandleLevelUnload(string eventName, EventParamProperties eventParamProperties) {
            ProcessLevelUnload();
        }


        public void ProcessLevelUnload() {
            SetWaitingForAutoAttack(false);
            DropCombat();
        }

        protected virtual void Update() {
            if (baseCharacter == null) {
                return;
            }
            if (!baseCharacter.CharacterStats.IsAlive) {
                return;
            }

            // reduce the autoattack cooldown
            //if (attackCooldown > 0f) {
                //Debug.Log(gameObject.name + ".CharacterCombat.Update(): attackCooldown: " + attackCooldown);
                //attackCooldown -= Time.deltaTime;
            //}

            if (inCombat && baseCharacter.CharacterController.MyTarget == null) {
                TryToDropCombat();
            }
        }

        public bool WaitingForAction() {
            if (baseCharacter.CharacterAbilityManager.WaitingForAnimatedAbility == true || baseCharacter.CharacterCombat.MyWaitingForAutoAttack == true || baseCharacter.CharacterAbilityManager.IsCasting) {
                // can't auto-attack during auto-attack, animated attack, or cast
                return true;
            }
            return false;
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

        public virtual void ProcessTakeDamage(AbilityEffectContext abilityEffectContext, PowerResource powerResource, int damage, IAbilityCaster target, CombatMagnitude combatMagnitude, AbilityEffect abilityEffect) {
            //Debug.Log(gameObject.name + ".CharacterCombat.ProcessTakeDamage(" + damage + ", " + (target == null ? "null" : target.Name) + ", " + combatMagnitude.ToString() + ", " + abilityEffect.MyName);

            if (abilityEffectContext == null) {
                abilityEffectContext = new AbilityEffectContext();
            }
            abilityEffectContext.powerResource = powerResource;
            abilityEffectContext.SetResourceAmount(powerResource.DisplayName, damage);

            // prevent infinite reflect loops
            if (abilityEffectContext.reflectDamage == false) {
                foreach (StatusEffectNode statusEffectNode in MyBaseCharacter.CharacterStats.StatusEffects.Values) {
                    //Debug.Log("Casting Reflection On Take Damage");
                    // this could maybe be done better through an event subscription
                    if (statusEffectNode.StatusEffect.MyReflectAbilityEffectList.Count > 0) {
                        statusEffectNode.StatusEffect.CastReflect(MyBaseCharacter.CharacterAbilityManager, target.UnitGameObject, abilityEffectContext);
                    }
                }
            }

            if (target != null && PlayerManager.MyInstance != null && PlayerManager.MyInstance.MyCharacter != null && PlayerManager.MyInstance.MyCharacter.CharacterUnit != null && PlayerManager.MyInstance.MyPlayerUnitObject != null && baseCharacter != null && baseCharacter.CharacterUnit != null) {
                if (target == (PlayerManager.MyInstance.MyCharacter.CharacterAbilityManager as IAbilityCaster) ||
                    (PlayerManager.MyInstance.MyCharacter as BaseCharacter) == (baseCharacter as BaseCharacter) ||
                    target.IsPlayerControlled()) {
                    // spawn text over enemies damaged by the player and over the player itself
                    CombatTextType combatTextType = CombatTextType.normal;
                    if ((abilityEffect as AttackEffect).DamageType == DamageType.physical) {
                        combatTextType = CombatTextType.normal;
                    } else if ((abilityEffect as AttackEffect).DamageType == DamageType.ability) {
                        combatTextType = CombatTextType.ability;
                    }
                    CombatTextManager.MyInstance.SpawnCombatText(baseCharacter.CharacterUnit.gameObject, damage, combatTextType, combatMagnitude, abilityEffectContext);
                    SystemEventManager.MyInstance.NotifyOnTakeDamage(target, MyBaseCharacter.CharacterUnit, damage, abilityEffect.DisplayName);
                }
                lastCombatEvent = Time.time;
                float totalThreat = damage;
                totalThreat *= abilityEffect.ThreatMultiplier * target.GetThreatModifiers();
                target.AddToAggroTable(baseCharacter.CharacterUnit, (int)totalThreat);
                //aggroTable.AddToAggroTable(target.CharacterUnit, );
                EnterCombat(target);
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
                if (MyBaseCharacter.CharacterUnit != null) {
                    foreach (AggroNode aggroNode in MyAggroTable.MyAggroNodes) {
                        AIController _aiController = aggroNode.aggroTarget.MyCharacter.CharacterController as AIController;
                        // since players don't have an agro radius, we can skip the check and drop combat automatically
                        if (_aiController != null) {
                            if (Vector3.Distance(MyBaseCharacter.CharacterUnit.transform.position, aggroNode.aggroTarget.transform.position) < _aiController.AggroRadius) {
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
            if (baseCharacter != null && baseCharacter.AnimatedUnit != null && baseCharacter.AnimatedUnit.MyCharacterAnimator != null) {
                baseCharacter.AnimatedUnit.MyCharacterAnimator.SetBool("InCombat", false);
            }
            DeActivateAutoAttack();
            //Debug.Log(gameObject.name + ".CharacterCombat.DropCombat(): dropped combat.");
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

        /// <summary>
        /// Adds the target to the aggro table with 0 agro to ensure you have something to send a drop combat message to if you did 0 damage to them during combat.
        /// Set inCombat to true.
        /// </summary>
        /// <param name="target"></param>
        /// return true if this is a new entry, false if not
        public virtual bool EnterCombat(IAbilityCaster target) {
            //Debug.Log(gameObject.name + ".CharacterCombat.EnterCombat(" + (target != null && target.MyName != null ? target.MyName : "null") + ")");
            if (MyBaseCharacter.CharacterStats.IsAlive == false) {
                //Debug.Log(gameObject.name + ".CharacterCombat.EnterCombat(" + (target != null && target.MyName != null ? target.MyName : "null") + "): character is not alive, returning!");
                return false;
            }
            // try commenting this out to fix bug where things that have agrod but done no damage don't get death notifications
            //if (!inCombat) {
            //Debug.Log(gameObject.name + " Entering Combat with " + target.name);
            //}
            lastCombatEvent = Time.time;
            // maybe do this in update?
            if (baseCharacter != null && baseCharacter.AnimatedUnit != null && baseCharacter.AnimatedUnit.MyCharacterAnimator != null) {
                baseCharacter.AnimatedUnit.MyCharacterAnimator.SetBool("InCombat", true);
            }
            inCombat = true;
            OnEnterCombat();
            if (target.AddToAggroTable(baseCharacter.CharacterUnit, 0)) {
                return true;
            }
            /*
            if (aggroTable.AddToAggroTable(target.CharacterUnit, 0)) {
                return true;
            }
            */
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
            if (!baseCharacter.CharacterStats.IsAlive) {
                //Debug.Log(gameObject.name + ".CharacterCombat.AttackHit_AnimationEvent() Character is not alive!");
                return false;
            }
            CharacterUnit targetCharacterUnit = null;
            //stats.TakeDamage(myStats.damage.GetValue());
            if (MyBaseCharacter.CharacterController.MyTarget != null) {
                targetCharacterUnit = MyBaseCharacter.CharacterController.MyTarget.GetComponent<CharacterUnit>();
            }

            if (MyBaseCharacter.CharacterController.MyTarget != null && targetCharacterUnit != null) {

                // OnHitEvent is responsible for performing ability effects for animated abilities, and needs to fire no matter what because those effects may not require targets
                //OnHitEvent(baseCharacter as BaseCharacter, MyBaseCharacter.MyCharacterController.MyTarget);

                BaseAbility animatorCurrentAbility = null;
                bool attackLanded = true;
                if (MyBaseCharacter != null && MyBaseCharacter.AnimatedUnit != null && MyBaseCharacter.AnimatedUnit.MyCharacterAnimator != null && MyBaseCharacter.AnimatedUnit.MyCharacterAnimator.MyCurrentAbilityEffectContext != null) {
                    animatorCurrentAbility = MyBaseCharacter.AnimatedUnit.MyCharacterAnimator.MyCurrentAbilityEffectContext.baseAbility;
                    if (animatorCurrentAbility is AnimatedAbility) {
                        attackLanded = (MyBaseCharacter.AnimatedUnit.MyCharacterAnimator.MyCurrentAbilityEffectContext.baseAbility as AnimatedAbility).HandleAbilityHit(
                            MyBaseCharacter.CharacterAbilityManager,
                            MyBaseCharacter.CharacterController.MyTarget,
                            MyBaseCharacter.AnimatedUnit.MyCharacterAnimator.MyCurrentAbilityEffectContext);
                    }
                }

                // onHitAbility is only for weapons, not for special moves
                if (!attackLanded) {
                    return false;
                }

                if (animatorCurrentAbility != null) {
                    AudioClip audioClip = animatorCurrentAbility.GetHitSound(baseCharacter.CharacterAbilityManager);
                    if (audioClip != null) {
                        baseCharacter.CharacterUnit.UnitAudio.PlayEffect(audioClip);
                    }
                    //AudioManager.MyInstance.PlayEffect(overrideHitSoundEffect);
                }

                AbilityEffectContext abilityAffectInput = new AbilityEffectContext();
                foreach (StatusEffectNode statusEffectNode in MyBaseCharacter.CharacterStats.StatusEffects.Values) {
                    //Debug.Log(gameObject.name + ".CharacterCombat.AttackHit_AnimationEvent(): Casting OnHit Ability On Take Damage");
                    // this could maybe be done better through an event subscription
                    if (statusEffectNode.StatusEffect.MyWeaponHitAbilityEffectList.Count > 0) {
                        statusEffectNode.StatusEffect.CastWeaponHit(MyBaseCharacter.CharacterAbilityManager as IAbilityCaster, targetCharacterUnit.gameObject, abilityAffectInput);
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
                if (baseCharacter != null && baseCharacter.CharacterUnit != null && baseCharacter.AnimatedUnit.MyCharacterAnimator != null && baseCharacter.AnimatedUnit.MyCharacterAnimator.MyCurrentAbilityEffectContext != null) {
                    if (baseCharacter.AnimatedUnit.MyCharacterAnimator.MyCurrentAbilityEffectContext.baseAbility.MyRequiresTarget == false) {
                        OnHitEvent(baseCharacter as BaseCharacter, MyBaseCharacter.CharacterController.MyTarget);
                        return true;
                    }
                }
            }
            return false;
        }

        public virtual void ReceiveCombatMiss(GameObject targetObject, AbilityEffectContext abilityEffectContext) {
            //Debug.Log(gameObject.name + ".CharacterCombat.ReceiveCombatMiss()");
            if (targetObject == PlayerManager.MyInstance.MyPlayerUnitObject || baseCharacter.CharacterUnit == PlayerManager.MyInstance.MyCharacter.CharacterUnit) {
                CombatTextManager.MyInstance.SpawnCombatText(targetObject, 0, CombatTextType.miss, CombatMagnitude.normal, abilityEffectContext);
            }
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

            damage = (int)(damage * MyBaseCharacter.CharacterStats.GetIncomingDamageModifiers());

            ProcessTakeDamage(abilityEffectContext, powerResource, damage, source, combatMagnitude, abilityEffect);
            //Debug.Log(gameObject.name + " sending " + damage.ToString() + " to character stats");
            baseCharacter.CharacterStats.ReducePowerResource(powerResource, damage);
            return true;
        }

        public virtual bool TakeDamage(AbilityEffectContext abilityEffectContext, PowerResource powerResource, int damage, IAbilityCaster sourceCharacter, CombatMagnitude combatMagnitude, AbilityEffect abilityEffect) {
            //Debug.Log(gameObject.name + ".TakeDamage(" + damage + ", " + sourcePosition + ", " + source.name + ")");
            if (baseCharacter.CharacterStats.IsAlive) {
                //Debug.Log(gameObject.name + " about to take " + damage.ToString() + " damage. Character is alive");
                //float distance = Vector3.Distance(transform.position, sourcePosition);
                // replace with hitbox check
                bool canPerformAbility = true;
                if ((abilityEffect as AttackEffect).DamageType == DamageType.physical) {
                    damage -= (int)baseCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Armor].CurrentValue;
                    damage = Mathf.Clamp(damage, 0, int.MaxValue);
                }
                if (abilityEffect.UseMeleeRange) {
                    if (!sourceCharacter.IsTargetInMeleeRange(baseCharacter.CharacterUnit.gameObject)) {
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
            //Debug.Log(gameObject.name + " received death broadcast from " + sourceCharacter.name);
            if (sourceCharacter != null) {
                OnKillEvent(sourceCharacter, creditPercent);
            }
            aggroTable.ClearSingleTarget(sourceCharacter.CharacterUnit);
            TryToDropCombat();
        }

        public virtual void BroadcastCharacterDeath() {
            if (!baseCharacter.CharacterStats.IsAlive) {
                // putting this here because it can be overwritten easier than the event handler that calls it
                //Debug.Log(gameObject.name + " broadcasting death to aggro table");
                Dictionary<CharacterCombat, float> broadcastDictionary = new Dictionary<CharacterCombat, float>();
                foreach (AggroNode _aggroNode in MyAggroTable.MyAggroNodes) {
                    if (_aggroNode.aggroTarget == null) {
                        //Debug.Log(gameObject.name + ": aggronode.aggrotarget is null!");
                    } else {
                        CharacterCombat _otherCharacterCombat = _aggroNode.aggroTarget.GetComponent<CharacterUnit>().MyCharacter.CharacterCombat as CharacterCombat;
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

        public virtual void HandleEquipmentChanged(Equipment newItem, Equipment oldItem) {
            //Debug.Log(gameObject.name + ".CharacterCombat.HandleEquipmentChanged(" + (newItem == null ? "null" : newItem.DisplayName) + ", " + (oldItem == null ? "null" : oldItem.DisplayName) + ")");

            if (oldItem != null && oldItem is Weapon) {
                if ((oldItem as Weapon).MyOnHitEffect != null) {
                    onHitEffect = null;
                }
                EquipmentSlotProfile equipmentSlotProfile = baseCharacter.CharacterEquipmentManager.FindEquipmentSlotForEquipment(oldItem);
                if (equipmentSlotProfile != null && equipmentSlotProfile.SetOnHitAudio == true) {
                    defaultHitSoundEffect = null;
                }
            }

            if (newItem != null) {
                if (newItem is Weapon) {
                    onHitEffect = null;
                    //Debug.Log(gameObject.name + ".CharacterCombat.HandleEquipmentChanged(): item is a weapon");
                    //overrideHitSoundEffect = null;
                    //defaultHitSoundEffect = null;
                    if ((newItem as Weapon).MyOnHitEffect != null) {
                        //Debug.Log(gameObject.name + ".CharacterCombat.HandleEquipmentChanged(): New item is a weapon and has the on hit effect " + (newItem as Weapon).MyOnHitEffect.MyName);
                        onHitEffect = (newItem as Weapon).MyOnHitEffect;
                    }
                    EquipmentSlotProfile equipmentSlotProfile = baseCharacter.CharacterEquipmentManager.FindEquipmentSlotForEquipment(newItem);
                    if (equipmentSlotProfile != null && equipmentSlotProfile.SetOnHitAudio == true) {
                        defaultHitSoundEffect = null;
                        if ((newItem as Weapon).MyDefaultHitSoundEffect != null) {
                            //Debug.Log(gameObject.name + ".CharacterCombat.HandleEquipmentChanged(): setting default hit sound");
                            defaultHitSoundEffect = (newItem as Weapon).MyDefaultHitSoundEffect;
                        }
                    }
                }
            }
        }

    }

}