using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class DialogPanel : WindowPanel {

        [Header("Dialog Panel")]

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
        private int dialogFontSize = 30;

        [SerializeField]
        private Image portraitImage = null;

        [SerializeField]
        private RawImage portraitSnapshotImage = null;


        private int dialogIndex = 0;

        // game manager references
        protected UIManager uIManager = null;
        protected MessageLogClient messageLogClient = null;
        protected DialogManagerClient dialogManagerClient = null;
        protected PlayerManager playerManager = null;
        protected QuestGiverManagerClient questGiverManager = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            uIManager = systemGameManager.UIManager;
            messageLogClient = systemGameManager.MessageLogClient;
            dialogManagerClient = systemGameManager.DialogManagerClient;
            playerManager = systemGameManager.PlayerManager;
            questGiverManager = systemGameManager.QuestGiverManagerClient;
        }

        public void CancelAction() {
            //Debug.Log("NewGameMenuController.CancelAction()");
            uIManager.dialogWindow.CloseWindow();
        }

        public void ConfirmAction() {
            //Debug.Log("NewGameMenuController.ConfirmAction(): dialogIndex: " + dialogIndex + "; DialogNode Count: " + MyDialog.MyDialogNodes.Count);
            dialogIndex++;
            if (dialogIndex >= dialogManagerClient.Dialog.DialogNodes.Count) {
                if (dialogManagerClient.Quest == null) {
                    dialogManagerClient.RequestTurnInDialog(playerManager.UnitController);
                    uIManager.dialogWindow.CloseWindow();
                } else {
                    dialogManagerClient.RequestTurnInQuestDialog(playerManager.UnitController);
                    if (!dialogManagerClient.Quest.TurnedIn(playerManager.UnitController)) {
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
            if (dialogManagerClient.Quest != null) {
                dialogText.text = dialogManagerClient.Quest.GetObjectiveDescription(playerManager.UnitController);
            }
        }

        public void ViewQuest() {
            uIManager.questGiverWindow.OpenWindow();
            playerManager.UnitController.CharacterQuestLog.ShowQuestGiverDescription(dialogManagerClient.Quest, null);
            uIManager.dialogWindow.CloseWindow();
        }

        public void AcceptQuest() {
            //Debug.Log("DialogPanelController.AcceptQuest()");
            Quest quest = dialogManagerClient.Quest;

            // CLOSE THIS FIRST SO OTHER WINDOWS AREN'T BLOCKED FROM POPPING
            uIManager.dialogWindow.CloseWindow();

            questGiverManager.RequestAcceptQuest(playerManager.UnitController, quest);

            //interactable.CheckForInteractableObjectives(MyQuest.DisplayName);
        }

        public void DisplayNodeText() {
            if (dialogIndex > dialogManagerClient.Dialog.DialogNodes.Count + 1) {
                //Debug.Log("Past last node index.  will not display");
                return;
            }
            if (characterNameText != null) {
                characterNameText.text = dialogManagerClient.Interactable.DisplayName;
            }
            
            if (dialogText != null) {
                dialogText.text = string.Format("<size={0}>{1}</size>", dialogFontSize, dialogManagerClient.Dialog.DialogNodes[dialogIndex].Description);
            }
            string chatMessage = dialogManagerClient.Dialog.DialogNodes[dialogIndex].Description;
            if (dialogManagerClient.Interactable != null) {
                chatMessage = $"{dialogManagerClient.Interactable.DisplayName}: {chatMessage}";
            }
            messageLogClient.WriteGeneralMessage(chatMessage);
            if (dialogManagerClient.Dialog.DialogNodes[dialogIndex].AudioClip != null) {
                audioManager.PlayVoice(dialogManagerClient.Dialog.DialogNodes[dialogIndex].AudioClip);
            }

            if (buttonText != null) {
                if (dialogManagerClient.Dialog.DialogNodes[dialogIndex].NextOption != string.Empty) {
                    buttonText.text = dialogManagerClient.Dialog.DialogNodes[dialogIndex].NextOption;
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
            uIManager.dialogWindow.SetWindowTitle(dialogManagerClient.Interactable.DisplayName);

            SetNavigationController(uINavigationControllers[0]);

            TargetInitialization();

            // this one last because it does a layout rebuild
            DisplayNodeText();
        }

        private void TargetInitialization() {
            dialogManagerClient.Interactable.ConfigureDialogPanel(this);
        }

        public void ConfigurePortrait(Sprite icon) {
            portraitSnapshotImage.gameObject.SetActive(false);
            portraitImage.gameObject.SetActive(true);

            portraitImage.sprite = icon;
        }

        public void ConfigureSnapshotPortrait() {
            portraitImage.gameObject.SetActive(false);
            portraitSnapshotImage.gameObject.SetActive(true);
            /*
            if (namePlateController.NamePlateUnit.CameraTargetReady) {
                HandleTargetReady();
            }// else {
             // testing subscribe no matter what in case unit appearance changes
            SubscribeToTargetReady();
            //}
            */
        }

        public override void ReceiveClosedWindowNotification() {
            base.ReceiveClosedWindowNotification();
            dialogManagerClient.EndInteraction();

            viewQuestButton.gameObject.SetActive(false);
            acceptQuestButton.gameObject.SetActive(false);
            continueButton.gameObject.SetActive(false);
        }


    }

}