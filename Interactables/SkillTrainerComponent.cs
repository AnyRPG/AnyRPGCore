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
            SystemEventManager.MyInstance.OnSkillListChanged += HandleSkillListChanged;
        }

        public override bool Interact(CharacterUnit source) {
            //Debug.Log(gameObject.name + ".SkillTrainer.Interact(" + source + ")");
            base.Interact(source);
            if (!PopupWindowManager.MyInstance.skillTrainerWindow.IsOpen) {
                //Debug.Log(source + " interacting with " + gameObject.name);
                //vendorWindow.MyVendorUI.CreatePages(items);
                PopupWindowManager.MyInstance.skillTrainerWindow.OpenWindow();
                (PopupWindowManager.MyInstance.skillTrainerWindow.CloseableWindowContents as SkillTrainerUI).ShowSkills(this);
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

        public override void CleanupEventSubscriptions() {
            //Debug.Log(gameObject.name + ".SkillTrainer.CleanupEventSubscriptions()");
            base.CleanupEventSubscriptions();
            if (SystemEventManager.MyInstance != null) {
                SystemEventManager.MyInstance.OnSkillListChanged -= HandleSkillListChanged;
            }
        }

        public void HandleSkillListChanged(Skill skill) {
            // this is a special case.  since skill is not a prerequisites, we need to subscribe directly to the event to get notified things have changed
            if (Props.Skills.Contains(skill)) {
                HandlePrerequisiteUpdates();
            }
        }

        public override int GetValidOptionCount() {
            if (base.GetValidOptionCount() == 0) {
                return 0;
            }
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

        public override bool CanInteract(bool processRangeCheck = false, bool passedRangeCheck = false) {
            //Debug.Log(gameObject.name + ".SkillTrainer.CanInteract()");
            bool returnValue = ((GetCurrentOptionCount() > 0 && base.CanInteract(processRangeCheck, passedRangeCheck)) ? true : false);
            //Debug.Log(gameObject.name + ".SkillTrainer.CanInteract(): return: " + returnValue);
            return returnValue;
        }

        public override bool CanShowMiniMapIcon() {
            return CanInteract();
        }

        public override void CallMiniMapStatusUpdateHandler() {
            MiniMapStatusUpdateHandler(this);
        }

        public override void HandlePlayerUnitSpawn() {
            base.HandlePlayerUnitSpawn();
            MiniMapStatusUpdateHandler(this);
        }

    }

}