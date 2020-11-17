using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class CharacterAbilityManager : AbilityManager {

        public event System.Action<BaseCharacter> OnAttack = delegate { };
        public event System.Action<IAbilityCaster, BaseAbility, float> OnCastTimeChanged = delegate { };
        public event System.Action<BaseCharacter> OnCastStop = delegate { };
        public event System.Action<BaseAbility> OnAttemptPerformAbility = delegate { };
        public event System.Action<BaseAbility> OnPerformAbility = delegate { };
        public event System.Action OnUnlearnAbilities = delegate { };
        public event System.Action<BaseAbility> OnLearnedCheckFail = delegate { };
        public event System.Action<BaseAbility> OnCombatCheckFail = delegate { };
        public event System.Action<AnimatedAbility> OnAnimatedAbilityCheckFail = delegate { };
        public event System.Action<BaseAbility, IAbilityCaster> OnPowerResourceCheckFail = delegate { };
        public event System.Action<BaseAbility, Interactable> OnTargetInAbilityRangeFail = delegate { };
        public event System.Action<bool> OnUnlearnAbility = delegate { };
        public event System.Action<BaseAbility> OnLearnAbility = delegate { };
        public event System.Action<BaseAbility> OnActivateTargetingMode = delegate { };

        protected BaseCharacter baseCharacter;

        protected Dictionary<string, BaseAbility> abilityList = new Dictionary<string, BaseAbility>();

        protected bool isCasting = false;

        protected Vector3 groundTarget = Vector3.zero;

        protected bool targettingModeActive = false;

        // does killing the player you are currently targetting stop your cast.  gets set to false when channeling aoe.
        private bool killStopCast = true;

        protected float remainingGlobalCoolDown = 0f;

        // we need a reference to the total length of the current global cooldown to properly calculate radial fill on the action buttons
        protected float initialGlobalCoolDown;

        protected BaseAbility autoAttackAbility = null;

        // the holdable objects spawned during an ability cast and removed when the cast is complete
        protected Dictionary<AbilityAttachmentNode, GameObject> abilityObjects = new Dictionary<AbilityAttachmentNode, GameObject>();

        public float MyInitialGlobalCoolDown { get => initialGlobalCoolDown; set => initialGlobalCoolDown = value; }

        public float MyRemainingGlobalCoolDown { get => remainingGlobalCoolDown; set => remainingGlobalCoolDown = value; }

        private bool waitingForAnimatedAbility = false;


        public BaseCharacter BaseCharacter {
            get => baseCharacter;
            set => baseCharacter = value;
        }

        public override GameObject UnitGameObject {
            get {
                if (baseCharacter != null && baseCharacter.UnitController != null && baseCharacter.UnitController.CharacterUnit != null) {
                    return baseCharacter.UnitController.gameObject;
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

        public Dictionary<string, BaseAbility> AbilityList {
            get {
                Dictionary<string, BaseAbility> returnAbilityList = new Dictionary<string, BaseAbility>();
                foreach (string abilityName in abilityList.Keys) {
                    if (abilityList[abilityName].CharacterClassRequirementList == null || abilityList[abilityName].CharacterClassRequirementList.Count == 0 || abilityList[abilityName].CharacterClassRequirementList.Contains(baseCharacter.CharacterClass)) {
                        returnAbilityList.Add(abilityName, abilityList[abilityName]);
                    }
                }
                return returnAbilityList;
            }
            
        }
        public bool WaitingForAnimatedAbility { get => waitingForAnimatedAbility; set => waitingForAnimatedAbility = value; }
        public bool IsCasting { get => isCasting; set => isCasting = value; }
        public Dictionary<string, AbilityCoolDownNode> MyAbilityCoolDownDictionary { get => abilityCoolDownDictionary; set => abilityCoolDownDictionary = value; }
        public Coroutine MyCurrentCastCoroutine { get => currentCastCoroutine; }
        public BaseAbility AutoAttackAbility { get => autoAttackAbility; set => autoAttackAbility = value; }
        
        // direct access for save manager so we don't miss saving abilities we know but belong to another class
        public Dictionary<string, BaseAbility> RawAbilityList { get => abilityList; set => abilityList = value; }

        public CharacterAbilityManager(BaseCharacter baseCharacter) : base(baseCharacter) {
            this.baseCharacter = baseCharacter;
        }

        public void Init() {
            UpdateAbilityList(baseCharacter.CharacterStats.Level);
            LearnDefaultAutoAttackAbility();
        }

        public override List<AbilityAttachmentNode> GetWeaponAbilityObjectList() {
            if (baseCharacter.CharacterEquipmentManager != null) {
                return baseCharacter.CharacterEquipmentManager.WeaponHoldableObjects;
            }
            return base.GetWeaponAbilityObjectList();
        }

        public void LoadAbility(string abilityName) {
            //Debug.Log(gameObject.name + ".PlayerAbilityManager.LoadAbility(" + abilityName + ")");
            BaseAbility ability = SystemAbilityManager.MyInstance.GetResource(abilityName);
            if (ability != null) {
                // if we renamed an ability, old save data could load a null.  prevent invalid abilities from loading.
                bool isAutoAttack = false;
                if (ability is AnimatedAbility && (ability as AnimatedAbility).IsAutoAttack) {
                    isAutoAttack = true;
                }
                if (isAutoAttack && autoAttackAbility != null) {
                    // can't learn 2 auto-attacks
                    return;
                }

                string keyName = SystemResourceManager.prepareStringForMatch(abilityName);
                if (!abilityList.ContainsKey(keyName)) {
                    //Debug.Log("PlayerAbilityManager.LoadAbility(" + abilityName + "): found it!");
                    if (ability is AnimatedAbility && (ability as AnimatedAbility).IsAutoAttack == true) {
                        UnLearnDefaultAutoAttackAbility();
                        //Debug.Log(gameObject.name + ".PlayerAbilityManager.LoadAbility(" + abilityName + "): is auto-attack!");
                        autoAttackAbility = ability;
                    }
                    abilityList[keyName] = ability;
                }
            }
        }

        public AttachmentPointNode GetHeldAttachmentPointNode(AbilityAttachmentNode attachmentNode) {
            if (attachmentNode.UseUniversalAttachment == false) {
                AttachmentPointNode attachmentPointNode = new AttachmentPointNode();
                attachmentPointNode.TargetBone = attachmentNode.HoldableObject.TargetBone;
                attachmentPointNode.Position = attachmentNode.HoldableObject.Position;
                attachmentPointNode.Rotation = attachmentNode.HoldableObject.Rotation;
                attachmentPointNode.RotationIsGlobal = attachmentNode.HoldableObject.RotationIsGlobal;
                return attachmentPointNode;
            } else {
                // find unit profile, find prefab profile, find universal attachment profile, find universal attachment node
                if (baseCharacter != null && baseCharacter.UnitProfile != null && baseCharacter.UnitProfile != null && baseCharacter.UnitProfile.UnitPrefabProps.AttachmentProfile != null) {
                    if (baseCharacter.UnitProfile.UnitPrefabProps.AttachmentProfile.AttachmentPointDictionary.ContainsKey(attachmentNode.AttachmentName)) {
                        return baseCharacter.UnitProfile.UnitPrefabProps.AttachmentProfile.AttachmentPointDictionary[attachmentNode.AttachmentName];
                    }
                }
            }

            return null;
        }

        public void HoldObject(GameObject go, AbilityAttachmentNode attachmentNode, GameObject searchObject) {
            //public void HoldObject(GameObject go, PrefabProfile holdableObject, GameObject searchObject) {
            //Debug.Log(gameObject + ".CharacterEquipmentManager.HoldObject(" + go.name + ", " + holdableObjectName + ", " + searchObject.name + ")");
            if (attachmentNode == null || attachmentNode.HoldableObject == null || go == null || searchObject == null) {
                //Debug.Log(gameObject + ".CharacterEquipmentManager.HoldObject(): MyHoldableObjectName is empty");
                return;
            }

            AttachmentPointNode attachmentPointNode = GetHeldAttachmentPointNode(attachmentNode);
            if (attachmentPointNode != null && attachmentPointNode.TargetBone != null && attachmentPointNode.TargetBone != string.Empty) {
                Transform targetBone = searchObject.transform.FindChildByRecursive(attachmentPointNode.TargetBone);
                if (targetBone != null) {
                    //Debug.Log(gameObject + ".CharacterEquipmentManager.HoldObject(): targetBone: " + targetBone + "; position: " + holdableObject.MyPosition + "; holdableObject.MyPhysicalRotation: " + holdableObject.MyRotation);
                    go.transform.parent = targetBone;
                    go.transform.localPosition = attachmentPointNode.Position;
                    if (attachmentPointNode.RotationIsGlobal) {
                        go.transform.rotation = Quaternion.LookRotation(targetBone.transform.forward) * Quaternion.Euler(attachmentPointNode.Rotation);
                    } else {
                        go.transform.localEulerAngles = attachmentPointNode.Rotation;
                    }
                } else {
                    Debug.Log("CharacterAbilityManager.HoldObject(): Unable to find target bone : " + attachmentPointNode.TargetBone);
                }
            } else {
                // disabled message because some equipment (like quivers) does not have held attachment points intentionally because it should stay in the same place in combat
                //Debug.Log(gameObject + ".CharacterEquipmentManager.HoldObject(): Unable to get attachment point " + attachmentNode.UnsheathedAttachmentName);
            }
        }

        public void SpawnAbilityObjects(List<AbilityAttachmentNode> abilityAttachmentNodes) {
            //Debug.Log(gameObject.name + ".CharacterEquipmentManager.SpawnAbilityObjects()");
            Dictionary<AbilityAttachmentNode, GameObject> holdableObjects = new Dictionary<AbilityAttachmentNode, GameObject>();
            foreach (AbilityAttachmentNode abilityAttachmentNode in abilityAttachmentNodes) {
                if (abilityAttachmentNode != null) {
                    // NEW CODE
                    if (abilityAttachmentNode.HoldableObject != null && abilityAttachmentNode.HoldableObject.Prefab != null) {
                        //Debug.Log("EquipmentManager.HandleWeaponSlot(): " + newItem.name + " has a physical prefab");
                        // attach a mesh to a bone for weapons

                        AttachmentPointNode attachmentPointNode = GetHeldAttachmentPointNode(abilityAttachmentNode);
                        if (attachmentPointNode != null) {
                            Transform targetBone = baseCharacter.UnitController.transform.FindChildByRecursive(attachmentPointNode.TargetBone);

                            if (targetBone != null) {
                                //Debug.Log("EquipmentManager.HandleWeaponSlot(): " + newItem.name + " has a physical prefab. targetbone is not null: equipSlot: " + newItem.equipSlot);
                                GameObject newEquipmentPrefab = UnityEngine.Object.Instantiate(abilityAttachmentNode.HoldableObject.Prefab, targetBone, false);
                                //holdableObjects.Add(attachmentNode.MyHoldableObject, newEquipmentPrefab);
                                holdableObjects.Add(abilityAttachmentNode, newEquipmentPrefab);
                                //currentEquipmentPhysicalObjects[equipmentSlotProfile] = newEquipmentPrefab;

                                newEquipmentPrefab.transform.localScale = abilityAttachmentNode.HoldableObject.Scale;
                                HoldObject(newEquipmentPrefab, abilityAttachmentNode, baseCharacter.UnitController.gameObject);
                            } else {
                                Debug.Log("CharacterAbilityManager.SpawnAbilityObjects(). We could not find the target bone " + attachmentPointNode.TargetBone);
                            }
                        }
                    }
                    // END NEW CODE

                }
            }
            if (holdableObjects.Count > 0) {
                abilityObjects = holdableObjects;
            }

        }

        public override void DespawnAbilityObjects() {
            base.DespawnAbilityObjects();

            if (abilityObjects == null || abilityObjects.Count == 0) {
                return;
            }

            foreach (GameObject abilityObject in abilityObjects.Values) {
                if (abilityObject != null) {
                    UnityEngine.Object.Destroy(abilityObject);
                }
            }
            abilityObjects.Clear();
        }

        public override void GeneratePower(BaseAbility ability) {
            //Debug.Log(gameObject.name + ".GeneratePower(" + ability.MyName + ")");
            if (ability.GeneratePowerResource == null) {
                // nothing to generate
                return;
            }
            base.GeneratePower(ability);
            if (baseCharacter != null && baseCharacter.CharacterStats != null) {
                //Debug.Log(gameObject.name + ".GeneratePower(" + ability.MyName + "): name " + ability.GeneratePowerResource.MyName  + "; " + ability.GetResourceGain(this));
                baseCharacter.CharacterStats.AddResourceAmount(ability.GeneratePowerResource.DisplayName, ability.GetResourceGain(baseCharacter));
            }
        }

        public override List<AnimationClip> GetDefaultAttackAnimations() {
            //Debug.Log(gameObject.name + ".GetDefaultAttackAnimations()");
            if (autoAttackAbility != null) {
                return autoAttackAbility.AnimationClips;
            }
            return base.GetDefaultAttackAnimations();
        }

        public override List<AnimationClip> GetUnitAttackAnimations() {
            //Debug.Log(gameObject.name + ".GetDefaultAttackAnimations()");
            if (baseCharacter.UnitProfile != null
                && baseCharacter.UnitProfile != null
                && baseCharacter.UnitProfile.UnitPrefabProps.AnimationProps != null) {
                return baseCharacter.UnitProfile.UnitPrefabProps.AnimationProps.AttackClips;
            }
            return base.GetUnitAttackAnimations();
        }

        public override List<AnimationClip> GetUnitCastAnimations() {
            //Debug.Log(gameObject.name + ".GetDefaultAttackAnimations()");
            if (baseCharacter.UnitProfile != null
                && baseCharacter.UnitProfile != null
                && baseCharacter.UnitProfile.UnitPrefabProps.AnimationProps != null) {
                return baseCharacter.UnitProfile.UnitPrefabProps.AnimationProps.CastClips;
            }
            return base.GetUnitCastAnimations();
        }


        public override float GetMeleeRange() {
            if (baseCharacter != null && baseCharacter.UnitController.CharacterUnit != null) {
                return baseCharacter.UnitController.CharacterUnit.HitBoxSize;
            }
            return base.GetMeleeRange();
        }

        

        public override float GetThreatModifiers() {
            if (baseCharacter != null && baseCharacter.CharacterStats != null) {
                return baseCharacter.CharacterStats.GetThreatModifiers();
            }
            return base.GetThreatModifiers();
        }
        /*
        public override bool AddToAggroTable(CharacterUnit targetCharacterUnit, int usedAgroValue) {
            // intentionally don't call the base
            if (baseCharacter.CharacterStats.IsAlive) {
                return targetCharacterUnit.BaseCharacter.CharacterCombat.MyAggroTable.AddToAggroTable(baseCharacter.CharacterUnit, usedAgroValue);
            }
            return false;
        }
        */

        public override void GenerateAgro(CharacterUnit targetCharacterUnit, int usedAgroValue) {
            base.GenerateAgro(targetCharacterUnit, usedAgroValue);
            if (baseCharacter != null && baseCharacter.UnitController != null && baseCharacter.UnitController.CharacterUnit != null) {
                AddToAggroTable(baseCharacter.UnitController.CharacterUnit, usedAgroValue);
                //AgroNode = targetCharacterUnit.MyCharacter.MyCharacterCombat.MyAggroTable.MyTopAgroNode;
                //Debug.Log("StatusEffect.Cast(" + source.name + ", " + (target ? target.name : "null") + ") topNode agro value: " + AgroNode.aggroValue + "; target: " + AgroNode.aggroTarget.MyName);
                targetCharacterUnit.BaseCharacter.CharacterCombat.MyAggroTable.LockAgro();
            }

        }

        public override void PerformCastingAnimation(AnimationClip animationClip, BaseAbility baseAbility) {
            base.PerformCastingAnimation(animationClip, baseAbility);
            if (animationClip != null) {
                baseCharacter.UnitController.UnitAnimator.HandleCastingAbility(animationClip, baseAbility);
            }
        }

        public override void CapturePet(UnitController targetUnitController) {
            base.CapturePet(targetUnitController);
            if (baseCharacter.MyCharacterPetManager != null && targetUnitController != null) {
                //Debug.Log(gameObject.name + ".CapturePet(): adding to pet manager");
                baseCharacter.MyCharacterPetManager.AddPet(targetUnitController.UnitProfile);
                baseCharacter.MyCharacterPetManager.MyActiveUnitProfiles.Add(targetUnitController.UnitProfile, targetUnitController);
            }
        }

        public override bool IsTargetInAbilityRange(BaseAbility baseAbility, Interactable target, AbilityEffectContext abilityEffectContext = null) {
            // if none of those is true, then we are casting on ourselves, so don't need to do range check
            bool returnResult = IsTargetInRange(target, baseAbility.UseMeleeRange, baseAbility.MaxRange, baseAbility, abilityEffectContext);
            if (returnResult == false) {
                OnTargetInAbilityRangeFail(baseAbility, target);
            }
            return returnResult;
        }

        public override bool IsTargetInAbilityEffectRange(AbilityEffect abilityEffect, Interactable target, AbilityEffectContext abilityEffectContext = null) {
            // if none of those is true, then we are casting on ourselves, so don't need to do range check
            return IsTargetInRange(target, abilityEffect.UseMeleeRange, abilityEffect.MaxRange, abilityEffect, abilityEffectContext);
        }

        public override bool IsTargetInMeleeRange(Interactable target) {
            return baseCharacter.UnitController.IsTargetInHitBox(target);
        }

        public override bool PerformLOSCheck(Interactable target, ITargetable targetable, AbilityEffectContext abilityEffectContext = null) {

            if (targetable.RequireLineOfSight == false) {
                return true;
            }

            Vector3 sourcePosition = abilityCaster.transform.position;
            // get initial positions in case of no collider
            if (targetable.LineOfSightSourceLocation == LineOfSightSourceLocation.Caster) {
                sourcePosition = abilityCaster.transform.position;
                Collider sourceCollider = abilityCaster.GetComponent<Collider>();
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

            int targetMask = 1 << target.gameObject.layer;
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

        public bool IsTargetInRange(Interactable target, bool useMeleeRange, float maxRange, ITargetable targetable, AbilityEffectContext abilityEffectContext = null) {
            //Debug.Log(baseCharacter.gameObject.name + ".IsTargetInRange()");
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

            //Debug.Log(baseCharacter.gameObject.name + ".IsTargetInRange(): return true");
            return true;
        }

        public bool IsTargetInMaxRange(Interactable target, float maxRange, ITargetable targetable, AbilityEffectContext abilityEffectContext) {
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

        public override float PerformAnimatedAbility(AnimationClip animationClip, AnimatedAbility animatedAbility, BaseCharacter targetBaseCharacter, AbilityEffectContext abilityEffectContext) {
            // this type of ability is allowed to interrupt other types of animations, so clear them all
            baseCharacter.UnitController.UnitAnimator.ClearAnimationBlockers();

            // now block further animations of other types from starting
            if (!animatedAbility.IsAutoAttack) {
                baseCharacter.CharacterAbilityManager.WaitingForAnimatedAbility = true;
            } else {
                baseCharacter.CharacterCombat.SetWaitingForAutoAttack(true);
            }
            return baseCharacter.UnitController.UnitAnimator.HandleAbility(animationClip, animatedAbility, targetBaseCharacter, abilityEffectContext);
        }

        /// <summary>
        /// Return true if the ability hit, false if it missed
        /// </summary>
        /// <returns></returns>
        public override bool AbilityHit(Interactable target, AbilityEffectContext abilityEffectContext) {
            if (baseCharacter.CharacterCombat.DidAttackMiss() == true) {
                //Debug.Log(MyName + ".BaseAbility.PerformAbilityHit(" + source.name + ", " + target.name + "): attack missed");
                baseCharacter.CharacterCombat.ReceiveCombatMiss(target, abilityEffectContext);
                return false;
            }
            return base.AbilityHit(target, abilityEffectContext);
        }

        public override bool PerformAnimatedAbilityCheck(AnimatedAbility animatedAbility) {
            if (WaitingForAnimatedAbility == true) {
                OnAnimatedAbilityCheckFail(animatedAbility);
                return false;
            }
            return base.PerformAnimatedAbilityCheck(animatedAbility);
        }

        public override bool ProcessAnimatedAbilityHit(Interactable target, bool deactivateAutoAttack) {
            // we can now continue because everything beyond this point is single target oriented and it's ok if we cancel attacking due to lack of alive/unfriendly target
            // check for friendly target in case it somehow turned friendly mid swing
            if (target == null || deactivateAutoAttack == true) {
                baseCharacter.CharacterCombat.DeActivateAutoAttack();
                return false;
            }

            if (baseCharacter.CharacterCombat.AutoAttackActive == false) {
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

        public override void CleanupCoroutines() {
            //Debug.Log(gameObject.name + ".CharacterAbilitymanager.CleanupCoroutines()");
            base.CleanupCoroutines();
            if (currentCastCoroutine != null) {
                abilityCaster.StopCoroutine(currentCastCoroutine);
                EndCastCleanup();
            }
            CleanupCoolDownRoutines();

            if (globalCoolDownCoroutine != null) {
                abilityCaster.StopCoroutine(globalCoolDownCoroutine);
                globalCoolDownCoroutine = null;
            }
            abilityCaster.StopAllCoroutines();
        }

        /// <summary>
        /// Return false if the target does not meet the faction requirements
        /// </summary>
        /// <param name="baseAbility"></param>
        /// <param name="targetCharacterUnit"></param>
        /// <param name="targetIsSelf"></param>
        /// <returns></returns>
        public override bool PerformFactionCheck(ITargetable targetableEffect, CharacterUnit targetCharacterUnit, bool targetIsSelf) {

            // if this ability has no faction requirements, we can cast it on anyone
            // added cancastonothers because we can have no faction requirement but need to only cast on self
            if (targetableEffect.CanCastOnOthers == true
                && targetableEffect.CanCastOnEnemy == false
                && targetableEffect.CanCastOnNeutral == false
                && targetableEffect.CanCastOnFriendly == false) {
                return true;
            }

            float relationValue = Faction.RelationWith(targetCharacterUnit.BaseCharacter, baseCharacter);

            if (targetableEffect.CanCastOnEnemy == true && relationValue <= -1) {
                return true;
            }

            if (targetableEffect.CanCastOnNeutral == true && relationValue > -1 && targetableEffect.CanCastOnNeutral == true && relationValue < 1) {
                return true;
            }

            if (targetableEffect.CanCastOnFriendly == true && relationValue >= 1) {
                return true;
            }

            return false;

            //return base.PerformFactionCheck(targetableEffect, targetCharacterUnit, targetIsSelf);
        }

        // this ability exists to allow a caster to auto-self cast
        public override Interactable ReturnTarget(AbilityEffect abilityEffect, Interactable target) {
            //Debug.Log("BaseAbility.ReturnTarget(" + (sourceCharacter == null ? "null" : sourceCharacter.AbilityManager.MyName) + ", " + (target == null ? "null" : target.name) + ")");
            CharacterUnit targetCharacterUnit = null;
            if (target != null) {
                targetCharacterUnit = CharacterUnit.GetCharacterUnit(target);
                if (targetCharacterUnit != null) {
                    bool targetIsSelf = false;
                    if (baseCharacter != null && baseCharacter.UnitController != null && baseCharacter.UnitController.CharacterUnit != null) {
                        targetIsSelf = (target == baseCharacter.UnitController.gameObject);
                    }
                    if (!PerformFactionCheck(abilityEffect, targetCharacterUnit, targetIsSelf)) {
                        target = null;
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
                    target = baseCharacter.UnitController;
                }
            }

            if (!abilityEffect.CanCastOnSelf && baseCharacter != null && baseCharacter.UnitController != null && target == baseCharacter.UnitController.gameObject) {
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

            if (abilityCoolDown <= 0f && baseAbility.IgnoreGlobalCoolDown == false && baseAbility.GetAbilityCastingTime(baseCharacter) == 0f) {
                // if the ability had no cooldown, and wasn't ignoring global cooldown, it gets a global cooldown length cooldown as we shouldn't have 0 cooldown instant cast abilities
                abilityCoolDown = Mathf.Clamp(abilityCoolDown, 1, Mathf.Infinity);
            }

            if (abilityCoolDown == 0f) {
                // if the ability CoolDown is still zero (this was an ability with a cast time that doesn't need a cooldown), don't start cooldown coroutine
                return;
            }

            AbilityCoolDownNode abilityCoolDownNode = new AbilityCoolDownNode();
            abilityCoolDownNode.MyAbilityName = baseAbility.DisplayName;

            // need to account for auto-attack
            if (SystemConfigurationManager.MyInstance.MyAllowAutoAttack == false && (baseAbility is AnimatedAbility) && (baseAbility as AnimatedAbility).IsAutoAttack == true) {
                abilityCoolDownNode.MyRemainingCoolDown = abilityCoolDown;
            } else {
                abilityCoolDownNode.MyRemainingCoolDown = abilityCoolDown;
            }

            abilityCoolDownNode.MyInitialCoolDown = abilityCoolDownNode.MyRemainingCoolDown;

            if (!abilityCoolDownDictionary.ContainsKey(baseAbility.DisplayName)) {
                abilityCoolDownDictionary[baseAbility.DisplayName] = abilityCoolDownNode;
            }

            // ordering important.  don't start till after its in the dictionary or it will fail to remove itself from the dictionary, then add it self
            Coroutine coroutine = abilityCaster.StartCoroutine(PerformAbilityCoolDown(baseAbility.DisplayName));
            abilityCoolDownNode.MyCoroutine = coroutine;

        }

        public void HandleEquipmentChanged(Equipment newItem, Equipment oldItem, int slotIndex) {
            //Debug.Log(gameObject.name + ".CharacterAbilityManager.HandleEquipmentChanged(" + (newItem != null ? newItem.MyName : "null") + ", " + (oldItem != null ? oldItem.MyName : "null") + ")");
            if (oldItem != null) {
                foreach (BaseAbility baseAbility in oldItem.LearnedAbilities) {
                    UnlearnAbility(baseAbility);
                }
            }
            UpdateEquipmentTraits(oldItem);

            if (newItem != null) {
                if (newItem.OnEquipAbility != null) {
                    if (baseCharacter.UnitController != null) {
                        BeginAbility(newItem.OnEquipAbility);
                    }
                }
                foreach (BaseAbility baseAbility in newItem.LearnedAbilities) {
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

        public void UpdateEquipmentTraits(Equipment equipment) {

            if (equipment == null || equipment.EquipmentSet == null) {
                // nothing to do
                return;
            }

            int equipmentCount = 0;

            if (baseCharacter != null && baseCharacter.CharacterEquipmentManager != null) {
                equipmentCount = baseCharacter.CharacterEquipmentManager.GetEquipmentSetCount(equipment.EquipmentSet);
            }

            for (int i = 0; i < equipment.EquipmentSet.MyTraitList.Count; i++) {
                StatusEffect statusEffect = equipment.EquipmentSet.MyTraitList[i];
                if (statusEffect != null) {
                    if (equipmentCount > i) {
                        // we are allowed to have this buff
                        if (!baseCharacter.CharacterStats.StatusEffects.ContainsKey(SystemResourceManager.prepareStringForMatch(statusEffect.DisplayName))) {
                            ApplyStatusEffect(statusEffect);
                        }
                    } else {
                        // we are not allowed to have this buff
                        if (baseCharacter.CharacterStats.StatusEffects.ContainsKey(SystemResourceManager.prepareStringForMatch(statusEffect.DisplayName))) {
                            baseCharacter.CharacterStats.StatusEffects[SystemResourceManager.prepareStringForMatch(statusEffect.DisplayName)].CancelStatusEffect();
                        }
                    }
                }
            }

        }


        public void UnLearnDefaultAutoAttackAbility() {
            if (baseCharacter != null && baseCharacter.UnitProfile != null && baseCharacter.UnitProfile.DefaultAutoAttackAbility != null) {
                UnlearnAbility(baseCharacter.UnitProfile.DefaultAutoAttackAbility);
            }
        }

        public void LearnDefaultAutoAttackAbility() {
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

        public void HandleCapabilityProviderChange(CapabilityConsumerSnapshot oldSnapshot, CapabilityConsumerSnapshot newSnapshot) {
            Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.HandleAbilityProviderChange()");
            RemoveCapabilityProviderTraits(oldSnapshot.GetTraitsToRemove(newSnapshot));
            UnLearnCapabilityProviderAbilities(oldSnapshot.GetAbilitiesToRemove(newSnapshot));
            LearnCapabilityProviderAbilities(oldSnapshot.GetAbilitiesToAdd(newSnapshot));
            ApplyCapabilityProviderTraits(oldSnapshot.GetTraitsToAdd(newSnapshot));
        }

        public void ApplyCapabilityProviderTraits(List<StatusEffect> statusEffects) {
            if (statusEffects == null) {
                return;
            }
            foreach (StatusEffect statusEffect in statusEffects) {
                ApplyStatusEffect(statusEffect);
            }
        }

        public void ApplyStatusEffect(AbilityEffect statusEffect, int overrideDuration = 0) {
            if (baseCharacter.CharacterStats != null) {
                AbilityEffectContext abilityEffectOutput = new AbilityEffectContext();
                abilityEffectOutput.overrideDuration = overrideDuration;
                // rememeber this method is meant for saved status effects
                abilityEffectOutput.savedEffect = true;
                AbilityEffect _abilityEffect = SystemAbilityEffectManager.MyInstance.GetNewResource(statusEffect.DisplayName);
                if (_abilityEffect != null) {
                    _abilityEffect.Cast(baseCharacter, null, null, abilityEffectOutput);
                }
            }
        }

        public override bool IsPlayerControlled() {
            if (baseCharacter != null &&
                baseCharacter.UnitController != null &&
                baseCharacter.UnitController.MasterUnit != null &&
                baseCharacter.UnitController.MasterUnit == (PlayerManager.MyInstance.MyCharacter as BaseCharacter)) {

                return true;
            }
            return base.IsPlayerControlled();
        }

        public override void AddPet(CharacterUnit target) {
            if (baseCharacter.MyCharacterPetManager != null
                && target.Interactable != null
                && target.Interactable is UnitController
                && (target.Interactable as UnitController).UnitProfile != null) {
                baseCharacter.MyCharacterPetManager.AddPet((target.Interactable as UnitController).UnitProfile);
            }
        }

        public void ApplySavedStatusEffects(StatusEffectSaveData statusEffectSaveData) {
            ApplyStatusEffect(SystemAbilityEffectManager.MyInstance.GetNewResource(statusEffectSaveData.MyName), statusEffectSaveData.remainingSeconds);
        }

        public void RemoveCapabilityProviderTraits(List<StatusEffect> statusEffects) {
            if (statusEffects == null) {
                return;
            }
            foreach (StatusEffect statusEffect in statusEffects) {
                if (baseCharacter.CharacterStats != null && baseCharacter.CharacterStats.StatusEffects.ContainsKey(SystemResourceManager.prepareStringForMatch(statusEffect.DisplayName))) {
                    baseCharacter.CharacterStats.StatusEffects[SystemResourceManager.prepareStringForMatch(statusEffect.DisplayName)].CancelStatusEffect();
                }
            }
        }

        public void LearnSystemAbilities() {
            //Debug.Log(gameObject.name + ".CharacterAbilityManager.LearnSystemAbilities(" + newFaction + ")");
            foreach (BaseAbility ability in SystemAbilityManager.MyInstance.GetResourceList()) {
                if (ability.RequiredLevel <= baseCharacter.CharacterStats.Level && ability.AutoLearn == true) {
                    if (!HasAbility(ability)) {
                        LearnAbility(ability);
                    } else {
                        //Debug.Log(ability.MyName + " already known, no need to re-learn");
                    }
                }
            }

        }

        public void LearnCapabilityProviderAbilities(List<BaseAbility> abilities) {
            if (abilities == null) {
                return;
            }
            foreach (BaseAbility baseAbility in abilities) {
                if (baseAbility.RequiredLevel <= baseCharacter.CharacterStats.Level && baseCharacter.CharacterAbilityManager.HasAbility(baseAbility) == false) {
                    if (baseAbility is AnimatedAbility && (baseAbility as AnimatedAbility).IsAutoAttack == true) {
                        UnLearnDefaultAutoAttackAbility();
                    }
                    LearnAbility(baseAbility);
                }
            }
        }

        public void UnLearnCapabilityProviderAbilities (List<BaseAbility> abilities, bool updateActionBars = false) {
            if (abilities == null) {
                return;
            }
            foreach (BaseAbility oldAbility in abilities) {
                UnlearnAbility(oldAbility, updateActionBars);
            }
            OnUnlearnAbilities();
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


        public void HandleDie(CharacterStats _characterStats) {
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

        public override bool HasAbility(BaseAbility baseAbility) {
            //Debug.Log(gameObject.name + ".CharacterAbilitymanager.HasAbility(" + abilityName + ")");
            //string keyName = SystemResourceManager.prepareStringForMatch(baseAbility);
            //Debug.Log(gameObject.name + ".CharacterAbilitymanager.HasAbility(" + abilityName + "): keyname: " + keyName);
            if (AbilityList.ContainsKey(SystemResourceManager.prepareStringForMatch(baseAbility.DisplayName))) {
                //Debug.Log(gameObject.name + ".CharacterAbilitymanager.HasAbility( " + abilityName + "): keyname: " + keyName + " TRUE!");
                return true;
            }
            //Debug.Log(gameObject.name + ".CharacterAbilitymanager.HasAbility( " + abilityName + "): keyname: " + keyName + " FALSE!");
            return base.HasAbility(baseAbility);
        }

        public void ActivateTargettingMode(BaseAbility baseAbility, Interactable target) {
            //Debug.Log("CharacterAbilityManager.ActivateTargettingMode()");
            targettingModeActive = true;
            if (baseCharacter != null && baseCharacter.UnitController != null && baseCharacter.UnitController.UnitControllerMode == UnitControllerMode.AI) {
                targettingModeActive = false;
                groundTarget = target.transform.position;
            }
            OnActivateTargetingMode(baseAbility);
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

        public void UpdateAbilityList(int newLevel) {
            //Debug.Log(gameObject.name + ".CharacterAbilityManager.UpdateAbilityList(). length: " + abilityList.Count);

            LearnSystemAbilities();

            CapabilityConsumerSnapshot capabilityConsumerSnapshot = new CapabilityConsumerSnapshot(baseCharacter);

            // TODO : still need to work out how unit profile fits into this
            /*
            if (baseCharacter.UnitProfile != null) {
                LearnCapabilityProviderAbilities(baseCharacter.UnitProfile);
            }
            */
            LearnCapabilityProviderAbilities(capabilityConsumerSnapshot.GetAbilityList());
        }

        public bool LearnAbility(BaseAbility newAbility) {
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
            if (!HasAbility(newAbility) && newAbility.RequiredLevel <= BaseCharacter.CharacterStats.Level) {
                abilityList[SystemResourceManager.prepareStringForMatch(newAbility.DisplayName)] = newAbility;
                if (isAutoAttack) {
                    //Debug.Log(gameObject.name + ".CharacterAbilityManager.LearnAbility(" + (newAbility == null ? "null" : newAbility.MyName) + "): setting auto-attack ability");
                    autoAttackAbility = newAbility;
                }
                OnLearnAbility(newAbility);
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

        public void UnlearnAbility(BaseAbility oldAbility, bool updateActionBars = true) {
            string keyName = SystemResourceManager.prepareStringForMatch(oldAbility.DisplayName);
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
            OnUnlearnAbility(updateActionBars);
        }

        /// <summary>
        /// Cast a spell with a cast timer
        /// </summary>
        /// <param name="ability"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public IEnumerator PerformAbilityCast(BaseAbility ability, Interactable target, AbilityEffectContext abilityEffectContext) {
            float startTime = Time.time;
            //Debug.Log(gameObject.name + "CharacterAbilitymanager.PerformAbilityCast(" + ability.DisplayName + ", " + (target == null ? "null" : target.name) + ") Enter Ienumerator with tag: " + startTime);
            bool canCast = true;
            if (ability.RequiresTarget == false || ability.CanCastOnEnemy == false) {
                // prevent the killing of your enemy target from stopping aoe casts and casts that cannot be cast on an ememy
                KillStopCastOverride();
            } else {
                KillStopCastNormal();
            }
            abilityEffectContext.originalTarget = target;
            if (ability.RequiresGroundTarget == true) {
                //Debug.Log("CharacterAbilitymanager.PerformAbilityCast() Ability requires a ground target.");
                ActivateTargettingMode(ability, target);
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
                    ability.StartCasting(baseCharacter);
                }
                float currentCastPercent = 0f;
                float nextTickPercent = 0f;
                //Debug.Log(gameObject.name + ".CharacterAbilitymanager.PerformAbilityCast() currentCastTime: " + currentCastTime + "; MyAbilityCastingTime: " + ability.MyAbilityCastingTime);

                if (baseCharacter != null && ability.GetHoldableObjectList(baseCharacter).Count != 0) {
                    //Debug.Log(gameObject.name + ".CharacterAbilityManager.PerformAbilityCast(" + ability.MyName + "): spawning ability objects");
                    if (!ability.AnimatorCreatePrefabs) {
                        SpawnAbilityObjects(ability.GetHoldableObjectList(baseCharacter));
                    }
                }
                if (ability.CastingAudioClip != null) {
                    baseCharacter.UnitController.UnitComponentController.PlayCast(ability.CastingAudioClip);
                }
                

                // added target condition to allow channeled spells to stop casting if target disappears
                while (currentCastPercent < 1f
                    && (ability.RequiresTarget == false
                    || (target != null && target.gameObject.activeInHierarchy == true))) {
                    yield return null;
                    currentCastPercent += (Time.deltaTime / ability.GetAbilityCastingTime(baseCharacter));

                    // call this first because it updates the cast bar
                    //Debug.Log(gameObject.name + ".CharacterAbilitymanager.PerformAbilityCast() currentCastTime: " + currentCastTime + "; MyAbilityCastingTime: " + ability.MyAbilityCastingTime + "; calling OnCastTimeChanged()");
                    OnCastTimeChanged(baseCharacter, ability, currentCastPercent);
                    if (baseCharacter.UnitController != null) {
                        baseCharacter.UnitController.NotifyOnCastTimeChanged(baseCharacter, ability, currentCastPercent);
                    }

                    // now call the ability on casttime changed (really only here for channeled stuff to do damage)
                    nextTickPercent = ability.OnCastTimeChanged(currentCastPercent, nextTickPercent, baseCharacter, target, abilityEffectContext);
                }

            }

            //Debug.Log(gameObject + ".CharacterAbilityManager.PerformAbilityCast(). nulling tag: " + startTime);
            // set currentCast to null because it isn't automatically null until the next frame and we are about to do stuff which requires it to be null immediately
            EndCastCleanup();

            if (canCast) {
                //Debug.Log(gameObject.name + ".CharacterAbilitymanager.PerformAbilityCast(): Cast Complete currentCastTime: " + currentCastTime + "; abilitycastintime: " + ability.MyAbilityCastingTime);
                if (!ability.CanSimultaneousCast) {
                    NotifyOnCastStop();
                    BaseCharacter.UnitController.UnitAnimator.SetCasting(false);
                }
                PerformAbility(ability, target, abilityEffectContext);

            }
        }

        public void NotifyOnCastStop() {
            OnCastStop(baseCharacter);
            if (baseCharacter.UnitController != null) {
                baseCharacter.UnitController.NotifyOnCastStop(baseCharacter);
            }
        }

        public void SpawnAbilityObjects(int indexValue = -1) {
            //Debug.Log(gameObject.name + ".CharacterAbilityManager.SpawnAbilityObjects(" + indexValue + ")");
            BaseAbility usedBaseAbility = null;
            if (BaseCharacter.UnitController.UnitAnimator.MyCurrentAbilityEffectContext != null) {
                usedBaseAbility = BaseCharacter.UnitController.UnitAnimator.MyCurrentAbilityEffectContext.baseAbility;
            }
            if (usedBaseAbility == null) {
                usedBaseAbility = currentCastAbility;
            }

            if (baseCharacter != null &&
                baseCharacter.CharacterEquipmentManager != null &&
                usedBaseAbility != null &&
                usedBaseAbility.GetHoldableObjectList(baseCharacter).Count != 0) {
                //if (baseCharacter != null && baseCharacter.MyCharacterEquipmentManager != null && ability.MyAbilityCastingTime > 0f && ability.MyHoldableObjectNames.Count != 0) {
                //Debug.Log(gameObject.name + ".CharacterAbilityManager.PerformAbilityCast(): spawning ability objects");
                if (usedBaseAbility.AnimatorCreatePrefabs) {
                    if (indexValue == -1) {
                        SpawnAbilityObjects(usedBaseAbility.GetHoldableObjectList(baseCharacter));
                    } else {
                        List<AbilityAttachmentNode> passList = new List<AbilityAttachmentNode>();
                        passList.Add(usedBaseAbility.GetHoldableObjectList(baseCharacter)[indexValue - 1]);
                        SpawnAbilityObjects(passList);
                    }
                }
            }

        }

        public override void EndCastCleanup() {
            base.EndCastCleanup();
            if (baseCharacter.UnitController != null) {
                baseCharacter.UnitController.UnitComponentController.StopCast();
            }
        }

        public void ReceiveKillDetails(BaseCharacter killedcharacter, float creditPercent) {
            //Debug.Log("CharacterAbilityManager.ReceiveKillDetails()");
            if (BaseCharacter.UnitController.Target == killedcharacter.UnitController) {
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
        public void BeginAbility(BaseAbility ability) {
            //Debug.Log(baseCharacter.gameObject.name + "CharacterAbilitymanager.BeginAbility(" + (ability == null ? "null" : ability.DisplayName) + ")");
            if (ability == null) {
                //Debug.Log("CharacterAbilityManager.BeginAbility(): ability is null! Exiting!");
                return;
            } else {
                //Debug.Log("CharacterAbilityManager.BeginAbility(" + ability.MyName + ")");
            }
            BeginAbilityCommon(ability, baseCharacter.UnitController.Target);
        }

        public void BeginAbility(BaseAbility ability, Interactable target) {
            //Debug.Log(gameObject.name + ".CharacterAbilityManager.BeginAbility(" + ability.MyName + ")");
            BeginAbilityCommon(ability, target);
        }

        public override float GetSpeed() {
            return 1f / (baseCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Speed].CurrentValue / 100f);
        }

        public override float GetAnimationLengthMultiplier() {
            if (baseCharacter != null && baseCharacter.UnitController != null && baseCharacter.UnitController.UnitAnimator != null) {
                return (baseCharacter.UnitController.UnitAnimator.LastAnimationLength / (float)baseCharacter.UnitController.UnitAnimator.LastAnimationHits);
            }
            return base.GetAnimationLengthMultiplier();
        }

        public override float GetOutgoingDamageModifiers() {
            if (baseCharacter != null && baseCharacter.CharacterStats != null) {
                return baseCharacter.CharacterStats.GetOutGoingDamageModifiers();
            }
            return base.GetOutgoingDamageModifiers();
        }

        public override void ProcessWeaponHitEffects(AttackEffect attackEffect, Interactable target, AbilityEffectContext abilityEffectOutput) {
            base.ProcessWeaponHitEffects(attackEffect, target, abilityEffectOutput);
            // handle weapon on hit effects
            if (baseCharacter.CharacterCombat != null
                && baseCharacter.CharacterCombat.OnHitEffects != null
                && attackEffect.DamageType == DamageType.physical) {
                List<AbilityEffect> onHitEffectList = new List<AbilityEffect>();
                foreach (AttackEffect _attackEffect in baseCharacter.CharacterCombat.OnHitEffects) {
                    // prevent accidental infinite recursion of ability effect
                    if (_attackEffect.DisplayName != attackEffect.DisplayName) {
                        onHitEffectList.Add(_attackEffect);
                    }
                }
                attackEffect.PerformAbilityEffects(baseCharacter, target, abilityEffectOutput, onHitEffectList);
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

                // not needed anymore after stat system upgrade ?
                // +damage stat from gear
                //float returnValue = GetPhysicalPower();

                float returnValue = 0f;

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
                return LevelEquations.GetSecondaryStatForCharacter(SecondaryStatType.PhysicalDamage, baseCharacter.CharacterStats) + LevelEquations.GetSecondaryStatForCharacter(SecondaryStatType.Damage, baseCharacter.CharacterStats);
            }
            return base.GetPhysicalPower();
        }

        public override float GetSpellPower() {
            if (baseCharacter != null) {
                return LevelEquations.GetSecondaryStatForCharacter(SecondaryStatType.SpellDamage, baseCharacter.CharacterStats) + LevelEquations.GetSecondaryStatForCharacter(SecondaryStatType.Damage, baseCharacter.CharacterStats);
            }
            return base.GetSpellPower();
        }

        public override float GetCritChance() {
            if (baseCharacter != null) {
                return LevelEquations.GetSecondaryStatForCharacter(SecondaryStatType.CriticalStrike, baseCharacter.CharacterStats);
            }
            return base.GetCritChance();
        }

        protected void BeginAbilityCommon(BaseAbility ability, Interactable target) {
            //Debug.Log(gameObject.name + ".CharacterAbilityManager.BeginAbilityCommon(" + (ability == null ? "null" : ability.DisplayName) + ", " + (target == null ? "null" : target.name) + ")");
            BaseAbility usedAbility = SystemAbilityManager.MyInstance.GetResource(ability.DisplayName);
            if (usedAbility == null) {
                Debug.LogError("CharacterAbilityManager.BeginAbilityCommon(" + (ability == null ? "null" : ability.DisplayName) + ", " + (target == null ? "null" : target.name) + ") NO ABILITY FOUND");
                return;
            }

            if (!CanCastAbility(usedAbility)) {
                //Debug.Log(gameObject.name + ".CharacterAbilityManager.BeginAbilityCommon(" + ability.DisplayName + ", " + (target != null ? target.name : "null") + ") cannot cast");
                return;
            }

            AbilityEffectContext abilityEffectContext = new AbilityEffectContext();
            abilityEffectContext.baseAbility = ability;

            // get final target before beginning casting
            Interactable finalTarget = usedAbility.ReturnTarget(baseCharacter, target, true, abilityEffectContext);


            CharacterUnit targetCharacterUnit = null;
            if (finalTarget != null) {
                targetCharacterUnit = CharacterUnit.GetCharacterUnit(finalTarget);
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

            OnAttemptPerformAbility(ability);

            if (finalTarget == null && usedAbility.RequiresTarget == true) {
                //Debug.Log(gameObject.name + ".CharacterAbilityManager.BeginAbilityCommon(): finalTarget is null. exiting");
                return;
            }
            if (finalTarget != null && PerformLOSCheck(finalTarget, usedAbility as ITargetable) == false) {
                Debug.Log("CharacterAbilityManager.BeginAbilityCommon(): LOS check failed. exiting");
                return;
            }

            baseCharacter.UnitController.CancelMountEffects();

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
                    if (BaseCharacter != null && BaseCharacter.UnitController != null && BaseCharacter.UnitController.UnitAnimator != null) {
                        BaseCharacter.UnitController.UnitAnimator.ClearAnimationBlockers();
                    }

                    // start the cast (or cast targetting projector)
                    currentCastCoroutine = abilityCaster.StartCoroutine(PerformAbilityCast(usedAbility, finalTarget, abilityEffectContext));
                    currentCastAbility = usedAbility;
                } else {
                    //CombatLogUI.MyInstance.WriteCombatMessage("A cast was already in progress WE SHOULD NOT BE HERE BECAUSE WE CHECKED FIRST! iscasting: " + isCasting + "; currentcast==null? " + (currentCast == null));
                    // unless.... we got here from the crafting queue, which launches the next item as the last step of the currently in progress cast
                    //Debug.Log(gameObject.name + ".CharacterAbilityManager.BeginAbilityCommon(): A cast was already in progress!");
                }
            }

            if (baseCharacter != null && baseCharacter.UnitController != null && baseCharacter.UnitController.UnitControllerMode == UnitControllerMode.AI) {
                if (currentCastAbility != null && currentCastAbility.RequiresGroundTarget == true) {
                    Vector3 groundTarget = Vector3.zero;
                    if (baseCharacter.UnitController.Target != null) {
                        groundTarget = baseCharacter.UnitController.Target.transform.position;
                    }
                    SetGroundTarget(groundTarget);
                }

            }
        }

        // this only checks if the ability is able to be cast based on character state.  It does not check validity of target or ability specific requirements
        public override bool CanCastAbility(BaseAbility ability) {
            //Debug.Log(gameObject.name + ".CharacterAbilityManager.CanCastAbility(" + ability.DisplayName + ")");

            // check if the ability is learned yet
            if (!PerformLearnedCheck(ability)) {
                //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.CanCastAbility(" + ability.DisplayName + "): Have not learned ability!");
                return false;
            }

            // check if the ability is on cooldown
            if (!PerformCooldownCheck(ability)) {
                //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.CanCastAbility(" + ability.DisplayName + "): ability is on cooldown!");
                return false;
            }

            // check if we have enough mana
            if (!PerformPowerResourceCheck(ability)) {
                //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.CanCastAbility(" + ability.DisplayName + "): do not have sufficient power resource to cast!");
                return false;
            }

            if (!PerformCombatCheck(ability)) {
                //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.CanCastAbility(" + ability.DisplayName + "): cannot cast ability in combat!");
                return false;
            }

            if (!PerformLivenessCheck(ability)) {
                //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.CanCastAbility(" + ability.DisplayName + "): cannot cast while dead!");
                return false;
            }
            
            if (!PerformMovementCheck(ability)) {
                //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.CanCastAbility(" + ability.DisplayName + "): velocity too high to cast!");
                return false;
            }


            // default is true, nothing has stopped us so far
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.CanCastAbility(" + ability.DisplayName + "): returning true");
            return base.CanCastAbility(ability);
        }

        public bool PerformLivenessCheck(BaseAbility ability) {
            if (!baseCharacter.CharacterStats.IsAlive) {
                return false;
            }
            return true;
        }

        public bool PerformMovementCheck(BaseAbility ability) {
            if (ability.CanCastWhileMoving || ability.GetAbilityCastingTime(baseCharacter) == 0f) {
                return true;
            }
            return !(baseCharacter.UnitController.ApparentVelocity > 0.1f);
        }

        public bool PerformLearnedCheck(BaseAbility ability) {

            string keyName = SystemResourceManager.prepareStringForMatch(ability.DisplayName);

            if (!ability.UseableWithoutLearning && !AbilityList.ContainsKey(keyName)) {
                OnLearnedCheckFail(ability);
                return false;
            }
            return true;
        }

        public bool PerformCooldownCheck(BaseAbility ability) {
            //Debug.Log(gameObject.name + ".CharacterAbilityManager.PerformCooldownCheck(" + ability.DisplayName + ") : global: " + MyRemainingGlobalCoolDown);
            if (abilityCoolDownDictionary.ContainsKey(ability.DisplayName) ||
                (MyRemainingGlobalCoolDown > 0f && ability.IgnoreGlobalCoolDown == false)) {
                return false;
            }
            return true;
        }

        public bool PerformCombatCheck(BaseAbility ability) {
            if (ability.RequireOutOfCombat == true && BaseCharacter.CharacterCombat.GetInCombat() == true) {
                OnCombatCheckFail(ability);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Check if the caster has the required amount of the power resource to cast the ability
        /// </summary>
        /// <param name="ability"></param>
        /// <returns></returns>
        public bool PerformPowerResourceCheck(BaseAbility ability) {
            if (BaseCharacter.CharacterStats.PerformPowerResourceCheck(ability, ability.GetResourceCost(baseCharacter))) {
                return true;
            }
            OnPowerResourceCheckFail(ability, baseCharacter);
            return false;
        }

        /// <summary>
        /// Casts a spell.  Note that this does not do the actual damage yet since the ability may have a travel time
        /// </summary>
        /// <param name="ability"></param>
        /// <param name="target"></param>
        public void PerformAbility(BaseAbility ability, Interactable target, AbilityEffectContext abilityEffectContext) {
            //Debug.Log(gameObject.name + ".CharacterAbilityManager.PerformAbility(" + ability.DisplayName + ")");
            if (abilityEffectContext == null) {
                abilityEffectContext = new AbilityEffectContext();
                abilityEffectContext.baseAbility = ability;
            }
            abilityEffectContext.originalTarget = target;
            Interactable finalTarget = target;
            if (finalTarget != null) {
                //Debug.Log(gameObject.name + ": performing ability: " + ability.MyName + " on " + finalTarget.name);
            } else {
                //Debug.Log(gameObject.name + ": performing ability: " + ability.MyName + ": finalTarget is null");
            }

            if (!PerformPowerResourceCheck(ability)) {
                return;
            }

            if (ability.GetResourceCost(baseCharacter) != 0 && ability.PowerResource != null) {
                // intentionally not keeping track of this coroutine.  many of these could be in progress at once.
                abilityEffectContext.AddResourceAmount(ability.PowerResource.DisplayName, ability.GetResourceCost(baseCharacter));
                abilityCaster.StartCoroutine(UsePowerResourceDelay(ability.PowerResource, (int)ability.GetResourceCost(baseCharacter), ability.SpendDelay));
            }

            // cast the system manager version so we can track globally the spell cooldown
            SystemAbilityManager.MyInstance.GetResource(ability.DisplayName).Cast(baseCharacter, finalTarget, abilityEffectContext);
            //ability.Cast(MyBaseCharacter.MyCharacterUnit.gameObject, finalTarget);
            OnPerformAbility(ability);
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
            if (BaseCharacter.UnitController.ApparentVelocity > 0.1f) {
                //Debug.Log("CharacterAbilityManager.HandleManualMovement(): stop casting");
                if (currentCastAbility != null && currentCastAbility.RequiresGroundTarget == true && CastTargettingManager.MyInstance.ProjectorIsActive() == true) {
                    // do nothing
                    //Debug.Log("CharacterAbilityManager.HandleManualMovement(): not cancelling casting because we have a ground target active");
                } else {
                    StopCasting();
                }
            } else {
                //Debug.Log("CharacterAbilityManager.HandleManualMovement(): velocity too low, doing nothing");
            }
        }

        public void StopCasting() {
            //Debug.Log(gameObject.name + ".CharacterAbilityManager.StopCasting()");
            if (currentCastCoroutine != null) {
                // REMOVED ISCASTING == TRUE BECAUSE IT WAS PREVENTING THE CRAFTING QUEUE FROM WORKING.  TECHNICALLY THIS GOT CALLED RIGHT AFTER ISCASTING WAS SET TO FALSE, BUT BEFORE CURRENTCAST WAS NULLED
                //if (currentCast != null && isCasting == true) {
                //Debug.Log(gameObject.name + ".CharacterAbilityManager.StopCasting(): currentCast is not null, stopping coroutine");
                abilityCaster.StopCoroutine(currentCastCoroutine);
                EndCastCleanup();
                if (baseCharacter != null && baseCharacter.CharacterEquipmentManager != null) {
                    DespawnAbilityObjects();
                }

            } else {
                //Debug.Log(gameObject.name + ".currentCast is null, nothing to stop");
            }
            if (BaseCharacter.UnitController != null && BaseCharacter.UnitController.UnitAnimator != null) {
                BaseCharacter.UnitController.UnitAnimator.ClearAnimationBlockers();
            }
            NotifyOnCastStop();
        }

        public void ProcessLevelUnload() {
            StopCasting();
            WaitingForAnimatedAbility = false;
        }

        public override AudioClip GetAnimatedAbilityHitSound() {
            //Debug.Log(gameObject.name + ".CharacterAbilitymanager.GetAnimatedAbilityHitSound()");
            if (baseCharacter != null && baseCharacter.CharacterCombat != null) {
                return baseCharacter.CharacterCombat.DefaultHitSoundEffect;
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
                    baseCharacter.UnitController.UnitComponentController.PlayEffect(audioClip);
                }
            }
        }

        public override void InitiateGlobalCooldown(float coolDownToUse = 0f) {
            //Debug.Log(gameObject.name + ".CharacterAbilitymanager.InitiateGlobalCooldown(" + coolDownToUse + ")");
            base.InitiateGlobalCooldown(coolDownToUse);
            if (globalCoolDownCoroutine == null) {
                // set global cooldown length to animation length so we don't end up in situation where cast bars look fine, but we can't actually cast
                globalCoolDownCoroutine = abilityCaster.StartCoroutine(BeginGlobalCoolDown(coolDownToUse));
            } else {
                Debug.Log("CharacterAbilityManager.InitiateGlobalCooldown(): INVESTIGATE: GCD COROUTINE WAS NOT NULL");
            }
        }

        public IEnumerator BeginGlobalCoolDown(float coolDownTime) {
            //Debug.Log(gameObject.name + ".CharacterAbilityManager.BeginGlobalCoolDown(" + coolDownTime + ")");
            // 10 is kinda arbitrary, but if any animation is causing a GCD greater than 10 seconds, we've probably got issues anyway...
            // the current longest animated attack is ground slam at around 4 seconds
            remainingGlobalCoolDown = Mathf.Clamp(coolDownTime, 1, 10);
            initialGlobalCoolDown = remainingGlobalCoolDown;
            while (remainingGlobalCoolDown > 0f) {
                yield return null;
                remainingGlobalCoolDown -= Time.deltaTime;
                // we want to end immediately if the time is up or the cooldown coroutine will not be nullifed until the next frame
                //Debug.Log(gameObject.name + ".CharacterAbilityManager.BeginGlobalCoolDown(): in loop; remaining time: " + remainingGlobalCoolDown);
            }
            globalCoolDownCoroutine = null;
        }

        public override void ProcessAbilityCoolDowns(AnimatedAbility baseAbility, float animationLength, float abilityCoolDown) {
            base.ProcessAbilityCoolDowns(baseAbility, animationLength, abilityCoolDown);
            if (baseCharacter != null && baseCharacter.UnitController != null && baseCharacter.UnitController.UnitControllerMode == UnitControllerMode.Player) {
                if (SystemConfigurationManager.MyInstance.MyAllowAutoAttack == true && baseAbility.IsAutoAttack) {
                    return;
                }
            }

            //Debug.Log(baseAbility.MyName + ".Cast(): Setting GCD for length: " + animationLength);
            baseAbility.ProcessGCDManual(baseCharacter, Mathf.Min(animationLength, abilityCoolDown));
            BeginAbilityCoolDown(baseAbility, Mathf.Max(animationLength, abilityCoolDown));
        }

    }

}