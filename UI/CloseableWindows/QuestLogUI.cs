using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    //public delegate void OnCheckCompletion();

    /// <summary>
    /// Maintains a list of all quests
    /// </summary>
    public class QuestLogUI : WindowContentController {

        #region Singleton
        private static QuestLogUI instance;

        public static QuestLogUI MyInstance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<QuestLogUI>();
                }

                return instance;
            }
        }
        #endregion

        public override event Action<ICloseableWindowContents> OnOpenWindow = delegate { };

        [SerializeField]
        private GameObject abandonButton = null;

        //[SerializeField]
        //private GameObject trackButton = null;

        [SerializeField]
        private GameObject questPrefab = null;

        [SerializeField]
        private Transform questParent = null;

        [SerializeField]
        private TextMeshProUGUI questCount = null;

        [SerializeField]
        private int maxCount = 25;

        [SerializeField]
        private QuestDetailsArea questDetailsArea = null;

        private List<QuestScript> questScripts = new List<QuestScript>();

        private QuestScript selectedQuestScript = null;

        //private string currentQuestName = null;

        private Quest currentQuest = null;

        public QuestScript MySelectedQuestScript { get => selectedQuestScript; set => selectedQuestScript = value; }

        private void Start() {
            SystemEventManager.MyInstance.OnQuestStatusUpdated += UpdateQuestCount;
            //QuestLog.MyInstance.OnQuestCompleted += HandleCompleteQuest;
            DeactivateButtons();
            UpdateQuestCount();
        }

        private void UpdateQuestCount() {
            questCount.text = QuestLog.MyInstance.MyQuests.Count + " / " + maxCount;
        }

        public void ShowQuestsCommon() {

            ClearQuests();
            ClearDescription(null);
            DeactivateButtons();

            QuestScript firstAvailableQuest = null;

            foreach (Quest quest in QuestLog.MyInstance.MyQuests.Values) {
                GameObject go = Instantiate(questPrefab, questParent);

                QuestScript qs = go.GetComponent<QuestScript>();
                qs.SetQuest(quest);
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

        public void ShowDescription(Quest newQuest) {
            //Debug.Log("QuestLogUI.ShowDescription()");

            ClearDescription(newQuest);

            if (newQuest == null) {
                return;
            }

            UpdateButtons(newQuest);

            questDetailsArea.gameObject.SetActive(true);
            questDetailsArea.ShowDescription(newQuest);
        }

        public void ClearDescription(Quest newQuest) {
            //Debug.Log("QuestLogUI.ClearDescription(" + newQuestName + ")");

            questDetailsArea.ClearDescription();
            questDetailsArea.gameObject.SetActive(false);

            DeselectQuestScripts(newQuest);
        }

        public void DeselectQuestScripts(Quest newQuest) {
            //Debug.Log("QuestLogUI.DeselectQuestScripts()");
            foreach (QuestScript questScript in questScripts) {
                if (MySelectedQuestScript == null) {
                    // we came from questtracker UI
                    if (SystemResourceManager.MatchResource(newQuest.MyName, questScript.MyQuest.MyName)) {
                        questScript.RawSelect();
                        MySelectedQuestScript = questScript;
                    } else {
                        questScript.DeSelect();
                    }
                } else {
                    if (SystemResourceManager.MatchResource(newQuest.MyName, questScript.MyQuest.MyName)) {
                        questScript.RawSelect();
                        MySelectedQuestScript = questScript;
                    } else {
                        questScript.DeSelect();
                    }
                }
            }

            // since questlog can be 
        }

        private void UpdateButtons(Quest quest) {
            abandonButton.SetActive(true);
            //abandonButton.GetComponent<Button>().enabled = true;
            //trackButton.GetComponent<Button>().enabled = true;
        }

        public void DeactivateButtons() {

            abandonButton.SetActive(false);
            //abandonButton.GetComponent<Button>().enabled = false;
            //trackButton.GetComponent<Button>().enabled = false;
        }

        public override void RecieveClosedWindowNotification() {
            //Debug.Log("QuestLogUI.OnCloseWindow()");
            base.RecieveClosedWindowNotification();
            ClearQuests();
            DeactivateButtons();
            MySelectedQuestScript = null;
        }

        public override void ReceiveOpenWindowNotification() {
            //Debug.Log("QuestLogUI.OnOpenWindow()");

            base.ReceiveOpenWindowNotification();

            OnOpenWindow(this);

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
            QuestLog.MyInstance.AbandonQuest(currentQuest);
            ShowQuestsCommon();
        }
    }

}