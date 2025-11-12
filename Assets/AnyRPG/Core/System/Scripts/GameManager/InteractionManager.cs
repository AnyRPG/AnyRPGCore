using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

namespace AnyRPG {
    public class InteractionManager : ConfiguredClass {

        public event System.Action<Interactable> OnSetInteractable = delegate { };

        private Interactable currentInteractable = null;
        private InteractableOptionComponent currentInteractableOptionComponent = null;
        private InteractableOptionManager interactableOptionManager = null;

        private PlayerManager playerManager = null;
        private UIManager uIManager = null;

        public override void SetGameManagerReferences() {
            //Debug.Log($"InteractionManager.SetGameManagerReferences()");

            base.SetGameManagerReferences();
            playerManager = systemGameManager.PlayerManager;
            uIManager = systemGameManager.UIManager;
        }

        public bool Interact(UnitController sourceUnitController, Interactable target) {
            //Debug.Log($"InteractionManager.Interact({sourceUnitController.gameObject.name}, {target.gameObject.name})");

            // get reference to name now since interactable could change scene and then target reference is lost
            //string targetDisplayName = target.DisplayName;

            if (InteractWithInteractable(playerManager.UnitController, target)) {
                //Debug.Log($"{gameObject.name}.PlayerController.InteractionSucceeded(): Interaction Succeeded.  Setting interactable to null");
                return true;
            }
            return false;
        }

        public bool InteractWithInteractable(UnitController sourceUnitController, Interactable targetInteractable) {
            //Debug.Log($"InteractionManager.InteractWithInteractable({sourceUnitController.gameObject.name}, {targetInteractable.gameObject.name})");

            // perform range check
            bool passedRangeCheck = false;

            Collider[] colliders = new Collider[100];
            int playerMask = 1 << LayerMask.NameToLayer("Player");
            int characterMask = 1 << LayerMask.NameToLayer("CharacterUnit");
            int interactableMask = 1 << LayerMask.NameToLayer("Interactable");
            int triggerMask = 1 << LayerMask.NameToLayer("Triggers");
            int validMask = (playerMask | characterMask | interactableMask | triggerMask);
            Vector3 bottomPoint = new Vector3(sourceUnitController.Collider.bounds.center.x,
            sourceUnitController.Collider.bounds.center.y - sourceUnitController.Collider.bounds.extents.y,
                sourceUnitController.Collider.bounds.center.z);
            Vector3 topPoint = new Vector3(sourceUnitController.Collider.bounds.center.x,
            sourceUnitController.Collider.bounds.center.y + sourceUnitController.Collider.bounds.extents.y,
                sourceUnitController.Collider.bounds.center.z);
            sourceUnitController.PhysicsScene.OverlapCapsule(bottomPoint, topPoint, targetInteractable.InteractionMaxRange, colliders, validMask);
            foreach (Collider collider in colliders) {
                if (collider == null) {
                    continue;
                }
                if (collider.gameObject == targetInteractable.gameObject) {
                    passedRangeCheck = true;
                    break;
                }
            }

            //float factionValue = targetInteractable.PerformFactionCheck(sourceUnitController);

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
                        OpenInteractionWindow(targetInteractable);
                    } else {
                        InteractWithOptionClient(sourceUnitController, targetInteractable, inRangeInteractables[firstInteractable], firstInteractable, 0);
                        
                    }
                } else {
                    OpenInteractionWindow(targetInteractable);
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
                InteractWithOptionInternal(unitController, triggerInteractable, validInteractables[firstInteractable], firstInteractable, 0);
            }
        }

        public void InteractWithOptionClient(UnitController sourceUnitController, Interactable targetInteractable, InteractableOptionComponent interactableOptionComponent, int componentIndex, int choiceIndex) {
            if (systemGameManager.GameMode == GameMode.Local) {
                InteractWithOptionInternal(sourceUnitController, targetInteractable, interactableOptionComponent, componentIndex, choiceIndex);
            } else {
                networkManagerClient.InteractWithOption(sourceUnitController, targetInteractable, componentIndex, choiceIndex);
            }
        }

        public void InteractWithOptionServer(UnitController sourceUnitController, Interactable targetInteractable, int componentIndex, int choiceIndex) {
            //Debug.Log($"InteractionManager.InteractWithOptionServer({sourceUnitController.gameObject.name}, {targetInteractable.gameObject.name}, {componentIndex}, {choiceIndex})");

            Dictionary<int, InteractableOptionComponent> interactionOptions = targetInteractable.GetCurrentInteractables(sourceUnitController);
            if (interactionOptions.ContainsKey(componentIndex)) {
                InteractWithOptionInternal(sourceUnitController, targetInteractable, interactionOptions[componentIndex], componentIndex, choiceIndex);
            }
        }

        public void InteractWithOptionInternal(UnitController sourceUnitController, Interactable targetInteractable, InteractableOptionComponent interactableOptionComponent, int componentIndex, int choiceIndex) {
            //Debug.Log($"InteractionManager.InteractWithOptionInternal({sourceUnitController.gameObject.name}, {targetInteractable.gameObject.name}, {componentIndex}, {choiceIndex})");

            interactableOptionComponent.Interact(sourceUnitController, componentIndex, choiceIndex);
        }

        public void OpenInteractionWindow(Interactable targetInteractable) {
            //Debug.Log($"InteractionManager.OpenInteractionWindow");

            BeginInteraction(targetInteractable);
            uIManager.craftingWindow.CloseWindow();
            uIManager.interactionWindow.OpenWindow();
        }


        public void BeginInteraction(Interactable interactable) {
            SetInteractable(interactable);
            interactable.ProcessStartInteract();
        }

        public void EndInteraction() {
            currentInteractable.ProcessStopInteract();
            SetInteractable(null);
        }

        public void SetInteractable(Interactable interactable) {
            currentInteractable = interactable;
            OnSetInteractable(currentInteractable);
        }

        public void BeginInteractionWithOption(InteractableOptionComponent interactableOptionComponent, InteractableOptionManager interactableOptionManager) {
            this.interactableOptionManager = interactableOptionManager;
            currentInteractableOptionComponent = interactableOptionComponent;
            SetInteractable(interactableOptionComponent.Interactable);
        }

    }
}