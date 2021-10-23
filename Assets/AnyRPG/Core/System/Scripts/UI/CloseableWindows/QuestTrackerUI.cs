using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class QuestTrackerUI : WindowContentController {

        [Tooltip("The gameobject to use for each quest item")]
        [SerializeField]
        private GameObject questPrefab = null;

        [Tooltip("Quests will be spawned under this transform")]
        [SerializeField]
        private Transform questParent = null;

        [SerializeField]
        private DraggableWindow dragHandle = null;

        private List<QuestTrackerQuestScript> questScripts = new List<QuestTrackerQuestScript>();

        // game manager references
        private PlayerManager playerManager = null;
        private QuestLog questLog = null;
        private ObjectPooler objectPooler = null;

        
        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            dragHandle.Configure(systemGameManager);
        }
        

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            playerManager = systemGameManager.PlayerManager;
            questLog = systemGameManager.QuestLog;
            objectPooler = systemGameManager.ObjectPooler;
        }

        protected override void CreateEventSubscriptions() {
            //Debug.Log("QuestTrackerUI.InitializeReferences()");
            if (eventSubscriptionsInitialized == true) {
                return;
            }
            base.CreateEventSubscriptions();
            SystemEventManager.StartListening("OnQuestObjectiveStatusUpdated", HandleQuestObjectiveStatusUpdated);
            SystemEventManager.StartListening("OnAfterQuestStatusUpdated", HandleAfterQuestStatusUpdated);
            SystemEventManager.StartListening("OnPlayerUnitSpawn", HandlePlayerUnitSpawn);
            if (playerManager.PlayerUnitSpawned == true) {
                ShowQuests();
            }
        }

        protected override void CleanupEventSubscriptions() {
            //Debug.Log("QuestTrackerUI.CleanupEventSubscriptions()");
            if (eventSubscriptionsInitialized == false) {
                return;
            }
            base.CleanupEventSubscriptions();
            SystemEventManager.StopListening("OnQuestObjectiveStatusUpdated", HandleQuestObjectiveStatusUpdated);
            SystemEventManager.StopListening("OnAfterQuestStatusUpdated", HandleAfterQuestStatusUpdated);
            SystemEventManager.StopListening("OnPlayerUnitSpawn", HandlePlayerUnitSpawn);
        }

        public void HandleQuestObjectiveStatusUpdated(string eventName, EventParamProperties eventParamProperties) {
            ShowQuests();
        }

        public void HandleAfterQuestStatusUpdated(string eventName, EventParamProperties eventParamProperties) {
            ShowQuests();
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
            if (playerManager != null && playerManager.PlayerUnitSpawned == false) {
                // shouldn't be doing anything without a player spawned.
                return;
            }
            ClearQuests();

            foreach (Quest quest in questLog.Quests.Values) {
                //Debug.Log("QuestTrackerUI.ShowQuestsCommon(): quest: " + quest);
                GameObject go = objectPooler.GetPooledObject(questPrefab, questParent);
                QuestTrackerQuestScript qs = go.GetComponent<QuestTrackerQuestScript>();
                qs.Configure(systemGameManager);
                qs.MyQuest = quest;
                if (qs == null) {
                    //Debug.Log("QuestTrackerUI.ShowQuestsCommon(): QuestGiverQuestScript is null");
                }
                qs.MyText.text = "[" + quest.ExperienceLevel + "] " + quest.DisplayName;
                if (quest.IsComplete) {
                    qs.MyText.text += " (Complete)";
                }
                string objectives = string.Empty;

                qs.MyText.text += "\n<size=12>" + quest.GetUnformattedObjectiveList() + "</size>";

                //Debug.Log("QuestTrackerUI.ShowQuestsCommon(" + questGiver.name + "): " + questNode.MyQuest.MyTitle);
                qs.MyText.color = LevelEquations.GetTargetColor(playerManager.MyCharacter.CharacterStats.Level, quest.ExperienceLevel);
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
                    objectPooler.ReturnObjectToPool(qs.gameObject);
                }
            }
            questScripts.Clear();
        }

        public override void ReceiveClosedWindowNotification() {
            //Debug.Log("QuestTrackerUI.OnCloseWindow()");
            base.ReceiveClosedWindowNotification();
            //CleanupEventSubscriptions();
        }

        public override void ReceiveOpenWindowNotification() {
            //Debug.Log("QuestTrackerUI.OnOpenWindow()");
            // prevent attempting to show quests if no player is logged in
            if (playerManager.PlayerConnectionSpawned == false) {
                return;
            }
            ShowQuests();
        }


    }

}