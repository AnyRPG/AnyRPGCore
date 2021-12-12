using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class InteractionPanelUI : WindowContentController {

        private Interactable interactable = null;

        [SerializeField]
        private GameObject interactableButtonPrefab = null;

        [SerializeField]
        private Transform interactableButtonParent = null;

        [SerializeField]
        private GameObject questPrefab = null;

        private List<InteractionPanelQuestScript> questScripts = new List<InteractionPanelQuestScript>();

        //[SerializeField]
        //private GameObject availableArea = null;

        [SerializeField]
        private GameObject availableQuestArea = null;

        /*
        [SerializeField]
        private GameObject completeQuestArea = null;
        */

        private List<InteractionPanelScript> interactionPanelScripts = new List<InteractionPanelScript>();

        // game manager references
        private InteractionManager interactionManager = null;
        private UIManager uIManager = null;
        private PlayerManager playerManager = null;
        private ObjectPooler objectPooler = null;
        private QuestLog questLog = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            interactionManager = systemGameManager.InteractionManager;
            uIManager = systemGameManager.UIManager;
            playerManager = systemGameManager.PlayerManager;
            objectPooler = systemGameManager.ObjectPooler;
            questLog = systemGameManager.QuestLog;
        }

        public void HandleSetInteractable(Interactable _interactable) {
            //Debug.Log("InteractionPanelUI.HandleSetInteractable()");
            if (interactable != null) {
                interactable.OnPrerequisiteUpdates -= HandlePrerequisiteUpdates;
            }
            interactable = _interactable;
            if (interactable != null) {
                interactable.OnPrerequisiteUpdates += HandlePrerequisiteUpdates;
            }
        }

        protected override void ProcessCreateEventSubscriptions() {
            //Debug.Log("InteractionPanelUI.CreateEventSubscriptions()");
            base.ProcessCreateEventSubscriptions();
            interactionManager.OnSetInteractable += HandleSetInteractable;
        }

        protected override void ProcessCleanupEventSubscriptions() {
            //Debug.Log("InteractionPanelUI.CleanupEventSubscriptions()");
            base.ProcessCleanupEventSubscriptions();
            interactionManager.OnSetInteractable -= HandleSetInteractable;
        }

        public void CheckPrerequisites(Skill skill) {
            //Debug.Log("InteractionPanelUI.CheckPrerequisites(skill)");
            CheckPrerequisites();
        }

        public void CheckPrerequisites(Quest quest) {
            //Debug.Log("InteractionPanelUI.CheckPrerequisites(quest)");
            CheckPrerequisites();
        }

        public void HandlePrerequisiteUpdates() {
            //Debug.Log("InteractionPanelUI.HandlePrerequisiteUpdates()");
            CheckPrerequisites();
        }

        public void CheckPrerequisites() {
            //Debug.Log("InteractionPanelUI.CheckPrerequisites()");
            if (interactable == null) {
                //Debug.Log("InteractionPanelUI.CheckPrerequisites(): no interactable. exiting");
                return;
            }
            if (isActiveAndEnabled == false || uIManager.interactionWindow.IsOpen == false) {
                //Debug.Log("InteractionPanelUI.CheckPrerequisites(): window is not active. exiting");
                return;
            }
            ShowInteractables(true);
        }

        public void ShowInteractablesCommon(Interactable interactable, bool suppressAutoInteract = false) {
            //Debug.Log("InteractionPanelUI.ShowInteractablesCommon(" + interactable.name + ")");
            ClearButtons();

            // updated to only use valid interactables
            if (playerManager.PlayerUnitSpawned == false) {
                //Debug.Log("InteractionPanelUI.ShowInteractablesCommon(" + interactable.name + ") player unit is null");
                return;
            }
            List<InteractableOptionComponent> currentInteractables = interactable.GetCurrentInteractables();
            if (currentInteractables.Count == 0) {
                // this could have been a refresh from while a quest was open overtop.  close it if there are no valid interactables
                uIManager.interactionWindow.CloseWindow();
                return;
            }

            // going to just pop the first available interaction window for now and see how that feels
            //bool optionOpened = false;
            foreach (InteractableOptionComponent _interactable in currentInteractables) {
                // handle questgiver
                if (_interactable is QuestGiverComponent) {
                    foreach (QuestNode questNode in (_interactable as QuestGiverComponent).Props.Quests) {
                        Quest quest = questNode.Quest;
                        if (quest != null) {
                            string displayText = string.Empty;
                            string questStatus = quest.GetStatus();
                            if (questStatus == "complete" && questNode.EndQuest == true) {
                                displayText = "<color=yellow>?</color> ";
                            } else if (questNode.StartQuest == true && questStatus == "available") {
                                displayText = "<color=yellow>!</color> ";
                            }
                            // only display complete and available quests here

                            if (displayText != string.Empty) {
                                GameObject go = objectPooler.GetPooledObject(questPrefab, availableQuestArea.transform);
                                InteractionPanelQuestScript qs = go.GetComponent<InteractionPanelQuestScript>();
                                qs.Configure(systemGameManager);
                                qs.Quest = quest;
                                qs.QuestGiver = (_interactable as QuestGiverComponent);

                                displayText += quest.DisplayName;

                                qs.Text.text = displayText;

                                //Debug.Log("QuestTrackerUI.ShowQuestsCommon(" + questGiver.name + "): " + questNode.MyQuest.MyTitle);
                                qs.Text.color = LevelEquations.GetTargetColor(playerManager.MyCharacter.CharacterStats.Level, quest.ExperienceLevel);
                                questScripts.Add(qs);
                                // disabled this next bit because it was causing repeatables with no objectives to be marked as complete
                                /*
                                if (quest.IsComplete && !quest.TurnedIn) {
                                    go.transform.SetParent(completeQuestArea.transform);
                                } else if (!quest.IsComplete && questLog.HasQuest(quest.DisplayName) == false) {
                                    go.transform.SetParent(availableQuestArea.transform);
                                }
                                */
                                

                            }

                        }
                    }
                } else {
                    // this block used to be outside the else statement, but for now we don't want quests to show as an interaction option because they are handled separately above
                    // handle generic stuff
                    if (_interactable.DisplayName != null && _interactable.DisplayName != string.Empty && _interactable.GetCurrentOptionCount() > 0) {
                        //Debug.Log("InteractionPanelUI.ShowInteractablesCommon(" + interactable.name + "): Instantiating button");
                        for (int i = 0; i < _interactable.GetCurrentOptionCount(); i++) {
                            GameObject go = objectPooler.GetPooledObject(interactableButtonPrefab, interactableButtonParent);
                            InteractionPanelScript iPS = go.GetComponent<InteractionPanelScript>();
                            if (iPS != null) {
                                iPS.Configure(systemGameManager);
                                iPS.Setup(_interactable, i);
                                interactionPanelScripts.Add(iPS);
                            }
                        }
                    }

                }

            }
            foreach (InteractionPanelQuestScript questScript in questScripts) {
                uINavigationControllers[0].AddActiveButton(questScript);
            }
            foreach (InteractionPanelScript interactionPanelScript in interactionPanelScripts) {
                uINavigationControllers[0].AddActiveButton(interactionPanelScript);
            }

            uINavigationControllers[0].FocusFirstButton();

            if (uIManager.dialogWindow.IsOpen) {
                //Debug.Log("InteractionPanelUI.ShowInteractablesCommon(" + interactable.name + "): Dialog Window is open, returning to prevent other windows from popping");
                // if we are mid dialog, we don't want to pop another window yet
                return;
            }


            // priority open - any other current interactable third, but only if there is one
            if (currentInteractables.Count > 1 || suppressAutoInteract == true || uINavigationControllers[0].ActiveNavigableButtonCount > 1) {
                //Debug.Log("InteractionPanelUI.Interact(): currentInteractables count: " + currentInteractables.Count);
                return;
            }

            // priority open - completed quest first
            foreach (InteractionPanelQuestScript questScript in questScripts) {
                //Debug.Log("InteractionPanelUI.ShowInteractablesCommon(" + interactable.name + "): Checking questScript for complete quest");
                if (questScript.Quest.MarkedComplete) {
                    //Debug.Log("InteractionPanelUI.ShowInteractablesCommon(" + interactable.name + "): Checking questScript: quest is complete, selecting");
                    questScript.Interact();
                    //optionOpened = true;
                    return;
                }
            }

            // priority open - available quest second
            foreach (InteractionPanelQuestScript questScript in questScripts) {
                //Debug.Log("InteractionPanelUI.ShowInteractablesCommon(" + interactable.name + "): Checking questScript for available quest");
                if (questScript.Quest.GetStatus() == "available") {
                    //Debug.Log("InteractionPanelUI.ShowInteractablesCommon(" + interactable.name + "): Checking questScript: quest is available, selecting");
                    questScript.Interact();
                    //optionOpened = true;
                    return;
                }
            }

            foreach (InteractionPanelScript interactionPanelScript in interactionPanelScripts) {
                //Debug.Log("InteractionPanelUI.ShowInteractablesCommon(" + interactable.name + "): Checking interaction Panel Script");
                if (interactionPanelScript.InteractableOption.CanInteract() && interactionPanelScript.InteractableOption.GetCurrentOptionCount() == 1) {
                    //Debug.Log("InteractionPanelUI.ShowInteractablesCommon(" + interactable.name + "): Checking interaction Panel Script: canInteract is TRUE!!!");
                    interactionPanelScript.InteractableOption.Interact(playerManager.UnitController.CharacterUnit);
                    //optionOpened = true;
                    return;
                }
            }

        }

        public void ShowInteractables(bool suppressAutoInteract = false) {
            //Debug.Log("InteractionPanelUI.ShowInteractables(" + suppressAutoInteract + ")");
            if (interactable != null) {
                ShowInteractablesCommon(interactable, suppressAutoInteract);
            }
        }

        public void ShowInteractables(Interactable interactable) {
            //Debug.Log("InteractionPanelUI.ShowInteractables(" + interactable.name + ")");
            interactionManager.CurrentInteractable = interactable;
            ShowInteractablesCommon(this.interactable);
        }

        public void ClearButtons() {
            //Debug.Log("InteractionPanelUI.ClearButtons()");
            // clear the skill list so any skill left over from a previous time opening the window aren't shown
            foreach (InteractionPanelQuestScript qs in questScripts) {
                qs.transform.SetParent(null);
                qs.DeSelect();
                objectPooler.ReturnObjectToPool(qs.gameObject);
            }
            questScripts.Clear();

            foreach (InteractionPanelScript interactionPanelScript in interactionPanelScripts) {
                interactionPanelScript.transform.SetParent(null);
                objectPooler.ReturnObjectToPool(interactionPanelScript.gameObject);
            }
            interactionPanelScripts.Clear();
            uINavigationControllers[0].ClearActiveButtons();
        }

        public override void ReceiveClosedWindowNotification() {
            //Debug.Log("InteractionPanelUI.OnCloseWindow()");
            //ClearButtons();
            base.ReceiveClosedWindowNotification();
            // clear this so window doesn't pop open again when it's closed
            interactionManager.CurrentInteractable = null;
        }

        public override void ProcessOpenWindowNotification() {
            //Debug.Log("InteractionPanelUI.ProcessOpenWindowNotification()");

            // do this last or it could close the window before we set the title.  it just calls the onopenwindowhandler, so nothing that needs to be done before the 2 above lines
            // ??? ??? IS THIS STILL TRUE?  WINDOW STACKS ARE IN WRONG ORDER BECAUSE OF THIS.  MOVED FROM BOTTOM TO TOP TO TEST
            base.ProcessOpenWindowNotification();

            SetBackGroundColor(new Color32(0, 0, 0, (byte)(int)(PlayerPrefs.GetFloat("PopupWindowOpacity") * 255)));

            // this has to be done first, because the next line after could close the window and set the interactable to null
            if (uIManager != null) {
                if (interactable == null) {
                    Debug.Log("interactable is null");
                }
                if (interactable.DisplayName == null) {
                    Debug.Log("interactable.displayname is null");
                }
                if (uIManager.interactionWindow == null) {
                    Debug.Log("interactactionwindow is null");
                }
                uIManager.interactionWindow.SetWindowTitle(interactable.DisplayName);
            }

            ShowInteractables();

            // do this last or it could close the window before we set the title.  it just calls the onopenwindowhandler, so nothing that needs to be done before the 2 above lines
            // ??? ??? IS THIS STILL TRUE?  WINDOW STACKS ARE IN WRONG ORDER BECAUSE OF THIS.  MOVED FROM BOTTOM TO TOP TO TEST
            //base.ProcessOpenWindowNotification();
        }
    }

}