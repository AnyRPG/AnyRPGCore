using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AnyRPG {
    public class CraftingNodeComponent : InteractableOptionComponent {

        // game manager references
        private CraftingManager craftingManager = null;

        public CraftingNodeProps Props { get => interactableOptionProps as CraftingNodeProps; }

        public CraftingNodeComponent(Interactable interactable, CraftingNodeProps interactableOptionProps, SystemGameManager systemGameManager) : base(interactable, interactableOptionProps, systemGameManager) {
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            craftingManager = systemGameManager.CraftingManager;
        }

        public override bool PrerequisitesMet(UnitController sourceUnitController) {
                if (sourceUnitController.CharacterAbilityManager.HasAbility(Props.Ability) == false) {
                    return false;
                }
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

            systemEventManager.OnAbilityListChanged -= HandleAbilityListChange;
        }

        public static List<CraftingNodeComponent> GetCraftingNodeComponents(Interactable searchInteractable) {
            if (searchInteractable == null) {
                return new List<CraftingNodeComponent>();
            }
            return searchInteractable.GetInteractableOptionList(typeof(CraftingNodeComponent)).Values.Cast<CraftingNodeComponent>().ToList();
        }

        public override string GetInteractionButtonText(UnitController sourceUnitController, int componentIndex = 0, int choiceIndex = 0) {
            return (Props.Ability != null ? Props.Ability.DisplayName : base.GetInteractionButtonText(sourceUnitController, componentIndex, choiceIndex));
        }

        public void HandleAbilityListChange(UnitController sourceUnitController, AbilityProperties baseAbility) {
            //Debug.Log($"{gameObject.name}.GatheringNode.HandleAbilityListChange(" + baseAbility.DisplayName + ")");
            HandlePrerequisiteUpdates(sourceUnitController);
        }

        public override int GetCurrentOptionCount(UnitController sourceUnitController) {
            //Debug.Log($"{gameObject.name}.GatheringNode.GetCurrentOptionCount()");
            return ((sourceUnitController.CharacterAbilityManager.HasAbility(Props.Ability) == true) ? 1 : 0);
        }

        public override bool ProcessInteract(UnitController sourceUnitController, int componentIndex, int choiceIndex = 0) {
            //Debug.Log($"{interactable.gameObject.name}.CraftingNode.ProcessInteract({sourceUnitController.gameObject.name}, {componentIndex}, {choiceIndex})");

            base.ProcessInteract(sourceUnitController, componentIndex, choiceIndex);

            if (Props == null || Props.Ability == null) {
                Debug.LogWarning("Props is null");
            }
            sourceUnitController.CharacterCraftingManager.SetCraftAbility(Props.Ability);
            return true;
        }

        public override void ClientInteraction(UnitController sourceUnitController, int componentIndex, int choiceIndex) {
            //Debug.Log($"{interactable.gameObject.name}.CraftingNodeComponent.ClientInteraction({sourceUnitController.gameObject.name}, {componentIndex}, {choiceIndex})");

            base.ClientInteraction(sourceUnitController, componentIndex, choiceIndex);
            uIManager.craftingWindow.OpenWindow();
        }

        public override void StopInteract() {
            base.StopInteract();

            uIManager.craftingWindow.CloseWindow();
        }

    }

}