using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class DialogComponent : InteractableOptionComponent {

        public override event Action<InteractableOptionComponent> MiniMapStatusUpdateHandler = delegate { };

        public DialogProps Props { get => interactableOptionProps as DialogProps; }

        public DialogComponent(Interactable interactable, DialogProps interactableOptionProps) : base(interactable, interactableOptionProps) {
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
            if (PopupWindowManager.MyInstance != null && PopupWindowManager.MyInstance.dialogWindow != null && PopupWindowManager.MyInstance.dialogWindow.CloseableWindowContents != null) {
                (PopupWindowManager.MyInstance.dialogWindow.CloseableWindowContents as DialogPanelController).OnConfirmAction -= HandleConfirmAction;
                (PopupWindowManager.MyInstance.dialogWindow.CloseableWindowContents as DialogPanelController).OnCloseWindow -= CleanupConfirm;
            }
        }

        public void CleanupConfirm(ICloseableWindowContents contents) {
            CleanupConfirm();
        }

        public List<Dialog> GetCurrentOptionList() {
            //Debug.Log(gameObject.name + ".DialogInteractable.GetCurrentOptionList()");
            List<Dialog> currentList = new List<Dialog>();
            foreach (Dialog dialog in Props.DialogList) {
                //Debug.Log(interactable.gameObject.name + ".DialogInteractable.GetCurrentOptionList() : found dialog: " + dialog.DisplayName);
                if (dialog.MyPrerequisitesMet == true && (dialog.TurnedIn == false || dialog.Repeatable == true)) {
                    currentList.Add(dialog);
                }
            }
            //Debug.Log(gameObject.name + ".DialogInteractable.GetCurrentOptionList(): List Size: " + currentList.Count);
            return currentList;
        }

        public override bool Interact(CharacterUnit source) {
            //Debug.Log(gameObject.name + ".DialogInteractable.Interact()");
            List<Dialog> currentList = GetCurrentOptionList();
            if (currentList.Count == 0) {
                return false;
            } else if (currentList.Count == 1) {
                if (currentList[0].MyAutomatic) {
                    interactable.DialogController.PlayDialog(currentList[0]);
                } else {
                    (PopupWindowManager.MyInstance.dialogWindow.CloseableWindowContents as DialogPanelController).Setup(currentList[0], this.interactable);
                    (PopupWindowManager.MyInstance.dialogWindow.CloseableWindowContents as DialogPanelController).OnConfirmAction += HandleConfirmAction;
                    (PopupWindowManager.MyInstance.dialogWindow.CloseableWindowContents as DialogPanelController).OnCloseWindow += CleanupConfirm;
                }
            } else {
                interactable.OpenInteractionWindow();
            }
            base.Interact(source);
            return true;
        }

        public override bool CanInteract(bool processRangeCheck = false, bool passedRangeCheck = false) {
            //Debug.Log(gameObject.name + ".DialogInteractable.CanInteract()");
            if (!base.CanInteract(processRangeCheck, passedRangeCheck)) {
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
            PopupWindowManager.MyInstance.dialogWindow.CloseWindow();
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
            text.fontSize = 50;
            text.color = Color.white;
            return true;
        }

        public override int GetCurrentOptionCount() {
            //Debug.Log(gameObject.name + ".DialogInteractable.GetCurrentOptionCount(): " + GetCurrentOptionList().Count);
            return GetCurrentOptionList().Count;
        }

        public override void CallMiniMapStatusUpdateHandler() {
            MiniMapStatusUpdateHandler(this);
        }

        public override void HandlePlayerUnitSpawn() {
            base.HandlePlayerUnitSpawn();
            UpdateDialogStatuses();
            MiniMapStatusUpdateHandler(this);
        }

        public void UpdateDialogStatuses() {
            foreach (Dialog dialog in Props.DialogList) {
                dialog.UpdatePrerequisites(false);
            }

            bool preRequisitesUpdated = false;
            foreach (Dialog dialog in Props.DialogList) {
                if (dialog.MyPrerequisitesMet == true) {
                    preRequisitesUpdated = true;
                }
            }

            if (preRequisitesUpdated) {
                HandlePrerequisiteUpdates();
            }

        }

    }

}