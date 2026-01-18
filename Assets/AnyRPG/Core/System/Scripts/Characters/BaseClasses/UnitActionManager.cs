using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class UnitActionManager : ConfiguredClass {

        public event System.Action<string> OnCombatMessage = delegate { };

        private UnitController unitController = null;

        // it is necessary to use the boolean instead of just the coroutine because if the action is misconfigured and has no animation
        // the coroutine will exit without actually setting the coroutine variable, because it doesn't get set until the first yield statement executes
        protected bool isPerformingAction = false;
        private Coroutine currentActionCoroutine = null;

        //private AnimatedActionProperties currentAction = null;

        // the holdable objects spawned during an ability cast and removed when the cast is complete
        protected Dictionary<AbilityAttachmentNode, List<GameObject>> actionObjects = new Dictionary<AbilityAttachmentNode, List<GameObject>>();

        // game manager references
        //private PlayerManager playerManager = null;
        protected ObjectPooler objectPooler = null;
        protected LevelManager levelManager = null;

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
                if (unitController.CharacterStats.IsAlive == false) {
                    return true;
                }
                return false;
            }
        }

        public UnitActionManager(UnitController unitController, SystemGameManager systemGameManager) {
            this.unitController = unitController;
            Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            //playerManager = systemGameManager.PlayerManager;
            objectPooler = systemGameManager.ObjectPooler;
            levelManager = systemGameManager.LevelManager;
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
                if (unitController.BaseCharacter.UnitProfile.UnitPrefabProps.AttachmentProfile != null) {
                    if (unitController.BaseCharacter.UnitProfile.UnitPrefabProps.AttachmentProfile.AttachmentPointDictionary.ContainsKey(attachmentNode.AttachmentName)) {
                        return unitController.BaseCharacter.UnitProfile.UnitPrefabProps.AttachmentProfile.AttachmentPointDictionary[attachmentNode.AttachmentName];
                    }
                }
            }

            return null;
        }

        public void HoldObject(GameObject go, AbilityAttachmentNode attachmentNode, GameObject searchObject) {
            //Debug.Log($"{unitController.gameObject.name}.UnitActionManager.HoldObject(" + go.name + ", " + searchObject.name + ")");
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
                    Debug.LogWarning($"CharacterAbilityManager.HoldObject(): Unable to find target bone : {attachmentPointNode.TargetBone}");
                }
            } else {
                // this code appears to have been copied from equipmentmanager (now in mecanimModelController) so the below line is false ?
                // disabled message because some equipment (like quivers) does not have held attachment points intentionally because it should stay in the same place in combat
                //Debug.Log(baseCharacter.gameObject + ".CharacterEquipmentManager.HoldObject(): Unable to get attachment point");
                // testing because this is ability manager, if no attachment point node or target bone was found, set rotation to match parent
                go.transform.rotation = searchObject.transform.rotation;
            }
        }

        public void SpawnActionObjectsInternal(AnimatedAction animatedAction) {
            //Debug.Log($"{unitController.gameObject.name}.UnitActionManager.SpawnActionObjectsInternal({animatedAction.ResourceName})");

            SpawnActionObjects(animatedAction.ActionProperties.HoldableObjectList);

            unitController.UnitEventController.NotifyOnSpawnActionObjects(animatedAction);

        }

        public void SpawnActionObjects(List<AbilityAttachmentNode> abilityAttachmentNodes) {
            //Debug.Log($"{unitController.gameObject.name}.UnitActionManager.SpawnActionObjects(" + abilityAttachmentNodes.Count + ")");

            // ensure that any current ability objects are cleared before spawning new ones
            DespawnActionObjects();

            Dictionary<AbilityAttachmentNode, GameObject> holdableObjects = new Dictionary<AbilityAttachmentNode, GameObject>();
            foreach (AbilityAttachmentNode abilityAttachmentNode in abilityAttachmentNodes) {
                if (abilityAttachmentNode != null) {
                    if (abilityAttachmentNode.HoldableObject != null && abilityAttachmentNode.HoldableObject.Prefab != null) {
                        //Debug.Log("UnitActionManager.SpawnActionObjects(): attachment node has a physical prefab");
                        // attach a mesh to a bone for weapons

                        AttachmentPointNode attachmentPointNode = GetHeldAttachmentPointNode(abilityAttachmentNode);
                        if (attachmentPointNode != null) {
                            Transform targetBone = unitController.transform.FindChildByRecursive(attachmentPointNode.TargetBone);

                            if (targetBone != null) {
                                //Debug.Log("UnitActionManager.SpawnActionObjects(): targetbone (" + attachmentPointNode.TargetBone + ") is " + targetBone.gameObject.name);
                                GameObject newEquipmentPrefab = objectPooler.GetPooledObject(abilityAttachmentNode.HoldableObject.Prefab, targetBone);
                                //holdableObjects.Add(attachmentNode.MyHoldableObject, newEquipmentPrefab);
                                holdableObjects.Add(abilityAttachmentNode, newEquipmentPrefab);
                                //currentEquipmentPhysicalObjects[equipmentSlotProfile] = newEquipmentPrefab;

                                newEquipmentPrefab.transform.localScale = abilityAttachmentNode.HoldableObject.Scale;
                                HoldObject(newEquipmentPrefab, abilityAttachmentNode, unitController.gameObject);
                            } else {
                                //Debug.Log($"CharacterAbilityManager.SpawnAbilityObjects(). We could not find the target bone {attachmentPointNode.TargetBone} while spawning {abilityAttachmentNode.HoldableObject.ResourceName}");
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
            //Debug.Log($"{unitController.gameObject.name}.UnitActionManager.DespawnActionObjects()");

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

            unitController.UnitEventController.NotifyOnDespawnActionObjects();
        }

        public AnimationProps GetUnitAnimationProps() {
            //Debug.Log($"{gameObject.name}.GetDefaultAttackAnimations()");
            if (unitController.BaseCharacter.UnitProfile?.UnitPrefabProps?.AnimationProps != null) {
                return unitController.BaseCharacter.UnitProfile.UnitPrefabProps.AnimationProps;
            }
            if (systemConfigurationManager.DefaultAnimationProfile != null) {
                return systemConfigurationManager.DefaultAnimationProfile.AnimationProps;
            }
            return null;
        }

        /*
        public List<AnimationClip> GetUnitCastAnimations() {
            //Debug.Log($"{gameObject.name}.GetDefaultAttackAnimations()");
            if (unitController?.UnitAnimator?.CurrentAnimations != null) {
                return unitController.UnitAnimator.CurrentAnimations.CastClips;
            }
            return new List<AnimationClip>();
        }
        */

        public void PerformActionAnimation(AnimatedAction animatedAction) {
            //Debug.Log($"{unitController.gameObject.name}.PerformActionAnimation({animatedAction.ResourceName})");

            if (animatedAction.ActionProperties.AnimationClip == null) {
                return;
            }

            unitController.UnitAnimator.PerformAnimatedAction(animatedAction);
        }

        /*
        public void CleanupCoroutines() {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilitymanager.CleanupCoroutines()");

            TryToStopAction();
        }
        */

        public void HandleDie(CharacterStats _characterStats) {
            //Debug.Log(baseCharacter.gameObject.name + ".HandleDie()");

            TryToStopAction();
        }


        /// <summary>
        /// Cast a spell with a cast timer
        /// </summary>
        /// <param name="animatedAction"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public IEnumerator PerformActionCast(AnimatedAction animatedAction, Interactable target) {
            //Debug.Log($"{unitController.gameObject.name}.UnitActionManager.PerformActionCast({animatedAction.ResourceName})");
            
            float startTime = Time.time;
            isPerformingAction = true;
            AnimatedActionProperties animatedActionProperties = animatedAction.ActionProperties;
            //Debug.Log(baseCharacter.gameObject.name + "CharacterAbilitymanager.PerformAbilityCast(" + ability.DisplayName + ", " + (target == null ? "null" : target.name) + ") Enter Ienumerator with tag: " + startTime);

            PerformActionAnimation(animatedAction);

            float currentCastPercent = 0f;
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilitymanager.PerformAbilityCast() currentCastPercent: " + currentCastPercent + "; MyAbilityCastingTime: " + ability.MyAbilityCastingTime);

            if (animatedActionProperties.HoldableObjectList.Count != 0) {
                //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.PerformAbilityCast(" + ability.DisplayName + "): spawning ability objects");
                SpawnActionObjectsInternal(animatedAction);
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
        public void BeginAction(string actionName) {
            //Debug.Log($"{unitController.gameObject.name}.UnitActionManager.BeginAction({actionName})");

            AnimatedAction animatedAction = systemDataFactory.GetResource<AnimatedAction>(actionName);
            if (animatedAction != null) {
                //return BeginAction(animatedAction);
                BeginAction(animatedAction);
            }
        }

        /// <summary>
        /// Call an action directly, checking if the action is known
        /// </summary>
        /// <returns></returns>
        public void BeginAction(AnimatedAction animatedAction, bool playerInitiated = false) {
            //Debug.Log($"{unitController.gameObject.name}.UnitActionManager.BeginAction({(animatedAction == null ? "null" : animatedAction.DisplayName)}, {playerInitiated})");
            
            unitController.UnitEventController.NotifyOnBeginAction(animatedAction.ResourceName, playerInitiated);
            
            if (systemGameManager.GameMode == GameMode.Local || networkManagerServer.ServerModeActive == true || levelManager.IsCutscene()) {
                BeginActionInternal(animatedAction, playerInitiated);
            }
        }

        /// <summary>
        /// The entrypoint to Casting a spell.  handles all logic such as instant/timed cast, current cast in progress, enough mana, target being alive etc
        /// </summary>
        /// <param name="animatedAction"></param>
        public void BeginActionInternal(AnimatedAction animatedAction, bool playerInitiated) {
            //Debug.Log($"{unitController.gameObject.name}.UnitActionManager.BeginAction({(animatedAction == null ? "null" : animatedAction.ResourceName)})");

            BeginActionCommon(animatedAction, unitController.Target, playerInitiated);
        }

        /*
        public void BeginAction(AnimatedAction animatedAction, Interactable target) {
            //Debug.Log($"{gameObject.name}.CharacterAbilityManager.BeginAbility(" + ability.DisplayName + ")");
            BeginActionCommon(animatedAction, target);
        }
        */

        protected void BeginActionCommon(AnimatedAction animatedAction, Interactable target, bool playerInitiated = false) {
            //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.BeginAbilityCommon(" + (ability == null ? "null" : ability.DisplayName) + ", " + (target == null ? "null" : target.gameObject.name) + ")");

            if (unitController != null) {
                if (unitController.ControlLocked == true) {
                    return;
                }
            }

            if (!CanPerformAction(animatedAction.ActionProperties, playerInitiated)) {
                if (playerInitiated) {
                    //Debug.Log(baseCharacter.gameObject.name + ".CharacterAbilityManager.BeginAbilityCommon(" + ability.DisplayName + ", " + (target != null ? target.name : "null") + ") cannot cast");
                }
                return;
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
            TryToStopAction();

            if (isPerformingAction == false) {
                //Debug.Log("Performing Ability " + ability.DisplayName + " at a cost of " + ability.MyAbilityManaCost.ToString() + ": ABOUT TO START COROUTINE");

                // currentAction must be set before starting the coroutine because for animated events, the cast time is zero and the variable will be cleared in the coroutine
                //currentAction = animatedAction;
                currentActionCoroutine = unitController.StartCoroutine(PerformActionCast(animatedAction, target));
            } else {
                // return false so that items in the inventory don't get used if this came from a castable item
                return;
            }

            return;
        }

        // this only checks if the ability is able to be cast based on character state.  It does not check validity of target or ability specific requirements
        public bool CanPerformAction(AnimatedActionProperties animatedAction, bool playerInitiated = false) {
            //Debug.Log($"{gameObject.name}.CharacterAbilityManager.CanCastAbility(" + ability.DisplayName + ")");

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
            if (!NotMounted()) {
                return false;
            }

            // actions cannot be performed while any cast is in progress
            if (!NotCasting()) {
                return false;
            }

            // actions cannot be performed while dead
            if (!NotDead()) {
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

        private bool NotMounted() {
            if (unitController.IsMounted == true) {
                return false;
            }
            return true;
        }

        private bool NotCasting() {
            if (unitController.CharacterAbilityManager.PerformingAnyAbility() == true) {
                return false;
            }
            return true;
        }

        private bool NotDead() {
            if (!unitController.CharacterStats.IsAlive) {
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

            //if (!animatedAction.UseableWithoutLearning && !AbilityList.ContainsKey(animatedAction.ResourceName)) {
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
            if (unitController.ApparentVelocity <= 0.1f) {
                return;
            }

            TryToStopAction();
        }

        public void TryToStopAction() {
            if (isPerformingAction == false) {
                return;
            }

            StopAction();
        }

        private void StopAction() {
            //Debug.Log($"{unitController.gameObject.name}.UnitActionManager.StopAction()");

            if (isPerformingAction == false) {
                return;
            }

            if (currentActionCoroutine != null) {
                unitController.StopCoroutine(currentActionCoroutine);
                currentActionCoroutine = null;
            }

            EndActionCleanup();
            DespawnActionObjects();

            unitController.UnitAnimator?.ClearAction();
            isPerformingAction = false;
        }

        public void HandleCharacterUnitDespawn() {
            TryToStopAction();
        }


    }

}