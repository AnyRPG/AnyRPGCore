using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class DialogComponent : InteractableOptionComponent {

        // game manager references
        private DialogManagerClient dialogManager = null;

        public DialogProps Props { get => interactableOptionProps as DialogProps; }

        public DialogComponent(Interactable interactable, DialogProps interactableOptionProps, SystemGameManager systemGameManager) : base(interactable, interactableOptionProps, systemGameManager) {
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            dialogManager = systemGameManager.DialogManagerClient;
        }

        public override string GetInteractionButtonText(UnitController sourceUnitController, int componentIndex = 0, int choiceIndex = 0) {

            List<Dialog> currentList = GetCurrentOptionList(sourceUnitController);
            if (currentList.Count > choiceIndex) {
                return currentList[choiceIndex].DisplayName;
            }
            return base.GetInteractionButtonText(sourceUnitController, componentIndex, choiceIndex);
        }


        public override void Cleanup() {
            base.Cleanup();
            CleanupPrerequisiteOwner();
        }

        public void CleanupPrerequisiteOwner() {
            Props.CleanupPrerequisiteOwner(this);
        }

        public override void NotifyOnConfirmAction(UnitController sourceUnitController) {
            //Debug.Log($"{gameObject.name}.NameChangeInteractable.HandleConfirmAction()");
            base.NotifyOnConfirmAction(sourceUnitController);

            // since the dialog completion status is itself a form of prerequisite, we should call the prerequisite update here
            HandleOptionStateChange();
        }

        public List<Dialog> GetCurrentOptionList(UnitController sourceUnitController) {
            //Debug.Log($"{gameObject.name}.DialogInteractable.GetCurrentOptionList()");
            List<Dialog> currentList = new List<Dialog>();
            if (interactable.CombatOnly == false) {
                foreach (Dialog dialog in Props.DialogList) {
                    //Debug.Log(interactable.gameObject.name + ".DialogInteractable.GetCurrentOptionList() : found dialog: " + dialog.DisplayName);
                    if (dialog.PrerequisitesMet(sourceUnitController) == true && (dialog.TurnedIn(sourceUnitController) == false || dialog.Repeatable == true)) {
                        currentList.Add(dialog);
                    }
                }
            }
            //Debug.Log($"{gameObject.name}.DialogInteractable.GetCurrentOptionList(): List Size: " + currentList.Count);
            return currentList;
        }

        public override bool ProcessInteract(UnitController sourceUnitController, int componentIndex, int choiceIndex = 0) {
            //Debug.Log($"{interactable.gameObject.name}.DialogComponent.Interact({sourceUnitController.gameObject.name}, {componentIndex}, {choiceIndex})");

            base.ProcessInteract(sourceUnitController, componentIndex, choiceIndex);
            List<Dialog> currentList = GetCurrentOptionList(sourceUnitController);
            if (currentList.Count == 0) {
                return false;
            } else {
                if (currentList[choiceIndex].Automatic) {
                    interactable.DialogController.BeginDialog(sourceUnitController, currentList[choiceIndex]);
                }
            }

            return true;
        }

        public override void ClientInteraction(UnitController sourceUnitController, int componentIndex, int choiceIndex) {
            base.ClientInteraction(sourceUnitController, componentIndex, choiceIndex);
            
            List<Dialog> currentList = GetCurrentOptionList(sourceUnitController);
            if (currentList.Count == 0) {
                return;
            } else {
                if (currentList[choiceIndex].Automatic == false) {
                    dialogManager.SetDialog(currentList[choiceIndex], this.interactable, this, componentIndex, choiceIndex);
                    uIManager.dialogWindow.OpenWindow();
                }
            }

        }

        public override bool CanInteract(UnitController sourceUnitController, bool processRangeCheck, bool passedRangeCheck, bool processNonCombatCheck, bool viaSwitch = false) {
            //Debug.Log($"{gameObject.name}.DialogInteractable.CanInteract()");
            if (!base.CanInteract(sourceUnitController, processRangeCheck, passedRangeCheck, processNonCombatCheck)) {
                return false;
            }
            if (GetCurrentOptionList(sourceUnitController).Count == 0) {
                return false;
            }
            return true;
        }

        public override void StopInteract() {
            base.StopInteract();
            uIManager.dialogWindow.CloseWindow();
        }

        public override int GetCurrentOptionCount(UnitController sourceUnitController) {
            //Debug.Log($"{gameObject.name}.DialogInteractable.GetCurrentOptionCount(): " + GetCurrentOptionList().Count);
            return GetCurrentOptionList(sourceUnitController).Count;
        }

        public override void HandlePlayerUnitSpawn(UnitController sourceUnitController) {
            UpdateDialogStatuses(sourceUnitController);
            base.HandlePlayerUnitSpawn(sourceUnitController);
        }

        public void UpdateDialogStatuses(UnitController sourceUnitController) {
            foreach (Dialog dialog in Props.DialogList) {
                dialog.UpdatePrerequisites(sourceUnitController, false);
            }

            bool preRequisitesUpdated = false;
            foreach (Dialog dialog in Props.DialogList) {
                if (dialog.PrerequisitesMet(sourceUnitController) == true) {
                    preRequisitesUpdated = true;
                }
            }

            if (preRequisitesUpdated) {
                HandlePrerequisiteUpdates(sourceUnitController);
            }

        }

        public void TurnInDialog(UnitController sourceUnitController, Dialog dialog) {
            //Debug.Log($"{interactable.gameObject.name}.DialogInteractable.TurnInDialog({dialog.ResourceName})");

            sourceUnitController.CharacterDialogManager.TurnInDialog(dialog);
            NotifyOnConfirmAction(sourceUnitController);
        }
    }

}