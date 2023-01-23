using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    /// <summary>
    /// Maintains a list of all quests
    /// </summary>
    public class QuestLogUI : WindowContentController {

        [SerializeField]
        private HighlightButton abandonButton = null;

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

        // game manager references
        private QuestLog questLog = null;
        private ObjectPooler objectPooler = null;

        public QuestScript SelectedQuestScript { get => selectedQuestScript; set => selectedQuestScript = value; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            abandonButton.Configure(systemGameManager);

            questLog.OnShowQuestLogDescription += HandleShowQuestDescription;
            SystemEventManager.StartListening("OnQuestStatusUpdated", HandleQuestStatusUpdated);
            UpdateQuestCount();

            questDetailsArea.Configure(systemGameManager);
            questDetailsArea.SetOwner(this);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            questLog = systemGameManager.QuestLog;
            objectPooler = systemGameManager.ObjectPooler;
        }

        public void HandleQuestStatusUpdated(string eventName, EventParamProperties eventParamProperties) {
            UpdateQuestCount();
        }

        private void UpdateQuestCount() {
            questCount.text = questLog.Quests.Count + " / " + maxCount;
        }

        public void ShowQuestsCommon() {

            ClearQuests();
            ClearDescription(null);
            DeactivateButtons();

            QuestScript firstAvailableQuest = null;

            foreach (Quest quest in questLog.Quests.Values) {
                GameObject go = objectPooler.GetPooledObject(questPrefab, questParent);

                QuestScript qs = go.GetComponent<QuestScript>();
                qs.Configure(systemGameManager);
                qs.SetQuest(this, quest);
                questScripts.Add(qs);
                uINavigationControllers[0].AddActiveButton(qs);
                if (firstAvailableQuest == null) {
                    firstAvailableQuest = qs;
                }
            }

            if (selectedQuestScript == null && firstAvailableQuest != null) {
                firstAvailableQuest.Select();
                uINavigationControllers[0].FocusFirstButton();
            }
            SetNavigationController(uINavigationControllers[0]);

            UpdateQuestCount();
        }

        public void HandleShowQuestDescription(Quest quest) {
            ShowDescription(quest);
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
                if (SelectedQuestScript == null) {
                    // we came from questtracker UI
                    if (SystemDataFactory.MatchResource(newQuest.DisplayName, questScript.Quest.DisplayName)) {
                        questScript.RawSelect();
                        SelectedQuestScript = questScript;
                    } else {
                        questScript.DeSelect();
                    }
                } else {
                    if (SystemDataFactory.MatchResource(newQuest.DisplayName, questScript.Quest.DisplayName)) {
                        questScript.RawSelect();
                        SelectedQuestScript = questScript;
                    } else {
                        questScript.DeSelect();
                    }
                }
            }

            uINavigationControllers[0].UnHightlightButtons(SelectedQuestScript);

            // since questlog can be 
        }

        private void UpdateButtons(Quest quest) {
            abandonButton.gameObject.SetActive(true);
            //abandonButton.GetComponent<Button>().enabled = true;
            //trackButton.GetComponent<Button>().enabled = true;
        }

        public void DeactivateButtons() {

            abandonButton.gameObject.SetActive(false);
            //abandonButton.GetComponent<Button>().enabled = false;
            //trackButton.GetComponent<Button>().enabled = false;
        }

        public override void ReceiveClosedWindowNotification() {
            //Debug.Log("QuestLogUI.OnCloseWindow()");
            base.ReceiveClosedWindowNotification();
            ClearQuests();
            SelectedQuestScript = null;
        }

        public override void ProcessOpenWindowNotification() {
            //Debug.Log("QuestLogUI.OnOpenWindow()");

            base.ProcessOpenWindowNotification();
            SetBackGroundColor(new Color32(0, 0, 0, (byte)(int)(PlayerPrefs.GetFloat("PopupWindowOpacity") * 255)));
            
            //reset button state before showing new quests
            DeactivateButtons();

            ShowQuestsCommon();
        }

        public void ClearQuests() {
            //Debug.Log("QuestLogUI.ClearQuests()");
            foreach (QuestScript _questScript in questScripts) {
                _questScript.DeSelect();
                objectPooler.ReturnObjectToPool(_questScript.gameObject);
            }
            questScripts.Clear();
            selectedQuestScript = null;
            uINavigationControllers[0].ClearActiveButtons();
        }

        public void AbandonQuest() {
            questLog.AbandonQuest(SelectedQuestScript.Quest);
            ShowQuestsCommon();
        }
    }

}