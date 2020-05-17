using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class CharacterAbilityManager : AbilityManager, IAbilityCaster {

        public virtual event System.Action<BaseCharacter> OnAttack = delegate { };
        public event System.Action<IAbility, float> OnCastTimeChanged = delegate { };
        public event System.Action<BaseCharacter> OnCastStop = delegate { };

        protected BaseCharacter baseCharacter;

        protected Dictionary<string, IAbility> abilityList = new Dictionary<string, IAbility>();

        protected bool isCasting = false;

        private Vector3 groundTarget = Vector3.zero;

        private bool targettingModeActive = false;

        // does killing the player you are currently targetting stop your cast.  gets set to false when channeling aoe.
        private bool killStopCast = true;

        protected float remainingGlobalCoolDown = 0f;

        // we need a reference to the total length of the current global cooldown to properly calculate radial fill on the action buttons
        protected float initialGlobalCoolDown;


        public float MyInitialGlobalCoolDown { get => initialGlobalCoolDown; set => initialGlobalCoolDown = value; }

        public float MyRemainingGlobalCoolDown { get => remainingGlobalCoolDown; set => remainingGlobalCoolDown = value; }

        private bool waitingForAnimatedAbility = false;


        public BaseCharacter BaseCharacter {
            get => baseCharacter;
            set => baseCharacter = value;
        }

        public GameObject UnitGameObject {
            get {
                if (baseCharacter != null && baseCharacter.CharacterUnit != null) {
                    return baseCharacter.CharacterUnit.gameObject;
                }
                return null;
            }
        }

        public bool PerformingAbility {
            get {
                if (WaitingForAnimatedAbility == true) {
                    return true;
                }
                if (IsCasting == true) {
                    return true;
                }
                return false;
            }
        }

        public int Level {
            get {
                if (baseCharacter != null && baseCharacter.CharacterStats != null) {
                    return baseCharacter.CharacterStats.Level;
                }
                return 1;
            }
        }

        public string Name {
            get {
                if (baseCharacter.CharacterName != null) {
                    return baseCharacter.CharacterName;
                }
                return gameObject.name;
            }
        }

        public Dictionary<string, IAbility> MyAbilityList { get => abilityList; }
        public bool WaitingForAnimatedAbility { get => waitingForAnimatedAbility; set => waitingForAnimatedAbility = value; }
        public bool IsCasting { get => isCasting; set => isCasting = value; }
        public Dictionary<string, AbilityCoolDownNode> MyAbilityCoolDownDictionary { get => abilityCoolDownDictionary; set => abilityCoolDownDictionary = value; }
        public Coroutine MyCurrentCastCoroutine { get => currentCastCoroutine; }

        protected virtual void Start() {
            //Debug.Log(gameObject.name + "CharacterAbilityManager.Start()");
            UpdateAbilityList(baseCharacter.CharacterStats.Level);
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
            baseCharacter.CharacterCombat.OnKillEvent += ReceiveKillDetails;
            baseCharacter.OnClassChange += HandleClassChange;
            baseCharacter.OnSpecializationChange += HandleSpecializationChange;
            SystemEventManager.StartListening("OnLevelUnload", HandleLevelUnload);
            if (baseCharacter != null && baseCharacter.CharacterStats != null) {
                baseCharacter.CharacterStats.OnDie += OnDieHandler;
            }
            if (baseCharacter != null && baseCharacter.CharacterEquipmentManager != null) {
                //Debug.Log(gameObject.name + ".CharacterAbilityManager.CreateEventSubscriptions(): subscribing to onequipmentchanged");
                baseCharacter.CharacterEquipmentManager.OnEquipmentChanged += HandleEquipmentChanged;
            } else {
                //Debug.Log(gameObject.name + ".CharacterAbilityManager.CreateEventSubscriptions(): could not subscribe to ONEQUIPMENTCHANGED");
            }
            eventSubscriptionsInitialized = true;
        }

        public override void CleanupEventSubscriptions() {
            if (!eventSubscriptionsInitialized) {
                return;
            }
            base.CleanupEventSubscriptions();
            if (SystemEventManager.MyInstance != null) {
                SystemEventManager.StopListening("OnLevelUnload", HandleLevelUnload);
            }
            if (baseCharacter != null && baseCharacter.CharacterCombat != null) {
                baseCharacter.CharacterCombat.OnKillEvent -= ReceiveKillDetails;
            }
            if (baseCharacter != null && baseCharacter.CharacterStats != null) {
                baseCharacter.CharacterStats.OnDie -= OnDieHandler;
            }
            if (baseCharacter != null && baseCharacter.CharacterEquipmentManager != null) {
                baseCharacter.CharacterEquipmentManager.OnEquipmentChanged -= HandleEquipmentChanged;
            }
            HandleCharacterUnitDespawn();
        }

        public override void OnDisable() {
            base.OnDisable();
        }

        public override float GetMeleeRange() {
            if (baseCharacter != null && baseCharacter.CharacterUnit != null) {
                return baseCharacter.CharacterUnit.HitBoxSize;
            }
            return base.GetMeleeRange();
        }

        /// <summary>
        /// Return false if the target does not meet the faction requirements
        /// </summary>
        /// <param name="baseAbility"></param>
        /// <param name="targetCharacterUnit"></param>
        /// <param name="targetIsSelf"></param>
        /// <returns></returns>
        public bool PerformFactionCheck(ITargetable targetableEffect, CharacterUnit targetCharacterUnit, bool targetIsSelf) {

            if (Faction.RelationWith(targetCharacterUnit.MyCharacter, baseCharacter) <= -1) {
                //targetIsEnemy = true;
                if (!targetableEffect.CanCastOnEnemy) {
                    //Debug.Log(MyName + ": Can't cast on enemy. return false");
                    return false;
                }
            }

            // this if statement is needed because self will return as a friendly target
            if (!targetIsSelf) {
                if (Faction.RelationWith(targetCharacterUnit.MyCharacter, baseCharacter) >= 0) {
                    //targetIsFriendly = true;
                    if (!targetableEffect.CanCastOnFriendly) {
                        //Debug.Log(MyName + ": Can't cast on friendly. return false");
                        return false;
                    }
                }
            }

            return true;
        }

        public override float GetThreatModifiers() {
            if (baseCharacter != null && baseCharacter.CharacterStats != null) {
                return baseCharacter.CharacterStats.GetThreatModifiers();
            }
            return base.GetThreatModifiers();
        }

        public override bool AddToAggroTable(CharacterUnit targetCharacterUnit, int usedAgroValue) {
            // intentionally don't call the base
            if (baseCharacter.CharacterStats.IsAlive) {
                return targetCharacterUnit.MyCharacter.CharacterCombat.MyAggroTable.AddToAggroTable(baseCharacter.CharacterUnit, usedAgroValue);
            }
            return false;
        }

        public override void GenerateAgro(CharacterUnit targetCharacterUnit, int usedAgroValue) {
            base.GenerateAgro(targetCharacterUnit, usedAgroValue);
            if (baseCharacter != null && baseCharacter.CharacterUnit != null) {
                AddToAggroTable(baseCharacter.CharacterUnit, usedAgroValue);
                //AgroNode = targetCharacterUnit.MyCharacter.MyCharacterCombat.MyAggroTable.MyTopAgroNode;
                //Debug.Log("StatusEffect.Cast(" + source.name + ", " + (target ? target.name : "null") + ") topNode agro value: " + AgroNode.aggroValue + "; target: " + AgroNode.aggroTarget.MyName);
                targetCharacterUnit.MyCharacter.CharacterCombat.MyAggroTable.LockAgro();
            }

        }

        public override void PerformCastingAnimation(AnimationClip animationClip, BaseAbility baseAbility) {
            base.PerformCastingAnimation(animationClip, baseAbility);
            if (animationClip != null) {
                baseCharacter.AnimatedUnit.MyCharacterAnimator.HandleCastingAbility(animationClip, baseAbility);
            }
        }

        public override void CapturePet(UnitProfile unitProfile, GameObject target) {
            base.CapturePet(unitProfile, target);
            if (baseCharacter.MyCharacterPetManager != null&& unitProfile != null) {
                //Debug.Log(gameObject.name + ".CapturePet(): adding to pet manager");
                baseCharacter.MyCharacterPetManager.AddPet(unitProfile);
                baseCharacter.MyCharacterPetManager.MyActiveUnitProfiles.Add(unitProfile, target);
            }
        }

        public bool IsTargetInAbilityRange(BaseAbility baseAbility, GameObject target) {
            // if none of those is true, then we are casting on ourselves, so don't need to do range check
            return IsTargetInRange(target, baseAbility.UseMeleeRange, baseAbility.MaxRange, baseAbility);
        }

        public bool IsTargetInAbilityEffectRange(AbilityEffect abilityEffect, GameObject target) {
            // if none of those is true, then we are casting on ourselves, so don't need to do range check
            return IsTargetInRange(target, abilityEffect.UseMeleeRange, abilityEffect.MaxRange, abilityEffect);
        }

        public bool IsTargetInMeleeRange(GameObject target) {
            return baseCharacter.CharacterController.IsTargetInHitBox(target);
        }

        public override bool PerformLOSCheck(GameObject target, ITargetable targetable) {

            if (targetable.RequireLineOfSight == false) {
                return true;
            }

            // get initial positions in case of no collider
            Vector3 sourcePosition = transform.position;
            Vector3 targetPosition = target.transform.position;

            Collider sourceCollider = GetComponent<Collider>();
            if (sourceCollider != null) {
                sourcePosition = sourceCollider.bounds.center;
            }

            Collider targetCollider = target.GetComponent<Collider>();
            if (targetCollider != null) {
                targetPosition = targetCollider.bounds.center;
            }

            Debug.DrawLine(sourcePosition, targetPosition, Color.cyan);
            RaycastHit wallHit = new RaycastHit();

            int targetMask = 1 << target.layer;
            int defaultMask = 1 << LayerMask.NameToLayer("Default");

            int layerMask = (defaultMask | targetMask);

            if (Physics.Linecast(sourcePosition, targetPosition, out wallHit, layerMask)) {
                //Debug.Log("hit: " + wallHit.transform.name);
                Debug.DrawRay(wallHit.point, wallHit.point - targetPosition, Color.red);
                if (wallHit.collider.gameObject != target) {
                    //Debug.Log("return false; hit: " + wallHit.collider.gameObject + "; target: " + target);
                    return false;
                }
            }
            //Debug.Log(gameObject.name + ".PerformLOSCheck(): return true;");
            return base.PerformLOSCheck(target, targetable);
        }

        public bool IsTargetInRange(GameObject target, bool useMeleeRange, float maxRange, ITargetable targetable) {
            // if none of those is true, then we are casting on ourselves, so don't need to do range check

            if (useMeleeRange) {
                if (!IsTargetInMeleeRange(target)) {
                    return false;
                }
            } else {
                if (!IsTargetInMaxRange(target, maxRange, targetable)) {
                    return false;
                }
                if (!PerformLOSCheck(target, targetable)) {
                    return false;
                }
            }
            return true;
        }

        public virtual bool IsTargetInMaxRange(GameObject target, float maxRange, ITargetable targetable) {
            if (maxRange > 0 && Vector3.Distance(UnitGameObject.transform.position, target.transform.position) > maxRange) {
                //Debug.Log(target.name + " is out of range");
                return false;
            }

            return true;
        }

        public void HandleLevelUnload(string eventName, EventParamProperties eventParamProperties) {
            ProcessLevelUnload();
        }

        public float PerformAnimatedAbility(AnimationClip animationClip, AnimatedAbility animatedAbility, BaseCharacter targetBaseCharacter) {
            // this type of ability is allowed to interrupt other types of animations, so clear them all
            baseCharacter.AnimatedUnit.MyCharacterAnimator.ClearAnimationBlockers();

            // now block further animations of other types from starting
            if (!animatedAbility.IsAutoAttack) {
                baseCharacter.CharacterAbilityManager.WaitingForAnimatedAbility = true;
            } else {
                baseCharacter.CharacterCombat.SetWaitingForAutoAttack(true);
            }
            return baseCharacter.AnimatedUnit.MyCharacterAnimator.HandleAbility(animationClip, animatedAbility, targetBaseCharacter);
        }

        /// <summary>
        /// Return true if the ability hit, false if it missed
        /// </summary>
        /// <returns></returns>
        public bool AbilityHit(GameObject target) {
            if (baseCharacter.CharacterCombat.DidAttackMiss() == true) {
                //Debug.Log(MyName + ".BaseAbility.PerformAbilityHit(" + source.name + ", " + target.name + "): attack missed");
                baseCharacter.CharacterCombat.ReceiveCombatMiss(target);
                return false;
            }
            return true;
        }

        public virtual bool PerformAnimatedAbilityCheck(AnimatedAbility animatedAbility) {
            if (WaitingForAnimatedAbility == true) {
                return false;
            }
            return true;
        }

        public virtual bool ProcessAnimatedAbilityHit(GameObject target, bool deactivateAutoAttack) {
            // we can now continue because everything beyond this point is single target oriented and it's ok if we cancel attacking due to lack of alive/unfriendly target
            // check for friendly target in case it somehow turned friendly mid swing
            if (target == null || deactivateAutoAttack == true) {
                baseCharacter.CharacterCombat.DeActivateAutoAttack();
                return false;
            }

            if (baseCharacter.CharacterCombat.MyAutoAttackActive == false) {
                //Debug.Log(gameObject.name + ".CharacterCombat.AttackHit_AnimationEvent(): activating auto-attack");
                baseCharacter.CharacterCombat.ActivateAutoAttack();
            }
            return true;
        }

        public virtual bool PerformWeaponAffinityCheck(BaseAbility baseAbility) {
            foreach (WeaponSkill _weaponAffinity in baseAbility.WeaponAffinityList) {
                if (baseCharacter != null && baseCharacter.CharacterEquipmentManager != null && baseCharacter.CharacterEquipmentManager.HasAffinity(_weaponAffinity)) {
                    return true;
                }
            }
            return false;
        }


        public virtual void LearnUnitProfileAbilities() {
            //Debug.Log(gameObject.name + ".CharacterAbilityManager.LearnUnitProfileAbilities()");
            if (baseCharacter != null && baseCharacter.MyUnitProfile != null) {
                foreach (BaseAbility baseAbility in baseCharacter.MyUnitProfile.MyLearnedAbilities) {
                    if (baseAbility is AnimatedAbility && (baseAbility as AnimatedAbility).IsAutoAttack == true) {
                        UnLearnDefaultAutoAttackAbility();
                    }
                    LearnAbility(baseAbility);
                }
            }
        }

        public override void DespawnAbilityObjects() {
            base.DespawnAbilityObjects();
            if (baseCharacter != null && baseCharacter.CharacterEquipmentManager != null) {
                baseCharacter.CharacterEquipmentManager.DespawnAbilityObjects();
            }
        }

        public override void CleanupCoroutines() {
            //Debug.Log(gameObject.name + ".CharacterAbilitymanager.CleanupCoroutines()");
            base.CleanupCoroutines();
            if (currentCastCoroutine != null) {
                StopCoroutine(currentCastCoroutine);
                EndCastCleanup();
            }
            CleanupCoolDownRoutines();

            if (globalCoolDownCoroutine != null) {
                StopCoroutine(globalCoolDownCoroutine);
                globalCoolDownCoroutine = null;
            }
        }

        // this ability exists to allow a caster to auto-self cast
        public virtual GameObject ReturnTarget(AbilityEffect abilityEffect, GameObject target) {
            //Debug.Log("BaseAbility.ReturnTarget(" + (sourceCharacter == null ? "null" : sourceCharacter.MyName) + ", " + (target == null ? "null" : target.name) + ")");
            CharacterUnit targetCharacterUnit = null;
            if (target != null) {
                targetCharacterUnit = target.GetComponent<CharacterUnit>();
                if (targetCharacterUnit != null) {
                    if (!abilityEffect.CanCastOnEnemy) {
                        if (Faction.RelationWith(targetCharacterUnit.MyCharacter, baseCharacter) <= -1) {
                            //Debug.Log("we cannot cast this on an enemy but the target was an enemy.  set target to null");
                            target = null;
                        }
                    }
                    if (!abilityEffect.CanCastOnFriendly) {
                        if (Faction.RelationWith(targetCharacterUnit.MyCharacter, baseCharacter) >= 0) {
                            //Debug.Log("we cannot cast this on a friendly target but the target was friendly.  set target to null");
                            target = null;
                        }
                    }
                } else {
                    //Debug.Log("target did not have a characterUnit.  set target to null");
                    target = null;
                }
            }

            // convert null target to self if possible
            if (target == null) {
                if (abilityEffect.AutoSelfCast == true) {
                    //Debug.Log("target is null and autoselfcast is true.  setting target to self");
                    target = baseCharacter.CharacterUnit.gameObject;
                }
            }

            if (!abilityEffect.CanCastOnSelf && target == baseCharacter.CharacterUnit.gameObject) {
                //Debug.Log("we cannot cast this on ourself but the target was ourself.  set target to null");
                target = null;
            }

            return target;
        }

        public override void BeginAbilityCoolDown(BaseAbility baseAbility, float coolDownLength = -1f) {

            base.BeginAbilityCoolDown(baseAbility, coolDownLength);

            float abilityCoolDown = 0f;

            if (coolDownLength == -1f) {
                abilityCoolDown = baseAbility.abilityCoolDown;
            } else {
                abilityCoolDown = coolDownLength;
            }

            if (abilityCoolDown <= 0f && baseAbility.MyIgnoreGlobalCoolDown == false && baseAbility.MyAbilityCastingTime == 0f) {
                // if the ability had no cooldown, and wasn't ignoring global cooldown, it gets a global cooldown length cooldown as we shouldn't have 0 cooldown instant cast abilities
                abilityCoolDown = Mathf.Clamp(abilityCoolDown, 1, Mathf.Infinity);
            }

            if (abilityCoolDown == 0f) {
                // if the ability CoolDown is still zero (this was an ability with a cast time that doesn't need a cooldown), don't start cooldown coroutine
                return;
            }

            AbilityCoolDownNode abilityCoolDownNode = new AbilityCoolDownNode();
            abilityCoolDownNode.MyAbilityName = baseAbility.MyName;

            // need to account for auto-attack
            if (SystemConfigurationManager.MyInstance.MyAllowAutoAttack == false && (baseAbility is AnimatedAbility) && (baseAbility as AnimatedAbility).IsAutoAttack == true) {
                abilityCoolDownNode.MyRemainingCoolDown = abilityCoolDown;
            } else {
                abilityCoolDownNode.MyRemainingCoolDown = abilityCoolDown;
            }

            abilityCoolDownNode.MyInitialCoolDown = abilityCoolDownNode.MyRemainingCoolDown;

            if (!abilityCoolDownDictionary.ContainsKey(baseAbility.MyName)) {
                abilityCoolDownDictionary[baseAbility.MyName] = abilityCoolDownNode;
            }

            // ordering important.  don't start till after its in the dictionary or it will fail to remove itself from the dictionary, then add it self
            Coroutine coroutine = StartCoroutine(PerformAbilityCoolDown(baseAbility.MyName));
            abilityCoolDownNode.MyCoroutine = coroutine;

        }

        public void HandleEquipmentChanged(Equipment newItem, Equipment oldItem) {
            //Debug.Log(gameObject.name + ".CharacterAbilityManager.HandleEquipmentChanged(" + (newItem != null ? newItem.MyName : "null") + ", " + (oldItem != null ? oldItem.MyName : "null") + ")");
            if (oldItem != null) {
                foreach (BaseAbility baseAbility in oldItem.MyLearnedAbilities) {
                    UnlearnAbility(baseAbility);
                }
            }
            UpdateEquipmentTraits(oldItem);

            if (newItem != null) {
                if (newItem.MyOnEquipAbility != null) {
                    if (baseCharacter.CharacterUnit != null) {
                        BeginAbility(newItem.MyOnEquipAbility);
                    }
                }
                foreach (BaseAbility baseAbility in newItem.MyLearnedAbilities) {
                    if (baseAbility is AnimatedAbility && (baseAbility as AnimatedAbility).IsAutoAttack == true) {
                        UnLearnDefaultAutoAttackAbility();
                    }
                    LearnAbility(baseAbility);
                }
            }
            LearnDefaultAutoAttackAbility();

            // after equipment change, check all equipment sets and bonuses
            UpdateEquipmentTraits(newItem);
        }

        public virtual void UpdateEquipmentTraits(Equipment equipment) {

            if (equipment == null || equipment.MyEquipmentSet == null) {
                // nothing to do
                return;
            }

            int equipmentCount = 0;

            if (baseCharacter != null && baseCharacter.CharacterEquipmentManager != null) {
                equipmentCount = baseCharacter.CharacterEquipmentManager.GetEquipmentSetCount(equipment.MyEquipmentSet);
            }

            for (int i = 0; i < equipment.MyEquipmentSet.MyTraitList.Count; i++) {
                StatusEffect statusEffect = equipment.MyEquipmentSet.MyTraitList[i];
                if (statusEffect != null) {
                    if (equipmentCount > i) {
                        // we are allowed to have this buff
                        if (!baseCharacter.CharacterStats.MyStatusEffects.ContainsKey(SystemResourceManager.prepareStringForMatch(statusEffect.MyName))) {
                            ApplyStatusEffect(statusEffect);
                        }
                    } else {
                        // we are not allowed to have this buff
                        if (baseCharacter.CharacterStats.MyStatusEffects.ContainsKey(SystemResourceManager.prepareStringForMatch(statusEffect.MyName))) {
                            baseCharacter.CharacterStats.MyStatusEffects[SystemResourceManager.prepareStringForMatch(statusEffect.MyName)].CancelStatusEffect();
                        }
                    }
                }
            }

        }


        public virtual void UnLearnDefaultAutoAttackAbility() {
            if (baseCharacter != null && baseCharacter.MyUnitProfile != null && baseCharacter.MyUnitProfile.MyDefaultAutoAttackAbility != null) {
                UnlearnAbility(baseCharacter.MyUnitProfile.MyDefaultAutoAttackAbility);
            }
        }

        public virtual void LearnDefaultAutoAttackAbility() {
            //Debug.Log(gameObject.name + ".CharacterAbilityManager.LearnDefaultAutoAttackAbility()");
            if (AutoAttackKnown() == true) {
                //Debug.Log(gameObject.name + ".CharacterAbilityManager.LearnDefaultAutoAttackAbility(): auto-attack already know, exiting");
                // can't learn two auto-attacks at the same time
                return;
            }
            if (baseCharacter != null && baseCharacter.MyUnitProfile != null && baseCharacter.MyUnitProfile.MyDefaultAutoAttackAbility != null) {
                //Debug.Log(gameObject.name + ".CharacterAbilityManager.LearnDefaultAutoAttackAbility(): learning default auto attack ability");
                LearnAbility(baseCharacter.MyUnitProfile.MyDefaultAutoAttackAbility);
            }
        }

        public void HandleSpecializationChange(ClassSpecialization newClassSpecialization, ClassSpecialization oldClassSpecialization) {
            RemoveSpecializationTraits(oldClassSpecialization);
            UnLearnSpecializationAbilities(oldClassSpecialization);
            LearnSpecializationAbilities(newClassSpecialization);
            ApplySpecializationTraits(newClassSpecialization);
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
                foreach (AbilityEffect classTrait in newCharacterClass.MyTraitList) {
                    //Debug.Log(gameObject.name + ".CharacterAbilityManager.ApplyClassTraits(" + (newCharacterClass == null ? "null" : newCharacterClass.MyName) + "): trait: " + classTrait);
                    ApplyStatusEffect(classTrait);
                }
            }
        }

        public void ApplySpecializationTraits(ClassSpecialization newClassSpecialization) {
            //Debug.Log(gameObject.name + ".CharacterAbilityManager.ApplyClassTraits(" + (newCharacterClass == null ? "null" : newCharacterClass.MyName) + ")");
            if (newClassSpecialization != null && newClassSpecialization.MyTraitList != null && newClassSpecialization.MyTraitList.Count > 0) {
                foreach (AbilityEffect classTrait in newClassSpecialization.MyTraitList) {
                    //Debug.Log(gameObject.name + ".CharacterAbilityManager.ApplyClassTraits(" + (newCharacterClass == null ? "null" : newCharacterClass.MyName) + "): trait: " + classTrait);
                    ApplyStatusEffect(classTrait);
                }
            }
        }

        public void ApplyStatusEffect(AbilityEffect statusEffect, int overrideDuration = 0) {
            if (baseCharacter.CharacterStats != null) {
                AbilityEffectOutput abilityEffectOutput = new AbilityEffectOutput();
                abilityEffectOutput.overrideDuration = overrideDuration;
                // rememeber this method is meant for saved status effects
                abilityEffectOutput.savedEffect = true;
                AbilityEffect _abilityEffect = SystemAbilityEffectManager.MyInstance.GetNewResource(statusEffect.MyName);
                if (_abilityEffect != null) {
                    _abilityEffect.Cast(this, null, null, abilityEffectOutput);
                }
            }
        }

        public override bool IsPlayerControlled() {
            if (baseCharacter != null &&
                baseCharacter.CharacterController != null &&
                baseCharacter.CharacterController.MyMasterUnit != null &&
                baseCharacter.CharacterController.MyMasterUnit == (PlayerManager.MyInstance.MyCharacter as BaseCharacter)) {

                return true;
            }
            return base.IsPlayerControlled();
        }

        public override void AddPet(CharacterUnit target) {
            if (baseCharacter.MyCharacterPetManager != null && target.MyCharacter != null && target.MyCharacter.MyUnitProfile != null) {
                baseCharacter.MyCharacterPetManager.AddPet(target.MyCharacter.MyUnitProfile);
            }
        }

        public void ApplySavedStatusEffects(StatusEffectSaveData statusEffectSaveData) {
            ApplyStatusEffect(SystemAbilityEffectManager.MyInstance.GetNewResource(statusEffectSaveData.MyName), statusEffectSaveData.remainingSeconds);
        }

        public void RemoveClassTraits(CharacterClass oldCharacterClass) {
            if (oldCharacterClass !=null && oldCharacterClass.MyTraitList != null && oldCharacterClass.MyTraitList.Count > 0) {
                foreach (AbilityEffect classTrait in oldCharacterClass.MyTraitList) {
                    if (baseCharacter.CharacterStats != null && baseCharacter.CharacterStats.MyStatusEffects.ContainsKey(SystemResourceManager.prepareStringForMatch(classTrait.MyName))) {
                        baseCharacter.CharacterStats.MyStatusEffects[SystemResourceManager.prepareStringForMatch(classTrait.MyName)].CancelStatusEffect();
                    }
                }
            }
        }

        public void RemoveSpecializationTraits(ClassSpecialization oldClassSpecialization) {
            if (oldClassSpecialization != null && oldClassSpecialization.MyTraitList != null && oldClassSpecialization.MyTraitList.Count > 0) {
                foreach (AbilityEffect classTrait in oldClassSpecialization.MyTraitList) {
                    if (baseCharacter.CharacterStats != null && baseCharacter.CharacterStats.MyStatusEffects.ContainsKey(SystemResourceManager.prepareStringForMatch(classTrait.MyName))) {
                        baseCharacter.CharacterStats.MyStatusEffects[SystemResourceManager.prepareStringForMatch(classTrait.MyName)].CancelStatusEffect();
                    }
                }
            }
        }

        public void LearnClassAbilities(CharacterClass characterClass) {
            //Debug.Log(gameObject.name + ".PlayerCharacter.LearnFactionAbilities(" + newFaction + ")");
            if (characterClass == null) {
                return;
            }
            foreach (BaseAbility baseAbility in characterClass.MyAbilityList) {
                //Debug.Log(gameObject.name + ".PlayerCharacter.LearnFactionAbilities(" + newFaction + "); ability name: " + abilityName);
                if (baseAbility.MyRequiredLevel <= PlayerManager.MyInstance.MyCharacter.CharacterStats.Level && PlayerManager.MyInstance.MyCharacter.CharacterAbilityManager.HasAbility(baseAbility) == false) {
                    //Debug.Log(gameObject.name + ".PlayerCharacter.LearnFactionAbilities(" + newFaction + "); ability name: " + abilityName + " is not learned yet, LEARNING!");
                    PlayerManager.MyInstance.MyCharacter.CharacterAbilityManager.LearnAbility(baseAbility);
                } else {
                    //Debug.Log(gameObject.name + ".PlayerCharacter.LearnFactionAbilities(" + newFaction + "); ability name: " + abilityName + "; level: " + SystemAbilityManager.MyInstance.GetResource(abilityName).MyRequiredLevel + "; playerlevel: " + PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyLevel + "; hasability: " + (PlayerManager.MyInstance.MyCharacter.MyCharacterAbilityManager.HasAbility(abilityName)));
                }
            }
        }

        public void LearnSpecializationAbilities(ClassSpecialization classSpecialization) {
            //Debug.Log(gameObject.name + ".PlayerCharacter.LearnFactionAbilities(" + newFaction + ")");
            if (classSpecialization == null) {
                return;
            }
            foreach (BaseAbility baseAbility in classSpecialization.MyAbilityList) {
                //Debug.Log(gameObject.name + ".PlayerCharacter.LearnFactionAbilities(" + newFaction + "); ability name: " + abilityName);
                if (baseAbility.MyRequiredLevel <= PlayerManager.MyInstance.MyCharacter.CharacterStats.Level && PlayerManager.MyInstance.MyCharacter.CharacterAbilityManager.HasAbility(baseAbility) == false) {
                    //Debug.Log(gameObject.name + ".PlayerCharacter.LearnFactionAbilities(" + newFaction + "); ability name: " + abilityName + " is not learned yet, LEARNING!");
                    PlayerManager.MyInstance.MyCharacter.CharacterAbilityManager.LearnAbility(baseAbility);
                } else {
                    //Debug.Log(gameObject.name + ".PlayerCharacter.LearnFactionAbilities(" + newFaction + "); ability name: " + abilityName + "; level: " + SystemAbilityManager.MyInstance.GetResource(abilityName).MyRequiredLevel + "; playerlevel: " + PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyLevel + "; hasability: " + (PlayerManager.MyInstance.MyCharacter.MyCharacterAbilityManager.HasAbility(abilityName)));
                }
            }
        }
        public void UnLearnClassAbilities (CharacterClass characterClass) {
            if (characterClass == null) {
                return;
            }
            foreach (BaseAbility oldAbility in characterClass.MyAbilityList) {
                UnlearnAbility(oldAbility);
            }
        }

        public void UnLearnSpecializationAbilities(ClassSpecialization classSpecialization) {
            if (classSpecialization == null) {
                return;
            }
            foreach (BaseAbility oldAbility in classSpecialization.MyAbilityList) {
                UnlearnAbility(oldAbility);
            }
        }

        public IEnumerator PerformAbilityCoolDown(string abilityName) {
            //Debug.Log(gameObject + ".CharacterAbilityManager.BeginAbilityCoolDown(" + abilityName + ") IENUMERATOR");

            //Debug.Log(gameObject + ".BaseAbility.BeginAbilityCoolDown(): about to enter loop  IENUMERATOR");

            while (abilityCoolDownDictionary.ContainsKey(abilityName) && abilityCoolDownDictionary[abilityName].MyRemainingCoolDown > 0f) {
                yield return null;
                abilityCoolDownDictionary[abilityName].MyRemainingCoolDown -= Time.deltaTime;
                //Debug.Log(gameObject.name + ".CharacterAbilityManager.PerformAbilityCooldown():  IENUMERATOR: " + abilityCoolDownDictionary[abilityName].MyRemainingCoolDown);
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
            StopCasting();
            //MyWaitingForAnimatedAbility = false;
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

        public bool HasAbility(BaseAbility baseAbility) {
            //Debug.Log(gameObject.name + ".CharacterAbilitymanager.HasAbility(" + abilityName + ")");
            //string keyName = SystemResourceManager.prepareStringForMatch(baseAbility);
            //Debug.Log(gameObject.name + ".CharacterAbilitymanager.HasAbility(" + abilityName + "): keyname: " + keyName);
            if (MyAbilityList.ContainsValue(baseAbility)) {
                //Debug.Log(gameObject.name + ".CharacterAbilitymanager.HasAbility( " + abilityName + "): keyname: " + keyName + " TRUE!");
                return true;
            }
            //Debug.Log(gameObject.name + ".CharacterAbilitymanager.HasAbility( " + abilityName + "): keyname: " + keyName + " FALSE!");
            return false;
        }

        public void ActivateTargettingMode(BaseAbility baseAbility) {
            //Debug.Log("CharacterAbilityManager.ActivateTargettingMode()");
            targettingModeActive = true;
            CastTargettingManager.MyInstance.EnableProjector(baseAbility);
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

        public virtual void ProcessCharacterUnitSpawn() {
            //Debug.Log("CharacterAbilityManager.OnCharacterUnitSpawn()");

            if (BaseCharacter != null && BaseCharacter.AnimatedUnit != null && BaseCharacter.AnimatedUnit.MyCharacterMotor != null) {
                //Debug.Log("CharacterAbilityManager.OnCharacterUnitSpawn(): CharacterMotor is not null");
                BaseCharacter.AnimatedUnit.MyCharacterMotor.OnMovement += HandleManualMovement;
            } else {
                //Debug.Log("CharacterAbilityManager.OnCharacterUnitSpawn(): CharacterMotor is null!");
            }
        }

        public virtual void HandleCharacterUnitDespawn() {
            //Debug.Log(gameObject.name + ".CharacterAbilityManager.OnCharacterUnitDespawn()");
        }

        public virtual void UpdateAbilityList(int newLevel) {
            //Debug.Log(gameObject.name + ".CharacterAbilityManager.UpdateAbilityList(). length: " + abilityList.Count);
            LearnUnitProfileAbilities();
        }

        public virtual bool LearnAbility(BaseAbility newAbility) {
            //Debug.Log(gameObject.name + ".CharacterAbilityManager.LearnAbility(" + (newAbility == null ? "null" : newAbility.MyName) + ")");
            if (newAbility == null) {
                //Debug.Log(gameObject.name + ".CharacterAbilityManager.LearnAbility(): baseAbility is null");
                // can't learn a nonexistent ability
                return false;
            }
            if (newAbility is AnimatedAbility && (newAbility as AnimatedAbility).IsAutoAttack && baseCharacter.CharacterAbilityManager.AutoAttackKnown() == true) {
                // can't learn 2 auto-attacks
                return false;
            }
            if (!HasAbility(newAbility) && newAbility.MyRequiredLevel <= BaseCharacter.CharacterStats.Level) {
                abilityList[SystemResourceManager.prepareStringForMatch(newAbility.MyName)] = newAbility;
                return true;
            }/* else {
                if (HasAbility(newAbility)) {
                    //Debug.Log(gameObject.name + ".CharacterAbilityManager.LearnAbility(): already had ability");
                }
                if (!(newAbility.MyRequiredLevel <= MyBaseCharacter.MyCharacterStats.MyLevel)) {
                    //Debug.Log(gameObject.name + ".CharacterAbilityManager.LearnAbility(): level is too low");
                }
            }*/
            return false;
        }

        public void UnlearnAbility(BaseAbility oldAbility) {
            string keyName = SystemResourceManager.prepareStringForMatch(oldAbility.MyName);
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
            //Debug.Log(gameObject.name + "CharacterAbilitymanager.PerformAbilityCast(" + ability.MyName + ", " + (target == null ? "null" : target.name) + ") Enter Ienumerator with tag: " + startTime);
            bool canCast = true;
            if (ability.MyRequiresTarget == false || ability.CanCastOnEnemy == false) {
                // prevent the killing of your enemy target from stopping aoe casts and casts that cannot be cast on an ememy
                KillStopCastOverride();
            } else {
                KillStopCastNormal();
            }
            if (ability.MyRequiresGroundTarget == true) {
                //Debug.Log("CharacterAbilitymanager.PerformAbilityCast() Ability requires a ground target.");
                ActivateTargettingMode(ability as BaseAbility);
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
                // dismount if mounted

                //Debug.Log("Ground Targetting: cancast is true");
                if (!ability.CanSimultaneousCast) {
                    //Debug.Log("CharacterAbilitymanager.PerformAbilityCast() ability: " + ability.MyName + " can simultaneous cast is false, setting casting to true");
                    ability.StartCasting(this);
                }
                float currentCastTime = 0f;
                float nextTickTime = 0f;
                //Debug.Log(gameObject.name + ".CharacterAbilitymanager.PerformAbilityCast() currentCastTime: " + currentCastTime + "; MyAbilityCastingTime: " + ability.MyAbilityCastingTime);

                if (baseCharacter != null && baseCharacter.CharacterEquipmentManager != null && ability.MyHoldableObjects.Count != 0) {
                    //if (baseCharacter != null && baseCharacter.MyCharacterEquipmentManager != null && ability.MyAbilityCastingTime > 0f && ability.MyHoldableObjectNames.Count != 0) {
                    //Debug.Log(gameObject.name + ".CharacterAbilityManager.PerformAbilityCast(" + ability.MyName + "): spawning ability objects");
                    if (!ability.MyAnimatorCreatePrefabs) {
                        baseCharacter.CharacterEquipmentManager.SpawnAbilityObjects(ability.MyHoldableObjects);
                    }
                }
                if (ability.MyCastingAudioClip != null) {
                    //AudioManager.MyInstance.PlayEffect(ability.MyCastingAudioClip);
                    //baseCharacter.MyCharacterUnit.MyAudioSource.PlayOneShot(ability.MyCastingAudioClip);
                    //baseCharacter.MyCharacterUnit.MyUnitAudio.MyEffectSource.clip = ability.MyCastingAudioClip;
                    baseCharacter.CharacterUnit.MyUnitAudio.PlayEffect(ability.MyCastingAudioClip);
                }
                
                while (currentCastTime < ability.MyAbilityCastingTime) {
                    yield return null;
                    currentCastTime += Time.deltaTime;

                    // call this first because it updates the cast bar
                    //Debug.Log(gameObject.name + ".CharacterAbilitymanager.PerformAbilityCast() currentCastTime: " + currentCastTime + "; MyAbilityCastingTime: " + ability.MyAbilityCastingTime + "; calling OnCastTimeChanged()");
                    OnCastTimeChanged(ability, currentCastTime);

                    // now call the ability on casttime changed (really only here for channeled stuff to do damage)
                    nextTickTime = ability.OnCastTimeChanged(currentCastTime, nextTickTime, this, target);
                }
                //Debug.Log(gameObject.name + "CharacterAbilitymanager.PerformAbilityCast(" + ability.MyName + ", " + (target == null ? "null" : target.name) + ") Done casting with tag: " + startTime);
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
                if (!ability.CanSimultaneousCast) {
                    OnCastStop(BaseCharacter as BaseCharacter);
                    BaseCharacter.AnimatedUnit.MyCharacterAnimator.SetCasting(false);
                }
                PerformAbility(ability, target, GetGroundTarget());

            }
        }

        public void SpawnAbilityObjects(int indexValue = -1) {
            //Debug.Log(gameObject.name + ".CharacterAbilityManager.SpawnAbilityObjects(" + indexValue + ")");
            BaseAbility usedBaseAbility = null;
            if (BaseCharacter.AnimatedUnit.MyCharacterAnimator.MyCurrentAbility != null) {
                usedBaseAbility = BaseCharacter.AnimatedUnit.MyCharacterAnimator.MyCurrentAbility;
            }
            if (usedBaseAbility == null) {
                usedBaseAbility = currentCastAbility;
            }

            if (baseCharacter != null && baseCharacter.CharacterEquipmentManager != null && usedBaseAbility.MyHoldableObjects.Count != 0) {
                //if (baseCharacter != null && baseCharacter.MyCharacterEquipmentManager != null && ability.MyAbilityCastingTime > 0f && ability.MyHoldableObjectNames.Count != 0) {
                //Debug.Log(gameObject.name + ".CharacterAbilityManager.PerformAbilityCast(): spawning ability objects");
                if (usedBaseAbility.MyAnimatorCreatePrefabs) {
                    if (indexValue == -1) {
                        baseCharacter.CharacterEquipmentManager.SpawnAbilityObjects(usedBaseAbility.MyHoldableObjects);
                    } else {
                        List<PrefabProfile> passList = new List<PrefabProfile>();
                        passList.Add(usedBaseAbility.MyHoldableObjects[indexValue - 1]);
                        baseCharacter.CharacterEquipmentManager.SpawnAbilityObjects(passList);
                    }
                }
            }

        }

        public override void EndCastCleanup() {
            base.EndCastCleanup();
            if (baseCharacter.CharacterUnit.MyUnitAudio != null) {
                baseCharacter.CharacterUnit.MyUnitAudio.StopEffect();
            }
        }

        public void ReceiveKillDetails(BaseCharacter killedcharacter, float creditPercent) {
            //Debug.Log("CharacterAbilityManager.ReceiveKillDetails()");
            if (BaseCharacter.CharacterController.MyTarget == killedcharacter.CharacterUnit.gameObject) {
                if (killStopCast) {
                    StopCasting();
                }
            }
        }

        public void AttemptAutoAttack() {
            //Debug.Log(gameObject.name + ".CharacterAbilitymanager.AttemtpAutoAttack()");

            foreach (BaseAbility baseAbility in MyAbilityList.Values) {
                if (baseAbility is AnimatedAbility && (baseAbility as AnimatedAbility).IsAutoAttack) {
                    BeginAbility(baseAbility);
                }
            }
        }

        /// <summary>
        /// This is the entrypoint for character behavior calls and should not be used for anything else due to the runtime ability lookup that happens
        /// </summary>
        /// <param name="abilityName"></param>
        public void BeginAbility(string abilityName) {
            //Debug.Log(gameObject.name + "CharacterAbilitymanager.BeginAbility(" + (abilityName == null ? "null" : abilityName) + ")");
            BaseAbility baseAbility = SystemAbilityManager.MyInstance.GetResource(abilityName);
            // these have to be new resources because the ability stores a tick time
            //BaseAbility baseAbility = SystemAbilityManager.MyInstance.GetNewResource(abilityName);
            if (baseAbility != null) {
                BeginAbility(baseAbility);
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
            BeginAbilityCommon(ability, baseCharacter.CharacterController.MyTarget);
        }

        public void BeginAbility(IAbility ability, GameObject target) {
            //Debug.Log(gameObject.name + ".CharacterAbilityManager.BeginAbility(" + ability.MyName + ")");
            BeginAbilityCommon(ability, target);
        }

        public virtual float GetAnimationLengthMultiplier() {
            return baseCharacter.AnimatedUnit.MyCharacterAnimator.MyLastAnimationLength;
        }

        public virtual float GetOutgoingDamageModifiers() {
            return baseCharacter.CharacterStats.GetOutGoingDamageModifiers();
        }

        public override void ProcessWeaponHitEffects(AttackEffect attackEffect, GameObject target, AbilityEffectOutput abilityEffectOutput) {
            base.ProcessWeaponHitEffects(attackEffect, target, abilityEffectOutput);
            // handle weapon on hit effects
            if (baseCharacter.CharacterCombat != null && baseCharacter.CharacterCombat.MyOnHitEffect != null && attackEffect.DamageType == DamageType.physical && baseCharacter.CharacterCombat.MyOnHitEffect.MyName != attackEffect.MyName) {
                List<AbilityEffect> onHitEffectList = new List<AbilityEffect>();
                onHitEffectList.Add(baseCharacter.CharacterCombat.MyOnHitEffect);
                attackEffect.PerformAbilityEffects(this, target, abilityEffectOutput, onHitEffectList);
            } else {
                //Debug.Log(MyName + ".AttackEffect.PerformAbilityHit(" + (source == null ? "null" : source.name) + ", " + (target == null ? "null" : target.name) + "): no on hit effect set");
            }

        }

        /// <summary>
        /// +damage stat from gear and weapon damage
        /// </summary>
        /// <returns></returns>
        public virtual float GetPhysicalDamage() {
            // +damage stat from gear
            float returnValue = baseCharacter.CharacterStats.MyPhysicalDamage;
            
            // weapon damage
            if (baseCharacter.CharacterEquipmentManager != null) {
                returnValue += baseCharacter.CharacterEquipmentManager.GetWeaponDamage();
            }

            return returnValue;
        }

        public virtual float GetPhysicalPower() {
            return LevelEquations.GetPhysicalPowerForCharacter(baseCharacter);
        }

        public virtual float GetSpellPower() {
            return LevelEquations.GetSpellPowerForCharacter(baseCharacter);
        }

        public virtual float GetCritChance() {
            return LevelEquations.GetCritChanceForCharacter(baseCharacter);
        }

        protected virtual void BeginAbilityCommon(IAbility ability, GameObject target) {
            //Debug.Log(gameObject.name + ".CharacterAbilityManager.BeginAbilityCommon(" + (ability == null ? "null" : ability.MyName) + ", " + (target == null ? "null" : target.name) + ")");
            IAbility usedAbility = SystemAbilityManager.MyInstance.GetResource(ability.MyName);
            if (usedAbility == null) {
                Debug.LogError("CharacterAbilityManager.BeginAbilityCommon(" + (ability == null ? "null" : ability.MyName) + ", " + (target == null ? "null" : target.name) + ") NO ABILITY FOUND");
                return;
            }

            if (!CanCastAbility(usedAbility)) {
                //Debug.Log(gameObject.name + ".CharacterAbilityManager.BeginAbilityCommon(" + ability.MyName + ", " + (target != null ? target.name : "null") + ") cannot cast");
                return;
            }

            CharacterUnit targetCharacterUnit = null;
            if (target != null) {
                targetCharacterUnit = target.GetComponent<CharacterUnit>();
            }
            if (targetCharacterUnit != null && targetCharacterUnit.MyBaseCharacter != null) {
                if (Faction.RelationWith(targetCharacterUnit.MyBaseCharacter, baseCharacter) <= -1) {
                    if (targetCharacterUnit.MyBaseCharacter.CharacterCombat != null && usedAbility.CanCastOnEnemy == true && targetCharacterUnit.MyBaseCharacter.CharacterStats.IsAlive == true) {

                        // disable this for now.  npc should pull character into combat when he enters their agro range.  character should pull npc into combat when status effect is applied or ability lands
                        // agro includes a liveness check, so casting necromancy on a dead enemy unit should not pull it into combat with us if we haven't applied a faction or master control buff yet
                        /*
                        if (baseCharacter.MyCharacterCombat.GetInCombat() == false) {
                            baseCharacter.MyCharacterCombat.EnterCombat(targetCharacterUnit.MyCharacter);
                        }
                        */
                        baseCharacter.CharacterCombat.ActivateAutoAttack();
                        OnAttack(targetCharacterUnit.MyBaseCharacter);
                    }
                }
            }

            NotifyAttemptPerformAbility(usedAbility);

            // get final target before beginning casting
            GameObject finalTarget = usedAbility.ReturnTarget(this, target);

            if (finalTarget == null && usedAbility.MyRequiresTarget == true) {
                //Debug.Log(gameObject.name + ".CharacterAbilityManager.BeginAbilityCommon(): finalTarget is null. exiting");
                return;
            }
            if (finalTarget != null && PerformLOSCheck(target, usedAbility as ITargetable) == false) {
                Debug.Log(gameObject.name + ".CharacterAbilityManager.BeginAbilityCommon(): LOS check failed. exiting");
                return;
            }

            baseCharacter.CharacterUnit.CancelMountEffects();

            if (usedAbility.CanSimultaneousCast) {
                // directly performing to avoid interference with other abilities being casted
                //Debug.Log(gameObject.name + ".CharacterAbilityManager.BeginAbilityCommon(): can simultaneous cast");
                PerformAbility(usedAbility, finalTarget, GetGroundTarget());
            } else {
                //Debug.Log(gameObject.name + ".CharacterAbilityManager.BeginAbilityCommon(): can't simultanous cast");
                if (currentCastCoroutine == null) {
                    //Debug.Log("Performing Ability " + ability.MyName + " at a cost of " + ability.MyAbilityManaCost.ToString() + ": ABOUT TO START COROUTINE");

                    // we need to do this because we are allowed to stop an outstanding auto-attack to start this cast
                    if (BaseCharacter != null && BaseCharacter.AnimatedUnit != null && BaseCharacter.AnimatedUnit.MyCharacterAnimator != null) {
                        BaseCharacter.AnimatedUnit.MyCharacterAnimator.ClearAnimationBlockers();
                    }

                    // start the cast (or cast targetting projector)
                    currentCastCoroutine = StartCoroutine(PerformAbilityCast(usedAbility, finalTarget));
                    currentCastAbility = usedAbility as BaseAbility;
                } else {
                    //CombatLogUI.MyInstance.WriteCombatMessage("A cast was already in progress WE SHOULD NOT BE HERE BECAUSE WE CHECKED FIRST! iscasting: " + isCasting + "; currentcast==null? " + (currentCast == null));
                    // unless.... we got here from the crafting queue, which launches the next item as the last step of the currently in progress cast
                    //Debug.Log(gameObject.name + ".CharacterAbilityManager.BeginAbilityCommon(): A cast was already in progress!");
                }
            }
        }

        public virtual void NotifyAttemptPerformAbility(IAbility ability) {
            //OnPerformAbility(ability);
        }

        // this only checks if the ability is able to be cast based on character state.  It does not check validity of target or ability specific requirements
        public virtual bool CanCastAbility(IAbility ability) {
            //Debug.Log(gameObject.name + ".CharacterAbilityManager.CanCastAbility(" + ability.MyName + ")");

            // check if the ability is learned yet
            if (!PerformLearnedCheck(ability)) {
                return false;
            }

            // check if the ability is on cooldown
            if (!PerformCooldownCheck(ability)) {
                return false;
            }

            // check if we have enough mana
            if (!PerformManaCheck(ability)) {
                return false;
            }

            if (!PerformCombatCheck(ability)) {
                return false;
            }

            // default is true, nothing has stopped us so far
            //Debug.Log(gameObject.name + ".CharacterAbilityManager.CanCastAbility(" + ability.MyName + "): returning true");
            return true;
        }

        public virtual bool PerformLearnedCheck(IAbility ability) {

            string keyName = SystemResourceManager.prepareStringForMatch(ability.MyName);

            if (!ability.MyUseableWithoutLearning && !abilityList.ContainsKey(keyName)) {
                return false;
            }
            return true;
        }

        public virtual bool PerformCooldownCheck(IAbility ability) {
            if (abilityCoolDownDictionary.ContainsKey(ability.MyName) || (MyRemainingGlobalCoolDown > 0f && ability.MyIgnoreGlobalCoolDown == false)) {
                return false;
            }
            return true;
        }

        public virtual bool PerformCombatCheck(IAbility ability) {
            if (ability.MyRequireOutOfCombat == true && BaseCharacter.CharacterCombat.GetInCombat() == true) {
                return false;
            }
            return true;
        }

        public virtual bool PerformManaCheck(IAbility ability) {
            if (BaseCharacter.CharacterStats.currentMana < ability.MyAbilityManaCost) {
                return false;
            }
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

            if (BaseCharacter.CharacterStats.currentMana < ability.MyAbilityManaCost) {
                CombatLogUI.MyInstance.WriteCombatMessage("Not enough mana to perform " + ability.MyName + " at a cost of " + ability.MyAbilityManaCost.ToString());
                //Debug.Log("Not enough mana to perform " + ability.MyName + " at a cost of " + ability.MyAbilityManaCost.ToString());
                // GET RID OF CASTING PREFABS HERE
                return;
            }

            if (ability.MyAbilityManaCost != 0) {
                BaseCharacter.CharacterStats.UseMana(ability.MyAbilityManaCost);
            }

            // cast the system manager version so we can track globally the spell cooldown
            SystemAbilityManager.MyInstance.GetResource(ability.MyName).Cast(this, finalTarget, groundTarget);
            //ability.Cast(MyBaseCharacter.MyCharacterUnit.gameObject, finalTarget);
        }

        /// <summary>
        /// Stop casting if the character is manually moved with the movement keys
        /// </summary>
        public void HandleManualMovement() {
            //Debug.Log(gameObject.name + ".CharacterAbilityManager.HandleManualMovement(): Received On Manual Movement Handler");
            // adding new code to require some movement distance to prevent gravity while standing still from triggering this
            if (BaseCharacter.CharacterController.MyApparentVelocity > 0.1f) {
                //Debug.Log("CharacterAbilityManager.HandleManualMovement(): stop casting");
                if (currentCastAbility != null && currentCastAbility.MyRequiresGroundTarget == true && CastTargettingManager.MyInstance.ProjectorIsActive() == true) {
                    // do nothing
                    //Debug.Log("CharacterAbilityManager.HandleManualMovement(): not cancelling casting because we have a ground target active");
                } else {
                    StopCasting();
                }
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
                if (baseCharacter != null && baseCharacter.CharacterEquipmentManager != null) {
                    baseCharacter.CharacterEquipmentManager.DespawnAbilityObjects();
                }

            } else {
                //Debug.Log(gameObject.name + ".currentCast is null, nothing to stop");
            }
            if (BaseCharacter.AnimatedUnit != null && BaseCharacter.AnimatedUnit.MyCharacterAnimator != null) {
                BaseCharacter.AnimatedUnit.MyCharacterAnimator.ClearAnimationBlockers();
            }
            OnCastStop(BaseCharacter as BaseCharacter);
        }

        public void ProcessLevelUnload() {
            StopCasting();
            WaitingForAnimatedAbility = false;
        }



        public void AnimationHitAnimationEvent() {
            if (currentCastAbility != null) {
                if (currentCastAbility.MyAnimationHitAudioClip != null) {
                    //AudioManager.MyInstance.PlayEffect(ability.MyCastingAudioClip);
                    baseCharacter.CharacterUnit.MyUnitAudio.PlayEffect(currentCastAbility.MyAnimationHitAudioClip);
                }

            }
        }

        public override void InitiateGlobalCooldown(float coolDownToUse = 0f) {
            //Debug.Log(gameObject.name + ".CharacterAbilitymanager.InitiateGlobalCooldown(" + coolDownToUse + ")");
            base.InitiateGlobalCooldown(coolDownToUse);
            if (globalCoolDownCoroutine == null) {
                // set global cooldown length to animation length so we don't end up in situation where cast bars look fine, but we can't actually cast
                globalCoolDownCoroutine = StartCoroutine(BeginGlobalCoolDown(coolDownToUse));
            } else {
                Debug.Log(gameObject.name + ".CharacterAbilityManager.InitiateGlobalCooldown(): INVESTIGATE: GCD COROUTINE WAS NOT NULL");
            }

        }

        public IEnumerator BeginGlobalCoolDown(float coolDownTime) {
            //Debug.Log(gameObject.name + ".PlayerAbilityManager.BeginGlobalCoolDown(" + coolDownTime + ")");
            // 10 is kinda arbitrary, but if any animation is causing a GCD greater than 10 seconds, we've probably got issues anyway...
            // the current longest animated attack is ground slam at around 4 seconds
            remainingGlobalCoolDown = Mathf.Clamp(coolDownTime, 1, 10);
            initialGlobalCoolDown = remainingGlobalCoolDown;
            while (remainingGlobalCoolDown > 0f) {
                yield return null;
                remainingGlobalCoolDown -= Time.deltaTime;
                // we want to end immediately if the time is up or the cooldown coroutine will not be nullifed until the next frame
                //Debug.Log("BaseAbility.BeginAbilityCooldown():" + MyName + ". time: " + remainingCoolDown);
            }
            globalCoolDownCoroutine = null;
        }

        public bool AutoAttackKnown() {
            foreach (BaseAbility baseAbility in abilityList.Values) {
                if (baseAbility is AnimatedAbility && (baseAbility as AnimatedAbility).IsAutoAttack == true) {
                    return true;
                }
            }
            return false;
        }

    }

}