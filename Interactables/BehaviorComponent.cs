using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class BehaviorComponent : InteractableOptionComponent {

        public override event Action<InteractableOptionComponent> MiniMapStatusUpdateHandler = delegate { };

        public BehaviorProps Props { get => interactableOptionProps as BehaviorProps; }

        private UnitController unitController = null;

        private List<BehaviorProfile> behaviorList = new List<BehaviorProfile>();

        public BehaviorComponent(Interactable interactable, BehaviorProps interactableOptionProps) : base(interactable, interactableOptionProps) {
            if ((interactable as UnitController) is UnitController) {
                unitController = (interactable as UnitController);
            }
            InitBehaviors();
        }

        public static BehaviorComponent GetBehaviorComponent(Interactable searchInteractable) {
            return searchInteractable.GetFirstInteractableOption(typeof(BehaviorComponent)) as BehaviorComponent;
        }

        public override bool Interact(CharacterUnit source) {
            //Debug.Log(gameObject.name + ".BehaviorInteractable.Interact()");
            List<BehaviorProfile> currentList = GetCurrentOptionList();
            if (currentList.Count == 0) {
                return false;
            } else if (currentList.Count == 1) {
                if (unitController != null) {
                    unitController.BehaviorController.TryPlayBehavior(currentList[0], this);
                }
                base.Interact(source);
                interactable.CloseInteractionWindow();
            } else {
                interactable.OpenInteractionWindow();
            }
            return true;
        }

        public void InitBehaviors() {
            if (Props.BehaviorNames != null) {
                foreach (string behaviorName in Props.BehaviorNames) {
                    BehaviorProfile tmpBehaviorProfile = null;
                    if (Props.UseBehaviorCopy == true) {
                        tmpBehaviorProfile = SystemBehaviorProfileManager.MyInstance.GetNewResource(behaviorName);
                    } else {
                        tmpBehaviorProfile = SystemBehaviorProfileManager.MyInstance.GetResource(behaviorName);
                    }
                    if (tmpBehaviorProfile != null) {
                        unitController.BehaviorController.AddToBehaviorList(tmpBehaviorProfile);
                    }
                }
            }

        }

        public List<BehaviorProfile> GetCurrentOptionList() {
            //Debug.Log(unitController.gameObject.name +  ".BehaviorComponent.GetCurrentOptionList()");
            List<BehaviorProfile> currentList = new List<BehaviorProfile>();
            foreach (BehaviorProfile behaviorProfile in unitController.BehaviorController.BehaviorList) {
                //Debug.Log(unitController.gameObject.name + ".BehaviorComponent.GetCurrentOptionList() processing behavior: " + behaviorProfile.DisplayName);
                if (behaviorProfile.MyPrerequisitesMet == true
                    && (behaviorProfile.Completed == false || behaviorProfile.Repeatable == true)
                    && behaviorProfile.AllowManualStart == true) {
                    //Debug.Log(unitController.gameObject.name +  ".BehaviorComponent.GetCurrentOptionList() adding behaviorProfile " + behaviorProfile.DisplayName + "; id: " + behaviorProfile.GetInstanceID());
                    currentList.Add(behaviorProfile);
                }
            }
            //Debug.Log("BehaviorInteractable.GetValidOptionList(): List Size: " + validList.Count);
            return currentList;
        }

        public override bool CanInteract() {
            //Debug.Log(gameObject.name + ".BehaviorInteractable.CanInteract()");
            if (!base.CanInteract()) {
                return false;
            }
            if (GetCurrentOptionCount() == 0 || unitController.BehaviorController.SuppressNameplateImage == true) {
                return false;
            }
            return true;

        }

        public override void StopInteract() {
            base.StopInteract();
            PopupWindowManager.MyInstance.dialogWindow.CloseWindow();
        }

        public override bool HasMiniMapText() {
            return true;
        }

        public override bool SetMiniMapText(TextMeshProUGUI text) {
            if (!base.SetMiniMapText(text)) {
                text.text = "";
                text.color = new Color32(0, 0, 0, 0);
                return false;
            }
            text.text = "o";
            text.fontSize = 50;
            text.color = Color.white;
            return true;
        }

        public override int GetCurrentOptionCount() {
            //Debug.Log(unitController.gameObject.name + ".BehaviorComponent.GetCurrentOptionCount()");

            if (unitController != null && unitController.BehaviorController.BehaviorPlaying == false) {
                //return GetCurrentOptionList().Count;
                int count = 0;
                foreach (BehaviorProfile behaviorProfile in GetCurrentOptionList()) {
                    //Debug.Log(unitController.gameObject.name + ".BehaviorInteractable.GetCurrentOptionCount(): found behaviorProfile: " + behaviorProfile);
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
            base.HandlePrerequisiteUpdates();
            MiniMapStatusUpdateHandler(this);
        }

        public override void HandlePrerequisiteUpdates() {
            //Debug.Log(interactable.gameObject.name + ".BehaviorComponent.HandlePrerequisiteUpdates()");
            base.HandlePrerequisiteUpdates();
            MiniMapStatusUpdateHandler(this);
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