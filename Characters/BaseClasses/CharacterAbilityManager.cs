using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class CharacterAbilityManager : AbilityManager, IAbilityCaster {

        public virtual event System.Action<BaseCharacter> OnAttack = delegate { };
        public event System.Action<IAbilityCaster, IAbility, float> OnCastTimeChanged = delegate { };
        public event System.Action<BaseCharacter> OnCastStop = delegate { };

        protected BaseCharacter baseCharacter;

        protected Dictionary<string, IAbility> abilityList = new Dictionary<string, IAbility>();

        protected bool isCasting = false;

        protected Vector3 groundTarget = Vector3.zero;

        protected bool targettingModeActive = false;

        // does killing the player you are currently targetting stop your cast.  gets set to false when channeling aoe.
        private bool killStopCast = true;

        protected float remainingGlobalCoolDown = 0f;

        // we need a reference to the total length of the current global cooldown to properly calculate radial fill on the action buttons
        protected float initialGlobalCoolDown;

        protected BaseAbility autoAttackAbility = null;

        public float MyInitialGlobalCoolDown { get => initialGlobalCoolDown; set => initialGlobalCoolDown = value; }

        public float MyRemainingGlobalCoolDown { get => remainingGlobalCoolDown; set => remainingGlobalCoolDown = value; }

        private bool waitingForAnimatedAbility = false;


        public BaseCharacter BaseCharacter {
            get => baseCharacter;
            set => baseCharacter = value;
        }

        public override GameObject UnitGameObject {
            get {
                if (baseCharacter != null && baseCharacter.CharacterUnit != null) {
                    return baseCharacter.CharacterUnit.gameObject;
                }
                return null;
            }
        }

        public override bool PerformingAbility {
            get {
                if (WaitingForAnimatedAbility == true) {
                    return true;
                }
                if (IsCasting == true) {
                    return true;
                }
                return base.PerformingAbility;
            }
        }

        public override int Level {
            get {
                if (baseCharacter != null && baseCharacter.CharacterStats != null) {
                    return baseCharacter.CharacterStats.Level;
                }
                return base.Level;
            }
        }

        public override string Name {
            get {
                if (baseCharacter.CharacterName != null) {
                    return baseCharacter.CharacterName;
                }
                return base.Name;
            }
        }

        public override bool IsDead {
            get {
                if (baseCharacter.CharacterStats.IsAlive == false) {
                    return true;
                }
                return base.IsDead;
            }
        }

        public Dictionary<string, IAbility> MyAbilityList { get => abilityList; }
        public bool WaitingForAnimatedAbility { get => waitingForAnimatedAbility; set => waitingForAnimatedAbility = value; }
        public bool IsCasting { get => isCasting; set => isCasting = value; }
        public Dictionary<string, AbilityCoolDownNode> MyAbilityCoolDownDictionary { get => abilityCoolDownDictionary; set => abilityCoolDownDictionary = value; }
        public Coroutine MyCurrentCastCoroutine { get => currentCastCoroutine; }
        public BaseAbility AutoAttackAbility { get => autoAttackAbility; set => autoAttackAbility = value; }

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
            if (baseCharacter != null) {
                baseCharacter.OnClassChange -= HandleClassChange;
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

        public override void GeneratePower(IAbility ability) {
            //Debug.Log(gameObject.name + ".GeneratePower(" + ability.MyName + ")");
            if (ability.GeneratePowerResource == null) {
                // nothing to generate
                return;
            }
            base.GeneratePower(ability);
            if (baseCharacter != null && baseCharacter.CharacterStats != null) {
                //Debug.Log(gameObject.name + ".GeneratePower(" + ability.MyName + "): name " + ability.GeneratePowerResource.MyName  + "; " + ability.GetResourceGain(this));
                baseCharacter.CharacterStats.AddResourceAmount(ability.GeneratePowerResource.MyDisplayName, ability.GetResourceGain(this));
            }
        }

        public override List<AnimationClip> GetDefaultAttackAnimations() {
            //Debug.Log(gameObject.name + ".GetDefaultAttackAnimations()");
            if (autoAttackAbility != null) {
                return autoAttackAbility.AnimationClips;
            }
            return base.GetDefaultAttackAnimations();
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
        public override bool PerformFactionCheck(ITargetable targetableEffect, CharacterUnit targetCharacterUnit, bool targetIsSelf) {

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

            return base.PerformFactionCheck(targetableEffect, targetCharacterUnit, targetIsSelf);
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

        public override bool IsTargetInAbilityRange(BaseAbility baseAbility, GameObject target, AbilityEffectContext abilityEffectContext = null) {
            // if none of those is true, then we are casting on ourselves, so don't need to do range check
            return IsTargetInRange(target, baseAbility.UseMeleeRange, baseAbility.MaxRange, baseAbility, abilityEffectContext);
        }

        public override bool IsTargetInAbilityEffectRange(AbilityEffect abilityEffect, GameObject target, AbilityEffectContext abilityEffectContext = null) {
            // if none of those is true, then we are casting on ourselves, so don't need to do range check
            return IsTargetInRange(target, abilityEffect.UseMeleeRange, abilityEffect.MaxRange, abilityEffect, abilityEffectContext);
        }

        public override bool IsTargetInMeleeRange(GameObject target) {
            return baseCharacter.CharacterController.IsTargetInHitBox(target);
        }

        public override bool PerformLOSCheck(GameObject target, ITargetable targetable, AbilityEffectContext abilityEffectContext = null) {

            if (targetable.RequireLineOfSight == false) {
                return true;
            }

            Vector3 sourcePosition = transform.position;
            // get initial positions in case of no collider
            if (targetable.LineOfSightSourceLocation == LineOfSightSourceLocation.Caster) {
                sourcePosition = transform.position;
                Collider sourceCollider = GetComponent<Collider>();
                if (sourceCollider != null) {
                    sourcePosition = sourceCollider.bounds.center;
                }
            } else if (targetable.LineOfSightSourceLocation == LineOfSightSourceLocation.GroundTarget && abilityEffectContext != null) {
                sourcePosition = abilityEffectContext.groundTargetLocation;
            } else if (targetable.LineOfSightSourceLocation == LineOfSightSourceLocation.OriginalTarget && abilityEffectContext != null) {
                sourcePosition = abilityEffectContext.originalTarget.transform.position;
            }

            Vector3 targetPosition = target.transform.position;

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
            return base.PerformLOSCheck(target, targetable, abilityEffectContext);
        }

        public bool IsTargetInRange(GameObject target, bool useMeleeRange, float maxRange, ITargetable targetable, AbilityEffectContext abilityEffectContext = null) {
            // if none of those is true, then we are casting on ourselves, so don't need to do range check

            if (useMeleeRange) {
                if (!IsTargetInMeleeRange(target)) {
                    return false;
                }
            } else {
                if (!IsTargetInMaxRange(target, maxRange, targetable, abilityEffectContext)) {
                    return false;
                }
                if (!PerformLOSCheck(target, targetable, abilityEffectContext)) {
                    return false;
                }
            }
            return true;
        }

        public virtual bool IsTargetInMaxRange(GameObject target, float maxRange, ITargetable targetable, AbilityEffectContext abilityEffectContext) {
            if (target == null || UnitGameObject == null) {
                return false;
            }
            Vector3 sourcePosition = UnitGameObject.transform.position;
            if (targetable.TargetRangeSourceLocation == TargetRangeSourceLocation.GroundTarget && abilityEffectContext != null) {
                sourcePosition = abilityEffectContext.groundTargetLocation;
            } else if (targetable.TargetRangeSourceLocation == TargetRangeSourceLocation.OriginalTarget && abilityEffectContext != null) {
                sourcePosition = abilityEffectContext.originalTarget.transform.position;
            }
            //Debug.Log(target.name + " range(" + maxRange + ": " + Vector3.Distance(UnitGameObject.transform.position, target.transform.position));
            if (maxRange > 0 && Vector3.Distance(sourcePosition, target.transform.position) > maxRange) {
                //Debug.Log(target.name + " is out of range(" + maxRange + "): " + Vector3.Distance(UnitGameObject.transform.position, target.transform.position));
                return false;
            }

            return true;
        }

        public void HandleLevelUnload(string eventName, EventParamProperties eventParamProperties) {
            ProcessLevelUnload();
        }

        public override float PerformAnimatedAbility(AnimationClip animationClip, AnimatedAbility animatedAbility, BaseCharacter targetBaseCharacter, AbilityEffectContext abilityEffectContext) {
            // this type of ability is allowed to interrupt other types of animations, so clear them all
            baseCharacter.AnimatedUnit.MyCharacterAnimator.ClearAnimationBlockers();

            // now block further animations of other types from starting
            if (!animatedAbility.IsAutoAttack) {
                baseCharacter.CharacterAbilityManager.WaitingForAnimatedAbility = true;
            } else {
                baseCharacter.CharacterCombat.SetWaitingForAutoAttack(true);
            }
            return baseCharacter.AnimatedUnit.MyCharacterAnimator.HandleAbility(animationClip, animatedAbility, targetBaseCharacter, abilityEffectContext);
        }

        /// <summary>
        /// Return true if the ability hit, false if it missed
        /// </summary>
        /// <returns></returns>
        public override bool AbilityHit(GameObject target, AbilityEffectContext abilityEffectContext) {
            if (baseCharacter.CharacterCombat.DidAttackMiss() == true) {
                //Debug.Log(MyName + ".BaseAbility.PerformAbilityHit(" + source.name + ", " + target.name + "): attack missed");
                baseCharacter.CharacterCombat.ReceiveCombatMiss(target, abilityEffectContext);
                return false;
            }
            return base.AbilityHit(target, abilityEffectContext);
        }

        public override bool PerformAnimatedAbilityCheck(AnimatedAbility animatedAbility) {
            if (WaitingForAnimatedAbility == true) {
                return false;
            }
            return base.PerformAnimatedAbilityCheck(animatedAbility);
        }

        public override bool ProcessAnimatedAbilityHit(GameObject target, bool deactivateAutoAttack) {
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
            return base.ProcessAnimatedAbilityHit(target, deactivateAutoAttack);
        }

        public override bool PerformWeaponAffinityCheck(BaseAbility baseAbility) {
            foreach (WeaponSkill _weaponAffinity in baseAbility.WeaponAffinityList) {
                if (baseCharacter != null && baseCharacter.CharacterEquipmentManager != null && baseCharacter.CharacterEquipmentManager.HasAffinity(_weaponAffinity)) {
                    return true;
                }
            }

            // intentionally not calling base because it's always true
            return false;
        }


        public virtual void LearnUnitProfileAbilities() {
            //Debug.Log(gameObject.name + ".CharacterAbilityManager.LearnUnitProfileAbilities()");
            if (baseCharacter != null && baseCharacter.UnitProfile != null) {
                foreach (BaseAbility baseAbility in baseCharacter.UnitProfile.LearnedAbilities) {
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
            StopAllCoroutines();
        }

        // this ability exists to allow a caster to auto-self cast
        public override GameObject ReturnTarget(AbilityEffect abilityEffect, GameObject target) {
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

            if (!abilityEffect.CanCastOnSelf && baseCharacter != null && baseCharacter.CharacterUnit != null && target == baseCharacter.CharacterUnit.gameObject) {
                //Debug.Log("we cannot cast this on ourself but the target was ourself.  set target to null");
                target = null;
            }

            // intentionally not calling base as it always returns the original target
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

            if (abilityCoolDown <= 0f && baseAbility.MyIgnoreGlobalCoolDown == false && baseAbility.GetAbilityCastingTime(this) == 0f) {
                // if the ability had no cooldown, and wasn't ignoring global cooldown, it gets a global cooldown length cooldown as we shouldn't have 0 cooldown instant cast abilities
                abilityCoolDown = Mathf.Clamp(abilityCoolDown, 1, Mathf.Infinity);
            }

            if (abilityCoolDown == 0f) {
                // if the ability CoolDown is still zero (this was an ability with a cast time that doesn't need a cooldown), don't start cooldown coroutine
                return;
            }

            AbilityCoolDownNode abilityCoolDownNode = new AbilityCoolDownNode();
            abilityCoolDownNode.MyAbilityName = baseAbility.MyDisplayName;

            // need to account for auto-attack
            if (SystemConfigurationManager.MyInstance.MyAllowAutoAttack == false && (baseAbility is AnimatedAbility) && (baseAbility as AnimatedAbility).IsAutoAttack == true) {
                abilityCoolDownNode.MyRemainingCoolDown = abilityCoolDown;
            } else {
                abilityCoolDownNode.MyRemainingCoolDown = abilityCoolDown;
            }

            abilityCoolDownNode.MyInitialCoolDown = abilityCoolDownNode.MyRemainingCoolDown;

            if (!abilityCoolDownDictionary.ContainsKey(baseAbility.MyDisplayName)) {
                abilityCoolDownDictionary[baseAbility.MyDisplayName] = abilityCoolDownNode;
            }

            // ordering important.  don't start till after its in the dictionary or it will fail to remove itself from the dictionary, then add it self
            Coroutine coroutine = StartCoroutine(PerformAbilityCoolDown(baseAbility.MyDisplayName));
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
                        if (!baseCharacter.CharacterStats.StatusEffects.ContainsKey(SystemResourceManager.prepareStringForMatch(statusEffect.MyDisplayName))) {
                            ApplyStatusEffect(statusEffect);
                        }
                    } else {
                        // we are not allowed to have this buff
                        if (baseCharacter.CharacterStats.StatusEffects.ContainsKey(SystemResourceManager.prepareStringForMatch(statusEffect.MyDisplayName))) {
                            baseCharacter.CharacterStats.StatusEffects[SystemResourceManager.prepareStringForMatch(statusEffect.MyDisplayName)].CancelStatusEffect();
                        }
                    }
                }
            }

        }


        public virtual void UnLearnDefaultAutoAttackAbility() {
            if (baseCharacter != null && baseCharacter.UnitProfile != null && baseCharacter.UnitProfile.DefaultAutoAttackAbility != null) {
                UnlearnAbility(baseCharacter.UnitProfile.DefaultAutoAttackAbility);
            }
        }

        public virtual void LearnDefaultAutoAttackAbility() {
            //Debug.Log(gameObject.name + ".CharacterAbilityManager.LearnDefaultAutoAttackAbility()");
            if (autoAttackAbility != null) {
                //Debug.Log(gameObject.name + ".CharacterAbilityManager.LearnDefaultAutoAttackAbility(): auto-attack already know, exiting");
                // can't learn two auto-attacks at the same time
                return;
            }
            if (baseCharacter != null && baseCharacter.UnitProfile != null && baseCharacter.UnitProfile.DefaultAutoAttackAbility != null) {
                //Debug.Log(gameObject.name + ".CharacterAbilityManager.LearnDefaultAutoAttackAbility(): learning default auto attack ability");
                LearnAbility(baseCharacter.UnitProfile.DefaultAutoAttackAbility);
            }
        }

        public void HandleSpecializationChange(ClassSpecialization newClassSpecialization, ClassSpecialization oldClassSpecialization) {
            RemoveSpecializationTraits(oldClassSpecialization);
            UnLearnSpecializationAbilities(oldClassSpecialization);
            LearnSpecializationAbilities(newClassSpecialization);
            ApplySpecializationTraits(newClassSpecialization);
        }

        public void HandleClassChange(CharacterClass newCharacterClass, CharacterClass oldCharacterClass) {
            //Debug.Log(gameObject.name + ".CharacterAbilityManager.HandleClassChange(" + (newCharacterClass == null ? "null" : newCharacterClass.MyName) + ")");
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
            if (newClassSpecialization != null && newClassSpecialization.TraitList != null && newClassSpecialization.TraitList.Count > 0) {
                foreach (AbilityEffect classTrait in newClassSpecialization.TraitList) {
                    //Debug.Log(gameObject.name + ".CharacterAbilityManager.ApplyClassTraits(" + (newCharacterClass == null ? "null" : newCharacterClass.MyName) + "): trait: " + classTrait);
                    ApplyStatusEffect(classTrait);
                }
            }
        }

        public void ApplyStatusEffect(AbilityEffect statusEffect, int overrideDuration = 0) {
            if (baseCharacter.CharacterStats != null) {
                AbilityEffectContext abilityEffectOutput = new AbilityEffectContext();
                abilityEffectOutput.overrideDuration = overrideDuration;
                // rememeber this method is meant for saved status effects
                abilityEffectOutput.savedEffect = true;
                AbilityEffect _abilityEffect = SystemAbilityEffectManager.MyInstance.GetNewResource(statusEffect.MyDisplayName);
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
            if (baseCharacter.MyCharacterPetManager != null && target.MyCharacter != null && target.MyCharacter.UnitProfile != null) {
                baseCharacter.MyCharacterPetManager.AddPet(target.MyCharacter.UnitProfile);
            }
        }

        public void ApplySavedStatusEffects(StatusEffectSaveData statusEffectSaveData) {
            ApplyStatusEffect(SystemAbilityEffectManager.MyInstance.GetNewResource(statusEffectSaveData.MyName), statusEffectSaveData.remainingSeconds);
        }

        public void RemoveClassTraits(CharacterClass oldCharacterClass) {
            if (oldCharacterClass !=null && oldCharacterClass.MyTraitList != null && oldCharacterClass.MyTraitList.Count > 0) {
                foreach (AbilityEffect classTrait in oldCharacterClass.MyTraitList) {
                    if (baseCharacter.CharacterStats != null && baseCharacter.CharacterStats.StatusEffects.ContainsKey(SystemResourceManager.prepareStringForMatch(classTrait.MyDisplayName))) {
                        baseCharacter.CharacterStats.StatusEffects[SystemResourceManager.prepareStringForMatch(classTrait.MyDisplayName)].CancelStatusEffect();
                    }
                }
            }
        }

        public void RemoveSpecializationTraits(ClassSpecialization oldClassSpecialization) {
            if (oldClassSpecialization != null && oldClassSpecialization.TraitList != null && oldClassSpecialization.TraitList.Count > 0) {
                foreach (AbilityEffect classTrait in oldClassSpecialization.TraitList) {
                    if (baseCharacter.CharacterStats != null && baseCharacter.CharacterStats.StatusEffects.ContainsKey(SystemResourceManager.prepareStringForMatch(classTrait.MyDisplayName))) {
                        baseCharacter.CharacterStats.StatusEffects[SystemResourceManager.prepareStringForMatch(classTrait.MyDisplayName)].CancelStatusEffect();
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
            foreach (BaseAbility baseAbility in classSpecialization.AbilityList) {
                //Debug.Log(gameObject.name + ".PlayerCharacter.LearnFactionAbilities(" + newFaction + "); ability name: " + abilityName);
                if (baseAbility.MyRequiredLevel <= PlayerManager.MyInstance.MyCharacter.CharacterStats.Level && PlayerManager.MyInstance.MyCharacter.CharacterAbilityManager.HasAbility(baseAbility) == false) {
                    //Debug.Log(gameObject.name + ".PlayerCharacter.LearnFactionAbilities(" + newFaction + "); ability name: " + abilityName + " is not learned yet, LEARNING!");
                    PlayerManager.MyInstance.MyCharacter.CharacterAbilityManager.LearnAbility(baseAbility);
                } else {
                    //Debug.Log(gameObject.name + ".PlayerCharacter.LearnFactionAbilities(" + newFaction + "); ability name: " + abilityName + "; level: " + SystemAbilityManager.MyInstance.GetResource(abilityName).MyRequiredLevel + "; playerlevel: " + PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyLevel + "; hasability: " + (PlayerManager.MyInstance.MyCharacter.MyCharacterAbilityManager.HasAbility(abilityName)));
                }
            }
        }

        public virtual void UnLearnClassAbilities (CharacterClass characterClass, bool updateActionBars = false) {
            if (characterClass == null) {
                return;
            }
            foreach (BaseAbility oldAbility in characterClass.MyAbilityList) {
                UnlearnAbility(oldAbility, updateActionBars);
            }
        }

        public void UnLearnSpecializationAbilities(ClassSpecialization classSpecialization) {
            if (classSpecialization == null) {
                return;
            }
            foreach (BaseAbility oldAbility in classSpecialization.AbilityList) {
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

        public virtual void ActivateTargettingMode(BaseAbility baseAbility, GameObject target) {
            //Debug.Log("CharacterAbilityManager.ActivateTargettingMode()");
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
            bool isAutoAttack = false;
            if (newAbility is AnimatedAbility && (newAbility as AnimatedAbility).IsAutoAttack) {
                isAutoAttack = true;
            }
            if (isAutoAttack && autoAttackAbility != null) {
                // can't learn 2 auto-attacks
                return false;
            }
            if (!HasAbility(newAbility) && newAbility.MyRequiredLevel <= BaseCharacter.CharacterStats.Level) {
                abilityList[SystemResourceManager.prepareStringForMatch(newAbility.MyDisplayName)] = newAbility;
                if (isAutoAttack) {
                    //Debug.Log(gameObject.name + ".CharacterAbilityManager.LearnAbility(" + (newAbility == null ? "null" : newAbility.MyName) + "): setting auto-attack ability");
                    autoAttackAbility = newAbility;
                }
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

        public virtual void UnlearnAbility(BaseAbility oldAbility, bool updateActionBars = true) {
            string keyName = SystemResourceManager.prepareStringForMatch(oldAbility.MyDisplayName);
            if (abilityList.ContainsKey(keyName)) {
                bool isAutoAttack = false;
                if (oldAbility is AnimatedAbility && (oldAbility as AnimatedAbility).IsAutoAttack) {
                    isAutoAttack = true;
                }
                if (isAutoAttack) {
                    autoAttackAbility = null;
                }
                abilityList.Remove(keyName);
            }
        }

        /// <summary>
        /// Cast a spell with a cast timer
        /// </summary>
        /// <param name="ability"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public IEnumerator PerformAbilityCast(IAbility ability, GameObject target, AbilityEffectContext abilityEffectContext) {
            float startTime = Time.time;
            //Debug.Log(gameObject.name + "CharacterAbilitymanager.PerformAbilityCast(" + ability.MyName + ", " + (target == null ? "null" : target.name) + ") Enter Ienumerator with tag: " + startTime);
            bool canCast = true;
            if (ability.MyRequiresTarget == false || ability.CanCastOnEnemy == false) {
                // prevent the killing of your enemy target from stopping aoe casts and casts that cannot be cast on an ememy
                KillStopCastOverride();
            } else {
                KillStopCastNormal();
            }
            abilityEffectContext.originalTarget = target;
            if (ability.MyRequiresGroundTarget == true) {
                //Debug.Log("CharacterAbilitymanager.PerformAbilityCast() Ability requires a ground target.");
                ActivateTargettingMode(ability as BaseAbility, target);
                while (WaitingForTarget() == true) {
                    //Debug.Log("CharacterAbilitymanager.PerformAbilityCast() waiting for target");
                    yield return null;
                }
                if (GetGroundTarget() == Vector3.zero) {
                    //Debug.Log("Ground Targetting: groundtarget is vector3.zero, cannot cast");
                    canCast = false;
                }
                abilityEffectContext.groundTargetLocation = GetGroundTarget();
            }
            if (canCast == true) {
                // dismount if mounted

                //Debug.Log("Ground Targetting: cancast is true");
                if (!ability.CanSimultaneousCast) {
                    //Debug.Log("CharacterAbilitymanager.PerformAbilityCast() ability: " + ability.MyName + " can simultaneous cast is false, setting casting to true");
                    ability.StartCasting(this);
                }
                float currentCastPercent = 0f;
                float nextTickPercent = 0f;
                //Debug.Log(gameObject.name + ".CharacterAbilitymanager.PerformAbilityCast() currentCastTime: " + currentCastTime + "; MyAbilityCastingTime: " + ability.MyAbilityCastingTime);

                if (baseCharacter != null && baseCharacter.CharacterEquipmentManager != null && ability.MyHoldableObjects.Count != 0) {
                    //if (baseCharacter != null && baseCharacter.MyCharacterEquipmentManager != null && ability.MyAbilityCastingTime > 0f && ability.MyHoldableObjectNames.Count != 0) {
                    //Debug.Log(gameObject.name + ".CharacterAbilityManager.PerformAbilityCast(" + ability.MyName + "): spawning ability objects");
                    if (!ability.MyAnimatorCreatePrefabs) {
                        baseCharacter.CharacterEquipmentManager.SpawnAbilityObjects(ability.MyHoldableObjects);
                    }
                }
                if (ability.MyCastingAudioClip != null) {
                    baseCharacter.CharacterUnit.UnitAudio.PlayCast(ability.MyCastingAudioClip);
                }
                

                while (currentCastPercent < 1f) {
                    yield return null;
                    currentCastPercent += (Time.deltaTime / ability.GetAbilityCastingTime(this));

                    // call this first because it updates the cast bar
                    //Debug.Log(gameObject.name + ".CharacterAbilitymanager.PerformAbilityCast() currentCastTime: " + currentCastTime + "; MyAbilityCastingTime: " + ability.MyAbilityCastingTime + "; calling OnCastTimeChanged()");
                    OnCastTimeChanged(this, ability, currentCastPercent);

                    // now call the ability on casttime changed (really only here for channeled stuff to do damage)
                    nextTickPercent = ability.OnCastTimeChanged(currentCastPercent, nextTickPercent, this, target, abilityEffectContext);
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
                PerformAbility(ability, target, abilityEffectContext);

            }
        }

        public void SpawnAbilityObjects(int indexValue = -1) {
            //Debug.Log(gameObject.name + ".CharacterAbilityManager.SpawnAbilityObjects(" + indexValue + ")");
            BaseAbility usedBaseAbility = null;
            if (BaseCharacter.AnimatedUnit.MyCharacterAnimator.MyCurrentAbilityEffectContext != null) {
                usedBaseAbility = BaseCharacter.AnimatedUnit.MyCharacterAnimator.MyCurrentAbilityEffectContext.baseAbility;
            }
            if (usedBaseAbility == null) {
                usedBaseAbility = currentCastAbility;
            }

            if (baseCharacter != null &&
                baseCharacter.CharacterEquipmentManager != null &&
                usedBaseAbility != null &&
                usedBaseAbility.MyHoldableObjects != null &&
                usedBaseAbility.MyHoldableObjects.Count != 0) {
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
            if (baseCharacter.CharacterUnit.UnitAudio != null) {
                baseCharacter.CharacterUnit.UnitAudio.StopCast();
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

            if (autoAttackAbility != null) {
                BeginAbility(autoAttackAbility);
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

        public override float GetSpeed() {
            return 1f / (baseCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Speed].CurrentValue / 100f);
        }

        public override float GetAnimationLengthMultiplier() {
            if (baseCharacter != null && baseCharacter.AnimatedUnit != null && baseCharacter.AnimatedUnit.MyCharacterAnimator != null) {
                return (baseCharacter.AnimatedUnit.MyCharacterAnimator.LastAnimationLength / (float)baseCharacter.AnimatedUnit.MyCharacterAnimator.LastAnimationHits);
            }
            return base.GetAnimationLengthMultiplier();
        }

        public override float GetOutgoingDamageModifiers() {
            if (baseCharacter != null && baseCharacter.CharacterStats != null) {
                return baseCharacter.CharacterStats.GetOutGoingDamageModifiers();
            }
            return base.GetOutgoingDamageModifiers();
        }

        public override void ProcessWeaponHitEffects(AttackEffect attackEffect, GameObject target, AbilityEffectContext abilityEffectOutput) {
            base.ProcessWeaponHitEffects(attackEffect, target, abilityEffectOutput);
            // handle weapon on hit effects
            if (baseCharacter.CharacterCombat != null && baseCharacter.CharacterCombat.MyOnHitEffect != null && attackEffect.DamageType == DamageType.physical && baseCharacter.CharacterCombat.MyOnHitEffect.MyDisplayName != attackEffect.MyDisplayName) {
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
        public override float GetPhysicalDamage() {
            if (baseCharacter != null  && baseCharacter.CharacterStats != null) {
                // +damage stat from gear
                float returnValue = GetPhysicalPower();

                // weapon damage
                if (baseCharacter.CharacterEquipmentManager != null) {
                    returnValue += baseCharacter.CharacterEquipmentManager.GetWeaponDamage();
                }

                return returnValue;
            }
            return base.GetPhysicalDamage();
        }

        public override float GetPhysicalPower() {
            if (baseCharacter != null) {
                return LevelEquations.GetSecondaryStatForCharacter(SecondaryStatType.PhysicalDamage, baseCharacter) + LevelEquations.GetSecondaryStatForCharacter(SecondaryStatType.Damage, baseCharacter);
            }
            return base.GetPhysicalPower();
        }

        public override float GetSpellPower() {
            if (baseCharacter != null) {
                return LevelEquations.GetSecondaryStatForCharacter(SecondaryStatType.SpellDamage, baseCharacter) + LevelEquations.GetSecondaryStatForCharacter(SecondaryStatType.Damage, baseCharacter);
            }
            return base.GetSpellPower();
        }

        public override float GetCritChance() {
            if (baseCharacter != null) {
                return LevelEquations.GetSecondaryStatForCharacter(SecondaryStatType.CriticalStrike, baseCharacter);
            }
            return base.GetCritChance();
        }

        protected virtual void BeginAbilityCommon(IAbility ability, GameObject target) {
            //Debug.Log(gameObject.name + ".CharacterAbilityManager.BeginAbilityCommon(" + (ability == null ? "null" : ability.MyName) + ", " + (target == null ? "null" : target.name) + ")");
            IAbility usedAbility = SystemAbilityManager.MyInstance.GetResource(ability.MyDisplayName);
            if (usedAbility == null) {
                Debug.LogError("CharacterAbilityManager.BeginAbilityCommon(" + (ability == null ? "null" : ability.MyDisplayName) + ", " + (target == null ? "null" : target.name) + ") NO ABILITY FOUND");
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
            if (targetCharacterUnit != null && targetCharacterUnit.BaseCharacter != null) {
                if (Faction.RelationWith(targetCharacterUnit.BaseCharacter, baseCharacter) <= -1) {
                    if (targetCharacterUnit.BaseCharacter.CharacterCombat != null && usedAbility.CanCastOnEnemy == true && targetCharacterUnit.BaseCharacter.CharacterStats.IsAlive == true) {

                        // disable this for now.  npc should pull character into combat when he enters their agro range.  character should pull npc into combat when status effect is applied or ability lands
                        // agro includes a liveness check, so casting necromancy on a dead enemy unit should not pull it into combat with us if we haven't applied a faction or master control buff yet
                        /*
                        if (baseCharacter.MyCharacterCombat.GetInCombat() == false) {
                            baseCharacter.MyCharacterCombat.EnterCombat(targetCharacterUnit.MyCharacter);
                        }
                        */
                        baseCharacter.CharacterCombat.ActivateAutoAttack();
                        OnAttack(targetCharacterUnit.BaseCharacter);
                    }
                }
            }

            NotifyAttemptPerformAbility(usedAbility);

            AbilityEffectContext abilityEffectContext = new AbilityEffectContext();
            abilityEffectContext.baseAbility = ability as BaseAbility;

            // get final target before beginning casting
            GameObject finalTarget = usedAbility.ReturnTarget(this, target, true, abilityEffectContext);

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

                // there is no ground target yet because that is handled in performabilitycast below
                PerformAbility(usedAbility, finalTarget, abilityEffectContext);
            } else {
                //Debug.Log(gameObject.name + ".CharacterAbilityManager.BeginAbilityCommon(): can't simultanous cast");
                if (currentCastCoroutine == null) {
                    //Debug.Log("Performing Ability " + ability.MyName + " at a cost of " + ability.MyAbilityManaCost.ToString() + ": ABOUT TO START COROUTINE");

                    // we need to do this because we are allowed to stop an outstanding auto-attack to start this cast
                    if (BaseCharacter != null && BaseCharacter.AnimatedUnit != null && BaseCharacter.AnimatedUnit.MyCharacterAnimator != null) {
                        BaseCharacter.AnimatedUnit.MyCharacterAnimator.ClearAnimationBlockers();
                    }

                    // start the cast (or cast targetting projector)
                    currentCastCoroutine = StartCoroutine(PerformAbilityCast(usedAbility, finalTarget, abilityEffectContext));
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
            if (!PerformPowerResourceCheck(ability)) {
                return false;
            }

            if (!PerformCombatCheck(ability)) {
                return false;
            }

            if (!PerformLivenessCheck(ability)) {
                return false;
            }
            
            if (!PerformMovementCheck(ability)) {
                return false;
            }
            

            // default is true, nothing has stopped us so far
            //Debug.Log(gameObject.name + ".CharacterAbilityManager.CanCastAbility(" + ability.MyName + "): returning true");
            return true;
        }

        public virtual bool PerformLivenessCheck(IAbility ability) {
            if (!baseCharacter.CharacterStats.IsAlive) {
                return false;
            }
            return true;
        }

        public virtual bool PerformMovementCheck(IAbility ability) {
            if (ability.GetAbilityCastingTime(this) == 0f) {
                return true;
            }
            return !(baseCharacter.CharacterController.MyApparentVelocity > 0.1f);
        }

        public virtual bool PerformLearnedCheck(IAbility ability) {

            string keyName = SystemResourceManager.prepareStringForMatch(ability.MyDisplayName);

            if (!ability.MyUseableWithoutLearning && !abilityList.ContainsKey(keyName)) {
                return false;
            }
            return true;
        }

        public virtual bool PerformCooldownCheck(IAbility ability) {
            if (abilityCoolDownDictionary.ContainsKey(ability.MyDisplayName) || (MyRemainingGlobalCoolDown > 0f && ability.MyIgnoreGlobalCoolDown == false)) {
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

        /// <summary>
        /// Check if the caster has the required amount of the power resource to cast the ability
        /// </summary>
        /// <param name="ability"></param>
        /// <returns></returns>
        public virtual bool PerformPowerResourceCheck(IAbility ability) {
            if (BaseCharacter.CharacterStats.PerformPowerResourceCheck(ability, ability.GetResourceCost(this))) {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Casts a spell.  Note that this does not do the actual damage yet since the ability may have a travel time
        /// </summary>
        /// <param name="ability"></param>
        /// <param name="target"></param>
        public virtual void PerformAbility(IAbility ability, GameObject target, AbilityEffectContext abilityEffectContext) {
            //Debug.Log(gameObject.name + ".CharacterAbilityManager.PerformAbility(" + ability.MyName + ")");
            if (abilityEffectContext == null) {
                abilityEffectContext = new AbilityEffectContext();
                abilityEffectContext.baseAbility = ability as BaseAbility;
                abilityEffectContext.originalTarget = target;
            }
            GameObject finalTarget = target;
            if (finalTarget != null) {
                //Debug.Log(gameObject.name + ": performing ability: " + ability.MyName + " on " + finalTarget.name);
            } else {
                //Debug.Log(gameObject.name + ": performing ability: " + ability.MyName + ": finalTarget is null");
            }

            if (!PerformPowerResourceCheck(ability)) {
                return;
            }

            if (ability.GetResourceCost(this) != 0 && ability.PowerResource != null) {
                // intentionally not keeping track of this coroutine.  many of these could be in progress at once.
                abilityEffectContext.AddResourceAmount(ability.PowerResource.MyDisplayName, ability.GetResourceCost(this));
                StartCoroutine(UsePowerResourceDelay(ability.PowerResource, (int)ability.GetResourceCost(this), ability.SpendDelay));
            }

            // cast the system manager version so we can track globally the spell cooldown
            SystemAbilityManager.MyInstance.GetResource(ability.MyDisplayName).Cast(this, finalTarget, abilityEffectContext);
            //ability.Cast(MyBaseCharacter.MyCharacterUnit.gameObject, finalTarget);
        }

        public IEnumerator UsePowerResourceDelay(PowerResource powerResource, int amount, float delay) {
            float elapsedTime = 0f;
            while (elapsedTime < delay) {
                yield return null;
                elapsedTime += Time.deltaTime;
            }
            BaseCharacter.CharacterStats.UsePowerResource(powerResource, amount);
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

        public override AudioClip GetAnimatedAbilityHitSound() {
            if (baseCharacter != null && baseCharacter.CharacterCombat != null) {
                return baseCharacter.CharacterCombat.MyDefaultHitSoundEffect;
            }
            return base.GetAnimatedAbilityHitSound();
        }

        /// <summary>
        /// This will be triggered in response to things like hammer taps, not attacks
        /// </summary>
        public void AnimationHitAnimationEvent() {
            //Debug.Log(gameObject.name + ".CharacterAbilitymanager.AnimationHitAnimationEvent()");

            if (currentCastAbility != null) {
                AudioClip audioClip = currentCastAbility.GetAnimationHitSound();
                if (audioClip != null) {
                    //AudioManager.MyInstance.PlayEffect(ability.MyCastingAudioClip);
                    baseCharacter.CharacterUnit.UnitAudio.PlayEffect(audioClip);
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

    }

}