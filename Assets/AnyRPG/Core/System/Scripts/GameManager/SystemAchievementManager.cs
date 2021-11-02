using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {

    /// <summary>
    /// allow us to query scriptable objects for equivalence by storing a template ID on all instantiated objects
    /// </summary>
    public class SystemAchievementManager : ConfiguredMonoBehaviour {

        private bool eventSubscriptionsInitialized = false;

        // game manager references
        PlayerManager playerManager = null;
        SystemDataFactory systemDataFactory = null;

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);
            playerManager = systemGameManager.PlayerManager;
            systemDataFactory = systemGameManager.SystemDataFactory;

            CreateEventSubscriptions();
        }

        public void CreateEventSubscriptions() {
            //Debug.Log("PlayerManager.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized) {
                return;
            }
            SystemEventManager.StartListening("OnPlayerConnectionSpawn", HandlePlayerConnectionSpawn);
            if (playerManager.PlayerConnectionSpawned == true) {
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
            foreach (Quest resource in systemDataFactory.GetResourceList<Quest>()) {
                if (resource.IsAchievement == true && resource.TurnedIn == false && resource.IsComplete == false) {
                    resource.AcceptQuest();
                }
            }
        }

       
    }

}