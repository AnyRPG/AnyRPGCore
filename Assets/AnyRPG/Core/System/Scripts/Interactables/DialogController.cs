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

        private int shownNodeCount = 0;

        private float maxDialogTime = 300f;
        private float chatDisplayTime = 5f;

        private Coroutine chatCoroutine = null;
        private Coroutine dialogCoroutine = null;

        // game manager references
        private PlayerManager playerManager = null;
        private MessageLogClient messageLogClient = null;

        public int DialogIndex { get => shownNodeCount; }

        public DialogController(Interactable interactable, SystemGameManager systemGameManager) {
            this.interactable = interactable;
            Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            playerManager = systemGameManager.PlayerManager;
            messageLogClient = systemGameManager.MessageLogClient;
        }

        public void Cleanup() {
            CleanupDialog();
        }

        private void CleanupDialog() {
            if (dialogCoroutine != null) {
                interactable.StopCoroutine(dialogCoroutine);
                interactable.ProcessEndDialog();
                dialogCoroutine = null;
            }
        }

        private void ResetChat() {
            if (chatCoroutine != null) {
                interactable.StopCoroutine(chatCoroutine);
            }
        }

        public void BeginDialog(UnitController sourceUnitController, string dialogName, DialogComponent caller = null) {
            //Debug.Log(interactable.gameObject.name + ".DialogController.BeginDialog(" + dialogName + ")");
            Dialog tmpDialog = systemDataFactory.GetResource<Dialog>(dialogName);
            if (tmpDialog != null) {
                BeginDialog(sourceUnitController, tmpDialog, caller);
            }
        }

        public void BeginDialog(UnitController sourceUnitController, Dialog dialog, DialogComponent caller = null) {
            //Debug.Log($"{interactable.gameObject.name}.DialogController.BeginDialog({sourceUnitController.gameObject.name}, {dialog.ResourceName})");

            if (dialog != null && dialogCoroutine == null) {
                dialogCoroutine = interactable.StartCoroutine(PlayDialog(sourceUnitController, dialog, caller));
            }
        }

        public void BeginChatMessage(string messageText) {
            BeginChatMessage(messageText, chatDisplayTime);
        }

        public void BeginChatMessage(string messageText, float displayTime) {
            
            //CleanupDialog();
            ResetChat();
            chatCoroutine = interactable.StartCoroutine(PlayChatMessage(messageText, displayTime));
        }

        public IEnumerator PlayChatMessage(string messageText, float displayTime) {
            //Debug.Log($"{interactable.gameObject.name}.DialogController.PlayChatMessage({messageText}, {displayTime})");

            interactable.ProcessBeginDialog();

            interactable.ProcessDialogTextUpdate(messageText);

            yield return new WaitForSeconds(displayTime);
            interactable.ProcessEndDialog();
        }

        public IEnumerator PlayDialog(UnitController sourceUnitController, Dialog dialog, DialogComponent caller = null) {
            //Debug.Log($"{interactable.gameObject.name}.DialogController.PlayDialog({dialog.DisplayName})");

            //interactable.ProcessBeginDialog();
            float elapsedTime = 0f;
            shownNodeCount = 0;
            DialogNode currentdialogNode = null;

            // this needs to be reset to allow for repeatable dialogs to replay
            sourceUnitController.CharacterDialogManager.ResetDialogStatus(dialog);

            while (dialog.TurnedIn(sourceUnitController) == false) {
                //Debug.Log($"{interactable.gameObject.name}.DialogController.PlayDialog({dialog.DisplayName}) begin loop");
                for (int i = 0; i < dialog.DialogNodes.Count; i++) {
                    currentdialogNode = dialog.DialogNodes[i];
                    //foreach (DialogNode dialogNode in dialog.DialogNodes) {
                    //Debug.Log($"{currentdialogNode.StartTime}, {currentdialogNode.ShowTime}, {currentdialogNode.Description}");
                    if (elapsedTime >= currentdialogNode.StartTime && currentdialogNode.Shown(sourceUnitController, dialog, i) == false) {
                        //Debug.Log($"{interactable.gameObject.name}.DialogController.PlayDialog({dialog.DisplayName}) index: {i} {elapsedTime} >= {currentdialogNode.StartTime}");
                        PlayDialogNode(currentdialogNode);
                        interactable.InteractableEventController.NotifyOnPlayDialogNode(dialog, i);
                        currentdialogNode.SetShown(sourceUnitController, dialog, true, i);
                        shownNodeCount++;
                    }
                }
                if (shownNodeCount >= dialog.DialogNodes.Count) {
                    sourceUnitController.CharacterDialogManager.TurnInDialog(dialog);
                    if (caller != null) {
                        caller.NotifyOnConfirmAction(sourceUnitController);
                    }
                }
                //Debug.Log($"{interactable.gameObject.name}.DialogController.PlayDialog({dialog.DisplayName}) adding elapsed time {Time.deltaTime}");

                elapsedTime += Time.deltaTime;

                // circuit breaker
                if (elapsedTime >= maxDialogTime) {
                    break;
                }
                yield return null;
            }
            dialogCoroutine = null;

            /*
            if (currentdialogNode != null) {
                yield return new WaitForSeconds(currentdialogNode.ShowTime);
            }
            */
            //interactable.ProcessEndDialog();
        }

        public void PlayDialogNode(string dialogName, int dialogIndex) {
            //Debug.Log($"{interactable.gameObject.name}.DialogController.PlayDialogNode({dialogName}, {dialogIndex})");

            Dialog dialog = systemDataFactory.GetResource<Dialog>(dialogName);
            if (dialog != null && dialog.DialogNodes.Count > dialogIndex) {
                PlayDialogNode(dialog.DialogNodes[dialogIndex]);
            }
        }

        public void PlayDialogNode(DialogNode dialogNode) {
            //Debug.Log($"{interactable.gameObject.name}.DialogController.PlayDialogNode()");

            if (networkManagerServer.ServerModeActive == true) {
                return;
            }
            //bool writeMessage = true;
            if (playerManager != null && playerManager.ActiveUnitController != null) {
                if (Vector3.Distance(interactable.transform.position, playerManager.ActiveUnitController.transform.position) > systemConfigurationManager.MaxChatTextDistance) {
                    //writeMessage = false;
                    return;
                }
            }
            messageLogClient.WriteGeneralMessage($"{interactable.DisplayName}: {dialogNode.Description}");
            //interactable.ProcessDialogTextUpdate(dialogNode.Description);
            BeginChatMessage(dialogNode.Description, dialogNode.ShowTime);
            if (dialogNode.AudioClip != null) {
                interactable.UnitComponentController.PlayVoiceSound(dialogNode.AudioClip);
            }
        }


    }

}