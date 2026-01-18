using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class QuestGiverPanel : WindowPanel {

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
        private UIManager uIManager = null;
        private ObjectPooler objectPooler = null;
        private MessageFeedManager messageFeedManager = null;
        private PlayerManager playerManager = null;
        private DialogManagerClient dialogManagerClient = null;

        public QuestGiverQuestScript SelectedQuestGiverQuestScript { get => selectedQuestGiverQuestScript; set => selectedQuestGiverQuestScript = value; }
        //public Interactable MyInteractable { get => interactable; set => interactable = value; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            //acceptButton.Configure(systemGameManager);
            //completeButton.Configure(systemGameManager);

            questDetailsArea.Configure(systemGameManager);
            questDetailsArea.SetOwner(this);
            systemEventManager.OnPlayerUnitSpawn += HandlePlayerUnitSpawn;
            systemEventManager.OnPlayerUnitDespawn += HandlePlayerUnitDespawn;
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            uIManager = systemGameManager.UIManager;
            objectPooler = systemGameManager.ObjectPooler;
            messageFeedManager = uIManager.MessageFeedManager;
            playerManager = systemGameManager.PlayerManager;
            dialogManagerClient = systemGameManager.DialogManagerClient;
        }

        private void HandlePlayerUnitSpawn(UnitController unitController) {
            unitController.CharacterQuestLog.OnShowQuestGiverDescription += HandleShowQuestGiverDescription;
        }

        private void HandlePlayerUnitDespawn(UnitController unitController) {
            unitController.CharacterQuestLog.OnShowQuestGiverDescription -= HandleShowQuestGiverDescription;
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
                //Debug.Log("QuestGiverUI.ShowQuestsCommon() QUESTGIVER IS NULL!!!");
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
                qs.MyText.color = LevelEquations.GetTargetColor(playerManager.UnitController.MyCharacterStats.MyLevel, questNode.MyQuest.MyExperienceLevel);
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
                qs.MyText.color = LevelEquations.GetTargetColor(playerManager.UnitController.MyCharacterStats.MyLevel, questNode.MyQuest.MyExperienceLevel);
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
            //Debug.Log("QuestGiverUI.ShowQuests(" + (questGiver != null ? questGiver.ToString() : "null") + ")");
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
            if (newQuest.GetStatus(playerManager.UnitController) == "available" && playerManager.UnitController.CharacterQuestLog.HasQuest(newQuest.ResourceName) == false) {
                acceptButton.gameObject.SetActive(true);
                acceptButton.Button.enabled = true;
                completeButton.gameObject.SetActive(false);
                return;
            }

            //Debug.Log("questGiver: " + questGiver.ToString());
            if (newQuest.GetStatus(playerManager.UnitController) == "complete" && playerManager.UnitController.CharacterQuestLog.HasQuest(newQuest.ResourceName) == true && questGiver != null && questGiver.EndsQuest(newQuest.ResourceName)) {
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

        public void ShowDescription(Quest quest, IQuestGiver questGiver) {
            //Debug.Log("QuestGiverUI.ShowDescription(" + quest.DisplayName + ", " + (questGiver == null ? "null" : questGiver.ToString()) + ")");

            if (quest == null) {
                return;
            }
            currentQuest = quest;

            if (quest.HasOpeningDialog == true) {
                if (quest.OpeningDialog != null && quest.OpeningDialog.TurnedIn(playerManager.UnitController) == false) {
                    //Debug.Log("QuestGiverUI.ShowDescription(): opening dialog is not complete, showing dialog");
                    // FIX ME - that 0 should be the optionIndex of the interactableOption, but some quests can start from items.  There is no interactableOption in that case...
                    dialogManagerClient.SetQuestDialog(quest, interactable, questGiver.InteractableOptionComponent, 0, 0);
                    uIManager.dialogWindow.OpenWindow();
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

                if (questGiver != null) {
                    // notify a bag item so it can remove itself
                    //Debug.Log("QuestGiverUI.AcceptQuest() questgiver was not null");
                    questGiver.RequestAcceptQuest(playerManager.UnitController, currentQuest);
                } else {
                    //Debug.Log("QuestGiverUI.AcceptQuest() questgiver was null");
                }
                // don't need to do this since the window just closes anyway?
                //UpdateButtons(currentQuest);
                
            }
        }

        public void CompleteQuest() {
            //Debug.Log("QuestGiverUI.CompleteQuest()");
            if (!currentQuest.IsComplete(playerManager.UnitController)) {
                Debug.LogWarning("QuestGiverUI.CompleteQuest(): currentQuest is not complete, exiting!");
                return;
            }

            QuestRewardChoices questRewardChoices = new QuestRewardChoices();

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

            //int loopIndex = 0;

            // item rewards first in case not enough space in inventory
            // TO FIX: THIS CODE DOES NOT DEAL WITH PARTIAL STACKS AND WILL REQUEST ONE FULL SLOT FOR EVERY REWARD
            if (questDetailsArea.GetHighlightedItemRewardIcons().Count > 0) {
                if (playerManager.UnitController.CharacterInventoryManager.EmptySlotCount() < questDetailsArea.GetHighlightedItemRewardIcons().Count) {
                    messageFeedManager.WriteMessage("Not enough room in inventory!");
                    return;
                }

                questRewardChoices.itemRewardIndexes.AddRange(questDetailsArea.GetHighlightedItemRewardIcons().Keys);
                /*
                foreach (int rewardIndex in questDetailsArea.GetHighlightedItemRewardIcons().Keys) {
                    if (rewardIndex.Rewardable != null) {
                        rewardIndex.Rewardable.GiveReward(playerManager.UnitController);
                    }
                }
                */
            }

            //currentQuest.HandInItems(playerManager.UnitController);

            // faction rewards
            if (currentQuest.FactionRewards.Count > 0) {
                //Debug.Log("QuestGiverUI.CompleteQuest(): Giving Faction Rewards");
                questRewardChoices.factionRewardIndexes.AddRange(questDetailsArea.GetHighlightedFactionRewardIcons().Keys);
                /*
                foreach (RewardButton rewardButton in questDetailsArea.GetHighlightedFactionRewardIcons()) {
                    //Debug.Log("QuestGiverUI.CompleteQuest(): Giving Faction Rewards: got a reward button!");
                    if (rewardButton.Rewardable != null) {
                        rewardButton.Rewardable.GiveReward(playerManager.UnitController);
                    }
                }
                */
            }

            // ability rewards
            if (currentQuest.AbilityRewards.Count > 0) {
                //Debug.Log("QuestGiverUI.CompleteQuest(): Giving Ability Rewards");
                questRewardChoices.abilityRewardIndexes.AddRange(questDetailsArea.GetHighlightedAbilityRewardIcons().Keys);
                /*
                foreach (RewardButton rewardButton in questDetailsArea.GetHighlightedAbilityRewardIcons()) {
                    if (rewardButton.Rewardable != null) {
                        rewardButton.Rewardable.GiveReward(playerManager.UnitController);
                    }
                }
                */
            }

            // skill rewards
            if (currentQuest.SkillRewards.Count > 0) {
                //Debug.Log("QuestGiverUI.CompleteQuest(): Giving Skill Rewards");
                questRewardChoices.skillRewardIndexes.AddRange(questDetailsArea.GetHighlightedSkillRewardIcons().Keys);
                /*
                foreach (RewardButton rewardButton in questDetailsArea.GetHighlightedSkillRewardIcons()) {
                    if (rewardButton.Rewardable != null) {
                        rewardButton.Rewardable.GiveReward(playerManager.UnitController);
                    }
                }
                */
            }

            UpdateButtons(currentQuest);

            // DO THIS HERE OR TURNING THE QUEST RESULTING IN THIS WINDOW RE-OPENING WOULD JUST INSTA-CLOSE IT INSTEAD
            uIManager.questGiverWindow.CloseWindow();

            questGiver.RequestCompleteQuest(playerManager.UnitController, currentQuest, questRewardChoices);
            //playerManager.UnitController.CharacterQuestLog.TurnInQuest(currentQuest);

        }

        public override void ProcessOpenWindowNotification() {
            base.ProcessOpenWindowNotification();
            SetBackGroundColor(new Color32(0, 0, 0, (byte)(int)(PlayerPrefs.GetFloat("PopupWindowOpacity") * 255)));

            // clear first because open window handler could show a description
            ClearDescription();

            // reset button state to clear previous state
            DeactivateButtons();

            if (interactable != null) {
                uIManager.questGiverWindow.SetWindowTitle(interactable.DisplayName + " (Quests)");
            } else {
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