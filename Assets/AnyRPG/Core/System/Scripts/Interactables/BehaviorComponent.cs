using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class BehaviorComponent : InteractableOptionComponent {

        public BehaviorProps Props { get => interactableOptionProps as BehaviorProps; }

        private UnitController unitController = null;

        //private List<BehaviorProfile> behaviorList = new List<BehaviorProfile>();

        public BehaviorComponent(Interactable interactable, BehaviorProps interactableOptionProps, SystemGameManager systemGameManager) : base(interactable, interactableOptionProps, systemGameManager) {
            if ((interactable as UnitController) is UnitController) {
                unitController = (interactable as UnitController);
            }
            InitBehaviors();
        }

        public static BehaviorComponent GetBehaviorComponent(Interactable searchInteractable) {
            return searchInteractable.GetFirstInteractableOption(typeof(BehaviorComponent)) as BehaviorComponent;
        }

        public override string GetInteractionButtonText(UnitController sourceUnitController, int componentIndex, int choiceIndex) {
            // FIX ME - THIS PULLS FROM THE Props, but all others come from a dictionary that includes local behaviours.  The sizes may not match
            return (Props.BehaviorNames.Count > choiceIndex ? Props.BehaviorNames[choiceIndex] : base.GetInteractionButtonText(sourceUnitController, componentIndex, choiceIndex));
        }

        public override string GetOptionChoiceName(UnitController sourceUnitController, int choiceIndex) {
            List<BehaviorProfile> currentList = GetCurrentOptionList(sourceUnitController);
            if (currentList.Count > choiceIndex) {
                return currentList[choiceIndex].DisplayName;
            }
            return string.Empty;
        }

        public override bool ProcessInteract(UnitController sourceUnitController, int componentIndex, int choiceIndex = 0) {
            //Debug.Log($"{unitController.gameObject.name}.BehaviorComponent.Interact({sourceUnitController.gameObject.name}, {componentIndex}, {choiceIndex})");

            List<BehaviorProfile> currentList = GetCurrentOptionList(sourceUnitController);
            if (currentList.Count == 0) {
                return false;
            } else {
                if (unitController != null) {
                    unitController.BehaviorController.TryPlayBehavior(currentList[choiceIndex], this, sourceUnitController);
                }
                base.ProcessInteract(sourceUnitController, componentIndex, choiceIndex);
            }
            return true;
        }

        public override void ClientInteraction(UnitController sourceUnitController, int componentIndex, int choiceIndex) {
            //Debug.Log($"{unitController.gameObject.name}.BehaviorComponent.ClientInteraction({sourceUnitController.gameObject.name}, {componentIndex}, {choiceIndex})");

            base.ClientInteraction(sourceUnitController, componentIndex, choiceIndex);
            interactable.CloseInteractionWindow();

        }

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

        public List<BehaviorProfile> GetCurrentOptionList(UnitController sourceUnitController) {
            //Debug.Log(unitController.gameObject.name +  ".BehaviorComponent.GetCurrentOptionList()");
            List<BehaviorProfile> currentList = new List<BehaviorProfile>();
            if (interactable.CombatOnly == false) {
                foreach (BehaviorProfile behaviorProfile in unitController.BehaviorController.BehaviorList.Keys) {
                    //Debug.Log($"{unitController.gameObject.name}.BehaviorComponent.GetCurrentOptionList() processing behavior: " + behaviorProfile.DisplayName);
                    if (behaviorProfile.PrerequisitesMet(sourceUnitController) == true
                        && (behaviorProfile.Completed(sourceUnitController) == false || behaviorProfile.Repeatable == true)
                        && behaviorProfile.AllowManualStart == true) {
                        //Debug.Log(unitController.gameObject.name +  ".BehaviorComponent.GetCurrentOptionList() adding behaviorProfile " + behaviorProfile.DisplayName + "; id: " + behaviorProfile.GetInstanceID());
                        currentList.Add(behaviorProfile);
                    }
                }
            }
            //Debug.Log("BehaviorInteractable.GetValidOptionList(): List Size: " + validList.Count);
            return currentList;
        }

        public override bool CanInteract(UnitController sourceUnitController, bool processRangeCheck, bool passedRangeCheck, bool processNonCombatCheck, bool viaSwitch = false) {
            //Debug.Log($"{gameObject.name}.BehaviorInteractable.CanInteract()");
            if (!base.CanInteract(sourceUnitController, processRangeCheck, passedRangeCheck, processNonCombatCheck)) {
                return false;
            }
            if (GetCurrentOptionCount(sourceUnitController) == 0 || unitController.BehaviorController.SuppressNameplateImage == true) {
                return false;
            }
            return true;

        }

        public override void StopInteract() {
            base.StopInteract();
            uIManager.dialogWindow.CloseWindow();
        }

        public override int GetCurrentOptionCount(UnitController sourceUnitController) {
            //Debug.Log($"{unitController.gameObject.name}.BehaviorComponent.GetCurrentOptionCount()");
            if (interactable.CombatOnly) {
                return 0;
            }

            if (unitController != null && unitController.BehaviorController.BehaviorPlaying == false) {
                //return GetCurrentOptionList().Count;
                int count = 0;
                foreach (BehaviorProfile behaviorProfile in GetCurrentOptionList(sourceUnitController)) {
                    //Debug.Log($"{unitController.gameObject.name}.BehaviorInteractable.GetCurrentOptionCount(): found behaviorProfile: " + behaviorProfile);
                    if (behaviorProfile.AllowManualStart == true) {
                        count++;
                    }
                }
                return count;
            } else {
                return 0;
            }
        }

        public void ProcessBehaviorBeginEnd() {
            //Debug.Log(interactable.gameObject.name + ".BehaviorComponent.ProcessBehaviorBeginEnd()");
            base.HandleOptionStateChange();
            CallMiniMapStatusUpdateHandler();
        }

        // testing - since behavior component requires behavior controller, let it handle player unit spawn calls for proper ordering
        /*
        public override void HandlePlayerUnitSpawn() {
            //Debug.Log(interactable.gameObject.name + ".BehaviorComponent.HandlePlayerUnitSpawn()");
            base.HandlePlayerUnitSpawn();
            MiniMapStatusUpdateHandler(this);
        }
        */

        public override bool CanShowMiniMapIcon(UnitController sourceUnitController) {
            if (unitController.BehaviorController.SuppressNameplateImage == true) {
                return false;
            }
            return base.CanShowMiniMapIcon(sourceUnitController);
        }

    }

}