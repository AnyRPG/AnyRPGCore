using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class SystemQuestGiverProfileManager : SystemResourceManager {

        #region Singleton
        private static SystemQuestGiverProfileManager instance;

        public static SystemQuestGiverProfileManager MyInstance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<SystemQuestGiverProfileManager>();
                }

                return instance;
            }
        }
        #endregion

        const string resourceClassName = "QuestGiverProfile";

        protected override void Awake() {
            //Debug.Log(this.GetType().Name + ".Awake()");
            base.Awake();
        }

        public override void LoadResourceList() {
            //Debug.Log(this.GetType().Name + ".LoadResourceList()");
            masterList.Add(Resources.LoadAll<QuestGiverProfile>(resourceClassName));
            if (SystemConfigurationManager.MyInstance != null) {
                foreach (string loadResourcesFolder in SystemConfigurationManager.MyInstance.MyLoadResourcesFolders) {
                    masterList.Add(Resources.LoadAll<QuestGiverProfile>(loadResourcesFolder + "/" + resourceClassName));
                }
            }
            base.LoadResourceList();
        }

        public QuestGiverProfile GetResource(string resourceName) {
            //Debug.Log(this.GetType().Name + ".GetResource(" + resourceName + ")");
            if (!RequestIsEmpty(resourceName)) {
                string keyName = prepareStringForMatch(resourceName);
                if (resourceList.ContainsKey(keyName)) {
                    return (resourceList[keyName] as QuestGiverProfile);
                }
            }
            return null;
        }

        public List<QuestGiverProfile> GetResourceList() {
            List<QuestGiverProfile> returnList = new List<QuestGiverProfile>();

            foreach (UnityEngine.Object listItem in resourceList.Values) {
                returnList.Add(listItem as QuestGiverProfile);
            }
            return returnList;
        }
    }

}