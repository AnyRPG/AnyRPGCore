using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class DialogPanelController : WindowContentController {

        public event System.Action OnConfirmAction = delegate { };
        public override event Action<ICloseableWindowContents> OnCloseWindow = delegate { };

        [Tooltip("If no next text is provided for a dialog, this text will be used")]
        [SerializeField]
        private string defaultNextText = "Next";

        [SerializeField]
        private TextMeshProUGUI characterNameText = null;

        [SerializeField]
        private TextMeshProUGUI dialogText = null;

        /*
        [SerializeField]
        private Button nextButton;
        */

        [SerializeField]
        private TextMeshProUGUI buttonText = null;

        [SerializeField]
        private GameObject viewQuestButton = null;

        [SerializeField]
        private GameObject acceptQuestButton = null;

        [SerializeField]
        private GameObject continueButton = null;

        [SerializeField]
        int dialogFontSize = 30;

        private Interactable interactable = null;

        private Dialog dialog = null;

        private Quest quest = null;

        public Dialog MyDialog { get => dialog; set => dialog = value; }
        public Interactable MyInteractable { get => interactable; set => interactable = value; }
        public Quest MyQuest { get => quest; set => quest = value; }

        private int dialogIndex = 0;

        public void Setup(Quest quest, Interactable interactable) {
            //Debug.Log("DialogPanelController.Setup(" + (quest == null ? "null" : quest.DisplayName) + ", " + (interactable == null ? "null" : interactable.DisplayName) + ")");
            ClearSettings();
            MyQuest = quest;
            MyInteractable = interactable;
            MyDialog = quest.MyOpeningDialog;
            PopupWindowManager.MyInstance.dialogWindow.OpenWindow();
        }

        public void Setup(Dialog dialog, Interactable interactable) {
            //Debug.Log("DialogPanelController.Setup(" + dialog.DisplayName + ", " + interactable.DisplayName + ")");
            ClearSettings();
            MyInteractable = interactable;
            MyDialog = dialog;
            PopupWindowManager.MyInstance.dialogWindow.OpenWindow();
        }

        public void ClearSettings() {
            dialogIndex = 0;
            MyInteractable = null;
            MyQuest = null;
            MyDialog = null;
        }

        public void CancelAction() {
            //Debug.Log("NewGameMenuController.CancelAction()");
            PopupWindowManager.MyInstance.dialogWindow.CloseWindow();
        }

        public void ConfirmAction() {
            //Debug.Log("NewGameMenuController.ConfirmAction(): dialogIndex: " + dialogIndex + "; DialogNode Count: " + MyDialog.MyDialogNodes.Count);
            dialogIndex++;
            if (dialogIndex >= MyDialog.DialogNodes.Count) {
                MyDialog.TurnedIn = true;
                if (quest == null) {

                    // next line is no longer true because onconfirmaction calls a prerequisiteupdate on the dialog controller
                    // no one is currently subscribed so safe to set turnedIn at bottom because nothing here depends on it being set yet

                    OnConfirmAction();
                    PopupWindowManager.MyInstance.dialogWindow.CloseWindow();
                } else {
                    if (!quest.TurnedIn) {
                        DisplayQuestText();
                        continueButton.SetActive(false);
                        viewQuestButton.SetActive(true);
                        acceptQuestButton.SetActive(true);
                    } else {
                        //Debug.Log("NewGameMenuController.ConfirmAction(): dialogIndex: " + dialogIndex + "; DialogNode Count: " + MyDialog.MyDialogNodes.Count + "; TRIED TO DISPLAY ALREADY TURNED IN QUEST!");
                        PopupWindowManager.MyInstance.dialogWindow.CloseWindow();
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
            if (MyQuest != null) {
                //nodeText.text = MyQuest.GetObjectiveDescription();
                dialogText.text = MyQuest.GetObjectiveDescription();
            }
        }

        public void ViewQuest() {
            // testing disable this because it was already set by showquests
            //QuestGiverUI.MyInstance.MyInteractable = interactable;
            PopupWindowManager.MyInstance.questGiverWindow.OpenWindow();
            QuestGiverUI.MyInstance.ShowDescription(MyQuest);
            PopupWindowManager.MyInstance.dialogWindow.CloseWindow();
        }

        public void AcceptQuest() {
            //Debug.Log("DialogPanelController.AcceptQuest()");
            // CLOSE THIS FIRST SO OTHER WINDOWS AREN'T BLOCKED FROM POPPING
            PopupWindowManager.MyInstance.dialogWindow.CloseWindow();

            QuestLog.MyInstance.AcceptQuest(MyQuest);
            //interactable.CheckForInteractableObjectives(MyQuest.MyName);
        }

        public void DisplayNodeText() {
            if (dialogIndex > MyDialog.DialogNodes.Count + 1) {
                //Debug.Log("Past last node index.  will not display");
                return;
            }
            if (characterNameText != null) {
                characterNameText.text = MyInteractable.DisplayName;
            }
            
            if (dialogText != null) {
                dialogText.text = string.Format("<size={0}>{1}</size>", dialogFontSize, MyDialog.DialogNodes[dialogIndex].MyDescription);
            }

            CombatLogUI.MyInstance.WriteChatMessage(MyDialog.DialogNodes[dialogIndex].MyDescription);
            if (AudioManager.MyInstance != null && MyDialog.AudioProfile != null && MyDialog.AudioProfile.AudioClips != null && MyDialog.AudioProfile.AudioClips.Count > dialogIndex) {
                AudioManager.MyInstance.PlayVoice(MyDialog.AudioProfile.AudioClips[dialogIndex]);
            }

            if (buttonText != null) {
                if (MyDialog.DialogNodes[dialogIndex].MyNextOption != string.Empty) {
                    buttonText.text = MyDialog.DialogNodes[dialogIndex].MyNextOption;
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
        }

        public override void ReceiveOpenWindowNotification() {
            //Debug.Log("DialogPanelController.OnOpenWindow()");
            base.ReceiveOpenWindowNotification();
            SetBackGroundColor(new Color32(0, 0, 0, (byte)(int)(PlayerPrefs.GetFloat("PopupWindowOpacity") * 255)));

            // these go first or they will squish the text of the continue button out of place
            viewQuestButton.SetActive(false);
            acceptQuestButton.SetActive(false);
            continueButton.SetActive(true);
            dialogIndex = 0;
            PopupWindowManager.MyInstance.dialogWindow.SetWindowTitle(interactable.DisplayName);

            // this one last because it does a layout rebuild
            DisplayNodeText();
        }

        public override void RecieveClosedWindowNotification() {
            base.RecieveClosedWindowNotification();
            OnCloseWindow(this);

            viewQuestButton.SetActive(false);
            acceptQuestButton.SetActive(false);
            continueButton.SetActive(false);
        }


    }

}