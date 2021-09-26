using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class DialogComponent : InteractableOptionComponent {

        public DialogProps Props { get => interactableOptionProps as DialogProps; }

        public DialogComponent(Interactable interactable, DialogProps interactableOptionProps, SystemGameManager systemGameManager) : base(interactable, interactableOptionProps, systemGameManager) {
            //AddUnitProfileSettings();
        }

        /*
        protected override void AddUnitProfileSettings() {
            base.AddUnitProfileSettings();
            if (unitProfile != null) {
                interactableOptionProps = unitProfile.DialogProps;
            }

            // testing - add handle prerequisiteupdates here
            // this is necessary because addUnitProfileSettings is called late in startup order
            HandlePrerequisiteUpdates();
        }
        */

        public override void Cleanup() {
            base.Cleanup();
            CleanupConfirm();
            CleanupPrerequisiteOwner();
        }

        public void CleanupPrerequisiteOwner() {
            Props.CleanupPrerequisiteOwner(this);
        }

        public override void HandleConfirmAction() {
            //Debug.Log(gameObject.name + ".NameChangeInteractable.HandleConfirmAction()");
            base.HandleConfirmAction();
            // just to be safe
            CleanupConfirm();

            // since the dialog completion status is itself a form of prerequisite, we should call the prerequisite update here
            HandlePrerequisiteUpdates();
        }

        public void CleanupConfirm() {
            if (uIManager.dialogWindow.CloseableWindowContents != null) {
                (uIManager.dialogWindow.CloseableWindowContents as DialogPanelController).OnConfirmAction -= HandleConfirmAction;
                (uIManager.dialogWindow.CloseableWindowContents as DialogPanelController).OnCloseWindow -= CleanupConfirm;
            }
        }

        public void CleanupConfirm(ICloseableWindowContents contents) {
            CleanupConfirm();
        }

        public List<Dialog> GetCurrentOptionList() {
            //Debug.Log(gameObject.name + ".DialogInteractable.GetCurrentOptionList()");
            List<Dialog> currentList = new List<Dialog>();
            if (interactable.CombatOnly == false) {
                foreach (Dialog dialog in Props.DialogList) {
                    //Debug.Log(interactable.gameObject.name + ".DialogInteractable.GetCurrentOptionList() : found dialog: " + dialog.DisplayName);
                    if (dialog.PrerequisitesMet == true && (dialog.TurnedIn == false || dialog.Repeatable == true)) {
                        currentList.Add(dialog);
                    }
                }
            }
            //Debug.Log(gameObject.name + ".DialogInteractable.GetCurrentOptionList(): List Size: " + currentList.Count);
            return currentList;
        }

        public override bool Interact(CharacterUnit source, int optionIndex = 0) {
            //Debug.Log(interactable.gameObject.name + ".DialogInteractable.Interact()");
            List<Dialog> currentList = GetCurrentOptionList();
            if (currentList.Count == 0) {
                return false;
            } else /*if (currentList.Count == 1)*/ {
                if (currentList[optionIndex].Automatic) {
                    interactable.DialogController.BeginDialog(currentList[optionIndex]);
                } else {
                    (uIManager.dialogWindow.CloseableWindowContents as DialogPanelController).Setup(currentList[optionIndex], this.interactable);
                    (uIManager.dialogWindow.CloseableWindowContents as DialogPanelController).OnConfirmAction += HandleConfirmAction;
                    (uIManager.dialogWindow.CloseableWindowContents as DialogPanelController).OnCloseWindow += CleanupConfirm;
                }
            }/* else {
                interactable.OpenInteractionWindow();
            }*/
            base.Interact(source, optionIndex);
            return true;
        }

        public override bool CanInteract(bool processRangeCheck = false, bool passedRangeCheck = false, float factionValue = 0f, bool processNonCombatCheck = true) {
            //Debug.Log(gameObject.name + ".DialogInteractable.CanInteract()");
            if (!base.CanInteract(processRangeCheck, passedRangeCheck, factionValue, processNonCombatCheck)) {
                return false;
            }
            if (GetCurrentOptionList().Count == 0) {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Pick an item up off the ground and put it in the inventory
        /// </summary>

        public override void StopInteract() {
            base.StopInteract();
            uIManager.dialogWindow.CloseWindow();
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
            text.color = Color.white;
            return true;
        }

        public override int GetCurrentOptionCount() {
            //Debug.Log(gameObject.name + ".DialogInteractable.GetCurrentOptionCount(): " + GetCurrentOptionList().Count);
            return GetCurrentOptionList().Count;
        }


        public override void HandlePlayerUnitSpawn() {
            UpdateDialogStatuses();
            base.HandlePlayerUnitSpawn();
        }

        public void UpdateDialogStatuses() {
            foreach (Dialog dialog in Props.DialogList) {
                dialog.UpdatePrerequisites(false);
            }

            bool preRequisitesUpdated = false;
            foreach (Dialog dialog in Props.DialogList) {
                if (dialog.PrerequisitesMet == true) {
                    preRequisitesUpdated = true;
                }
            }

            if (preRequisitesUpdated) {
                HandlePrerequisiteUpdates();
            }

        }

    }

}