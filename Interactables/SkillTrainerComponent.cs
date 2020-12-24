using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class SkillTrainerComponent : InteractableOptionComponent {

        public override event System.Action<InteractableOptionComponent> MiniMapStatusUpdateHandler = delegate { };

        public SkillTrainerProps Props { get => interactableOptionProps as SkillTrainerProps; }

        public SkillTrainerComponent(Interactable interactable, SkillTrainerProps interactableOptionProps) : base(interactable, interactableOptionProps) {
            if (interactableOptionProps.InteractionPanelTitle == string.Empty) {
                //Debug.Log("SkillTrainer.Start(): interactionPanelTitle is empty: setting to default (Train Me)!!!");
                interactableOptionProps.InteractionPanelTitle = "Train Me";
            }
        }

        public void InitWindow(ICloseableWindowContents skillTrainerUI) {
            //Debug.Log(gameObject.name + ".SkillTrainer.InitWindow()");
            (skillTrainerUI as SkillTrainerUI).ShowSkills(this);
        }

        public override bool Interact(CharacterUnit source) {
            //Debug.Log(gameObject.name + ".SkillTrainer.Interact(" + source + ")");
            base.Interact(source);
            if (!PopupWindowManager.MyInstance.skillTrainerWindow.IsOpen) {
                //Debug.Log(source + " interacting with " + gameObject.name);
                //vendorWindow.MyVendorUI.CreatePages(items);
                PopupWindowManager.MyInstance.skillTrainerWindow.MyCloseableWindowContents.OnOpenWindow += InitWindow;
                PopupWindowManager.MyInstance.skillTrainerWindow.MyCloseableWindowContents.OnCloseWindow += CleanupEventSubscriptions;
                SystemEventManager.MyInstance.OnSkillListChanged += HandleSkillListChanged;
                PopupWindowManager.MyInstance.skillTrainerWindow.OpenWindow();
                return true;
            }
            return false;
        }

        public override void StopInteract() {
            //Debug.Log(gameObject.name + ".SkillTrainer.StopInteract()");
            base.StopInteract();
            //vendorUI.ClearPages();
            PopupWindowManager.MyInstance.skillTrainerWindow.CloseWindow();
        }

        public void CleanupEventSubscriptions(ICloseableWindowContents windowContents) {
            //Debug.Log(gameObject.name + ".SkillTrainer.CleanupEventSubscriptions(windowContents)");
            CleanupWindowEventSubscriptions();
        }

        public void CleanupWindowEventSubscriptions() {
            if (PopupWindowManager.MyInstance != null && PopupWindowManager.MyInstance.skillTrainerWindow != null && PopupWindowManager.MyInstance.skillTrainerWindow.MyCloseableWindowContents != null) {
                PopupWindowManager.MyInstance.skillTrainerWindow.MyCloseableWindowContents.OnOpenWindow -= InitWindow;
                PopupWindowManager.MyInstance.skillTrainerWindow.MyCloseableWindowContents.OnCloseWindow -= CleanupEventSubscriptions;
                SystemEventManager.MyInstance.OnSkillListChanged -= HandleSkillListChanged;
            }
        }

        public override void CleanupEventSubscriptions() {
            //Debug.Log(gameObject.name + ".SkillTrainer.CleanupEventSubscriptions()");
            base.CleanupEventSubscriptions();
            CleanupWindowEventSubscriptions();
        }

        public void HandleSkillListChanged(Skill skill) {
            // this is a special case.  since skill is not a prerequisites, we need to subscribe directly to the event to get notified things have changed
            if (Props.Skills.Contains(skill)) {
                HandlePrerequisiteUpdates();
            }
        }

        public override int GetValidOptionCount() {
            return GetCurrentOptionCount();
        }

        public override int GetCurrentOptionCount() {
            //Debug.Log(gameObject.name + ".SkillTrainerInteractable.GetCurrentOptionCount()");
            int optionCount = 0;
            foreach (Skill skill in Props.Skills) {
                if (!PlayerManager.MyInstance.MyCharacter.CharacterSkillManager.HasSkill(skill)) {
                    optionCount++;
                }
            }
            //Debug.Log(gameObject.name + ".SkillTrainerInteractable.GetCurrentOptionCount(); return: " + optionCount);
            return optionCount;
        }

        public override bool CanInteract() {
            //Debug.Log(gameObject.name + ".SkillTrainer.CanInteract()");
            bool returnValue = ((GetCurrentOptionCount() > 0 && MyPrerequisitesMet) ? true : false);
            //Debug.Log(gameObject.name + ".SkillTrainer.CanInteract(): return: " + returnValue);
            return returnValue;
        }

        public override bool CanShowMiniMapIcon() {
            return CanInteract();
        }

        public override void HandlePrerequisiteUpdates() {
            //Debug.Log(gameObject.name + ".SkillTrainer.HandlePrerequisiteUpdates()");
            base.HandlePrerequisiteUpdates();
            MiniMapStatusUpdateHandler(this);
        }

        public override void HandlePlayerUnitSpawn() {
            base.HandlePlayerUnitSpawn();
            MiniMapStatusUpdateHandler(this);
        }

    }

}