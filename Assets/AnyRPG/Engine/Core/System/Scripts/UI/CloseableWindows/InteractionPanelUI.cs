using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class InteractionPanelUI : WindowContentController {

        #region Singleton
        private static InteractionPanelUI instance;

        public static InteractionPanelUI MyInstance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<InteractionPanelUI>();
                }

                return instance;
            }
        }

        #endregion

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

        [SerializeField]
        private GameObject completeQuestArea = null;

        private List<GameObject> interactionPanelScripts = new List<GameObject>();

        protected bool eventSubscriptionsInitialized = false;

        public Interactable MyInteractable {
            get => interactable;
            set {
                //Debug.Log("InteractionPanelUI.MyInteractable.Set(" + (value == null ? "null" : value.MyName ) + ")");
                if (interactable != null) {
                    interactable.OnPrerequisiteUpdates -= HandlePrerequisiteUpdates;
                }
                interactable = value;
                if (interactable != null) {
                    interactable.OnPrerequisiteUpdates += HandlePrerequisiteUpdates;
                }
            }
        }

        private void Start() {
            CreateEventSubscriptions();
        }

        private void OnEnable() {
            //Debug.Log("InteractionPanelUI.OnEnable()");
            CreateEventSubscriptions();
        }

        private void CreateEventSubscriptions() {
            //Debug.Log("PlayerManager.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized) {
                return;
            }
            //SystemEventManager.MyInstance.OnPrerequisiteUpdated += CheckPrerequisites;
            eventSubscriptionsInitialized = true;
        }

        private void CleanupEventSubscriptions() {
            //Debug.Log("PlayerManager.CleanupEventSubscriptions()");
            if (!eventSubscriptionsInitialized) {
                return;
            }
            //SystemEventManager.MyInstance.OnPrerequisiteUpdated -= CheckPrerequisites;
            eventSubscriptionsInitialized = false;
        }

        public void OnDisable() {
            //Debug.Log("PlayerManager.OnDisable()");
            CleanupEventSubscriptions();
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
            if (isActiveAndEnabled == false || PopupWindowManager.MyInstance.interactionWindow.IsOpen == false) {
                //Debug.Log("InteractionPanelUI.CheckPrerequisites(): window is not active. exiting");
                return;
            }
            ShowInteractables(true);
        }

        public void ShowInteractablesCommon(Interactable interactable, bool suppressAutoInteract = false) {
            //Debug.Log("InteractionPanelUI.ShowInteractablesCommon(" + interactable.name + ")");
            ClearButtons();

            // updated to only use valid interactables
            if (PlayerManager.MyInstance.PlayerUnitSpawned == false) {
                //Debug.Log("InteractionPanelUI.ShowInteractablesCommon(" + interactable.name + ") player unit is null");
                return;
            }
            List<InteractableOptionComponent> currentInteractables = interactable.GetCurrentInteractables();
            if (currentInteractables.Count == 0) {
                // this could have been a refresh from while a quest was open overtop.  close it if there are no valid interactables
                PopupWindowManager.MyInstance.interactionWindow.CloseWindow();
                return;
            }

            // going to just pop the first available interaction window for now and see how that feels
            //bool optionOpened = false;
            foreach (InteractableOptionComponent _interactable in currentInteractables) {
                //Debug.Log("InteractionPanelUI.ShowInteractablesCommon(" + interactable.name + "): _interactable: " + _interactable.MyName + "; type: " + _interactable.GetType() + "; Checking for valid button");
                // handle questgiver
                if (_interactable is QuestGiverComponent) {
                    foreach (QuestNode questNode in (_interactable as QuestGiverComponent).Props.Quests) {
                        Quest quest = questNode.MyQuest;
                        if (quest != null) {
                            //Debug.Log("InteractionPanelUI.ShowQuestsCommon(): quest: " + quest.MyName);
                            string displayText = string.Empty;
                            string questStatus = quest.GetStatus();
                            if (questStatus == "complete" && questNode.MyEndQuest == true) {
                                displayText = "<color=yellow>?</color> ";
                            } else if (questNode.MyStartQuest == true && questStatus == "available") {
                                displayText = "<color=yellow>!</color> ";
                            }
                            // only display complete and available quests here

                            if (displayText != string.Empty) {
                                GameObject go = Instantiate(questPrefab, availableQuestArea.transform);
                                InteractionPanelQuestScript qs = go.GetComponent<InteractionPanelQuestScript>();
                                qs.MyQuest = quest;
                                qs.MyQuestGiver = (_interactable as QuestGiverComponent);

                                displayText += quest.DisplayName;

                                qs.MyText.text = displayText;

                                //Debug.Log("QuestTrackerUI.ShowQuestsCommon(" + questGiver.name + "): " + questNode.MyQuest.MyTitle);
                                qs.MyText.color = LevelEquations.GetTargetColor(PlayerManager.MyInstance.MyCharacter.CharacterStats.Level, quest.MyExperienceLevel);
                                //quests.Add(go);
                                questScripts.Add(qs);
                                if (quest.IsComplete && !quest.TurnedIn) {
                                    go.transform.SetParent(completeQuestArea.transform);
                                } else if (!quest.IsComplete && QuestLog.MyInstance.HasQuest(quest.DisplayName) == false) {
                                    go.transform.SetParent(availableQuestArea.transform);
                                }

                            }

                        }
                    }
                } else {
                    // this block used to be outside the else statement, but for now we don't want quests to show as an interaction option because they are handled separately above
                    // handle generic stuff
                    if (_interactable.DisplayName != null && _interactable.DisplayName != string.Empty && _interactable.GetCurrentOptionCount() > 0) {
                        //Debug.Log("InteractionPanelUI.ShowInteractablesCommon(" + interactable.name + "): Instantiating button");
                        for (int i = 0; i < _interactable.GetCurrentOptionCount(); i++) {
                            GameObject go = Instantiate(interactableButtonPrefab, interactableButtonParent);
                            InteractionPanelScript iPS = go.GetComponent<InteractionPanelScript>();
                            if (iPS != null) {
                                iPS.Setup(_interactable, i);
                                interactionPanelScripts.Add(go);
                            }
                        }
                    }

                }

            }

            if (PopupWindowManager.MyInstance.dialogWindow.IsOpen) {
                //Debug.Log("InteractionPanelUI.ShowInteractablesCommon(" + interactable.name + "): Dialog Window is open, returning to prevent other windows from popping");
                // if we are mid dialog, we don't want to pop another window yet
                return;
            }

            // priority open - completed quest first
            foreach (InteractionPanelQuestScript questScript in questScripts) {
                //Debug.Log("InteractionPanelUI.ShowInteractablesCommon(" + interactable.name + "): Checking questScript for complete quest");
                if (questScript.MyQuest.IsComplete) {
                    //Debug.Log("InteractionPanelUI.ShowInteractablesCommon(" + interactable.name + "): Checking questScript: quest is complete, selecting");
                    questScript.Select();
                    //optionOpened = true;
                    return;
                }
            }

            // priority open - available quest second
            foreach (InteractionPanelQuestScript questScript in questScripts) {
                //Debug.Log("InteractionPanelUI.ShowInteractablesCommon(" + interactable.name + "): Checking questScript for available quest");
                if (questScript.MyQuest.GetStatus() == "available") {
                    //Debug.Log("InteractionPanelUI.ShowInteractablesCommon(" + interactable.name + "): Checking questScript: quest is available, selecting");
                    questScript.Select();
                    //optionOpened = true;
                    return;
                }
            }

            // priority open - any other current interactable third, but only if there is one
            if (currentInteractables.Count > 1 || suppressAutoInteract == true) {
                //Debug.Log("InteractionPanelUI.Interact(): currentInteractables count: " + currentInteractables.Count);
                return;
            }
            foreach (GameObject interactionPanelScript in interactionPanelScripts) {
                //Debug.Log("InteractionPanelUI.ShowInteractablesCommon(" + interactable.name + "): Checking interaction Panel Script");
                InteractionPanelScript iPS = interactionPanelScript.GetComponent<InteractionPanelScript>();
                if (iPS.InteractableOption.CanInteract() && iPS.InteractableOption.GetCurrentOptionCount() == 1) {
                    //Debug.Log("InteractionPanelUI.ShowInteractablesCommon(" + interactable.name + "): Checking interaction Panel Script: canInteract is TRUE!!!");
                    iPS.InteractableOption.Interact(PlayerManager.MyInstance.UnitController.CharacterUnit);
                    //optionOpened = true;
                    return;
                }
            }

        }

        public void ShowInteractables(bool suppressAutoInteract = false) {
            //Debug.Log("InteractionPanelUI.ShowInteractables(" + suppressAutoInteract + ")");
            if (interactable != null) {
                //Debug.Log("InteractionPanelUI.ShowInteractables() interactable: " + interactable.MyName);
                ShowInteractablesCommon(interactable, suppressAutoInteract);
            } else {
                //Debug.Log("InteractionPanelUI.ShowInteractables() interactable IS NULL!!!");
            }
        }

        public void ShowInteractables(Interactable interactable) {
            //Debug.Log("InteractionPanelUI.ShowInteractables(" + interactable.name + ")");
            MyInteractable = interactable;
            ShowInteractablesCommon(this.interactable);
        }

        public void ClearButtons() {
            //Debug.Log("InteractionPanelUI.ClearButtons()");
            // clear the skill list so any skill left over from a previous time opening the window aren't shown
            foreach (InteractionPanelQuestScript qs in questScripts) {
                qs.transform.SetParent(null);
                Destroy(qs.gameObject);
            }
            questScripts.Clear();

            foreach (GameObject go in interactionPanelScripts) {
                InteractionPanelScript iPS = go.GetComponent<InteractionPanelScript>();
                go.transform.SetParent(null);
                Destroy(go);
            }
            interactionPanelScripts.Clear();
        }

        public override void RecieveClosedWindowNotification() {
            //Debug.Log("InteractionPanelUI.OnCloseWindow()");
            //ClearButtons();
            base.RecieveClosedWindowNotification();
            // clear this so window doesn't pop open again when it's closed
            MyInteractable = null;
        }

        public override void ReceiveOpenWindowNotification() {
            //Debug.Log("InteractionPanelUI.OnOpenWindow()");

            // this has to be done first, because the next line after could close the window and set the interactable to null
            if (PopupWindowManager.MyInstance != null) {
                PopupWindowManager.MyInstance.interactionWindow.SetWindowTitle(interactable.DisplayName);
            }

            ShowInteractables();

            // do this last or it could close the window before we set the title.  it just calls the onopenwindowhandler, so nothing that needs to be done before the 2 above lines
            base.ReceiveOpenWindowNotification();
        }
    }

}