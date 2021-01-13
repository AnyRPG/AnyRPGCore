using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class DialogController {

        // references
        private Interactable interactable;

        private int dialogIndex = 0;

        private float maxDialogTime = 300f;

        private Coroutine dialogCoroutine = null;

        public int DialogIndex { get => dialogIndex; }

        public DialogController(Interactable interactable) {
            this.interactable = interactable;
        }

        public void Cleanup() {
            CleanupDialog();
        }

        private void CleanupDialog() {
            //nameplate
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
            Dialog tmpDialog = SystemDialogManager.MyInstance.GetResource(dialogName);
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
                foreach (DialogNode dialogNode in dialog.MyDialogNodes) {
                    if (dialogNode.MyStartTime <= elapsedTime && dialogNode.Shown == false) {
                        currentdialogNode = dialogNode;
                        interactable.ProcessDialogTextUpdate(dialogNode.MyDescription);
                        if (interactable != null && dialog.MyAudioProfile != null && dialog.MyAudioProfile.AudioClips != null && dialog.MyAudioProfile.AudioClips.Count > dialogIndex) {
                            interactable.UnitComponentController.PlayVoice(dialog.MyAudioProfile.AudioClips[dialogIndex]);
                        }
                        bool writeMessage = true;
                        if (PlayerManager.MyInstance != null && PlayerManager.MyInstance.ActiveUnitController != null) {
                            if (Vector3.Distance(interactable.transform.position, PlayerManager.MyInstance.ActiveUnitController.transform.position) > SystemConfigurationManager.MyInstance.MaxChatTextDistance) {
                                writeMessage = false;
                            }
                        }
                        if (writeMessage && CombatLogUI.MyInstance != null) {
                            CombatLogUI.MyInstance.WriteChatMessage(dialogNode.MyDescription);
                        }

                        dialogNode.Shown = true;
                        dialogIndex++;
                    }
                }
                if (dialogIndex >= dialog.MyDialogNodes.Count) {
                    dialog.TurnedIn = true;
                    if (caller != null) {
                        caller.HandleConfirmAction();
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
                yield return new WaitForSeconds(currentdialogNode.MyShowTime);
            }
            interactable.ProcessEndDialog();
        }


    }

}