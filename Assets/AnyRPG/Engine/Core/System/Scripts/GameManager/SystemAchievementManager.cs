using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {

    /// <summary>
    /// allow us to query scriptable objects for equivalence by storing a template ID on all instantiated objects
    /// </summary>
    public class SystemAchievementManager : MonoBehaviour {

        private bool eventSubscriptionsInitialized = false;

        public void Init() {
            CreateEventSubscriptions();
        }

        public  void CreateEventSubscriptions() {
            //Debug.Log("PlayerManager.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized) {
                return;
            }
            SystemEventManager.StartListening("OnPlayerConnectionSpawn", HandlePlayerConnectionSpawn);
            if (SystemGameManager.Instance.PlayerManager.PlayerConnectionSpawned == true) {
                AcceptAchievements();
            }
            eventSubscriptionsInitialized = true;
        }

        public void CleanupEventSubscriptions() {
            //Debug.Log("PlayerManager.CleanupEventSubscriptions()");
            if (!eventSubscriptionsInitialized) {
                return;
            }
            SystemEventManager.StopListening("OnPlayerConnectionSpawn", HandlePlayerConnectionSpawn);
            eventSubscriptionsInitialized = false;
        }

        public void HandlePlayerConnectionSpawn(string eventName, EventParamProperties eventParamProperties) {
            AcceptAchievements();
        }

        public void OnDisable() {
            //Debug.Log("PlayerManager.OnDisable()");
            if (SystemGameManager.IsShuttingDown) {
                return;
            }
            CleanupEventSubscriptions();
        }

        public void AcceptAchievements() {
            //Debug.Log("SystemQuestManager.AcceptAchievements()");
            foreach (Quest resource in SystemDataFactory.Instance.GetResourceList<Quest>()) {
                //Debug.Log("SystemQuestManager.AcceptAchievements(): quest: " + resource.MyName + "; isAchievement: " + resource.MyIsAchievement);
                if (resource.MyIsAchievement == true && resource.TurnedIn == false && resource.IsComplete == false) {
                    //Debug.Log("SystemQuestManager.AcceptAchievements(): quest: " + resource.MyName + "; isAchievement: " + resource.MyIsAchievement + " TRUE!!!");
                    resource.AcceptQuest();
                }
            }
        }

       
    }

}