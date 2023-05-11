using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class PatrolComponent : InteractableOptionComponent {

        public PatrolProps Props { get => interactableOptionProps as PatrolProps; }

        private UnitController unitController = null;

        //private List<BehaviorProfile> behaviorList = new List<BehaviorProfile>();

        public PatrolComponent(Interactable interactable, PatrolProps interactableOptionProps, SystemGameManager systemGameManager) : base(interactable, interactableOptionProps, systemGameManager) {
            if ((interactable as UnitController) is UnitController) {
                unitController = (interactable as UnitController);
            }
            //InitBehaviors();
        }

        /*
        public static PatrolComponent GetPatrolComponent(Interactable searchInteractable) {
            return searchInteractable.GetFirstInteractableOption(typeof(PatrolComponent)) as PatrolComponent;
        }
        */

        public override bool Interact(CharacterUnit source, int optionIndex = 0) {
            //Debug.Log($"{gameObject.name}.BehaviorInteractable.Interact()");
            //List<BehaviorProfile> currentList = GetCurrentOptionList();
            /*
            if (currentList.Count == 0) {
                return false;
                //} else if (currentList.Count == 1) {
            } else {*/
            if (unitController != null) {
                //unitController.PatrolController.BeginPatrolByIndex(currentList[optionIndex]);
                unitController.PatrolController.BeginPatrol(Props.PatrolProperties);
            }
            base.Interact(source, optionIndex);
            interactable.CloseInteractionWindow();
            //}/* else {

            interactable.OpenInteractionWindow();
            //}*/
            return true;
        }

        /*
    public void InitBehaviors() {
        if (Props.BehaviorNames != null) {
            foreach (string behaviorName in Props.BehaviorNames) {
                BehaviorProfile tmpBehaviorProfile = null;
                tmpBehaviorProfile = systemDataFactory.GetResource<BehaviorProfile>(behaviorName);
                if (tmpBehaviorProfile != null) {
                    unitController.BehaviorController.AddToBehaviorList(tmpBehaviorProfile);
                }
            }
        }


    }
        */

        public List<PatrolProps> GetCurrentOptionList() {
            //Debug.Log(unitController.gameObject.name +  ".BehaviorComponent.GetCurrentOptionList()");
            List<PatrolProps> currentList = new List<PatrolProps>();
            if (interactable.CombatOnly == false) {
                //foreach (BehaviorProfile behaviorProfile in unitController.BehaviorController.BehaviorList.Keys) {
                //Debug.Log($"{unitController.gameObject.name}.BehaviorComponent.GetCurrentOptionList() processing behavior: " + behaviorProfile.DisplayName);
                if (PrerequisitesMet == true
                    && Props.PatrolProperties.AutoStart == false) {
                    //Debug.Log(unitController.gameObject.name +  ".BehaviorComponent.GetCurrentOptionList() adding behaviorProfile " + behaviorProfile.DisplayName + "; id: " + behaviorProfile.GetInstanceID());
                    currentList.Add(Props);
                }
                //}
            }
            //Debug.Log("BehaviorInteractable.GetValidOptionList(): List Size: " + validList.Count);
            return currentList;
        }

        public override bool CanInteract(bool processRangeCheck = false, bool passedRangeCheck = false, float factionValue = 0f, bool processNonCombatCheck = true) {
            //Debug.Log($"{gameObject.name}.BehaviorInteractable.CanInteract()");
            if (!base.CanInteract(processRangeCheck, passedRangeCheck, factionValue, processNonCombatCheck)) {
                return false;
            }
            if (GetCurrentOptionCount() == 0 || unitController.BehaviorController.SuppressNameplateImage == true) {
                return false;
            }
            return true;

        }

        public override void StopInteract() {
            base.StopInteract();
            uIManager.dialogWindow.CloseWindow();
        }


        public override int GetCurrentOptionCount() {
            //Debug.Log($"{unitController.gameObject.name}.BehaviorComponent.GetCurrentOptionCount()");
            if (interactable.CombatOnly) {
                return 0;
            }

            if (unitController != null && unitController.BehaviorController.BehaviorPlaying == false) {
                //return GetCurrentOptionList().Count;
                /*
                int count = 0;
                foreach (BehaviorProfile behaviorProfile in GetCurrentOptionList()) {
                    //Debug.Log($"{unitController.gameObject.name}.BehaviorInteractable.GetCurrentOptionCount(): found behaviorProfile: " + behaviorProfile);
                    if (behaviorProfile.AllowManualStart == true) {
                        count++;
                    }
                }
                return count;
                */
                return GetCurrentOptionList().Count;
            } else {
                return 0;
            }
        }

        public void ProcessBehaviorBeginEnd() {
            //Debug.Log(interactable.gameObject.name + ".BehaviorComponent.ProcessBehaviorBeginEnd()");
            base.HandlePrerequisiteUpdates();
            CallMiniMapStatusUpdateHandler();
        }

        // testing - since behavior component requires behavior controller, let it handle player unit spawn calls for proper ordering
        /*
        public override void HandlePlayerUnitSpawn() {
            Debug.Log(interactable.gameObject.name + ".BehaviorComponent.HandlePlayerUnitSpawn()");
            base.HandlePlayerUnitSpawn();
            MiniMapStatusUpdateHandler(this);
        }
        */

        public override bool CanShowMiniMapIcon() {
            if (unitController.BehaviorController.SuppressNameplateImage == true) {
                return false;
            }
            return base.CanShowMiniMapIcon();
        }

    }

}