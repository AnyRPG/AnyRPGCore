using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class CharacterAbilityManager : MonoBehaviour, ICharacterAbilityManager {
        public event System.Action<IAbility, float> OnCastTimeChanged = delegate { };
        public event System.Action<BaseCharacter> OnCastStop = delegate { };

        protected ICharacter baseCharacter;

        protected Coroutine currentCast = null;
        protected Coroutine abilityHitDelayCoroutine = null;
        protected Coroutine destroyAbilityEffectObjectCoroutine = null;

        protected Dictionary<string, IAbility> abilityList = new Dictionary<string, IAbility>();
        protected Dictionary<string, AbilityCoolDownNode> abilityCoolDownDictionary = new Dictionary<string, AbilityCoolDownNode>();

        protected bool isCasting = false;

        private Vector3 groundTarget = Vector3.zero;

        private bool targettingModeActive = false;

        // does killing the player you are currently targetting stop your cast.  gets set to false when channeling aoe.
        private bool killStopCast = true;

        protected float remainingGlobalCoolDown = 0f;

        protected bool startHasRun = false;

        protected bool eventSubscriptionsInitialized = false;

        // we need a reference to the total length of the current global cooldown to properly calculate radial fill on the action buttons
        protected float initialGlobalCoolDown;

        public float MyInitialGlobalCoolDown { get => initialGlobalCoolDown; set => initialGlobalCoolDown = value; }

        public float MyRemainingGlobalCoolDown { get => remainingGlobalCoolDown; set => remainingGlobalCoolDown = value; }

        private bool waitingForAnimatedAbility = false;

        public ICharacter MyBaseCharacter {
            get => baseCharacter;
            set => baseCharacter = value;
        }

        public Dictionary<string, IAbility> MyAbilityList { get => abilityList; }
        public bool MyWaitingForAnimatedAbility { get => waitingForAnimatedAbility; set => waitingForAnimatedAbility = value; }
        public bool MyIsCasting { get => isCasting; set => isCasting = value; }
        public Dictionary<string, AbilityCoolDownNode> MyAbilityCoolDownDictionary { get => abilityCoolDownDictionary; set => abilityCoolDownDictionary = value; }
        public Coroutine MyCurrentCast { get => currentCast; }

        protected virtual void Awake() {
            //Debug.Log("CharacterAbilityManager.Awake()");
            baseCharacter = GetComponent<BaseCharacter>() as ICharacter;
            //abilityList = SystemAbilityManager.MyInstance.GetResourceList();
        }

        protected virtual void Start() {
            //Debug.Log(gameObject.name + "CharacterAbilityManager.Start()");
            startHasRun = true;
            UpdateAbilityList(baseCharacter.MyCharacterStats.MyLevel);
            //CreateEventSubscriptions();
        }

        public virtual void CreateEventSubscriptions() {
            //Debug.Log("CharacterAbilityManager.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized) {
                return;
            }
            SystemEventManager.MyInstance.OnLevelChanged += UpdateAbilityList;
            baseCharacter.MyCharacterCombat.OnKillEvent += ReceiveKillDetails;
            SystemEventManager.MyInstance.OnLevelUnload += HandleLevelUnload;
            if (baseCharacter != null && baseCharacter.MyCharacterStats != null) {
                baseCharacter.MyCharacterStats.OnDie += OnDieHandler;
            }
            if (baseCharacter != null && baseCharacter.MyCharacterEquipmentManager != null) {
                baseCharacter.MyCharacterEquipmentManager.OnEquipmentChanged += HandleEquipmentChanged;
            } else {

            }
            eventSubscriptionsInitialized = true;
        }

        public virtual void CleanupEventSubscriptions() {
            if (!eventSubscriptionsInitialized) {
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
            if (baseCharacter != null && baseCharacter.MyCharacterEquipmentManager != null) {
                baseCharacter.MyCharacterEquipmentManager.OnEquipmentChanged -= HandleEquipmentChanged;
            }
            OnCharacterUnitDespawn();
            eventSubscriptionsInitialized = false;
        }

        public virtual void OnDisable() {
            CleanupEventSubscriptions();
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
            CleanupCoolDownRoutines();
        }

        public void BeginAbilityCoolDown(BaseAbility baseAbility) {
            float abilityCoolDown = baseAbility.abilityCoolDown;

            Coroutine coroutine = StartCoroutine(PerformAbilityCoolDown(baseAbility.MyName));
            AbilityCoolDownNode abilityCoolDownNode = new AbilityCoolDownNode();
            abilityCoolDownNode.MyAbilityName = baseAbility.MyName;
            abilityCoolDownNode.MyCoroutine = coroutine;
            abilityCoolDownNode.MyRemainingCoolDown = abilityCoolDown;

            if (!abilityCoolDownDictionary.ContainsKey(baseAbility.MyName)) {
                abilityCoolDownDictionary[baseAbility.MyName] = abilityCoolDownNode;
            }
        }

        public void CleanupCoolDownRoutines() {
            foreach (AbilityCoolDownNode abilityCoolDownNode in abilityCoolDownDictionary.Values) {
                if (abilityCoolDownNode.MyCoroutine != null) {
                    StopCoroutine(abilityCoolDownNode.MyCoroutine);
                }
            }
            abilityCoolDownDictionary.Clear();
        }

        public void HandleEquipmentChanged(Equipment newItem, Equipment oldItem) {
            Debug.Log(gameObject.name + ".CharacterAbilityManager.HandleEquipmentChanged(" + (newItem != null ? newItem.MyName : "null") + ", " + (oldItem != null ? oldItem.MyName : "null") + ")");
            if (newItem != null) {
                if (newItem.MyOnEquipAbility != null) {
                    BeginAbility(newItem.MyOnEquipAbility);
                }
                foreach (BaseAbility baseAbility in newItem.MyLearnedAbilities) {
                    LearnAbility(baseAbility.MyName);
                }
            }
            if (oldItem != null) {
                foreach (BaseAbility baseAbility in oldItem.MyLearnedAbilities) {
                    UnlearnAbility(baseAbility.MyName);
                }
            }
        }


        public IEnumerator PerformAbilityCoolDown(string abilityName) {
            //Debug.Log(gameObject + ".CharacterAbilityManager.BeginAbilityCoolDown(" + abilityName + ") IENUMERATOR");

            yield return null;

            //Debug.Log(gameObject + ".BaseAbility.BeginAbilityCoolDown(): about to enter loop  IENUMERATOR");

            while (abilityCoolDownDictionary.ContainsKey(abilityName) && abilityCoolDownDictionary[abilityName].MyRemainingCoolDown > 0f) {
                abilityCoolDownDictionary[abilityName].MyRemainingCoolDown -= Time.deltaTime;
                //Debug.Log(gameObject.name + ".CharacterAbilityManager.PerformAbilityCooldown():  IENUMERATOR: " + abilityCoolDownDictionary[abilityName].MyRemainingCoolDown);
                yield return null;
            }
            if (abilityCoolDownDictionary.ContainsKey(abilityName)) {
                //Debug.Log(gameObject + ".CharacterAbilityManager.BeginAbilityCoolDown(" + abilityName + ") REMOVING FROM DICTIONARY");
                abilityCoolDownDictionary.Remove(abilityName);
            } else {
                //Debug.Log(gameObject + ".CharacterAbilityManager.BeginAbilityCoolDown(" + abilityName + ") WAS NOT IN DICTIONARY");
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

        public bool HasAbility(string abilityName) {
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
            //Debug.Log("CharacterAbilityManager.ActivateTargettingMode()");
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
            //Debug.Log("CharacterAbilityManager.SetGroundTarget(" + newGroundTarget + ")");
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
                //Debug.Log("CharacterAbilityManager.OnCharacterUnitSpawn(): CharacterMotor is null!");
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
            Debug.Log(gameObject.name + ".CharacterAbilityManager.LearnAbility()");
            string keyName = SystemResourceManager.prepareStringForMatch(abilityName);
            BaseAbility baseAbility = SystemAbilityManager.MyInstance.GetResource(abilityName);
            if (baseAbility == null) {
                Debug.Log(gameObject.name + ".CharacterAbilityManager.LearnAbility(): baseAbility is null");
            }
            if (!HasAbility(abilityName) && baseAbility.MyRequiredLevel <= MyBaseCharacter.MyCharacterStats.MyLevel) {
                abilityList[keyName] = baseAbility;
                return true;
            } else {
                if (HasAbility(abilityName)) {
                    Debug.Log(gameObject.name + ".CharacterAbilityManager.LearnAbility(): already had ability");
                }
                if (!(baseAbility.MyRequiredLevel <= MyBaseCharacter.MyCharacterStats.MyLevel)) {
                    Debug.Log(gameObject.name + ".CharacterAbilityManager.LearnAbility(): level is too low");
                }
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
            //Debug.Log(gameObject.name + "CharacterAbilitymanager.PerformAbilityCast(" + ability.MyName + ") Enter Ienumerator with tag: " + startTime);
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
                if (!ability.MyCanSimultaneousCast) {
                    //Debug.Log("CharacterAbilitymanager.PerformAbilityCast() ability: " + ability.MyName + " can simultaneous cast is false, setting casting to true");
                    ability.StartCasting(baseCharacter as BaseCharacter);
                }
                float currentCastTime = 0f;
                //Debug.Log("CharacterAbilitymanager.PerformAbilityCast() currentCastTime: " + currentCastTime + "; MyAbilityCastingTime: " + ability.MyAbilityCastingTime);

                if (baseCharacter != null && baseCharacter.MyCharacterEquipmentManager != null && ability.MyAbilityCastingTime > 0f && ability.MyHoldableObjectName != null && ability.MyHoldableObjectName != string.Empty) {
                    baseCharacter.MyCharacterEquipmentManager.SpawnAbilityObject(ability.MyHoldableObjectName);
                }
                if (ability.MyCastingAudioClip != null) {
                    AudioManager.MyInstance.PlayEffect(ability.MyCastingAudioClip);
                }
                while (currentCastTime < ability.MyAbilityCastingTime) {
                    currentCastTime += Time.deltaTime;

                    // call this first because it updates the cast bar
                    //Debug.Log("CharacterAbilitymanager.PerformAbilityCast() currentCastTime: " + currentCastTime + "; MyAbilityCastingTime: " + ability.MyAbilityCastingTime + "; calling OnCastTimeChanged()");
                    OnCastTimeChanged(ability, currentCastTime);

                    // now call the ability on casttime changed (really only here for channeled stuff to do damage)
                    ability.OnCastTimeChanged(currentCastTime, baseCharacter as BaseCharacter, target);

                    yield return null;
                }
                if (baseCharacter != null && baseCharacter.MyCharacterEquipmentManager != null) {
                    baseCharacter.MyCharacterEquipmentManager.DespawnAbilityObject();
                }

            }

            //Debug.Log(gameObject + ".CharacterAbilityManager.PerformAbilityCast(). nulling tag: " + startTime);
            // set currentCast to null because it isn't automatically null until the next frame and we are about to do stuff which requires it to be null immediately
            currentCast = null;

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
            //Debug.Log("CharacterAbilityManager.BeginAbilityCommon(" + (ability == null ? "null" : ability.MyName) + ", " + (target == null ? "null" : target.name) + ")");
            IAbility usedAbility = SystemAbilityManager.MyInstance.GetResource(ability.MyName);
            if (usedAbility == null) {
                Debug.LogError("CharacterAbilityManager.BeginAbilityCommon(" + (ability == null ? "null" : ability.MyName) + ", " + (target == null ? "null" : target.name) + ") NO ABILITY FOUND");
                return;
            }
            if (!CanCastAbility(ability)) {
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

        // this only checks if the ability is able to be cast based on character state.  It does not check validity of target or ability specific requirements
        public bool CanCastAbility(IAbility ability) {
            //Debug.Log(gameObject.name + ".CharacterAbilityManager.CanCastAbility(" + ability.MyName + ")");

            string keyName = SystemResourceManager.prepareStringForMatch(ability.MyName);

            // check if the ability is learned yet
            if (!ability.MyUseableWithoutLearning && !abilityList.ContainsKey(keyName)) {
                //Debug.Log("ability.MyUseableWithoutLearning: " + ability.MyUseableWithoutLearning + "; abilityList.Contains(" + keyName + "): " + abilityList.ContainsKey(keyName));
                return false;
            }

            // check if the ability is on cooldown
            if (abilityCoolDownDictionary.ContainsKey(ability.MyName) || MyRemainingGlobalCoolDown > 0f) {
                //CombatLogUI.MyInstance.WriteCombatMessage(ability.MyName + " is on cooldown: " + SystemAbilityManager.MyInstance.GetResource(ability.MyName).MyRemainingCoolDown);
                // write some common notify method here that only has content in it in playerabilitymanager to show messages so don't get spammed with npc messages
                //Debug.Log(gameObject.name + ".CharacterAbilityManager.CanCastAbility(" + ability.MyName + "): gcd: " + MyRemainingGlobalCoolDown + "; key in dictionary: " + abilityCoolDownDictionary.ContainsKey(ability.MyName));
                if (abilityCoolDownDictionary.ContainsKey(ability.MyName)) {
                    Debug.Log(abilityCoolDownDictionary[ability.MyName].MyRemainingCoolDown);
                }
                return false;
            }

            // check if we have enough mana
            if (MyBaseCharacter.MyCharacterStats.currentMana < ability.MyAbilityManaCost) {
                //CombatLogUI.MyInstance.WriteCombatMessage("Not enough mana to perform " + ability.MyName + " at a cost of " + ability.MyAbilityManaCost.ToString());
                //Debug.Log("not enough mana");
                return false;
            }

            // default is true, nothing has stopped us so far
            return true;
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
            // adding new code to require some movement distance to prevent gravity while standing still from triggering this
            if (MyBaseCharacter.MyCharacterController.MyApparentVelocity > 0.1f) {
                StopCasting();
            }
        }

        public virtual void StopCasting() {
            //Debug.Log(gameObject.name + ".CharacterAbilityManager.StopCasting()");
            // REMOVED ISCASTING == TRUE BECAUSE IT WAS PREVENTING THE CRAFTING QUEUE FROM WORKING.  TECHNICALLY THIS GOT CALLED RIGHT AFTER ISCASTING WAS SET TO FALSE, BUT BEFORE CURRENTCAST WAS NULLED
            if (currentCast != null) {
                //if (currentCast != null && isCasting == true) {
                //Debug.Log(gameObject.name + ".CharacterAbilityManager.StopCasting(): currentCast is not null, stopping coroutine");
                StopCoroutine(currentCast);
                currentCast = null;
                baseCharacter.MyCharacterEquipmentManager.DespawnAbilityObject();

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

}