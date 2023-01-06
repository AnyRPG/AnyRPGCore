using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class DialogController : ConfiguredClass {

        // references
        private Interactable interactable;

        private int dialogIndex = 0;

        private float maxDialogTime = 300f;

        private Coroutine dialogCoroutine = null;

        // game manager references
        private PlayerManager playerManager = null;
        private LogManager logManager = null;

        public int DialogIndex { get => dialogIndex; }

        public DialogController(Interactable interactable, SystemGameManager systemGameManager) {
            this.interactable = interactable;
            Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            playerManager = systemGameManager.PlayerManager;
            logManager = systemGameManager.LogManager;
        }

        public void Cleanup() {
            CleanupDialog();
        }

        private void CleanupDialog() {
            if (dialogCoroutine != null) {
                interactable.StopCoroutine(dialogCoroutine);
            }
            dialogCoroutine = null;

            // testing - is this needed ?  it should be cleaned up by namePlate removal anyway
            /*
            if (interactable != null && interactable.NamePlateController.NamePlate != null) {
                interactable.NamePlateController.NamePlate.HideSpeechBubble();
            }
            */
        }

        public void BeginDialog(string dialogName, DialogComponent caller = null) {
            //Debug.Log(interactable.gameObject.name + ".DialogController.BeginDialog(" + dialogName + ")");
            Dialog tmpDialog = systemDataFactory.GetResource<Dialog>(dialogName);
            if (tmpDialog != null) {
                BeginDialog(tmpDialog, caller);
            }
        }

        public void BeginDialog(Dialog dialog, DialogComponent caller = null) {
            //Debug.Log(interactable.gameObject.name + ".DialogController.BeginDialog()");
            if (dialog != null && dialogCoroutine == null) {
                dialogCoroutine = interactable.StartCoroutine(PlayDialog(dialog, caller));
            }
        }

        public IEnumerator PlayDialog(Dialog dialog, DialogComponent caller = null) {
            //Debug.Log(interactable.gameObject.name + ".DialogController.PlayDialog(" + dialog.DisplayName + ")");

            interactable.ProcessBeginDialog();
            float elapsedTime = 0f;
            dialogIndex = 0;
            DialogNode currentdialogNode = null;

            // this needs to be reset to allow for repeatable dialogs to replay
            dialog.ResetStatus();

            while (dialog.TurnedIn == false) {
                foreach (DialogNode dialogNode in dialog.DialogNodes) {
                    if (dialogNode.StartTime <= elapsedTime && dialogNode.Shown == false) {
                        currentdialogNode = dialogNode;
                        interactable.ProcessDialogTextUpdate(dialogNode.Description);
                        if (interactable != null && dialogNode.AudioClip != null) {
                            interactable.UnitComponentController.PlayVoiceSound(dialogNode.AudioClip);
                        }
                        bool writeMessage = true;
                        if (playerManager != null && playerManager.ActiveUnitController != null) {
                            if (Vector3.Distance(interactable.transform.position, playerManager.ActiveUnitController.transform.position) > systemConfigurationManager.MaxChatTextDistance) {
                                writeMessage = false;
                            }
                        }
                        if (writeMessage && logManager != null) {
                            logManager.WriteChatMessage(dialogNode.Description);
                        }

                        dialogNode.Shown = true;
                        dialogIndex++;
                    }
                }
                if (dialogIndex >= dialog.DialogNodes.Count) {
                    dialog.TurnedIn = true;
                    if (caller != null) {
                        caller.NotifyOnConfirmAction();
                    }
                }
                elapsedTime += Time.deltaTime;

                // circuit breaker
                if (elapsedTime >= maxDialogTime) {
                    break;
                }
                yield return null;
                dialogCoroutine = null;
            }

            if (currentdialogNode != null) {
                yield return new WaitForSeconds(currentdialogNode.ShowTime);
            }
            interactable.ProcessEndDialog();
        }


    }

}