using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class SkillTrainerComponent : InteractableOptionComponent {

        // game manager references
        private SkillTrainerManager skillTrainerManager = null;

        public SkillTrainerProps Props { get => interactableOptionProps as SkillTrainerProps; }

        public SkillTrainerComponent(Interactable interactable, SkillTrainerProps interactableOptionProps, SystemGameManager systemGameManager) : base(interactable, interactableOptionProps, systemGameManager) {
            if (interactableOptionProps.GetInteractionPanelTitle() == string.Empty) {
                interactableOptionProps.InteractionPanelTitle = "Train Me";
            }
            systemEventManager.OnSkillListChanged += HandleSkillListChanged;
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            skillTrainerManager = systemGameManager.SkillTrainerManager;
        }

        public override bool Interact(CharacterUnit source, int optionIndex = 0) {
            //Debug.Log($"{gameObject.name}.SkillTrainer.Interact(" + source + ")");
            base.Interact(source, optionIndex);
            if (!uIManager.skillTrainerWindow.IsOpen) {
                skillTrainerManager.SetSkillTrainer(this);
                uIManager.skillTrainerWindow.OpenWindow();
                return true;
            }
            return false;
        }

        public override void StopInteract() {
            //Debug.Log($"{gameObject.name}.SkillTrainer.StopInteract()");
            base.StopInteract();
            uIManager.skillTrainerWindow.CloseWindow();
        }

        public override void ProcessCleanupEventSubscriptions() {
            //Debug.Log($"{gameObject.name}.SkillTrainer.CleanupEventSubscriptions()");
            base.ProcessCleanupEventSubscriptions();
            if (systemEventManager != null) {
                systemEventManager.OnSkillListChanged -= HandleSkillListChanged;
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
            //Debug.Log($"{gameObject.name}.SkillTrainerInteractable.GetCurrentOptionCount()");
            if (interactable.CombatOnly) {
                return 0;
            }
            int optionCount = 0;
            foreach (Skill skill in Props.Skills) {
                if (!playerManager.UnitController.CharacterSkillManager.HasSkill(skill)) {
                    optionCount++;
                }
            }
            //Debug.Log($"{gameObject.name}.SkillTrainerInteractable.GetCurrentOptionCount(); return: " + optionCount);
            // testing - having the actual skill count causes multiple interaction window items
            // return 1 for anything other than no skills
            return (optionCount == 0 ? 0 : 1);
        }

        public override bool CanInteract(bool processRangeCheck = false, bool passedRangeCheck = false, float factionValue = 0f, bool processNonCombatCheck = true) {
            //Debug.Log($"{gameObject.name}.SkillTrainer.CanInteract()");
            bool returnValue = ((GetCurrentOptionCount() > 0 && base.CanInteract(processRangeCheck, passedRangeCheck, factionValue, processNonCombatCheck)) ? true : false);
            //Debug.Log($"{gameObject.name}.SkillTrainer.CanInteract(): return: " + returnValue);
            return returnValue;
        }

        public override bool CanShowMiniMapIcon() {
            float relationValue = interactable.PerformFactionCheck(playerManager.UnitController);
            return CanInteract(false, false, relationValue);
        }

        //public override bool PlayInteractionSound() {
        //    return true;
        //}


    }

}