using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestGiverUI : WindowContentController {

    #region Singleton
    private static QuestGiverUI instance;

    public static QuestGiverUI MyInstance
    {
        get
        {
            if (instance == null) {
                instance = FindObjectOfType<QuestGiverUI>();
            }

            return instance;
        }
    }

    #endregion

    private IQuestGiver questGiver;

    [SerializeField]
    private GameObject acceptButton;

    [SerializeField]
    private GameObject completeButton;

    [SerializeField]
    private GameObject leftPane;

    [SerializeField]
    private GameObject questPrefab;

    [SerializeField]
    private Transform questParent;

    [SerializeField]
    private Text questDescription;

    [SerializeField]
    private GameObject availableHeading;

    [SerializeField]
    private GameObject availableArea;

    [SerializeField]
    private GameObject inProgressHeading;

    [SerializeField]
    private GameObject inProgressArea;

    [SerializeField]
    private GameObject completeHeading;

    [SerializeField]
    private GameObject completeArea;

    [SerializeField]
    private GameObject hiddenHeading;

    [SerializeField]
    private GameObject hiddenArea;

    [SerializeField]
    private QuestDetailsArea questDetailsArea;

    private Interactable interactable;

    private List<QuestNode> questNodes = new List<QuestNode>();

    private List<QuestGiverQuestScript> questScripts = new List<QuestGiverQuestScript>();

    private QuestGiverQuestScript selectedQuestGiverQuestScript;

    private bool showAllQuests = false;

    private string currentQuestName;

    public override event System.Action<ICloseableWindowContents> OnOpenWindowHandler = delegate { };

    public QuestGiverQuestScript MySelectedQuestGiverQuestScript { get => selectedQuestGiverQuestScript; set => selectedQuestGiverQuestScript = value; }
    public Interactable MyInteractable { get => interactable; set => interactable = value; }
    public IQuestGiver MyQuestGiver { get => questGiver;
        set {
            questGiver = value;
            MyInteractable = questGiver.MyInteractable;
        } 
    }

    private void Start() {
        DeactivateButtons();
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
        acceptButton.GetComponent<Button>().enabled = false;
        completeButton.GetComponent<Button>().enabled = false;
    }

    public void ShowQuestsCommon(IQuestGiver questGiver) {
        Debug.Log("QuestGiverUI.ShowQuestsCommon()");
        if (questGiver == null) {
            Debug.Log("QuestGiverUI.ShowQuestsCommon() QUESTGIVER IS NULL!!!");
            return;
        }
        interactable = questGiver.MyInteractable;
        //Debug.Log("QuestGiverUI.ShowQuestsCommon(): about to clear quests");
        ClearQuests();
        //Debug.Log("QuestGiverUI.ShowQuestsCommon(): cleared quests");
        // TESTING FOR NOW, WE DON'T USE THE LEFT SIDE LIST ON QUESTGIVERUI ANYWAY, SO NO POINT DRAWING DESCRIPTION 2 OR 3 TIMES ON A WINDOW LOAD
        /*
        foreach (QuestNode questNode in questGiver.MyQuests) {
            //Debug.Log("QuestGiverUI.ShowQuestsCommon(): questNode.MyQuest.MyName: " + questNode.MyQuest.MyName);
            GameObject go = Instantiate(questPrefab, questParent);
            QuestGiverQuestScript qs = go.GetComponent<QuestGiverQuestScript>();
            qs.MyText.text = "[" + questNode.MyQuest.MyExperienceLevel + "] " + questNode.MyQuest.MyName;
            //Debug.Log("QuestGiverUI.ShowQuestsCommon(" + questGiver.name + "): " + questNode.MyQuest.MyTitle);
            qs.MyText.color = LevelEquations.GetTargetColor(PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyLevel, questNode.MyQuest.MyExperienceLevel);
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
        // TESTING DO NOTHING FOR NOW, SINCE WE DON'T USE THIS PANEL AT ALL CURRENTLY
        /*
        QuestGiverQuestScript firstAvailableQuest = null;
        QuestGiverQuestScript firstInProgressQuest = null;
        foreach (QuestNode questNode in questGiver.MyQuests) {
            QuestGiverQuestScript qs = questNode.MyGameObject.GetComponent<QuestGiverQuestScript>();
            qs.MyText.text = "[" + questNode.MyQuest.MyExperienceLevel + "] " + questNode.MyQuest.MyName;
            qs.MyText.color = LevelEquations.GetTargetColor(PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyLevel, questNode.MyQuest.MyExperienceLevel);
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
        this.questGiver = questGiver;
        ShowQuestsCommon(this.questGiver);
    }

    public void UpdateSelected() {
        if (selectedQuestGiverQuestScript != null) {
            ShowDescription(selectedQuestGiverQuestScript.MyQuest);
        }
    }

    private void UpdateButtons(string questName) {
        //Debug.Log("QuestGiverUI.UpdateButtons(" + quest.name + "). iscomplete: " + quest.IsComplete + ". HasQuest: " + QuestLog.MyInstance.HasQuest(quest));
        Quest quest = SystemQuestManager.MyInstance.GetResource(questName);
        if (quest.MyAllowRawComplete == true) {
            acceptButton.gameObject.SetActive(false);
            completeButton.gameObject.SetActive(true);
            completeButton.GetComponent<Button>().enabled = true;
            return;
        }
        if (quest.GetStatus() == "available" && QuestLog.MyInstance.HasQuest(quest.MyName) == false) {
            acceptButton.gameObject.SetActive(true);
            acceptButton.GetComponent<Button>().enabled = true;
            completeButton.gameObject.SetActive(false);
            return;
        }

        if (quest.GetStatus() == "complete" && QuestLog.MyInstance.HasQuest(quest.MyName) == true) {
            completeButton.gameObject.SetActive(true);
            completeButton.GetComponent<Button>().enabled = true;
            acceptButton.gameObject.SetActive(false);
            return;
        }

        // default state, not complete or available, no buttons to show
        acceptButton.gameObject.SetActive(false);
        completeButton.gameObject.SetActive(false);

    }

    public void ShowDescription(Quest quest) {
        //Debug.Log("QuestGiverUI.ShowDescription()");

        if (quest == null) {
            //Debug.Log("QuestGiverUI.ShowDescription(): quest is null, doing nothing");
            return;
        }

        currentQuestName = quest.MyName;

        if (quest.MyHasOpeningDialog == true && SystemDialogManager.MyInstance.GetResource(quest.MyName).TurnedIn == false) {
            //Debug.Log("QuestGiverUI.ShowDescription(): opening dialog is not complete, showing dialog");
            (PopupWindowManager.MyInstance.dialogWindow.MyCloseableWindowContents as DialogPanelController).Setup(quest, interactable);
            //Debug.Log("QuestGiverUI.ShowDescription(): about to close window because of dialog");
            if (PopupWindowManager.MyInstance.questGiverWindow.IsOpen) {
                PopupWindowManager.MyInstance.questGiverWindow.CloseWindow();
            }
            return;
        }
        
        // TESTING CODE
        
        if (!PopupWindowManager.MyInstance.questGiverWindow.IsOpen) {
            PopupWindowManager.MyInstance.questGiverWindow.OpenWindow();
            //ShowDescription(quest);
            return;
        }
        

        if (MySelectedQuestGiverQuestScript == null || MySelectedQuestGiverQuestScript.MyQuest != quest) {
            foreach (QuestGiverQuestScript questScript in questScripts) {
                if (questScript.MyQuest == quest) {
                    questScript.RawSelect();
                }
            }
        }

        ClearDescription();

        questDetailsArea.gameObject.SetActive(true);
        questDetailsArea.ShowDescription(quest);

        UpdateButtons(quest.MyName);

    }

    public void ClearDescription() {
        //Debug.Log("QuestGiverUI.ClearDescription()");

        questDetailsArea.gameObject.SetActive(false);
    }

    public void CheckCompletion() {
        //Debug.Log("quest log checking completion");
        foreach (QuestGiverQuestScript qs in questScripts) {
            qs.IsComplete();
        }
    }

    public void ClearQuests() {
        // clear the quest list so any quests left over from a previous time opening the window aren't shown
        //Debug.Log("QuestGiverUI.ClearQuests()");
        foreach (QuestNode questNode in questNodes) {
            if (questNode.MyGameObject != null) {
                //Debug.Log("The questnode has a gameobject we need to clear");
                questNode.MyGameObject.transform.SetParent(null);
                Destroy(questNode.MyGameObject);
                questNode.MyGameObject = null;
            }
        }
        // TRY THIS FOR FIX.  OTHERWISE REFERENCE CAN REMAIN TO DESTROYED QUESTSCRIPT
        MySelectedQuestGiverQuestScript = null;
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

    public override void OnCloseWindow() {
        //Debug.Log("QuestGiverUI.OnCloseWindow()");
        base.OnCloseWindow();
        DeactivateButtons();
        MySelectedQuestGiverQuestScript = null;
    }

    public void AcceptQuest() {
        Debug.Log("QuestGiverUI.AcceptQuest()");
        if (currentQuestName != null && currentQuestName != string.Empty) {
            //if (MySelectedQuestGiverQuestScript != null && MySelectedQuestGiverQuestScript.MyQuest != null) {
            // TESTING, DO THIS HERE SO IT DOESN'T INSTA-CLOSE ANY AUTO-POPUP BACK TO HERE ON ACCEPT QUEST CAUSING STATUS CHANGE
            PopupWindowManager.MyInstance.questGiverWindow.CloseWindow();

            QuestLog.MyInstance.AcceptQuest(currentQuestName);

            if (MyQuestGiver != null) {
                // notify a bag item so it can remove itself
                MyQuestGiver.HandleAcceptQuest();
            }
            UpdateButtons(currentQuestName);
            if (interactable != null) {
                /*
                if (interactable.CheckForInteractableObjectives(currentQuestName)) {
                    PopupWindowManager.MyInstance.questGiverWindow.CloseWindow();
                }
                */
            }
            if (MySelectedQuestGiverQuestScript != null) {
                MySelectedQuestGiverQuestScript.DeSelect();
            }

            // disabled this stuff for now since only a single pane is being used
            //RefreshQuestDisplay();
            //if (availableArea.transform.childCount == 0 && inProgressArea.transform.childCount == 0) {
                //Debug.Log("Nothing to show, closing window for smoother UI experience");
                //PopupWindowManager.MyInstance.questGiverWindow.CloseWindow();
            //}
            // do it anyway for now since 

            //ShowQuests();
            //questGiver.UpdateQuestStatus();
        }
    }

    public void CompleteQuest() {
        //Debug.Log("QuestGiverUI.CompleteQuest()");
        if (!SystemQuestManager.MyInstance.GetResource(currentQuestName).IsComplete) {
            return;
        }

        // DO THIS NOW SO NO NULL REFERENCES WHEN IT GETS DESELECTED DURING THIS PROCESS
        Quest questToComplete = SystemQuestManager.MyInstance.GetResource(currentQuestName);

        //questDetailsArea.myreward

        bool itemCountMatches = false;
        bool abilityCountMatches = false;
        bool factionCountMatches = false;
        if (questToComplete.MyItemRewards.Count == 0 || questToComplete.MyMaxItemRewards == 0 || questToComplete.MyMaxItemRewards == questDetailsArea.GetHighlightedItemRewardIcons().Count) {
            itemCountMatches = true;
        }
        if (questToComplete.MyFactionRewards.Count == 0 || questToComplete.MyMaxFactionRewards == 0 || questToComplete.MyMaxFactionRewards == questDetailsArea.GetHighlightedFactionRewardIcons().Count) {
            factionCountMatches = true;
        }

        if (questToComplete.MyAbilityRewards.Count == 0 || questToComplete.MyMaxAbilityRewards == 0 || questToComplete.MyMaxAbilityRewards == questDetailsArea.GetHighlightedAbilityRewardIcons().Count) {
            abilityCountMatches = true;
        }

        if (!itemCountMatches || !abilityCountMatches || !factionCountMatches) {
            MessageFeedManager.MyInstance.WriteMessage("You must choose rewards before turning in this quest(!itemCountMatches || !abilityCountMatches || !factionCountMatches): " + !itemCountMatches + !abilityCountMatches + !factionCountMatches);
            return;
        }

        // item rewards first in case not enough space in inventory
        // TO FIX: THIS CODE DOES NOT DEAL WITH PARTIAL STACKS AND WILL REQUEST ONE FULL SLOT FOR EVERY REWARD
        if (questDetailsArea.GetHighlightedItemRewardIcons().Count > 0) {
            if (InventoryManager.MyInstance.MyEmptySlotCount < questDetailsArea.GetHighlightedItemRewardIcons().Count) {
                Debug.Log("Not enough room in inventory!");
                return;
            }
            foreach (RewardButton rewardButton in questDetailsArea.GetHighlightedItemRewardIcons()) {
                //Debug.Log("rewardButton.MyDescribable: " + rewardButton.MyDescribable.MyName);
                Item newItem = SystemItemManager.MyInstance.GetNewResource(rewardButton.MyDescribable.MyName);
                if (newItem != null) {
                    //Debug.Log("RewardButton.CompleteQuest(): newItem is not null, adding to inventory");
                    InventoryManager.MyInstance.AddItem(newItem);
                }
            }
        }

        foreach (CollectObjective o in questToComplete.MyCollectObjectives) {
            if (questToComplete.MyTurnInItems == true) {
                o.Complete();
            }
        }

        // faction rewards
        if (questToComplete.MyFactionRewards.Count > 0) {
            Debug.Log("QuestGiverUI.CompleteQuest(): Giving Faction Rewards");
            foreach (RewardButton rewardButton in questDetailsArea.GetHighlightedFactionRewardIcons()) {
                Debug.Log("QuestGiverUI.CompleteQuest(): Giving Faction Rewards: got a reward button!");
                PlayerManager.MyInstance.MyCharacter.MyPlayerFactionManager.AddReputation((rewardButton.MyDescribable as FactionNode).faction, (rewardButton.MyDescribable as FactionNode).reputationAmount);
            }
        }

        // ability rewards
        if (questToComplete.MyAbilityRewards.Count > 0) {
            //Debug.Log("QuestGiverUI.CompleteQuest(): Giving Ability Rewards");
            foreach (RewardButton rewardButton in questDetailsArea.GetHighlightedAbilityRewardIcons()) {
                PlayerManager.MyInstance.MyCharacter.MyCharacterAbilityManager.LearnAbility(rewardButton.MyDescribable.MyName);
            }
        }

        // skill rewards
        if (questToComplete.MySkillRewards.Count > 0) {
            //Debug.Log("QuestGiverUI.CompleteQuest(): Giving Skill Rewards");
            foreach (RewardButton rewardButton in questDetailsArea.GetHighlightedSkillRewardIcons()) {
                PlayerManager.MyInstance.MyCharacter.MyCharacterSkillManager.LearnSkill(rewardButton.MyDescribable.MyName);
            }
        }

        // xp reward
        PlayerManager.MyInstance.MyCharacter.MyCharacterStats.GainXP(LevelEquations.GetXPAmountForQuest(PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyLevel, questToComplete));

        UpdateButtons(questToComplete.MyName);

        // TESTING DO THIS HERE OR TURNING THE QUEST RESULTING IN THIS WINDOW RE-OPENING WOULD JUST INSTA-CLOSE IT INSTEAD
        PopupWindowManager.MyInstance.questGiverWindow.CloseWindow();

        QuestLog.MyInstance.TurnInQuest(questToComplete.MyName);

        // do this last
        // DO THIS AT THE END OR THERE WILL BE NO SELECTED QUESTGIVERQUESTSCRIPT
        //foreach (QuestNode questNode in questGiver.MyQuests) {
        //if (questNode.MyQuest == MySelectedQuestGiverQuestScript.MyQuest) {
        //questNode.MyQuest.TurnedIn = true;
        //}
        //}
        if (questGiver != null) {
            // MUST BE DONE IN CASE WINDOW WAS OPEN INBETWEEN SCENES BY ACCIDENT
            questGiver.UpdateQuestStatus();
            //if (MyQuestGiver != null) {
                // notify a bag item so it can remove itself
                MyQuestGiver.HandleCompleteQuest();
            //}

        }
        if (MySelectedQuestGiverQuestScript != null) {
            MySelectedQuestGiverQuestScript.DeSelect();
        }
        //RefreshQuestDisplay();
        //ShowQuests();
        /*
        if (showAllQuests == true) {
            if (availableArea.transform.childCount == 0 && inProgressArea.transform.childCount == 0) {
                Debug.Log("Nothing to show, closing window for smoother UI experience");
                PopupWindowManager.MyInstance.questGiverWindow.CloseWindow();
            }
        } else {
        */
            //Debug.Log("QuestGiverUI.CompleteQuest(): ");
            //PopupWindowManager.MyInstance.questGiverWindow.CloseWindow();
            /*
            if (Quest.GetAvailableQuests(questGiver.MyQuests).Count > 0) {
                // TESTING: MAYBE QUESTGIVER INSTEAD OF BASE INTERACTABLE?
                questGiver.Interact(PlayerManager.MyInstance.MyCharacter.MyCharacterUnit);
            }
            */
        //}

    }

    public override void OnOpenWindow() {
        //Debug.Log("QuestGiverUI.OnOpenWindow()");
        // clear first because open window handler could show a description
        ClearDescription();
        if (interactable != null) {
            PopupWindowManager.MyInstance.questGiverWindow.SetWindowTitle(interactable.MyName + " (Quests)");
        }
        OnOpenWindowHandler(this);
    }

    public void OnDisable() {
        //Debug.Log("QuestGiverUI.OnDisable()");
        // TESTING, CLOSE THIS BEFORE LEVEL UNLOADS
        if (PopupWindowManager.MyInstance != null && PopupWindowManager.MyInstance.questGiverWindow != null) {
            PopupWindowManager.MyInstance.questGiverWindow.CloseWindow();
        }
    }

}
