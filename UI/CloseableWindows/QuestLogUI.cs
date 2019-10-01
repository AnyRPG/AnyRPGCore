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

    public void AcceptQuest(Quest quest) {
        if (QuestLog.MyInstance.MyQuests.Count >= maxCount) {
            // quest log is full. we can't accept the quest
            return;
        }
        QuestLog.MyInstance.AcceptQuest(quest.MyName);
    }

    public void ShowQuestsCommon() {
        foreach (Quest quest in QuestLog.MyInstance.MyQuests.Values) {
            GameObject go = Instantiate(questPrefab, questParent);

            QuestScript qs = go.GetComponent<QuestScript>();
            qs.SetQuest(quest);
            questScripts.Add(qs);
            if (MySelectedQuestScript == null) {
                qs.Select();
            }
        }
        UpdateQuestCount();
    }

    public void UpdateSelected() {
        if (selectedQuestScript != null && selectedQuestScript.MyQuest != null) {
            ShowDescription(selectedQuestScript.MyQuest);
        }
    }

    public void ShowDescription(Quest quest) {
        //Debug.Log("QuestLogUI.ShowDescription()");

        if (MySelectedQuestScript.MyQuest != quest) {
            foreach (QuestScript questScript in questScripts) {
                if (questScript.MyQuest == quest) {
                    questScript.RawSelect();
                }
            }
        }

        ClearDescription();
        if (quest == null) {
            return;
        }

        questDetailsArea.gameObject.SetActive(true);
        questDetailsArea.ShowDescription(quest);

        UpdateButtons(quest);

    }

    public void ClearDescription() {
        //Debug.Log("QuestLogUI.ClearDescription()");

        questDetailsArea.gameObject.SetActive(false);
    }

    public bool HasQuest(Quest quest) {
        foreach (QuestScript qs in questScripts) {
            if (qs.MyQuest == quest) {
                return true;
            }
        }
        return false;
    }

    public void AbandonQuest() {
        MySelectedQuestScript.MyQuest.OnAbandonQuest();
        QuestLog.MyInstance.AbandonQuest(MySelectedQuestScript.MyQuest.MyName);
    }

    public void RemoveQuest(Quest _quest) {
        //Debug.Log("QuestLogUI.RemoveQuest(" + _quest.MyTitle + ")");

        QuestScript removeScript = null;
        foreach (QuestScript _questScript in questScripts) {
            if (_questScript.MyQuest == _quest) {
                removeScript = _questScript;
                break;
            }
        }
        if (removeScript != null) {
            questScripts.Remove(removeScript);
            removeScript.MyQuest.RemoveQuest();
            Destroy(removeScript.gameObject);
            ClearDescription();
            selectedQuestScript = null;
            removeScript = null;
        }
        DeactivateButtons();
        UpdateQuestCount();
    }

    private void UpdateButtons(Quest quest) {
        abandonButton.GetComponent<Button>().enabled = true;
        trackButton.GetComponent<Button>().enabled = true;
    }

    public void DeactivateButtons() {
        abandonButton.GetComponent<Button>().enabled = false;
        trackButton.GetComponent<Button>().enabled = false;
    }

    public override void OnCloseWindow() {
        //Debug.Log("QuestLogUI.OnCloseWindow()");
        base.OnCloseWindow();
        ClearQuests();
        DeactivateButtons();
    }

    public override void OnOpenWindow() {
        //Debug.Log("QuestLogUI.OnOpenWindow()");
        base.OnOpenWindow();
        ClearDescription();
        OnOpenWindowHandler(this);
        ShowQuestsCommon();
        if (MySelectedQuestScript != null) {
            ShowDescription(MySelectedQuestScript.MyQuest);
        }
    }

    public void ClearQuests() {
        //Debug.Log("QuestLogUI.ClearQuests()");
        foreach (QuestScript _questScript in questScripts) {
            Destroy(_questScript.gameObject);
        }
        questScripts.Clear();
        selectedQuestScript = null;
    }
}
