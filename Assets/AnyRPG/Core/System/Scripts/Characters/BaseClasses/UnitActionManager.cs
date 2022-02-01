using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class UnitActionManager : ConfiguredClass {

        //public event System.Action<BaseCharacter> OnCastCancel = delegate { };
        //public event System.Action<AnimatedAction> OnCombatCheckFail = delegate { };
        //public event System.Action<AnimatedAbility> OnAnimatedAbilityCheckFail = delegate { };
        public event System.Action<string> OnCombatMessage = delegate { };

        private UnitController unitController = null;

        //protected bool isPerformingAction = false;

        private Coroutine currentActionCoroutine = null;

        //private AnimatedActionProperties currentAction = null;

        // the holdable objects spawned during an ability cast and removed when the cast is complete
        protected Dictionary<AbilityAttachmentNode, List<GameObject>> actionObjects = new Dictionary<AbilityAttachmentNode, List<GameObject>>();

        // game manager references
        protected SystemDataFactory systemDataFactory = null;
        //private PlayerManager playerManager = null;
        protected ObjectPooler objectPooler = null;

        public bool ControlLocked {
            get {
                if (unitController != null) {
                    return unitController.ControlLocked;
                }
                return false;
            }
        }

        public bool IsDead {
            get {
                if (unitController.CharacterUnit.BaseCharacter.CharacterStats.IsAlive == false) {
                    return true;
                }
                return false;
            }
        }

        public Coroutine CurrentActionCoroutine { get => currentActionCoroutine; }

        public UnitActionManager(UnitController unitController, SystemGameManager systemGameManager) {
            this.unitController = unitController;
            Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            systemDataFactory = systemGameManager.SystemDataFactory;
            //playerManager = systemGameManager.PlayerManager;
            objectPooler = systemGameManager.ObjectPooler;
        }

        public AttachmentPointNode GetHeldAttachmentPointNode(AbilityAttachmentNode attachmentNode) {
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
                if (unitController.CharacterUnit.BaseCharacter.UnitProfile.UnitPrefabProps.AttachmentProfile != null) {
                    if (unitController.CharacterUnit.BaseCharacter.UnitProfile.UnitPrefabProps.AttachmentProfile.AttachmentPointDictionary.ContainsKey(attachmentNode.AttachmentName)) {
                        return unitController.CharacterUnit.BaseCharacter.UnitProfile.UnitPrefabProps.AttachmentProfile.AttachmentPointDictionary[attachmentNode.AttachmentName];
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

        public void SpawnActionObjects(List<AbilityAttachmentNode> abilityAttachmentNodes) {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.SpawnAbilityObjects()");

            // ensure that any current ability objects are cleared before spawning new ones
            DespawnActionObjects();

            Dictionary<AbilityAttachmentNode, GameObject> holdableObjects = new Dictionary<AbilityAttachmentNode, GameObject>();
            foreach (AbilityAttachmentNode abilityAttachmentNode in abilityAttachmentNodes) {
                if (abilityAttachmentNode != null) {
                    if (abilityAttachmentNode.HoldableObject != null && abilityAttachmentNode.HoldableObject.Prefab != null) {
                        //Debug.Log("EquipmentManager.HandleWeaponSlot(): " + newItem.name + " has a physical prefab");
                        // attach a mesh to a bone for weapons

                        AttachmentPointNode attachmentPointNode = GetHeldAttachmentPointNode(abilityAttachmentNode);
                        if (attachmentPointNode != null) {
                            Transform targetBone = unitController.transform.FindChildByRecursive(attachmentPointNode.TargetBone);

                            if (targetBone != null) {
                                //Debug.Log("CharacterAbilityManager.SpawnAbilityObjects(): targetbone (" + attachmentPointNode.TargetBone + ") is " + targetBone.gameObject.name);
                                GameObject newEquipmentPrefab = objectPooler.GetPooledObject(abilityAttachmentNode.HoldableObject.Prefab, targetBone);
                                //holdableObjects.Add(attachmentNode.MyHoldableObject, newEquipmentPrefab);
                                holdableObjects.Add(abilityAttachmentNode, newEquipmentPrefab);
                                //currentEquipmentPhysicalObjects[equipmentSlotProfile] = newEquipmentPrefab;

                                newEquipmentPrefab.transform.localScale = abilityAttachmentNode.HoldableObject.Scale;
                                HoldObject(newEquipmentPrefab, abilityAttachmentNode, unitController.gameObject);
                            } else {
                                Debug.Log("CharacterAbilityManager.SpawnAbilityObjects(). We could not find the target bone " + attachmentPointNode.TargetBone);
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

        public void AddAbilityObject(AbilityAttachmentNode abilityAttachmentNode, GameObject go) {
            if (actionObjects.ContainsKey(abilityAttachmentNode)) {
                actionObjects[abilityAttachmentNode].Add(go);
            } else {
                actionObjects.Add(abilityAttachmentNode, new List<GameObject>() { go });
            }
        }

        public void DespawnActionObjects() {
            //Debug.Log(gameObject.name + ".CharacterAbilityManager.DespawnAbilityObjects()");

            if (actionObjects == null || actionObjects.Count == 0) {
                return;
            }

            foreach (List<GameObject> abilityObjectPrefabs in actionObjects.Values) {
                if (abilityObjectPrefabs != null) {
                    foreach (GameObject abilityObject in abilityObjectPrefabs) {
                        if (abilityObject != null) {
                            objectPooler.ReturnObjectToPool(abilityObject);
                        }
                    }
                }
            }
            actionObjects.Clear();
        }

        public AnimationProps GetUnitAnimationProps() {
            //Debug.Log(gameObject.name + ".GetDefaultAttackAnimations()");
            if (unitController.CharacterUnit.BaseCharacter.UnitProfile?.UnitPrefabProps?.AnimationProps != null) {
                return unitController.CharacterUnit.BaseCharacter.UnitProfile.UnitPrefabProps.AnimationProps;
            }
            if (systemConfigurationManager.DefaultAnimationProfile != null) {
                return systemConfigurationManager.DefaultAnimationProfile.AnimationProps;
            }
            return null;
        }

        /*
        public List<AnimationClip> GetUnitCastAnimations() {
            //Debug.Log(gameObject.name + ".GetDefaultAttackAnimations()");
            if (unitController?.UnitAnimator?.CurrentAnimations != null) {
                return unitController.UnitAnimator.CurrentAnimations.CastClips;
            }
            return new List<AnimationClip>();
        }
        */

        public void PerformActionAnimation(AnimationClip animationClip, AnimatedActionProperties animatedActionProperties) {
            if (animationClip != null) {
                unitController.UnitAnimator.HandleAction(animationClip, animatedActionProperties);
            }
        }

        public void CleanupCoroutines() {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilitymanager.CleanupCoroutines()");
            if (currentActionCoroutine != null) {
                StopAction();
                //unitController.StopCoroutine(currentActionCoroutine);
                //EndActionCleanup();
            }

            //abilityCaster.StopAllCoroutines();
        }


        public void HandleDie(CharacterStats _characterStats) {
            //Debug.Log(baseCharacter.gameObject.name + ".HandleDie()");

            if (currentActionCoroutine != null) {
                StopAction();
            }
        }


        /// <summary>
        /// Cast a spell with a cast timer
        /// </summary>
        /// <param name="animatedAction"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public IEnumerator PerformActionCast(AnimatedActionProperties animatedActionProperties, Interactable target) {
            float startTime = Time.time;
            //Debug.Log(baseCharacter.gameObject.name + "CharacterAbilitymanager.PerformAbilityCast(" + ability.DisplayName + ", " + (target == null ? "null" : target.name) + ") Enter Ienumerator with tag: " + startTime);

            PerformActionAnimation(animatedActionProperties.AnimationClip, animatedActionProperties);

            float currentCastPercent = 0f;
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilitymanager.PerformAbilityCast() currentCastPercent: " + currentCastPercent + "; MyAbilityCastingTime: " + ability.MyAbilityCastingTime);

            if (animatedActionProperties.HoldableObjectList.Count != 0) {
                //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.PerformAbilityCast(" + ability.DisplayName + "): spawning ability objects");
                SpawnActionObjects(animatedActionProperties.HoldableObjectList);
            }
            if (animatedActionProperties.CastingAudioClip != null) {
                unitController.UnitComponentController.PlayCastSound(animatedActionProperties.CastingAudioClip);
            }

            if (animatedActionProperties.ActionCastingTime > 0f) {
                while (currentCastPercent < 1f) {
                    //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilitymanager.PerformAbilityCast(" + ability.DisplayName + "): currentCastPercent: " + currentCastPercent);

                    yield return null;
                    currentCastPercent += (Time.deltaTime / animatedActionProperties.ActionCastingTime);
                }
            }

            /*
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.PerformAbilityCast(" + ability.DisplayName + "). nulling tag: " + startTime);
            // set currentCast to null because it isn't automatically null until the next frame and we are about to do stuff which requires it to be null immediately
            EndActionCleanup();

            //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilitymanager.PerformAbilityCast(): Cast Complete and can cast");
            unitController.UnitAnimator.ClearAction();
            */

            StopAction();
        }

        public void EndActionCleanup() {
            //Debug.Log(abilityCaster.gameObject.name + ".CharacterAbilitymanager.EndCastCleanup()");
            if (unitController != null) {
                unitController.UnitComponentController.StopCastSound();
            }
        }

        /// <summary>
        /// This is the entrypoint for character behavior calls and should not be used for anything else due to the runtime ability lookup that happens
        /// </summary>
        /// <param name="actionName"></param>
        public bool BeginAction(string actionName) {
            //Debug.Log(baseCharacter.gameObject.name + "CharacterAbilitymanager.BeginAbility(" + (abilityName == null ? "null" : abilityName) + ")");
            AnimatedAction animatedAction = systemDataFactory.GetResource<AnimatedAction>(actionName);
            if (animatedAction != null) {
                //return BeginAction(animatedAction);
                return BeginAction(animatedAction);
            }
            return false;
        }

        /// <summary>
        /// Call an action directly, checking if the action is known
        /// </summary>
        /// <returns></returns>
        public bool BeginAction(AnimatedAction animatedAction) {
            return BeginAction(animatedAction.ActionProperties);
        }

        /// <summary>
        /// The entrypoint to Casting a spell.  handles all logic such as instant/timed cast, current cast in progress, enough mana, target being alive etc
        /// </summary>
        /// <param name="animatedAction"></param>
        public bool BeginAction(AnimatedActionProperties animatedActionProperties, bool playerInitiated = false) {
            //Debug.Log(baseCharacter.gameObject.name + "CharacterAbilitymanager.BeginAbility(" + (ability == null ? "null" : ability.DisplayName) + ")");
            if (animatedActionProperties == null) {
                //Debug.Log("CharacterAbilityManager.BeginAbility(): ability is null! Exiting!");
                return false;
            }
            return BeginActionCommon(animatedActionProperties, unitController.Target, playerInitiated);
        }

        public bool BeginAction(AnimatedActionProperties animatedActionProperties, Interactable target) {
            //Debug.Log(gameObject.name + ".CharacterAbilityManager.BeginAbility(" + ability.DisplayName + ")");
            return BeginActionCommon(animatedActionProperties, target);
        }

        protected bool BeginActionCommon(AnimatedActionProperties animatedActionProperties, Interactable target, bool playerInitiated = false) {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.BeginAbilityCommon(" + (ability == null ? "null" : ability.DisplayName) + ", " + (target == null ? "null" : target.gameObject.name) + ")");

            if (unitController != null) {
                if (unitController.ControlLocked == true) {
                    return false;
                }
            }

            if (!CanPerformAction(animatedActionProperties, playerInitiated)) {
                if (playerInitiated) {
                    //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.BeginAbilityCommon(" + ability.DisplayName + ", " + (target != null ? target.name : "null") + ") cannot cast");
                }
                return false;
            }

            /*
            if (playerInitiated) {
                CharacterUnit targetCharacterUnit = null;
                if (target != null) {
                    targetCharacterUnit = CharacterUnit.GetCharacterUnit(target);
                }
            }
            */

            // any action can interrupt any other action
            if (currentActionCoroutine != null) {
                StopAction();
            }

            if (currentActionCoroutine == null) {
                //Debug.Log("Performing Ability " + ability.DisplayName + " at a cost of " + ability.MyAbilityManaCost.ToString() + ": ABOUT TO START COROUTINE");

                // currentAction must be set before starting the coroutine because for animated events, the cast time is zero and the variable will be cleared in the coroutine
                //currentAction = animatedAction;
                currentActionCoroutine = unitController.StartCoroutine(PerformActionCast(animatedActionProperties, target));
            } else {
                // return false so that items in the inventory don't get used if this came from a castable item
                return false;
            }

            return true;
        }

        // this only checks if the ability is able to be cast based on character state.  It does not check validity of target or ability specific requirements
        public bool CanPerformAction(AnimatedActionProperties animatedAction, bool playerInitiated = false) {
            //Debug.Log(gameObject.name + ".CharacterAbilityManager.CanCastAbility(" + ability.DisplayName + ")");

            /*
            // check if the action is learned yet
            if (!PerformLearnedCheck(animatedAction)) {
                //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.CanCastAbility(" + ability.DisplayName + "): Have not learned ability!");
                if (playerInitiated) {
                    OnCombatMessage("Cannot cast " + animatedAction.DisplayName + "): Have not learned ability!");
                }
                return false;
            }
            */

            // actions cannot be performed while mounted
            if (!PerformMountedCheck()) {
                return false;
            }

            // actions cannot be performed while any cast is in progress
            if (!PerformCastingCheck()) {
                return false;
            }

            // actions cannot be performed while dead
            if (!PerformLivenessCheck()) {
                //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.CanCastAbility(" + ability.DisplayName + "): cannot cast while dead!");
                if (playerInitiated) {
                    OnCombatMessage("Cannot perform action " + animatedAction.DisplayName + " while dead!");
                }
                return false;
            }

            /*
            // this check is designed to prevent players from casting anything other than instant casts while running
            if (playerInitiated && !PerformMovementCheck(animatedAction)) {
                //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.CanCastAbility(" + ability.DisplayName + "): velocity too high to cast!");
                if (playerInitiated) {
                    OnCombatMessage("Cannot perform action " + animatedAction.DisplayName + " while moving!");
                }
                return false;
            }
            */

            // default is true, nothing has stopped us so far
            return true;
        }

        private bool PerformMountedCheck() {
            if (unitController.Mounted == true) {
                return false;
            }
            return true;
        }

        private bool PerformCastingCheck() {
            if (unitController.CharacterUnit.BaseCharacter.CharacterAbilityManager.WaitingForAnimatedAbility == true
                || unitController.CharacterUnit.BaseCharacter.CharacterCombat.WaitingForAutoAttack == true
                || unitController.CharacterUnit.BaseCharacter.CharacterAbilityManager.IsCasting) {
                return false;
            }
            return true;
        }

        private bool PerformLivenessCheck() {
            if (!unitController.CharacterUnit.BaseCharacter.CharacterStats.IsAlive) {
                return false;
            }
            return true;
        }

        /*
        public bool PerformMovementCheck(AnimatedActionProperties animatedAction) {
            
            //if (animatedAction.GetAbilityCastingTime(baseCharacter) == 0f) {
              //  return true;
            //}
            
            return !(unitController.ApparentVelocity > 0.1f);
        }
    */

        /*
        public bool PerformLearnedCheck(AnimatedAction animatedAction) {

            
            //string keyName = SystemDataFactory.PrepareStringForMatch(animatedAction.DisplayName);
            
            //if (!animatedAction.UseableWithoutLearning && !AbilityList.ContainsKey(keyName)) {
              //  OnLearnedCheckFail(animatedAction);
               // return false;
            //}
            
            return true;
        }
        */

        /// <summary>
        /// Stop casting if the character is manually moved with the movement keys
        /// </summary>
        public void HandleManualMovement() {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.HandleManualMovement()");
            
            // require some movement distance to prevent gravity while standing still from triggering this
            if (unitController.ApparentVelocity > 0.1f && currentActionCoroutine != null) {
                StopAction();
            }
        }

        public void StopAction() {
            //Debug.Log(unitController.gameObject.name + ".UnitActionManager.StopAction()");
            bool stoppedAction = false;
            if (currentActionCoroutine != null) {
                unitController.StopCoroutine(currentActionCoroutine);
                currentActionCoroutine = null;
                EndActionCleanup();
                stoppedAction = true;
            }
            if (stoppedAction) {
                if (unitController.CharacterUnit.BaseCharacter.CharacterEquipmentManager != null) {
                    DespawnActionObjects();
                }
                if (unitController.UnitAnimator != null) {
                    unitController.UnitAnimator.ClearAction();
                }
            }

        }

        public void HandleCharacterUnitDespawn() {
            if (currentActionCoroutine != null) {
                StopAction();
            }
        }


    }

}