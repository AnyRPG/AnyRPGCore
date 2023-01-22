using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class CharacterAbilityManager : AbilityManager {

        public event System.Action<BaseCharacter> OnAttack = delegate { };
        public event System.Action<IAbilityCaster, BaseAbilityProperties, float> OnCastTimeChanged = delegate { };
        public event System.Action<BaseCharacter> OnCastComplete = delegate { };
        public event System.Action<BaseCharacter> OnCastCancel = delegate { };
        public event System.Action<BaseAbilityProperties> OnAttemptPerformAbility = delegate { };
        public event System.Action<BaseAbilityProperties> OnPerformAbility = delegate { };
        public event System.Action OnUnlearnAbilities = delegate { };
        public event System.Action<BaseAbilityProperties> OnLearnedCheckFail = delegate { };
        public event System.Action<BaseAbilityProperties> OnCombatCheckFail = delegate { };
        public event System.Action<BaseAbilityProperties> OnStealthCheckFail = delegate { };
        public event System.Action<AnimatedAbilityProperties> OnAnimatedAbilityCheckFail = delegate { };
        public event System.Action<BaseAbilityProperties, IAbilityCaster> OnPowerResourceCheckFail = delegate { };
        public event System.Action<BaseAbilityProperties, Interactable> OnTargetInAbilityRangeFail = delegate { };
        public event System.Action<bool> OnUnlearnAbility = delegate { };
        public event System.Action<BaseAbilityProperties> OnLearnAbility = delegate { };
        public event System.Action<BaseAbilityProperties> OnActivateTargetingMode = delegate { };
        public event System.Action<string> OnCombatMessage = delegate { };
        public event System.Action<string> OnMessageFeedMessage = delegate { };
        public event System.Action OnBeginAbilityCoolDown = delegate { };

        protected BaseCharacter baseCharacter;

        protected Dictionary<string, BaseAbilityProperties> abilityList = new Dictionary<string, BaseAbilityProperties>();

        protected Vector3 groundTarget = Vector3.zero;

        protected bool targettingModeActive = false;

        // does killing the player you are currently targetting stop your cast.  gets set to false when channeling aoe.
        // disabled to prevent weapon going out of character hand mid animation swing if mob dies while swinging
        //private bool killStopCast = true;

        protected float remainingGlobalCoolDown = 0f;

        // we need a reference to the total length of the current global cooldown to properly calculate radial fill on the action buttons
        protected float initialGlobalCoolDown;

        // the auto-attack ability provided by capablity providers
        protected BaseAbilityProperties autoAttackAbility = null;

        // an auto-attack override provided by the currently equipped weapon
        //protected BaseAbilityProperties autoAttackOverride = null;

        // waiting for the animator to let us know we can hit again
        protected bool performingAutoAttack = false;
        protected bool performingCast = false;
        protected bool performingAnimatedAbility = false;

        protected Coroutine attackCoroutine = null;

        // a reference to any current ability we are casting
        private AbilityEffectContext currentAbilityEffectContext = null;


        // the holdable objects spawned during an ability cast and removed when the cast is complete
        protected Dictionary<AbilityAttachmentNode, List<GameObject>> abilityObjects = new Dictionary<AbilityAttachmentNode, List<GameObject>>();

        // game manager references
        private PlayerManager playerManager = null;
        private CastTargettingManager castTargettingManager = null;

        public float InitialGlobalCoolDown { get => initialGlobalCoolDown; set => initialGlobalCoolDown = value; }
        public float RemainingGlobalCoolDown { get => remainingGlobalCoolDown; set => remainingGlobalCoolDown = value; }
        public AbilityEffectContext CurrentAbilityEffectContext { get => currentAbilityEffectContext; set => currentAbilityEffectContext = value; }

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

        public override bool ControlLocked {
            get {
                if (baseCharacter?.UnitController != null) {
                    return baseCharacter.UnitController.ControlLocked;
                }
                return base.ControlLocked;
            }
        }

        public override bool PerformingAbility {
            get {
                if (performingAnimatedAbility == true) {
                    return true;
                }
                if (performingCast == true) {
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

        public Dictionary<string, BaseAbilityProperties> AbilityList {
            get {
                Dictionary<string, BaseAbilityProperties> returnAbilityList = new Dictionary<string, BaseAbilityProperties>();
                foreach (string abilityName in abilityList.Keys) {
                    if ((abilityList[abilityName].CharacterClassRequirementList == null || abilityList[abilityName].CharacterClassRequirementList.Count == 0 || abilityList[abilityName].CharacterClassRequirementList.Contains(baseCharacter.CharacterClass))
                        && (abilityList[abilityName].ClassSpecializationRequirementList == null || abilityList[abilityName].ClassSpecializationRequirementList.Count == 0 || abilityList[abilityName].ClassSpecializationRequirementList.Contains(baseCharacter.ClassSpecialization))) {
                        returnAbilityList.Add(abilityName, abilityList[abilityName]);
                    }
                }
                return returnAbilityList;
            }

        }

        public Dictionary<string, AbilityCoolDownNode> AbilityCoolDownDictionary { get => abilityCoolDownDictionary; set => abilityCoolDownDictionary = value; }
        public Coroutine CurrentCastCoroutine { get => currentCastCoroutine; }
        public BaseAbilityProperties AutoAttackAbility {
            get {
                /*
                if (autoAttackOverride != null) {
                    return autoAttackOverride;
                }
                */
                return autoAttackAbility;
            }
            set => autoAttackAbility = value;
        }

        public bool PerformingAutoAttack {
            get => performingAutoAttack;
        }

        // direct access for save manager so we don't miss saving abilities we know but belong to another class
        public override Dictionary<string, BaseAbilityProperties> RawAbilityList { get => abilityList; }

        public CharacterAbilityManager(BaseCharacter baseCharacter, SystemGameManager systemGameManager) : base(baseCharacter, systemGameManager) {
            this.baseCharacter = baseCharacter;
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            playerManager = systemGameManager.PlayerManager;
            castTargettingManager = systemGameManager.CastTargettingManager;
        }

        public void Init() {
            // testing - disable this because there is no way in which any unit profile properties are set at this point
            //UpdateAbilityList(baseCharacter.CharacterStats.Level);
            LearnDefaultAutoAttackAbility();
        }

        public bool PerformingAnyAbility() {
            if (performingAnimatedAbility == true || performingAutoAttack == true || performingCast == true) {
                // can't auto-attack during auto-attack, animated attack, or cast
                return true;
            }
            return false;
        }

        public override CharacterUnit GetCharacterUnit() {
            return baseCharacter?.UnitController?.CharacterUnit;
        }

        public override void SetMountedState(UnitController mountUnitController, UnitProfile mountUnitProfile) {
            base.SetMountedState(mountUnitController, mountUnitProfile);
            if (baseCharacter != null && baseCharacter.UnitController != null) {
                baseCharacter.UnitController.SetMountedState(mountUnitController, mountUnitProfile);
            }
        }

        public override List<AbilityEffectProperties> GetDefaultHitEffects() {
            if (baseCharacter.CharacterCombat.DefaultHitEffects.Count > 0) {
                return baseCharacter.CharacterCombat.DefaultHitEffects;
            }
            return base.GetDefaultHitEffects();
        }

        public override List<AbilityAttachmentNode> GetWeaponAbilityAnimationObjectList() {
            if (baseCharacter.CharacterEquipmentManager != null) {
                return baseCharacter.CharacterEquipmentManager.WeaponAbilityAnimationObjects;
            }
            return base.GetWeaponAbilityAnimationObjectList();
        }

        public override List<AbilityAttachmentNode> GetWeaponAbilityObjectList() {
            if (baseCharacter.CharacterEquipmentManager != null) {
                return baseCharacter.CharacterEquipmentManager.WeaponAbilityObjects;
            }
            return base.GetWeaponAbilityObjectList();
        }

        public override AttachmentPointNode GetHeldAttachmentPointNode(AbilityAttachmentNode attachmentNode) {
            if (attachmentNode.UseUniversalAttachment == false) {
                AttachmentPointNode attachmentPointNode = new AttachmentPointNode();
                attachmentPointNode.TargetBone = attachmentNode.HoldableObject.TargetBone;
                attachmentPointNode.Position = attachmentNode.HoldableObject.Position;
                attachmentPointNode.Rotation = attachmentNode.HoldableObject.Rotation;
                attachmentPointNode.RotationIsGlobal = attachmentNode.HoldableObject.RotationIsGlobal;
                attachmentPointNode.Scale = attachmentNode.HoldableObject.Scale;
                return attachmentPointNode;
            } else {
                // find unit profile, find prefab profile, find universal attachment profile, find universal attachment node
                if (baseCharacter?.UnitProfile?.UnitPrefabProps.AttachmentProfile != null) {
                    if (baseCharacter.UnitProfile.UnitPrefabProps.AttachmentProfile.AttachmentPointDictionary.ContainsKey(attachmentNode.AttachmentName)) {
                        return baseCharacter.UnitProfile.UnitPrefabProps.AttachmentProfile.AttachmentPointDictionary[attachmentNode.AttachmentName];
                    }
                }
            }

            return null;
        }

        public void HoldObject(GameObject go, AbilityAttachmentNode attachmentNode, GameObject searchObject) {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.HoldObject(" + go.name + ", " + searchObject.name + ")");
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
                // this code appears to have been copied from equipmentmanager (now in mecanimModelController) so the below line is false ?
                // disabled message because some equipment (like quivers) does not have held attachment points intentionally because it should stay in the same place in combat
                //Debug.Log(baseCharacter.gameObject + ".CharacterEquipmentManager.HoldObject(): Unable to get attachment point");
                // testing because this is ability manager, if no attachment point node or target bone was found, set rotation to match parent
                go.transform.rotation = searchObject.transform.rotation;
            }
        }

        public void SpawnAbilityObjects(List<AbilityAttachmentNode> abilityAttachmentNodes) {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.SpawnAbilityObjects()");

            // ensure that any current ability objects are cleared before spawning new ones
            DespawnAbilityObjects();

            Dictionary<AbilityAttachmentNode, GameObject> holdableObjects = new Dictionary<AbilityAttachmentNode, GameObject>();
            foreach (AbilityAttachmentNode abilityAttachmentNode in abilityAttachmentNodes) {
                if (abilityAttachmentNode != null) {
                    if (abilityAttachmentNode.HoldableObject != null && abilityAttachmentNode.HoldableObject.Prefab != null) {
                        //Debug.Log("EquipmentManager.HandleWeaponSlot(): " + newItem.name + " has a physical prefab");
                        // attach a mesh to a bone for weapons

                        AttachmentPointNode attachmentPointNode = GetHeldAttachmentPointNode(abilityAttachmentNode);
                        if (attachmentPointNode != null) {
                            Transform targetBone = baseCharacter.UnitController.transform;
                            if (attachmentPointNode.TargetBone != null && attachmentPointNode.TargetBone != string.Empty) {
                                targetBone = baseCharacter.UnitController.transform.FindChildByRecursive(attachmentPointNode.TargetBone);
                            }

                            if (targetBone != null) {
                                //Debug.Log("CharacterAbilityManager.SpawnAbilityObjects(): targetbone (" + attachmentPointNode.TargetBone + ") is " + targetBone.gameObject.name);
                                GameObject newEquipmentPrefab = objectPooler.GetPooledObject(abilityAttachmentNode.HoldableObject.Prefab, targetBone);
                                //holdableObjects.Add(attachmentNode.MyHoldableObject, newEquipmentPrefab);
                                holdableObjects.Add(abilityAttachmentNode, newEquipmentPrefab);
                                //currentEquipmentPhysicalObjects[equipmentSlotProfile] = newEquipmentPrefab;

                                newEquipmentPrefab.transform.localScale = abilityAttachmentNode.HoldableObject.Scale;
                                HoldObject(newEquipmentPrefab, abilityAttachmentNode, baseCharacter.UnitController.gameObject);
                            } else {
                                Debug.Log(baseCharacter.gameObject.name + "CharacterAbilityManager.SpawnAbilityObjects(). We could not find the target bone " + attachmentPointNode.TargetBone + " while attempting to hold " + abilityAttachmentNode.HoldableObject.DisplayName);
                            }
                        }
                    }
                }
            }
            if (holdableObjects.Count > 0) {
                foreach (AbilityAttachmentNode abilityAttachmentNode in holdableObjects.Keys) {
                    AddAbilityObject(abilityAttachmentNode, holdableObjects[abilityAttachmentNode]);
                }
                //abilityObjects = holdableObjects;
            }

        }

        public override void AddAbilityObject(AbilityAttachmentNode abilityAttachmentNode, GameObject go) {
            base.AddAbilityObject(abilityAttachmentNode, go);
            if (abilityObjects.ContainsKey(abilityAttachmentNode)) {
                abilityObjects[abilityAttachmentNode].Add(go);
            } else {
                abilityObjects.Add(abilityAttachmentNode, new List<GameObject>() { go });
            }
        }

        public override void DespawnAbilityObjects() {
            //Debug.Log(gameObject.name + ".CharacterAbilityManager.DespawnAbilityObjects()");
            base.DespawnAbilityObjects();

            if (abilityObjects == null || abilityObjects.Count == 0) {
                return;
            }

            foreach (List<GameObject> abilityObjectPrefabs in abilityObjects.Values) {
                if (abilityObjectPrefabs != null) {
                    foreach (GameObject abilityObject in abilityObjectPrefabs) {
                        if (abilityObject != null) {
                            objectPooler.ReturnObjectToPool(abilityObject);
                        }
                    }
                }
            }
            abilityObjects.Clear();
        }

        public override void GeneratePower(BaseAbilityProperties ability) {
            //Debug.Log(gameObject.name + ".CharacterAbilityManager.GeneratePower(" + ability.DisplayName + ")");
            if (ability.GeneratePowerResource == null) {
                // nothing to generate
                return;
            }
            base.GeneratePower(ability);
            if (baseCharacter != null && baseCharacter.CharacterStats != null) {
                //Debug.Log(gameObject.name + ".GeneratePower(" + ability.DisplayName + "): name " + ability.GeneratePowerResource.DisplayName  + "; " + ability.GetResourceGain(this));
                baseCharacter.CharacterStats.AddResourceAmount(ability.GeneratePowerResource.DisplayName, ability.GetResourceGain(baseCharacter));
            }
        }

        public override List<AnimationClip> GetDefaultAttackAnimations() {
            //Debug.Log(gameObject.name + ".GetDefaultAttackAnimations()");
            if (AutoAttackAbility != null) {
                return AutoAttackAbility.AttackClips;
            }
            return base.GetDefaultAttackAnimations();
        }

        /// <summary>
        /// get the current attack animations, accounting for any equippped weapon
        /// </summary>
        /// <returns></returns>
        public override List<AnimationClip> GetUnitAttackAnimations() {
            //Debug.Log(gameObject.name + ".GetDefaultAttackAnimations()");
            if (baseCharacter?.UnitController?.UnitAnimator?.CurrentAnimations != null) {
                return baseCharacter.UnitController.UnitAnimator.CurrentAnimations.AttackClips;
            }
            return base.GetUnitAttackAnimations();
        }

        public override AnimationProps GetUnitAnimationProps() {
            //Debug.Log(gameObject.name + ".GetDefaultAttackAnimations()");
            if (baseCharacter.UnitProfile?.UnitPrefabProps?.AnimationProps != null) {
                return baseCharacter.UnitProfile.UnitPrefabProps.AnimationProps;
            }
            if (systemConfigurationManager.DefaultAnimationProfile != null) {
                return systemConfigurationManager.DefaultAnimationProfile.AnimationProps;
            }
            return base.GetUnitAnimationProps();
        }

        public override List<AnimationClip> GetUnitCastAnimations() {
            //Debug.Log(gameObject.name + ".GetDefaultAttackAnimations()");
            if (baseCharacter?.UnitController?.UnitAnimator?.CurrentAnimations != null) {
                return baseCharacter.UnitController.UnitAnimator.CurrentAnimations.CastClips;
            }
            return base.GetUnitCastAnimations();
        }


        public override float GetMeleeRange() {
            if (baseCharacter?.UnitController.CharacterUnit != null) {
                return baseCharacter.UnitController.CharacterUnit.HitBoxSize;
            }
            return base.GetMeleeRange();
        }



        public override float GetThreatModifiers() {
            if (baseCharacter?.CharacterStats != null) {
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
                targetCharacterUnit.BaseCharacter.CharacterCombat.AggroTable.LockAgro();
            }

        }

        public override void PerformCastingAnimation(AnimationClip animationClip, BaseAbilityProperties baseAbility) {
            base.PerformCastingAnimation(animationClip, baseAbility);
            if (animationClip != null) {
                baseCharacter.UnitController.UnitAnimator.HandleCastingAbility(animationClip, baseAbility);
            }
        }

        public override void AddTemporaryPet(UnitProfile unitProfile, UnitController unitController) {
            base.AddTemporaryPet(unitProfile, unitController);
            baseCharacter.CharacterPetManager.AddTemporaryPet(unitProfile, unitController);
        }

        public override void CapturePet(UnitController targetUnitController) {
            base.CapturePet(targetUnitController);
            if (baseCharacter.CharacterPetManager != null && targetUnitController != null) {
                //Debug.Log(gameObject.name + ".CapturePet(): adding to pet manager");
                baseCharacter.CharacterPetManager.CapturePet(targetUnitController.UnitProfile, targetUnitController);
            }
        }

        /*
        public override bool IsTargetInAbilityRange(BaseAbility baseAbility, Interactable target, AbilityEffectContext abilityEffectContext = null) {
            // if none of those is true, then we are casting on ourselves, so don't need to do range check
            bool returnResult = IsTargetInRange(target, baseAbility.UseMeleeRange, baseAbility.MaxRange, baseAbility, abilityEffectContext);
            return returnResult;
        }

        public override bool IsTargetInAbilityEffectRange(AbilityEffect abilityEffect, Interactable target, AbilityEffectContext abilityEffectContext = null) {
            // if none of those is true, then we are casting on ourselves, so don't need to do range check
            return IsTargetInRange(target, abilityEffect.UseMeleeRange, abilityEffect.MaxRange, abilityEffect, abilityEffectContext);
        }
        */

        public override bool IsTargetInMeleeRange(Interactable target) {
            return baseCharacter.UnitController.IsTargetInHitBox(target);
        }

        public override bool PerformLOSCheck(Interactable target, ITargetable targetable, AbilityEffectContext abilityEffectContext = null) {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.PerformLOSCheck()");

            if (targetable.GetTargetOptions(baseCharacter).RequireLineOfSight == false) {
                return true;
            }

            Vector3 sourcePosition = abilityCaster.transform.position;
            // get initial positions in case of no collider
            if (targetable.GetTargetOptions(baseCharacter).LineOfSightSourceLocation == LineOfSightSourceLocation.Caster) {
                sourcePosition = abilityCaster.transform.position;
                Collider sourceCollider = abilityCaster.GetComponent<Collider>();
                if (sourceCollider != null) {
                    sourcePosition = sourceCollider.bounds.center;
                }
            } else if (targetable.GetTargetOptions(baseCharacter).LineOfSightSourceLocation == LineOfSightSourceLocation.GroundTarget && abilityEffectContext != null) {
                sourcePosition = abilityEffectContext.groundTargetLocation;
            } else if (targetable.GetTargetOptions(baseCharacter).LineOfSightSourceLocation == LineOfSightSourceLocation.OriginalTarget && abilityEffectContext != null) {
                sourcePosition = abilityEffectContext.originalTarget.transform.position;
            }

            Vector3 targetPosition = target.transform.position;

            Collider targetCollider = target.GetComponent<Collider>();
            if (targetCollider != null) {
                targetPosition = targetCollider.bounds.center;
            }

            Debug.DrawLine(sourcePosition, targetPosition, Color.cyan);

            int targetMask = 1 << target.gameObject.layer;
            int defaultMask = 1 << LayerMask.NameToLayer("Default");
            //int layerMask = (defaultMask | targetMask);

            // check if a wall was hit
            RaycastHit wallHit = new RaycastHit();
            if (Physics.Linecast(sourcePosition, targetPosition, out wallHit, defaultMask)) {
                //Debug.Log("hit: " + wallHit.transform.name);
                Debug.DrawRay(wallHit.point, wallHit.point - targetPosition, Color.red);
                if (wallHit.collider.gameObject != target.gameObject) {
                    //Debug.Log("return false; hit: " + wallHit.collider.gameObject + "; target: " + target);
                    return false;
                }
            }

            return base.PerformLOSCheck(target, targetable, abilityEffectContext);
        }

        public override bool IsTargetInRange(Interactable target, ITargetable targetable, AbilityEffectContext abilityEffectContext = null) {
            //Debug.Log(baseCharacter.gameObject.name + ".IsTargetInRange(" + (target == null ? "null" : target.DisplayName) + ")");
            // if none of those is true, then we are casting on ourselves, so don't need to do range check
            TargetProps targetProps = targetable.GetTargetOptions(baseCharacter);
            if (targetProps.UseMeleeRange) {
                if (!IsTargetInMeleeRange(target)) {
                    return false;
                }
            } else {
                if (!IsTargetInMaxRange(target, targetProps.MaxRange, targetable, abilityEffectContext)) {
                    return false;
                }
                if (targetProps.RequireLineOfSight == true && !PerformLOSCheck(target, targetable, abilityEffectContext)) {
                    return false;
                }
            }

            //Debug.Log(baseCharacter.gameObject.name + ".IsTargetInRange(): return true");
            return base.IsTargetInRange(target, targetable, abilityEffectContext);
        }

        public bool IsTargetInMaxRange(Interactable target, float maxRange, ITargetable targetable, AbilityEffectContext abilityEffectContext) {
            if (target == null || UnitGameObject == null) {
                return false;
            }
            Vector3 sourcePosition = UnitGameObject.transform.position;
            if (targetable.GetTargetOptions(baseCharacter).TargetRangeSourceLocation == TargetRangeSourceLocation.GroundTarget && abilityEffectContext != null) {
                sourcePosition = abilityEffectContext.groundTargetLocation;
            } else if (targetable.GetTargetOptions(baseCharacter).TargetRangeSourceLocation == TargetRangeSourceLocation.OriginalTarget && abilityEffectContext != null) {
                sourcePosition = abilityEffectContext.originalTarget.transform.position;
            }
            //Debug.Log(target.name + " range(" + maxRange + ": " + Vector3.Distance(UnitGameObject.transform.position, target.transform.position));
            if (maxRange > 0 && Vector3.Distance(sourcePosition, target.InteractableGameObject.transform.position) > maxRange) {
                //Debug.Log(target.name + " is out of range(" + maxRange + "): " + Vector3.Distance(UnitGameObject.transform.position, target.transform.position));
                return false;
            }

            return true;
        }

        public override float PerformAnimatedAbility(AnimationClip animationClip, AnimatedAbilityProperties animatedAbility, BaseCharacter targetBaseCharacter, AbilityEffectContext abilityEffectContext) {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.PerformAnimatedAbility(" + animatedAbility.DisplayName + ")");
            
            // this type of ability is allowed to interrupt other types of animations, so clear them all
            // is this really necessary ?  shouldn't checks have been performed before we got there as to whether anything specific was happening, and then cancel it already ?
            //TryToStopAnyAbility();

            // block further animations of other types from starting
            if (!animatedAbility.IsAutoAttack) {
                performingAnimatedAbility = true;
            } else {
                performingAutoAttack = true;
            }

            // reset animated ability timer
            baseCharacter.CharacterCombat.RegisterAnimatedAbilityBegin();

            // notify for attack sounds
            if (animatedAbility.PlayAttackVoice(baseCharacter.CharacterCombat) == true) {
                baseCharacter.UnitController?.UnitEventController.NotifyOnAttack();
            }

            float speedNormalizedAnimationLength = 1f;
            if (baseCharacter.CharacterStats != null) {
                speedNormalizedAnimationLength = (1f / (baseCharacter.CharacterStats.GetSpeedModifiers() / 100f)) * animationClip.length;
            }

            baseCharacter.CharacterCombat.SwingTarget = targetBaseCharacter;
            currentAbilityEffectContext = abilityEffectContext;

            baseCharacter.UnitController.UnitAnimator.HandleAbility(animationClip, animatedAbility);

            // wait for the attack to complete before allowing the character to attack again
            attackCoroutine = abilityCaster.StartCoroutine(WaitForAttackToComplete(animatedAbility, speedNormalizedAnimationLength));

            return speedNormalizedAnimationLength;
        }

        public IEnumerator WaitForAttackToComplete(AnimatedAbilityProperties animatedAbilityProperties, float animationLength) {
            //Debug.Log(unitController.gameObject.name + ".WaitForAnimation(" + baseAbility + ", " + animationLength + ", " + clearAutoAttack + ", " + clearAnimatedAttack + ", " + clearCasting + ")");
            float remainingTime = animationLength;
            //Debug.Log(gameObject.name + "waitforanimation remainingtime: " + remainingTime + "; MyWaitingForHits: " + PerformingAutoAttack + "; PerformingAnimatedAbility: " + performingAnimatedAbility);
            while (remainingTime > 0f && PerformingAnyAbility() == true) {
                //Debug.Log(gameObject.name + ".WaitForAttackToComplete(" + animationLength + "): remainingTime: " + remainingTime + "; PerformingAutoAttack: " + PerformingAutoAttack + "; PerformingAnimatedAbility: " + performingAnimatedAbility + "; animationSpeed: " + animator.GetFloat("AnimationSpeed"));
                yield return null;
                remainingTime -= Time.deltaTime;
            }

            ProcessAnimatedAbilityEnd();
        }

        /// <summary>
        /// Return true if the ability hit, false if it missed
        /// </summary>
        /// <returns></returns>
        public override bool AbilityHit(Interactable target, AbilityEffectContext abilityEffectContext) {
            if (baseCharacter.CharacterCombat.DidAttackMiss() == true) {
                //Debug.Log(DisplayName + ".BaseAbility.PerformAbilityHit(" + source.name + ", " + target.name + "): attack missed");
                baseCharacter.CharacterCombat.ReceiveCombatMiss(target, abilityEffectContext);
                if (target?.CharacterUnit != null) {
                    target.CharacterUnit.BaseCharacter.CharacterCombat.ReceiveCombatMiss(target, abilityEffectContext);
                }
                return false;
            }
            return base.AbilityHit(target, abilityEffectContext);
        }

        public override bool PerformAnimatedAbilityCheck(AnimatedAbilityProperties animatedAbility) {
            if (performingAnimatedAbility == true) {
                OnAnimatedAbilityCheckFail(animatedAbility);
                return false;
            }
            return base.PerformAnimatedAbilityCheck(animatedAbility);
        }

        public override bool ProcessAnimatedAbilityHit(Interactable target, bool deactivateAutoAttack) {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.ProcessAnimatedAbilityHit(" + (target == null ? "null" : target.gameObject.name) + ", " + deactivateAutoAttack + ")");
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

        public override bool PerformWeaponAffinityCheck(BaseAbilityProperties baseAbility, bool playerInitiated = false) {
            foreach (WeaponSkill _weaponAffinity in baseAbility.WeaponAffinityList) {
                if (baseCharacter != null && baseCharacter.CharacterEquipmentManager != null && baseCharacter.CharacterEquipmentManager.HasAffinity(_weaponAffinity)) {
                    return true;
                }
            }

            if (playerInitiated) {
                OnCombatMessage("Cannot cast " + baseAbility.DisplayName + ". Required weapon not equipped!");
            }
            // intentionally not calling base because it's always true
            return false;
        }

        public override void CleanupCoroutines() {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilitymanager.CleanupCoroutines()");
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
            if (targetableEffect.GetTargetOptions(baseCharacter).CanCastOnOthers == true
                && targetableEffect.GetTargetOptions(baseCharacter).CanCastOnEnemy == false
                && targetableEffect.GetTargetOptions(baseCharacter).CanCastOnNeutral == false
                && targetableEffect.GetTargetOptions(baseCharacter).CanCastOnFriendly == false) {
                return true;
            }

            float relationValue = Faction.RelationWith(targetCharacterUnit.BaseCharacter, baseCharacter);

            if (targetableEffect.GetTargetOptions(baseCharacter).CanCastOnEnemy == true && relationValue <= -1) {
                return true;
            }

            if (targetableEffect.GetTargetOptions(baseCharacter).CanCastOnNeutral == true && relationValue > -1 && targetableEffect.GetTargetOptions(baseCharacter).CanCastOnNeutral == true && relationValue < 1) {
                return true;
            }

            if (targetableEffect.GetTargetOptions(baseCharacter).CanCastOnFriendly == true && relationValue >= 1) {
                return true;
            }

            return false;

            //return base.PerformFactionCheck(targetableEffect, targetCharacterUnit, targetIsSelf);
        }

        public override void BeginAbilityCoolDown(BaseAbilityProperties baseAbility, float coolDownLength = -1f) {

            base.BeginAbilityCoolDown(baseAbility, coolDownLength);

            float abilityCoolDown = 0f;

            if (coolDownLength == -1f) {
                abilityCoolDown = baseAbility.CoolDown;
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
            abilityCoolDownNode.AbilityName = baseAbility.DisplayName;

            // need to account for auto-attack
            if (systemConfigurationManager.AllowAutoAttack == false && (baseAbility is AnimatedAbilityProperties) && (baseAbility as AnimatedAbilityProperties).IsAutoAttack == true) {
                abilityCoolDownNode.RemainingCoolDown = abilityCoolDown;
            } else {
                abilityCoolDownNode.RemainingCoolDown = abilityCoolDown;
            }

            abilityCoolDownNode.InitialCoolDown = abilityCoolDownNode.RemainingCoolDown;

            if (!abilityCoolDownDictionary.ContainsKey(baseAbility.DisplayName)) {
                abilityCoolDownDictionary[baseAbility.DisplayName] = abilityCoolDownNode;
            }

            // ordering important.  don't start till after its in the dictionary or it will fail to remove itself from the dictionary, then add it self
            Coroutine coroutine = abilityCaster.StartCoroutine(PerformAbilityCoolDown(baseAbility.DisplayName));
            abilityCoolDownNode.Coroutine = coroutine;

            OnBeginAbilityCoolDown();
        }

        public override void BeginActionCoolDown(IUseable useable, float coolDownLength = -1f) {

            base.BeginActionCoolDown(useable, coolDownLength);

            float coolDown = 0f;

            if (coolDownLength == -1f) {
                coolDown = useable.CoolDown;
            } else {
                coolDown = coolDownLength;
            }

            if (coolDown <= 0f) {
                // if the ability had no cooldown, and wasn't ignoring global cooldown, it gets a global cooldown length cooldown as we shouldn't have 0 cooldown instant cast abilities
                coolDown = Mathf.Clamp(coolDown, 1, Mathf.Infinity);
            }

            if (coolDown == 0f) {
                // if the ability CoolDown is still zero (this was a useable that doesn't need a cooldown), don't start cooldown coroutine
                return;
            }

            AbilityCoolDownNode abilityCoolDownNode = new AbilityCoolDownNode();
            abilityCoolDownNode.AbilityName = useable.DisplayName;
            abilityCoolDownNode.RemainingCoolDown = coolDown;
            abilityCoolDownNode.InitialCoolDown = abilityCoolDownNode.RemainingCoolDown;

            if (!abilityCoolDownDictionary.ContainsKey(useable.DisplayName)) {
                abilityCoolDownDictionary[useable.DisplayName] = abilityCoolDownNode;
            }

            // ordering important.  don't start till after its in the dictionary or it will fail to remove itself from the dictionary, then add it self
            Coroutine coroutine = abilityCaster.StartCoroutine(PerformAbilityCoolDown(useable.DisplayName));
            abilityCoolDownNode.Coroutine = coroutine;

            OnBeginAbilityCoolDown();
        }

        public void HandleEquipmentChanged(Equipment newItem, Equipment oldItem, int slotIndex) {
            //Debug.Log(gameObject.name + ".CharacterAbilityManager.HandleEquipmentChanged(" + (newItem != null ? newItem.DisplayName : "null") + ", " + (oldItem != null ? oldItem.DisplayName : "null") + ")");
            if (oldItem != null) {
                foreach (BaseAbilityProperties baseAbility in oldItem.LearnedAbilities) {
                    UnlearnAbility(baseAbility);
                }
            }
            UpdateEquipmentTraits(oldItem);

            if (newItem != null) {
                if (newItem.OnEquipAbility != null) {
                    if (baseCharacter.UnitController != null) {
                        BeginAbility(newItem.OnEquipAbility.AbilityProperties);
                    }
                }
                foreach (BaseAbilityProperties baseAbility in newItem.LearnedAbilities) {
                    baseAbility.PrepareToLearnAbility(this);
                    LearnAbility(baseAbility);
                }
            }

            // give a chance to learn default auto attack ability in case no other known ability is an auto-attack
            LearnDefaultAutoAttackAbility();

            // after equipment change, check all equipment sets and bonuses
            UpdateEquipmentTraits(newItem);
        }


        public void WeaponEquipped(Weapon weapon) {
            /*
            if (weapon != null && weapon.AutoAttackOverride != null) {
                autoAttackOverride = weapon.AutoAttackOverride;
            }
            */
        }

        public void WeaponUnequipped(Weapon weapon) {
            /*
            if (weapon != null && weapon.AutoAttackOverride != null) {
                autoAttackOverride = null;
            }
            */
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

            for (int i = 0; i < equipment.EquipmentSet.TraitList.Count; i++) {
                StatusEffectProperties statusEffect = equipment.EquipmentSet.TraitList[i];
                if (statusEffect != null) {
                    if (equipmentCount > i) {
                        // we are allowed to have this buff
                        if (!baseCharacter.CharacterStats.StatusEffects.ContainsKey(SystemDataFactory.PrepareStringForMatch(statusEffect.DisplayName))) {
                            ApplyStatusEffect(statusEffect);
                        }
                    } else {
                        // we are not allowed to have this buff
                        if (baseCharacter.CharacterStats.StatusEffects.ContainsKey(SystemDataFactory.PrepareStringForMatch(statusEffect.DisplayName))) {
                            baseCharacter.CharacterStats.StatusEffects[SystemDataFactory.PrepareStringForMatch(statusEffect.DisplayName)].CancelStatusEffect();
                        }
                    }
                }
            }

        }


        public void UnLearnDefaultAutoAttackAbility() {
            if (baseCharacter?.UnitProfile?.DefaultAutoAttackAbility != null) {
                UnlearnAbility(baseCharacter.UnitProfile.DefaultAutoAttackAbility);
            }
        }

        public void LearnDefaultAutoAttackAbility() {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.LearnDefaultAutoAttackAbility()");
            if (autoAttackAbility != null) {
                //Debug.Log(gameObject.name + ".CharacterAbilityManager.LearnDefaultAutoAttackAbility(): auto-attack already know, exiting");
                // can't learn two auto-attacks at the same time
                return;
            }
            if (baseCharacter.UnitProfile?.DefaultAutoAttackAbility != null) {
                //Debug.Log(gameObject.name + ".CharacterAbilityManager.LearnDefaultAutoAttackAbility(): learning default auto attack ability");
                LearnAbility(baseCharacter.UnitProfile.DefaultAutoAttackAbility);
            }
        }

        public void HandleCapabilityProviderChange(CapabilityConsumerSnapshot oldSnapshot, CapabilityConsumerSnapshot newSnapshot) {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.HandleCapabilityProviderChange()");


            // remove old abilities, making space on action bars
            UnLearnCapabilityProviderAbilities(oldSnapshot.GetAbilitiesToRemove(newSnapshot));
            RemoveCapabilityProviderTraits(oldSnapshot.GetTraitsToRemove(newSnapshot));


            // learn new abilites, now that there is space on the action bars
            ApplyCapabilityProviderTraits(newSnapshot.GetTraitList());
            LearnCapabilityProviderAbilities(newSnapshot.GetAbilityList());

        }

        public void ApplyCapabilityProviderTraits(List<StatusEffect> statusEffects) {
            if (statusEffects == null) {
                return;
            }
            foreach (StatusEffect statusEffect in statusEffects) {
                if (baseCharacter.CharacterStats.HasStatusEffect(statusEffect.AbilityEffectProperties.DisplayName) == false) {
                    ApplyStatusEffect(statusEffect.AbilityEffectProperties);
                }
            }
        }

        public void ApplyStatusEffect(AbilityEffectProperties statusEffect, int overrideDuration = 0) {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.ApplyStatusEffect(" + statusEffect.DisplayName + ")");
            if (baseCharacter.CharacterStats != null) {
                AbilityEffectContext abilityEffectContext = new AbilityEffectContext(baseCharacter);
                abilityEffectContext.overrideDuration = overrideDuration;
                // rememeber this method is meant for saved status effects
                // and traits
                abilityEffectContext.savedEffect = true;
                if (statusEffect != null) {
                    // testing : to allow npcs to get their visuals from traits, send in unit controller if it exists
                    //_abilityEffect.Cast(baseCharacter, baseCharacter?.UnitController, null, abilityEffectContext);
                    // testing prevent spawn of object since unitController now handles notifications that do that for all characters, not just the player
                    statusEffect.Cast(baseCharacter, null, null, abilityEffectContext);
                }
            }
        }

        public override bool IsPlayerControlled() {
            if (baseCharacter != null &&
                baseCharacter.UnitController != null &&
                baseCharacter.UnitController.MasterUnit != null &&
                baseCharacter.UnitController.MasterUnit == (playerManager.MyCharacter as BaseCharacter)) {

                return true;
            }
            return base.IsPlayerControlled();
        }

        public override void AddPet(CharacterUnit target) {
            if (baseCharacter.CharacterPetManager != null
                && target.Interactable != null
                && target.Interactable is UnitController
                && (target.Interactable as UnitController).UnitProfile != null) {
                baseCharacter.CharacterPetManager.AddPet((target.Interactable as UnitController).UnitProfile);
            }
        }

        public void ApplySavedStatusEffects(StatusEffectSaveData statusEffectSaveData) {
            // don't crash when loading old save data
            if (statusEffectSaveData.StatusEffectName == null || statusEffectSaveData.StatusEffectName == string.Empty) {
                return;
            }
            AbilityEffect savedEffect = systemDataFactory.GetResource<AbilityEffect>(statusEffectSaveData.StatusEffectName);
            if (savedEffect != null) {
                ApplyStatusEffect(savedEffect.AbilityEffectProperties, statusEffectSaveData.remainingSeconds);
            } else {
                Debug.LogWarning(baseCharacter.CharacterName + ".CharacterAbilityManager.ApplySavedStatusEffects() could not find effect in database with name " + statusEffectSaveData.StatusEffectName);
            }
        }

        public void RemoveCapabilityProviderTraits(List<StatusEffect> statusEffects) {
            if (statusEffects == null) {
                return;
            }
            foreach (StatusEffect statusEffect in statusEffects) {
                if (baseCharacter.CharacterStats != null && baseCharacter.CharacterStats.StatusEffects.ContainsKey(SystemDataFactory.PrepareStringForMatch(statusEffect.DisplayName))) {
                    baseCharacter.CharacterStats.StatusEffects[SystemDataFactory.PrepareStringForMatch(statusEffect.DisplayName)].CancelStatusEffect();
                }
            }
        }

        public void LearnCapabilityProviderAbilities(List<BaseAbilityProperties> abilities) {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.LearnCapabilityProviderAbilities()");

            if (abilities == null) {
                //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.LearnCapabilityProviderAbilities(): abilities is null");
                return;
            }
            foreach (BaseAbilityProperties baseAbility in abilities) {
                //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.LearnCapabilityProviderAbilities(): process: " + baseAbility.DisplayName);
                if (baseAbility.RequiredLevel <= baseCharacter.CharacterStats.Level && baseCharacter.CharacterAbilityManager.HasAbility(baseAbility) == false) {
                    baseAbility.PrepareToLearnAbility(this);
                    LearnAbility(baseAbility);
                }
            }
        }

        public void UnLearnCapabilityProviderAbilities(List<BaseAbilityProperties> abilities, bool updateActionBars = false) {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.UnLearnCapabilityProviderAbilities(" + updateActionBars + ")");
            if (abilities == null) {
                return;
            }
            foreach (BaseAbilityProperties oldAbility in abilities) {
                UnlearnAbility(oldAbility, updateActionBars);
            }
            OnUnlearnAbilities();
        }

        public IEnumerator PerformAbilityCoolDown(string abilityName) {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.BeginAbilityCoolDown(" + abilityName + ") IENUMERATOR");

            //Debug.Log(gameObject + ".BaseAbility.BeginAbilityCoolDown(): about to enter loop  IENUMERATOR");

            while (abilityCoolDownDictionary.ContainsKey(abilityName) && abilityCoolDownDictionary[abilityName].RemainingCoolDown > 0f) {
                yield return null;
                if (abilityCoolDownDictionary.ContainsKey(abilityName)) {
                    // in case ability is somehow accidentally cast while on cooldown, this will prevent null reference when other coroutine removes it from the dictionary
                    abilityCoolDownDictionary[abilityName].RemainingCoolDown -= Time.deltaTime;
                }
                //Debug.Log(gameObject.name + ".CharacterAbilityManager.PerformAbilityCooldown():  IENUMERATOR: " + abilityCoolDownDictionary[abilityName].MyRemainingCoolDown);
            }
            if (abilityCoolDownDictionary.ContainsKey(abilityName)) {
                //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.BeginAbilityCoolDown(" + abilityName + ") REMOVING FROM DICTIONARY");
                abilityCoolDownDictionary.Remove(abilityName);
            } else {
                //Debug.Log(gameObject + ".CharacterAbilityManager.BeginAbilityCoolDown(" + abilityName + ") WAS NOT IN DICTIONARY");
            }
        }


        public void HandleDie(CharacterStats _characterStats) {
            //Debug.Log(baseCharacter.gameObject.name + ".HandleDie()");

            TryToStopAnyAbility();
        }

        /*
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
        */

        public override bool HasAbility(string abilityName) {

            if (AbilityList.ContainsKey(SystemDataFactory.PrepareStringForMatch(abilityName))) {
                //Debug.Log(gameObject.name + ".CharacterAbilitymanager.HasAbility( " + abilityName + "): keyname: " + keyName + " TRUE!");
                return true;
            }

            return base.HasAbility(abilityName);
        }

        public override bool HasAbility(BaseAbilityProperties baseAbility) {
            //Debug.Log(gameObject.name + ".CharacterAbilitymanager.HasAbility(" + abilityName + ")");

            return HasAbility(baseAbility.DisplayName);
        }

        public void ActivateTargettingMode(BaseAbilityProperties baseAbility, Interactable target) {
            //Debug.Log("CharacterAbilityManager.ActivateTargettingMode()");
            targettingModeActive = true;
            if (baseCharacter != null && baseCharacter.UnitController != null
                && (baseCharacter.UnitController.UnitControllerMode == UnitControllerMode.AI || baseCharacter.UnitController.UnitControllerMode == UnitControllerMode.Pet)) {
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
            castTargettingManager.DisableProjector();
        }

        public void UpdateAbilityList(int newLevel) {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.UpdateAbilityList(). length: " + abilityList.Count);

            CapabilityConsumerSnapshot capabilityConsumerSnapshot = new CapabilityConsumerSnapshot(baseCharacter, systemGameManager);

            LearnCapabilityProviderAbilities(capabilityConsumerSnapshot.GetAbilityList());
        }

        public void LoadAbility(string abilityName) {
            //Debug.Log(gameObject.name + ".PlayerAbilityManager.LoadAbility(" + abilityName + ")");
            BaseAbilityProperties abilityProperties = systemDataFactory.GetResource<BaseAbility>(abilityName)?.AbilityProperties;
            if (abilityProperties == null) {
                // if we renamed an ability, old save data could load a null.  prevent invalid abilities from loading.
                return;
            }

            string keyName = SystemDataFactory.PrepareStringForMatch(abilityProperties.DisplayName);
            if (abilityList.ContainsKey(keyName)) {
                // ability is already known, exit
                return;
            }
            if (abilityProperties.CanLearnAbility(this) == false) {
                return;
            }
            abilityProperties.ProcessLoadAbility(this);

            abilityList[keyName] = abilityProperties;
        }

        public void LearnAutoAttack(BaseAbilityProperties baseAbilityProperties) {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.LearnAutoAttack(" + baseAbilityProperties.DisplayName + "): is auto-attack!");
            UnLearnDefaultAutoAttackAbility();
            autoAttackAbility = baseAbilityProperties;
        }

        public bool LearnAbility(BaseAbilityProperties newAbility) {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.LearnAbility(" + (newAbility == null ? "null" : newAbility.DisplayName) + ")");

            if (newAbility == null) {
                //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.LearnAbility(): baseAbility is null");
                // can't learn a nonexistent ability
                return false;
            }
            if (HasAbility(newAbility)) {
                //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.LearnAbility(" + (newAbility == null ? "null" : newAbility.DisplayName) + "): already known");
                return false;
            }
            if (newAbility.RequiredLevel > BaseCharacter.CharacterStats.Level) {
                //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.LearnAbility(" + (newAbility == null ? "null" : newAbility.DisplayName) + "): level too low");
                return false;
            }

            if (newAbility.CanLearnAbility(this) == false) {
                //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.LearnAbility(" + (newAbility == null ? "null" : newAbility.DisplayName) + "): cannot learn ability");
                return false;
            }

            // if we made it this far, there is no reason to not learn the ability
            abilityList[SystemDataFactory.PrepareStringForMatch(newAbility.DisplayName)] = newAbility;

            newAbility.ProcessLearnAbility(this);

            OnLearnAbility(newAbility);
            return true;
        }

        public void SetAutoAttackAbility(BaseAbilityProperties baseAbilityProperties) {
            autoAttackAbility = baseAbilityProperties;
        }

        public void UnlearnAbility(BaseAbilityProperties oldAbility, bool updateActionBars = true) {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.UnleanAbility(" + oldAbility.DisplayName + ", " + updateActionBars + ")");
            
            string keyName = SystemDataFactory.PrepareStringForMatch(oldAbility.DisplayName);
            if (abilityList.ContainsKey(keyName)) {
                oldAbility.ProcessUnLearnAbility(this);
                abilityList.Remove(keyName);
            }
            OnUnlearnAbility(updateActionBars);
        }

        public void UnsetAutoAttackAbility() {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.UnsetAutoAttackAbility()");
            autoAttackAbility = null;
        }

        /// <summary>
        /// Cast a spell with a cast timer
        /// </summary>
        /// <param name="ability"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public IEnumerator PerformAbilityCast(BaseAbilityProperties ability, Interactable target, AbilityEffectContext abilityEffectContext) {
            float startTime = Time.time;
            //Debug.Log(baseCharacter.gameObject.name + "CharacterAbilitymanager.PerformAbilityCast(" + ability.DisplayName + ", " + (target == null ? "null" : target.name) + ") Enter Ienumerator with tag: " + startTime);

            bool canCast = true;
           
            abilityEffectContext.originalTarget = target;
            if (ability.GetTargetOptions(baseCharacter).RequiresGroundTarget == true) {
                //Debug.Log("CharacterAbilitymanager.PerformAbilityCast() Ability requires a ground target.");
                ActivateTargettingMode(ability, target);
                while (WaitingForTarget() == true) {
                    //Debug.Log("CharacterAbilitymanager.PerformAbilityCast(" + ability.DisplayName + ") waiting for target");
                    yield return null;
                }
                if (GetGroundTarget() == Vector3.zero) {
                    //Debug.Log("CharacterAbilitymanager.PerformAbilityCast(" + ability.DisplayName + ") Ground Targetting: groundtarget is vector3.zero, cannot cast");
                    canCast = false;
                }
                abilityEffectContext.groundTargetLocation = GetGroundTarget();
            }
            if (canCast == true) {
                // dismount if mounted

                //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilitymanager.PerformAbilityCast(" + ability.DisplayName + "): cancast is true");
                if (!ability.CanSimultaneousCast) {
                    //Debug.Log("CharacterAbilitymanager.PerformAbilityCast() ability: " + ability.DisplayName + " can simultaneous cast is false, setting casting to true");
                    performingCast = true;
                    ability.StartCasting(baseCharacter);
                }
                float currentCastPercent = 0f;
                float nextTickPercent = 0f;
                //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilitymanager.PerformAbilityCast() currentCastPercent: " + currentCastPercent + "; MyAbilityCastingTime: " + ability.MyAbilityCastingTime);

                if (baseCharacter != null && ability.GetHoldableObjectList(baseCharacter).Count != 0) {
                    //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.PerformAbilityCast(" + ability.DisplayName + "): spawning ability objects");
                    if (!ability.AnimatorCreatePrefabs) {
                        SpawnAbilityObjects(ability.GetHoldableObjectList(baseCharacter));
                    }
                }
                if (ability.CastingAudioClip != null) {
                    baseCharacter.UnitController.UnitComponentController.PlayCastSound(ability.CastingAudioClip, ability.LoopAudio);
                }
                if (ability.CoolDownOnCast == true) {
                    ability.BeginAbilityCoolDown(baseCharacter);
                }

                if (ability.GetAbilityCastingTime(baseCharacter) > 0f) {
                    // added target condition to allow channeled spells to stop casting if target disappears
                    while (currentCastPercent < 1f
                        && (ability.GetTargetOptions(baseCharacter).RequireTarget == false
                        || (target != null && target.gameObject.activeInHierarchy == true))) {
                        //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilitymanager.PerformAbilityCast(" + ability.DisplayName + "): currentCastPercent: " + currentCastPercent);

                        yield return null;
                        currentCastPercent += (Time.deltaTime / ability.GetAbilityCastingTime(baseCharacter));

                        // call this first because it updates the cast bar
                        //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilitymanager.PerformAbilityCast() currentCastTime: " + currentCastPercent + "; MyAbilityCastingTime: " + ability.GetAbilityCastingTime(baseCharacter) + "; calling OnCastTimeChanged()");
                        OnCastTimeChanged(baseCharacter, ability, currentCastPercent);
                        if (baseCharacter.UnitController != null) {
                            baseCharacter.UnitController.UnitEventController.NotifyOnCastTimeChanged(baseCharacter, ability, currentCastPercent);
                        }

                        // now call the ability on casttime changed (really only here for channeled stuff to do damage)
                        nextTickPercent = ability.OnCastTimeChanged(currentCastPercent, nextTickPercent, baseCharacter, target, abilityEffectContext);
                    }
                }

            }

            //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.PerformAbilityCast(" + ability.DisplayName + "). nulling tag: " + startTime);
            // set currentCast to null because it isn't automatically null until the next frame and we are about to do stuff which requires it to be null immediately
            EndCastCleanup();

            if (canCast) {
                //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilitymanager.PerformAbilityCast(): Cast Complete and can cast");
                if (!ability.CanSimultaneousCast) {
                    NotifyOnCastComplete();
                    performingCast = false;
                    BaseCharacter.UnitController.UnitAnimator.SetCasting(false);
                }
                PerformAbility(ability, target, abilityEffectContext);
            } else {
                //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilitymanager.PerformAbilityCast(): Cast Complete and cannot cast");
            }
        }

        public void NotifyOnCastCancel() {
            OnCastCancel(baseCharacter);
            if (baseCharacter.UnitController != null) {
                baseCharacter.UnitController.UnitEventController.NotifyOnCastCancel(baseCharacter);
            }
        }

        public void NotifyOnCastComplete() {
            OnCastComplete(baseCharacter);
            if (baseCharacter.UnitController != null) {
                baseCharacter.UnitController.UnitEventController.NotifyOnCastComplete(baseCharacter);
            }
        }

        public void SpawnAbilityObjects(int indexValue = -1) {
            //Debug.Log(gameObject.name + ".CharacterAbilityManager.SpawnAbilityObjects(" + indexValue + ")");
            BaseAbilityProperties usedBaseAbility = null;
            if (currentAbilityEffectContext != null) {
                usedBaseAbility = currentAbilityEffectContext.baseAbility;
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
            //Debug.Log(abilityCaster.gameObject.name + ".CharacterAbilitymanager.EndCastCleanup()");
            base.EndCastCleanup();
            if (baseCharacter.UnitController != null) {
                // stop any casting audio clip
                baseCharacter.UnitController.UnitComponentController.StopCastSound();

                //stop any animation event audio clip
                baseCharacter.UnitController.UnitComponentController.StopEffectSound();
            }
        }

        public void ReceiveKillDetails(BaseCharacter killedcharacter, float creditPercent) {
            //Debug.Log("CharacterAbilityManager.ReceiveKillDetails()");
            // this is disabled in order to allow the character to complete current cast animation instead of suddenly stopping mid cast
            // the new workflow upon target death is : complete current animation -> wait for 2 seconds -> sheath weapons and go back to normal idle
            // monitor for breakage elsewhere
            /*
            if (BaseCharacter.UnitController.Target == killedcharacter.UnitController) {
                if (killStopCast) {
                    StopCasting();
                }
            }
            */
        }

        public void AttemptAutoAttack(bool playerInitiated = false) {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilitymanager.AttemtpAutoAttack()");

            if (AutoAttackAbility != null) {
                BeginAbility(AutoAttackAbility, playerInitiated);
            }/* else {
                Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilitymanager.AttemtpAutoAttack() no autoAttackAbility found!");
            }*/
        }

        /// <summary>
        /// This is the entrypoint for character behavior calls and should not be used for anything else due to the runtime ability lookup that happens
        /// </summary>
        /// <param name="abilityName"></param>
        public override bool BeginAbility(string abilityName) {
            //Debug.Log(baseCharacter.gameObject.name + "CharacterAbilitymanager.BeginAbility(" + (abilityName == null ? "null" : abilityName) + ")");
            BaseAbility baseAbility = systemDataFactory.GetResource<BaseAbility>(abilityName);
            if (baseAbility != null) {
                return BeginAbility(baseAbility.AbilityProperties);
            }
            return false;
        }

        /// <summary>
        /// The entrypoint to Casting a spell.  handles all logic such as instant/timed cast, current cast in progress, enough mana, target being alive etc
        /// </summary>
        /// <param name="ability"></param>
        public bool BeginAbility(BaseAbilityProperties ability, bool playerInitiated = false) {
            //Debug.Log(baseCharacter.gameObject.name + "CharacterAbilitymanager.BeginAbility(" + (ability == null ? "null" : ability.DisplayName) + ")");

            if (ability == null) {
                //Debug.Log("CharacterAbilityManager.BeginAbility(): ability is null! Exiting!");
                return false;
            } else {
                //Debug.Log("CharacterAbilityManager.BeginAbility(" + ability.DisplayName + ")");
            }
            return BeginAbilityCommon(ability, baseCharacter.UnitController.Target, playerInitiated);
        }

        public bool BeginAbility(BaseAbilityProperties ability, Interactable target) {
            //Debug.Log(gameObject.name + ".CharacterAbilityManager.BeginAbility(" + ability.DisplayName + ")");
            return BeginAbilityCommon(ability, target);
        }

        public override float GetSpeed() {
            return baseCharacter.CharacterStats.SecondaryStats[SecondaryStatType.Speed].CurrentValue / 100f;
        }

        public override float GetAnimationLengthMultiplier() {
            if (baseCharacter?.UnitController?.UnitAnimator != null) {

                // ensure minimum length is the minimum attack speed
                //return (baseCharacter.UnitController.UnitAnimator.LastAnimationLength / (float)baseCharacter.UnitController.UnitAnimator.LastAnimationHits);
                return (Mathf.Clamp(baseCharacter.UnitController.UnitAnimator.LastAnimationLength, baseCharacter.CharacterCombat.AttackSpeed, Mathf.Infinity) / (float)baseCharacter.UnitController.UnitAnimator.LastAnimationHits);
            }
            return base.GetAnimationLengthMultiplier();
        }

        public override float GetOutgoingDamageModifiers() {
            if (baseCharacter != null && baseCharacter.CharacterStats != null) {
                return baseCharacter.CharacterStats.GetOutGoingDamageModifiers();
            }
            return base.GetOutgoingDamageModifiers();
        }

        public override void ProcessWeaponHitEffects(AttackEffectProperties attackEffect, Interactable target, AbilityEffectContext abilityEffectContext) {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.ProcessWeaponHitEffects(" + (abilityEffectContext == null ? "null" : "valid") + ")");
            base.ProcessWeaponHitEffects(attackEffect, target, abilityEffectContext);

            // perform default weapon hit sound
            if (abilityEffectContext.baseAbility != null) {
                AudioClip audioClip = abilityEffectContext.baseAbility.GetHitSound(baseCharacter);
                if (audioClip != null) {
                    baseCharacter.UnitController.UnitComponentController.PlayEffectSound(audioClip);
                }
            }

            if (baseCharacter?.CharacterCombat?.OnHitEffects != null) {
                // handle weapon on hit effects
                List<AbilityEffectProperties> onHitEffectList = new List<AbilityEffectProperties>();
                foreach (AbilityEffectProperties abilityEffect in baseCharacter.CharacterCombat.OnHitEffects) {
                    // prevent accidental infinite recursion of ability effect
                    if (abilityEffect.DisplayName != attackEffect.DisplayName) {
                        onHitEffectList.Add(abilityEffect);
                    }
                }
                attackEffect.PerformAbilityEffects(baseCharacter, target, abilityEffectContext, onHitEffectList);
            }

            foreach (StatusEffectNode statusEffectNode in BaseCharacter.CharacterStats.StatusEffects.Values) {
                //Debug.Log(gameObject.name + ".CharacterCombat.AttackHit_AnimationEvent(): Casting OnHit Ability On Take Damage");
                // this could maybe be done better through an event subscription
                if (statusEffectNode.StatusEffect.WeaponHitAbilityEffectList.Count > 0) {
                    statusEffectNode.StatusEffect.CastWeaponHit(BaseCharacter, target, abilityEffectContext);
                }
            }
        }

        /// <summary>
        /// +damage stat from gear and weapon damage
        /// </summary>
        /// <returns></returns>
        public override float GetPhysicalDamage() {
            if (baseCharacter != null && baseCharacter.CharacterStats != null) {

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

        protected bool BeginAbilityCommon(BaseAbilityProperties ability, Interactable target, bool playerInitiated = false) {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.BeginAbilityCommon(" + (ability == null ? "null" : ability.DisplayName) + ", " + (target == null ? "null" : target.gameObject.name) + ")");
            
            if (ability == null) {
                Debug.LogError("CharacterAbilityManager.BeginAbilityCommon(" + (ability == null ? "null" : ability.DisplayName) + ", " + (target == null ? "null" : target.name) + ") NO ABILITY FOUND");
                return false;
            }
            if (baseCharacter?.UnitController != null) {
                if (baseCharacter.UnitController.ControlLocked == true) {
                    return false;
                }
            }

            if (!CanCastAbility(ability, playerInitiated)) {
                if (playerInitiated) {
                    //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.BeginAbilityCommon(" + ability.DisplayName + ", " + (target != null ? target.name : "null") + ") cannot cast");
                }
                return false;
            }

            AbilityEffectContext abilityEffectContext = new AbilityEffectContext(baseCharacter);
            abilityEffectContext.baseAbility = ability;

            // testing - if this was player initiated, the attack attempt came through right click or action button press
            // those things attempted to interact with a characterUnit which passed a faction check.  assume target is valid and perform quick sanity check
            // if it passes, attempt to enter combat so weapons can be unsheathed even if out of range
            if (playerInitiated) {
                CharacterUnit targetCharacterUnit = null;
                if (target != null) {
                    targetCharacterUnit = CharacterUnit.GetCharacterUnit(target);
                }
                if (targetCharacterUnit != null && targetCharacterUnit.BaseCharacter != null) {
                    if (Faction.RelationWith(targetCharacterUnit.BaseCharacter, baseCharacter) <= -1) {
                        if (targetCharacterUnit.BaseCharacter.CharacterCombat != null
                            && ability.GetTargetOptions(baseCharacter).CanCastOnEnemy == true
                            && targetCharacterUnit.BaseCharacter.CharacterStats.IsAlive == true) {

                            // disable this for now.  npc should pull character into combat when he enters their agro range.  character should pull npc into combat when status effect is applied or ability lands
                            // agro includes a liveness check, so casting necromancy on a dead enemy unit should not pull it into combat with us if we haven't applied a faction or master control buff yet
                            // ...re-enable this because rangers need to pull out their weapons when doing their animation when clicking on action bar
                            if (baseCharacter.CharacterCombat.GetInCombat() == false) {
                                baseCharacter.CharacterCombat.EnterCombat(target);
                            }

                            baseCharacter.CharacterCombat.ActivateAutoAttack();
                            OnAttack(targetCharacterUnit.BaseCharacter);
                        }
                    }
                }
            }

            // get final target before beginning casting
            Interactable finalTarget = ability.ReturnTarget(baseCharacter, target, true, abilityEffectContext, playerInitiated);
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.BeginAbilityCommon(" + ability.DisplayName + ") finalTarget: " + (finalTarget == null ? "null" : finalTarget.DisplayName));

            OnAttemptPerformAbility(ability);

            if (finalTarget == null && ability.GetTargetOptions(baseCharacter).RequireTarget == true) {
                if (playerInitiated) {
                    //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.BeginAbilityCommon(): finalTarget is null. exiting");
                }
                return false;
            }
            if (finalTarget != null && PerformLOSCheck(finalTarget, ability) == false) {
                if (playerInitiated) {
                    ReceiveCombatMessage("Target is not in line of sight");
                }
                return false;
            }

            baseCharacter.UnitController.CancelMountEffects();

            if (ability.CanSimultaneousCast) {
                // directly performing to avoid interference with other abilities being casted
                //Debug.Log(gameObject.name + ".CharacterAbilityManager.BeginAbilityCommon(): can simultaneous cast");

                // there is no ground target yet because that is handled in performabilitycast below
                PerformAbility(ability, finalTarget, abilityEffectContext);
            } else {
                //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.BeginAbilityCommon(): can't simultanous cast");
                if (currentCastCoroutine == null) {
                    //Debug.Log("Performing Ability " + ability.DisplayName + " at a cost of " + ability.MyAbilityManaCost.ToString() + ": ABOUT TO START COROUTINE");

                    // we need to do this because we are allowed to stop an outstanding auto-attack to start this cast
                    // we also need to stop any outstanding actions
                    TryToStopCastBlockers();

                    // start the cast (or cast targetting projector)
                    //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.BeginAbilityCommon(" + usedAbility + "): setting currentCastAbility");
                    // currentCastAbility must be set before starting the coroutine because for animated events, the cast time is zero and the variable will be cleared in the coroutine
                    currentCastAbility = ability;
                    currentCastCoroutine = abilityCaster.StartCoroutine(PerformAbilityCast(ability, finalTarget, abilityEffectContext));
                } else {
                    // return false so that items in the inventory don't get used if this came from a castable item
                    return false;
                    //systemGameManager.LogManager.WriteCombatMessage("A cast was already in progress WE SHOULD NOT BE HERE BECAUSE WE CHECKED FIRST! iscasting: " + isCasting + "; currentcast==null? " + (currentCast == null));
                    // unless.... we got here from the crafting queue, which launches the next item as the last step of the currently in progress cast
                    //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.BeginAbilityCommon(): A cast was already in progress!");
                }
            }

            if (baseCharacter != null && baseCharacter.UnitController != null
                && (baseCharacter.UnitController.UnitControllerMode == UnitControllerMode.AI || baseCharacter.UnitController.UnitControllerMode == UnitControllerMode.Pet)) {
                if (currentCastAbility != null && currentCastAbility.GetTargetOptions(baseCharacter).RequiresGroundTarget == true) {
                    Vector3 groundTarget = Vector3.zero;
                    if (baseCharacter.UnitController.Target != null) {
                        groundTarget = baseCharacter.UnitController.Target.transform.position;
                    }
                    SetGroundTarget(groundTarget);
                }

            }

            return true;
        }

        public override void ReceiveCombatMessage(string messageText) {
            base.ReceiveCombatMessage(messageText);
            OnCombatMessage(messageText);
        }

        public override void ReceiveMessageFeedMessage(string messageText) {
            base.ReceiveMessageFeedMessage(messageText);
            OnMessageFeedMessage(messageText);
        }


        // this only checks if the ability is able to be cast based on character state.  It does not check validity of target or ability specific requirements
        public override bool CanCastAbility(BaseAbilityProperties ability, bool playerInitiated = false) {
            //Debug.Log(gameObject.name + ".CharacterAbilityManager.CanCastAbility(" + ability.DisplayName + ")");

            // check if the ability is learned yet
            if (!PerformLearnedCheck(ability)) {
                //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.CanCastAbility(" + ability.DisplayName + "): Have not learned ability!");
                if (playerInitiated) {
                    OnCombatMessage("Cannot cast " + ability.DisplayName + "): Have not learned ability!");
                }
                return false;
            }

            // check if the ability is on cooldown
            if (!PerformCooldownCheck(ability)) {
                //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.CanCastAbility(" + ability.DisplayName + "): ability is on cooldown!");
                if (playerInitiated) {
                    OnCombatMessage("Cannot cast " + ability.DisplayName + "): ability is on cooldown!");
                }
                return false;
            }

            // check if auto-attack cooldown based on attack speed applies
            if (ability.ReadyToCast(baseCharacter.CharacterCombat) == false) {
                return false;
            }

            // check if we have enough mana
            if (!PerformPowerResourceCheck(ability)) {
                //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.CanCastAbility(" + ability.DisplayName + "): do not have sufficient power resource to cast!");
                return false;
            }

            if (!PerformCombatCheck(ability)) {
                //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.CanCastAbility(" + ability.DisplayName + "): cannot cast ability in combat!");
                if (playerInitiated) {
                    OnCombatMessage("Cannot cast " + ability.DisplayName + "): cannot cast ability in combat!");
                }
                return false;
            }

            if (!PerformStealthCheck(ability)) {
                //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.CanCastAbility(" + ability.DisplayName + "): cannot cast ability in combat!");
                if (playerInitiated) {
                    OnCombatMessage("Cannot cast " + ability.DisplayName + "): cannot cast ability unless stealthed!");
                }
                return false;
            }


            if (!PerformLivenessCheck(ability)) {
                //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.CanCastAbility(" + ability.DisplayName + "): cannot cast while dead!");
                if (playerInitiated) {
                    OnCombatMessage("Cannot cast " + ability.DisplayName + "): cannot cast while dead!");
                }
                return false;
            }

            // for now require a player to perform movement check because an NPC will by default stop and go into attack mode to cast an ability
            // this check is designed to prevent players from casting anything other than instant casts while running
            if (playerInitiated && !PerformMovementCheck(ability)) {
                //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.CanCastAbility(" + ability.DisplayName + "): velocity too high to cast!");
                if (playerInitiated) {
                    OnCombatMessage("Cannot cast " + ability.DisplayName + "): cannot cast while moving!");
                }
                return false;
            }

            // default is true, nothing has stopped us so far
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.CanCastAbility(" + ability.DisplayName + "): returning true");
            return base.CanCastAbility(ability);
        }

        public bool PerformLivenessCheck(BaseAbilityProperties ability) {
            if (!baseCharacter.CharacterStats.IsAlive) {
                return false;
            }
            return true;
        }

        public bool PerformMovementCheck(BaseAbilityProperties ability) {
            if (ability.CanCastWhileMoving || ability.GetAbilityCastingTime(baseCharacter) == 0f) {
                return true;
            }
            return !(baseCharacter.UnitController.ApparentVelocity > 0.1f);
        }

        public bool PerformLearnedCheck(BaseAbilityProperties ability) {

            string keyName = SystemDataFactory.PrepareStringForMatch(ability.DisplayName);

            if (!ability.UseableWithoutLearning && !AbilityList.ContainsKey(keyName)) {
                OnLearnedCheckFail(ability);
                return false;
            }
            return true;
        }

        public bool PerformCooldownCheck(BaseAbilityProperties ability) {
            //Debug.Log(gameObject.name + ".CharacterAbilityManager.PerformCooldownCheck(" + ability.DisplayName + ") : global: " + MyRemainingGlobalCoolDown);
            if (abilityCoolDownDictionary.ContainsKey(ability.DisplayName) ||
                (RemainingGlobalCoolDown > 0f && ability.IgnoreGlobalCoolDown == false)) {
                return false;
            }
            return true;
        }

        public bool PerformCombatCheck(BaseAbilityProperties ability) {
            if (ability.RequireOutOfCombat == true && BaseCharacter.CharacterCombat.GetInCombat() == true) {
                OnCombatCheckFail(ability);
                return false;
            }
            return true;
        }

        public bool PerformStealthCheck(BaseAbilityProperties ability) {
            if (ability.RequireStealth == true && BaseCharacter.CharacterStats.IsStealthed == false) {
                OnStealthCheckFail(ability);
                return false;
            }
            return true;
        }


        /// <summary>
        /// Check if the caster has the required amount of the power resource to cast the ability
        /// </summary>
        /// <param name="ability"></param>
        /// <returns></returns>
        public bool PerformPowerResourceCheck(BaseAbilityProperties ability) {
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
        public void PerformAbility(BaseAbilityProperties ability, Interactable target, AbilityEffectContext abilityEffectContext) {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.PerformAbility(" + ability.DisplayName + ", " + target.gameObject.name + ")");
            if (abilityEffectContext == null) {
                abilityEffectContext = new AbilityEffectContext(baseCharacter);
                abilityEffectContext.baseAbility = ability;
            }
            abilityEffectContext.originalTarget = target;
            Interactable finalTarget = target;

            if (!PerformPowerResourceCheck(ability)) {
                return;
            }

            if (ability.GetResourceCost(baseCharacter) != 0 && ability.PowerResource != null) {
                // testing - should add ever be used?
                //abilityEffectContext.AddResourceAmount(ability.PowerResource.DisplayName, ability.GetResourceCost(baseCharacter));
                abilityEffectContext.SetResourceAmount(ability.PowerResource.DisplayName, ability.GetResourceCost(baseCharacter));
                // intentionally not keeping track of this coroutine.  many of these could be in progress at once.
                abilityCaster.StartCoroutine(UsePowerResourceDelay(ability.PowerResource, (int)ability.GetResourceCost(baseCharacter), ability.SpendDelay));
            }

            ability.Cast(baseCharacter, finalTarget, abilityEffectContext);
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
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.HandleManualMovement()");
            // adding new code to require some movement distance to prevent gravity while standing still from triggering this
            if (baseCharacter.UnitController.ApparentVelocity <= 0.1f) {
                //Debug.Log("CharacterAbilityManager.HandleManualMovement(): velocity too low, doing nothing");
                return;
            }

            //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.HandleManualMovement(): apparent velocity > 0.1f : " + baseCharacter.UnitController.ApparentVelocity);
            if (currentCastAbility != null
                && (currentCastAbility.CanCastWhileMoving == true ||
                currentCastAbility.GetTargetOptions(baseCharacter).RequiresGroundTarget == true
                && castTargettingManager.ProjectorIsActive() == true)) {
                // do nothing
                //Debug.Log("CharacterAbilityManager.HandleManualMovement(): not cancelling casting because we have a ground target active");
            } else {
                //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.HandleManualMovement(): stop casting as a result of manual movement with velocity: " + BaseCharacter.UnitController.ApparentVelocity);
                TryToStopAnyAbility();
            }
        }

        public void TryToStopAnyAbility() {
            TryToStopAnyAttack();
            TryToStopCasting();
        }

        private void TryToStopAnyAttack() {
            TryToStopAnimatedAbility();
            TryToStopAutoAttack();
        }
        
        public void TryToStopCastBlockers() {
            TryToStopAutoAttack();
            baseCharacter.UnitController?.UnitActionManager.TryToStopAction();
        }

        private void TryToStopAutoAttack() {
            if (performingAutoAttack == false) {
                return;
            }

            StopAnimatedAbility();
        }

        private void TryToStopAnimatedAbility() {
            if (performingAnimatedAbility == false) {
                return;
            }

            StopAnimatedAbility();
        }

        public void TryToStopCasting() {
            if (currentCastCoroutine == null) {
                // REMOVED ISCASTING == TRUE BECAUSE IT WAS PREVENTING THE CRAFTING QUEUE FROM WORKING.  TECHNICALLY THIS GOT CALLED RIGHT AFTER ISCASTING WAS SET TO FALSE, BUT BEFORE CURRENTCAST WAS NULLED
                return;
            }

            performingCast = false;
            StopCasting();
        }

        private void StopAnimatedAbility() {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.StopAnimatedAbility()");

            abilityCaster.StopCoroutine(attackCoroutine);
            
            ProcessAnimatedAbilityEnd();

            NotifyOnCastCancel();
        }

        private void ProcessAnimatedAbilityEnd() {
            attackCoroutine = null;
            if (performingAutoAttack == true) {
                performingAutoAttack = false;
            }
            if (performingAnimatedAbility == true) {
                performingAnimatedAbility = false;
            }
            currentAbilityEffectContext = null;
            baseCharacter.UnitController.UnitAnimator.ClearAnimatedAbility();
            DespawnAbilityObjects();
        }

        private void StopCasting() {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.StopCasting()");

            abilityCaster.StopCoroutine(currentCastCoroutine);
            currentCastCoroutine = null;
            BaseCharacter.UnitController?.UnitAnimator.ClearCasting();
            DespawnAbilityObjects();
            EndCastCleanup();
            NotifyOnCastCancel();
        }

        public void HandleCharacterUnitDespawn() {
            TryToStopAnyAbility();
        }

        public override AudioClip GetAnimatedAbilityHitSound() {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilitymanager.GetAnimatedAbilityHitSound()");
            if (baseCharacter?.CharacterCombat != null && baseCharacter.CharacterCombat.DefaultHitSoundEffects.Count > 0) {
                return baseCharacter.CharacterCombat.DefaultHitSoundEffects[UnityEngine.Random.Range(0, baseCharacter.CharacterCombat.DefaultHitSoundEffects.Count)];
            }
            return base.GetAnimatedAbilityHitSound();
        }

        /// <summary>
        /// This will be triggered in response to things like hammer taps, not attacks
        /// </summary>
        public void AnimationHitAnimationEvent() {
            //Debug.Log(gameObject.name + ".CharacterAbilitymanager.AnimationHitAnimationEvent()");

            if (currentCastAbility != null) {
                AudioClip audioClip = currentCastAbility.GetAnimationEventSound();
                if (audioClip != null) {
                    baseCharacter.UnitController.UnitComponentController.PlayEffectSound(audioClip);
                }
            }
        }

        /// <summary>
        /// Play audio in response to the StartAudio() animation event
        /// </summary>
        public void StartAudioAnimationEvent() {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilitymanager.StartAudioAnimationEvent()");

            if (currentCastAbility != null) {
                AudioClip audioClip = currentCastAbility.GetAnimationEventSound();
                if (audioClip != null) {
                    baseCharacter.UnitController.UnitComponentController.PlayEffectSound(audioClip, currentCastAbility.LoopAudio);
                }
                return;
            }

            // here character combat is sent in because currentAbilityEffectContext is only used for animated abilities
            // which requires considering the weapon skill
            if (currentAbilityEffectContext != null) {
                AudioClip audioClip = currentAbilityEffectContext.baseAbility.GetAnimationEventSound(baseCharacter.CharacterCombat);
                if (audioClip != null) {
                    baseCharacter.UnitController.UnitComponentController.PlayEffectSound(audioClip, currentAbilityEffectContext.baseAbility.LoopAudio);
                }
                return;
            }
        }

        /// <summary>
        /// Stops playing audio in response to the StopAudio() animation event
        /// </summary>
        public void StopAudioAnimationEvent() {
            //Debug.Log(gameObject.name + ".CharacterAbilitymanager.StopAudioAnimationEvent()");

            baseCharacter.UnitController.UnitComponentController.StopEffectSound();
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

        public override void ProcessAbilityCoolDowns(AnimatedAbilityProperties baseAbility, float animationLength, float abilityCoolDown) {
            base.ProcessAbilityCoolDowns(baseAbility, animationLength, abilityCoolDown);
            if (baseCharacter?.UnitController != null && baseCharacter.UnitController.UnitControllerMode == UnitControllerMode.Player) {
                if (systemConfigurationManager.AllowAutoAttack == true && baseAbility.IsAutoAttack) {
                    return;
                }
            }

            baseAbility.ProcessGCDManual(baseCharacter, Mathf.Min(animationLength, abilityCoolDown));
            BeginAbilityCoolDown(baseAbility, Mathf.Max(animationLength, abilityCoolDown));
        }

    }

}