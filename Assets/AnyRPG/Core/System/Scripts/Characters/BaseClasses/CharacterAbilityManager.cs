using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace AnyRPG {
    public class CharacterAbilityManager : AbilityManager {

        protected UnitController unitController;

        protected Dictionary<string, AbilityProperties> abilityList = new Dictionary<string, AbilityProperties>();

        protected Vector3 groundTarget = Vector3.zero;

        protected bool targetingModeActive = false;
        protected AbilityProperties groundTargetAbility = null;

        // does killing the player you are currently targetting stop your cast.  gets set to false when channeling aoe.
        // disabled to prevent weapon going out of character hand mid animation swing if mob dies while swinging
        //private bool killStopCast = true;

        protected float remainingGlobalCoolDown = 0f;

        // we need a reference to the total length of the current global cooldown to properly calculate radial fill on the action buttons
        protected float initialGlobalCoolDown;

        // the auto-attack ability provided by capablity providers
        protected AbilityProperties autoAttackAbility = null;

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

        // the holdable objects spawned during ability effects and removed when the cast is complete
        protected Dictionary<AbilityAttachmentNode, List<GameObject>> abilityEffectObjects = new Dictionary<AbilityAttachmentNode, List<GameObject>>();
        protected Dictionary<GameObject, AbilityAttachmentNode> abilityEffectObjectLookup = new Dictionary<GameObject, AbilityAttachmentNode>();

        private struct PendingEvent {
            public float Time;
            public string FunctionName;
            public bool Fired;
        }

        private List<PendingEvent> hitEventCache = new List<PendingEvent>();

        // game manager references
        private PlayerManager playerManager = null;
        private CastTargettingManager castTargettingManager = null;
        private CharacterManager characterManager = null;
        private SystemAbilityController systemAbilityController = null;

        public float InitialGlobalCoolDown { get => initialGlobalCoolDown; set => initialGlobalCoolDown = value; }
        public float RemainingGlobalCoolDown { get => remainingGlobalCoolDown; set => remainingGlobalCoolDown = value; }
        public AbilityEffectContext CurrentAbilityEffectContext { get => currentAbilityEffectContext; set => currentAbilityEffectContext = value; }

        public UnitController UnitController { get => unitController; }
        public override GameObject UnitGameObject { get => unitController.gameObject; }

        public override bool ControlLocked { get => unitController.ControlLocked; }

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
                return unitController.CharacterStats.Level;
            }
        }

        public override string Name {
            get {
                if (unitController.BaseCharacter.CharacterName != null) {
                    return unitController.BaseCharacter.CharacterName;
                }
                return base.Name;
            }
        }

        public Dictionary<string, AbilityProperties> AbilityList {
            get {
                Dictionary<string, AbilityProperties> returnAbilityList = new Dictionary<string, AbilityProperties>();
                foreach (string abilityName in abilityList.Keys) {
                    if ((abilityList[abilityName].CharacterClassRequirementList == null || abilityList[abilityName].CharacterClassRequirementList.Count == 0 || abilityList[abilityName].CharacterClassRequirementList.Contains(unitController.BaseCharacter.CharacterClass))
                        && (abilityList[abilityName].ClassSpecializationRequirementList == null || abilityList[abilityName].ClassSpecializationRequirementList.Count == 0 || abilityList[abilityName].ClassSpecializationRequirementList.Contains(unitController.BaseCharacter.ClassSpecialization))) {
                        returnAbilityList.Add(abilityName, abilityList[abilityName]);
                    }
                }
                return returnAbilityList;
            }

        }

        public Dictionary<string, AbilityCoolDownNode> AbilityCoolDownDictionary { get => abilityCoolDownDictionary; set => abilityCoolDownDictionary = value; }
        public Coroutine CurrentCastCoroutine { get => currentCastCoroutine; }
        public AbilityProperties AutoAttackAbility {
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
        public override Dictionary<string, AbilityProperties> RawAbilityList { get => abilityList; }

        public CharacterAbilityManager(UnitController unitController, SystemGameManager systemGameManager) : base(unitController, systemGameManager) {
            this.unitController = unitController;
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            playerManager = systemGameManager.PlayerManager;
            castTargettingManager = systemGameManager.CastTargettingManager;
            characterManager = systemGameManager.CharacterManager;
            systemAbilityController = systemGameManager.SystemAbilityController;
        }

        public bool PerformingAnyAbility() {
            if (performingAnimatedAbility == true || performingAutoAttack == true || performingCast == true) {
                // can't auto-attack during auto-attack, animated attack, or cast
                return true;
            }
            return false;
        }

        public override CharacterUnit GetCharacterUnit() {
            return unitController.CharacterUnit;
        }

        public override void SummonMount(UnitProfile mountUnitProfile) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterAbilityManager.SummonMount({mountUnitProfile.ResourceName})");

            base.SummonMount(mountUnitProfile);

            unitController.UnitMountManager.SummonMount(mountUnitProfile);
        }

        public override List<AbilityEffectProperties> GetDefaultHitEffects() {
            if (unitController.CharacterCombat.DefaultHitEffects.Count > 0) {
                return unitController.CharacterCombat.DefaultHitEffects;
            }
            return base.GetDefaultHitEffects();
        }

        public override List<AbilityAttachmentNode> GetWeaponAbilityAnimationObjectList() {
            if (unitController.CharacterEquipmentManager != null) {
                return unitController.CharacterEquipmentManager.WeaponAbilityAnimationObjects;
            }
            return base.GetWeaponAbilityAnimationObjectList();
        }

        public override List<AbilityAttachmentNode> GetWeaponAbilityObjectList() {
            if (unitController.CharacterEquipmentManager != null) {
                return unitController.CharacterEquipmentManager.WeaponAbilityObjects;
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
                if (unitController?.UnitProfile?.UnitPrefabProps.AttachmentProfile != null) {
                    if (unitController.UnitProfile.UnitPrefabProps.AttachmentProfile.AttachmentPointDictionary.ContainsKey(attachmentNode.AttachmentName)) {
                        return unitController.UnitProfile.UnitPrefabProps.AttachmentProfile.AttachmentPointDictionary[attachmentNode.AttachmentName];
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
                    Debug.LogWarning("CharacterAbilityManager.HoldObject(): Unable to find target bone : " + attachmentPointNode.TargetBone);
                }
            } else {
                // this code appears to have been copied from equipmentmanager (now in mecanimModelController) so the below line is false ?
                // disabled message because some equipment (like quivers) does not have held attachment points intentionally because it should stay in the same place in combat
                //Debug.Log(baseCharacter.gameObject + ".CharacterEquipmentManager.HoldObject(): Unable to get attachment point");
                // testing because this is ability manager, if no attachment point node or target bone was found, set rotation to match parent
                go.transform.rotation = searchObject.transform.rotation;
            }
        }

        public void SpawnAbilityObjects(int indexValue = -1) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterAbilityManager.SpawnAbilityObjects({indexValue})");

            AbilityProperties usedBaseAbility = null;
            if (currentAbilityEffectContext != null) {
                usedBaseAbility = currentAbilityEffectContext.baseAbility;
            }
            if (usedBaseAbility == null) {
                usedBaseAbility = currentCastAbility;
            }

            if (unitController != null &&
                unitController.CharacterEquipmentManager != null &&
                usedBaseAbility != null &&
                usedBaseAbility.GetHoldableObjectList(unitController).Count != 0) {
                //if (baseCharacter != null && baseCharacter.MyCharacterEquipmentManager != null && ability.MyAbilityCastingTime > 0f && ability.MyHoldableObjectNames.Count != 0) {
                //Debug.Log($"{gameObject.name}.CharacterAbilityManager.PerformAbilityCast(): spawning ability objects");
                if (usedBaseAbility.AnimatorCreatePrefabs) {
                    SpawnAbilityObjectsInternal(usedBaseAbility, indexValue);
                }
            }
        }

        public void SpawnAbilityObjects(AbilityProperties usedBaseAbility) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterAbilityManager.SpawnAbilityObjects({usedBaseAbility.DisplayName})");

            SpawnAbilityObjectsInternal(usedBaseAbility, -1);
        }

        public void SpawnAbilityObjectsInternal(AbilityProperties ability, int index) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterAbilityManager.SpawnAbilityObjectsInternal({ability.DisplayName}, {index})");

            //ability.abilityProperties.GetHoldableObjectList(unitController)
            if (index == -1) {
                SpawnAbilityObjects(ability.GetHoldableObjectList(unitController));
            } else {
                List<AbilityAttachmentNode> passList = new List<AbilityAttachmentNode>();
                passList.Add(ability.GetHoldableObjectList(unitController)[index - 1]);
                SpawnAbilityObjects(passList);
            }
            
            unitController.UnitEventController.NotifyOnSpawnAbilityObjects(ability, index);

        }

        public void SpawnAbilityObjects(List<AbilityAttachmentNode> abilityAttachmentNodes) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterAbilityManager.SpawnAbilityObjects(count {abilityAttachmentNodes.Count})");

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
                            Transform targetBone = unitController.transform;
                            if (attachmentPointNode.TargetBone != null && attachmentPointNode.TargetBone != string.Empty) {
                                targetBone = unitController.transform.FindChildByRecursive(attachmentPointNode.TargetBone);
                            }

                            if (targetBone != null) {
                                //Debug.Log("CharacterAbilityManager.SpawnAbilityObjects(): targetbone (" + attachmentPointNode.TargetBone + ") is " + targetBone.gameObject.name);
                                GameObject newEquipmentPrefab = objectPooler.GetPooledObject(abilityAttachmentNode.HoldableObject.Prefab, targetBone);
                                //holdableObjects.Add(attachmentNode.MyHoldableObject, newEquipmentPrefab);
                                holdableObjects.Add(abilityAttachmentNode, newEquipmentPrefab);
                                //currentEquipmentPhysicalObjects[equipmentSlotProfile] = newEquipmentPrefab;

                                newEquipmentPrefab.transform.localScale = abilityAttachmentNode.HoldableObject.Scale;
                                HoldObject(newEquipmentPrefab, abilityAttachmentNode, unitController.gameObject);
                            } else {
                                Debug.LogWarning($"{unitController.gameObject.name}.CharacterAbilityManager.SpawnAbilityObjects(). We could not find the target bone {attachmentPointNode.TargetBone} while attempting to hold {abilityAttachmentNode.HoldableObject.ResourceName}");
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

        public override void AddAbilityEffectObject(AbilityAttachmentNode abilityAttachmentNode, GameObject go) {
            base.AddAbilityEffectObject(abilityAttachmentNode, go);
            if (abilityEffectObjects.ContainsKey(abilityAttachmentNode)) {
                abilityEffectObjects[abilityAttachmentNode].Add(go);
            } else {
                abilityEffectObjects.Add(abilityAttachmentNode, new List<GameObject>() { go });
            }
            abilityEffectObjectLookup.Add(go, abilityAttachmentNode);
        }

        private void DespawnAbilityEffectObjects() {
            //Debug.Log($"{unitController.gameObject.name}.CharacterAbilityManager.DespawnAbilityObjects()");

            if (abilityEffectObjects == null || abilityEffectObjects.Count == 0) {
                return;
            }

            foreach (List<GameObject> abilityObjectPrefabs in abilityEffectObjects.Values) {
                if (abilityObjectPrefabs != null) {
                    foreach (GameObject abilityObject in abilityObjectPrefabs) {
                        if (abilityObject != null) {
                            systemAbilityController.CancelDestroyAbilityEffectObject(abilityObject);
                            objectPooler.ReturnObjectToPool(abilityObject);
                        }
                    }
                }
            }
            abilityEffectObjects.Clear();
            abilityEffectObjectLookup.Clear();
        }

        public override void DespawnAbilityObjects() {
            //Debug.Log($"{unitController.gameObject.name}.CharacterAbilityManager.DespawnAbilityObjects()");

            base.DespawnAbilityObjects();

            DespawnAbilityEffectObjects();

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
            UnitController.UnitEventController.NotifyOnDespawnAbilityObjects();
        }

        public override void GeneratePower(AbilityProperties ability) {
            //Debug.Log($"{gameObject.name}.CharacterAbilityManager.GeneratePower({ability.DisplayName})");
            if (ability.GeneratePowerResource == null) {
                // nothing to generate
                return;
            }
            base.GeneratePower(ability);
            if (unitController != null && unitController.CharacterStats != null) {
                //Debug.Log($"{gameObject.name}.GeneratePower({ability.DisplayName}): name " + ability.GeneratePowerResource.DisplayName  + "; " + ability.GetResourceGain(this));
                unitController.CharacterStats.AddResourceAmount(ability.GeneratePowerResource.ResourceName, ability.GetResourceGain(unitController));
            }
        }

        public override List<AnimationClip> GetDefaultAttackAnimations() {
            //Debug.Log($"{gameObject.name}.GetDefaultAttackAnimations()");
            if (AutoAttackAbility != null) {
                return AutoAttackAbility.ActionClips;
            }
            return base.GetDefaultAttackAnimations();
        }

        /// <summary>
        /// get the current attack animations, accounting for any equippped weapon
        /// </summary>
        /// <returns></returns>
        public override List<AnimationClip> GetUnitAttackAnimations() {
            //Debug.Log($"{baseCharacter.gameObject.name}.GetUnitAttackAnimations()");

            if (unitController.UnitAnimator.CurrentAnimations != null) {
                return unitController.UnitAnimator.CurrentAnimations.AttackClips;
            }
            return base.GetUnitAttackAnimations();
        }

        public override AnimationProps GetUnitAnimationProps() {
            //Debug.Log($"{gameObject.name}.GetDefaultAttackAnimations()");
            if (unitController.UnitProfile?.UnitPrefabProps?.AnimationProps != null) {
                return unitController.UnitProfile.UnitPrefabProps.AnimationProps;
            }
            if (systemConfigurationManager.DefaultAnimationProfile != null) {
                return systemConfigurationManager.DefaultAnimationProfile.AnimationProps;
            }
            return base.GetUnitAnimationProps();
        }

        public override List<AnimationClip> GetUnitCastAnimations() {
            //Debug.Log($"{unitController.gameObject.name}.GetUnitCastAnimations()");

            if (unitController.UnitAnimator.CurrentAnimations != null) {
                //Debug.Log($"{unitController.gameObject.name}.GetUnitCastAnimations() returning {unitController.UnitAnimator.CurrentAnimations.CastClips.Count}");
                return unitController.UnitAnimator.CurrentAnimations.CastClips;
            }
            return base.GetUnitCastAnimations();
        }


        public override float GetMeleeRange() {
            return unitController.CharacterUnit.HitBoxSize;
        }



        public override float GetThreatModifiers() {
            return unitController.CharacterStats.GetThreatModifiers();
        }

        public override void GenerateAgro(CharacterUnit targetCharacterUnit, int usedAgroValue) {
            base.GenerateAgro(targetCharacterUnit, usedAgroValue);
            AddToAggroTable(unitController.CharacterUnit, usedAgroValue);
            targetCharacterUnit.UnitController.CharacterCombat.AggroTable.LockAgro();

        }

        public override void PerformCastingAnimation(AbilityProperties abilityProperties) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterAbilityManager.PerformCastingAnimation({abilityProperties.DisplayName})");

            base.PerformCastingAnimation(abilityProperties);

            int clipIndex = 0;
            List<AnimationClip> usedCastAnimationClips = abilityProperties.GetAbilityCastClips(unitController);
            if (usedCastAnimationClips == null || usedCastAnimationClips.Count == 0) {
                //Debug.Log($"{unitController.gameObject.name}.CharacterAbilityManager.PerformCastingAnimation({abilityProperties.DisplayName}): no cast animation clips found");
                return;
            }
            clipIndex = UnityEngine.Random.Range(0, usedCastAnimationClips.Count);
            if (usedCastAnimationClips[clipIndex] == null) {
                Debug.LogWarning($"{unitController.gameObject.name}.CharacterAbilityManager.PerformCastingAnimation({abilityProperties.DisplayName}): cast animation clip is null");
                return;
            }

            unitController.UnitAnimator.PerformAbilityCast(abilityProperties, clipIndex);
        }

        public override void AddTemporaryPet(UnitProfile unitProfile, UnitController unitController) {
            base.AddTemporaryPet(unitProfile, unitController);
            this.unitController.CharacterPetManager.AddActivePet(unitProfile, unitController);
        }

        public override void CapturePet(UnitController targetUnitController) {
            base.CapturePet(targetUnitController);
            if (unitController.CharacterPetManager != null && targetUnitController != null) {
                //Debug.Log($"{gameObject.name}.CapturePet(): adding to pet manager");
                unitController.CharacterPetManager.CapturePet(targetUnitController.UnitProfile, targetUnitController);
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
            return unitController.IsTargetInHitBox(target);
        }

        public override bool PerformLOSCheck(Interactable target, ITargetable targetable, AbilityEffectContext abilityEffectContext = null) {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.PerformLOSCheck()");

            if (targetable.GetTargetOptions(unitController).RequireLineOfSight == false) {
                return true;
            }

            Vector3 sourcePosition = abilityCasterMonoBehaviour.transform.position;
            // get initial positions in case of no collider
            if (targetable.GetTargetOptions(unitController).LineOfSightSourceLocation == LineOfSightSourceLocation.Caster) {
                sourcePosition = abilityCasterMonoBehaviour.transform.position;
                Collider sourceCollider = abilityCasterMonoBehaviour.GetComponent<Collider>();
                if (sourceCollider != null) {
                    sourcePosition = sourceCollider.bounds.center;
                }
            } else if (targetable.GetTargetOptions(unitController).LineOfSightSourceLocation == LineOfSightSourceLocation.GroundTarget && abilityEffectContext != null) {
                sourcePosition = abilityEffectContext.groundTargetLocation;
            } else if (targetable.GetTargetOptions(unitController).LineOfSightSourceLocation == LineOfSightSourceLocation.OriginalTarget && abilityEffectContext != null) {
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
            TargetProps targetProps = targetable.GetTargetOptions(unitController);
            if (targetProps.UseMeleeRange) {
                if (!IsTargetInMeleeRange(target)) {
                    //Debug.Log($"{unitController.gameObject.name}.IsTargetInRange(): target {target.DisplayName} is out of melee range");
                    return false;
                }
            } else {
                if (!IsTargetInMaxRange(target, targetProps.MaxRange, targetable, abilityEffectContext)) {
                    //Debug.Log($"{unitController.gameObject.name}.IsTargetInRange(): target {target.DisplayName} is out of max range ({targetProps.MaxRange})");
                    return false;
                }
                if (targetProps.RequireLineOfSight == true && !PerformLOSCheck(target, targetable, abilityEffectContext)) {
                    //Debug.Log($"{unitController.gameObject.name}.IsTargetInRange(): target {target.DisplayName} is not in line of sight");
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
            if (targetable.GetTargetOptions(unitController).TargetRangeSourceLocation == TargetRangeSourceLocation.GroundTarget && abilityEffectContext != null) {
                sourcePosition = abilityEffectContext.groundTargetLocation;
            } else if (targetable.GetTargetOptions(unitController).TargetRangeSourceLocation == TargetRangeSourceLocation.OriginalTarget && abilityEffectContext != null) {
                sourcePosition = abilityEffectContext.originalTarget.transform.position;
            }
            //Debug.Log(target.name + " range(" + maxRange + ": " + Vector3.Distance(UnitGameObject.transform.position, target.transform.position));
            if (maxRange > 0 && Vector3.Distance(sourcePosition, target.InteractableGameObject.transform.position) > maxRange) {
                //Debug.Log(target.name + " is out of range(" + maxRange + "): " + Vector3.Distance(UnitGameObject.transform.position, target.transform.position));
                return false;
            }

            return true;
        }

        public override float PerformAbilityAction(AbilityProperties baseAbility, AnimationClip animationClip, int clipIndex, UnitController targetUnitController, AbilityEffectContext abilityEffectContext) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterAbilityManager.PerformAbilityAction({baseAbility.ResourceName})");

            // this type of ability is allowed to interrupt other types of animations, so clear them all
            // is this really necessary ?  shouldn't checks have been performed before we got there as to whether anything specific was happening, and then cancel it already ?
            //TryToStopAnyAbility();

            // block further animations of other types from starting
            if (!baseAbility.IsAutoAttack) {
                performingAnimatedAbility = true;
            } else {
                performingAutoAttack = true;
            }

            // reset animated ability timer
            unitController.CharacterCombat.RegisterAnimatedAbilityBegin();

            // notify for attack sounds
            if (baseAbility.PlayAttackVoice(unitController.CharacterCombat) == true) {
                unitController?.UnitEventController.NotifyOnAttack();
            }

            float speedNormalizedAnimationLength = 1f;
            if (unitController.CharacterStats != null) {
                speedNormalizedAnimationLength = (1f / (unitController.CharacterStats.GetSpeedModifiers() / 100f)) * animationClip.length;
            }

            // setup event tracking to ensure hit events fire regardless of frame rates / drops / animation compression etc
            float timeScaleRatio = speedNormalizedAnimationLength / animationClip.length;
            hitEventCache.Clear();
            foreach (var evt in animationClip.events) {
                if (evt.functionName != "Hit") {
                    continue;
                }
                hitEventCache.Add(new PendingEvent {
                    // Scale the designer's timestamp to match your buffed speed
                    Time = speedNormalizedAnimationLength - (evt.time * timeScaleRatio),
                    FunctionName = evt.functionName,
                    Fired = false
                });
            }

            unitController.CharacterCombat.SwingTarget = targetUnitController;
            currentAbilityEffectContext = abilityEffectContext;

            unitController.UnitAnimator.PerformAbilityAction(baseAbility, clipIndex);

            // wait for the attack to complete before allowing the character to attack again
            attackCoroutine = abilityCasterMonoBehaviour.StartCoroutine(WaitForAbilityActionToComplete(baseAbility, abilityEffectContext, targetUnitController, speedNormalizedAnimationLength));

            return speedNormalizedAnimationLength;
        }

        public IEnumerator WaitForAbilityActionToComplete(AbilityProperties baseAbilityProperties, AbilityEffectContext abilityEffectContext, UnitController targetUnitController, float animationLength) {
            //Debug.Log($"{unitController.gameObject.name}.WaitForAbilityActionToComplete({baseAbilityProperties.ResourceName}, animationLength: {animationLength})");
            
            float remainingTime = animationLength;
            //Debug.Log($"{gameObject.name}waitforanimation remainingtime: " + remainingTime + "; MyWaitingForHits: " + PerformingAutoAttack + "; PerformingAnimatedAbility: " + performingAnimatedAbility);
            while (remainingTime > 0f && PerformingAnyAbility() == true) {
                //Debug.Log($"{gameObject.name}.WaitForAttackToComplete(" + animationLength + "): remainingTime: " + remainingTime + "; PerformingAutoAttack: " + PerformingAutoAttack + "; PerformingAnimatedAbility: " + performingAnimatedAbility + "; animationSpeed: " + animator.GetFloat("AnimationSpeed"));
                yield return null;
                remainingTime -= Time.deltaTime;
                for (int i = 0; i < hitEventCache.Count; i++) {
                    var e = hitEventCache[i];
                    if (!e.Fired && remainingTime <= e.Time) {
                        // Trigger the logic (e.g., Hit 1, then Hit 2)
                        unitController.CharacterCombat.AttackHitAnimationEvent(abilityEffectContext);

                        e.Fired = true;
                        hitEventCache[i] = e; // Update the struct in the list
                    }
                }
            }

            abilityEffectContext.baseAbility.HandleAbilityEndHit(
                unitController,
                targetUnitController,
                abilityEffectContext);

            ProcessAbilityActionEnd();
        }

        /// <summary>
        /// Return true if the ability hit, false if it missed
        /// </summary>
        /// <returns></returns>
        public override bool DidAbilityHit(Interactable target, AbilityEffectContext abilityEffectContext) {
            if (unitController.CharacterCombat.DidAttackMiss() == true) {
                //Debug.Log(DisplayName + ".BaseAbility.PerformAbilityHit(" + source.name + ", " + target.name + "): attack missed");
                unitController.CharacterCombat.ReceiveCombatMiss(target, abilityEffectContext);
                if (target?.CharacterUnit != null) {
                    target.CharacterUnit.UnitController.CharacterCombat.ReceiveCombatMiss(target, abilityEffectContext);
                }
                return false;
            }
            return base.DidAbilityHit(target, abilityEffectContext);
        }

        public override bool PerformAbilityActionCheck(AbilityProperties baseAbility) {
            if (performingAnimatedAbility == true) {
                unitController.UnitEventController.NotifyOnAbilityActionCheckFail(baseAbility);
                return false;
            }
            return base.PerformAbilityActionCheck(baseAbility);
        }

        public override bool ProcessAnimatedAbilityHit(Interactable target, bool deactivateAutoAttack) {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.ProcessAnimatedAbilityHit(" + (target == null ? "null" : target.gameObject.name) + ", " + deactivateAutoAttack + ")");
            // we can now continue because everything beyond this point is single target oriented and it's ok if we cancel attacking due to lack of alive/unfriendly target
            // check for friendly target in case it somehow turned friendly mid swing
            if (target == null || deactivateAutoAttack == true) {
                unitController.CharacterCombat.DeactivateAutoAttack();
                return false;
            }

            if (unitController.CharacterCombat.AutoAttackActive == false) {
                //Debug.Log($"{gameObject.name}.CharacterCombat.AttackHit_AnimationEvent(): activating auto-attack");
                unitController.CharacterCombat.ActivateAutoAttack();
            }
            return base.ProcessAnimatedAbilityHit(target, deactivateAutoAttack);
        }

        public override bool PerformWeaponAffinityCheck(AbilityProperties baseAbility, bool playerInitiated = false) {
            foreach (WeaponSkill _weaponAffinity in baseAbility.WeaponAffinityList) {
                if (unitController != null && unitController.CharacterEquipmentManager != null && unitController.CharacterEquipmentManager.HasAffinity(_weaponAffinity)) {
                    return true;
                }
            }

            if (playerInitiated) {
                unitController.UnitEventController.NotifyOnCombatMessage("Cannot cast " + baseAbility.DisplayName + ". Required weapon not equipped!");
            }
            // intentionally not calling base because it's always true
            return false;
        }

        public override void CleanupCoroutines() {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilitymanager.CleanupCoroutines()");
            base.CleanupCoroutines();
            if (currentCastCoroutine != null) {
                abilityCasterMonoBehaviour.StopCoroutine(currentCastCoroutine);
                EndCastCleanup();
            }
            CleanupCoolDownRoutines();

            if (globalCoolDownCoroutine != null) {
                abilityCasterMonoBehaviour.StopCoroutine(globalCoolDownCoroutine);
                globalCoolDownCoroutine = null;
            }
            abilityCasterMonoBehaviour.StopAllCoroutines();
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
            if (targetableEffect.GetTargetOptions(unitController).CanCastOnOthers == true
                && targetableEffect.GetTargetOptions(unitController).CanCastOnEnemy == false
                && targetableEffect.GetTargetOptions(unitController).CanCastOnNeutral == false
                && targetableEffect.GetTargetOptions(unitController).CanCastOnFriendly == false) {
                return true;
            }

            float relationValue = Faction.RelationWith(targetCharacterUnit.UnitController, unitController);

            if (targetableEffect.GetTargetOptions(unitController).CanCastOnEnemy == true && relationValue <= -1) {
                return true;
            }

            if (targetableEffect.GetTargetOptions(unitController).CanCastOnNeutral == true && relationValue > -1 && targetableEffect.GetTargetOptions(unitController).CanCastOnNeutral == true && relationValue < 1) {
                return true;
            }

            if (targetableEffect.GetTargetOptions(unitController).CanCastOnFriendly == true && relationValue >= 1) {
                return true;
            }

            return false;

            //return base.PerformFactionCheck(targetableEffect, targetCharacterUnit, targetIsSelf);
        }

        public override void BeginAbilityCoolDown(AbilityProperties baseAbility, float coolDownLength = -1f) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterAbilityManager.BeginAbilityCoolDown({baseAbility.ResourceName}, {coolDownLength})");

            base.BeginAbilityCoolDown(baseAbility, coolDownLength);

            float abilityCoolDown = 0f;

            if (coolDownLength == -1f) {
                abilityCoolDown = baseAbility.CoolDown;
            } else {
                abilityCoolDown = coolDownLength;
            }

            if (abilityCoolDown <= 0f && baseAbility.IgnoreGlobalCoolDown == false && baseAbility.GetAbilityCastingTime(unitController) == 0f) {
                // if the ability had no cooldown, and wasn't ignoring global cooldown, it gets a global cooldown length cooldown as we shouldn't have 0 cooldown instant cast abilities
                abilityCoolDown = Mathf.Clamp(abilityCoolDown, 1, Mathf.Infinity);
            }

            if (abilityCoolDown == 0f) {
                // if the ability CoolDown is still zero (this was an ability with a cast time that doesn't need a cooldown), don't start cooldown coroutine
                return;
            }

            AbilityCoolDownNode abilityCoolDownNode = new AbilityCoolDownNode();
            abilityCoolDownNode.AbilityName = baseAbility.ResourceName;

            // need to account for auto-attack
            if (systemConfigurationManager.AllowAutoAttack == false && baseAbility.IsAutoAttack == true) {
                abilityCoolDownNode.RemainingCoolDown = abilityCoolDown;
            } else {
                abilityCoolDownNode.RemainingCoolDown = abilityCoolDown;
            }

            abilityCoolDownNode.InitialCoolDown = abilityCoolDownNode.RemainingCoolDown;

            if (!abilityCoolDownDictionary.ContainsKey(baseAbility.ResourceName)) {
                abilityCoolDownDictionary[baseAbility.ResourceName] = abilityCoolDownNode;
            }

            // ordering important.  don't start till after its in the dictionary or it will fail to remove itself from the dictionary, then add it self
            Coroutine coroutine = abilityCasterMonoBehaviour.StartCoroutine(PerformAbilityCoolDown(baseAbility.ResourceName));
            abilityCoolDownNode.Coroutine = coroutine;

            unitController.UnitEventController.NotifyOnBeginAbilityCoolDown(baseAbility, coolDownLength);
        }

        public override void BeginActionCoolDown(InstantiatedActionItem actionItem, float coolDownLength = -1f) {

            base.BeginActionCoolDown(actionItem, coolDownLength);

            float coolDown = 0f;

            if (coolDownLength == -1f) {
                coolDown = actionItem.CoolDown;
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
            abilityCoolDownNode.AbilityName = actionItem.ResourceName;
            abilityCoolDownNode.RemainingCoolDown = coolDown;
            abilityCoolDownNode.InitialCoolDown = abilityCoolDownNode.RemainingCoolDown;

            if (!abilityCoolDownDictionary.ContainsKey(actionItem.ResourceName)) {
                abilityCoolDownDictionary[actionItem.ResourceName] = abilityCoolDownNode;
            }

            // ordering important.  don't start till after its in the dictionary or it will fail to remove itself from the dictionary, then add it self
            Coroutine coroutine = abilityCasterMonoBehaviour.StartCoroutine(PerformAbilityCoolDown(actionItem.ResourceName));
            abilityCoolDownNode.Coroutine = coroutine;

            unitController.UnitEventController.NotifyOnBeginActionCoolDown(actionItem, coolDownLength);
        }

        public void HandleEquipmentChanged(InstantiatedEquipment newItem, InstantiatedEquipment oldItem, int slotIndex) {
            //Debug.Log($"{gameObject.name}.CharacterAbilityManager.HandleEquipmentChanged(" + (newItem != null ? newItem.DisplayName : "null") + ", " + (oldItem != null ? oldItem.DisplayName : "null") + ")");
            if (oldItem != null) {
                if (oldItem.Equipment.OnEquipAbilityEffect != null) {
                    unitController.CharacterStats.GetStatusEffectNode(oldItem.Equipment.OnEquipAbilityEffect.StatusEffectProperties)?.CancelStatusEffect();
                }
                foreach (AbilityProperties baseAbility in oldItem.Equipment.LearnedAbilities) {
                    UnlearnAbility(baseAbility);
                }
            }
            UpdateEquipmentTraits(oldItem);

            if (newItem != null) {
                if (newItem.Equipment.OnEquipAbilityEffect != null) {
                    if (unitController != null) {
                        newItem.Equipment.OnEquipAbilityEffect.AbilityEffectProperties.Cast(unitController, unitController, unitController, null);
                    }
                }
                foreach (AbilityProperties baseAbility in newItem.Equipment.LearnedAbilities) {
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

        public void UpdateEquipmentTraits(InstantiatedEquipment equipment) {

            if (equipment == null || equipment.Equipment.EquipmentSet == null) {
                // nothing to do
                return;
            }

            int equipmentCount = 0;

            if (unitController != null && unitController.CharacterEquipmentManager != null) {
                equipmentCount = unitController.CharacterEquipmentManager.GetEquipmentSetCount(equipment.Equipment.EquipmentSet);
            }

            for (int i = 0; i < equipment.Equipment.EquipmentSet.TraitList.Count; i++) {
                StatusEffectProperties statusEffect = equipment.Equipment.EquipmentSet.TraitList[i];
                if (statusEffect != null) {
                    if (equipmentCount > i) {
                        // we are allowed to have this buff
                        if (!unitController.CharacterStats.StatusEffects.ContainsKey(statusEffect.ResourceName)) {
                            ApplyStatusEffect(statusEffect);
                        }
                    } else {
                        // we are not allowed to have this buff
                        if (unitController.CharacterStats.StatusEffects.ContainsKey(statusEffect.ResourceName)) {
                            unitController.CharacterStats.StatusEffects[statusEffect.ResourceName].CancelStatusEffect();
                        }
                    }
                }
            }

        }


        public void UnLearnDefaultAutoAttackAbility() {
            if (unitController?.UnitProfile?.DefaultAutoAttackAbility != null) {
                UnlearnAbility(unitController.UnitProfile.DefaultAutoAttackAbility);
            }
        }

        public void LearnDefaultAutoAttackAbility() {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.LearnDefaultAutoAttackAbility()");
            if (autoAttackAbility != null) {
                //Debug.Log($"{gameObject.name}.CharacterAbilityManager.LearnDefaultAutoAttackAbility(): auto-attack already know, exiting");
                // can't learn two auto-attacks at the same time
                return;
            }
            if (unitController.UnitProfile?.DefaultAutoAttackAbility != null) {
                //Debug.Log($"{gameObject.name}.CharacterAbilityManager.LearnDefaultAutoAttackAbility(): learning default auto attack ability");
                LearnAbility(unitController.UnitProfile.DefaultAutoAttackAbility);
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
                if (unitController.CharacterStats.HasStatusEffect(statusEffect.AbilityEffectProperties.ResourceName) == false) {
                    ApplyStatusEffect(statusEffect.AbilityEffectProperties);
                }
            }
        }

        public void ApplyStatusEffect(AbilityEffectProperties statusEffect, int overrideDuration = 0) {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.ApplyStatusEffect(" + statusEffect.DisplayName + ")");
            if (unitController.CharacterStats != null) {
                AbilityEffectContext abilityEffectContext = new AbilityEffectContext(unitController);
                abilityEffectContext.overrideDuration = overrideDuration;
                // rememeber this method is meant for saved status effects
                // and traits
                abilityEffectContext.savedEffect = true;
                if (statusEffect != null) {
                    // testing : to allow npcs to get their visuals from traits, send in unit controller if it exists
                    //_abilityEffect.Cast(baseCharacter, baseCharacter?.UnitController, null, abilityEffectContext);
                    // testing prevent spawn of object since unitController now handles notifications that do that for all characters, not just the player
                    statusEffect.Cast(unitController, null, null, abilityEffectContext);
                }
            }
        }

        public override bool IsPlayerControlled() {
            if (unitController.MasterUnit != null &&
                unitController.MasterUnit == playerManager.UnitController) {

                return true;
            }
            return base.IsPlayerControlled();
        }

        public override void AddPet(CharacterUnit target) {
            if (unitController.CharacterPetManager != null
                && target.Interactable != null
                && target.Interactable is UnitController
                && (target.Interactable as UnitController).UnitProfile != null) {
                unitController.CharacterPetManager.AddPet((target.Interactable as UnitController).UnitProfile);
            }
        }

        public void ApplySavedStatusEffects(StatusEffectSaveData statusEffectSaveData) {
            // don't crash when loading old save data
            if (statusEffectSaveData.StatusEffectName == null || statusEffectSaveData.StatusEffectName == string.Empty) {
                return;
            }
            StatusEffect savedEffect = systemDataFactory.GetResource<AbilityEffect>(statusEffectSaveData.StatusEffectName) as StatusEffect;
            if (savedEffect != null && savedEffect.StatusEffectProperties.ZoneRequirementMet(unitController)) {
                ApplyStatusEffect(savedEffect.AbilityEffectProperties, statusEffectSaveData.RemainingSeconds);
            }
        }

        public void RemoveCapabilityProviderTraits(List<StatusEffect> statusEffects) {
            if (statusEffects == null) {
                return;
            }
            foreach (StatusEffect statusEffect in statusEffects) {
                if (unitController.CharacterStats != null && unitController.CharacterStats.StatusEffects.ContainsKey(statusEffect.ResourceName)) {
                    unitController.CharacterStats.StatusEffects[statusEffect.ResourceName].CancelStatusEffect();
                }
            }
        }

        public void LearnCapabilityProviderAbilities(List<AbilityProperties> abilities) {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.LearnCapabilityProviderAbilities()");

            if (abilities == null) {
                //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.LearnCapabilityProviderAbilities(): abilities is null");
                return;
            }
            foreach (AbilityProperties baseAbility in abilities) {
                //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.LearnCapabilityProviderAbilities(): process: " + baseAbility.DisplayName);
                if (baseAbility.RequiredLevel <= unitController.CharacterStats.Level && unitController.CharacterAbilityManager.HasAbility(baseAbility) == false) {
                    baseAbility.PrepareToLearnAbility(this);
                    LearnAbility(baseAbility);
                }
            }
        }

        public void UnLearnCapabilityProviderAbilities(List<AbilityProperties> abilities) {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.UnLearnCapabilityProviderAbilities(" + updateActionBars + ")");
            if (abilities == null) {
                return;
            }
            foreach (AbilityProperties oldAbility in abilities) {
                UnlearnAbility(oldAbility);
            }
            unitController.UnitEventController.NotifyOnUnlearnAbilities();
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
                //Debug.Log($"{gameObject.name}.CharacterAbilityManager.PerformAbilityCooldown():  IENUMERATOR: " + abilityCoolDownDictionary[abilityName].MyRemainingCoolDown);
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

            if (AbilityList.ContainsKey(abilityName)) {
                //Debug.Log($"{gameObject.name}.CharacterAbilitymanager.HasAbility( " + abilityName + "): keyname: " + keyName + " TRUE!");
                return true;
            }

            return base.HasAbility(abilityName);
        }

        public override bool HasAbility(AbilityProperties baseAbility) {
            //Debug.Log($"{gameObject.name}.CharacterAbilitymanager.HasAbility(" + abilityName + ")");

            return HasAbility(baseAbility.DisplayName);
        }

        public void ActivateTargettingMode(AbilityProperties baseAbility, Interactable target) {
            //Debug.Log("CharacterAbilityManager.ActivateTargettingMode()");
            targetingModeActive = true;
            groundTargetAbility = baseAbility;
            if (unitController.UnitControllerMode == UnitControllerMode.AI || unitController.UnitControllerMode == UnitControllerMode.Pet) {
                targetingModeActive = false;
                groundTarget = target.transform.position;
            }
            unitController.UnitEventController.NotifyOnActivateTargetingMode(baseAbility);
        }

        public bool WaitingForTarget() {
            //Debug.Log("CharacterAbilityManager.WaitingForTarget(): returning: " + targettingModeActive);
            return targetingModeActive;
        }

        private Vector3 GetGroundTarget() {
            //Debug.Log("CharacterAbilityManager.GetGroundTarget(): returning: " + groundTarget);
            return groundTarget;
        }

        public void SetGroundTarget(Vector3 newGroundTarget) {
            groundTarget = newGroundTarget;
            unitController.UnitEventController.NotifyOnSetGroundTarget(newGroundTarget);
        }

        public void SetGroundTargetClient(Vector3 newGroundTarget) {
            //Debug.Log("CharacterAbilityManager.SetGroundTarget(" + newGroundTarget + ")");
            SetGroundTarget(newGroundTarget);
            if (systemGameManager.GameMode == GameMode.Local || networkManagerServer.ServerModeActive == false || levelManager.IsCutscene()) {
                AbilityProperties ability = groundTargetAbility;
                DeactivateTargetingMode();
                BeginAbility(ability, null);
            }
        }

        public void DeactivateTargetingMode() {
            //Debug.Log($"{unitController.gameObject.name}.CharacterAbilityManager.DeactivateTargetingMode()");

            targetingModeActive = false;
            groundTargetAbility = null;
            castTargettingManager.DisableProjector();
        }

        public void UpdateTraitList(int newLevel) {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.UpdateAbilityList(). length: " + abilityList.Count);

            CapabilityConsumerSnapshot capabilityConsumerSnapshot = new CapabilityConsumerSnapshot(unitController.BaseCharacter, systemGameManager);

            ApplyCapabilityProviderTraits(capabilityConsumerSnapshot.GetTraitList());

        }

        public void UpdateAbilityList(int newLevel) {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.UpdateAbilityList(). length: " + abilityList.Count);

            CapabilityConsumerSnapshot capabilityConsumerSnapshot = new CapabilityConsumerSnapshot(unitController.BaseCharacter, systemGameManager);

            LearnCapabilityProviderAbilities(capabilityConsumerSnapshot.GetAbilityList());
        }

        public void LoadAbility(string abilityName) {
            //Debug.Log($"{unitController.gameObject.name}.PlayerAbilityManager.LoadAbility({abilityName})");

            AbilityProperties abilityProperties = systemDataFactory.GetResource<Ability>(abilityName)?.AbilityProperties;
            if (abilityProperties == null) {
                // if we renamed an ability, old save data could load a null.  prevent invalid abilities from loading.
                return;
            }

            if (abilityList.ContainsKey(abilityName)) {
                // ability is already known, exit
                return;
            }
            if (abilityProperties.CanLearnAbility(this) == false) {
                return;
            }
            abilityProperties.ProcessLoadAbility(this);

            abilityList[abilityName] = abilityProperties;
        }

        public void LearnAutoAttack(AbilityProperties baseAbilityProperties) {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.LearnAutoAttack(" + baseAbilityProperties.DisplayName + "): is auto-attack!");
            UnLearnDefaultAutoAttackAbility();
            autoAttackAbility = baseAbilityProperties;
        }

        public bool LearnAbility(AbilityProperties newAbility) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterAbilityManager.LearnAbility(" + (newAbility == null ? "null" : newAbility.DisplayName) + ")");

            if (newAbility == null) {
                //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.LearnAbility(): baseAbility is null");
                // can't learn a nonexistent ability
                return false;
            }
            if (HasAbility(newAbility)) {
                //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.LearnAbility(" + (newAbility == null ? "null" : newAbility.DisplayName) + "): already known");
                return false;
            }
            if (newAbility.RequiredLevel > unitController.CharacterStats.Level) {
                //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.LearnAbility(" + (newAbility == null ? "null" : newAbility.DisplayName) + "): level too low");
                return false;
            }

            if (newAbility.CanLearnAbility(this) == false) {
                //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.LearnAbility(" + (newAbility == null ? "null" : newAbility.DisplayName) + "): cannot learn ability");
                return false;
            }

            // if we made it this far, there is no reason to not learn the ability
            abilityList[newAbility.ResourceName] = newAbility;

            newAbility.ProcessLearnAbility(this);

            // for now prerequisites are only used on the client
            //newAbility.NotifyOnLearn(unitController);

            unitController.UnitEventController.NotifyOnLearnAbility(newAbility);
            return true;
        }

        public void SetAutoAttackAbility(AbilityProperties baseAbilityProperties) {
            autoAttackAbility = baseAbilityProperties;
        }

        public void UnlearnAbility(AbilityProperties oldAbility) {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.UnleanAbility(" + oldAbility.DisplayName + ", " + updateActionBars + ")");

            if (abilityList.ContainsKey(oldAbility.ResourceName)) {
                oldAbility.ProcessUnLearnAbility(this);
                abilityList.Remove(oldAbility.ResourceName);
            }
            unitController.UnitEventController.NotifyOnUnlearnAbility(oldAbility);
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
        public IEnumerator PerformAbilityCast(AbilityProperties ability, Interactable target, AbilityEffectContext abilityEffectContext) {
            float startTime = Time.time;
            //Debug.Log($"{unitController.gameObject.name}.CharacterAbilitymanager.PerformAbilityCast({ability.ResourceName}, {(target == null ? "null" : target.name)}) Enter Ienumerator with start time: {startTime}");

            abilityEffectContext.originalTarget = target;
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilitymanager.PerformAbilityCast({ability.DisplayName}): cancast is true");
            if (!ability.CanSimultaneousCast) {
                //Debug.Log("CharacterAbilitymanager.PerformAbilityCast() ability: {ability.DisplayName} can simultaneous cast is false, setting casting to true");
                performingCast = true;
                //ability.StartCasting(unitController);
                PerformCastingAnimation(ability);
            }
            float currentCastPercent = 0f;
            float nextTickPercent = 0f;
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilitymanager.PerformAbilityCast() currentCastPercent: " + currentCastPercent + "; MyAbilityCastingTime: " + ability.MyAbilityCastingTime);

            if (unitController != null && ability.GetHoldableObjectList(unitController).Count != 0) {
                //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.PerformAbilityCast({ability.DisplayName}): spawning ability objects");
                if (!ability.AnimatorCreatePrefabs) {
                    SpawnAbilityObjects(ability);
                }
            }
            if (ability.CastingAudioClip != null) {
                unitController.UnitComponentController.PlayCastSound(ability.CastingAudioClip, ability.LoopAudio);
            }
            if (ability.CoolDownOnCast == true) {
                ability.BeginAbilityCoolDown(unitController);
            }

            if (ability.GetAbilityCastingTime(unitController) > 0f) {
                // added target condition to allow channeled spells to stop casting if target disappears
                while (currentCastPercent < 1f
                    && (ability.GetTargetOptions(unitController).RequireTarget == false
                    || (target != null && target.gameObject.activeInHierarchy == true))) {
                    //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilitymanager.PerformAbilityCast({ability.DisplayName}): currentCastPercent: " + currentCastPercent);

                    yield return null;
                    currentCastPercent += (Time.deltaTime / ability.GetAbilityCastingTime(unitController));

                    // call this first because it updates the cast bar
                    //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilitymanager.PerformAbilityCast() currentCastTime: " + currentCastPercent + "; MyAbilityCastingTime: " + ability.GetAbilityCastingTime(baseCharacter) + "; calling OnCastTimeChanged()");
                    if (unitController != null) {
                        unitController.UnitEventController.NotifyOnCastTimeChanged(unitController, ability, currentCastPercent);
                    }

                    // now call the ability on casttime changed (really only here for channeled stuff to do damage)
                    nextTickPercent = ability.OnCastTimeChanged(currentCastPercent, nextTickPercent, unitController, target, abilityEffectContext);
                }
            }

            // set currentCast to null because it isn't automatically null until the next frame and we are about to do stuff which requires it to be null immediately
            EndCastCleanup();

            if (!ability.CanSimultaneousCast) {
                NotifyOnCastComplete();
                performingCast = false;
                unitController.UnitAnimator.ClearCasting();
            }
            PerformAbility(ability, target, abilityEffectContext);
        }

        /*
        public void StartCasting(BaseAbilityProperties ability) {
            //Debug.Log("BaseAbility.OnCastStart(" + source.name + ")");
            List<AnimationClip> usedCastAnimationClips = ability.GetCastClips(unitController);
            if (usedCastAnimationClips != null && usedCastAnimationClips.Count > 0) {
                int clipIndex = UnityEngine.Random.Range(0, usedCastAnimationClips.Count);
                if (usedCastAnimationClips[clipIndex] != null) {
                    // perform the actual animation
                    PerformCastingAnimation(usedCastAnimationClips[clipIndex], ability);
                }

            }
        }
        */

        public void NotifyOnCastCancel() {
            unitController.UnitEventController.NotifyOnCastCancel();
        }

        public void NotifyOnCastComplete() {
            unitController.UnitEventController.NotifyOnCastComplete();
        }

        public override void EndCastCleanup() {
            //Debug.Log(abilityCaster.gameObject.name + ".CharacterAbilitymanager.EndCastCleanup()");
            base.EndCastCleanup();
            if (unitController != null) {
                // stop any casting audio clip
                unitController.UnitComponentController.StopCastSound();

                //stop any animation event audio clip
                unitController.UnitComponentController.StopEffectSound();
            }
        }

        public void AttemptAutoAttack(bool playerInitiated = false) {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilitymanager.AttemtpAutoAttack()");

            if (AutoAttackAbility != null) {
                BeginAbility(AutoAttackAbility, playerInitiated);
            }/* else {
                Debug.LogWarning(baseCharacter.gameObject.name + ".CharacterAbilitymanager.AttemtpAutoAttack() no autoAttackAbility found!");
            }*/
        }

        /// <summary>
        /// This is the entrypoint for character behavior calls and should not be used for anything else due to the runtime ability lookup that happens
        /// </summary>
        /// <param name="abilityName"></param>
        public override bool BeginAbility(string abilityName) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterAbilitymanager.BeginAbility(" + (abilityName == null ? "null" : abilityName) + ")");

            Ability baseAbility = systemDataFactory.GetResource<Ability>(abilityName);
            if (baseAbility != null) {
                return BeginAbility(baseAbility.AbilityProperties);
            }
            return false;
        }

        /// <summary>
        /// The entrypoint to Casting a spell.  handles all logic such as instant/timed cast, current cast in progress, enough mana, target being alive etc
        /// </summary>
        /// <param name="ability"></param>
        public bool BeginAbility(AbilityProperties ability, bool playerInitiated = false) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterAbilitymanager.BeginAbility({ability.ResourceName}, {playerInitiated})");

            if (ability.GetTargetOptions(unitController).RequiresGroundTarget == true) {
                //Debug.Log("CharacterAbilitymanager.BeginAbility() Ability requires a ground target.");
                // we need to wait for the player to select a target
                ActivateTargettingMode(ability, unitController.Target);
                if (WaitingForTarget() == true) {
                    return false;
                }
            }

            return BeginAbility(ability, unitController.Target, playerInitiated);
        }

        public bool BeginAbility(AbilityProperties ability, Interactable target, bool playerInitiated = false) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterAbilityManager.BeginAbility({ability.ResourceName})");

            unitController.UnitEventController.NotifyOnBeginAbility(ability, target, playerInitiated);

            if (systemGameManager.GameMode == GameMode.Local || networkManagerServer.ServerModeActive == true || levelManager.IsCutscene()) {
                AbilityEffectContext abilityEffectContext = new AbilityEffectContext(unitController);
                abilityEffectContext.baseAbility = ability;
                abilityEffectContext.groundTargetLocation = GetGroundTarget();
                return BeginAbilityInternal(ability, target, abilityEffectContext, playerInitiated);
            }

            // in the case this is a client in network mode, returning true will not matter because the only thing that checks this value is consumables, which
            // are called from the server anyway
            return true;
        }

        public override float GetSpeed() {
            return unitController.CharacterStats.SecondaryStats[SecondaryStatType.Speed].CurrentValue / 100f;
        }

        public override float GetAnimationLengthMultiplier() {

            // ensure minimum length is the minimum attack speed
            //return (baseCharacter.UnitController.UnitAnimator.LastAnimationLength / (float)baseCharacter.UnitController.UnitAnimator.LastAnimationHits);
            return (Mathf.Clamp(unitController.UnitAnimator.LastAnimationLength, unitController.CharacterCombat.AttackSpeed, Mathf.Infinity) / (float)unitController.UnitAnimator.LastAnimationHits);
        }

        public override float GetOutgoingDamageModifiers() {
            return unitController.CharacterStats.GetOutGoingDamageModifiers();
        }

        public override void ProcessWeaponHitEffects(AttackEffectProperties attackEffect, Interactable target, AbilityEffectContext abilityEffectContext) {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.ProcessWeaponHitEffects(" + (abilityEffectContext == null ? "null" : "valid") + ")");
            base.ProcessWeaponHitEffects(attackEffect, target, abilityEffectContext);

            // perform default weapon hit sound
            if (abilityEffectContext.baseAbility != null) {
                AudioClip audioClip = abilityEffectContext.baseAbility.GetHitSound(unitController);
                if (audioClip != null) {
                    unitController.UnitComponentController.PlayEffectSound(audioClip);
                }
            }

            if (unitController?.CharacterCombat?.OnHitEffects != null) {
                // handle weapon on hit effects
                List<AbilityEffectProperties> onHitEffectList = new List<AbilityEffectProperties>();
                foreach (AbilityEffectProperties abilityEffect in unitController.CharacterCombat.OnHitEffects) {
                    // prevent accidental infinite recursion of ability effect
                    if (abilityEffect.DisplayName != attackEffect.DisplayName) {
                        onHitEffectList.Add(abilityEffect);
                    }
                }
                attackEffect.PerformAbilityEffects(unitController, target, abilityEffectContext, onHitEffectList);
            }

            foreach (StatusEffectNode statusEffectNode in unitController.CharacterStats.StatusEffects.Values) {
                //Debug.Log($"{gameObject.name}.CharacterCombat.AttackHit_AnimationEvent(): Casting OnHit Ability On Take Damage");
                // this could maybe be done better through an event subscription
                if (statusEffectNode.StatusEffect.WeaponHitAbilityEffectList.Count > 0) {
                    statusEffectNode.StatusEffect.CastWeaponHit(unitController, target, abilityEffectContext);
                }
            }
        }

        /// <summary>
        /// +damage stat from gear and weapon damage
        /// </summary>
        /// <returns></returns>
        public override float GetPhysicalDamage() {
            float returnValue = 0f;
            // weapon damage
            if (unitController.CharacterEquipmentManager != null) {
                returnValue += unitController.CharacterEquipmentManager.GetWeaponDamage();
            }
            return returnValue;
        }

        public override float GetPhysicalPower() {
            if (unitController != null) {
                return LevelEquations.GetSecondaryStatForCharacter(SecondaryStatType.PhysicalDamage, unitController) + LevelEquations.GetSecondaryStatForCharacter(SecondaryStatType.Damage, unitController);
            }
            return base.GetPhysicalPower();
        }

        public override float GetSpellPower() {
            if (unitController != null) {
                return LevelEquations.GetSecondaryStatForCharacter(SecondaryStatType.SpellDamage, unitController) + LevelEquations.GetSecondaryStatForCharacter(SecondaryStatType.Damage, unitController);
            }
            return base.GetSpellPower();
        }

        public override float GetCritChance() {
            if (unitController != null) {
                return LevelEquations.GetSecondaryStatForCharacter(SecondaryStatType.CriticalStrike, unitController);
            }
            return base.GetCritChance();
        }

        protected bool BeginAbilityInternal(AbilityProperties ability, Interactable target, AbilityEffectContext abilityEffectContext, bool playerInitiated = false) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterAbilityManager.BeginAbilityInternal({ability.ResourceName}, {(target == null ? "null" : target.gameObject.name)}, {playerInitiated})");

            if (ability == null) {
                Debug.LogError($"CharacterAbilityManager.BeginAbilityInternal({(ability == null ? "null" : ability.DisplayName)}, {(target == null ? "null" : target.name)}) NO ABILITY FOUND");
                return false;
            }

            if (unitController.ControlLocked == true) {
                //Debug.Log($"{unitController.gameObject.name}.CharacterAbilityManager.BeginAbilityInternal({ability.ResourceName}, {(target != null ? target.name : "null")}) control locked");
                return false;
            }

            if (!CanCastAbility(ability, playerInitiated)) {
                if (playerInitiated) {
                    //Debug.Log($"{unitController.gameObject.name}.CharacterAbilityManager.BeginAbilityInternal({ability.ResourceName}, {(target != null ? target.name : "null")}) cannot cast");
                }
                //Debug.Log($"{unitController.gameObject.name}.CharacterAbilityManager.BeginAbilityInternal({ability.ResourceName}, {(target != null ? target.name : "null")}) cannot cast");
                return false;
            }

            // testing - if this was player initiated, the attack attempt came through right click or action button press
            // those things attempted to interact with a characterUnit which passed a faction check.  assume target is valid and perform quick sanity check
            // if it passes, attempt to enter combat so weapons can be unsheathed even if out of range
            if (playerInitiated) {
                CharacterUnit targetCharacterUnit = null;
                if (target != null) {
                    targetCharacterUnit = CharacterUnit.GetCharacterUnit(target);
                }
                if (targetCharacterUnit?.UnitController != null) {
                    if (Faction.RelationWith(targetCharacterUnit.UnitController, unitController) <= -1) {
                        if (ability.GetTargetOptions(unitController).CanCastOnEnemy == true
                            && targetCharacterUnit.UnitController.CharacterStats.IsAlive == true) {

                            // disable this for now.  npc should pull character into combat when he enters their agro range.  character should pull npc into combat when status effect is applied or ability lands
                            // agro includes a liveness check, so casting necromancy on a dead enemy unit should not pull it into combat with us if we haven't applied a faction or master control buff yet
                            // ...re-enable this because rangers need to pull out their weapons when doing their animation when clicking on action bar
                            if (unitController.CharacterCombat.GetInCombat() == false) {
                                unitController.CharacterCombat.EnterCombat(target);
                            }

                            unitController.CharacterCombat.ActivateAutoAttack();
                            unitController.UnitEventController.NotifyOnBeginCastOnEnemy(targetCharacterUnit.UnitController);
                        }
                    }
                }
            }

            // get final target before beginning casting
            Interactable finalTarget = ability.ReturnTarget(unitController, target, true, abilityEffectContext, playerInitiated);
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.BeginAbilityCommon({ability.DisplayName}) finalTarget: " + (finalTarget == null ? "null" : finalTarget.DisplayName));

            unitController.UnitEventController.NotifyOnAttemptPerformAbility(ability);

            if (finalTarget == null && ability.GetTargetOptions(unitController).RequireTarget == true) {
                if (playerInitiated) {
                    //Debug.Log($"{unitController.gameObject.name}.CharacterAbilityManager.BeginAbilityCommon({ability.ResourceName}, {target?.name}): finalTarget is null. exiting");
                }
                //Debug.Log($"{unitController.gameObject.name}.CharacterAbilityManager.BeginAbilityCommon({ability.ResourceName}, {target?.name}): finalTarget is null. exiting");
                return false;
            }
            if (finalTarget != null && PerformLOSCheck(finalTarget, ability) == false) {
                if (playerInitiated) {
                    ReceiveCombatMessage("Target is not in line of sight");
                }
                //Debug.Log($"{unitController.gameObject.name}.CharacterAbilityManager.BeginAbilityCommon({ability.ResourceName}, {target?.name}): finalTarget is not in line of sight. exiting");
                return false;
            }

            unitController.CancelMountEffects();

            if (ability.CanSimultaneousCast) {
                // directly performing to avoid interference with other abilities being casted
                //Debug.Log($"{gameObject.name}.CharacterAbilityManager.BeginAbilityCommon(): can simultaneous cast");

                // there is no ground target yet because that is handled in performabilitycast below
                PerformAbility(ability, finalTarget, abilityEffectContext);
            } else {
                //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.BeginAbilityCommon(): can't simultanous cast");
                if (currentCastCoroutine == null) {
                    //Debug.Log("Performing Ability {ability.DisplayName} at a cost of " + ability.MyAbilityManaCost.ToString() + ": ABOUT TO START COROUTINE");

                    // we need to do this because we are allowed to stop an outstanding auto-attack to start this cast
                    // we also need to stop any outstanding actions
                    TryToStopCastBlockers();

                    // start the cast (or cast targetting projector)
                    //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.BeginAbilityCommon(" + usedAbility + "): setting currentCastAbility");
                    // currentCastAbility must be set before starting the coroutine because for animated events, the cast time is zero and the variable will be cleared in the coroutine
                    currentCastAbility = ability;
                    currentCastCoroutine = abilityCasterMonoBehaviour.StartCoroutine(PerformAbilityCast(ability, finalTarget, abilityEffectContext));
                } else {
                    // return false so that items in the inventory don't get used if this came from a castable item
                    //Debug.Log($"{unitController.gameObject.name}.CharacterAbilityManager.BeginAbilityCommon({ability.ResourceName}, {target?.name}): A cast was already in progress!");
                    return false;
                    //systemGameManager.LogManager.WriteCombatMessage("A cast was already in progress WE SHOULD NOT BE HERE BECAUSE WE CHECKED FIRST! iscasting: " + isCasting + "; currentcast==null? " + (currentCast == null));
                    // unless.... we got here from the crafting queue, which launches the next item as the last step of the currently in progress cast
                }
            }

            if (unitController.UnitControllerMode == UnitControllerMode.AI || unitController.UnitControllerMode == UnitControllerMode.Pet) {
                if (currentCastAbility != null && currentCastAbility.GetTargetOptions(unitController).RequiresGroundTarget == true) {
                    Vector3 groundTarget = Vector3.zero;
                    if (unitController.Target != null) {
                        groundTarget = unitController.Target.transform.position;
                    }
                    SetGroundTarget(groundTarget);
                }

            }

            return true;
        }

        public override void ReceiveCombatMessage(string messageText) {
            base.ReceiveCombatMessage(messageText);
            unitController.UnitEventController.NotifyOnCombatMessage(messageText);
        }

        public override void ReceiveMessageFeedMessage(string messageText) {
            base.ReceiveMessageFeedMessage(messageText);
            unitController.UnitEventController.NotifyOnWriteMessageFeedMessage(messageText);
        }


        // this only checks if the ability is able to be cast based on character state.  It does not check validity of target or ability specific requirements
        public override bool CanCastAbility(AbilityProperties ability, bool playerInitiated = false) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterAbilityManager.CanCastAbility({ability.ResourceName}, {playerInitiated})");

            // check if the ability is learned yet
            if (!PerformLearnedCheck(ability)) {
                //Debug.Log($"{unitController.gameObject.name}.CharacterAbilityManager.CanCastAbility({ability.ResourceName}): Have not learned ability!");
                unitController.UnitEventController.NotifyOnCombatMessage($"Cannot cast {ability.DisplayName} Have not learned ability!");
                return false;
            }

            // check if the ability is on cooldown
            if (!PerformCooldownCheck(ability)) {
                //Debug.Log($"{unitController.gameObject.name}.CharacterAbilityManager.CanCastAbility({ability.ResourceName}): ability is on cooldown!");
                unitController.UnitEventController.NotifyOnCombatMessage($"Cannot cast {ability.DisplayName}: ability is on cooldown!");
                return false;
            }

            // check if auto-attack cooldown based on attack speed applies
            if (ability.ReadyToCast(unitController.CharacterCombat) == false) {
                //Debug.Log($"{unitController.gameObject.name}.CharacterAbilityManager.CanCastAbility({ability.ResourceName}): auto-attack cooldown based on attack speed applies!");
                return false;
            }

            // check if we have enough mana
            if (!PerformPowerResourceCheck(ability)) {
                //Debug.Log($"{unitController.gameObject.name}.CharacterAbilityManager.CanCastAbility({ability.ResourceName}): do not have sufficient power resource to cast!");
                return false;
            }

            if (!PerformCombatCheck(ability)) {
                //Debug.Log($"{unitController.gameObject.name}.CharacterAbilityManager.CanCastAbility({ability.DisplayName}): cannot cast ability in combat!");
                if (playerInitiated) {
                    unitController.UnitEventController.NotifyOnCombatMessage($"Cannot cast {ability.DisplayName}: cannot cast ability in combat!");
                }
                return false;
            }

            if (!PerformStealthCheck(ability)) {
                //Debug.Log($"{unitController.gameObject.name}.CharacterAbilityManager.CanCastAbility({ability.DisplayName}): cannot cast ability in combat!");
                if (playerInitiated) {
                    unitController.UnitEventController.NotifyOnCombatMessage($"Cannot cast {ability.DisplayName}: cannot cast ability unless stealthed!");
                }
                return false;
            }


            if (!PerformLivenessCheck(ability)) {
                //Debug.Log($"{unitController.gameObject.name}.CharacterAbilityManager.CanCastAbility({ability.DisplayName}): cannot cast while dead!");
                if (playerInitiated) {
                    unitController.UnitEventController.NotifyOnCombatMessage($"Cannot cast {ability.DisplayName}: cannot cast while dead!");
                }
                return false;
            }

            // for now require a player to perform movement check because an NPC will by default stop and go into attack mode to cast an ability
            // this check is designed to prevent players from casting anything other than instant casts while running
            if (playerInitiated && !PerformMovementCheck(ability)) {
                //Debug.Log($"{unitController.gameObject.name}.CharacterAbilityManager.CanCastAbility({ability.DisplayName}): velocity too high to cast!");
                unitController.UnitEventController.NotifyOnCombatMessage($"Cannot cast {ability.DisplayName}: cannot cast while moving!");
                return false;
            }

            // default is true, nothing has stopped us so far
            //Debug.Log($"{unitController.gameObject.name}.CharacterAbilityManager.CanCastAbility({ability.DisplayName}): returning true");
            return base.CanCastAbility(ability);
        }

        public bool PerformLivenessCheck(AbilityProperties ability) {
            if (!unitController.CharacterStats.IsAlive) {
                return false;
            }
            return true;
        }

        public bool PerformMovementCheck(AbilityProperties ability) {
            if (ability.CanCastWhileMoving || ability.GetAbilityCastingTime(unitController) == 0f) {
                return true;
            }
            return !(unitController.ApparentVelocity > 0.1f);
        }

        public bool PerformLearnedCheck(AbilityProperties ability) {

            if (!ability.UseableWithoutLearning && !AbilityList.ContainsKey(ability.ResourceName)) {
                unitController.UnitEventController.NotifyOnLearnedCheckFail(ability);
                return false;
            }
            return true;
        }

        public bool PerformCooldownCheck(AbilityProperties ability) {
            //Debug.Log($"{gameObject.name}.CharacterAbilityManager.PerformCooldownCheck({ability.DisplayName}) : global: " + MyRemainingGlobalCoolDown);
            if (abilityCoolDownDictionary.ContainsKey(ability.DisplayName) ||
                (RemainingGlobalCoolDown > 0f && ability.IgnoreGlobalCoolDown == false)) {
                return false;
            }
            return true;
        }

        public bool PerformCombatCheck(AbilityProperties ability) {
            if (ability.RequireOutOfCombat == true && unitController.CharacterCombat.GetInCombat() == true) {
                unitController.UnitEventController.NotifyOnCombatCheckFail(ability);
                return false;
            }
            return true;
        }

        public bool PerformStealthCheck(AbilityProperties ability) {
            if (ability.RequireStealth == true && unitController.CharacterStats.IsStealthed == false) {
                unitController.UnitEventController.NotifyOnStealthCheckFail(ability);
                return false;
            }
            return true;
        }


        /// <summary>
        /// Check if the caster has the required amount of the power resource to cast the ability
        /// </summary>
        /// <param name="ability"></param>
        /// <returns></returns>
        public bool PerformPowerResourceCheck(AbilityProperties ability) {
            if (unitController.CharacterStats.PerformPowerResourceCheck(ability, ability.GetResourceCost(unitController))) {
                return true;
            }
            unitController.UnitEventController.NotifyOnPowerResourceCheckFail(ability, unitController);
            return false;
        }

        /// <summary>
        /// Casts a spell.  Note that this does not do the actual damage yet since the ability may have a travel time
        /// </summary>
        /// <param name="ability"></param>
        /// <param name="target"></param>
        public void PerformAbility(AbilityProperties ability, Interactable target, AbilityEffectContext abilityEffectContext) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterAbilityManager.PerformAbility({ability.ResourceName})");

            if (abilityEffectContext == null) {
                abilityEffectContext = new AbilityEffectContext(unitController);
                abilityEffectContext.baseAbility = ability;
            }
            abilityEffectContext.originalTarget = target;
            Interactable finalTarget = target;

            if (!PerformPowerResourceCheck(ability)) {
                return;
            }

            if (ability.GetResourceCost(unitController) != 0 && ability.PowerResource != null) {
                // testing - should add ever be used?
                //abilityEffectContext.AddResourceAmount(ability.PowerResource.DisplayName, ability.GetResourceCost(baseCharacter));
                abilityEffectContext.SetResourceAmount(ability.PowerResource.ResourceName, ability.GetResourceCost(unitController));
                // intentionally not keeping track of this coroutine.  many of these could be in progress at once.
                abilityCasterMonoBehaviour.StartCoroutine(UsePowerResourceDelay(ability.PowerResource, (int)ability.GetResourceCost(unitController), ability.SpendDelay));
            }

            ability.Cast(unitController, finalTarget, abilityEffectContext);
            unitController.UnitEventController.NotifyOnPerformAbility(ability);
        }


        public IEnumerator UsePowerResourceDelay(PowerResource powerResource, int amount, float delay) {
            float elapsedTime = 0f;
            while (elapsedTime < delay) {
                yield return null;
                elapsedTime += Time.deltaTime;
            }
            unitController.CharacterStats.UsePowerResource(powerResource, amount);
        }

        /// <summary>
        /// Stop casting if the character is manually moved with the movement keys
        /// </summary>
        public void HandleManualMovement() {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.HandleManualMovement()");
            // adding new code to require some movement distance to prevent gravity while standing still from triggering this
            if (unitController.ApparentVelocity <= 0.1f) {
                //Debug.Log("CharacterAbilityManager.HandleManualMovement(): velocity too low, doing nothing");
                return;
            }

            //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.HandleManualMovement(): apparent velocity > 0.1f : " + baseCharacter.UnitController.ApparentVelocity);
            if (currentCastAbility != null
                && (currentCastAbility.CanCastWhileMoving == true ||
                currentCastAbility.GetTargetOptions(unitController).RequiresGroundTarget == true
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
            unitController?.UnitActionManager.TryToStopAction();
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

            abilityCasterMonoBehaviour.StopCoroutine(attackCoroutine);

            ProcessAbilityActionEnd();

            NotifyOnCastCancel();
        }

        private void ProcessAbilityActionEnd() {
            //Debug.Log($"{unitController.gameObject.name}.CharacterAbilityManager.ProcessAbilityActionEnd()");

            attackCoroutine = null;
            if (performingAutoAttack == true) {
                performingAutoAttack = false;
            }
            if (performingAnimatedAbility == true) {
                performingAnimatedAbility = false;
            }
            currentAbilityEffectContext = null;
            unitController.UnitAnimator.ClearAnimatedAbility();
            DespawnAbilityObjects();
        }

        private void StopCasting() {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.StopCasting()");

            abilityCasterMonoBehaviour.StopCoroutine(currentCastCoroutine);
            currentCastCoroutine = null;
            unitController.UnitAnimator.ClearCasting();
            DespawnAbilityObjects();
            EndCastCleanup();
            NotifyOnCastCancel();
        }

        public void HandleCharacterUnitDespawn() {
            TryToStopAnyAbility();
        }

        public override AudioClip GetAnimatedAbilityHitSound() {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilitymanager.GetAnimatedAbilityHitSound()");
            if (unitController?.CharacterCombat != null && unitController.CharacterCombat.DefaultHitSoundEffects.Count > 0) {
                return unitController.CharacterCombat.DefaultHitSoundEffects[UnityEngine.Random.Range(0, unitController.CharacterCombat.DefaultHitSoundEffects.Count)];
            }
            return base.GetAnimatedAbilityHitSound();
        }

        /// <summary>
        /// This will be triggered in response to things like hammer taps, not attacks
        /// </summary>
        public void AnimationHitAnimationEvent() {
            //Debug.Log($"{gameObject.name}.CharacterAbilitymanager.AnimationHitAnimationEvent()");

            if (currentCastAbility != null) {
                AudioClip audioClip = currentCastAbility.GetAnimationEventSound();
                if (audioClip != null) {
                    unitController.UnitComponentController.PlayEffectSound(audioClip);
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
                    unitController.UnitComponentController.PlayEffectSound(audioClip, currentCastAbility.LoopAudio);
                }
                return;
            }

            // here character combat is sent in because currentAbilityEffectContext is only used for animated abilities
            // which requires considering the weapon skill
            if (currentAbilityEffectContext != null) {
                AudioClip audioClip = currentAbilityEffectContext.baseAbility.GetAnimationEventSound(unitController.CharacterCombat);
                if (audioClip != null) {
                    unitController.UnitComponentController.PlayEffectSound(audioClip, currentAbilityEffectContext.baseAbility.LoopAudio);
                }
                return;
            }
        }

        /// <summary>
        /// Stops playing audio in response to the StopAudio() animation event
        /// </summary>
        public void StopAudioAnimationEvent() {
            //Debug.Log($"{gameObject.name}.CharacterAbilitymanager.StopAudioAnimationEvent()");

            unitController.UnitComponentController.StopEffectSound();
        }

        public override void InitiateGlobalCooldown(float coolDownToUse = 0f) {
            //Debug.Log($"{gameObject.name}.CharacterAbilitymanager.InitiateGlobalCooldown(" + coolDownToUse + ")");
            base.InitiateGlobalCooldown(coolDownToUse);
            if (globalCoolDownCoroutine == null) {
                // set global cooldown length to animation length so we don't end up in situation where cast bars look fine, but we can't actually cast
                globalCoolDownCoroutine = abilityCasterMonoBehaviour.StartCoroutine(BeginGlobalCoolDown(coolDownToUse));
                unitController.UnitEventController.NotifyOnInitiateGlobalCooldown(coolDownToUse);
            } else {
                Debug.LogWarning("CharacterAbilityManager.InitiateGlobalCooldown(): INVESTIGATE: GCD COROUTINE WAS NOT NULL");
            }
        }

        public IEnumerator BeginGlobalCoolDown(float coolDownTime) {
            //Debug.Log($"{gameObject.name}.CharacterAbilityManager.BeginGlobalCoolDown(" + coolDownTime + ")");
            // 10 is kinda arbitrary, but if any animation is causing a GCD greater than 10 seconds, we've probably got issues anyway...
            // the current longest animated attack is ground slam at around 4 seconds
            remainingGlobalCoolDown = Mathf.Clamp(coolDownTime, 1, 10);
            initialGlobalCoolDown = remainingGlobalCoolDown;
            while (remainingGlobalCoolDown > 0f) {
                yield return null;
                remainingGlobalCoolDown -= Time.deltaTime;
                // we want to end immediately if the time is up or the cooldown coroutine will not be nullifed until the next frame
                //Debug.Log($"{gameObject.name}.CharacterAbilityManager.BeginGlobalCoolDown(): in loop; remaining time: " + remainingGlobalCoolDown);
            }
            globalCoolDownCoroutine = null;
        }

        public override void ProcessAbilityCoolDowns(AbilityProperties baseAbility, float animationLength, float abilityCoolDown) {
            base.ProcessAbilityCoolDowns(baseAbility, animationLength, abilityCoolDown);
            if (unitController.UnitControllerMode == UnitControllerMode.Player) {
                if (systemConfigurationManager.AllowAutoAttack == true && baseAbility.IsAutoAttack) {
                    return;
                }
            }

            baseAbility.ProcessGCDManual(unitController, Mathf.Min(animationLength, abilityCoolDown));
            BeginAbilityCoolDown(baseAbility, Mathf.Max(animationLength, abilityCoolDown));
        }

        public override Dictionary<PrefabProfile, List<GameObject>> SpawnAbilityEffectPrefabs(Interactable target, Interactable originalTarget, FixedLengthEffectProperties fixedLengthEffectProperties, AbilityEffectContext abilityEffectContext) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterAbilityManager.SpawnAbilityEffectPrefabs({target?.name}, {originalTarget?.name}, {fixedLengthEffectProperties.ResourceName})");

            Dictionary<PrefabProfile, List<GameObject>> returnValue = base.SpawnAbilityEffectPrefabs(target, originalTarget, fixedLengthEffectProperties, abilityEffectContext);
            unitController.UnitEventController.NotifyOnSpawnAbilityEffectPrefabs(target, originalTarget, fixedLengthEffectProperties, abilityEffectContext);
            
            return returnValue;
        }

        public override Dictionary<PrefabProfile, List<GameObject>> SpawnStatusEffectPrefabs(Interactable target, StatusEffectProperties statusEffectProperties, AbilityEffectContext abilityEffectContext) {
            //Dictionary<PrefabProfile, List<GameObject>> returnList = new Dictionary<PrefabProfile, List<GameObject>>();
            //unitController.UnitEventController.NotifyOnSpawnAbilityEffectPrefabs(target, originalTarget, statusEffectProperties, abilityEffectContext);
            return base.SpawnStatusEffectPrefabs(target, statusEffectProperties, abilityEffectContext);
        }

        public override Dictionary<PrefabProfile, List<GameObject>> SpawnProjectileEffectPrefabs(Interactable target, Interactable originalTarget, ProjectileEffectProperties projectileEffectProperties, AbilityEffectContext abilityEffectContext) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterAbilityManager.SpawnProjectileEffectPrefabs({target?.name}, {originalTarget?.name}, {projectileEffectProperties.ResourceName})");
            
            Dictionary<PrefabProfile, List<GameObject>> returnValue = base.SpawnProjectileEffectPrefabs(target, originalTarget, projectileEffectProperties, abilityEffectContext);
            unitController.UnitEventController.NotifyOnSpawnProjectileEffectPrefabs(target, originalTarget, projectileEffectProperties, abilityEffectContext);
            
            return returnValue;
        }

        public override Dictionary<PrefabProfile, List<GameObject>> SpawnChanneledEffectPrefabs(Interactable target, Interactable originalTarget, ChanneledEffectProperties channeledEffectProperties, AbilityEffectContext abilityEffectContext) {
            //Debug.Log($"{unitController.gameObject.name}.CharacterAbilityManager.SpawnProjectileEffectPrefabs({target?.name}, {originalTarget?.name}, {channeledEffectProperties.ResourceName})");

            Dictionary<PrefabProfile, List<GameObject>> returnValue = base.SpawnChanneledEffectPrefabs(target, originalTarget, channeledEffectProperties, abilityEffectContext);
            unitController.UnitEventController.NotifyOnSpawnChanneledEffectPrefabs(target, originalTarget, channeledEffectProperties, abilityEffectContext);

            return returnValue;
        }

        public override void ReceiveCombatTextEvent(UnitController targetUnitController, int damage, CombatTextType combatTextType, CombatMagnitude combatMagnitude, AbilityEffectContext abilityEffectContext) {
            unitController.UnitEventController.NotifyOnReceiveCombatTextEvent(targetUnitController, damage, combatTextType, combatMagnitude, abilityEffectContext);
            base.ReceiveCombatTextEvent(targetUnitController, damage, combatTextType, combatMagnitude, abilityEffectContext);
        }

        public override void ProcessAbilityEffectPooled(GameObject go) {
            base.ProcessAbilityEffectPooled(go);
            if (abilityEffectObjectLookup.ContainsKey(go) == false) {
                return;
            }
            AbilityAttachmentNode abilityAttachmentNode = abilityEffectObjectLookup[go];
            abilityEffectObjects[abilityAttachmentNode].Remove(go);
            abilityEffectObjectLookup.Remove(go);
        }

    }

}