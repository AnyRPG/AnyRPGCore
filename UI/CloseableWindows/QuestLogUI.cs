using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//public delegate void OnCheckCompletion();

/// <summary>
/// Maintains a list of all quests
/// </summary>
public class QuestLogUI : WindowContentController {

    #region Singleton
    private static QuestLogUI instance;

    public static QuestLogUI MyInstance
    {
        get
        {
            if (instance == null) {
                instance = FindObjectOfType<QuestLogUI>();
            }

            return instance;
        }
    }
    #endregion

    public override event Action<ICloseableWindowContents> OnOpenWindowHandler = delegate { };

    [SerializeField]
    private GameObject abandonButton, trackButton;

    [SerializeField]
    private GameObject questPrefab;

    [SerializeField]
    private Transform questParent;

    [SerializeField]
    private Text questCount;

    [SerializeField]
    private int maxCount;

    [SerializeField]
    private QuestDetailsArea questDetailsArea;

    private List<QuestScript> questScripts = new List<QuestScript>();

    private QuestScript selectedQuestScript;

    private string currentQuestName = null;

    public QuestScript MySelectedQuestScript { get => selectedQuestScript; set => selectedQuestScript = value; }

    private void Start() {
        SystemEventManager.MyInstance.OnQuestStatusUpdated += UpdateQuestCount;
        //QuestLog.MyInstance.OnQuestCompleted += HandleCompleteQuest;
        DeactivateButtons();
        UpdateQuestCount();
    }

    private void UpdateQuestCount () {
        questCount.text = QuestLog.MyInstance.MyQuests.Count + " / " + maxCount;
    }

    public void ShowQuestsCommon() {

        ClearQuests();
        ClearDescription(string.Empty);
        DeactivateButtons();

        QuestScript firstAvailableQuest = null;

        foreach (Quest quest in QuestLog.MyInstance.MyQuests.Values) {
            GameObject go = Instantiate(questPrefab, questParent);

            QuestScript qs = go.GetComponent<QuestScript>();
            qs.SetQuestName(quest.MyName);
            questScripts.Add(qs);
            if (firstAvailableQuest == null) {
                firstAvailableQuest = qs;
            }
        }

        if (selectedQuestScript == null && firstAvailableQuest != null) {
            firstAvailableQuest.Select();
        } else if (firstAvailableQuest == null) {

        }

        UpdateQuestCount();
    }

    public void ShowDescription(string questName) {
        //Debug.Log("QuestLogUI.ShowDescription()");

        ClearDescription(questName);

        if (questName == null || questName == string.Empty) {
            return;
        }
        currentQuestName = questName;

        Quest quest = SystemQuestManager.MyInstance.GetResource(questName);
        if (quest == null) {
            Debug.Log("QuestLogUI.ShowDescription(" + questName + "): failed to get quest from SystemQuestManager");
        }

        UpdateButtons(questName);

        questDetailsArea.gameObject.SetActive(true);
        questDetailsArea.ShowDescription(quest);
    }

    public void ClearDescription(string newQuestName) {
        //Debug.Log("QuestLogUI.ClearDescription(" + newQuestName + ")");

        questDetailsArea.ClearDescription();
        questDetailsArea.gameObject.SetActive(false);

        DeselectQuestScripts(newQuestName);
    }

    public void DeselectQuestScripts(string newQuestName) {
        //Debug.Log("QuestLogUI.DeselectQuestScripts()");
        foreach (QuestScript questScript in questScripts) {
            if (MySelectedQuestScript == null) {
                // we came from questtracker UI
                if (SystemResourceManager.MatchResource(newQuestName, questScript.MyQuestName)) {
                    questScript.RawSelect();
                    MySelectedQuestScript = questScript;
                } else {
                    questScript.DeSelect();
                }
            } else {
                if (SystemResourceManager.MatchResource(newQuestName, questScript.MyQuestName)) {
                    questScript.RawSelect();
                    MySelectedQuestScript = questScript;
                } else {
                    questScript.DeSelect();
                }
            }
        }

        // since questlog can be 
    }

    private void UpdateButtons(string questName) {
        abandonButton.SetActive(true);
        //abandonButton.GetComponent<Button>().enabled = true;
        //trackButton.GetComponent<Button>().enabled = true;
    }

    public void DeactivateButtons() {

        abandonButton.SetActive(false);
        //abandonButton.GetComponent<Button>().enabled = false;
        //trackButton.GetComponent<Button>().enabled = false;
    }

    public override void OnCloseWindow() {
        //Debug.Log("QuestLogUI.OnCloseWindow()");
        base.OnCloseWindow();
        ClearQuests();
        DeactivateButtons();
        MySelectedQuestScript = null;
    }

    public override void OnOpenWindow() {
        //Debug.Log("QuestLogUI.OnOpenWindow()");

        base.OnOpenWindow();

        OnOpenWindowHandler(this);

        ShowQuestsCommon();
    }

    public void ClearQuests() {
        //Debug.Log("QuestLogUI.ClearQuests()");
        foreach (QuestScript _questScript in questScripts) {
            Destroy(_questScript.gameObject);
        }
        questScripts.Clear();
        selectedQuestScript = null;
    }

    public void AbandonQuest() {
        QuestLog.MyInstance.AbandonQuest(currentQuestName);
        ShowQuestsCommon();
    }
}
