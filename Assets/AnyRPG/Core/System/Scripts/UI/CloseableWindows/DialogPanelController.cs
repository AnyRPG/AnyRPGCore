using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class DialogPanelController : WindowContentController {

        private int dialogIndex = 0;

        [Tooltip("If no next text is provided for a dialog, this text will be used")]
        [SerializeField]
        private string defaultNextText = "Next";

        [SerializeField]
        private TextMeshProUGUI characterNameText = null;

        [SerializeField]
        private TextMeshProUGUI dialogText = null;

        [SerializeField]
        private TextMeshProUGUI buttonText = null;

        [SerializeField]
        private HighlightButton viewQuestButton = null;

        [SerializeField]
        private HighlightButton acceptQuestButton = null;

        [SerializeField]
        private HighlightButton continueButton = null;

        [SerializeField]
        int dialogFontSize = 30;


        // game manager references
        protected UIManager uIManager = null;
        protected QuestLog questLog = null;
        protected LogManager logManager = null;
        protected DialogManager dialogManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            uIManager = systemGameManager.UIManager;
            questLog = systemGameManager.QuestLog;
            logManager = systemGameManager.LogManager;
            dialogManager = systemGameManager.DialogManager;
        }

        public void CancelAction() {
            //Debug.Log("NewGameMenuController.CancelAction()");
            uIManager.dialogWindow.CloseWindow();
        }

        public void ConfirmAction() {
            //Debug.Log("NewGameMenuController.ConfirmAction(): dialogIndex: " + dialogIndex + "; DialogNode Count: " + MyDialog.MyDialogNodes.Count);
            dialogIndex++;
            if (dialogIndex >= dialogManager.Dialog.DialogNodes.Count) {
                dialogManager.Dialog.TurnedIn = true;
                if (dialogManager.Quest == null) {

                    // next line is no longer true because onconfirmaction calls a prerequisiteupdate on the dialog controller
                    // no one is currently subscribed so safe to set turnedIn at bottom because nothing here depends on it being set yet

                    dialogManager.ConfirmAction();
                    uIManager.dialogWindow.CloseWindow();
                } else {
                    if (!dialogManager.Quest.TurnedIn) {
                        DisplayQuestText();
                        continueButton.gameObject.SetActive(false);
                        viewQuestButton.gameObject.SetActive(true);
                        acceptQuestButton.gameObject.SetActive(true);
                        currentNavigationController.UpdateNavigationList();
                        if (controlsManager.GamePadInputActive) {
                            currentNavigationController.FocusFirstButton();
                        }

                    } else {
                        //Debug.Log("NewGameMenuController.ConfirmAction(): dialogIndex: " + dialogIndex + "; DialogNode Count: " + MyDialog.MyDialogNodes.Count + "; TRIED TO DISPLAY ALREADY TURNED IN QUEST!");
                        uIManager.dialogWindow.CloseWindow();
                    }
                }
                // going to see what got blocked from popping, because this needs to be set true before we call onconfirmaction
                // SETTING THIS LAST SO THE DIALOG WINDOW IS DETECTED AS CLOSED, AND OTHER WINDOWS DON'T GET BLOCKED FROM POPPING.
                //MyDialog.TurnedIn = true;
            } else {
                DisplayNodeText();
            }
        }

        public void DisplayQuestText() {
            //Debug.Log("DialogPanelController.DisplayQuestText()");
            if (dialogManager.Quest != null) {
                dialogText.text = dialogManager.Quest.GetObjectiveDescription();
            }
        }

        public void ViewQuest() {
            uIManager.questGiverWindow.OpenWindow();
            questLog.ShowQuestGiverDescription(dialogManager.Quest, null);
            uIManager.dialogWindow.CloseWindow();
        }

        public void AcceptQuest() {
            //Debug.Log("DialogPanelController.AcceptQuest()");
            // CLOSE THIS FIRST SO OTHER WINDOWS AREN'T BLOCKED FROM POPPING
            uIManager.dialogWindow.CloseWindow();

            questLog.AcceptQuest(dialogManager.Quest);
            //interactable.CheckForInteractableObjectives(MyQuest.DisplayName);
        }

        public void DisplayNodeText() {
            if (dialogIndex > dialogManager.Dialog.DialogNodes.Count + 1) {
                //Debug.Log("Past last node index.  will not display");
                return;
            }
            if (characterNameText != null) {
                characterNameText.text = dialogManager.Interactable.DisplayName;
            }
            
            if (dialogText != null) {
                dialogText.text = string.Format("<size={0}>{1}</size>", dialogFontSize, dialogManager.Dialog.DialogNodes[dialogIndex].Description);
            }

            logManager.WriteChatMessage(dialogManager.Dialog.DialogNodes[dialogIndex].Description);
            if (dialogManager.Dialog.DialogNodes[dialogIndex].AudioClip != null) {
                audioManager.PlayVoice(dialogManager.Dialog.DialogNodes[dialogIndex].AudioClip);
            }

            if (buttonText != null) {
                if (dialogManager.Dialog.DialogNodes[dialogIndex].NextOption != string.Empty) {
                    buttonText.text = dialogManager.Dialog.DialogNodes[dialogIndex].NextOption;
                } else {
                    buttonText.text = defaultNextText;
                }
                //Debug.Log("DialogPanelController.OnOpenWindow(): ButtonText is not null, rebuilding layout");
                //LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
                LayoutRebuilder.ForceRebuildLayoutImmediate(continueButton.GetComponent<RectTransform>());

                // testing to see if this helps button resize properly
                //LayoutRebuilder.ForceRebuildLayoutImmediate(gameObject.GetComponentInParent<RectTransform>());
            } else {
                //Debug.Log("DialogPanelController.OnOpenWindow(): ButtonText is null!!");
            }
            uINavigationControllers[0].UpdateNavigationList();
            if (controlsManager.GamePadInputActive) {
                currentNavigationController.FocusFirstButton();
            }
        }

        public override void ProcessOpenWindowNotification() {
            //Debug.Log("DialogPanelController.OnOpenWindow()");
            base.ProcessOpenWindowNotification();
            SetBackGroundColor(new Color32(0, 0, 0, (byte)(int)(PlayerPrefs.GetFloat("PopupWindowOpacity") * 255)));

            // these go first or they will squish the text of the continue button out of place
            viewQuestButton.gameObject.SetActive(false);
            acceptQuestButton.gameObject.SetActive(false);
            continueButton.gameObject.SetActive(true);
            dialogIndex = 0;
            uIManager.dialogWindow.SetWindowTitle(dialogManager.Interactable.DisplayName);

            SetNavigationController(uINavigationControllers[0]);

            // this one last because it does a layout rebuild
            DisplayNodeText();
        }

        public override void ReceiveClosedWindowNotification() {
            base.ReceiveClosedWindowNotification();
            dialogManager.EndInteraction();

            viewQuestButton.gameObject.SetActive(false);
            acceptQuestButton.gameObject.SetActive(false);
            continueButton.gameObject.SetActive(false);
        }


    }

}