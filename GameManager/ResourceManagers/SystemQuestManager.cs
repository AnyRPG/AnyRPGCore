using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {

    /// <summary>
    /// allow us to query scriptable objects for equivalence by storing a template ID on all instantiated objects
    /// </summary>
    public class SystemQuestManager : SystemResourceManager {

        #region Singleton
        private static SystemQuestManager instance;

        public static SystemQuestManager MyInstance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<SystemQuestManager>();
                }

                return instance;
            }
        }

        #endregion

        const string resourceClassName = "Quest";

        public override void CreateEventSubscriptions() {
            //Debug.Log("PlayerManager.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized) {
                return;
            }
            base.CreateEventSubscriptions();
            SystemEventManager.MyInstance.OnPlayerConnectionSpawn += AcceptAchievements;
            if (PlayerManager.MyInstance.MyPlayerConnectionSpawned == true) {
                AcceptAchievements();
            }
            eventSubscriptionsInitialized = true;
        }

        public override void CleanupEventSubscriptions() {
            //Debug.Log("PlayerManager.CleanupEventSubscriptions()");
            if (!eventSubscriptionsInitialized) {
                return;
            }
            base.CleanupEventSubscriptions();
            SystemEventManager.MyInstance.OnPlayerConnectionSpawn -= AcceptAchievements;
            eventSubscriptionsInitialized = false;
        }

        public override void OnDisable() {
            //Debug.Log("PlayerManager.OnDisable()");
            base.OnDisable();
            CleanupEventSubscriptions();
        }

        public void AcceptAchievements() {
            //Debug.Log("SystemQuestManager.AcceptAchievements()");
            foreach (Quest resource in resourceList.Values) {
                //Debug.Log("SystemQuestManager.AcceptAchievements(): quest: " + resource.MyName + "; isAchievement: " + resource.MyIsAchievement);
                if (resource.MyIsAchievement == true && resource.TurnedIn == false && resource.IsComplete == false) {
                    //Debug.Log("SystemQuestManager.AcceptAchievements(): quest: " + resource.MyName + "; isAchievement: " + resource.MyIsAchievement + " TRUE!!!");
                    resource.AcceptQuest();
                }
            }
        }

        public override void LoadResourceList() {
            //Debug.Log(this.GetType().Name + ".LoadResourceList()");
            masterList.Add(Resources.LoadAll<Quest>(resourceClassName));
            if (SystemConfigurationManager.MyInstance != null) {
                foreach (string loadResourcesFolder in SystemConfigurationManager.MyInstance.MyLoadResourcesFolders) {
                    masterList.Add(Resources.LoadAll<Quest>(loadResourcesFolder + "/" + resourceClassName));
                }
            }
            base.LoadResourceList();
        }

        public Quest GetResource(string resourceName) {
            //Debug.Log(this.GetType().Name + ".GetResource(" + resourceName + ")");
            if (!RequestIsEmpty(resourceName)) {
                string keyName = prepareStringForMatch(resourceName);
                if (resourceList.ContainsKey(keyName)) {
                    return (resourceList[keyName] as Quest);
                }
            }
            return null;
        }

        public List<Quest> GetResourceList() {
            //Debug.Log(this.GetType().Name + ".GetResourceList()");
            List<Quest> returnList = new List<Quest>();

            foreach (UnityEngine.Object listItem in resourceList.Values) {
                returnList.Add(listItem as Quest);
            }
            return returnList;
        }
    }

}