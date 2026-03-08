using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AnyRPG {
    public class InteractionManagerServer : ConfiguredClass {

        // game manager references
        private InteractionManagerClient interactionManagerClient = null;

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            interactionManagerClient = systemGameManager.InteractionManagerClient;
        }

        public bool InteractWithInteractable(UnitController sourceUnitController, Interactable targetInteractable) {
            //Debug.Log($"InteractionManager.InteractWithInteractable({sourceUnitController.gameObject.name}, {targetInteractable.gameObject.name})");

            // perform range check
            bool passedRangeCheck = false;

            passedRangeCheck = targetInteractable.IsInRange(sourceUnitController);

            // get a list of valid interactables to determine if there is an action we can treat as default
            Dictionary<int, InteractableOptionComponent> validInteractables = targetInteractable.GetCurrentInteractables(sourceUnitController);
            Dictionary<int, InteractableOptionComponent> inRangeInteractables = new Dictionary<int, InteractableOptionComponent>();
            foreach (KeyValuePair<int, InteractableOptionComponent> validInteractable in validInteractables) {
                //Debug.Log($"{gameObject.name}.Interactable.Interact(" + source.name + "): valid interactable name: " + validInteractable);
                if (validInteractable.Value.CanInteract(sourceUnitController, true, passedRangeCheck, true)) {
                    inRangeInteractables.Add(validInteractable.Key, validInteractable.Value);
                }
            }

            if (inRangeInteractables.Count > 0) {
                targetInteractable.InteractWithPlayer(sourceUnitController);
                if (targetInteractable.SuppressInteractionWindow == true || inRangeInteractables.Count == 1) {
                    int firstInteractable = inRangeInteractables.Take(1).Select(d => d.Key).First();
                    if (inRangeInteractables[firstInteractable].GetCurrentOptionCount(sourceUnitController) > 1) {
                        OpenInteractionWindow(sourceUnitController, targetInteractable);
                    } else {
                        InteractWithOption(sourceUnitController, targetInteractable, inRangeInteractables[firstInteractable], firstInteractable, 0);
                        
                    }
                } else {
                    OpenInteractionWindow(sourceUnitController, targetInteractable);
                }
                return true;
            }

            if (validInteractables.Count > 0 && inRangeInteractables.Count == 0) {
                if (passedRangeCheck == false) {
                    sourceUnitController.UnitEventController.NotifyOnWriteMessageFeedMessage($"{targetInteractable.DisplayName} is out of range");
                }
            }
            return false;
        }

        public void InteractWithTrigger(UnitController unitController, Interactable triggerInteractable) {
            //Debug.Log($"InteractionManager.InteractionWithTrigger({unitController.gameObject.name}, {triggerInteractable.gameObject.name})");

            // no range check for triggers since the unit walked into it so we know its in range
            Dictionary<int, InteractableOptionComponent> validInteractables = triggerInteractable.GetCurrentInteractables(unitController);
            if (validInteractables.Count == 1) {
                int firstInteractable = validInteractables.Take(1).Select(d => d.Key).First();
                InteractWithOption(unitController, triggerInteractable, validInteractables[firstInteractable], firstInteractable, 0);
            }
        }

        public void InteractWithOption(UnitController sourceUnitController, Interactable targetInteractable, int componentIndex, int choiceIndex) {
            //Debug.Log($"InteractionManager.InteractWithOptionServer({sourceUnitController.gameObject.name}, {targetInteractable.gameObject.name}, {componentIndex}, {choiceIndex})");

            Dictionary<int, InteractableOptionComponent> interactionOptions = targetInteractable.GetCurrentInteractables(sourceUnitController);
            if (interactionOptions.ContainsKey(componentIndex)) {
                InteractWithOption(sourceUnitController, targetInteractable, interactionOptions[componentIndex], componentIndex, choiceIndex);
            }
        }

        public void InteractWithOption(UnitController sourceUnitController, Interactable targetInteractable, InteractableOptionComponent interactableOptionComponent, int componentIndex, int choiceIndex) {
            Debug.Log($"InteractionManager.InteractWithOptionInternal({sourceUnitController.gameObject.name}, {targetInteractable.gameObject.name}, {componentIndex}, {choiceIndex})");
            
            sourceUnitController.UnitMotor.StickToGround();
            sourceUnitController.ResetApparentVelocity();

            interactableOptionComponent.Interact(sourceUnitController, componentIndex, choiceIndex);
        }

        public void OpenInteractionWindow(UnitController sourceUnitController, Interactable targetInteractable) {
            //Debug.Log($"InteractionManager.OpenInteractionWindow");

            if (systemGameManager.GameMode == GameMode.Local) {
                interactionManagerClient.OpenInteractionWindow(targetInteractable);
            } else {
                networkManagerServer.AdvertiseOpenInteractionWindow(sourceUnitController, targetInteractable);
            }
        }


    }
}