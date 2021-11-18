using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class QuestGiverUI : WindowContentController {

        private IQuestGiver questGiver;

        [SerializeField]
        private HighlightButton acceptButton = null;

        [SerializeField]
        private HighlightButton completeButton = null;

        [SerializeField]
        private GameObject leftPane = null;

        //[SerializeField]
        //private GameObject questPrefab = null;

        //[SerializeField]
        //private Transform questParent = null;

        //[SerializeField]
        //private TextMeshProUGUI questDescription = null;

        //[SerializeField]
        //private GameObject availableHeading = null;

        //[SerializeField]
        //private GameObject availableArea = null;

        //[SerializeField]
        //private GameObject inProgressHeading = null;

        //[SerializeField]
        //private GameObject inProgressArea = null;

        //[SerializeField]
        //private GameObject completeHeading = null;

        //[SerializeField]
        //private GameObject completeArea = null;

        //[SerializeField]
        //private GameObject hiddenHeading = null;

        //[SerializeField]
        //private GameObject hiddenArea = null;

        [SerializeField]
        private QuestDetailsArea questDetailsArea = null;

        private Interactable interactable;

        private List<QuestNode> questNodes = new List<QuestNode>();

        private List<QuestGiverQuestScript> questScripts = new List<QuestGiverQuestScript>();

        private QuestGiverQuestScript selectedQuestGiverQuestScript;

        private bool showAllQuests = false;

        //private string currentQuestName;

        private Quest currentQuest = null;

        // game manager references
        private QuestLog questLog = null;
        private UIManager uIManager = null;
        private ObjectPooler objectPooler = null;
        private MessageFeedManager messageFeedManager = null;
        private PlayerManager playerManager = null;
        private LogManager logManager = null;
        //private InventoryManager inventoryManager = null;
        private SystemItemManager systemItemManager = null;
        private CurrencyConverter currencyConverter = null;

        public QuestGiverQuestScript SelectedQuestGiverQuestScript { get => selectedQuestGiverQuestScript; set => selectedQuestGiverQuestScript = value; }
        //public Interactable MyInteractable { get => interactable; set => interactable = value; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            //acceptButton.Configure(systemGameManager);
            //completeButton.Configure(systemGameManager);

            questDetailsArea.Configure(systemGameManager);

            questLog.OnShowQuestGiverDescription += HandleShowQuestGiverDescription;
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            questLog = systemGameManager.QuestLog;
            uIManager = systemGameManager.UIManager;
            objectPooler = systemGameManager.ObjectPooler;
            messageFeedManager = uIManager.MessageFeedManager;
            playerManager = systemGameManager.PlayerManager;
            logManager = systemGameManager.LogManager;
            //inventoryManager = systemGameManager.InventoryManager;
            systemItemManager = systemGameManager.SystemItemManager;
            currencyConverter = systemGameManager.CurrencyConverter;
        }

        public void ToggleShowAllQuests(bool showAllQuests) {
            this.showAllQuests = showAllQuests;
            if (showAllQuests) {
                leftPane.gameObject.SetActive(true);
            } else {
                leftPane.gameObject.SetActive(false);
            }
        }

        public void DeactivateButtons() {
            //Debug.Log("QuestGiverUI.DeactivateButtons()");
            acceptButton.Button.enabled = false;
            completeButton.Button.enabled = false;
        }

        public void ShowQuestsCommon(IQuestGiver questGiver) {
            //Debug.Log("QuestGiverUI.ShowQuestsCommon()");
            if (questGiver == null) {
                Debug.Log("QuestGiverUI.ShowQuestsCommon() QUESTGIVER IS NULL!!!");
                return;
            }
            interactable = questGiver.Interactable;
            //Debug.Log("QuestGiverUI.ShowQuestsCommon(): about to clear quests");
            ClearQuests();
            //Debug.Log("QuestGiverUI.ShowQuestsCommon(): cleared quests");
            // TESTING FOR NOW, WE DON'T USE THE LEFT SIDE LIST ON QUESTGIVERUI ANYWAY, SO NO POINT DRAWING DESCRIPTION 2 OR 3 TIMES ON A WINDOW LOAD
            /*
            foreach (QuestNode questNode in questGiver.MyQuests) {
                //Debug.Log("QuestGiverUI.ShowQuestsCommon(): questNode.MyQuest.DisplayName: " + questNode.MyQuest.DisplayName);
                GameObject go = Instantiate(questPrefab, questParent);
                QuestGiverQuestScript qs = go.GetComponent<QuestGiverQuestScript>();
                qs.MyText.text = "[" + questNode.MyQuest.MyExperienceLevel + "] " + questNode.MyQuest.DisplayName;
                //Debug.Log("QuestGiverUI.ShowQuestsCommon(" + questGiver.name + "): " + questNode.MyQuest.MyTitle);
                qs.MyText.color = LevelEquations.GetTargetColor(playerManager.MyCharacter.MyCharacterStats.MyLevel, questNode.MyQuest.MyExperienceLevel);
                qs.MyQuest = questNode.MyQuest;
                questNode.MyGameObject = go;
                //quests.Add(go);
                questNodes.Add(questNode);

            }

            RefreshQuestDisplay();


            if (MySelectedQuestGiverQuestScript != null) {
                MySelectedQuestGiverQuestScript.Select();
            }
            */
        }

        public void RefreshQuestDisplay() {
            //Debug.Log("QuestGiverUI.RefreshQuestDisplay(): quest count: " + questGiver.MyQuests.Length.ToString());
            // DO NOTHING FOR NOW, SINCE WE DON'T USE THIS PANEL AT ALL CURRENTLY
            /*
            QuestGiverQuestScript firstAvailableQuest = null;
            QuestGiverQuestScript firstInProgressQuest = null;
            foreach (QuestNode questNode in questGiver.MyQuests) {
                QuestGiverQuestScript qs = questNode.MyGameObject.GetComponent<QuestGiverQuestScript>();
                qs.MyText.text = "[" + questNode.MyQuest.MyExperienceLevel + "] " + questNode.MyQuest.DisplayName;
                qs.MyText.color = LevelEquations.GetTargetColor(playerManager.MyCharacter.MyCharacterStats.MyLevel, questNode.MyQuest.MyExperienceLevel);
                //Debug.Log("Evaluating quest: " + qs.MyQuest.MyTitle + "; turnedin: " + qs.MyQuest.TurnedIn.ToString());
                string questStatus = qs.MyQuest.GetStatus();
                if (questStatus == "completed" && questNode.MyEndQuest == true) {
                    questNode.MyGameObject.transform.SetParent(completeArea.transform);
                } else if (questStatus == "complete" && questNode.MyEndQuest == true) { 
                    qs.MyText.text += " (Complete)";
                    questNode.MyGameObject.transform.SetParent(inProgressArea.transform);
                    if (firstInProgressQuest == null) {
                        firstInProgressQuest = qs;
                    }
                } else if (questStatus == "inprogress" && questNode.MyEndQuest == true) {
                    questNode.MyGameObject.transform.SetParent(inProgressArea.transform);
                    if (firstInProgressQuest == null) {
                        firstInProgressQuest = qs;
                    }
                } else if (questNode.MyStartQuest == true && questStatus == "available") {
                    questNode.MyGameObject.transform.SetParent(availableArea.transform);
                    if (firstAvailableQuest == null) {
                        firstAvailableQuest = qs;
                    }
                } else {
                    questNode.MyGameObject.transform.SetParent(hiddenArea.transform);
                    if (MySelectedQuestGiverQuestScript == questNode.MyGameObject.GetComponent<QuestGiverQuestScript>()) {
                        // clear description and clear selected quest if the selected quest was this one because it's invisible now
                        ClearDescription();
                        MySelectedQuestGiverQuestScript.DeSelect() ;
                    }
                }
            }

            // enable or disable headers based on 
            if (availableArea.transform.childCount == 0) {
                availableHeading.SetActive(false);
            } else {
                availableHeading.SetActive(true);
                if (MySelectedQuestGiverQuestScript == null && firstAvailableQuest != null) {
                    firstAvailableQuest.Select();
                }
            }
            //Debug.Log("the inprogress area child count is " + inProgressArea.transform.childCount.ToString());
            if (inProgressArea.transform.childCount == 0) {
                inProgressHeading.SetActive(false);
            } else {
                inProgressHeading.SetActive(true);
                if (MySelectedQuestGiverQuestScript == null && firstInProgressQuest != null) {
                    firstInProgressQuest.Select();
                }
            }
            if (completeArea.transform.childCount == 0) {
                completeHeading.SetActive(false);
            } else {
                completeHeading.SetActive(true);
            }
            */
        }

        public void ShowQuests() {
            //Debug.Log("QuestGiverUI.ShowQuests()");
            ShowQuestsCommon(questGiver);
        }

        public void ShowQuests(IQuestGiver questGiver) {
            Debug.Log("QuestGiverUI.ShowQuests(" + (questGiver != null ? questGiver.ToString() : "null") + ")");
            SetQuestGiver(questGiver);
            ShowQuestsCommon(this.questGiver);
        }

        /*
         * not currently in use because questgiverUI gets quests from interaction window now
        public void UpdateSelected() {
            if (selectedQuestGiverQuestScript != null) {
                ShowDescription(selectedQuestGiverQuestScript.MyQuest);
            }
        }
        */

        private void UpdateButtons(Quest newQuest) {
            //Debug.Log("QuestGiverUI.UpdateButtons(" + newQuest.DisplayName + "). iscomplete: " + newQuest.IsComplete + ". HasQuest: " + questLog.HasQuest(newQuest.DisplayName));
            if (newQuest.AllowRawComplete == true && newQuest.Steps.Count == 0) {
                acceptButton.gameObject.SetActive(false);
                completeButton.gameObject.SetActive(true);
                completeButton.Button.enabled = true;
                return;
            }
            if (newQuest.GetStatus() == "available" && questLog.HasQuest(newQuest.DisplayName) == false) {
                acceptButton.gameObject.SetActive(true);
                acceptButton.Button.enabled = true;
                completeButton.gameObject.SetActive(false);
                return;
            }

            //Debug.Log("questGiver: " + questGiver.ToString());
            if (newQuest.GetStatus() == "complete" && questLog.HasQuest(newQuest.DisplayName) == true && questGiver != null && questGiver.EndsQuest(newQuest.DisplayName)) {
                completeButton.gameObject.SetActive(true);
                completeButton.Button.enabled = true;
                acceptButton.gameObject.SetActive(false);
                return;
            }

            // default state, not complete or available, no buttons to show
            acceptButton.gameObject.SetActive(false);
            completeButton.gameObject.SetActive(false);

        }

        public void SetQuestGiver(IQuestGiver questGiver) {
            if (questGiver != null) {
                this.questGiver = questGiver;
                interactable = questGiver.Interactable;
            }
        }

        public void HandleShowQuestGiverDescription(Quest quest, IQuestGiver questGiver) {
            SetQuestGiver(questGiver);
            if (uIManager.questGiverWindow.IsOpen == false) {
                uIManager.questGiverWindow.OpenWindow();
            }
            ShowDescription(quest, questGiver);
        }

        public void ShowDescription(Quest quest, IQuestGiver questGiver = null) {
            //Debug.Log("QuestGiverUI.ShowDescription(" + quest.DisplayName + ", " + (questGiver == null ? "null" : questGiver.ToString()) + ")");

            if (quest == null) {
                return;
            }
            currentQuest = quest;

            if (quest.HasOpeningDialog == true) {
                if (quest.OpeningDialog != null && quest.OpeningDialog.TurnedIn == false) {
                    //Debug.Log("QuestGiverUI.ShowDescription(): opening dialog is not complete, showing dialog");
                    (uIManager.dialogWindow.CloseableWindowContents as DialogPanelController).Setup(quest, interactable);
                    //Debug.Log("QuestGiverUI.ShowDescription(): about to close window because of dialog");
                    if (uIManager.questGiverWindow.IsOpen) {
                        uIManager.questGiverWindow.CloseWindow();
                    }
                    return;
                }
            }
            /*
            if (!uIManager.questGiverWindow.IsOpen) {
                uIManager.questGiverWindow.OpenWindow();
                //ShowDescription(quest);
                return;
            }
            */
            /*
            if (SelectedQuestGiverQuestScript == null || SelectedQuestGiverQuestScript.MyQuest != quest) {
                foreach (QuestGiverQuestScript questScript in questScripts) {
                    if (questScript.MyQuest == quest) {
                        questScript.RawSelect();
                    }
                }
            }
            */

            ClearDescription();

            questDetailsArea.gameObject.SetActive(true);
            questDetailsArea.ShowDescription(quest);

            UpdateButtons(quest);

            uINavigationControllers[0].UpdateNavigationList();
            FocusCurrentButton();
        }

        public void ClearDescription() {
            //Debug.Log("QuestGiverUI.ClearDescription()");

            questDetailsArea.gameObject.SetActive(false);
        }

        public void CheckCompletion() {
            //Debug.Log("quest log checking completion");
            /*
            foreach (QuestGiverQuestScript qs in questScripts) {
                qs.IsComplete();
            }
            */
        }

        public void ClearQuests() {
            // clear the quest list so any quests left over from a previous time opening the window aren't shown
            //Debug.Log("QuestGiverUI.ClearQuests()");
            foreach (QuestNode questNode in questNodes) {
                if (questNode.GameObject != null) {
                    //Debug.Log("The questnode has a gameobject we need to clear");
                    questNode.GameObject.transform.SetParent(null);
                    objectPooler.ReturnObjectToPool(questNode.GameObject);
                    questNode.GameObject = null;
                }
            }
            // TRY THIS FOR FIX.  OTHERWISE REFERENCE CAN REMAIN TO DESTROYED QUESTSCRIPT
            SelectedQuestGiverQuestScript = null;
            /*
            for (int i = 0; i < quests.Count; i++) {
                GameObject go = quests[i];
                go.transform.SetParent(null);
                Destroy(go);
                go = null;
            }
            */

            //quests.Clear();
        }

        public override void ReceiveClosedWindowNotification() {
            //Debug.Log("QuestGiverUI.OnCloseWindow()");
            base.ReceiveClosedWindowNotification();
            SelectedQuestGiverQuestScript = null;
        }

        public void AcceptQuest() {
            //Debug.Log("QuestGiverUI.AcceptQuest()");
            if (currentQuest != null) {
                // DO THIS HERE SO IT DOESN'T INSTA-CLOSE ANY AUTO-POPUP BACK TO HERE ON ACCEPT QUEST CAUSING STATUS CHANGE
                uIManager.questGiverWindow.CloseWindow();

                questLog.AcceptQuest(currentQuest);

                if (questGiver != null) {
                    // notify a bag item so it can remove itself
                    //Debug.Log("QuestGiverUI.AcceptQuest() questgiver was not null");
                    questGiver.HandleAcceptQuest();
                } else {
                    //Debug.Log("QuestGiverUI.AcceptQuest() questgiver was null");
                }
                UpdateButtons(currentQuest);
                if (interactable != null) {
                    /*
                    if (interactable.CheckForInteractableObjectives(currentQuestName)) {
                        uIManager.questGiverWindow.CloseWindow();
                    }
                    */
                }
                /*
                if (SelectedQuestGiverQuestScript != null) {
                    SelectedQuestGiverQuestScript.DeSelect();
                }
                */

                // disabled this stuff for now since only a single pane is being used
                //RefreshQuestDisplay();
                //if (availableArea.transform.childCount == 0 && inProgressArea.transform.childCount == 0) {
                //Debug.Log("Nothing to show, closing window for smoother UI experience");
                //uIManager.questGiverWindow.CloseWindow();
                //}
                // do it anyway for now since 

                //ShowQuests();
                //questGiver.UpdateQuestStatus();
            }
        }

        public void CompleteQuest() {
            //Debug.Log("QuestGiverUI.CompleteQuest()");
            if (!currentQuest.IsComplete) {
                Debug.Log("QuestGiverUI.CompleteQuest(): currentQuest is not complete, exiting!");
                return;
            }

            // DO THIS NOW SO NO NULL REFERENCES WHEN IT GETS DESELECTED DURING THIS PROCESS
            //Quest questToComplete = systemDataFactory.GetResource<Quest>(currentQuestName);

            //questDetailsArea.myreward

            bool itemCountMatches = false;
            bool abilityCountMatches = false;
            bool factionCountMatches = false;
            if (currentQuest.ItemRewards.Count == 0 || currentQuest.MaxItemRewards == 0 || currentQuest.MaxItemRewards == questDetailsArea.GetHighlightedItemRewardIcons().Count) {
                itemCountMatches = true;
            }
            if (currentQuest.FactionRewards.Count == 0 || currentQuest.MaxFactionRewards == 0 || currentQuest.MaxFactionRewards == questDetailsArea.GetHighlightedFactionRewardIcons().Count) {
                factionCountMatches = true;
            }

            if (currentQuest.AbilityRewards.Count == 0 || currentQuest.MaxAbilityRewards == 0 || currentQuest.MaxAbilityRewards == questDetailsArea.GetHighlightedAbilityRewardIcons().Count) {
                abilityCountMatches = true;
            }

            if (!itemCountMatches || !abilityCountMatches || !factionCountMatches) {
                messageFeedManager.WriteMessage("You must choose rewards before turning in this quest");
                return;
            }

            // currency rewards
            List<CurrencyNode> currencyNodes = currentQuest.GetCurrencyReward();
            foreach (CurrencyNode currencyNode in currencyNodes) {
                playerManager.MyCharacter.CharacterCurrencyManager.AddCurrency(currencyNode.currency, currencyNode.Amount);
                List<CurrencyNode> tmpCurrencyNode = new List<CurrencyNode>();
                tmpCurrencyNode.Add(currencyNode);
                logManager.WriteSystemMessage("Gained " + currencyConverter.RecalculateValues(tmpCurrencyNode, false).Value.Replace("\n", ", "));
            }

            // item rewards first in case not enough space in inventory
            // TO FIX: THIS CODE DOES NOT DEAL WITH PARTIAL STACKS AND WILL REQUEST ONE FULL SLOT FOR EVERY REWARD
            if (questDetailsArea.GetHighlightedItemRewardIcons().Count > 0) {
                if (playerManager.MyCharacter.CharacterInventoryManager.EmptySlotCount() < questDetailsArea.GetHighlightedItemRewardIcons().Count) {
                    messageFeedManager.WriteMessage("Not enough room in inventory!");
                    return;
                }
                foreach (RewardButton rewardButton in questDetailsArea.GetHighlightedItemRewardIcons()) {
                    if (rewardButton.Describable != null && rewardButton.Describable.DisplayName != null && rewardButton.Describable.DisplayName != string.Empty) {
                        Item newItem = systemItemManager.GetNewResource(rewardButton.Describable.DisplayName);
                        if (newItem != null) {
                            //Debug.Log("RewardButton.CompleteQuest(): newItem is not null, adding to inventory");
                            newItem.DropLevel = playerManager.MyCharacter.CharacterStats.Level;
                            playerManager.MyCharacter.CharacterInventoryManager.AddItem(newItem, false);
                        }
                    }
                }
            }

            currentQuest.HandInItems();

            // faction rewards
            if (currentQuest.FactionRewards.Count > 0) {
                //Debug.Log("QuestGiverUI.CompleteQuest(): Giving Faction Rewards");
                foreach (RewardButton rewardButton in questDetailsArea.GetHighlightedFactionRewardIcons()) {
                    //Debug.Log("QuestGiverUI.CompleteQuest(): Giving Faction Rewards: got a reward button!");
                    if (rewardButton.Describable != null && rewardButton.Describable.DisplayName != null && rewardButton.Describable.DisplayName != string.Empty) {
                        playerManager.MyCharacter.CharacterFactionManager.AddReputation((rewardButton.Describable as FactionNode).Faction, (rewardButton.Describable as FactionNode).reputationAmount);
                    }
                }
            }

            // ability rewards
            if (currentQuest.AbilityRewards.Count > 0) {
                //Debug.Log("QuestGiverUI.CompleteQuest(): Giving Ability Rewards");
                foreach (RewardButton rewardButton in questDetailsArea.GetHighlightedAbilityRewardIcons()) {
                    if (rewardButton.Describable != null && rewardButton.Describable.DisplayName != null && rewardButton.Describable.DisplayName != string.Empty) {
                        playerManager.MyCharacter.CharacterAbilityManager.LearnAbility(rewardButton.Describable as BaseAbility);
                    }
                }
            }

            // skill rewards
            if (currentQuest.SkillRewards.Count > 0) {
                //Debug.Log("QuestGiverUI.CompleteQuest(): Giving Skill Rewards");
                foreach (RewardButton rewardButton in questDetailsArea.GetHighlightedSkillRewardIcons()) {
                    if (rewardButton.Describable != null && rewardButton.Describable.DisplayName != null && rewardButton.Describable.DisplayName != string.Empty) {
                        playerManager.MyCharacter.CharacterSkillManager.LearnSkill(rewardButton.Describable as Skill);
                    }
                }
            }

            // xp reward
            playerManager.MyCharacter.CharacterStats.GainXP(LevelEquations.GetXPAmountForQuest(playerManager.MyCharacter.CharacterStats.Level, currentQuest, systemConfigurationManager));

            UpdateButtons(currentQuest);

            // DO THIS HERE OR TURNING THE QUEST RESULTING IN THIS WINDOW RE-OPENING WOULD JUST INSTA-CLOSE IT INSTEAD
            uIManager.questGiverWindow.CloseWindow();

            questLog.TurnInQuest(currentQuest);

            // do this last
            // DO THIS AT THE END OR THERE WILL BE NO SELECTED QUESTGIVERQUESTSCRIPT
            if (questGiver != null) {
                //Debug.Log("QuestGiverUI.CompleteQuest(): questGiver is not null");
                // MUST BE DONE IN CASE WINDOW WAS OPEN INBETWEEN SCENES BY ACCIDENT
                //Debug.Log("QuestGiverUI.CompleteQuest() Updating questGiver queststatus");
                questGiver.UpdateQuestStatus();
                questGiver.HandleCompleteQuest();
            } else {
                Debug.Log("QuestGiverUI.CompleteQuest(): questGiver is null!");
            }

            /*
            if (SelectedQuestGiverQuestScript != null) {
                SelectedQuestGiverQuestScript.DeSelect();
            }
            */

        }

        public override void ReceiveOpenWindowNotification() {
            base.ReceiveOpenWindowNotification();
            SetBackGroundColor(new Color32(0, 0, 0, (byte)(int)(PlayerPrefs.GetFloat("PopupWindowOpacity") * 255)));

            // clear first because open window handler could show a description
            ClearDescription();

            // reset button state to clear previous state
            DeactivateButtons();

            if (interactable != null) {
                uIManager.questGiverWindow.SetWindowTitle(interactable.DisplayName + " (Quests)");
            } else {
                //Debug.Log("QuestGiverUI.ReceiveOpenWindowNotification() interactable is null");
                // interactable is null if this quest is started from an item in the inventory
                // in that case it doesn't make sense to show a questGiver name
                uIManager.questGiverWindow.SetWindowTitle("");
            }

        }

        public void OnDisable() {
            //Debug.Log("QuestGiverUI.OnDisable()");
            // CLOSE THIS BEFORE LEVEL UNLOADS
            if (SystemGameManager.IsShuttingDown) {
                return;
            }
            if (uIManager != null && uIManager.questGiverWindow != null) {
                uIManager.questGiverWindow.CloseWindow();
            }
        }

    }

}