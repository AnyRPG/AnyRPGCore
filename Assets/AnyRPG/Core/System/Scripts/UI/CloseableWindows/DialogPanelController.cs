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
        //public override event Action<ICloseableWindowContents> OnCloseWindow = delegate { };
        public override event Action<CloseableWindowContents> OnCloseWindow = delegate { };

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
        private HighlightButton viewQuestButton = null;

        [SerializeField]
        private HighlightButton acceptQuestButton = null;

        [SerializeField]
        private HighlightButton continueButton = null;

        [SerializeField]
        int dialogFontSize = 30;

        private Interactable interactable = null;

        private Dialog dialog = null;

        private Quest quest = null;

        public Dialog Dialog { get => dialog; set => dialog = value; }
        public Interactable Interactable { get => interactable; set => interactable = value; }
        public Quest Quest { get => quest; set => quest = value; }

        private int dialogIndex = 0;

        // game manager references
        protected UIManager uIManager = null;
        protected QuestLog questLog = null;
        protected LogManager logManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            /*
            continueButton.Configure(systemGameManager);
            viewQuestButton.Configure(systemGameManager);
            acceptQuestButton.Configure(systemGameManager);
            */
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            uIManager = systemGameManager.UIManager;
            questLog = systemGameManager.QuestLog;
            logManager = systemGameManager.LogManager;
        }

        public void Setup(Quest quest, Interactable interactable) {
            //Debug.Log("DialogPanelController.Setup(" + (quest == null ? "null" : quest.DisplayName) + ", " + (interactable == null ? "null" : interactable.DisplayName) + ")");
            ClearSettings();
            Quest = quest;
            Interactable = interactable;
            Dialog = quest.OpeningDialog;
            uIManager.dialogWindow.OpenWindow();
        }

        public void Setup(Dialog dialog, Interactable interactable) {
            //Debug.Log("DialogPanelController.Setup(" + dialog.DisplayName + ", " + interactable.DisplayName + ")");
            ClearSettings();
            Interactable = interactable;
            Dialog = dialog;
            uIManager.dialogWindow.OpenWindow();
        }

        public void ClearSettings() {
            dialogIndex = 0;
            Interactable = null;
            Quest = null;
            Dialog = null;
        }

        public void CancelAction() {
            //Debug.Log("NewGameMenuController.CancelAction()");
            uIManager.dialogWindow.CloseWindow();
        }

        public void ConfirmAction() {
            //Debug.Log("NewGameMenuController.ConfirmAction(): dialogIndex: " + dialogIndex + "; DialogNode Count: " + MyDialog.MyDialogNodes.Count);
            dialogIndex++;
            if (dialogIndex >= Dialog.DialogNodes.Count) {
                Dialog.TurnedIn = true;
                if (quest == null) {

                    // next line is no longer true because onconfirmaction calls a prerequisiteupdate on the dialog controller
                    // no one is currently subscribed so safe to set turnedIn at bottom because nothing here depends on it being set yet

                    OnConfirmAction();
                    uIManager.dialogWindow.CloseWindow();
                } else {
                    if (!quest.TurnedIn) {
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
            if (Quest != null) {
                dialogText.text = Quest.GetObjectiveDescription();
            }
        }

        public void ViewQuest() {
            uIManager.questGiverWindow.OpenWindow();
            questLog.ShowQuestGiverDescription(Quest, null);
            uIManager.dialogWindow.CloseWindow();
        }

        public void AcceptQuest() {
            //Debug.Log("DialogPanelController.AcceptQuest()");
            // CLOSE THIS FIRST SO OTHER WINDOWS AREN'T BLOCKED FROM POPPING
            uIManager.dialogWindow.CloseWindow();

            questLog.AcceptQuest(Quest);
            //interactable.CheckForInteractableObjectives(MyQuest.DisplayName);
        }

        public void DisplayNodeText() {
            if (dialogIndex > Dialog.DialogNodes.Count + 1) {
                //Debug.Log("Past last node index.  will not display");
                return;
            }
            if (characterNameText != null) {
                characterNameText.text = Interactable.DisplayName;
            }
            
            if (dialogText != null) {
                dialogText.text = string.Format("<size={0}>{1}</size>", dialogFontSize, Dialog.DialogNodes[dialogIndex].Description);
            }

            logManager.WriteChatMessage(Dialog.DialogNodes[dialogIndex].Description);
            if (Dialog.DialogNodes[dialogIndex].AudioClip != null) {
                audioManager.PlayVoice(Dialog.DialogNodes[dialogIndex].AudioClip);
            }

            if (buttonText != null) {
                if (Dialog.DialogNodes[dialogIndex].NextOption != string.Empty) {
                    buttonText.text = Dialog.DialogNodes[dialogIndex].NextOption;
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
            uIManager.dialogWindow.SetWindowTitle(interactable.DisplayName);

            SetNavigationController(uINavigationControllers[0]);

            // this one last because it does a layout rebuild
            DisplayNodeText();
        }

        public override void ReceiveClosedWindowNotification() {
            base.ReceiveClosedWindowNotification();
            OnCloseWindow(this);

            viewQuestButton.gameObject.SetActive(false);
            acceptQuestButton.gameObject.SetActive(false);
            continueButton.gameObject.SetActive(false);
        }


    }

}