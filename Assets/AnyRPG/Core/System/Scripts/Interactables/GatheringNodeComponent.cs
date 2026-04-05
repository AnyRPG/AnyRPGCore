using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class GatheringNodeComponent : LootableNodeComponent {

        //private bool available = true;

        public GatheringNodeProps GatheringNodeProps { get => interactableOptionProps as GatheringNodeProps; }

        public GatheringNodeComponent(Interactable interactable, GatheringNodeProps interactableOptionProps, SystemGameManager systemGameManager) : base(interactable, interactableOptionProps, systemGameManager) {
        }

        public override bool PrerequisitesMet(UnitController sourceUnitController) {
                return base.PrerequisitesMet(sourceUnitController);
        }

        public override void ProcessCreateEventSubscriptions() {
            //Debug.Log("GatheringNode.CreateEventSubscriptions()");
            base.ProcessCreateEventSubscriptions();

            systemEventManager.OnAbilityListChanged += HandleAbilityListChange;
        }

        public override void ProcessCleanupEventSubscriptions() {
            //Debug.Log("GatheringNode.CleanupEventSubscriptions()");
            base.ProcessCleanupEventSubscriptions();

            if (systemEventManager != null) {
                systemEventManager.OnAbilityListChanged -= HandleAbilityListChange;
            }
        }

        public void HandleAbilityListChange(UnitController sourceUnitController, AbilityProperties baseAbility) {
            //Debug.Log($"{gameObject.name}.GatheringNode.HandleAbilityListChange(" + baseAbility.DisplayName + ")");
            HandlePrerequisiteUpdates(sourceUnitController);
        }

        public static GatheringNodeComponent GetGatheringNodeComponent(Interactable searchInteractable) {
            if (searchInteractable == null) {
                return null;
            }
            return searchInteractable.GetFirstInteractableOption(typeof(GatheringNodeComponent)) as GatheringNodeComponent;
        }

        public override string GetSummary(UnitController sourceUnitController) {
            string returnValue = base.GetSummary(sourceUnitController);
            string colorstring  = "#ffff00ff";
            if (GatheringNodeProps.Skill != null && GatheringNodeProps.RequiredSkillLevel > sourceUnitController.CharacterSkillManager.GetSkillLevel(GatheringNodeProps.Skill)) {
                colorstring = "#ff0000";
                returnValue += $"\n<color={colorstring}>Requires Skill Level {GatheringNodeProps.RequiredSkillLevel} skill</color>";
            }
            return returnValue;
        }

        public override string GetInteractionButtonText(UnitController sourceUnitController, int componentIndex, int choiceIndex) {
            return (GatheringNodeProps.BaseAbility != null ? GatheringNodeProps.BaseAbility.DisplayName : base.GetInteractionButtonText(sourceUnitController, componentIndex, choiceIndex));
        }

        public override bool ProcessInteract(UnitController sourceUnitController, int componentIndex, int choiceIndex) {
            //Debug.Log($"{interactable.gameObject.name}.GatheringNode.ProcessInteract({sourceUnitController.gameObject.name}, {componentIndex}, {choiceIndex})");

            if (Props.LootTables == null) {
                Debug.LogWarning($"{interactable.gameObject.name}.GatheringNode.ProcessInteract({sourceUnitController.gameObject.name}, {componentIndex}, {choiceIndex}) loot table was null");
                return true;
            }
            // base.Interact() will drop loot automatically so we will intentionally not call it because the loot drop in this class is activated by the gatherability
            if (lootDropped == true) {
                // this call is safe, it will internally check if loot is already dropped and just pickup instead
                Gather(sourceUnitController, componentIndex);
            } else {
                // attempt physics sync on server in case character was moving
                if (networkManagerServer.ServerModeActive == true) {
                    //Debug.Log($"{interactable.gameObject.name}.GatheringNode.ProcessInteract() calling Physics.SyncTransforms()");
                    Physics.SyncTransforms();
                }
                sourceUnitController.CharacterAbilityManager.BeginAbility(GatheringNodeProps.BaseAbility.AbilityProperties, interactable);
            }
            return true;
        }

        public override void ProcessClientNotifications(UnitController sourceUnitController, int componentIndex, int choiceIndex) {
            // do not send to base class, we'll do this later
        }

        public override void ClientInteraction(UnitController sourceUnitController, int componentIndex, int choiceIndex) {
            base.ClientInteraction(sourceUnitController, componentIndex, choiceIndex);
        }

        public void Gather(UnitController sourceUnitController, int componentIndex = 0, int choiceIndex = 0) {
            //Debug.Log($"{interactable.gameObject.name}.GatheringNode.Gather({sourceUnitController.gameObject.name}, {componentIndex}, {choiceIndex})");

            base.ProcessInteract(sourceUnitController, componentIndex, choiceIndex);
            base.ProcessClientNotifications(sourceUnitController, componentIndex, choiceIndex);
        }

        public override void DropLoot(UnitController sourceUnitController) {
            base.DropLoot(sourceUnitController);
            if (lootDropped == true) {
                return;
            }
            if (GatheringNodeProps.Skill == null || GatheringNodeProps.Skill.UseSkillLevels == false) {
                return;
            }
            if (sourceUnitController.CharacterStats.Level > GatheringNodeProps.MaxCharacterExperienceLevel && GatheringNodeProps.MaxCharacterExperienceLevel > 0) {
                return;
            }
            if (sourceUnitController.CharacterSkillManager.GetSkillLevel(GatheringNodeProps.Skill) > GatheringNodeProps.MaxSkillExperienceLevel && GatheringNodeProps.MaxSkillExperienceLevel > 0) {
                return;
            }
            sourceUnitController.CharacterSkillManager.AddSkillLevel(GatheringNodeProps.Skill, 1);
        }

        public override int GetCurrentOptionCount(UnitController sourceUnitController) {
            //Debug.Log($"{gameObject.name}.GatheringNode.GetCurrentOptionCount()");

            if (Props.SpawnObject == null) {
                return 0;
            }
            if (Props.SpawnObject.activeInHierarchy == false) {
                return 0;
            }
            if (GatheringNodeProps.Skill != null && sourceUnitController.CharacterSkillManager.HasSkill(GatheringNodeProps.Skill) == false) {
                return 0;
            }
            if (sourceUnitController.CharacterAbilityManager.HasAbility(GatheringNodeProps.BaseAbility.AbilityProperties) == false) {
                return 0;
            }
            return 1;
        }

        /*

        public override bool CanInteract(CharacterUnit source) {
            bool returnValue = base.CanInteract(source);
            if (returnValue == false) {
                return false;
            }
            return (GetCurrentOptionCount() == 0 ? false : true);
        }
        */

    }

}