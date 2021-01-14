using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class SystemBehaviorProfileManager : SystemResourceManager {

        #region Singleton
        private static SystemBehaviorProfileManager instance;

        public static SystemBehaviorProfileManager MyInstance {
            get {
                if (instance == null) {
                    instance = FindObjectOfType<SystemBehaviorProfileManager>();
                }

                return instance;
            }
        }

        #endregion

        const string resourceClassName = "BehaviorProfile";

        protected override void Awake() {
            base.Awake();
        }

        public override void LoadResourceList() {
            masterList.Add(Resources.LoadAll<BehaviorProfile>(resourceClassName));
            if (SystemConfigurationManager.MyInstance != null) {
                foreach (string loadResourcesFolder in SystemConfigurationManager.MyInstance.MyLoadResourcesFolders) {
                    masterList.Add(Resources.LoadAll<BehaviorProfile>(loadResourcesFolder + "/" + resourceClassName));
                }
            }
            base.LoadResourceList();
        }

        public BehaviorProfile GetResource(string resourceName) {
            //Debug.Log(this.GetType().Name + ".GetResource(" + resourceName + ")");
            if (!RequestIsEmpty(resourceName)) {
                string keyName = prepareStringForMatch(resourceName);
                if (resourceList.ContainsKey(keyName)) {
                    return (resourceList[keyName] as BehaviorProfile);
                }
            }
            return null;
        }

        public BehaviorProfile GetNewResource(string resourceName) {
            //Debug.Log(this.GetType().Name + ".GetResource(" + resourceName + ")");
            if (!RequestIsEmpty(resourceName)) {
                string keyName = prepareStringForMatch(resourceName);
                if (resourceList.ContainsKey(keyName)) {
                    BehaviorProfile returnValue = ScriptableObject.Instantiate(resourceList[keyName]) as BehaviorProfile;
                    returnValue.SetupScriptableObjects();
                    return returnValue;
                }
            }
            return null;
        }

        public List<BehaviorProfile> GetResourceList() {
            List<BehaviorProfile> returnList = new List<BehaviorProfile>();

            foreach (UnityEngine.Object listItem in resourceList.Values) {
                returnList.Add(listItem as BehaviorProfile);
            }
            return returnList;
        }

        /*
        public void LoadBehavior(BehaviorSaveData behaviorSaveData) {
            //Debug.Log("QuestLog.LoadQuest(" + questSaveData.MyName + ")");

            BehaviorProfile behaviorProfile = GetResource(behaviorSaveData.MyName);
            if (behaviorProfile != null) {
                behaviorProfile.Completed = behaviorSaveData.completed;
            }
        }
        */

    }
}