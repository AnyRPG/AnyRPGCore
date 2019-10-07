using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestTrackerUI : WindowContentController {

    #region Singleton
    private static QuestTrackerUI instance;

    public static QuestTrackerUI MyInstance
    {
        get
        {
            if (instance == null) {
                instance = FindObjectOfType<QuestTrackerUI>();
            }

            return instance;
        }
    }

    #endregion

    [SerializeField]
    private GameObject questPrefab;

    [SerializeField]
    private Transform questParent;

    [SerializeField]
    private GameObject inProgressHeading;

    [SerializeField]
    private GameObject inProgressArea;

    // delete me
    //private List<QuestNode> questNodes = new List<QuestNode>();

    private bool referencesInitialized = false;

    private List<QuestTrackerQuestScript> questScripts = new List<QuestTrackerQuestScript>();

    public override event System.Action<ICloseableWindowContents> OnOpenWindowHandler = delegate { };

    private void Start() {
        //Debug.Log("QuestTrackerUI.Start()");
        InitializeReferences();
    }

    private void OnEnable() {
        //Debug.Log("QuestTrackerUI.OnEnable()");
        InitializeReferences();
    }

    public void InitializeReferences() {
        //Debug.Log("QuestTrackerUI.InitializeReferences()");
        if (referencesInitialized == true) {
            return;
        }
        SystemEventManager.MyInstance.OnQuestObjectiveStatusUpdated += ShowQuests;
        SystemEventManager.MyInstance.OnQuestStatusUpdated += ShowQuests;
        SystemEventManager.MyInstance.OnPlayerUnitSpawn += ShowQuests;
        if (PlayerManager.MyInstance != null && PlayerManager.MyInstance.MyPlayerUnitSpawned == true) {
            ShowQuests();
        }

        referencesInitialized = true;
    }

    public void CleanupEventReferences() {
        //Debug.Log("QuestTrackerUI.CleanupEventReferences()");
        if (SystemEventManager.MyInstance != null) {
            SystemEventManager.MyInstance.OnQuestObjectiveStatusUpdated -= ShowQuests;
            SystemEventManager.MyInstance.OnQuestStatusUpdated -= ShowQuests;
            SystemEventManager.MyInstance.OnPlayerUnitSpawn -= ShowQuests;
        }
        referencesInitialized = false;
    }


    public void ShowQuestsCommon() {
        //Debug.Log("QuestTrackerUI.ShowQuestsCommon()");
        if (PlayerManager.MyInstance != null && PlayerManager.MyInstance.MyPlayerUnitSpawned == false) {
            // shouldn't be doing anything without a player spawned.
            return;
        }
        ClearQuests();

        foreach (Quest quest in QuestLog.MyInstance.MyQuests.Values) {
            //Debug.Log("QuestTrackerUI.ShowQuestsCommon(): quest: " + quest);
            GameObject go = Instantiate(questPrefab, questParent);
            QuestTrackerQuestScript qs = go.GetComponent<QuestTrackerQuestScript>();
            qs.MyQuest = quest;
            if (qs == null) {
                //Debug.Log("QuestTrackerUI.ShowQuestsCommon(): QuestGiverQuestScript is null");
            }
            qs.MyText.text = "[" + quest.MyExperienceLevel + "] " + quest.MyName;
            if (quest.IsComplete) {
                qs.MyText.text += " (Complete)";
            }
            string objectives = string.Empty;

            qs.MyText.text += "\n<size=12>" + quest.GetUnformattedObjectiveList() + "</size>";

            //Debug.Log("QuestTrackerUI.ShowQuestsCommon(" + questGiver.name + "): " + questNode.MyQuest.MyTitle);
            qs.MyText.color = LevelEquations.GetTargetColor(PlayerManager.MyInstance.MyCharacter.MyCharacterStats.MyLevel, quest.MyExperienceLevel);
            //quests.Add(go);
            questScripts.Add(qs);
            go.transform.SetParent(inProgressArea.transform);

        }

    }

    public void ShowQuests() {
        //Debug.Log("QuestTrackerUI.ShowQuests()");
        ShowQuestsCommon();
    }

    public void ShowQuests(Quest quest) {
        //Debug.Log("QuestTrackerUI.ShowQuests()");
        ShowQuestsCommon();
    }

    public void ClearQuests() {
        //Debug.Log("QuestTrackerUI.ClearQuests()");
        // clear the quest list so any quests left over from a previous time opening the window aren't shown
        foreach (QuestTrackerQuestScript qs in questScripts) {
            if (qs.gameObject != null) {
                //Debug.Log("The questnode has a gameobject we need to clear");
                qs.gameObject.transform.SetParent(null);
                Destroy(qs.gameObject);
            }
        }
        questScripts.Clear();
    }

    public override void OnCloseWindow() {
        //Debug.Log("QuestTrackerUI.OnCloseWindow()");
        base.OnCloseWindow();
        CleanupEventReferences();
    }

    public override void OnOpenWindow() {
        //Debug.Log("QuestTrackerUI.OnOpenWindow()");
        // clear first because open window handler could show a description
        ShowQuests();
        OnOpenWindowHandler(this);
    }

    public void OnDisable() {
        //Debug.Log("QuestTrackerUI.OnDisable()");
        CleanupEventReferences();
    }
}
