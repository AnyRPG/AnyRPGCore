using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class CharacterAbilityManager : MonoBehaviour {

        public virtual event System.Action<BaseCharacter> OnAttack = delegate { };
        public event System.Action<IAbility, float> OnCastTimeChanged = delegate { };
        public event System.Action<BaseCharacter> OnCastStop = delegate { };

        protected BaseCharacter baseCharacter;

        protected Coroutine currentCastCoroutine = null;
        protected BaseAbility currentCastAbility = null;
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

        protected bool eventSubscriptionsInitialized = false;

        // we need a reference to the total length of the current global cooldown to properly calculate radial fill on the action buttons
        protected float initialGlobalCoolDown;

        public float MyInitialGlobalCoolDown { get => initialGlobalCoolDown; set => initialGlobalCoolDown = value; }

        public float MyRemainingGlobalCoolDown { get => remainingGlobalCoolDown; set => remainingGlobalCoolDown = value; }

        private bool waitingForAnimatedAbility = false;

        private Coroutine globalCoolDownCoroutine = null;

        public BaseCharacter MyBaseCharacter {
            get => baseCharacter;
            set => baseCharacter = value;
        }

        public Dictionary<string, IAbility> MyAbilityList { get => abilityList; }
        public bool MyWaitingForAnimatedAbility { get => waitingForAnimatedAbility; set => waitingForAnimatedAbility = value; }
        public bool MyIsCasting { get => isCasting; set => isCasting = value; }
        public Dictionary<string, AbilityCoolDownNode> MyAbilityCoolDownDictionary { get => abilityCoolDownDictionary; set => abilityCoolDownDictionary = value; }
        public Coroutine MyCurrentCastCoroutine { get => currentCastCoroutine; }

        protected virtual void Start() {
            //Debug.Log(gameObject.name + "CharacterAbilityManager.Start()");
            UpdateAbilityList(baseCharacter.MyCharacterStats.MyLevel);
            //CreateEventSubscriptions();
        }

        public void OrchestratorStart() {
            GetComponentReferences();
            CreateEventSubscriptions();
            //Debug.Log(gameObject.name + ".CharacterAbilityManager.OrchestratorStart()");
            /*
            if (AutoAttackKnown() == false) {
                Debug.Log(gameObject.name + ".CharacterAbilityManager.OrchestratorStart(): auto attack not known, learning auto attack");
                LearnAbility(SystemConfigurationManager.MyInstance.MyDefaultAutoAttackAbility);
            } else {
                Debug.Log(gameObject.name + ".CharacterAbilityManager.OrchestratorStart(): auto attack already known");
            }
            */
        }

        public void GetComponentReferences() {
            baseCharacter = GetComponent<BaseCharacter>();
        }

        public virtual void CreateEventSubscriptions() {
            //Debug.Log(gameObject.name + "CharacterAbilityManager.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized) {
                return;
            }
            baseCharacter.MyCharacterCombat.OnKillEvent += ReceiveKillDetails;
            baseCharacter.OnClassChange += HandleClassChange;
            SystemEventManager.MyInstance.OnLevelUnload += HandleLevelUnload;
            if (baseCharacter != null && baseCharacter.MyCharacterStats != null) {
                baseCharacter.MyCharacterStats.OnDie += OnDieHandler;
            }
            if (baseCharacter != null && baseCharacter.MyCharacterEquipmentManager != null) {
                //Debug.Log(gameObject.name + ".CharacterAbilityManager.CreateEventSubscriptions(): subscribing to onequipmentchanged");
                baseCharacter.MyCharacterEquipmentManager.OnEquipmentChanged += HandleEquipmentChanged;
            } else {
                //Debug.Log(gameObject.name + ".CharacterAbilityManager.CreateEventSubscriptions(): could not subscribe to ONEQUIPMENTCHANGED");
            }
            eventSubscriptionsInitialized = true;
        }

        public virtual void CleanupEventSubscriptions() {
            if (!eventSubscriptionsInitialized) {
                return;
            }
            if (SystemEventManager.MyInstance != null) {
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
            HandleCharacterUnitDespawn();
            eventSubscriptionsInitialized = false;
        }

        public virtual void OnDisable() {
            CleanupEventSubscriptions();
            CleanupCoroutines();
        }

        public virtual void CleanupCoroutines() {
            //Debug.Log(gameObject.name + ".CharacterAbilitymanager.CleanupCoroutines()");
            if (currentCastCoroutine != null) {
                StopCoroutine(currentCastCoroutine);
                EndCastCleanup();
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

            if (globalCoolDownCoroutine != null) {
                StopCoroutine(globalCoolDownCoroutine);
                globalCoolDownCoroutine = null;
            }

        }

        public void BeginAbilityCoolDown(BaseAbility baseAbility) {
            float abilityCoolDown = baseAbility.abilityCoolDown;
            if (abilityCoolDown == 0f) {
                // no point making a cooldown if it is zero length
                return;
            }
            AbilityCoolDownNode abilityCoolDownNode = new AbilityCoolDownNode();
            abilityCoolDownNode.MyAbilityName = baseAbility.MyName;
            abilityCoolDownNode.MyRemainingCoolDown = abilityCoolDown;

            if (!abilityCoolDownDictionary.ContainsKey(baseAbility.MyName)) {
                abilityCoolDownDictionary[baseAbility.MyName] = abilityCoolDownNode;
            }

            // ordering important.  don't start till after its in the dictionary or it will fail to remove itself from the dictionary, then add it self
            Coroutine coroutine = StartCoroutine(PerformAbilityCoolDown(baseAbility.MyName));
            abilityCoolDownNode.MyCoroutine = coroutine;

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
            //Debug.Log(gameObject.name + ".CharacterAbilityManager.HandleEquipmentChanged(" + (newItem != null ? newItem.MyName : "null") + ", " + (oldItem != null ? oldItem.MyName : "null") + ")");
            if (oldItem != null) {
                foreach (BaseAbility baseAbility in oldItem.MyLearnedAbilities) {
                    UnlearnAbility(baseAbility.MyName);
                }
            }

            if (newItem != null) {
                if (newItem.MyOnEquipAbility != null) {
                    if (baseCharacter.MyCharacterUnit != null) {
                        BeginAbility(newItem.MyOnEquipAbility);
                    }
                }
                foreach (BaseAbility baseAbility in newItem.MyLearnedAbilities) {
                    if (baseAbility is AnimatedAbility && (baseAbility as AnimatedAbility).MyIsAutoAttack == true) {
                        UnLearnDefaultAutoAttackAbility();
                    }
                    LearnAbility(baseAbility.MyName);
                }
            }
            LearnDefaultAutoAttackAbility();
        }

        public virtual void UnLearnDefaultAutoAttackAbility() {
            if (baseCharacter != null && baseCharacter.MyUnitProfile != null && baseCharacter.MyUnitProfile.MyDefaultAutoAttackAbility != null && baseCharacter.MyUnitProfile.MyDefaultAutoAttackAbility != string.Empty) {
                UnlearnAbility(baseCharacter.MyUnitProfile.MyDefaultAutoAttackAbility);
            }
        }

        public virtual void LearnDefaultAutoAttackAbility() {
            if (AutoAttackKnown() == true) {
                // can't learn two auto-attacks at the same time
                return;
            }
            if (baseCharacter != null && baseCharacter.MyUnitProfile != null && baseCharacter.MyUnitProfile.MyDefaultAutoAttackAbility != null && baseCharacter.MyUnitProfile.MyDefaultAutoAttackAbility != string.Empty) {
                LearnAbility(baseCharacter.MyUnitProfile.MyDefaultAutoAttackAbility);
            }
        }

        public void HandleClassChange(CharacterClass newCharacterClass, CharacterClass oldCharacterClass) {
            RemoveClassTraits(oldCharacterClass);
            UnLearnClassAbilities(oldCharacterClass);
            LearnClassAbilities(newCharacterClass);
            ApplyClassTraits(newCharacterClass);
        }

        public void ApplyClassTraits(CharacterClass newCharacterClass) {
            //Debug.Log(gameObject.name + ".CharacterAbilityManager.ApplyClassTraits(" + (newCharacterClass == null ? "null" : newCharacterClass.MyName) + ")");
            if (newCharacterClass != null && newCharacterClass.MyTraitList != null && newCharacterClass.MyTraitList.Count > 0) {
                foreach (string classTrait in newCharacterClass.MyTraitList) {
                    //Debug.Log(gameObject.name + ".CharacterAbilityManager.ApplyClassTraits(" + (newCharacterClass == null ? "null" : newCharacterClass.MyName) + "): trait: " + classTrait);
                    ApplyStatusEffect(classTrait);
                }
            }
        }

        public void ApplyStatusEffect(string statusEffectname, int overrideDuration = 0) {
            if (baseCharacter.MyCharacterStats != null) {
                AbilityEffectOutput abilityEffectOutput = new AbilityEffectOutput();
                abilityEffectOutput.overrideDuration = overrideDuration;
                // rememeber this method is meant for saved status effects
                abilityEffectOutput.savedEffect = true;
                AbilityEffect _abilityEffect = SystemAbilityEffectManager.MyInstance.GetResource(statusEffectname);
                if (_abilityEffect != null) {
                    _abilityEffect.Cast(baseCharacter, null, null, abilityEffectOutput);
                }
            }

        }

        public void ApplySavedStatusEffects(StatusEffectSaveData statusEffectSaveData) {
            ApplyStatusEffect(statusEffectSaveData.MyName, statusEffectSaveData.remainingSeconds);
        }

        public void RemoveClassTraits(CharacterClass oldCharacterClass) {
            if (oldCharacterClass !=null && oldCharacterClass.MyTraitList != null && oldCharacterClass.MyTraitList.Count > 0) {
                foreach (string classTrait in oldCharacterClass.MyTraitList) {
                    if (baseCharacter.MyCharacterStats != null && baseCharacter.MyCharacterStats.MyStatusEffects.ContainsKey(SystemResourceManager.prepareStringForMatch(classTrait))) {
                        baseCharacter.MyCharacterStats.MyStatusEffects[SystemResourceManager.prepareStringForMatch(classTrait)].CancelStatusEffect();
                    }
                }
            }
        }

        public void LearnClassAbilities(CharacterClass characterClass) {
            //Debug.Log(gameObject.name + ".PlayerCharacter.LearnFactionAbilities(" + newFaction + ")");
            if (characterClass == null) {
                return;
            }
            foreach (string abilityName in characterClass.MyAbilityList) {
                //Debug.Log(gameObject.name + ".PlayerCharacter.LearnFactionAbilities(" + newFaction + "); ability name: " + abilityName);
                if (SystemAbilityManager.MyInstance.GetResource(abilityName).MyRequiredLevel <= PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyLevel && PlayerManager.MyInstance.MyCharacter.MyCharacterAbilityManager.HasAbility(abilityName) == false) {
                    //Debug.Log(gameObject.name + ".PlayerCharacter.LearnFactionAbilities(" + newFaction + "); ability name: " + abilityName + " is not learned yet, LEARNING!");
                    PlayerManager.MyInstance.MyCharacter.MyCharacterAbilityManager.LearnAbility(abilityName);
                } else {
                    //Debug.Log(gameObject.name + ".PlayerCharacter.LearnFactionAbilities(" + newFaction + "); ability name: " + abilityName + "; level: " + SystemAbilityManager.MyInstance.GetResource(abilityName).MyRequiredLevel + "; playerlevel: " + PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyLevel + "; hasability: " + (PlayerManager.MyInstance.MyCharacter.MyCharacterAbilityManager.HasAbility(abilityName)));
                }
            }
        }

        public void UnLearnClassAbilities (CharacterClass characterClass) {
            if (characterClass == null) {
                return;
            }
            foreach (string abilityName in characterClass.MyAbilityList) {
                UnlearnAbility(abilityName);
            }
        }


        public IEnumerator PerformAbilityCoolDown(string abilityName) {
            //Debug.Log(gameObject + ".CharacterAbilityManager.BeginAbilityCoolDown(" + abilityName + ") IENUMERATOR");

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

        public virtual void HandleCharacterUnitSpawn() {
            //Debug.Log("CharacterAbilityManager.OnCharacterUnitSpawn()");

            if (MyBaseCharacter.MyAnimatedUnit.MyCharacterMotor != null) {
                //Debug.Log("CharacterAbilityManager.OnCharacterUnitSpawn(): CharacterMotor is not null");
                MyBaseCharacter.MyAnimatedUnit.MyCharacterMotor.OnMovement += HandleManualMovement;
            } else {
                //Debug.Log("CharacterAbilityManager.OnCharacterUnitSpawn(): CharacterMotor is null!");
            }
        }

        public virtual void HandleCharacterUnitDespawn() {
            //Debug.Log(gameObject.name + ".CharacterAbilityManager.OnCharacterUnitDespawn()");
        }

        public virtual void UpdateAbilityList(int newLevel) {
            //Debug.Log(gameObject.name + ".CharacterAbilityManager.UpdateAbilityList(). length: " + abilityList.Count);
        }

        public virtual bool LearnAbility(string abilityName) {
            //Debug.Log(gameObject.name + ".CharacterAbilityManager.LearnAbility()");
            string keyName = SystemResourceManager.prepareStringForMatch(abilityName);
            BaseAbility baseAbility = SystemAbilityManager.MyInstance.GetResource(abilityName);
            if (baseAbility == null) {
                //Debug.Log(gameObject.name + ".CharacterAbilityManager.LearnAbility(): baseAbility is null");
                // can't learn a nonexistent ability
                return false;
            }
            if (!HasAbility(abilityName) && baseAbility.MyRequiredLevel <= MyBaseCharacter.MyCharacterStats.MyLevel) {
                abilityList[keyName] = baseAbility;
                return true;
            } else {
                if (HasAbility(abilityName)) {
                    //Debug.Log(gameObject.name + ".CharacterAbilityManager.LearnAbility(): already had ability");
                }
                if (!(baseAbility.MyRequiredLevel <= MyBaseCharacter.MyCharacterStats.MyLevel)) {
                    //Debug.Log(gameObject.name + ".CharacterAbilityManager.LearnAbility(): level is too low");
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

                if (baseCharacter != null && baseCharacter.MyCharacterEquipmentManager != null && ability.MyHoldableObjectNames.Count != 0) {
                    //if (baseCharacter != null && baseCharacter.MyCharacterEquipmentManager != null && ability.MyAbilityCastingTime > 0f && ability.MyHoldableObjectNames.Count != 0) {
                    //Debug.Log(gameObject.name + ".CharacterAbilityManager.PerformAbilityCast(" + ability.MyName + "): spawning ability objects");
                    if (!ability.MyAnimatorCreatePrefabs) {
                        baseCharacter.MyCharacterEquipmentManager.SpawnAbilityObject(ability.MyHoldableObjectNames);
                    }
                }
                if (ability.MyCastingAudioClip != null) {
                    //AudioManager.MyInstance.PlayEffect(ability.MyCastingAudioClip);
                    //baseCharacter.MyCharacterUnit.MyAudioSource.PlayOneShot(ability.MyCastingAudioClip);
                    baseCharacter.MyCharacterUnit.MyAudioSource.clip = ability.MyCastingAudioClip;
                    baseCharacter.MyCharacterUnit.MyAudioSource.Play();
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
                /*
                if (baseCharacter != null && baseCharacter.MyCharacterEquipmentManager != null) {
                    baseCharacter.MyCharacterEquipmentManager.DespawnAbilityObjects();
                }
                */

            }

            //Debug.Log(gameObject + ".CharacterAbilityManager.PerformAbilityCast(). nulling tag: " + startTime);
            // set currentCast to null because it isn't automatically null until the next frame and we are about to do stuff which requires it to be null immediately
            EndCastCleanup();

            if (canCast) {
                //Debug.Log(gameObject.name + ".CharacterAbilitymanager.PerformAbilityCast(): Cast Complete currentCastTime: " + currentCastTime + "; abilitycastintime: " + ability.MyAbilityCastingTime);
                if (!ability.MyCanSimultaneousCast) {
                    OnCastStop(MyBaseCharacter as BaseCharacter);
                    MyBaseCharacter.MyAnimatedUnit.MyCharacterAnimator.SetCasting(false);
                }
                PerformAbility(ability, target, GetGroundTarget());

            }
        }

        public void SpawnAbilityObjects() {
            BaseAbility usedBaseAbility = null;
            if (MyBaseCharacter.MyAnimatedUnit.MyCharacterAnimator.MyCurrentAbility != null) {
                usedBaseAbility = MyBaseCharacter.MyAnimatedUnit.MyCharacterAnimator.MyCurrentAbility;
            }
            if (usedBaseAbility == null) {
                usedBaseAbility = currentCastAbility;
            }

            if (baseCharacter != null && baseCharacter.MyCharacterEquipmentManager != null && usedBaseAbility.MyHoldableObjectNames.Count != 0) {
                //if (baseCharacter != null && baseCharacter.MyCharacterEquipmentManager != null && ability.MyAbilityCastingTime > 0f && ability.MyHoldableObjectNames.Count != 0) {
                //Debug.Log(gameObject.name + ".CharacterAbilityManager.PerformAbilityCast(): spawning ability objects");
                if (usedBaseAbility.MyAnimatorCreatePrefabs) {
                    baseCharacter.MyCharacterEquipmentManager.SpawnAbilityObject(usedBaseAbility.MyHoldableObjectNames);
                }
            }

        }

        public void EndCastCleanup() {
            currentCastCoroutine = null;
            currentCastAbility = null;
            baseCharacter.MyCharacterUnit.MyAudioSource.Stop();
        }

        public void ReceiveKillDetails(BaseCharacter killedcharacter, float creditPercent) {
            //Debug.Log("CharacterAbilityManager.ReceiveKillDetails()");
            if (MyBaseCharacter.MyCharacterController.MyTarget == killedcharacter.MyCharacterUnit.gameObject) {
                if (killStopCast) {
                    StopCasting();
                }
            }
        }

        public void AttemptAutoAttack() {
            //Debug.Log(gameObject.name + ".CharacterAbilitymanager.AttemtpAutoAttack()");
            foreach (BaseAbility baseAbility in MyAbilityList.Values) {
                if (baseAbility is AnimatedAbility && (baseAbility as AnimatedAbility).MyIsAutoAttack) {
                    BeginAbility(baseAbility);
                }
            }
        }

        /// <summary>
        /// The entrypoint to Casting a spell.  handles all logic such as instant/timed cast, current cast in progress, enough mana, target being alive etc
        /// </summary>
        /// <param name="ability"></param>
        public void BeginAbility(IAbility ability) {
            //Debug.Log(gameObject.name + "CharacterAbilitymanager.BeginAbility(" + (ability == null ? "null" : ability.MyName) + ")");
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
            //Debug.Log(gameObject.name + ".CharacterAbilityManager.BeginAbilityCommon(" + (ability == null ? "null" : ability.MyName) + ", " + (target == null ? "null" : target.name) + ")");
            IAbility usedAbility = SystemAbilityManager.MyInstance.GetResource(ability.MyName);
            if (usedAbility == null) {
                Debug.LogError("CharacterAbilityManager.BeginAbilityCommon(" + (ability == null ? "null" : ability.MyName) + ", " + (target == null ? "null" : target.name) + ") NO ABILITY FOUND");
                return;
            }

            if (!CanCastAbility(ability)) {
                //Debug.Log("ability.CanUseOn(" + ability.MyName + ", " + (target != null ? target.name : "null") + ") cannot cast");
                return;
            }

            CharacterUnit targetCharacterUnit = null;
            if (target != null) {
                targetCharacterUnit = target.GetComponent<CharacterUnit>();
            }
            if (targetCharacterUnit != null && targetCharacterUnit.MyBaseCharacter != null) {
                if (Faction.RelationWith(targetCharacterUnit.MyBaseCharacter, baseCharacter) <= -1) {
                    if (targetCharacterUnit.MyBaseCharacter.MyCharacterCombat != null && ability.MyCanCastOnEnemy == true) {
                        // agro includes a liveness check, so casting necromancy on a dead enemy unit should not pull it into combat with us if we haven't applied a faction or master control buff yet
                        if (baseCharacter.MyCharacterCombat.GetInCombat() == false) {
                            baseCharacter.MyCharacterCombat.EnterCombat(targetCharacterUnit.MyCharacter);
                        }
                        baseCharacter.MyCharacterCombat.ActivateAutoAttack();
                        OnAttack(targetCharacterUnit.MyBaseCharacter);
                    }
                }
            }

            NotifyAttemptPerformAbility(ability);

            // get final target before beginning casting
            GameObject finalTarget = usedAbility.ReturnTarget(baseCharacter as BaseCharacter, target);

            if (finalTarget == null && usedAbility.MyRequiresTarget == true) {
                return;
            }

            if (usedAbility.MyCanSimultaneousCast) {
                // directly performing to avoid interference with other abilities being casted
                PerformAbility(usedAbility, finalTarget, GetGroundTarget());
            } else {
                if (currentCastCoroutine == null) {
                    //Debug.Log("Performing Ability " + ability.MyName + " at a cost of " + ability.MyAbilityManaCost.ToString() + ": ABOUT TO START COROUTINE");

                    // we need to do this because we are allowed to stop an outstanding auto-attack to start this cast
                    MyBaseCharacter.MyAnimatedUnit.MyCharacterAnimator.ClearAnimationBlockers();

                    // start the cast (or cast targetting projector)
                    currentCastCoroutine = StartCoroutine(PerformAbilityCast(usedAbility, finalTarget));
                    currentCastAbility = usedAbility as BaseAbility;
                } else {
                    //CombatLogUI.MyInstance.WriteCombatMessage("A cast was already in progress WE SHOULD NOT BE HERE BECAUSE WE CHECKED FIRST! iscasting: " + isCasting + "; currentcast==null? " + (currentCast == null));
                    // unless.... we got here from the crafting queue, which launches the next item as the last step of the currently in progress cast
                    //Debug.Log("A cast was already in progress!");
                }
            }
        }

        public virtual void NotifyAttemptPerformAbility(IAbility ability) {
            //OnPerformAbility(ability);
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
            if (abilityCoolDownDictionary.ContainsKey(ability.MyName) || (MyRemainingGlobalCoolDown > 0f && ability.MyIgnoreGlobalCoolDown == false)) {
                //CombatLogUI.MyInstance.WriteCombatMessage(ability.MyName + " is on cooldown: " + SystemAbilityManager.MyInstance.GetResource(ability.MyName).MyRemainingCoolDown);
                // write some common notify method here that only has content in it in playerabilitymanager to show messages so don't get spammed with npc messages
                //Debug.Log(gameObject.name + ".CharacterAbilityManager.CanCastAbility(" + ability.MyName + "): gcd: " + MyRemainingGlobalCoolDown + "; key in dictionary: " + abilityCoolDownDictionary.ContainsKey(ability.MyName));
                //if (abilityCoolDownDictionary.ContainsKey(ability.MyName)) {
                    //Debug.Log(abilityCoolDownDictionary[ability.MyName].MyRemainingCoolDown);
                //}
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
                // GET RID OF CASTING PREFABS HERE
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
        public void HandleManualMovement() {
            //Debug.Log(gameObject.name + ".CharacterAbilityManager.HandleManualMovement(): Received On Manual Movement Handler");
            // adding new code to require some movement distance to prevent gravity while standing still from triggering this
            if (MyBaseCharacter.MyCharacterController.MyApparentVelocity > 0.1f) {
                //Debug.Log("CharacterAbilityManager.HandleManualMovement(): stop casting");
                StopCasting();
            } else {
                //Debug.Log("CharacterAbilityManager.HandleManualMovement(): velocity too low, doing nothing");
            }
        }

        public virtual void StopCasting() {
            //Debug.Log(gameObject.name + ".CharacterAbilityManager.StopCasting()");
            // REMOVED ISCASTING == TRUE BECAUSE IT WAS PREVENTING THE CRAFTING QUEUE FROM WORKING.  TECHNICALLY THIS GOT CALLED RIGHT AFTER ISCASTING WAS SET TO FALSE, BUT BEFORE CURRENTCAST WAS NULLED
            if (currentCastCoroutine != null) {
                //if (currentCast != null && isCasting == true) {
                //Debug.Log(gameObject.name + ".CharacterAbilityManager.StopCasting(): currentCast is not null, stopping coroutine");
                StopCoroutine(currentCastCoroutine);
                EndCastCleanup();
                baseCharacter.MyCharacterEquipmentManager.DespawnAbilityObjects();

            } else {
                //Debug.Log(gameObject.name + ".currentCast is null, nothing to stop");
            }
            if (MyBaseCharacter.MyAnimatedUnit != null) {
                MyBaseCharacter.MyAnimatedUnit.MyCharacterAnimator.ClearAnimationBlockers();
            }
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

            CharacterStats targetStats = null;
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

        public void AnimationHitAnimationEvent() {
            if (currentCastAbility != null) {
                if (currentCastAbility.MyAnimationHitAudioClip != null) {
                    //AudioManager.MyInstance.PlayEffect(ability.MyCastingAudioClip);
                    baseCharacter.MyCharacterUnit.MyAudioSource.PlayOneShot(currentCastAbility.MyAnimationHitAudioClip);
                }

            }
        }

        public void InitiateGlobalCooldown(float coolDownToUse = 0f) {
            //Debug.Log(gameObject.name + ".PlayerAbilitymanager.InitiateGlobalCooldown(" + ability.MyName + ")");
            if (globalCoolDownCoroutine == null) {
                // set global cooldown length to animation length so we don't end up in situation where cast bars look fine, but we can't actually cast
                globalCoolDownCoroutine = StartCoroutine(BeginGlobalCoolDown(coolDownToUse));
            } else {
                Debug.Log(gameObject.name + ".CharacterAbilityManager.InitiateGlobalCooldown(): INVESTIGATE: GCD COROUTINE WAS NOT NULL");
            }

        }

        public IEnumerator BeginGlobalCoolDown(float coolDownTime) {
            //Debug.Log(gameObject.name + ".PlayerAbilityManager.BeginGlobalCoolDown()");
            // 10 is kinda arbitrary, but if any animation is causing a GCD greater than 10 seconds, we've probably got issues anyway...
            // the current longest animated attack is ground slam at around 4 seconds
            remainingGlobalCoolDown = Mathf.Clamp(coolDownTime, 1, 10);
            initialGlobalCoolDown = remainingGlobalCoolDown;
            while (remainingGlobalCoolDown > 0f) {
                remainingGlobalCoolDown -= Time.deltaTime;
                //Debug.Log("BaseAbility.BeginAbilityCooldown():" + MyName + ". time: " + remainingCoolDown);
                yield return null;
            }
            globalCoolDownCoroutine = null;
        }

        public bool AutoAttackKnown() {
            foreach (BaseAbility baseAbility in abilityList.Values) {
                if (baseAbility is AnimatedAbility && (baseAbility as AnimatedAbility).MyIsAutoAttack == true) {
                    return true;
                }
            }
            return false;
        }

    }

}