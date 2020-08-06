using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class QuestTrackerUI : WindowContentController {

        #region Singleton
        private static QuestTrackerUI instance;

        public static QuestTrackerUI MyInstance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<QuestTrackerUI>();
                }

                return instance;
            }
        }

        #endregion

        [Tooltip("The gameobject to use for each quest item")]
        [SerializeField]
        private GameObject questPrefab = null;

        [Tooltip("Quests will be spawned under this transform")]
        [SerializeField]
        private Transform questParent = null;

        private bool referencesInitialized = false;

        private List<QuestTrackerQuestScript> questScripts = new List<QuestTrackerQuestScript>();

        public override event System.Action<ICloseableWindowContents> OnOpenWindow = delegate { };

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
            SystemEventManager.MyInstance.OnAfterQuestStatusUpdated += ShowQuests;
            SystemEventManager.StartListening("OnPlayerUnitSpawn", HandlePlayerUnitSpawn);
            if (PlayerManager.MyInstance != null && PlayerManager.MyInstance.MyPlayerUnitSpawned == true) {
                ShowQuests();
            }

            referencesInitialized = true;
        }

        public void CleanupEventSubscriptions() {
            //Debug.Log("QuestTrackerUI.CleanupEventSubscriptions()");
            if (SystemEventManager.MyInstance != null) {
                SystemEventManager.MyInstance.OnQuestObjectiveStatusUpdated -= ShowQuests;
                SystemEventManager.MyInstance.OnQuestStatusUpdated -= ShowQuests;
                SystemEventManager.StopListening("OnPlayerUnitSpawn", HandlePlayerUnitSpawn);
            }
            referencesInitialized = false;
        }

        public void HandlePlayerUnitSpawn(string eventName, EventParamProperties eventParamProperties) {
            //Debug.Log(gameObject.name + ".InanimateUnit.HandlePlayerUnitSpawn()");
            ProcessPlayerUnitSpawn();
        }


        public void ProcessPlayerUnitSpawn() {
            ShowQuests();
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
                qs.MyText.text = "[" + quest.MyExperienceLevel + "] " + quest.DisplayName;
                if (quest.IsComplete) {
                    qs.MyText.text += " (Complete)";
                }
                string objectives = string.Empty;

                qs.MyText.text += "\n<size=12>" + quest.GetUnformattedObjectiveList() + "</size>";

                //Debug.Log("QuestTrackerUI.ShowQuestsCommon(" + questGiver.name + "): " + questNode.MyQuest.MyTitle);
                qs.MyText.color = LevelEquations.GetTargetColor(PlayerManager.MyInstance.MyCharacter.CharacterStats.Level, quest.MyExperienceLevel);
                //quests.Add(go);
                questScripts.Add(qs);

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

        public override void RecieveClosedWindowNotification() {
            //Debug.Log("QuestTrackerUI.OnCloseWindow()");
            base.RecieveClosedWindowNotification();
            //CleanupEventSubscriptions();
        }

        public override void ReceiveOpenWindowNotification() {
            //Debug.Log("QuestTrackerUI.OnOpenWindow()");
            // clear first because open window handler could show a description
            ShowQuests();
            OnOpenWindow(this);
        }

        // moved below functionality to OnDestroy because these are static objects that exist for the entire game
        public void OnDestroy() {
            //Debug.Log("QuestTrackerUI.OnDestroy()");
            CleanupEventSubscriptions();
        }

    }

}