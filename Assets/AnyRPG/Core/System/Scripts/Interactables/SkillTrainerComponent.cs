using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class SkillTrainerComponent : InteractableOptionComponent {

        // game manager references
        private SkillTrainerManagerClient skillTrainerManager = null;

        public SkillTrainerProps Props { get => interactableOptionProps as SkillTrainerProps; }

        public SkillTrainerComponent(Interactable interactable, SkillTrainerProps interactableOptionProps, SystemGameManager systemGameManager) : base(interactable, interactableOptionProps, systemGameManager) {
            if (interactionPanelTitle == string.Empty) {
                interactionPanelTitle = "Train Me";
            }
            if (systemGameManager.GameMode == GameMode.Local || systemGameManager.NetworkManagerServer.ServerModeActive == false) {
                systemEventManager.OnLearnSkill += HandleSkillListChanged;
                systemEventManager.OnUnLearnSkill += HandleSkillListChanged;
            }
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            skillTrainerManager = systemGameManager.SkillTrainerManagerClient;
        }

        public override bool ProcessInteract(UnitController sourceUnitController, int componentIndex, int choiceIndex = 0) {
            //Debug.Log($"{gameObject.name}.SkillTrainer.Interact(" + source + ")");
            base.ProcessInteract(sourceUnitController, componentIndex, choiceIndex);

            //interactionManager.InteractWithSkillTrainerComponent(sourceUnitController, this, optionIndex);

            return true;
        }

        public override void ClientInteraction(UnitController sourceUnitController, int componentIndex, int choiceIndex) {
            if (!uIManager.skillTrainerWindow.IsOpen) {
                skillTrainerManager.SetSkillTrainer(this, componentIndex, choiceIndex);
                uIManager.skillTrainerWindow.OpenWindow();
            }
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
                systemEventManager.OnLearnSkill -= HandleSkillListChanged;
                systemEventManager.OnUnLearnSkill -= HandleSkillListChanged;
            }
        }

        public void HandleSkillListChanged(UnitController sourceUnitController, Skill skill) {
            // this is a special case.  since skill is not a prerequisites, we need to subscribe directly to the event to get notified things have changed
            if (Props.Skills.Contains(skill)) {
                HandlePrerequisiteUpdates(sourceUnitController);
            }
        }

        public override int GetValidOptionCount(UnitController sourceUnitController) {
            if (base.GetValidOptionCount(sourceUnitController) == 0) {
                return 0;
            }
            return GetCurrentOptionCount(sourceUnitController);
        }

        public override int GetCurrentOptionCount(UnitController sourceUnitController) {
            //Debug.Log($"{gameObject.name}.SkillTrainerInteractable.GetCurrentOptionCount()");
            if (interactable.CombatOnly) {
                return 0;
            }
            int optionCount = 0;
            foreach (Skill skill in Props.Skills) {
                if (!sourceUnitController.CharacterSkillManager.HasSkill(skill)) {
                    optionCount++;
                }
            }
            //Debug.Log($"{gameObject.name}.SkillTrainerInteractable.GetCurrentOptionCount(); return: " + optionCount);
            // testing - having the actual skill count causes multiple interaction window items
            // return 1 for anything other than no skills
            return (optionCount == 0 ? 0 : 1);
        }

        public override bool CanInteract(UnitController sourceUnitController, bool processRangeCheck, bool passedRangeCheck, bool processNonCombatCheck, bool viaSwitch = false) {
            //Debug.Log($"{gameObject.name}.SkillTrainer.CanInteract()");
            bool returnValue = ((GetCurrentOptionCount(sourceUnitController) > 0 && base.CanInteract(sourceUnitController, processRangeCheck, passedRangeCheck, processNonCombatCheck)) ? true : false);
            //Debug.Log($"{gameObject.name}.SkillTrainer.CanInteract(): return: " + returnValue);
            return returnValue;
        }

        public override bool CanShowMiniMapIcon(UnitController sourceUnitController) {
            float relationValue = interactable.PerformFactionCheck(sourceUnitController);
            return CanInteract(sourceUnitController, false, false, true);
        }

        public Dictionary<int, Skill> GetAvailableSkillList(UnitController sourceUnitController) {
            Dictionary<int, Skill> returnList = new Dictionary<int, Skill>();

            int counter = 0;
            foreach (Skill skill in Props.Skills) {
                if (!sourceUnitController.CharacterSkillManager.HasSkill(skill)) {
                    returnList.Add(counter, skill);
                }
                counter++;
            }

            return returnList;
        }

        public void LearnSkill(UnitController sourceUnitController, int skillId) {
            Dictionary<int, Skill> skillList = GetAvailableSkillList(sourceUnitController);
            if (!skillList.ContainsKey(skillId)) {
                //Debug.Log($"{gameObject.name}.SkillTrainerComponent.LearnSkill(): player does not have skill {skillId}");
                return;
            }
            sourceUnitController.CharacterSkillManager.LearnSkill(skillList[skillId]);
            NotifyOnConfirmAction(sourceUnitController);
        }

        //public override bool PlayInteractionSound() {
        //    return true;
        //}


    }

}